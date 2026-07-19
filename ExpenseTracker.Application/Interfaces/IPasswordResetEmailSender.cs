namespace ExpenseTracker.Application.Interfaces;

public interface IPasswordResetEmailSender
{
    Task SendAsync(
        string email,
        string fullName,
        string encodedResetToken,
        CancellationToken cancellationToken);
}
