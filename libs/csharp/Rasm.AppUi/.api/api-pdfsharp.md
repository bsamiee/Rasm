# [RASM_APPUI_API_PDFSHARP]

`PDFsharp` (assembly `PdfSharp.dll`, namespace `PdfSharp.*`) is the structured vector-PDF page object model: `PdfDocument` owns the page tree, info, options, and security; `XGraphics` is the device-independent drawing surface (the same `Draw*`/`XPen`/`XBrush`/`XFont`/`XMatrix` algebra over a page, an image, or a measure context); `PdfReader` opens/imports existing PDFs for read-modify-merge; and the `Signatures`/`Security`/`AcroForms`/`Annotations`/`UniversalAccessibility` namespaces add digital signing, encryption, form fields, link/markup annotations, and tagged-PDF accessibility. `PDFsharp-MigraDoc` (assemblies `MigraDoc.DocumentObjectModel.dll` / `MigraDoc.Rendering.dll` / `MigraDoc.RtfRendering.dll`, namespace `MigraDoc.*`) is the auto-paginated flow-report DOM rendered ONTO a `PdfDocument` through `PdfDocumentRenderer`. Together they are the vector-PDF + evidence-report deliverable the OOXML/DXF/raster export set (`api-drafting-export.md`) lacked: the `Render/drafting` and `Render/evidence` surfaces compose a MigraDoc `Document` (flow content) or draw directly with `XGraphics` (precise sheet layout), and the `NodeEditorAvalonia.ExportRenderer.RenderPdf` canvas export (`api-nodeeditor.md`) targets the same PDF deliverable. PDFsharp is platform-neutral (no Windows/GDI coupling), so it runs on the headless render path beside `Avalonia.Headless`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PDFsharp` 6.2.4
- package: `PDFsharp`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/PdfSharp.dll`, casing is `PdfSharp`); multi-targets, `net10.0` bound
- assembly: `PdfSharp` (primary) + satellites `PdfSharp.Charting`, `PdfSharp.Cryptography`, `PdfSharp.Quality`, `PdfSharp.Snippets`, `PdfSharp.BarCodes`, `PdfSharp.WPFonts`, `PdfSharp.System`, `PdfSharp.Shared`
- namespace: `PdfSharp.Drawing` (`XGraphics`, `XPen`/`XBrush`/`XFont`/`XColor`/`XImage`/`XPdfForm`/`XMatrix`/`XPoint`/`XRect`, 85 types), `PdfSharp.Drawing.Layout` (`XTextFormatter`), `PdfSharp.Pdf` (`PdfDocument`/`PdfPage`/`PdfDocumentOptions`/`PdfDocumentInformation`/`PdfOutlineCollection`, 62 types), `PdfSharp.Pdf.IO` (`PdfReader`/`PdfWriter`), `PdfSharp.Pdf.Security` (encryption), `PdfSharp.Pdf.Signatures` (digital signing), `PdfSharp.Pdf.AcroForms`, `PdfSharp.Pdf.Annotations`, `PdfSharp.Pdf.Content` (content-stream parser), `PdfSharp.UniversalAccessibility` (tagged PDF), `PdfSharp.Fonts`/`PdfSharp.Fonts.OpenType`, `PdfSharp.Logging`, `PdfSharp.Capabilities`
- depends: `Microsoft.Extensions.Logging.Abstractions`, `System.Security.Cryptography.Pkcs` (both ride existing/in-box rows)
- rail: pdf

