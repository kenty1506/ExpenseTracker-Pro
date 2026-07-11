using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var budgets = await _budgetService.GetAllAsync();
        return Ok(budgets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var budget = await _budgetService.GetByIdAsync(id);
        if (budget == null)
            return NotFound();
        return Ok(budget);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateBudgetDto dto)
    {
        var budget = await _budgetService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id },budget);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,UpdateBudgetDto dto)
    {
        var budget =
            await _budgetService.UpdateAsync(id, dto);
        if (budget == null)
            return NotFound();
        return Ok(budget);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _budgetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int year,[FromQuery] int month)
    {
        var summary =await _budgetService.GetSummaryAsync(year,month);
        return Ok(summary);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] int year, [FromQuery] int month)
    {
        var alerts =await _budgetService.GetAlertsAsync(year,month);
        return Ok(alerts);
    }

    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> GetBudgetVsActual([FromQuery] int year,[FromQuery] int month)
    {
        var report = await _budgetService.GetBudgetVsActualAsync(year,month);
        return Ok(report);
    }
}