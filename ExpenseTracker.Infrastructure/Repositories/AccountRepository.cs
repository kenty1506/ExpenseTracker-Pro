using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTracker.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public AccountRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Account>> GetAllAsync(
        string userId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(account => account.IncomingTransfers)
            .Include(account => account.OutgoingTransfers)
            .Where(account =>account.UserId == userId)
            .Include(account =>account.Transactions)
            .OrderByDescending(account =>account.IsActive)
            .ThenBy(account =>account.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id, string userId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(account => account.Transactions)
            .Include(account => account.IncomingTransfers)
            .ThenInclude(transfer => transfer.FromAccount)
            .Include(account => account.OutgoingTransfers)
            .ThenInclude(transfer => transfer.ToAccount)
            .FirstOrDefaultAsync(account =>account.Id == id && account.UserId == userId);
    }

    public async Task<Account?> GetByNameAsync(string name,string userId)
    {
        var normalizedName = name.Trim().ToLower();
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account =>
                account.UserId == userId &&
                account.Name.ToLower() == normalizedName);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> UpdateAsync(Account account)
    {
        var existing =
            await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    x.Id == account.Id &&
                    x.UserId == account.UserId);

        if (existing == null)
            return false;

        existing.Name = account.Name;
        existing.Type = account.Type;
        existing.Currency = account.Currency;
        existing.OpeningBalance = account.OpeningBalance;
        existing.Color = account.Color;
        existing.Icon = account.Icon;
        existing.IncludeInNetWorth =account.IncludeInNetWorth;
        existing.IsActive =account.IsActive;
        existing.UpdatedAt =DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var account =
            await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (account == null)
            return false;

        var hasTransactions = await HasTransactionsAsync(id,userId);

        if (hasTransactions)
        {
            throw new ArgumentException("This account cannot be deleted because it has transactions.");
        }
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasTransactionsAsync(int id, string userId)
    {
        return await _context.Transactions
            .AnyAsync(transaction =>
                transaction.UserId == userId &&
                transaction.AccountId == id);
    }

    public async Task<PagedResult<Account>> GetPagedAsync(
    string userId,
    AccountQueryDto query)
    {
        var accounts = _context.Accounts
            .AsNoTracking()
            .AsSplitQuery()
            .Where(account =>
                account.UserId == userId)
            .Include(account =>
                account.Transactions)
            .Include(account =>
                account.IncomingTransfers)
            .Include(account =>
                account.OutgoingTransfers)
            .AsQueryable();

        if (query.Type.HasValue)
        {
            accounts = accounts.Where(account =>
                account.Type == query.Type.Value);
        }

        if (query.IsActive.HasValue)
        {
            accounts = accounts.Where(account =>
                account.IsActive == query.IsActive.Value);
        }

        if (query.IncludeInNetWorth.HasValue)
        {
            accounts = accounts.Where(account =>
                account.IncludeInNetWorth ==
                query.IncludeInNetWorth.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            var currency =
                query.Currency.Trim().ToUpper();

            accounts = accounts.Where(account =>
                account.Currency == currency);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            accounts = accounts.Where(account =>
                account.Name.Contains(search) ||
                account.Currency.Contains(search));
        }

        if (query.MinBalance.HasValue)
        {
            var minimumBalance = query.MinBalance.Value;

            accounts = accounts.Where(account =>
                account.OpeningBalance
                + (
                    account.Transactions
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Income)
                        .Sum(transaction =>
                            (decimal?)transaction.Amount)
                    ?? 0m
                )
                - (
                    account.Transactions
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Expense)
                        .Sum(transaction =>
                            (decimal?)transaction.Amount)
                    ?? 0m
                )
                + (
                    account.IncomingTransfers
                        .Sum(transfer =>
                            (decimal?)transfer.Amount)
                    ?? 0m
                )
                - (
                    account.OutgoingTransfers
                        .Sum(transfer =>
                            (decimal?)transfer.Amount)
                    ?? 0m
                )
                >= minimumBalance);
        }

        if (query.MaxBalance.HasValue)
        {
            var maximumBalance = query.MaxBalance.Value;

            accounts = accounts.Where(account =>
                account.OpeningBalance
                + (
                    account.Transactions
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Income)
                        .Sum(transaction =>
                            (decimal?)transaction.Amount)
                    ?? 0m
                )
                - (
                    account.Transactions
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Expense)
                        .Sum(transaction =>
                            (decimal?)transaction.Amount)
                    ?? 0m
                )
                + (
                    account.IncomingTransfers
                        .Sum(transfer =>
                            (decimal?)transfer.Amount)?? 0m)
                    - (
                    account.OutgoingTransfers
                        .Sum(transfer => (decimal?) transfer.Amount) ?? 0m) <= maximumBalance);
        }

        accounts = ApplySorting(accounts,query.SortBy,query.SortDirection);
        var totalRecords =await accounts.CountAsync();
        var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords /(double)query.PageSize);
        var items = await accounts
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

        return new PagedResult<Account>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }
    private static IQueryable<Account> ApplySorting(IQueryable<Account> query,string? sortBy,string? sortDirection)
    {
        var descending = string.Equals(sortDirection,"desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "type" => descending
                ? query
                    .OrderByDescending(account =>account.Type)
                    .ThenByDescending(account =>account.Id)
                : query
                    .OrderBy(account =>account.Type)
                    .ThenBy(account =>account.Id),

            "openingbalance" => descending
                ? query
                    .OrderByDescending(account =>account.OpeningBalance)
                    .ThenByDescending(account =>account.Id)
                : query
                    .OrderBy(account =>account.OpeningBalance)
                    .ThenBy(account =>account.Id),

            "balance" => descending
                ? query
                    .OrderByDescending(account =>
                        account.OpeningBalance
                        + (account.Transactions
                            .Where(transaction =>
                                transaction.Type ==
                                    TransactionType.Income)
                            .Sum(transaction =>
                                (decimal?)transaction.Amount) ?? 0m)
                        - (account.Transactions
                            .Where(transaction =>
                                transaction.Type ==
                                    TransactionType.Expense)
                            .Sum(transaction =>
                                (decimal?)transaction.Amount) ?? 0m)
                        + (account.IncomingTransfers
                            .Sum(transfer =>
                                (decimal?)transfer.Amount) ?? 0m)
                        - (account.OutgoingTransfers
                            .Sum(transfer =>
                                (decimal?)transfer.Amount) ?? 0m))
                    .ThenByDescending(account => account.Id)
                : query
                    .OrderBy(account =>
                        account.OpeningBalance
                        + (account.Transactions
                            .Where(transaction =>
                                transaction.Type ==
                                    TransactionType.Income)
                            .Sum(transaction =>
                                (decimal?)transaction.Amount) ?? 0m)
                        - (account.Transactions
                            .Where(transaction =>
                                transaction.Type ==
                                    TransactionType.Expense)
                            .Sum(transaction =>
                                (decimal?)transaction.Amount) ?? 0m)
                        + (account.IncomingTransfers
                            .Sum(transfer =>
                                (decimal?)transfer.Amount) ?? 0m)
                        - (account.OutgoingTransfers
                            .Sum(transfer =>
                                (decimal?)transfer.Amount) ?? 0m))
                    .ThenBy(account => account.Id), _ => descending? query
                    .OrderByDescending(account => account.Name)
                    .ThenByDescending(account => account.Id) : query
                    .OrderBy(account => account.Name)
                    .ThenBy(account => account.Id)
        };
    }
}
