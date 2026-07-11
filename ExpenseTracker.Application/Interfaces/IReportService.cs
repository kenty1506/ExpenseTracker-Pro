using ExpenseTracker.Application.DTOs.Reports;

namespace ExpenseTracker.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<MonthlyReportDto>> GetMonthlyReportAsync(int year);
}