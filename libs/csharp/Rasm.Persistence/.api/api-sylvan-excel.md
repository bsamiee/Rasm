# [RASM_PERSISTENCE_API_SYLVAN_EXCEL]

`Sylvan.Data.Excel` is a streaming spreadsheet codec shaped as ADO.NET: `ExcelDataReader`
subclasses `DbDataReader` (and implements `IDbColumnSchemaGenerator`), reading `.xlsx`/`.xlsb`/
`.xls` forward-only over one workbook with multi-worksheet navigation (`WorksheetNames`/
`TryOpenWorksheet`/`NextResult`), the full typed accessor surface (`GetInt32`/`GetDouble`/
`GetDateTime`/`GetGuid`/`GetFieldValue<T>` + the Excel-specific `GetExcelDataType`/
`GetExcelValue`/`GetFormulaError`/`GetFormat`/`GetTimeSpan`), a pluggable `IExcelSchemaProvider`
binding cell ordinals to typed `DbColumn` schema, and `FormulaErrorHandling` for error-cell
policy. `ExcelDataWriter` is the egress codec — `Write(DbDataReader, worksheetName)` streams any
reader into a new `.xlsx`/`.xlsb` worksheet with a `WriteResult` row count. It is the spreadsheet
boundary the `Sep` delimited-only lane cannot reach: both project into managed ADO.NET rows, so a
spreadsheet ingress composes the identical `Enumerate`/typed-parse/bulk-copy rail as a CSV
ingress, feeding the NodaTime/Thinktecture wire converters and the EF/linq2db bulk path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sylvan.Data.Excel`
- package: `Sylvan.Data.Excel`
- version: `0.5.6`
- license: MIT (Mark Pflug)
- assembly: `Sylvan.Data.Excel`
- namespace: `Sylvan.Data.Excel` (public codec); `Sylvan.Data` (`SchemaTable`); the `Xls`/`Xlsx`/`Xlsb` format readers/writers and OLE2/OpenXml packaging are internal implementation
- target: multi-target (`net6.0`, `netstandard2.0`, `netstandard2.1`); the `net10.0` consumer binds `lib/net6.0` (highest available)
- asset: pure-managed runtime library, AnyCPU, no native runtime; the `net6.0` asset has zero package dependencies (self-contained xlsx/xlsb/xls + OLE2 parser)
- abi: `DbDataReader`/`DbColumn`/`IDbColumnSchemaGenerator` ADO.NET surface; readers are `IDisposable`, writers are `IDisposable`+`IAsyncDisposable`; no `IBufferWriter`/`Span` row API (it is a `DbDataReader`, not a span codec)
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[READER_TYPES]: ingress reader + options + schema (namespace `Sylvan.Data.Excel`)
- rail: interchange-codec

`ExcelDataReader` is `abstract` — the `Create`/`CreateAsync` factories return the format-bound
concrete reader (xlsx/xlsb/xls). It is forward-only over the active worksheet; `NextResult`/
`TryOpenWorksheet` move between sheets in one workbook.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                                  |
| :-----: | :------------------------ | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ExcelDataReader`         | ingress reader root | `: DbDataReader, IDbColumnSchemaGenerator`; `Create`/`CreateAsync`, typed getters, `GetExcelDataType`/`GetExcelValue`/`GetFormulaError`/`GetFormat`, worksheet navigation, `GetColumnSchema` |
|  [02]   | `ExcelDataReaderOptions`  | reader policy bag   | `Schema`, `Culture`, `FormulaErrorHandling`, `GetErrorAsNull`, `IgnoreEmptyTrailingRows`, `ReadHiddenWorksheets`/`ReadHiddenRows`, `TrueString`/`FalseString`, `DateTimeFormat`, `OwnsStream` |
|  [03]   | `IExcelSchemaProvider`    | schema hook         | `GetColumn(string sheetName, string? name, int ordinal) -> DbColumn?` + `HasHeaders(sheetName)` |
|  [04]   | `ExcelSchema`             | schema provider     | `: ExcelSchemaProvider`; static `Default`/`NoHeaders`/`Dynamic`/`DynamicNoHeaders`; builder `Add(sheetName, hasHeaders, columns)` |

[WRITER_TYPES]: egress writer + options (namespace `Sylvan.Data.Excel`)
- rail: interchange-codec

