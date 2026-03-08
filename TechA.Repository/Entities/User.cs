using System.ComponentModel.DataAnnotations;
using TechA.Core.Enums;

namespace TechA.Repository.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    public AuthProvider AuthProvider { get; set; } = AuthProvider.Email;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserProfile? Profile { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
