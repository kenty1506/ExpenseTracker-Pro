using System.Globalization;
using ExpenseTracker.Api.DTOs.Momo;
using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;

namespace ExpenseTracker.Api.Services.Momo;

public sealed class MomoAssistantService : IMomoAssistantService
{
    private static readonly string[] AllowedPagePaths =
    [
        "/dashboard",
        "/notifications",
        "/expenses",
        "/accounts",
        "/transfers",
        "/budgets",
        "/financial-goals",
        "/budget-forecast",
        "/recurring-transactions",
        "/categories",
        "/reports",
        "/expense-calendar",
        "/audit-trails",
        "/settings",
        "/profile"
    ];

    private readonly IAccountService _accountService;
    private readonly IBudgetService _budgetService;
    private readonly IFinancialGoalService _financialGoalService;
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly ITransactionService _transactionService;
    private readonly IReportService _reportService;
    private readonly MomoConversationEngine _conversationEngine;

    public MomoAssistantService(
        IAccountService accountService,
        IBudgetService budgetService,
        IFinancialGoalService financialGoalService,
        IRecurringTransactionService recurringTransactionService,
        ITransactionService transactionService,
        IReportService reportService,
        MomoConversationEngine conversationEngine)
    {
        _accountService = accountService;
        _budgetService = budgetService;
        _financialGoalService = financialGoalService;
        _recurringTransactionService = recurringTransactionService;
        _transactionService = transactionService;
        _reportService = reportService;
        _conversationEngine = conversationEngine;
    }

    public async Task<MomoChatResponse> ChatAsync(
        MomoChatRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = await BuildSnapshotAsync(request.CurrentPath);
        cancellationToken.ThrowIfCancellationRequested();

        return _conversationEngine.Reply(request, snapshot);
    }

