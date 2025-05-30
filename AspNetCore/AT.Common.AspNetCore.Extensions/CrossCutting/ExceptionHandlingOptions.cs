using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

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
    public IReadOnlyDictionary<Type, int> ExceptionToStatusCodeMapping =>
        _exceptionToStatusCodeMapping;

    /// <summary>
    /// Adds a mapping from an exception type to a specific HTTP status code.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    public ExceptionHandlingOptions AddExceptionMapping<TException>(int statusCode)
        where TException : Exception
    {
        _exceptionToStatusCodeMapping[typeof(TException)] = statusCode;
        return this;
    }
}
