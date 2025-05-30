using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

internal static class ApiExceptionHandler
{
    public static RequestDelegate CreateExceptionHandler(ExceptionHandlingOptions options)
    {
        return context => Handle(context, options);
    }

    private static Task Handle(HttpContext context, ExceptionHandlingOptions options)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var statusCode = options.ExceptionToStatusCodeMapping.GetStatusCode(exception);

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(exception.GetProblemDetails(statusCode));
    }

    private static int GetStatusCode(
        this IReadOnlyDictionary<Type, int> mapping,
        Exception? exception
    )
    {
        return exception is null
            ? StatusCodes.Status500InternalServerError
            : mapping.GetValueOrDefault(
                exception.GetType(),
                StatusCodes.Status500InternalServerError
            );
    }

    private static ProblemDetails GetProblemDetails(this Exception? exception, int statusCode)
    {
        return new()
        {
            Title = $"{exception?.GetType().Name ?? "Exception"} occured",
            Detail = exception?.Message ?? "An unexpected error occured",
            Status = statusCode,
        };
    }
}
