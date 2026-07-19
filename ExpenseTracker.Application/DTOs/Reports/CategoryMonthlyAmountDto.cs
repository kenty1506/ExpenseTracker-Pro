namespace ExpenseTracker.Application.DTOs.Reports;

public class CategoryMonthlyAmountDto
{
    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int TransactionCount { get; set; }
}