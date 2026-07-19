namespace ExpenseTracker.Application.DTOs.Accounts;

public class AccountTransferActivityDto
{
    public int TransferId { get; set; }

    public string Direction { get; set; } = string.Empty;

    public int OtherAccountId { get; set; }

    public string OtherAccount { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}