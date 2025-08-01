using System.Net;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

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
        this IReadOnlyDictionary<Type, HttpStatusCode> mapping,
        Exception? exception
    )
    {
        var statusCode = exception is null
            ? HttpStatusCode.InternalServerError
            : mapping.GetValueOrDefault(exception.GetType(), HttpStatusCode.InternalServerError);

        return (int)statusCode;
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