[PACKAGE_SURFACE]: `PDFsharp-MigraDoc` 6.2.4
- package: `PDFsharp-MigraDoc`
- license: MIT
- floor: `net10.0` consumer; multi-targets net8.0 / net9.0 / net10.0 / netstandard2.0, `net10.0` bound
- assembly: `MigraDoc.DocumentObjectModel`, `MigraDoc.Rendering`, `MigraDoc.RtfRendering`
- namespace: `MigraDoc.DocumentObjectModel` (`Document`/`Section`/`Paragraph`/`Styles`/`Style`/`PageSetup`/`DocumentInfo`, 64 types), `…Tables` (`Table`/`Row`/`Column`/`Cell`), `…Shapes` + `…Shapes.Charts` (image/text-frame + native chart DOM), `…Fields` (page/date/info fields), `…IO` (DDL read/write), `…Visitors`; `MigraDoc.Rendering` (`PdfDocumentRenderer`/`DocumentRenderer`/the per-element renderers + `ChartMapper`); `MigraDoc.RtfRendering` (RTF emit)
- depends: `PDFsharp` (renders onto its `PdfDocument`/`XGraphics`)
- rail: pdf

## [02]-[PUBLIC_TYPES]

[DOCUMENT_TYPES]: `PdfSharp.Pdf` page-tree model
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PdfDocument`                                       | the document — page tree, `Info`/`Options`/`Settings`/`Outlines`/`SecuritySettings`, save/import |
|  [02]   | `PdfPage`                                           | a page — `Size`/`Orientation`/`MediaBox`/`Width`/`Height`/`Rotate`/`Annotations` |
|  [03]   | `PdfDocumentOptions`                                | output policy — `ColorMode`/`CompressContentStreams`/`NoCompression`/`FlateEncodeMode`/`Layout`/`UseFlateDecoderForJpegImages` |
|  [04]   | `PdfDocumentInformation`                            | `Title`/`Author`/`Subject`/`Keywords`/`Creator`/`CreationDate` metadata |
|  [05]   | `PdfDocumentSettings`                               | font-embedding + trim-margin settings                          |
|  [06]   | `PdfOutlineCollection` / `PdfOutline`              | bookmark/outline tree                                          |
|  [07]   | `PdfSharp.Pdf.PdfPageLayout` / `PdfSharp.Pdf.PdfPageMode` / `PdfSharp.PageOrientation` | viewer layout, open mode, page orientation enums |
|  [08]   | `PdfSharp.Capabilities.Build` / `Capabilities.Features` | runtime build + feature capability probe                  |

[GRAPHICS_TYPES]: `PdfSharp.Drawing` device-independent drawing surface
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `XGraphics`                                         | the drawing context (over a page / image / measure context) — the full `Draw*` algebra + transform stack |
|  [02]   | `XPen` / `XBrush` / `XSolidBrush` / `XLinearGradientBrush` / `XColor` / `XColors` | stroke + fill + color vocabulary           |
|  [03]   | `XFont` / `XFontStyleEx` / `XPdfFontOptions` / `XStringFormat` / `XStringFormats` | font + text-format vocabulary             |
|  [04]   | `XImage` / `XBitmapImage`                           | importable raster image                                        |
|  [05]   | `XPdfForm` / `XForm`                                | an imported PDF page (`PageNumber`/`PageCount`) drawn as a form — the read-modify-MERGE primitive |
|  [06]   | `XGraphicsPath` / `XGraphicsState` / `XGraphicsContainer` | vector path + save/restore + container state            |
|  [07]   | `XMatrix` / `XPoint` / `XRect` / `XSize` / `XVector` / `XUnit` | geometry + unit primitives                            |
|  [08]   | `XGraphicsPdfPageOptions` / `XGraphicsUnit` / `XPageDirection` / `XFillMode` / `XLineCap` / `XLineJoin` | drawing-context policy enums   |
|  [09]   | `PdfSharp.Drawing.Layout.XTextFormatter`           | rectangle-bounded multi-line text layout (`DrawString` into an `XRect` with wrapping + `Block` runs) |

[SECURITY_TYPES]: `PdfSharp.Pdf.Security` + `PdfSharp.Pdf.Signatures`
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PdfSecuritySettings` / `PdfStandardSecurityHandler` | encryption + permission policy on `PdfDocument.SecuritySettings` |
|  [02]   | `PdfDefaultEncryption` / `PdfUserAccessPermission`  | encryption-level + access-permission vocabulary (`PermitAll`, print/copy/modify flags) |
|  [03]   | `DigitalSignatureHandler` / `DigitalSignatureOptions` / `IDigitalSigner` | digital-signature handler, appearance/reason/location options, and the pluggable signer contract |
|  [04]   | `IAnnotationAppearanceHandler` / `DefaultSignatureAppearanceHandler` | custom vs default visible-signature appearance        |
|  [05]   | `PdfCryptFilter` / `IdentityCryptFilter`            | crypt-filter primitives                                        |

