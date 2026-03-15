using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Requests.Auth;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;
}
