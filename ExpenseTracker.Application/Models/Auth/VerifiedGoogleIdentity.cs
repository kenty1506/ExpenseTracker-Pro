namespace ExpenseTracker.Application.Models.Auth;

public sealed class VerifiedGoogleIdentity
{
    public string Subject { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;
}
