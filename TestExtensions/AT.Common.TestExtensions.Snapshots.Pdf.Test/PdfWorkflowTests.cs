namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

public sealed class PdfWorkflowTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pdf-snapshots-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task ChangedWord_FailsAndWritesReceivedPng()
    {
        Assert.SkipWhen(
            Environment.GetEnvironmentVariable("SNAPSHOT_ACCEPT") is not null
                || Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") is not null,
            "Accept mode is enabled."
        );
        var settings = new SnapshotSettings().UseDirectory(_directory).UseFileName("Letter");

        await Should.ThrowAsync<SnapshotMismatchException>(() =>
            PdfSnapshot.VerifyPdf(SamplePdf.Create(["Forhandsmelding", "Linje to"]), settings)
        );
        foreach (var received in Directory.GetFiles(_directory, "*.received.*"))
        {
            File.Move(received, received.Replace(".received.", ".verified."));
        }

        await PdfSnapshot.VerifyPdf(SamplePdf.Create(["Forhandsmelding", "Linje to"]), settings);

        var exception = await Should.ThrowAsync<SnapshotMismatchException>(() =>
            PdfSnapshot.VerifyPdf(SamplePdf.Create(["Forhandsmelding", "Linje tre"]), settings)
        );
        exception.Message.ShouldContain("+Linje tre");
        exception.Message.ShouldContain("Local SSIM");
        File.Exists(Path.Combine(_directory, "Letter.received.png")).ShouldBeTrue();
    }
}
