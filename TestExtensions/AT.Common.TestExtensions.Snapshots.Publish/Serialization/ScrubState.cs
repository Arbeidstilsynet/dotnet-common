using System.Globalization;
using System.Text.RegularExpressions;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Serialization;

/// <summary>Per-serialization scrubbing state: one counter per category, equal values reuse numbers.</summary>
internal sealed partial class ScrubState(SnapshotSettings settings)
{
    private readonly Dictionary<object, int> _guids = [];
    private readonly Dictionary<object, int> _dateTimes = [];
    private readonly Dictionary<object, int> _dateTimeOffsets = [];
    private readonly Dictionary<object, int> _dates = [];
    private readonly Dictionary<object, int> _times = [];

    public SnapshotSettings Settings { get; } = settings;

    public string Guid(Guid value)
    {
        if (!Settings.ScrubGuidsEnabled)
        {
            return value.ToString("D");
        }

        return value == System.Guid.Empty ? "Guid_Empty" : "Guid_" + Number(_guids, value);
    }

    public string DateTime(DateTime value)
    {
        if (!Settings.ScrubDateTimesEnabled)
        {
            return ScalarFormatter.FormatDateTime(value);
        }

        if (value == System.DateTime.MinValue)
        {
            return "Date_MinValue";
        }

        if (value == System.DateTime.MaxValue)
        {
            return "Date_MaxValue";
        }

        return "DateTime_" + Number(_dateTimes, value);
    }

    public string DateTimeOffset(DateTimeOffset value)
    {
        if (!Settings.ScrubDateTimesEnabled)
        {
            return ScalarFormatter.FormatDateTimeOffset(value);
        }

        if (value == System.DateTimeOffset.MinValue)
        {
            return "Date_MinValue";
        }

        if (value == System.DateTimeOffset.MaxValue)
        {
            return "Date_MaxValue";
        }

        return "DateTimeOffset_" + Number(_dateTimeOffsets, (value.UtcTicks, true));
    }

    public string DateOnly(DateOnly value)
    {
        if (!Settings.ScrubDateTimesEnabled)
        {
            return ScalarFormatter.FormatDateOnly(value);
        }

        if (value == System.DateOnly.MinValue)
        {
            return "Date_MinValue";
        }

        if (value == System.DateOnly.MaxValue)
        {
            return "Date_MaxValue";
        }

        return "Date_" + Number(_dates, value);
    }

    public string TimeOnly(TimeOnly value)
    {
        if (!Settings.ScrubDateTimesEnabled)
        {
            return ScalarFormatter.FormatTimeOnly(value);
        }

        if (value == System.TimeOnly.MinValue)
        {
            return "Time_MinValue";
        }

        if (value == System.TimeOnly.MaxValue)
        {
            return "Time_MaxValue";
        }

        return "Time_" + Number(_times, value);
    }

    /// <summary>Scrubs a string value: whole-value GUID, whole-value ISO date-time, then inline GUIDs.</summary>
    public string String(string value)
    {
        if (Settings.ScrubGuidsEnabled && TryParseGuid(value, out var guid))
        {
            return Guid(guid);
        }

        if (Settings.ScrubDateTimesEnabled && IsoDateTime().IsMatch(value))
        {
            var zoned = value.EndsWith('Z') || IsoOffsetSuffix().IsMatch(value);
            if (
                System.DateTimeOffset.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var parsed
                )
            )
            {
                // Zoned strings compare by instant; zoneless strings only match the identical text.
                object key = zoned ? (parsed.UtcTicks, true) : value;
                return "DateTimeOffset_" + Number(_dateTimeOffsets, key);
            }
        }

        return InlineGuids(value);
    }

    /// <summary>Replaces GUIDs embedded in text when <see cref="SnapshotSettings.ScrubInlineGuids"/> is on.</summary>
    public string InlineGuids(string value)
    {
        if (!Settings.ScrubInlineGuidsEnabled)
        {
            return value;
        }

        return InlineGuid()
            .Replace(
                value,
                match =>
                {
                    var guid = System.Guid.Parse(match.Value);
                    return guid == System.Guid.Empty
                        ? "Guid_Empty"
                        : "Guid_" + Number(_guids, guid);
                }
            );
    }

    /// <summary>Whole-value GUIDs in any standard format; surrounding whitespace is ignored.</summary>
    private static bool TryParseGuid(string value, out Guid guid)
    {
        guid = System.Guid.Empty;
        return value.Length is >= 32 and <= 80 && System.Guid.TryParse(value, out guid);
    }

    private static int Number(Dictionary<object, int> counter, object key)
    {
        if (!counter.TryGetValue(key, out var number))
        {
            number = counter.Count + 1;
            counter[key] = number;
        }

        return number;
    }

    [GeneratedRegex(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d{1,7})?)?(Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex IsoDateTime();

    [GeneratedRegex(@"[+-]\d{2}:\d{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex IsoOffsetSuffix();

    [GeneratedRegex(
        "(?<![0-9A-Za-z])(?:[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}|[0-9A-Fa-f]{32})(?![0-9A-Za-z])",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex InlineGuid();
}
