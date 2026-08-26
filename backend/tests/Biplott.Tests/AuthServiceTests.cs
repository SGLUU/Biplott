using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Biplott.Tests;

public class AuthServiceTests
{
    private static (BiplottDbContext db, IAuthService authService) CreateTestAuthService(string dbName)
    {
        var services = new ServiceCollection();

        services.AddDbContext<BiplottDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddLogging(builder => builder.AddDebug());

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

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "TestSecretKeyForAuthTestsPhase3_MustBeAtLeast32BytesLong!" },
            { "Jwt:Issuer", "BiplottTest" },
            { "Jwt:Audience", "BiplottTestClient" },
            { "Jwt:ExpiryMinutes", "30" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        services.AddSingleton(configuration);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<BiplottDbContext>();
        var authService = provider.GetRequiredService<IAuthService>();

        return (db, authService);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldCreateUserAndReturnTokens()
    {
        var (db, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        var req = new RegisterRequest
        {
            Email = "user1@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123",
            DisplayName = "Biplot Master"
        };

        var result = await authService.RegisterAsync(req);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal("user1@biplott.local", result.User.Email);
        Assert.Equal("Biplot Master", result.User.DisplayName);

        var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        Assert.NotNull(dbUser);
        Assert.Equal(result.RefreshToken, dbUser.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldThrowArgumentException()
    {
        var (_, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        var req = new RegisterRequest
        {
            Email = "duplicate@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        await authService.RegisterAsync(req);

        await Assert.ThrowsAsync<ArgumentException>(() => authService.RegisterAsync(req));
    }

    [Fact]
    public async Task RegisterAsync_PasswordMismatch_ShouldThrowArgumentException()
    {
        var (_, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        var req = new RegisterRequest
        {
            Email = "mismatch@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password456"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => authService.RegisterAsync(req));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldSucceedAndRotateToken()
    {
        var (db, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        await authService.RegisterAsync(new RegisterRequest
        {
            Email = "login@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        var userBefore = await db.Users.FirstAsync(u => u.Email == "login@biplott.local");
        var oldRefreshToken = userBefore.RefreshToken;

        var loginResult = await authService.LoginAsync(new LoginRequest
        {
            Email = "login@biplott.local",
            Password = "Password123"
        });

        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.AccessToken));
        Assert.NotEqual(oldRefreshToken, loginResult.RefreshToken); // Token Rotated
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldThrowUnauthorized()
    {
        var (_, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        await authService.RegisterAsync(new RegisterRequest
        {
            Email = "wrongpass@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequest
            {
                Email = "wrongpass@biplott.local",
                Password = "WrongPassword999"
            }));
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ShouldRotateRefreshToken()
    {
        var (_, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        var reg = await authService.RegisterAsync(new RegisterRequest
        {
            Email = "refresh@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        var refreshResult = await authService.RefreshTokenAsync(reg.RefreshToken);

        Assert.NotNull(refreshResult);
        Assert.NotEqual(reg.RefreshToken, refreshResult.RefreshToken); // Rotated
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.AccessToken));
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken()
    {
        var (db, authService) = CreateTestAuthService(Guid.NewGuid().ToString());

        var reg = await authService.RegisterAsync(new RegisterRequest
        {
            Email = "logout@biplott.local",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        await authService.LogoutAsync(reg.User.Id);

        var dbUser = await db.Users.FirstAsync(u => u.Id == reg.User.Id);
        Assert.Null(dbUser.RefreshToken);
        Assert.NotNull(dbUser.RefreshTokenRevokedAt);

        // Trying to refresh should now fail
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.RefreshTokenAsync(reg.RefreshToken));
    }
}
