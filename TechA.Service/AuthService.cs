using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TechA.Core.DTOs;
using TechA.Core.Enums;
using TechA.Core.Interfaces.Service;
using TechA.Core.RequestObjects.Auth;
using TechA.Core.ResponseObjects;
using TechA.Repository.Data;
using TechA.Repository.Entities;

namespace TechA.Service;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        
        if (emailExists)
        {
            return new AuthResponse { Success = false, Message = "Email is already registered." };
        }

        var user = new Repository.Entities.User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            AuthProvider = AuthProvider.Email
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse { Success = true, Token = GenerateJwtToken(user), User = MapToDto(user) };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResponse { Success = false, Message = "Invalid credentials." };

        return new AuthResponse { Success = true, Token = GenerateJwtToken(user), User = MapToDto(user) };
    }

    public async Task<AuthResponse> GoogleSignInAsync(GoogleAuthRequest request)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_configuration["Google:ClientId"]]
            };

            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
        }
        catch (InvalidJwtException)
        {
            return new AuthResponse { Success = false, Message = "Invalid Google token." };
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user is null)
        {
            user = new Repository.Entities.User
            {
                Email = payload.Email,
                DisplayName = payload.Name,
                AuthProvider = AuthProvider.Google,
                Profile = new Repository.Entities.UserProfile
                {
                    ProfilePictureUrl = payload.Picture
                }
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        return new AuthResponse { Success = true, Token = GenerateJwtToken(user), User = MapToDto(user) };
    }

    private static Core.DTOs.User MapToDto(Repository.Entities.User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        AuthProvider = user.AuthProvider.ToString(),
        CreatedAt = user.CreatedAt
    };

    private string GenerateJwtToken(Repository.Entities.User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSection["ExpiryInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
