# [RASM_PERSISTENCE_API_SEP]

`Sep` owns zero-allocation, SIMD-vectorized separated-value read and write over `ReadOnlySpan<char>` columns: typed `ISpanParsable<T>` parsing, ref-struct row/col projections scope-bound to the read, and a sync/async/parallel enumeration boundary that lifts them into domain records. It is the column codec for Persistence tabular interchange — schedule/cost import-export, the Arrow/DuckDB CSV bridge edge, and the `Query/lane` row-projection seam — feeding parsed spans into the NodaTime/Thinktecture wire converters and the linq2db bulk rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sep`
- package: `Sep` (MIT)
- assembly: `Sep`
- namespace: `nietras.SeparatedValues`
- asset: runtime library
- target: net10.0 (multi-targets net8.0/net9.0/net10.0; the net10.0 asset binds)
- abi: ref-struct row/col projections, `allows ref struct` generic delegates, ref-struct `IDisposable`/`IAsyncDisposable` rows
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[ENTRY_TYPES]: specification and options

`Sep` wraps one separator `char`; `SepReaderOptions` and `SepWriterOptions` are option bags mutated through `with` or a `Func<Options,Options>` configure lambda.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Sep`                | struct        | `readonly record struct`; selects the separator                  |
|  [02]   | `SepSpec`            | struct        | `readonly record struct`; binds `Sep` + culture                  |
|  [03]   | `SepDefaults`        | class         | static default separator/culture policy                          |
|  [04]   | `SepReaderOptions`   | struct        | `readonly record struct`; parse policy bag                       |
|  [05]   | `SepWriterOptions`   | struct        | `readonly record struct`; emit policy bag                        |
|  [06]   | `SepTrim`            | enum          | `[Flags] byte` `None`/`Outer`/`AfterUnescape`/`All`              |
|  [07]   | `SepColNotSetOption` | enum          | `Throw`/`Empty`/`Skip`; writer-only unset policy                 |
|  [08]   | `SepToString`        | class         | abstract pool root; `Direct` + `PoolPerCol*`/`OnePool` factories |
|  [09]   | `SepCreateToString`  | delegate      | `(SepReaderHeader?, int colCount) -> SepToString` factory        |

[READER_TYPES]: read surfaces

`SepReader` enumerates one buffer as sync `IEnumerable<Row>` and async `IAsyncEnumerable<Row>`; `Row`/`Col`/`Cols` are ref-struct projections scope-bound to the read.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `SepReader`                 | class         | sync + async row enumeration over one buffer            |
|  [02]   | `SepReader.Row`             | struct        | `ColCount`, whole-row `Span`, name/index/range indexers |
|  [03]   | `SepReader.Col`             | struct        | `Span`/`Parse<T>`/`TryParse<T>`                         |
|  [04]   | `SepReader.Cols`            | struct        | column-set `Parse`/`TryParse`/`Select`/`Join`           |
|  [05]   | `SepReader.RowFunc<T>`      | delegate      | `(Row) -> T where T : allows ref struct`                |
|  [06]   | `SepReader.RowTryFunc<T>`   | delegate      | `(Row, out T) -> bool`; skips on false                  |
|  [07]   | `SepReader.ColFunc<T>`      | delegate      | `(Col) -> T where T : allows ref struct`                |
|  [08]   | `SepReader.AsyncEnumerator` | struct        | `IAsyncEnumerator<Row>`/`IAsyncDisposable`              |
|  [09]   | `SepReaderHeader`           | class         | name/span index resolution + prefix window              |
|  [10]   | `SepReaderExtensions`       | class         | option builders, `From*`/`From*Async`, `Enumerate*`     |

[WRITER_TYPES]: write surfaces

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `SepWriter`                                     | class         | `IDisposable`/`IAsyncDisposable` row emitter         |
|  [02]   | `SepWriter.Row`                                 | struct        | `IDisposable`/`IAsyncDisposable`; commits on dispose |
|  [03]   | `SepWriter.Col`                                 | struct        | `Set`/`Format`; interpolated-string handler          |
|  [04]   | `SepWriter.Col.FormatInterpolatedStringHandler` | struct        | `[InterpolatedStringHandler]` zero-alloc col write   |
|  [05]   | `SepWriter.Cols`                                | struct        | column-set `Set`                                     |
|  [06]   | `SepWriterHeader`                               | class         | owns the written header                              |
|  [07]   | `SepWriterExtensions`                           | class         | option builders, `To*`, `Strict`                     |
|  [08]   | `SepReaderWriterExtensions`                     | class         | `NewRow(readerRow)` + `CopyTo` reader->writer        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: option construction

