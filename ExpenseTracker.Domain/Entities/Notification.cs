using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime OccurredAt { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? ActionUrl { get; set; }

    public string? UniqueKey { get; set; }
}