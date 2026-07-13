# [RASM_PERSISTENCE_API_MINIEXCEL]

`MiniExcel` is a zero-template, streaming spreadsheet codec shaped around one static façade
(`MiniExcelLibs.MiniExcel`) over `Stream`/path: it reads `.xlsx`/`.csv` forward-only into lazy
`IEnumerable<dynamic>` (`ExpandoObject` rows), strongly-typed `IEnumerable<T>` (attribute-mapped
POCOs), `IDataReader` (`MiniExcelDataReader`), or a materialized `DataTable`, with cell-range
windowing (`QueryRange` by `A1:C3` string or by row/column index), header-row and start-cell
control, and a shared-strings disk cache so a million-row workbook never fully resides in memory.
It writes any `IEnumerable`/`IDataReader`/`DataTable`/anonymous-object value into a worksheet
(`SaveAs`), appends/overwrites a sheet in an existing file (`Insert`), and renders a workbook
from an `.xlsx` template (`SaveAsByTemplate`) with `{{value}}`/`{{collection.field}}` placeholders,
merged-cell folding (`MergeSameCells`), embedded pictures (`AddPicture`), and direct
CSV↔XLSX transcoding. Column mapping, format, width, index, hidden, and ignore are declared by
attribute or by a runtime `DynamicExcelColumn` (with a `CustomFormatter` delegate). It is the
spreadsheet boundary the `Sep` delimited lane cannot reach and the `Apache.Arrow`/`DuckDB`
columnar lane is the wrong shape for: the `MiniExcelDataReader : IDataReader` projection feeds the
identical downstream record rail — the NodaTime/Thinktecture wire converters, the linq2db/EF
bulk-copy path, and the Arrow/DuckDB columnar materializer — that a CSV ingress feeds, retiring
`Sylvan.Data.Excel` as the spreadsheet codec.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MiniExcel`
- package: `MiniExcel`
- license: Apache-2.0 (Wei Lin / Mini-Software team)
- assembly: `MiniExcel`
- namespace: `MiniExcelLibs` (façade + reader); `MiniExcelLibs.Attributes` (column/sheet mapping); `MiniExcelLibs.Csv` (`CsvConfiguration`); `MiniExcelLibs.OpenXml` (`OpenXmlConfiguration`, sheet/style vocabulary, `Models.ExcelRange`); `MiniExcelLibs.Picture` (`MiniExcelPicture`); `MiniExcelLibs.Exceptions`; the `OpenXml.SaveByTemplate`/`OpenXml.Styles`/`Utils`/`Zip`/`WriteAdapter` types and the `Excel{Reader,Writer,Template}Factory`→internal `IExcelReader`/`IExcelWriter`/`IExcelTemplate` providers are implementation
- target: multi-target (`net10.0`/`net9.0`/`net8.0`/`netstandard2.0`/`net461`/`net45`); the `net10.0` consumer binds `lib/net10.0`
- asset: pure-managed runtime library, AnyCPU, no native runtime (self-contained OpenXML zip + CSV reader/writer; uses source-generated `Regex` for cell/template parsing)
- abi: a static façade whose ingress/egress methods exist as both a `string path` overload and a `this Stream` extension; reads project to `dynamic`/`T`/`IDictionary<string,object>`/`IDataReader`/`DataTable`; the reader is `IDataReader`+`IAsyncDisposable`; row enumeration is lazy (`yield`), only `QueryAsDataTable` and the `[Obsolete]` variants materialize
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[FACADE_TYPE]: the single static ingress/egress entry (namespace `MiniExcelLibs`)
- rail: interchange-codec

`MiniExcel` is a `static class`; every operation has a `string path` overload (opens a shared-read
`FileStream`) and a `this Stream` extension overload (caller owns the stream). `excelType`
defaults to `UNKNOWN` on the path overloads (sniffed from the extension) and to `XLSX` on most
stream overloads (no filename to sniff).

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `MiniExcel`               | static façade       | the sole static façade; each op = path + `this Stream` overload, `*Async` mirror       |
|  [02]   | `MiniExcelDataReader`     | ingress reader      | `: MiniExcelDataReaderBase`; a lazy `Query` enumerator as `IDataReader` over one sheet |
|  [03]   | `MiniExcelDataReaderBase` | reader base         | `abstract`; the `IMiniExcelDataReader`/`IDataReader`/`IAsyncDisposable` skeleton       |
|  [04]   | `IMiniExcelDataReader`    | reader contract     | `: IDataReader, IDataRecord, IDisposable, IAsyncDisposable`; adds the async row API    |
|  [05]   | `ExcelType`               | format discriminant | `XLSX` / `CSV` / `UNKNOWN` (sniff from extension/stream) — the `excelType` selector    |

[CONFIGURATION_TYPES]: the codec policy bag (namespaces `MiniExcelLibs`, `MiniExcelLibs.Csv`, `MiniExcelLibs.OpenXml`)
- rail: interchange-codec

`IConfiguration` is the empty marker the façade accepts; `Configuration` is the shared abstract
base; `OpenXmlConfiguration` and `CsvConfiguration` are the two concrete policies. Pass the
matching concrete type for the `ExcelType` in play.

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                                                                          |
| :-----: | :----------------------- | :--------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `IConfiguration`         | marker interface | the policy slot every `Query`/`SaveAs`/`Insert` overload accepts (`= null` → default) |
|  [02]   | `Configuration`          | abstract base    | the shared base — culture, dynamic columns, buffer, fast-mode, date-only mode         |
|  [03]   | `OpenXmlConfiguration`   | `.xlsx` policy   | `: Configuration`; freeze panes, auto-filter, shared-string cache, styles, auto width |
|  [04]   | `CsvConfiguration`       | `.csv` policy    | `: Configuration`; separator, newline, quote policy, encoding hooks, `SplitFn`        |
|  [05]   | `DateOnlyConversionMode` | date-cell policy | `None`/`RequireMidnight`/`IgnoreTimePart` — `DateOnly` date-time-serial read mode     |

[PROPERTIES]: the settable config members and defaults.
- [02]-[CONFIGURATION]: `Culture` (`InvariantCulture`), `DynamicColumns` (`DynamicExcelColumn[]`), `BufferSize` (524288), `FastMode`, `DynamicColumnFirst`, `DateOnlyConversionMode`
- [03]-[OPENXMLCONFIGURATION]: `FillMergedCells`, `TableStyles`, `AutoFilter` (true), `RightToLeft`, `FreezeRowCount` (1)/`FreezeColumnCount`, `IgnoreEmptyRows`, `TrimColumnNames` (true), `EnableSharedStringCache` (true)/`SharedStringCacheSize` (5 MiB)/`SharedStringCachePath`, `StyleOptions`, `DynamicSheets`, `EnableAutoWidth`/`MinWidth`/`MaxWidth`, `EnableWriteNullValueCell`/`WriteEmptyStringAsNull`, `EnableConvertByteArray`, `IgnoreTemplateParameterMissing`
- [04]-[CSVCONFIGURATION]: `Seperator` (`,`), `NewLine` (`\r\n`), `ReadLineBreaksWithinQuotes` (true), `ReadEmptyStringAsNull`, `AlwaysQuote`/`QuoteWhitespaces`, `SplitFn` (`Func<string,string[]>`), `StreamReaderFunc`/`StreamWriterFunc` (encoding hooks, default UTF-8 BOM)

[MAPPING_ATTRIBUTES]: declarative POCO↔column/sheet binding (namespace `MiniExcelLibs.Attributes`)
- rail: interchange-codec

`ExcelColumnAttribute` is the unified column binder (name/alias/index/format/width/hidden/ignore/
type in one attribute); the single-purpose attributes are narrower aliases. `DynamicExcelColumn`
is the runtime (no-recompile) form supplied through `Configuration.DynamicColumns`.

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]      | [CAPABILITY]                                                                      |
| :-----: | :-------------------------- | :------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `ExcelColumnAttribute`      | unified column bind | `Name`/`Aliases`/`Index`/`IndexName`/`Format`/`Width`/`Hidden`/`Ignore`/`Type`    |
|  [02]   | `ColumnType`                | column kind         | `Value` / `Formula` — write the cell as a literal value or an Excel formula       |
|  [03]   | `DynamicExcelColumn`        | runtime column      | `: ExcelColumnAttribute`; `(string key)`, `CustomFormatter` `Func<object,object>` |
|  [04]   | `ExcelColumnNameAttribute`  | name + alias        | ctor `(string name, string[] aliases = null)` — header-name binding               |
|  [05]   | `ExcelColumnIndexAttribute` | ordinal             | ctor `(int)` or `(string columnName)` — fixed column position                     |
|  [06]   | `ExcelFormatAttribute`      | number/date format  | ctor `(string format)` — the cell number-format string                            |
|  [07]   | `ExcelColumnWidthAttribute` | column width        | ctor `(double)` — explicit column width                                           |
|  [08]   | `ExcelHiddenAttribute`      | hidden column       | ctor `(bool = true)`                                                              |
|  [09]   | `ExcelIgnoreAttribute`      | skip member         | ctor `(bool = true)`                                                              |
|  [10]   | `ExcelSheetAttribute`       | sheet bind          | `Name`, `State` (`SheetState`) — worksheet name + visibility on a POCO type       |
|  [11]   | `DynamicExcelSheet`         | runtime sheet       | `: ExcelSheetAttribute`; `(string key)` via `OpenXmlConfiguration.DynamicSheets`  |

[WORKBOOK_VOCABULARY]: sheet, style, range, and picture surface (namespaces `MiniExcelLibs.OpenXml`, `MiniExcelLibs.OpenXml.Models`, `MiniExcelLibs.Picture`)
- rail: interchange-codec
- style rows `OpenXmlStyleOptions`/`OpenXmlHeaderStyle` both carry `HorizontalAlignment`/`VerticalAlignment`

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]     | [CAPABILITY]                                                                        |
| :-----: | :------------------------ | :----------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `SheetInfo`               | sheet descriptor   | `Id`/`Index`/`Name`/`State` (`SheetState`)/`Active`                                 |
|  [02]   | `SheetState`              | sheet visibility   | `Visible` / `Hidden` / `VeryHidden`                                                 |
|  [03]   | `TableStyles`             | table style preset | `None` / `Default` (`OpenXmlConfiguration.TableStyles`)                             |
|  [04]   | `OpenXmlStyleOptions`     | cell style policy  | `HeaderStyle` (`OpenXmlHeaderStyle`), `WrapCellContents`                            |
|  [05]   | `OpenXmlHeaderStyle`      | header style       | `WrapText`, `BackgroundColor` (`System.Drawing.Color`)                              |
|  [06]   | `HorizontalCellAlignment` | horizontal align   | `Left` / `Center` / `Right`                                                         |
|  [07]   | `VerticalCellAlignment`   | vertical align     | `Bottom` / `Center` / `Top`                                                         |
|  [08]   | `ExcelRange`              | sheet dimension    | `StartCell`/`EndCell` + `Rows`/`Columns` (`ExcelRangeElement`)                      |
|  [09]   | `ExcelRangeElement`       | range axis         | `StartIndex`/`EndIndex`/`Count` (1-based inclusive axis span)                       |
|  [10]   | `MiniExcelPicture`        | embedded image     | `ImageBytes`/`SheetName`/`PictureType`/`CellAddress`/`WidthPx` (80)/`HeightPx` (24) |

[FAULT_TYPES]: typed read/serialize faults (namespace `MiniExcelLibs.Exceptions`)
- rail: interchange-codec

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]    | [CAPABILITY]                                                              |
| :-----: | :---------------------------------- | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `ExcelColumnNotFoundException`      | missing column    | `: KeyNotFoundException`; a `Query<T>` member with no matching header     |
|  [02]   | `ExcelInvalidCastException`         | cell cast failure | the `TabularFault.CellCast` lift source (decompile-verified)              |
|  [03]   | `MiniExcelNotSerializableException` | egress reject     | the `TabularFault.NotSerializable` lift source (decompile-verified)       |
|  [04]   | `ExcelInvalidCastException`         | cell cast fault   | `: InvalidCastException`; a cell that cannot coerce to the typed member   |
|  [05]   | `MiniExcelNotSerializableException` | non-serializable  | `: InvalidOperationException`; a write value member that cannot serialize |

[MEMBERS]: the typed fault members each exception carries.
- [01]-[EXCELCOLUMNNOTFOUNDEXCEPTION]: `ColumnName`/`ColumnAliases`/`ColumnIndex`/`RowIndex`/`Headers`/`RowValues`
- [02]-[EXCELINVALIDCASTEXCEPTION]: `ColumnName`/`Row`/`InvalidCastType`
- [03]-[MINIEXCELNOTSERIALIZABLEEXCEPTION]: `Member`
- [04]-[EXCELINVALIDCASTEXCEPTION]: `ColumnName`/`Row`/`Value`/`InvalidCastType`
- [05]-[MINIEXCELNOTSERIALIZABLEEXCEPTION]: `Member` (`MemberInfo`)

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingress — lazy row enumeration (sync + async mirror)
- rail: interchange-codec
- surface-root: `MiniExcel` (path overload + `this Stream` extension overload)

`Query<T>` materializes one POCO per row (attribute-mapped); `Query` yields a `dynamic`
(`ExpandoObject`) per row whose members are the header names (when `useHeaderRow: true`) or the
`A`/`B`/`C` column letters. Both are lazy `yield` enumerables — back-pressured, never fully
buffered. `startCell` (`"A1"`) skips a leading block; `sheetName` selects a worksheet (first sheet
when `null`). Every overload has a `string path` and a `this Stream` form sharing the tail
`(sheetName?, ExcelType=UNKNOWN, startCell="A1", IConfiguration?)`; `QueryAsync` adds a `CancellationToken`.

| [INDEX] | [SURFACE]                                             | [RETURNS]                     | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------- | :---------------------------- | :------------------------------------------ |
|  [01]   | `MiniExcel.Query<T>(string path, …)`                  | `IEnumerable<T>` (lazy)       | typed POCO rows by attribute mapping        |
|  [02]   | `stream.Query<T>(…)`                                  | `IEnumerable<T>` (lazy)       | typed rows over a caller-owned stream       |
|  [03]   | `MiniExcel.Query(string path, useHeaderRow=false, …)` | `IEnumerable<dynamic>` (lazy) | dynamic `ExpandoObject` rows                |
|  [04]   | `stream.Query(useHeaderRow=false, …)`                 | `IEnumerable<dynamic>` (lazy) | dynamic rows over a stream                  |
|  [05]   | `MiniExcel.QueryAsync<T>` / `QueryAsync`              | `Task<IEnumerable<…>>`        | async-launched mirror of `Query`/`Query<T>` |

[ENTRYPOINT_SCOPE]: ingress — cell-range windowing
- rail: interchange-codec

`QueryRange` reads a sub-rectangle by an `A1:C3`-style `startCell`/`endCell` pair, or by 1-based
`startRowIndex`/`startColumnIndex` + nullable `endRowIndex`/`endColumnIndex` (null = to the
sheet edge). Each form has a `string path` and `this Stream` overload sharing the common
`(useHeaderRow=false, sheetName?, ExcelType=UNKNOWN, …, IConfiguration?)`. The public façade exposes
the `dynamic` form; the typed `QueryRange<T>` lives on the internal reader contract, so a typed range
read combines `Query<T>` with `startCell`/window policy.

| [INDEX] | [SURFACE]                                                         | [RETURNS]              | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------- | :--------------------- | :------------------------------- |
|  [01]   | `MiniExcel.QueryRange(path, …, startCell, endCell)`               | `IEnumerable<dynamic>` | window by `A1:C3` cell string    |
|  [02]   | `stream.QueryRange(…, startCell, endCell)`                        | `IEnumerable<dynamic>` | cell-string window over a stream |
|  [03]   | `MiniExcel.QueryRange(path, …, startRowIndex, …, endColumnIndex)` | `IEnumerable<dynamic>` | window by row/column index       |
|  [04]   | `stream.QueryRange(…, startRowIndex, …, endColumnIndex)`          | `IEnumerable<dynamic>` | index window over a stream       |

[ENTRYPOINT_SCOPE]: ingress — `IDataReader` + `DataTable` + introspection
- rail: interchange-codec

`GetReader` is the bulk-rail seam: a `MiniExcelDataReader : IDataReader` streaming one sheet, the
shape `linq2db`/EF bulk-copy and the Arrow/DuckDB materializer consume. `QueryAsDataTable` (and its
async form) is `[Obsolete]` because it buffers the whole sheet — use the reader or `Query` for
streaming. `GetColumns`/`GetSheetNames`/`GetSheetInformations`/`GetSheetDimensions` introspect
without reading the body. `GetReader`/`GetColumns`/`QueryAsDataTable` share
`(path/stream, useHeaderRow, sheetName?, ExcelType=UNKNOWN, startCell="A1", IConfiguration?)`;
`GetSheetNames`/`GetSheetInformations` take `(path/stream, OpenXmlConfiguration?)`, `GetSheetDimensions(path/stream)`.

| [INDEX] | [SURFACE]                                    | [RETURNS]             | [CAPABILITY]                                             |
| :-----: | :------------------------------------------- | :-------------------- | :------------------------------------------------------- |
|  [01]   | `MiniExcel.GetReader(path/stream, …)`        | `MiniExcelDataReader` | streaming `IDataReader` over one sheet                   |
|  [02]   | `MiniExcel.GetColumns(path/stream, …)`       | `ICollection<string>` | first-row column keys (header names or `A`/`B`/…)        |
|  [03]   | `MiniExcel.GetSheetNames(…)`                 | `List<string>`        | worksheet roster                                         |
|  [04]   | `MiniExcel.GetSheetInformations(…)`          | `List<SheetInfo>`     | per-sheet `Id`/`Index`/`Name`/`State`/`Active`           |
|  [05]   | `MiniExcel.GetSheetDimensions(…)`            | `IList<ExcelRange>`   | used-range dimension per sheet                           |
|  [06]   | `MiniExcel.QueryAsDataTable(…)` `[Obsolete]` | `DataTable`           | full-buffer materialization (avoid; `useHeaderRow=true`) |

[ENTRYPOINT_SCOPE]: egress — write, insert, template, picture, transcode (sync + async mirror)
- rail: interchange-codec

`SaveAs` writes a fresh file/sheet from any `IEnumerable`/`IDataReader`/`DataTable`/anonymous-object
`value`; `Insert` appends or overwrites a sheet in an existing workbook (`overwriteSheet`); CSV
`Insert` uses `FileMode.Append`, XLSX `Insert` defaults the config to `FastMode = true`.
`SaveAsByTemplate` renders an `.xlsx` template with `{{value}}`/`{{collection.field}}` placeholders.
`.xlsm` is rejected by `SaveAs`/`Insert`. `SaveAs`/`Insert` share
`(value, printHeader=true, sheetName="Sheet1", ExcelType=UNKNOWN|XLSX, IConfiguration?)`, `SaveAs` adding
`overwriteFile=false` and `Insert` `overwriteSheet=false`; `SaveAsByTemplate(path|Stream, templatePath|Stream|byte[], value, IConfiguration?)`,
`MergeSameCells(mergedFilePath|Stream, path|byte[], ExcelType=XLSX, IConfiguration?)`, `AddPicture(path|Stream, params MiniExcelPicture[])`.
Each carries an `*Async` mirror — `SaveAsAsync`/`InsertAsync`/`SaveAsByTemplateAsync`/`MergeSameCellsAsync(…, CancellationToken)` returning `Task`/`Task<int>`/`Task<int[]>`.

| [INDEX] | [SURFACE]                                         | [RETURNS] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------ | :-------- | :----------------------------------------------------- |
|  [01]   | `MiniExcel.SaveAs(string path, value, …)`         | `int[]`   | write a workbook; per-sheet row counts                 |
|  [02]   | `stream.SaveAs(value, …)`                         | `int[]`   | write into a caller-owned stream                       |
|  [03]   | `MiniExcel.Insert(path/stream, value, …)`         | `int`     | append/overwrite a sheet in an existing file           |
|  [04]   | `MiniExcel.SaveAsByTemplate(…)`                   | `void`    | render a workbook from an `.xlsx` template             |
|  [05]   | `MiniExcel.MergeSameCells(…)`                     | `void`    | fold vertically-repeated cell values into merged cells |
|  [06]   | `MiniExcel.AddPicture(…)`                         | `void`    | embed `MiniExcelPicture[]` images at cell addresses    |
|  [07]   | `MiniExcel.ConvertCsvToXlsx` / `ConvertXlsxToCsv` | `void`    | direct CSV↔XLSX transcode (stream the rows, no buffer) |

[ENTRYPOINT_SCOPE]: `MiniExcelDataReader` — the streaming `IDataReader` row API
- rail: interchange-codec

The reader is forward-only over one sheet; `Read` advances (the first `Read` surfaces the row
captured at construction), `GetValue(i)`/`GetName(i)`/`GetOrdinal(name)`/`FieldCount` index the
current row, the inherited `MiniExcelDataReaderBase` supplies the typed getters and the async
(`ReadAsync`/`GetValueAsync`/`NextResultAsync`/`CloseAsync`) surface; `Dispose`/`DisposeAsync`
release the inner stream + enumerator. Each member below is a `reader` call.

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `Read()/ReadAsync(CancellationToken)`                                                       | advance one row                   |
|  [02]   | `GetValue(int)/GetValueAsync(int,…)/GetValues(object[])`                                    | boxed / async / bulk row value    |
|  [03]   | `GetName(int)/GetNameAsync(int,…)/GetOrdinal(string)/FieldCount`                            | header name ↔ ordinal, live width |
|  [04]   | `GetString/GetInt32/GetDouble/GetDecimal/GetDateTime/GetBoolean/GetGuid(int)/IsDBNull(int)` | inherited typed ADO.NET getters   |
|  [05]   | `NextResult()/NextResultAsync(…)/Close()/CloseAsync()/Dispose()/DisposeAsync()`             | result advance / lifecycle        |

## [04]-[IMPLEMENTATION_LAW]

[INTERCHANGE_PROFILE]:
- ingress root: `MiniExcel.Query`/`Query<T>`/`QueryRange` — lazy `yield` enumerables over `.xlsx`/`.csv`; `Query<T>` attribute-maps to a POCO, `Query` yields a header-keyed `ExpandoObject`. `GetReader` projects the same stream as a `MiniExcelDataReader : IDataReader` for the bulk/columnar rail.
- egress root: `MiniExcel.SaveAs`/`Insert` — write any `IEnumerable`/`IDataReader`/`DataTable`/anonymous value into a worksheet; `SaveAsByTemplate` renders an `.xlsx` template; `int[]`/`int` return the per-sheet row counts.
- format selector: `ExcelType` (`XLSX`/`CSV`/`UNKNOWN`) — `UNKNOWN` sniffs from the path extension; a `Stream` overload requires an explicit `XLSX`/`CSV`. `.xlsm` is unsupported on the write path.
- policy seam: `IConfiguration` — `OpenXmlConfiguration` for `.xlsx` (shared-string disk cache, freeze panes, auto-filter, style options, merged-cell fill, dynamic sheets, auto width) and `CsvConfiguration` for `.csv` (separator, newline, quote policy, encoding hooks, custom `SplitFn`); never an ad-hoc inline literal.
- mapping seam: `ExcelColumnAttribute`/`ExcelSheetAttribute` on the POCO, or `Configuration.DynamicColumns` (`DynamicExcelColumn` with a `CustomFormatter` delegate) and `OpenXmlConfiguration.DynamicSheets` for the no-recompile runtime form.

[LOCAL_ADMISSION]:
- MiniExcel is the spreadsheet + CSV codec for tabular interchange profiles (`Ingest/schedule`, `Catalog/cost` import-export, `Ingest/tabular` element ingress) where the source is `.xlsx` or `.csv`; profile options live on a typed `OpenXmlConfiguration`/`CsvConfiguration`, never inlined.
- a typed read uses `Query<T>` with `ExcelColumnAttribute`/`ExcelColumnNameAttribute` aliases (never positional magic-index `dynamic` access on a known schema); a `Query<T>` against a missing header surfaces `ExcelColumnNotFoundException` and a bad cell `ExcelInvalidCastException`, both folded into a typed `Fin`/`Validation` failure at the row boundary rather than thrown through the receipt path.
- a large read streams through `Query`/`GetReader` (lazy `yield`), keeping `OpenXmlConfiguration.EnableSharedStringCache` on so the shared-strings table spills to `SharedStringCachePath`; `QueryAsDataTable` is `[Obsolete]` (whole-sheet buffer) and is used only where a `DataTable` is the explicit boundary contract.
- an egress reuses the reader/lane rail the query produces (`SaveAs(stream, dataReader)` / `Insert`), so a schedule/cost/catalog export never re-materializes a table; report-shaped output uses `SaveAsByTemplate` with placeholder binding instead of cell-by-cell writes.

[STACKING]:
- delimited sibling: MiniExcel and `Sep` (`api-sep`) are the two tabular-text-and-spreadsheet codecs — `Sep` owns high-throughput delimited text over zero-allocation `ReadOnlySpan<char>` rows, MiniExcel owns `.xlsx` workbooks (and a convenience CSV path via `CsvConfiguration`); the profile selects one by source format, and both project into the SAME downstream record rail. `Sep` remains the performance-critical CSV codec; MiniExcel's CSV leg exists for symmetric xlsx↔csv transcode (`ConvertXlsxToCsv`/`ConvertCsvToXlsx`).
- retired codec: MiniExcel replaces `Sylvan.Data.Excel` as the spreadsheet codec — the `DbDataReader`-only Sylvan surface is superseded by MiniExcel's `dynamic`/`T`/`IDataReader`/`DataTable` ingress plus template/picture/merged-cell egress; the former Sylvan catalog and any `Sylvan.Data.Excel` design-page reference are retired (no `Sylvan` `PackageReference` remains in the folder csproj).
- bulk ingress: `MiniExcel.GetReader` yields a `MiniExcelDataReader : IDataReader` — an `IDataReader` source for `LinqToDBForEFTools.BulkCopy`/`BulkCopyAsync` (`api-linq2db-ef`), so a spreadsheet streams into PostgreSQL binary COPY through the same bulk rail a CSV uses; both arrive as a reader.
- columnar edge: MiniExcel is the spreadsheet/CSV codec only; `Apache.Arrow`/`DuckDB.NET`/`ParquetSharp` own the binary columnar path. A spreadsheet ingress reads rows, the record rail projects them, and the columnar rail materializes the Arrow batch — MiniExcel never re-implements a columnar reader, and a `.xlsx` is never treated as a columnar file.
- wire converters: a typed read feeds the NodaTime STJ converters (`api-nodatime-stj`) and the Thinktecture value-object/smart-enum factories — `Query<T>` maps a header to a POCO member, a `DynamicExcelColumn.CustomFormatter` (`Func<object,object>`) projects an `Instant`/`LocalDate`/value-object cell at the boundary, and `DateOnlyConversionMode` governs a `DateOnly` target; the semantic type is minted by the wire rail, MiniExcel only delivers the cell.
- element ingress (`Rasm.Element`): MiniExcel is the `Ingest/tabular` codec — a workbook/CSV of element rows reads through `Query<T>`/`GetReader`, the per-app tabular→element map (the wire-composition owner) projects each row into an `ElementGraph` node, and a catalog/cost/schedule egress writes element-derived tables back through `SaveAs`/`SaveAsByTemplate`; the codec never knows the element graph, only the row shape at the wire.
- redaction/egress: the reader→writer bridge re-emits an `IDataReader` (optionally wrapped by a redacting `DbDataReader` decorator applying `Microsoft.Extensions.Compliance.Redaction` per column) into a worksheet, so a redacted spreadsheet export streams without materializing the table.

[RAIL_LAW]:
- Package: `MiniExcel`
- Owns: spreadsheet + CSV interchange — lazy `dynamic`/typed/`IDataReader`/`DataTable` `.xlsx`/`.csv` ingress with header-row, start-cell, and cell-range windowing; attribute and runtime (`DynamicExcelColumn`) column/sheet mapping; `IEnumerable`/`IDataReader`/`DataTable`/anonymous-object egress; `.xlsx` template rendering, merged-cell folding, embedded pictures, and CSV↔XLSX transcode; the shared-string disk cache for memory-bounded large workbooks.
- Accept: profile-declared spreadsheet/CSV reads/writes through the static façade; `Query<T>` attribute-mapped typed reads with `ExcelColumnNotFoundException`/`ExcelInvalidCastException` folded into `Fin`; `GetReader` `IDataReader` streamed into the bulk/columnar rail; `OpenXmlConfiguration`/`CsvConfiguration` typed policy; `SaveAsByTemplate` for report-shaped output.
- Reject: hand-rolled OpenXML/CSV parsing; `QueryAsDataTable` whole-sheet buffering on a streaming path; positional magic-index `dynamic` access on a known schema; treating a spreadsheet as a columnar file (`Apache.Arrow`/`DuckDB`/`ParquetSharp` own the binary path); high-throughput delimited parsing that `Sep` owns; citing the internal `IExcelReader`/`IExcelWriter`/`Excel*Factory`/`Utils`/`Zip`/`WriteAdapter` types or the retired `Sylvan.Data.Excel` surface.
