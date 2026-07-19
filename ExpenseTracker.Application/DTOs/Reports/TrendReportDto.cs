namespace ExpenseTracker.Application.DTOs.Reports;

public class TrendReportDto
{
    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public decimal Balance { get; set; }

    public int TransactionCount { get; set; }
}