`Func<Options,Options>` configure is the canonical seam; the bare form with `with` is the fallback. `Sep.Auto` returns a `Sep?` of `null`, which `Reader(this Sep?)` reads as auto-detect from the first row.

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Sep.New(char)` / `new Sep(char)`             | factory  | explicit separator                                  |
|  [02]   | `Sep.Default`                                 | static   | `SepDefaults.Separator` (`;`)                       |
|  [03]   | `Sep.Auto`                                    | static   | `Sep?` = `null`; routes to auto-detect              |
|  [04]   | `Sep.Reader()` / `sep.Reader()`               | factory  | starts `SepReaderOptions`                           |
|  [05]   | `Sep.Reader(Func<…>)` / `sep.Reader(Func<…>)` | factory  | starts + configures reader options in one call      |
|  [06]   | `spec.Reader(…)`                              | factory  | reader options from a culture-bound `SepSpec`       |
|  [07]   | `Strict`                                      | instance | `in` reader/writer options; hardens count + quoting |
|  [08]   | `Sep.Writer()` / `sep.Writer(Func<…>)`        | factory  | starts + configures writer options                  |

[ENTRYPOINT_SCOPE]: reader options (`SepReaderOptions` init members)

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Sep`                            | property | `Sep?`; `null` = auto-detect separator              |
|  [02]   | `CultureInfo`                    | property | `CultureInfo?`; `null` = invariant fast path        |
|  [03]   | `HasHeader`                      | property | `bool` default true; header drives name indexing    |
|  [04]   | `ColNameComparer`                | property | `IEqualityComparer<string>` header lookup           |
|  [05]   | `Trim`                           | property | `SepTrim` `[Flags]` classifier                      |
|  [06]   | `Unescape`                       | property | `bool`; unescape quoted columns in place            |
|  [07]   | `DisableFastFloat`               | property | `bool`; opt out of the vectorized float parser      |
|  [08]   | `DisableColCountCheck`           | property | `bool`; allow ragged rows                           |
|  [09]   | `DisableQuotesParsing`           | property | `bool`; treat quotes as literal characters          |
|  [10]   | `InitialBufferLength`            | property | `int`; initial read buffer sizing                   |
|  [11]   | `CreateToString`                 | property | `SepCreateToString`; per-column string-pool factory |
|  [12]   | `AsyncContinueOnCapturedContext` | property | `bool`; `From*Async` continuation context           |

[ENTRYPOINT_SCOPE]: reader sources — sync + async mirror

`From*` are `in SepReaderOptions` extensions; the `name + factory` overloads admit a named stream/reader for diagnostics with a `leaveOpen` variant.

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------ | :------ | :----------------------------------------- |
|  [01]   | `FromText(string) -> SepReader`                         | factory | string source                              |
|  [02]   | `FromFile(string) -> SepReader`                         | factory | file source                                |
|  [03]   | `From(byte[]) / From(Stream[, leaveOpen]) -> SepReader` | factory | buffer / stream source                     |
|  [04]   | `From(TextReader[, leaveOpen]) -> SepReader`            | factory | reader source                              |
|  [05]   | `From(name, Func<…>[, leaveOpen]) -> SepReader`         | factory | named stream/reader source                 |
|  [06]   | `FromTextAsync` / `FromFileAsync` / `FromAsync(…)`      | factory | `ValueTask<SepReader>` mirror of [01]-[05] |

[ENTRYPOINT_SCOPE]: row / column access (ref-struct, scope-bound)

