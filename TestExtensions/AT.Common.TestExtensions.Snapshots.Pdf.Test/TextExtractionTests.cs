using Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

public class TextExtractionTests
{
    [Fact]
    public void Extract_ReturnsVersionPageCountAndText()
    {
        var document = PdfTextExtractor.Extract(SamplePdf.Create(["A", "B"], ["C"]));

        document.PageCount.ShouldBe(2);
        document.Version.ShouldMatch(@"^\d\.\d$");
        document.Pages.Select(p => p.Index).ShouldBe([0, 1]);
        document.Pages[0].Text.ShouldBe("A\nB");
        document.Pages[1].Text.ShouldBe("C");
    }

    [Fact]
    public void Serialize_OmitsIndexZero()
    {
        var text = Snapshot.Serialize(PdfTextExtractor.Extract(SamplePdf.Create(["A"], ["B"])));

        text.ShouldContain(
            "Pages: [\n    {\n      Text: A\n    },\n    {\n      Index: 1,\n      Text: B\n    }\n  ]"
        );
    }

    [Fact]
    public void Version_IsInvariantUnderNorwegianCulture()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nb-NO");
        try
        {
            PdfTextExtractor.Extract(SamplePdf.Create(["A"])).Version.ShouldNotContain(",");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }
}
