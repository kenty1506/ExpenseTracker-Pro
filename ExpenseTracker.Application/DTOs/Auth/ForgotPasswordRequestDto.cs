using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public sealed class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}
