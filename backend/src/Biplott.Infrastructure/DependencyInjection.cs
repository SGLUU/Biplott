using Biplott.Core.Entities;
using Biplott.Core.Interfaces;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biplott.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=BiplottDb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<BiplottDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(BiplottDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<BiplottDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ISlipRepository, SlipRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IUserActivityRepository, UserActivityRepository>();

        return services;
    }
}
