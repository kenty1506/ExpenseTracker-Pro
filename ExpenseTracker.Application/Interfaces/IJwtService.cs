namespace ExpenseTracker.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(
        string userId,
        string? email,
        string? phoneNumber,
        DateTime expiresAtUtc);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
