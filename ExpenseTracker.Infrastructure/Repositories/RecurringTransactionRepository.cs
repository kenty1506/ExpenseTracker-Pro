using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class RecurringTransactionRepository
    : IRecurringTransactionRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public RecurringTransactionRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecurringTransaction>>
        GetAllAsync(string userId)
    {
        return await _context.RecurringTransactions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .Include(x => x.Account)
            .OrderBy(x => x.NextRunDate)
            .ToListAsync();
    }

    public async Task<RecurringTransaction?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.RecurringTransactions
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);
    }

    public async Task<RecurringTransaction> CreateAsync(
        RecurringTransaction recurringTransaction)
    {
        _context.RecurringTransactions.Add(
            recurringTransaction);

        await _context.SaveChangesAsync();

        return recurringTransaction;
    }

    public async Task<bool> UpdateAsync(
        RecurringTransaction recurringTransaction)
    {
        var existing =
            await _context.RecurringTransactions
                .FirstOrDefaultAsync(x =>
                    x.Id == recurringTransaction.Id &&
                    x.UserId ==
                        recurringTransaction.UserId);

        if (existing == null)
            return false;

        existing.Type = recurringTransaction.Type;
        existing.CategoryId =
            recurringTransaction.CategoryId;
        existing.AccountId =
            recurringTransaction.AccountId;
        existing.Amount =
            recurringTransaction.Amount;
        existing.Notes =
            recurringTransaction.Notes;
        existing.DayOfMonth =
            recurringTransaction.DayOfMonth;
        existing.EndDate =
            recurringTransaction.EndDate;
        existing.NextRunDate =
            recurringTransaction.NextRunDate;
        existing.LastRunDate =
            recurringTransaction.LastRunDate;
        existing.IsActive =
            recurringTransaction.IsActive;
        existing.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var recurringTransaction =
            await _context.RecurringTransactions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (recurringTransaction == null)
            return false;

        _context.RecurringTransactions.Remove(
            recurringTransaction);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RecurringTransaction>>
        GetDueAsync(
            string userId,
            DateTime throughDate)
    {
        var date = throughDate.Date;

        return await _context.RecurringTransactions
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Account)
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                x.NextRunDate <= date &&
                (!x.EndDate.HasValue ||
                 x.NextRunDate <=
                    x.EndDate.Value.Date))
            .OrderBy(x => x.NextRunDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecurringTransaction>>
        GetUpcomingAsync(
            string userId,
            DateTime fromDate,
            DateTime throughDate)
    {
        var start = fromDate.Date;
        var end = throughDate.Date;

        return await _context.RecurringTransactions
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Account)
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                x.NextRunDate >= start &&
                x.NextRunDate <= end &&
                (!x.EndDate.HasValue ||
                 x.NextRunDate <=
                    x.EndDate.Value.Date))
            .OrderBy(x => x.NextRunDate)
            .ThenBy(x =>
                x.Category != null
                    ? x.Category.Name
                    : string.Empty)
            .ToListAsync();
    }

    public async Task<Transaction?>
        GenerateOccurrenceAsync(
            int recurringTransactionId,
            string userId,
            DateTime occurrenceDate,
            DateTime nextRunDate)
    {
        var occurrence = occurrenceDate.Date;
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
                    var recurring =
                        await _context.RecurringTransactions
                            .FirstOrDefaultAsync(x =>
                                x.Id == recurringTransactionId &&
                                x.UserId == userId);

                    if (recurring == null ||
                        !recurring.IsActive ||
                        recurring.NextRunDate.Date != occurrence)
                    {
                        await databaseTransaction.RollbackAsync();
                        return null;
                    }

                    var alreadyGenerated =
                        await _context.Transactions.AnyAsync(x =>
                            x.UserId == userId &&
                            x.RecurringTransactionId ==
                                recurringTransactionId &&
                            x.Date == occurrence);

                    if (alreadyGenerated)
                    {
                        recurring.LastRunDate = occurrence;
                        recurring.NextRunDate = nextRunDate;
                        recurring.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                        await databaseTransaction.CommitAsync();

                        return null;
                    }

                    var transaction = new Transaction
                    {
                        UserId = userId,
                        Type = recurring.Type,
                        CategoryId = recurring.CategoryId,
                        AccountId = recurring.AccountId,
                        Amount = recurring.Amount,
                        Notes = recurring.Notes,
                        Date = occurrence,
                        RecurringTransactionId = recurring.Id
                    };

                    _context.Transactions.Add(transaction);

                    recurring.LastRunDate = occurrence;
                    recurring.NextRunDate = nextRunDate;
                    recurring.UpdatedAt = DateTime.UtcNow;

                    if (recurring.EndDate.HasValue &&
                        nextRunDate.Date >
                            recurring.EndDate.Value.Date)
                    {
                        recurring.IsActive = false;
                    }

                    await _context.SaveChangesAsync();
                    await databaseTransaction.CommitAsync();

                    return transaction;
                }
                catch
                {
                    await databaseTransaction.RollbackAsync();
                    throw;
                }
            });
    }
}
