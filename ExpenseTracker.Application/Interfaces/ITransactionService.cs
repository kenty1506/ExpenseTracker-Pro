using ExpenseTracker.Application.DTOs.Transactions;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync();

    Task<TransactionDto?> GetByIdAsync(int id);

    Task<TransactionDto> CreateAsync(CreateTransactionDto dto);

    Task<bool> DeleteAsync(int id);
    Task<TransactionDto?> UpdateAsync(int id, UpdateTransactionDto dto);
}