using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

public class Transaction : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int? AccountId { get; set; }

    public Account? Account { get; set; }

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int? RecurringTransactionId { get; set; }

    public RecurringTransaction? RecurringTransaction { get; set; }
    public ICollection<GoalContribution> GoalContributions
    { get; set; } = new List<GoalContribution>();
}