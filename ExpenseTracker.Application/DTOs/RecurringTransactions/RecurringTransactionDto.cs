using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.RecurringTransactions;

public class RecurringTransactionDto
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public int? AccountId { get; set; }

    public string Account { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public int DayOfMonth { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime NextRunDate { get; set; }

    public DateTime? LastRunDate { get; set; }

    public bool IsActive { get; set; }
}