using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts =
            await _accountService.GetAllAsync();

        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account =
            await _accountService.GetByIdAsync(id);

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAccountDto dto)
    {
        var account =
            await _accountService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            account);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAccountDto dto)
    {
        var account =
            await _accountService.UpdateAsync(
                id,
                dto);

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _accountService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary =
            await _accountService.GetSummaryAsync();

        return Ok(summary);
    }
}