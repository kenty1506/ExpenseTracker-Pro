using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Accounts;

public class AccountQueryDto : PagedQuery
{
    public AccountType? Type { get; set; }

    public bool? IsActive { get; set; }

    public bool? IncludeInNetWorth { get; set; }

    public decimal? MinBalance { get; set; }

    public decimal? MaxBalance { get; set; }

    public string? Currency { get; set; }

    public string? Search { get; set; }
}