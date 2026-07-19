using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Tests.Accounts;

public class AccountBalanceCalculatorTests
{
    [Fact]
    public void Calculate_SubtractsOutgoingTransfersFromCurrentBalance()
    {
        var account = new Account
        {
            Id = 28,
            OpeningBalance = 25_000m,
            Transactions =
            [
                new Transaction
                {
                    Type = TransactionType.Income,
                    Amount = 56_450m
                },
                new Transaction
                {
                    Type = TransactionType.Expense,
                    Amount = 6_999m
                }
            ]
        };

        var transfers = new List<Transfer>
        {
            new()
            {
                FromAccountId = 27,
                ToAccountId = 28,
                Amount = 2_000m
            },
            new()
            {
                FromAccountId = 28,
                ToAccountId = 30,
                Amount = 1_500m
            },
            new()
            {
                FromAccountId = 28,
                ToAccountId = 29,
                Amount = 2_500m
            },
            new()
            {
                FromAccountId = 28,
                ToAccountId = 32,
                Amount = 15_000m
            }
        };

        var result = AccountBalanceCalculator.Calculate(
            account,
            transfers);

        Assert.Equal(56_450m, result.TotalIncome);
        Assert.Equal(6_999m, result.TotalExpense);
        Assert.Equal(2_000m, result.IncomingTransfers);
        Assert.Equal(19_000m, result.OutgoingTransfers);
        Assert.Equal(57_451m, result.CurrentBalance);
    }
}
