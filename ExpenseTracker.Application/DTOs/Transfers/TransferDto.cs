namespace ExpenseTracker.Application.DTOs.Transfers;

public class TransferDto
{
    public int Id { get; set; }

    public int FromAccountId { get; set; }

    public string FromAccount { get; set; } = string.Empty;

    public int ToAccountId { get; set; }

    public string ToAccount { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}