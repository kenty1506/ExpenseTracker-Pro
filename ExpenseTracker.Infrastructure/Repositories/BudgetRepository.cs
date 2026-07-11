using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public BudgetRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Budget>> GetAllAsync(string userId)
    {
        return await _context.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenBy(x => x.Category!.Name)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id,string userId)
    {
        return await _context.Budgets
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<bool> UpdateAsync(Budget budget)
    {
        var existingBudget =
            await _context.Budgets
                .FirstOrDefaultAsync(x =>
                    x.Id == budget.Id &&
                    x.UserId == budget.UserId);

        if (existingBudget == null)
            return false;
        existingBudget.Amount = budget.Amount;
        existingBudget.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        var budget = await _context.Budgets
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (budget == null)
            return false;

        _context.Budgets.Remove(budget);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<IEnumerable<Budget>> GetByMonthAsync(string userId,int year,int month)
    {
        return await _context.Budgets
            .AsNoTracking()
            .Where(budget =>
                budget.UserId == userId &&
                budget.Year == year &&
                budget.Month == month)
            .Include(budget => budget.Category)
            .OrderBy(budget => budget.Category!.Name)
            .ToListAsync();
    }
}