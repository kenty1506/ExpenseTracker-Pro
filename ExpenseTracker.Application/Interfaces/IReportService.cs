using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<MonthlyReportDto>> GetMonthlyReportAsync(int year);
    Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(int year, int limit);
    Task<IEnumerable<TrendReportDto>> GetTrendAsync(int year);
    Task<IEnumerable<CashFlowReportDto>> GetCashFlowAsync(int year);
    Task<IEnumerable<DailySpendingDto>> GetDailySpendingAsync(int year, int month);
    Task<IEnumerable<CalendarSpendingDto>> GetCalendarAsync(int year,int month);
    Task<IEnumerable<LargestTransactionDto>>GetLargestTransactionsAsync(int limit,TransactionType? type);
    Task<IEnumerable<CategoryComparisonDto>>GetCategoryComparisonAsync(int year);
    Task<FinancialStatisticsDto> GetStatisticsAsync(int year);
}