namespace ExpenseTracker.Application.DTOs.AuditTrails;

public sealed class AuditTrailDto
{
    public long Id { get; set; }

    public string Module { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string Method { get; set; } = string.Empty;

    public string Route { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public bool Succeeded { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string TraceId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
