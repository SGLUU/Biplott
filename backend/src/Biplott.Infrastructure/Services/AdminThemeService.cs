using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class AdminThemeService : IAdminThemeService
{
    private readonly BiplottDbContext _dbContext;

    public AdminThemeService(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminThemeDto>> GetThemesPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _dbContext.Themes.Include(t => t.Questions).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s) || t.Code.ToLower().Contains(s) || (t.Description != null && t.Description.ToLower().Contains(s)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new AdminThemeDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Icon = t.Icon,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive,
                QuestionsCount = t.Questions.Count,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminThemeDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminThemeDto?> GetThemeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var theme = await _dbContext.Themes
            .Include(t => t.Questions)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (theme == null) return null;

        return new AdminThemeDto
        {
            Id = theme.Id,
            Code = theme.Code,
            Name = theme.Name,
            Description = theme.Description,
            Icon = theme.Icon,
            SortOrder = theme.SortOrder,
            IsActive = theme.IsActive,
            QuestionsCount = theme.Questions.Count,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }

    public async Task<AdminThemeDto> CreateThemeAsync(CreateThemeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Mã chủ đề (Code) không được để trống.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên chủ đề (Name) không được để trống.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var existing = await _dbContext.Themes.AnyAsync(t => t.Code == codeNormalized, cancellationToken);
        if (existing)
            throw new ArgumentException($"Mã chủ đề '{codeNormalized}' đã tồn tại trong hệ thống.");

        var now = DateTime.UtcNow;
        var theme = new Theme
        {
            Code = codeNormalized,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Icon = request.Icon?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _dbContext.Themes.AddAsync(theme, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminThemeDto
        {
            Id = theme.Id,
            Code = theme.Code,
            Name = theme.Name,
            Description = theme.Description,
            Icon = theme.Icon,
            SortOrder = theme.SortOrder,
            IsActive = theme.IsActive,
            QuestionsCount = 0,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }

    public async Task<AdminThemeDto> UpdateThemeAsync(int id, UpdateThemeRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên chủ đề không được để trống.");

        var theme = await _dbContext.Themes.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (theme == null)
            throw new KeyNotFoundException($"Không tìm thấy chủ đề có ID = {id}.");

        theme.Name = request.Name.Trim();
        theme.Description = request.Description?.Trim();
        theme.Icon = request.Icon?.Trim();
        theme.SortOrder = request.SortOrder;
        theme.IsActive = request.IsActive;
        theme.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminThemeDto
        {
            Id = theme.Id,
            Code = theme.Code,
            Name = theme.Name,
            Description = theme.Description,
            Icon = theme.Icon,
            SortOrder = theme.SortOrder,
            IsActive = theme.IsActive,
            QuestionsCount = theme.Questions.Count,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }

    public async Task<AdminThemeDto> SetThemeStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var theme = await _dbContext.Themes.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (theme == null)
            throw new KeyNotFoundException($"Không tìm thấy chủ đề có ID = {id}.");

        theme.IsActive = isActive;
        theme.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminThemeDto
        {
            Id = theme.Id,
            Code = theme.Code,
            Name = theme.Name,
            Description = theme.Description,
            Icon = theme.Icon,
            SortOrder = theme.SortOrder,
            IsActive = theme.IsActive,
            QuestionsCount = theme.Questions.Count,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }

    public async Task DeleteThemeAsync(int id, CancellationToken cancellationToken = default)
    {
        var theme = await _dbContext.Themes.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (theme == null)
            throw new KeyNotFoundException($"Không tìm thấy chủ đề có ID = {id}.");

        if (theme.Questions.Count > 0)
        {
            throw new InvalidOperationException($"Không thể xóa chủ đề '{theme.Name}' vì đang có {theme.Questions.Count} câu hỏi sử dụng. Vui lòng chuyển sang trạng thái Vô hiệu hóa.");
        }

        _dbContext.Themes.Remove(theme);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}