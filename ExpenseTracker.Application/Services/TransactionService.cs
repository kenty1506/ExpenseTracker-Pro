using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
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
        var transaction = new Transaction
        {
            Type = dto.Type,
            Amount = dto.Amount,
            Notes = dto.Notes,
            Date = dto.Date
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        return new TransactionDto
        {
            Id = created.Id,
            Type = created.Type,
            Category = created.Category?.Name ?? string.Empty,
            Amount = created.Amount,
            Notes = created.Notes,
            Date = created.Date
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _transactionRepository.DeleteAsync(id);
    }
}