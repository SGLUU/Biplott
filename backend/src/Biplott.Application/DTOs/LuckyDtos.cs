using Biplott.Core.Enums;

namespace Biplott.Application.DTOs;

public class StartJourneyRequest
{
    public string GameCode { get; set; } = string.Empty;
    public string LineLabel { get; set; } = "A";
    public List<int>? RecentQuestionIds { get; set; }
    public List<int>? RecentThemeIds { get; set; }
}

public class ChoiceDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SubContent { get; set; }
    public string? MediaUrl { get; set; }
    public int OrderIndex { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public string ThemeCode { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public string? ThemeIcon { get; set; }
    public QuestionType QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class StartJourneyResponse
{
    public string JourneyId { get; set; } = string.Empty;
    public string GameCode { get; set; } = string.Empty;
    public string LineLabel { get; set; } = "A";
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 6;
    public int CurrentPoolIndex { get; set; } = 0;
    public string CurrentPoolName { get; set; } = "Dãy số chính";
    public bool IsClimaxStep { get; set; } = false;
    public QuestionDto FirstQuestion { get; set; } = null!;
}

public class AnswerStepRequest
{
    public int QuestionId { get; set; }
    public int ChoiceId { get; set; }
    public List<int>? RecentQuestionIds { get; set; }
    public List<int>? RecentThemeIds { get; set; }
}

public class RevealedNumberDto
{
    public int Value { get; set; }
    public string Formatted => Value.ToString("D2");
    public int PoolIndex { get; set; }
    public NumberSource Source { get; set; } = NumberSource.Lucky;
    public string Explanation { get; set; } = string.Empty;
    public string? DominantTrait { get; set; }
    public string? ThemeName { get; set; }
    public string? QuestionText { get; set; }
    public string? ChoiceText { get; set; }
    public string? MetadataJson { get; set; }
}

public class AnswerStepResponse
{
    public string JourneyId { get; set; } = string.Empty;
    public RevealedNumberDto RevealedNumber { get; set; } = null!;
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; } = 6;
    public int CurrentPoolIndex { get; set; }
    public string CurrentPoolName { get; set; } = "Dãy số chính";
    public bool IsClimaxStep { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public QuestionDto? NextQuestion { get; set; }
    public List<RevealedNumberDto>? CompletedNumbers { get; set; }
    public string? JourneyCommentary { get; set; }
}

public class NoveltyContext
{
    public List<int> AnsweredQuestionIdsInJourney { get; set; } = new();
    public List<int> ThemesUsedInJourney { get; set; } = new();
    public List<QuestionType> QuestionTypesUsedInJourney { get; set; } = new();
    public List<int> RecentQuestionIds { get; set; } = new();
    public List<int> RecentThemeIds { get; set; } = new();
}
