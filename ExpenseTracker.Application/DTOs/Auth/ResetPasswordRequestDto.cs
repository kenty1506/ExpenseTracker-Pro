using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public sealed class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(4096)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
