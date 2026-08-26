using Biplott.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Biplott.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGameService, GameService>();
        services.AddSingleton<IGameRuleValidator, GameRuleValidator>();

        return services;
    }
}
