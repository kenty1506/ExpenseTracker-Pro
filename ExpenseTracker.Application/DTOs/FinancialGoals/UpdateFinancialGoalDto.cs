using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class UpdateFinancialGoalDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 999999999)]
    public decimal TargetAmount { get; set; }

    public decimal StartingAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public int? AccountId { get; set; }

    public FinancialGoalStatus Status { get; set; }

    [MaxLength(20)]
    public string Color { get; set; } = "#6366F1";

    [MaxLength(100)]
    public string Icon { get; set; } = "flag";

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}