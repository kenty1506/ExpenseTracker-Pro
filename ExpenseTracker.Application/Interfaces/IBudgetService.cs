using ExpenseTracker.Application.DTOs.Budgets;

namespace ExpenseTracker.Application.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllAsync();
    Task<BudgetDto?> GetByIdAsync(int id);
    Task<BudgetDto> CreateAsync(CreateBudgetDto dto);
    Task<BudgetDto?> UpdateAsync(int id,UpdateBudgetDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<BudgetSummaryDto>> GetSummaryAsync(int year, int month);
    Task<IEnumerable<BudgetAlertDto>> GetAlertsAsync(int year, int month);
    Task<IEnumerable<BudgetVsActualDto>> GetBudgetVsActualAsync(int year,int month);
}