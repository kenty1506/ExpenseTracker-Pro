using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class GoalContribution : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public int FinancialGoalId { get; set; }

    public FinancialGoal? FinancialGoal { get; set; }

    public int? AccountId { get; set; }

    public Account? Account { get; set; }

    public decimal Amount { get; set; }

    public DateTime ContributionDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}