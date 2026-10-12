using Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;
using SkiaSharp;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Test;

public class SsimTests
{
    private static readonly byte[] Page = PdfRenderer.RenderPages(
        SamplePdf.Create(["Forhandsmelding", "Linje to"]),
        72
    )[0];

    [Fact]
    public void IdenticalImages_AreEqual()
    {
        Ssim.Compare(Page, Page, 0.98).ShouldBe(SnapshotCompareResult.Equal);
    }

    [Fact]
    public void Rendering_IsDeterministic()
    {
        var again = PdfRenderer.RenderPages(SamplePdf.Create(["Forhandsmelding", "Linje to"]), 72)[
            0
        ];
        Ssim.Compare(Page, again, 0.999).ShouldBe(SnapshotCompareResult.Equal);
    }

    [Fact]
    public void DifferentContent_IsNotEqual()
    {
        var other = PdfRenderer.RenderPages(SamplePdf.Create(["Ulykkesvarsel", "Annen linje"]), 72)[
            0
        ];

        var result = Ssim.Compare(Page, other, 0.98);

        result.ShouldNotBe(SnapshotCompareResult.Equal);
        result.Message!.ShouldContain("SSIM");
    }

    [Fact]
    public void OneChangedWord_IsNotEqual()
    {
        var other = PdfRenderer.RenderPages(SamplePdf.Create(["Forhandsmelding", "Linje tre"]), 72)[
            0
        ];

        Ssim.Compare(Page, other, 0.98).IsEqual.ShouldBeTrue();
        Ssim.Compare(Page, other, 0.98, 0.5).Message!.ShouldContain("Local SSIM");
    }

    [Fact]
    public void AntiAliasingNoise_IsTolerated()
    {
        using var bitmap = SKBitmap.Decode(Page);
        var random = new Random(42);
        for (var i = 0; i < 500; i++)
        {
            var (x, y) = (random.Next(bitmap.Width), random.Next(bitmap.Height));
            var pixel = bitmap.GetPixel(x, y);
            var value = (byte)Math.Clamp(pixel.Red + random.Next(-10, 11), 0, 255);
            bitmap.SetPixel(x, y, new SKColor(value, value, value));
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);

        Ssim.Compare(Page, data.ToArray(), 0.98, 0.5).ShouldBe(SnapshotCompareResult.Equal);
    }

    [Fact]
    public void DifferentSize_IsNotEqual()
    {
        var larger = PdfRenderer.RenderPages(SamplePdf.Create(["Forhandsmelding", "Linje to"]), 96)[
            0
        ];

        Ssim.Compare(Page, larger, 0.98).Message!.ShouldContain("size differs");
    }

    [Fact]
    public void TransparentPixels_AreCompositedOverWhite()
    {
        Ssim.Compute([255, 255], [255, 255], 2, 1).Mean.ShouldBe(1);
        Ssim.Compute([0, 255], [255, 0], 2, 1).Mean.ShouldBeLessThan(0.5);
        using var transparent = new SKBitmap(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
        transparent.Erase(SKColors.Transparent);
        using var white = new SKBitmap(8, 8, SKColorType.Rgba8888, SKAlphaType.Premul);
        white.Erase(SKColors.White);

        Ssim.Compare(Encode(transparent), Encode(white), 0.999)
            .ShouldBe(SnapshotCompareResult.Equal);
    }

    [Fact]
    public void UndecodableImage_IsNotEqual()
    {
        Ssim.Compare(Page, [1, 2, 3], 0.98).Message!.ShouldContain("decode");
    }

    private static byte[] Encode(SKBitmap bitmap)
    {
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
