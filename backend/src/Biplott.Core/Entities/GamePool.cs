namespace Biplott.Core.Entities;

public class GamePool
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int PoolIndex { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public int MinNumber { get; set; } = 1;
    public int MaxNumber { get; set; }
    public int PickCount { get; set; }
    public bool AllowDuplicates { get; set; } = false;
    public string? BadgeColor { get; set; }

    public Game Game { get; set; } = null!;
}
