using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetAllAsync(string userId);

    Task<Budget?> GetByIdAsync(int id, string userId);

    Task<Budget> CreateAsync(Budget budget);

    Task<bool> UpdateAsync(Budget budget);

    Task<bool> DeleteAsync(int id, string userId);

    Task<IEnumerable<Budget>> GetByMonthAsync(string userId,int year,int month);

}