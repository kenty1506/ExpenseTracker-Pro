using ExpenseTracker.Application.Models.Auth;

namespace ExpenseTracker.Application.Interfaces;

public interface IGoogleIdentityVerifier
{
    Task<VerifiedGoogleIdentity?> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken);
}
