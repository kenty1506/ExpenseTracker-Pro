namespace ExpenseTracker.Application.DTOs.Reports;

public class DailySpendingDto
{
    public int Day { get; set; }

    public DateTime Date { get; set; }

    public decimal Expense { get; set; }

    public int TransactionCount { get; set; }
}