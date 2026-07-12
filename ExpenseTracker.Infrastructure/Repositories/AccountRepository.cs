using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
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
            .Where(account =>account.UserId == userId)
            .Include(account =>account.Transactions)
            .OrderByDescending(account =>account.IsActive)
            .ThenBy(account =>account.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id,string userId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account =>account.Id == id &&account.UserId == userId);
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
}