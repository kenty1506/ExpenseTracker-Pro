using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class SystemUserRepository : ISystemUserRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public SystemUserRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>>
        GetActiveUserIdsPageAsync(
            string? afterUserId,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var boundedPageSize = Math.Clamp(pageSize, 1, 500);
        var users = _context.Users
            .AsNoTracking()
            .Where(user =>
                afterUserId == null ||
                string.Compare(user.Id, afterUserId) > 0)
            .OrderBy(user => user.Id)
            .Select(user => user.Id)
            .Take(boundedPageSize);

        return await users.ToListAsync(cancellationToken);
    }
}