[CONTENT_TYPES]: `PdfSharp.Pdf.AcroForms` / `Annotations` / `Content` / `UniversalAccessibility`
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PdfAcroForm` / `PdfAcroField` (`PdfTextField`/`PdfCheckBoxField`/`PdfComboBoxField`/…) | interactive form + typed field tree |
|  [02]   | `PdfLinkAnnotation` / `PdfTextAnnotation` / `PdfWidgetAnnotation` / `PdfAnnotations` | link/markup/widget annotations |
|  [03]   | `PdfSharp.Pdf.Content` / `…Content.Objects`        | content-stream tokenizer + object model (inspect/extract drawing operators) |
|  [04]   | `PdfSharp.UniversalAccessibility.*`                | tagged-PDF / PDF-UA structure tree (accessible evidence reports) |
|  [05]   | `PdfSharp.Pdf.Structure` / `…Pdf.Actions`          | logical structure tree + document actions (goto/embedded-goto) |

[REPORT_TYPES]: `MigraDoc.DocumentObjectModel` flow-content DOM
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Document`                                          | the report root — `Sections`/`Styles`/`Info`/`DefaultPageSetup`/`LastSection`/`AddSection()` |
|  [02]   | `Section` / `PageSetup` / `HeaderFooter`            | page-flow section + page geometry + running header/footer      |
|  [03]   | `Paragraph` / `FormattedText` / `Text` / `Hyperlink` | flow text runs                                               |
|  [04]   | `Styles` / `Style` / `ParagraphFormat` / `Font`     | named cascading style sheet                                    |
|  [05]   | `MigraDoc.DocumentObjectModel.Tables.{Table,Row,Rows,Column,Columns,Cell,Cells}` + `…DocumentObjectModel.{Borders,Shading}` | flow table model + cell borders/shading |
|  [06]   | `MigraDoc.DocumentObjectModel.Shapes.{Image,TextFrame,Chart}` + `…Shapes.Charts.*` | image/frame/native-chart DOM            |
|  [07]   | `MigraDoc.DocumentObjectModel.Fields.{PageField,NumPagesField,DateField,InfoField}` | auto-computed fields                  |
|  [08]   | `MigraDoc.DocumentObjectModel.IO.{DdlReader,DdlWriter}` | DDL (MigraDoc markup) round-trip                          |

