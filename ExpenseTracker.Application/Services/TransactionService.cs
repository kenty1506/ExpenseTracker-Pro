using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync(
            _currentUserService.UserId);

        return transactions.Select(MapToDto);
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(
            id,
            _currentUserService.UserId);

        return transaction == null
            ? null
            : MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(
        CreateTransactionDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(
            dto.CategoryId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var transaction = new Transaction
        {
            UserId = _currentUserService.UserId,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Notes = dto.Notes,
            Date = dto.Date
        };

        var created = await _transactionRepository.CreateAsync(
            transaction);

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(
                created.Id,
                _currentUserService.UserId);

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
        var existingTransaction =
            await _transactionRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        if (existingTransaction == null)
            return null;

        var category = await _categoryRepository.GetByIdAsync(
            dto.CategoryId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        existingTransaction.Type = dto.Type;
        existingTransaction.CategoryId = dto.CategoryId;
        existingTransaction.Amount = dto.Amount;
        existingTransaction.Notes = dto.Notes;
        existingTransaction.Date = dto.Date;
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        var updated = await _transactionRepository.UpdateAsync(
            existingTransaction);

        if (!updated)
            return null;

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

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
            Category =
                transaction.Category?.Name ?? string.Empty,
            Amount = transaction.Amount,
            Notes = transaction.Notes,
            Date = transaction.Date
        };
    }
}