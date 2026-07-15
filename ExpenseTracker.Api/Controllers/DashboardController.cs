using Asp.Versioning;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Provides dashboard summaries and analytics for the authenticated user.
/// </summary>
/// <remarks>
/// Combines financial summaries, category breakdowns, goals, budgets, recurring items, notifications, and recent activity.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retrieves the primary financial dashboard summary.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Dashboard summary retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves expense totals grouped by category.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Category breakdown retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("category-breakdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoryBreakdown()
    {
        var breakdown = await _dashboardService.GetCategoryBreakdownAsync();
        return Ok(breakdown);
    }

    /// <summary>
    /// Retrieves the comprehensive Dashboard V2 overview.
    /// </summary>
    /// <remarks>
    /// Combines financial and account summaries, goals, notifications, budget alerts, upcoming recurring items, recent transactions, and recent transfers.
    /// </remarks>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Dashboard overview retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("v2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardV2()
    {
        var dashboard = await _dashboardService.GetDashboardV2Async();

        return Ok(dashboard);
    }
}
