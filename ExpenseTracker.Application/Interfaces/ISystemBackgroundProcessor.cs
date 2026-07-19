namespace ExpenseTracker.Application.Interfaces;

public interface ISystemBackgroundProcessor
{
    Task ProcessAsync(
        CancellationToken cancellationToken = default);
}