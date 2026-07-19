using ExpenseTracker.Application.Interfaces;

namespace ExpenseTracker.Application.Services;

public class SystemBackgroundProcessor
    : ISystemBackgroundProcessor
{
    private readonly ISystemUserRepository _systemUserRepository;
    private readonly IRecurringTransactionService
        _recurringTransactionService;
    private readonly INotificationEngineService
        _notificationEngineService;

    public SystemBackgroundProcessor(
        ISystemUserRepository systemUserRepository,
        IRecurringTransactionService recurringTransactionService,
        INotificationEngineService notificationEngineService)
    {
        _systemUserRepository = systemUserRepository;
        _recurringTransactionService =
            recurringTransactionService;
        _notificationEngineService =
            notificationEngineService;
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken = default)
    {
        var userIds =
            await _systemUserRepository
                .GetAllActiveUserIdsAsync();

        foreach (var userId in userIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _recurringTransactionService
                .GenerateDueForUserAsync(
                    userId,
                    DateTime.UtcNow);

            await _notificationEngineService
                .GenerateForUserAsync(userId);
        }
    }
}