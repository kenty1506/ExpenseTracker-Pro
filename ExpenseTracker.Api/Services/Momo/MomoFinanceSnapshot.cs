namespace ExpenseTracker.Api.Services.Momo;

public sealed record MomoFinanceSnapshot(
    DateTime AsOfUtc,
    string CurrentPath,
    MomoMonthlyContext CurrentMonth,
    MomoMonthlyContext? PreviousMonth,
    MomoAccountSummaryContext Accounts,
    List<MomoCategoryContext> Categories,
    List<MomoBudgetContext> Budgets,
    MomoGoalSummaryContext GoalSummary,
    List<MomoGoalContext> Goals,
    List<MomoRecurringContext> UpcomingRecurring,
    List<MomoTransactionContext> RecentTransactions);

public sealed record MomoMonthlyContext(
    int Year,
    int Month,
    string MonthName,
    decimal Income,
    decimal Expense,
    decimal Balance,
    int TransactionCount);

public sealed record MomoAccountSummaryContext(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal NetWorth,
    List<MomoAccountContext> Items);

public sealed record MomoAccountContext(
    string Name,
    string Type,
    decimal CurrentBalance,
    string Currency,
    bool IncludeInNetWorth,
    int TransactionCount);

public sealed record MomoCategoryContext(
    string Name,
    decimal CurrentMonthAmount,
    decimal PreviousMonthAmount,
    int TransactionCount);

public sealed record MomoBudgetContext(
    string Category,
    decimal Budget,
    decimal Actual,
    decimal Remaining,
    decimal PercentageUsed,
    bool IsOverBudget,
    int TransactionCount);

public sealed record MomoGoalSummaryContext(
    int TotalGoals,
    int ActiveGoals,
    int OverdueGoals,
    decimal TotalTargetAmount,
    decimal TotalSavedAmount,
    decimal TotalRemainingAmount,
    decimal OverallPercentageCompleted);

public sealed record MomoGoalContext(
    string Name,
    decimal TargetAmount,
    decimal SavedAmount,
    decimal RemainingAmount,
    decimal PercentageCompleted,
    DateTime? TargetDate,
    int DaysRemaining,
    string Status,
    bool IsOverdue,
    string Account);

public sealed record MomoRecurringContext(
    string Type,
    string Category,
    string Account,
    decimal Amount,
    DateTime NextRunDate,
    int DaysUntilDue,
    bool IsDueToday,
    bool IsOverdue);

public sealed record MomoTransactionContext(
    string Type,
    string Category,
    string Account,
    decimal Amount,
    DateTime Date);
