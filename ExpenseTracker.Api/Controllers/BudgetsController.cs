using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages monthly category budgets for the authenticated user.
/// </summary>
/// <remarks>
/// Provides budget CRUD operations, monthly summaries, alerts, and budget-versus-actual analysis.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Budgets")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    /// <summary>
    /// Retrieves all budgets.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budgets retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var budgets = await _budgetService.GetAllAsync();
        return Ok(budgets);
    }

    /// <summary>
    /// Retrieves a budget by its identifier.
    /// </summary>
    /// <param name="id">
    /// The budget identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budget found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Budget not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var budget = await _budgetService.GetByIdAsync(id);
        if (budget == null)
            return NotFound();
        return Ok(budget);
    }

    /// <summary>
    /// Creates a monthly category budget.
    /// </summary>
    /// <param name="dto">
    /// The budget information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Budget created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid budget data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="409">
    /// A conflicting budget already exists.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateBudgetDto dto)
    {
        var budget = await _budgetService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }

    /// <summary>
    /// Updates an existing budget.
    /// </summary>
    /// <param name="id">
    /// The budget identifier.
    /// </param>
    /// <param name="dto">
    /// The updated budget information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budget updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid budget data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Budget not found.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateBudgetDto dto)
    {
        var budget =await _budgetService.UpdateAsync(id, dto);
        if (budget == null)
            return NotFound();
        return Ok(budget);
    }

    /// <summary>
    /// Deletes a budget.
    /// </summary>
    /// <param name="id">
    /// The budget identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Budget deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Budget not found.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _budgetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Retrieves budget performance for a selected month.
    /// </summary>
    /// <param name="year">
    /// The budget year.
    /// </param>
    /// <param name="month">
    /// The budget month from 1 to 12.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budget summary retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or month is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary([FromQuery] int year, [FromQuery] int month)
    {
        var summary = await _budgetService.GetSummaryAsync(year, month);
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves budget alerts for a selected month.
    /// </summary>
    /// <param name="year">
    /// The budget year.
    /// </param>
    /// <param name="month">
    /// The budget month from 1 to 12.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budget alerts retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or month is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAlerts([FromQuery] int year, [FromQuery] int month)
    {
        var alerts = await _budgetService.GetAlertsAsync(year, month);
        return Ok(alerts);
    }

    /// <summary>
    /// Compares budgeted amounts with actual expenses.
    /// </summary>
    /// <param name="year">
    /// The budget year.
    /// </param>
    /// <param name="month">
    /// The budget month from 1 to 12.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Budget comparison retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The year or month is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("budget-vs-actual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBudgetVsActual([FromQuery] int year, [FromQuery] int month)
    {
        var report = await _budgetService.GetBudgetVsActualAsync(year, month);
        return Ok(report);
    }
}
