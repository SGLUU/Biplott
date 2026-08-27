using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Xunit;

namespace Biplott.Tests;

public class MixedServiceTests
{
    private static (IGameRepository gameRepo, IQuestionRepository qRepo, IMixedService service) CreateMockService()
    {
        var powerGame = new Game
        {
            Code = "POWER_655",
            Name = "Power 6/55",
            IsActive = true,
            Pools = new List<GamePool>
            {
                new() { PoolIndex = 0, Name = "Dãy số chính", MinNumber = 1, MaxNumber = 55, PickCount = 6, AllowDuplicates = false }
            }
        };

        var lottoGame = new Game
        {
            Code = "LOTTO_535",
            Name = "Lotto 5/35",
            IsActive = true,
            Pools = new List<GamePool>
            {
                new() { PoolIndex = 0, Name = "Dãy số chính", MinNumber = 1, MaxNumber = 35, PickCount = 5, AllowDuplicates = false },
                new() { PoolIndex = 1, Name = "Số đặc biệt", MinNumber = 1, MaxNumber = 12, PickCount = 1, AllowDuplicates = false }
            }
        };

        var games = new List<Game> { powerGame, lottoGame };
        var gameRepo = new SlipServiceTests.FakeGameRepository(games);

        var questions = new List<Question>();
        for (int i = 1; i <= 5; i++)
        {
            var theme = new Theme { Id = i, Code = $"THEME_{i}", Name = $"Theme {i}", IsActive = true };
            var q = new Question
            {
                Id = i,
                ThemeId = i,
                Theme = theme,
                QuestionType = QuestionType.SingleChoice,
                Content = $"Question {i}?",
                IsActive = true,
                Choices = new List<QuestionChoice>
                {
                    new()
                    {
                        Id = i * 10 + 1,
                        QuestionId = i,
                        Content = $"Choice 1 of Q{i}",
                        ChoiceTraits = new() { new() { Trait = new Trait { Code = "Independence", Name = "Độc lập" }, Weight = 0.9 } },
                        IsActive = true
                    }
                }
            };
            foreach (var c in q.Choices) c.Question = q;
            questions.Add(q);
        }

        var qRepo = new LuckyJourneyIntegrationTestsFakeQuestionRepo(questions);
        var rng = new DeterministicRandomSource(100);
        var randomEngine = new RandomNumberEngine();
        var luckyEngine = new LuckyNumberEngine(rng);
        var noveltyEngine = new NoveltyEngine(rng);

        var service = new MixedService(
            gameRepo,
            qRepo,
            randomEngine,
            luckyEngine,
            noveltyEngine,
            rng);

        return (gameRepo, qRepo, service);
    }

    private class LuckyJourneyIntegrationTestsFakeQuestionRepo : IQuestionRepository
    {
        private readonly List<Question> _questions;

        public LuckyJourneyIntegrationTestsFakeQuestionRepo(List<Question> questions) => _questions = questions;

        public Task<IReadOnlyList<Question>> GetAllActiveQuestionsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Question>>(_questions.Where(q => q.IsActive).ToList().AsReadOnly());

        public Task<QuestionChoice?> GetChoiceWithDetailsAsync(int choiceId, CancellationToken cancellationToken = default)
        {
            var choice = _questions.SelectMany(q => q.Choices).FirstOrDefault(c => c.Id == choiceId);
            return Task.FromResult(choice);
        }
    }

    [Fact]
    public async Task GenerateRandomSlot_ShouldExclude_AlreadySelectedNumbers()
    {
        var (_, _, service) = CreateMockService();

        var excluded = new List<int> { 1, 2, 3, 4, 5 };
        var response = await service.GenerateRandomSlotAsync(new GenerateRandomSlotRequest
        {
            GameCode = "POWER_655",
            PoolIndex = 0,
            Strategy = RandomStrategy.PureRandom,
            ExcludedNumbers = excluded
        });

        Assert.NotNull(response.Number);
        Assert.Equal(NumberSource.Random, response.Number.Source);
        Assert.InRange(response.Number.Value, 1, 55);
        Assert.DoesNotContain(response.Number.Value, excluded);
    }

