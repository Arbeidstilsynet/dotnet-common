using System.Net;
using System.Text.Json;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

/// <summary>
/// A rejected Maskinporten token request, with structured diagnostics for the caller.
/// </summary>
/// <remarks>
/// Remains an <see cref="HttpRequestException"/> with the response status and bounded response
/// body in its message. The message can contain sensitive data and must not be logged directly.
/// The caller decides how to sanitize and log the provider's error code and configured scopes.
/// </remarks>
public sealed class MaskinportenTokenException : HttpRequestException
{
    internal MaskinportenTokenException(
        HttpStatusCode statusCode,
        IReadOnlyList<string> requestedScopes,
        string responseBody
    )
        : base(
            $"Maskinporten token request failed with {(int)statusCode} ({statusCode}): {responseBody}",
            inner: null,
            statusCode: statusCode
        )
    {
        RequestedScopes = Array.AsReadOnly(requestedScopes.ToArray());
        ResponseBody = responseBody;
        (ErrorCode, ErrorDescription) = ReadError(responseBody);
    }

    /// <summary>
    /// The response's OAuth <c>error</c> value, or <see langword="null"/> when absent, malformed,
    /// truncated or not a string. This is provider-supplied text, not a log-safe value.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// The provider's OAuth <c>error_description</c>, or <see langword="null"/> when unavailable
    /// or not a string. Can contain sensitive data; do not log or include in trace tags.
    /// </summary>
    public string? ErrorDescription { get; }

    /// <summary>
    /// The bounded response body, including a truncation marker when applicable. Preserves
    /// non-JSON responses and provider-specific fields. Can contain sensitive data; do not log.
    /// </summary>
    public string ResponseBody { get; }

    /// <summary>A read-only snapshot of the scopes requested by the failed call.</summary>
    public IReadOnlyList<string> RequestedScopes { get; }

    private static (string? Code, string? Description) ReadError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return (null, null);
            return (ReadString(root, "error"), ReadString(root, "error_description"));
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