`ExcelDataWriter` is `abstract`; the factory binds the `.xlsx`/`.xlsb` writer. It writes any
`DbDataReader` — including an `ExcelDataReader`, a `Sep` reader projection, or an EF/DuckDB
reader — into a worksheet.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                                  |
| :-----: | :------------------------ | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ExcelDataWriter`         | egress writer root  | `: IDisposable, IAsyncDisposable`; `Create`/`CreateAsync`, `Write`/`WriteAsync(DbDataReader, worksheetName)`, `MaxColumnCount`/`MaxRowCount` |
|  [02]   | `ExcelDataWriter.WriteResult` | write receipt   | `readonly struct`; `RowsWritten`, `IsComplete` (false = hit the per-sheet row cap) |
|  [03]   | `ExcelDataWriterOptions`  | writer policy bag   | `CompressionLevel` (`System.IO.Compression.CompressionLevel` of the xlsx zip), `TruncateStrings`, `OwnsStream` |

[CLASSIFICATION_ENUMS]: workbook/cell/error vocabulary
- rail: interchange-codec

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]      | [CAPABILITY]                                                                  |
| :-----: | :---------------------- | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ExcelWorkbookType`     | format discriminant | `Unknown` / `Excel` (.xls) / `ExcelXml` (.xlsx) / `ExcelBinary` (.xlsb) — the `Create(stream, type)` selector |
|  [02]   | `ExcelDataType`         | cell value kind     | `Null` / `Numeric` / `DateTime` / `String` / `Boolean` / `Error` (the `GetExcelDataType` result; numeric cells may carry a date/time format) |
|  [03]   | `FormulaErrorHandling`  | error-cell policy   | `Exception` (default — throw on access) / `Null` (read as empty) / `String` (read as `"#DIV/0!"` etc.) |
|  [04]   | `ExcelErrorCode`        | formula error code  | `Null`/`DivideByZero`/`Value`/`Reference`/`Name`/`Number`/`NotAvailable` — on `GetFormulaError` / `ExcelFormulaException` |
|  [05]   | `ExcelFileType`         | extension catalog   | static `Excel`/`ExcelXml`/`ExcelXmlMacroEnabled`/`ExcelBinary` rows; `FindForFilename`, `ReaderSupported`/`WriterSupported`; extension + content-type constants |
|  [06]   | `ExcelFormulaException` | formula fault       | thrown when `FormulaErrorHandling.Exception` and an error cell is accessed     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reader construction — sync + async mirror
- rail: interchange-codec

The `string filename` factories sniff the workbook type from the extension; the `Stream`
factories require an explicit `ExcelWorkbookType` (use `GetWorkbookType(filename)` to resolve
one). `OwnsStream` governs whether disposing the reader disposes the inner stream.

| [INDEX] | [SURFACE]                                                                          | [RETURNS]                  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------- | :------------------------------------ |
|  [01]   | `ExcelDataReader.Create(string filename, ExcelDataReaderOptions? options = null)`  | `ExcelDataReader`          | open by filename (type sniffed)       |
|  [02]   | `ExcelDataReader.Create(Stream stream, ExcelWorkbookType fileType, ExcelDataReaderOptions? options = null)` | `ExcelDataReader` | open a stream of an explicit type |
|  [03]   | `ExcelDataReader.CreateAsync(string filename, …, CancellationToken)`               | `Task<ExcelDataReader>`    | async filename open                   |
|  [04]   | `ExcelDataReader.CreateAsync(Stream, ExcelWorkbookType, …, CancellationToken)`      | `Task<ExcelDataReader>`    | async stream open                     |
|  [05]   | `ExcelDataReader.GetWorkbookType(string filename)`                                 | `ExcelWorkbookType`        | resolve the type for a stream open    |

[ENTRYPOINT_SCOPE]: row iteration + worksheet navigation
- rail: interchange-codec

`Read`/`ReadAsync` advance a row within the active worksheet; `NextResult`/`NextResultAsync`
(inherited `DbDataReader`) and `TryOpenWorksheet` move between worksheets in one workbook — the
multi-sheet seam the delimited codec has no analogue for.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]    | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `reader.Read()` / `reader.ReadAsync(CancellationToken)` | reader call | advance one row in the active worksheet            |
|  [02]   | `reader.NextResult()` / `NextResultAsync(…)`       | reader call     | advance to the next worksheet                      |
|  [03]   | `reader.TryOpenWorksheet(string name)`             | reader call     | jump to a named worksheet (`bool`)                 |
|  [04]   | `reader.WorksheetNames` / `WorksheetCount` / `WorksheetName` | reader prop | sheet roster / count / active sheet name      |
|  [05]   | `reader.RowNumber` / `RowCount` / `RowFieldCount` / `IsRowHidden` | reader prop | physical row position / count / live width / hidden flag |

[ENTRYPOINT_SCOPE]: typed cell access (ADO.NET + Excel extensions)
- rail: interchange-codec

