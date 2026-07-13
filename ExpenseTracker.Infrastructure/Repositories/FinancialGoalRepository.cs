using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class FinancialGoalRepository : IFinancialGoalRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public FinancialGoalRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FinancialGoal>> GetAllAsync(
        string userId)
    {
        return await _context.FinancialGoals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId)
            .Include(goal => goal.Account)
            .Include(goal => goal.Contributions)
                .ThenInclude(contribution => contribution.Account)
            .OrderBy(goal => goal.Status)
            .ThenBy(goal => goal.TargetDate)
            .ThenBy(goal => goal.Name)
            .ToListAsync();
    }

    public async Task<FinancialGoal?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.FinancialGoals
            .AsNoTracking()
            .Include(goal => goal.Account)
            .Include(goal => goal.Contributions)
                .ThenInclude(contribution => contribution.Account)
            .FirstOrDefaultAsync(goal =>
                goal.Id == id &&
                goal.UserId == userId);
    }

    public async Task<FinancialGoal?> GetByNameAsync(
        string name,
        string userId)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.FinancialGoals
            .AsNoTracking()
            .FirstOrDefaultAsync(goal =>
                goal.UserId == userId &&
                goal.Name.ToLower() == normalizedName);
    }

    public async Task<FinancialGoal> CreateAsync(
        FinancialGoal financialGoal)
    {
        _context.FinancialGoals.Add(financialGoal);

        await _context.SaveChangesAsync();

        return financialGoal;
    }

    public async Task<bool> UpdateAsync(
        FinancialGoal financialGoal)
    {
        var existing = await _context.FinancialGoals
            .FirstOrDefaultAsync(goal =>
                goal.Id == financialGoal.Id &&
                goal.UserId == financialGoal.UserId);

        if (existing == null)
            return false;

        existing.Name = financialGoal.Name;
        existing.TargetAmount = financialGoal.TargetAmount;
        existing.StartingAmount = financialGoal.StartingAmount;
        existing.TargetDate = financialGoal.TargetDate;
        existing.Status = financialGoal.Status;
        existing.AccountId = financialGoal.AccountId;
        existing.Color = financialGoal.Color;
        existing.Icon = financialGoal.Icon;
        existing.Notes = financialGoal.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var goal = await _context.FinancialGoals
            .FirstOrDefaultAsync(goal =>
                goal.Id == id &&
                goal.UserId == userId);

        if (goal == null)
            return false;

        _context.FinancialGoals.Remove(goal);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<GoalContribution> AddContributionAsync(
        GoalContribution contribution)
    {
        _context.GoalContributions.Add(contribution);

        await _context.SaveChangesAsync();

        return contribution;
    }

    public async Task<bool> DeleteContributionAsync(
        int contributionId,
        int financialGoalId,
        string userId)
    {
        var contribution =
            await _context.GoalContributions
                .FirstOrDefaultAsync(item =>
                    item.Id == contributionId &&
                    item.FinancialGoalId == financialGoalId &&
                    item.UserId == userId);

        if (contribution == null)
            return false;

        _context.GoalContributions.Remove(contribution);

        await _context.SaveChangesAsync();

        return true;
    }
}