using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(string userId);

    Task<Category?> GetByIdAsync(int id,string userId);

    Task<Category?> GetByNameAsync(string name,string userId);

    Task<Category> CreateAsync(Category category);

    Task<bool> UpdateAsync(Category category);

    Task<bool> DeleteAsync(int id,string userId);
}