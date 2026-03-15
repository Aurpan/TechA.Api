using System.ComponentModel.DataAnnotations;

namespace TechA.Core.Requests.Auth;

public class GoogleAuthRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
