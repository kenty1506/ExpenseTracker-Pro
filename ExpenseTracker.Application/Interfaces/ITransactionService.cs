using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transfers;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync();

    Task<TransactionDto?> GetByIdAsync(int id);

    Task<TransactionDto> CreateAsync(CreateTransactionDto dto);

    Task<bool> DeleteAsync(int id);
    Task<TransactionDto?> UpdateAsync(int id, UpdateTransactionDto dto);
    Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionQueryDto query);

}