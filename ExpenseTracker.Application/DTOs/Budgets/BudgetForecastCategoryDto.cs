namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetForecastCategoryDto
{
    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int? BudgetId { get; set; }

    public bool HasBudget { get; set; }

    public string BudgetSource { get; set; } = string.Empty;

    public decimal PlannedBudget { get; set; }

    public decimal HistoricalAverageExpense { get; set; }

    public decimal RecurringCommittedExpense { get; set; }

    public decimal ForecastExpense { get; set; }

    public decimal RecommendedBudget { get; set; }

    public decimal ProjectedRemaining { get; set; }

    public decimal? UtilizationPercent { get; set; }

    public string RiskLevel { get; set; } = string.Empty;
}
