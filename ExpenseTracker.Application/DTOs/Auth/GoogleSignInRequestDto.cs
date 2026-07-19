using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public sealed class GoogleSignInRequestDto
{
    [Required]
    [MaxLength(4096)]
    public string IdToken { get; set; } = string.Empty;
}
