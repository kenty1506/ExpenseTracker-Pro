using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Provides financial reports and analytics for the authenticated user.
/// </summary>
/// <remarks>
/// Includes monthly reports, trends, cash flow, spending calendars, category comparisons, statistics, and paginated transaction analysis.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Reports & Analytics")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Retrieves monthly income, expense, balance, and transaction totals.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Monthly report retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The report year is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("monthly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year)
    {
        var report = await _reportService.GetMonthlyReportAsync(year);
        return Ok(report);
    }

    /// <summary>
    /// Retrieves the highest-spending expense categories.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <param name="limit">
    /// The maximum number of categories to return.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Top categories retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or limit is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("top-categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTopCategories([FromQuery] int year, [FromQuery] int limit = 5)
    {
        var categories = await _reportService.GetTopCategoriesAsync(year, limit);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves monthly income and expense trends.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Trend report retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The report year is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTrend([FromQuery] int year)
    {
        var trend = await _reportService.GetTrendAsync(year);
        return Ok(trend);
    }

    /// <summary>
    /// Retrieves monthly opening balance, cash flow, and closing balance.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Cash-flow report retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The report year is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("cash-flow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCashFlow([FromQuery] int year)
    {
        var cashFlow = await _reportService.GetCashFlowAsync(year);
        return Ok(cashFlow);
    }

    /// <summary>
    /// Retrieves daily expense totals for a selected month.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <param name="month">
    /// The report month from 1 to 12.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Daily spending report retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or month is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("daily-spending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailySpending([FromQuery] int year, [FromQuery] int month)
    {
        var report = await _reportService.GetDailySpendingAsync(year, month);
        return Ok(report);
    }

    /// <summary>
    /// Retrieves calendar-based daily spending totals.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <param name="month">
    /// The report month from 1 to 12.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Spending calendar retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or month is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Calendar(int year, int month)
    {
        var result = await _reportService.GetCalendarAsync(year, month);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the largest transactions.
    /// </summary>
    /// <param name="limit">
    /// The maximum number of records to return.
    /// </param>
    /// <param name="type">
    /// Optional transaction type filter.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Largest transactions retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The limit or type is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("largest-transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLargestTransactions([FromQuery] int limit = 10, [FromQuery] TransactionType? type = null)
    {
        var transactions = await _reportService.GetLargestTransactionsAsync(limit, type);
        return Ok(transactions);
    }

    /// <summary>
    /// Compares monthly spending across expense categories.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Category comparison retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The report year is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("category-comparison")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoryComparison([FromQuery] int year)
    {
        var comparison = await _reportService.GetCategoryComparisonAsync(year);
        return Ok(comparison);
    }

    /// <summary>
    /// Retrieves annual financial statistics.
    /// </summary>
    /// <param name="year">
    /// The report year.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial statistics retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The report year is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatistics([FromQuery] int year)
    {
        var statistics = await _reportService.GetStatisticsAsync(year);
        return Ok(statistics);
    }
    /// <summary>
    /// Retrieves a paginated and filtered transaction report.
    /// </summary>
    /// <remarks>
    /// Supports transaction type, account, category, date range, amount range, search text, and sorting.
    /// </remarks>
    /// <param name="query">
    /// Pagination, filtering, searching, and sorting options.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Transaction report page retrieved successfully.
    /// </response>
    /// <response code="400">
    /// Invalid query parameters.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("largest-transactions/paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult>
    GetLargestTransactionsPaged([FromQuery] LargestTransactionQueryDto query)
    {
        var result = await _reportService.GetLargestTransactionsPagedAsync(query);
        return Ok(result);
    }
}
