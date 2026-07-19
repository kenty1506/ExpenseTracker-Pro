using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class AddGoalContributionDto
{
    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    public DateTime ContributionDate { get; set; }

    public int? AccountId { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}