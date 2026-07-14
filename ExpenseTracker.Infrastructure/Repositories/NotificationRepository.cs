using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Notifications;

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
    public async Task<PagedResult<Notification>> GetPagedAsync(
    string userId,
    NotificationQueryDto query)
    {
        var notifications = _context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == userId)
            .AsQueryable();

        if (query.IsRead.HasValue)
        {
            notifications = notifications.Where(notification =>
                notification.IsRead == query.IsRead.Value);
        }

        if (query.Type.HasValue)
        {
            notifications = notifications.Where(notification =>
                notification.Type == query.Type.Value);
        }

        if (query.Priority.HasValue)
        {
            notifications = notifications.Where(notification =>
                notification.Priority == query.Priority.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDate = query.FromDate.Value.Date;

            notifications = notifications.Where(notification =>
                notification.OccurredAt >= fromDate);
        }

        if (query.ToDate.HasValue)
        {
            var nextDay = query.ToDate.Value.Date.AddDays(1);

            notifications = notifications.Where(notification =>
                notification.OccurredAt < nextDay);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            notifications = notifications.Where(notification =>
                notification.Title.Contains(search) ||
                notification.Message.Contains(search) ||
                (notification.ReferenceType != null &&
                 notification.ReferenceType.Contains(search)));
        }

        notifications = ApplySorting(
            notifications,
            query.SortBy,
            query.SortDirection);

        var totalRecords =
            await notifications.CountAsync();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)query.PageSize);

        var items =
            await notifications
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

        return new PagedResult<Notification>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Items = items
        };
    }
    private static IQueryable<Notification> ApplySorting(
    IQueryable<Notification> query,
    string? sortBy,
    string? sortDirection)
    {
        var descending =
            string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "priority" => descending
                ? query
                    .OrderByDescending(notification =>
                        notification.Priority)
                    .ThenByDescending(notification =>
                        notification.OccurredAt)
                    .ThenByDescending(notification =>
                        notification.Id)
                : query
                    .OrderBy(notification =>
                        notification.Priority)
                    .ThenBy(notification =>
                        notification.OccurredAt)
                    .ThenBy(notification =>
                        notification.Id),

            "type" => descending
                ? query
                    .OrderByDescending(notification =>
                        notification.Type)
                    .ThenByDescending(notification =>
                        notification.OccurredAt)
                    .ThenByDescending(notification =>
                        notification.Id)
                : query
                    .OrderBy(notification =>
                        notification.Type)
                    .ThenBy(notification =>
                        notification.OccurredAt)
                    .ThenBy(notification =>
                        notification.Id),

            "read" or "isread" => descending
                ? query
                    .OrderByDescending(notification =>
                        notification.IsRead)
                    .ThenByDescending(notification =>
                        notification.OccurredAt)
                    .ThenByDescending(notification =>
                        notification.Id)
                : query
                    .OrderBy(notification =>
                        notification.IsRead)
                    .ThenBy(notification =>
                        notification.OccurredAt)
                    .ThenBy(notification =>
                        notification.Id),

            "title" => descending
                ? query
                    .OrderByDescending(notification =>
                        notification.Title)
                    .ThenByDescending(notification =>
                        notification.Id)
                : query
                    .OrderBy(notification =>
                        notification.Title)
                    .ThenBy(notification =>
                        notification.Id),

            _ => descending
                ? query
                    .OrderByDescending(notification =>
                        notification.OccurredAt)
                    .ThenByDescending(notification =>
                        notification.Id)
                : query
                    .OrderBy(notification =>
                        notification.OccurredAt)
                    .ThenBy(notification =>
                        notification.Id)
        };
    }
}