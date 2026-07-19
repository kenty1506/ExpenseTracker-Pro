using System.Globalization;
using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public static class BudgetForecastCalculator
{
    public static BudgetForecastDto Calculate(
        IEnumerable<Budget> budgets,
        IEnumerable<Transaction> transactions,
        IEnumerable<RecurringTransaction> recurringTransactions,
        DateTime nowUtc,
        int months,
        int historyMonths,
        decimal safetyBufferPercent)
    {
        ArgumentNullException.ThrowIfNull(budgets);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(recurringTransactions);

        if (months < 3 || months > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(months),
                "Forecast months must be between 3 and 12.");
        }

        if (historyMonths < 1 || historyMonths > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(historyMonths),
                "History months must be between 1 and 12.");
        }

        if (safetyBufferPercent < 0 || safetyBufferPercent > 50)
        {
            throw new ArgumentOutOfRangeException(
                nameof(safetyBufferPercent),
                "The safety buffer must be between 0 and 50 percent.");
        }

        var generatedAtUtc = nowUtc.Kind == DateTimeKind.Utc
            ? nowUtc
            : nowUtc.ToUniversalTime();

        var currentMonthStart = new DateTime(
            generatedAtUtc.Year,
            generatedAtUtc.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var forecastStart = currentMonthStart.AddMonths(1);
        var forecastEndExclusive = forecastStart.AddMonths(months);
        var historyStart = currentMonthStart.AddMonths(-historyMonths);
        var historyEndExclusive = currentMonthStart;

        var budgetList = budgets.ToList();
        var transactionList = transactions.ToList();
        var recurringList = recurringTransactions
            .Where(recurring => recurring.IsActive)
            .ToList();

        var historicalTransactions = transactionList
            .Where(transaction =>
                transaction.Date >= historyStart &&
                transaction.Date < historyEndExclusive)
            .ToList();

        var historicalExpenses = historicalTransactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .ToList();

        var historicalIncomeAverage = RoundMoney(
            historicalTransactions
                .Where(transaction => transaction.Type == TransactionType.Income)
                .Sum(transaction => transaction.Amount) / historyMonths);

        var historicalMonthsWithData = historicalTransactions
            .Select(transaction => new
            {
                transaction.Date.Year,
                transaction.Date.Month
            })
            .Distinct()
            .Count();

        var categoryIds = historicalExpenses
            .Select(transaction => transaction.CategoryId)
            .Concat(
                recurringList
                    .Where(recurring =>
                        recurring.Type == TransactionType.Expense &&
                        HasOccurrenceInRange(
                            recurring,
                            forecastStart,
                            forecastEndExclusive))
                    .Select(recurring => recurring.CategoryId))
            .Concat(
                budgetList
                    .Where(budget =>
                        ToMonthStart(budget.Year, budget.Month) < forecastEndExclusive)
                    .Select(budget => budget.CategoryId))
            .Distinct()
            .ToList();

        var forecastMonths = Enumerable.Range(0, months)
            .Select(offset =>
            {
                var targetMonth = forecastStart.AddMonths(offset);

                var categories = categoryIds
                    .Select(categoryId => BuildCategoryForecast(
                        categoryId,
                        targetMonth,
                        budgetList,
                        historicalExpenses,
                        recurringList,
                        historyMonths,
                        safetyBufferPercent))
                    .Where(category =>
                        category.HasBudget ||
                        category.ForecastExpense > 0)
                    .OrderByDescending(category => category.ForecastExpense)
                    .ThenBy(category => category.Category)
                    .ToList();

                var recurringIncome = RoundMoney(
                    recurringList
                        .Where(recurring =>
                            recurring.Type == TransactionType.Income &&
                            HasOccurrenceInMonth(recurring, targetMonth))
                        .Sum(recurring => recurring.Amount));

                var forecastIncome = Math.Max(
                    historicalIncomeAverage,
                    recurringIncome);

                var forecastExpense = RoundMoney(
                    categories.Sum(category => category.ForecastExpense));

                var plannedBudget = RoundMoney(
                    categories
                        .Where(category => category.HasBudget)
                        .Sum(category => category.PlannedBudget));

                var recommendedBudget = RoundMoney(
                    categories.Sum(category => category.RecommendedBudget));

                return new BudgetForecastMonthDto
                {
                    Year = targetMonth.Year,
                    Month = targetMonth.Month,
                    MonthName = CultureInfo.InvariantCulture.DateTimeFormat
                        .GetMonthName(targetMonth.Month),
                    ForecastIncome = RoundMoney(forecastIncome),
                    ForecastExpense = forecastExpense,
                    PlannedBudget = plannedBudget,
                    RecommendedBudget = recommendedBudget,
                    ProjectedBudgetRemaining = RoundMoney(
                        plannedBudget - forecastExpense),
                    ProjectedNetCashFlow = RoundMoney(
                        forecastIncome - forecastExpense),
                    BudgetUtilizationPercent = plannedBudget <= 0
                        ? null
                        : RoundPercent(forecastExpense / plannedBudget * 100),
                    RiskLevel = GetMonthRisk(categories, forecastExpense),
                    Categories = categories
                };
            })
            .ToList();

        var warnings = new List<string>();

        if (historicalTransactions.Count == 0)
        {
            warnings.Add(
                "No completed-month transaction history was found. The forecast relies on recurring transactions and saved budgets.");
        }
        else if (historicalMonthsWithData < historyMonths)
        {
            warnings.Add(
                $"Only {historicalMonthsWithData} of {historyMonths} history months contain transactions; months with no activity are included as zero.");
        }

        if (forecastMonths.Any(month =>
            month.Categories.Any(category => !category.HasBudget)))
        {
            warnings.Add(
                "Some forecast categories do not have a saved budget. Their recommended amounts can be used as budget suggestions.");
        }

        warnings.Add(
            "Forecast values are planning estimates and may differ from actual income and expenses.");

        return new BudgetForecastDto
        {
            GeneratedAtUtc = generatedAtUtc,
            ForecastStart = forecastStart,
            ForecastMonths = months,
            HistoryStart = historyStart,
            HistoryEndExclusive = historyEndExclusive,
            HistoryMonths = historyMonths,
            HistoricalMonthsWithData = historicalMonthsWithData,
            SafetyBufferPercent = safetyBufferPercent,
            Methodology =
                "Uses completed-month averages and active monthly recurring transactions. " +
                "For each category, the larger value becomes the forecast so recurring entries already present in history are not counted twice. " +
                "Exact monthly budgets are preferred, otherwise the latest earlier budget is carried forward. " +
                "Recommended budgets add the requested safety buffer.",
            Warnings = warnings,
            Months = forecastMonths
        };
    }

    private static BudgetForecastCategoryDto BuildCategoryForecast(
        int categoryId,
        DateTime targetMonth,
        IReadOnlyCollection<Budget> budgets,
        IReadOnlyCollection<Transaction> historicalExpenses,
        IReadOnlyCollection<RecurringTransaction> recurringTransactions,
        int historyMonths,
        decimal safetyBufferPercent)
    {
        var applicableBudget = budgets
            .Where(budget =>
                budget.CategoryId == categoryId &&
                ToMonthStart(budget.Year, budget.Month) <= targetMonth)
            .OrderByDescending(budget => budget.Year)
            .ThenByDescending(budget => budget.Month)
            .FirstOrDefault();

        var hasExplicitBudget = applicableBudget != null &&
            applicableBudget.Year == targetMonth.Year &&
            applicableBudget.Month == targetMonth.Month;

        var categoryExpenses = historicalExpenses
            .Where(transaction => transaction.CategoryId == categoryId)
            .ToList();

        var categoryRecurring = recurringTransactions
            .Where(recurring =>
                recurring.CategoryId == categoryId &&
                recurring.Type == TransactionType.Expense)
            .ToList();

        var historicalAverage = RoundMoney(
            categoryExpenses.Sum(transaction => transaction.Amount) / historyMonths);

        var recurringCommitted = RoundMoney(
            categoryRecurring
                .Where(recurring => HasOccurrenceInMonth(recurring, targetMonth))
                .Sum(recurring => recurring.Amount));

        var forecastExpense = Math.Max(
            historicalAverage,
            recurringCommitted);

        var recommendedBudget = RoundMoney(
            forecastExpense * (1 + safetyBufferPercent / 100));

        var plannedBudget = applicableBudget?.Amount ?? 0;

        var category = applicableBudget?.Category ??
            categoryRecurring.FirstOrDefault()?.Category ??
            categoryExpenses.FirstOrDefault()?.Category;

        return new BudgetForecastCategoryDto
        {
            CategoryId = categoryId,
            Category = category?.Name ?? "Uncategorized",
            Color = category?.Color ?? "#808080",
            Icon = category?.Icon ?? "category",
            BudgetId = applicableBudget?.Id,
            HasBudget = applicableBudget != null,
            BudgetSource = applicableBudget == null
                ? "Suggested"
                : hasExplicitBudget
                    ? "Explicit"
                    : "Carried Forward",
            PlannedBudget = plannedBudget,
            HistoricalAverageExpense = historicalAverage,
            RecurringCommittedExpense = recurringCommitted,
            ForecastExpense = forecastExpense,
            RecommendedBudget = recommendedBudget,
            ProjectedRemaining = RoundMoney(plannedBudget - forecastExpense),
            UtilizationPercent = applicableBudget == null || plannedBudget <= 0
                ? null
                : RoundPercent(forecastExpense / plannedBudget * 100),
            RiskLevel = GetCategoryRisk(applicableBudget != null, plannedBudget, forecastExpense)
        };
    }

    private static bool HasOccurrenceInRange(
        RecurringTransaction recurring,
        DateTime rangeStart,
        DateTime rangeEndExclusive)
    {
        for (var month = rangeStart; month < rangeEndExclusive; month = month.AddMonths(1))
        {
            if (HasOccurrenceInMonth(recurring, month))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasOccurrenceInMonth(
        RecurringTransaction recurring,
        DateTime monthStart)
    {
        var occurrenceDay = Math.Min(
            Math.Max(recurring.DayOfMonth, 1),
            DateTime.DaysInMonth(monthStart.Year, monthStart.Month));

        var occurrenceDate = new DateTime(
            monthStart.Year,
            monthStart.Month,
            occurrenceDay,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var effectiveStart = recurring.StartDate.Date;

        if (recurring.NextRunDate != default &&
            recurring.NextRunDate.Date > effectiveStart)
        {
            effectiveStart = recurring.NextRunDate.Date;
        }

        return occurrenceDate >= effectiveStart &&
            (!recurring.EndDate.HasValue ||
             occurrenceDate <= recurring.EndDate.Value.Date);
    }

    private static DateTime ToMonthStart(int year, int month)
    {
        return new DateTime(
            year,
            month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }

    private static string GetCategoryRisk(
        bool hasBudget,
        decimal plannedBudget,
        decimal forecastExpense)
    {
        if (forecastExpense <= 0)
        {
            return "No Activity";
        }

        if (!hasBudget || plannedBudget <= 0)
        {
            return "Unbudgeted";
        }

        var utilization = forecastExpense / plannedBudget * 100;

        return utilization switch
        {
            > 100 => "Over Budget",
            > 80 => "Warning",
            _ => "On Track"
        };
    }

    private static string GetMonthRisk(
        IReadOnlyCollection<BudgetForecastCategoryDto> categories,
        decimal forecastExpense)
    {
        if (forecastExpense <= 0)
        {
            return "No Activity";
        }

        if (categories.Any(category => category.RiskLevel == "Over Budget"))
        {
            return "Over Budget";
        }

        if (categories.Any(category => category.RiskLevel == "Unbudgeted"))
        {
            return "Unbudgeted";
        }

        if (categories.Any(category => category.RiskLevel == "Warning"))
        {
            return "Warning";
        }

        return "On Track";
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundPercent(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
