using System.Globalization;
using System.Text.RegularExpressions;
using ExpenseTracker.Api.DTOs.Momo;

namespace ExpenseTracker.Api.Services.Momo;

public sealed partial class MomoConversationEngine
{
    private static readonly IReadOnlyDictionary<string, NavigationTarget>
        NavigationTargets =
            new Dictionary<string, NavigationTarget>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["dashboard"] = new("/dashboard", "Open dashboard"),
                ["transactions"] = new("/expenses", "Review transactions"),
                ["add_transaction"] = new("/expenses?new=1", "Add transaction"),
                ["accounts"] = new("/accounts", "Open accounts"),
                ["budgets"] = new("/budgets", "Review budgets"),
                ["budget_forecast"] = new("/budget-forecast", "Open forecast"),
                ["goals"] = new("/financial-goals", "Open goals"),
                ["recurring"] = new("/recurring-transactions", "Review recurring items"),
                ["reports"] = new("/reports", "Open reports"),
                ["transfers"] = new("/transfers", "Open transfers"),
                ["settings"] = new("/settings", "Open settings")
            };

    public MomoChatResponse Reply(
        MomoChatRequest request,
        MomoFinanceSnapshot snapshot)
    {
        var directMessage = Normalize(request.Message);
        var conversationText = BuildConversationText(
            directMessage,
            request.History);
        var taglish = LooksTaglish(directMessage);

        if (ContainsAny(
                directMessage,
                "hello",
                "hi momo",
                "hey momo",
                "good morning",
                "good afternoon",
                "good evening"))
        {
            return Response(
                taglish
                    ? "Hello! Ready ako. Pwede mong itanong ang balance, gastos, budget, goals, o upcoming bills mo."
                    : "Hello! I’m ready. Ask about your balance, spending, budgets, goals, or upcoming bills.",
                "none",
                [
                    "How am I doing this month?",
                    "Where am I spending the most?",
                    "Can I cover my upcoming bills?"
                ]);
        }

        if (ContainsAny(
                directMessage,
                "what can i do here",
                "what is this page",
                "help with this page",
                "ano pwede dito",
                "anong pwede dito"))
        {
            return PageGuidance(snapshot.CurrentPath, taglish);
        }

        if (ContainsAny(
                directMessage,
                "add expense",
                "record expense",
                "new transaction",
                "add transaction",
                "mag add ng expense",
                "maglagay ng expense"))
        {
            return Response(
                taglish
                    ? "Buksan ang New transaction, piliin ang tamang account at category, tapos ilagay ang amount at date. Automatic na mag-a-update ang balance, budget, at reports."
                    : "Open New transaction, choose the correct account and category, then enter the amount and date. Balances, budgets, and reports update automatically.",
                "add_transaction",
                ["How should I choose a category?"]);
        }

        if (ContainsAny(
                directMessage,
                "choose a category",
                "which category should",
                "anong category"))
        {
            return Response(
                taglish
                    ? "Piliin ang category na pinakamalinaw na purpose ng transaction. Kung paulit-ulit ang same merchant o bill, gamitin palagi ang parehong category para accurate ang budget at trends."
                    : "Choose the category that best describes the transaction's purpose. Use the same category for repeat merchants or bills so budgets and trends stay accurate.",
                "add_transaction",
                ["Add a transaction"]);
        }

        if (ContainsAny(
                directMessage,
                "add account",
                "create account",
                "first account"))
        {
            return Response(
                taglish
                    ? "Buksan ang Accounts at idagdag ang wallet, bank, cash, o credit account na gusto mong i-track. Ilagay ang tamang opening balance para accurate agad ang net worth."
                    : "Open Accounts and add the wallet, bank, cash, or credit account you want to track. Enter the correct opening balance so net worth starts accurately.",
                "accounts",
                ["Open accounts"]);
        }

        var matchedAccount = FindNamedAccount(
            snapshot.Accounts.Items,
            conversationText);
        var matchedBudget = FindNamedBudget(
            snapshot.Budgets,
            conversationText);
        var matchedCategory = FindNamedCategory(
            snapshot.Categories,
            conversationText);
        var matchedGoal = FindNamedGoal(
            snapshot.Goals,
            conversationText);

        if (matchedAccount is not null &&
            ContainsAny(
                conversationText,
                "balance",
                "account",
                "how much",
                "magkano",
                "included"))
        {
            return AccountAnswer(matchedAccount, taglish);
        }

        if (matchedGoal is not null)
        {
            return GoalAnswer(
                matchedGoal,
                directMessage,
                snapshot,
                taglish);
        }

        if (matchedBudget is not null ||
            matchedCategory is not null &&
            ContainsAny(
                conversationText,
                "spend",
                "spent",
                "expense",
                "budget",
                "gastos",
                "ginastos"))
        {
            return CategoryAnswer(
                matchedBudget,
                matchedCategory,
                snapshot,
                conversationText,
                taglish);
        }

        if (ContainsAny(
                directMessage,
                "which account has",
                "highest account",
                "largest account",
                "most money",
                "pinakamalaking account"))
        {
            var highestAccount = snapshot.Accounts.Items
                .OrderByDescending(item => item.CurrentBalance)
                .FirstOrDefault();

            return highestAccount is null
                ? Response(
                    taglish
                        ? "Wala ka pang active account. Gumawa muna ng account para maikumpara ko ang balances."
                        : "You do not have an active account yet. Add one so I can compare balances.",
                    "accounts",
                    ["Create my first account"])
                : Response(
                    taglish
                        ? $"{highestAccount.Name} ang may pinakamataas na balance: {FormatMoney(highestAccount.CurrentBalance, highestAccount.Currency)}."
                        : $"{highestAccount.Name} has your highest current balance at {FormatMoney(highestAccount.CurrentBalance, highestAccount.Currency)}.",
                    "accounts",
                    ["Which accounts count toward my net worth?"]);
        }

        if (ContainsAny(
                directMessage,
                "improve my net worth",
                "increase my net worth",
                "grow my net worth",
                "paano tataas net worth"))
        {
            return NetWorthAdvice(snapshot, taglish);
        }

        if (ContainsAny(
                directMessage,
                "accounts count toward",
                "accounts included",
                "included in my net worth"))
        {
            return IncludedAccountsAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "net worth",
                "total balance",
                "all my money",
                "how much money do i have",
                "magkano pera ko",
                "assets",
                "liabilities"))
        {
            return NetWorthAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                directMessage,
                "how am i doing",
                "financial health",
                "money health",
                "financially",
                "kamusta finances",
                "kamusta pera"))
        {
            return FinancialHealthAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "which category needs attention",
                "worst budget",
                "closest to limit",
                "over budget category",
                "anong category ang delikado"))
        {
            return BudgetRiskAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "plan next month",
                "next month budget",
                "recommended budget",
                "budget next month"))
        {
            return NextBudgetAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "biggest budget",
                "largest budget next month",
                "category needs the biggest budget"))
        {
            return BudgetCategoryRecommendation(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "budget",
                "overspend",
                "over budget",
                "spending limit",
                "budget ko"))
        {
            return BudgetSummaryAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "can i afford",
                "can i buy",
                "kaya ko ba",
                "pwede ko ba bilhin"))
        {
            var amount = ExtractAmount(directMessage);

            if (amount.HasValue)
            {
                return AffordabilityAnswer(
                    amount.Value,
                    snapshot,
                    taglish);
            }
        }

        if (ContainsAny(
                directMessage,
                "safe to spend",
                "spendable amount",
                "available to spend",
                "magkano safe gastusin"))
        {
            return SafeToSpendAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "upcoming bill",
                "upcoming payment",
                "recurring",
                "subscription",
                "next bill",
                "mga bill",
                "bayarin"))
        {
            return UpcomingAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "which goal",
                "goal priority",
                "prioritize goal",
                "goal first",
                "anong goal"))
        {
            return GoalPriorityAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "goal",
                "saving target",
                "savings goal",
                "ipon goal"))
        {
            return GoalSummaryAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                directMessage,
                "recent transaction",
                "latest transaction",
                "last transaction",
                "recent activity",
                "huling transaction"))
        {
            return RecentTransactionsAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "category changed the most",
                "biggest category change",
                "which category changed"))
        {
            return ChangedCategoryAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "where am i overspending",
                "where can i cut",
                "spending the most",
                "top category",
                "largest category",
                "saan pinakamalaki gastos"))
        {
            return TopSpendingAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "trend",
                "compared to last month",
                "versus last month",
                "better than last month",
                "kumpara last month"))
        {
            return TrendAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "spending",
                "spent",
                "expenses",
                "expense this month",
                "gastos",
                "ginastos"))
        {
            return SpendingSummaryAnswer(snapshot, taglish);
        }

        if (ContainsAny(
                conversationText,
                "savings rate",
                "how much should i save",
                "how much can i save",
                "magkano dapat ipon"))
        {
            return SavingsAnswer(snapshot, taglish);
        }

        if (ContainsAny(directMessage, "transfer", "move money"))
        {
            return Response(
                "Use Transfers to move money between accounts without counting it as income or spending.",
                "transfers",
                ["Open transfers"]);
        }

        if (ContainsAny(
                directMessage,
                "report",
                "chart",
                "export",
                "analysis"))
        {
            return Response(
                "Reports is best for period comparisons, category trends, and exports. Choose the date range that matches the decision you are making.",
                "reports",
                ["How does this month compare with last month?"]);
        }

        return SmartFallback(snapshot, taglish);
    }

    private static MomoChatResponse FinancialHealthAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var month = snapshot.CurrentMonth;

        if (month.TransactionCount == 0)
        {
            return Response(
                taglish
                    ? "Wala pang transactions ngayong buwan, kaya kulang pa ang data para sa solid na assessment. Mag-record muna ng income at expenses; pagkatapos makikita natin ang savings rate at spending pressure mo."
                    : "There are no transactions this month, so there is not enough data for a solid assessment yet. Record income and expenses first, then I can evaluate your savings rate and spending pressure.",
                "add_transaction",
                ["Add my first transaction", "What should I track first?"]);
        }

        var savingsRate = GetSavingsRate(month);
        var overBudget = snapshot.Budgets.Count(item => item.IsOverBudget);
        var tone = savingsRate >= 20
            ? "strong"
            : savingsRate >= 0
                ? "stable but improvable"
                : "under pressure";

        var answer = taglish
            ? $"Ngayong {month.MonthName}, {tone} ang cash flow mo: {FormatMoney(month.Income, GetCurrency(snapshot))} income versus {FormatMoney(month.Expense, GetCurrency(snapshot))} expenses. Savings rate mo ay {FormatPercent(savingsRate)} at net worth ay {FormatMoney(snapshot.Accounts.NetWorth, GetCurrency(snapshot))}. {(overBudget > 0 ? $"May {overBudget} budget {(overBudget == 1 ? "category" : "categories")} na lampas sa limit—iyon ang unahin." : "Wala pang budget category na lampas sa limit.")}"
            : $"Your {month.MonthName} cash flow looks {tone}: {FormatMoney(month.Income, GetCurrency(snapshot))} income versus {FormatMoney(month.Expense, GetCurrency(snapshot))} expenses. Your savings rate is {FormatPercent(savingsRate)} and net worth is {FormatMoney(snapshot.Accounts.NetWorth, GetCurrency(snapshot))}. {(overBudget > 0 ? $"You have {overBudget} budget {(overBudget == 1 ? "category" : "categories")} over the limit; address those first." : "No budget category is currently over its limit.")}";

        return Response(
            answer,
            overBudget > 0 ? "budgets" : "dashboard",
            [
                "Where am I spending the most?",
                "Can I cover my upcoming bills?",
                "How can I improve my net worth?"
            ]);
    }

    private static MomoChatResponse NetWorthAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var accounts = snapshot.Accounts;
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Net worth mo ay {FormatMoney(accounts.NetWorth, currency)}: {FormatMoney(accounts.TotalAssets, currency)} assets minus {FormatMoney(accounts.TotalLiabilities, currency)} liabilities. {accounts.Items.Count} active {(accounts.Items.Count == 1 ? "account" : "accounts")} ang kasama sa analysis."
                : $"Your net worth is {FormatMoney(accounts.NetWorth, currency)}: {FormatMoney(accounts.TotalAssets, currency)} in assets minus {FormatMoney(accounts.TotalLiabilities, currency)} in liabilities. The analysis covers {accounts.Items.Count} active {(accounts.Items.Count == 1 ? "account" : "accounts")}.",
            "accounts",
            [
                "Which account has the highest balance?",
                "How can I improve my net worth?"
            ]);
    }

    private static MomoChatResponse NetWorthAdvice(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var month = snapshot.CurrentMonth;
        var surplus = month.Balance;
        var topCategory = snapshot.Categories
            .OrderByDescending(item => item.CurrentMonthAmount)
            .FirstOrDefault();
        var currency = GetCurrency(snapshot);

        var action = surplus > 0
            ? taglish
                ? $"May {FormatMoney(surplus, currency)} kang monthly surplus. Maglaan ng fixed portion nito sa priority goal bago magdagdag ng bagong recurring expense."
                : $"You have a {FormatMoney(surplus, currency)} monthly surplus. Direct a fixed portion to your priority goal before adding a new recurring expense."
            : taglish
                ? $"Negative ng {FormatMoney(Math.Abs(surplus), currency)} ang monthly cash flow mo. Bawasan muna ang flexible spending{(topCategory is null ? string.Empty : $", starting with {topCategory.Name}")}."
                : $"Your monthly cash flow is negative by {FormatMoney(Math.Abs(surplus), currency)}. Reduce flexible spending first{(topCategory is null ? string.Empty : $", starting with {topCategory.Name}")}.";

        return Response(
            action,
            surplus > 0 ? "goals" : "reports",
            ["Where am I spending the most?", "Which goal should I prioritize?"]);
    }

    private static MomoChatResponse AccountAnswer(
        MomoAccountContext account,
        bool taglish)
    {
        var included = account.IncludeInNetWorth
            ? "included in"
            : "excluded from";

        return Response(
            taglish
                ? $"Current balance ng {account.Name} ay {FormatMoney(account.CurrentBalance, account.Currency)}. {account.Type} account ito, may {account.TransactionCount} transactions, at {included} your net worth."
                : $"{account.Name} has a current balance of {FormatMoney(account.CurrentBalance, account.Currency)}. It is a {account.Type} account with {account.TransactionCount} transactions and is {included} your net worth.",
            "accounts",
            ["Which account has the highest balance?"]);
    }

    private static MomoChatResponse IncludedAccountsAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var included = snapshot.Accounts.Items
            .Where(item => item.IncludeInNetWorth)
            .Select(item => item.Name)
            .ToList();
        var excluded = snapshot.Accounts.Items
            .Where(item => !item.IncludeInNetWorth)
            .Select(item => item.Name)
            .ToList();

        var includedText = included.Count == 0
            ? taglish
                ? "Walang account na included ngayon."
                : "No account is currently included."
            : taglish
                ? $"Included sa net worth: {string.Join(", ", included)}."
                : $"Included in net worth: {string.Join(", ", included)}.";
        var excludedText = excluded.Count == 0
            ? string.Empty
            : taglish
                ? $" Excluded: {string.Join(", ", excluded)}."
                : $" Excluded: {string.Join(", ", excluded)}.";

        return Response(
            $"{includedText}{excludedText}",
            "accounts",
            ["What is my net worth?"]);
    }

    private static MomoChatResponse SpendingSummaryAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var month = snapshot.CurrentMonth;
        var dailyPace = snapshot.AsOfUtc.Day <= 0
            ? month.Expense
            : month.Expense / snapshot.AsOfUtc.Day;
        var top = snapshot.Categories
            .OrderByDescending(item => item.CurrentMonthAmount)
            .FirstOrDefault();
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Nakagastos ka ng {FormatMoney(month.Expense, currency)} ngayong {month.MonthName} across {month.TransactionCount} transactions. Average pace mo ay {FormatMoney(dailyPace, currency)} per calendar day.{(top is null ? string.Empty : $" Pinakamalaki ang {top.Name} sa {FormatMoney(top.CurrentMonthAmount, currency)}.")}"
                : $"You have spent {FormatMoney(month.Expense, currency)} this {month.MonthName} across {month.TransactionCount} transactions. That is an average pace of {FormatMoney(dailyPace, currency)} per calendar day.{(top is null ? string.Empty : $" {top.Name} is the largest category at {FormatMoney(top.CurrentMonthAmount, currency)}.")}",
            "transactions",
            ["Where can I cut spending?", "Show my recent transactions"]);
    }

    private static MomoChatResponse TopSpendingAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var top = snapshot.Categories
            .OrderByDescending(item => item.CurrentMonthAmount)
            .FirstOrDefault();

        if (top is null || top.CurrentMonthAmount <= 0)
        {
            return Response(
                taglish
                    ? "Wala pang categorized expenses ngayong buwan. Mag-record muna ng transactions para makita natin kung saan pinakamalaki ang gastos."
                    : "There are no categorized expenses this month yet. Record transactions first so I can identify where spending is concentrated.",
                "add_transaction",
                ["Add an expense"]);
        }

        var share = snapshot.CurrentMonth.Expense <= 0
            ? 0
            : top.CurrentMonthAmount / snapshot.CurrentMonth.Expense * 100;
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"{top.Name} ang pinakamalaking expense category mo ngayong buwan: {FormatMoney(top.CurrentMonthAmount, currency)}, o {FormatPercent(share)} ng total spending. I-review muna ang transactions dito bago mag-cut para malaman kung alin ang flexible."
                : $"{top.Name} is your largest expense category this month at {FormatMoney(top.CurrentMonthAmount, currency)}, or {FormatPercent(share)} of total spending. Review its transactions before cutting so you can separate flexible costs from essentials.",
            "reports",
            ["How does that compare with last month?", "Review my budgets"]);
    }

    private static MomoChatResponse CategoryAnswer(
        MomoBudgetContext? budget,
        MomoCategoryContext? category,
        MomoFinanceSnapshot snapshot,
        string conversationText,
        bool taglish)
    {
        var name = budget?.Category ?? category?.Name ?? "This category";
        var actual = budget?.Actual ?? category?.CurrentMonthAmount ?? 0;
        var currency = GetCurrency(snapshot);

        if (ContainsAny(
                conversationText,
                "compare",
                "last month",
                "changed"))
        {
            var categoryHistory = category ?? snapshot.Categories
                .FirstOrDefault(item => item.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase));

            if (categoryHistory is null ||
                categoryHistory.PreviousMonthAmount <= 0)
            {
                return Response(
                    taglish
                        ? $"Wala pang enough previous-month data para maikumpara nang maayos ang {name}."
                        : $"There is not enough previous-month data for a reliable {name} comparison yet.",
                    "reports",
                    ["Open reports"]);
            }

            var difference = categoryHistory.CurrentMonthAmount -
                categoryHistory.PreviousMonthAmount;
            var percent = Math.Abs(difference) /
                categoryHistory.PreviousMonthAmount * 100;
            var direction = difference > 0
                ? taglish ? "mas mataas" : "higher"
                : difference < 0
                    ? taglish ? "mas mababa" : "lower"
                    : taglish ? "pareho" : "unchanged";

            return Response(
                taglish
                    ? $"Ang {name} spending mo ay {FormatMoney(categoryHistory.CurrentMonthAmount, currency)} ngayong buwan, {FormatPercent(percent)} {direction} kumpara sa {FormatMoney(categoryHistory.PreviousMonthAmount, currency)} last month."
                    : $"{name} spending is {FormatMoney(categoryHistory.CurrentMonthAmount, currency)} this month, {FormatPercent(percent)} {direction} than {FormatMoney(categoryHistory.PreviousMonthAmount, currency)} last month.",
                "reports",
                ["Review my budgets"]);
        }

        if (budget is null)
        {
            return Response(
                taglish
                    ? $"{FormatMoney(actual, currency)} ang nagastos mo sa {name} ngayong buwan, pero wala itong budget limit. Mag-set ng budget kung gusto mong ma-track ang remaining allowance."
                    : $"You have spent {FormatMoney(actual, currency)} on {name} this month, but it has no budget limit. Add one if you want MoMo to track the remaining allowance.",
                "budgets",
                ["Create a budget", "How much should I budget next month?"]);
        }

        var status = budget.IsOverBudget
            ? taglish
                ? $"Lampas ka ng {FormatMoney(Math.Abs(budget.Remaining), currency)}."
                : $"You are over by {FormatMoney(Math.Abs(budget.Remaining), currency)}."
            : taglish
                ? $"May {FormatMoney(budget.Remaining, currency)} ka pang natitira."
                : $"You have {FormatMoney(budget.Remaining, currency)} remaining.";

        return Response(
            taglish
                ? $"Sa {name}, nagamit mo na ang {FormatMoney(budget.Actual, currency)} sa {FormatMoney(budget.Budget, currency)} budget ({FormatPercent(budget.PercentageUsed)}). {status}"
                : $"For {name}, you have used {FormatMoney(budget.Actual, currency)} of the {FormatMoney(budget.Budget, currency)} budget ({FormatPercent(budget.PercentageUsed)}). {status}",
            "budgets",
            ["Which category needs attention?", "Help me plan next month’s budget"]);
    }

    private static MomoChatResponse BudgetSummaryAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        if (snapshot.Budgets.Count == 0)
        {
            return Response(
                taglish
                    ? "Wala ka pang budget ngayong buwan. Mag-set ng limit per category para maikumpara ko ang plan at actual spending mo."
                    : "You do not have a budget this month. Set category limits so I can compare your plan with actual spending.",
                "budgets",
                ["Create my first budget"]);
        }

        var totalBudget = snapshot.Budgets.Sum(item => item.Budget);
        var totalActual = snapshot.Budgets.Sum(item => item.Actual);
        var remaining = totalBudget - totalActual;
        var overCount = snapshot.Budgets.Count(item => item.IsOverBudget);
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Nagamit mo na ang {FormatMoney(totalActual, currency)} sa {FormatMoney(totalBudget, currency)} total budget ngayong buwan. {(remaining >= 0 ? $"May {FormatMoney(remaining, currency)} pang natitira." : $"Lampas ka ng {FormatMoney(Math.Abs(remaining), currency)}.")} {overCount} {(overCount == 1 ? "category" : "categories")} ang over budget."
                : $"You have used {FormatMoney(totalActual, currency)} of your {FormatMoney(totalBudget, currency)} total budget this month. {(remaining >= 0 ? $"You have {FormatMoney(remaining, currency)} remaining." : $"You are over by {FormatMoney(Math.Abs(remaining), currency)}.")} {overCount} {(overCount == 1 ? "category is" : "categories are")} over budget.",
            "budgets",
            ["Which category needs attention?", "Help me plan next month’s budget"]);
    }

    private static MomoChatResponse BudgetRiskAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var risk = snapshot.Budgets
            .OrderByDescending(item => item.IsOverBudget)
            .ThenByDescending(item => item.PercentageUsed)
            .FirstOrDefault();

        if (risk is null)
        {
            return Response(
                taglish
                    ? "Wala pang budgets na maa-analyze. Gumawa muna ng category budget."
                    : "There are no budgets to analyze yet. Create a category budget first.",
                "budgets",
                ["Create a budget"]);
        }

        var currency = GetCurrency(snapshot);
        var reason = risk.IsOverBudget
            ? taglish
                ? $"lampas na ng {FormatMoney(Math.Abs(risk.Remaining), currency)}"
                : $"it is over by {FormatMoney(Math.Abs(risk.Remaining), currency)}"
            : taglish
                ? $"{FormatPercent(risk.PercentageUsed)} na ang nagamit"
                : $"{FormatPercent(risk.PercentageUsed)} has been used";

        return Response(
            taglish
                ? $"{risk.Category} ang dapat mong unahin dahil {reason}. I-review ang recent transactions nito bago baguhin ang limit."
                : $"{risk.Category} needs the most attention because {reason}. Review its recent transactions before changing the limit.",
            "budgets",
            [$"How much did I spend on {risk.Category}?", "Open budgets"]);
    }

    private static MomoChatResponse NextBudgetAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var currentExpense = snapshot.CurrentMonth.Expense;

        if (currentExpense <= 0)
        {
            return Response(
                taglish
                    ? "Kulang pa ang spending history para gumawa ng useful recommendation. Mag-record muna ng expenses ngayong buwan."
                    : "There is not enough spending history for a useful recommendation yet. Record this month’s expenses first.",
                "add_transaction",
                ["Add an expense"]);
        }

        var baseline = snapshot.PreviousMonth is { Expense: > 0 }
            ? (currentExpense + snapshot.PreviousMonth.Expense) / 2
            : currentExpense;
        var suggested = decimal.Round(baseline * 1.05m, 2);
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Practical starting budget next month: around {FormatMoney(suggested, currency)}. Base ito sa recent monthly spending na may 5% buffer; i-adjust pababa kung may one-time expense ngayong buwan."
                : $"A practical starting budget for next month is about {FormatMoney(suggested, currency)}. It uses recent monthly spending with a 5% buffer; reduce it if this month includes a one-time expense.",
            "budget_forecast",
            ["Which category needs the biggest budget?"]);
    }

    private static MomoChatResponse BudgetCategoryRecommendation(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var category = snapshot.Categories
            .Select(item => new
            {
                Item = item,
                Baseline = item.PreviousMonthAmount > 0
                    ? (item.CurrentMonthAmount + item.PreviousMonthAmount) / 2
                    : item.CurrentMonthAmount
            })
            .OrderByDescending(item => item.Baseline)
            .FirstOrDefault();

        if (category is null || category.Baseline <= 0)
        {
            return Response(
                taglish
                    ? "Kulang pa ang category history para mag-recommend ng budget split."
                    : "There is not enough category history to recommend a budget split yet.",
                "budget_forecast",
                ["Add an expense"]);
        }

        var suggested = decimal.Round(category.Baseline * 1.05m, 2);
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"{category.Item.Name} ang pinakamalaking recent category mo. Practical starting limit next month: around {FormatMoney(suggested, currency)}, based sa two-month average plus 5% buffer."
                : $"{category.Item.Name} is your largest recent category. A practical starting limit next month is about {FormatMoney(suggested, currency)}, based on the two-month average plus a 5% buffer.",
            "budget_forecast",
            [$"How much did I spend on {category.Item.Name}?"]);
    }

    private static MomoChatResponse UpcomingAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var expenses = snapshot.UpcomingRecurring
            .Where(item => item.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.NextRunDate)
            .ToList();

        if (expenses.Count == 0)
        {
            return Response(
                taglish
                    ? "Wala kang recurring expense na due sa susunod na 30 days."
                    : "You have no recurring expense due in the next 30 days.",
                "recurring",
                ["Review recurring items"]);
        }

        var total = expenses.Sum(item => item.Amount);
        var spendable = GetSpendableBalance(snapshot);
        var next = expenses[0];
        var currency = GetCurrency(snapshot);
        var covered = spendable >= total;

        return Response(
            taglish
                ? $"May {expenses.Count} upcoming {(expenses.Count == 1 ? "bill" : "bills")} totaling {FormatMoney(total, currency)} sa susunod na 30 days. Una ang {next.Category} na {FormatMoney(next.Amount, currency)} sa {next.NextRunDate:MMM d}. {(covered ? "Covered ito ng current positive account balances mo." : $"Kulang ng humigit-kumulang {FormatMoney(total - spendable, currency)} ang current positive balances mo.")}"
                : $"You have {expenses.Count} upcoming {(expenses.Count == 1 ? "bill" : "bills")} totaling {FormatMoney(total, currency)} in the next 30 days. The next is {next.Category} for {FormatMoney(next.Amount, currency)} on {next.NextRunDate:MMM d}. {(covered ? "Your current positive account balances cover them." : $"Your current positive balances are short by about {FormatMoney(total - spendable, currency)}.")}",
            "recurring",
            ["Show all recurring items", "How much is safe to spend?"]);
    }

    private static MomoChatResponse AffordabilityAnswer(
        decimal amount,
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var bills = snapshot.UpcomingRecurring
            .Where(item => item.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Amount);
        var spendable = GetSpendableBalance(snapshot);
        var roomAfterBills = spendable - bills;
        var remaining = roomAfterBills - amount;
        var currency = GetCurrency(snapshot);

        return Response(
            remaining >= 0
                ? taglish
                    ? $"Based sa current positive balances at upcoming 30-day bills, kaya ang {FormatMoney(amount, currency)} at may matitirang humigit-kumulang {FormatMoney(remaining, currency)}. I-check pa rin kung may unrecorded bills bago bumili."
                    : $"Based on current positive balances and upcoming 30-day bills, {FormatMoney(amount, currency)} appears affordable with about {FormatMoney(remaining, currency)} left. Check for unrecorded bills before buying."
                : taglish
                    ? $"Hindi pa safe based sa recorded data: kulang ka ng humigit-kumulang {FormatMoney(Math.Abs(remaining), currency)} after reserving for upcoming bills."
                    : $"It does not look safe from the recorded data: you would be short by about {FormatMoney(Math.Abs(remaining), currency)} after reserving for upcoming bills.",
            "accounts",
            ["Can I cover my upcoming bills?", "Where can I cut spending?"]);
    }

    private static MomoChatResponse SafeToSpendAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var bills = snapshot.UpcomingRecurring
            .Where(item => item.Type.Equals(
                "Expense",
                StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Amount);
        var spendable = GetSpendableBalance(snapshot);
        var room = Math.Max(0, spendable - bills);
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"May hanggang {FormatMoney(room, currency)} kang recorded room after reserving {FormatMoney(bills, currency)} para sa upcoming 30-day bills. Ceiling ito, hindi target—mag-iwan pa ng emergency buffer at isama ang unrecorded commitments."
                : $"Your recorded spending room is up to {FormatMoney(room, currency)} after reserving {FormatMoney(bills, currency)} for upcoming 30-day bills. Treat that as a ceiling, not a target, and keep an emergency buffer for unrecorded commitments.",
            "accounts",
            ["Where can I cut spending?", "Open accounts"]);
    }

    private static MomoChatResponse GoalSummaryAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var goals = snapshot.GoalSummary;
        var currency = GetCurrency(snapshot);

        return Response(
            goals.TotalGoals == 0
                ? taglish
                    ? "Wala ka pang financial goal. Gumawa ng target amount at date para ma-track natin ang progress."
                    : "You do not have a financial goal yet. Create a target amount and date so I can track progress."
                : taglish
                    ? $"May {goals.TotalGoals} goals ka at {FormatMoney(goals.TotalSavedAmount, currency)} na ang na-save sa {FormatMoney(goals.TotalTargetAmount, currency)} target ({FormatPercent(goals.OverallPercentageCompleted)} complete). {goals.OverdueGoals} ang overdue."
                    : $"You have {goals.TotalGoals} goals with {FormatMoney(goals.TotalSavedAmount, currency)} saved toward {FormatMoney(goals.TotalTargetAmount, currency)} ({FormatPercent(goals.OverallPercentageCompleted)} complete). {goals.OverdueGoals} are overdue.",
            "goals",
            ["Which goal should I prioritize?", "Am I on track for my goals?"]);
    }

    private static MomoChatResponse GoalPriorityAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var priority = snapshot.Goals
            .Where(item => !item.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.IsOverdue)
            .ThenBy(item => item.TargetDate ?? DateTime.MaxValue)
            .ThenBy(item => item.PercentageCompleted)
            .FirstOrDefault();

        if (priority is null)
        {
            return Response(
                snapshot.Goals.Count == 0
                    ? "You do not have an active goal yet."
                    : "All visible goals are completed—nice work. You can create the next target when ready.",
                "goals",
                ["Create a new goal"]);
        }

        var months = Math.Max(1, (int)Math.Ceiling(Math.Max(priority.DaysRemaining, 1) / 30m));
        var monthlyNeeded = priority.RemainingAmount / months;
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Unahin ang {priority.Name}{(priority.IsOverdue ? " dahil overdue na ito" : priority.TargetDate.HasValue ? $" dahil pinakamalapit ang target date na {priority.TargetDate:MMM d, yyyy}" : string.Empty)}. Kailangan pa ng {FormatMoney(priority.RemainingAmount, currency)}, roughly {FormatMoney(monthlyNeeded, currency)} per month sa current timeline."
                : $"Prioritize {priority.Name}{(priority.IsOverdue ? " because it is overdue" : priority.TargetDate.HasValue ? $" because its {priority.TargetDate:MMM d, yyyy} target is closest" : string.Empty)}. It needs {FormatMoney(priority.RemainingAmount, currency)} more, roughly {FormatMoney(monthlyNeeded, currency)} per month on the current timeline.",
            "goals",
            [$"How is {priority.Name} doing?", "Open goals"]);
    }

    private static MomoChatResponse GoalAnswer(
        MomoGoalContext goal,
        string directMessage,
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var currency = GetCurrency(snapshot);
        var months = Math.Max(1, (int)Math.Ceiling(Math.Max(goal.DaysRemaining, 1) / 30m));
        var monthlyNeeded = goal.RemainingAmount / months;
        var status = goal.IsOverdue
            ? "overdue"
            : goal.Status.ToLowerInvariant();
        var prefix = ContainsAny(directMessage, "why", "bakit")
            ? "That’s because "
            : string.Empty;

        return Response(
            taglish
                ? $"{goal.Name} is {FormatPercent(goal.PercentageCompleted)} complete: {FormatMoney(goal.SavedAmount, currency)} saved at {FormatMoney(goal.RemainingAmount, currency)} remaining. {status} ang status{(goal.TargetDate.HasValue ? $" at target date ay {goal.TargetDate:MMM d, yyyy}" : string.Empty)}. Para ma-hit ang timeline, around {FormatMoney(monthlyNeeded, currency)} per month ang kailangan."
                : $"{prefix}{goal.Name} is {FormatPercent(goal.PercentageCompleted)} complete, with {FormatMoney(goal.SavedAmount, currency)} saved and {FormatMoney(goal.RemainingAmount, currency)} remaining. Its status is {status}{(goal.TargetDate.HasValue ? $" with a {goal.TargetDate:MMM d, yyyy} target" : string.Empty)}. The current timeline requires about {FormatMoney(monthlyNeeded, currency)} per month.",
            "goals",
            ["Which goal should I prioritize?", "Open goals"]);
    }

    private static MomoChatResponse RecentTransactionsAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var recent = snapshot.RecentTransactions.Take(3).ToList();

        if (recent.Count == 0)
        {
            return Response(
                taglish
                    ? "Wala ka pang recent transactions."
                    : "You do not have any recent transactions yet.",
                "add_transaction",
                ["Add a transaction"]);
        }

        var currency = GetCurrency(snapshot);
        var lines = recent.Select(item =>
            $"• {item.Date:MMM d}: {item.Category} — {FormatMoney(item.Amount, currency)} ({item.Type.ToLowerInvariant()})");

        return Response(
            $"{(taglish ? "Ito ang latest activity mo:" : "Here is your latest activity:")}\n{string.Join("\n", lines)}",
            "transactions",
            ["Open all transactions", "Where am I spending the most?"]);
    }

    private static MomoChatResponse ChangedCategoryAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var changed = snapshot.Categories
            .Where(item =>
                item.CurrentMonthAmount > 0 ||
                item.PreviousMonthAmount > 0)
            .OrderByDescending(item => Math.Abs(
                item.CurrentMonthAmount - item.PreviousMonthAmount))
            .FirstOrDefault();

        if (changed is null)
        {
            return Response(
                taglish
                    ? "Kulang pa ang category data para makahanap ng biggest change."
                    : "There is not enough category data to identify the biggest change yet.",
                "reports",
                ["Open reports"]);
        }

        var difference = changed.CurrentMonthAmount -
            changed.PreviousMonthAmount;
        var currency = GetCurrency(snapshot);
        var movement = difference > 0
            ? taglish ? "tumaas" : "increased"
            : difference < 0
                ? taglish ? "bumaba" : "decreased"
                : taglish ? "hindi nagbago" : "did not change";

        return Response(
            taglish
                ? $"{changed.Name} ang may pinakamalaking change: {movement} ng {FormatMoney(Math.Abs(difference), currency)}, from {FormatMoney(changed.PreviousMonthAmount, currency)} to {FormatMoney(changed.CurrentMonthAmount, currency)}."
                : $"{changed.Name} changed the most: it {movement} by {FormatMoney(Math.Abs(difference), currency)}, from {FormatMoney(changed.PreviousMonthAmount, currency)} to {FormatMoney(changed.CurrentMonthAmount, currency)}.",
            "reports",
            [$"How much did I spend on {changed.Name}?"]);
    }

    private static MomoChatResponse TrendAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        if (snapshot.PreviousMonth is null ||
            snapshot.PreviousMonth.Expense <= 0)
        {
            return Response(
                taglish
                    ? "Kulang pa ang previous-month data para sa reliable comparison."
                    : "There is not enough previous-month data for a reliable comparison yet.",
                "reports",
                ["Open reports"]);
        }

        var previous = snapshot.PreviousMonth;
        var current = snapshot.CurrentMonth;
        var difference = current.Expense - previous.Expense;
        var percent = previous.Expense == 0
            ? 0
            : Math.Abs(difference) / previous.Expense * 100;
        var direction = difference > 0 ? "higher" : difference < 0 ? "lower" : "unchanged";
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Ang spending mo ngayong {current.MonthName} ay {FormatMoney(current.Expense, currency)}, {FormatPercent(percent)} {direction} kumpara sa {FormatMoney(previous.Expense, currency)} noong {previous.MonthName}."
                : $"Your {current.MonthName} spending is {FormatMoney(current.Expense, currency)}, {FormatPercent(percent)} {direction} than {previous.MonthName} at {FormatMoney(previous.Expense, currency)}.",
            "reports",
            ["Which category changed the most?", "Open reports"]);
    }

    private static MomoChatResponse SavingsAnswer(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var month = snapshot.CurrentMonth;

        if (month.Income <= 0)
        {
            return Response(
                taglish
                    ? "Wala pang recorded income ngayong buwan, kaya hindi pa makukuwenta nang tama ang savings rate."
                    : "There is no recorded income this month, so the savings rate cannot be calculated accurately yet.",
                "add_transaction",
                ["Record income"]);
        }

        var rate = GetSavingsRate(month);
        var target = month.Income * 0.20m;
        var actual = Math.Max(month.Balance, 0);
        var gap = target - actual;
        var currency = GetCurrency(snapshot);

        return Response(
            taglish
                ? $"Savings rate mo ngayong buwan ay {FormatPercent(rate)}. Ang 20% target ay {FormatMoney(target, currency)}; {(gap <= 0 ? "naabot o nalampasan mo na ito" : $"kailangan pa ng {FormatMoney(gap, currency)} para maabot iyon")}."
                : $"Your savings rate this month is {FormatPercent(rate)}. A 20% target equals {FormatMoney(target, currency)}; {(gap <= 0 ? "you have already met or exceeded it" : $"you need {FormatMoney(gap, currency)} more to reach it")}.",
            "goals",
            ["Which goal should receive my savings?"]);
    }

    private static MomoChatResponse SmartFallback(
        MomoFinanceSnapshot snapshot,
        bool taglish)
    {
        var overBudget = snapshot.Budgets
            .OrderByDescending(item => item.PercentageUsed)
            .FirstOrDefault(item => item.IsOverBudget);
        var insight = overBudget is not null
            ? taglish
                ? $"Quick insight: over budget ang {overBudget.Category}."
                : $"Quick insight: {overBudget.Category} is over budget."
            : snapshot.UpcomingRecurring.Any(item => item.IsOverdue)
                ? taglish
                    ? "Quick insight: may overdue recurring item ka."
                    : "Quick insight: you have an overdue recurring item."
                : taglish
                    ? "Pwede mong itanong ang balance, spending, budgets, goals, o upcoming bills mo."
                    : "Ask about your balance, spending, budgets, goals, or upcoming bills.";

        return Response(
            $"{PageGuidanceText(snapshot.CurrentPath, taglish)} {insight}",
            "none",
            [
                "How am I doing financially?",
                "Where am I spending the most?",
                "Can I cover my upcoming bills?"
            ]);
    }

    private static MomoChatResponse PageGuidance(
        string path,
        bool taglish) =>
        Response(
            PageGuidanceText(path, taglish),
            "none",
            PagePrompts(path));

    private static string PageGuidanceText(
        string path,
        bool taglish)
    {
        var page = GetPageName(path);

        return page switch
        {
            "Accounts" => taglish
                ? "Nasa Accounts ka. Dito makikita ang current balances, account types, at kung alin ang kasama sa net worth."
                : "You’re on Accounts. Review current balances, account types, and which accounts count toward net worth.",
            "Transactions" => taglish
                ? "Nasa Transactions ka. Mag-record, mag-filter, o mag-group ng income at expenses para accurate ang ibang insights."
                : "You’re on Transactions. Record, filter, or group income and expenses to keep every other insight accurate.",
            "Budgets" => taglish
                ? "Nasa Budgets ka. I-compare ang planned at actual spending at unahin ang categories na malapit o lampas sa limit."
                : "You’re on Budgets. Compare planned and actual spending, then focus on categories near or over their limits.",
            "Financial Goals" => taglish
                ? "Nasa Financial Goals ka. Tingnan ang remaining amount, target date, at required monthly pace ng bawat goal."
                : "You’re on Financial Goals. Review each goal’s remaining amount, target date, and required monthly pace.",
            "Reports" => taglish
                ? "Nasa Reports ka. Gamitin ito para sa month-to-month trends, category comparisons, at exports."
                : "You’re on Reports. Use this page for month-to-month trends, category comparisons, and exports.",
            _ => taglish
                ? $"Nasa {page} ka. Sabihin mo kung anong decision ang gusto mong gawin at tutulungan kitang hanapin ang relevant na data."
                : $"You’re on {page}. Tell me what decision you are making and I’ll point you to the relevant data."
        };
    }

    private static string[] PagePrompts(string path) =>
        GetPageName(path) switch
        {
            "Accounts" =>
            [
                "What is my net worth?",
                "Which account has the highest balance?"
            ],
            "Transactions" =>
            [
                "Show my recent transactions",
                "Where am I spending the most?"
            ],
            "Budgets" =>
            [
                "Which category needs attention?",
                "Help me plan next month’s budget"
            ],
            "Financial Goals" =>
            [
                "Which goal should I prioritize?",
                "Am I on track for my goals?"
            ],
            _ =>
            [
                "How am I doing financially?",
                "Can I cover my upcoming bills?"
            ]
        };

    private static MomoChatResponse Response(
        string message,
        string intent,
        IEnumerable<string> prompts)
    {
        NavigationTargets.TryGetValue(
            intent,
            out var target);

        return new MomoChatResponse
        {
            Message = message.Trim(),
            SuggestedRoute = target?.Route,
            SuggestedRouteLabel = target?.Label,
            SuggestedPrompts = prompts
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList(),
            IsSmartResponse = true,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static MomoAccountContext? FindNamedAccount(
        IEnumerable<MomoAccountContext> accounts,
        string conversationText) =>
        FindNamedEntity(
            accounts,
            item => item.Name,
            conversationText);

    private static MomoBudgetContext? FindNamedBudget(
        IEnumerable<MomoBudgetContext> budgets,
        string conversationText) =>
        FindNamedEntity(
            budgets,
            item => item.Category,
            conversationText);

    private static MomoCategoryContext? FindNamedCategory(
        IEnumerable<MomoCategoryContext> categories,
        string conversationText) =>
        FindNamedEntity(
            categories,
            item => item.Name,
            conversationText);

    private static MomoGoalContext? FindNamedGoal(
        IEnumerable<MomoGoalContext> goals,
        string conversationText) =>
        FindNamedEntity(
            goals,
            item => item.Name,
            conversationText);

    private static T? FindNamedEntity<T>(
        IEnumerable<T> items,
        Func<T, string> nameSelector,
        string conversationText)
        where T : class
    {
        var matches = items
            .Select(item => new
            {
                Item = item,
                Name = Normalize(nameSelector(item))
            })
            .Where(item =>
                item.Name.Length > 1 &&
                conversationText.Contains(
                    item.Name,
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Name.Length)
            .FirstOrDefault();

        return matches?.Item;
    }

    private static string BuildConversationText(
        string directMessage,
        IReadOnlyCollection<MomoConversationMessage> history)
    {
        if (!IsFollowUp(directMessage))
        {
            return directMessage;
        }

        var priorContext = history
            .TakeLast(4)
            .Select(item => Normalize(item.Text));

        return $"{string.Join(' ', priorContext)} {directMessage}";
    }

    private static bool IsFollowUp(string message)
    {
        var wordCount = message.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries).Length;

        if (wordCount <= 10 &&
            ContainsAny(
                message,
                "how does that compare",
                "what does that mean",
                "tell me more about",
                "what about",
                "how about"))
        {
            return true;
        }

        if (wordCount > 5)
        {
            return false;
        }

        return message is "it" or "that" or "why" or "more" ||
            ContainsAny(
                message,
                "bakit",
                "which one",
                "tell me more",
                "that one");
    }

    private static bool LooksTaglish(string message) =>
        ContainsAny(
            $" {message} ",
            " ba ",
            " ko ",
            " ang ",
            " ng ",
            " mga ",
            " magkano ",
            " paano ",
            " pwede ",
            " kaya ",
            " gastos ",
            " pera ",
            " ipon ",
            " bakit ",
            " kamusta ",
            " kumusta ");

    private static decimal? ExtractAmount(string message)
    {
        var match = MoneyAmountRegex().Match(message);

        if (!match.Success)
        {
            return null;
        }

        var normalized = match.Groups[1].Value.Replace(",", string.Empty);

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount)
            ? amount
            : null;
    }

    private static decimal GetSpendableBalance(
        MomoFinanceSnapshot snapshot) =>
        snapshot.Accounts.Items
            .Where(item =>
                !item.Type.Equals(
                    "CreditCard",
                    StringComparison.OrdinalIgnoreCase) &&
                item.CurrentBalance > 0)
            .Sum(item => item.CurrentBalance);

    private static decimal GetSavingsRate(MomoMonthlyContext month) =>
        month.Income <= 0
            ? 0
            : (month.Income - month.Expense) / month.Income * 100;

    private static string GetCurrency(MomoFinanceSnapshot snapshot)
    {
        var currencies = snapshot.Accounts.Items
            .Select(item => item.Currency)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return currencies.Count switch
        {
            1 => currencies[0],
            > 1 => "mixed currencies",
            _ => string.Empty
        };
    }

    private static string FormatMoney(decimal amount, string currency)
    {
        var value = amount.ToString("N2", CultureInfo.InvariantCulture);

        return currency switch
        {
            "" => value,
            "mixed currencies" => $"{value} across mixed currencies",
            _ => $"{currency} {value}"
        };
    }

    private static string FormatPercent(decimal value) =>
        $"{value:N1}%";

    private static bool ContainsAny(
        string value,
        params string[] candidates) =>
        candidates.Any(candidate =>
            value.Contains(
                candidate,
                StringComparison.OrdinalIgnoreCase));

    private static string GetPageName(string path)
    {
        var segment = path.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        return segment switch
        {
            "expenses" => "Transactions",
            "financial-goals" => "Financial Goals",
            "budget-forecast" => "Budget Forecast",
            "recurring-transactions" => "Recurring Transactions",
            "expense-calendar" => "Expense Calendar",
            "audit-trails" => "Audit Trail",
            { } value => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                value.Replace('-', ' ')),
            _ => "Dashboard"
        };
    }

    private static string Normalize(string value) =>
        WhitespaceRegex().Replace(
            NonWordRegex().Replace(
                value.Trim().ToLowerInvariant(),
                " "),
            " ")
        .Trim();

    private sealed record NavigationTarget(
        string Route,
        string Label);

    [GeneratedRegex(@"[^\p{L}\p{N}.,%]+")]
    private static partial Regex NonWordRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"(?:php|usd|eur|gbp|jpy|₱|\$|€|£)?\s*(\d[\d,]*(?:\.\d{1,2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex MoneyAmountRegex();
}
