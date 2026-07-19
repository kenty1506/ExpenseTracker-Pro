using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);

    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);

    Task<AuthResponseDto> GoogleSignInAsync(
        GoogleSignInRequestDto dto,
        CancellationToken cancellationToken);

    Task RequestMobileRegistrationCodeAsync(
        MobileRegistrationRequestDto dto,
        CancellationToken cancellationToken);

    Task<AuthResponseDto> VerifyMobileRegistrationAsync(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken);

    Task RequestMobileLoginCodeAsync(
        MobileCodeRequestDto dto,
        CancellationToken cancellationToken);

    Task<AuthResponseDto> VerifyMobileLoginAsync(
        MobileCodeVerificationDto dto,
        CancellationToken cancellationToken);

    Task RequestPasswordResetAsync(
        ForgotPasswordRequestDto dto,
        CancellationToken cancellationToken);

    Task<AuthResponseDto> ResetPasswordAsync(
        ResetPasswordRequestDto dto);

    Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto dto);

    Task LogoutAsync(string userId);

    Task LogoutByRefreshTokenAsync(string refreshToken);
}
