# [RASM_PERSISTENCE_API_MINIEXCEL]

`MiniExcel` owns zero-template streaming spreadsheet interchange: one static façade reads `.xlsx`/`.csv` forward-only into lazy `dynamic`, typed, or `IDataReader` rows under a shared-strings disk cache, and writes any enumerable, reader, table, or anonymous value through direct, template, and transcode egress. Column, sheet, style, range, and picture binding ride declared policy values, so workbook size never bounds memory. Persistence routes its spreadsheet lane here, taking row shape alone into the record rail the delimited codec also feeds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MiniExcel`
- package: `MiniExcel` (Apache-2.0)
- assembly: `MiniExcel`
- namespace: `MiniExcelLibs` (façade, reader, configuration base); `MiniExcelLibs.Attributes`; `MiniExcelLibs.Csv`; `MiniExcelLibs.OpenXml` and `.Models`; `MiniExcelLibs.Picture`; `MiniExcelLibs.Exceptions`
- asset: pure-managed AnyCPU runtime library; self-contained OpenXML zip and CSV reader/writer over source-generated `Regex` cell and template parsing
- target: net10.0 (multi-targets net45/net461/netstandard2.0/net8.0/net9.0/net10.0; the net10.0 asset binds)
- abi: every façade op carries a `string path` overload and a `this Stream` extension overload, and every ingress and egress op an `*Async` mirror returning the sync result in a `Task` and taking `CancellationToken`; ingress is `yield`-lazy end to end
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: the façade, its streaming reader, and the format-keyed policy bag

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `MiniExcel`               | class         | `static`; the sole ingress and egress façade                     |
|  [02]   | `MiniExcelDataReader`     | class         | `: MiniExcelDataReaderBase`; one lazy row enumerator as a reader |
|  [03]   | `MiniExcelDataReaderBase` | class         | `abstract`; typed getters, async row surface, disposal           |
|  [04]   | `IMiniExcelDataReader`    | interface     | `: IDataReader, IDataRecord, IDisposable, IAsyncDisposable`      |
|  [05]   | `ExcelType`               | enum          | `XLSX`/`CSV`/`UNKNOWN`; the format discriminant                  |
|  [06]   | `IConfiguration`          | interface     | the policy slot every op accepts; `null` takes the default       |
|  [07]   | `Configuration`           | class         | `abstract`; the shared culture, column, and buffer policy        |
|  [08]   | `OpenXmlConfiguration`    | class         | `: Configuration`; the `.xlsx` policy                            |
|  [09]   | `CsvConfiguration`        | class         | `: Configuration`; the `.csv` policy                             |
|  [10]   | `DateOnlyConversionMode`  | enum          | `None`/`RequireMidnight`/`IgnoreTimePart` serial admission       |

`AutoFilter`, `TrimColumnNames`, `EnableSharedStringCache`, `EnableWriteNullValueCell`, `EnableConvertByteArray`, `EnableWriteFilePath`, `IgnoreTemplateParameterMissing`, `ReadLineBreaksWithinQuotes`, and `QuoteWhitespaces` default true; `BufferSize` 524288, `FreezeRowCount` 1, `SharedStringCacheSize` 5 MiB, `SharedStringCachePath` the temp directory, `MinWidth` 8.42857143, `MaxWidth` 200, `Seperator` `,`, `NewLine` CRLF, `Culture` invariant.

- `Configuration`: `Culture` `DynamicColumns` `BufferSize` `FastMode` `DynamicColumnFirst` `DateOnlyConversionMode`
- `OpenXmlConfiguration`: `FillMergedCells` `TableStyles` `AutoFilter` `RightToLeft` `FreezeRowCount` `FreezeColumnCount` `IgnoreEmptyRows` `TrimColumnNames` `EnableSharedStringCache` `SharedStringCacheSize` `SharedStringCachePath` `StyleOptions` `DynamicSheets` `EnableAutoWidth` `MinWidth` `MaxWidth` `EnableWriteNullValueCell` `WriteEmptyStringAsNull` `EnableConvertByteArray` `EnableWriteFilePath` `IgnoreTemplateParameterMissing`
- `CsvConfiguration`: `Seperator` `NewLine` `ReadLineBreaksWithinQuotes` `ReadEmptyStringAsNull` `AlwaysQuote` `QuoteWhitespaces` `SplitFn` `StreamReaderFunc` `StreamWriterFunc`

[MAPPING_TYPES]: declarative and runtime column and sheet binding

`ExcelColumnAttribute` binds `Name` `Aliases` `Index` `IndexName` `Format` `Width` `Hidden` `Ignore` `Type` in one declaration; each single-purpose attribute derives from `Attribute` directly and owns one differently-named property, so a binder reading them reads nine distinct member names.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `ExcelColumnAttribute`      | class         | the unified column binder                                           |
|  [02]   | `ColumnType`                | enum          | `Value`/`Formula`; writes a literal cell or an Excel formula        |
|  [03]   | `DynamicExcelColumn`        | class         | `: ExcelColumnAttribute`; `Key` + `CustomFormatter` cell projection |
|  [04]   | `ExcelColumnNameAttribute`  | class         | `ExcelColumnName` + `Aliases` header binding                        |
|  [05]   | `ExcelColumnIndexAttribute` | class         | `ExcelColumnIndex` from an ordinal or a column letter               |
|  [06]   | `ExcelFormatAttribute`      | class         | `Format`; the cell number-format string                             |
|  [07]   | `ExcelColumnWidthAttribute` | class         | `ExcelColumnWidth`; explicit column width                           |
|  [08]   | `ExcelHiddenAttribute`      | class         | `ExcelHidden`; hides the column                                     |
|  [09]   | `ExcelIgnoreAttribute`      | class         | `ExcelIgnore`; skips the member                                     |
|  [10]   | `ExcelSheetAttribute`       | class         | `Name` + `State`; worksheet name and visibility                     |
|  [11]   | `DynamicExcelSheet`         | class         | `: ExcelSheetAttribute`; `Key`-addressed runtime sheet              |

[WORKBOOK_TYPES]: sheet, style, range, and picture vocabulary

`OpenXmlStyleOptions` and `OpenXmlHeaderStyle` each carry `HorizontalAlignment` and `VerticalAlignment`, so a header stance and a body stance set alignment independently; `MiniExcelPicture` sizes through `WidthPx` (80) and `HeightPx` (24).

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `SheetInfo`               | class         | `Id`/`Index`/`Name`/`State`/`Active` sheet descriptor     |
|  [02]   | `SheetState`              | enum          | `Visible`/`Hidden`/`VeryHidden`                           |
|  [03]   | `TableStyles`             | enum          | `None`/`Default` table preset                             |
|  [04]   | `OpenXmlStyleOptions`     | class         | `HeaderStyle` + `WrapCellContents` cell policy            |
|  [05]   | `OpenXmlHeaderStyle`      | class         | `WrapText` + `BackgroundColor`                            |
|  [06]   | `HorizontalCellAlignment` | enum          | `Left`/`Center`/`Right`                                   |
|  [07]   | `VerticalCellAlignment`   | enum          | `Bottom`/`Center`/`Top`                                   |
|  [08]   | `ExcelRange`              | class         | `StartCell`/`EndCell` + `Rows`/`Columns` used-range       |
|  [09]   | `ExcelRangeElement`       | class         | `StartIndex`/`EndIndex`/`Count`; a 1-based inclusive axis |
|  [10]   | `MiniExcelPicture`        | class         | `ImageBytes`/`SheetName`/`PictureType`/`CellAddress`      |

[FAULT_TYPES]: the typed read and serialize faults the boundary lifts

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `ExcelColumnNotFoundException`      | class         | `: KeyNotFoundException`; a member with no matching header |
|  [02]   | `ExcelInvalidCastException`         | class         | `: InvalidCastException`; a cell the member cannot take    |
|  [03]   | `MiniExcelNotSerializableException` | class         | `: InvalidOperationException`; a write member that resists |

- `ExcelColumnNotFoundException`: `ColumnName` `ColumnAliases` `ColumnIndex` `RowIndex` `Headers` `RowValues`
- `ExcelInvalidCastException`: `ColumnName` `Row` `Value` `InvalidCastType`
- `MiniExcelNotSerializableException`: `Member` (`MemberInfo`)

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingress — row enumeration, windowing, reader projection, sheet introspection

Every read carries the tail `(sheetName, ExcelType, startCell, IConfiguration)`; the path overload sniffs `UNKNOWN` from the extension while a `Stream` overload names `XLSX` or `CSV`. `Query` keys each `ExpandoObject` by header name under `useHeaderRow`, by `A`/`B`/`C` column letter otherwise; the `GetColumns`/`GetSheet*` probes resolve shape without reading the body.

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `Query<T>(…, bool) -> IEnumerable<T>`                              | static  | attribute-mapped rows, `T : class, new()`      |
|  [02]   | `Query(…, bool) -> IEnumerable<dynamic>`                           | static  | `ExpandoObject` rows keyed by header or letter |
|  [03]   | `QueryRange(…, string, string) -> IEnumerable<dynamic>`            | static  | window by an `A1:C3` cell pair                 |
|  [04]   | `QueryRange(…, int, int, int?, int?) -> IEnumerable<dynamic>`      | static  | 1-based index window; `null` = sheet edge      |
|  [05]   | `GetReader(…) -> MiniExcelDataReader`                              | static  | streaming `IDataReader` over a sheet           |
|  [06]   | `GetColumns(…) -> ICollection<string>`                             | static  | first-row column keys                          |
|  [07]   | `GetSheetNames(…, OpenXmlConfiguration) -> List<string>`           | static  | worksheet roster                               |
|  [08]   | `GetSheetInformations(…, OpenXmlConfiguration) -> List<SheetInfo>` | static  | per-sheet descriptor roster                    |
|  [09]   | `GetSheetDimensions(…) -> IList<ExcelRange>`                       | static  | used-range dimension per sheet                 |

- `QueryRange`: owns windowing alone and yields `dynamic`, so a typed windowed read binds that window through the caller's own projection.
- `QueryAsync`: every form but the `Stream` typed one wraps the sync read in `Task.Run`, so it launches asynchronously over synchronous I/O.

[ENTRYPOINT_SCOPE]: egress — write, append, template, adorn, transcode

`SaveAs` and `Insert` share `(value, printHeader, sheetName, ExcelType, IConfiguration)`, `SaveAs` adding `overwriteFile` and `Insert` `overwriteSheet`; `value` is any `IEnumerable`, `IDataReader`, `DataTable`, or anonymous object. CSV `Insert` appends to the file, XLSX `Insert` defaults its configuration to `FastMode`, and either write path rejects `.xlsm` with `NotSupportedException`.

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `SaveAs(…, object, …) -> int[]`                          | static  | writes a workbook; per-sheet row counts        |
|  [02]   | `Insert(…, object, …) -> int`                            | static  | appends or overwrites one sheet in place       |
|  [03]   | `SaveAsByTemplate(…, string\|Stream\|byte[], object, …)` | static  | renders `{{value}}`/`{{collection.field}}`     |
|  [04]   | `MergeSameCells(…, string\|byte[], ExcelType, …)`        | static  | folds repeated column values into merged cells |
|  [05]   | `AddPicture(…, params MiniExcelPicture[])`               | static  | embeds images at cell addresses                |
|  [06]   | `ConvertCsvToXlsx(string\|Stream, string\|Stream)`       | static  | streams a delimited file into a workbook       |
|  [07]   | `ConvertXlsxToCsv(string\|Stream, string\|Stream)`       | static  | streams a workbook into a delimited file       |

[ENTRYPOINT_SCOPE]: `MiniExcelDataReader` — the streaming row API

`MiniExcelDataReader` reads forward-only over one sheet, its first `Read` surfacing the row captured at construction; disposal releases the inner stream and enumerator. `MiniExcelDataReaderBase` supplies every typed getter over an ordinal: `GetBoolean` `GetByte` `GetBytes` `GetChar` `GetChars` `GetDataTypeName` `GetDateTime` `GetDecimal` `GetDouble` `GetFieldType` `GetFloat` `GetGuid` `GetInt16` `GetInt32` `GetInt64` `GetSchemaTable` `GetString` `GetValues` `IsDBNull`.

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Read()` / `ReadAsync(CancellationToken)` | instance | advance one row                    |
|  [02]   | `GetValue(int)` / `GetValueAsync(int, …)` | instance | current-row value, sync and async  |
|  [03]   | `GetName(int)` / `GetNameAsync(int, …)`   | instance | column name at an ordinal          |
|  [04]   | `GetOrdinal(string)` / `FieldCount`       | instance | name-to-ordinal, live column width |
|  [05]   | `NextResult()` / `NextResultAsync(…)`     | instance | advance past one result set        |
|  [06]   | `Close/CloseAsync/Dispose/DisposeAsync()` | instance | release stream and enumerator      |

