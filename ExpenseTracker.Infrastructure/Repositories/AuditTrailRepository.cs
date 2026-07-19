using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public sealed class AuditTrailRepository : IAuditTrailRepository
{
    private static readonly string[] ExcludedModules =
    [
        "Auth",
        "Authentication",
        "Authorization",
        "Identity",
        "Session",
        "Token",
        "Security"
    ];

    private readonly ExpenseTrackerDbContext _context;

    public AuditTrailRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(
        string userId,
        AuditTrailQueryDto query)
    {
        var auditLogs = _context.AuditLogs
            .AsNoTracking()
            .Where(audit =>
                audit.UserId == userId &&
                !ExcludedModules.Contains(audit.Module))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            var module = query.Module.Trim();
            auditLogs = auditLogs.Where(audit =>
                audit.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(query.Operation))
        {
            var operation = query.Operation.Trim();
            auditLogs = auditLogs.Where(audit =>
                audit.Operation == operation);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            var entityId = query.EntityId.Trim();
            auditLogs = auditLogs.Where(audit =>
                audit.EntityId == entityId);
        }

        if (query.StatusCode.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.StatusCode == query.StatusCode.Value);
        }

        if (query.Succeeded.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.Succeeded == query.Succeeded.Value);
        }

        if (query.FromUtc.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.CreatedAtUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.CreatedAtUtc <= query.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.TraceId))
        {
            var traceId = query.TraceId.Trim();
            auditLogs = auditLogs.Where(audit =>
                audit.TraceId == traceId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            auditLogs = auditLogs.Where(audit =>
                audit.Module.Contains(search) ||
                audit.Operation.Contains(search) ||
                (audit.EntityId != null &&
                 audit.EntityId.Contains(search)));
        }

        auditLogs = ApplySorting(
            auditLogs,
            query.SortBy,
            query.SortDirection);

        var totalRecords = await auditLogs.CountAsync();
        var totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)query.PageSize);

        var items = await auditLogs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }

    public Task<AuditLog?> GetByIdAsync(
        long id,
        string userId)
    {
        return _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(audit =>
                audit.Id == id &&
                audit.UserId == userId &&
                !ExcludedModules.Contains(audit.Module));
    }

    public async Task<IReadOnlyList<AuditModuleSummaryDto>>
        GetModuleSummaryAsync(
            string userId,
            DateTime? fromUtc,
            DateTime? toUtc)
    {
        var auditLogs = _context.AuditLogs
            .AsNoTracking()
            .Where(audit =>
                audit.UserId == userId &&
                !ExcludedModules.Contains(audit.Module))
            .AsQueryable();

        if (fromUtc.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            auditLogs = auditLogs.Where(audit =>
                audit.CreatedAtUtc <= toUtc.Value);
        }

        return await auditLogs
            .GroupBy(audit => audit.Module)
            .Select(group => new AuditModuleSummaryDto
            {
                Module = group.Key,
                TotalEvents = group.Count(),
                SuccessfulEvents = group.Count(audit =>
                    audit.Succeeded),
                FailedEvents = group.Count(audit =>
                    !audit.Succeeded),
                LastEventAtUtc = group.Max(audit =>
                    audit.CreatedAtUtc)
            })
            .OrderByDescending(summary =>
                summary.LastEventAtUtc)
            .ThenBy(summary => summary.Module)
            .ToListAsync();
    }

    private static IQueryable<AuditLog> ApplySorting(
        IQueryable<AuditLog> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "module" => descending
                ? query
                    .OrderByDescending(audit => audit.Module)
                    .ThenByDescending(audit => audit.Id)
                : query
                    .OrderBy(audit => audit.Module)
                    .ThenBy(audit => audit.Id),

            "operation" => descending
                ? query
                    .OrderByDescending(audit => audit.Operation)
                    .ThenByDescending(audit => audit.Id)
                : query
                    .OrderBy(audit => audit.Operation)
                    .ThenBy(audit => audit.Id),

            "status" => descending
                ? query
                    .OrderByDescending(audit => audit.StatusCode)
                    .ThenByDescending(audit => audit.Id)
                : query
                    .OrderBy(audit => audit.StatusCode)
                    .ThenBy(audit => audit.Id),

            "elapsed" => descending
                ? query
                    .OrderByDescending(audit =>
                        audit.ElapsedMilliseconds)
                    .ThenByDescending(audit => audit.Id)
                : query
                    .OrderBy(audit => audit.ElapsedMilliseconds)
                    .ThenBy(audit => audit.Id),

            _ => descending
                ? query
                    .OrderByDescending(audit => audit.CreatedAtUtc)
                    .ThenByDescending(audit => audit.Id)
                : query
                    .OrderBy(audit => audit.CreatedAtUtc)
                    .ThenBy(audit => audit.Id)
        };
    }
}
