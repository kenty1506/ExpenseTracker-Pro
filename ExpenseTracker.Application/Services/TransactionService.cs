using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Type = t.Type,
            Category = t.Category?.Name ?? string.Empty,
            Amount = t.Amount,
            Notes = t.Notes,
            Date = t.Date
        });
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction == null)
            return null;

        return new TransactionDto
        {
            Id = transaction.Id,
            Type = transaction.Type,
            Category = transaction.Category?.Name ?? string.Empty,
            Amount = transaction.Amount,
            Notes = transaction.Notes,
            Date = transaction.Date
        };
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var transaction = new Transaction
        {
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Notes = dto.Notes,
            Date = dto.Date
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(created.Id);

        if (savedTransaction == null)
        {
            throw new InvalidOperationException(
                "The transaction was created but could not be reloaded.");
        }

        return new TransactionDto
        {
            Id = savedTransaction.Id,
            Type = savedTransaction.Type,
            Category = savedTransaction.Category?.Name ?? string.Empty,
            Amount = savedTransaction.Amount,
            Notes = savedTransaction.Notes,
            Date = savedTransaction.Date
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _transactionRepository.DeleteAsync(id);
    }

    public async Task<TransactionDto?> UpdateAsync(int id, UpdateTransactionDto dto)
    {
        var existingTransaction =
            await _transactionRepository.GetByIdAsync(id);

        if (existingTransaction == null)
            return null;

        var category =
            await _categoryRepository.GetByIdAsync(dto.CategoryId);

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

        var updated =
            await _transactionRepository.UpdateAsync(existingTransaction);

        if (!updated)
            return null;

        var savedTransaction =
            await _transactionRepository.GetByIdAsync(id);

        if (savedTransaction == null)
            return null;

        return new TransactionDto
        {
            Id = savedTransaction.Id,
            Type = savedTransaction.Type,
            Category = savedTransaction.Category?.Name ?? string.Empty,
            Amount = savedTransaction.Amount,
            Notes = savedTransaction.Notes,
            Date = savedTransaction.Date
        };
    }
}