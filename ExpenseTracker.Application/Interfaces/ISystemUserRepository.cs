namespace ExpenseTracker.Application.Interfaces;

public interface ISystemUserRepository
{
    Task<IReadOnlyList<string>> GetAllActiveUserIdsAsync();
}