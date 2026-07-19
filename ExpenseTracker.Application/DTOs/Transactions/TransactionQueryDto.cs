using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Transactions;

public class TransactionQueryDto : PagedQuery
{
    public TransactionType? Type { get; set; }

    public int? CategoryId { get; set; }

    public int? AccountId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    public string? Search { get; set; }
}