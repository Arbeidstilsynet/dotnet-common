# Third-party notices

`Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf` is licensed under MIT. It does not include third-party source code. It references the NuGet packages below, which are downloaded separately under their own licenses.

| Package | Version | License | Project |
| --- | --- | --- | --- |
| PdfPig | 0.1.16 | Apache-2.0 | https://github.com/UglyToad/PdfPig |
| PDFtoImage | 5.4.0 | MIT | https://github.com/sungaila/PDFtoImage |
| SkiaSharp (+ NativeAssets.Linux.NoDependencies / macOS / Win32) | 4.150.1 | MIT | https://github.com/mono/SkiaSharp |
| bblanchon.PDFium (Linux / macOS / Win32) | 152.0.7961 | Apache-2.0 (package); PDFium itself is BSD-3-Clause with Apache-2.0 components | https://github.com/bblanchon/pdfium-binaries |

Native binaries that ship with these packages (PDFium, Skia, and the FreeType, libpng and zlib libraries they bundle) remain under their upstream licenses. The full license texts are distributed inside each package.

Every license above is on the allow-list enforced in CI (MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause).
