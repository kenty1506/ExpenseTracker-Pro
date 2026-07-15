using Asp.Versioning;
using ExpenseTracker.Application.DTOs.RecurringTransactions;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages recurring income and expense schedules for the authenticated user.
/// </summary>
/// <remarks>
/// Provides recurring transaction CRUD operations, due-occurrence generation, and upcoming schedule retrieval.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Recurring Transactions")]
public class RecurringTransactionsController : ControllerBase
{
    private readonly IRecurringTransactionService _recurringTransactionService;
    public RecurringTransactionsController(
        IRecurringTransactionService recurringTransactionService)
    {
        _recurringTransactionService = recurringTransactionService;
    }
    /// <summary>
    /// Retrieves all recurring transactions.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Recurring transactions retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var recurringTransactions = await _recurringTransactionService.GetAllAsync();
        return Ok(recurringTransactions);
    }

    /// <summary>
    /// Retrieves a recurring transaction by its identifier.
    /// </summary>
    /// <param name="id">
    /// The recurring transaction identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Recurring transaction found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Recurring transaction not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var recurringTransaction = await _recurringTransactionService.GetByIdAsync(id);
        if (recurringTransaction == null)
            return NotFound();
        return Ok(recurringTransaction);
    }

    /// <summary>
    /// Creates a recurring income or expense schedule.
    /// </summary>
    /// <param name="dto">
    /// The recurring transaction information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Recurring transaction created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid recurring transaction data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateRecurringTransactionDto dto)
    {
        var recurringTransaction = await _recurringTransactionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),new { id = recurringTransaction.Id }, recurringTransaction);
    }

    /// <summary>
    /// Updates an existing recurring transaction.
    /// </summary>
    /// <param name="id">
    /// The recurring transaction identifier.
    /// </param>
    /// <param name="dto">
    /// The updated recurring transaction information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Recurring transaction updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid recurring transaction data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Recurring transaction not found.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateRecurringTransactionDto dto)
    {
        var recurringTransaction =
            await _recurringTransactionService.UpdateAsync(id, dto);
        if (recurringTransaction == null)
            return NotFound();
        return Ok(recurringTransaction);
    }

    /// <summary>
    /// Deletes a recurring transaction.
    /// </summary>
    /// <param name="id">
    /// The recurring transaction identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Recurring transaction deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Recurring transaction not found.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _recurringTransactionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Generates transaction occurrences that are due.
    /// </summary>
    /// <remarks>
    /// The operation is duplicate-safe and advances each processed recurring schedule.
    /// </remarks>
    /// <param name="throughDate">
    /// Optional inclusive date through which due occurrences are generated.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Due recurring transactions processed successfully.
    /// </response>
    /// <response code="400">
    /// The supplied date is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPost("generate-due")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateDue([FromQuery] DateTime? throughDate = null)
    {
        var result = await _recurringTransactionService.GenerateDueAsync(throughDate);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves recurring transactions due within a future window.
    /// </summary>
    /// <param name="days">
    /// The number of days to include, from 1 to 365.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Upcoming recurring transactions retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The day range is invalid.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 30)
    {
        var upcoming =await _recurringTransactionService.GetUpcomingAsync(days);
        return Ok(upcoming);
    }
}
