using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
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

    public async Task<GoalContribution?>
    GetContributionByIdAsync(
        int contributionId,
        int financialGoalId,
        string userId)
    {
        return await _context.GoalContributions
            .AsNoTracking()
            .FirstOrDefaultAsync(contribution =>
                contribution.Id ==
                    contributionId &&
                contribution.FinancialGoalId ==
                    financialGoalId &&
                contribution.UserId ==
                    userId);
    }
    public async Task<bool> DeleteContributionAsync(int contributionId,
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

    public async Task<PagedResult<FinancialGoal>> GetPagedAsync(
    string userId,
    FinancialGoalQueryDto query)
    {
        var goals = _context.FinancialGoals
            .AsNoTracking()
            .Where(goal =>
                goal.UserId == userId)
            .Include(goal =>
                goal.Account)
            .Include(goal =>
                goal.Contributions)
                    .ThenInclude(contribution =>
                        contribution.Account)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            goals = goals.Where(goal =>
                goal.Status == query.Status.Value);
        }

        if (query.AccountId.HasValue)
        {
            goals = goals.Where(goal =>
                goal.AccountId == query.AccountId.Value);
        }

        if (query.TargetDateFrom.HasValue)
        {
            var fromDate =
                query.TargetDateFrom.Value.Date;

            goals = goals.Where(goal =>
                goal.TargetDate.HasValue &&
                goal.TargetDate.Value >= fromDate);
        }

        if (query.TargetDateTo.HasValue)
        {
            var nextDay =
                query.TargetDateTo.Value.Date.AddDays(1);

            goals = goals.Where(goal =>
                goal.TargetDate.HasValue &&
                goal.TargetDate.Value < nextDay);
        }

        if (query.MinTargetAmount.HasValue)
        {
            goals = goals.Where(goal =>
                goal.TargetAmount >=
                query.MinTargetAmount.Value);
        }

        if (query.MaxTargetAmount.HasValue)
        {
            goals = goals.Where(goal =>
                goal.TargetAmount <=
                query.MaxTargetAmount.Value);
        }

        if (query.MinSavedAmount.HasValue)
        {
            var minimumSaved =
                query.MinSavedAmount.Value;

            goals = goals.Where(goal =>
                goal.StartingAmount
                + (
                    goal.Contributions
                        .Sum(contribution =>
                            (decimal?)contribution.Amount)
                    ?? 0m
                )
                >= minimumSaved);
        }

        if (query.MaxSavedAmount.HasValue)
        {
            var maximumSaved =
                query.MaxSavedAmount.Value;

            goals = goals.Where(goal =>
                goal.StartingAmount
                + (
                    goal.Contributions
                        .Sum(contribution =>
                            (decimal?)contribution.Amount)
                    ?? 0m
                )
                <= maximumSaved);
        }

        if (query.IsCompleted.HasValue)
        {
            if (query.IsCompleted.Value)
            {
                goals = goals.Where(goal =>
                    goal.Status ==
                        FinancialGoalStatus.Completed ||
                    goal.StartingAmount
                    + (
                        goal.Contributions
                            .Sum(contribution =>
                                (decimal?)contribution.Amount)
                        ?? 0m
                    )
                    >= goal.TargetAmount);
            }
            else
            {
                goals = goals.Where(goal =>
                    goal.Status !=
                        FinancialGoalStatus.Completed &&
                    goal.StartingAmount
                    + (
                        goal.Contributions
                            .Sum(contribution =>
                                (decimal?)contribution.Amount)
                        ?? 0m
                    )
                    < goal.TargetAmount);
            }
        }

        if (query.IsOverdue.HasValue)
        {
            var today = DateTime.UtcNow.Date;

            if (query.IsOverdue.Value)
            {
                goals = goals.Where(goal =>
                    goal.TargetDate.HasValue &&
                    goal.TargetDate.Value < today &&
                    goal.Status !=
                        FinancialGoalStatus.Completed &&
                    goal.StartingAmount
                    + (
                        goal.Contributions
                            .Sum(contribution =>
                                (decimal?)contribution.Amount)
                        ?? 0m
                    )
                    < goal.TargetAmount);
            }
            else
            {
                goals = goals.Where(goal =>
                    !goal.TargetDate.HasValue ||
                    goal.TargetDate.Value >= today ||
                    goal.Status ==
                        FinancialGoalStatus.Completed ||
                    goal.StartingAmount
                    + (
                        goal.Contributions
                            .Sum(contribution =>
                                (decimal?)contribution.Amount)
                        ?? 0m
                    )
                    >= goal.TargetAmount);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            goals = goals.Where(goal =>
                goal.Name.Contains(search) ||
                goal.Notes.Contains(search) ||
                (goal.Account != null &&
                 goal.Account.Name.Contains(search)));
        }

        goals = ApplySorting(
            goals,
            query.SortBy,
            query.SortDirection);

        var totalRecords =
            await goals.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)query.PageSize);

        var items =
            await goals
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

        return new PagedResult<FinancialGoal>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }
    private static IQueryable<FinancialGoal> ApplySorting(
    IQueryable<FinancialGoal> query,
    string? sortBy,
    string? sortDirection)
    {
        var descending =
            string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "targetamount" => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.TargetAmount)
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.TargetAmount)
                    .ThenBy(goal =>
                        goal.Id),

            "savedamount" => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.StartingAmount
                        + (
                            goal.Contributions
                                .Sum(contribution =>
                                    (decimal?)contribution.Amount)
                            ?? 0m
                        ))
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.StartingAmount
                        + (
                            goal.Contributions
                                .Sum(contribution =>
                                    (decimal?)contribution.Amount)
                            ?? 0m
                        ))
                    .ThenBy(goal =>
                        goal.Id),

            "percentage" => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.TargetAmount <= 0
                            ? 0m
                            : (
                                goal.StartingAmount
                                + (
                                    goal.Contributions
                                        .Sum(contribution =>
                                            (decimal?)contribution.Amount)
                                    ?? 0m
                                )
                            ) / goal.TargetAmount)
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.TargetAmount <= 0
                            ? 0m
                            : (
                                goal.StartingAmount
                                + (
                                    goal.Contributions
                                        .Sum(contribution =>
                                            (decimal?)contribution.Amount)
                                    ?? 0m
                                )
                            ) / goal.TargetAmount)
                    .ThenBy(goal =>
                        goal.Id),

            "targetdate" or "date" => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.TargetDate)
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.TargetDate)
                    .ThenBy(goal =>
                        goal.Id),

            "status" => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.Status)
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.Status)
                    .ThenBy(goal =>
                        goal.Id),

            _ => descending
                ? query
                    .OrderByDescending(goal =>
                        goal.Name)
                    .ThenByDescending(goal =>
                        goal.Id)
                : query
                    .OrderBy(goal =>
                        goal.Name)
                    .ThenBy(goal =>
                        goal.Id)
        };
    }

    public async Task<GoalContribution>
    AddContributionWithTransactionAsync(
        GoalContribution contribution,
        Transaction transaction)
    {
        var executionStrategy =
            _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var databaseTransaction =
                    await _context.Database
                        .BeginTransactionAsync();

                try
                {
                    _context.Transactions.Add(transaction);

                    await _context.SaveChangesAsync();

                    contribution.TransactionId =
                        transaction.Id;

                    _context.GoalContributions.Add(
                        contribution);

                    await _context.SaveChangesAsync();

                    await databaseTransaction.CommitAsync();

                    return contribution;
                }
                catch
                {
                    await databaseTransaction.RollbackAsync();
                    throw;
                }
            });
    }
    public async Task<bool>
    DeleteContributionWithTransactionAsync(
        int contributionId,
        int financialGoalId,
        string userId)
    {
        var executionStrategy =
            _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var databaseTransaction =
                    await _context.Database
                        .BeginTransactionAsync();

                try
                {
                    var contribution =
                        await _context.GoalContributions
                            .Include(item =>
                                item.Transaction)
                            .FirstOrDefaultAsync(item =>
                                item.Id == contributionId &&
                                item.FinancialGoalId ==
                                    financialGoalId &&
                                item.UserId == userId);

                    if (contribution == null)
                    {
                        await databaseTransaction
                            .RollbackAsync();

                        return false;
                    }

                    if (contribution.TransferId.HasValue)
                    {
                        throw new ArgumentException(
                            "Transfer-generated contributions cannot be " +
                            "deleted directly. Delete or update the transfer.");
                    }

                    var transaction =
                        contribution.Transaction;

                    _context.GoalContributions.Remove(
                        contribution);

                    if (transaction != null)
                    {
                        _context.Transactions.Remove(
                            transaction);
                    }

                    await _context.SaveChangesAsync();

                    await databaseTransaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await databaseTransaction.RollbackAsync();
                    throw;
                }
            });
    }
}