[RENDER_TYPES]: `MigraDoc.Rendering` flow-to-PDF renderer
- rail: pdf

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PdfDocumentRenderer`                               | renders a MigraDoc `Document` onto a `PdfSharp.Pdf.PdfDocument` — `Document`/`PdfDocument`/`PageCount`, `RenderDocument()`/`Save(...)` |
|  [02]   | `DocumentRenderer`                                  | the lower renderer (`PrepareDocument`/`RenderPage` onto an `XGraphics`) for custom page composition |
|  [03]   | `FormattedDocument`                                 | the laid-out, paginated document (page count + per-page geometry) |
|  [04]   | `MigraDoc.RtfRendering.RtfDocumentRenderer`        | the same `Document` rendered to RTF (alternative deliverable)  |

## [03]-[ENTRYPOINTS]

[DOCUMENT_LIFECYCLE]: `PdfDocument` + `PdfReader` create / open / import / save (sync + async mirrors)
- rail: pdf

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `new PdfDocument()` / `AddPage()` / `AddPage(PdfPage)` / `InsertPage(int, PdfPage)` | `PdfDocument` | author a new document, append/insert pages |
|  [02]   | `Save(string)` / `Save(Stream)` / `SaveAsync(string)` / `SaveAsync(Stream, bool closeStream)` / `CanSave(ref string)` | `PdfDocument` | persist, sync + async, with a pre-flight check |
|  [03]   | `PdfReader.Open(string, PdfDocumentOpenMode, PdfReaderOptions?)` / `Open(Stream, string? password, PdfDocumentOpenMode, …)` | `PdfReader` | open existing for `Import`/`Modify`/`InformationOnly` |
|  [04]   | `PdfReader.TestPdfFile(string/Stream/byte[]) : int`            | `PdfReader`    | validate a PDF without full parse     |
|  [05]   | `Info` / `Options` / `Settings` / `Outlines` / `SecuritySettings` / `PageCount` / `Version` | `PdfDocument` | metadata, output policy, bookmarks, encryption, page count |

[DRAWING]: `XGraphics` context creation + the draw algebra
- rail: pdf

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `FromPdfPage(PdfPage, XGraphicsUnit, XPageDirection)` (+ overloads with `XGraphicsPdfPageOptions`) | `XGraphics` | draw onto a page, unit + origin chosen |
|  [02]   | `FromImage(XImage, XGraphicsUnit, RenderEvents?)` / `CreateMeasureContext(XSize, XGraphicsUnit, XPageDirection, …)` | `XGraphics` | draw onto an image / measure-only context |
|  [03]   | `FromPdfForm(XPdfForm)` / `FromForm(XForm)`                     | `XGraphics`    | draw onto a reusable form/template     |
|  [04]   | `DrawString(string, XFont, XBrush, XPoint/XRect[, XStringFormat])` | `XGraphics`  | text — point-anchored or rect-bounded  |
|  [05]   | `DrawImage(XImage, …)` (incl. `XPdfForm` for PDF-page merge), `DrawLine`/`DrawLines`/`DrawRectangle`/`DrawEllipse`/`DrawBezier(s)`/`DrawCurve`/`DrawClosedCurve`/`DrawPolygon`/`DrawPath`/`DrawArc` | `XGraphics` | the full vector-draw algebra |
|  [06]   | `Save() : XGraphicsState` / `Restore(XGraphicsState)` / `BeginContainer(...)` / `EndContainer(...)` | `XGraphics` | transform/clip state stack |
|  [07]   | `RotateTransform(double)` / `TranslateTransform(dx,dy)` / `ScaleTransform(sx,sy)`  | `XGraphics` | accumulate the transform (never set the matrix directly — use Save/Restore) |
|  [08]   | `XTextFormatter(XGraphics){ Alignment }.DrawString(text, XFont, XBrush, XRect)` | `XTextFormatter` | wrapped multi-line text into a rectangle |

[SECURITY_SIGNING]: encryption + digital signature entrypoints
- rail: pdf

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]                 | [RAIL]                          |
| :-----: | :-------------------------------------------------------------- | :----------------------------- | :------------------------------ |
|  [01]   | `SecuritySettings.DocumentSecurityLevel` + `…SecurityHandler.UserPassword`/`OwnerPassword` | `PdfDocument.SecuritySettings` | set passwords         |
|  [02]   | `SetEncryptionToV5(bool encryptMetadata)` / `SetEncryptionToV4UsingAES(...)` / `SetEncryptionToNoneAndResetPasswords()` | `PdfStandardSecurityHandler` | choose AES-256 / AES-128 / clear |
|  [03]   | `PermitAccessPermissions` (`PdfUserAccessPermission` flags: print/copy/modify/annotate) | `PdfStandardSecurityHandler` | restrict consumer permissions |
|  [04]   | `DigitalSignatureHandler.ForDocument(PdfDocument, IDigitalSigner, DigitalSignatureOptions)` | `DigitalSignatureHandler` | attach a visible/invisible signature |
|  [05]   | `DigitalSignatureOptions { Reason; Location; ContactInfo; Rectangle; PageIndex; AppearanceHandler }` | `DigitalSignatureOptions` | signature appearance + placement |

[REPORT_RENDER]: MigraDoc flow `Document` -> `PdfDocument`
- rail: pdf

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]        | [RAIL]                          |
| :-----: | :-------------------------------------------------------------- | :-------------------- | :------------------------------ |
|  [01]   | `Document.AddSection() : Section` -> `Section.AddParagraph()/AddTable()/AddImage()` / `Document.Styles[...]` | `Document` | author flow content + styles |
|  [02]   | `new PdfDocumentRenderer{ Document = doc }.RenderDocument()`    | `PdfDocumentRenderer` | paginate + render onto a `PdfDocument` |
|  [03]   | `PdfDocumentRenderer.Save(string)` / `Save(Stream, bool)` / `.PdfDocument` | `PdfDocumentRenderer` | persist, or take the underlying `PdfDocument` to post-process |
|  [04]   | `DocumentRenderer.PrepareDocument()` + `RenderPage(XGraphics, int)` | `DocumentRenderer` | render a flow page onto a custom `XGraphics` (mix flow + precise layout) |

## [04]-[IMPLEMENTATION_LAW]

[PDF_LAW]:
- Package: `PDFsharp`
- Owns: the structured vector-PDF deliverable — `PdfDocument` (page tree + `Info`/`Options`/`Settings`/`Outlines`/security), `XGraphics` (the device-independent draw algebra), `PdfReader` (open/import/merge), and the `Security`/`Signatures`/`AcroForms`/`Annotations`/`Content`/`UniversalAccessibility` namespaces.
- Accept: `Render/drafting` draws precise sheet layout with `XGraphics` (`DrawString`/`DrawImage`/`DrawPath` + Save/Restore transform stack, `XTextFormatter` for wrapped text); read-modify-merge imports through `PdfReader.Open(..., PdfDocumentOpenMode.Import)` and draws imported pages as `XPdfForm` via `DrawImage`; `Render/evidence` adds digital signatures (`DigitalSignatureHandler.ForDocument`), AES-256 encryption (`SetEncryptionToV5`), and tagged-PDF accessibility (`UniversalAccessibility`); output policy (`ColorMode`/`CompressContentStreams`/`FlateEncodeMode`) lives on `PdfDocumentOptions`.
- Reject: hand-emitting PDF byte syntax or a parallel page model; a separate raster path where `XGraphics` draws vector directly; assuming Windows/GDI (PDFsharp 6.2.x is platform-neutral and runs on the headless render path); blocking on a custom signer where `IDigitalSigner` is the pluggable contract.

[REPORT_LAW]:
- Package: `PDFsharp-MigraDoc` (renders onto `PDFsharp`)
- Owns: the auto-paginated flow-report DOM — `Document`/`Section`/`Paragraph`/`Table`/`Image`/`TextFrame`/`Chart` with a named `Styles` cascade and auto-computed `Fields`, rendered to PDF by `PdfDocumentRenderer` (or to RTF by `RtfDocumentRenderer`).
- Accept: `Render/evidence` flow reports (multi-page, headers/footers, tables, running page fields) are authored as a MigraDoc `Document` and rendered with `PdfDocumentRenderer.RenderDocument()` + `Save`; the underlying `.PdfDocument` is post-processed for signing/encryption through the PDFsharp surface; `DocumentRenderer.RenderPage` mixes flow content onto a custom `XGraphics` where a page needs both flow and precise layout.
- Reject: hand-paginating a multi-page report with raw `XGraphics`/`DrawString` cursor math where the flow DOM owns pagination; a bespoke report model duplicating `Document`/`Section`/`Table`; emitting reports through the OOXML/DXF set (`api-drafting-export.md`) when the deliverable is PDF — those codecs own DWG/DXF/OOXML, PDFsharp+MigraDoc own vector PDF, and `NodeEditorAvalonia.ExportRenderer.RenderPdf` (`api-nodeeditor.md`) targets the same PDF deliverable for canvas snapshots.
