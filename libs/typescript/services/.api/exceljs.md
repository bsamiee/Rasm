# [API_CATALOGUE] exceljs

`exceljs` supplies `Workbook`, `Worksheet`, `Row`, `Column`, `Cell`, and associated style, value, and formula types for reading and writing XLSX and CSV spreadsheet files, with full streaming writer and reader variants via `stream.WorkbookWriter` and `stream.WorkbookReader`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `exceljs`
- package: `exceljs`
- namespace: `exceljs` (`index.d.ts`)
- asset: runtime library
- rail: spreadsheet

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and worksheet
- rail: spreadsheet

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `Workbook`            | class         | root workbook model and IO entry point          |
|   [2]   | `Worksheet`           | interface     | single sheet with rows, columns, cells          |
|   [3]   | `WorkbookModel`       | interface     | serialization model for a workbook              |
|   [4]   | `WorksheetModel`      | interface     | serialization model for a worksheet             |
|   [5]   | `WorksheetProperties` | interface     | tab color, outline level, default row/col sizes |
|   [6]   | `WorksheetState`      | type          | `'visible' \| 'hidden' \| 'veryHidden'`         |
|   [7]   | `WorkbookProperties`  | interface     | date1904, filterPrivacy, defaultFont            |
|   [8]   | `WorkbookView`        | interface     | window position and active tab                  |
|   [9]   | `Xlsx`                | interface     | XLSX read/write operations                      |
|  [10]   | `Csv`                 | interface     | CSV read/write operations                       |

[PUBLIC_TYPE_SCOPE]: row, column, cell
- rail: spreadsheet

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [RAIL]                                                                                                                                                                |
| :-----: | :---------- | :------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Row`       | interface     | worksheet row with cell access and iteration                                                                                                                          |
|   [2]   | `Column`    | interface     | worksheet column with header, key, width                                                                                                                              |
|   [3]   | `Cell`      | interface     | individual cell value, style, and formula                                                                                                                             |
|   [4]   | `CellValue` | type          | `null \| number \| string \| boolean \| Date \| CellErrorValue \| CellRichTextValue \| CellHyperlinkValue \| CellFormulaValue \| CellSharedFormulaValue \| undefined` |
|   [5]   | `RowValues` | type          | `CellValue[] \| { [key: string]: CellValue } \| undefined \| null`                                                                                                    |
|   [6]   | `RowModel`  | interface     | serialization shape for a row                                                                                                                                         |
|   [7]   | `CellModel` | interface     | serialization shape for a cell                                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: value and formula types
- rail: spreadsheet

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | `ValueType`              | enum          | `Null(0)`, `Number(2)`, `String(3)`, `Date(4)`, `Formula(6)`, `RichText(8)`, `Boolean(9)`, `Error(10)`, … |
|   [2]   | `FormulaType`            | enum          | `None(0)`, `Master(1)`, `Shared(2)`                                                                       |
|   [3]   | `ErrorValue`             | enum          | `#N/A`, `#REF!`, `#NAME?`, `#DIV/0!`, `#NULL!`, `#VALUE!`, `#NUM!`                                        |
|   [4]   | `CellErrorValue`         | interface     | `error` string discriminant                                                                               |
|   [5]   | `CellRichTextValue`      | interface     | `richText: RichText[]`                                                                                    |
|   [6]   | `CellHyperlinkValue`     | interface     | `text`, `hyperlink`, `tooltip?`                                                                           |
|   [7]   | `CellFormulaValue`       | interface     | `formula`, `result?`, `date1904?`                                                                         |
|   [8]   | `CellSharedFormulaValue` | interface     | `sharedFormula`, `formula?`, `result?`                                                                    |

