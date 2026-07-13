namespace ExpenseTracker.Domain.Enums;

public enum NotificationType
{
    BudgetWarning = 1,
    BudgetExceeded = 2,
    RecurringDue = 3,
    RecurringOverdue = 4,
    GoalNearTarget = 5,
    GoalCompleted = 6,
    GoalOverdue = 7,
    AccountLowBalance = 8,
    MonthlySummary = 9,
    General = 10
}