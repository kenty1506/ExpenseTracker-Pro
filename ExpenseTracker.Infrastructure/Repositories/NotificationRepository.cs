using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public NotificationRepository(
        ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(
        string userId,
        bool unreadOnly)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(notification =>
                !notification.IsRead);
        }

        return await query
            .OrderByDescending(notification =>
                notification.OccurredAt)
            .ThenByDescending(notification =>
                notification.Id)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(notification =>
                notification.Id == id &&
                notification.UserId == userId);
    }

    public async Task<Notification?> GetByUniqueKeyAsync(
        string userId,
        string uniqueKey)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(notification =>
                notification.UserId == userId &&
                notification.UniqueKey == uniqueKey);
    }

    public async Task<Notification> CreateAsync(
        Notification notification)
    {
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return notification;
    }

    public async Task<bool> MarkAsReadAsync(
        int id,
        string userId)
    {
        var notification =
            await _context.Notifications
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

        if (notification == null)
            return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        string userId)
    {
        var notifications =
            await _context.Notifications
                .Where(notification =>
                    notification.UserId == userId &&
                    !notification.IsRead)
                .ToListAsync();

        if (notifications.Count == 0)
            return 0;

        var readAt = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = readAt;
            notification.UpdatedAt = readAt;
        }

        await _context.SaveChangesAsync();

        return notifications.Count;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var notification =
            await _context.Notifications
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteReadAsync(
        string userId)
    {
        var notifications =
            await _context.Notifications
                .Where(notification =>
                    notification.UserId == userId &&
                    notification.IsRead)
                .ToListAsync();

        if (notifications.Count == 0)
            return 0;

        _context.Notifications.RemoveRange(
            notifications);

        await _context.SaveChangesAsync();

        return notifications.Count;
    }
}