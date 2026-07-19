using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.DTOs.AuditTrails;

public sealed class AuditTrailQueryDto : PagedQuery
{
    public string? Module { get; set; }

    public string? Operation { get; set; }

    public string? EntityId { get; set; }

    public int? StatusCode { get; set; }

    public bool? Succeeded { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public string? TraceId { get; set; }

    public string? Search { get; set; }
}
