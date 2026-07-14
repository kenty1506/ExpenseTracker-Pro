using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Notifications;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository
        _notificationRepository;

    private readonly ICurrentUserService
        _currentUserService;

    public NotificationService(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository =
            notificationRepository;

        _currentUserService =
            currentUserService;
    }

    public async Task<IEnumerable<NotificationDto>>
        GetAllAsync(bool unreadOnly = false)
    {
        var notifications =
            await _notificationRepository.GetAllAsync(
                _currentUserService.UserId,
                unreadOnly);

        return notifications.Select(MapToDto);
    }

    public async Task<NotificationDto?> GetByIdAsync(
        int id)
    {
        var notification =
            await _notificationRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        return notification == null
            ? null
            : MapToDto(notification);
    }

    public async Task<NotificationSummaryDto>
        GetSummaryAsync()
    {
        var notifications =
            (await _notificationRepository.GetAllAsync(
                _currentUserService.UserId,
                unreadOnly: false))
            .ToList();

        var unread =
            notifications
                .Where(notification =>
                    !notification.IsRead)
                .ToList();

        return new NotificationSummaryDto
        {
            TotalCount = notifications.Count,
            UnreadCount = unread.Count,

            HighPriorityUnreadCount =
                unread.Count(notification =>
                    notification.Priority ==
                    NotificationPriority.High),

            CriticalUnreadCount =
                unread.Count(notification =>
                    notification.Priority ==
                    NotificationPriority.Critical)
        };
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        return await _notificationRepository
            .MarkAsReadAsync(
                id,
                _currentUserService.UserId);
    }

    public async Task<int> MarkAllAsReadAsync()
    {
        return await _notificationRepository
            .MarkAllAsReadAsync(
                _currentUserService.UserId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _notificationRepository.DeleteAsync(
            id,
            _currentUserService.UserId);
    }

    public async Task<int> DeleteReadAsync()
    {
        return await _notificationRepository.DeleteReadAsync(
            _currentUserService.UserId);
    }

    public Task<NotificationDto?> CreateIfMissingAsync(
    CreateNotificationDto dto)
    {
        return CreateIfMissingForUserAsync(
            _currentUserService.UserId,
            dto);
    }

    public async Task<NotificationDto?>
        CreateIfMissingForUserAsync(
            string userId,
            CreateNotificationDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        ValidateNotificationType(dto.Type);
        ValidateNotificationPriority(dto.Priority);

        if (!string.IsNullOrWhiteSpace(dto.UniqueKey))
        {
            var existing =
                await _notificationRepository
                    .GetByUniqueKeyAsync(
                        userId,
                        dto.UniqueKey.Trim());

            if (existing != null)
                return null;
        }

        var notification = new Notification
        {
            UserId = userId,
            Type = dto.Type,
            Priority = dto.Priority,
            Title = dto.Title.Trim(),
            Message = dto.Message.Trim(),
            IsRead = false,
            ReadAt = null,
            OccurredAt =
                dto.OccurredAt == default
                    ? DateTime.UtcNow
                    : dto.OccurredAt,
            ReferenceType =
                string.IsNullOrWhiteSpace(dto.ReferenceType)
                    ? null
                    : dto.ReferenceType.Trim(),
            ReferenceId = dto.ReferenceId,
            ActionUrl =
                string.IsNullOrWhiteSpace(dto.ActionUrl)
                    ? null
                    : dto.ActionUrl.Trim(),
            UniqueKey =
                string.IsNullOrWhiteSpace(dto.UniqueKey)
                    ? null
                    : dto.UniqueKey.Trim(),
            IsActive = true
        };

        var created =
            await _notificationRepository.CreateAsync(
                notification);

        return MapToDto(created);
    }

    private static void ValidateNotificationType(
        NotificationType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException(
                "Please provide a valid notification type.");
        }
    }

    private static void ValidateNotificationPriority(
        NotificationPriority priority)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentException(
                "Please provide a valid notification priority.");
        }
    }

    private static NotificationDto MapToDto(
        Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Priority = notification.Priority,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            OccurredAt = notification.OccurredAt,
            ReferenceType =
                notification.ReferenceType,
            ReferenceId =
                notification.ReferenceId,
            ActionUrl =
                notification.ActionUrl
        };
    }
    public async Task<PagedResult<NotificationDto>> GetPagedAsync(NotificationQueryDto query)
    {
        var result =
            await _notificationRepository.GetPagedAsync(
                _currentUserService.UserId,
                query);

        return new PagedResult<NotificationDto>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages,
            Items = result.Items
                .Select(MapToDto)
                .ToList()
        };
    }
}