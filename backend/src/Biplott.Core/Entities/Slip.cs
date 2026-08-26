using Biplott.Core.Enums;

namespace Biplott.Core.Entities;

public class Slip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? UserId { get; set; }
    public string? GuestSessionToken { get; set; }
    public int GameId { get; set; }
    public string SlipCode { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsFavorite { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Game Game { get; set; } = null!;
    public List<SlipLine> Lines { get; set; } = new();
}

public class SlipLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SlipId { get; set; }
    public string LineLabel { get; set; } = "A"; // A, B, C, D, E, F
    public SlipLineStatus Status { get; set; } = SlipLineStatus.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Slip Slip { get; set; } = null!;
    public List<SlipLineNumber> Numbers { get; set; } = new();
}

public class SlipLineNumber
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SlipLineId { get; set; }
    public int Value { get; set; }
    public int PoolIndex { get; set; } = 0;
    public NumberSource Source { get; set; } = NumberSource.Manual;
    public int OrderIndex { get; set; } = 0;
    public string? MetadataJson { get; set; }

    public SlipLine SlipLine { get; set; } = null!;
}
