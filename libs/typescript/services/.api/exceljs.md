# [API_CATALOGUE] exceljs

`exceljs` supplies `Workbook`, `Worksheet`, `Row`, `Column`, `Cell`, and associated style, value, and formula types for reading and writing XLSX and CSV spreadsheet files, with full streaming writer and reader variants via `stream.WorkbookWriter` and `stream.WorkbookReader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `exceljs`
- package: `exceljs`
- namespace: `exceljs` (`index.d.ts`)
- asset: runtime library
- rail: spreadsheet

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and worksheet
- rail: spreadsheet

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Workbook`            | class         | root workbook model and IO entry point          |
|  [02]   | `Worksheet`           | interface     | single sheet with rows, columns, cells          |
|  [03]   | `WorkbookModel`       | interface     | serialization model for a workbook              |
|  [04]   | `WorksheetModel`      | interface     | serialization model for a worksheet             |
|  [05]   | `WorksheetProperties` | interface     | tab color, outline level, default row/col sizes |
|  [06]   | `WorksheetState`      | type          | `'visible' \| 'hidden' \| 'veryHidden'`         |
|  [07]   | `WorkbookProperties`  | interface     | date1904, filterPrivacy, defaultFont            |
|  [08]   | `WorkbookView`        | interface     | window position and active tab                  |
|  [09]   | `Xlsx`                | interface     | XLSX read/write operations                      |
|  [10]   | `Csv`                 | interface     | CSV read/write operations                       |

[PUBLIC_TYPE_SCOPE]: row, column, cell
- rail: spreadsheet

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [RAIL]                                                                                                                                                                |
| :-----: | :---------- | :------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Row`       | interface     | worksheet row with cell access and iteration                                                                                                                          |
|  [02]   | `Column`    | interface     | worksheet column with header, key, width                                                                                                                              |
|  [03]   | `Cell`      | interface     | individual cell value, style, and formula                                                                                                                             |
|  [04]   | `CellValue` | type          | `null \| number \| string \| boolean \| Date \| CellErrorValue \| CellRichTextValue \| CellHyperlinkValue \| CellFormulaValue \| CellSharedFormulaValue \| undefined` |
|  [05]   | `RowValues` | type          | `CellValue[] \| { [key: string]: CellValue } \| undefined \| null`                                                                                                    |
|  [06]   | `RowModel`  | interface     | serialization shape for a row                                                                                                                                         |
|  [07]   | `CellModel` | interface     | serialization shape for a cell                                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: value and formula types
- rail: spreadsheet

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `ValueType`              | enum          | `Null(0)`, `Number(2)`, `String(3)`, `Date(4)`, `Formula(6)`, `RichText(8)`, `Boolean(9)`, `Error(10)`, … |
|  [02]   | `FormulaType`            | enum          | `None(0)`, `Master(1)`, `Shared(2)`                                                                       |
|  [03]   | `ErrorValue`             | enum          | `#N/A`, `#REF!`, `#NAME?`, `#DIV/0!`, `#NULL!`, `#VALUE!`, `#NUM!`                                        |
|  [04]   | `CellErrorValue`         | interface     | `error` string discriminant                                                                               |
|  [05]   | `CellRichTextValue`      | interface     | `richText: RichText[]`                                                                                    |
|  [06]   | `CellHyperlinkValue`     | interface     | `text`, `hyperlink`, `tooltip?`                                                                           |
|  [07]   | `CellFormulaValue`       | interface     | `formula`, `result?`, `date1904?`                                                                         |
|  [08]   | `CellSharedFormulaValue` | interface     | `sharedFormula`, `formula?`, `result?`                                                                    |

[PUBLIC_TYPE_SCOPE]: style types
- rail: spreadsheet

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------ | :------------ | :------------------------------------------------------------ |
|  [01]   | `Style`       | interface     | `numFmt`, `font`, `alignment`, `protection`, `border`, `fill` |
|  [02]   | `Font`        | interface     | name, size, bold, italic, underline, color                    |
|  [03]   | `Alignment`   | interface     | horizontal, vertical, wrapText, textRotation                  |
|  [04]   | `Border`      | interface     | `style: BorderStyle`, `color`                                 |
|  [05]   | `Borders`     | interface     | top, left, bottom, right, diagonal borders                    |
|  [06]   | `Fill`        | type          | `FillPattern \| FillGradientAngle \| FillGradientPath`        |
|  [07]   | `Color`       | interface     | `argb: string`, `theme: number`                               |
|  [08]   | `BorderStyle` | type          | `'thin' \| 'dotted' \| 'medium' \| ...`                       |
|  [09]   | `Protection`  | interface     | `locked`, `hidden`                                            |

