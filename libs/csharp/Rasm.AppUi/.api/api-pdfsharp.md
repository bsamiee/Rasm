# [RASM_APPUI_API_PDFSHARP]

`PDFsharp` (assembly `PdfSharp.dll`, namespace `PdfSharp.*`) is the structured vector-PDF page object model: `PdfDocument` owns the page tree, info, options, and security; `XGraphics` is the device-independent drawing surface (the same `Draw*`/`XPen`/`XBrush`/`XFont`/`XMatrix` algebra over a page, an image, or a measure context); `PdfReader` opens/imports existing PDFs for read-modify-merge; and the `Signatures`/`Security`/`AcroForms`/`Annotations`/`UniversalAccessibility` namespaces add digital signing, encryption, form fields, link/markup annotations, and tagged-PDF accessibility. `PDFsharp-MigraDoc` (assemblies `MigraDoc.DocumentObjectModel.dll` / `MigraDoc.Rendering.dll` / `MigraDoc.RtfRendering.dll`, namespace `MigraDoc.*`) is the auto-paginated flow-report DOM rendered ONTO a `PdfDocument` through `PdfDocumentRenderer`. Together they are the paginated-document + vector-PDF owner of `Document/export.md` (`[V8]`a) — the MigraDoc flow DOM REPLACES the hand-rolled `FlowBlock`/`FlowFold` pagination engine, and `export.md` is the single owner drafting's sheet-PDF and the Diagnostics report-PDF both compose: a caller authors a MigraDoc `Document` (flow content, auto-pagination) or draws directly with `XGraphics` (precise sheet layout), and the `NodeEditorAvalonia.ExportRenderer.RenderPdf` canvas export (`api-nodeeditor.md`) targets the same PDF deliverable. PDFsharp is platform-neutral (no Windows/GDI coupling), so it runs on the headless render path beside `Avalonia.Headless`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PDFsharp`
- package: `PDFsharp`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/PdfSharp.dll`, casing is `PdfSharp`); multi-targets, `net10.0` bound
- assembly: `PdfSharp` (primary) + satellites `PdfSharp.Charting`, `PdfSharp.Cryptography`, `PdfSharp.Quality`, `PdfSharp.Snippets`, `PdfSharp.BarCodes`, `PdfSharp.WPFonts`, `PdfSharp.System`, `PdfSharp.Shared`
- namespace: `PdfSharp.Drawing` (`XGraphics`, `XPen`/`XBrush`/`XFont`/`XColor`/`XImage`/`XPdfForm`/`XMatrix`/`XPoint`/`XRect`, 85 types), `PdfSharp.Drawing.Layout` (`XTextFormatter`), `PdfSharp.Pdf` (`PdfDocument`/`PdfPage`/`PdfDocumentOptions`/`PdfDocumentInformation`/`PdfOutlineCollection`, 62 types), `PdfSharp.Pdf.IO` (`PdfReader`/`PdfWriter`), `PdfSharp.Pdf.Security` (encryption), `PdfSharp.Pdf.Signatures` (digital signing), `PdfSharp.Pdf.AcroForms`, `PdfSharp.Pdf.Annotations`, `PdfSharp.Pdf.Content` (content-stream parser), `PdfSharp.UniversalAccessibility` (tagged PDF), `PdfSharp.Fonts`/`PdfSharp.Fonts.OpenType`, `PdfSharp.Logging`, `PdfSharp.Capabilities`
- depends: `Microsoft.Extensions.Logging.Abstractions`, `System.Security.Cryptography.Pkcs` (both ride existing/in-box rows)
- rail: pdf

[PACKAGE_SURFACE]: `PDFsharp-MigraDoc`
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

`PdfDocument` owns the page tree, save/import, and `Info`/`Options`/`Settings`/`Outlines`/`SecuritySettings`. `PdfPage` exposes `Size`/`Orientation`/`MediaBox`/`Width`/`Height`/`Rotate`/`Annotations`. `PdfDocumentOptions` carries `ColorMode`/`CompressContentStreams`/`NoCompression`/`FlateEncodeMode`/`Layout`/`UseFlateDecoderForJpegImages`, while `PdfDocumentInformation` carries `Title`/`Author`/`Subject`/`Keywords`/`Creator`/`CreationDate`.

| [INDEX] | [SYMBOL]                      | [KIND]             |
| :-----: | :---------------------------- | :----------------- |
|  [01]   | `PdfDocument`                 | document root      |
|  [02]   | `PdfPage`                     | page               |
|  [03]   | `PdfDocumentOptions`          | output policy      |
|  [04]   | `PdfDocumentInformation`      | metadata           |
|  [05]   | `PdfDocumentSettings`         | embedding settings |
|  [06]   | `PdfOutlineCollection`        | outline collection |
|  [07]   | `PdfOutline`                  | outline node       |
|  [08]   | `PdfSharp.Pdf.PdfPageLayout`  | viewer layout      |
|  [09]   | `PdfSharp.Pdf.PdfPageMode`    | open mode          |
|  [10]   | `PdfSharp.PageOrientation`    | page orientation   |
|  [11]   | `PdfSharp.Capabilities.Build` | build probe        |
|  [12]   | `Capabilities.Features`       | feature probe      |

`PdfDocumentSettings` carries font-embedding and trim-margin settings.

[GRAPHICS_TYPES]: `PdfSharp.Drawing` device-independent drawing surface
- rail: pdf

`XGraphics` is the drawing context over a page, image, or measure context and owns the `Draw*` algebra plus transform stack. `XPdfForm` exposes `PageNumber`/`PageCount` and draws an imported PDF page as the read-modify-merge primitive. `XTextFormatter.DrawString` lays wrapped `Block` runs into an `XRect`.

| [INDEX] | [SYMBOL]                                 | [KIND]            |
| :-----: | :--------------------------------------- | :---------------- |
|  [01]   | `XGraphics`                              | drawing context   |
|  [02]   | `XPen`                                   | stroke            |
|  [03]   | `XBrush`                                 | fill              |
|  [04]   | `XSolidBrush`                            | solid fill        |
|  [05]   | `XLinearGradientBrush`                   | gradient fill     |
|  [06]   | `XColor`                                 | color value       |
|  [07]   | `XColors`                                | color vocabulary  |
|  [08]   | `XFont`                                  | font              |
|  [09]   | `XFontStyleEx`                           | font style        |
|  [10]   | `XPdfFontOptions`                        | font policy       |
|  [11]   | `XStringFormat`                          | text format       |
|  [12]   | `XStringFormats`                         | format vocabulary |
|  [13]   | `XImage`                                 | raster image      |
|  [14]   | `XBitmapImage`                           | bitmap image      |
|  [15]   | `XPdfForm`                               | imported PDF form |
|  [16]   | `XForm`                                  | reusable form     |
|  [17]   | `XGraphicsPath`                          | vector path       |
|  [18]   | `XGraphicsState`                         | saved state       |
|  [19]   | `XGraphicsContainer`                     | container state   |
|  [20]   | `XMatrix`                                | transform         |
|  [21]   | `XPoint`                                 | point             |
|  [22]   | `XRect`                                  | rectangle         |
|  [23]   | `XSize`                                  | size              |
|  [24]   | `XVector`                                | vector            |
|  [25]   | `XUnit`                                  | unit              |
|  [26]   | `XGraphicsPdfPageOptions`                | page policy       |
|  [27]   | `XGraphicsUnit`                          | context unit      |
|  [28]   | `XPageDirection`                         | page origin       |
|  [29]   | `XFillMode`                              | fill policy       |
|  [30]   | `XLineCap`                               | cap policy        |
|  [31]   | `XLineJoin`                              | join policy       |
|  [32]   | `PdfSharp.Drawing.Layout.XTextFormatter` | text layout       |

[SECURITY_TYPES]: `PdfSharp.Pdf.Security` + `PdfSharp.Pdf.Signatures`
- rail: pdf

`PdfSecuritySettings` exposes the encryption and permission policy through `PdfDocument.SecuritySettings`. `PdfUserAccessPermission` carries `PermitAll` and print/copy/modify flags. `DigitalSignatureOptions` carries appearance, reason, and location, while `IDigitalSigner` is the pluggable signer contract.

| [INDEX] | [SYMBOL]                            | [KIND]              |
| :-----: | :---------------------------------- | :------------------ |
|  [01]   | `PdfSecuritySettings`               | security policy     |
|  [02]   | `PdfStandardSecurityHandler`        | security handler    |
|  [03]   | `PdfDefaultEncryption`              | encryption level    |
|  [04]   | `PdfUserAccessPermission`           | permission flags    |
|  [05]   | `DigitalSignatureHandler`           | signature handler   |
|  [06]   | `DigitalSignatureOptions`           | signature options   |
|  [07]   | `IDigitalSigner`                    | signer contract     |
|  [08]   | `IAnnotationAppearanceHandler`      | appearance contract |
|  [09]   | `DefaultSignatureAppearanceHandler` | default appearance  |
|  [10]   | `PdfCryptFilter`                    | crypt filter        |
|  [11]   | `IdentityCryptFilter`               | identity filter     |

[CONTENT_TYPES]: `PdfSharp.Pdf.AcroForms` / `Annotations` / `Content` / `UniversalAccessibility`
- rail: pdf

`PdfAcroField` admits typed fields including `PdfTextField`/`PdfCheckBoxField`/`PdfComboBoxField` and further variants. `PdfSharp.Pdf.Content` tokenizes content streams, while `PdfSharp.Pdf.Content.Objects` models drawing operators for inspection and extraction. `PdfSharp.UniversalAccessibility.*` carries the tagged-PDF/PDF-UA tree for accessible evidence reports. `PdfSharp.Pdf.Actions` includes goto and embedded-goto actions.

| [INDEX] | [SYMBOL]                            | [KIND]            |
| :-----: | :---------------------------------- | :---------------- |
|  [01]   | `PdfAcroForm`                       | form root         |
|  [02]   | `PdfAcroField`                      | typed field tree  |
|  [03]   | `PdfLinkAnnotation`                 | link annotation   |
|  [04]   | `PdfTextAnnotation`                 | markup annotation |
|  [05]   | `PdfWidgetAnnotation`               | widget annotation |
|  [06]   | `PdfAnnotations`                    | annotation set    |
|  [07]   | `PdfSharp.Pdf.Content`              | tokenizer         |
|  [08]   | `PdfSharp.Pdf.Content.Objects`      | content objects   |
|  [09]   | `PdfSharp.UniversalAccessibility.*` | PDF-UA tree       |
|  [10]   | `PdfSharp.Pdf.Structure`            | logical tree      |
|  [11]   | `PdfSharp.Pdf.Actions`              | document actions  |

[REPORT_TYPES]: `MigraDoc.DocumentObjectModel` flow-content DOM
- rail: pdf

`Document` exposes `Sections`/`Styles`/`Info`/`DefaultPageSetup`/`LastSection`/`AddSection()`. The table symbols resolve under `MigraDoc.DocumentObjectModel`; `Tables.*`, `Shapes.*`, `Shapes.Charts.*`, `Fields.*`, and `IO.*` retain their nested namespaces.

| [INDEX] | [SYMBOL]               | [KIND]            |
| :-----: | :--------------------- | :---------------- |
|  [01]   | `Document`             | report root       |
|  [02]   | `Section`              | flow section      |
|  [03]   | `PageSetup`            | page geometry     |
|  [04]   | `HeaderFooter`         | running region    |
|  [05]   | `Paragraph`            | paragraph         |
|  [06]   | `FormattedText`        | formatted run     |
|  [07]   | `Text`                 | text run          |
|  [08]   | `Hyperlink`            | hyperlink run     |
|  [09]   | `Styles`               | style collection  |
|  [10]   | `Style`                | named style       |
|  [11]   | `ParagraphFormat`      | paragraph style   |
|  [12]   | `Font`                 | font style        |
|  [13]   | `Tables.Table`         | flow table        |
|  [14]   | `Tables.Row`           | table row         |
|  [15]   | `Tables.Rows`          | row collection    |
|  [16]   | `Tables.Column`        | table column      |
|  [17]   | `Tables.Columns`       | column collection |
|  [18]   | `Tables.Cell`          | table cell        |
|  [19]   | `Tables.Cells`         | cell collection   |
|  [20]   | `Borders`              | cell borders      |
|  [21]   | `Shading`              | cell shading      |
|  [22]   | `Shapes.Image`         | image             |
|  [23]   | `Shapes.TextFrame`     | text frame        |
|  [24]   | `Shapes.Chart`         | native chart      |
|  [25]   | `Shapes.Charts.*`      | chart vocabulary  |
|  [26]   | `Fields.PageField`     | page field        |
|  [27]   | `Fields.NumPagesField` | page-count field  |
|  [28]   | `Fields.DateField`     | date field        |
|  [29]   | `Fields.InfoField`     | info field        |
|  [30]   | `IO.DdlReader`         | DDL reader        |
|  [31]   | `IO.DdlWriter`         | DDL writer        |

[RENDER_TYPES]: `MigraDoc.Rendering` flow-to-PDF renderer
- rail: pdf

`PdfDocumentRenderer` renders a MigraDoc `Document` onto `PdfSharp.Pdf.PdfDocument` through `Document`/`PdfDocument`/`PageCount` and `RenderDocument()`/`Save(...)`. `DocumentRenderer` uses `PrepareDocument`/`RenderPage` over `XGraphics` for custom page composition. `FormattedDocument` carries page count and per-page geometry.

| [INDEX] | [SYMBOL]                                    | [KIND]             |
| :-----: | :------------------------------------------ | :----------------- |
|  [01]   | `PdfDocumentRenderer`                       | PDF renderer       |
|  [02]   | `DocumentRenderer`                          | page renderer      |
|  [03]   | `FormattedDocument`                         | paginated document |
|  [04]   | `MigraDoc.RtfRendering.RtfDocumentRenderer` | RTF renderer       |

## [03]-[ENTRYPOINTS]

[DOCUMENT_LIFECYCLE]: `PdfDocument` + `PdfReader` create / open / import / save (sync + async mirrors)
- rail: pdf

| [INDEX] | [OPERATION] | [SURFACE_ROOT] |
| :-----: | :---------- | :------------- |
|  [01]   | author      | `PdfDocument`  |
|  [02]   | persist     | `PdfDocument`  |
|  [03]   | open        | `PdfReader`    |
|  [04]   | validate    | `PdfReader`    |
|  [05]   | inspect     | `PdfDocument`  |

[AUTHOR]:
- Surface: `new PdfDocument()` / `AddPage()` / `AddPage(PdfPage)` / `InsertPage(int, PdfPage)`
- Effect: Author a document and append or insert pages.

[PERSIST]:
- Surface: `Save(string)` / `Save(Stream)` / `SaveAsync(string)` / `SaveAsync(Stream, bool closeStream)` / `CanSave(ref string)`
- Effect: Persist through sync or async entrypoints after the preflight check.

[OPEN]:
- Surface: `PdfReader.Open(string, PdfDocumentOpenMode, PdfReaderOptions?)` / `Open(Stream, string? password, PdfDocumentOpenMode, …)`
- Effect: Open existing content for `Import`/`Modify`/`InformationOnly`.

[VALIDATE]:
- Surface: `PdfReader.TestPdfFile(string/Stream/byte[]) : int`
- Effect: Validate a PDF without a full parse.

[INSPECT]:
- Surface: `Info` / `Options` / `Settings` / `Outlines` / `SecuritySettings` / `PageCount` / `Version`
- Effect: Read metadata, output policy, bookmarks, encryption, and page count.

[DRAWING]: `XGraphics` context creation + the draw algebra
- rail: pdf

| [INDEX] | [OPERATION] | [SURFACE_ROOT]   |
| :-----: | :---------- | :--------------- |
|  [01]   | page        | `XGraphics`      |
|  [02]   | image       | `XGraphics`      |
|  [03]   | measure     | `XGraphics`      |
|  [04]   | form        | `XGraphics`      |
|  [05]   | text        | `XGraphics`      |
|  [06]   | vector      | `XGraphics`      |
|  [07]   | state       | `XGraphics`      |
|  [08]   | transform   | `XGraphics`      |
|  [09]   | wrap        | `XTextFormatter` |

[PAGE]:
- Surface: `FromPdfPage(PdfPage, XGraphicsUnit, XPageDirection)` with `XGraphicsPdfPageOptions` overloads
- Effect: Draw onto a page under the selected unit and origin.

[IMAGE]:
- Surface: `FromImage(XImage, XGraphicsUnit, RenderEvents?)`
- Effect: Draw onto an image.

[MEASURE]:
- Surface: `CreateMeasureContext(XSize, XGraphicsUnit, XPageDirection, …)`
- Effect: Create a measure-only context.

[FORM]:
- Surface: `FromPdfForm(XPdfForm)` / `FromForm(XForm)`
- Effect: Draw onto a reusable form or template.

[TEXT]:
- Surface: `DrawString(string, XFont, XBrush, XPoint/XRect[, XStringFormat])`
- Effect: Draw point-anchored or rectangle-bounded text.

[VECTOR]:
- Surface: `DrawImage(XImage, …)` including `XPdfForm` for PDF-page merge
- Surface: `DrawLine` / `DrawLines` / `DrawRectangle` / `DrawEllipse` / `DrawBezier(s)` / `DrawCurve` / `DrawClosedCurve`
- Surface: `DrawPolygon` / `DrawPath` / `DrawArc`
- Effect: Apply the full vector-draw algebra.

[STATE]:
- Surface: `Save() : XGraphicsState` / `Restore(XGraphicsState)` / `BeginContainer(...)` / `EndContainer(...)`
- Effect: Own the transform and clip state stack.

[TRANSFORM]:
- Surface: `RotateTransform(double)` / `TranslateTransform(dx,dy)` / `ScaleTransform(sx,sy)`
- Effect: Accumulate transforms through `Save`/`Restore` without directly setting the matrix.

[WRAP]:
- Surface: `XTextFormatter(XGraphics){ Alignment }.DrawString(text, XFont, XBrush, XRect)`
- Effect: Draw wrapped multi-line text into a rectangle.

[SECURITY_SIGNING]: encryption + digital signature entrypoints
- rail: pdf

| [INDEX] | [OPERATION] | [SURFACE_ROOT]                 |
| :-----: | :---------- | :----------------------------- |
|  [01]   | passwords   | `PdfDocument.SecuritySettings` |
|  [02]   | encryption  | `PdfStandardSecurityHandler`   |
|  [03]   | permissions | `PdfStandardSecurityHandler`   |
|  [04]   | signature   | `DigitalSignatureHandler`      |
|  [05]   | appearance  | `DigitalSignatureOptions`      |

[PASSWORDS]:
- Surface: `SecuritySettings.DocumentSecurityLevel` / `SecurityHandler.UserPassword` / `SecurityHandler.OwnerPassword`
- Effect: Set document passwords.

[ENCRYPTION]:
- Surface: `SetEncryptionToV5(bool encryptMetadata)` / `SetEncryptionToV4UsingAES(...)` / `SetEncryptionToNoneAndResetPasswords()`
- Effect: Select AES-256, AES-128, or cleared encryption.

[PERMISSIONS]:
- Surface: `PermitAccessPermissions`
- Policy: `PdfUserAccessPermission` flags govern print, copy, modify, and annotate access.

[SIGNATURE]:
- Surface: `DigitalSignatureHandler.ForDocument(PdfDocument, IDigitalSigner, DigitalSignatureOptions)`
- Effect: Attach a visible or invisible signature.

[APPEARANCE]:
- Surface: `DigitalSignatureOptions { Reason; Location; ContactInfo; Rectangle; PageIndex; AppearanceHandler }`
- Effect: Set signature appearance and placement.

[REPORT_RENDER]: MigraDoc flow `Document` -> `PdfDocument`
- rail: pdf

| [INDEX] | [OPERATION] | [SURFACE_ROOT]        |
| :-----: | :---------- | :-------------------- |
|  [01]   | author      | `Document`            |
|  [02]   | render      | `PdfDocumentRenderer` |
|  [03]   | persist     | `PdfDocumentRenderer` |
|  [04]   | compose     | `DocumentRenderer`    |

[AUTHOR]:
- Surface: `Document.AddSection() : Section` / `Section.AddParagraph()` / `AddTable()` / `AddImage()` / `Document.Styles[...]`
- Effect: Author flow content and styles.

[RENDER]:
- Surface: `new PdfDocumentRenderer{ Document = doc }.RenderDocument()`
- Effect: Paginate and render onto a `PdfDocument`.

[PERSIST]:
- Surface: `PdfDocumentRenderer.Save(string)` / `Save(Stream, bool)` / `.PdfDocument`
- Effect: Persist output or expose the underlying `PdfDocument` for post-processing.

[COMPOSE]:
- Surface: `DocumentRenderer.PrepareDocument()` / `RenderPage(XGraphics, int)`
- Effect: Mix flow content and precise layout on a custom `XGraphics` page.

## [04]-[IMPLEMENTATION_LAW]

[PDF_LAW]:
- Package: `PDFsharp`
- Owns: the structured vector-PDF deliverable — `PdfDocument` (page tree + `Info`/`Options`/`Settings`/`Outlines`/security), `XGraphics` (the device-independent draw algebra), `PdfReader` (open/import/merge), and the `Security`/`Signatures`/`AcroForms`/`Annotations`/`Content`/`UniversalAccessibility` namespaces.
- Accept: `Document/export.md` draws precise sheet layout with `XGraphics` (`DrawString`/`DrawImage`/`DrawPath` + Save/Restore transform stack, `XTextFormatter` for wrapped text); read-modify-merge imports through `PdfReader.Open(..., PdfDocumentOpenMode.Import)` and draws imported pages as `XPdfForm` via `DrawImage`; the export policy rows add digital signatures (`DigitalSignatureHandler.ForDocument(PdfDocument, IDigitalSigner, DigitalSignatureOptions)`), AES-256 encryption (`SetEncryptionToV5`), AcroForm fields, and tagged-PDF accessibility (`UniversalAccessibility`); output policy (`ColorMode`/`CompressContentStreams`/`FlateEncodeMode`) lives on `PdfDocumentOptions`.
- Signing credential ingress is honest: `IDigitalSigner` is COMPOSED at the boundary from key material the AppHost `Runtime/secrets.md` lease owns (acquire/renew/zeroize — the `[V8]`a secrets-lease consumer clause, AppUi named), never AppUi-held key bytes; the crossing is a declared `[V9]` ledger row.
- Reject: hand-emitting PDF byte syntax or a parallel page model; a separate raster path where `XGraphics` draws vector directly; assuming Windows/GDI (PDFsharp 6.2.x is platform-neutral and runs on the headless render path); blocking on a custom signer where `IDigitalSigner` is the pluggable contract; AppUi-held signing key bytes where the AppHost secrets lease owns the credential lifecycle.

[REPORT_LAW]:
- Package: `PDFsharp-MigraDoc` (renders onto `PDFsharp`)
- Owns: the auto-paginated flow-report DOM — `Document`/`Section`/`Paragraph`/`Table`/`Image`/`TextFrame`/`Chart` with a named `Styles` cascade and auto-computed `Fields`, rendered to PDF by `PdfDocumentRenderer` (or to RTF by `RtfDocumentRenderer`).
- Accept: `Document/export.md` flow reports (multi-page, headers/footers, tables, running page fields — the Diagnostics report-PDF and drafting sheet-PDF included) are authored as a MigraDoc `Document` and rendered with `PdfDocumentRenderer.RenderDocument()` + `Save`; the underlying `.PdfDocument` is post-processed for signing/encryption through the PDFsharp surface; `DocumentRenderer.RenderPage` mixes flow content onto a custom `XGraphics` where a page needs both flow and precise layout.
- Reject: hand-paginating a multi-page report with raw `XGraphics`/`DrawString` cursor math where the flow DOM owns pagination; a bespoke report model duplicating `Document`/`Section`/`Table`; emitting reports through the OOXML/DXF set (`api-drafting-export.md`) when the deliverable is PDF — those codecs own DWG/DXF/OOXML, PDFsharp+MigraDoc own vector PDF, and `NodeEditorAvalonia.ExportRenderer.RenderPdf` (`api-nodeeditor.md`) targets the same PDF deliverable for canvas snapshots.
