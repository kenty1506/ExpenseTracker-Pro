using ExpenseTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Accounts;

public class CreateAccountDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    public decimal OpeningBalance { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "PHP";

    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = "#6366F1";

    [Required]
    [MaxLength(100)]
    public string Icon { get; set; } = "account_balance_wallet";

    public bool IncludeInNetWorth { get; set; } = true;
}