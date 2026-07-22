# [RASM_APPUI_API_DRAFTING_EXPORT]

`ACadSharp` (WRITE-scoped) and `DocumentFormat.OpenXml` supply AppUi's drafting and document-export rail. `ACadSharp` folds one `CadDocument` typed-entity graph to DWG, DXF, and SVG through `DwgWriter`/`DxfWriter`/`SvgWriter`, and AppUi holds CAD WRITE authority alone — the Bim catalog owns the `DwgReader`/`DxfReader` READ surface over the same document model. `DocumentFormat.OpenXml` authors OOXML docx/xlsx/pptx through the `WordprocessingDocument`/`SpreadsheetDocument`/`PresentationDocument` part graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp` (MIT)
- assembly: `ACadSharp`
- namespace: `ACadSharp`, `ACadSharp.Entities`, `ACadSharp.Tables`, `ACadSharp.IO`, `ACadSharp.IO.SVG`
- asset: managed runtime library; geometry points are `CSMath.XYZ`/`XY` (depends `CSMath`, `CSUtilities`)
- rail: drafting

[PACKAGE_SURFACE]: `DocumentFormat.OpenXml`
- package: `DocumentFormat.OpenXml` (MIT, © Microsoft)
- assembly: `DocumentFormat.OpenXml`
- namespace: `DocumentFormat.OpenXml.Packaging`, `.Wordprocessing`, `.Spreadsheet`, `.Presentation`
- asset: managed runtime library (depends `DocumentFormat.OpenXml.Framework`)
- rail: document-export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ACadSharp document and object model

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                             |
| :-----: | :------------------ | :----------------- | :--------------------------------------- |
|  [01]   | `CadDocument`       | document root      | drawing container                        |
|  [02]   | `CadObject`         | object base        | graph node                               |
|  [03]   | `CadSummaryInfo`    | metadata record    | document metadata                        |
|  [04]   | `CadSystemVariable` | system variable    | drawing header variable                  |
|  [05]   | `ACadVersion`       | version enum       | DWG format selector (version-policy row) |
|  [06]   | `Color`             | color value        | ACI and true-color                       |
|  [07]   | `Transparency`      | transparency value | alpha channel                            |
|  [08]   | `LineWeightType`    | lineweight enum    | pen weight vocabulary                    |
|  [09]   | `ObjectType`        | object type enum   | entity discriminant                      |

[PUBLIC_TYPE_SCOPE]: ACadSharp entity family (the write-fold content)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :----------- | :---------------- | :-------------------- |
|  [01]   | `Entity`     | entity base       | geometric entity root |
|  [02]   | `Line`       | geometry entity   | line segment          |
|  [03]   | `Arc`        | geometry entity   | arc segment           |
|  [04]   | `Circle`     | geometry entity   | circle                |
|  [05]   | `Spline`     | geometry entity   | NURBS spline          |
|  [06]   | `Polyline`   | geometry entity   | polyline family       |
|  [07]   | `LwPolyline` | geometry entity   | lightweight polyline  |
|  [08]   | `Hatch`      | geometry entity   | hatch fill            |
|  [09]   | `MText`      | text entity       | multi-line text       |
|  [10]   | `TextEntity` | text entity base  | single-line text      |
|  [11]   | `Dimension`  | annotation entity | dimension family root |
|  [12]   | `Insert`     | block entity      | block reference       |
|  [13]   | `Mesh`       | mesh entity       | 3D mesh               |
|  [14]   | `Viewport`   | viewport entity   | paper-space viewport  |

[PUBLIC_TYPE_SCOPE]: ACadSharp table and WRITE-IO family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------------- | :------------ | :-------------------- |
|  [01]   | `Layer`                  | table entry   | layer definition      |
|  [02]   | `LineType`               | table entry   | linetype definition   |
|  [03]   | `LineType.Segment`       | pattern row   | dash, gap, or dot     |
|  [04]   | `TextStyle`              | table entry   | text style definition |
|  [05]   | `DimensionStyle`         | table entry   | dimension style       |
|  [06]   | `BlockRecord`            | table entry   | block registry        |
|  [07]   | `DwgWriter`              | IO writer     | DWG emit entry        |
|  [08]   | `DxfWriter`              | IO writer     | DXF emit entry        |
|  [09]   | `SvgWriter`              | IO writer     | SVG emit entry        |
|  [10]   | `DxfWriterConfiguration` | writer config | DXF write options     |
|  [11]   | `CadFileFormat`          | format enum   | file format selector  |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml document packages

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]      |
| :-----: | :--------------------------- | :------------- | :---------------- |
|  [01]   | `WordprocessingDocument`     | Word document  | docx package root |
|  [02]   | `SpreadsheetDocument`        | Excel document | xlsx package root |
|  [03]   | `PresentationDocument`       | PowerPoint doc | pptx package root |
|  [04]   | `WordprocessingDocumentType` | document type  | Word format enum  |
|  [05]   | `SpreadsheetDocumentType`    | document type  | Excel format enum |
|  [06]   | `PresentationDocumentType`   | document type  | PPT format enum   |
|  [07]   | `OpenXmlPackage`             | package base   | package root base |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml part and content-element family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]          | [CAPABILITY]                           |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------- |
|  [01]   | `WorkbookPart`               | part                   | xlsx workbook part (AddWorkbookPart)   |
|  [02]   | `WorksheetPart`              | part                   | xlsx sheet part (AddNewPart)           |
|  [03]   | `MainDocumentPart`           | part                   | docx body part (AddMainDocumentPart)   |
|  [04]   | `FontTablePart` / `FontPart` | part                   | docx embedded-font parts               |
|  [05]   | `Workbook` / `Sheets`        | content element        | xlsx workbook + sheet registry         |
|  [06]   | `Sheet`                      | content element        | sheet registry entry (Id/SheetId/Name) |
|  [07]   | `Worksheet` / `SheetData`    | content element        | xlsx sheet body + row container        |
|  [08]   | `Row` / `Cell`               | content element        | xlsx row + cell                        |
|  [09]   | `CellValue` / `CellValues`   | content element + enum | cell value + data-type enum            |
|  [10]   | `Document` / `Body`          | content element        | docx document + body                   |
|  [11]   | `Paragraph` / `Run` / `Text` | content element        | docx paragraph/run/text run            |
|  [12]   | `SpaceProcessingModeValues`  | enum                   | run-text whitespace preservation       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ACadSharp WRITE operations — one `CadDocument` fold, three format writers

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `DwgWriter.Write(string\|Stream, CadDocument)`       | static   | one-call DWG emit                 |
|  [02]   | `DxfWriter.Write(string\|Stream, CadDocument, bool)` | static   | one-call DXF emit, binary or text |
|  [03]   | `new DwgWriter(string\|Stream, CadDocument)`         | ctor     | configured DWG emit               |
|  [04]   | `new DxfWriter(string\|Stream, CadDocument, bool)`   | ctor     | configured DXF emit               |
|  [05]   | `new SvgWriter(string\|Stream, CadDocument)`         | ctor     | configured SVG emit               |
|  [06]   | `LineType.AddSegment(Segment)`                       | instance | append ordered dash pattern       |

