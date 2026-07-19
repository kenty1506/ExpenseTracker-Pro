namespace ExpenseTracker.Infrastructure.Identity;

public sealed class SendGridSettings
{
    public const string SectionName =
        "AuthenticationProviders:SendGrid";

    public string ApiKey { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } =
        "ExpenseTracker Pro";

    public string PasswordResetUrl { get; set; } = string.Empty;
}
