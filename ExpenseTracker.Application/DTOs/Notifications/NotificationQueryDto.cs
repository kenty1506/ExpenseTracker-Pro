using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Notifications;

public class NotificationQueryDto : PagedQuery
{
    public bool? IsRead { get; set; }

    public NotificationType? Type { get; set; }

    public NotificationPriority? Priority { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? Search { get; set; }
}