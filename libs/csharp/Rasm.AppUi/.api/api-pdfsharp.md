# [RASM_APPUI_API_PDFSHARP]

`PDFsharp` owns the structured vector-PDF page model: `PdfDocument` holds the page tree, `XGraphics` draws the device-independent mark algebra over a page, image, or form, and `PdfReader` opens existing PDFs for read-modify-merge, with sibling namespaces adding encryption, digital signing, AcroForm fields, annotations, and tagged-PDF accessibility. `PDFsharp-MigraDoc` renders an auto-paginated flow-report DOM (`Document`/`Section`/`Paragraph`/`Table`/`Chart`) onto a `PdfDocument` through `PdfDocumentRenderer`. Together they own the branch vector-PDF rail: precise sheets draw through `XGraphics`, flow reports author a MigraDoc `Document`, and platform-neutral rendering stays on the headless path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PDFsharp`
- package: `PDFsharp` (MIT)
- assembly: `PdfSharp` (+ `PdfSharp.Cryptography` carrying the ready CMS signer)
- namespace: `PdfSharp.Drawing`, `PdfSharp.Drawing.Layout`, `PdfSharp.Pdf`, `PdfSharp.Pdf.IO`, `PdfSharp.Pdf.Security`, `PdfSharp.Pdf.Signatures`, `PdfSharp.Pdf.AcroForms`, `PdfSharp.Pdf.Annotations`, `PdfSharp.Pdf.Content`, `PdfSharp.Pdf.Structure`, `PdfSharp.Pdf.Actions`, `PdfSharp.UniversalAccessibility`, `PdfSharp.Capabilities`
- target: `net10.0`
- depends: `Microsoft.Extensions.Logging.Abstractions`, `System.Security.Cryptography.Pkcs`
- rail: pdf

[PACKAGE_SURFACE]: `PDFsharp-MigraDoc`
- package: `PDFsharp-MigraDoc` (MIT)
- assembly: `MigraDoc.DocumentObjectModel`, `MigraDoc.Rendering`, `MigraDoc.RtfRendering`
- namespace: `MigraDoc.DocumentObjectModel` (with `.Tables`, `.Shapes`, `.Shapes.Charts`, `.Fields`, `.IO`), `MigraDoc.Rendering`, `MigraDoc.RtfRendering`
- target: `net10.0`
- depends: `PDFsharp` (renders onto its `PdfDocument`/`XGraphics`)
- rail: pdf

## [02]-[PUBLIC_TYPES]

[DOCUMENT_TYPES]: `PdfSharp.Pdf` page-tree model.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `PdfDocument`            | sealed class  | page tree, save, import, security root           |
|  [02]   | `PdfPage`                | sealed class  | page size, orientation, media box, rotation      |
|  [03]   | `PdfDocumentOptions`     | sealed class  | color mode, compression, flate-encode policy     |
|  [04]   | `PdfDocumentInformation` | sealed class  | title, author, subject, keywords, dates metadata |
|  [05]   | `PdfDocumentSettings`    | sealed class  | font-embedding and trim-margin settings          |
|  [06]   | `PdfOutlineCollection`   | class         | outline collection                               |
|  [07]   | `PdfOutline`             | sealed class  | outline node                                     |
|  [08]   | `PdfPageLayout`          | enum          | viewer page layout                               |
|  [09]   | `PdfPageMode`            | enum          | document open mode                               |
|  [10]   | `PageOrientation`        | enum          | page orientation                                 |
|  [11]   | `Capabilities.Build`     | static class  | build probe                                      |
|  [12]   | `Capabilities.Features`  | static class  | feature probe                                    |

[GRAPHICS_TYPES]: `PdfSharp.Drawing` device-independent drawing surface.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                      |
| :-----: | :------------------------ | :------------- | :-------------------------------- |
|  [01]   | `XGraphics`               | sealed class   | draw context over page/image/form |
|  [02]   | `XPen`                    | sealed class   | stroke                            |
|  [03]   | `XBrush`                  | abstract class | fill base                         |
|  [04]   | `XSolidBrush`             | sealed class   | solid fill                        |
|  [05]   | `XLinearGradientBrush`    | sealed class   | gradient fill                     |
|  [06]   | `XColor`                  | struct         | color value                       |
|  [07]   | `XColors`                 | static class   | named-color vocabulary            |
|  [08]   | `XFont`                   | sealed class   | font                              |
|  [09]   | `XFontStyleEx`            | enum           | font style                        |
|  [10]   | `XPdfFontOptions`         | class          | font-embedding policy             |
|  [11]   | `XStringFormat`           | class          | text alignment format             |
|  [12]   | `XStringFormats`          | static class   | format vocabulary                 |
|  [13]   | `XImage`                  | class          | raster image                      |
|  [14]   | `XBitmapImage`            | sealed class   | in-memory bitmap                  |
|  [15]   | `XPdfForm`                | class          | imported PDF page as draw source  |
|  [16]   | `XForm`                   | class          | reusable draw template            |
|  [17]   | `XGraphicsPath`           | sealed class   | vector path                       |
|  [18]   | `XGraphicsState`          | sealed class   | saved transform/clip state        |
|  [19]   | `XGraphicsContainer`      | sealed class   | nested container state            |
|  [20]   | `XMatrix`                 | struct         | affine transform                  |
|  [21]   | `XPoint`                  | struct         | point                             |
|  [22]   | `XRect`                   | struct         | rectangle                         |
|  [23]   | `XSize`                   | struct         | size                              |
|  [24]   | `XVector`                 | struct         | vector                            |
|  [25]   | `XUnit`                   | struct         | typed length unit                 |
|  [26]   | `XGraphicsPdfPageOptions` | enum           | page-append policy                |
|  [27]   | `XGraphicsUnit`           | enum           | context unit                      |
|  [28]   | `XPageDirection`          | enum           | page origin direction             |
|  [29]   | `XFillMode`               | enum           | fill rule                         |
|  [30]   | `XLineCap`                | enum           | stroke cap                        |
|  [31]   | `XLineJoin`               | enum           | stroke join                       |
|  [32]   | `XTextFormatter`          | class          | wrapped multi-line text layout    |

[SECURITY_TYPES]: `PdfSharp.Pdf.Security` + `PdfSharp.Pdf.Signatures` encryption and signing.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `PdfSecuritySettings`          | sealed class  | encryption and `Permit*` permission policy |
|  [02]   | `PdfStandardSecurityHandler`   | sealed class  | passwords and encryption-level selection   |
|  [03]   | `PdfDefaultEncryption`         | enum          | encryption level                           |
|  [04]   | `DigitalSignatureHandler`      | class         | signature attachment                       |
|  [05]   | `DigitalSignatureOptions`      | class         | signature appearance and placement         |
|  [06]   | `IDigitalSigner`               | interface     | pluggable signer contract                  |
|  [07]   | `PdfSharpDefaultSigner`        | class         | ready CMS signer                           |
|  [08]   | `IAnnotationAppearanceHandler` | interface     | signature appearance contract              |
|  [09]   | `PdfCryptFilter`               | class         | crypt filter                               |

[CONTENT_TYPES]: `PdfSharp.Pdf.AcroForms` / `Annotations` / `Content` / `Structure` / `Actions` / `UniversalAccessibility`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [CAPABILITY]                  |
| :-----: | :---------------------- | :------------- | :---------------------------- |
|  [01]   | `PdfAcroForm`           | sealed class   | AcroForm root                 |
|  [02]   | `PdfAcroField`          | abstract class | typed form-field tree         |
|  [03]   | `PdfLinkAnnotation`     | sealed class   | link annotation               |
|  [04]   | `PdfTextAnnotation`     | sealed class   | markup annotation             |
|  [05]   | `PdfAnnotations`        | sealed class   | annotation set                |
|  [06]   | `ContentReader`         | static class   | content-stream reader         |
|  [07]   | `CObject`               | abstract class | parsed content-operator model |
|  [08]   | `UAManager`             | class          | tagged-PDF / PDF-UA manager   |
|  [09]   | `PdfStructureTreeRoot`  | sealed class   | logical structure tree        |
|  [10]   | `PdfGoToAction`         | sealed class   | intra-document goto action    |
|  [11]   | `PdfEmbeddedGoToAction` | sealed class   | embedded-file goto action     |

[REPORT_TYPES]: `MigraDoc.DocumentObjectModel` flow-content DOM.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :--------------------- | :------------ | :------------------ |
|  [01]   | `Document`             | sealed class  | report root         |
|  [02]   | `Section`              | class         | flow section        |
|  [03]   | `PageSetup`            | class         | page geometry       |
|  [04]   | `HeaderFooter`         | class         | running region      |
|  [05]   | `Paragraph`            | class         | paragraph           |
|  [06]   | `FormattedText`        | class         | formatted run       |
|  [07]   | `Text`                 | class         | text run            |
|  [08]   | `Hyperlink`            | class         | hyperlink run       |
|  [09]   | `Styles`               | class         | style collection    |
|  [10]   | `Style`                | sealed class  | named style         |
|  [11]   | `ParagraphFormat`      | class         | paragraph style     |
|  [12]   | `Font`                 | sealed class  | font style          |
|  [13]   | `Tables.Table`         | class         | flow table          |
|  [14]   | `Tables.Row`           | class         | table row           |
|  [15]   | `Tables.Column`        | class         | table column        |
|  [16]   | `Tables.Cell`          | class         | table cell          |
|  [17]   | `Borders`              | class         | cell borders        |
|  [18]   | `Shading`              | sealed class  | cell shading        |
|  [19]   | `Shapes.Image`         | class         | image               |
|  [20]   | `Shapes.TextFrame`     | class         | text frame          |
|  [21]   | `Shapes.Chart`         | class         | native chart        |
|  [22]   | `Fields.PageField`     | class         | page-number field   |
|  [23]   | `Fields.NumPagesField` | class         | page-count field    |
|  [24]   | `Fields.DateField`     | class         | date field          |
|  [25]   | `Fields.InfoField`     | class         | document-info field |
|  [26]   | `IO.DdlReader`         | class         | DDL reader          |
|  [27]   | `IO.DdlWriter`         | sealed class  | DDL writer          |

[RENDER_TYPES]: `MigraDoc.Rendering` flow-to-PDF renderer.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :-------------------- | :------------ | :------------------------------------- |
|  [01]   | `PdfDocumentRenderer` | class         | paginate and render onto `PdfDocument` |
|  [02]   | `DocumentRenderer`    | class         | per-page render over `XGraphics`       |
|  [03]   | `FormattedDocument`   | class         | paginated document with page geometry  |
|  [04]   | `RtfDocumentRenderer` | class         | RTF emit                               |

## [03]-[ENTRYPOINTS]

[DOCUMENT_LIFECYCLE]: `PdfDocument` and `PdfReader` create, persist, open, and inspect.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new PdfDocument()`                                              | ctor     | author a document                              |
|  [02]   | `AddPage()` / `AddPage(PdfPage)` / `InsertPage(int, PdfPage)`    | instance | append or insert pages                         |
|  [03]   | `Save(string)` / `Save(Stream, bool)`                            | instance | persist                                        |
|  [04]   | `SaveAsync(string)` / `SaveAsync(Stream, bool)`                  | instance | async persist                                  |
|  [05]   | `CanSave(ref string)`                                            | instance | preflight check                                |
|  [06]   | `PdfReader.Open(string, PdfDocumentOpenMode, PdfReaderOptions?)` | static   | open for import, modify, or info-only          |
|  [07]   | `PdfReader.TestPdfFile(string\|Stream\|byte[]) -> int`           | static   | validate without a full parse                  |
|  [08]   | `Info` / `Options` / `Settings`                                  | property | metadata, output policy, font/trim settings    |
|  [09]   | `Outlines` / `SecuritySettings` / `PageCount` / `Version`        | property | bookmarks, encryption, page count, PDF version |

