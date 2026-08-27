using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BiplottDbContext _dbContext;

    public AdminUserService(UserManager<ApplicationUser> userManager, BiplottDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? role,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => (u.Email != null && u.Email.ToLower().Contains(s)) || u.DisplayName.ToLower().Contains(s));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var roles = (await _userManager.GetRolesAsync(u)).ToList();
            if (!string.IsNullOrWhiteSpace(role) && !roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var slipsCount = await _dbContext.Slips.CountAsync(s => s.UserId == u.Id, cancellationToken);

            items.Add(new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                DisplayName = u.DisplayName,
                Roles = roles,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                SavedSlipsCount = slipsCount
            });
        }

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var slipsCount = await _dbContext.Slips.CountAsync(s => s.UserId == user.Id, cancellationToken);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Roles = roles,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            SavedSlipsCount = slipsCount
        };
    }

    public async Task<AdminUserDto> SetUserStatusAsync(
        string currentAdminId,
        string targetUserId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (targetUser == null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng có ID = '{targetUserId}'.");

        if (!isActive && string.Equals(currentAdminId, targetUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Bạn không thể tự vô hiệu hóa tài khoản Quản trị viên của chính mình.");
        }

        var roles = (await _userManager.GetRolesAsync(targetUser)).ToList();
        if (!isActive && roles.Contains("Admin"))
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var activeAdminsCount = adminUsers.Count(a => a.IsActive && a.Id != targetUserId);
            if (activeAdminsCount == 0)
            {
                throw new InvalidOperationException("Không thể vô hiệu hóa Quản trị viên đang hoạt động cuối cùng của hệ thống.");
            }
        }

        targetUser.IsActive = isActive;
        targetUser.UpdatedAt = DateTime.UtcNow;

        if (!isActive)
        {
            targetUser.RefreshToken = null;
            targetUser.RefreshTokenRevokedAt = DateTime.UtcNow;
        }

        await _userManager.UpdateAsync(targetUser);

        var slipsCount = await _dbContext.Slips.CountAsync(s => s.UserId == targetUser.Id, cancellationToken);

        return new AdminUserDto
        {
            Id = targetUser.Id,
            Email = targetUser.Email ?? string.Empty,
            DisplayName = targetUser.DisplayName,
            Roles = roles,
            IsActive = targetUser.IsActive,
            CreatedAt = targetUser.CreatedAt,
            UpdatedAt = targetUser.UpdatedAt,
            SavedSlipsCount = slipsCount
        };
    }
}