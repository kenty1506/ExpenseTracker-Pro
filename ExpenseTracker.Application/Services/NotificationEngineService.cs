using ExpenseTracker.Application.DTOs.Notifications;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class NotificationEngineService
    : INotificationEngineService
{
    private readonly INotificationService _notificationService;
    private readonly IBudgetService _budgetService;
    private readonly IRecurringTransactionService
        _recurringTransactionService;
    private readonly IFinancialGoalService
        _financialGoalService;
    private readonly ICurrentUserService
        _currentUserService;

    public NotificationEngineService(
        INotificationService notificationService,
        IBudgetService budgetService,
        IRecurringTransactionService recurringTransactionService,
        IFinancialGoalService financialGoalService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _budgetService = budgetService;
        _recurringTransactionService =
            recurringTransactionService;
        _financialGoalService = financialGoalService;
        _currentUserService = currentUserService;
    }

    public Task<NotificationGenerationResultDto> GenerateAsync()
    {
        return GenerateForUserAsync(
            _currentUserService.UserId);
    }

    public async Task<NotificationGenerationResultDto>
        GenerateForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        var result =
            new NotificationGenerationResultDto();

        await GenerateBudgetNotificationsAsync(
            userId,
            result);

        await GenerateRecurringNotificationsAsync(
            userId,
            result);

        await GenerateGoalNotificationsAsync(
            userId,
            result);

        return result;
    }

    private async Task GenerateBudgetNotificationsAsync(
        string userId,
        NotificationGenerationResultDto result)
    {
        var today = DateTime.UtcNow.Date;

        var alerts =
            await _budgetService.GetAlertsForUserAsync(
                userId,
                today.Year,
                today.Month);

        foreach (var alert in alerts)
        {
            if (alert.AlertLevel == "Safe")
                continue;

            var isExceeded =
                alert.AlertLevel == "Over Budget";

            var dto = new CreateNotificationDto
            {
                Type = isExceeded
                    ? NotificationType.BudgetExceeded
                    : NotificationType.BudgetWarning,

                Priority = isExceeded
                    ? NotificationPriority.Critical
                    : NotificationPriority.High,

                Title = isExceeded
                    ? $"{alert.Category} budget exceeded"
                    : $"{alert.Category} budget warning",

                Message = isExceeded
                    ? $"You exceeded your {alert.Category} budget by " +
                      $"{Math.Abs(alert.Remaining):N2}."
                    : $"You have used {alert.PercentageUsed:N2}% of your " +
                      $"{alert.Category} budget.",

                OccurredAt = DateTime.UtcNow,

                ReferenceType = "Budget",
                ReferenceId = alert.BudgetId,

                ActionUrl =
                    $"/budgets/{alert.BudgetId}",

                UniqueKey =
                    $"budget-alert:{alert.BudgetId}:" +
                    $"{today:yyyy-MM}:{alert.AlertLevel}"
            };

            await TryCreateAsync(
                userId,
                dto,
                result);
        }
    }

    private async Task GenerateRecurringNotificationsAsync(
        string userId,
        NotificationGenerationResultDto result)
    {
        var recurringItems =
            await _recurringTransactionService
                .GetUpcomingForUserAsync(
                    userId,
                    7);

        foreach (var item in recurringItems)
        {
            var title = item.DaysUntilDue switch
            {
                0 => $"{item.Category} is due today",
                1 => $"{item.Category} is due tomorrow",
                _ => $"{item.Category} is due soon"
            };

            var message = item.DaysUntilDue switch
            {
                0 =>
                    $"{item.Notes} worth {item.Amount:N2} " +
                    "is due today.",

                1 =>
                    $"{item.Notes} worth {item.Amount:N2} " +
                    "is due tomorrow.",

                _ =>
                    $"{item.Notes} worth {item.Amount:N2} " +
                    $"is due in {item.DaysUntilDue} days."
            };

            var priority = item.DaysUntilDue switch
            {
                0 => NotificationPriority.High,
                1 => NotificationPriority.Normal,
                _ => NotificationPriority.Low
            };

            var dto = new CreateNotificationDto
            {
                Type = NotificationType.RecurringDue,
                Priority = priority,
                Title = title,
                Message = message,
                OccurredAt = DateTime.UtcNow,

                ReferenceType = "RecurringTransaction",
                ReferenceId = item.Id,

                ActionUrl =
                    $"/recurring-transactions/{item.Id}",

                UniqueKey =
                    $"recurring-due:{item.Id}:" +
                    $"{item.NextRunDate:yyyy-MM-dd}"
            };

            await TryCreateAsync(
                userId,
                dto,
                result);
        }
    }

    private async Task GenerateGoalNotificationsAsync(
        string userId,
        NotificationGenerationResultDto result)
    {
        var goals =
            await _financialGoalService
                .GetAllForUserAsync(userId);

        foreach (var goal in goals)
        {
            if (goal.Status ==
                    FinancialGoalStatus.Cancelled ||
                goal.Status ==
                    FinancialGoalStatus.Paused)
            {
                continue;
            }

            if (goal.IsCompleted)
            {
                var completedNotification =
                    new CreateNotificationDto
                    {
                        Type =
                            NotificationType.GoalCompleted,

                        Priority =
                            NotificationPriority.High,

                        Title =
                            $"{goal.Name} completed",

                        Message =
                            "Congratulations! You reached your " +
                            $"{goal.TargetAmount:N2} target.",

                        OccurredAt = DateTime.UtcNow,

                        ReferenceType = "FinancialGoal",
                        ReferenceId = goal.Id,

                        ActionUrl =
                            $"/financial-goals/{goal.Id}",

                        UniqueKey =
                            $"goal-completed:{goal.Id}"
                    };

                await TryCreateAsync(
                    userId,
                    completedNotification,
                    result);

                continue;
            }

            if (goal.IsOverdue)
            {
                var targetDateKey =
                    goal.TargetDate?.ToString("yyyy-MM-dd")
                    ?? "no-date";

                var overdueNotification =
                    new CreateNotificationDto
                    {
                        Type =
                            NotificationType.GoalOverdue,

                        Priority =
                            NotificationPriority.High,

                        Title =
                            $"{goal.Name} is overdue",

                        Message =
                            "The target date has passed. You still " +
                            $"need {goal.RemainingAmount:N2}.",

                        OccurredAt = DateTime.UtcNow,

                        ReferenceType = "FinancialGoal",
                        ReferenceId = goal.Id,

                        ActionUrl =
                            $"/financial-goals/{goal.Id}",

                        UniqueKey =
                            $"goal-overdue:{goal.Id}:" +
                            targetDateKey
                    };

                await TryCreateAsync(
                    userId,
                    overdueNotification,
                    result);

                continue;
            }

            if (goal.PercentageCompleted >= 80)
            {
                var progressLevel =
                    (int)Math.Floor(
                        goal.PercentageCompleted / 5) * 5;

                var nearTargetNotification =
                    new CreateNotificationDto
                    {
                        Type =
                            NotificationType.GoalNearTarget,

                        Priority =
                            NotificationPriority.Normal,

                        Title =
                            $"{goal.Name} is almost complete",

                        Message =
                            $"Your goal is now " +
                            $"{goal.PercentageCompleted:N2}% complete. " +
                            $"{goal.RemainingAmount:N2} remains.",

                        OccurredAt = DateTime.UtcNow,

                        ReferenceType = "FinancialGoal",
                        ReferenceId = goal.Id,

                        ActionUrl =
                            $"/financial-goals/{goal.Id}",

                        UniqueKey =
                            $"goal-progress:{goal.Id}:" +
                            $"{progressLevel}"
                    };

                await TryCreateAsync(
                    userId,
                    nearTargetNotification,
                    result);
            }
        }
    }

    private async Task TryCreateAsync(
        string userId,
        CreateNotificationDto dto,
        NotificationGenerationResultDto result)
    {
        var created =
            await _notificationService
                .CreateIfMissingForUserAsync(
                    userId,
                    dto);

        if (created == null)
        {
            result.SkippedCount++;
            return;
        }

        result.GeneratedCount++;
        result.GeneratedNotifications.Add(created);
    }
}