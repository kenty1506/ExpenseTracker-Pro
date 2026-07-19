namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetForecastMonthDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal ForecastIncome { get; set; }

    public decimal ForecastExpense { get; set; }

    public decimal PlannedBudget { get; set; }

    public decimal RecommendedBudget { get; set; }

    public decimal ProjectedBudgetRemaining { get; set; }

    public decimal ProjectedNetCashFlow { get; set; }

    public decimal? BudgetUtilizationPercent { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public List<BudgetForecastCategoryDto> Categories { get; set; } = [];
}
