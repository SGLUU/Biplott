using Biplott.Core.Enums;

namespace Biplott.Core.Entities;

public class Theme
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Question> Questions { get; set; } = new();
}

public class Trait
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ChoiceTrait> ChoiceTraits { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;
    public string Content { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public long ViewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Theme Theme { get; set; } = null!;
    public List<QuestionChoice> Choices { get; set; } = new();
    public List<UserQuestionHistory> Histories { get; set; } = new();
}

public class QuestionChoice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SubContent { get; set; }
    public string? MediaUrl { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
    public List<ChoiceTrait> ChoiceTraits { get; set; } = new();
}

public class ChoiceTrait
{
    public int Id { get; set; }
    public int QuestionChoiceId { get; set; }
    public int TraitId { get; set; }
    public double Weight { get; set; } = 0.0;

    public QuestionChoice QuestionChoice { get; set; } = null!;
    public Trait Trait { get; set; } = null!;
}

public class UserQuestionHistory
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? GuestSessionToken { get; set; }
    public int QuestionId { get; set; }
    public int ChoiceId { get; set; }
    public int RevealedNumber { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
    public QuestionChoice Choice { get; set; } = null!;
}

public class EngineConfig
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string ValueJson { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
