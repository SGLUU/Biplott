using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Biplott.Tests;

public class EngineConfigServiceTests
{
    private static (BiplottDbContext db, EngineConfigService service) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);

        var services = new ServiceCollection();
        services.AddSingleton(db);
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var engineConfigService = new EngineConfigService(scopeFactory, NullLogger<EngineConfigService>.Instance);
        return (db, engineConfigService);
    }

    [Fact]
    public async Task GetSettings_Initial_ShouldReturnDefaultSettings()
    {
        var (_, service) = CreateTestContext();

        var settings = await service.GetSettingsAsync();

        Assert.NotNull(settings);
        Assert.Equal(10.0, settings.Lucky.BaseWeight);
        Assert.Equal(100.0, settings.Novelty.BaseWeight);
        Assert.Equal(1, settings.Random.BalancedMaxDeviation);
    }

    [Fact]
    public async Task UpdateSettings_Valid_ShouldPersistAndChangeCachedValues()
    {
        var (db, service) = CreateTestContext();

        var newSettings = new AdminSettingsDto
        {
            Lucky = new LuckyEngineConfigDto { BaseWeight = 25.0, NoiseMagnitude = 4.0 },
            Novelty = new NoveltyEngineConfigDto { BaseWeight = 200.0, NeverSeenBonus = 120.0 },
            Random = new RandomEngineConfigDto { BalancedMaxDeviation = 2 }
        };

        var updated = await service.UpdateSettingsAsync(newSettings);

        Assert.Equal(25.0, updated.Lucky.BaseWeight);
        Assert.Equal(200.0, updated.Novelty.BaseWeight);
        Assert.Equal(2, updated.Random.BalancedMaxDeviation);

        // Check synchronous cache
        Assert.Equal(25.0, service.GetCurrentLuckyConfig().BaseWeight);
        Assert.Equal(200.0, service.GetCurrentNoveltyConfig().BaseWeight);
        Assert.Equal(2, service.GetCurrentRandomConfig().BalancedMaxDeviation);

        // Check in DB
        var luckyDb = await db.EngineConfigs.FirstOrDefaultAsync(c => c.Key == "EngineConfig:Lucky");
        Assert.NotNull(luckyDb);
        Assert.Contains("25", luckyDb.ValueJson);
    }

    [Fact]
    public async Task UpdateSettings_InvalidValue_ShouldThrowArgumentException()
    {
        var (_, service) = CreateTestContext();

        var invalidSettings = new AdminSettingsDto
        {
            Lucky = new LuckyEngineConfigDto { BaseWeight = 0.5 } // Below min 1.0
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateSettingsAsync(invalidSettings));
    }

    [Fact]
    public async Task ResetToDefaults_ShouldRestoreDefaultValues()
    {
        var (_, service) = CreateTestContext();

        var modified = new AdminSettingsDto
        {
            Lucky = new LuckyEngineConfigDto { BaseWeight = 50.0 }
        };
        await service.UpdateSettingsAsync(modified);
        Assert.Equal(50.0, service.GetCurrentLuckyConfig().BaseWeight);

        var reset = await service.ResetToDefaultsAsync();
        Assert.Equal(10.0, reset.Lucky.BaseWeight);
        Assert.Equal(10.0, service.GetCurrentLuckyConfig().BaseWeight);
    }
}