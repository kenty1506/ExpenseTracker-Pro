using System.Diagnostics;
using System.Security.Claims;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;

namespace ExpenseTracker.Api.Middleware;

public sealed class AuditLogMiddleware
{
    public const string AuditUserIdItemKey =
        "ExpenseTracker.AuditUserId";

    private static readonly HashSet<string> MutatingMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Patch,
            HttpMethods.Delete
        };

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!MutatingMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var auditLog = new AuditLog
        {
            UserId = ResolveUserId(context),
            Method = context.Request.Method,
            Module = ResolveModule(context),
            Operation = ResolveOperation(context),
            EntityId = ResolveEntityId(context),
            Route = context.Request.Path.Value ?? string.Empty,
            Action = ResolveAction(context),
            StatusCode = context.Response.StatusCode,
            Succeeded = context.Response.StatusCode is >= 200 and < 400,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            TraceId = context.TraceIdentifier,
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ExpenseTrackerDbContext>();

            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync(
                context.RequestAborted.IsCancellationRequested
                    ? CancellationToken.None
                    : context.RequestAborted);

            _logger.LogInformation(
                "Mutation {Method} {Route} returned {StatusCode} in {ElapsedMs} ms. TraceId: {TraceId}",
                auditLog.Method,
                auditLog.Route,
                auditLog.StatusCode,
                stopwatch.ElapsedMilliseconds,
                auditLog.TraceId);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not persist audit event for TraceId {TraceId}.",
                context.TraceIdentifier);
        }
    }

    private static string ResolveAction(HttpContext context)
    {
        var controller = context.GetRouteValue("controller")?.ToString();
        var action = context.GetRouteValue("action")?.ToString();

        return string.Join(
            ".",
            new[] { controller, action }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string? ResolveUserId(HttpContext context)
    {
        var authenticatedUserId =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            return authenticatedUserId;
        }

        return context.Items.TryGetValue(
                AuditUserIdItemKey,
                out var auditUserId)
            ? auditUserId?.ToString()
            : null;
    }

    private static string ResolveModule(HttpContext context)
    {
        return context.GetRouteValue("controller")?.ToString()
            ?? "System";
    }

    private static string ResolveOperation(HttpContext context)
    {
        var action = context.GetRouteValue("action")?.ToString();

        if (!string.IsNullOrWhiteSpace(action))
        {
            return action;
        }

        return context.Request.Method.ToUpperInvariant() switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Execute"
        };
    }

    private static string? ResolveEntityId(HttpContext context)
    {
        var preferredKeys = new[]
        {
            "contributionId",
            "id",
            "financialGoalId",
            "accountId",
            "transactionId",
            "transferId",
            "budgetId",
            "categoryId",
            "notificationId",
            "recurringTransactionId"
        };

        foreach (var key in preferredKeys)
        {
            if (context.Request.RouteValues.TryGetValue(
                    key,
                    out var value) &&
                value is not null)
            {
                return value.ToString();
            }
        }

        var routeIdentifier = context.Request.RouteValues
            .FirstOrDefault(item =>
                item.Key.EndsWith(
                    "Id",
                    StringComparison.OrdinalIgnoreCase) &&
                item.Value is not null)
            .Value?
            .ToString();

        if (!string.IsNullOrWhiteSpace(routeIdentifier))
        {
            return routeIdentifier;
        }

        var location = context.Response.Headers["Location"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var path = location.Split('?', 2)[0]
            .TrimEnd('/');
        var lastSegment = path.Split('/')
            .LastOrDefault();

        return long.TryParse(lastSegment, out _)
            ? lastSegment
            : null;
    }
}
