using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync(string userId,bool unreadOnly);

    Task<Notification?> GetByIdAsync(int id,string userId);

    Task<Notification?> GetByUniqueKeyAsync(string userId,string uniqueKey);

    Task<Notification> CreateAsync(Notification notification);

    Task<bool> MarkAsReadAsync(int id,string userId);

    Task<int> MarkAllAsReadAsync(string userId);

    Task<bool> DeleteAsync(int id,string userId);

    Task<int> DeleteReadAsync(string userId);
}