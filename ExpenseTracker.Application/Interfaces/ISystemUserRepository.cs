namespace ExpenseTracker.Application.Interfaces;

public interface ISystemUserRepository
{
    Task<IReadOnlyList<string>> GetActiveUserIdsPageAsync(
        string? afterUserId,
        int pageSize,
        CancellationToken cancellationToken = default);
}
