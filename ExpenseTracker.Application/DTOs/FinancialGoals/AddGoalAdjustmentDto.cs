using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class AddGoalAdjustmentDto
{
    [Range(
        typeof(decimal),
        "-999999999.99",
        "999999999.99")]
    public decimal Amount { get; set; }

    public DateTime AdjustmentDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}