using ExpenseTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.RecurringTransactions;

public class CreateRecurringTransactionDto
{
    public TransactionType Type { get; set; }

    public int AccountId { get; set; }

    public int CategoryId { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    public string Notes { get; set; } = string.Empty;

    [Range(1, 31)]
    public int DayOfMonth { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}