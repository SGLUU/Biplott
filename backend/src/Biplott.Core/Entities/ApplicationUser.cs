using Microsoft.AspNetCore.Identity;

namespace Biplott.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Refresh Token Management
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? RefreshTokenRevokedAt { get; set; }

    public List<Slip> Slips { get; set; } = new();
    public List<UserActivityHistory> ActivityHistories { get; set; } = new();
}
