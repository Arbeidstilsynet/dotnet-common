using System.Globalization;
using System.Runtime.CompilerServices;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Test.Golden;

/// <summary>
/// Serializes every case in <see cref="GoldenCorpus"/> and compares it byte-for-byte with the output
/// produced by Verify for the same input (stored in <c>Golden/Expected</c>).
/// </summary>
public class GoldenTests
{
    public static TheoryData<string> CaseNames => [.. GoldenCorpus.Cases.Select(c => c.Name)];

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Serialize_MatchesVerifyOutput(string name)
    {
        var goldenCase = GoldenCorpus.Cases.Single(c => c.Name == name);
        var expected = ReadExpected(name);

        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nb-NO");
        try
        {
            var actual = Snapshot.Serialize(goldenCase.Create(), ToSettings(goldenCase.Options));
            actual.ShouldBe(expected);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    private static SnapshotSettings ToSettings(GoldenOptions options)
    {
        var settings = new SnapshotSettings();
        if (options.HasFlag(GoldenOptions.DontScrubGuids))
        {
            settings.DontScrubGuids();
        }

        if (options.HasFlag(GoldenOptions.DontScrubDateTimes))
        {
            settings.DontScrubDateTimes();
        }

        if (options.HasFlag(GoldenOptions.DontIgnoreEmptyCollections))
        {
            settings.DontIgnoreEmptyCollections();
        }

        if (options.HasFlag(GoldenOptions.IncludeDefaultValues))
        {
            settings.IncludeDefaultValues();
        }

        if (options.HasFlag(GoldenOptions.ScrubInlineGuids))
        {
            settings.ScrubInlineGuids();
        }

        return settings;
    }

    private static string ReadExpected(string name, [CallerFilePath] string sourceFile = "")
    {
        var path = Path.Combine(
            Path.GetDirectoryName(sourceFile)!,
            "Expected",
            name + ".verified.txt"
        );
        var bytes = File.ReadAllBytes(path);
        bytes
            .AsSpan(0, 3)
            .ToArray()
            .ShouldBe(new byte[] { 0xEF, 0xBB, 0xBF }, "golden files start with a UTF-8 BOM");
        return new System.Text.UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
    }
}
