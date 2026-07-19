namespace ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Domain.Enums;

public class GoalContributionDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime ContributionDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string Account { get; set; } = string.Empty;

    public GoalContributionType ContributionType { get; set; }

    public int? TransferId { get; set; }

    public bool IsAutomatic =>
        ContributionType == GoalContributionType.TransferDeposit ||
        ContributionType == GoalContributionType.TransferWithdrawal;

    public int? TransactionId { get; set; }
}
