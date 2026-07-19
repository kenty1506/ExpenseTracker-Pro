namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class FinancialGoalsSummaryDto
{
    public int TotalGoals { get; set; }

    public int ActiveGoals { get; set; }

    public int CompletedGoals { get; set; }

    public int PausedGoals { get; set; }

    public int CancelledGoals { get; set; }

    public int OverdueGoals { get; set; }

    public decimal TotalTargetAmount { get; set; }

    public decimal TotalSavedAmount { get; set; }

    public decimal TotalRemainingAmount { get; set; }

    public decimal OverallPercentageCompleted { get; set; }
}