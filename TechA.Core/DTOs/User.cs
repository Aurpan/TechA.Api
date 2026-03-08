namespace TechA.Core.DTOs;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string AuthProvider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
