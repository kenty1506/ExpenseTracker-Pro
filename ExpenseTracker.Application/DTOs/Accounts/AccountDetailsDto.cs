using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Accounts;

public class AccountDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal IncomingTransfers { get; set; }

    public decimal OutgoingTransfers { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool IncludeInNetWorth { get; set; }

    public bool IsActive { get; set; }

    public int TransactionCount { get; set; }

    public List<AccountTransferActivityDto> RecentTransfers { get; set; } = [];
}
