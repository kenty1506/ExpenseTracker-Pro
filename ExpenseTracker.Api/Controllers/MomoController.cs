using Asp.Versioning;
using ExpenseTracker.Api.DTOs.Momo;
using ExpenseTracker.Api.Services.Momo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Provides authenticated, context-aware MoMo finance-assistant responses.
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("MoMo")]
[EnableRateLimiting("momo")]
public sealed class MomoController : ControllerBase
{
    private readonly IMomoAssistantService _momoAssistantService;

    public MomoController(
        IMomoAssistantService momoAssistantService)
    {
        _momoAssistantService = momoAssistantService;
    }

    /// <summary>
    /// Answers a finance question using the authenticated user's current data.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType<MomoChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MomoChatResponse>> Chat(
        [FromBody] MomoChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _momoAssistantService.ChatAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
}
