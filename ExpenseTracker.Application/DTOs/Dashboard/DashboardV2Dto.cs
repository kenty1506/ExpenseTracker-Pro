using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.DTOs.Notifications;
using ExpenseTracker.Application.DTOs.RecurringTransactions;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.DTOs.Transfers;

namespace ExpenseTracker.Application.DTOs.Dashboard;

public class DashboardV2Dto
{
    public DateTime GeneratedAt { get; set; }

    public DashboardSummaryDto FinancialSummary { get; set; } =
        new();

    public AccountSummaryDto AccountSummary { get; set; } =
        new();

    public FinancialGoalsSummaryDto GoalSummary { get; set; } =
        new();

    public NotificationSummaryDto NotificationSummary { get; set; } =
        new();

    public List<BudgetAlertDto> BudgetAlerts { get; set; } = [];

    public List<UpcomingRecurringTransactionDto>
        UpcomingRecurringItems
    { get; set; } = [];

    public List<TransactionDto> RecentTransactions { get; set; } = [];

    public List<TransferDto> RecentTransfers { get; set; } = [];
}