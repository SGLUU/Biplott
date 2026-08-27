namespace Biplott.Application.DTOs;

public class TraitScoreDto
{
    public string TraitCode { get; set; } = string.Empty;
    public string TraitName { get; set; } = string.Empty;
    public int Score { get; set; } // 0 - 100
    public int SampleCount { get; set; }
}

public class LuckyDnaResponse
{
    public string Status { get; set; } = "NotFormed"; // NotFormed | Forming | Completed
    public int TotalAnswers { get; set; }
    public string Archetype { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TraitScoreDto> TopTraits { get; set; } = new();
    public List<TraitScoreDto> AllTraits { get; set; } = new();
    public DateTime? UpdatedAt { get; set; }
}
