using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechA.Core.Interfaces.Service;
using TechA.Core.RequestObjects.Profile;

namespace TechA.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromClaims();
        
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var profile = await _userProfileService.GetProfileAsync(userId);

        if (profile is null)
            return NotFound(new { Message = "User not found." });

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserIdFromClaims();
        
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var profile = await _userProfileService.UpdateProfileAsync(userId, request);

        if (profile is null)
            return BadRequest(new { Message = "Failed to update profile." });

        return Ok(profile);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
