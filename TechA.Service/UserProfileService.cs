using TechA.Core.Interfaces.Service;
using TechA.Core.RequestObjects.Profile;
using TechA.Repository.Entities;
using TechA.Repository.Interfaces;
using UserProfileDto = TechA.Core.DTOs.UserProfile;

namespace TechA.Service;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return null;

        return MapToDto(user);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return null;

        user.Profile ??= new UserProfile { UserId = user.Id };

        var profile = user.Profile;

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            profile.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            profile.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
            profile.ProfilePictureUrl = request.ProfilePictureUrl;

        if (!string.IsNullOrWhiteSpace(request.ContactNumber))
            profile.ContactNumber = request.ContactNumber;

        if (request.DateOfBirth.HasValue)
            profile.DateOfBirth = request.DateOfBirth;

        if (!string.IsNullOrWhiteSpace(request.Bio))
            profile.Bio = request.Bio;

        if (!string.IsNullOrWhiteSpace(request.Address))
            profile.Address = request.Address;

        if (!string.IsNullOrWhiteSpace(request.City))
            profile.City = request.City;

        if (!string.IsNullOrWhiteSpace(request.State))
            profile.State = request.State;

        if (!string.IsNullOrWhiteSpace(request.Country))
            profile.Country = request.Country;

        if (!string.IsNullOrWhiteSpace(request.ZipCode))
            profile.ZipCode = request.ZipCode;

        var updated = await _userRepository.UpdateAsync(user);

        if (!updated)
            return null;

        return MapToDto(user);
    }

    private static UserProfileDto MapToDto(User user)
    {
        var profile = user.Profile;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = profile?.FirstName,
            LastName = profile?.LastName,
            ProfilePictureUrl = profile?.ProfilePictureUrl,
            ContactNumber = profile?.ContactNumber,
            DateOfBirth = profile?.DateOfBirth,
            Bio = profile?.Bio,
            Address = profile?.Address,
            City = profile?.City,
            State = profile?.State,
            Country = profile?.Country,
            ZipCode = profile?.ZipCode,
            AuthProvider = user.AuthProvider.ToString(),
            CreatedAt = user.CreatedAt
        };
    }
}