[DRAWING]: `XGraphics` context creation and the draw algebra.

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `XGraphics.FromPdfPage(PdfPage, XGraphicsPdfPageOptions?, ...)`                       | factory  | draw onto a page                      |
|  [02]   | `XGraphics.FromImage(XImage)`                                                         | factory  | draw onto an image                    |
|  [03]   | `XGraphics.CreateMeasureContext(XSize, XGraphicsUnit, XPageDirection)`                | factory  | measure-only context                  |
|  [04]   | `XGraphics.FromPdfForm(XPdfForm)` / `FromForm(XForm)`                                 | factory  | draw onto a reusable form             |
|  [05]   | `DrawString(string, XFont, XBrush, XPoint\|XRect, XStringFormat?)`                    | instance | point- or rect-anchored text          |
|  [06]   | `DrawImage(XImage, ...)`                                                              | instance | draw image, `XPdfForm` for page merge |
|  [07]   | `DrawLine` / `DrawRectangle` / `DrawEllipse` / `DrawBezier`                           | instance | line and curve vector-draw algebra    |
|  [08]   | `DrawCurve` / `DrawPolygon` / `DrawPath` / `DrawArc`                                  | instance | path and arc vector-draw algebra      |
|  [09]   | `Save() -> XGraphicsState` / `Restore(XGraphicsState)`                                | instance | transform/clip state save-restore     |
|  [10]   | `BeginContainer` / `EndContainer`                                                     | instance | nested container state                |
|  [11]   | `RotateTransform` / `TranslateTransform` / `ScaleTransform`                           | instance | accumulate transforms                 |
|  [12]   | `new XTextFormatter(XGraphics){ Alignment }.DrawString(string, XFont, XBrush, XRect)` | instance | wrapped multi-line text               |

