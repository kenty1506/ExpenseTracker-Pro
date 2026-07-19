using Asp.Versioning;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Provides privacy-safe audit history for the authenticated user.
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-trails")]
[Tags("Audit Trails")]
public sealed class AuditTrailsController : ControllerBase
{
    private readonly IAuditTrailService _auditTrailService;

    public AuditTrailsController(
        IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    /// <summary>
    /// Retrieves one consolidated audit trail across all modules.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<AuditTrailDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConsolidated(
        [FromQuery] AuditTrailQueryDto query)
    {
        var result = await _auditTrailService
            .GetConsolidatedAsync(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves audit history for one module.
    /// </summary>
    [HttpGet("modules/{module}")]
    [ProducesResponseType(
        typeof(PagedResult<AuditTrailDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetModule(
        string module,
        [FromQuery] AuditTrailQueryDto query)
    {
        var result = await _auditTrailService
            .GetModuleAsync(module, query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves audit totals grouped by module.
    /// </summary>
    [HttpGet("modules")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AuditModuleSummaryDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetModuleSummary(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var result = await _auditTrailService
            .GetModuleSummaryAsync(fromUtc, toUtc);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves one audit event by identifier.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(
        typeof(AuditTrailDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _auditTrailService.GetByIdAsync(id);

        return result is null
            ? NotFound()
            : Ok(result);
    }
}
