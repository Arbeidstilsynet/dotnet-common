namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

public class PdfSnapshotTests
{
    private static SnapshotSettings Settings => new SnapshotSettings().UseDirectory("Snapshots");

    [Fact]
    public Task SinglePage() =>
        PdfSnapshot.VerifyPdf(
            SamplePdf.Create([
                "Forhandsmelding",
                "Referanse: 3f2504e0-4f89-11d3-9a0c-0305e82c3301",
            ]),
            Settings
        );

    [Fact]
    public Task MultiplePages() =>
        PdfSnapshot.VerifyPdf(SamplePdf.Create(["Side en", "Linje to"], ["Side to"]), Settings);

    [Fact]
    public Task InlineGuidsAreScrubbedInText() =>
        PdfSnapshot.VerifyPdf(
            SamplePdf.Create(["Referanse: 3f2504e0-4f89-11d3-9a0c-0305e82c3301"]),
            Settings.ScrubInlineGuids(),
            new PdfSnapshotOptions { IncludeImages = false }
        );

    [Fact]
    public async Task Stream_IsSupported()
    {
        using var stream = new MemoryStream(SamplePdf.Create(["Side en", "Linje to"], ["Side to"]));
        await PdfSnapshot.VerifyPdf(stream, Settings.UseFileName("PdfSnapshotTests.MultiplePages"));
    }
}
