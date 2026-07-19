using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Account : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    public decimal OpeningBalance { get; set; }

    public string Currency { get; set; } = "PHP";

    public string Color { get; set; } = "#6366F1";

    public string Icon { get; set; } = "account_balance_wallet";

    public bool IncludeInNetWorth { get; set; } = true;

    public List<Transaction> Transactions { get; set; } = [];

    public List<RecurringTransaction> RecurringTransactions { get; set; } = [];

    public List<Transfer> OutgoingTransfers { get; set; } = [];

    public List<Transfer> IncomingTransfers { get; set; } = [];

    public List<FinancialGoal> FinancialGoals { get; set; } = [];

    public List<GoalContribution> GoalContributions { get; set; } = [];
}