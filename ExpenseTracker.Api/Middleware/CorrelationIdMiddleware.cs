namespace ExpenseTracker.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestedId = context.Request.Headers[HeaderName]
            .FirstOrDefault();

        var correlationId = IsValid(requestedId)
            ? requestedId!
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }

    private static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            value.Length <= 100 &&
            value.All(character =>
                char.IsLetterOrDigit(character) ||
                character is '-' or '_' or '.');
    }
}
