using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Xunit;

namespace Biplott.Tests;

public class RandomNumberEngineTests
{
    private readonly RandomNumberEngine _engine = new();

    private Game CreatePower655() => new()
    {
        Id = 1,
        Code = "POWER_655",
        Name = "Power 6/55",
        Pools = new List<GamePool>
        {
            new() { Id = 1, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 55, PickCount = 6, AllowDuplicates = false }
        }
    };

    private Game CreateMega645() => new()
    {
        Id = 2,
        Code = "MEGA_645",
        Name = "Mega 6/45",
        Pools = new List<GamePool>
        {
            new() { Id = 2, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 45, PickCount = 6, AllowDuplicates = false }
        }
    };

    private Game CreateLotto535() => new()
    {
        Id = 3,
        Code = "LOTTO_535",
        Name = "Lotto 5/35",
        Pools = new List<GamePool>
        {
            new() { Id = 3, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 35, PickCount = 5, AllowDuplicates = false },
            new() { Id = 4, PoolIndex = 1, Name = "Số đặc biệt", MinNumber = 1, MaxNumber = 12, PickCount = 1, AllowDuplicates = false }
        }
    };

    [Theory]
    [InlineData(RandomStrategy.PureRandom)]
    [InlineData(RandomStrategy.Balanced)]
    [InlineData(RandomStrategy.Spread)]
    [InlineData(RandomStrategy.Surprise)]
    public void Power655_AllStrategies_ProduceValidLine(RandomStrategy strategy)
    {
        var game = CreatePower655();

        for (int i = 0; i < 50; i++)
        {
            var result = _engine.GenerateLine(game, strategy);

            Assert.Equal(strategy, result.Strategy);
            Assert.Equal(6, result.Numbers.Count);
            Assert.All(result.Numbers, n => Assert.InRange(n.Value, 1, 55));
            Assert.All(result.Numbers, n => Assert.Equal(0, n.PoolIndex));
            Assert.All(result.Numbers, n => Assert.Equal(NumberSource.Random, n.Source));

            // Must have 6 distinct numbers
            var distinctValues = result.Numbers.Select(n => n.Value).Distinct().ToList();
            Assert.Equal(6, distinctValues.Count);
        }
    }

    [Theory]
    [InlineData(RandomStrategy.PureRandom)]
    [InlineData(RandomStrategy.Balanced)]
    [InlineData(RandomStrategy.Spread)]
    [InlineData(RandomStrategy.Surprise)]
    public void Mega645_AllStrategies_ProduceValidLine(RandomStrategy strategy)
    {
        var game = CreateMega645();

        for (int i = 0; i < 50; i++)
        {
            var result = _engine.GenerateLine(game, strategy);

            Assert.Equal(6, result.Numbers.Count);
            Assert.All(result.Numbers, n => Assert.InRange(n.Value, 1, 45));
            Assert.All(result.Numbers, n => Assert.Equal(0, n.PoolIndex));

            var distinctValues = result.Numbers.Select(n => n.Value).Distinct().ToList();
            Assert.Equal(6, distinctValues.Count);
        }
    }

    [Theory]
    [InlineData(RandomStrategy.PureRandom)]
    [InlineData(RandomStrategy.Balanced)]
    [InlineData(RandomStrategy.Spread)]
    [InlineData(RandomStrategy.Surprise)]
    public void Lotto535_AllStrategies_ProduceValidDualPoolLine(RandomStrategy strategy)
    {
        var game = CreateLotto535();

        for (int i = 0; i < 50; i++)
        {
            var result = _engine.GenerateLine(game, strategy);

            Assert.Equal(6, result.Numbers.Count); // 5 main + 1 special

            var pool0 = result.Numbers.Where(n => n.PoolIndex == 0).ToList();
            var pool1 = result.Numbers.Where(n => n.PoolIndex == 1).ToList();

            Assert.Equal(5, pool0.Count);
            Assert.Single(pool1);

            Assert.All(pool0, n => Assert.InRange(n.Value, 1, 35));
            Assert.All(pool1, n => Assert.InRange(n.Value, 1, 12));

            // Pool 0 must be 5 distinct numbers
            Assert.Equal(5, pool0.Select(n => n.Value).Distinct().Count());
        }
    }

    [Fact]
    public void Spread_GeneratesNumbersAcrossBuckets()
    {
        var game = CreatePower655();
        var result = _engine.GenerateLine(game, RandomStrategy.Spread);

        Assert.Equal(6, result.Numbers.Count);
        // Sorted ascending
        for (int i = 1; i < result.Numbers.Count; i++)
        {
            Assert.True(result.Numbers[i].Value > result.Numbers[i - 1].Value);
        }
    }

    [Fact]
    public void Commentary_IsNotEmpty_And_EntertainmentCompliant()
    {
        var game = CreatePower655();
        var result = _engine.GenerateLine(game, RandomStrategy.Balanced);

        Assert.False(string.IsNullOrWhiteSpace(result.Commentary));
        Assert.False(string.IsNullOrWhiteSpace(result.StrategyName));
    }
}
