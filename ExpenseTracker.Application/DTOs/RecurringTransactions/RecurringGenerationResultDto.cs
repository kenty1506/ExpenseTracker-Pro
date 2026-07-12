namespace ExpenseTracker.Application.DTOs.RecurringTransactions;

public class RecurringGenerationResultDto
{
    public DateTime ProcessedThrough { get; set; }

    public int GeneratedCount { get; set; }

    public List<GeneratedRecurringTransactionDto> Generated { get; set; } = [];
}