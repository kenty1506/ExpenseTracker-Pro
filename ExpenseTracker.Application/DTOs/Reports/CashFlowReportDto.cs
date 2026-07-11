namespace ExpenseTracker.Application.DTOs.Reports;

public class CashFlowReportDto
{
    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public decimal NetCashFlow { get; set; }

    public decimal ClosingBalance { get; set; }

    public int TransactionCount { get; set; }
}