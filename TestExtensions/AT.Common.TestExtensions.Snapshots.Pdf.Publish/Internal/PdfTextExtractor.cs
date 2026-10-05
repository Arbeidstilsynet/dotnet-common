using System.Globalization;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;

internal sealed record PdfTextPage(int Index, string Text);

internal sealed record PdfTextDocument(string Version, int PageCount, IReadOnlyList<PdfTextPage> Pages);

internal static class PdfTextExtractor
{
    public static PdfTextDocument Extract(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);
        var pages = document
            .GetPages()
            .Select((page, index) => new PdfTextPage(index, Normalize(ContentOrderTextExtractor.GetText(page))))
            .ToList();
        return new PdfTextDocument(
            document.Version.ToString("0.0", CultureInfo.InvariantCulture),
            document.NumberOfPages,
            pages
        );
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');
}
