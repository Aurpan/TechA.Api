using TechA.Repository.Entities;

namespace TechA.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(User user);
}
