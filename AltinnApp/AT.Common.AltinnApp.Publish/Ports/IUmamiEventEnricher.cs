using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Enriches Umami events emitted for an Altinn HTTP request, or records custom events via <see cref="IUmamiEventRecorder"/>.
/// </summary>
public interface IUmamiEventEnricher
{
    /// <summary>
    /// Called before the HTTP response is sent (and when an unhandled exception occurs) to attach
    /// sanitized request properties or record custom events on <paramref name="recorder"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="recorder">The request-scoped <see cref="IUmamiEventRecorder"/>.</param>
    /// <param name="exception">The unhandled exception thrown by the request pipeline, if any.</param>
    /// <returns>A task representing the asynchronous enrichment operation.</returns>
    ValueTask EnrichAsync(
        HttpContext context,
        IUmamiEventRecorder recorder,
        Exception? exception = null
    );
}
