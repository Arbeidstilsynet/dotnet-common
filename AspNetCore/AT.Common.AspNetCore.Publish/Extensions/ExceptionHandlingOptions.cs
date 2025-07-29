using System.Net;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

/// <summary>
/// Options for configuring exception handling in the API.
/// </summary>
public record ExceptionHandlingOptions
{
    private readonly Dictionary<Type, HttpStatusCode> _exceptionToStatusCodeMapping = new()
    {
        { typeof(ArgumentException), HttpStatusCode.BadRequest },
        { typeof(FormatException), HttpStatusCode.BadRequest },
        { typeof(BadHttpRequestException), HttpStatusCode.BadRequest },
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
    public IReadOnlyDictionary<Type, HttpStatusCode> ExceptionToStatusCodeMapping =>
        _exceptionToStatusCodeMapping;

    /// <summary>
    /// Adds a mapping from an exception type to a specific HTTP status code.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    public ExceptionHandlingOptions AddExceptionMapping<TException>(HttpStatusCode statusCode)
        where TException : Exception
    {
        _exceptionToStatusCodeMapping[typeof(TException)] = statusCode;
        return this;
    }
}