`Cols.Parse<T>(Span<T>)` parses a whole column set into a caller buffer zero-alloc; `Cols.Select<T>(ColFunc<T>)` projects each column through a delegate.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `MoveNext` / `MoveNextAsync`                             | instance | advance row (sync / `ValueTask<bool>`)       |
|  [02]   | `Current`                                                | property | current `Row` ref struct                     |
|  [03]   | `Row[name]` / `Row[index]` / `Row[range]`                | instance | selects `Col`/`Cols`                         |
|  [04]   | `Row.ColCount`                                           | property | live column count (`int`)                    |
|  [05]   | `Row.Span`                                               | property | whole-row `ReadOnlySpan<char>`               |
|  [06]   | `Row.TryGet(string, out Col)`                            | instance | non-throwing column fetch                    |
|  [07]   | `Col.Span`                                               | property | `ReadOnlySpan<char>` column chars            |
|  [08]   | `Col.Parse<T>()`                                         | instance | `where T : ISpanParsable<T>`                 |
|  [09]   | `Col.TryParse<T>()`                                      | instance | `where T : struct, ISpanParsable<T>` -> `T?` |
|  [10]   | `Cols.Parse<T>()` / `Cols.Parse<T>(Span<T>)`             | instance | parse set -> `Span<T>` or into caller buffer |
|  [11]   | `Cols.TryParse<T>()` / `Cols.TryParse<T>(Span<T?>)`      | instance | nullable struct set parse (`T : struct`)     |
|  [12]   | `Cols.Select<T>(ColFunc<T>)`                             | instance | delegate-project each column -> `Span<T>`    |
|  [13]   | `Cols.Join(ReadOnlySpan<char>)`                          | instance | join the column set -> `ReadOnlySpan<char>`  |
|  [14]   | `Header.IndexOf(string)` / `IndexOf(ReadOnlySpan<char>)` | instance | resolve column index (string / span)         |
|  [15]   | `Header.TryIndexOf(…, out int)`                          | instance | non-throwing index resolution                |
|  [16]   | `Header.IndicesOf(IReadOnlyList<string>)`                | instance | resolve many column indices                  |
|  [17]   | `Header.IndicesOf(params string[])`                      | instance | resolve many (params form)                   |
|  [18]   | `Header.IndicesOf(ReadOnlySpan<string>, Span<int>)`      | instance | resolve many into a caller buffer            |
|  [19]   | `Header.NamesStartingWith(prefix[, StringComparison])`   | instance | prefixed name window (default `Ordinal`)     |
|  [20]   | `Header.ColNames` / `Header.IsEmpty` / `Header.Empty`    | property | name roster / emptiness / sentinel           |

[ENTRYPOINT_SCOPE]: row enumeration projections — sync, async, parallel

`Enumerate` is the materialization boundary: ref-struct rows out to a materialized `IEnumerable<T>`, the canonical seam into `Query/lane`.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Enumerate(RowFunc<T>)` / `Enumerate(RowTryFunc<T>) -> IEnumerable<T>`     | instance | project / project-or-skip rows        |
|  [02]   | `EnumerateAsync(RowFunc<T>)` / `(RowTryFunc<T>) -> IAsyncEnumerable<T>`    | instance | async mirror                          |
|  [03]   | `ParallelEnumerate(RowFunc<T>)` / `(RowTryFunc<T>) -> IEnumerable<T>`      | instance | parallel project; preserves row order |
|  [04]   | `ParallelEnumerate(RowFunc<T>, int degreeOfParallelism) -> IEnumerable<T>` | instance | parallel project, bounded fan         |

[ENTRYPOINT_SCOPE]: writer sinks + column writes

`SepWriterOptions` init members carry: `Sep` `CultureInfo` `WriteHeader` `Escape` `DisableColCountCheck` `ColNotSetOption` `AsyncContinueOnCapturedContext`. `Col.Set` accepts a span or an interpolated string via `FormatInterpolatedStringHandler`; `Col.Format` writes an `ISpanFormattable` with an optional format span.

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ToText()` / `ToText(int capacity)` | factory  | string sink (capacity-presized)         |
|  [02]   | `ToFile(string)`                    | factory  | file sink                               |
|  [03]   | `To(Stream[, leaveOpen])`           | factory  | stream sink                             |
|  [04]   | `To(TextWriter[, leaveOpen])`       | factory  | writer sink                             |
|  [05]   | `To(StringBuilder)`                 | factory  | builder sink                            |
|  [06]   | `To(name, factory[, leaveOpen])`    | factory  | named sink                              |
|  [07]   | `writer.NewRow()`                   | instance | starts a writer row (commit on dispose) |
|  [08]   | `Col.Set(span)` / `Col.Set($"…")`   | instance | sets span or interpolated value         |
|  [09]   | `Col.Format(value[, format])`       | instance | formats `ISpanFormattable`              |
|  [10]   | `writer.Flush()`                    | instance | flushes pending rows to the sink        |

[ENTRYPOINT_SCOPE]: reader->writer copy bridge

