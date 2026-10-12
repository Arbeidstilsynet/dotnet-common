using Arbeidstilsynet.Common.Altinn.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;

/// <summary>
/// Decorates a request adapter so that a failed request throws an <see cref="AltinnApiException"/>
/// stating what went wrong, rather than a bare <see cref="ApiException"/>.
/// </summary>
/// <remarks>
/// The response body is reported as captured by <see cref="AltinnErrorResponseCaptureHandler"/>.
/// If the handler is not in the pipeline, the parsed problem details are summarised instead.
/// </remarks>
internal sealed class AltinnErrorReportingRequestAdapter(IRequestAdapter inner, string clientName)
    : IRequestAdapter
{
    public ISerializationWriterFactory SerializationWriterFactory =>
        inner.SerializationWriterFactory;

    public string? BaseUrl
    {
        get => inner.BaseUrl;
        set => inner.BaseUrl = value;
    }

    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) =>
        inner.EnableBackingStore(backingStoreFactory);

    public Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default
    )
        where ModelType : IParsable =>
        Report(
            requestInfo,
            () => inner.SendAsync(requestInfo, factory, errorMapping, cancellationToken)
        );

    public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default
    )
        where ModelType : IParsable =>
        Report(
            requestInfo,
            () => inner.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken)
        );

    public Task<ModelType?> SendPrimitiveAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default
    ) =>
        Report(
            requestInfo,
            () => inner.SendPrimitiveAsync<ModelType>(requestInfo, errorMapping, cancellationToken)
        );

    public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default
    ) =>
        Report(
            requestInfo,
            () =>
                inner.SendPrimitiveCollectionAsync<ModelType>(
                    requestInfo,
                    errorMapping,
                    cancellationToken
                )
        );

    public Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default
    ) =>
        Report(
            requestInfo,
            async () =>
            {
                await inner.SendNoContentAsync(requestInfo, errorMapping, cancellationToken);
                return true;
            }
        );

    public Task<T?> ConvertToNativeRequestAsync<T>(
        RequestInformation requestInfo,
        CancellationToken cancellationToken = default
    ) => inner.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);

    private async Task<T> Report<T>(RequestInformation requestInfo, Func<Task<T>> send)
    {
        var capture = new AltinnErrorResponseCapture();
        requestInfo.AddRequestOptions([capture]);

        try
        {
            return await send();
        }
        catch (ApiException e) when (e is not AltinnApiException)
        {
            throw new AltinnApiException(Describe(requestInfo, e, capture.Body), e, capture.Body);
        }
    }

    private string Describe(RequestInformation requestInfo, ApiException exception, string? body)
    {
        var details = string.IsNullOrWhiteSpace(body) ? Summarise(exception) : body.Trim();

        return $"Altinn {clientName} request {requestInfo.HttpMethod} {Target(requestInfo)} "
            + $"failed with status {exception.ResponseStatusCode}: {details}";
    }

    /// <summary>
    /// The request URL without its query, which is where callers put identifying filters.
    /// </summary>
    private static string Target(RequestInformation requestInfo)
    {
        try
        {
            return requestInfo.URI.GetLeftPart(UriPartial.Path);
        }
        catch (Exception e) when (e is InvalidOperationException or UriFormatException)
        {
            return requestInfo.UrlTemplate ?? "(unknown URL)";
        }
    }

    private static string Summarise(ApiException exception)
    {
        var problem = exception.GetAltinnProblemDetails();

        if (problem is null)
        {
            return "(no response body)";
        }

        var parts = new[]
        {
            problem.Title,
            problem.Detail,
            problem.Code is { } code ? $"code={code}" : null,
            problem.ErrorCode is { } errorCode ? $"errorCode={errorCode}" : null,
            problem.TraceId is { } traceId ? $"traceId={traceId}" : null,
        };

        var summary = string.Join(" - ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

        return summary.Length > 0 ? summary : "(empty problem details)";
    }
}
