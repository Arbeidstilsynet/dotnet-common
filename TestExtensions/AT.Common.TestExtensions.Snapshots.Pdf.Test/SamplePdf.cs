using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

internal static class SamplePdf
{
    public static byte[] Create(params string[][] pages)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
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
}
