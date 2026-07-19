using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Api.Middleware;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Handles user registration and authentication.
/// </summary>
/// <remarks>
/// Provides public endpoints for creating an account and obtaining a JWT access token.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">
    /// The registration details.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Registration completed successfully.
    /// </response>
    /// <response code="400">
    /// Registration data is invalid or the account already exists.
    /// </response>
    [HttpPost("register")]
    [EnableRateLimiting("authentication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        AttachAuditUser(result);

        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    /// <param name="dto">
    /// The login credentials.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Authentication completed successfully.
    /// </response>
    /// <response code="401">
    /// The credentials are invalid.
    /// </response>
    [HttpPost("login")]
    [EnableRateLimiting("authentication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        AttachAuditUser(result);

        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>
    /// Registers or signs in a user using a Google Identity Services ID token.
    /// </summary>
    [HttpPost("google")]
    [EnableRateLimiting("authentication")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GoogleSignIn(
        GoogleSignInRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.GoogleSignInAsync(
            dto,
            cancellationToken);
        AttachAuditUser(result);

        return result.Success
            ? Ok(result)
            : Unauthorized(result);
    }

    /// <summary>
    /// Starts passwordless mobile registration by sending an SMS code.
    /// </summary>
    [HttpPost("mobile/register/request-code")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestMobileRegistrationCode(
        MobileRegistrationRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.RequestMobileRegistrationCodeAsync(
            dto,
            cancellationToken);

        return Accepted(new
        {
            message =
                "If the mobile number can be used, a verification code " +
                "has been sent."
        });
    }

    /// <summary>
    /// Verifies the SMS code, activates the mobile account, and signs it in.
    /// </summary>
    [HttpPost("mobile/register/verify")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> VerifyMobileRegistration(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyMobileRegistrationAsync(
            dto,
            cancellationToken);
        AttachAuditUser(result);

        return result.Success
            ? Ok(result)
            : Unauthorized(result);
    }

    /// <summary>
    /// Sends an SMS code to a registered mobile account for passwordless login.
    /// </summary>
    [HttpPost("mobile/login/request-code")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestMobileLoginCode(
        MobileCodeRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.RequestMobileLoginCodeAsync(
            dto,
            cancellationToken);

        return Accepted(new
        {
            message =
                "If the mobile number can be used, a verification code " +
                "has been sent."
        });
    }

    /// <summary>
    /// Verifies an SMS code and signs in the registered mobile account.
    /// </summary>
    [HttpPost("mobile/login/verify")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> VerifyMobileLogin(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyMobileLoginAsync(
            dto,
            cancellationToken);
        AttachAuditUser(result);

        return result.Success
            ? Ok(result)
            : Unauthorized(result);
    }

    /// <summary>
    /// Requests a time-limited password-reset email without revealing whether
    /// the email address is registered.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(
            dto,
            cancellationToken);

        return Accepted(new
        {
            message =
                "If an account exists for that email, a password reset " +
                "link has been sent."
        });
    }

    /// <summary>
    /// Applies a valid password-reset token and revokes existing refresh tokens.
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("verification")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        AttachAuditUser(result);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a rotated token pair.
    /// </summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        AttachAuditUser(result);

        return result.Success
            ? Ok(result)
            : Unauthorized(result);
    }

    /// <summary>
    /// Revokes the current user's refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [EnableRateLimiting("token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId);

        return NoContent();
    }

    private void AttachAuditUser(AuthResponseDto result)
    {
        if (!result.Success ||
            string.IsNullOrWhiteSpace(result.AuditUserId))
        {
            return;
        }

        HttpContext.Items[
            AuditLogMiddleware.AuditUserIdItemKey] =
            result.AuditUserId;
    }
}
