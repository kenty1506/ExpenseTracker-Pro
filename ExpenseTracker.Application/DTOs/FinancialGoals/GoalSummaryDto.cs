namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class GoalSummaryDto
{
    public decimal SavedAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal PercentageCompleted { get; set; }

    public int DaysRemaining { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsOverdue { get; set; }
}