using ExpenseTracker.Application.DTOs.Dashboard;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountService _accountService;
    private readonly IBudgetService _budgetService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly IFinancialGoalService _financialGoalService;
    private readonly INotificationService _notificationService;
    private readonly ITransactionService _transactionService;
    private readonly ITransferService _transferService;

    public DashboardService(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IAccountService accountService,
        IBudgetService budgetService,
        IRecurringTransactionService recurringTransactionService,
        IFinancialGoalService financialGoalService,
        INotificationService notificationService,
        ITransactionService transactionService,
        ITransferService transferService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _accountService = accountService;
        _budgetService = budgetService;
        _recurringTransactionService = recurringTransactionService;
        _financialGoalService = financialGoalService;
        _notificationService = notificationService;
        _transactionService = transactionService;
        _transferService = transferService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        return await _transactionRepository.GetDashboardSummaryAsync(
            _currentUserService.UserId);
    }

    public async Task<IEnumerable<CategoryBreakdownDto>>
        GetCategoryBreakdownAsync()
    {
        return await _transactionRepository.GetCategoryBreakdownAsync(
            _currentUserService.UserId);
    }
    public async Task<DashboardV2Dto> GetDashboardV2Async()
    {
        var today = DateTime.UtcNow.Date;

        var financialSummary =
            await GetSummaryAsync();

        var accountSummary =
            await _accountService.GetSummaryAsync();

        var goalSummary =
            await _financialGoalService.GetSummaryAsync();

        var notificationSummary =
            await _notificationService.GetSummaryAsync();

        var budgetAlerts =
            await _budgetService.GetAlertsAsync(
                today.Year,
                today.Month);

        var upcomingRecurring =
            await _recurringTransactionService
                .GetUpcomingAsync(30);

        var transactions =
            await _transactionService.GetAllAsync();

        var transfers =
            await _transferService.GetAllAsync();

        return new DashboardV2Dto
        {
            GeneratedAt = DateTime.UtcNow,

            FinancialSummary =
                financialSummary,

            AccountSummary =
                accountSummary,

            GoalSummary =
                goalSummary,

            NotificationSummary =
                notificationSummary,

            BudgetAlerts =
                budgetAlerts
                    .Where(alert =>
                        alert.AlertLevel != "Safe")
                    .OrderByDescending(alert =>
                        alert.PercentageUsed)
                    .ToList(),

            UpcomingRecurringItems =
                upcomingRecurring
                    .OrderBy(item =>
                        item.NextRunDate)
                    .Take(10)
                    .ToList(),

            RecentTransactions =
                transactions
                    .OrderByDescending(transaction =>
                        transaction.Date)
                    .ThenByDescending(transaction =>
                        transaction.Id)
                    .Take(10)
                    .ToList(),

            RecentTransfers =
                transfers
                    .OrderByDescending(transfer =>
                        transfer.TransferDate)
                    .ThenByDescending(transfer =>
                        transfer.Id)
                    .Take(10)
                    .ToList()
        };
    }
}
