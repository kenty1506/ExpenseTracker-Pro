using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Transactions;

public class UpdateTransactionDto
{
    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}