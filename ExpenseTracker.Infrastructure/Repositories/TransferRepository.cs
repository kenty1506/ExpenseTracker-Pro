using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        _context.Transfers.Add(transfer);

        await _context.SaveChangesAsync();

        return transfer;
    }

    public async Task<bool> UpdateAsync(
        Transfer transfer)
    {
        var existing =
            await _context.Transfers
                .FirstOrDefaultAsync(x =>
                    x.Id == transfer.Id &&
                    x.UserId == transfer.UserId);

        if (existing == null)
            return false;

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

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var transfer =
            await _context.Transfers
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (transfer == null)
            return false;

        _context.Transfers.Remove(transfer);

        await _context.SaveChangesAsync();

        return true;
    }
}