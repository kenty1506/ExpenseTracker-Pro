using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Interfaces;

public interface IFinancialGoalService
{
    Task<IEnumerable<FinancialGoalDto>> GetAllAsync();

    Task<FinancialGoalDto?> GetByIdAsync(int id);

    Task<FinancialGoalDto> CreateAsync(CreateFinancialGoalDto dto);

    Task<FinancialGoalDto?> UpdateAsync(int id,UpdateFinancialGoalDto dto);

    Task<bool> DeleteAsync(int id);

    Task<GoalContributionDto?> AddContributionAsync(int financialGoalId,AddGoalContributionDto dto);
    Task<IEnumerable<FinancialGoalDto>> GetAllForUserAsync(string userId);

    Task<bool> DeleteContributionAsync(int financialGoalId,int contributionId);

    Task<FinancialGoalsSummaryDto> GetSummaryAsync();
    Task<PagedResult<FinancialGoalDto>> GetPagedAsync(FinancialGoalQueryDto query);

    Task<GoalContributionDto?> AddAdjustmentAsync(int financialGoalId,AddGoalAdjustmentDto dto);

    Task<GoalContributionDto?> AddInterestAsync(int financialGoalId,AddGoalInterestDto dto);

}
