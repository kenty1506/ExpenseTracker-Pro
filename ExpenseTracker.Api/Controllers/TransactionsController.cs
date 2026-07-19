using Asp.Versioning;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages income and expense transactions for the authenticated user.
/// </summary>
/// <remarks>
/// Provides endpoints to create, retrieve, update, delete,
/// search, filter, sort, and paginate financial transactions.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Transactions")]

public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Retrieves all transactions for the authenticated user.
    /// </summary>
    /// <returns>
    /// A collection of income and expense transactions.
    /// </returns>
    /// <response code="200">
    /// Transactions were retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _transactionService.GetAllAsync();
        return Ok(transactions);
    }

    /// <summary>
    /// Retrieves a transaction by its identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the transaction.
    /// </param>
    /// <returns>
    /// The requested transaction.
    /// </returns>
    /// <response code="200">
    /// Transaction found.
    /// </response>
    /// <response code="404">
    /// Transaction not found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    /// <summary>
    /// Creates a new income or expense transaction.
    /// </summary>
    /// <param name="dto">
    /// The transaction information including account, category,
    /// amount, transaction type, date, and notes.
    /// </param>
    /// <returns>
    /// The newly created transaction.
    /// </returns>
    /// <response code="200">
    /// Transaction created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid transaction data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateTransactionDto dto)
    {
        var transaction =
            await _transactionService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            transaction);
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    /// <param name="id">
    /// The transaction identifier.
    /// </param>
    /// <param name="dto">
    /// Updated transaction information.
    /// </param>
    /// <returns>
    /// The updated transaction.
    /// </returns>
    /// <response code="200">
    /// Transaction updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid transaction data.
    /// </response>
    /// <response code="404">
    /// Transaction not found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, UpdateTransactionDto dto)
    {
        var transaction =
            await _transactionService.UpdateAsync(id, dto);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    /// <param name="id">
    /// The transaction identifier.
    /// </param>
    /// <response code="204">
    /// Transaction deleted successfully.
    /// </response>
    /// <response code="404">
    /// Transaction not found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _transactionService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated list of transactions.
    /// </summary>
    /// <remarks>
    /// Supports:
    ///
    /// • Pagination
    ///
    /// • Searching
    ///
    /// • Sorting
    ///
    /// • Filtering by account
    ///
    /// • Filtering by category
    ///
    /// • Filtering by transaction type
    ///
    /// • Filtering by date range
    ///
    /// • Filtering by amount range
    ///
    /// Example:
    ///
    /// GET /api/Transactions/paged?page=1&amp;pageSize=20
    /// </remarks>
    /// <param name="query">
    /// Pagination, sorting and filtering options.
    /// </param>
    /// <returns>
    /// A paginated list of transactions.
    /// </returns>
    /// <response code="200">
    /// Transactions retrieved successfully.
    /// </response>
    /// <response code="400">
    /// Invalid query parameters.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] TransactionQueryDto query)
    {
        var result =
            await _transactionService.GetPagedAsync(query);

        return Ok(result);
    }
}