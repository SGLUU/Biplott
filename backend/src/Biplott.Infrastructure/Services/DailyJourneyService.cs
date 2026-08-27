using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class DailyJourneyService : IDailyJourneyService
{
    private readonly BiplottDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INoveltyEngine _noveltyEngine;
    private readonly ILuckyNumberEngine _luckyEngine;
    private readonly ILuckyDnaService _dnaService;
    private readonly IQuestionRepository _questionRepository;

    public DailyJourneyService(
        BiplottDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        INoveltyEngine noveltyEngine,
        ILuckyNumberEngine luckyEngine,
        ILuckyDnaService dnaService,
        IQuestionRepository questionRepository)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _noveltyEngine = noveltyEngine;
        _luckyEngine = luckyEngine;
        _dnaService = dnaService;
        _questionRepository = questionRepository;
    }

    public async Task<StartJourneyResponse> StartDailyJourneyAsync(
        StartJourneyRequest request,
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

        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        int totalSteps = pools.Sum(p => p.PickCount);

        var date = _dateTimeProvider.GetCurrentLocalDate();

        // Check if journey exists
        var existing = await _dbContext.DailyJourneys
            .Include(j => j.Numbers)
            .Include(j => j.Answers)
            .FirstOrDefaultAsync(j =>
                (userId != null && j.UserId == userId && j.GameId == game.Id && j.DailyDate == date) ||
                (userId == null && j.GuestSessionToken == request.GuestSessionToken && j.GuestSessionToken != null && j.GameId == game.Id && j.DailyDate == date),
                cancellationToken);

        if (existing != null)
        {
            if (existing.Status == "Completed")
            {
                throw new InvalidOperationException("Hành trình hôm nay đã hoàn thành.");
            }

            // Resume in-progress journey
            var activeQuestion = await GetQuestionDtoAsync(existing.ExpectedQuestionId, cancellationToken);
            var curPool = GetPoolForStep(game, existing.CurrentStep);

            return new StartJourneyResponse
            {
                JourneyId = existing.Id.ToString(),
                GameCode = game.Code,
                LineLabel = request.LineLabel,
                CurrentStep = existing.CurrentStep,
                TotalSteps = existing.TotalSteps,
                CurrentPoolIndex = curPool.PoolIndex,
                CurrentPoolName = curPool.Name,
                IsClimaxStep = existing.CurrentStep == existing.TotalSteps && game.Pools.Count > 1,
                FirstQuestion = activeQuestion ?? throw new InvalidOperationException("Không tìm thấy câu hỏi cho bước tiếp theo.")
            };
        }

        var allQuestions = await _questionRepository.GetAllActiveQuestionsAsync(cancellationToken);
        if (allQuestions.Count == 0)
        {
            throw new InvalidOperationException("Hệ thống chưa có câu hỏi nào được kích hoạt.");
        }

        // Novelty Engine select question
        var userHistory = await GetUserAnsweredHistoryAsync(userId, request.GuestSessionToken, cancellationToken);
        var noveltyCtx = new NoveltyContext
        {
            RecentQuestionIds = userHistory.QuestionIds,
            RecentThemeIds = userHistory.ThemeIds
        };

        var firstQuestion = _noveltyEngine.SelectNextQuestion(allQuestions, noveltyCtx, isClimaxStep: false);

        var journey = new DailyJourney
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GuestSessionToken = request.GuestSessionToken,
            GameId = game.Id,
            DailyDate = date,
            Status = "InProgress",
            CurrentStep = 1,
            TotalSteps = totalSteps,
            ExpectedQuestionId = firstQuestion.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            _dbContext.DailyJourneys.Add(journey);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Concurrency fallback: if duplicate insert, fetch the existing one
            _dbContext.Entry(journey).State = EntityState.Detached;
            var reFetch = await _dbContext.DailyJourneys
                .Include(j => j.Numbers)
                .Include(j => j.Answers)
                .FirstAsync(j =>
                    (userId != null && j.UserId == userId && j.GameId == game.Id && j.DailyDate == date) ||
                    (userId == null && j.GuestSessionToken == request.GuestSessionToken && j.GameId == game.Id && j.DailyDate == date),
                    cancellationToken);

            if (reFetch.Status == "Completed")
            {
                throw new InvalidOperationException("Hành trình hôm nay đã hoàn thành.");
            }

            var activeQuestion = await GetQuestionDtoAsync(reFetch.ExpectedQuestionId, cancellationToken);
            var curPool = GetPoolForStep(game, reFetch.CurrentStep);

            return new StartJourneyResponse
            {
                JourneyId = reFetch.Id.ToString(),
                GameCode = game.Code,
                LineLabel = request.LineLabel,
                CurrentStep = reFetch.CurrentStep,
                TotalSteps = reFetch.TotalSteps,
                CurrentPoolIndex = curPool.PoolIndex,
                CurrentPoolName = curPool.Name,
                IsClimaxStep = reFetch.CurrentStep == reFetch.TotalSteps && game.Pools.Count > 1,
                FirstQuestion = activeQuestion ?? throw new InvalidOperationException("Không tìm thấy câu hỏi cho bước tiếp theo.")
            };
        }

        var firstPool = GetPoolForStep(game, 1);

        return new StartJourneyResponse
        {
            JourneyId = journey.Id.ToString(),
            GameCode = game.Code,
            LineLabel = request.LineLabel,
            CurrentStep = 1,
            TotalSteps = totalSteps,
            CurrentPoolIndex = firstPool.PoolIndex,
            CurrentPoolName = firstPool.Name,
            IsClimaxStep = false,
            FirstQuestion = firstQuestion
        };
    }

    public async Task<AnswerStepResponse> AnswerDailyStepAsync(
        Guid journeyId,
        AnswerStepRequest request,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var journey = await _dbContext.DailyJourneys
            .Include(j => j.Numbers)
            .Include(j => j.Answers)
            .Include(j => j.Game)
                .ThenInclude(g => g.Pools)
            .FirstOrDefaultAsync(j => j.Id == journeyId, cancellationToken);

        if (journey == null || journey.Status != "InProgress")
        {
            throw new KeyNotFoundException("Hành trình Daily Journey không tồn tại hoặc đã hoàn tất.");
        }

        if (journey.ExpectedQuestionId != request.QuestionId)
        {
            throw new InvalidOperationException("Câu hỏi trả lời không khớp với bước hiện tại.");
        }

        var game = journey.Game;
        var currentPool = GetPoolForStep(game, journey.CurrentStep);

        // Load choice from DB
        var choice = await _questionRepository.GetChoiceWithDetailsAsync(request.ChoiceId, cancellationToken);
        if (choice == null || choice.QuestionId != request.QuestionId)
        {
            throw new ArgumentException("Lựa chọn không hợp lệ.");
        }

        // Generate lucky number
        var excludedInPool = journey.Numbers
            .Where(n => n.PoolIndex == currentPool.PoolIndex)
            .Select(n => n.Value)
            .ToHashSet();

        var previousInLine = journey.Numbers.Select(n => n.Value).ToList();

        var revealed = _luckyEngine.GenerateLuckyNumber(
            currentPool,
            choice,
            excludedInPool,
            previousInLine);

        // Save revealed number to journey
        var journeyNum = new DailyJourneyNumber
        {
            Id = Guid.NewGuid(),
            DailyJourneyId = journey.Id,
            Value = revealed.Value,
            PoolIndex = currentPool.PoolIndex,
            OrderIndex = journey.CurrentStep - 1,
            DominantTrait = revealed.DominantTrait,
            Explanation = revealed.Explanation
        };
        _dbContext.DailyJourneyNumbers.Add(journeyNum);

        // Save answer snapshot
        var themeName = choice.Question?.Theme?.Name ?? "Chung";
        var journeyAns = new DailyJourneyAnswer
        {
            Id = Guid.NewGuid(),
            DailyJourneyId = journey.Id,
            QuestionId = request.QuestionId,
            ChoiceId = request.ChoiceId,
            StepIndex = journey.CurrentStep,
            QuestionContent = choice.Question?.Content ?? string.Empty,
            ChoiceContent = choice.Content,
            ThemeName = themeName,
            Subtitle = choice.Question?.Subtitle
        };
        _dbContext.DailyJourneyAnswers.Add(journeyAns);

        // Update Lucky DNA Profile
        await _dnaService.UpdateDnaForAnswerAsync(
            userId,
            journey.GuestSessionToken,
            request.QuestionId,
            request.ChoiceId,
            journeyId: journey.Id.ToString(),
            cancellationToken);

        bool isCompleted = journey.CurrentStep >= journey.TotalSteps;

        if (isCompleted)
        {
            journey.Status = "Completed";
            journey.ExpectedQuestionId = 0;
            journey.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var sortedNumbers = journey.Numbers
                .Concat(new[] { journeyNum }) // include the last generated
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .Select((n, idx) => new RevealedNumberDto
                {
                    Value = n.Value,
                    PoolIndex = n.PoolIndex,
                    Source = NumberSource.Lucky,
                    Explanation = n.Explanation ?? string.Empty,
                    DominantTrait = n.DominantTrait,
                    ThemeName = n.Explanation != null ? themeName : null
                }).ToList();

            return new AnswerStepResponse
            {
                JourneyId = journey.Id.ToString(),
                RevealedNumber = new RevealedNumberDto
                {
                    Value = revealed.Value,
                    PoolIndex = currentPool.PoolIndex,
                    Source = NumberSource.Lucky,
                    Explanation = revealed.Explanation,
                    DominantTrait = revealed.DominantTrait,
                    ThemeName = themeName
                },
                CurrentStep = journey.CurrentStep,
                TotalSteps = journey.TotalSteps,
                CurrentPoolIndex = currentPool.PoolIndex,
                CurrentPoolName = currentPool.Name,
                IsClimaxStep = journey.CurrentStep == journey.TotalSteps && game.Pools.Count > 1,
                IsCompleted = true,
                CompletedNumbers = sortedNumbers,
                JourneyCommentary = "Hành trình hôm nay đã hoàn thành! Dưới đây là bộ số định mệnh dành cho bạn."
            };
        }

        // Move to next step
        journey.CurrentStep++;
        journey.UpdatedAt = DateTime.UtcNow;

        var nextPool = GetPoolForStep(game, journey.CurrentStep);
        bool isNextClimax = journey.CurrentStep == journey.TotalSteps && game.Pools.Count > 1;

        var allQuestions = await _questionRepository.GetAllActiveQuestionsAsync(cancellationToken);
        var userHistory = await GetUserAnsweredHistoryAsync(userId, journey.GuestSessionToken, cancellationToken);

        var noveltyCtx = new NoveltyContext
        {
            AnsweredQuestionIdsInJourney = journey.Answers.Select(a => a.QuestionId).Concat(new[] { request.QuestionId }).ToList(),
            ThemesUsedInJourney = journey.Answers.Select(a => choice.Question?.ThemeId ?? 0).ToList(),
            RecentQuestionIds = userHistory.QuestionIds,
            RecentThemeIds = userHistory.ThemeIds
        };

        var nextQuestion = _noveltyEngine.SelectNextQuestion(allQuestions, noveltyCtx, isClimaxStep: isNextClimax);
        journey.ExpectedQuestionId = nextQuestion.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AnswerStepResponse
        {
            JourneyId = journey.Id.ToString(),
            RevealedNumber = new RevealedNumberDto
            {
                Value = revealed.Value,
                PoolIndex = currentPool.PoolIndex,
                Source = NumberSource.Lucky,
                Explanation = revealed.Explanation,
                DominantTrait = revealed.DominantTrait,
                ThemeName = themeName
            },
            CurrentStep = journey.CurrentStep - 1, // return step that was just completed
            TotalSteps = journey.TotalSteps,
            CurrentPoolIndex = nextPool.PoolIndex,
            CurrentPoolName = nextPool.Name,
            IsClimaxStep = isNextClimax,
            IsCompleted = false,
            NextQuestion = nextQuestion
        };
    }

    public async Task<DailyJourneyDto?> GetTodayDailyJourneyAsync(
        string gameCode,
        string? userId = null,
        string? guestSessionToken = null,
        CancellationToken cancellationToken = default)
    {
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Code == gameCode, cancellationToken);
        if (game == null) return null;

        var date = _dateTimeProvider.GetCurrentLocalDate();

        var journey = await _dbContext.DailyJourneys
            .Include(j => j.Numbers)
            .Include(j => j.Answers)
            .FirstOrDefaultAsync(j =>
                (userId != null && j.UserId == userId && j.GameId == game.Id && j.DailyDate == date) ||
                (userId == null && j.GuestSessionToken == guestSessionToken && j.GuestSessionToken != null && j.GameId == game.Id && j.DailyDate == date),
                cancellationToken);

        if (journey == null) return null;

        var result = new DailyJourneyDto
        {
            JourneyId = journey.Id,
            GameCode = gameCode,
            DailyDate = journey.DailyDate,
            Status = journey.Status,
            CurrentStep = journey.CurrentStep,
            TotalSteps = journey.TotalSteps,
            Numbers = journey.Numbers
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .Select(n => new RevealedNumberDto
                {
                    Value = n.Value,
                    PoolIndex = n.PoolIndex,
                    Source = NumberSource.Lucky,
                    Explanation = n.Explanation ?? string.Empty,
                    DominantTrait = n.DominantTrait
                }).ToList(),
            Answers = journey.Answers.Select(a => new DailyJourneyAnswerDto
            {
                QuestionId = a.QuestionId,
                ChoiceId = a.ChoiceId,
                StepIndex = a.StepIndex,
                QuestionContent = a.QuestionContent,
                ChoiceContent = a.ChoiceContent,
                ThemeName = a.ThemeName,
                Subtitle = a.Subtitle
            }).ToList()
        };

        if (journey.Status == "InProgress")
        {
            result.ActiveQuestion = await GetQuestionDtoAsync(journey.ExpectedQuestionId, cancellationToken);
        }

        return result;
    }

    private static GamePool GetPoolForStep(Game game, int step)
    {
        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        int accumulated = 0;

        foreach (var pool in pools)
        {
            accumulated += pool.PickCount;
            if (step <= accumulated) return pool;
        }

        return pools.Last();
    }

    private async Task<QuestionDto?> GetQuestionDtoAsync(int questionId, CancellationToken cancellationToken)
    {
        if (questionId <= 0) return null;
        var q = await _dbContext.Questions
            .Include(x => x.Theme)
            .Include(x => x.Choices)
            .FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken);

        if (q == null) return null;

        return new QuestionDto
        {
            Id = q.Id,
            ThemeId = q.ThemeId,
            ThemeCode = q.Theme.Code,
            ThemeName = q.Theme.Name,
            ThemeIcon = q.Theme.Icon,
            QuestionType = q.QuestionType,
            Content = q.Content,
            Subtitle = q.Subtitle,
            MediaUrl = q.MediaUrl,
            Choices = q.Choices.Where(c => c.IsActive).OrderBy(c => c.OrderIndex).Select(c => new ChoiceDto
            {
                Id = c.Id,
                Content = c.Content,
                SubContent = c.SubContent,
                MediaUrl = c.MediaUrl,
                OrderIndex = c.OrderIndex
            }).ToList()
        };
    }

    private async Task<(List<int> QuestionIds, List<int> ThemeIds)> GetUserAnsweredHistoryAsync(
        string? userId,
        string? guestSessionToken,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserQuestionHistories.AsQueryable();
        if (userId != null)
        {
            query = query.Where(h => h.UserId == userId);
        }
        else if (guestSessionToken != null)
        {
            query = query.Where(h => h.GuestSessionToken == guestSessionToken);
        }
        else
        {
            return (new List<int>(), new List<int>());
        }

        var records = await query
            .Select(h => new { h.QuestionId, h.Question.ThemeId })
            .ToListAsync(cancellationToken);

        return (records.Select(r => r.QuestionId).Distinct().ToList(),
                records.Select(r => r.ThemeId).Distinct().ToList());
    }
}
