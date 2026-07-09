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

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
            return false;

        _context.Transactions.Remove(transaction);

        await _context.SaveChangesAsync();

        return true;
    }
}