namespace ExpenseTracker.Application.Common;

public sealed class ExternalAuthenticationProviderException : Exception
{
    public ExternalAuthenticationProviderException(string message)
        : base(message)
    {
    }

    public ExternalAuthenticationProviderException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
