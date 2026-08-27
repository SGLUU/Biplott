using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class AdminTraitService : IAdminTraitService
{
    private readonly BiplottDbContext _dbContext;

    public AdminTraitService(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminTraitDto>> GetTraitsPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _dbContext.Traits.Include(t => t.ChoiceTraits).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s) || t.Code.ToLower().Contains(s) || (t.Category != null && t.Category.ToLower().Contains(s)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new AdminTraitDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                IsActive = t.IsActive,
                ChoicesCount = t.ChoiceTraits.Count,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminTraitDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<AdminTraitDto>> GetAllActiveTraitsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Traits
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new AdminTraitDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                IsActive = t.IsActive,
                ChoicesCount = t.ChoiceTraits.Count,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminTraitDto?> GetTraitByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var trait = await _dbContext.Traits
            .Include(t => t.ChoiceTraits)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (trait == null) return null;

        return new AdminTraitDto
        {
            Id = trait.Id,
            Code = trait.Code,
            Name = trait.Name,
            Description = trait.Description,
            Category = trait.Category,
            IsActive = trait.IsActive,
            ChoicesCount = trait.ChoiceTraits.Count,
            CreatedAt = trait.CreatedAt,
            UpdatedAt = trait.UpdatedAt
        };
    }

    public async Task<AdminTraitDto> CreateTraitAsync(CreateTraitRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Mã thuộc tính (Code) không được để trống.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên thuộc tính (Name) không được để trống.");

        var codeNormalized = request.Code.Trim();
        var existing = await _dbContext.Traits.AnyAsync(t => t.Code.ToLower() == codeNormalized.ToLower(), cancellationToken);
        if (existing)
            throw new ArgumentException($"Mã thuộc tính '{codeNormalized}' đã tồn tại.");

        var now = DateTime.UtcNow;
        var trait = new Trait
        {
            Code = codeNormalized,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Category = request.Category?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _dbContext.Traits.AddAsync(trait, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminTraitDto
        {
            Id = trait.Id,
            Code = trait.Code,
            Name = trait.Name,
            Description = trait.Description,
            Category = trait.Category,
            IsActive = trait.IsActive,
            ChoicesCount = 0,
            CreatedAt = trait.CreatedAt,
            UpdatedAt = trait.UpdatedAt
        };
    }

    public async Task<AdminTraitDto> UpdateTraitAsync(int id, UpdateTraitRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên thuộc tính không được để trống.");

        var trait = await _dbContext.Traits.Include(t => t.ChoiceTraits).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (trait == null)
            throw new KeyNotFoundException($"Không tìm thấy thuộc tính có ID = {id}.");

        trait.Name = request.Name.Trim();
        trait.Description = request.Description?.Trim();
        trait.Category = request.Category?.Trim();
        trait.IsActive = request.IsActive;
        trait.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminTraitDto
        {
            Id = trait.Id,
            Code = trait.Code,
            Name = trait.Name,
            Description = trait.Description,
            Category = trait.Category,
            IsActive = trait.IsActive,
            ChoicesCount = trait.ChoiceTraits.Count,
            CreatedAt = trait.CreatedAt,
            UpdatedAt = trait.UpdatedAt
        };
    }

    public async Task<AdminTraitDto> SetTraitStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var trait = await _dbContext.Traits.Include(t => t.ChoiceTraits).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (trait == null)
            throw new KeyNotFoundException($"Không tìm thấy thuộc tính có ID = {id}.");

        trait.IsActive = isActive;
        trait.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminTraitDto
        {
            Id = trait.Id,
            Code = trait.Code,
            Name = trait.Name,
            Description = trait.Description,
            Category = trait.Category,
            IsActive = trait.IsActive,
            ChoicesCount = trait.ChoiceTraits.Count,
            CreatedAt = trait.CreatedAt,
            UpdatedAt = trait.UpdatedAt
        };
    }

    public async Task DeleteTraitAsync(int id, CancellationToken cancellationToken = default)
    {
        var trait = await _dbContext.Traits.Include(t => t.ChoiceTraits).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (trait == null)
            throw new KeyNotFoundException($"Không tìm thấy thuộc tính có ID = {id}.");

        if (trait.ChoiceTraits.Count > 0)
        {
            throw new InvalidOperationException($"Không thể xóa thuộc tính '{trait.Name}' vì đang được liên kết bởi {trait.ChoiceTraits.Count} lựa chọn. Vui lòng chuyển sang trạng thái Vô hiệu hóa.");
        }

        _dbContext.Traits.Remove(trait);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}