    private async Task<MomoFinanceSnapshot> BuildSnapshotAsync(
        string currentPath)
    {
        // The application services share one scoped EF Core context, so these
        // reads must stay sequential.
        var accountSummary = await _accountService.GetSummaryAsync();

        var today = DateTime.UtcNow.Date;
        var previousDate = today.AddMonths(-1);
        var budgets = (await _budgetService.GetSummaryAsync(
                today.Year,
                today.Month))
            .ToList();
        var goalSummary = await _financialGoalService.GetSummaryAsync();
        var goals = await _financialGoalService.GetPagedAsync(
            new FinancialGoalQueryDto
            {
                Page = 1,
                PageSize = 12,
                SortBy = "targetDate",
                SortDirection = "asc"
            });
        var upcoming = (await _recurringTransactionService
                .GetUpcomingAsync(30))
            .ToList();
        var recentTransactions = await _transactionService.GetPagedAsync(
            new TransactionQueryDto
            {
                Page = 1,
                PageSize = 8,
                SortBy = "date",
                SortDirection = "desc"
            });
        var currentYearMonths = (await _reportService
                .GetMonthlyReportAsync(today.Year))
            .ToList();
        var currentYearCategories = (await _reportService
                .GetCategoryComparisonAsync(today.Year))
            .ToList();

        List<MonthlyReportDto> previousYearMonths = [];
        List<CategoryComparisonDto> previousYearCategories = [];

        if (previousDate.Year != today.Year)
        {
            previousYearMonths = (await _reportService
                    .GetMonthlyReportAsync(previousDate.Year))
                .ToList();
            previousYearCategories = (await _reportService
                    .GetCategoryComparisonAsync(previousDate.Year))
                .ToList();
        }

        var currentMonth = ToMonthlyContext(
            currentYearMonths.FirstOrDefault(item =>
                item.Month == today.Month),
            today);
        var previousMonthSource = previousDate.Year == today.Year
            ? currentYearMonths
            : previousYearMonths;
        var previousMonth = ToMonthlyContext(
            previousMonthSource.FirstOrDefault(item =>
                item.Month == previousDate.Month),
            previousDate);

        var categoryNames = currentYearCategories
            .Select(item => item.Category)
            .Concat(previousYearCategories.Select(item => item.Category))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var categoryContexts = categoryNames
            .Select(category =>
            {
                var current = FindCategoryMonth(
                    currentYearCategories,
                    category,
                    today.Month);
                var previousSource = previousDate.Year == today.Year
                    ? currentYearCategories
                    : previousYearCategories;
                var previous = FindCategoryMonth(
                    previousSource,
                    category,
                    previousDate.Month);

                return new MomoCategoryContext(
                    category,
                    current?.Amount ?? 0,
                    previous?.Amount ?? 0,
                    current?.TransactionCount ?? 0);
            })
            .Where(item =>
                item.CurrentMonthAmount > 0 ||
                item.PreviousMonthAmount > 0)
            .OrderByDescending(item => item.CurrentMonthAmount)
            .Take(20)
            .ToList();

        return new MomoFinanceSnapshot(
            DateTime.UtcNow,
            NormalizePath(currentPath),
            currentMonth,
            previousMonth,
            new MomoAccountSummaryContext(
                accountSummary.TotalAssets,
                accountSummary.TotalLiabilities,
                accountSummary.NetWorth,
                accountSummary.Accounts
                    .Where(item => item.IsActive)
                    .OrderByDescending(item => item.CurrentBalance)
                    .Take(20)
                    .Select(item => new MomoAccountContext(
                        item.Name,
                        item.Type.ToString(),
                        item.CurrentBalance,
                        item.Currency,
                        item.IncludeInNetWorth,
                        item.TransactionCount))
                    .ToList()),
            categoryContexts,
            budgets
                .OrderByDescending(item => item.PercentageUsed)
                .Take(20)
                .Select(item => new MomoBudgetContext(
                    item.Category,
                    item.Budget,
                    item.Actual,
                    item.Remaining,
                    item.PercentageUsed,
                    item.IsOverBudget,
                    item.TransactionCount))
                .ToList(),
            new MomoGoalSummaryContext(
                goalSummary.TotalGoals,
                goalSummary.ActiveGoals,
                goalSummary.OverdueGoals,
                goalSummary.TotalTargetAmount,
                goalSummary.TotalSavedAmount,
                goalSummary.TotalRemainingAmount,
                goalSummary.OverallPercentageCompleted),
            goals.Items
                .Select(item => new MomoGoalContext(
                    item.Name,
                    item.TargetAmount,
                    item.SavedAmount,
                    item.RemainingAmount,
                    item.PercentageCompleted,
                    item.TargetDate,
                    item.DaysRemaining,
                    item.Status.ToString(),
                    item.IsOverdue,
                    item.Account))
                .ToList(),
            upcoming
                .OrderBy(item => item.NextRunDate)
                .Take(12)
                .Select(item => new MomoRecurringContext(
                    item.Type.ToString(),
                    item.Category,
                    item.Account,
                    item.Amount,
                    item.NextRunDate,
                    item.DaysUntilDue,
                    item.IsDueToday,
                    item.IsOverdue))
                .ToList(),
            recentTransactions.Items
                .Select(item => new MomoTransactionContext(
                    item.Type.ToString(),
                    item.Category,
                    item.Account,
                    item.Amount,
                    item.Date))
                .ToList());
    }

    private static MomoMonthlyContext ToMonthlyContext(
        MonthlyReportDto? month,
        DateTime date) =>
        new(
            date.Year,
            date.Month,
            month?.MonthName ?? date.ToString("MMMM", CultureInfo.InvariantCulture),
            month?.TotalIncome ?? 0,
            month?.TotalExpense ?? 0,
            month?.Balance ?? 0,
            month?.TransactionCount ?? 0);

    private static CategoryMonthlyAmountDto? FindCategoryMonth(
        IEnumerable<CategoryComparisonDto> categories,
        string category,
        int month) =>
        categories
            .FirstOrDefault(item => item.Category.Equals(
                category,
                StringComparison.OrdinalIgnoreCase))?
            .Months.FirstOrDefault(item => item.Month == month);

    private static string NormalizePath(string currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath) ||
            !currentPath.StartsWith('/'))
        {
            return "/dashboard";
        }

        var path = currentPath.Split('?', '#')[0];

        return AllowedPagePaths.Any(allowedPath =>
                path.Equals(
                    allowedPath,
                    StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(
                    $"{allowedPath}/",
                    StringComparison.OrdinalIgnoreCase))
            ? path[..Math.Min(path.Length, 160)]
            : "/dashboard";
    }
}
