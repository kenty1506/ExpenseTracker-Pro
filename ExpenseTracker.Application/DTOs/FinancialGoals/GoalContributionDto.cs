namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class GoalContributionDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime ContributionDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string Account { get; set; } = string.Empty;
}