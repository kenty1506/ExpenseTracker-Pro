using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool unreadOnly = false)
    {
        var notifications =
            await _notificationService.GetAllAsync(
                unreadOnly);

        return Ok(notifications);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary =
            await _notificationService.GetSummaryAsync();

        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification =
            await _notificationService.GetByIdAsync(id);

        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var updated =
            await _notificationService.MarkAsReadAsync(id);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var updatedCount =
            await _notificationService.MarkAllAsReadAsync();

        return Ok(new
        {
            updatedCount
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _notificationService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteRead()
    {
        var deletedCount =
            await _notificationService.DeleteReadAsync();

        return Ok(new
        {
            deletedCount
        });
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        var result =
            await _notificationEngineService.GenerateAsync();

        return Ok(result);
    }
}