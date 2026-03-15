using TechA.Core.Entities;

namespace TechA.Core.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(User user);
}
