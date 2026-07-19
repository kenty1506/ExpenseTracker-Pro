namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetSummaryDto
{
    public int BudgetId { get; set; }

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    public decimal Actual { get; set; }

    public decimal Remaining { get; set; }

    public decimal PercentageUsed { get; set; }

    public bool IsOverBudget { get; set; }

    public int TransactionCount { get; set; }
}