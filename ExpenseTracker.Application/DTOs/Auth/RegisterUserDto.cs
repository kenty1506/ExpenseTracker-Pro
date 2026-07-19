using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Auth;

public class RegisterUserDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
