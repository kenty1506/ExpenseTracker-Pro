using ExpenseTracker.Application.DTOs.Notifications;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync( bool unreadOnly = false);

    Task<NotificationDto?> GetByIdAsync(int id);

    Task<NotificationSummaryDto> GetSummaryAsync();

    Task<bool> MarkAsReadAsync(int id);

    Task<int> MarkAllAsReadAsync();

    Task<bool> DeleteAsync(int id);

    Task<int> DeleteReadAsync();


    Task<NotificationDto?> CreateIfMissingAsync(CreateNotificationDto dto);
    Task<NotificationDto?> CreateIfMissingForUserAsync(string userId,CreateNotificationDto dto);
    Task<PagedResult<NotificationDto>> GetPagedAsync(NotificationQueryDto query);
}