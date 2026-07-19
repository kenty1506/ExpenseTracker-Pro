using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public sealed class MobileCodeRequestDto
{
    [Required]
    [RegularExpression(
        @"^\+[1-9]\d{7,14}$",
        ErrorMessage =
            "Use E.164 format, for example +639171234567.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
