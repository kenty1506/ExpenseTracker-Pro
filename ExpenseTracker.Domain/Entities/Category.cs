using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Category : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#6366F1";

    public string Icon { get; set; } = "category";

    public List<Transaction> Transactions { get; set; } = new();

    public List<Budget> Budgets { get; set; } = new();

    public List<RecurringTransaction> RecurringTransactions { get; set; } = new();

}