- `MiniExcelDataReaderBase`: `this[int]`, `this[string]`, `Depth`, `IsClosed`, and `RecordsAffected` return base defaults; `GetValue` indexes the current row.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds through one `IConfiguration` policy value selected by the `ExcelType` in play — `OpenXmlConfiguration` for `.xlsx`, `CsvConfiguration` for `.csv` — so format, style, mapping, and cache posture arrive as one declaration rather than per-call arguments.
- Ingress is lazy end to end: `Query`, `QueryRange`, and `GetReader` all `yield` from one provider enumeration, the path overloads holding the shared-read `FileStream` open only for the enumeration's life, and `EnableSharedStringCache` spilling the shared-strings table to `SharedStringCachePath` so workbook size never bounds memory.

[STACKING]:
- `Sep`(`.api/api-sep.md`): peer codec on one record rail — Sep owns SIMD delimited text over `ReadOnlySpan<char>` rows, MiniExcel owns `.xlsx` workbooks and the symmetric `ConvertXlsxToCsv`/`ConvertCsvToXlsx` transcode; source format selects the owner and both deliver the same anonymous row stream downstream.
- `linq2db.EntityFrameworkCore`(`.api/api-linq2db-ef.md`): a typed row sequence sources `LinqToDBForEFTools.BulkCopyAsync<T>` into PostgreSQL binary COPY, and `BulkCopy`/`BulkCopyAsync` bind `IEnumerable<T>`/`IAsyncEnumerable<T>`, so the typed enumerable is the bulk source and the reader never is.
- `Apache.Arrow`(`.api/api-arrow.md`)/`DuckDB.NET`(`.api/api-duckdb.md`): `GetReader`'s `MiniExcelDataReader : IDataReader` is the row source the columnar materializer pulls, and the binary columnar path stays theirs — a workbook is never read as a columnar file.
- `NodaTime.Serialization.SystemTextJson`(`.api/api-nodatime-stj.md`)/`Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`): a header-keyed `dynamic` row binds through the composed wire converters, minting `Instant`, `LocalDate`, value-object, and smart-enum cells; `DateOnlyConversionMode` governs `DateOnly` admission and `DynamicExcelColumn.CustomFormatter` projects the cell on the dynamic, reader, and write legs.
- `Microsoft.Extensions.Compliance.Redaction`(`.api/api-redaction.md`): the façade exposes no reader-to-writer copy bridge, so a redacted export resolves one `Redactor` per column from the field's `DataClassificationSet` and rewrites each classified cell in the row enumerable before `SaveAs`.
- within-lib: `Ingest/tabular` folds typed read, dynamic scan, streaming reader, sheet probe, write, append, template render, adorn, and transcode onto one spec value, so the path-versus-stream overload pair collapses to one source case; a typed read binds header-keyed `dynamic` rows through the wire projection rather than `Query<T>`, whose `where T : class, new()` reflective binder neither invokes `CustomFormatter` nor mints a semantic type.

