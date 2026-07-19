using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public sealed class RefreshTokenRequestDto
{
    [Required]
    [MaxLength(512)]
    public string RefreshToken { get; set; } = string.Empty;
}
