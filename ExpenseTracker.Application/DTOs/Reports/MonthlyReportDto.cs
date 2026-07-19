namespace ExpenseTracker.Application.DTOs.Reports;

public class MonthlyReportDto
{
    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal Balance { get; set; }

    public int TransactionCount { get; set; }
}