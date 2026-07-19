namespace ExpenseTracker.Application.DTOs.AuditTrails;

public sealed class AuditModuleSummaryDto
{
    public string Module { get; set; } = string.Empty;

    public int TotalEvents { get; set; }

    public int SuccessfulEvents { get; set; }

    public int FailedEvents { get; set; }

    public DateTime LastEventAtUtc { get; set; }
}
