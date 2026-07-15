using Asp.Versioning;
using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages financial goals and goal contributions for the authenticated user.
/// </summary>
/// <remarks>
/// Provides goal CRUD operations, contribution tracking, progress summaries, filtering, sorting, and pagination.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Financial Goals")]
public class FinancialGoalsController : ControllerBase
{
    private readonly IFinancialGoalService _financialGoalService;
    public FinancialGoalsController(
        IFinancialGoalService financialGoalService)
    {
        _financialGoalService = financialGoalService;
    }

    /// <summary>
    /// Retrieves all financial goals.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial goals retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var goals = await _financialGoalService.GetAllAsync();
        return Ok(goals);
    }

    /// <summary>
    /// Retrieves a financial goal by its identifier.
    /// </summary>
    /// <param name="id">
    /// The financial goal identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial goal found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Financial goal not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var goal = await _financialGoalService.GetByIdAsync(id);
        if (goal == null)
            return NotFound();
        return Ok(goal);
    }

    /// <summary>
    /// Creates a new financial goal.
    /// </summary>
    /// <param name="dto">
    /// The financial goal information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Financial goal created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid goal data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="409">
    /// A conflicting goal already exists.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateFinancialGoalDto dto)
    {
        var goal = await _financialGoalService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),new { id = goal.Id }, goal);
    }

    /// <summary>
    /// Updates an existing financial goal.
    /// </summary>
    /// <param name="id">
    /// The financial goal identifier.
    /// </param>
    /// <param name="dto">
    /// The updated financial goal information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial goal updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid goal data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Financial goal not found.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateFinancialGoalDto dto)
    {
        var goal =await _financialGoalService.UpdateAsync(id, dto);
        if (goal == null)
            return NotFound();
        return Ok(goal);
    }

    /// <summary>
    /// Deletes a financial goal and its contributions.
    /// </summary>
    /// <param name="id">
    /// The financial goal identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Financial goal deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Financial goal not found.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _financialGoalService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Adds a contribution to a financial goal.
    /// </summary>
    /// <param name="id">
    /// The financial goal identifier.
    /// </param>
    /// <param name="dto">
    /// The contribution information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Contribution added successfully.
    /// </response>
    /// <response code="400">
    /// Invalid contribution data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Financial goal not found.
    /// </response>
    [HttpPost("{id:int}/contributions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddContribution(int id, AddGoalContributionDto dto)
    {
        var contribution =await _financialGoalService.AddContributionAsync(id, dto);
        if (contribution == null)
            return NotFound();
        return Ok(contribution);
    }

    /// <summary>
    /// Deletes a contribution from a financial goal.
    /// </summary>
    /// <param name="goalId">
    /// The financial goal identifier.
    /// </param>
    /// <param name="contributionId">
    /// The contribution identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Contribution deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Goal or contribution not found.
    /// </response>
    [HttpDelete("{goalId:int}/contributions/{contributionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContribution(int goalId,int contributionId)
    {
        var deleted =await _financialGoalService.DeleteContributionAsync(goalId, contributionId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Retrieves the financial goals portfolio summary.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial goals summary retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary()
    {
        var summary =await _financialGoalService.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of financial goals.
    /// </summary>
    /// <remarks>
    /// Supports status, account, completion, overdue, amount, target-date, search, and sorting filters.
    /// </remarks>
    /// <param name="query">
    /// Pagination, filtering, searching, and sorting options.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Financial goal page retrieved successfully.
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
    [FromQuery] FinancialGoalQueryDto query)
    {
        var result = await _financialGoalService.GetPagedAsync(query);
        return Ok(result);
    }
}
