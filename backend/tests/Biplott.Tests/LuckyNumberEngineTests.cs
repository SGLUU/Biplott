using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Xunit;

namespace Biplott.Tests;

public class LuckyNumberEngineTests
{
    private readonly GamePool _powerMainPool = new()
    {
        PoolIndex = 0,
        Name = "Dãy số chính Power",
        MinNumber = 1,
        MaxNumber = 55,
        PickCount = 6,
        AllowDuplicates = false
    };

    private readonly GamePool _lottoSpecialPool = new()
    {
        PoolIndex = 1,
        Name = "Số đặc biệt Lotto",
        MinNumber = 1,
        MaxNumber = 12,
        PickCount = 1,
        AllowDuplicates = false
    };

    private static QuestionChoice CreateSampleChoice()
    {
        var question = new Question
        {
            Id = 1,
            Content = "Bạn chọn điều gì?",
            Theme = new Theme { Name = "Tự do" }
        };

        var choice = new QuestionChoice
        {
            Id = 10,
            QuestionId = 1,
            Question = question,
            Content = "Khám phá không giới hạn",
            ChoiceTraits = new List<ChoiceTrait>
            {
                new() { Trait = new Trait { Code = "Independence", Name = "Độc lập" }, Weight = 0.9 },
                new() { Trait = new Trait { Code = "RiskTolerance", Name = "Liều lĩnh" }, Weight = 0.7 }
            }
        };

        return choice;
    }

    [Fact]
    public void GenerateLuckyNumber_ShouldReturnNumber_WithinPoolRange()
    {
        var rng = new DeterministicRandomSource(12345);
        var engine = new LuckyNumberEngine(rng);
        var choice = CreateSampleChoice();

        var result = engine.GenerateLuckyNumber(
            _powerMainPool,
            choice,
            excludedNumbersInPool: new HashSet<int>(),
            previousNumbersInLine: new List<int>());

        Assert.InRange(result.Value, _powerMainPool.MinNumber, _powerMainPool.MaxNumber);
        Assert.Equal(NumberSource.Lucky, result.Source);
        Assert.NotEmpty(result.Explanation);
        Assert.False(string.IsNullOrWhiteSpace(result.DominantTrait));
    }

    [Fact]
    public void GenerateLuckyNumber_ShouldExclude_PreviouslyGeneratedNumbersInSamePool()
    {
        var rng = new DeterministicRandomSource(42);
        var engine = new LuckyNumberEngine(rng);
        var choice = CreateSampleChoice();

        // Exclude all except 3 numbers: 10, 20, 30
        var allNumbers = Enumerable.Range(1, 55).ToHashSet();
        allNumbers.Remove(10);
        allNumbers.Remove(20);
        allNumbers.Remove(30);

        for (int i = 0; i < 10; i++)
        {
            var result = engine.GenerateLuckyNumber(
                _powerMainPool,
                choice,
                excludedNumbersInPool: allNumbers,
                previousNumbersInLine: new List<int>());

            Assert.Contains(result.Value, new[] { 10, 20, 30 });
        }
    }

    [Fact]
    public void GenerateLuckyNumber_ShouldHandleLottoSpecialPool_Within1To12()
    {
        var rng = new DeterministicRandomSource(777);
        var engine = new LuckyNumberEngine(rng);
        var choice = CreateSampleChoice();

        for (int seed = 1; seed <= 20; seed++)
        {
            var testRng = new DeterministicRandomSource(seed);
            var result = engine.GenerateLuckyNumber(
                _lottoSpecialPool,
                choice,
                excludedNumbersInPool: new HashSet<int>(),
                previousNumbersInLine: new List<int> { 5, 12, 19, 27, 33 },
                randomSource: testRng);

            Assert.InRange(result.Value, 1, 12);
            Assert.Equal(1, result.PoolIndex);
        }
    }

    [Fact]
    public void NoFixedNumberMapping_DifferentSeeds_ShouldProduceDifferentNumbersFromSameChoice()
    {
        // Principle 3: QuestionChoice MUST NOT be fixed to a single number
        var engine = new LuckyNumberEngine(new DeterministicRandomSource());
        var choice = CreateSampleChoice();

        var generatedNumbers = new HashSet<int>();

        for (int seed = 1; seed <= 30; seed++)
        {
            var testRng = new DeterministicRandomSource(seed * 100);
            var result = engine.GenerateLuckyNumber(
                _powerMainPool,
                choice,
                excludedNumbersInPool: new HashSet<int>(),
                previousNumbersInLine: new List<int>(),
                randomSource: testRng);

            generatedNumbers.Add(result.Value);
        }

        // Must produce multiple different candidate numbers, not just a single static number!
        Assert.True(generatedNumbers.Count > 5, "Candidate scoring & weighted random must produce diverse numbers across sessions.");
    }
}
