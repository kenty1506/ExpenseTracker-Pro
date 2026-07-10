using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Transaction : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;
}