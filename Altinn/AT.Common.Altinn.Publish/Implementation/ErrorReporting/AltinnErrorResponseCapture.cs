using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;

/// <summary>
/// Carries the body of an error response from <see cref="AltinnErrorResponseCaptureHandler"/>
/// back to <see cref="AltinnErrorReportingRequestAdapter"/>.
/// </summary>
/// <remarks>
/// Kiota copies request options onto the outgoing <see cref="HttpRequestMessage"/>, which is the
/// only channel between the request adapter and the HTTP pipeline for a single request.
/// </remarks>
internal sealed class AltinnErrorResponseCapture : IRequestOption
{
    public static readonly HttpRequestOptionsKey<IRequestOption> Key = new(
        typeof(AltinnErrorResponseCapture).FullName!
    );

    public string? Body { get; set; }
}

/// <summary>
/// Records the body of an error response, so that it survives even when the specification
/// declares no error type for the status code and Kiota would otherwise discard it.
/// </summary>
/// <remarks>
/// The content is buffered first, so Kiota can still read it to parse the problem details.
/// </remarks>
internal sealed class AltinnErrorResponseCaptureHandler : DelegatingHandler
{
    internal const int MaxBodyLength = 4096;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (
            response.IsSuccessStatusCode
            || !request.Options.TryGetValue(AltinnErrorResponseCapture.Key, out var option)
            || option is not AltinnErrorResponseCapture capture
        )
        {
            return response;
        }

        await response.Content.LoadIntoBufferAsync(cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        capture.Body =
            body.Length > MaxBodyLength ? $"{body[..MaxBodyLength]}... (truncated)" : body;

        return response;
    }
}
