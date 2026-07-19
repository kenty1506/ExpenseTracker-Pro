using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Budgets;

public class CreateBudgetDto
{
    [Required]
    public int CategoryId { get; set; }

    [Range(2000, 9999)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }
}