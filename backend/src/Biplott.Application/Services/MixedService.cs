using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface IMixedService
{
    Task<GenerateRandomSlotResponse> GenerateRandomSlotAsync(GenerateRandomSlotRequest request, CancellationToken cancellationToken = default);
    Task<GetMixedLuckyQuestionResponse> GetMixedLuckyQuestionAsync(GetMixedLuckyQuestionRequest request, CancellationToken cancellationToken = default);
    Task<AnswerMixedLuckySlotResponse> AnswerMixedLuckySlotAsync(AnswerMixedLuckySlotRequest request, CancellationToken cancellationToken = default);
    Task<FillRemainderResponse> FillRemainderAsync(FillRemainderRequest request, CancellationToken cancellationToken = default);
}

public class MixedService : IMixedService
{
    private readonly IGameRepository _gameRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRandomNumberEngine _randomEngine;
    private readonly ILuckyNumberEngine _luckyEngine;
    private readonly INoveltyEngine _noveltyEngine;
    private readonly IRandomSource _randomSource;

    public MixedService(
        IGameRepository gameRepository,
        IQuestionRepository questionRepository,
        IRandomNumberEngine randomEngine,
        ILuckyNumberEngine luckyEngine,
        INoveltyEngine noveltyEngine,
        IRandomSource randomSource)
    {
        _gameRepository = gameRepository;
        _questionRepository = questionRepository;
        _randomEngine = randomEngine;
        _luckyEngine = luckyEngine;
        _noveltyEngine = noveltyEngine;
        _randomSource = randomSource;
    }

    public async Task<GenerateRandomSlotResponse> GenerateRandomSlotAsync(
        GenerateRandomSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var pool = game.Pools.FirstOrDefault(p => p.PoolIndex == request.PoolIndex);
        if (pool == null)
        {
            throw new ArgumentException($"Không tìm thấy tập số (PoolIndex: {request.PoolIndex}) cho game '{request.GameCode}'.");
        }

        var excluded = request.ExcludedNumbers != null
            ? new HashSet<int>(request.ExcludedNumbers)
            : new HashSet<int>();

        // Generate 1 number for this pool excluding already selected numbers
        var generatedNumbers = _randomEngine.GeneratePoolNumbers(pool, 1, request.Strategy, excluded);
        int selectedValue = generatedNumbers[0].Value;

        string strategyName = request.Strategy switch
        {
            RandomStrategy.PureRandom => "Pure Random",
            RandomStrategy.Balanced => "Balanced",
            RandomStrategy.Spread => "Spread",
            RandomStrategy.Surprise => "Surprise",
            _ => "Thần Tài"
        };

        return new GenerateRandomSlotResponse
        {
            Number = new GeneratedNumberDto
            {
                Value = selectedValue,
                PoolIndex = pool.PoolIndex,
                Source = NumberSource.Random,
                MetadataJson = $"{{\"strategy\":\"{request.Strategy}\"}}"
            },
            Strategy = request.Strategy,
            StrategyName = strategyName,
            Commentary = $"Thần Tài ({strategyName}) đã chọn số {selectedValue:D2} cho bạn!"
        };
    }

    public async Task<GetMixedLuckyQuestionResponse> GetMixedLuckyQuestionAsync(
        GetMixedLuckyQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
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

        var question = _noveltyEngine.SelectNextQuestion(
            allQuestions,
            noveltyCtx,
            isClimaxStep: request.IsClimaxStep);

        return new GetMixedLuckyQuestionResponse
        {
            Question = question
        };
    }

    public async Task<AnswerMixedLuckySlotResponse> AnswerMixedLuckySlotAsync(
        AnswerMixedLuckySlotRequest request,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var pool = game.Pools.FirstOrDefault(p => p.PoolIndex == request.PoolIndex);
        if (pool == null)
        {
            throw new ArgumentException($"Không tìm thấy tập số (PoolIndex: {request.PoolIndex}) cho game '{request.GameCode}'.");
        }

        var choice = await _questionRepository.GetChoiceWithDetailsAsync(request.ChoiceId, cancellationToken);
        if (choice == null || choice.QuestionId != request.QuestionId)
        {
            throw new ArgumentException("Đáp án không hợp lệ hoặc không thuộc về câu hỏi này.");
        }

        var excludedInPool = request.ExcludedNumbers != null
            ? new HashSet<int>(request.ExcludedNumbers)
            : new HashSet<int>();

        var previousInLine = request.PreviousNumbersInLine ?? new List<int>();

        var revealed = _luckyEngine.GenerateLuckyNumber(
            pool,
            choice,
            excludedInPool,
            previousInLine);

        return new AnswerMixedLuckySlotResponse
        {
            RevealedNumber = revealed
        };
    }

    public async Task<FillRemainderResponse> FillRemainderAsync(
        FillRemainderRequest request,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var resultNumbers = new List<GeneratedNumberDto>();

        foreach (var pool in game.Pools.OrderBy(p => p.PoolIndex))
        {
            var existingInPool = request.ExistingNumbers
                .Where(n => n.PoolIndex == pool.PoolIndex)
                .ToList();

            // Preserve all existing numbers in this pool
            resultNumbers.AddRange(existingInPool);

            int remainingNeeded = pool.PickCount - existingInPool.Count;
            if (remainingNeeded > 0)
            {
                var excludedValues = existingInPool.Select(n => n.Value).ToHashSet();

                var generatedValues = _randomEngine.GeneratePoolNumbers(
                    pool,
                    remainingNeeded,
                    request.Strategy,
                    excludedValues);

                resultNumbers.AddRange(generatedValues);
            }
        }

        // Sort by PoolIndex ascending, then Value ascending within each pool
        var sorted = resultNumbers
            .OrderBy(n => n.PoolIndex)
            .ThenBy(n => n.Value)
            .ToList();

        return new FillRemainderResponse
        {
            GameCode = game.Code,
            Strategy = request.Strategy,
            Numbers = sorted,
            Commentary = $"Đã điền đủ {sorted.Count} số cho phiếu bằng Thần Tài ({request.Strategy})!"
        };
    }
}
