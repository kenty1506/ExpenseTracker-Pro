using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Common;

public static class AccountBalanceCalculator
{
    public static AccountBalanceBreakdown Calculate(
        Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return Calculate(
            account,
            account.IncomingTransfers,
            account.OutgoingTransfers);
    }

    public static AccountBalanceBreakdown Calculate(
        Account account,
        IEnumerable<Transfer> transfers)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(transfers);

        var transferList = transfers as IReadOnlyCollection<Transfer>
            ?? transfers.ToList();

        return Calculate(
            account,
            transferList.Where(transfer =>
                transfer.ToAccountId == account.Id),
            transferList.Where(transfer =>
                transfer.FromAccountId == account.Id));
    }

    private static AccountBalanceBreakdown Calculate(
        Account account,
        IEnumerable<Transfer> incomingTransfers,
        IEnumerable<Transfer> outgoingTransfers)
    {
        var totalIncome = account.Transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);

        var totalExpense = account.Transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);

        var totalIncomingTransfers = incomingTransfers
            .Sum(transfer => transfer.Amount);

        var totalOutgoingTransfers = outgoingTransfers
            .Sum(transfer => transfer.Amount);

        return new AccountBalanceBreakdown
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            IncomingTransfers = totalIncomingTransfers,
            OutgoingTransfers = totalOutgoingTransfers,
            CurrentBalance =
                account.OpeningBalance
                + totalIncome
                - totalExpense
                + totalIncomingTransfers
                - totalOutgoingTransfers
        };
    }
}

public sealed class AccountBalanceBreakdown
{
    public decimal TotalIncome { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal IncomingTransfers { get; init; }

    public decimal OutgoingTransfers { get; init; }

    public decimal CurrentBalance { get; init; }
}
