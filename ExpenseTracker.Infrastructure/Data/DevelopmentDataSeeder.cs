using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Data;

public class DevelopmentDataSeeder
{
    private const string TestEmail =
        "testuser@expensetracker.local";

    private const string TestPassword =
        "Password1";

    private readonly ExpenseTrackerDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DevelopmentDataSeeder(
        ExpenseTrackerDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

        await SeedBudgetsAsync(
            user.Id,
            categories,
            now);

        await SeedRecurringTransactionsAsync(
            user.Id,
            categories,
            accounts,
            now);
    }

    private async Task<ApplicationUser> CreateUserAsync()
    {
        var existingUser =
            await _userManager.FindByEmailAsync(TestEmail);

        if (existingUser != null)
            return existingUser;

        var user = new ApplicationUser
        {
            FullName = "Expense Tracker Test User",
            Email = TestEmail,
            UserName = TestEmail,
            EmailConfirmed = true
        };

        var result =
            await _userManager.CreateAsync(
                user,
                TestPassword);

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
        if (await _context.Categories.AnyAsync(
                x => x.UserId == userId))
        {
            return;
        }

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
                now)
        };

        _context.Categories.AddRange(categories);

        await _context.SaveChangesAsync();
    }

    private async Task SeedAccountsAsync(
        string userId,
        DateTime now)
    {
        if (await _context.Accounts.AnyAsync(
                x => x.UserId == userId))
        {
            return;
        }

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

        _context.Accounts.AddRange(accounts);

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