using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);

    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
}