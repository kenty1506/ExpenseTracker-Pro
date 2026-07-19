namespace ExpenseTracker.Application.DTOs.Reports;

public class CalendarSpendingDto
{
    public DateTime Date { get; set; }

    public int Day { get; set; }

    public string DayOfWeek { get; set; } = string.Empty;

    public decimal TotalExpense { get; set; }

    public int TransactionCount { get; set; }

    public decimal LargestExpense { get; set; }

    public string? TopCategory { get; set; }

    public bool IsToday { get; set; }

    public List<CalendarExpenseTransactionDto> Transactions { get; set; } = [];

    public bool HasSpending => TotalExpense > 0;
}
