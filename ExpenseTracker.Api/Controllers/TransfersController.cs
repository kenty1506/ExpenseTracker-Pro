using ExpenseTracker.Application.DTOs.Transfers;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(
        ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transfers =
            await _transferService.GetAllAsync();

        return Ok(transfers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer =
            await _transferService.GetByIdAsync(id);

        if (transfer == null)
            return NotFound();

        return Ok(transfer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTransferDto dto)
    {
        var transfer =
            await _transferService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transfer.Id },
            transfer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTransferDto dto)
    {
        var transfer =
            await _transferService.UpdateAsync(
                id,
                dto);

        if (transfer == null)
            return NotFound();

        return Ok(transfer);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _transferService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
    [FromQuery] TransferQueryDto query)
    {
        var result =
            await _transferService.GetPagedAsync(query);

        return Ok(result);
    }
}