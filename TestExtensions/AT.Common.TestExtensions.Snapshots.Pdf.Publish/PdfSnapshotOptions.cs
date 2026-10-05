namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf;

/// <summary>Options for <see cref="PdfSnapshot.VerifyPdf(Stream, SnapshotSettings?, PdfSnapshotOptions?, string, string)"/>.</summary>
public sealed record PdfSnapshotOptions
{
    /// <summary>Resolution used to render page images. Default 144 (an A4 page becomes 1191×1684 pixels).</summary>
    public int Dpi { get; init; } = 144;

    /// <summary>Minimum mean SSIM (0–1) for a rendered page to match its verified image. Default 0.98.</summary>
    public double SsimThreshold { get; init; } = 0.98;

    /// <summary>
    /// Minimum SSIM (−1–1) for every 8×8 window that contains content. Catches small local changes such as a
    /// changed word, which barely move the mean on a full page. Default 0.5. Set to −1 to disable.
    /// </summary>
    public double MinWindowSsim { get; init; } = 0.5;

    /// <summary>Whether to render and compare page images. When <c>false</c> only the text snapshot is verified.</summary>
    public bool IncludeImages { get; init; } = true;
}
