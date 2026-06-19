# [RASM_APPUI_API_DRAFTING_EXPORT]

`ACadSharp`, `netDxf`, and `DocumentFormat.OpenXml` supply the drafting and sheet export rail: DWG/DXF round-trip through `CadDocument`/`DxfDocument` with typed entity, table, and IO surfaces, and OOXML document creation and editing through `WordprocessingDocument`, `SpreadsheetDocument`, and `PresentationDocument`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- assembly: `ACadSharp`
- namespace: `ACadSharp`
- namespace: `ACadSharp.Entities`
- namespace: `ACadSharp.Tables`
- namespace: `ACadSharp.IO`
- asset: runtime library
- rail: drafting

[PACKAGE_SURFACE]: `netDxf`
- package: `netDxf`
- assembly: `netDxf`
- namespace: `netDxf`
- namespace: `netDxf.Entities`
- namespace: `netDxf.IO`
- asset: runtime library
- rail: drafting

[PACKAGE_SURFACE]: `DocumentFormat.OpenXml`
- package: `DocumentFormat.OpenXml`
- assembly: `DocumentFormat.OpenXml`
- namespace: `DocumentFormat.OpenXml.Packaging`
- namespace: `DocumentFormat.OpenXml.Wordprocessing`
- namespace: `DocumentFormat.OpenXml.Spreadsheet`
- namespace: `DocumentFormat.OpenXml.Presentation`
- asset: runtime library
- rail: drafting

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ACadSharp document and object model
- rail: drafting

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                  |
| :-----: | :------------------ | :----------------- | :---------------------- |
|   [1]   | `CadDocument`       | document root      | drawing container       |
|   [2]   | `CadObject`         | object base        | graph node              |
|   [3]   | `CadSummaryInfo`    | metadata record    | document metadata       |
|   [4]   | `CadSystemVariable` | system variable    | drawing header variable |
|   [5]   | `ACadVersion`       | version enum       | DWG format selector     |
|   [6]   | `Color`             | color value        | ACI and true-color      |
|   [7]   | `Transparency`      | transparency value | alpha channel           |
|   [8]   | `LineWeightType`    | lineweight enum    | pen weight vocabulary   |
|   [9]   | `ObjectType`        | object type enum   | entity discriminant     |
|  [10]   | `DwgPreview`        | preview image      | DWG thumbnail           |

[PUBLIC_TYPE_SCOPE]: ACadSharp entity family
- rail: drafting

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [RAIL]                |
| :-----: | :----------- | :---------------- | :-------------------- |
|   [1]   | `Entity`     | entity base       | geometric entity root |
|   [2]   | `Line`       | geometry entity   | line segment          |
|   [3]   | `Arc`        | geometry entity   | arc segment           |
|   [4]   | `Circle`     | geometry entity   | circle                |
|   [5]   | `Spline`     | geometry entity   | NURBS spline          |
|   [6]   | `Polyline`   | geometry entity   | polyline family       |
|   [7]   | `LwPolyline` | geometry entity   | lightweight polyline  |
|   [8]   | `Hatch`      | geometry entity   | hatch fill            |
|   [9]   | `MText`      | text entity       | multi-line text       |
|  [10]   | `TextEntity` | text entity base  | single-line text      |
|  [11]   | `Dimension`  | annotation entity | dimension family root |
|  [12]   | `Insert`     | block entity      | block reference       |
|  [13]   | `Mesh`       | mesh entity       | 3D mesh               |
|  [14]   | `Viewport`   | viewport entity   | paper-space viewport  |

[PUBLIC_TYPE_SCOPE]: ACadSharp table and IO family
- rail: drafting

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                |
| :-----: | :----------------------- | :------------ | :-------------------- |
|   [1]   | `Layer`                  | table entry   | layer definition      |
|   [2]   | `LineType`               | table entry   | linetype definition   |
|   [3]   | `TextStyle`              | table entry   | text style definition |
|   [4]   | `DimensionStyle`         | table entry   | dimension style       |
|   [5]   | `BlockRecord`            | table entry   | block registry        |
|   [6]   | `DwgReader`              | IO reader     | DWG parse entry       |
|   [7]   | `DwgWriter`              | IO writer     | DWG emit entry        |
|   [8]   | `DxfReader`              | IO reader     | DXF parse entry       |
|   [9]   | `DxfWriter`              | IO writer     | DXF emit entry        |
|  [10]   | `SvgWriter`              | IO writer     | SVG emit entry        |
|  [11]   | `CadFileFormat`          | format enum   | file format selector  |
|  [12]   | `CadReaderConfiguration` | reader config | read options          |
|  [13]   | `DwgReaderConfiguration` | reader config | DWG read options      |
|  [14]   | `DxfWriterConfiguration` | writer config | DXF write options     |

