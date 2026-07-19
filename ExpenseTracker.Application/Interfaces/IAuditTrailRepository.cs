using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IAuditTrailRepository
{
    Task<PagedResult<AuditLog>> GetPagedAsync(
        string userId,
        AuditTrailQueryDto query);

    Task<AuditLog?> GetByIdAsync(
        long id,
        string userId);

    Task<IReadOnlyList<AuditModuleSummaryDto>>
        GetModuleSummaryAsync(
            string userId,
            DateTime? fromUtc,
            DateTime? toUtc);
}
