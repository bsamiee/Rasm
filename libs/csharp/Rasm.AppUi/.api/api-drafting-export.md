# [RASM_APPUI_API_DRAFTING_EXPORT]

`ACadSharp` (WRITE-scoped) and `DocumentFormat.OpenXml` supply the drafting + document export rail: DWG/DXF/SVG emit through ONE `CadDocument` typed-entity fold (`DwgWriter`/`DxfWriter`/`SvgWriter`), and OOXML docx/xlsx/pptx authoring through `WordprocessingDocument`/`SpreadsheetDocument`/`PresentationDocument`. `ACadSharp` is AppUi's WRITE authority only — `Render/drafting.md` composes the two-format write leg (DWG + DXF over one document); its READ surface (`DwgReader`/`DxfReader` and the reader configurations) is Bim-owned (`Rasm.Bim` is the CAD READ authority) and its rows live in the Bim catalog, not here. `DocumentFormat.OpenXml`'s part-graph writers are the OOXML arm of `Document/export.md`. `netDxf` is REMOVED — archived upstream (haplokuon/netDxf, feed-latest `2023.11.10`, a `net6.0` asset under the net10 consumer; `netDxf.netstandard` a distinct abandoned `3.0.1` id): the DXF concern lands on the stronger admitted owner's `DxfWriter`, and the DXF round-trip fidelity probe (dimensions/hatches/leaders/blocks against the drafting entity set) confirms the removal — a failed entity class re-pins `netDxf` as a recorded exception, so capability never deletes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp` (write-scoped)
- package: `ACadSharp` (MIT) — shared central pin (`[BIM]` group); Bim owns READ, AppUi owns WRITE
- assembly: `ACadSharp`
- namespace: `ACadSharp`, `ACadSharp.Entities`, `ACadSharp.Tables`, `ACadSharp.IO` (`DwgWriter`/`DxfWriter`), `ACadSharp.IO.SVG` (`SvgWriter`)
- asset: managed runtime library (`lib/net9.0` binds the `net10.0` consumer — highest available TFM); geometry points are `CSMath.XYZ`/`XY` (depends `CSMath`, `CSUtilities`)
- rail: drafting

[PACKAGE_SURFACE]: `DocumentFormat.OpenXml`
- package: `DocumentFormat.OpenXml` (MIT, © Microsoft) — the SDK; depends `DocumentFormat.OpenXml.Framework`
- assembly: `DocumentFormat.OpenXml`
- namespace: `DocumentFormat.OpenXml.Packaging`, `.Wordprocessing`, `.Spreadsheet`, `.Presentation`
- asset: managed runtime library (`lib/net10.0` binds the consumer directly)
- rail: document-export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ACadSharp document and object model
- rail: drafting

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                  |
| :-----: | :------------------ | :----------------- | :---------------------- |
|  [01]   | `CadDocument`       | document root      | drawing container       |
|  [02]   | `CadObject`         | object base        | graph node              |
|  [03]   | `CadSummaryInfo`    | metadata record    | document metadata       |
|  [04]   | `CadSystemVariable` | system variable    | drawing header variable |
|  [05]   | `ACadVersion`       | version enum       | DWG format selector (version-policy row) |
|  [06]   | `Color`             | color value        | ACI and true-color      |
|  [07]   | `Transparency`      | transparency value | alpha channel           |
|  [08]   | `LineWeightType`    | lineweight enum    | pen weight vocabulary   |
|  [09]   | `ObjectType`        | object type enum   | entity discriminant     |

