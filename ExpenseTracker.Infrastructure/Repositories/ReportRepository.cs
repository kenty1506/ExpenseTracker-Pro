using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseTracker.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ExpenseTrackerDbContext _context;

    public ReportRepository(ExpenseTrackerDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<MonthlyReportDto>> GetMonthlyReportAsync(string userId,int year)
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date.Year == year)
            .Select(transaction => new
            {
                transaction.Date,
                transaction.Type,
                transaction.Amount
            })
            .ToListAsync();

        return transactions
            .GroupBy(transaction => transaction.Date.Month)
            .Select(group =>
            {
                var totalIncome = group
                    .Where(transaction =>
                        transaction.Type == TransactionType.Income)
                    .Sum(transaction => transaction.Amount);

                var totalExpense = group
                    .Where(transaction =>
                        transaction.Type == TransactionType.Expense)
                    .Sum(transaction => transaction.Amount);

                return new MonthlyReportDto
                {
                    Month = group.Key,

                    MonthName = CultureInfo
                        .InvariantCulture
                        .DateTimeFormat
                        .GetMonthName(group.Key),

                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    Balance = totalIncome - totalExpense,
                    TransactionCount = group.Count()
                };
            })
            .OrderBy(report => report.Month)
            .ToList();
    }
    public async Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId,int year,int limit)
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == TransactionType.Expense &&
                transaction.Date.Year == year)
            .Select(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category != null
                    ? transaction.Category.Name
                    : "Uncategorized",
                CategoryColor = transaction.Category != null
                    ? transaction.Category.Color
                    : "#808080",
                CategoryIcon = transaction.Category != null
                    ? transaction.Category.Icon
                    : "category",
                transaction.Amount
            })
            .ToListAsync();

        var totalExpenses = transactions.Sum(
            transaction => transaction.Amount);

        if (totalExpenses == 0)
            return [];

        return transactions
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                transaction.CategoryName,
                transaction.CategoryColor,
                transaction.CategoryIcon
            })
            .Select(group =>
            {
                var amount = group.Sum(
                    transaction => transaction.Amount);

                return new TopCategoryDto
                {
                    CategoryId = group.Key.CategoryId,
                    Category = group.Key.CategoryName,
                    Color = group.Key.CategoryColor,
                    Icon = group.Key.CategoryIcon,
                    Amount = amount,
                    Percentage = Math.Round(
                        amount / totalExpenses * 100,
                        2),
                    TransactionCount = group.Count()
                };
            })
            .OrderByDescending(category => category.Amount)
            .Take(limit)
            .ToList();
    }
    public async Task<IEnumerable<TrendReportDto>> GetTrendAsync(string userId, int year)
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date.Year == year)
            .Select(transaction => new
            {
                transaction.Date,
                transaction.Type,
                transaction.Amount
            })
            .ToListAsync();

        var monthlyTotals = transactions
            .GroupBy(transaction => transaction.Date.Month)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var income = group
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Income)
                        .Sum(transaction => transaction.Amount);

                    var expense = group
                        .Where(transaction =>
                            transaction.Type ==
                            TransactionType.Expense)
                        .Sum(transaction => transaction.Amount);

                    return new
                    {
                        Income = income,
                        Expense = expense,
                        TransactionCount = group.Count()
                    };
                });

        return Enumerable.Range(1, 12)
            .Select(month =>
            {
                monthlyTotals.TryGetValue(
                    month,
                    out var totals);

                var income = totals?.Income ?? 0;
                var expense = totals?.Expense ?? 0;

                return new TrendReportDto
                {
                    Month = month,

                    MonthName = CultureInfo
                        .InvariantCulture
                        .DateTimeFormat
                        .GetAbbreviatedMonthName(month),

                    Income = income,
                    Expense = expense,
                    Balance = income - expense,

                    TransactionCount =
                        totals?.TransactionCount ?? 0
                };
            })
            .ToList();
    }
    public async Task<IEnumerable<CashFlowReportDto>> GetCashFlowAsync(string userId,int year)
    {
        var yearStart = new DateTime(year,1,1,0,0,0, DateTimeKind.Utc);

        var nextYearStart = yearStart.AddYears(1);

        // Include older transactions to calculate the opening balance.
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date < nextYearStart)
            .Select(transaction => new
            {
                transaction.Date,
                transaction.Type,
                transaction.Amount
            })
            .ToListAsync();

        var openingBalance = transactions
            .Where(transaction =>
                transaction.Date < yearStart)
            .Sum(transaction =>
                transaction.Type == TransactionType.Income
                    ? transaction.Amount
                    : -transaction.Amount);

        var selectedYearTransactions = transactions
            .Where(transaction =>
                transaction.Date >= yearStart &&
                transaction.Date < nextYearStart)
            .ToList();

        var monthlyGroups = selectedYearTransactions
            .GroupBy(transaction =>
                transaction.Date.Month)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        var results = new List<CashFlowReportDto>();

        var runningBalance = openingBalance;

        for (var month = 1; month <= 12; month++)
        {
            monthlyGroups.TryGetValue(month,out var monthTransactions);

            monthTransactions ??= [];

            var income = monthTransactions
                .Where(transaction =>
                    transaction.Type ==
                    TransactionType.Income)
                .Sum(transaction => transaction.Amount);

            var expense = monthTransactions
                .Where(transaction =>
                    transaction.Type ==
                    TransactionType.Expense)
                .Sum(transaction => transaction.Amount);

            var netCashFlow = income - expense;
            var monthOpeningBalance = runningBalance;
            var closingBalance = monthOpeningBalance + netCashFlow;

            results.Add(new CashFlowReportDto
            {
                Month = month,

                MonthName = CultureInfo
                    .InvariantCulture
                    .DateTimeFormat
                    .GetAbbreviatedMonthName(month),

                OpeningBalance = monthOpeningBalance,
                Income = income,
                Expense = expense,
                NetCashFlow = netCashFlow,
                ClosingBalance = closingBalance,
                TransactionCount =
                    monthTransactions.Count
            });

            runningBalance = closingBalance;
        }
        return results;
    }
    public async Task<IEnumerable<DailySpendingDto>>GetDailySpendingAsync(string userId,int year,int month)
    {
        var monthStart = new DateTime(year,month,1, 0,0,0,DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);
        var expenses = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == TransactionType.Expense &&
                transaction.Date >= monthStart &&
                transaction.Date < nextMonthStart)
            .Select(transaction => new
            {
                transaction.Date,
                transaction.Amount
            })
            .ToListAsync();

        var dailyTotals = expenses
            .GroupBy(transaction => transaction.Date.Day)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Expense = group.Sum(
                        transaction => transaction.Amount),

                    TransactionCount = group.Count()
                });

        var daysInMonth = DateTime.DaysInMonth(year,month);

        return Enumerable.Range(1, daysInMonth)
            .Select(day =>
            {
                dailyTotals.TryGetValue(
                    day,
                    out var totals);

                return new DailySpendingDto
                {
                    Day = day,

                    Date = new DateTime(
                        year,
                        month,
                        day,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc),

                    Expense = totals?.Expense ?? 0,

                    TransactionCount =
                        totals?.TransactionCount ?? 0
                };
            })
            .ToList();
    }
    public async Task<IEnumerable<CalendarSpendingDto>> GetCalendarAsync(string userId,int year,int month)
    {
        var monthStart = new DateTime(year,month,1);
        var nextMonth = monthStart.AddMonths(1);
        var expenses = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == TransactionType.Expense &&
                transaction.Date >= monthStart &&
                transaction.Date < nextMonth)
            .Select(transaction => new
            {
                transaction.Date,
                transaction.Amount
            })
            .ToListAsync();

        var grouped = expenses
            .GroupBy(transaction => transaction.Date.Day)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Expense = group.Sum(transaction => transaction.Amount),
                    Count = group.Count()
                });

        var days = DateTime.DaysInMonth(year,month);
        return Enumerable.Range(1, days)
            .Select(day =>
            {
                grouped.TryGetValue(day, out var totals);

                return new CalendarSpendingDto
                {
                    Date = new DateTime(year, month, day),
                    Day = day,
                    TotalExpense = totals?.Expense ?? 0,
                    TransactionCount = totals?.Count ?? 0
                };
            })
            .ToList();
    }
    public async Task<IEnumerable<LargestTransactionDto>>GetLargestTransactionsAsync(string userId,int limit,TransactionType? type)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(transaction => transaction.Type == type.Value);
        }
        var transactions = await query
            .Select(transaction => new LargestTransactionDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Type = transaction.Type,
                CategoryId = transaction.CategoryId,

                Category = transaction.Category != null
                    ? transaction.Category.Name
                    : "Uncategorized",

                Color = transaction.Category != null
                    ? transaction.Category.Color
                    : "#808080",

                Icon = transaction.Category != null
                    ? transaction.Category.Icon
                    : "category",

                Amount = transaction.Amount,
                Notes = transaction.Notes
            })
            .ToListAsync();

        return transactions
            .OrderByDescending(transaction =>
                transaction.Amount)
            .ThenByDescending(transaction =>
                transaction.Date)
            .Take(limit)
            .ToList();
    }
    public async Task<IEnumerable<CategoryComparisonDto>>GetCategoryComparisonAsync(string userId,int year)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var nextYearStart = yearStart.AddYears(1);

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Type == TransactionType.Expense &&
                transaction.Date >= yearStart &&
                transaction.Date < nextYearStart)
            .Select(transaction => new
            {
                transaction.CategoryId,
                CategoryName = transaction.Category != null
                    ? transaction.Category.Name
                    : "Uncategorized",
                CategoryColor = transaction.Category != null
                    ? transaction.Category.Color
                    : "#808080",
                CategoryIcon = transaction.Category != null
                    ? transaction.Category.Icon
                    : "category",
                transaction.Date,
                transaction.Amount
            })
            .ToListAsync();

        return transactions
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                transaction.CategoryName,
                transaction.CategoryColor,
                transaction.CategoryIcon
            })
            .Select(categoryGroup =>
            {
                var monthlyGroups = categoryGroup
                    .GroupBy(transaction => transaction.Date.Month)
                    .ToDictionary(
                        group => group.Key,
                        group => new
                        {
                            Amount = group.Sum(transaction => transaction.Amount),
                            Count = group.Count()
                        });

                var monthlyResults = Enumerable
                    .Range(1, 12)
                    .Select(month =>
                    {
                        monthlyGroups.TryGetValue(month,out var totals);
                        return new CategoryMonthlyAmountDto
                        {
                            Month = month,
                            MonthName = CultureInfo
                                .InvariantCulture
                                .DateTimeFormat
                                .GetAbbreviatedMonthName(month),
                            Amount = totals?.Amount ?? 0,
                            TransactionCount =totals?.Count ?? 0
                        };
                    })
                    .ToList();

                var totalExpense = categoryGroup.Sum(transaction => transaction.Amount);

                return new CategoryComparisonDto
                {
                    CategoryId =categoryGroup.Key.CategoryId,
                    Category =categoryGroup.Key.CategoryName,
                    Color =categoryGroup.Key.CategoryColor,
                    Icon =categoryGroup.Key.CategoryIcon,
                    TotalExpense = totalExpense,
                    AverageMonthlyExpense = Math.Round(totalExpense / 12,2),
                    TransactionCount =categoryGroup.Count(),
                    Months = monthlyResults
                };
            })
            .OrderByDescending(result =>
                result.TotalExpense)
            .ToList();
    }
    public async Task<FinancialStatisticsDto> GetStatisticsAsync(string userId,int year)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var nextYearStart = yearStart.AddYears(1);

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Date >= yearStart &&
                transaction.Date < nextYearStart)
            .Select(transaction => new
            {
                transaction.Type,
                transaction.Amount,
                transaction.CategoryId,
                CategoryName = transaction.Category != null ? transaction.Category.Name: "Uncategorized"
            })
            .ToListAsync();

        var incomes = transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Income)
            .ToList();

        var expenses = transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense)
            .ToList();

        var totalIncome = incomes.Sum(
            transaction => transaction.Amount);

        var totalExpense = expenses.Sum(
            transaction => transaction.Amount);

        var topCategory = expenses
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                transaction.CategoryName
            })
            .Select(group => new
            {
                Category = group.Key.CategoryName,
                Amount = group.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(result => result.Amount)
            .FirstOrDefault();

        return new FinancialStatisticsDto
        {
            Year = year,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            LargestIncome = incomes.Count == 0 ? 0: incomes.Max(transaction =>transaction.Amount),
            LargestExpense = expenses.Count == 0 ? 0: expenses.Max(transaction =>transaction.Amount),
            AverageIncome = incomes.Count == 0 ? 0: Math.Round(incomes.Average(transaction =>transaction.Amount),2),
            AverageExpense = expenses.Count == 0 ? 0: Math.Round(expenses.Average(transaction =>transaction.Amount),2),
            AverageTransactionAmount =transactions.Count == 0? 0: Math.Round(transactions.Average(transaction =>transaction.Amount),2),
            TransactionCount =transactions.Count,
            IncomeTransactionCount =incomes.Count,
            ExpenseTransactionCount =expenses.Count,
            TopExpenseCategory =topCategory?.Category ?? string.Empty,
            TopExpenseCategoryAmount =topCategory?.Amount ?? 0
        };
    }
}