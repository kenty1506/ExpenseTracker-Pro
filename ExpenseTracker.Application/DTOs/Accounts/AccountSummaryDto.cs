namespace ExpenseTracker.Application.DTOs.Accounts;

public class AccountSummaryDto
{
    public decimal TotalAssets { get; set; }

    public decimal TotalLiabilities { get; set; }

    public decimal NetWorth { get; set; }

    public int ActiveAccountCount { get; set; }

    public List<AccountDto> Accounts { get; set; } = [];
}