using System.Text;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Test;

[CollectionDefinition(nameof(EnvironmentCollection), DisableParallelization = true)]
public class EnvironmentCollection;

/// <summary>Received/verified workflow. Runs without parallelization because it changes environment variables.</summary>
[Collection(nameof(EnvironmentCollection))]
public sealed class WorkflowTests : IDisposable
{
    private static readonly string[] Variables = ["CI", "SNAPSHOT_ACCEPT", "UPDATE_SNAPSHOTS"];

    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "snapshots-" + Guid.NewGuid().ToString("N")
    );
    private readonly Dictionary<string, string?> _originalVariables = Variables.ToDictionary(
        v => v,
        Environment.GetEnvironmentVariable
    );

    public WorkflowTests()
    {
        foreach (var variable in Variables)
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    public void Dispose()
    {
        foreach (var (variable, value) in _originalVariables)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private SnapshotSettings Settings =>
        new SnapshotSettings().UseDirectory(_directory).UseFileName("Sample");

    private string Verified => Path.Combine(_directory, "Sample.verified.txt");
    private string Received => Path.Combine(_directory, "Sample.received.txt");

    [Fact]
    public async Task NewSnapshot_WritesReceivedWithBom_AndFails()
    {
        var exception = await Should.ThrowAsync<SnapshotMismatchException>(() =>
            Snapshot.Verify(new { Name = "a" }, Settings)
        );

        exception.Message.ShouldContain("New snapshot");
        File.Exists(Verified).ShouldBeFalse();
        var bytes = await File.ReadAllBytesAsync(Received, TestContext.Current.CancellationToken);
        bytes.Take(3).ShouldBe(new byte[] { 0xEF, 0xBB, 0xBF });
        Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3).ShouldBe("{\n  Name: a\n}");
    }

    [Fact]
    public async Task Match_Passes_AndDeletesStaleReceived()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            Verified,
            "{\r\n  Name: a\r\n}\r\n",
            new UTF8Encoding(true),
            TestContext.Current.CancellationToken
        );
        await File.WriteAllTextAsync(Received, "stale", TestContext.Current.CancellationToken);

        await Snapshot.Verify(new { Name = "a" }, Settings);

        File.Exists(Received).ShouldBeFalse();
    }

    [Fact]
    public async Task Mismatch_ThrowsWithUnifiedDiff()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            Verified,
            "{\n  Name: a,\n  Other: same\n}",
            TestContext.Current.CancellationToken
        );

        var exception = await Should.ThrowAsync<SnapshotMismatchException>(() =>
            Snapshot.Verify(new { Name = "b", Other = "same" }, Settings)
        );

        exception.Message.ShouldContain("--- Sample.verified.txt");
        exception.Message.ShouldContain("+++ Sample.received.txt");
        exception.Message.ShouldContain("-  Name: a,");
        exception.Message.ShouldContain("+  Name: b,");
        exception.Message.ShouldContain("   Other: same");
        File.Exists(Received).ShouldBeTrue();
    }

    [Theory]
    [InlineData("SNAPSHOT_ACCEPT", "1")]
    [InlineData("UPDATE_SNAPSHOTS", "true")]
    public async Task AcceptMode_WritesVerified(string variable, string value)
    {
        Environment.SetEnvironmentVariable(variable, value);

        await Snapshot.Verify(new { Name = "a" }, Settings);

        (await File.ReadAllTextAsync(Verified, TestContext.Current.CancellationToken)).ShouldBe(
            "{\n  Name: a\n}"
        );
        File.Exists(Received).ShouldBeFalse();
    }

    [Fact]
    public async Task AcceptMode_IsIgnoredOnCi()
    {
        Environment.SetEnvironmentVariable("SNAPSHOT_ACCEPT", "1");
        Environment.SetEnvironmentVariable("CI", "true");

        await Should.ThrowAsync<SnapshotMismatchException>(() =>
            Snapshot.Verify(new { Name = "a" }, Settings)
        );

        File.Exists(Verified).ShouldBeFalse();
    }

    [Fact]
    public async Task MultipleTargets_ReportAllFailures_AndUseBinaryComparer()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(
            Path.Combine(_directory, "Sample.verified.txt"),
            "text",
            TestContext.Current.CancellationToken
        );
        await File.WriteAllBytesAsync(
            Path.Combine(_directory, "Sample#00.verified.bin"),
            [1, 2, 3],
            TestContext.Current.CancellationToken
        );
        await File.WriteAllBytesAsync(
            Path.Combine(_directory, "Sample#01.verified.bin"),
            [9],
            TestContext.Current.CancellationToken
        );

        var exception = await Should.ThrowAsync<SnapshotMismatchException>(() =>
            Snapshot.VerifyTargets(
                [
                    SnapshotTarget.ForText("text"),
                    SnapshotTarget.ForBinary(
                        [1, 2, 4],
                        "bin",
                        (_, _) => SnapshotCompareResult.Equal,
                        "#00"
                    ),
                    SnapshotTarget.ForBinary([8], "bin", suffix: "#01"),
                    SnapshotTarget.ForBinary(
                        [7],
                        "bin",
                        (_, _) => SnapshotCompareResult.NotEqual("custom reason"),
                        "#02"
                    ),
                ],
                Settings,
                "",
                ""
            )
        );

        exception.Message.ShouldContain("Sample#01.verified.bin");
        exception.Message.ShouldContain("Sample#02.received.bin");
        exception.Message.ShouldNotContain("Sample.verified.txt");
        File.Exists(Path.Combine(_directory, "Sample#00.received.bin")).ShouldBeFalse();
        File.Exists(Path.Combine(_directory, "Sample#01.received.bin")).ShouldBeTrue();
    }

    [Fact]
    public async Task CustomComparerMessage_IsReported()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllBytesAsync(
            Path.Combine(_directory, "Sample.verified.bin"),
            [1],
            TestContext.Current.CancellationToken
        );

        var exception = await Should.ThrowAsync<SnapshotMismatchException>(() =>
            Snapshot.VerifyTargets(
                [
                    SnapshotTarget.ForBinary(
                        [2],
                        "bin",
                        (_, _) => SnapshotCompareResult.NotEqual("custom reason")
                    ),
                ],
                Settings,
                "",
                ""
            )
        );

        exception.Message.ShouldContain("custom reason");
    }
}