[PUBLIC_TYPE_SCOPE]: page setup, images, streaming
- rail: spreadsheet

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :---------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `PageSetup`             | interface     | orientation, margins, paper size, print area   |
|  [02]   | `HeaderFooter`          | interface     | odd/even/first page header and footer          |
|  [03]   | `Image`                 | interface     | `extension`, `base64?`, `filename?`, `buffer?` |
|  [04]   | `stream.WorkbookWriter` | class         | streaming XLSX writer                          |
|  [05]   | `stream.WorkbookReader` | class         | streaming XLSX reader                          |
|  [06]   | `XlsxReadOptions`       | interface     | XLSX read options                              |
|  [07]   | `XlsxWriteOptions`      | interface     | XLSX write options                             |
|  [08]   | `CsvReadOptions`        | interface     | CSV read options                               |
|  [09]   | `CsvWriteOptions`       | interface     | CSV write options                              |
|  [10]   | `DocumentType`          | enum          | `Xlsx = 1`                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Workbook construction and IO
- rail: spreadsheet

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :---------------------------------------- | :------------- | :----------------------- |
|  [01]   | `new Workbook()`                          | constructor    | empty workbook           |
|  [02]   | `workbook.xlsx.readFile(path, options?)`  | read           | load XLSX from file      |
|  [03]   | `workbook.xlsx.read(stream, options?)`    | read           | load XLSX from stream    |
|  [04]   | `workbook.xlsx.load(buffer, options?)`    | read           | load XLSX from Buffer    |
|  [05]   | `workbook.xlsx.writeFile(path, options?)` | write          | save XLSX to file        |
|  [06]   | `workbook.xlsx.writeBuffer(options?)`     | write          | serialize XLSX to Buffer |
|  [07]   | `workbook.xlsx.write(stream, options?)`   | write          | stream XLSX to writable  |
|  [08]   | `workbook.csv.readFile(path, options?)`   | read           | load CSV from file       |
|  [09]   | `workbook.csv.writeFile(path, options?)`  | write          | save CSV to file         |
|  [10]   | `workbook.csv.writeBuffer(options?)`      | write          | serialize CSV to Buffer  |

[ENTRYPOINT_SCOPE]: Workbook sheet management
- rail: spreadsheet

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `workbook.addWorksheet(name?, options?)` | factory        | add and return a new Worksheet   |
|  [02]   | `workbook.getWorksheet(indexOrName?)`    | lookup         | returns `Worksheet \| undefined` |
|  [03]   | `workbook.removeWorksheet(indexOrName)`  | mutation       | remove sheet by name or index    |
|  [04]   | `workbook.eachSheet(callback)`           | iteration      | iterate all sheets               |
|  [05]   | `workbook.addImage(img)`                 | media          | embed image, returns id          |

[ENTRYPOINT_SCOPE]: Worksheet row and cell access
- rail: spreadsheet

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `worksheet.getRow(index)`         | row access     | get or create 1-based row            |
|  [02]   | `worksheet.addRow(data, style?)`  | row add        | append row from array or object      |
|  [03]   | `worksheet.addRows(rows, style?)` | row add        | append multiple rows                 |
|  [04]   | `worksheet.getCell(r, c?)`        | cell access    | get or create cell by address        |
|  [05]   | `worksheet.eachRow(callback)`     | iteration      | iterate non-empty rows               |
|  [06]   | `worksheet.getColumn(indexOrKey)` | column access  | get column by letter, number, or key |
|  [07]   | `worksheet.mergeCells(...)`       | layout         | merge a cell range                   |

## [04]-[IMPLEMENTATION_LAW]

[EXCEL_TOPOLOGY]:
- namespace: `exceljs`; `Workbook` is the sole root; `xlsx` and `csv` are sub-services accessed as `workbook.xlsx` / `workbook.csv`
- row/column/cell indices are 1-based throughout the API
- `stream.WorkbookWriter` must call `worksheet.commit()` and `workbook.commit()` to flush; omitting either silently drops rows
- images are embedded as base64, filename path, or `Buffer` via `workbook.addImage(img)` and positioned with `worksheet.addImage(id, range)`

[LOCAL_ADMISSION]:
- Use `xlsx.writeBuffer()` for in-memory serialization (HTTP responses, S3 uploads); `xlsx.writeFile()` for disk.
- `CellFormulaValue.result` holds the last-calculated value from a read file; it is not recalculated on write.
- `WorksheetState: 'veryHidden'` hides a sheet from the Excel UI and the sheet list; it is not password-protected.

[RAIL_LAW]:
- Package: `exceljs`
- Owns: XLSX and CSV read/write, workbook/worksheet/cell model, styles, formulas, images, and streaming IO
- Accept: `Buffer`, file path, `Stream` for read; `Buffer`, file path, `Stream` for write
- Reject: hand-rolled XLSX zip/XML assembly or direct `fast-csv` calls when this package is in use
