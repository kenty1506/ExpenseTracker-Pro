using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.FinancialGoals;

namespace ExpenseTracker.Application.Interfaces;

public interface IFinancialGoalRepository
{
    Task<IEnumerable<FinancialGoal>> GetAllAsync(
        string userId);

    Task<FinancialGoal?> GetByIdAsync(
        int id,
        string userId);

    Task<FinancialGoal?> GetByNameAsync(
        string name,
        string userId);

    Task<FinancialGoal> CreateAsync(
        FinancialGoal financialGoal);

    Task<bool> UpdateAsync(
        FinancialGoal financialGoal);

    Task<bool> DeleteAsync(
        int id,
        string userId);

    Task<GoalContribution> AddContributionAsync(
        GoalContribution contribution);

    Task<bool> DeleteContributionAsync(
        int contributionId,
        int financialGoalId,
        string userId);

    Task<PagedResult<FinancialGoal>> GetPagedAsync(
    string userId,
    FinancialGoalQueryDto query);
}