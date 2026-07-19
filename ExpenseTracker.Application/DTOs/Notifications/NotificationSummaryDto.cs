namespace ExpenseTracker.Application.DTOs.Notifications;

public class NotificationSummaryDto
{
    public int TotalCount { get; set; }

    public int UnreadCount { get; set; }

    public int HighPriorityUnreadCount { get; set; }

    public int CriticalUnreadCount { get; set; }
}