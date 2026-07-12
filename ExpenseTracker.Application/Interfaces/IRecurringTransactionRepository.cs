using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IRecurringTransactionRepository
{
    Task<IEnumerable<RecurringTransaction>> GetAllAsync(string userId);

    Task<RecurringTransaction?> GetByIdAsync(int id,string userId);

    Task<RecurringTransaction> CreateAsync(RecurringTransaction recurringTransaction);

    Task<bool> UpdateAsync(RecurringTransaction recurringTransaction);

    Task<bool> DeleteAsync(int id,string userId);

    Task<IEnumerable<RecurringTransaction>> GetDueAsync(string userId,DateTime throughDate);

    Task<Transaction?> GenerateOccurrenceAsync(int recurringTransactionId,string userId,DateTime occurrenceDate,DateTime nextRunDate);
    Task<IEnumerable<RecurringTransaction>> GetUpcomingAsync(string userId,DateTime fromDate,DateTime throughDate);
}