using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

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
    public GoalContributionType ContributionType { get; set; }
        = GoalContributionType.Manual;

    public int? TransferId { get; set; }

    public Transfer? Transfer { get; set; }

    public int? TransactionId { get; set; }

    public Transaction? Transaction { get; set; }
}