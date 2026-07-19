using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Tests.Validation;

public sealed class AuthDtoValidationTests
{
    [Fact]
    public void Register_RejectsInvalidEmailAndShortPassword()
    {
        var dto = new RegisterUserDto
        {
            FullName = "Test User",
            Email = "invalid-email",
            Password = "Short1!"
        };

        var errors = Validate(dto);

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(dto.Email)));
        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(dto.Password)));
    }

    [Fact]
    public void Login_RequiresEmailAndPassword()
    {
        var errors = Validate(new LoginUserDto());

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginUserDto.Email)));
        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginUserDto.Password)));
    }

    [Fact]
    public void Refresh_RequiresToken()
    {
        var errors = Validate(new RefreshTokenRequestDto());

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(RefreshTokenRequestDto.RefreshToken)));
    }

    [Fact]
    public void GoogleSignIn_RequiresIdToken()
    {
        var errors = Validate(new GoogleSignInRequestDto());

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(GoogleSignInRequestDto.IdToken)));
    }

    [Theory]
    [InlineData("09171234567")]
    [InlineData("+0123456789")]
    [InlineData("+63 917 123 4567")]
    public void MobileRegistration_RequiresE164PhoneNumber(
        string phoneNumber)
    {
        var errors = Validate(new MobileRegistrationRequestDto
        {
            FullName = "Mobile Test User",
            PhoneNumber = phoneNumber
        });

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(MobileRegistrationRequestDto.PhoneNumber)));
    }

    [Fact]
    public void MobileCode_AcceptsE164PhoneAndNumericCode()
    {
        var errors = Validate(new MobileCodeVerificationDto
        {
            PhoneNumber = "+639171234567",
            Code = "123456"
        });

        Assert.Empty(errors);
    }

    [Fact]
    public void ResetPassword_RequiresValidEmailTokenAndStrongLength()
    {
        var errors = Validate(new ResetPasswordRequestDto
        {
            Email = "invalid-email",
            Token = string.Empty,
            NewPassword = "Short1!"
        });

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(ResetPasswordRequestDto.Email)));
        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(ResetPasswordRequestDto.Token)));
        Assert.Contains(errors, error =>
            error.MemberNames.Contains(
                nameof(ResetPasswordRequestDto.NewPassword)));
    }

    [Fact]
    public void AuthResponse_DoesNotExposeInternalAuditUserId()
    {
        var response = new AuthResponseDto
        {
            Success = true,
            AuditUserId = "internal-user-id"
        };

        var json = JsonSerializer.Serialize(response);

        Assert.False(json.Contains(
            "internal-user-id",
            StringComparison.Ordinal));
        Assert.False(json.Contains(
            "AuditUserId",
            StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);

        return results;
    }
}