[SECURITY_SIGNING]: encryption and digital signature.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `SecuritySettings.DocumentSecurityLevel`                                         | property | encryption level selection         |
|  [02]   | `SecurityHandler.UserPassword` / `OwnerPassword`                                 | property | document passwords                 |
|  [03]   | `SecurityHandler.SetEncryptionToV5(bool)` / `SetEncryptionToV4UsingAES(bool)`    | instance | AES-256 or AES-128 encryption      |
|  [04]   | `SetEncryptionToNoneAndResetPasswords()`                                         | instance | reset encryption and passwords     |
|  [05]   | `SecuritySettings.PermitPrint` / `PermitModifyDocument` / `PermitExtractContent` | property | access-permission flags            |
|  [06]   | `PermitAnnotations` / `PermitFormsFill` / `PermitAssembleDocument`               | property | access-permission flags            |
|  [07]   | `DigitalSignatureHandler.ForDocument(PdfDocument, IDigitalSigner, ...)`          | static   | attach a document signature        |
|  [08]   | `DigitalSignatureOptions { Reason; Location; ContactInfo }`                      | property | signature reason and contact       |
|  [09]   | `DigitalSignatureOptions { Rectangle; PageIndex; AppearanceHandler }`            | property | signature placement and appearance |

[REPORT_RENDER]: MigraDoc flow `Document` to `PdfDocument`.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `Document.AddSection() -> Section` / `Document.Styles[...]`         | instance | author flow content and styles            |
|  [02]   | `Section.AddParagraph()` / `AddTable()` / `AddImage()`              | instance | author flow content and styles            |
|  [03]   | `new PdfDocumentRenderer { Document = doc }.RenderDocument()`       | instance | paginate and render onto a `PdfDocument`  |
|  [04]   | `PdfDocumentRenderer.Save(string)` / `.PdfDocument`                 | instance | persist or expose the underlying document |
|  [05]   | `DocumentRenderer.PrepareDocument()` / `RenderPage(XGraphics, int)` | instance | mix flow content with precise layout      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `PdfDocument` owns the page tree, and every vector mark lands through one `XGraphics` device-independent draw algebra over a page, image, or form; `PdfReader.Open(..., Import)` folds imported pages back as `XPdfForm` drawn through `DrawImage`, so read-modify-merge and fresh authoring share the one surface.
- MigraDoc `Document` is the auto-paginated flow DOM; `PdfDocumentRenderer.RenderDocument()` renders it onto a `PdfDocument`, and `.PdfDocument` re-exposes that document for signing and encryption post-processing through the same PDFsharp surface.

