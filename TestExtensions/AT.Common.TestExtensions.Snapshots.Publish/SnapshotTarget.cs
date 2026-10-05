namespace Arbeidstilsynet.Common.TestExtensions.Snapshots;

/// <summary>Result of comparing a received binary snapshot with the verified one.</summary>
/// <param name="IsEqual">Whether the snapshots are considered equal.</param>
/// <param name="Message">Optional explanation shown when they differ.</param>
public readonly record struct SnapshotCompareResult(bool IsEqual, string? Message = null)
{
    /// <summary>The snapshots are equal.</summary>
    public static SnapshotCompareResult Equal => new(true);

    /// <summary>The snapshots differ.</summary>
    public static SnapshotCompareResult NotEqual(string? message = null) => new(false, message);
}

/// <summary>
/// One file produced by a snapshot assertion. Extension packages (for example PDF support)
/// build targets and pass them to <see cref="Snapshot.VerifyTargets"/>.
/// </summary>
public sealed class SnapshotTarget
{
    private SnapshotTarget(
        string extension,
        string? suffix,
        string? text,
        byte[]? data,
        Func<byte[], byte[], SnapshotCompareResult>? comparer
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        Extension = extension.TrimStart('.');
        Suffix = suffix;
        Text = text;
        Data = data;
        Comparer = comparer;
    }

    /// <summary>File extension without the leading dot, for example <c>txt</c>.</summary>
    public string Extension { get; }

    /// <summary>Optional suffix appended to the snapshot name, for example <c>#00</c>.</summary>
    public string? Suffix { get; }

    /// <summary>Text content, when this is a text target.</summary>
    public string? Text { get; }

    /// <summary>Binary content, when this is a binary target.</summary>
    public byte[]? Data { get; }

    /// <summary>Comparer for binary content. Byte equality is used when <c>null</c>.</summary>
    public Func<byte[], byte[], SnapshotCompareResult>? Comparer { get; }

    /// <summary>Creates a text target. Text is stored as UTF-8 with BOM and <c>\n</c> line endings.</summary>
    public static SnapshotTarget ForText(string text, string extension = "txt", string? suffix = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new(extension, suffix, text, null, null);
    }

    /// <summary>Creates a binary target with an optional custom comparer (verified, received).</summary>
    public static SnapshotTarget ForBinary(
        byte[] data,
        string extension,
        Func<byte[], byte[], SnapshotCompareResult>? comparer = null,
        string? suffix = null
    )
    {
        ArgumentNullException.ThrowIfNull(data);
        return new(extension, suffix, null, data, comparer);
    }
}

/// <summary>Thrown when one or more received snapshots do not match the verified snapshots.</summary>
public sealed class SnapshotMismatchException : Exception
{
    /// <summary>Creates the exception.</summary>
    public SnapshotMismatchException(string message)
        : base(message) { }
}
