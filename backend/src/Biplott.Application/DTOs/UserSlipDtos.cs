using Biplott.Core.Enums;

namespace Biplott.Application.DTOs;

public class SaveSlipRequest
{
    public string GameCode { get; set; } = string.Empty;
    public string? SlipCode { get; set; }
    public string? Title { get; set; }
    public bool IsFavorite { get; set; } = false;
    public List<SaveSlipLineDto> Lines { get; set; } = new();
}

public class SaveSlipLineDto
{
    public string LineLabel { get; set; } = "A";
    public List<SaveSlipNumberDto> Numbers { get; set; } = new();
}

public class SaveSlipNumberDto
{
    public int Value { get; set; }
    public int PoolIndex { get; set; }
    public NumberSource Source { get; set; } = NumberSource.Manual;
    public string? MetadataJson { get; set; }
}

public class SavedSlipSummaryDto
{
    public Guid Id { get; set; }
    public string GameCode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string SlipCode { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsFavorite { get; set; }
    public int CompletedLineCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SavedSlipLineSummaryDto> Lines { get; set; } = new();
}

public class SavedSlipLineSummaryDto
{
    public string LineLabel { get; set; } = "A";
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
    public string DerivedMode { get; set; } = "Mixed"; // "Manual", "Random", "Lucky", "Mixed"
}

public class SavedSlipDetailDto
{
    public Guid Id { get; set; }
    public string GameCode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string SlipCode { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SavedSlipLineDetailDto> Lines { get; set; } = new();
    public List<LuckyStoryDto> LuckyStories { get; set; } = new();
}

public class SavedSlipLineDetailDto
{
    public Guid Id { get; set; }
    public string LineLabel { get; set; } = "A";
    public SlipLineStatus Status { get; set; }
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
    public string DerivedMode { get; set; } = "Mixed";
}

public class LuckyStoryDto
{
    public string LineLabel { get; set; } = "A";
    public int NumberValue { get; set; }
    public string Formatted => NumberValue.ToString("D2");
    public int PoolIndex { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string ChoiceText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? DominantTrait { get; set; }
}

public class ToggleFavoriteResponse
{
    public Guid SlipId { get; set; }
    public bool IsFavorite { get; set; }
}

public class UserActivityDto
{
    public Guid Id { get; set; }
    public string GameCode { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? NumbersJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
