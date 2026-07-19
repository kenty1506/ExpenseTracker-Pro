using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Transfers;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransferService
{
    Task<IEnumerable<TransferDto>> GetAllAsync();

    Task<TransferDto?> GetByIdAsync(int id);

    Task<TransferDto> CreateAsync(
        CreateTransferDto dto);

    Task<TransferDto?> UpdateAsync(
        int id,
        UpdateTransferDto dto);

    Task<bool> DeleteAsync(int id);

    Task<PagedResult<TransferDto>> GetPagedAsync(TransferQueryDto query);

}