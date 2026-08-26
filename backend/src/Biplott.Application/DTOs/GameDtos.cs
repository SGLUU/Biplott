namespace Biplott.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string Message { get; set; } = "Thao tác thành công";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Thao tác thành công") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Data = default, Message = message };
}

public class GamePoolDto
{
    public int Id { get; set; }
    public int PoolIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public int PickCount { get; set; }
    public bool AllowDuplicates { get; set; }
    public string? BadgeColor { get; set; }
}

public class GameDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public List<GamePoolDto> Pools { get; set; } = new();
}
