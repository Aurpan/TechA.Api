using Microsoft.AspNetCore.Mvc;
using TechA.Core.Interfaces.Domain;
using TechA.Core.Requests.Auth;

namespace TechA.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new { result.Token, result.User });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(new { result.Message });

        return Ok(new { result.Token, result.User });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.GoogleSignInAsync(request);

        if (!result.Success)
            return Unauthorized(new { result.Message });

        return Ok(new { result.Token, result.User });
    }
}
