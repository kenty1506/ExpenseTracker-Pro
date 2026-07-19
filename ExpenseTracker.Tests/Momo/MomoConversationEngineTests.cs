using ExpenseTracker.Api.DTOs.Momo;
using ExpenseTracker.Api.Services.Momo;

namespace ExpenseTracker.Tests.Momo;

public sealed class MomoConversationEngineTests
{
    private readonly MomoConversationEngine _engine = new();

    [Fact]
    public void Reply_UsesExactAccountTotalsForNetWorth()
    {
        var response = Reply("What is my net worth?");

        Assert.Contains("PHP 8,000.00", response.Message);
        Assert.Contains("PHP 10,000.00", response.Message);
        Assert.Equal("/accounts", response.SuggestedRoute);
        Assert.True(response.IsSmartResponse);
    }

    [Fact]
    public void Reply_AnswersTaglishInTaglish()
    {
        var response = Reply("Magkano pera ko?");

        Assert.StartsWith("Net worth mo", response.Message);
        Assert.Contains("PHP 8,000.00", response.Message);
    }

    [Fact]
    public void Reply_TreatsKamustaFinancesAsAHealthQuestion()
    {
        var response = Reply("Kamusta finances ko?");

        Assert.Contains("cash flow mo", response.Message);
        Assert.Contains("Savings rate mo", response.Message);
        Assert.NotNull(response.SuggestedRoute);
    }

    [Fact]
    public void Reply_ExplainsANamedCategoryBudget()
    {
        var response = Reply("How is my Food budget?");

        Assert.Contains("For Food", response.Message);
        Assert.Contains("PHP 1,500.00", response.Message);
        Assert.Contains("PHP 2,000.00", response.Message);
        Assert.Equal("/budgets", response.SuggestedRoute);
    }

    [Fact]
    public void Reply_UsesRecentConversationForAShortFollowUp()
    {
        var request = new MomoChatRequest
        {
            Message = "Tell me more",
            CurrentPath = "/budgets",
            History =
            [
                new MomoConversationMessage
                {
                    Role = "user",
                    Text = "Which category needs attention?"
                },
                new MomoConversationMessage
                {
                    Role = "assistant",
                    Text = "Transport needs attention before Food."
                }
            ]
        };

        var response = _engine.Reply(request, CreateSnapshot());

        Assert.Contains("For Transport", response.Message);
        Assert.Contains("over by PHP 100.00", response.Message);
    }

    [Fact]
    public void Reply_ReservesUpcomingBillsWhenCheckingAffordability()
    {
        var response = Reply("Can I afford PHP 6,500?");

        Assert.Contains("does not look safe", response.Message);
        Assert.Contains("PHP 500.00", response.Message);
    }

    [Fact]
    public void Reply_PrioritizesTheClosestActiveGoal()
    {
        var response = Reply("Which goal should I prioritize?");

        Assert.Contains("Prioritize Travel Fund", response.Message);
        Assert.Contains("PHP 1,000.00 per month", response.Message);
        Assert.Equal("/financial-goals", response.SuggestedRoute);
    }

    [Fact]
    public void Reply_ComparesSpendingWithThePreviousMonth()
    {
        var response = Reply("How is my spending compared to last month?");

        Assert.Contains("higher", response.Message);
        Assert.Contains("20.0%", response.Message);
        Assert.Equal("/reports", response.SuggestedRoute);
    }

    [Fact]
    public void Reply_CalculatesSafeSpendingRoomWithoutAModel()
    {
        var response = Reply("How much is safe to spend?");

        Assert.Contains("PHP 6,000.00", response.Message);
        Assert.Contains("upcoming 30-day bills", response.Message);
    }

    [Fact]
    public void Reply_ResolvesCategoryComparisonFromFollowUpContext()
    {
        var request = new MomoChatRequest
        {
            Message = "How does that compare with last month?",
            History =
            [
                new MomoConversationMessage
                {
                    Role = "assistant",
                    Text = "Food is your largest expense category this month."
                }
            ]
        };

        var response = _engine.Reply(request, CreateSnapshot());

        Assert.Contains("Food spending", response.Message);
        Assert.Contains("50.0% higher", response.Message);
    }

    private MomoChatResponse Reply(string message) =>
        _engine.Reply(
            new MomoChatRequest
            {
                Message = message,
                CurrentPath = "/dashboard"
            },
            CreateSnapshot());

    private static MomoFinanceSnapshot CreateSnapshot() =>
        new(
            new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc),
            "/dashboard",
            new MomoMonthlyContext(
                2026,
                7,
                "July",
                5_000m,
                3_000m,
                2_000m,
                10),
            new MomoMonthlyContext(
                2026,
                6,
                "June",
                4_500m,
                2_500m,
                2_000m,
                9),
            new MomoAccountSummaryContext(
                10_000m,
                2_000m,
                8_000m,
                [
                    new MomoAccountContext(
                        "Main Wallet",
                        "Checking",
                        7_000m,
                        "PHP",
                        true,
                        20),
                    new MomoAccountContext(
                        "Credit Card",
                        "CreditCard",
                        -2_000m,
                        "PHP",
                        true,
                        6)
                ]),
            [
                new MomoCategoryContext("Food", 1_500m, 1_000m, 5),
                new MomoCategoryContext("Transport", 500m, 600m, 3)
            ],
            [
                new MomoBudgetContext(
                    "Transport",
                    400m,
                    500m,
                    -100m,
                    125m,
                    true,
                    3),
                new MomoBudgetContext(
                    "Food",
                    2_000m,
                    1_500m,
                    500m,
                    75m,
                    false,
                    5)
            ],
            new MomoGoalSummaryContext(
                1,
                1,
                0,
                6_000m,
                3_000m,
                3_000m,
                50m),
            [
                new MomoGoalContext(
                    "Travel Fund",
                    6_000m,
                    3_000m,
                    3_000m,
                    50m,
                    new DateTime(2026, 10, 17),
                    90,
                    "Active",
                    false,
                    "Main Wallet")
            ],
            [
                new MomoRecurringContext(
                    "Expense",
                    "Utilities",
                    "Main Wallet",
                    1_000m,
                    new DateTime(2026, 7, 25),
                    6,
                    false,
                    false)
            ],
            [
                new MomoTransactionContext(
                    "Expense",
                    "Food",
                    "Main Wallet",
                    500m,
                    new DateTime(2026, 7, 18))
            ]);
}
