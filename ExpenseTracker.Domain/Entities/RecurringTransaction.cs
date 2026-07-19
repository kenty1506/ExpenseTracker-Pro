using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class RecurringTransaction : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
    public int AccountId { get; set; }

    public Account? Account { get; set; }

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int DayOfMonth { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime NextRunDate { get; set; }

    public DateTime? LastRunDate { get; set; }

    public List<Transaction> GeneratedTransactions { get; set; } = new();
}