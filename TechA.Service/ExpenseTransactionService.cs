using TechA.Core.Entities;
using TechA.Core.Interfaces.Domain;
using TechA.Core.Interfaces.Persistence;

namespace TechA.Services;

public class ExpenseTransactionService : IExpenseTransactionService
{
    private readonly IExpenseTransactionRepository _repository;

    public ExpenseTransactionService(IExpenseTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ExpenseTransaction>> GetAllByUserIdAsync(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<ExpenseTransaction?> GetByIdAsync(Guid id, Guid userId)
    {
        var transaction = await _repository.GetByIdAsync(id);
        
        if (transaction is null || transaction.UserId != userId)
            return null;

        return transaction;
    }

    public async Task<ExpenseTransaction> CreateAsync(Guid userId, string productName, string category, decimal amount, ExpenseType type, DateTime? dateTime)
    {
        var transaction = new ExpenseTransaction
        {
            UserId = userId,
            ProductName = productName,
            Category = category,
            Amount = amount,
            Type = type,
            DateTime = dateTime ?? DateTime.UtcNow
        };

        await _repository.AddAsync(transaction);
        return transaction;
    }

    public async Task<ExpenseTransaction?> UpdateAsync(Guid id, Guid userId, string productName, string category, decimal amount, ExpenseType type, DateTime? dateTime)
    {
        var transaction = await _repository.GetByIdAsync(id);

        if (transaction is null || transaction.UserId != userId)
            return null;

        transaction.ProductName = productName;
        transaction.Category = category;
        transaction.Amount = amount;
        transaction.Type = type;
        transaction.DateTime = dateTime ?? transaction.DateTime;

        await _repository.UpdateAsync(transaction);
        return transaction;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var transaction = await _repository.GetByIdAsync(id);

        if (transaction is null || transaction.UserId != userId)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }
}
