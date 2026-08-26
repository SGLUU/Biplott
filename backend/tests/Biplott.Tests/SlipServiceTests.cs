using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;
using Xunit;

namespace Biplott.Tests;

public class SlipServiceTests
{
    private readonly ISlipService _slipService;
    private readonly IGameRepository _mockRepo;

    public SlipServiceTests()
    {
        var games = new List<Game>
        {
            new()
            {
                Id = 1,
                Code = "POWER_655",
                Name = "Power 6/55",
                Pools = new List<GamePool>
                {
                    new() { Id = 1, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 55, PickCount = 6, AllowDuplicates = false }
                }
            },
            new()
            {
                Id = 3,
                Code = "LOTTO_535",
                Name = "Lotto 5/35",
                Pools = new List<GamePool>
                {
                    new() { Id = 3, PoolIndex = 0, Name = "Dãy chính", MinNumber = 1, MaxNumber = 35, PickCount = 5, AllowDuplicates = false },
                    new() { Id = 4, PoolIndex = 1, Name = "Số đặc biệt", MinNumber = 1, MaxNumber = 12, PickCount = 1, AllowDuplicates = false }
                }
            }
        };

        _mockRepo = new FakeGameRepository(games);
        var validator = new GameRuleValidator();
        var randomEngine = new RandomNumberEngine();
        _slipService = new SlipService(_mockRepo, validator, randomEngine);
    }

    [Fact]
    public async Task ValidateLine_ValidPower655_ReturnsTrue()
    {
        var request = new ValidateLineRequest
        {
            GameCode = "POWER_655",
            LineLabel = "A",
            Numbers = new List<GeneratedNumberDto>
            {
                new() { Value = 3, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 12, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 25, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 33, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 41, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 54, PoolIndex = 0, Source = NumberSource.Manual }
            }
        };

        var result = await _slipService.ValidateLineAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateLine_DuplicateNumbers_ReturnsFalse()
    {
        var request = new ValidateLineRequest
        {
            GameCode = "POWER_655",
            LineLabel = "A",
            Numbers = new List<GeneratedNumberDto>
            {
                new() { Value = 7, PoolIndex = 0 },
                new() { Value = 7, PoolIndex = 0 }, // Duplicate!
                new() { Value = 25, PoolIndex = 0 },
                new() { Value = 33, PoolIndex = 0 },
                new() { Value = 41, PoolIndex = 0 },
                new() { Value = 54, PoolIndex = 0 }
            }
        };

        var result = await _slipService.ValidateLineAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("trùng lặp"));
    }

    [Fact]
    public async Task ValidateLine_OutOfRange_ReturnsFalse()
    {
        var request = new ValidateLineRequest
        {
            GameCode = "POWER_655",
            LineLabel = "A",
            Numbers = new List<GeneratedNumberDto>
            {
                new() { Value = 0, PoolIndex = 0 }, // Under min!
                new() { Value = 12, PoolIndex = 0 },
                new() { Value = 25, PoolIndex = 0 },
                new() { Value = 33, PoolIndex = 0 },
                new() { Value = 41, PoolIndex = 0 },
                new() { Value = 56, PoolIndex = 0 } // Over max!
            }
        };

        var result = await _slipService.ValidateLineAsync(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public async Task ValidateLine_Lotto535_ValidMultiPool_ReturnsTrue()
    {
        var request = new ValidateLineRequest
        {
            GameCode = "LOTTO_535",
            LineLabel = "A",
            Numbers = new List<GeneratedNumberDto>
            {
                // Pool 0 (5 numbers)
                new() { Value = 5, PoolIndex = 0 },
                new() { Value = 10, PoolIndex = 0 },
                new() { Value = 15, PoolIndex = 0 },
                new() { Value = 20, PoolIndex = 0 },
                new() { Value = 25, PoolIndex = 0 },
                // Pool 1 (1 special number - can match value in pool 0)
                new() { Value = 5, PoolIndex = 1 }
            }
        };

        var result = await _slipService.ValidateLineAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GenerateSlip_GeneratesExact6Lines_AtoF()
    {
        var request = new GenerateSlipRequest
        {
            GameCode = "POWER_655",
            Strategy = RandomStrategy.PureRandom,
            FillMode = "All"
        };

        var result = await _slipService.GenerateSlipAsync(request);

        Assert.Equal(6, result.Lines.Count);
        var labels = result.Lines.Select(l => l.LineLabel).ToList();
        Assert.Equal(new[] { "A", "B", "C", "D", "E", "F" }, labels);
        Assert.All(result.Lines, l => Assert.Equal(SlipLineStatus.Complete, l.Status));
        Assert.All(result.Lines, l => Assert.Equal(6, l.Numbers.Count));
    }

    [Fact]
    public async Task GenerateSlip_EmptyOnly_PreservesExistingManualLine()
    {
        var existingManual = new SlipLineDto
        {
            LineLabel = "A",
            Status = SlipLineStatus.Complete,
            Numbers = new List<GeneratedNumberDto>
            {
                new() { Value = 1, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 2, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 3, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 4, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 5, PoolIndex = 0, Source = NumberSource.Manual },
                new() { Value = 6, PoolIndex = 0, Source = NumberSource.Manual }
            }
        };

        var request = new GenerateSlipRequest
        {
            GameCode = "POWER_655",
            Strategy = RandomStrategy.Balanced,
            FillMode = "EmptyOnly",
            ExistingLines = new List<SlipLineDto> { existingManual }
        };

        var result = await _slipService.GenerateSlipAsync(request);

        Assert.Equal(6, result.Lines.Count);
        var lineA = result.Lines.First(l => l.LineLabel == "A");
        Assert.Equal(existingManual.Numbers.Select(n => n.Value), lineA.Numbers.Select(n => n.Value));
        Assert.All(lineA.Numbers, n => Assert.Equal(NumberSource.Manual, n.Source));

        // Lines B-F should be generated with Random
        var remainingLines = result.Lines.Where(l => l.LineLabel != "A").ToList();
        Assert.Equal(5, remainingLines.Count);
        Assert.All(remainingLines, l => Assert.All(l.Numbers, n => Assert.Equal(NumberSource.Random, n.Source)));
    }

    public class FakeGameRepository : IGameRepository
    {
        private readonly List<Game> _games;

        public FakeGameRepository(List<Game> games) => _games = games;

        public Task<IReadOnlyList<Game>> GetActiveGamesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Game>>(_games.AsReadOnly());

        public Task<Game?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_games.FirstOrDefault(g => string.Equals(g.Code, code, StringComparison.OrdinalIgnoreCase)));

        public Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_games.FirstOrDefault(g => g.Id == id));
    }
}
