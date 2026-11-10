namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Records custom Umami events and request-scoped event enrichments during an Altinn HTTP request
/// so they are emitted in the user's browser session when the response completes.
/// </summary>
public interface IUmamiEventRecorder
{
    /// <summary>
    /// Queues a custom Umami event to be emitted by the browser in the current user's session
    /// when the current HTTP request completes.
    /// </summary>
    /// <param name="eventName">The custom event name (for example <c>"adresse-ikke-funnet"</c>).</param>
    /// <param name="properties">Optional sanitized metadata properties to attach to the event's <c>data</c> payload.</param>
    void RecordEvent(string eventName, IReadOnlyDictionary<string, object?>? properties = null);

    /// <summary>
    /// Attaches additional sanitized properties to any event emitted for the current HTTP request
    /// (such as <c>skjema-feil</c>, <c>skjema-innsendt</c>, or custom events).
    /// </summary>
    /// <param name="properties">The key/value pairs to merge into the event's <c>data</c> payload.</param>
    void EnrichCurrentRequest(IReadOnlyDictionary<string, object?> properties);
}
