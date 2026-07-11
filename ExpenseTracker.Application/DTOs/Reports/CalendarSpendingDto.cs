namespace ExpenseTracker.Application.DTOs.Reports;

public class CalendarSpendingDto
{
    public DateTime Date { get; set; }

    public int Day { get; set; }

    public decimal TotalExpense { get; set; }

    public int TransactionCount { get; set; }

    public bool HasSpending => TotalExpense > 0;
}