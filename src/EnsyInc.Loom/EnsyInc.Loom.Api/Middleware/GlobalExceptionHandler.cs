using EnsyInc.Loom.Api.Exceptions;
using EnsyInc.Loom.Api.Models;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;

namespace EnsyInc.Loom.Api.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is ValidationException validationException)
        {
            var parameters = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => string.Join(" ", g.Select(e => e.ErrorMessage)));

            // No exception instance passed here on purpose: client-input validation failures are
            // routine, not exceptional, and logging the exception object would print its full stack
            // trace on every single one.
            logger.LogWarning("Validation failed while processing {Method} {Path}: {Errors}", httpContext.Request.Method, httpContext.Request.Path, string.Join("; ", parameters.Select(p => $"{p.Key}: {p.Value}")));

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(ErrorResponses.ValidationError(parameters), ct);

            return true;
        }

        if (exception is UnhandledResultErrorException)
        {
            logger.LogError(exception, "Controller reached an unhandled result error case while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(ErrorResponses.UnexpectedError, ct);

        return true;
    }
}
