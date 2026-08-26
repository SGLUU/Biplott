using Biplott.Application.Services;
using Biplott.Core.Entities;
using Xunit;

namespace Biplott.Tests;

public class GameRuleValidatorTests
{
    private readonly GameRuleValidator _validator = new();

    private static Game CreatePower655() => new()
    {
        Code = "POWER_655",
        Name = "Power 6/55",
        Pools = new List<GamePool>
        {
            new() { PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 55, PickCount = 6, AllowDuplicates = false }
        }
    };

    private static Game CreateMega645() => new()
    {
        Code = "MEGA_645",
        Name = "Mega 6/45",
        Pools = new List<GamePool>
        {
            new() { PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 45, PickCount = 6, AllowDuplicates = false }
        }
    };

    private static Game CreateLotto535() => new()
    {
        Code = "LOTTO_535",
        Name = "Lotto 5/35",
        Pools = new List<GamePool>
        {
            new() { PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 35, PickCount = 5, AllowDuplicates = false },
            new() { PoolIndex = 1, Name = "Số đặc biệt", MinNumber = 1, MaxNumber = 12, PickCount = 1, AllowDuplicates = false }
        }
    };

    [Fact]
    public void Power655_ValidNumbers_ShouldPass()
    {
        var game = CreatePower655();
        var numbers = new List<(int, int)> { (3, 0), (14, 0), (28, 0), (33, 0), (45, 0), (55, 0) };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Power655_DuplicateNumbers_ShouldFail()
    {
        var game = CreatePower655();
        var numbers = new List<(int, int)> { (7, 0), (7, 0), (28, 0), (33, 0), (45, 0), (55, 0) };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("không cho phép trùng lặp"));
    }

    [Fact]
    public void Power655_OutOfRange_ShouldFail()
    {
        var game = CreatePower655();
        var numbers = new List<(int, int)> { (0, 0), (14, 0), (28, 0), (33, 0), (45, 0), (56, 0) };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("không hợp lệ"));
    }

    [Fact]
    public void Power655_InvalidPickCount_ShouldFail()
    {
        var game = CreatePower655();
        var numbers = new List<(int, int)> { (3, 0), (14, 0), (28, 0), (33, 0) }; // only 4 numbers

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("yêu cầu chọn đúng 6 số"));
    }

    [Fact]
    public void Mega645_OutOfRange45_ShouldFail()
    {
        var game = CreateMega645();
        var numbers = new List<(int, int)> { (3, 0), (14, 0), (28, 0), (33, 0), (45, 0), (46, 0) }; // 46 > 45

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("không hợp lệ"));
    }

    [Fact]
    public void Lotto535_ValidMultiPool_ShouldPass()
    {
        var game = CreateLotto535();
        // 5 numbers for Pool 0 (1-35) + 1 number for Pool 1 (1-12)
        var numbers = new List<(int, int)>
        {
            (7, 0), (12, 0), (19, 0), (25, 0), (35, 0), // Pool 0
            (12, 1) // Pool 1 (Note: 12 is valid even if also in pool 0)
        };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Lotto535_MissingSpecialNumber_ShouldFail()
    {
        var game = CreateLotto535();
        var numbers = new List<(int, int)>
        {
            (7, 0), (12, 0), (19, 0), (25, 0), (35, 0) // Missing Pool 1
        };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Số đặc biệt"));
    }

    [Fact]
    public void Lotto535_SpecialNumberOutOfRange_ShouldFail()
    {
        var game = CreateLotto535();
        var numbers = new List<(int, int)>
        {
            (7, 0), (12, 0), (19, 0), (25, 0), (35, 0),
            (15, 1) // 15 > 12
        };

        var result = _validator.ValidateNumbers(game, numbers);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Số đặc biệt"));
    }
}
