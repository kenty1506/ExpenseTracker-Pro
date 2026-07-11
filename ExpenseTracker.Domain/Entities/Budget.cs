using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Budget : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Amount { get; set; }
}