`CopyTo`/`NewRow` copies a whole row or selectively re-emits columns — the transform-passthrough seam for redaction-pass and column-projection egress without materializing strings.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `writer.NewRow(SepReader.Row)`                    | instance | new writer row seeded from a reader row     |
|  [02]   | `writer.NewRow(SepReader.Row, CancellationToken)` | instance | cancellable seeded row                      |
|  [03]   | `readerRow.CopyTo(writerRow)`                     | instance | copies reader row columns into a writer row |

[ENTRYPOINT_SCOPE]: string-pool factories (`SepToString`)

`SepReaderOptions.CreateToString` selects how repeated column text interns; low-cardinality catalog columns collapse allocation to a pooled lookup, and the thread-safe variants are required under `ParallelEnumerate`.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `SepToString.Direct`                                         | static  | no pooling; new string per column read     |
|  [02]   | `SepToString.PoolPerCol(maxLen, initCap, maxCap)`            | factory | per-column pool                            |
|  [03]   | `SepToString.PoolPerColThreadSafe(maxLen, initCap, maxCap)`  | factory | thread-safe per-column pool                |
|  [04]   | `SepToString.PoolPerColThreadSafeFixedCapacity(maxLen, cap)` | factory | thread-safe fixed-capacity per-column pool |
|  [05]   | `SepToString.OnePool(maxLen, initCap, maxCap)`               | factory | single shared pool across all columns      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every read and write folds through a declared `SepReaderOptions`/`SepWriterOptions` profile; ref-struct `Row`/`Col`/`Cols` projections stay scope-bound and never escape the read, `Enumerate(RowFunc<T>)` the one boundary lifting them into `IEnumerable<T>`/`IAsyncEnumerable<T>` of domain records.

[STACKING]:
- `NodaTime`(`api-nodatime-stj`): a parsed `Col` span feeds the NodaTime STJ converters; a column typed `Instant`/`LocalDate` parses through `Col.Parse<T>` where `T : ISpanParsable<T>`, else its span hands to the converter read path.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): `Col.Parse<T>`/`TryParse<T>` admit `[ValueObject]`/`[SmartEnum]` types implementing `ISpanParsable<T>`, folding parse failure onto the `Fin`/`Validation` rail at the row boundary.
- `linq2db.EntityFrameworkCore`(`api-linq2db-ef`): `Enumerate(RowFunc<T>) -> IEnumerable<T>` and `EnumerateAsync -> IAsyncEnumerable<T>` source `LinqToDBForEFTools.BulkCopy<T>`/`BulkCopyAsync<T>`, streaming a CSV file into PostgreSQL binary COPY without buffering.
- `Apache.Arrow`(`api-arrow`)/`DuckDB`(`api-duckdb`): Sep owns the text-CSV codec, Arrow/DuckDB the binary columnar path; `Enumerate` projects rows into the record shape the columnar rail materializes, and Sep never re-implements a columnar reader.
- `Microsoft.Extensions.Compliance.Redaction`(`api-redaction`): the reader->writer `CopyTo`/`NewRow` bridge applies redaction per column before `Col.Set`, streaming redacted CSV egress without string materialization.
- within-lib: `Query/lane` consumes `Enumerate`/`ParallelEnumerate` as its row-projection seam; `Ingest/schedule` and `Ingest/tabular` drive schedule/cost CSV import-export through the profile.

[LOCAL_ADMISSION]:
- Sep is the separated-value codec for tabular interchange profiles; profile options (`Sep`, culture, trim, unset policy, pool factory) are declared, never inlined ad hoc.
- Typed parse rides `ISpanParsable<T>`; nullable optionality rides `TryParse<T>() -> T?` (struct-constrained), folded onto the `Fin`/`Validation` rail at the row boundary.
- `Cols.Parse<T>(Span<T>)`/`Header.IndicesOf(ReadOnlySpan<string>, Span<int>)` write into caller buffers on hot paths, keeping the row scope allocation-free.

[RAIL_LAW]:
- Package: `Sep`
- Owns: separated-value interchange — typed span parsing, header indexing, ref-struct row projection, parallel enumeration, pooled string interning, interpolated-string writes.
- Accept: profile-declared tabular reads/writes; `ISpanParsable<T>` typed columns; `RowFunc<T>` materialization at the scope boundary.
- Reject: hand-rolled CSV split/parse pipelines; `string.Split`; ref-struct rows escaping the read scope; per-column string-materialize-then-parse in hot paths; a bespoke columnar reader where Arrow/DuckDB owns the binary path.
