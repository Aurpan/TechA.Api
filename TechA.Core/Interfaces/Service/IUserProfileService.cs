using TechA.Core.DTOs;
using TechA.Core.RequestObjects.Profile;

namespace TechA.Core.Interfaces.Service;

public interface IUserProfileService
{
    Task<UserProfile?> GetProfileAsync(Guid userId);
    Task<UserProfile?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}
