using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public CategoryRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(
        string userId)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);
    }

    public async Task<Category?> GetByNameAsync(
        string name,
        string userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Name.ToLower() == name.ToLower());
    }

    public async Task<Category> CreateAsync(
        Category category)
    {
        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> UpdateAsync(
        Category category)
    {
        var existingCategory =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == category.Id &&
                    x.UserId == category.UserId);

        if (existingCategory == null)
            return false;

        existingCategory.Name = category.Name;
        existingCategory.Color = category.Color;
        existingCategory.Icon = category.Icon;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var category =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (category == null)
            return false;

        var isUsed = await _context.Transactions
            .AnyAsync(x =>
                x.CategoryId == id &&
                x.UserId == userId);

        if (isUsed)
        {
            throw new ArgumentException(
                "This category cannot be deleted because it is used by one or more transactions.");
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return true;
    }
}