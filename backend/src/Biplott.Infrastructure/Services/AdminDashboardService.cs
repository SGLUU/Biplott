using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly BiplottDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardService(BiplottDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<AdminDashboardDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
        var totalSavedSlips = await _dbContext.Slips.CountAsync(cancellationToken);
        var totalThemes = await _dbContext.Themes.CountAsync(cancellationToken);
        var totalQuestions = await _dbContext.Questions.CountAsync(cancellationToken);
        var activeQuestions = await _dbContext.Questions.CountAsync(q => q.IsActive, cancellationToken);
        var inactiveQuestions = totalQuestions - activeQuestions;
        var totalChoices = await _dbContext.QuestionChoices.CountAsync(cancellationToken);
        var totalTraits = await _dbContext.Traits.CountAsync(cancellationToken);

        var recentUsersList = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var recentUsers = new List<AdminUserDto>();
        foreach (var u in recentUsersList)
        {
            var roles = (await _userManager.GetRolesAsync(u)).ToList();
            var slipsCount = await _dbContext.Slips.CountAsync(s => s.UserId == u.Id, cancellationToken);
            recentUsers.Add(new AdminUserDto
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

        var recentQuestionsList = await _dbContext.Questions
            .Include(q => q.Theme)
            .Include(q => q.Choices)
            .OrderByDescending(q => q.UpdatedAt)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var recentQuestions = recentQuestionsList.Select(q => new AdminQuestionListDto
        {
            Id = q.Id,
            ThemeId = q.ThemeId,
            ThemeCode = q.Theme?.Code ?? string.Empty,
            ThemeName = q.Theme?.Name ?? string.Empty,
            QuestionType = q.QuestionType,
            Content = q.Content,
            Subtitle = q.Subtitle,
            MediaUrl = q.MediaUrl,
            IsActive = q.IsActive,
            ViewCount = q.ViewCount,
            ChoicesCount = q.Choices.Count,
            ActiveChoicesCount = q.Choices.Count(c => c.IsActive),
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalSavedSlips = totalSavedSlips,
            TotalThemes = totalThemes,
            TotalQuestions = totalQuestions,
            ActiveQuestions = activeQuestions,
            InactiveQuestions = inactiveQuestions,
            TotalChoices = totalChoices,
            TotalTraits = totalTraits,
            RecentUsers = recentUsers,
            RecentQuestions = recentQuestions
        };
    }
}