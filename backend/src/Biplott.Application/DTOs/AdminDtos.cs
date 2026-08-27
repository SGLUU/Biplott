using Biplott.Core.Enums;

namespace Biplott.Application.DTOs;



// ----------------------------------------------------
// 1. Dashboard
// ----------------------------------------------------
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalSavedSlips { get; set; }
    public int TotalThemes { get; set; }
    public int TotalQuestions { get; set; }
    public int ActiveQuestions { get; set; }
    public int InactiveQuestions { get; set; }
    public int TotalChoices { get; set; }
    public int TotalTraits { get; set; }
    public List<AdminUserDto> RecentUsers { get; set; } = new();
    public List<AdminQuestionListDto> RecentQuestions { get; set; } = new();
}

// ----------------------------------------------------
// 2. Themes
// ----------------------------------------------------
public class AdminThemeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int QuestionsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateThemeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateThemeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

// ----------------------------------------------------
// 3. Traits
// ----------------------------------------------------
public class AdminTraitDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public int ChoicesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTraitRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTraitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}

// ----------------------------------------------------
// 4. Questions & Choices
// ----------------------------------------------------
public class AdminQuestionListDto
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public string ThemeCode { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsActive { get; set; }
    public long ViewCount { get; set; }
    public int ChoicesCount { get; set; }
    public int ActiveChoicesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminChoiceTraitDto
{
    public int TraitId { get; set; }
    public string TraitCode { get; set; } = string.Empty;
    public string TraitName { get; set; } = string.Empty;
    public double Weight { get; set; }
}

public class AdminChoiceDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SubContent { get; set; }
    public string? MediaUrl { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public List<AdminChoiceTraitDto> ChoiceTraits { get; set; } = new();
}

public class AdminQuestionDetailDto
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public string ThemeCode { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsActive { get; set; }
    public long ViewCount { get; set; }
    public List<AdminChoiceDto> Choices { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateChoiceTraitRequest
{
    public int TraitId { get; set; }
    public double Weight { get; set; }
}

public class CreateChoiceRequest
{
    public string Content { get; set; } = string.Empty;
    public string? SubContent { get; set; }
    public string? MediaUrl { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CreateChoiceTraitRequest> ChoiceTraits { get; set; } = new();
}

public class CreateQuestionRequest
{
    public int ThemeId { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CreateChoiceRequest> Choices { get; set; } = new();
}

public class UpdateQuestionRequest
{
    public int ThemeId { get; set; }
    public QuestionType QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsActive { get; set; }
    public List<CreateChoiceRequest> Choices { get; set; } = new();
}

public class QuestionFilterParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? ThemeId { get; set; }
    public QuestionType? QuestionType { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } // "updatedAt_desc", "createdAt_desc", "content_asc", "viewCount_desc"
}

// ----------------------------------------------------
// 5. Bulk Content Import
// ----------------------------------------------------
public class ImportRowErrorDto
{
    public int RowIndex { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ImportTraitPreviewDto
{
    public string TraitCode { get; set; } = string.Empty;
    public double Weight { get; set; }
}

public class ImportChoicePreviewDto
{
    public string Content { get; set; } = string.Empty;
    public string? SubContent { get; set; }
    public List<ImportTraitPreviewDto> Traits { get; set; } = new();
}

public class ImportQuestionPreviewDto
{
    public int RowIndex { get; set; }
    public string ThemeCode { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "SingleChoice";
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<ImportChoicePreviewDto> Choices { get; set; } = new();
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}

public class ImportValidationResultDto
{
    public bool IsValid { get; set; }
    public int TotalRows { get; set; }
    public int ValidCount { get; set; }
    public int InvalidCount { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = new();
    public List<ImportQuestionPreviewDto> PreviewItems { get; set; } = new();
    public string ImportSessionId { get; set; } = string.Empty;
}

public class ImportConfirmRequest
{
    public string? ImportSessionId { get; set; }
    public List<ImportQuestionPreviewDto>? Items { get; set; }
}

public class ImportConfirmResponseDto
{
    public bool Success { get; set; }
    public int ImportedQuestionsCount { get; set; }
    public int ImportedChoicesCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

// ----------------------------------------------------
// 6. Engine Settings
// ----------------------------------------------------
public class LuckyEngineConfigDto
{
    public double BaseWeight { get; set; } = 10.0;
    public double TraitAffinityMultiplier { get; set; } = 5.0;
    public double NoiseMagnitude { get; set; } = 2.0;
    public double MinWeight { get; set; } = 1.0;
}

public class NoveltyEngineConfigDto
{
    public double BaseWeight { get; set; } = 100.0;
    public double NeverSeenBonus { get; set; } = 50.0;
    public double RecentlySeenPenalty { get; set; } = 70.0;
    public double RepeatedThemePenalty { get; set; } = 60.0;
    public double RecentThemePenalty { get; set; } = 30.0;
    public double QuestionTypeDiversityBonus { get; set; } = 25.0;
    public double ClimaxDestinyThemeBoost { get; set; } = 500.0;
    public double ClimaxQuickInstinctBoost { get; set; } = 100.0;
}

public class RandomEngineConfigDto
{
    public int BalancedMaxDeviation { get; set; } = 1;
    public int SpreadMinPartitions { get; set; } = 3;
    public bool EnableSurpriseOutliers { get; set; } = true;
}

public class AdminSettingsDto
{
    public LuckyEngineConfigDto Lucky { get; set; } = new();
    public NoveltyEngineConfigDto Novelty { get; set; } = new();
    public RandomEngineConfigDto Random { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ----------------------------------------------------
// 7. Users
// ----------------------------------------------------
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int SavedSlipsCount { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}