using System.Text;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Internal;

internal static class SnapshotRunner
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static async Task RunAsync(
        IReadOnlyList<SnapshotTarget> targets,
        SnapshotSettings settings,
        string sourceFilePath,
        string memberName
    )
    {
        var location = SnapshotNaming.Resolve(settings, sourceFilePath, memberName);
        Directory.CreateDirectory(location.Directory);
        var accept = AcceptMode.IsEnabled;
        var failures = new List<string>();

        foreach (var target in targets)
        {
            var baseName = location.Name + target.Suffix;
            var verifiedPath = Path.Combine(
                location.Directory,
                $"{baseName}.verified.{target.Extension}"
            );
            var receivedPath = Path.Combine(
                location.Directory,
                $"{baseName}.received.{target.Extension}"
            );

            var failure = target.Text is not null
                ? await CompareTextAsync(target.Text, verifiedPath, receivedPath, accept)
                : await CompareBinaryAsync(target, verifiedPath, receivedPath, accept);
            if (failure is not null)
            {
                failures.Add(failure);
            }
        }

        if (failures.Count > 0)
        {
            var message = new StringBuilder();
            message.Append("Snapshot verification failed.\n");
            message.Append(
                $"Accept by renaming *.received.* to *.verified.*, or rerun with {AcceptMode.Variables} set to 1.\n\n"
            );
            message.AppendJoin("\n", failures);
            throw new SnapshotMismatchException(message.ToString());
        }
    }

    internal static string NormalizeText(string text)
    {
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static async Task<string?> CompareTextAsync(
        string text,
        string verifiedPath,
        string receivedPath,
        bool accept
    )
    {
        var received = NormalizeText(text);
        string? verified = null;
        if (File.Exists(verifiedPath))
        {
            verified = NormalizeText(await File.ReadAllTextAsync(verifiedPath));
            if (verified == received || verified == received + "\n")
            {
                DeleteIfExists(receivedPath);
                return null;
            }
        }

        if (accept)
        {
            await File.WriteAllTextAsync(verifiedPath, received, Utf8WithBom);
            DeleteIfExists(receivedPath);
            return null;
        }

        await File.WriteAllTextAsync(receivedPath, received, Utf8WithBom);
        if (verified is null)
        {
            return $"New snapshot: {receivedPath}\n{received}\n";
        }

        return $"Snapshot mismatch: {verifiedPath}\n"
            + LineDiff.Unified(
                verified,
                received,
                Path.GetFileName(verifiedPath),
                Path.GetFileName(receivedPath)
            );
    }

    private static async Task<string?> CompareBinaryAsync(
        SnapshotTarget target,
        string verifiedPath,
        string receivedPath,
        bool accept
    )
    {
        var received = target.Data!;
        string? reason = null;
        if (File.Exists(verifiedPath))
        {
            var verified = await File.ReadAllBytesAsync(verifiedPath);
            var result = target.Comparer is null
                ? CompareBytes(verified, received)
                : target.Comparer(verified, received);
            if (result.IsEqual)
            {
                DeleteIfExists(receivedPath);
                return null;
            }

            reason = result.Message;
        }

        if (accept)
        {
            await File.WriteAllBytesAsync(verifiedPath, received);
            DeleteIfExists(receivedPath);
            return null;
        }

        await File.WriteAllBytesAsync(receivedPath, received);
        return File.Exists(verifiedPath)
            ? $"Snapshot mismatch: {verifiedPath}\n{reason ?? "Binary content differs."}\n"
            : $"New snapshot: {receivedPath}\n";
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static SnapshotCompareResult CompareBytes(byte[] verified, byte[] received) =>
        verified.AsSpan().SequenceEqual(received)
            ? SnapshotCompareResult.Equal
            : SnapshotCompareResult.NotEqual();
}

internal static class AcceptMode
{
    public const string Variables = "SNAPSHOT_ACCEPT or UPDATE_SNAPSHOTS";

    public static bool IsEnabled =>
        !IsTrue(Environment.GetEnvironmentVariable("CI"))
        && (
            IsTrue(Environment.GetEnvironmentVariable("SNAPSHOT_ACCEPT"))
            || IsTrue(Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS"))
        );

    private static bool IsTrue(string? value) =>
        value is not null
        && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
}
