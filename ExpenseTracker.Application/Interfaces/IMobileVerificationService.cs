namespace ExpenseTracker.Application.Interfaces;

public interface IMobileVerificationService
{
    Task StartAsync(
        string phoneNumber,
        CancellationToken cancellationToken);

    Task<bool> CheckAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken);
}
