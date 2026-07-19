namespace ExpenseTracker.Infrastructure.Identity;

public sealed class TwilioVerifySettings
{
    public const string SectionName =
        "AuthenticationProviders:TwilioVerify";

    public string AccountSid { get; set; } = string.Empty;

    public string AuthToken { get; set; } = string.Empty;

    public string ServiceSid { get; set; } = string.Empty;
}