The inherited `DbDataReader` getters (`GetInt32`/`GetDouble`/`GetDecimal`/`GetString`/
`GetGuid`/`GetBoolean`/`GetFieldValue<T>`/`IsDBNull`) plus the Excel-specific surface that a
generic reader lacks: `GetExcelDataType` (raw cell kind before coercion), `GetExcelValue`
(the unconverted value), `GetFormat` (the cell's number format), `GetFormulaError`,
`GetTimeSpan` (Excel stores durations as serial numbers).

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]    | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `reader.GetFieldValue<T>(int ordinal)` / `GetValue` / `GetValues(object[])` | reader call | typed / boxed / bulk row value retrieval  |
|  [02]   | `reader.GetInt32/GetInt64/GetDouble/GetDecimal/GetString/GetBoolean/GetGuid(int)` | reader call | typed ADO.NET getters             |
|  [03]   | `reader.GetDateTime(int)` / `reader.GetTimeSpan(int)` | reader call  | date / duration from the Excel serial number       |
|  [04]   | `reader.GetExcelDataType(int)` -> `ExcelDataType`  | reader call     | raw cell kind (`Numeric`/`DateTime`/`String`/`Boolean`/`Error`/`Null`) |
|  [05]   | `reader.GetExcelValue(int)` / `reader.GetFormat(int)` -> `ExcelFormat?` | reader call | unconverted value / cell number format     |
|  [06]   | `reader.GetFormulaError(int)` -> `ExcelErrorCode`  | reader call     | error code of an error cell                        |
|  [07]   | `reader.GetOrdinal(string)` / `GetName(int)` / `GetFieldType(int)` | reader call | header name <-> ordinal, declared field type   |
|  [08]   | `reader.GetColumnSchema()` -> `ReadOnlyCollection<DbColumn>` / `GetSchemaTable()` | reader call | typed column schema (ADO.NET)        |

[ENTRYPOINT_SCOPE]: schema binding (`IExcelSchemaProvider`)
- rail: interchange-codec

`ExcelDataReaderOptions.Schema` is the typed-ingest seam: a provider maps each `(sheet, name,
ordinal)` to a `DbColumn` with a CLR type, so a `Numeric` cell is exposed as `Int32`/`Decimal`/
`DateTime` directly. The static `ExcelSchema` providers cover the common cases; `ExcelSchema.Add`
builds a per-sheet typed schema.

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]    | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `ExcelSchema.Default` / `NoHeaders`                                | static provider | all-string columns, header / no-header             |
|  [02]   | `ExcelSchema.Dynamic` / `DynamicNoHeaders`                         | static provider | `object` columns inferred per cell                 |
|  [03]   | `new ExcelSchema(bool hasHeaders, IEnumerable<DbColumn> columns)`  | constructor     | explicit typed single-sheet schema                 |
|  [04]   | `excelSchema.Add(string sheetName, bool hasHeaders, IEnumerable<DbColumn> columns)` | builder | per-sheet typed schema in one provider     |
|  [05]   | `IExcelSchemaProvider.GetColumn(sheetName, name, ordinal)`         | hook            | custom ordinal -> typed `DbColumn` mapping         |

[ENTRYPOINT_SCOPE]: writer (egress codec)
- rail: interchange-codec

`Write(DbDataReader data, string? worksheetName)` streams any reader into a worksheet and
returns a `WriteResult`; `IsComplete=false` means the per-worksheet row cap (`MaxRowCount`) was
hit. Call `Write` repeatedly with distinct `worksheetName`s to author a multi-sheet workbook.

| [INDEX] | [SURFACE]                                                                  | [RETURNS]                  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------- | :------------------------------------ |
|  [01]   | `ExcelDataWriter.Create(string file, ExcelDataWriterOptions? options = null)` | `ExcelDataWriter`       | open a writer by filename             |
|  [02]   | `ExcelDataWriter.Create(Stream stream, ExcelWorkbookType type, …)` / `CreateAsync(…)` | `ExcelDataWriter`/`Task<…>` | open a writer over a stream  |
|  [03]   | `writer.Write(DbDataReader data, string? worksheetName = null)`            | `WriteResult`              | stream a reader into a worksheet      |
|  [04]   | `writer.WriteAsync(DbDataReader data, string? worksheetName, CancellationToken)` | `Task<WriteResult>`  | async worksheet write                 |
|  [05]   | `writer.DisposeAsync()` / `writer.Dispose()`                              | call                       | finalize + flush the workbook zip     |

## [04]-[IMPLEMENTATION_LAW]

[INTERCHANGE_PROFILE]:
- ingress root: `ExcelDataReader : DbDataReader` — forward-only over the active worksheet; `Create`/`CreateAsync` bind the concrete xlsx/xlsb/xls reader from `ExcelWorkbookType`.
- egress root: `ExcelDataWriter` — `Write(DbDataReader, worksheetName)` streams any reader into a `.xlsx`/`.xlsb` worksheet; `WriteResult.IsComplete` flags the `MaxRowCount` cap.
- schema seam: `ExcelDataReaderOptions.Schema` (`IExcelSchemaProvider`) maps cell ordinals to typed `DbColumn`s; without it cells read as `Numeric`/`String`/`DateTime` per `ExcelDataType` and coerce on the typed getter.
- error policy: `FormulaErrorHandling` (`Exception`/`Null`/`String`) governs an error cell on read; `GetExcelDataType`/`GetFormulaError` inspect it without triggering the policy.

