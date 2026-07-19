namespace ExpenseTracker.Application.DTOs.Reports;

public class TopCategoryDto
{
    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal Percentage { get; set; }

    public int TransactionCount { get; set; }
}