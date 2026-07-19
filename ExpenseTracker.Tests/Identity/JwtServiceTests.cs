using System.IdentityModel.Tokens.Jwt;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Tests.Identity;

public sealed class JwtServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Key = "unit-test-signing-key-with-at-least-32-bytes!",
        Issuer = "ExpenseTracker.Tests",
        Audience = "ExpenseTracker.TestClient",
        DurationInMinutes = 15,
        RefreshTokenDurationInDays = 30
    };

    [Fact]
    public void GenerateAccessToken_IncludesExpectedIdentityAndExpiry()
    {
        var service = CreateService();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);

        var encodedToken = service.GenerateAccessToken(
            "user-123",
            "user@example.test",
            null,
            expiresAtUtc);

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(encodedToken);

        Assert.Equal(Settings.Issuer, token.Issuer);
        Assert.Contains(Settings.Audience, token.Audiences);
        Assert.Contains(token.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Sub &&
            claim.Value == "user-123");
        Assert.Contains(token.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Email &&
            claim.Value == "user@example.test");
        Assert.Equal(
            expiresAtUtc,
            token.ValidTo,
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateAccessToken_SupportsPhoneOnlyIdentity()
    {
        var service = CreateService();

        var encodedToken = service.GenerateAccessToken(
            "mobile-user-123",
            null,
            "+639171234567",
            DateTime.UtcNow.AddMinutes(15));

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(encodedToken);

        Assert.DoesNotContain(token.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Email);
        Assert.Contains(token.Claims, claim =>
            claim.Type == "phone_number" &&
            claim.Value == "+639171234567");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueHighEntropyTokens()
    {
        var service = CreateService();

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.True(first.Length >= 80);
        Assert.True(second.Length >= 80);
    }

    [Fact]
    public void HashRefreshToken_IsDeterministicWithoutStoringPlaintext()
    {
        var service = CreateService();
        var refreshToken = service.GenerateRefreshToken();

        var firstHash = service.HashRefreshToken(refreshToken);
        var secondHash = service.HashRefreshToken(refreshToken);

        Assert.Equal(firstHash, secondHash);
        Assert.Equal(64, firstHash.Length);
        Assert.DoesNotContain(refreshToken, firstHash);
    }

    private static JwtService CreateService()
    {
        return new JwtService(Options.Create(Settings));
    }
}