[PUBLIC_TYPE_SCOPE]: ACadSharp entity family (the write-fold content)
- rail: drafting

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [RAIL]                |
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
- rail: drafting

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                |
| :-----: | :----------------------- | :------------ | :-------------------- |
|  [01]   | `Layer`                  | table entry   | layer definition      |
|  [02]   | `LineType`               | table entry   | linetype definition   |
|  [03]   | `TextStyle`              | table entry   | text style definition |
|  [04]   | `DimensionStyle`         | table entry   | dimension style       |
|  [05]   | `BlockRecord`            | table entry   | block registry        |
|  [06]   | `DwgWriter`              | IO writer     | DWG emit entry        |
|  [07]   | `DxfWriter`              | IO writer     | DXF emit entry        |
|  [08]   | `SvgWriter`              | IO writer     | SVG emit entry        |
|  [09]   | `DxfWriterConfiguration` | writer config | DXF write options     |
|  [10]   | `CadFileFormat`          | format enum   | file format selector  |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml document packages
- rail: document-export

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]            |
| :-----: | :--------------------------- | :------------- | :---------------- |
|  [01]   | `WordprocessingDocument`     | Word document  | docx package root |
|  [02]   | `SpreadsheetDocument`        | Excel document | xlsx package root |
|  [03]   | `PresentationDocument`       | PowerPoint doc | pptx package root |
|  [04]   | `WordprocessingDocumentType` | document type  | Word format enum  |
|  [05]   | `SpreadsheetDocumentType`    | document type  | Excel format enum |
|  [06]   | `PresentationDocumentType`   | document type  | PPT format enum   |
|  [07]   | `OpenXmlPackage`             | package base   | package root base |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml part and content-element family
- rail: document-export
- namespace: `DocumentFormat.OpenXml.Packaging` (parts), `DocumentFormat.OpenXml.Spreadsheet`, `DocumentFormat.OpenXml.Wordprocessing`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]          | [RAIL]                                 |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------- |
|  [01]   | `WorkbookPart`               | part                   | xlsx workbook part (AddWorkbookPart)   |
|  [02]   | `WorksheetPart`              | part                   | xlsx sheet part (AddNewPart)           |
|  [03]   | `MainDocumentPart`           | part                   | docx body part (AddMainDocumentPart)   |
|  [04]   | `FontTablePart`              | part                   | embedded-font part (GetStream)         |
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
- rail: drafting

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :------------------------------------------------- | :------------- | :------------------ |
|  [01]   | static `DwgWriter.Write(string\|Stream, CadDocument, DwgWriterConfiguration?, NotificationEventHandler?)` | `DwgWriter` | one-call DWG emit |
|  [02]   | static `DxfWriter.Write(string\|Stream, CadDocument, bool binary, DxfWriterConfiguration?, NotificationEventHandler?)` | `DxfWriter` | one-call DXF emit (binary/text) |
|  [03]   | `new DwgWriter(string\|Stream, CadDocument)` + `.Configuration` + `Write()` | `DwgWriter` | instance DWG emit (reusable config) |
|  [04]   | `new DxfWriter(string\|Stream, CadDocument, bool binary = false)` + `.Configuration` + `Write()` | `DxfWriter` | instance DXF emit (binary at ctor) |
|  [05]   | `new SvgWriter(string\|Stream, CadDocument)` + `.Configuration` (`SvgConfiguration`) + `Write()` | `SvgWriter` | SVG emit (`SvgConfiguration.LineWeightRatio`/`DefaultLineWeight`) |

[ENTRYPOINT_SCOPE]: DocumentFormat.OpenXml package factory operations
- rail: document-export

| [INDEX] | [SURFACE]                                    | [SURFACE_ROOT]           | [RAIL]           |
| :-----: | :------------------------------------------- | :----------------------- | :--------------- |
|  [01]   | `Create(string, WordprocessingDocumentType)` | `WordprocessingDocument` | create docx      |
|  [02]   | `Create(Stream, WordprocessingDocumentType)` | `WordprocessingDocument` | stream create    |
|  [03]   | `Open(string, isEditable)`                   | `WordprocessingDocument` | open docx        |
|  [04]   | `Create(string, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | create xlsx      |
|  [05]   | `Create(Stream, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | stream create    |
|  [06]   | `Create(string, PresentationDocumentType)`   | `PresentationDocument`   | create pptx      |
|  [07]   | `Save` / `Dispose`                           | `OpenXmlPackage`         | commit and close |

[ENTRYPOINT_SCOPE]: OpenXml part-add and content-build operations
- rail: document-export

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]                 | [RAIL]                |
| :-----: | :--------------------------------------------------- | :----------------------------- | :-------------------- |
|  [01]   | `AddWorkbookPart()`                                  | `SpreadsheetDocument`          | workbook part create  |
|  [02]   | `AddNewPart<WorksheetPart>()`                        | `WorkbookPart`                 | sheet part create     |
|  [03]   | `GetIdOfPart(part)`                                  | `WorkbookPart`                 | relationship-id query |
|  [04]   | `AddMainDocumentPart()`                              | `WordprocessingDocument`       | docx body part create |
|  [05]   | `AddNewPart<FontTablePart>()` / `GetStream()`        | `WorkbookPart`/`FontTablePart` | embedded-font pack    |
|  [06]   | `AppendChild(element)` / `Append(elements)`          | content element                | child-element insert  |
|  [07]   | `part.Workbook` / `part.Worksheet` / `part.Document` | part                           | root-element assign   |

