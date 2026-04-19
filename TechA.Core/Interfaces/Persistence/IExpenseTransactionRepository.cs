using TechA.Core.Entities;

namespace TechA.Core.Interfaces.Persistence;

public interface IExpenseTransactionRepository
{
    Task<List<ExpenseTransaction>> GetByUserIdAsync(Guid userId);
    Task<ExpenseTransaction?> GetByIdAsync(Guid id);
    Task AddAsync(ExpenseTransaction transaction);
    Task UpdateAsync(ExpenseTransaction transaction);
    Task DeleteAsync(Guid id);
}
