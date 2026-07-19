using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        /*
         * The client disconnected or cancelled the request.
         * Avoid trying to write a response when the connection is gone.
         */
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request was cancelled by the client. TraceId: {TraceId}",
                httpContext.TraceIdentifier);

            return true;
        }

        var error = MapException(exception);

        LogException(
            exception,
            error.StatusCode,
            httpContext.TraceIdentifier,
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Type = GetProblemType(error.StatusCode),
            Title = GetTitle(error.StatusCode),
            Status = error.StatusCode,
            Detail = GetDetail(
                exception,
                error.StatusCode,
                error.SafeDetail),
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        problemDetails.Extensions["timestamp"] =
            DateTime.UtcNow;

        httpContext.Response.StatusCode =
            error.StatusCode;

        httpContext.Response.ContentType =
            "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private ExceptionMapping MapException(
        Exception exception)
    {
        return exception switch
        {
            ExpenseTracker.Application.Common
                .ExternalAuthenticationProviderException =>
                new ExceptionMapping(
                    StatusCodes.Status503ServiceUnavailable,
                    "The external authentication service is temporarily " +
                    "unavailable. Please try again."),

            ValidationException validationException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    validationException.Message),

            ArgumentException argumentException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    argumentException.Message),

            KeyNotFoundException keyNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    keyNotFoundException.Message),

            UnauthorizedAccessException =>
                new ExceptionMapping(
                    StatusCodes.Status401Unauthorized,
                    "You are not authorized to perform this action."),

            DbUpdateConcurrencyException =>
                new ExceptionMapping(
                    StatusCodes.Status409Conflict,
                    "The record was modified by another operation. " +
                    "Refresh the data and try again."),

            DbUpdateException dbUpdateException =>
                MapDatabaseException(dbUpdateException),

            InvalidOperationException invalidOperationException =>
                new ExceptionMapping(
                    StatusCodes.Status409Conflict,
                    invalidOperationException.Message),

            NotSupportedException =>
                new ExceptionMapping(
                    StatusCodes.Status405MethodNotAllowed,
                    "This operation is not supported."),

            TimeoutException =>
                new ExceptionMapping(
                    StatusCodes.Status408RequestTimeout,
                    "The operation timed out. Please try again."),

            OperationCanceledException =>
                new ExceptionMapping(
                    StatusCodes.Status408RequestTimeout,
                    "The request was cancelled or timed out."),

            _ =>
                new ExceptionMapping(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
        };
    }

    private static ExceptionMapping MapDatabaseException(
        DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return sqlException.Number switch
            {
                // Unique constraint or duplicate index
                2601 or 2627 =>
                    new ExceptionMapping(
                        StatusCodes.Status409Conflict,
                        "A record with the same unique value already exists."),

                // Foreign-key or check-constraint violation
                547 =>
                    new ExceptionMapping(
                        StatusCodes.Status409Conflict,
                        "This operation conflicts with related data."),

                // Cannot insert NULL
                515 =>
                    new ExceptionMapping(
                        StatusCodes.Status400BadRequest,
                        "A required value is missing."),

                // String or binary data would be truncated
                2628 or 8152 =>
                    new ExceptionMapping(
                        StatusCodes.Status400BadRequest,
                        "One or more values exceed the allowed length."),

                // Deadlock victim
                1205 =>
                    new ExceptionMapping(
                        StatusCodes.Status503ServiceUnavailable,
                        "The database is temporarily busy. Please try again."),

                _ =>
                    new ExceptionMapping(
                        StatusCodes.Status500InternalServerError,
                        "A database error occurred.")
            };
        }

        return new ExceptionMapping(
            StatusCodes.Status500InternalServerError,
            "A database error occurred.");
    }

    private string GetDetail(
        Exception exception,
        int statusCode,
        string safeDetail)
    {
        if (statusCode !=
            StatusCodes.Status500InternalServerError)
        {
            return safeDetail;
        }

        /*
         * Development receives useful debugging details.
         * Production receives a safe generic message.
         */
        return _environment.IsDevelopment()
            ? exception.Message
            : safeDetail;
    }

    private void LogException(
        Exception exception,
        int statusCode,
        string traceId,
        string method,
        PathString path)
    {
        if (statusCode >=
            StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. " +
                "StatusCode: {StatusCode}. TraceId: {TraceId}",
                method,
                path,
                statusCode,
                traceId);

            return;
        }

        _logger.LogWarning(
            exception,
            "Request failed for {Method} {Path}. " +
            "StatusCode: {StatusCode}. TraceId: {TraceId}",
            method,
            path,
            statusCode,
            traceId);
    }

    private static string GetProblemType(
        int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.1",

            StatusCodes.Status401Unauthorized =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.2",

            StatusCodes.Status404NotFound =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.5",

            StatusCodes.Status405MethodNotAllowed =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.6",

            StatusCodes.Status408RequestTimeout =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.9",

            StatusCodes.Status409Conflict =>
                "https://tools.ietf.org/html/rfc9110#section-15.5.10",

            StatusCodes.Status503ServiceUnavailable =>
                "https://tools.ietf.org/html/rfc9110#section-15.6.4",

            _ =>
                "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }

    private static string GetTitle(
        int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "Bad request",

            StatusCodes.Status401Unauthorized =>
                "Unauthorized",

            StatusCodes.Status404NotFound =>
                "Resource not found",

            StatusCodes.Status405MethodNotAllowed =>
                "Method not allowed",

            StatusCodes.Status408RequestTimeout =>
                "Request timeout",

            StatusCodes.Status409Conflict =>
                "Conflict",

            StatusCodes.Status503ServiceUnavailable =>
                "Service unavailable",

            _ =>
                "Server error"
        };
    }

    private sealed record ExceptionMapping(
        int StatusCode,
        string SafeDetail);
}
