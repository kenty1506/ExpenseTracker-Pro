using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Identity;

public sealed class SendGridPasswordResetEmailSender
    : IPasswordResetEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly SendGridSettings _settings;

    public SendGridPasswordResetEmailSender(
        HttpClient httpClient,
        IOptions<SendGridSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task SendAsync(
        string email,
        string fullName,
        string encodedResetToken,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        var separator = _settings.PasswordResetUrl.Contains('?')
            ? "&"
            : "?";

        var resetUrl =
            $"{_settings.PasswordResetUrl}{separator}" +
            $"email={Uri.EscapeDataString(email)}&" +
            $"token={Uri.EscapeDataString(encodedResetToken)}";

        var safeName = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(fullName)
                ? "there"
                : fullName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        var payload = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[]
                    {
                        new { email }
                    }
                }
            },
            @from = new
            {
                email = _settings.FromEmail,
                name = _settings.FromName
            },
            subject = "Reset your ExpenseTracker Pro password",
            content = new object[]
            {
                new
                {
                    type = "text/plain",
                    value =
                        "Use this link to reset your ExpenseTracker Pro " +
                        $"password: {resetUrl}"
                },
                new
                {
                    type = "text/html",
                    value =
                        $"<p>Hello {safeName},</p>" +
                        "<p>Use the link below to reset your password.</p>" +
                        $"<p><a href=\"{safeUrl}\">Reset password</a></p>" +
                        "<p>If you did not request this, ignore this email.</p>"
                }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.sendgrid.com/v3/mail/send");
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _settings.ApiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalAuthenticationProviderException(
                "The password reset email could not be sent.");
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail) ||
            string.IsNullOrWhiteSpace(_settings.PasswordResetUrl))
        {
            throw new ExternalAuthenticationProviderException(
                "SendGrid password reset email is not configured.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(
                request,
                cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalAuthenticationProviderException(
                "Password reset email delivery timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalAuthenticationProviderException(
                "Password reset email delivery is temporarily unavailable.",
                exception);
        }
    }
}