[LOCAL_ADMISSION]:
- Sylvan is the spreadsheet codec for tabular interchange profiles (`Sync/schedule`, `Catalog/cost` import-export) where the source is `.xlsx`/`.xlsb`/`.xls`; profile options (`Schema`, `Culture`, `FormulaErrorHandling`, hidden-row/sheet policy) are declared on `ExcelDataReaderOptions`, never inlined ad hoc.
- the reader is consumed as a `DbDataReader` and materialized at one boundary into `IEnumerable<T>`/`IAsyncEnumerable<T>` of domain records; ordinal access uses `GetFieldValue<T>`/`GetOrdinal`, never a positional magic index, and `GetExcelDataType` guards a cell whose kind is ambiguous before a typed getter.
- formula-error cells route through `FormulaErrorHandling.Null` or `.String` (never the default `Exception`) so an error cell folds into a typed `Fin`/`Validation` failure at the row boundary instead of throwing `ExcelFormulaException` through the receipt path; `GetErrorAsNull` is the option-level twin.
- the writer streams an existing `DbDataReader` straight into a worksheet — a schedule/cost egress reuses the same reader the query/lane rail produces, so a spreadsheet export never buffers a materialized table; `ExcelDataWriterOptions.CompressionLevel` tunes the xlsx zip.

[STACKING_LAW]:
- delimited sibling: Sylvan and `Sep` (`api-sep`) are the two tabular-interchange codecs — `Sep` owns delimited text (CSV/TSV) over zero-allocation `ReadOnlySpan<char>` rows, Sylvan owns spreadsheets over `DbDataReader` rows; the profile selects one by source format, and both project into the SAME downstream record rail, so neither re-implements the other's format.
- wire converters: a typed cell feeds the NodaTime STJ converters (`api-nodatime-stj`) and the Thinktecture value-object/smart-enum factories — Sylvan reads the `DateTime`/`Numeric`/`String` cell, the wire rail validates it into the semantic-time or value-object type; an `Instant`/`LocalDate` column binds through an `IExcelSchemaProvider` `DbColumn` of the converter's CLR type, then `GetFieldValue<T>` returns it.
- bulk ingress: the `ExcelDataReader` is an `IDataReader` source for `LinqToDBForEFTools.BulkCopy`/`BulkCopyAsync` (`api-linq2db-ef`) — a spreadsheet streams into PostgreSQL binary COPY through the same bulk rail a CSV uses, because both arrive as a reader; `GetColumnSchema` supplies the typed column mapping.
- columnar edge: Sylvan is the spreadsheet codec only; Arrow/DuckDB own the binary columnar path. A spreadsheet ingress reads rows, the record rail projects them, and the columnar rail materializes the Arrow batch — Sylvan never re-implements a columnar reader, and a `.xlsx` is never treated as a columnar file.
- redaction/egress: the reader -> writer bridge re-emits a `DbDataReader` (optionally wrapped by a redacting `DbDataReader` decorator applying `Microsoft.Extensions.Compliance.Redaction` per column) into a worksheet, so a redacted spreadsheet export streams without materializing the table.
- BIM analytics frames: `Ara3D.BimOpenSchema.IO` (`api-ara3d-bimopenschema`) is the BIM analytics-frame producer whose `ClosedXML` writer (`WriteToExcel`) emits an `.xlsx` workbook with one worksheet per BIM table; this streaming `ExcelDataReader` reads those sheets back as `DbDataReader` rows into the same record rail — distinct codec (ClosedXML writer / Sylvan reader), one `.xlsx` file boundary.

[RAIL_LAW]:
- Package: `Sylvan.Data.Excel`
- Owns: spreadsheet interchange — `DbDataReader`-shaped xlsx/xlsb/xls ingress with typed + Excel-specific cell access, multi-worksheet navigation, pluggable typed schema, formula-error policy, and `DbDataReader`-sourced xlsx/xlsb egress
- Accept: profile-declared spreadsheet reads/writes; `GetFieldValue<T>` typed access guarded by `GetExcelDataType`; `IExcelSchemaProvider` typed-column binding; `FormulaErrorHandling.Null`/`.String` folded into `Fin`; any `DbDataReader` streamed into a worksheet by the writer
- Reject: hand-rolled OpenXML/OLE2 spreadsheet parsing; the default `FormulaErrorHandling.Exception` crossing the receipt boundary; positional magic-index cell access without `GetOrdinal`/schema; treating a spreadsheet as a columnar file (Arrow/DuckDB owns the binary path); citing the internal `ExcelColumn`/`Xlsx*`/`Ole2Package` types as public surface
