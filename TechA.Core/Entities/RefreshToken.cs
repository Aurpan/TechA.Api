using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Entities;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt is not null;

    public bool IsActive => !IsExpired && !IsRevoked;
}
