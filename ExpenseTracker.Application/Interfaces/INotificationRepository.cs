using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Notifications;

namespace ExpenseTracker.Application.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync(string userId,bool unreadOnly);

    Task<Notification?> GetByIdAsync(int id,string userId);

    Task<Notification?> GetByUniqueKeyAsync(string userId,string uniqueKey);

    Task<Notification> CreateAsync(Notification notification);

    Task<Notification> UpdateAsync(Notification notification);

    Task<int> DeactivateMissingAsync(
        string userId,
        string uniqueKeyPrefix,
        IReadOnlyCollection<string> activeUniqueKeys);

    Task<bool> MarkAsReadAsync(int id,string userId);

    Task<int> MarkAllAsReadAsync(string userId);

    Task<bool> DeleteAsync(int id,string userId);

    Task<int> DeleteReadAsync(string userId);
    Task<PagedResult<Notification>> GetPagedAsync(string userId, NotificationQueryDto query);
}
