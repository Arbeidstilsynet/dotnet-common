using System.Runtime.CompilerServices;
using Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf;

/// <summary>
/// PDF snapshot assertions: one text snapshot with the extracted page text, plus one PNG per page
/// compared with SSIM so that minor anti-aliasing differences between machines are tolerated.
/// </summary>
public static class PdfSnapshot
{
    /// <summary>Verifies a PDF document read from <paramref name="pdf"/>.</summary>
    /// <exception cref="SnapshotMismatchException">A snapshot is new or differs from the verified file.</exception>
    public static async Task VerifyPdf(
        Stream pdf,
        SnapshotSettings? settings = null,
        PdfSnapshotOptions? options = null,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(pdf);
        using var buffer = new MemoryStream();
        await pdf.CopyToAsync(buffer);
        await VerifyPdf(buffer.ToArray(), settings, options, sourceFilePath, memberName);
    }

    /// <summary>Verifies a PDF document given as bytes.</summary>
    /// <exception cref="SnapshotMismatchException">A snapshot is new or differs from the verified file.</exception>
    public static Task VerifyPdf(
        byte[] pdf,
        SnapshotSettings? settings = null,
        PdfSnapshotOptions? options = null,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(pdf);
        settings ??= new SnapshotSettings();
        options ??= new PdfSnapshotOptions();

        var targets = new List<SnapshotTarget>
        {
            SnapshotTarget.ForText(Snapshot.Serialize(PdfTextExtractor.Extract(pdf), settings)),
        };

        if (options.IncludeImages)
        {
            var pages = PdfRenderer.RenderPages(pdf, options.Dpi);
            var threshold = options.SsimThreshold;
            var windowThreshold = options.MinWindowSsim;
            for (var i = 0; i < pages.Count; i++)
            {
                var suffix = pages.Count == 1 ? null : "#" + i.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
                targets.Add(
                    SnapshotTarget.ForBinary(pages[i], "png", (verified, received) => Ssim.Compare(verified, received, threshold, windowThreshold), suffix)
                );
            }
        }

        return Snapshot.VerifyTargets(targets, settings, sourceFilePath, memberName);
    }
}
