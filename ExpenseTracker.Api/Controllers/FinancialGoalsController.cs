using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinancialGoalsController : ControllerBase
{
    private readonly IFinancialGoalService _financialGoalService;

    public FinancialGoalsController(
        IFinancialGoalService financialGoalService)
    {
        _financialGoalService = financialGoalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var goals = await _financialGoalService.GetAllAsync();
        return Ok(goals);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var goal = await _financialGoalService.GetByIdAsync(id);

        if (goal == null)
            return NotFound();

        return Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateFinancialGoalDto dto)
    {
        var goal =
            await _financialGoalService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = goal.Id },
            goal);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateFinancialGoalDto dto)
    {
        var goal =
            await _financialGoalService.UpdateAsync(id, dto);

        if (goal == null)
            return NotFound();

        return Ok(goal);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _financialGoalService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:int}/contributions")]
    public async Task<IActionResult> AddContribution(
        int id,
        AddGoalContributionDto dto)
    {
        var contribution =
            await _financialGoalService
                .AddContributionAsync(id, dto);

        if (contribution == null)
            return NotFound();

        return Ok(contribution);
    }

    [HttpDelete("{goalId:int}/contributions/{contributionId:int}")]
    public async Task<IActionResult> DeleteContribution(
        int goalId,
        int contributionId)
    {
        var deleted =
            await _financialGoalService
                .DeleteContributionAsync(
                    goalId,
                    contributionId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary =
            await _financialGoalService.GetSummaryAsync();

        return Ok(summary);
    }
}