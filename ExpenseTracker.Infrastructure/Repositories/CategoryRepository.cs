using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public CategoryRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        var existingCategory =
            await _context.Categories.FindAsync(category.Id);

        if (existingCategory == null)
            return false;

        existingCategory.Name = category.Name;
        existingCategory.Color = category.Color;
        existingCategory.Icon = category.Icon;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}