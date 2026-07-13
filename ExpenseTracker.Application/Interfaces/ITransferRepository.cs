using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransferRepository
{
    Task<IEnumerable<Transfer>> GetAllAsync(
        string userId);

    Task<Transfer?> GetByIdAsync(
        int id,
        string userId);

    Task<Transfer> CreateAsync(
        Transfer transfer);

    Task<bool> UpdateAsync(
        Transfer transfer);

    Task<bool> DeleteAsync(
        int id,
        string userId);
}