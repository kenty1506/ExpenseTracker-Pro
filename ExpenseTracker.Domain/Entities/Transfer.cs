using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Transfer : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public int FromAccountId { get; set; }

    public Account? FromAccount { get; set; }

    public int ToAccountId { get; set; }

    public Account? ToAccount { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}