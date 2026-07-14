using ExpenseTracker.Application.DTOs.Transfers;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Services;

public class TransferService : ITransferService
{
    private readonly ITransferRepository _transferRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public TransferService(
        ITransferRepository transferRepository,
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TransferDto>> GetAllAsync()
    {
        var transfers =
            await _transferRepository.GetAllAsync(
                _currentUserService.UserId);

        return transfers.Select(MapToDto);
    }

    public async Task<TransferDto?> GetByIdAsync(int id)
    {
        var transfer =
            await _transferRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        return transfer == null
            ? null
            : MapToDto(transfer);
    }

    public async Task<TransferDto> CreateAsync(
        CreateTransferDto dto)
    {
        var userId = _currentUserService.UserId;

        ValidateDifferentAccounts(
            dto.FromAccountId,
            dto.ToAccountId);

        var fromAccount =
            await GetValidAccountAsync(
                dto.FromAccountId,
                userId,
                "source");

        await GetValidAccountAsync(
            dto.ToAccountId,
            userId,
            "destination");

        var transfers =
            await _transferRepository.GetAllAsync(
                userId);

        var sourceBalance = CalculateAccountBalance(
            fromAccount,
            transfers);

        if (sourceBalance < dto.Amount)
        {
            throw new ArgumentException(
                $"Insufficient balance. The source account has " +
                $"{sourceBalance:N2} available.");
        }

        var transfer = new Transfer
        {
            UserId = userId,
            FromAccountId = dto.FromAccountId,
            ToAccountId = dto.ToAccountId,
            Amount = dto.Amount,
            TransferDate = dto.TransferDate,
            Notes = dto.Notes.Trim(),
            IsActive = true
        };

        var created =
            await _transferRepository.CreateAsync(
                transfer);

        var saved =
            await _transferRepository.GetByIdAsync(
                created.Id,
                userId);

        if (saved == null)
        {
            throw new InvalidOperationException(
                "The transfer was created but could not be reloaded.");
        }

        return MapToDto(saved);
    }

    public async Task<TransferDto?> UpdateAsync(
        int id,
        UpdateTransferDto dto)
    {
        var userId = _currentUserService.UserId;

        var existingTransfer =
            await _transferRepository.GetByIdAsync(
                id,
                userId);

        if (existingTransfer == null)
            return null;

        ValidateDifferentAccounts(
            dto.FromAccountId,
            dto.ToAccountId);

        var fromAccount =
            await GetValidAccountAsync(
                dto.FromAccountId,
                userId,
                "source");

        await GetValidAccountAsync(
            dto.ToAccountId,
            userId,
            "destination");

        var transfers =
            (await _transferRepository.GetAllAsync(
                userId))
            .Where(transfer =>
                transfer.Id != id)
            .ToList();

        var sourceBalance = CalculateAccountBalance(
            fromAccount,
            transfers);

        if (sourceBalance < dto.Amount)
        {
            throw new ArgumentException(
                $"Insufficient balance. The source account has " +
                $"{sourceBalance:N2} available.");
        }

        existingTransfer.FromAccountId =
            dto.FromAccountId;

        existingTransfer.ToAccountId =
            dto.ToAccountId;

        existingTransfer.Amount =
            dto.Amount;

        existingTransfer.TransferDate =
            dto.TransferDate;

        existingTransfer.Notes =
            dto.Notes.Trim();

        existingTransfer.UpdatedAt =
            DateTime.UtcNow;

        var updated =
            await _transferRepository.UpdateAsync(
                existingTransfer);

        if (!updated)
            return null;

        var saved =
            await _transferRepository.GetByIdAsync(
                id,
                userId);

        return saved == null
            ? null
            : MapToDto(saved);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _transferRepository.DeleteAsync(
            id,
            _currentUserService.UserId);
    }

    private async Task<Account> GetValidAccountAsync(
        int accountId,
        string userId,
        string accountRole)
    {
        var account =
            await _accountRepository.GetByIdAsync(
                accountId,
                userId);

        if (account == null)
        {
            throw new ArgumentException(
                $"The {accountRole} account with ID " +
                $"{accountId} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException(
                $"The selected {accountRole} account is inactive.");
        }

        return account;
    }

    private static void ValidateDifferentAccounts(
        int fromAccountId,
        int toAccountId)
    {
        if (fromAccountId == toAccountId)
        {
            throw new ArgumentException(
                "The source and destination accounts must be different.");
        }
    }

    private static decimal CalculateAccountBalance(
        Account account,
        IEnumerable<Transfer> transfers)
    {
        var totalIncome = account.Transactions
            .Where(transaction =>
                transaction.Type ==
                TransactionType.Income)
            .Sum(transaction =>
                transaction.Amount);

        var totalExpense = account.Transactions
            .Where(transaction =>
                transaction.Type ==
                TransactionType.Expense)
            .Sum(transaction =>
                transaction.Amount);

        var incomingTransfers = transfers
            .Where(transfer =>
                transfer.ToAccountId == account.Id)
            .Sum(transfer =>
                transfer.Amount);

        var outgoingTransfers = transfers
            .Where(transfer =>
                transfer.FromAccountId == account.Id)
            .Sum(transfer =>
                transfer.Amount);

        return account.OpeningBalance
            + totalIncome
            - totalExpense
            + incomingTransfers
            - outgoingTransfers;
    }

    private static TransferDto MapToDto(
        Transfer transfer)
    {
        return new TransferDto
        {
            Id = transfer.Id,
            FromAccountId =
                transfer.FromAccountId,
            FromAccount =
                transfer.FromAccount?.Name
                ?? string.Empty,
            ToAccountId =
                transfer.ToAccountId,
            ToAccount =
                transfer.ToAccount?.Name
                ?? string.Empty,
            Amount =
                transfer.Amount,
            TransferDate =
                transfer.TransferDate,
            Notes =
                transfer.Notes
        };
    }
    public async Task<PagedResult<TransferDto>> GetPagedAsync(
    TransferQueryDto query)
    {
        var result =
            await _transferRepository.GetPagedAsync(
                _currentUserService.UserId,
                query);

        return new PagedResult<TransferDto>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages,
            Items = result.Items
                .Select(MapToDto)
                .ToList()
        };
    }
}