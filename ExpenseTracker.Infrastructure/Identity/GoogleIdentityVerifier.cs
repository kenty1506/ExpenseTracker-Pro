using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Models.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Identity;

public sealed class GoogleIdentityVerifier
    : IGoogleIdentityVerifier
{
    private readonly GoogleAuthSettings _settings;

    public GoogleIdentityVerifier(
        IOptions<GoogleAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<VerifiedGoogleIdentity?> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            throw new ExternalAuthenticationProviderException(
                "Google authentication is not configured.");
        }

        try
        {
            var validationSettings =
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_settings.ClientId.Trim()]
                };

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                validationSettings);

            cancellationToken.ThrowIfCancellationRequested();

            if (!payload.EmailVerified ||
                string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email))
            {
                return null;
            }

            var fullName = string.IsNullOrWhiteSpace(payload.Name)
                ? payload.Email.Split('@')[0]
                : payload.Name.Trim();

            return new VerifiedGoogleIdentity
            {
                Subject = payload.Subject,
                Email = payload.Email.Trim(),
                FullName = fullName
            };
        }
        catch (InvalidJwtException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ExternalAuthenticationProviderException(
                "Google authentication is temporarily unavailable.",
                exception);
        }
    }
}
