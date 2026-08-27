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

    [Fact]
    public async Task SaveSlip_UnsortedNumbers_PersistsAndReturnsSortedAscending()
    {
        var (_, service, _) = CreateTestContext();
        var userId = "user-sort-test";

        var req = new SaveSlipRequest
        {
            GameCode = "POWER_655",
            Title = "Vé test sorting",
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        new() { Value = 45, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 2,  PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 34, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 16, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 30, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 13, PoolIndex = 0, Source = NumberSource.Random }
                    }
                }
            }
        };

        var saved = await service.SaveSlipAsync(userId, req);
        Assert.NotNull(saved);

        // Summary should have numbers in sorted ascending order: 2, 13, 16, 30, 34, 45
        var summaryValues = saved.Lines[0].Numbers.Select(n => n.Value).ToList();
        Assert.Equal(new List<int> { 2, 13, 16, 30, 34, 45 }, summaryValues);

        // Detail should also have numbers in sorted ascending order
        var detail = await service.GetSlipDetailAsync(userId, saved.Id);
        var detailValues = detail.Lines[0].Numbers.Select(n => n.Value).ToList();
        Assert.Equal(new List<int> { 2, 13, 16, 30, 34, 45 }, detailValues);
    }

    [Fact]
    public async Task SaveSlip_MixedMode_PreservesSourcesAndLuckyStoryMetadataAfterSort()
    {
        var (_, service, _) = CreateTestContext();
        var userId = "user-mixed-sort";

        var lucky17Meta = "{\"themeName\":\"Công sở\",\"questionText\":\"Sếp mắng?\",\"choiceText\":\"Cười trừ\",\"explanation\":\"Bình tâm\"}";
        var lucky31Meta = "{\"themeName\":\"Tình duyên\",\"questionText\":\"Người yêu cũ cưới?\",\"choiceText\":\"Đi ăn cỗ\",\"explanation\":\"Ăn no nê\"}";

        var req = new SaveSlipRequest
        {
            GameCode = "POWER_655",
            Title = "Vé Mixed Test",
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        new() { Value = 24, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 8,  PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 17, PoolIndex = 0, Source = NumberSource.Lucky, MetadataJson = lucky17Meta },
                        new() { Value = 44, PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 39, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 31, PoolIndex = 0, Source = NumberSource.Lucky, MetadataJson = lucky31Meta }
                    }
                }
            }
        };

        var saved = await service.SaveSlipAsync(userId, req);
        Assert.Equal("Mixed", saved.Lines[0].DerivedMode);

        var detail = await service.GetSlipDetailAsync(userId, saved.Id);
        var sortedLine = detail.Lines[0];

        // Values must be: 8, 17, 24, 31, 39, 44
        var values = sortedLine.Numbers.Select(n => n.Value).ToList();
        Assert.Equal(new List<int> { 8, 17, 24, 31, 39, 44 }, values);

        // Sources must stay correctly attached to each number
        var num8 = sortedLine.Numbers.First(n => n.Value == 8);
        Assert.Equal(NumberSource.Manual, num8.Source);

        var num17 = sortedLine.Numbers.First(n => n.Value == 17);
        Assert.Equal(NumberSource.Lucky, num17.Source);
        Assert.Contains("Công sở", num17.MetadataJson);

        var num24 = sortedLine.Numbers.First(n => n.Value == 24);
        Assert.Equal(NumberSource.Random, num24.Source);

        var num31 = sortedLine.Numbers.First(n => n.Value == 31);
        Assert.Equal(NumberSource.Lucky, num31.Source);
        Assert.Contains("Tình duyên", num31.MetadataJson);

        var num39 = sortedLine.Numbers.First(n => n.Value == 39);
        Assert.Equal(NumberSource.Manual, num39.Source);

        var num44 = sortedLine.Numbers.First(n => n.Value == 44);
        Assert.Equal(NumberSource.Random, num44.Source);

        // Lucky stories must map correctly to the lucky numbers
        Assert.Equal(2, detail.LuckyStories.Count);
        var story17 = detail.LuckyStories.First(s => s.NumberValue == 17);
        Assert.Equal("Công sở", story17.ThemeName);
        Assert.Equal("Cười trừ", story17.ChoiceText);

        var story31 = detail.LuckyStories.First(s => s.NumberValue == 31);
        Assert.Equal("Tình duyên", story31.ThemeName);
        Assert.Equal("Đi ăn cỗ", story31.ChoiceText);
    }

    [Fact]
    public async Task SaveSlip_DualPoolLotto535_SortsMainAndSpecialPoolsSeparately()
    {
        var (db, service, _) = CreateTestContext();
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
        db.Games.Add(lottoGame);
        await db.SaveChangesAsync();

        var userId = "user-lotto-sort";
        var req = new SaveSlipRequest
        {
            GameCode = "LOTTO_535",
            Title = "Vé Lotto 5/35 Test",
            Lines = new List<SaveSlipLineDto>
            {
                new()
                {
                    LineLabel = "A",
                    Numbers = new List<SaveSlipNumberDto>
                    {
                        // Unsorted main pool (21, 3, 18, 7, 30)
                        new() { Value = 21, PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 3,  PoolIndex = 0, Source = NumberSource.Random },
                        new() { Value = 18, PoolIndex = 0, Source = NumberSource.Lucky },
                        new() { Value = 7,  PoolIndex = 0, Source = NumberSource.Manual },
                        new() { Value = 30, PoolIndex = 0, Source = NumberSource.Random },
                        // Special pool (9)
                        new() { Value = 9,  PoolIndex = 1, Source = NumberSource.Lucky }
                    }
                }
            }
        };

        var saved = await service.SaveSlipAsync(userId, req);
        var detail = await service.GetSlipDetailAsync(userId, saved.Id);

        var pool0 = detail.Lines[0].Numbers.Where(n => n.PoolIndex == 0).Select(n => n.Value).ToList();
        var pool1 = detail.Lines[0].Numbers.Where(n => n.PoolIndex == 1).Select(n => n.Value).ToList();

        // Main pool is sorted: 3, 7, 18, 21, 30
        Assert.Equal(new List<int> { 3, 7, 18, 21, 30 }, pool0);
        // Special pool is separate: 9
        Assert.Equal(new List<int> { 9 }, pool1);
    }
}
