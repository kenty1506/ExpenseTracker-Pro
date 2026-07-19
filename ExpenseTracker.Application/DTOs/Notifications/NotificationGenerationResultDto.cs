namespace ExpenseTracker.Application.DTOs.Notifications;

public class NotificationGenerationResultDto
{
    public int GeneratedCount { get; set; }

    public int SkippedCount { get; set; }

    public List<NotificationDto> GeneratedNotifications { get; set; } = [];
}