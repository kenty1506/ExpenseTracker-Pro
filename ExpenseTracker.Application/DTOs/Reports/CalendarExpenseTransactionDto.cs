namespace ExpenseTracker.Application.DTOs.Reports;

public class CalendarExpenseTransactionDto
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string? Account { get; set; }

    public bool IsRecurring { get; set; }
}
