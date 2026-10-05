using System.Runtime.CompilerServices;
using Arbeidstilsynet.Common.TestExtensions.Snapshots.Internal;
using Arbeidstilsynet.Common.TestExtensions.Snapshots.Serialization;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots;

/// <summary>
/// Snapshot assertions. Import with <c>using static Arbeidstilsynet.Common.TestExtensions.Snapshots.Snapshot;</c>.
/// </summary>
public static class Snapshot
{
    /// <summary>
    /// Serializes <paramref name="target"/> and compares it with <c>{Class}.{Method}.verified.txt</c>
    /// next to the calling source file (or in <see cref="SnapshotSettings.UseDirectory"/>).
    /// </summary>
    /// <exception cref="SnapshotMismatchException">The snapshot is new or differs from the verified file.</exception>
    public static Task Verify(
        object? target,
        SnapshotSettings? settings = null,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = ""
    )
    {
        settings ??= new SnapshotSettings();
        var text = Serialize(target, settings);
        return VerifyTargets([SnapshotTarget.ForText(text)], settings, sourceFilePath, memberName);
    }

    /// <summary>Serializes <paramref name="target"/> to snapshot text, applying all scrubbers in <paramref name="settings"/>.</summary>
    public static string Serialize(object? target, SnapshotSettings? settings = null) =>
        SnapshotSerializer.Serialize(target, settings ?? new SnapshotSettings());

    /// <summary>
    /// Compares several snapshot files produced by one assertion. Intended for extension packages.
    /// </summary>
    /// <exception cref="SnapshotMismatchException">At least one target is new or differs from its verified file.</exception>
    public static Task VerifyTargets(
        IReadOnlyList<SnapshotTarget> targets,
        SnapshotSettings? settings,
        string sourceFilePath,
        string memberName
    )
    {
        ArgumentNullException.ThrowIfNull(targets);
        return SnapshotRunner.RunAsync(
            targets,
            settings ?? new SnapshotSettings(),
            sourceFilePath,
            memberName
        );
    }
}
