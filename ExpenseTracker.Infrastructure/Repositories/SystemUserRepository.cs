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
        GetAllActiveUserIdsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToListAsync();
    }
}