namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetForecastDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public DateTime ForecastStart { get; set; }

    public int ForecastMonths { get; set; }

    public DateTime HistoryStart { get; set; }

    public DateTime HistoryEndExclusive { get; set; }

    public int HistoryMonths { get; set; }

    public int HistoricalMonthsWithData { get; set; }

    public decimal SafetyBufferPercent { get; set; }

    public string Methodology { get; set; } = string.Empty;

    public List<string> Warnings { get; set; } = [];

    public List<BudgetForecastMonthDto> Months { get; set; } = [];
}
