namespace ExpenseTracker.Infrastructure.Identity;

public sealed class GoogleAuthSettings
{
    public const string SectionName =
        "AuthenticationProviders:Google";

    public string ClientId { get; set; } = string.Empty;
}
