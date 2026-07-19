using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;

namespace ExpenseTracker.Application.Interfaces;

public interface IAuditTrailService
{
    Task<PagedResult<AuditTrailDto>> GetConsolidatedAsync(
        AuditTrailQueryDto query);

    Task<PagedResult<AuditTrailDto>> GetModuleAsync(
        string module,
        AuditTrailQueryDto query);

    Task<AuditTrailDto?> GetByIdAsync(long id);

    Task<IReadOnlyList<AuditModuleSummaryDto>> GetModuleSummaryAsync(
        DateTime? fromUtc,
        DateTime? toUtc);
}
