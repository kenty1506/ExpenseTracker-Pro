namespace ExpenseTracker.Application.DTOs.RecurringTransactions;

public class GeneratedRecurringTransactionDto
{
    public int RecurringTransactionId { get; set; }

    public int TransactionId { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime OccurrenceDate { get; set; }

    public DateTime NextRunDate { get; set; }
}