using ExpenseTracker.Application.DTOs.RecurringTransactions;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecurringTransactionsController : ControllerBase
{
    private readonly IRecurringTransactionService
        _recurringTransactionService;

    public RecurringTransactionsController(
        IRecurringTransactionService recurringTransactionService)
    {
        _recurringTransactionService =
            recurringTransactionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var recurringTransactions =
            await _recurringTransactionService.GetAllAsync();

        return Ok(recurringTransactions);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var recurringTransaction =
            await _recurringTransactionService.GetByIdAsync(id);

        if (recurringTransaction == null)
            return NotFound();

        return Ok(recurringTransaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRecurringTransactionDto dto)
    {
        var recurringTransaction =
            await _recurringTransactionService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = recurringTransaction.Id },
            recurringTransaction);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateRecurringTransactionDto dto)
    {
        var recurringTransaction =
            await _recurringTransactionService.UpdateAsync(
                id,
                dto);

        if (recurringTransaction == null)
            return NotFound();

        return Ok(recurringTransaction);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _recurringTransactionService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("generate-due")]
    public async Task<IActionResult> GenerateDue(
    [FromQuery] DateTime? throughDate = null)
    {
        var result =
            await _recurringTransactionService.GenerateDueAsync(
                throughDate);

        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(
    [FromQuery] int days = 30)
    {
        var upcoming =
            await _recurringTransactionService
                .GetUpcomingAsync(days);

        return Ok(upcoming);
    }
}