[PUBLIC_TYPE_SCOPE]: style types
- rail: spreadsheet

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------ | :------------ | :------------------------------------------------------------ |
|   [1]   | `Style`       | interface     | `numFmt`, `font`, `alignment`, `protection`, `border`, `fill` |
|   [2]   | `Font`        | interface     | name, size, bold, italic, underline, color                    |
|   [3]   | `Alignment`   | interface     | horizontal, vertical, wrapText, textRotation                  |
|   [4]   | `Border`      | interface     | `style: BorderStyle`, `color`                                 |
|   [5]   | `Borders`     | interface     | top, left, bottom, right, diagonal borders                    |
|   [6]   | `Fill`        | type          | `FillPattern \| FillGradientAngle \| FillGradientPath`        |
|   [7]   | `Color`       | interface     | `argb: string`, `theme: number`                               |
|   [8]   | `BorderStyle` | type          | `'thin' \| 'dotted' \| 'medium' \| ...`                       |
|   [9]   | `Protection`  | interface     | `locked`, `hidden`                                            |

[PUBLIC_TYPE_SCOPE]: page setup, images, streaming
- rail: spreadsheet

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :---------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `PageSetup`             | interface     | orientation, margins, paper size, print area   |
|   [2]   | `HeaderFooter`          | interface     | odd/even/first page header and footer          |
|   [3]   | `Image`                 | interface     | `extension`, `base64?`, `filename?`, `buffer?` |
|   [4]   | `stream.WorkbookWriter` | class         | streaming XLSX writer                          |
|   [5]   | `stream.WorkbookReader` | class         | streaming XLSX reader                          |
|   [6]   | `XlsxReadOptions`       | interface     | XLSX read options                              |
|   [7]   | `XlsxWriteOptions`      | interface     | XLSX write options                             |
|   [8]   | `CsvReadOptions`        | interface     | CSV read options                               |
|   [9]   | `CsvWriteOptions`       | interface     | CSV write options                              |
|  [10]   | `DocumentType`          | enum          | `Xlsx = 1`                                     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Workbook construction and IO
- rail: spreadsheet

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :---------------------------------------- | :------------- | :----------------------- |
|   [1]   | `new Workbook()`                          | constructor    | empty workbook           |
|   [2]   | `workbook.xlsx.readFile(path, options?)`  | read           | load XLSX from file      |
|   [3]   | `workbook.xlsx.read(stream, options?)`    | read           | load XLSX from stream    |
|   [4]   | `workbook.xlsx.load(buffer, options?)`    | read           | load XLSX from Buffer    |
|   [5]   | `workbook.xlsx.writeFile(path, options?)` | write          | save XLSX to file        |
|   [6]   | `workbook.xlsx.writeBuffer(options?)`     | write          | serialize XLSX to Buffer |
|   [7]   | `workbook.xlsx.write(stream, options?)`   | write          | stream XLSX to writable  |
|   [8]   | `workbook.csv.readFile(path, options?)`   | read           | load CSV from file       |
|   [9]   | `workbook.csv.writeFile(path, options?)`  | write          | save CSV to file         |
|  [10]   | `workbook.csv.writeBuffer(options?)`      | write          | serialize CSV to Buffer  |

[ENTRYPOINT_SCOPE]: Workbook sheet management
- rail: spreadsheet

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `workbook.addWorksheet(name?, options?)` | factory        | add and return a new Worksheet   |
|   [2]   | `workbook.getWorksheet(indexOrName?)`    | lookup         | returns `Worksheet \| undefined` |
|   [3]   | `workbook.removeWorksheet(indexOrName)`  | mutation       | remove sheet by name or index    |
|   [4]   | `workbook.eachSheet(callback)`           | iteration      | iterate all sheets               |
|   [5]   | `workbook.addImage(img)`                 | media          | embed image, returns id          |

[ENTRYPOINT_SCOPE]: Worksheet row and cell access
- rail: spreadsheet

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `worksheet.getRow(index)`         | row access     | get or create 1-based row            |
|   [2]   | `worksheet.addRow(data, style?)`  | row add        | append row from array or object      |
|   [3]   | `worksheet.addRows(rows, style?)` | row add        | append multiple rows                 |
|   [4]   | `worksheet.getCell(r, c?)`        | cell access    | get or create cell by address        |
|   [5]   | `worksheet.eachRow(callback)`     | iteration      | iterate non-empty rows               |
|   [6]   | `worksheet.getColumn(indexOrKey)` | column access  | get column by letter, number, or key |
|   [7]   | `worksheet.mergeCells(...)`       | layout         | merge a cell range                   |

## [4]-[IMPLEMENTATION_LAW]

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
