using ExpenseTracker.Application.DTOs.Accounts;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public AccountService(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAsync()
    {
        var accounts =
            await _accountRepository.GetAllAsync(
                _currentUserService.UserId);

        return accounts.Select(MapToDto);
    }

    public async Task<AccountDetailsDto?> GetByIdAsync(int id)
    {
        var account =
            await _accountRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        return account == null
            ? null
            : MapToDetailsDto(account);
    }

    public async Task<AccountDto> CreateAsync(
        CreateAccountDto dto)
    {
        ValidateAccountType(dto.Type);
        
        var userId = _currentUserService.UserId;
        var name = dto.Name.Trim();

        var duplicate =
            await _accountRepository.GetByNameAsync(
                name,
                userId);

        if (duplicate != null)
        {
            throw new ArgumentException(
                $"An account named '{name}' already exists.");
        }

        var account = new Account
        {
            UserId = userId,
            Name = name,
            Type = dto.Type,
            OpeningBalance = dto.OpeningBalance,
            Currency = dto.Currency
                .Trim()
                .ToUpperInvariant(),
            Color = dto.Color.Trim(),
            Icon = dto.Icon.Trim(),
            IncludeInNetWorth =
                dto.IncludeInNetWorth,
            IsActive = true
        };

        var created =
            await _accountRepository.CreateAsync(
                account);

        var saved =
            await _accountRepository.GetByIdAsync(
                created.Id,
                userId);

        if (saved == null)
        {
            throw new InvalidOperationException(
                "The account was created but could not be reloaded.");
        }

        return MapToDto(saved);
    }

    public async Task<AccountDto?> UpdateAsync(
        int id,
        UpdateAccountDto dto)
    {
        ValidateAccountType(dto.Type);

        var userId = _currentUserService.UserId;

        var account =
            await _accountRepository.GetByIdAsync(
                id,
                userId);

        if (account == null)
            return null;

        var name = dto.Name.Trim();

        var duplicate =
            await _accountRepository.GetByNameAsync(
                name,
                userId);

        if (duplicate != null &&
            duplicate.Id != id)
        {
            throw new ArgumentException(
                $"An account named '{name}' already exists.");
        }

        account.Name = name;
        account.Type = dto.Type;
        account.Currency = dto.Currency
            .Trim()
            .ToUpperInvariant();
        account.Color = dto.Color.Trim();
        account.Icon = dto.Icon.Trim();
        account.IncludeInNetWorth =
            dto.IncludeInNetWorth;
        account.IsActive =
            dto.IsActive;
        account.UpdatedAt =
            DateTime.UtcNow;

        var updated =
            await _accountRepository.UpdateAsync(
                account);

        if (!updated)
            return null;

        var saved =
            await _accountRepository.GetByIdAsync(
                id,
                userId);

        return saved == null
            ? null
            : MapToDto(saved);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _accountRepository.DeleteAsync(
            id,
            _currentUserService.UserId);
    }

    public async Task<AccountSummaryDto> GetSummaryAsync()
    {
        var accounts =(await _accountRepository.GetAllAsync(_currentUserService.UserId))
            .Where(account =>account.IsActive)
            .ToList();

        var accountDtos =accounts.Select(MapToDto).ToList();

        var includedAccounts =accountDtos
                .Where(account =>account.IncludeInNetWorth)
                .ToList();

        var totalLiabilities =includedAccounts
                .Where(account =>account.Type ==AccountType.CreditCard)
                .Sum(account =>Math.Abs(Math.Min(account.CurrentBalance, 0)));

        var totalAssets =includedAccounts
                .Where(account =>account.Type !=AccountType.CreditCard)
                .Sum(account =>account.CurrentBalance);

        return new AccountSummaryDto
        {
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            NetWorth =totalAssets - totalLiabilities,
            ActiveAccountCount = accounts.Count,
            Accounts = accountDtos
        };
    }

    private static AccountDto MapToDto(Account account)
    {
        var income = account.Transactions
                .Where(transaction => transaction.Type ==TransactionType.Income)
                .Sum(transaction => transaction.Amount);

        var expense =account.Transactions
                .Where(transaction => transaction.Type ==TransactionType.Expense)
                .Sum(transaction =>transaction.Amount);

        var incomingTransfers =account.IncomingTransfers
                .Sum(transfer =>transfer.Amount);

        var outgoingTransfers = account.OutgoingTransfers
                .Sum(transfer => transfer.Amount);

        var currentBalance = account.OpeningBalance+ income - expense + incomingTransfers - outgoingTransfers;

        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            OpeningBalance =account.OpeningBalance,
            CurrentBalance =currentBalance,
            Currency =account.Currency,
            Color =account.Color,
            Icon =account.Icon,
            IncludeInNetWorth =account.IncludeInNetWorth,
            IsActive =account.IsActive,
            TransactionCount =account.Transactions.Count

        };
    }

    private static void ValidateAccountType(
        AccountType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException("Please provide a valid account type.");
        }
    }

    private static AccountDetailsDto MapToDetailsDto(Account account)
    {
        var income = account.Transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);

        var expense = account.Transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);

        var incomingTransfers = account.IncomingTransfers
            .Sum(transfer => transfer.Amount);

        var outgoingTransfers = account.OutgoingTransfers
            .Sum(transfer => transfer.Amount);

        var currentBalance =
            account.OpeningBalance
            + income
            - expense
            + incomingTransfers
            - outgoingTransfers;

        var incomingActivities = account.IncomingTransfers
            .Select(transfer =>
                new AccountTransferActivityDto
                {
                    TransferId = transfer.Id,
                    Direction = "Incoming",
                    OtherAccountId = transfer.FromAccountId,
                    OtherAccount =
                        transfer.FromAccount?.Name
                        ?? string.Empty,
                    Amount = transfer.Amount,
                    TransferDate = transfer.TransferDate,
                    Notes = transfer.Notes
                });

        var outgoingActivities = account.OutgoingTransfers
            .Select(transfer =>
                new AccountTransferActivityDto
                {
                    TransferId = transfer.Id,
                    Direction = "Outgoing",
                    OtherAccountId = transfer.ToAccountId,
                    OtherAccount =
                        transfer.ToAccount?.Name
                        ?? string.Empty,
                    Amount = transfer.Amount,
                    TransferDate = transfer.TransferDate,
                    Notes = transfer.Notes
                });

        var recentTransfers = incomingActivities
            .Concat(outgoingActivities)
            .OrderByDescending(transfer =>
                transfer.TransferDate)
            .ThenByDescending(transfer =>
                transfer.TransferId)
            .Take(10)
            .ToList();

        return new AccountDetailsDto
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            OpeningBalance = account.OpeningBalance,
            CurrentBalance = currentBalance,
            Currency = account.Currency,
            Color = account.Color,
            Icon = account.Icon,
            IncludeInNetWorth = account.IncludeInNetWorth,
            IsActive = account.IsActive,
            TransactionCount = account.Transactions.Count,
            RecentTransfers = recentTransfers
        };
    }
}