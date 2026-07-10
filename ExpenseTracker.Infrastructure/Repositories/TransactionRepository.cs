using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public TransactionRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync(string userId)
    {
        return await _context.Transactions
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id, string userId)
    {
        return await _context.Transactions
            .Include(x => x.Category)
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
}