[STACKING]:
- `api-nodeeditor`(`.api/api-nodeeditor.md`): `ExportRenderer.RenderPdf(Control, Size, Stream, dpi)` targets the vector-PDF deliverable, converging with this surface on the shared vector-export rail.
- `api-drafting-export`(`.api/api-drafting-export.md`): boundary split — `ACadSharp` and `DocumentFormat.OpenXml` own DWG/DXF/OOXML emit, this pair owns vector PDF; a PDF deliverable never routes through the CAD or OOXML writers.
- `api-headless`(`.api/api-headless.md`): PDFsharp is platform-neutral, so PDF authoring and MigraDoc rendering run on the windowless headless render path beside `Avalonia.Headless`.
- Within-lib: signing composes at the boundary — `IDigitalSigner` (or the ready `PdfSharpDefaultSigner`) receives key material from the AppHost secrets lease under acquire/renew/zeroize, never AppUi-held key bytes; MigraDoc reports post-process through `.PdfDocument` for `DigitalSignatureHandler.ForDocument` and `SetEncryptionToV5`.

[LOCAL_ADMISSION]:
- PDFsharp and MigraDoc are the branch's sole vector-PDF and flow-report owners: precise sheet layout draws directly with `XGraphics`, multi-page flow reports author a MigraDoc `Document`, and the drafting sheet-PDF and Diagnostics report-PDF both compose this pair.

[RAIL_LAW]:
- Package: `PDFsharp` + `PDFsharp-MigraDoc`
- Owns: the vector-PDF deliverable — the `PdfDocument` page tree, the `XGraphics` draw algebra, `PdfReader` open/import/merge, the security/signing/AcroForm/annotation/tagged-PDF namespaces, and the auto-paginated MigraDoc flow DOM rendered onto it.
- Accept: precise sheet layout drawn with `XGraphics` and `XTextFormatter`; imported pages merged as `XPdfForm`; multi-page flow reports authored as a MigraDoc `Document` and rendered by `PdfDocumentRenderer`; output policy on `PdfDocumentOptions`; signatures and AES encryption applied to the underlying `PdfDocument`.
- Reject: hand-emitting PDF byte syntax or a parallel page model; a raster path where `XGraphics` draws vector directly; hand-paginating a report with `DrawString` cursor math where the flow DOM owns pagination; a bespoke report model duplicating `Document`/`Section`/`Table`; AppUi-held signing key bytes where the AppHost secrets lease owns credential lifecycle.
