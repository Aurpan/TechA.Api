using Microsoft.EntityFrameworkCore;
using TechA.Core.Entities;
using TechA.Core.Interfaces.Persistence;
using TechA.DataManagement.DbContext;

namespace TechA.DataManagement.Persistence.Repositories;

public class ExpenseTransactionRepository : IExpenseTransactionRepository
{
    private readonly TechADbContext _dbContext;

    public ExpenseTransactionRepository(TechADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ExpenseTransaction>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.ExpenseTransactions
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();
    }

    public async Task<ExpenseTransaction?> GetByIdAsync(Guid id)
    {
        return await _dbContext.ExpenseTransactions.FindAsync(id);
    }

    public async Task AddAsync(ExpenseTransaction transaction)
    {
        _dbContext.ExpenseTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(ExpenseTransaction transaction)
    {
        _dbContext.ExpenseTransactions.Update(transaction);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbContext.ExpenseTransactions.FindAsync(id);
        if (entity is not null)
        {
            _dbContext.ExpenseTransactions.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
