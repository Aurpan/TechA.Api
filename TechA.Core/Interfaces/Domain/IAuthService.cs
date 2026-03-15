using TechA.Core.Requests.Auth;
using TechA.Core.Responses;

namespace TechA.Core.Interfaces.Domain;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleSignInAsync(GoogleAuthRequest request);
}
