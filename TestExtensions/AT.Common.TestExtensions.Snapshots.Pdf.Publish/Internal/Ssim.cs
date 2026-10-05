using System.Globalization;
using SkiaSharp;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf.Internal;

/// <summary>
/// Structural similarity index (Wang, Bovik, Sheikh &amp; Simoncelli, 2004) on 8-bit luminance,
/// averaged over non-overlapping 8×8 windows. Transparent pixels are composited over white.
/// Windows that are uniform and identical in both images (blank background) are excluded so that
/// small content changes on mostly empty pages are not averaged away.
/// </summary>
internal static class Ssim
{
    private const int Window = 8;
    private const double C1 = 0.01 * 255 * (0.01 * 255);
    private const double C2 = 0.03 * 255 * (0.03 * 255);

    public static SnapshotCompareResult Compare(
        byte[] verifiedPng,
        byte[] receivedPng,
        double threshold,
        double windowThreshold = 0
    )
    {
        using var verified = Decode(verifiedPng);
        using var received = Decode(receivedPng);
        if (verified is null || received is null)
        {
            return SnapshotCompareResult.NotEqual("Could not decode PNG.");
        }

        if (verified.Width != received.Width || verified.Height != received.Height)
        {
            return SnapshotCompareResult.NotEqual(
                $"Image size differs: verified {verified.Width}x{verified.Height}, received {received.Width}x{received.Height}."
            );
        }

        var score = Compute(
            Luminance(verified),
            Luminance(received),
            verified.Width,
            verified.Height
        );
        if (score.Mean < threshold)
        {
            return SnapshotCompareResult.NotEqual(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"SSIM {score.Mean:0.0000} is below the threshold {threshold:0.0000}."
                )
            );
        }

        if (score.Min < windowThreshold)
        {
            return SnapshotCompareResult.NotEqual(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Local SSIM {score.Min:0.0000} at ({score.MinX}, {score.MinY}) is below the window threshold {windowThreshold:0.0000}."
                )
            );
        }

        return SnapshotCompareResult.Equal;
    }

    internal readonly record struct Score(double Mean, double Min, int MinX, int MinY);

    internal static Score Compute(byte[] a, byte[] b, int width, int height)
    {
        double total = 0,
            min = 1;
        int windows = 0,
            minX = 0,
            minY = 0;
        for (var top = 0; top < height; top += Window)
        {
            for (var left = 0; left < width; left += Window)
            {
                var w = Math.Min(Window, width - left);
                var h = Math.Min(Window, height - top);
                if (WindowSsim(a, b, width, left, top, w, h) is not { } value)
                {
                    continue;
                }

                total += value;
                windows++;
                if (value < min)
                {
                    (min, minX, minY) = (value, left, top);
                }
            }
        }

        return windows == 0 ? new Score(1, 1, 0, 0) : new Score(total / windows, min, minX, minY);
    }

    private static double? WindowSsim(
        byte[] a,
        byte[] b,
        int stride,
        int left,
        int top,
        int w,
        int h
    )
    {
        double sumA = 0,
            sumB = 0,
            sumAA = 0,
            sumBB = 0,
            sumAB = 0;
        for (var y = top; y < top + h; y++)
        {
            var row = y * stride;
            for (var x = left; x < left + w; x++)
            {
                double va = a[row + x];
                double vb = b[row + x];
                sumA += va;
                sumB += vb;
                sumAA += va * va;
                sumBB += vb * vb;
                sumAB += va * vb;
            }
        }

        var n = (double)(w * h);
        var meanA = sumA / n;
        var meanB = sumB / n;
        var varA = sumAA / n - meanA * meanA;
        var varB = sumBB / n - meanB * meanB;
        var covariance = sumAB / n - meanA * meanB;
        if (varA <= 0 && varB <= 0 && sumA == sumB)
        {
            return null;
        }

        return (2 * meanA * meanB + C1)
            * (2 * covariance + C2)
            / ((meanA * meanA + meanB * meanB + C1) * (varA + varB + C2));
    }

    private static SKBitmap? Decode(byte[] png)
    {
        using var codec = SKCodec.Create(new MemoryStream(png));
        return codec is null ? null : SKBitmap.Decode(codec);
    }

    private static byte[] Luminance(SKBitmap bitmap)
    {
        using var rgba =
            bitmap.Copy(SKColorType.Rgba8888)
            ?? throw new InvalidOperationException("Unsupported PNG.");
        var pixels = rgba.GetPixelSpan();
        var premultiplied = rgba.AlphaType == SKAlphaType.Premul;
        var result = new byte[rgba.Width * rgba.Height];
        for (var i = 0; i < result.Length; i++)
        {
            var p = i * 4;
            double alpha = pixels[p + 3] / 255.0;
            double r = pixels[p],
                g = pixels[p + 1],
                bl = pixels[p + 2];
            if (!premultiplied)
            {
                r *= alpha;
                g *= alpha;
                bl *= alpha;
            }

            // Composite over white: premultiplied colour + white * (1 - alpha).
            var white = 255 * (1 - alpha);
            var y = 0.299 * (r + white) + 0.587 * (g + white) + 0.114 * (bl + white);
            result[i] = (byte)Math.Clamp(Math.Round(y), 0, 255);
        }

        return result;
    }
}
