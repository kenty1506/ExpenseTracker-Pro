using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Identity;

public sealed class TwilioMobileVerificationService
    : IMobileVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly TwilioVerifySettings _settings;

    public TwilioMobileVerificationService(
        HttpClient httpClient,
        IOptions<TwilioVerifySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task StartAsync(
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        using var request = CreateRequest(
            "Verifications",
            new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["Channel"] = "sms"
            });

        using var response = await SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalAuthenticationProviderException(
                "The mobile verification code could not be sent.");
        }
    }

    public async Task<bool> CheckAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        using var request = CreateRequest(
            "VerificationCheck",
            new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["Code"] = code
            });

        using var response = await SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or
            HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalAuthenticationProviderException(
                "Mobile verification is temporarily unavailable.");
        }

        try
        {
            await using var responseStream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            using var document = await JsonDocument.ParseAsync(
                responseStream,
                cancellationToken: cancellationToken);

            return document.RootElement.TryGetProperty(
                    "status",
                    out var status) &&
                string.Equals(
                    status.GetString(),
                    "approved",
                    StringComparison.OrdinalIgnoreCase);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalAuthenticationProviderException(
                "Mobile verification timed out.");
        }
        catch (JsonException exception)
        {
            throw new ExternalAuthenticationProviderException(
                "Mobile verification returned an invalid response.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalAuthenticationProviderException(
                "Mobile verification is temporarily unavailable.",
                exception);
        }
    }

    private HttpRequestMessage CreateRequest(
        string resource,
        Dictionary<string, string> formValues)
    {
        var serviceSid = Uri.EscapeDataString(
            _settings.ServiceSid.Trim());

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://verify.twilio.com/v2/Services/" +
            $"{serviceSid}/{resource}");

        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
                $"{_settings.AccountSid}:{_settings.AuthToken}"));

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(formValues);

        return request;
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
                "Mobile verification timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalAuthenticationProviderException(
                "Mobile verification is temporarily unavailable.",
                exception);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.AccountSid) ||
            string.IsNullOrWhiteSpace(_settings.AuthToken) ||
            string.IsNullOrWhiteSpace(_settings.ServiceSid))
        {
            throw new ExternalAuthenticationProviderException(
                "Twilio Verify is not configured.");
        }
    }
}
