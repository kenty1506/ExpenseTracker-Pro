using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class AddGoalInterestDto
{
    [Range(
        typeof(decimal),
        "0.01",
        "999999999.99")]
    public decimal Amount { get; set; }

    public DateTime InterestDate { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } =
        "Interest earned";
}