using TechA.Core.DTOs;
using TechA.Core.Requests.Profile;

namespace TechA.Core.Interfaces.Domain;

public interface IUserProfileService
{
    Task<UserProfile?> GetProfileAsync(Guid userId);
    Task<UserProfile?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}
