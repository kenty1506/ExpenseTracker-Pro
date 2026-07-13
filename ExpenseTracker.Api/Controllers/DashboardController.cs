using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();

        return Ok(summary);
    }

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown()
    {
        var breakdown =
            await _dashboardService.GetCategoryBreakdownAsync();

        return Ok(breakdown);
    }

    [HttpGet("v2")]
    public async Task<IActionResult> GetDashboardV2()
    {
        var dashboard =
            await _dashboardService.GetDashboardV2Async();

        return Ok(dashboard);
    }
}