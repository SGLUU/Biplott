using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Biplott.Tests;

public class AdminUserServiceTests
{
    private static async Task<(BiplottDbContext db, UserManager<ApplicationUser> userManager, AdminUserService service, ApplicationUser admin1, ApplicationUser admin2, ApplicationUser normalUser)> CreateTestContextAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BiplottDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<BiplottDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<BiplottDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateAsync(new IdentityRole("Admin"));
        await roleManager.CreateAsync(new IdentityRole("User"));

        var admin1 = new ApplicationUser { UserName = "admin1@biplott.local", Email = "admin1@biplott.local", DisplayName = "Admin 1", IsActive = true };
        var admin2 = new ApplicationUser { UserName = "admin2@biplott.local", Email = "admin2@biplott.local", DisplayName = "Admin 2", IsActive = true };
        var normalUser = new ApplicationUser { UserName = "user@biplott.local", Email = "user@biplott.local", DisplayName = "Regular User", IsActive = true, RefreshToken = "valid_token" };

        await userManager.CreateAsync(admin1, "Admin123!");
        await userManager.AddToRoleAsync(admin1, "Admin");

        await userManager.CreateAsync(admin2, "Admin123!");
        await userManager.AddToRoleAsync(admin2, "Admin");

        await userManager.CreateAsync(normalUser, "User123!");
        await userManager.AddToRoleAsync(normalUser, "User");

        var service = new AdminUserService(userManager, db);
        return (db, userManager, service, admin1, admin2, normalUser);
    }

    [Fact]
    public async Task SetUserStatus_SelfDeactivation_ShouldThrowInvalidOperationException()
    {
        var (_, _, service, admin1, _, _) = await CreateTestContextAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetUserStatusAsync(admin1.Id, admin1.Id, false));
    }

    [Fact]
    public async Task SetUserStatus_DeactivateLastActiveAdmin_ShouldThrowInvalidOperationException()
    {
        var (_, userManager, service, admin1, admin2, _) = await CreateTestContextAsync();

        // First deactivate admin2
        await service.SetUserStatusAsync(admin1.Id, admin2.Id, false);

        // Now trying to deactivate admin1 (who is now the only active admin) should fail
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetUserStatusAsync(admin2.Id, admin1.Id, false));
    }

    [Fact]
    public async Task SetUserStatus_DeactivateNormalUser_ShouldRevokeRefreshToken()
    {
        var (_, userManager, service, admin1, _, normalUser) = await CreateTestContextAsync();

        var result = await service.SetUserStatusAsync(admin1.Id, normalUser.Id, false);

        Assert.False(result.IsActive);
        var updated = await userManager.FindByIdAsync(normalUser.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
        Assert.Null(updated.RefreshToken);
        Assert.NotNull(updated.RefreshTokenRevokedAt);
    }
}