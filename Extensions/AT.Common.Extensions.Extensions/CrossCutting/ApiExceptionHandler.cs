using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arbeidstilsynet.Common.Extensions;


/// <summary>
/// Options for configuring exception handling in the API.
/// </summary>
public record ExceptionHandlingOptions
{
    private readonly Dictionary<Type, int> _exceptionToStatusCodeMapping = new()
    {
        { typeof(ArgumentException), StatusCodes.Status400BadRequest },
        { typeof(FormatException), StatusCodes.Status400BadRequest },
        { typeof(BadHttpRequestException), StatusCodes.Status400BadRequest },
    };

    /// <summary>
    /// Gets the mapping of exception types to HTTP status codes.
    ///
    /// Default mappings include:
    /// - ArgumentException: 400 Bad Request
    /// - FormatException: 400 Bad Request
    /// - BadHttpRequestException: 400 Bad Request
    /// - Default to 500 Internal Server Error for any unmapped exceptions.
    /// </summary>
    public IReadOnlyDictionary<Type, int> ExceptionToStatusCodeMapping => _exceptionToStatusCodeMapping;
    
    
    /// <summary>
    /// Adds a mapping from an exception type to a specific HTTP status code.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    public ExceptionHandlingOptions AddExceptionMapping<TException>(int statusCode) where TException : Exception
    {
        _exceptionToStatusCodeMapping[typeof(TException)] = statusCode;
        return this;
    }
}

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

    private static int GetStatusCode(this IReadOnlyDictionary<Type, int> mapping, Exception? exception)
    {
        return exception is null ? StatusCodes.Status500InternalServerError 
            : mapping.GetValueOrDefault(exception.GetType(), StatusCodes.Status500InternalServerError);
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
