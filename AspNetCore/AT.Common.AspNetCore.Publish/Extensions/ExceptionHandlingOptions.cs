using System.Net;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

/// <summary>
/// Options for configuring exception handling in the API.
/// </summary>
public record ExceptionHandlingOptions
{
    private readonly Dictionary<
        Type,
        Func<Exception, HttpStatusCode>
    > _exceptionToStatusCodeMapping = new()
    {
        { typeof(ArgumentException), _ => HttpStatusCode.BadRequest },
        { typeof(FormatException), _ => HttpStatusCode.BadRequest },
        { typeof(BadHttpRequestException), _ => HttpStatusCode.BadRequest },
    };

    /// <summary>
    /// Resolve the status code based on the <paramref name="exception"/>
    /// <br/>
    /// Default mappings include:
    /// <br/>
    /// - ArgumentException: 400 Bad Request
    /// <br/>
    /// - FormatException: 400 Bad Request
    /// <br/>
    /// - BadHttpRequestException: 400 Bad Request
    /// <br/>
    /// - Default to 500 Internal Server Error for any unmapped exceptions.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public int GetStatusCode(Exception? exception)
    {
        if (
            exception == null
            || !_exceptionToStatusCodeMapping.TryGetValue(
                exception.GetType(),
                out var statusCodeResolver
            )
        )
        {
            return (int)HttpStatusCode.InternalServerError;
        }

        return (int)statusCodeResolver.Invoke(exception);
    }

    /// <summary>
    /// Adds a mapping from an exception type to a specific HTTP status code.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    public ExceptionHandlingOptions AddExceptionMapping<TException>(HttpStatusCode statusCode)
        where TException : Exception
    {
        _exceptionToStatusCodeMapping[typeof(TException)] = _ => statusCode;
        return this;
    }

    /// <summary>
    /// Adds a mapping from an exception type to a specific HTTP status code, where the status code is determined dynamically based on the exception instance.
    /// </summary>
    /// <param name="statusCodeResolver"></param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    public ExceptionHandlingOptions AddExceptionMapping<TException>(
        Func<TException?, HttpStatusCode> statusCodeResolver
    )
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(statusCodeResolver);

        _exceptionToStatusCodeMapping[typeof(TException)] = ex =>
            statusCodeResolver(ex as TException);
        return this;
    }
}
