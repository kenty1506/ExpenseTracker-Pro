using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Infrastructure.Data;

public class DevelopmentDataSeeder
{
    private readonly ExpenseTrackerDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _testEmail;
    private readonly string _testPassword;

    public DevelopmentDataSeeder(
        ExpenseTrackerDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _testEmail = configuration["DevelopmentSeeder:Email"]?.Trim() ??
            string.Empty;
        _testPassword = configuration["DevelopmentSeeder:Password"] ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(_testEmail))
        {
            throw new InvalidOperationException(
                "Development seeding is enabled, but DevelopmentSeeder:Email is missing.");
        }
    }

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var user = await CreateUserAsync();

        await SeedCategoriesAsync(user.Id, now);
        await SeedAccountsAsync(user.Id, now);

        var categories = await _context.Categories
            .Where(x => x.UserId == user.Id)
            .ToDictionaryAsync(x => x.Name);

        var accounts = await _context.Accounts
            .Where(x => x.UserId == user.Id)
            .ToDictionaryAsync(x => x.Name);

        await SeedTransactionsAsync(
            user.Id,
            categories,
            accounts,
            now);

        await SeedTransfersAsync(
            user.Id,
            accounts,
            now);

        await SeedBudgetsAsync(
            user.Id,
            categories,
            now);

        await SeedRecurringTransactionsAsync(
            user.Id,
            categories,
            accounts,
            now);

        await SeedFinancialGoalsAsync(
            user.Id,
            categories,
            accounts,
            now);

        await SeedNotificationsAsync(
            user.Id,
            now);

        await SeedAuditLogsAsync(
            user.Id,
            now);
    }

    private async Task<ApplicationUser> CreateUserAsync()
    {
        var existingUser =
            await _userManager.FindByEmailAsync(_testEmail);

        if (existingUser != null)
            return existingUser;

        if (string.IsNullOrWhiteSpace(_testPassword))
        {
            throw new InvalidOperationException(
                "The configured development user does not exist. Register it first " +
                "or store DevelopmentSeeder:Password in .NET user secrets.");
        }

        var user = new ApplicationUser
        {
            FullName = "Expense Tracker Test User",
            Email = _testEmail,
            UserName = _testEmail,
            EmailConfirmed = true
        };

        var result =
            await _userManager.CreateAsync(
                user,
                _testPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(x => x.Description));

            throw new InvalidOperationException(
                $"Unable to create development user: {errors}");
        }

        return user;
    }

    private async Task SeedCategoriesAsync(
        string userId,
        DateTime now)
    {
        var categories = new[]
        {
            CreateCategory(
                userId,
                "Food",
                "#FF6B6B",
                "restaurant",
                now),

            CreateCategory(
                userId,
                "Transportation",
                "#4ECDC4",
                "directions_car",
                now),

            CreateCategory(
                userId,
                "Shopping",
                "#45B7D1",
                "shopping_cart",
                now),

            CreateCategory(
                userId,
                "Bills",
                "#9B59B6",
                "receipt_long",
                now),

            CreateCategory(
                userId,
                "Entertainment",
                "#F39C12",
                "movie",
                now),

            CreateCategory(
                userId,
                "Health",
                "#27AE60",
                "local_hospital",
                now),

            CreateCategory(
                userId,
                "Savings",
                "#16A085",
                "savings",
                now),

            CreateCategory(
                userId,
                "Salary",
                "#2ECC71",
                "payments",
                now),

            CreateCategory(
                userId,
                "Freelance",
                "#1ABC9C",
                "work",
                now),

            CreateCategory(
                userId,
                "Financial Goal Funding",
                "#0EA5E9",
                "flag",
                now)
        };

        var existingNames = (await _context.Categories
                .Where(category => category.UserId == userId)
                .Select(category => category.Name)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _context.Categories.AddRange(
            categories.Where(category =>
                !existingNames.Contains(category.Name)));

        await _context.SaveChangesAsync();
    }

    private async Task SeedAccountsAsync(
        string userId,
        DateTime now)
    {
        var accounts = new[]
        {
            CreateAccount(
                userId,
                "Cash",
                AccountType.Cash,
                5_000,
                "#22C55E",
                "payments",
                now),

            CreateAccount(
                userId,
                "BPI Savings",
                AccountType.BankAccount,
                25_000,
                "#1D4ED8",
                "account_balance",
                now),

            CreateAccount(
                userId,
                "GCash",
                AccountType.EWallet,
                3_000,
                "#2563EB",
                "smartphone",
                now),

            CreateAccount(
                userId,
                "Maya Wallet",
                AccountType.EWallet,
                2_500,
                "#10B981",
                "account_balance_wallet",
                now),

            CreateAccount(
                userId,
                "BDO Credit Card",
                AccountType.CreditCard,
                0,
                "#DC2626",
                "credit_card",
                now)
        };

        var existingNames = (await _context.Accounts
                .Where(account => account.UserId == userId)
                .Select(account => account.Name)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _context.Accounts.AddRange(
            accounts.Where(account =>
                !existingNames.Contains(account.Name)));

        await _context.SaveChangesAsync();
    }

    private async Task SeedTransactionsAsync(
        string userId,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, Account> accounts,
        DateTime now)
    {
        if (await _context.Transactions.AnyAsync(
                x => x.UserId == userId))
        {
            return;
        }

        var year = now.Year;
        var month = now.Month;

        DateTime Date(int day, int hour = 12)
        {
            var validDay = Math.Min(
                day,
                DateTime.DaysInMonth(year, month));

            return new DateTime(
                year,
                month,
                validDay,
                hour,
                0,
                0,
                DateTimeKind.Utc);
        }

        var transactions = new[]
        {
            // Income
            CreateTransaction(
                userId,
                TransactionType.Income,
                categories["Salary"],
                accounts["BPI Savings"],
                51_000,
                "Monthly salary",
                Date(1, 8),
                now),

            CreateTransaction(
                userId,
                TransactionType.Income,
                categories["Freelance"],
                accounts["GCash"],
                8_500,
                "Freelance project payment",
                Date(8, 10),
                now),

            CreateTransaction(
                userId,
                TransactionType.Income,
                categories["Freelance"],
                accounts["Cash"],
                4_000,
                "Weekend freelance work",
                Date(18, 11),
                now),

            // Food
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["Cash"],
                350,
                "Lunch",
                Date(2),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["GCash"],
                540,
                "Dinner delivery",
                Date(5, 19),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["Cash"],
                720,
                "Groceries",
                Date(8, 17),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["GCash"],
                560,
                "Tokyo Tokyo",
                Date(11),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["Cash"],
                480,
                "Lunch",
                Date(16),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["BDO Credit Card"],
                1_200,
                "Family dinner",
                Date(22, 19),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["Cash"],
                890,
                "Weekly groceries",
                Date(29, 16),
                now),

            // Transportation
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["Cash"],
                120,
                "Jeepney fare",
                Date(3, 8),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["GCash"],
                150,
                "Grab ride",
                Date(7, 9),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["Cash"],
                200,
                "Fuel",
                Date(10, 8),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["GCash"],
                300,
                "Ride-hailing",
                Date(15, 9),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["BDO Credit Card"],
                450,
                "Fuel",
                Date(20, 8),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Transportation"],
                accounts["Cash"],
                170,
                "Commute",
                Date(27, 8),
                now),

            // Shopping
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Shopping"],
                accounts["BDO Credit Card"],
                1_500,
                "New keyboard",
                Date(6, 18),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Shopping"],
                accounts["BDO Credit Card"],
                2_300,
                "Clothes",
                Date(14, 16),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Shopping"],
                accounts["GCash"],
                950,
                "Online shopping",
                Date(23, 15),
                now),

            // Bills
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["BPI Savings"],
                1_899,
                "PLDT Fiber",
                Date(15, 7),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["BDO Credit Card"],
                549,
                "Netflix",
                Date(16, 7),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["BPI Savings"],
                2_100,
                "Electricity bill",
                Date(20, 7),
                now),

            // Entertainment
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Entertainment"],
                accounts["Cash"],
                320,
                "Cinema snacks",
                Date(9, 20),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Entertainment"],
                accounts["BDO Credit Card"],
                700,
                "Movie tickets",
                Date(18, 20),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Entertainment"],
                accounts["GCash"],
                450,
                "Streaming and games",
                Date(26, 20),
                now),

            // Health
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Health"],
                accounts["Cash"],
                850,
                "Medicine",
                Date(12, 13),
                now),

            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Health"],
                accounts["BDO Credit Card"],
                1_300,
                "Medical checkup",
                Date(28, 10),
                now),

            // Savings
            CreateTransaction(
                userId,
                TransactionType.Expense,
                categories["Savings"],
                accounts["BPI Savings"],
                3_000,
                "Emergency fund contribution",
                Date(30, 8),
                now)
        };

        _context.Transactions.AddRange(transactions);

        await _context.SaveChangesAsync();
    }

    private async Task SeedBudgetsAsync(
        string userId,
        IReadOnlyDictionary<string, Category> categories,
        DateTime now)
    {
        var alreadySeeded =
            await _context.Budgets.AnyAsync(x =>
                x.UserId == userId &&
                x.Year == now.Year &&
                x.Month == now.Month);

        if (alreadySeeded)
            return;

        var budgets = new[]
        {
            CreateBudget(
                userId,
                categories["Food"],
                now,
                10_000),

            CreateBudget(
                userId,
                categories["Transportation"],
                now,
                2_000),

            CreateBudget(
                userId,
                categories["Shopping"],
                now,
                5_000),

            CreateBudget(
                userId,
                categories["Bills"],
                now,
                5_000),

            CreateBudget(
                userId,
                categories["Entertainment"],
                now,
                3_000),

            CreateBudget(
                userId,
                categories["Health"],
                now,
                2_500)
        };

        _context.Budgets.AddRange(budgets);

        await _context.SaveChangesAsync();
    }

    private async Task SeedRecurringTransactionsAsync(
        string userId,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, Account> accounts,
        DateTime now)
    {
        if (await _context.RecurringTransactions.AnyAsync(
                x => x.UserId == userId))
        {
            return;
        }

        var startDate = new DateTime(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var recurringTransactions = new[]
        {
            CreateRecurring(
                userId,
                TransactionType.Income,
                categories["Salary"],
                accounts["BPI Savings"],
                51_000,
                "Monthly salary",
                25,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Entertainment"],
                accounts["BDO Credit Card"],
                549,
                "Netflix subscription",
                18,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Entertainment"],
                accounts["GCash"],
                149,
                "Spotify subscription",
                10,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["BPI Savings"],
                1_899,
                "PLDT Fiber",
                15,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["BPI Savings"],
                2_100,
                "Electricity bill",
                20,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Bills"],
                accounts["Cash"],
                500,
                "Water bill",
                8,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Food"],
                accounts["Cash"],
                3_000,
                "Monthly grocery allowance",
                6,
                startDate,
                now),

            CreateRecurring(
                userId,
                TransactionType.Expense,
                categories["Savings"],
                accounts["BPI Savings"],
                3_000,
                "Emergency fund contribution",
                30,
                startDate,
                now)
        };

        _context.RecurringTransactions.AddRange(
            recurringTransactions);

        await _context.SaveChangesAsync();
    }

    private async Task SeedTransfersAsync(
        string userId,
        IReadOnlyDictionary<string, Account> accounts,
        DateTime now)
    {
        if (await _context.Transfers.AnyAsync(
                transfer => transfer.UserId == userId))
        {
            return;
        }

        var transfers = new[]
        {
            CreateTransfer(
                userId,
                accounts["BPI Savings"],
                accounts["GCash"],
                2_500,
                now.AddDays(-24),
                "Monthly GCash funding",
                now),

            CreateTransfer(
                userId,
                accounts["BPI Savings"],
                accounts["Maya Wallet"],
                1_500,
                now.AddDays(-14),
                "Travel wallet funding",
                now),

            CreateTransfer(
                userId,
                accounts["Cash"],
                accounts["BPI Savings"],
                2_000,
                now.AddDays(-7),
                "Cash deposit",
                now)
        };

        _context.Transfers.AddRange(transfers);
        await _context.SaveChangesAsync();
    }

    private async Task SeedFinancialGoalsAsync(
        string userId,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, Account> accounts,
        DateTime now)
    {
        if (await _context.FinancialGoals.AnyAsync(
                goal => goal.UserId == userId))
        {
            return;
        }

        var goals = new[]
        {
            CreateGoal(
                userId,
                "Emergency Fund",
                100_000,
                12_000,
                now.Date.AddMonths(-6),
                now.Date.AddMonths(18),
                FinancialGoalStatus.Active,
                accounts["BPI Savings"],
                "#EF4444",
                "health_and_safety",
                "Build six months of essential expenses.",
                now),

            CreateGoal(
                userId,
                "Japan Trip",
                80_000,
                5_000,
                now.Date.AddMonths(-2),
                now.Date.AddMonths(10),
                FinancialGoalStatus.Active,
                accounts["Maya Wallet"],
                "#3B82F6",
                "flight",
                "Travel savings for flights, hotel, food, and activities.",
                now),

            CreateGoal(
                userId,
                "New Laptop",
                60_000,
                60_000,
                now.Date.AddMonths(-8),
                now.Date.AddMonths(-1),
                FinancialGoalStatus.Completed,
                accounts["BPI Savings"],
                "#8B5CF6",
                "laptop_mac",
                "Completed technology replacement fund.",
                now),

            CreateGoal(
                userId,
                "Certification Fund",
                25_000,
                4_000,
                now.Date.AddMonths(-3),
                now.Date.AddMonths(8),
                FinancialGoalStatus.Paused,
                accounts["GCash"],
                "#F59E0B",
                "school",
                "Temporarily paused professional certification fund.",
                now)
        };

        _context.FinancialGoals.AddRange(goals);
        await _context.SaveChangesAsync();

        var fundingCategory = categories["Financial Goal Funding"];
        var transactions = new[]
        {
            CreateTransaction(
                userId,
                TransactionType.Income,
                fundingCategory,
                accounts["BPI Savings"],
                5_000,
                "Emergency Fund contribution",
                now.AddDays(-45),
                now),

            CreateTransaction(
                userId,
                TransactionType.Income,
                fundingCategory,
                accounts["BPI Savings"],
                450,
                "Emergency Fund interest",
                now.AddDays(-15),
                now),

            CreateTransaction(
                userId,
                TransactionType.Income,
                fundingCategory,
                accounts["Maya Wallet"],
                7_500,
                "Japan Trip contribution",
                now.AddDays(-20),
                now)
        };

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        var goalContributions = new[]
        {
            CreateGoalContribution(
                userId,
                goals[0],
                accounts["BPI Savings"],
                5_000,
                now.AddDays(-45),
                "Monthly savings contribution",
                GoalContributionType.Manual,
                transactions[0],
                now),

            CreateGoalContribution(
                userId,
                goals[0],
                accounts["BPI Savings"],
                450,
                now.AddDays(-15),
                "Interest earned",
                GoalContributionType.Interest,
                transactions[1],
                now),

            CreateGoalContribution(
                userId,
                goals[1],
                accounts["Maya Wallet"],
                7_500,
                now.AddDays(-20),
                "Travel savings contribution",
                GoalContributionType.Manual,
                transactions[2],
                now)
        };

        _context.GoalContributions.AddRange(goalContributions);
        await _context.SaveChangesAsync();
    }

    private async Task SeedNotificationsAsync(
        string userId,
        DateTime now)
    {
        if (await _context.Notifications.AnyAsync(
                notification => notification.UserId == userId))
        {
            return;
        }

        var goals = await _context.FinancialGoals
            .Where(goal => goal.UserId == userId)
            .ToDictionaryAsync(goal => goal.Name);

        var notifications = new[]
        {
            CreateNotification(
                userId,
                NotificationType.BudgetWarning,
                NotificationPriority.High,
                "Food budget is nearing its limit",
                "You have used most of this month's Food budget.",
                false,
                now.AddHours(-3),
                "Budget",
                null,
                "/budgets",
                $"seed:{userId}:budget-warning",
                now),

            CreateNotification(
                userId,
                NotificationType.RecurringDue,
                NotificationPriority.Normal,
                "Upcoming subscription",
                "Your Spotify subscription is due soon.",
                false,
                now.AddDays(-1),
                "RecurringTransaction",
                null,
                "/recurring-transactions",
                $"seed:{userId}:recurring-due",
                now),

            CreateNotification(
                userId,
                NotificationType.GoalCompleted,
                NotificationPriority.High,
                "Goal completed",
                "Your New Laptop savings goal is complete.",
                false,
                now.AddDays(-2),
                "FinancialGoal",
                goals["New Laptop"].Id,
                $"/financial-goals/{goals["New Laptop"].Id}",
                $"seed:{userId}:goal-completed",
                now),

            CreateNotification(
                userId,
                NotificationType.AccountLowBalance,
                NotificationPriority.Critical,
                "Check your Cash balance",
                "Your available Cash balance may need replenishment.",
                false,
                now.AddDays(-3),
                "Account",
                null,
                "/accounts",
                $"seed:{userId}:low-balance",
                now),

            CreateNotification(
                userId,
                NotificationType.MonthlySummary,
                NotificationPriority.Low,
                "Monthly financial summary ready",
                "Your monthly income, expenses, savings, and budget summary is ready.",
                true,
                now.AddDays(-5),
                "Report",
                null,
                "/reports",
                $"seed:{userId}:monthly-summary",
                now)
        };

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    private async Task SeedAuditLogsAsync(
        string userId,
        DateTime now)
    {
        const string tracePrefix = "dev-seed-audit-";

        var accountId = await _context.Accounts
            .Where(account =>
                account.UserId == userId &&
                account.Name == "BPI Savings")
            .Select(account => (int?)account.Id)
            .FirstOrDefaultAsync();

        var categoryId = await _context.Categories
            .Where(category =>
                category.UserId == userId &&
                category.Name == "Food")
            .Select(category => (int?)category.Id)
            .FirstOrDefaultAsync();

        var transactionId = await _context.Transactions
            .Where(transaction => transaction.UserId == userId)
            .OrderBy(transaction => transaction.Id)
            .Select(transaction => (int?)transaction.Id)
            .FirstOrDefaultAsync();

        var transferId = await _context.Transfers
            .Where(transfer => transfer.UserId == userId)
            .OrderBy(transfer => transfer.Id)
            .Select(transfer => (int?)transfer.Id)
            .FirstOrDefaultAsync();

        var budgetId = await _context.Budgets
            .Where(budget => budget.UserId == userId)
            .OrderBy(budget => budget.Id)
            .Select(budget => (int?)budget.Id)
            .FirstOrDefaultAsync();

        var recurringTransactionId =
            await _context.RecurringTransactions
                .Where(recurring => recurring.UserId == userId)
                .OrderBy(recurring => recurring.Id)
                .Select(recurring => (int?)recurring.Id)
                .FirstOrDefaultAsync();

        var financialGoalId = await _context.FinancialGoals
            .Where(goal =>
                goal.UserId == userId &&
                goal.Name == "Emergency Fund")
            .Select(goal => (int?)goal.Id)
            .FirstOrDefaultAsync();

        var notificationId = await _context.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderBy(notification => notification.Id)
            .Select(notification => (int?)notification.Id)
            .FirstOrDefaultAsync();

        string? EntityId(int? id) => id?.ToString();

        string Route(string module, int? id, string? suffix = null)
        {
            var route = id.HasValue
                ? $"/api/v1/{module}/{id.Value}"
                : $"/api/v1/{module}";

            return string.IsNullOrWhiteSpace(suffix)
                ? route
                : $"{route}/{suffix}";
        }

        AuditLog Log(
            int sequence,
            string method,
            string module,
            string operation,
            string? entityId,
            string route,
            string action,
            int statusCode,
            long elapsedMilliseconds,
            DateTime createdAtUtc)
        {
            return new AuditLog
            {
                UserId = userId,
                Method = method,
                Module = module,
                Operation = operation,
                EntityId = entityId,
                Route = route,
                Action = action,
                StatusCode = statusCode,
                Succeeded = statusCode is >= 200 and < 400,
                ElapsedMilliseconds = elapsedMilliseconds,
                TraceId = $"{tracePrefix}{sequence:D2}",
                CreatedAtUtc = createdAtUtc
            };
        }

        var sampleLogs = new[]
        {
            Log(
                1,
                "POST",
                "Auth",
                "Login",
                null,
                "/api/v1/auth/login",
                "Auth.Login",
                200,
                142,
                now.AddDays(-14)),

            Log(
                2,
                "POST",
                "Accounts",
                "Create",
                EntityId(accountId),
                "/api/v1/accounts",
                "Accounts.Create",
                201,
                96,
                now.AddDays(-13)),

            Log(
                3,
                "POST",
                "Categories",
                "Create",
                EntityId(categoryId),
                "/api/v1/categories",
                "Categories.Create",
                201,
                54,
                now.AddDays(-12)),

            Log(
                4,
                "POST",
                "Transactions",
                "Create",
                EntityId(transactionId),
                "/api/v1/transactions",
                "Transactions.Create",
                201,
                88,
                now.AddDays(-11).AddHours(-2)),

            Log(
                5,
                "PUT",
                "Transactions",
                "Update",
                EntityId(transactionId),
                Route("transactions", transactionId),
                "Transactions.Update",
                200,
                73,
                now.AddDays(-10)),

            Log(
                6,
                "POST",
                "Transfers",
                "Create",
                EntityId(transferId),
                "/api/v1/transfers",
                "Transfers.Create",
                201,
                105,
                now.AddDays(-9).AddHours(-3)),

            Log(
                7,
                "PUT",
                "Transfers",
                "Update",
                EntityId(transferId),
                Route("transfers", transferId),
                "Transfers.Update",
                200,
                91,
                now.AddDays(-9)),

            Log(
                8,
                "POST",
                "Budgets",
                "Create",
                EntityId(budgetId),
                "/api/v1/budgets",
                "Budgets.Create",
                201,
                67,
                now.AddDays(-8)),

            Log(
                9,
                "PUT",
                "Budgets",
                "Update",
                EntityId(budgetId),
                Route("budgets", budgetId),
                "Budgets.Update",
                200,
                61,
                now.AddDays(-7)),

            Log(
                10,
                "POST",
                "RecurringTransactions",
                "Create",
                EntityId(recurringTransactionId),
                "/api/v1/recurring-transactions",
                "RecurringTransactions.Create",
                201,
                84,
                now.AddDays(-6)),

            Log(
                11,
                "POST",
                "RecurringTransactions",
                "GenerateDue",
                null,
                "/api/v1/recurring-transactions/generate-due",
                "RecurringTransactions.GenerateDue",
                200,
                126,
                now.AddDays(-5)),

            Log(
                12,
                "POST",
                "FinancialGoals",
                "Create",
                EntityId(financialGoalId),
                "/api/v1/financial-goals",
                "FinancialGoals.Create",
                201,
                113,
                now.AddDays(-4).AddHours(-2)),

            Log(
                13,
                "POST",
                "FinancialGoals",
                "AddContribution",
                EntityId(financialGoalId),
                Route("financial-goals", financialGoalId, "contributions"),
                "FinancialGoals.AddContribution",
                200,
                119,
                now.AddDays(-4)),

            Log(
                14,
                "PATCH",
                "Notifications",
                "MarkAsRead",
                EntityId(notificationId),
                Route("notifications", notificationId, "read"),
                "Notifications.MarkAsRead",
                204,
                42,
                now.AddDays(-3)),

            Log(
                15,
                "POST",
                "Notifications",
                "Generate",
                null,
                "/api/v1/notifications/generate",
                "Notifications.Generate",
                200,
                157,
                now.AddDays(-2)),

            Log(
                16,
                "DELETE",
                "Transactions",
                "Delete",
                "999999",
                "/api/v1/transactions/999999",
                "Transactions.Delete",
                404,
                39,
                now.AddHours(-6)),

            Log(
                17,
                "POST",
                "Transfers",
                "Create",
                null,
                "/api/v1/transfers",
                "Transfers.Create",
                409,
                64,
                now.AddHours(-2)),

            Log(
                18,
                "POST",
                "Auth",
                "Logout",
                null,
                "/api/v1/auth/logout",
                "Auth.Logout",
                204,
                31,
                now.AddMinutes(-30))
        };

        var existingTraceIds = (await _context.AuditLogs
                .Where(audit =>
                    audit.UserId == userId &&
                    audit.TraceId.StartsWith(tracePrefix))
                .Select(audit => audit.TraceId)
                .ToListAsync())
            .ToHashSet(StringComparer.Ordinal);

        var missingLogs = sampleLogs
            .Where(log => !existingTraceIds.Contains(log.TraceId))
            .ToList();

        if (missingLogs.Count == 0)
            return;

        _context.AuditLogs.AddRange(missingLogs);
        await _context.SaveChangesAsync();
    }

    private static Category CreateCategory(
        string userId,
        string name,
        string color,
        string icon,
        DateTime now)
    {
        return new Category
        {
            UserId = userId,
            Name = name,
            Color = color,
            Icon = icon,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Account CreateAccount(
        string userId,
        string name,
        AccountType type,
        decimal openingBalance,
        string color,
        string icon,
        DateTime now)
    {
        return new Account
        {
            UserId = userId,
            Name = name,
            Type = type,
            OpeningBalance = openingBalance,
            Currency = "PHP",
            Color = color,
            Icon = icon,
            IncludeInNetWorth = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Transfer CreateTransfer(
        string userId,
        Account fromAccount,
        Account toAccount,
        decimal amount,
        DateTime transferDate,
        string notes,
        DateTime now)
    {
        return new Transfer
        {
            UserId = userId,
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = amount,
            TransferDate = transferDate,
            Notes = notes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static FinancialGoal CreateGoal(
        string userId,
        string name,
        decimal targetAmount,
        decimal startingAmount,
        DateTime startDate,
        DateTime? targetDate,
        FinancialGoalStatus status,
        Account account,
        string color,
        string icon,
        string notes,
        DateTime now)
    {
        return new FinancialGoal
        {
            UserId = userId,
            Name = name,
            TargetAmount = targetAmount,
            StartingAmount = startingAmount,
            StartDate = startDate,
            TargetDate = targetDate,
            Status = status,
            AccountId = account.Id,
            Color = color,
            Icon = icon,
            Notes = notes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static GoalContribution CreateGoalContribution(
        string userId,
        FinancialGoal goal,
        Account account,
        decimal amount,
        DateTime contributionDate,
        string notes,
        GoalContributionType contributionType,
        Transaction transaction,
        DateTime now)
    {
        return new GoalContribution
        {
            UserId = userId,
            FinancialGoalId = goal.Id,
            AccountId = account.Id,
            Amount = amount,
            ContributionDate = contributionDate,
            Notes = notes,
            ContributionType = contributionType,
            TransactionId = transaction.Id,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Notification CreateNotification(
        string userId,
        NotificationType type,
        NotificationPriority priority,
        string title,
        string message,
        bool isRead,
        DateTime occurredAt,
        string referenceType,
        int? referenceId,
        string actionUrl,
        string uniqueKey,
        DateTime now)
    {
        return new Notification
        {
            UserId = userId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            IsRead = isRead,
            ReadAt = isRead ? occurredAt.AddHours(1) : null,
            OccurredAt = occurredAt,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ActionUrl = actionUrl,
            UniqueKey = uniqueKey,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Transaction CreateTransaction(
        string userId,
        TransactionType type,
        Category category,
        Account account,
        decimal amount,
        string notes,
        DateTime date,
        DateTime now)
    {
        return new Transaction
        {
            UserId = userId,
            Type = type,
            CategoryId = category.Id,
            AccountId = account.Id,
            Amount = amount,
            Notes = notes,
            Date = date,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Budget CreateBudget(
        string userId,
        Category category,
        DateTime date,
        decimal amount)
    {
        return new Budget
        {
            UserId = userId,
            CategoryId = category.Id,
            Year = date.Year,
            Month = date.Month,
            Amount = amount,
            IsActive = true,
            CreatedAt = date,
            UpdatedAt = date
        };
    }

    private static RecurringTransaction CreateRecurring(
        string userId,
        TransactionType type,
        Category category,
        Account account,
        decimal amount,
        string notes,
        int dayOfMonth,
        DateTime startDate,
        DateTime now)
    {
        return new RecurringTransaction
        {
            UserId = userId,
            Type = type,
            CategoryId = category.Id,
            AccountId = account.Id,
            Amount = amount,
            Notes = notes,
            DayOfMonth = dayOfMonth,
            StartDate = startDate,
            EndDate = null,
            NextRunDate = CalculateNextOccurrence(
                now,
                dayOfMonth),
            LastRunDate = null,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static DateTime CalculateNextOccurrence(
        DateTime today,
        int dayOfMonth)
    {
        var date = today.Date;

        var validDay = Math.Min(
            dayOfMonth,
            DateTime.DaysInMonth(
                date.Year,
                date.Month));

        var candidate = new DateTime(
            date.Year,
            date.Month,
            validDay,
            0,
            0,
            0,
            DateTimeKind.Utc);

        if (candidate >= date)
            return candidate;

        var nextMonth = new DateTime(
            date.Year,
            date.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc)
            .AddMonths(1);

        validDay = Math.Min(
            dayOfMonth,
            DateTime.DaysInMonth(
                nextMonth.Year,
                nextMonth.Month));

        return new DateTime(
            nextMonth.Year,
            nextMonth.Month,
            validDay,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }
}
