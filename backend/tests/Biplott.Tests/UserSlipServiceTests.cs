using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biplott.Tests;

public class UserSlipServiceTests
{
    private static (BiplottDbContext db, IUserSlipService service, Game powerGame) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);

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
        db.Games.Add(powerGame);
        db.SaveChanges();

        var gameRepo = new GameRepository(db);
        var slipRepo = new SlipRepository(db);
        var activityRepo = new UserActivityRepository(db);
        var validator = new GameRuleValidator();

        var service = new UserSlipService(slipRepo, gameRepo, activityRepo, validator);
        return (db, service, powerGame);
    }

    [Fact]
    public async Task SaveSlip_ValidPartialSlip_ShouldPersistCorrectly()
    {
        var (_, service, _) = CreateTestContext();
        var userId = "user-123";

        var req = new SaveSlipRequest
        {
            GameCode = "POWER_655",
            Title = "Vé may mắn của tôi",
            IsFavorite = true,
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        new() { Value = 8, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 17, PoolIndex = 0, Source = NumberSource.Lucky, MetadataJson = "{\"themeName\":\"Ký ức\",\"questionText\":\"Q1\",\"choiceText\":\"C1\",\"explanation\":\"E1\"}" },
                        new() { Value = 24, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 31, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 39, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 44, PoolIndex = 0, Source = NumberSource.Lucky }
                    }
                }
            }
        };

        var result = await service.SaveSlipAsync(userId, req);

        Assert.NotNull(result);
        Assert.Equal("Vé may mắn của tôi", result.Title);
        Assert.True(result.IsFavorite);
        Assert.Equal(1, result.CompletedLineCount);
        Assert.Single(result.Lines);
        Assert.Equal("Mixed", result.Lines[0].DerivedMode); // Derived from multiple sources
    }

    [Fact]
    public async Task GetSlipDetail_ShouldReconstructLuckyStories_AndCheckOwnership()
    {
        var (_, service, _) = CreateTestContext();
        var userA = "user-A";
        var userB = "user-B";

        var saved = await service.SaveSlipAsync(userA, new SaveSlipRequest
        {
            GameCode = "POWER_655",
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        new() { Value = 8, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 17, PoolIndex = 0, Source = NumberSource.Lucky, MetadataJson = "{\"themeName\":\"Tình cảm\",\"questionText\":\"Mối tình đầu thế nào?\",\"choiceText\":\"Thầm kín\",\"explanation\":\"Một chút lãng mạn cho số 17\"}" },
                        new() { Value = 24, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 31, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 39, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 44, PoolIndex = 0, Source = NumberSource.Random }
                    }
                }
            }
        });

        // User A (Owner) can read detail and Lucky story
        var detail = await service.GetSlipDetailAsync(userA, saved.Id);
        Assert.NotNull(detail);
        Assert.Single(detail.LuckyStories);
        Assert.Equal("Tình cảm", detail.LuckyStories[0].ThemeName);
        Assert.Equal(17, detail.LuckyStories[0].NumberValue);

        // User B CANNOT read User A's slip (Enforce Ownership)
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetSlipDetailAsync(userB, saved.Id));
    }

    [Fact]
    public async Task ToggleFavorite_And_DeleteSlip_ShouldEnforceOwnership()
    {
        var (_, service, _) = CreateTestContext();
        var userA = "user-A";
        var userB = "user-B";

        var saved = await service.SaveSlipAsync(userA, new SaveSlipRequest
        {
            GameCode = "POWER_655",
            IsFavorite = false,
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        new() { Value = 1, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 2, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 3, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 4, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 5, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 6, PoolIndex = 0, Source = NumberSource.Random }
                    }
                }
            }
        });

        // User B cannot favorite User A's slip
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ToggleFavoriteAsync(userB, saved.Id));

        // User A can toggle favorite
        var favRes = await service.ToggleFavoriteAsync(userA, saved.Id);
        Assert.True(favRes.IsFavorite);

        // User B cannot delete User A's slip
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.DeleteSlipAsync(userB, saved.Id));

        // User A can delete own slip
        await service.DeleteSlipAsync(userA, saved.Id);

        // Slip is deleted
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetSlipDetailAsync(userA, saved.Id));
    }
}
