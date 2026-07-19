namespace ExpenseTracker.Application.DTOs.AuditTrails;

public sealed class AuditTrailDto
{
    public long Id { get; set; }

    public string Module { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public bool Succeeded { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
