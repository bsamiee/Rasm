# [RASM_APPUI_API_DRAFTING_EXPORT]

`DocumentFormat.OpenXml` supplies AppUi's document-export pipeline, authoring OOXML docx/xlsx/pptx through the `WordprocessingDocument`/`SpreadsheetDocument`/`PresentationDocument` part graph. The CAD drafting-write leg beside it composes `ACadSharp` — AppUi holds CAD WRITE authority alone, one authored `CadDocument` folded to DWG, DXF, and SVG — and that package's member truth is the branch substrate catalogue (`libs/dotnet/.api/api-acadsharp.md`); this file registers the write leg and carries the OOXML surface.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- OOXML authoring flows root-first over one disposable package: `Create` mints it, `AddWorkbookPart`/`AddMainDocumentPart` mints the root part, `GetIdOfPart` supplies the relationship id a `Sheet` binds, content appends through `Append`/`AppendChild`, and `Save` under a `using` scope commits the byte stream.

[STACKING]:
- `ACadSharp`(`libs/dotnet/.api/api-acadsharp.md`): `Render/drafting.md` composes the DWG+DXF two-format write leg over one `CadDocument` populated from `ACadSharp.Entities` and `ACadSharp.Tables`; the branch catalogue owns the member truth of that surface.
- `Document/export.md`: composes the OOXML part-graph arm through the three `DocumentFormat.OpenXml` document roots.

[LOCAL_ADMISSION]:
- AppUi emits a CAD file, never opens one — READ belongs to the Bim and Fabrication boundaries.
- OOXML packages are disposable: every create path pairs with `Save`/`Dispose` or a `using` scope, and part construction rides typed part/element APIs.
