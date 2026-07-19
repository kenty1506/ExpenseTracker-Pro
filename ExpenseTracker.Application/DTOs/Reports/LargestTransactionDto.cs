using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reports;

public class LargestTransactionDto
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;
}