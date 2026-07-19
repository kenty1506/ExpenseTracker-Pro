using ExpenseTracker.Application.DTOs.RecurringTransactions;

namespace ExpenseTracker.Application.Interfaces;

public interface IRecurringTransactionService
{
    Task<IEnumerable<RecurringTransactionDto>> GetAllAsync();

    Task<RecurringTransactionDto?> GetByIdAsync(int id);

    Task<RecurringTransactionDto> CreateAsync(CreateRecurringTransactionDto dto);

    Task<RecurringTransactionDto?> UpdateAsync(int id,UpdateRecurringTransactionDto dto);

    Task<bool> DeleteAsync(int id);

    Task<RecurringGenerationResultDto> GenerateDueAsync(DateTime? throughDate = null);
    Task<IEnumerable<UpcomingRecurringTransactionDto>>GetUpcomingAsync(int days);
    Task<IEnumerable<UpcomingRecurringTransactionDto>>GetUpcomingForUserAsync(string userId, int days);

    Task<RecurringGenerationResultDto> GenerateDueForUserAsync(string userId, DateTime? throughDate = null);


}