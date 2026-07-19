using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.DTOs.Transfers;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync(string userId);

    Task<Transaction?> GetByIdAsync(int id, string userId);

    Task<Transaction> CreateAsync(Transaction transaction);

    Task<bool> UpdateAsync(Transaction transaction);

    Task<bool> DeleteAsync(int id, string userId);

    Task<IEnumerable<Transaction>> GetAllForDashboardAsync(string userId);
    Task<PagedResult<Transaction>> GetPagedAsync(string userId, TransactionQueryDto query);

}