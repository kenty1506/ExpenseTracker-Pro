using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Budgets;

public class UpdateBudgetDto
{
    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }
}