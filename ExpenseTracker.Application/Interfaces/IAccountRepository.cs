using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(string userId);

    Task<Account?> GetByIdAsync(int id,string userId);

    Task<Account?> GetByNameAsync(string name,string userId);

    Task<Account> CreateAsync(Account account);

    Task<bool> UpdateAsync(Account account);

    Task<bool> DeleteAsync(int id,string userId);

    Task<bool> HasTransactionsAsync(int id,string userId);
}