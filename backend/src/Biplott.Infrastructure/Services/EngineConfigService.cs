using System.Text.Json;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Biplott.Infrastructure.Services;

public class EngineConfigService : IEngineConfigService
{
    private const string LuckyKey = "EngineConfig:Lucky";
    private const string NoveltyKey = "EngineConfig:Novelty";
    private const string RandomKey = "EngineConfig:Random";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EngineConfigService> _logger;

    private AdminSettingsDto _cachedSettings;
    private readonly object _lock = new();

    public EngineConfigService(IServiceScopeFactory scopeFactory, ILogger<EngineConfigService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _cachedSettings = new AdminSettingsDto();
    }

    public LuckyEngineConfigDto GetCurrentLuckyConfig() => _cachedSettings.Lucky;
    public NoveltyEngineConfigDto GetCurrentNoveltyConfig() => _cachedSettings.Novelty;
    public RandomEngineConfigDto GetCurrentRandomConfig() => _cachedSettings.Random;

    public async Task<AdminSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BiplottDbContext>();

        var configs = await db.EngineConfigs.AsNoTracking().ToListAsync(cancellationToken);

        var luckyConfig = configs.FirstOrDefault(c => c.Key == LuckyKey);
        var noveltyConfig = configs.FirstOrDefault(c => c.Key == NoveltyKey);
        var randomConfig = configs.FirstOrDefault(c => c.Key == RandomKey);

        var settings = new AdminSettingsDto
        {
            Lucky = luckyConfig != null ? Deserialize<LuckyEngineConfigDto>(luckyConfig.ValueJson) : new LuckyEngineConfigDto(),
            Novelty = noveltyConfig != null ? Deserialize<NoveltyEngineConfigDto>(noveltyConfig.ValueJson) : new NoveltyEngineConfigDto(),
            Random = randomConfig != null ? Deserialize<RandomEngineConfigDto>(randomConfig.ValueJson) : new RandomEngineConfigDto(),
            UpdatedAt = configs.Count > 0 ? configs.Max(c => c.UpdatedAt) : DateTime.UtcNow
        };

        lock (_lock)
        {
            _cachedSettings = settings;
        }