[PUBLIC_TYPE_SCOPE]: netDxf document and entity family
- rail: drafting

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]      | [RAIL]                |
| :-----: | :------------- | :----------------- | :-------------------- |
|   [1]   | `DxfDocument`  | document root      | DXF drawing container |
|   [2]   | `DxfObject`    | object base        | graph node            |
|   [3]   | `AciColor`     | color value        | ACI palette entry     |
|   [4]   | `Lineweight`   | lineweight value   | pen weight            |
|   [5]   | `Transparency` | transparency value | alpha channel         |
|   [6]   | `XData`        | extended data      | custom object data    |
|   [7]   | `Matrix2`      | 2D matrix value    | transform             |
|   [8]   | `Matrix4`      | 4D matrix value    | transform             |
|   [9]   | `Vector3`      | 3D vector value    | coordinate            |

[PUBLIC_TYPE_SCOPE]: netDxf entity and failure family
- rail: drafting

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [RAIL]               |
| :-----: | :-------------------------------- | :---------------- | :------------------- |
|   [1]   | `EntityObject`                    | entity base       | entity root          |
|   [2]   | `Line`                            | geometry entity   | line segment         |
|   [3]   | `Arc`                             | geometry entity   | arc segment          |
|   [4]   | `Circle`                          | geometry entity   | circle               |
|   [5]   | `Spline`                          | geometry entity   | NURBS spline         |
|   [6]   | `Polyline`                        | geometry entity   | polyline family      |
|   [7]   | `Hatch`                           | geometry entity   | hatch fill           |
|   [8]   | `MText`                           | text entity       | multi-line text      |
|   [9]   | `Insert`                          | block entity      | block reference      |
|  [10]   | `Mesh`                            | mesh entity       | 3D mesh              |
|  [11]   | `Dimension`                       | annotation entity | dimension base       |
|  [12]   | `Viewport`                        | viewport entity   | paper-space viewport |
|  [13]   | `DxfVersionNotSupportedException` | read failure      | unsupported version  |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml document packages
- rail: drafting

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]            |
| :-----: | :--------------------------- | :------------- | :---------------- |
|   [1]   | `WordprocessingDocument`     | Word document  | docx package root |
|   [2]   | `SpreadsheetDocument`        | Excel document | xlsx package root |
|   [3]   | `PresentationDocument`       | PowerPoint doc | pptx package root |
|   [4]   | `WordprocessingDocumentType` | document type  | Word format enum  |
|   [5]   | `SpreadsheetDocumentType`    | document type  | Excel format enum |
|   [6]   | `PresentationDocumentType`   | document type  | PPT format enum   |
|   [7]   | `OpenXmlPackage`             | package base   | package root base |

