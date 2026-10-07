namespace Arbeidstilsynet.Common.TestExtensions.Snapshots;

/// <summary>
/// Per-test (or per-class) configuration for <see cref="Snapshot.Verify"/>.
/// All methods return the same instance so calls can be chained.
/// </summary>
public sealed class SnapshotSettings
{
    /// <summary>Creates settings with the defaults: GUIDs and dates scrubbed, empty collections and default values omitted.</summary>
    public SnapshotSettings() { }

    /// <summary>Creates a copy of <paramref name="other"/>.</summary>
    public SnapshotSettings(SnapshotSettings other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Directory = other.Directory;
        FileName = other.FileName;
        Parameters = other.Parameters;
        ScrubGuidsEnabled = other.ScrubGuidsEnabled;
        ScrubDateTimesEnabled = other.ScrubDateTimesEnabled;
        IgnoreEmptyCollectionsEnabled = other.IgnoreEmptyCollectionsEnabled;
        IncludeDefaultValuesEnabled = other.IncludeDefaultValuesEnabled;
        ScrubInlineGuidsEnabled = other.ScrubInlineGuidsEnabled;
        Scrubbers = [.. other.Scrubbers];
        ScrubbedMembers = [.. other.ScrubbedMembers];
    }

    internal string? Directory { get; private set; }
    internal string? FileName { get; private set; }
    internal object?[]? Parameters { get; private set; }
    internal bool ScrubGuidsEnabled { get; private set; } = true;
    internal bool ScrubDateTimesEnabled { get; private set; } = true;
    internal bool IgnoreEmptyCollectionsEnabled { get; private set; } = true;
    internal bool IncludeDefaultValuesEnabled { get; private set; }
    internal bool ScrubInlineGuidsEnabled { get; private set; }
    internal List<Func<string, string>> Scrubbers { get; } = [];
    internal HashSet<string> ScrubbedMembers { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Stores snapshots in <paramref name="directory"/>, relative to the directory of the calling source file.
    /// </summary>
    public SnapshotSettings UseDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory = directory;
        return this;
    }

    /// <summary>Uses <paramref name="fileName"/> instead of <c>{Class}.{Method}{Parameters}</c>.</summary>
    public SnapshotSettings UseFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        FileName = fileName;
        return this;
    }

    /// <summary>
    /// Uses these values for the parameter part of the file name instead of the theory arguments.
    /// Values are paired positionally with the test method's parameter names.
    /// </summary>
    public SnapshotSettings UseParameters(params object?[] parameters)
    {
        Parameters = parameters;
        return this;
    }

    /// <summary>Replaces GUIDs embedded inside strings with <c>Guid_n</c>.</summary>
    public SnapshotSettings ScrubInlineGuids()
    {
        ScrubInlineGuidsEnabled = true;
        return this;
    }

    /// <summary>Writes GUID values as-is instead of <c>Guid_n</c>.</summary>
    public SnapshotSettings DontScrubGuids()
    {
        ScrubGuidsEnabled = false;
        return this;
    }

    /// <summary>Writes date and time values as-is instead of <c>DateTime_n</c>, <c>Date_n</c> and so on.</summary>
    public SnapshotSettings DontScrubDateTimes()
    {
        ScrubDateTimesEnabled = false;
        return this;
    }

    /// <summary>Writes empty collections as <c>[]</c>/<c>{}</c> instead of omitting them.</summary>
    public SnapshotSettings DontIgnoreEmptyCollections()
    {
        IgnoreEmptyCollectionsEnabled = false;
        return this;
    }

    /// <summary>Writes members holding <c>null</c> or their type's default value instead of omitting them.</summary>
    public SnapshotSettings IncludeDefaultValues()
    {
        IncludeDefaultValuesEnabled = true;
        return this;
    }

    /// <summary>
    /// Replaces the value of every member, dictionary entry or claim with one of these names
    /// (case-sensitive, at any depth) with <c>{Scrubbed}</c>, even when the value is null or default.
    /// </summary>
    public SnapshotSettings ScrubMembers(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);
        foreach (var name in names)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ScrubbedMembers.Add(name);
        }

        return this;
    }

    /// <summary>Adds a scrubber that runs on the complete serialized text before comparison.</summary>
    public SnapshotSettings AddScrubber(Func<string, string> scrubber)
    {
        ArgumentNullException.ThrowIfNull(scrubber);
        Scrubbers.Add(scrubber);
        return this;
    }
}
