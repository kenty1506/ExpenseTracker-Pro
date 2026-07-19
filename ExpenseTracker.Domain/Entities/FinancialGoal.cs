using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class FinancialGoal : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal StartingAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? TargetDate { get; set; }

    public FinancialGoalStatus Status { get; set; }

    public int? AccountId { get; set; }

    public Account? Account { get; set; }

    public string Color { get; set; } = "#6366F1";

    public string Icon { get; set; } = "flag";

    public string Notes { get; set; } = string.Empty;

    public List<GoalContribution> Contributions { get; set; } = [];
}