[PUBLIC_TYPE_SCOPE]: DocumentFormat.OpenXml part and content-element family
- rail: drafting
- namespace: `DocumentFormat.OpenXml.Packaging` (parts), `DocumentFormat.OpenXml.Spreadsheet`, `DocumentFormat.OpenXml.Wordprocessing`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]          | [RAIL]                                 |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------- |
|   [1]   | `WorkbookPart`               | part                   | xlsx workbook part (AddWorkbookPart)   |
|   [2]   | `WorksheetPart`              | part                   | xlsx sheet part (AddNewPart)           |
|   [3]   | `MainDocumentPart`           | part                   | docx body part (AddMainDocumentPart)   |
|   [4]   | `FontTablePart`              | part                   | embedded-font part (GetStream)         |
|   [5]   | `Workbook` / `Sheets`        | content element        | xlsx workbook + sheet registry         |
|   [6]   | `Sheet`                      | content element        | sheet registry entry (Id/SheetId/Name) |
|   [7]   | `Worksheet` / `SheetData`    | content element        | xlsx sheet body + row container        |
|   [8]   | `Row` / `Cell`               | content element        | xlsx row + cell                        |
|   [9]   | `CellValue` / `CellValues`   | content element + enum | cell value + data-type enum            |
|  [10]   | `Document` / `Body`          | content element        | docx document + body                   |
|  [11]   | `Paragraph` / `Run` / `Text` | content element        | docx paragraph/run/text run            |
|  [12]   | `SpaceProcessingModeValues`  | enum                   | run-text whitespace preservation       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ACadSharp read and write operations
- rail: drafting

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :------------------------------------------------- | :------------- | :------------------ |
|   [1]   | `DwgReader(string, config)`                        | `DwgReader`    | DWG stream open     |
|   [2]   | `DwgReader(Stream, config)`                        | `DwgReader`    | DWG stream open     |
|   [3]   | `Read()`                                           | `DwgReader`    | DWG document parse  |
|   [4]   | `DxfReader(string, config)`                        | `DxfReader`    | DXF stream open     |
|   [5]   | `DxfReader(Stream, config)`                        | `DxfReader`    | DXF stream open     |
|   [6]   | `Read()`                                           | `DxfReader`    | DXF document parse  |
|   [7]   | `Write(string, CadDocument, ...)`                  | `DwgWriter`    | static DWG emit     |
|   [8]   | `Write(Stream, CadDocument, ...)`                  | `DwgWriter`    | static DWG emit     |
|   [9]   | `Write(string, CadDocument, ...)`                  | `DxfWriter`    | static DXF emit     |
|  [10]   | `Write(Stream, CadDocument, ...)`                  | `DxfWriter`    | static DXF emit     |
|  [11]   | `Write()`                                          | `DxfWriter`    | instance DXF emit   |
|  [12]   | `DwgWriter(string, CadDocument)`                   | `DwgWriter`    | writer construction |
|  [13]   | `DxfWriter(string, CadDocument, binary)`           | `DxfWriter`    | writer construction |
|  [14]   | `SvgWriter(string, CadDocument, SvgConfiguration)` | `SvgWriter`    | SVG emit            |

[ENTRYPOINT_SCOPE]: netDxf document round-trip operations
- rail: drafting

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------------------------- | :------------- | :------------------ |
|   [1]   | `DxfDocument.Load(string)`         | `DxfDocument`  | file load           |
|   [2]   | `DxfDocument.Load(Stream)`         | `DxfDocument`  | stream load         |
|   [3]   | `Save(string)`                     | `DxfDocument`  | text file emit      |
|   [4]   | `Save(string, isBinary)`           | `DxfDocument`  | binary/text emit    |
|   [5]   | `Save(Stream)`                     | `DxfDocument`  | stream emit         |
|   [6]   | `DxfDocument(DxfVersion)`          | `DxfDocument`  | versioned construct |
|   [7]   | `Entities.*` collections           | `DxfDocument`  | entity access       |
|   [8]   | `Layers` / `TextStyles` / `Blocks` | `DxfDocument`  | table access        |

[ENTRYPOINT_SCOPE]: DocumentFormat.OpenXml package factory operations
- rail: drafting

