using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Transactions;

public class CreateTransactionDto
{
    public TransactionType Type { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}