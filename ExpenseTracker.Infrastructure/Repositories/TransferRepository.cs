using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transfers;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public TransferRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transfer>> GetAllAsync(
        string userId)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Where(transfer =>
                transfer.UserId == userId)
            .Include(transfer =>
                transfer.FromAccount)
            .Include(transfer =>
                transfer.ToAccount)
            .OrderByDescending(transfer =>
                transfer.TransferDate)
            .ThenByDescending(transfer =>
                transfer.Id)
            .ToListAsync();
    }

    public async Task<Transfer?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Include(transfer =>
                transfer.FromAccount)
            .Include(transfer =>
                transfer.ToAccount)
            .FirstOrDefaultAsync(transfer =>
                transfer.Id == id &&
                transfer.UserId == userId);
    }

    public async Task<Transfer> CreateAsync(
    Transfer transfer)
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
                    _context.Transfers.Add(transfer);

                    // Save first to generate transfer.Id.
                    await _context.SaveChangesAsync();

                    // Create automatic financial-goal contributions.
                    await SynchronizeGoalContributionsAsync(
                        transfer);

                    await _context.SaveChangesAsync();

                    // Recalculate after contributions are saved.
                    await RecalculateAffectedGoalStatusesAsync(
                        transfer.UserId,
                        transfer.FromAccountId,
                        transfer.ToAccountId);

                    await _context.SaveChangesAsync();

                    await databaseTransaction.CommitAsync();

                    return transfer;
                }
                catch
                {
                    await databaseTransaction.RollbackAsync();
                    throw;
                }
            });
    }

    public async Task<bool> UpdateAsync(
    Transfer transfer)
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
                    var existing =
                        await _context.Transfers
                            .Include(item =>
                                item.GoalContributions)
                            .FirstOrDefaultAsync(item =>
                                item.Id == transfer.Id &&
                                item.UserId ==
                                    transfer.UserId);

                    if (existing == null)
                    {
                        await databaseTransaction
                            .RollbackAsync();

                        return false;
                    }

                    var oldFromAccountId =
                        existing.FromAccountId;

                    var oldToAccountId =
                        existing.ToAccountId;

                    var oldContributions =
                        existing.GoalContributions
                            .Where(contribution =>
                                contribution.TransferId ==
                                    existing.Id)
                            .ToList();

                    _context.GoalContributions.RemoveRange(
                        oldContributions);

                    existing.FromAccountId =
                        transfer.FromAccountId;

                    existing.ToAccountId =
                        transfer.ToAccountId;

                    existing.Amount =
                        transfer.Amount;

                    existing.TransferDate =
                        transfer.TransferDate;

                    existing.Notes =
                        transfer.Notes;

                    existing.UpdatedAt =
                        DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    await SynchronizeGoalContributionsAsync(
                        existing);

                    await _context.SaveChangesAsync();

                    var affectedAccountIds = new[]
                    {
                    oldFromAccountId,
                    oldToAccountId,
                    existing.FromAccountId,
                    existing.ToAccountId
                    }
                    .Distinct()
                    .ToArray();

                    await RecalculateAffectedGoalStatusesAsync(
                        existing.UserId,
                        affectedAccountIds);

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

    public async Task<bool> DeleteAsync(
    int id,
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
                    var transfer =
                        await _context.Transfers
                            .Include(item =>
                                item.GoalContributions)
                            .FirstOrDefaultAsync(item =>
                                item.Id == id &&
                                item.UserId == userId);

                    if (transfer == null)
                    {
                        await databaseTransaction
                            .RollbackAsync();

                        return false;
                    }

                    var affectedAccountIds = new[]
                    {
                    transfer.FromAccountId,
                    transfer.ToAccountId
                    };

                    _context.GoalContributions.RemoveRange(
                        transfer.GoalContributions);

                    _context.Transfers.Remove(transfer);

                    await _context.SaveChangesAsync();

                    await RecalculateAffectedGoalStatusesAsync(
                        userId,
                        affectedAccountIds);

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

    public async Task<PagedResult<Transfer>> GetPagedAsync(
    string userId,
    TransferQueryDto query)
    {
        var transfers = _context.Transfers
            .AsNoTracking()
            .Where(transfer =>
                transfer.UserId == userId)
            .Include(transfer =>
                transfer.FromAccount)
            .Include(transfer =>
                transfer.ToAccount)
            .AsQueryable();

        if (query.FromAccountId.HasValue)
        {
            transfers = transfers.Where(transfer =>
                transfer.FromAccountId ==
                query.FromAccountId.Value);
        }

        if (query.ToAccountId.HasValue)
        {
            transfers = transfers.Where(transfer =>
                transfer.ToAccountId ==
                query.ToAccountId.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDate =
                query.FromDate.Value.Date;

            transfers = transfers.Where(transfer =>
                transfer.TransferDate >= fromDate);
        }

        if (query.ToDate.HasValue)
        {
            var nextDay =
                query.ToDate.Value.Date.AddDays(1);

            transfers = transfers.Where(transfer =>
                transfer.TransferDate < nextDay);
        }

        if (query.MinAmount.HasValue)
        {
            transfers = transfers.Where(transfer =>
                transfer.Amount >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            transfers = transfers.Where(transfer =>
                transfer.Amount <= query.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            transfers = transfers.Where(transfer =>
                transfer.Notes.Contains(search) ||
                (transfer.FromAccount != null &&
                 transfer.FromAccount.Name.Contains(search)) ||
                (transfer.ToAccount != null &&
                 transfer.ToAccount.Name.Contains(search)));
        }

        transfers = ApplySorting(
            transfers,
            query.SortBy,
            query.SortDirection);

        var totalRecords =
            await transfers.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)query.PageSize);

        var items =
            await transfers
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

        return new PagedResult<Transfer>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }
    private static IQueryable<Transfer> ApplySorting(
    IQueryable<Transfer> query,
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
            "amount" => descending
                ? query
                    .OrderByDescending(transfer =>
                        transfer.Amount)
                    .ThenByDescending(transfer =>
                        transfer.Id)
                : query
                    .OrderBy(transfer =>
                        transfer.Amount)
                    .ThenBy(transfer =>
                        transfer.Id),

            "fromaccount" => descending
                ? query
                    .OrderByDescending(transfer =>
                        transfer.FromAccount != null
                            ? transfer.FromAccount.Name
                            : string.Empty)
                    .ThenByDescending(transfer =>
                        transfer.Id)
                : query
                    .OrderBy(transfer =>
                        transfer.FromAccount != null
                            ? transfer.FromAccount.Name
                            : string.Empty)
                    .ThenBy(transfer =>
                        transfer.Id),

            "toaccount" => descending
                ? query
                    .OrderByDescending(transfer =>
                        transfer.ToAccount != null
                            ? transfer.ToAccount.Name
                            : string.Empty)
                    .ThenByDescending(transfer =>
                        transfer.Id)
                : query
                    .OrderBy(transfer =>
                        transfer.ToAccount != null
                            ? transfer.ToAccount.Name
                            : string.Empty)
                    .ThenBy(transfer =>
                        transfer.Id),

            _ => descending
                ? query
                    .OrderByDescending(transfer =>
                        transfer.TransferDate)
                    .ThenByDescending(transfer =>
                        transfer.Id)
                : query
                    .OrderBy(transfer =>
                        transfer.TransferDate)
                    .ThenBy(transfer =>
                        transfer.Id)
        };
    }
    private async Task SynchronizeGoalContributionsAsync(
    Transfer transfer)
    {
        var accountIds = new[]
        {
        transfer.FromAccountId,
        transfer.ToAccountId
    };

        var linkedGoals =
            await _context.FinancialGoals
                .Where(goal =>
                    goal.UserId == transfer.UserId &&
                    goal.IsActive &&
                    goal.AccountId.HasValue &&
                    accountIds.Contains(
                        goal.AccountId.Value))
                .ToListAsync();

        foreach (var goal in linkedGoals)
        {
            if (goal.Status ==
                    FinancialGoalStatus.Cancelled ||
                goal.Status ==
                    FinancialGoalStatus.Paused)
            {
                continue;
            }

            GoalContributionType contributionType;
            decimal amount;
            string action;

            if (goal.AccountId ==
                transfer.ToAccountId)
            {
                contributionType =
                    GoalContributionType.TransferDeposit;

                amount =
                    transfer.Amount;

                action =
                    "deposit";
            }
            else if (goal.AccountId ==
                     transfer.FromAccountId)
            {
                contributionType =
                    GoalContributionType.TransferWithdrawal;

                amount =
                    -transfer.Amount;

                action =
                    "withdrawal";
            }
            else
            {
                continue;
            }

            var alreadyExists =
                await _context.GoalContributions
                    .AnyAsync(contribution =>
                        contribution.FinancialGoalId ==
                            goal.Id &&
                        contribution.TransferId ==
                            transfer.Id &&
                        contribution.ContributionType ==
                            contributionType);

            if (alreadyExists)
                continue;

            var contribution =
                new GoalContribution
                {
                    UserId =
                        transfer.UserId,

                    FinancialGoalId =
                        goal.Id,

                    AccountId =
                        goal.AccountId,

                    Amount =
                        amount,

                    ContributionDate =
                        transfer.TransferDate,

                    Notes =
                        $"Automatic {action} from transfer " +
                        $"#{transfer.Id}: {transfer.Notes}",

                    ContributionType =
                        contributionType,

                    TransferId =
                        transfer.Id,

                    IsActive =
                        true
                };

            _context.GoalContributions.Add(
                contribution);
        }
    }
    private async Task RecalculateAffectedGoalStatusesAsync(
    string userId,
    params int[] accountIds)
    {
        var distinctAccountIds =
            accountIds
                .Distinct()
                .ToArray();

        var goals =
            await _context.FinancialGoals
                .Include(goal =>
                    goal.Contributions)
                .Where(goal =>
                    goal.UserId == userId &&
                    goal.AccountId.HasValue &&
                    distinctAccountIds.Contains(
                        goal.AccountId.Value))
                .ToListAsync();

        foreach (var goal in goals)
        {
            if (goal.Status ==
                    FinancialGoalStatus.Cancelled ||
                goal.Status ==
                    FinancialGoalStatus.Paused)
            {
                continue;
            }

            var savedAmount =
                goal.StartingAmount +
                goal.Contributions
                    .Where(contribution =>
                        contribution.IsActive)
                    .Sum(contribution =>
                        contribution.Amount);

            var expectedStatus =
                savedAmount >= goal.TargetAmount
                    ? FinancialGoalStatus.Completed
                    : FinancialGoalStatus.Active;

            if (goal.Status == expectedStatus)
                continue;

            goal.Status =
                expectedStatus;

            goal.UpdatedAt =
                DateTime.UtcNow;
        }
    }
}