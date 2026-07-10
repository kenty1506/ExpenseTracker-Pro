using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync(string userId);

    Task<Transaction?> GetByIdAsync(int id, string userId);

    Task<Transaction> CreateAsync(Transaction transaction);

    Task<bool> UpdateAsync(Transaction transaction);

    Task<bool> DeleteAsync(int id, string userId);

    Task<IEnumerable<Transaction>> GetAllForDashboardAsync(string userId);
}