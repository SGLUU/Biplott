namespace Biplott.Core.Entities;

public class UserActivityHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string ActivityType { get; set; } = "CompletedLine"; // CompletedManualLine, GeneratedRandomLine, CompletedLuckyJourney, CompletedMixedLine, SavedSlip
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? NumbersJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Game Game { get; set; } = null!;
}
