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
    public async Task<IEnumerable<MonthlyReportDto>>
    GetMonthlyReportAsync(
        string userId,
        int year)
    {
        // Let SQLite filter the records, but calculate decimal totals in C#.
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
}