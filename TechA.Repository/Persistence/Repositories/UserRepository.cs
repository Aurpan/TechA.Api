using Microsoft.EntityFrameworkCore;
using TechA.Core.Entities;
using TechA.Core.Interfaces.Persistence;
using TechA.DataManagement.DbContext;

namespace TechA.DataManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TechADbContext _dbContext;

    public UserRepository(TechADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        try
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
