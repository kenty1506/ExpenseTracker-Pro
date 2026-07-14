using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        var transactions =
            await _transactionRepository.GetAllAsync(
                _currentUserService.UserId);

        return transactions.Select(MapToDto);
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var transaction =
            await _transactionRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        return transaction == null
            ? null
            : MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(
        CreateTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var category =
            await _categoryRepository.GetByIdAsync(
                dto.CategoryId,
                userId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var account =
            await _accountRepository.GetByIdAsync(
                dto.AccountId,
                userId);

        if (account == null)
        {
            throw new ArgumentException(
                $"Account with ID {dto.AccountId} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException(
                "The selected account is inactive.");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            Notes = dto.Notes,
            Date = dto.Date
        };

        var created =
            await _transactionRepository.CreateAsync(
                transaction);

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(
                created.Id,
                userId);

        if (savedTransaction == null)
        {
            throw new InvalidOperationException(
                "The transaction was created but could not be reloaded.");
        }

        return MapToDto(savedTransaction);
    }

    public async Task<TransactionDto?> UpdateAsync(
        int id,
        UpdateTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var existingTransaction =
            await _transactionRepository.GetByIdAsync(
                id,
                userId);

        if (existingTransaction == null)
            return null;

        var category =
            await _categoryRepository.GetByIdAsync(
                dto.CategoryId,
                userId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var account =
            await _accountRepository.GetByIdAsync(
                dto.AccountId,
                userId);

        if (account == null)
        {
            throw new ArgumentException(
                $"Account with ID {dto.AccountId} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException(
                "The selected account is inactive.");
        }

        existingTransaction.Type = dto.Type;
        existingTransaction.CategoryId = dto.CategoryId;
        existingTransaction.AccountId = dto.AccountId;
        existingTransaction.Amount = dto.Amount;
        existingTransaction.Notes = dto.Notes;
        existingTransaction.Date = dto.Date;
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        var updated =
            await _transactionRepository.UpdateAsync(
                existingTransaction);

        if (!updated)
            return null;

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(
                id,
                userId);

        return savedTransaction == null
            ? null
            : MapToDto(savedTransaction);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _transactionRepository.DeleteAsync(
            id,
            _currentUserService.UserId);
    }

    private static TransactionDto MapToDto(
        Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Type = transaction.Type,
            Category = transaction.Category?.Name
                ?? string.Empty,
            AccountId = transaction.AccountId,
            Account = transaction.Account?.Name
                ?? string.Empty,
            Amount = transaction.Amount,
            Notes = transaction.Notes,
            Date = transaction.Date
        };
    }
    public async Task<PagedResult<TransactionDto>> GetPagedAsync(
    TransactionQueryDto query)
    {
        var result =
            await _transactionRepository.GetPagedAsync(
                _currentUserService.UserId,
                query);

        return new PagedResult<TransactionDto>
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