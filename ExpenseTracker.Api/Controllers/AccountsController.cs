using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages financial accounts for the authenticated user.
/// </summary>
/// <remarks>
/// Provides endpoints for account creation, retrieval, updates, deletion, summaries, filtering, sorting, and pagination.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Retrieves all accounts.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Accounts were retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _accountService.GetAllAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Retrieves an account by its identifier.
    /// </summary>
    /// <param name="id">
    /// The account identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Account found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Account not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null)
            return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Creates a new financial account.
    /// </summary>
    /// <param name="dto">
    /// The account information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Account created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid account data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="409">
    /// The account conflicts with existing data.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        var account =await _accountService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),new { id = account.Id },account);
    }

    /// <summary>
    /// Updates an existing financial account.
    /// </summary>
    /// <param name="id">
    /// The account identifier.
    /// </param>
    /// <param name="dto">
    /// The updated account information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Account updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid account data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Account not found.
    /// </response>
    /// <response code="409">
    /// The update conflicts with existing data.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id,UpdateAccountDto dto)
    {
        var account =await _accountService.UpdateAsync(id,dto);
        if (account == null)
            return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Deletes a financial account.
    /// </summary>
    /// <param name="id">
    /// The account identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Account deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Account not found.
    /// </response>
    /// <response code="409">
    /// The account cannot be deleted because related data exists.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _accountService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Retrieves the account portfolio summary.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Account summary retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _accountService.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of accounts.
    /// </summary>
    /// <remarks>
    /// Supports filtering by account type, active status, net-worth inclusion, currency, balance range, and search text.
    /// </remarks>
    /// <param name="query">
    /// Pagination, filtering, searching, and sorting options.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Account page retrieved successfully.
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
    [FromQuery] AccountQueryDto query)
    {
        var result = await _accountService.GetPagedAsync(query);
        return Ok(result);
    }
}
