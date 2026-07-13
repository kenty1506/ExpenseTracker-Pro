using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class FinancialGoalDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal StartingAmount { get; set; }

    public decimal SavedAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal PercentageCompleted { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? TargetDate { get; set; }

    public int DaysRemaining { get; set; }

    public FinancialGoalStatus Status { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsOverdue { get; set; }

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string Account { get; set; } = string.Empty;

    public List<GoalContributionDto> Contributions { get; set; } = [];
}