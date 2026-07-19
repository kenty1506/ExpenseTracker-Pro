using System.Security.Cryptography;
using System.Text;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string GoogleLoginProvider = "Google";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IGoogleIdentityVerifier _googleIdentityVerifier;
    private readonly IMobileVerificationService _mobileVerificationService;
    private readonly IPasswordResetEmailSender _passwordResetEmailSender;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IGoogleIdentityVerifier googleIdentityVerifier,
        IMobileVerificationService mobileVerificationService,
        IPasswordResetEmailSender passwordResetEmailSender,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _googleIdentityVerifier = googleIdentityVerifier;
        _mobileVerificationService = mobileVerificationService;
        _passwordResetEmailSender = passwordResetEmailSender;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
    {
        var normalizedEmail = dto.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "An account with this email already exists."
            };
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return IdentityFailure(result);
        }

        var response = await IssueTokenPairAsync(user);
        response.Message = "Registration successful.";
        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (user == null || await _userManager.IsLockedOutAsync(user))
        {
            return InvalidCredentials();
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            dto.Password);

        if (!passwordIsValid)
        {
            await _userManager.AccessFailedAsync(user);
            return InvalidCredentials();
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return await IssueTokenPairAsync(user);
    }

    public async Task<AuthResponseDto> GoogleSignInAsync(
        GoogleSignInRequestDto dto,
        CancellationToken cancellationToken)
    {
        var identity = await _googleIdentityVerifier.VerifyAsync(
            dto.IdToken.Trim(),
            cancellationToken);

        if (identity is null)
        {
            return ExternalLoginFailure(
                "The Google sign-in token is invalid or expired.");
        }

        var user = await _userManager.FindByLoginAsync(
            GoogleLoginProvider,
            identity.Subject);

        if (user is null)
        {
            var emailUser = await _userManager.FindByEmailAsync(identity.Email);

            if (emailUser is not null)
            {
                return ExternalLoginFailure(
                    "This email is already registered. Sign in using its " +
                    "existing method before linking Google.");
            }

            user = new ApplicationUser
            {
                FullName = Truncate(identity.FullName, 150),
                Email = identity.Email,
                UserName = identity.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return ExternalLoginFailure(
                    "The Google account could not be registered.");
            }

            var loginResult = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo(
                    GoogleLoginProvider,
                    identity.Subject,
                    "Google"));

            if (!loginResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return ExternalLoginFailure(
                    "The Google account could not be registered.");
            }
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return ExternalLoginFailure(
                "The account is temporarily locked. Try again later.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokenPairAsync(user);
    }

    public async Task RequestMobileRegistrationCodeAsync(
        MobileRegistrationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var phoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
        var user = await FindByPhoneNumberAsync(phoneNumber);

        if (user is not null && user.PhoneNumberConfirmed)
        {
            return;
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                FullName = dto.FullName.Trim(),
                UserName = CreateMobileUserName(phoneNumber),
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "The mobile registration could not be started.");
            }
        }
        else if (!string.Equals(
                     user.FullName,
                     dto.FullName.Trim(),
                     StringComparison.Ordinal))
        {
            user.FullName = dto.FullName.Trim();
            EnsureIdentityUpdateSucceeded(
                await _userManager.UpdateAsync(user),
                "The mobile registration could not be updated.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return;
        }

        await _mobileVerificationService.StartAsync(
            phoneNumber,
            cancellationToken);
    }

    public async Task<AuthResponseDto> VerifyMobileRegistrationAsync(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken)
    {
        var phoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
        var user = await FindByPhoneNumberAsync(phoneNumber);

        if (user is null ||
            user.PhoneNumberConfirmed ||
            await _userManager.IsLockedOutAsync(user))
        {
            return InvalidMobileCode();
        }

        var approved = await _mobileVerificationService.CheckAsync(
            phoneNumber,
            dto.Code.Trim(),
            cancellationToken);

        if (!approved)
        {
            await _userManager.AccessFailedAsync(user);
            return InvalidMobileCode();
        }

        user.PhoneNumberConfirmed = true;
        EnsureIdentityUpdateSucceeded(
            await _userManager.UpdateAsync(user),
            "The mobile account could not be activated.");

        await _userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokenPairAsync(user);
    }

    public async Task RequestMobileLoginCodeAsync(
        MobileCodeRequestDto dto,
        CancellationToken cancellationToken)
    {
        var phoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
        var user = await FindByPhoneNumberAsync(phoneNumber);

        if (user is null ||
            !user.PhoneNumberConfirmed ||
            await _userManager.IsLockedOutAsync(user))
        {
            return;
        }

        await _mobileVerificationService.StartAsync(
            phoneNumber,
            cancellationToken);
    }

    public async Task<AuthResponseDto> VerifyMobileLoginAsync(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken)
    {
        var phoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
        var user = await FindByPhoneNumberAsync(phoneNumber);

        if (user is null ||
            !user.PhoneNumberConfirmed ||
            await _userManager.IsLockedOutAsync(user))
        {
            return InvalidMobileCode();
        }

        var approved = await _mobileVerificationService.CheckAsync(
            phoneNumber,
            dto.Code.Trim(),
            cancellationToken);

        if (!approved)
        {
            await _userManager.AccessFailedAsync(user);
            return InvalidMobileCode();
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokenPairAsync(user);
    }

    public async Task RequestPasswordResetAsync(
        ForgotPasswordRequestDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = EncodeToken(resetToken);

        try
        {
            await _passwordResetEmailSender.SendAsync(
                user.Email,
                user.FullName,
                encodedToken,
                cancellationToken);
        }
        catch (ExternalAuthenticationProviderException exception)
        {
            // Preserve the same response for existing and unknown accounts.
            // Provider details are kept in server logs without email or token data.
            _logger.LogWarning(
                exception,
                "Password reset email delivery failed.");
        }
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        var resetToken = DecodeToken(dto.Token.Trim());

        if (user is null || resetToken is null)
        {
            return InvalidPasswordReset();
        }

        var resetResult = await _userManager.ResetPasswordAsync(
            user,
            resetToken,
            dto.NewPassword);

        if (!resetResult.Succeeded)
        {
            return InvalidPasswordReset();
        }

        user.RefreshTokenHash = null;
        user.RefreshTokenCreatedAtUtc = null;
        user.RefreshTokenExpiresAtUtc = null;

        EnsureIdentityUpdateSucceeded(
            await _userManager.UpdateAsync(user),
            "The existing sessions could not be revoked.");

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password reset successful. Sign in with your new password.",
            AuditUserId = user.Id
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto dto)
    {
        var refreshTokenHash = _jwtService.HashRefreshToken(
            dto.RefreshToken.Trim());

        var user = await _userManager.Users.SingleOrDefaultAsync(candidate =>
            candidate.RefreshTokenHash == refreshTokenHash);

        if (user is null ||
            user.RefreshTokenExpiresAtUtc is null ||
            user.RefreshTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "The refresh token is invalid or expired."
            };
        }

        return await IssueTokenPairAsync(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return;
        }

        user.RefreshTokenHash = null;
        user.RefreshTokenCreatedAtUtc = null;
        user.RefreshTokenExpiresAtUtc = null;

        EnsureIdentityUpdateSucceeded(
            await _userManager.UpdateAsync(user),
            "The session could not be revoked.");
    }

    public async Task LogoutByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var refreshTokenHash = _jwtService.HashRefreshToken(
            refreshToken.Trim());

        var user = await _userManager.Users.SingleOrDefaultAsync(candidate =>
            candidate.RefreshTokenHash == refreshTokenHash);

        if (user is null)
        {
            return;
        }

        user.RefreshTokenHash = null;
        user.RefreshTokenCreatedAtUtc = null;
        user.RefreshTokenExpiresAtUtc = null;

        EnsureIdentityUpdateSucceeded(
            await _userManager.UpdateAsync(user),
            "The session could not be revoked.");
    }

    private async Task<AuthResponseDto> IssueTokenPairAsync(
        ApplicationUser user)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAtUtc = now.AddMinutes(
            _jwtSettings.DurationInMinutes);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        user.RefreshTokenCreatedAtUtc = now;
        user.RefreshTokenExpiresAtUtc = now.AddDays(
            _jwtSettings.RefreshTokenDurationInDays);

        EnsureIdentityUpdateSucceeded(
            await _userManager.UpdateAsync(user),
            "The authentication session could not be created.");

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.PhoneNumber,
            accessTokenExpiresAtUtc);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Login successful.",
            Token = accessToken,
            TokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            AuditUserId = user.Id
        };
    }

    private Task<ApplicationUser?> FindByPhoneNumberAsync(
        string phoneNumber)
    {
        return _userManager.Users.SingleOrDefaultAsync(user =>
            user.PhoneNumber == phoneNumber);
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return phoneNumber.Trim();
    }

    private static string CreateMobileUserName(string phoneNumber)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(phoneNumber));
        return $"mobile_{Convert.ToHexString(hash)[..24].ToLowerInvariant()}";
    }

    private static string EncodeToken(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string? DecodeToken(string encodedToken)
    {
        try
        {
            var base64 = encodedToken
                .Replace('-', '+')
                .Replace('_', '/');

            base64 = base64.PadRight(
                base64.Length + ((4 - base64.Length % 4) % 4),
                '=');

            return Encoding.UTF8.GetString(
                Convert.FromBase64String(base64));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string Truncate(string value, int maximumLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maximumLength
            ? trimmed
            : trimmed[..maximumLength];
    }

    private static void EnsureIdentityUpdateSucceeded(
        IdentityResult result,
        string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static AuthResponseDto IdentityFailure(IdentityResult result)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = string.Join(
                "; ",
                result.Errors.Select(error => error.Description))
        };
    }

    private static AuthResponseDto InvalidCredentials()
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Invalid email or password."
        };
    }

    private static AuthResponseDto InvalidMobileCode()
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "The verification code is invalid or expired."
        };
    }

    private static AuthResponseDto InvalidPasswordReset()
    {
        return new AuthResponseDto
        {
            Success = false,
            Message =
                "The reset link is invalid or expired, or the new password " +
                "does not meet the password requirements."
        };
    }

    private static AuthResponseDto ExternalLoginFailure(string message)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = message
        };
    }
}
