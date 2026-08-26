using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Xunit;

namespace Biplott.Tests;

public class LuckyJourneyIntegrationTests
{
    private class FakeQuestionRepository : IQuestionRepository
    {
        private readonly List<Question> _questions;

        public FakeQuestionRepository(List<Question> questions) => _questions = questions;

        public Task<IReadOnlyList<Question>> GetAllActiveQuestionsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Question>>(_questions.Where(q => q.IsActive).ToList().AsReadOnly());

        public Task<QuestionChoice?> GetChoiceWithDetailsAsync(int choiceId, CancellationToken cancellationToken = default)
        {
            var choice = _questions
                .SelectMany(q => q.Choices)
                .FirstOrDefault(c => c.Id == choiceId);

            return Task.FromResult(choice);
        }
    }

    private static (IGameRepository gameRepo, IQuestionRepository qRepo) CreateMockRepositories()
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
        for (int i = 1; i <= 15; i++)
        {
            var theme = new Theme { Id = i, Code = $"THEME_{i}", Name = $"Chủ đề {i}", Icon = "⭐", IsActive = true };
            var q = new Question
            {
                Id = i,
                ThemeId = i,
                Theme = theme,
                QuestionType = QuestionType.SingleChoice,
                Content = $"Câu hỏi số {i}?",
                IsActive = true,
                Choices = new List<QuestionChoice>
                {
                    new()
                    {
                        Id = i * 10 + 1,
                        QuestionId = i,
                        Content = $"Đáp án 1 cho câu {i}",
                        ChoiceTraits = new() { new() { Trait = new Trait { Code = "Independence", Name = "Độc lập" }, Weight = 0.8 } },
                        IsActive = true
                    },
                    new()
                    {
                        Id = i * 10 + 2,
                        QuestionId = i,
                        Content = $"Đáp án 2 cho câu {i}",
                        ChoiceTraits = new() { new() { Trait = new Trait { Code = "Stability", Name = "Ổn định" }, Weight = 0.5 } },
                        IsActive = true
                    }
                }
            };
            foreach (var c in q.Choices) c.Question = q;
            questions.Add(q);
        }

        var qRepo = new FakeQuestionRepository(questions);
        return (gameRepo, qRepo);
    }

    [Fact]
    public async Task StartJourney_Power655_ShouldReturnTotal6Steps_AndFirstQuestion()
    {
        var (gameRepo, qRepo) = CreateMockRepositories();
        var rng = new DeterministicRandomSource(42);
        var novelty = new NoveltyEngine(rng);
        var luckyEngine = new LuckyNumberEngine(rng);
        var service = new LuckyJourneySessionService(gameRepo, novelty, luckyEngine, qRepo);

        var response = await service.StartJourneyAsync(new StartJourneyRequest
        {
            GameCode = "POWER_655",
            LineLabel = "A"
        });

        Assert.NotNull(response.JourneyId);
        Assert.Equal("POWER_655", response.GameCode);
        Assert.Equal("A", response.LineLabel);
        Assert.Equal(1, response.CurrentStep);
        Assert.Equal(6, response.TotalSteps);
        Assert.NotNull(response.FirstQuestion);
        Assert.NotEmpty(response.FirstQuestion.Choices);
    }

    [Fact]
    public async Task FullJourneyExecution_Power655_ShouldReveal6UniqueNumbers_AndComplete()
    {
        var (gameRepo, qRepo) = CreateMockRepositories();
        var rng = new DeterministicRandomSource(100);
        var novelty = new NoveltyEngine(rng);
        var luckyEngine = new LuckyNumberEngine(rng);
        var service = new LuckyJourneySessionService(gameRepo, novelty, luckyEngine, qRepo);

        var start = await service.StartJourneyAsync(new StartJourneyRequest { GameCode = "POWER_655", LineLabel = "B" });
        string journeyId = start.JourneyId;
        var currentQ = start.FirstQuestion;

        AnswerStepResponse? lastResponse = null;

        for (int step = 1; step <= 6; step++)
        {
            var choice = currentQ.Choices.First();
            lastResponse = await service.AnswerStepAsync(journeyId, new AnswerStepRequest
            {
                QuestionId = currentQ.Id,
                ChoiceId = choice.Id
            });

            Assert.NotNull(lastResponse.RevealedNumber);
            Assert.Equal(NumberSource.Lucky, lastResponse.RevealedNumber.Source);
            Assert.InRange(lastResponse.RevealedNumber.Value, 1, 55);

            if (step < 6)
            {
                Assert.False(lastResponse.IsCompleted);
                Assert.NotNull(lastResponse.NextQuestion);
                currentQ = lastResponse.NextQuestion;
            }
            else
            {
                Assert.True(lastResponse.IsCompleted);
                Assert.Null(lastResponse.NextQuestion);
                Assert.NotNull(lastResponse.CompletedNumbers);
                Assert.Equal(6, lastResponse.CompletedNumbers.Count);

                // All 6 numbers must be distinct in single pool
                var uniqueValues = new HashSet<int>(lastResponse.CompletedNumbers.Select(n => n.Value));
                Assert.Equal(6, uniqueValues.Count);
            }
        }
    }

    [Fact]
    public async Task FullJourneyExecution_Lotto535_ShouldHaveClimaxOnStep6_ForSpecialPool()
    {
        var (gameRepo, qRepo) = CreateMockRepositories();
        var rng = new DeterministicRandomSource(200);
        var novelty = new NoveltyEngine(rng);
        var luckyEngine = new LuckyNumberEngine(rng);
        var service = new LuckyJourneySessionService(gameRepo, novelty, luckyEngine, qRepo);

        var start = await service.StartJourneyAsync(new StartJourneyRequest { GameCode = "LOTTO_535", LineLabel = "C" });
        string journeyId = start.JourneyId;
        var currentQ = start.FirstQuestion;

        AnswerStepResponse? lastResponse = null;

        for (int step = 1; step <= 6; step++)
        {
            var choice = currentQ.Choices.First();
            lastResponse = await service.AnswerStepAsync(journeyId, new AnswerStepRequest
            {
                QuestionId = currentQ.Id,
                ChoiceId = choice.Id
            });

            if (step <= 5)
            {
                Assert.Equal(0, lastResponse.RevealedNumber.PoolIndex);
                Assert.InRange(lastResponse.RevealedNumber.Value, 1, 35);
                currentQ = lastResponse.NextQuestion!;
            }
            else
            {
                // Step 6 is Special pool
                Assert.Equal(1, lastResponse.RevealedNumber.PoolIndex);
                Assert.InRange(lastResponse.RevealedNumber.Value, 1, 12);
                Assert.True(lastResponse.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AnswerStep_WithInvalidChoice_ShouldThrowArgumentException()
    {
        var (gameRepo, qRepo) = CreateMockRepositories();
        var rng = new DeterministicRandomSource(300);
        var service = new LuckyJourneySessionService(gameRepo, new NoveltyEngine(rng), new LuckyNumberEngine(rng), qRepo);

        var start = await service.StartJourneyAsync(new StartJourneyRequest { GameCode = "POWER_655", LineLabel = "A" });

        // Choice 999 does not belong to question
        await Assert.ThrowsAsync<ArgumentException>(() => service.AnswerStepAsync(start.JourneyId, new AnswerStepRequest
        {
            QuestionId = start.FirstQuestion.Id,
            ChoiceId = 999
        }));
    }
}
