namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetAlertDto
{
    public int BudgetId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    public decimal Actual { get; set; }

    public decimal Remaining { get; set; }

    public decimal PercentageUsed { get; set; }

    public string AlertLevel { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}