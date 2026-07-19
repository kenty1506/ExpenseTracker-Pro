using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Tests.Accounts;

public sealed class AccountServiceValidationTests
{
    [Fact]
    public async Task UpdateAsync_RejectsOpeningBalanceChangeAfterTransactionsExist()
    {
        var account = CreateAccount();
        account.Transactions.Add(new Transaction { Amount = 100m });
        var service = new AccountService(
            new AccountRepositoryStub(account),
            new CurrentUserStub());

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(account.Id, CreateUpdate(openingBalance: 2_000m)));

        Assert.Contains("can't be changed", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_AllowsOtherChangesWhenOpeningBalanceIsUnchanged()
    {
        var account = CreateAccount();
        account.Transactions.Add(new Transaction { Amount = 100m });
        var repository = new AccountRepositoryStub(account);
        var service = new AccountService(repository, new CurrentUserStub());

        var result = await service.UpdateAsync(
            account.Id,
            CreateUpdate(account.OpeningBalance));

        Assert.NotNull(result);
        Assert.True(repository.UpdateWasCalled);
    }

    private static Account CreateAccount() => new()
    {
        Id = 4,
        UserId = "user-1",
        Name = "Daily wallet",
        Type = AccountType.Cash,
        OpeningBalance = 1_000m,
        Currency = "PHP",
        Color = "#123456",
        Icon = "wallet",
        IsActive = true
    };

    private static UpdateAccountDto CreateUpdate(decimal openingBalance) => new()
    {
        Name = "Updated wallet",
        Type = AccountType.Cash,
        OpeningBalance = openingBalance,
        Currency = "PHP",
        Color = "#123456",
        Icon = "wallet",
        IncludeInNetWorth = true,
        IsActive = true
    };

    private sealed class CurrentUserStub : ICurrentUserService
    {
        public string UserId => "user-1";
        public bool IsAuthenticated => true;
    }

    private sealed class AccountRepositoryStub : IAccountRepository
    {
        private readonly Account _account;

        public AccountRepositoryStub(Account account) => _account = account;

        public bool UpdateWasCalled { get; private set; }

        public Task<IEnumerable<Account>> GetAllAsync(string userId) =>
            Task.FromResult<IEnumerable<Account>>([_account]);

        public Task<Account?> GetByIdAsync(int id, string userId) =>
            Task.FromResult<Account?>(_account);

        public Task<Account?> GetByNameAsync(string name, string userId) =>
            Task.FromResult<Account?>(null);

        public Task<Account> CreateAsync(Account account) => Task.FromResult(account);

        public Task<bool> UpdateAsync(Account account)
        {
            UpdateWasCalled = true;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id, string userId) => Task.FromResult(true);

        public Task<bool> HasTransactionsAsync(int id, string userId) =>
            Task.FromResult(_account.Transactions.Count > 0);

        public Task<PagedResult<Account>> GetPagedAsync(
            string userId,
            AccountQueryDto query) =>
            Task.FromResult(new PagedResult<Account> { Items = [_account] });
    }
}
