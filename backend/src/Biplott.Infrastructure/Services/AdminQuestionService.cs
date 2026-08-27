using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class AdminQuestionService : IAdminQuestionService
{
    private readonly BiplottDbContext _dbContext;

    public AdminQuestionService(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminQuestionListDto>> GetQuestionsPagedAsync(
        QuestionFilterParams filter,
        CancellationToken cancellationToken = default)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 || filter.PageSize > 100 ? 20 : filter.PageSize;

        var query = _dbContext.Questions
            .Include(q => q.Theme)
            .Include(q => q.Choices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(q => q.Content.ToLower().Contains(s) || (q.Subtitle != null && q.Subtitle.ToLower().Contains(s)));
        }

        if (filter.ThemeId.HasValue && filter.ThemeId.Value > 0)
        {
            query = query.Where(q => q.ThemeId == filter.ThemeId.Value);
        }

        if (filter.QuestionType.HasValue)
        {
            query = query.Where(q => q.QuestionType == filter.QuestionType.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(q => q.IsActive == filter.IsActive.Value);
        }

        query = filter.SortBy switch
        {
            "createdAt_desc" => query.OrderByDescending(q => q.CreatedAt),
            "createdAt_asc" => query.OrderBy(q => q.CreatedAt),
            "updatedAt_asc" => query.OrderBy(q => q.UpdatedAt),
            "content_asc" => query.OrderBy(q => q.Content),
            "viewCount_desc" => query.OrderByDescending(q => q.ViewCount),
            _ => query.OrderByDescending(q => q.UpdatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new AdminQuestionListDto
            {
                Id = q.Id,
                ThemeId = q.ThemeId,
                ThemeCode = q.Theme.Code,
                ThemeName = q.Theme.Name,
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
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminQuestionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminQuestionDetailDto?> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var q = await _dbContext.Questions
            .Include(q => q.Theme)
            .Include(q => q.Choices)
                .ThenInclude(c => c.ChoiceTraits)
                    .ThenInclude(ct => ct.Trait)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (q == null) return null;

        return MapToDetailDto(q);
    }

    public async Task<AdminQuestionDetailDto> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateQuestionInputAsync(request.ThemeId, request.Content, request.IsActive, request.Choices, cancellationToken);

        var now = DateTime.UtcNow;
        var question = new Question
        {
            ThemeId = request.ThemeId,
            QuestionType = request.QuestionType,
            Content = request.Content.Trim(),
            Subtitle = request.Subtitle?.Trim(),
            MediaUrl = request.MediaUrl?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        int orderIdx = 0;
        foreach (var cReq in request.Choices)
        {
            var choice = new QuestionChoice
            {
                Content = cReq.Content.Trim(),
                SubContent = cReq.SubContent?.Trim(),
                MediaUrl = cReq.MediaUrl?.Trim(),
                OrderIndex = cReq.OrderIndex > 0 ? cReq.OrderIndex : orderIdx++,
                IsActive = cReq.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var ctReq in cReq.ChoiceTraits)
            {
                choice.ChoiceTraits.Add(new ChoiceTrait
                {
                    TraitId = ctReq.TraitId,
                    Weight = Math.Clamp(ctReq.Weight, 0.0, 1.0)
                });
            }

            question.Choices.Add(choice);
        }

        await _dbContext.Questions.AddAsync(question, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetQuestionByIdAsync(question.Id, cancellationToken))!;
    }

    public async Task<AdminQuestionDetailDto> UpdateQuestionAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateQuestionInputAsync(request.ThemeId, request.Content, request.IsActive, request.Choices, cancellationToken);

        var question = await _dbContext.Questions
            .Include(q => q.Choices)
                .ThenInclude(c => c.ChoiceTraits)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi có ID = {id}.");

        var now = DateTime.UtcNow;
        question.ThemeId = request.ThemeId;
        question.QuestionType = request.QuestionType;
        question.Content = request.Content.Trim();
        question.Subtitle = request.Subtitle?.Trim();
        question.MediaUrl = request.MediaUrl?.Trim();
        question.IsActive = request.IsActive;
        question.UpdatedAt = now;

        _dbContext.QuestionChoices.RemoveRange(question.Choices);
        question.Choices.Clear();

        int orderIdx = 0;
        foreach (var cReq in request.Choices)
        {
            var choice = new QuestionChoice
            {
                QuestionId = question.Id,
                Content = cReq.Content.Trim(),
                SubContent = cReq.SubContent?.Trim(),
                MediaUrl = cReq.MediaUrl?.Trim(),
                OrderIndex = cReq.OrderIndex > 0 ? cReq.OrderIndex : orderIdx++,
                IsActive = cReq.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var ctReq in cReq.ChoiceTraits)
            {
                choice.ChoiceTraits.Add(new ChoiceTrait
                {
                    TraitId = ctReq.TraitId,
                    Weight = Math.Clamp(ctReq.Weight, 0.0, 1.0)
                });
            }

            question.Choices.Add(choice);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetQuestionByIdAsync(question.Id, cancellationToken))!;
    }

    public async Task<AdminQuestionDetailDto> DuplicateQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var original = await _dbContext.Questions
            .Include(q => q.Choices)
                .ThenInclude(c => c.ChoiceTraits)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (original == null)
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi có ID = {id}.");

        var now = DateTime.UtcNow;
        var duplicate = new Question
        {
            ThemeId = original.ThemeId,
            QuestionType = original.QuestionType,
            Content = $"{original.Content} (Bản sao)",
            Subtitle = original.Subtitle,
            MediaUrl = original.MediaUrl,
            IsActive = false,
            ViewCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var c in original.Choices)
        {
            var newChoice = new QuestionChoice
            {
                Content = c.Content,
                SubContent = c.SubContent,
                MediaUrl = c.MediaUrl,
                OrderIndex = c.OrderIndex,
                IsActive = c.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var ct in c.ChoiceTraits)
            {
                newChoice.ChoiceTraits.Add(new ChoiceTrait
                {
                    TraitId = ct.TraitId,
                    Weight = ct.Weight
                });
            }

            duplicate.Choices.Add(newChoice);
        }

        await _dbContext.Questions.AddAsync(duplicate, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetQuestionByIdAsync(duplicate.Id, cancellationToken))!;
    }

    public async Task<AdminQuestionDetailDto> SetQuestionStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var question = await _dbContext.Questions
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi có ID = {id}.");

        if (isActive)
        {
            var activeChoices = question.Choices.Count(c => c.IsActive);
            if (activeChoices < 2)
            {
                throw new InvalidOperationException($"Không thể kích hoạt câu hỏi vì cần ít nhất 2 lựa chọn đang hoạt động (hiện có {activeChoices}).");
            }
        }

        question.IsActive = isActive;
        question.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetQuestionByIdAsync(question.Id, cancellationToken))!;
    }

    public async Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _dbContext.Questions
            .Include(q => q.Histories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi có ID = {id}.");

        if (question.Histories.Count > 0)
        {
            question.IsActive = false;
            question.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _dbContext.Questions.Remove(question);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ValidateQuestionInputAsync(
        int themeId,
        string content,
        bool isActive,
        List<CreateChoiceRequest> choices,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung câu hỏi không được để trống.");

        var themeExists = await _dbContext.Themes.AnyAsync(t => t.Id == themeId, cancellationToken);
        if (!themeExists)
            throw new ArgumentException($"Chủ đề có ID = {themeId} không tồn tại.");

        if (choices == null || choices.Count == 0)
            throw new ArgumentException("Câu hỏi phải có ít nhất một lựa chọn.");

        if (isActive)
        {
            var activeCount = choices.Count(c => c.IsActive);
            if (activeCount < 2)
                throw new ArgumentException($"Câu hỏi đang kích hoạt phải có ít nhất 2 lựa chọn hoạt động (hiện có {activeCount}).");
        }

        foreach (var c in choices)
        {
            if (string.IsNullOrWhiteSpace(c.Content))
                throw new ArgumentException("Nội dung lựa chọn không được để trống.");

            foreach (var ct in c.ChoiceTraits)
            {
                if (ct.Weight < 0.0 || ct.Weight > 1.0)
                    throw new ArgumentException($"Trọng số thuộc tính phải nằm trong khoảng từ 0.0 đến 1.0 (nhận được: {ct.Weight}).");

                var traitExists = await _dbContext.Traits.AnyAsync(t => t.Id == ct.TraitId, cancellationToken);
                if (!traitExists)
                    throw new ArgumentException($"Thuộc tính có ID = {ct.TraitId} không tồn tại.");
            }
        }
    }

    private static AdminQuestionDetailDto MapToDetailDto(Question q)
    {
        return new AdminQuestionDetailDto
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
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
            Choices = q.Choices
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Id)
                .Select(c => new AdminChoiceDto
                {
                    Id = c.Id,
                    QuestionId = c.QuestionId,
                    Content = c.Content,
                    SubContent = c.SubContent,
                    MediaUrl = c.MediaUrl,
                    OrderIndex = c.OrderIndex,
                    IsActive = c.IsActive,
                    ChoiceTraits = c.ChoiceTraits.Select(ct => new AdminChoiceTraitDto
                    {
                        TraitId = ct.TraitId,
                        TraitCode = ct.Trait?.Code ?? string.Empty,
                        TraitName = ct.Trait?.Name ?? string.Empty,
                        Weight = ct.Weight
                    }).ToList()
                }).ToList()
        };
    }
}