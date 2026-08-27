using System.Collections.Concurrent;
using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface ILuckyJourneySessionService
{
    Task<StartJourneyResponse> StartJourneyAsync(StartJourneyRequest request, CancellationToken cancellationToken = default);
    Task<AnswerStepResponse> AnswerStepAsync(string journeyId, AnswerStepRequest request, CancellationToken cancellationToken = default);
    Task CancelJourneyAsync(string journeyId);
}

public class LuckyJourneySession
{
    public string JourneyId { get; set; } = Guid.NewGuid().ToString();
    public string GameCode { get; set; } = string.Empty;
    public string LineLabel { get; set; } = "A";
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 6;
    public string Status { get; set; } = "InProgress"; // InProgress | Completed | Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RevealedNumberDto> GeneratedNumbers { get; set; } = new();
    public List<int> AnsweredQuestionIds { get; set; } = new();
    public List<int> ThemesUsed { get; set; } = new();
    public List<QuestionType> QuestionTypesUsed { get; set; } = new();
    public int ExpectedQuestionId { get; set; }
}

public class LuckyJourneySessionService : ILuckyJourneySessionService
{
    private static readonly ConcurrentDictionary<string, LuckyJourneySession> Sessions = new();

    private readonly IGameRepository _gameRepository;
    private readonly INoveltyEngine _noveltyEngine;
    private readonly ILuckyNumberEngine _luckyEngine;
    private readonly IQuestionRepository _questionRepository;

    public LuckyJourneySessionService(
        IGameRepository gameRepository,
        INoveltyEngine noveltyEngine,
        ILuckyNumberEngine luckyEngine,
        IQuestionRepository questionRepository)
    {
        _gameRepository = gameRepository;
        _noveltyEngine = noveltyEngine;
        _luckyEngine = luckyEngine;
        _questionRepository = questionRepository;
    }

    public async Task<StartJourneyResponse> StartJourneyAsync(StartJourneyRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        int totalSteps = pools.Sum(p => p.PickCount);

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

        var session = new LuckyJourneySession
        {
            GameCode = game.Code,
            LineLabel = string.IsNullOrWhiteSpace(request.LineLabel) ? "A" : request.LineLabel.ToUpper(),
            CurrentStep = 1,
            TotalSteps = totalSteps,
            Status = "InProgress",
            ExpectedQuestionId = firstQuestion.Id
        };

        session.AnsweredQuestionIds.Add(firstQuestion.Id);
        session.ThemesUsed.Add(firstQuestion.ThemeId);
        session.QuestionTypesUsed.Add(firstQuestion.QuestionType);

        Sessions[session.JourneyId] = session;

        var currentPool = GetPoolForStep(game, 1);

        return new StartJourneyResponse
        {
            JourneyId = session.JourneyId,
            GameCode = game.Code,
            LineLabel = session.LineLabel,
            CurrentStep = 1,
            TotalSteps = totalSteps,
            CurrentPoolIndex = currentPool.PoolIndex,
            CurrentPoolName = currentPool.Name,
            IsClimaxStep = false,
            FirstQuestion = firstQuestion
        };
    }

