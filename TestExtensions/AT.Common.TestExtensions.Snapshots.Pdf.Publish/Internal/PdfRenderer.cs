using PDFtoImage;
using SkiaSharp;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;

internal static class PdfRenderer
{
    public static IReadOnlyList<byte[]> RenderPages(byte[] pdf, int dpi)
    {
        if (
            !(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
        )
        {
            throw new PlatformNotSupportedException(
                "PDF rendering is supported on Linux, macOS and Windows."
            );
        }

        var options = new RenderOptions(Dpi: dpi, BackgroundColor: SKColors.White);
        var pages = new List<byte[]>();
#pragma warning disable CA1416 // Guarded above; the analyzer does not understand the combined platform check.
        foreach (var bitmap in Conversion.ToImages(pdf, options: options))
#pragma warning restore CA1416
        {
            using (bitmap)
            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                pages.Add(data.ToArray());
            }
        }

        return pages;
    }
}
