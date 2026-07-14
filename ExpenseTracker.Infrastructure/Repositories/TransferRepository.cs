using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transfers;

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
}