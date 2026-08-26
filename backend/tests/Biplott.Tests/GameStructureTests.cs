using Biplott.Application.Services;
using Biplott.Core.Entities;
using Xunit;

namespace Biplott.Tests;

public class GameStructureTests
{
    [Fact]
    public void Power655_ShouldHaveSinglePool_WithPick6AndRange1To55()
    {
        // Arrange
        var game = new Game
        {
            Code = "POWER_655",
            Name = "Power 6/55",
            Pools = new List<GamePool>
            {
                new()
                {
                    PoolIndex = 0,
                    Name = "Dãy số chính",
                    MinNumber = 1,
                    MaxNumber = 55,
                    PickCount = 6,
                    AllowDuplicates = false
                }
            }
        };

        // Assert
        Assert.Single(game.Pools);
        var mainPool = game.Pools[0];
        Assert.Equal(0, mainPool.PoolIndex);
        Assert.Equal(1, mainPool.MinNumber);
        Assert.Equal(55, mainPool.MaxNumber);
        Assert.Equal(6, mainPool.PickCount);
        Assert.False(mainPool.AllowDuplicates);
    }

    [Fact]
    public void Mega645_ShouldHaveSinglePool_WithPick6AndRange1To45()
    {
        // Arrange
        var game = new Game
        {
            Code = "MEGA_645",
            Name = "Mega 6/45",
            Pools = new List<GamePool>
            {
                new()
                {
                    PoolIndex = 0,
                    Name = "Dãy số chính",
                    MinNumber = 1,
                    MaxNumber = 45,
                    PickCount = 6,
                    AllowDuplicates = false
                }
            }
        };

        // Assert
        Assert.Single(game.Pools);
        var mainPool = game.Pools[0];
        Assert.Equal(0, mainPool.PoolIndex);
        Assert.Equal(1, mainPool.MinNumber);
        Assert.Equal(45, mainPool.MaxNumber);
        Assert.Equal(6, mainPool.PickCount);
        Assert.False(mainPool.AllowDuplicates);
    }

    [Fact]
    public void Lotto535_ShouldHaveDualPools_Main1To35Pick5_AndSpecial1To12Pick1()
    {
        // Arrange
        var game = new Game
        {
            Code = "LOTTO_535",
            Name = "Lotto 5/35",
            Pools = new List<GamePool>
            {
                new()
                {
                    PoolIndex = 0,
                    Name = "Dãy số chính",
                    MinNumber = 1,
                    MaxNumber = 35,
                    PickCount = 5,
                    AllowDuplicates = false
                },
                new()
                {
                    PoolIndex = 1,
                    Name = "Số đặc biệt",
                    MinNumber = 1,
                    MaxNumber = 12,
                    PickCount = 1,
                    AllowDuplicates = false
                }
            }
        };

        // Assert
        Assert.Equal(2, game.Pools.Count);
        
        var mainPool = game.Pools.First(p => p.PoolIndex == 0);
        Assert.Equal(1, mainPool.MinNumber);
        Assert.Equal(35, mainPool.MaxNumber);
        Assert.Equal(5, mainPool.PickCount);
        Assert.False(mainPool.AllowDuplicates);

        var specialPool = game.Pools.First(p => p.PoolIndex == 1);
        Assert.Equal(1, specialPool.MinNumber);
        Assert.Equal(12, specialPool.MaxNumber);
        Assert.Equal(1, specialPool.PickCount);
        Assert.False(specialPool.AllowDuplicates);
    }
}
