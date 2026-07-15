using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Transfers;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Manages account-to-account transfers for the authenticated user.
/// </summary>
/// <remarks>
/// Provides transfer CRUD operations, filtering, sorting, searching, and pagination without affecting income or expense totals.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Transfers")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    /// Retrieves all account transfers.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Transfers retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var transfers = await _transferService.GetAllAsync();
        return Ok(transfers);
    }

    /// <summary>
    /// Retrieves a transfer by its identifier.
    /// </summary>
    /// <param name="id">
    /// The transfer identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Transfer found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Transfer not found.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer = await _transferService.GetByIdAsync(id);
        if (transfer == null)
            return NotFound();
        return Ok(transfer);
    }

    /// <summary>
    /// Creates an account-to-account transfer.
    /// </summary>
    /// <param name="dto">
    /// The transfer information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Transfer created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid transfer data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="409">
    /// The transfer conflicts with the current account state.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateTransferDto dto)
    {
        var transfer = await _transferService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),new { id = transfer.Id },transfer);
    }

    /// <summary>
    /// Updates an existing account transfer.
    /// </summary>
    /// <param name="id">
    /// The transfer identifier.
    /// </param>
    /// <param name="dto">
    /// The updated transfer information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Transfer updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid transfer data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Transfer not found.
    /// </response>
    /// <response code="409">
    /// The transfer conflicts with the current account state.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, UpdateTransferDto dto)
    {
        var transfer = await _transferService.UpdateAsync(id,dto);

        if (transfer == null)
            return NotFound();
        return Ok(transfer);
    }

    /// <summary>
    /// Deletes an account transfer.
    /// </summary>
    /// <param name="id">
    /// The transfer identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Transfer deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Transfer not found.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _transferService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of transfers.
    /// </summary>
    /// <remarks>
    /// Supports source and destination account, date range, amount range, search text, and sorting.
    /// </remarks>
    /// <param name="query">
    /// Pagination, filtering, searching, and sorting options.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Transfer page retrieved successfully.
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
    public async Task<IActionResult> GetPaged([FromQuery] TransferQueryDto query)
    {
        var result =await _transferService.GetPagedAsync(query);
        return Ok(result);
    }
}
