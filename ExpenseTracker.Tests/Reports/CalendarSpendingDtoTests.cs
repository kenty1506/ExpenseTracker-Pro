using ExpenseTracker.Application.DTOs.Reports;

namespace ExpenseTracker.Tests.Reports;

public class CalendarSpendingDtoTests
{
    [Fact]
    public void HasSpending_IsTrueWhenDailyExpenseIsPositive()
    {
        var day = new CalendarSpendingDto
        {
            TotalExpense = 125.50m,
            TransactionCount = 1
        };

        Assert.True(day.HasSpending);
    }

    [Fact]
    public void HasSpending_IsFalseForEmptyCalendarDay()
    {
        var day = new CalendarSpendingDto();

        Assert.False(day.HasSpending);
        Assert.Empty(day.Transactions);
    }
}
