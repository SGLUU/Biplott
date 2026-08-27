using System.Collections.Concurrent;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class RemixService : IRemixService
{
    private static readonly ConcurrentDictionary<string, LuckyJourneySession> RemixSessions = new();

    private readonly BiplottDbContext _dbContext;
    private readonly IRandomNumberEngine _randomEngine;
    private readonly INoveltyEngine _noveltyEngine;
    private readonly ILuckyNumberEngine _luckyEngine;
    private readonly ILuckyDnaService _dnaService;
    private readonly IQuestionRepository _questionRepository;

    public RemixService(
        BiplottDbContext dbContext,
        IRandomNumberEngine randomEngine,
        INoveltyEngine noveltyEngine,
        ILuckyNumberEngine luckyEngine,
        ILuckyDnaService dnaService,
        IQuestionRepository questionRepository)
    {
        _dbContext = dbContext;
        _randomEngine = randomEngine;
        _noveltyEngine = noveltyEngine;
        _luckyEngine = luckyEngine;
        _dnaService = dnaService;
        _questionRepository = questionRepository;
    }

    public async Task<GenerateLineResponse> QuickRemixAsync(StartRemixJourneyRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _dbContext.Games
            .Include(g => g.Pools)
            .FirstOrDefaultAsync(g => g.Code == request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var locked = request.Numbers.Where(n => n.IsLocked).ToList();
        if (locked.Count == request.Numbers.Count && locked.Count > 0)
        {
            throw new InvalidOperationException("Bạn đã khóa toàn bộ bộ số.");
        }

        // Generate unlocked slots pool by pool
        var resultNumbers = new List<GeneratedNumberDto>();
        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();

        foreach (var pool in pools)
        {
            var lockedInPool = locked.Where(n => n.PoolIndex == pool.PoolIndex).ToList();
            resultNumbers.AddRange(lockedInPool);

            int needed = pool.PickCount - lockedInPool.Count;
            if (needed > 0)
            {
                var excluded = new HashSet<int>(lockedInPool.Select(l => l.Value));
                // We also exclude any other locked main numbers if game requires global uniqueness (like Power/Mega)
                if (game.Pools.Count == 1) // Power/Mega
                {
                    foreach (var l in locked)
                    {
                        excluded.Add(l.Value);
                    }
                }

                // Call RandomNumberEngine to generate remaining slots in this pool
                var strategy = RandomStrategy.PureRandom;
                var generated = _randomEngine.GeneratePoolNumbers(pool, needed, strategy, excluded);
                resultNumbers.AddRange(generated);
            }
        }

        // Stable sort: keep the locked flag aligned correctly because we sort the objects themselves
        var ordered = resultNumbers
            .OrderBy(n => n.PoolIndex)
            .ThenBy(n => n.Value)
            .ToList();

        return new GenerateLineResponse
        {
            Strategy = RandomStrategy.PureRandom,
            StrategyName = "Quick Remix",
            Numbers = ordered,
            Commentary = "Đã làm mới các số không bị khóa."
        };
    }

    public async Task<StartJourneyResponse> StartLuckyRemixAsync(
        StartRemixJourneyRequest request,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var game = await _dbContext.Games
            .Include(g => g.Pools)
            .FirstOrDefaultAsync(g => g.Code == request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var locked = request.Numbers.Where(n => n.IsLocked).ToList();
        if (locked.Count == request.Numbers.Count && locked.Count > 0)
        {
            throw new InvalidOperationException("Bạn đã khóa toàn bộ bộ số.");
        }

        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        var stepPools = new List<int>();

        foreach (var pool in pools)
        {
            var lockedCount = locked.Count(n => n.PoolIndex == pool.PoolIndex);
            int needed = pool.PickCount - lockedCount;
            for (int i = 0; i < needed; i++)
            {
                stepPools.Add(pool.PoolIndex);
            }
        }

        if (stepPools.Count == 0)
        {
            throw new InvalidOperationException("Không có vị trí trống nào cần remix.");
        }

        var allQuestions = await _questionRepository.GetAllActiveQuestionsAsync(cancellationToken);
        if (allQuestions.Count == 0)
        {
            throw new InvalidOperationException("Hệ thống chưa có câu hỏi nào được kích hoạt.");
        }

        var noveltyCtx = new NoveltyContext
        {
            RecentQuestionIds = request.RecentQuestionIds ?? new List<int>(),
            RecentThemeIds = request.RecentThemeIds ?? new List<int>()
        };

        var firstQuestion = _noveltyEngine.SelectNextQuestion(allQuestions, noveltyCtx, isClimaxStep: false);

        // Convert Locked to RevealedNumberDto
        var lockedDtoList = locked.Select(l => new RevealedNumberDto
        {
            Value = l.Value,
            PoolIndex = l.PoolIndex,
            Source = l.Source,
            MetadataJson = l.MetadataJson
        }).ToList();

        var session = new LuckyJourneySession
        {
            GameCode = game.Code,
            LineLabel = string.IsNullOrWhiteSpace(request.LineLabel) ? "A" : request.LineLabel.ToUpper(),
            CurrentStep = 1,
            TotalSteps = stepPools.Count,
            Status = "InProgress",
            ExpectedQuestionId = firstQuestion.Id,
            IsRemix = true,
            LockedNumbers = lockedDtoList
        };

        // Cache StepPools
        session.AnsweredQuestionIds.Add(firstQuestion.Id);
        session.ThemesUsed.Add(firstQuestion.ThemeId);
        session.QuestionTypesUsed.Add(firstQuestion.QuestionType);

        // Use expected step pools
        session.ExpectedQuestionId = firstQuestion.Id;
        
        // We can abuse the line label or custom session state to store stepPools
        session.ThemesUsed = stepPools; // Hack: temporarily borrow themesUsed list to store stepPools or customize LuckyJourneySession

        RemixSessions[session.JourneyId] = session;

        var firstPoolIndex = stepPools[0];
        var firstPool = pools.First(p => p.PoolIndex == firstPoolIndex);

        return new StartJourneyResponse
        {
            JourneyId = session.JourneyId,
            GameCode = game.Code,
            LineLabel = session.LineLabel,
            CurrentStep = 1,
            TotalSteps = session.TotalSteps,
            CurrentPoolIndex = firstPoolIndex,
            CurrentPoolName = firstPool.Name,
            IsClimaxStep = false,
            FirstQuestion = firstQuestion
        };
    }

    public async Task<AnswerStepResponse> AnswerLuckyRemixStepAsync(
        string journeyId,
        AnswerStepRequest request,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!RemixSessions.TryGetValue(journeyId, out var session) || session.Status != "InProgress")
        {
            throw new KeyNotFoundException("Hành trình Lucky Remix không tồn tại hoặc đã hoàn tất.");
        }

        if (session.ExpectedQuestionId != request.QuestionId)
        {
            throw new InvalidOperationException("Câu hỏi trả lời không khớp với bước hiện tại.");
        }

        var game = await _dbContext.Games
            .Include(g => g.Pools)
            .FirstOrDefaultAsync(g => g.Code == session.GameCode, cancellationToken);
        if (game == null)
        {
            throw new InvalidOperationException($"Không tìm thấy trò chơi '{session.GameCode}'.");
        }

        var stepPools = session.ThemesUsed; // retrieved stepPools
        var poolIndex = stepPools[session.CurrentStep - 1];
        var currentPool = game.Pools.First(p => p.PoolIndex == poolIndex);

        // Load choice
        var choice = await _questionRepository.GetChoiceWithDetailsAsync(request.ChoiceId, cancellationToken);
        if (choice == null || choice.QuestionId != request.QuestionId)
        {
            throw new ArgumentException("Lựa chọn không hợp lệ.");
        }

        // Exclude locked numbers and previously generated numbers in same pool
        var excluded = session.LockedNumbers
            .Where(n => n.PoolIndex == poolIndex)
            .Select(n => n.Value)
            .Concat(session.GeneratedNumbers.Where(n => n.PoolIndex == poolIndex).Select(n => n.Value))
            .ToHashSet();

        // Previous numbers in line
        var previousInLine = session.LockedNumbers.Select(n => n.Value)
            .Concat(session.GeneratedNumbers.Select(n => n.Value))
            .ToList();

        var revealed = _luckyEngine.GenerateLuckyNumber(
            currentPool,
            choice,
            excluded,
            previousInLine);

        session.GeneratedNumbers.Add(revealed);

        // Update Lucky DNA Profile
        await _dnaService.UpdateDnaForAnswerAsync(
            userId,
            session.GuestSessionToken,
            request.QuestionId,
            request.ChoiceId,
            journeyId: session.JourneyId,
            cancellationToken);

        bool isCompleted = session.CurrentStep >= session.TotalSteps;

        if (isCompleted)
        {
            session.Status = "Completed";

            // Merge newly generated numbers with locked numbers
            var allNumbers = session.LockedNumbers
                .Concat(session.GeneratedNumbers)
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .ToList();

            RemixSessions.TryRemove(session.JourneyId, out _);

            return new AnswerStepResponse
            {
                JourneyId = session.JourneyId,
                RevealedNumber = revealed,
                CurrentStep = session.CurrentStep,
                TotalSteps = session.TotalSteps,
                CurrentPoolIndex = poolIndex,
                CurrentPoolName = currentPool.Name,
                IsClimaxStep = session.CurrentStep == session.TotalSteps && game.Pools.Count > 1,
                IsCompleted = true,
                CompletedNumbers = allNumbers,
                JourneyCommentary = "Remix hoàn tất! Bộ số mới đã được hình thành."
            };
        }

        // Move to next step
        session.CurrentStep++;
        var nextPoolIndex = stepPools[session.CurrentStep - 1];
        var nextPool = game.Pools.First(p => p.PoolIndex == nextPoolIndex);
        bool isNextClimax = session.CurrentStep == session.TotalSteps && game.Pools.Count > 1;

        var allQuestions = await _questionRepository.GetAllActiveQuestionsAsync(cancellationToken);
        var noveltyCtx = new NoveltyContext
        {
            AnsweredQuestionIdsInJourney = session.AnsweredQuestionIds,
            RecentQuestionIds = request.RecentQuestionIds ?? new List<int>(),
            RecentThemeIds = request.RecentThemeIds ?? new List<int>()
        };

        var nextQuestion = _noveltyEngine.SelectNextQuestion(allQuestions, noveltyCtx, isClimaxStep: isNextClimax);

        session.ExpectedQuestionId = nextQuestion.Id;
        session.AnsweredQuestionIds.Add(nextQuestion.Id);

        return new AnswerStepResponse
        {
            JourneyId = session.JourneyId,
            RevealedNumber = revealed,
            CurrentStep = session.CurrentStep - 1,
            TotalSteps = session.TotalSteps,
            CurrentPoolIndex = nextPoolIndex,
            CurrentPoolName = nextPool.Name,
            IsClimaxStep = isNextClimax,
            IsCompleted = false,
            NextQuestion = nextQuestion
        };
    }
}
