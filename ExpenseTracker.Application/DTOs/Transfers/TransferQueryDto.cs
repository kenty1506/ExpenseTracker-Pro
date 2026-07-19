using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.DTOs.Transfers;

public class TransferQueryDto : PagedQuery
{
    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    public string? Search { get; set; }
}