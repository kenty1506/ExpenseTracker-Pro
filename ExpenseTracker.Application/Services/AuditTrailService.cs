using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public sealed class AuditTrailService : IAuditTrailService
{
    private readonly IAuditTrailRepository _auditTrailRepository;
    private readonly ICurrentUserService _currentUserService;

    public AuditTrailService(
        IAuditTrailRepository auditTrailRepository,
        ICurrentUserService currentUserService)
    {
        _auditTrailRepository = auditTrailRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuditTrailDto>>
        GetConsolidatedAsync(AuditTrailQueryDto query)
    {
        ValidateDateRange(query.FromUtc, query.ToUtc);

        var result = await _auditTrailRepository.GetPagedAsync(
            _currentUserService.UserId,
            query);

        return MapPage(result);
    }

    public Task<PagedResult<AuditTrailDto>> GetModuleAsync(
        string module,
        AuditTrailQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException(
                "The audit module is required.");
        }

        query.Module = module.Trim();

        return GetConsolidatedAsync(query);
    }

    public async Task<AuditTrailDto?> GetByIdAsync(long id)
    {
        var auditLog = await _auditTrailRepository.GetByIdAsync(
            id,
            _currentUserService.UserId);

        return auditLog is null
            ? null
            : MapToDto(auditLog);
    }

    public Task<IReadOnlyList<AuditModuleSummaryDto>>
        GetModuleSummaryAsync(
            DateTime? fromUtc,
            DateTime? toUtc)
    {
        ValidateDateRange(fromUtc, toUtc);

        return _auditTrailRepository.GetModuleSummaryAsync(
            _currentUserService.UserId,
            fromUtc,
            toUtc);
    }

    private static void ValidateDateRange(
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue &&
            toUtc.HasValue &&
            fromUtc.Value > toUtc.Value)
        {
            throw new ArgumentException(
                "FromUtc cannot be later than ToUtc.");
        }
    }

    private static PagedResult<AuditTrailDto> MapPage(
        PagedResult<AuditLog> result)
    {
        return new PagedResult<AuditTrailDto>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages,
            Items = result.Items
                .Select(MapToDto)
                .ToList()
        };
    }

    private static AuditTrailDto MapToDto(AuditLog auditLog)
    {
        return new AuditTrailDto
        {
            Id = auditLog.Id,
            Module = auditLog.Module,
            Operation = auditLog.Operation,
            EntityId = auditLog.EntityId,
            Method = auditLog.Method,
            Route = auditLog.Route,
            Action = auditLog.Action,
            StatusCode = auditLog.StatusCode,
            Succeeded = auditLog.Succeeded,
            ElapsedMilliseconds = auditLog.ElapsedMilliseconds,
            TraceId = auditLog.TraceId,
            CreatedAtUtc = auditLog.CreatedAtUtc
        };
    }
}
