using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Biplott.Tests;

public class Phase5IntegrationTests
{
    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime CustomUtcNow { get; set; } = DateTime.UtcNow;
        public string CustomLocalDate { get; set; } = "2026-08-27";

        public DateTime UtcNow => CustomUtcNow;

        public string GetCurrentLocalDate(string timezoneId = "Asia/Ho_Chi_Minh")
        {
            return CustomLocalDate;
        }
    }

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

    private static async Task<(BiplottDbContext db, FakeDateTimeProvider clock, LuckyDnaService dnaService, DailyJourneyService dailyService, RemixService remixService)> CreateTestContextAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BiplottDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<BiplottDbContext>();

        // Seed data
        var trait1 = new Trait { Id = 1, Code = "RiskTolerance", Name = "Risk Tolerance", IsActive = true };
        var trait2 = new Trait { Id = 2, Code = "ChaosEnergy", Name = "Chaos Energy", IsActive = true };
        db.Traits.AddRange(trait1, trait2);

        var game = new Game
        {
            Id = 1,
            Code = "POWER_655",
            Name = "Power 6/55",
            IsActive = true,
            Pools = new List<GamePool>
            {
                new() { Id = 1, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 55, PickCount = 6, AllowDuplicates = false }
            }
        };
        db.Games.Add(game);

        var theme = new Theme { Id = 1, Code = "THEME1", Name = "Theme 1", IsActive = true };
        db.Themes.Add(theme);

        var question = new Question
        {
            Id = 1,
            ThemeId = 1,
            Content = "Question 1?",
            IsActive = true,
            Choices = new List<QuestionChoice>
            {
                new()
                {
                    Id = 1,
                    QuestionId = 1,
                    Content = "Choice 1",
                    IsActive = true,
                    ChoiceTraits = new List<ChoiceTrait>
                    {
                        new() { TraitId = 1, Weight = 0.8 },
                        new() { TraitId = 2, Weight = 0.2 }
                    }
                }
            }
        };
        db.Questions.Add(question);
        var user = new ApplicationUser { Id = "user-1", UserName = "user1", Email = "user1@biplott.local" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var clock = new FakeDateTimeProvider();
        var dnaService = new LuckyDnaService(db);
        var rng = new DeterministicRandomSource(42);
        var novelty = new NoveltyEngine(rng);
        var luckyEngine = new LuckyNumberEngine(rng);
        var qRepo = new FakeQuestionRepository(new List<Question> { question });

        var dailyService = new DailyJourneyService(db, clock, novelty, luckyEngine, dnaService, qRepo);

        var randomEngine = new RandomNumberEngine();
        var remixService = new RemixService(db, randomEngine, novelty, luckyEngine, dnaService, qRepo);

        return (db, clock, dnaService, dailyService, remixService);
    }

    [Fact]
    public async Task LuckyDna_UpdateAndRetrieve_ShouldAccumulateCorrectly()
    {
        var (db, clock, dnaService, _, _) = await CreateTestContextAsync();

        // Update DNA for choice 1
        await dnaService.UpdateDnaForAnswerAsync(
            userId: "user-1",
            guestSessionToken: null,
            questionId: 1,
            choiceId: 1,
            journeyId: "journey-1");

        var dna = await dnaService.GetUserDnaAsync("user-1");

        Assert.Equal("Forming", dna.Status);
        Assert.Equal(1, dna.TotalAnswers);
        
        var riskTrait = dna.AllTraits.First(t => t.TraitCode == "RiskTolerance");
        Assert.Equal(80, riskTrait.Score); // 0.8 * 100
        Assert.Equal(1, riskTrait.SampleCount);
    }

    [Fact]
    public async Task LuckyDna_Idempotency_ShouldNotDoubleCount()
    {
        var (db, clock, dnaService, _, _) = await CreateTestContextAsync();

        // Update same journey step twice
        await dnaService.UpdateDnaForAnswerAsync(
            userId: "user-1",
            guestSessionToken: null,
            questionId: 1,
            choiceId: 1,
            journeyId: "journey-1");

        await dnaService.UpdateDnaForAnswerAsync(
            userId: "user-1",
            guestSessionToken: null,
            questionId: 1,
            choiceId: 1,
            journeyId: "journey-1");

        var dna = await dnaService.GetUserDnaAsync("user-1");
        Assert.Equal(1, dna.TotalAnswers); // Only 1 should be counted
    }

    [Fact]
    public async Task LuckyDna_Reset_ShouldClearProfilesNotHistory()
    {
        var (db, clock, dnaService, _, _) = await CreateTestContextAsync();



        await dnaService.UpdateDnaForAnswerAsync(
            userId: "user-1",
            guestSessionToken: null,
            questionId: 1,
            choiceId: 1,
            journeyId: "journey-1");

        // Reset
        await dnaService.ResetUserDnaAsync("user-1");

        var dna = await dnaService.GetUserDnaAsync("user-1");
        Assert.Equal(0, dna.TotalAnswers); // Count of answers after reset is 0
        Assert.Empty(await db.UserTraitProfiles.Where(p => p.UserId == "user-1").ToListAsync());
        Assert.Single(await db.UserQuestionHistories.Where(h => h.UserId == "user-1").ToListAsync()); // Historical logs preserved
    }

    [Fact]
    public async Task DailyJourney_UniquenessAndResume_ShouldBeEnforced()
    {
        var (db, clock, _, dailyService, _) = await CreateTestContextAsync();

        var start1 = await dailyService.StartDailyJourneyAsync(new StartJourneyRequest
        {
            GameCode = "POWER_655",
            GuestSessionToken = "guest-1"
        });

        // Start again on same day should return the same journey ID and resume state
        var start2 = await dailyService.StartDailyJourneyAsync(new StartJourneyRequest
        {
            GameCode = "POWER_655",
            GuestSessionToken = "guest-1"
        });

        Assert.Equal(start1.JourneyId, start2.JourneyId);
        Assert.Equal(1, start2.CurrentStep);
    }

    [Fact]
    public async Task QuickRemix_WithLockedNumbers_ShouldPreserveAndFillRemaining()
    {
        var (db, clock, _, _, remixService) = await CreateTestContextAsync();

        var inputNumbers = new List<GeneratedNumberDto>
        {
            new() { Value = 8, PoolIndex = 0, Source = NumberSource.Manual, IsLocked = true },
            new() { Value = 24, PoolIndex = 0, Source = NumberSource.Manual, IsLocked = true },
            new() { Value = 15, PoolIndex = 0, Source = NumberSource.Random, IsLocked = false },
            new() { Value = 30, PoolIndex = 0, Source = NumberSource.Random, IsLocked = false },
            new() { Value = 42, PoolIndex = 0, Source = NumberSource.Random, IsLocked = false },
            new() { Value = 50, PoolIndex = 0, Source = NumberSource.Random, IsLocked = false }
        };

        var request = new StartRemixJourneyRequest
        {
            GameCode = "POWER_655",
            Numbers = inputNumbers
        };

        var response = await remixService.QuickRemixAsync(request);

        Assert.Equal(6, response.Numbers.Count);
        // Verify locked numbers preserved
        Assert.Contains(response.Numbers, n => n.Value == 8 && n.IsLocked);
        Assert.Contains(response.Numbers, n => n.Value == 24 && n.IsLocked);
        // Verify all 6 numbers are distinct
        var unique = response.Numbers.Select(n => n.Value).Distinct().ToList();
        Assert.Equal(6, unique.Count);
    }
}
