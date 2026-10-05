# Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf

PDF snapshot testing on top of [Arbeidstilsynet.Common.TestExtensions.Snapshots](https://github.com/Arbeidstilsynet/dotnet-common/blob/main/TestExtensions/AT.Common.TestExtensions.Snapshots.Publish/README.md).

A PDF is verified as:

- **`{name}.verified.txt`**: the PDF version, the page count and the text of each page. Text diffs are readable in pull requests.
- **`{name}.verified.png`** (one page) or **`{name}#00.verified.png`, `#01`, …** (several pages): every page rendered with PDFium. Images are compared with SSIM, so small rasterisation differences between machines are tolerated while layout and content changes still fail.

> **Preview.** Legal sign-off on licensing is required before the first non-preview release. See [THIRD-PARTY-NOTICES.md](https://github.com/Arbeidstilsynet/dotnet-common/blob/main/TestExtensions/AT.Common.TestExtensions.Snapshots.Pdf.Publish/THIRD-PARTY-NOTICES.md).

## Usage

```csharp
using Arbeidstilsynet.Common.TestExtensions.Snapshots;
using Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf;

[Theory]
[InlineData(Språk.Bokmål)]
[InlineData(Språk.Nynorsk)]
public async Task Confirmation_MatchesSnapshot(Språk språk)
{
    await using var pdf = await _brevgen.GenerateConfirmation(melding, språk);

    await PdfSnapshot.VerifyPdf(pdf, new SnapshotSettings().UseDirectory("Snapshots").ScrubInlineGuids());
}
```

The text snapshot looks like this:

```
{
  Version: 1.7,
  PageCount: 2,
  Pages: [
    {
      Text:
Forhåndsmelding
Referanse: Guid_1
    },
    {
      Index: 1,
      Text: Side 2 av 2
    }
  ]
}
```

## Options

| `PdfSnapshotOptions` | Default | Meaning |
| --- | --- | --- |
| `Dpi` | `144` | Render resolution. A4 is 1191×1684 pixels. |
| `SsimThreshold` | `0.98` | Minimum mean SSIM over content areas. |
| `MinWindowSsim` | `0.5` | Minimum SSIM for every 8×8 content window. Catches small local changes such as a single changed word. `-1` disables it. |
| `IncludeImages` | `true` | Set to `false` to verify only the text snapshot. |

The received/verified workflow, accept mode (`SNAPSHOT_ACCEPT=1`) and all `SnapshotSettings` work as in the core package. The algorithm is specified in [SPEC.md §6](https://github.com/Arbeidstilsynet/dotnet-common/blob/main/TestExtensions/AT.Common.TestExtensions.Snapshots.Publish/SPEC.md#6-pdf-snapshots-pdf-package).

## Migrating from Verify.PdfPig / Verify.DocNet

- Text snapshots are compatible and can be kept as they are.
- Page images must be re-accepted once: delete the old `*.verified.png` files, run with `SNAPSHOT_ACCEPT=1` and commit the result.
- Delete any `#pdf.verified.pdf` files. They are no longer produced.

## Platforms

Native PDFium and Skia binaries are included for Linux (no extra system dependencies), macOS and Windows.

PNG snapshots are only stable across operating systems when the PDF **embeds its fonts**. Non-embedded fonts, such as the standard-14 Helvetica, are replaced by a system font, and that font differs between macOS, Linux and Windows. PDFs from QuestPDF embed their fonts. For hand-built test PDFs, embed a TrueType font, or set `IncludeImages = false`.
