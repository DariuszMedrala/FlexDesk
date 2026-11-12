using FlexDesk.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message
        };

        if (exception is DbUpdateConcurrencyException)
        {
            problemDetails.Detail = "The resource was modified concurrently. Please retry.";
        }

        if (exception is ValidationException validationException)
        {
            problemDetails.Detail = "One or more validation errors occurred.";
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
        BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error")
    };
}
