using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.FinancialGoals;

public class FinancialGoalQueryDto : PagedQuery
{
    public FinancialGoalStatus? Status { get; set; }

    public int? AccountId { get; set; }

    public bool? IsCompleted { get; set; }

    public bool? IsOverdue { get; set; }

    public decimal? MinTargetAmount { get; set; }

    public decimal? MaxTargetAmount { get; set; }

    public decimal? MinSavedAmount { get; set; }

    public decimal? MaxSavedAmount { get; set; }

    public DateTime? TargetDateFrom { get; set; }

    public DateTime? TargetDateTo { get; set; }

    public string? Search { get; set; }
}