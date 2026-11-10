using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.AltinnApp.Ports;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed record QueuedUmamiEvent(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("data")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyDictionary<string, object?>? Data
);

internal sealed class UmamiEventRecorder : IUmamiEventRecorder
{
    private const int MaxEventsPerRequest = 16;
    private const int MaxPropertiesPerCollection = 24;

    private readonly List<QueuedUmamiEvent> _events = [];
    private readonly Dictionary<string, object?> _enrichments = new(StringComparer.Ordinal);

    public IReadOnlyList<QueuedUmamiEvent> Events => _events;
    public IReadOnlyDictionary<string, object?> Enrichments => _enrichments;

    public void RecordEvent(
        string eventName,
        IReadOnlyDictionary<string, object?>? properties = null
    )
    {
        if (string.IsNullOrWhiteSpace(eventName) || _events.Count >= MaxEventsPerRequest)
        {
            return;
        }

        var normalizedName = eventName.Trim();
        var sanitizedProps = SanitizeProperties(properties);
        _events.Add(new QueuedUmamiEvent(normalizedName, sanitizedProps));
    }

    public void EnrichCurrentRequest(IReadOnlyDictionary<string, object?> properties)
    {
        if (properties is null)
        {
            return;
        }

        foreach (var (key, value) in properties)
        {
            if (string.IsNullOrWhiteSpace(key) || _enrichments.Count >= MaxPropertiesPerCollection)
            {
                continue;
            }

            if (SanitizeValue(value) is { } sanitized)
            {
                _enrichments[key.Trim()] = sanitized;
            }
        }
    }

    private static IReadOnlyDictionary<string, object?>? SanitizeProperties(
        IReadOnlyDictionary<string, object?>? properties
    )
    {
        if (properties is null || properties.Count == 0)
        {
            return null;
        }

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in properties)
        {
            if (string.IsNullOrWhiteSpace(key) || result.Count >= MaxPropertiesPerCollection)
            {
                continue;
            }

            if (SanitizeValue(value) is { } sanitized)
            {
                result[key.Trim()] = sanitized;
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static object? SanitizeValue(object? value) =>
        value switch
        {
            null => null,
            string s when !string.IsNullOrWhiteSpace(s) => s.Trim(),
            bool b => b,
            int or long or short or byte or double or float or decimal => value,
            IEnumerable<string> list => list.Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Take(16)
                .ToArray(),
            _ => value.ToString(),
        };
}
