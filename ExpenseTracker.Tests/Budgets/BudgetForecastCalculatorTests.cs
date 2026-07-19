using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Tests.Budgets;

public class BudgetForecastCalculatorTests
{
    private static readonly DateTime NowUtc = new(
        2026,
        7,
        18,
        12,
        0,
        0,
        DateTimeKind.Utc);

    [Fact]
    public void Calculate_UsesLargerOfHistoricalAverageAndRecurringCommitment()
    {
        var category = CreateCategory(1, "Food");
        var transactions = new List<Transaction>
        {
            CreateTransaction(category, 3_000m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc)),
            CreateTransaction(category, 3_000m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc)),
            CreateTransaction(category, 3_000m, new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc))
        };

        var budgets = new List<Budget>
        {
            new()
            {
                Id = 10,
                CategoryId = category.Id,
                Category = category,
                Year = 2026,
                Month = 8,
                Amount = 4_000m
            }
        };

        var recurring = new List<RecurringTransaction>
        {
            CreateRecurring(category, TransactionType.Expense, 3_500m, 5)
        };

        var result = BudgetForecastCalculator.Calculate(
            budgets,
            transactions,
            recurring,
            NowUtc,
            months: 3,
            historyMonths: 3,
            safetyBufferPercent: 10);

        var august = result.Months[0];
        var food = Assert.Single(august.Categories);

        Assert.Equal(3_000m, food.HistoricalAverageExpense);
        Assert.Equal(3_500m, food.RecurringCommittedExpense);
        Assert.Equal(3_500m, food.ForecastExpense);
        Assert.Equal(3_850m, food.RecommendedBudget);
        Assert.Equal("Explicit", food.BudgetSource);
        Assert.Equal("Warning", food.RiskLevel);
        Assert.Equal(500m, food.ProjectedRemaining);
    }

    [Fact]
    public void Calculate_CarriesEarlierBudgetIntoForecastMonths()
    {
        var category = CreateCategory(2, "Transport");

        var budget = new Budget
        {
            Id = 20,
            CategoryId = category.Id,
            Category = category,
            Year = 2026,
            Month = 6,
            Amount = 2_500m
        };

        var transactions = new List<Transaction>
        {
            CreateTransaction(category, 2_000m, new DateTime(2026, 4, 12, 0, 0, 0, DateTimeKind.Utc)),
            CreateTransaction(category, 2_000m, new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc)),
            CreateTransaction(category, 2_000m, new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc))
        };

        var result = BudgetForecastCalculator.Calculate(
            [budget],
            transactions,
            [],
            NowUtc,
            3,
            3,
            10);

        var categoryForecast = Assert.Single(result.Months[0].Categories);

        Assert.True(categoryForecast.HasBudget);
        Assert.Equal(20, categoryForecast.BudgetId);
        Assert.Equal("Carried Forward", categoryForecast.BudgetSource);
        Assert.Equal(2_500m, categoryForecast.PlannedBudget);
        Assert.Equal("On Track", categoryForecast.RiskLevel);
    }

    [Fact]
    public void Calculate_ReportsUnbudgetedCategoryAndRecurringIncomeFloor()
    {
        var expenseCategory = CreateCategory(3, "Utilities");
        var incomeCategory = CreateCategory(4, "Salary");

        var transactions = new List<Transaction>
        {
            CreateTransaction(expenseCategory, 1_500m, new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc)),
            CreateIncome(incomeCategory, 30_000m, new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc)),
            CreateIncome(incomeCategory, 30_000m, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc)),
            CreateIncome(incomeCategory, 30_000m, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc))
        };

        var recurring = new List<RecurringTransaction>
        {
            CreateRecurring(incomeCategory, TransactionType.Income, 35_000m, 15)
        };

        var result = BudgetForecastCalculator.Calculate(
            [],
            transactions,
            recurring,
            NowUtc,
            3,
            3,
            10);

        var august = result.Months[0];
        var utilities = Assert.Single(august.Categories);

        Assert.Equal(35_000m, august.ForecastIncome);
        Assert.Equal("Unbudgeted", utilities.RiskLevel);
        Assert.Equal("Suggested", utilities.BudgetSource);
        Assert.Null(utilities.UtilizationPercent);
        Assert.Equal("Unbudgeted", august.RiskLevel);
        Assert.Contains(result.Warnings, warning =>
            warning.Contains("do not have a saved budget"));
    }

    private static Category CreateCategory(int id, string name)
    {
        return new Category
        {
            Id = id,
            Name = name,
            Color = "#6366F1",
            Icon = "category"
        };
    }

    private static Transaction CreateTransaction(
        Category category,
        decimal amount,
        DateTime date)
    {
        return new Transaction
        {
            Type = TransactionType.Expense,
            CategoryId = category.Id,
            Category = category,
            Amount = amount,
            Date = date
        };
    }

    private static Transaction CreateIncome(
        Category category,
        decimal amount,
        DateTime date)
    {
        return new Transaction
        {
            Type = TransactionType.Income,
            CategoryId = category.Id,
            Category = category,
            Amount = amount,
            Date = date
        };
    }

    private static RecurringTransaction CreateRecurring(
        Category category,
        TransactionType type,
        decimal amount,
        int dayOfMonth)
    {
        return new RecurringTransaction
        {
            Type = type,
            CategoryId = category.Id,
            Category = category,
            Amount = amount,
            DayOfMonth = dayOfMonth,
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            NextRunDate = new DateTime(2026, 8, dayOfMonth, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
    }
}
