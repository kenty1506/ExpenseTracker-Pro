using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Api.Middleware;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ExpenseTracker.Infrastructure.Identity;

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
    private const string WebRefreshCookieName =
        "expense-tracker-refresh";

    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthService authService,
        IWebHostEnvironment environment,
        IOptions<JwtSettings> jwtOptions)
    {
        _authService = authService;
        _environment = environment;
        _jwtSettings = jwtOptions.Value;
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
        AttachWebSession(result);

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
        AttachWebSession(result);

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
        AttachWebSession(result);

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
        AttachWebSession(result);

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
        AttachWebSession(result);

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
    /// Restores a browser session from its HTTP-only refresh cookie and
    /// rotates the refresh token without exposing it to JavaScript.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("web/refresh")]
    [EnableRateLimiting("token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshWebSession()
    {
        if (!Request.Cookies.TryGetValue(
                WebRefreshCookieName,
                out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            DeleteWebSessionCookie();

            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "The browser session is unavailable or expired."
            });
        }

        var result = await _authService.RefreshTokenAsync(
            new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            });

        AttachAuditUser(result);

        if (!result.Success)
        {
            DeleteWebSessionCookie();
            return Unauthorized(result);
        }

        WriteWebSessionCookie(result.RefreshToken);
        result.RefreshToken = string.Empty;

        return Ok(result);
    }

    /// <summary>
    /// Revokes the browser refresh session and removes its HTTP-only cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("web/logout")]
    [EnableRateLimiting("token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutWebSession()
    {
        if (Request.Cookies.TryGetValue(
                WebRefreshCookieName,
                out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutByRefreshTokenAsync(refreshToken);
        }

        DeleteWebSessionCookie();
        return NoContent();
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

    private void AttachWebSession(AuthResponseDto result)
    {
        if (!IsWebClient() ||
            !result.Success ||
            string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            return;
        }

        WriteWebSessionCookie(result.RefreshToken);
        result.RefreshToken = string.Empty;
    }

    private bool IsWebClient()
    {
        return Request.Headers.TryGetValue(
                   "X-ExpenseTracker-Client",
                   out var client) &&
               string.Equals(
                   client.ToString(),
                   "Web",
                   StringComparison.OrdinalIgnoreCase);
    }

    private void WriteWebSessionCookie(string refreshToken)
    {
        Response.Cookies.Append(
            WebRefreshCookieName,
            refreshToken,
            CreateWebCookieOptions());
    }

    private void DeleteWebSessionCookie()
    {
        Response.Cookies.Delete(
            WebRefreshCookieName,
            CreateWebCookieOptions());
    }

    private CookieOptions CreateWebCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment() || Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/api/v1/Auth/web",
            MaxAge = TimeSpan.FromDays(
                _jwtSettings.RefreshTokenDurationInDays)
        };
    }
}
