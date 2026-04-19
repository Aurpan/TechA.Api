using TechA.Core.Entities;

namespace TechA.Core.Interfaces.Domain;

public interface IExpenseTransactionService
{
    Task<List<ExpenseTransaction>> GetAllByUserIdAsync(Guid userId);
    Task<ExpenseTransaction?> GetByIdAsync(Guid id, Guid userId);
    Task<ExpenseTransaction> CreateAsync(Guid userId, string productName, string category, decimal amount, ExpenseType type, DateTime? dateTime);
    Task<ExpenseTransaction?> UpdateAsync(Guid id, Guid userId, string productName, string category, decimal amount, ExpenseType type, DateTime? dateTime);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
