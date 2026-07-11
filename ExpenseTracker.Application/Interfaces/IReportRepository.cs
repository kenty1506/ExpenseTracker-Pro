using ExpenseTracker.Application.DTOs.Reports;

namespace ExpenseTracker.Application.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<MonthlyReportDto>> GetMonthlyReportAsync(string userId,int year);
}