    public async Task<AnswerStepResponse> AnswerStepAsync(string journeyId, AnswerStepRequest request, CancellationToken cancellationToken = default)
    {
        if (!Sessions.TryGetValue(journeyId, out var session) || session.Status != "InProgress")
        {
            throw new KeyNotFoundException("Phiên Lucky Journey không tồn tại hoặc đã kết thúc.");
        }

        if (session.ExpectedQuestionId != 0 && session.ExpectedQuestionId != request.QuestionId)
        {
            throw new InvalidOperationException("Câu hỏi trả lời không khớp với bước hiện tại của hành trình.");
        }

        var game = await _gameRepository.GetByCodeAsync(session.GameCode, cancellationToken);
        if (game == null)
        {
            throw new InvalidOperationException($"Không tìm thấy trò chơi '{session.GameCode}'.");
        }

        var currentPool = GetPoolForStep(game, session.CurrentStep);

        // Load choice from DB
        var choice = await _questionRepository.GetChoiceWithDetailsAsync(request.ChoiceId, cancellationToken);
        if (choice == null || choice.QuestionId != request.QuestionId)
        {
            throw new ArgumentException("Đáp án không hợp lệ hoặc không thuộc về câu hỏi này.");
        }

        // Exclude previously generated numbers in the same pool for this journey
        var excludedInPool = session.GeneratedNumbers
            .Where(n => n.PoolIndex == currentPool.PoolIndex)
            .Select(n => n.Value)
            .ToHashSet();

        var previousInLine = session.GeneratedNumbers.Select(n => n.Value).ToList();

        // Reveal number via Candidate Scoring & Weighted Sampling
        var revealed = _luckyEngine.GenerateLuckyNumber(
            currentPool,
            choice,
            excludedInPool,
            previousInLine);

        session.GeneratedNumbers.Add(revealed);

        bool isCompleted = session.CurrentStep >= session.TotalSteps;

        if (isCompleted)
        {
            session.Status = "Completed";

            // Clean commentary
            string commentary = $"Hành trình hoàn tất! Bạn đã mở đủ {session.TotalSteps} con số may mắn qua các cung bậc cảm xúc.";

            var sortedCompletedNumbers = session.GeneratedNumbers
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .ToList();

            return new AnswerStepResponse
            {
                JourneyId = session.JourneyId,
                RevealedNumber = revealed,
                CurrentStep = session.CurrentStep,
                TotalSteps = session.TotalSteps,
                CurrentPoolIndex = currentPool.PoolIndex,
                CurrentPoolName = currentPool.Name,
                IsClimaxStep = session.CurrentStep == session.TotalSteps && game.Pools.Count > 1,
                IsCompleted = true,
                NextQuestion = null,
                CompletedNumbers = sortedCompletedNumbers,
                JourneyCommentary = commentary
            };
        }

        // Move to next step
        session.CurrentStep++;
        var nextPool = GetPoolForStep(game, session.CurrentStep);
        bool isNextClimax = session.CurrentStep == session.TotalSteps && game.Pools.Count > 1;

        var allQuestions = await _questionRepository.GetAllActiveQuestionsAsync(cancellationToken);

        var noveltyCtx = new NoveltyContext
        {
            AnsweredQuestionIdsInJourney = session.AnsweredQuestionIds,
            ThemesUsedInJourney = session.ThemesUsed,
            QuestionTypesUsedInJourney = session.QuestionTypesUsed,
            RecentQuestionIds = request.RecentQuestionIds ?? new List<int>(),
            RecentThemeIds = request.RecentThemeIds ?? new List<int>()
        };

        var nextQuestion = _noveltyEngine.SelectNextQuestion(allQuestions, noveltyCtx, isClimaxStep: isNextClimax);

        session.ExpectedQuestionId = nextQuestion.Id;
        session.AnsweredQuestionIds.Add(nextQuestion.Id);
        session.ThemesUsed.Add(nextQuestion.ThemeId);
        session.QuestionTypesUsed.Add(nextQuestion.QuestionType);

        return new AnswerStepResponse
        {
            JourneyId = session.JourneyId,
            RevealedNumber = revealed,
            CurrentStep = session.CurrentStep,
            TotalSteps = session.TotalSteps,
            CurrentPoolIndex = nextPool.PoolIndex,
            CurrentPoolName = nextPool.Name,
            IsClimaxStep = isNextClimax,
            IsCompleted = false,
            NextQuestion = nextQuestion
        };
    }

    public Task CancelJourneyAsync(string journeyId)
    {
        Sessions.TryRemove(journeyId, out _);
        return Task.CompletedTask;
    }

    private static GamePool GetPoolForStep(Game game, int step)
    {
        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        int accumulated = 0;

        foreach (var pool in pools)
        {
            accumulated += pool.PickCount;
            if (step <= accumulated)
            {
                return pool;
            }
        }

        return pools.Last();
    }
}