- Static `Write`: a trailing optional `<Format>WriterConfiguration?` overrides `.Configuration`, and a `NotificationEventHandler?` takes the warning/error sink.
- `DxfWriter`: `bool binary` selects binary or ASCII DXF — the ctor fixes it, the static overload takes it per-call; `.IsBinary` reads it back.
- `SvgWriter`: `SvgConfiguration` exposes `LineWeightRatio` and `DefaultLineWeight`, settable before `Write()`.
- `LineType.AddSegment`: signed `Segment.Length` encodes dash (positive), space (negative), dot (zero).

[ENTRYPOINT_SCOPE]: DocumentFormat.OpenXml package factory operations

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]     |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------- |
|  [01]   | `WordprocessingDocument.Create(string\|Stream, WordprocessingDocumentType)` | static   | create docx      |
|  [02]   | `WordprocessingDocument.Open(string, bool)`                                 | static   | open docx        |
|  [03]   | `SpreadsheetDocument.Create(string\|Stream, SpreadsheetDocumentType)`       | static   | create xlsx      |
|  [04]   | `PresentationDocument.Create(string, PresentationDocumentType)`             | static   | create pptx      |
|  [05]   | `OpenXmlPackage.Save()` / `Dispose()`                                       | instance | commit and close |

[ENTRYPOINT_SCOPE]: OpenXml part-add and content-build operations

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `SpreadsheetDocument.AddWorkbookPart()`                                     | instance | workbook part create  |
|  [02]   | `WorkbookPart.AddNewPart<WorksheetPart>()`                                  | instance | sheet part create     |
|  [03]   | `WorkbookPart.GetIdOfPart(part)`                                            | instance | relationship-id query |
|  [04]   | `WordprocessingDocument.AddMainDocumentPart()`                              | instance | docx body part create |
|  [05]   | `MainDocumentPart.AddNewPart<FontTablePart>()` / `AddFontPart` / `FeedData` | instance | embedded-font pack    |
|  [06]   | `AppendChild(element)` / `Append(elements)`                                 | instance | child-element insert  |
|  [07]   | `part.Workbook` / `part.Worksheet` / `part.Document`                        | property | root-element assign   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every emit folds one `CadDocument` graph through `CadWriterBase<T>`, the three writers differing only by target format; the output DWG/DXF version is a policy row over `ACadVersion`, and the DWG+DXF write leg is two rows on one emit axis over the single document, never a second model.
- OOXML authoring flows root-first over one disposable package: `Create` mints it, `AddWorkbookPart`/`AddMainDocumentPart` mints the root part, `GetIdOfPart` supplies the relationship id a `Sheet` binds, content appends through `Append`/`AppendChild`, and `Save` under a `using` scope commits the byte stream.

[STACKING]:
- `ACadSharp`(`Rasm.Bim/.api/api-acadsharp.md`): the Bim catalog owns the `DwgReader`/`DxfReader` READ surface over the same `CadDocument` root — AppUi emits, Bim admits, one document model shared across the WRITE/READ split.
- `Render/drafting.md`: composes the DWG+DXF two-format write leg over one `CadDocument` populated from `ACadSharp.Entities` and `ACadSharp.Tables`.
- `Document/export.md`: composes the OOXML part-graph arm through the three `DocumentFormat.OpenXml` document roots.

[LOCAL_ADMISSION]:
- `ACadSharp` owns CAD WRITE (DWG/DXF/SVG) over `CadDocument`; AppUi emits a CAD file, never opens one — READ is Bim's.
- OOXML packages are disposable: every create path pairs with `Save`/`Dispose` or a `using` scope, and part construction rides typed part/element APIs.
- Entity construction flows through the typed entity constructor then collection `Add`.

[RAIL_LAW]:
- Package: `ACadSharp` — owns DWG/DXF/SVG CAD WRITE over one `CadDocument`
- Package: `DocumentFormat.OpenXml` — owns OOXML docx/xlsx/pptx package authoring
- Accept: export flows through typed document roots and their WRITE entry points
- Reject: hand-rolled binary DWG/DXF writers, a second CAD document model beside `CadDocument`, raw ZIP/XML manipulation of OOXML packages, an AppUi-side CAD reader
