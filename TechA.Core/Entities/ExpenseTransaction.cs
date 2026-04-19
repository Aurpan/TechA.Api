using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Entities;

public class ExpenseTransaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(256)]
    public string ProductName { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public ExpenseType Type { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
