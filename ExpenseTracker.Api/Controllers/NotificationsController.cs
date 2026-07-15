using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Notifications;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages in-app notifications for the authenticated user.
/// </summary>
/// <remarks>
/// Provides notification retrieval, summaries, read-state management, deletion, automatic generation, filtering, sorting, and pagination.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationEngineService _notificationEngineService;

    public NotificationsController(
        INotificationService notificationService,
        INotificationEngineService notificationEngineService)
    {
        _notificationService = notificationService;
        _notificationEngineService = notificationEngineService;
    }

    /// <summary>
    /// Retrieves notifications.
    /// </summary>
    /// <param name="unreadOnly">
    /// When true, returns unread notifications only.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notifications retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false)
    {
        var notifications =await _notificationService.GetAllAsync(unreadOnly);
        return Ok(notifications);
    }

    /// <summary>
    /// Retrieves notification counts and unread priority totals.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notification summary retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _notificationService.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves a notification by its identifier.
    /// </summary>
    /// <param name="id">
    /// The notification identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notification found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Notification not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        if (notification == null)
            return NotFound();
        return Ok(notification);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">
    /// The notification identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Notification marked as read.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Notification not found.
    /// </response>
    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var updated = await _notificationService.MarkAsReadAsync(id);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Marks all unread notifications as read.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notifications updated successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var updatedCount = await _notificationService.MarkAllAsReadAsync();
        return Ok(new
        {
            updatedCount
        });
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">
    /// The notification identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Notification deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Notification not found.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _notificationService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes all read notifications.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Read notifications deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpDelete("read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteRead()
    {
        var deletedCount = await _notificationService.DeleteReadAsync();
        return Ok(new
        {
            deletedCount
        });
    }

    /// <summary>
    /// Runs the notification engine for the authenticated user.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notification generation completed.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Generate()
    {
        var result = await _notificationEngineService.GenerateAsync();
        return Ok(result);
    }
    /// <summary>
    /// Retrieves a paginated and filtered list of notifications.
    /// </summary>
    /// <remarks>
    /// Supports read status, type, priority, date range, search text, and sorting.
    /// </remarks>
    /// <param name="query">
    /// Pagination, filtering, searching, and sorting options.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Notification page retrieved successfully.
    /// </response>
    /// <response code="400">
    /// Invalid query parameters.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
    [FromQuery] NotificationQueryDto query)
    {
        var result = await _notificationService.GetPagedAsync(query);
        return Ok(result);
    }
}
