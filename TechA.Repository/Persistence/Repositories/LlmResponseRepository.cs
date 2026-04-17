using TechA.Core.Entities;
using TechA.Core.Interfaces.Persistence;
using TechA.DataManagement.DbContext;

namespace TechA.DataManagement.Persistence.Repositories;

public class LlmResponseRepository : ILlmResponseRepository
{
    private readonly TechADbContext _dbContext;

    public LlmResponseRepository(TechADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LlmResponse response)
    {
        _dbContext.LlmResponses.Add(response);
        await _dbContext.SaveChangesAsync();
    }
}
