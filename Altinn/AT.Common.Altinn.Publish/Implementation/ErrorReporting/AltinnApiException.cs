using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;

/// <summary>
/// A failed Altinn request, described well enough to diagnose from a log line alone.
/// </summary>
/// <remarks>
/// The exceptions the generated clients throw carry no useful message -- the parsed problem
/// details are only reachable through <c>GetAltinnProblemDetails()</c>, and a status code the
/// specification does not describe loses the response body entirely. This wraps the original
/// exception, kept as <see cref="Exception.InnerException"/>, and states the request, the status
/// and what Altinn said in <see cref="Exception.Message"/>. It remains an
/// <see cref="ApiException"/> with the same status code and headers, so existing handling that
/// catches <see cref="ApiException"/> is unaffected.
/// </remarks>
internal sealed class AltinnApiException : ApiException
{
    public AltinnApiException(string message, ApiException original, string? responseBody = null)
        : base(message, original)
    {
        ResponseStatusCode = original.ResponseStatusCode;
        ResponseHeaders = original.ResponseHeaders;
        ResponseBody = responseBody;
    }

    public ApiException Original => (ApiException)InnerException!;

    /// <summary>
    /// The response body as captured, possibly truncated, or <see langword="null"/> if none was.
    /// </summary>
    public string? ResponseBody { get; }
}
