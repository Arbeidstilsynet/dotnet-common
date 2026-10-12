using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

internal static class SamplePdf
{
    // Embedded so rendering doesn't depend on OS font substitution (Noto Sans, OFL-1.1, see Fonts/OFL.txt).
    private static readonly byte[] Font = LoadFont();

    public static byte[] Create(params string[][] pages)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddTrueTypeFont(Font);
        foreach (var lines in pages)
        {
            var page = builder.AddPage(PageSize.A4);
            page.DrawRectangle(new PdfPoint(50, 700), 495, 80);
            for (var i = 0; i < lines.Length; i++)
            {
                page.AddText(lines[i], 14, new PdfPoint(60, 750 - i * 20), font);
            }
        }

        return builder.Build();
    }

    private static byte[] LoadFont()
    {
        using var stream = typeof(SamplePdf).Assembly.GetManifestResourceStream(
            "NotoSans-Regular.ttf"
        )!;
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
