using ExpenseTracker.Application.DTOs.Dashboard;

namespace ExpenseTracker.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync();
}