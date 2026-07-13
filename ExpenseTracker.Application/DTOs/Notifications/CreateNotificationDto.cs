using ExpenseTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Notifications;

public class CreateNotificationDto
{
    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    [MaxLength(100)]
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    [MaxLength(300)]
    public string? UniqueKey { get; set; }
}