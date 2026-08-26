using Biplott.Application.Services;
using Biplott.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Biplott.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGameService, GameService>();
        services.AddSingleton<IGameRuleValidator, GameRuleValidator>();
        services.AddSingleton<IRandomNumberEngine, RandomNumberEngine>();
        services.AddScoped<ISlipService, SlipService>();

        // Phase 2B: Lucky Journey & Novelty Engine
        services.AddSingleton<IRandomSource, CryptographicRandomSource>();
        services.AddSingleton<ILuckyNumberEngine, LuckyNumberEngine>();
        services.AddSingleton<INoveltyEngine, NoveltyEngine>();
        services.AddScoped<ILuckyJourneySessionService, LuckyJourneySessionService>();

        // Phase 2C: Mixed Mode
        services.AddScoped<IMixedService, MixedService>();

        return services;
    }
}