        return settings;
    }

    public async Task<AdminSettingsDto> UpdateSettingsAsync(AdminSettingsDto settings, CancellationToken cancellationToken = default)
    {
        ValidateSettings(settings);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BiplottDbContext>();

        var now = DateTime.UtcNow;

        await UpsertConfigAsync(db, LuckyKey, settings.Lucky, "Cấu hình thuật toán sinh số tâm linh Lucky Engine", now, cancellationToken);
        await UpsertConfigAsync(db, NoveltyKey, settings.Novelty, "Cấu hình thuật toán chọn câu hỏi Novelty Engine", now, cancellationToken);
        await UpsertConfigAsync(db, RandomKey, settings.Random, "Cấu hình thuật toán sinh số Thần Tài Random Engine", now, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        settings.UpdatedAt = now;
        lock (_lock)
        {
            _cachedSettings = settings;
        }

        _logger.LogInformation("Engine configurations updated successfully by Admin.");
        return settings;
    }

    public async Task<AdminSettingsDto> ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var defaultSettings = new AdminSettingsDto
        {
            Lucky = new LuckyEngineConfigDto(),
            Novelty = new NoveltyEngineConfigDto(),
            Random = new RandomEngineConfigDto(),
            UpdatedAt = DateTime.UtcNow
        };

        return await UpdateSettingsAsync(defaultSettings, cancellationToken);
    }

    private static async Task UpsertConfigAsync<T>(
        BiplottDbContext db,
        string key,
        T value,
        string description,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var config = await db.EngineConfigs.FirstOrDefaultAsync(c => c.Key == key, cancellationToken);
        var json = JsonSerializer.Serialize(value);

        if (config == null)
        {
            config = new EngineConfig
            {
                Key = key,
                ValueJson = json,
                Description = description,
                UpdatedAt = now
            };
            await db.EngineConfigs.AddAsync(config, cancellationToken);
        }
        else
        {
            config.ValueJson = json;
            config.Description = description;
            config.UpdatedAt = now;
        }
    }

    private static T Deserialize<T>(string json) where T : new()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    private static void ValidateSettings(AdminSettingsDto settings)
    {
        if (settings.Lucky.BaseWeight < 1.0 || settings.Lucky.BaseWeight > 100.0)
            throw new ArgumentException("Lucky BaseWeight phải nằm trong khoảng từ 1.0 đến 100.0.");

        if (settings.Lucky.TraitAffinityMultiplier < 0.0 || settings.Lucky.TraitAffinityMultiplier > 50.0)
            throw new ArgumentException("Lucky TraitAffinityMultiplier phải nằm trong khoảng từ 0.0 đến 50.0.");

        if (settings.Lucky.NoiseMagnitude < 0.0 || settings.Lucky.NoiseMagnitude > 10.0)
            throw new ArgumentException("Lucky NoiseMagnitude phải nằm trong khoảng từ 0.0 đến 10.0.");

        if (settings.Lucky.MinWeight < 0.1 || settings.Lucky.MinWeight > 10.0)
            throw new ArgumentException("Lucky MinWeight phải nằm trong khoảng từ 0.1 đến 10.0.");

        if (settings.Novelty.BaseWeight < 10.0 || settings.Novelty.BaseWeight > 500.0)
            throw new ArgumentException("Novelty BaseWeight phải nằm trong khoảng từ 10.0 đến 500.0.");

        if (settings.Novelty.NeverSeenBonus < 0.0 || settings.Novelty.NeverSeenBonus > 300.0)
            throw new ArgumentException("Novelty NeverSeenBonus phải nằm trong khoảng từ 0.0 đến 300.0.");

        if (settings.Novelty.RecentlySeenPenalty < 0.0 || settings.Novelty.RecentlySeenPenalty > 300.0)
            throw new ArgumentException("Novelty RecentlySeenPenalty phải nằm trong khoảng từ 0.0 đến 300.0.");

        if (settings.Novelty.RepeatedThemePenalty < 0.0 || settings.Novelty.RepeatedThemePenalty > 300.0)
            throw new ArgumentException("Novelty RepeatedThemePenalty phải nằm trong khoảng từ 0.0 đến 300.0.");

        if (settings.Novelty.RecentThemePenalty < 0.0 || settings.Novelty.RecentThemePenalty > 200.0)
            throw new ArgumentException("Novelty RecentThemePenalty phải nằm trong khoảng từ 0.0 đến 200.0.");

        if (settings.Novelty.QuestionTypeDiversityBonus < 0.0 || settings.Novelty.QuestionTypeDiversityBonus > 200.0)
            throw new ArgumentException("Novelty QuestionTypeDiversityBonus phải nằm trong khoảng từ 0.0 đến 200.0.");

        if (settings.Novelty.ClimaxDestinyThemeBoost < 0.0 || settings.Novelty.ClimaxDestinyThemeBoost > 2000.0)
            throw new ArgumentException("Novelty ClimaxDestinyThemeBoost phải nằm trong khoảng từ 0.0 đến 2000.0.");

        if (settings.Novelty.ClimaxQuickInstinctBoost < 0.0 || settings.Novelty.ClimaxQuickInstinctBoost > 1000.0)
            throw new ArgumentException("Novelty ClimaxQuickInstinctBoost phải nằm trong khoảng từ 0.0 đến 1000.0.");

        if (settings.Random.BalancedMaxDeviation < 0 || settings.Random.BalancedMaxDeviation > 3)
            throw new ArgumentException("Random BalancedMaxDeviation phải nằm trong khoảng từ 0 đến 3.");

        if (settings.Random.SpreadMinPartitions < 2 || settings.Random.SpreadMinPartitions > 6)
            throw new ArgumentException("Random SpreadMinPartitions phải nằm trong khoảng từ 2 đến 6.");
    }
}