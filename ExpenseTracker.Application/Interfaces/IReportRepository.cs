using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<MonthlyReportDto>> GetMonthlyReportAsync(string userId,int year);
    Task<IEnumerable<TopCategoryDto>> GetTopCategoriesAsync(string userId,int year,int limit);
    Task<IEnumerable<TrendReportDto>> GetTrendAsync(string userId, int year);
    Task<IEnumerable<CashFlowReportDto>> GetCashFlowAsync(string userId, int year);
    Task<IEnumerable<DailySpendingDto>> GetDailySpendingAsync(string userId,int year,int month);
    Task<IEnumerable<CalendarSpendingDto>> GetCalendarAsync(string userId, int year, int month);
    Task<ExpenseCalendarDto> GetExpenseCalendarAsync(string userId, int year, int month);
    Task<IEnumerable<LargestTransactionDto>>GetLargestTransactionsAsync(string userId,int limit,TransactionType? type);
    Task<IEnumerable<CategoryComparisonDto>>GetCategoryComparisonAsync(string userId, int year);
    Task<FinancialStatisticsDto> GetStatisticsAsync(string userId,int year);
    Task<PagedResult<LargestTransactionDto>> GetLargestTransactionsPagedAsync(string userId,LargestTransactionQueryDto query);


}
