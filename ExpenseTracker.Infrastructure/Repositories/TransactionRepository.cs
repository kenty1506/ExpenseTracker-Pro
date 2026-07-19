using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transactions;

namespace ExpenseTracker.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public TransactionRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync(
        string userId)
    {
        return await _context.Transactions
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .Include(x => x.Account)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.Transactions
            .Include(x => x.Category)
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);
    }


    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (transaction == null)
            return false;

        _context.Transactions.Remove(transaction);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        var existingTransaction =
            await _context.Transactions.FindAsync(transaction.Id);

        if (existingTransaction == null)
            return false;

        existingTransaction.Type = transaction.Type;
        existingTransaction.CategoryId = transaction.CategoryId;
        existingTransaction.Amount = transaction.Amount;
        existingTransaction.Notes = transaction.Notes;
        existingTransaction.Date = transaction.Date;
        existingTransaction.UpdatedAt = DateTime.UtcNow;
        existingTransaction.AccountId = transaction.AccountId;

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<IEnumerable<Transaction>> GetAllForDashboardAsync(string userId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }
    public async Task<PagedResult<Transaction>> GetPagedAsync(
    string userId,
    TransactionQueryDto query)
    {
        var transactions = _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId)
            .Include(transaction =>
                transaction.Category)
            .Include(transaction =>
                transaction.Account)
            .AsQueryable();

        if (query.Type.HasValue)
        {
            transactions = transactions.Where(transaction =>
                transaction.Type == query.Type.Value);
        }

        if (query.CategoryId.HasValue)
        {
            transactions = transactions.Where(transaction =>
                transaction.CategoryId == query.CategoryId.Value);
        }

        if (query.AccountId.HasValue)
        {
            transactions = transactions.Where(transaction =>
                transaction.AccountId == query.AccountId.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDate = query.FromDate.Value.Date;

            transactions = transactions.Where(transaction =>
                transaction.Date >= fromDate);
        }

        if (query.ToDate.HasValue)
        {
            var nextDay =
                query.ToDate.Value.Date.AddDays(1);

            transactions = transactions.Where(transaction =>
                transaction.Date < nextDay);
        }

        if (query.MinAmount.HasValue)
        {
            transactions = transactions.Where(transaction =>
                transaction.Amount >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            transactions = transactions.Where(transaction =>
                transaction.Amount <= query.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            transactions = transactions.Where(transaction =>
                transaction.Notes.Contains(search) ||
                (transaction.Category != null &&
                 transaction.Category.Name.Contains(search)) ||
                (transaction.Account != null &&
                 transaction.Account.Name.Contains(search)));
        }

        transactions = ApplySorting(
            transactions,
            query.SortBy,
            query.SortDirection);

        var totalRecords =
            await transactions.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)query.PageSize);

        var items =
            await transactions
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

        return new PagedResult<Transaction>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }
    private static IQueryable<Transaction> ApplySorting(
    IQueryable<Transaction> query,
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
                    .OrderByDescending(transaction =>
                        transaction.Amount)
                    .ThenByDescending(transaction =>
                        transaction.Id)
                : query
                    .OrderBy(transaction =>
                        transaction.Amount)
                    .ThenBy(transaction =>
                        transaction.Id),

            "category" => descending
                ? query
                    .OrderByDescending(transaction =>
                        transaction.Category != null
                            ? transaction.Category.Name
                            : string.Empty)
                    .ThenByDescending(transaction =>
                        transaction.Id)
                : query
                    .OrderBy(transaction =>
                        transaction.Category != null
                            ? transaction.Category.Name
                            : string.Empty)
                    .ThenBy(transaction =>
                        transaction.Id),

            "account" => descending
                ? query
                    .OrderByDescending(transaction =>
                        transaction.Account != null
                            ? transaction.Account.Name
                            : string.Empty)
                    .ThenByDescending(transaction =>
                        transaction.Id)
                : query
                    .OrderBy(transaction =>
                        transaction.Account != null
                            ? transaction.Account.Name
                            : string.Empty)
                    .ThenBy(transaction =>
                        transaction.Id),

            _ => descending
                ? query
                    .OrderByDescending(transaction =>
                        transaction.Date)
                    .ThenByDescending(transaction =>
                        transaction.Id)
                : query
                    .OrderBy(transaction =>
                        transaction.Date)
                    .ThenBy(transaction =>
                        transaction.Id)
        };
    }
}