| [INDEX] | [SURFACE]                                    | [SURFACE_ROOT]           | [RAIL]           |
| :-----: | :------------------------------------------- | :----------------------- | :--------------- |
|   [1]   | `Create(string, WordprocessingDocumentType)` | `WordprocessingDocument` | create docx      |
|   [2]   | `Create(Stream, WordprocessingDocumentType)` | `WordprocessingDocument` | stream create    |
|   [3]   | `Open(string, isEditable)`                   | `WordprocessingDocument` | open docx        |
|   [4]   | `Open(Stream, isEditable)`                   | `WordprocessingDocument` | stream open      |
|   [5]   | `CreateFromTemplate(string)`                 | `WordprocessingDocument` | template create  |
|   [6]   | `Create(string, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | create xlsx      |
|   [7]   | `Create(Stream, SpreadsheetDocumentType)`    | `SpreadsheetDocument`    | stream create    |
|   [8]   | `Open(string, isEditable)`                   | `SpreadsheetDocument`    | open xlsx        |
|   [9]   | `Open(Stream, isEditable)`                   | `SpreadsheetDocument`    | stream open      |
|  [10]   | `Create(string, PresentationDocumentType)`   | `PresentationDocument`   | create pptx      |
|  [11]   | `Open(string, isEditable)`                   | `PresentationDocument`   | open pptx        |
|  [12]   | `Save` / `Dispose`                           | `OpenXmlPackage`         | commit and close |

[ENTRYPOINT_SCOPE]: OpenXml part-add and content-build operations
- rail: drafting

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]                 | [RAIL]                |
| :-----: | :--------------------------------------------------- | :----------------------------- | :-------------------- |
|   [1]   | `AddWorkbookPart()`                                  | `SpreadsheetDocument`          | workbook part create  |
|   [2]   | `AddNewPart<WorksheetPart>()`                        | `WorkbookPart`                 | sheet part create     |
|   [3]   | `GetIdOfPart(part)`                                  | `WorkbookPart`                 | relationship-id query |
|   [4]   | `AddMainDocumentPart()`                              | `WordprocessingDocument`       | docx body part create |
|   [5]   | `AddNewPart<FontTablePart>()` / `GetStream()`        | `WorkbookPart`/`FontTablePart` | embedded-font pack    |
|   [6]   | `AppendChild(element)` / `Append(elements)`          | content element                | child-element insert  |
|   [7]   | `part.Workbook` / `part.Worksheet` / `part.Document` | part                           | root-element assign   |

## [4]-[IMPLEMENTATION_LAW]

[DRAFTING_TOPOLOGY]:
- `ACadSharp`: 706 types across 35 namespaces; `CadDocument` is the document root; `ACadSharp.IO` owns all read and write paths; `ACadSharp.Entities` covers the geometry entity roster; `ACadSharp.Tables` covers layer, linetype, style, and block-record entries
- `netDxf`: 365 types across 10 namespaces; `DxfDocument` is the document root with static `Load` factory and instance `Save`; read/write is self-contained through `DxfDocument.Load`/`Save`; `DxfDocument(Header.DxfVersion)` selects the output version, geometry points are `netDxf.Vector2`/`Vector3`, `netDxf.Entities.Line`/`MText` are the line and text entities, and `netDxf.Tables.Layer`/`Linetype` (with the `Linetype.Continuous`/`Dashed` singletons) carry the layer structure
- `ACadSharp` geometry points are `CSMath.XYZ`, entity layers attach through the entity `Layer` property bound to a `Layer` table entry (`LineType.Continuous`/`Dashed` linetype singletons), `MText` carries `Value`/`InsertPoint`/`Height`, and the document `Entities`/`Layers` collections take typed entities through `Add`
- `DocumentFormat.OpenXml`: 5210 types across 140 namespaces; `Packaging` owns the three document roots; `Wordprocessing`, `Spreadsheet`, and `Presentation` namespaces supply the open content element trees

[LOCAL_ADMISSION]:
- `ACadSharp` owns DWG authorship and round-trip; `netDxf` owns DXF authorship and round-trip; they are not interchangeable — `CadDocument` and `DxfDocument` are independent models.
- `DocumentFormat.OpenXml` package documents are disposable; every open or create path pairs with `Save`/`Dispose` or a `using` scope.
- Entity construction flows through the entity type constructor, then collection `Add`; never bypass typed entity APIs with raw group-code writes.
- Configuration objects (`DwgReaderConfiguration`, `DxfWriterConfiguration`) scope read/write posture; pass them at construction, not post-hoc.
- OOXML part-graph construction flows root-first: `Create(Stream, type)` mints the package, `AddWorkbookPart`/`AddMainDocumentPart` mints the root part, the part's root element (`Workbook`/`Document`) is assigned, child parts (`WorksheetPart` via `AddNewPart`, `FontTablePart` for embedded faces) attach under it, content elements (`Sheets`/`Sheet`/`SheetData`/`Row`/`Cell`, `Body`/`Paragraph`/`Run`/`Text`) append through `Append`/`AppendChild`, and `Save` on the root element plus the `using` package dispose commits the byte stream; `GetIdOfPart` supplies the relationship id a `Sheet` registry entry binds, never a hand-written `rId`.

[RAIL_LAW]:
- Package: `ACadSharp` — owns DWG/DXF CAD format read/write
- Package: `netDxf` — owns DXF-only CAD format read/write with a simpler API surface
- Package: `DocumentFormat.OpenXml` — owns OOXML (docx/xlsx/pptx) package authoring
- Accept: all drafting export flows through typed document roots and their IO entry points
- Reject: hand-rolled binary DWG/DXF writers or raw ZIP/XML manipulation of OOXML packages
