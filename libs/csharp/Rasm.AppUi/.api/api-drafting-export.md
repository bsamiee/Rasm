# [RASM_APPUI_API_DRAFTING_EXPORT]

`ACadSharp`, `netDxf`, and `DocumentFormat.OpenXml` supply the drafting and sheet export rail: DWG/DXF round-trip through `CadDocument`/`DxfDocument` with typed entity, table, and IO surfaces, and OOXML document creation and editing through `WordprocessingDocument`, `SpreadsheetDocument`, and `PresentationDocument`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp` (3.6.29, MIT)
- assembly: `ACadSharp`
- namespace: `ACadSharp`, `ACadSharp.Entities`, `ACadSharp.Tables`, `ACadSharp.IO`, `ACadSharp.IO.SVG`
- asset: managed runtime library (`lib/net9.0` binds the `net10.0` consumer — highest available TFM); geometry points are `CSMath.XYZ`/`XY` (depends `CSMath`, `CSUtilities`)
- rail: drafting

[PACKAGE_SURFACE]: `netDxf`
- package: `netDxf` (2023.11.10, MIT) — the maintained `netDxf.netstandard`/Reloaded fork on the `netDxf` id
- assembly: `netDxf`
- namespace: `netDxf`, `netDxf.Entities`, `netDxf.Tables`, `netDxf.IO`
- asset: managed runtime library (`lib/net6.0` binds the `net10.0` consumer — highest available TFM; `netstandard2.0` fallback); geometry points are `netDxf.Vector2`/`Vector3`
- rail: drafting

[PACKAGE_SURFACE]: `DocumentFormat.OpenXml`
- package: `DocumentFormat.OpenXml` (3.5.1, MIT, © Microsoft) — the SDK; depends `DocumentFormat.OpenXml.Framework`
- assembly: `DocumentFormat.OpenXml`
- namespace: `DocumentFormat.OpenXml.Packaging`, `.Wordprocessing`, `.Spreadsheet`, `.Presentation`
- asset: managed runtime library (`lib/net10.0` binds the consumer directly)
- rail: drafting

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ACadSharp document and object model
- rail: drafting

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                  |
| :-----: | :------------------ | :----------------- | :---------------------- |
|  [01]   | `CadDocument`       | document root      | drawing container       |
|  [02]   | `CadObject`         | object base        | graph node              |
|  [03]   | `CadSummaryInfo`    | metadata record    | document metadata       |
|  [04]   | `CadSystemVariable` | system variable    | drawing header variable |
|  [05]   | `ACadVersion`       | version enum       | DWG format selector     |
|  [06]   | `Color`             | color value        | ACI and true-color      |
|  [07]   | `Transparency`      | transparency value | alpha channel           |
|  [08]   | `LineWeightType`    | lineweight enum    | pen weight vocabulary   |
|  [09]   | `ObjectType`        | object type enum   | entity discriminant     |
|  [10]   | `DwgPreview`        | preview image      | DWG thumbnail           |

[PUBLIC_TYPE_SCOPE]: ACadSharp entity family
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

[PUBLIC_TYPE_SCOPE]: ACadSharp table and IO family
- rail: drafting

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                |
| :-----: | :----------------------- | :------------ | :-------------------- |
|  [01]   | `Layer`                  | table entry   | layer definition      |
|  [02]   | `LineType`               | table entry   | linetype definition   |
|  [03]   | `TextStyle`              | table entry   | text style definition |
|  [04]   | `DimensionStyle`         | table entry   | dimension style       |
|  [05]   | `BlockRecord`            | table entry   | block registry        |
|  [06]   | `DwgReader`              | IO reader     | DWG parse entry       |
|  [07]   | `DwgWriter`              | IO writer     | DWG emit entry        |
|  [08]   | `DxfReader`              | IO reader     | DXF parse entry       |
|  [09]   | `DxfWriter`              | IO writer     | DXF emit entry        |
|  [10]   | `SvgWriter`              | IO writer     | SVG emit entry        |
|  [11]   | `CadFileFormat`          | format enum   | file format selector  |
|  [12]   | `CadReaderConfiguration` | reader config | read options          |
|  [13]   | `DwgReaderConfiguration` | reader config | DWG read options      |
|  [14]   | `DxfWriterConfiguration` | writer config | DXF write options     |

[PUBLIC_TYPE_SCOPE]: netDxf document and entity family
- rail: drafting

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]      | [RAIL]                |
| :-----: | :------------- | :----------------- | :-------------------- |
|  [01]   | `DxfDocument`  | document root      | DXF drawing container |
|  [02]   | `DxfObject`    | object base        | graph node            |
|  [03]   | `AciColor`     | color value        | ACI palette entry     |
|  [04]   | `Lineweight`   | lineweight value   | pen weight            |
|  [05]   | `Transparency` | transparency value | alpha channel         |
|  [06]   | `XData`        | extended data      | custom object data    |
|  [07]   | `Matrix2`      | 2D matrix value    | transform             |
|  [08]   | `Matrix4`      | 4D matrix value    | transform             |
|  [09]   | `Vector3`      | 3D vector value    | coordinate            |

[PUBLIC_TYPE_SCOPE]: netDxf entity and failure family
- rail: drafting

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [RAIL]               |
| :-----: | :-------------------------------- | :---------------- | :------------------- |
|  [01]   | `EntityObject`                    | entity base       | entity root          |
|  [02]   | `Line`                            | geometry entity   | line segment         |
|  [03]   | `Arc`                             | geometry entity   | arc segment          |
|  [04]   | `Circle`                          | geometry entity   | circle               |
|  [05]   | `Spline`                          | geometry entity   | NURBS spline         |
|  [06]   | `Polyline`                        | geometry entity   | polyline family      |
|  [07]   | `Hatch`                           | geometry entity   | hatch fill           |
|  [08]   | `MText`                           | text entity       | multi-line text      |
|  [09]   | `Insert`                          | block entity      | block reference      |
|  [10]   | `Mesh`                            | mesh entity       | 3D mesh              |
|  [11]   | `Dimension`                       | annotation entity | dimension base       |
|  [12]   | `Viewport`                        | viewport entity   | paper-space viewport |
|  [13]   | `DxfVersionNotSupportedException` | read failure      | unsupported version  |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml document packages
- rail: drafting

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
- rail: drafting
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

[ENTRYPOINT_SCOPE]: ACadSharp read and write operations
- rail: drafting

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :------------------------------------------------- | :------------- | :------------------ |
|  [01]   | `DwgReader(string\|Stream, NotificationEventHandler?)` + `.Configuration` (`DwgReaderConfiguration`) | `DwgReader` | DWG open (config is a property) |
|  [02]   | `Read()` / static `DwgReader.Read(string\|Stream, DwgReaderConfiguration, NotificationEventHandler?)` | `DwgReader` | DWG document parse |
|  [03]   | `DxfReader(string\|Stream, NotificationEventHandler?)` + `.Configuration` (`DxfReaderConfiguration`) | `DxfReader` | DXF open (config is a property) |
|  [04]   | `Read()` / static `DxfReader.Read(string\|Stream, DxfReaderConfiguration, NotificationEventHandler?)` | `DxfReader` | DXF document parse |
|  [05]   | static `DwgWriter.Write(string\|Stream, CadDocument, DwgWriterConfiguration?, NotificationEventHandler?)` | `DwgWriter` | one-call DWG emit |
|  [06]   | static `DxfWriter.Write(string\|Stream, CadDocument, bool binary, DxfWriterConfiguration?, NotificationEventHandler?)` | `DxfWriter` | one-call DXF emit |
|  [07]   | `new DwgWriter(string\|Stream, CadDocument)` + `.Configuration` + `Write()` | `DwgWriter` | instance DWG emit (reusable config) |
|  [08]   | `new DxfWriter(string\|Stream, CadDocument, bool binary = false)` + `.Configuration` + `Write()` | `DxfWriter` | instance DXF emit (binary at ctor) |
|  [09]   | `new SvgWriter(string\|Stream, CadDocument)` + `.Configuration` (`SvgConfiguration`) + `Write()` | `SvgWriter` | SVG emit (`SvgConfiguration.LineWeightRatio`/`DefaultLineWeight`) |

[ENTRYPOINT_SCOPE]: netDxf document round-trip operations
- rail: drafting

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------------------------- | :------------- | :------------------ |
|  [01]   | `DxfDocument.Load(string)`         | `DxfDocument`  | file load           |
|  [02]   | `DxfDocument.Load(Stream)`         | `DxfDocument`  | stream load         |
|  [03]   | `Save(string)`                     | `DxfDocument`  | text file emit      |
|  [04]   | `Save(string, isBinary)`           | `DxfDocument`  | binary/text emit    |
|  [05]   | `Save(Stream)`                     | `DxfDocument`  | stream emit         |
|  [06]   | `DxfDocument(DxfVersion)`          | `DxfDocument`  | versioned construct |
|  [07]   | `Entities.*` collections           | `DxfDocument`  | entity access       |
|  [08]   | `Layers` / `TextStyles` / `Blocks` | `DxfDocument`  | table access        |

[ENTRYPOINT_SCOPE]: DocumentFormat.OpenXml package factory operations
- rail: drafting

| [INDEX] | [SURFACE]                                    | [SURFACE_ROOT]           | [RAIL]           |
| :-----: | :------------------------------------------- | :----------------------- | :--------------- |
|  [01]   | `Create(string, WordprocessingDocumentType)` | `WordprocessingDocument` | create docx      |
|  [02]   | `Create(Stream, WordprocessingDocumentType)` | `WordprocessingDocument` | stream create    |
|  [03]   | `Open(string, isEditable)`                   | `WordprocessingDocument` | open docx        |
|  [04]   | `Open(Stream, isEditable)`                   | `WordprocessingDocument` | stream open      |
|  [05]   | `CreateFromTemplate(string)`                 | `WordprocessingDocument` | template create  |
|  [06]   | `Create(string, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | create xlsx      |
|  [07]   | `Create(Stream, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | stream create    |
|  [08]   | `Open(string, isEditable)`                   | `SpreadsheetDocument`    | open xlsx        |
|  [09]   | `Open(Stream, isEditable)`                   | `SpreadsheetDocument`    | stream open      |
|  [10]   | `Create(string, PresentationDocumentType)`   | `PresentationDocument`   | create pptx      |
|  [11]   | `Open(string, isEditable)`                   | `PresentationDocument`   | open pptx        |
|  [12]   | `Save` / `Dispose`                           | `OpenXmlPackage`         | commit and close |

[ENTRYPOINT_SCOPE]: OpenXml part-add and content-build operations
- rail: drafting

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

[DRAFTING_TOPOLOGY]:
- `ACadSharp` (3.6.29, `lib/net9.0` bound): `CadDocument` is the document root; `ACadSharp.IO` owns all read and write paths through `CadReaderBase<T>`/`CadWriterBase<T>` (settable `.Configuration`, `Read()`/`Write()`, plus static one-call `Read`/`Write` overloads); a `NotificationEventHandler` is the optional warning/error sink threaded through every reader/writer ctor and static call; `SvgWriter` (in `ACadSharp.IO.SVG`) emits SVG with `SvgConfiguration.LineWeightRatio`/`DefaultLineWeight`; `ACadSharp.Entities` covers the geometry entity roster; `ACadSharp.Tables` covers layer, linetype, style, and block-record entries
- `netDxf`: 365 types across 10 namespaces; `DxfDocument` is the document root with static `Load` factory and instance `Save`; read/write is self-contained through `DxfDocument.Load`/`Save`; `DxfDocument(Header.DxfVersion)` selects the output version, geometry points are `netDxf.Vector2`/`Vector3`, `netDxf.Entities.Line`/`MText` are the line and text entities, and `netDxf.Tables.Layer`/`Linetype` (with the `Linetype.Continuous`/`Dashed` singletons) carry the layer structure
- `ACadSharp` geometry points are `CSMath.XYZ`, entity layers attach through the entity `Layer` property bound to a `Layer` table entry (`LineType.Continuous`/`Dashed` linetype singletons), `MText` carries `Value`/`InsertPoint`/`Height`, and the document `Entities`/`Layers` collections take typed entities through `Add`
- `DocumentFormat.OpenXml`: 5210 types across 140 namespaces; `Packaging` owns the three document roots; `Wordprocessing`, `Spreadsheet`, and `Presentation` namespaces supply the open content element trees

[LOCAL_ADMISSION]:
- `ACadSharp` owns DWG authorship and round-trip; `netDxf` owns DXF authorship and round-trip; they are not interchangeable — `CadDocument` and `DxfDocument` are independent models.
- `DocumentFormat.OpenXml` package documents are disposable; every open or create path pairs with `Save`/`Dispose` or a `using` scope.
- Entity construction flows through the entity type constructor, then collection `Add`; never bypass typed entity APIs with raw group-code writes.
- Configuration objects scope read/write posture two ways: the instance `Reader`/`Writer` exposes a settable `.Configuration` property (`CadReaderBase<T>`/`CadWriterBase<T>`, default-constructed), and the static `Read`/`Write` overloads take the config as an optional trailing argument alongside an optional `NotificationEventHandler` (the warning/error sink). The reader ctor's second argument is the `NotificationEventHandler`, never the config — set `reader.Configuration` after construction.
- OOXML part-graph construction flows root-first: `Create(Stream, type)` mints the package, `AddWorkbookPart`/`AddMainDocumentPart` mints the root part, the part's root element (`Workbook`/`Document`) is assigned, child parts (`WorksheetPart` via `AddNewPart`, `FontTablePart` for embedded faces) attach under it, content elements (`Sheets`/`Sheet`/`SheetData`/`Row`/`Cell`, `Body`/`Paragraph`/`Run`/`Text`) append through `Append`/`AppendChild`, and `Save` on the root element plus the `using` package dispose commits the byte stream; `GetIdOfPart` supplies the relationship id a `Sheet` registry entry binds, never a hand-written `rId`.

[RAIL_LAW]:
- Package: `ACadSharp` — owns DWG/DXF CAD format read/write
- Package: `netDxf` — owns DXF-only CAD format read/write with a simpler API surface
- Package: `DocumentFormat.OpenXml` — owns OOXML (docx/xlsx/pptx) package authoring
- Accept: all drafting export flows through typed document roots and their IO entry points
- Reject: hand-rolled binary DWG/DXF writers or raw ZIP/XML manipulation of OOXML packages
