namespace ExpenseTracker.Application.DTOs.Reports;

public class CategoryComparisonDto
{
    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal TotalExpense { get; set; }

    public decimal AverageMonthlyExpense { get; set; }

    public int TransactionCount { get; set; }

    public List<CategoryMonthlyAmountDto> Months { get; set; } = [];
}