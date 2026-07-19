namespace ExpenseTracker.Application.DTOs.Reports;

public class ExpenseCalendarDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEndExclusive { get; set; }

    public decimal TotalExpense { get; set; }

    public int TransactionCount { get; set; }

    public int SpendingDays { get; set; }

    public decimal AveragePerSpendingDay { get; set; }

    public decimal AveragePerCalendarDay { get; set; }

    public decimal LargestDailyExpense { get; set; }

    public int PreviousYear { get; set; }

    public int PreviousMonth { get; set; }

    public int NextYear { get; set; }

    public int NextMonth { get; set; }

    public List<CalendarSpendingDto> Days { get; set; } = [];
}
