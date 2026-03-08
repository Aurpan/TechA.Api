using TechA.Core.RequestObjects.Auth;
using TechA.Core.ResponseObjects.Auth;

namespace TechA.Core.Interfaces.Service;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleSignInAsync(GoogleAuthRequest request);
}
