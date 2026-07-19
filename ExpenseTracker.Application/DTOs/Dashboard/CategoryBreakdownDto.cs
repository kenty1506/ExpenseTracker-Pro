namespace ExpenseTracker.Application.DTOs.Dashboard;

public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}