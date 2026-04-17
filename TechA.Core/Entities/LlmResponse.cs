using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Entities;

public class LlmResponse
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(128)]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string ResponseJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