[LOCAL_ADMISSION]:
- MiniExcel is the spreadsheet and symmetric-CSV codec for tabular interchange profiles; read and write posture is a declared `OpenXmlConfiguration`/`CsvConfiguration` value.
- Column and sheet binding rides `ExcelColumnAttribute`/`ExcelSheetAttribute` on the type, or `DynamicColumns`/`DynamicSheets` for the runtime form the profile registers.
- `ExcelColumnNotFoundException`, `ExcelInvalidCastException`, and `MiniExcelNotSerializableException` lift through one funnel into a typed `Validation` at the row boundary, accumulating independent row faults across the sheet.
- Report-shaped output renders through `SaveAsByTemplate` with placeholder binding and finishes through `MergeSameCells`/`AddPicture`; header and cell styling ride the `OpenXmlStyleOptions` policy value.

[RAIL_LAW]:
- Package: `MiniExcel`
- Owns: spreadsheet and CSV interchange — lazy dynamic, typed, and `IDataReader` ingress with header, start-cell, and range windowing; attribute and runtime column and sheet mapping; enumerable, reader, table, and anonymous egress; template rendering, merged-cell folding, embedded pictures, transcode, and the shared-strings disk cache.
- Accept: profile-declared reads and writes through the façade; typed row faults folded into `Validation`; `GetReader` streamed into the columnar rail; a typed row sequence streamed into the bulk rail; `SaveAsByTemplate` for report output.
- Reject: hand-rolled OpenXML or CSV parsing; a whole-sheet materialization on a streaming path; positional magic-index `dynamic` access against a known schema; a workbook read as a columnar file, which `Apache.Arrow`/`DuckDB.NET`/`ParquetSharp` own; high-throughput delimited parsing, which `Sep` owns.
