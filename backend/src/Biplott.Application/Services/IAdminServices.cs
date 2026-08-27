using Biplott.Application.DTOs;

namespace Biplott.Application.Services;

public interface IEngineConfigService
{
    Task<AdminSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<AdminSettingsDto> UpdateSettingsAsync(AdminSettingsDto settings, CancellationToken cancellationToken = default);
    Task<AdminSettingsDto> ResetToDefaultsAsync(CancellationToken cancellationToken = default);
    LuckyEngineConfigDto GetCurrentLuckyConfig();
    NoveltyEngineConfigDto GetCurrentNoveltyConfig();
    RandomEngineConfigDto GetCurrentRandomConfig();
}

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);
}

public interface IAdminThemeService
{
    Task<PagedResult<AdminThemeDto>> GetThemesPagedAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken = default);
    Task<AdminThemeDto?> GetThemeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminThemeDto> CreateThemeAsync(CreateThemeRequest request, CancellationToken cancellationToken = default);
    Task<AdminThemeDto> UpdateThemeAsync(int id, UpdateThemeRequest request, CancellationToken cancellationToken = default);
    Task<AdminThemeDto> SetThemeStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteThemeAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAdminTraitService
{
    Task<PagedResult<AdminTraitDto>> GetTraitsPagedAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminTraitDto>> GetAllActiveTraitsAsync(CancellationToken cancellationToken = default);
    Task<AdminTraitDto?> GetTraitByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminTraitDto> CreateTraitAsync(CreateTraitRequest request, CancellationToken cancellationToken = default);
    Task<AdminTraitDto> UpdateTraitAsync(int id, UpdateTraitRequest request, CancellationToken cancellationToken = default);
    Task<AdminTraitDto> SetTraitStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteTraitAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAdminQuestionService
{
    Task<PagedResult<AdminQuestionListDto>> GetQuestionsPagedAsync(QuestionFilterParams filter, CancellationToken cancellationToken = default);
    Task<AdminQuestionDetailDto?> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminQuestionDetailDto> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<AdminQuestionDetailDto> UpdateQuestionAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<AdminQuestionDetailDto> DuplicateQuestionAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminQuestionDetailDto> SetQuestionStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default);
}

public interface IContentImportService
{
    Task<ImportValidationResultDto> ValidateImportFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<ImportConfirmResponseDto> ConfirmImportAsync(ImportConfirmRequest request, CancellationToken cancellationToken = default);
    Task<(byte[] FileBytes, string ContentType, string FileName)> GenerateTemplateAsync(string format, CancellationToken cancellationToken = default);
}

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(int page, int pageSize, string? search, bool? isActive, string? role, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<AdminUserDto> SetUserStatusAsync(string currentAdminId, string targetUserId, bool isActive, CancellationToken cancellationToken = default);
}