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
                ? "Wystąpił nieoczekiwany błąd."
                : exception.Message
        };

        if (exception is DbUpdateConcurrencyException)
        {
            problemDetails.Detail = "Zasób został zmodyfikowany równolegle. Spróbuj ponownie.";
        }

        if (exception is ValidationException validationException)
        {
            problemDetails.Detail = "Wystąpiły błędy walidacji.";
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
        ValidationException => (StatusCodes.Status400BadRequest, "Błąd walidacji"),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, "Brak autoryzacji"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Brak uprawnień"),
        NotFoundException => (StatusCodes.Status404NotFound, "Nie znaleziono"),
        ConflictException => (StatusCodes.Status409Conflict, "Konflikt"),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Konflikt"),
        BusinessRuleException => (StatusCodes.Status400BadRequest, "Naruszono regułę biznesową"),
        _ => (StatusCodes.Status500InternalServerError, "Błąd serwera")
    };
}
