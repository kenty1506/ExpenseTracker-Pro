using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year)
    {
        var report = await _reportService.GetMonthlyReportAsync(year);
        return Ok(report);
    }

    [HttpGet("top-categories")]
    public async Task<IActionResult> GetTopCategories([FromQuery] int year,[FromQuery] int limit = 5)
    {
        var categories =await _reportService.GetTopCategoriesAsync(year,limit);
        return Ok(categories);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrend([FromQuery] int year)
    {
        var trend =await _reportService.GetTrendAsync(year);
        return Ok(trend);
    }

    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] int year)
    {
        var cashFlow =await _reportService.GetCashFlowAsync(year);
        return Ok(cashFlow);
    }

    [HttpGet("daily-spending")]
    public async Task<IActionResult> GetDailySpending([FromQuery] int year, [FromQuery] int month)
    {
        var report =await _reportService.GetDailySpendingAsync(year,month);
        return Ok(report);
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> Calendar(int year,int month)
    {
        var result =await _reportService.GetCalendarAsync(year,month);
        return Ok(result);
    }

    [HttpGet("largest-transactions")]
    public async Task<IActionResult> GetLargestTransactions([FromQuery] int limit = 10,[FromQuery] TransactionType? type = null)
    {
        var transactions = await _reportService.GetLargestTransactionsAsync(limit,type);
        return Ok(transactions);
    }

    [HttpGet("category-comparison")]
    public async Task<IActionResult>GetCategoryComparison([FromQuery] int year)
    {
        var comparison =await _reportService.GetCategoryComparisonAsync(year);
        return Ok(comparison);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] int year)
    {
        var statistics =await _reportService.GetStatisticsAsync(year);
        return Ok(statistics);
    }
    [HttpGet("largest-transactions/paged")]
    public async Task<IActionResult>
    GetLargestTransactionsPaged(
        [FromQuery] LargestTransactionQueryDto query)
    {
        var result =
            await _reportService
                .GetLargestTransactionsPagedAsync(query);

        return Ok(result);
    }
}