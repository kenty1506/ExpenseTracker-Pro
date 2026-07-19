using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Tests.Identity;

public sealed class WebSessionControllerTests
{
    [Fact]
    public async Task Login_ForWebClient_MovesRefreshTokenIntoHttpOnlyCookie()
    {
        var authService = new AuthServiceStub
        {
            LoginResponse = Success("access-token", "refresh-token")
        };
        var controller = CreateController(authService);
        controller.Request.Headers["X-ExpenseTracker-Client"] = "Web";

        var result = await controller.Login(new LoginUserDto
        {
            Email = "user@example.com",
            Password = "Password123!"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(ok.Value);
        var cookie = controller.Response.Headers.SetCookie.ToString();

        Assert.Empty(response.RefreshToken);
        Assert.Contains("expense-tracker-refresh=refresh-token", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/v1/Auth/web", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshWebSession_RotatesCookieWithoutReturningRefreshToken()
    {
        var authService = new AuthServiceStub
        {
            RefreshResponse = Success("new-access-token", "rotated-refresh-token")
        };
        var controller = CreateController(authService);
        controller.Request.Headers.Cookie =
            "expense-tracker-refresh=original-refresh-token";

        var result = await controller.RefreshWebSession();

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(ok.Value);
        var cookie = controller.Response.Headers.SetCookie.ToString();

        Assert.Equal("original-refresh-token", authService.ReceivedRefreshToken);
        Assert.Empty(response.RefreshToken);
        Assert.Contains("rotated-refresh-token", cookie);
    }

    private static AuthController CreateController(AuthServiceStub authService)
    {
        var controller = new AuthController(
            authService,
            new TestWebHostEnvironment(),
            Options.Create(new JwtSettings
            {
                RefreshTokenDurationInDays = 30
            }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static AuthResponseDto Success(string token, string refreshToken) => new()
    {
        Success = true,
        Message = "Success",
        Token = token,
        RefreshToken = refreshToken
    };

    private sealed class AuthServiceStub : IAuthService
    {
        public AuthResponseDto LoginResponse { get; init; } = new();
        public AuthResponseDto RefreshResponse { get; init; } = new();
        public string? ReceivedRefreshToken { get; private set; }

        public Task<AuthResponseDto> LoginAsync(LoginUserDto dto) =>
            Task.FromResult(LoginResponse);

        public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            ReceivedRefreshToken = dto.RefreshToken;
            return Task.FromResult(RefreshResponse);
        }

        public Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto) => throw new NotSupportedException();
        public Task<AuthResponseDto> GoogleSignInAsync(GoogleSignInRequestDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RequestMobileRegistrationCodeAsync(MobileRegistrationRequestDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AuthResponseDto> VerifyMobileRegistrationAsync(MobileCodeVerificationDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RequestMobileLoginCodeAsync(MobileCodeRequestDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AuthResponseDto> VerifyMobileLoginAsync(MobileCodeVerificationDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RequestPasswordResetAsync(ForgotPasswordRequestDto dto, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto dto) => throw new NotSupportedException();
        public Task LogoutAsync(string userId) => Task.CompletedTask;
        public Task LogoutByRefreshTokenAsync(string refreshToken) => Task.CompletedTask;
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ExpenseTracker.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
