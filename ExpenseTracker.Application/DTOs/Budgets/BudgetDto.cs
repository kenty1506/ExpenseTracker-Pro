namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetDto
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Amount { get; set; }
}