## [04]-[IMPLEMENTATION_LAW]

[DRAFTING_WRITE_TOPOLOGY]:
[ACADSHARP_WRITE]:
`ACadSharp` (`lib/net9.0` bound) is AppUi's WRITE authority: `CadDocument` is the ONE document root the drafting leg builds, and `ACadSharp.IO` emits it three ways through `CadWriterBase<T>` — `DwgWriter` (DWG), `DxfWriter` (DXF binary/text), and `ACadSharp.IO.SVG.SvgWriter` (SVG, `SvgConfiguration.LineWeightRatio`/`DefaultLineWeight`). Each writer exposes a settable `.Configuration` and a static one-call `Write` overload with an optional trailing `NotificationEventHandler` warning/error sink.
- `ACadSharp.Entities` is the geometry roster the fold populates (`Line`/`Arc`/`Circle`/`Spline`/`Polyline`/`LwPolyline`/`Hatch`/`MText`/`Dimension`/`Insert`); `ACadSharp.Tables` carries the `Layer`/`LineType`/`TextStyle`/`DimensionStyle`/`BlockRecord` entries entities bind. Geometry points are `CSMath.XYZ`; entity layers attach through the entity `Layer` property bound to a `Layer` table entry; the document `Entities`/`Layers` collections take typed entities through `Add`.
- The output DWG/DXF version is a POLICY ROW over `ACadVersion` (never a hardcoded `AutoCad2018` literal); the two-format write leg (DWG + DXF) is two rows on one `DraftEmit` axis over the single `CadDocument`, not two document models.
- `DocumentFormat.OpenXml`: `Packaging` owns the three document roots; `Wordprocessing`/`Spreadsheet`/`Presentation` supply the open content-element trees. The OOXML part-graph is `Document/export.md`'s arm.

[LOCAL_ADMISSION]:
- `ACadSharp` owns CAD WRITE (DWG/DXF/SVG) over `CadDocument`; its READ surface (`DwgReader`/`DxfReader`, `CadReaderConfiguration`/`DwgReaderConfiguration`) is Bim-owned and lives in the Bim catalog — AppUi never opens a CAD file, only emits one.
- `netDxf` is not admitted — the DXF concern is `ACadSharp.DxfWriter`; a re-pin is a recorded exception only when the fidelity probe fails a specific entity class, never a silent restore.
- `DocumentFormat.OpenXml` package documents are disposable; every create path pairs with `Save`/`Dispose` or a `using` scope. OOXML part-graph construction flows root-first: `Create(Stream, type)` mints the package, `AddWorkbookPart`/`AddMainDocumentPart` mints the root part, the part's root element is assigned, child parts attach under it, content elements append through `Append`/`AppendChild`, `GetIdOfPart` supplies the relationship id a `Sheet` entry binds, and `Save` + `using`-dispose commits the byte stream — never a hand-written `rId` or raw ZIP/XML manipulation.
- Entity construction flows through the entity type constructor, then collection `Add`; never bypass typed entity APIs with raw group-code writes.

[RAIL_LAW]:
- Package: `ACadSharp` — owns DWG/DXF/SVG CAD WRITE over one `CadDocument` (READ is Bim's)
- Package: `DocumentFormat.OpenXml` — owns OOXML (docx/xlsx/pptx) package authoring
- Accept: `Render/drafting.md` composes the ACadSharp two-format write leg; `Document/export.md` composes the OOXML part-graph arm; all export flows through typed document roots and their WRITE entry points
- Reject: hand-rolled binary DWG/DXF writers; a `netDxf` DXF path where `DxfWriter` owns it; a second CAD document model beside `CadDocument`; raw ZIP/XML manipulation of OOXML packages; an AppUi-side CAD reader where Bim owns READ