    [Fact]
    public async Task AnswerMixedLuckySlot_ShouldGenerateValidLuckyNumber_AndExcludeUsed()
    {
        var (_, _, service) = CreateMockService();

        var excluded = new List<int> { 10, 20, 30 };
        var response = await service.AnswerMixedLuckySlotAsync(new AnswerMixedLuckySlotRequest
        {
            GameCode = "POWER_655",
            PoolIndex = 0,
            QuestionId = 1,
            ChoiceId = 11,
            ExcludedNumbers = excluded
        });

        Assert.NotNull(response.RevealedNumber);
        Assert.Equal(NumberSource.Lucky, response.RevealedNumber.Source);
        Assert.InRange(response.RevealedNumber.Value, 1, 55);
        Assert.DoesNotContain(response.RevealedNumber.Value, excluded);
    }

    [Fact]
    public async Task FillRemainder_Power655_ShouldPreserveExistingNumbers_AndFillRest()
    {
        var (_, _, service) = CreateMockService();

        var existing = new List<GeneratedNumberDto>
        {
            new() { Value = 8, PoolIndex = 0, Source = NumberSource.Manual },
            new() { Value = 17, PoolIndex = 0, Source = NumberSource.Lucky }
        };

        var response = await service.FillRemainderAsync(new FillRemainderRequest
        {
            GameCode = "POWER_655",
            Strategy = RandomStrategy.Balanced,
            ExistingNumbers = existing
        });

        Assert.Equal(6, response.Numbers.Count);

        // Verify existing 8 (Manual) and 17 (Lucky) are preserved
        var num8 = response.Numbers.FirstOrDefault(n => n.Value == 8);
        var num17 = response.Numbers.FirstOrDefault(n => n.Value == 17);

        Assert.NotNull(num8);
        Assert.Equal(NumberSource.Manual, num8.Source);

        Assert.NotNull(num17);
        Assert.Equal(NumberSource.Lucky, num17.Source);

        // Verify all 6 numbers are distinct
        var unique = new HashSet<int>(response.Numbers.Select(n => n.Value));
        Assert.Equal(6, unique.Count);

        // The remaining 4 numbers must have Source = Random
        var randomCount = response.Numbers.Count(n => n.Source == NumberSource.Random);
        Assert.Equal(4, randomCount);
    }

    [Fact]
    public async Task FillRemainder_Lotto535_ShouldFillMainAndSpecialIndependently()
    {
        var (_, _, service) = CreateMockService();

        // 2 main numbers exist, 0 special numbers exist
        var existing = new List<GeneratedNumberDto>
        {
            new() { Value = 7, PoolIndex = 0, Source = NumberSource.Manual },
            new() { Value = 14, PoolIndex = 0, Source = NumberSource.Lucky }
        };

        var response = await service.FillRemainderAsync(new FillRemainderRequest
        {
            GameCode = "LOTTO_535",
            Strategy = RandomStrategy.PureRandom,
            ExistingNumbers = existing
        });

        Assert.Equal(6, response.Numbers.Count);

        var mainNumbers = response.Numbers.Where(n => n.PoolIndex == 0).ToList();
        var specialNumbers = response.Numbers.Where(n => n.PoolIndex == 1).ToList();

        Assert.Equal(5, mainNumbers.Count);
        Assert.Single(specialNumbers);

        Assert.All(mainNumbers, n => Assert.InRange(n.Value, 1, 35));
        Assert.All(specialNumbers, n => Assert.InRange(n.Value, 1, 12));

        var mainUnique = new HashSet<int>(mainNumbers.Select(n => n.Value));
        Assert.Equal(5, mainUnique.Count);

        // Main numbers must be sorted ascending
        for (int j = 1; j < mainNumbers.Count; j++)
        {
            Assert.True(mainNumbers[j].Value > mainNumbers[j - 1].Value, "Main numbers in Lotto 5/35 must be sorted ascending");
        }
    }
}
