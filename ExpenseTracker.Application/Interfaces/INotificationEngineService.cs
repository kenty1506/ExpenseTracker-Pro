using ExpenseTracker.Application.DTOs.Notifications;

namespace ExpenseTracker.Application.Interfaces;

public interface INotificationEngineService
{
    Task<NotificationGenerationResultDto> GenerateAsync();

    Task<NotificationGenerationResultDto> GenerateForUserAsync(string userId);

}