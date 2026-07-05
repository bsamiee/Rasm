# [RASM_PERSISTENCE_API_SEP]

`Sep` is a zero-allocation, SIMD-vectorized separated-value reader/writer over
`ReadOnlySpan<char>` columns: typed `ISpanParsable<T>` column parsing, span-typed
column-set parse-into, header-indexed and prefix-windowed column access, ref-struct
row/col projections that never escape the read scope, an `IAsyncEnumerator` mirror of
the synchronous enumerator, parallel row enumeration with a degree-of-parallelism
knob, interpolated-string column formatting, and a pluggable per-column string-pool
family. It is the column-codec for Persistence tabular interchange — schedule/cost
import-export (`Ingest/schedule`, `Ingest/tabular`), Arrow/DuckDB CSV bridge edges, and the
`Query/lane` row-projection seam — feeding parsed spans straight into the
NodaTime/Thinktecture wire converters and the EF/linq2db bulk rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sep`
- package: `Sep`
- version: `0.15.0`
- license: `MIT`
- assembly: `Sep`
- namespace: `nietras.SeparatedValues`
- asset: runtime library
- target: net10.0 (consumer-bound; multi-targets net8.0/net9.0/net10.0 — the net10.0 asset binds)
- abi: ref-struct + `allows ref struct` generic delegates; requires C# 13 / net9+ `ref struct` interfaces for the `IDisposable`/`IAsyncDisposable` ref-struct rows
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[ENTRY_TYPES]: specification and options
- rail: interchange-codec

`Sep` is a `readonly record struct` wrapping a single `Separator char`; `SepReaderOptions`
and `SepWriterOptions` are `readonly record struct` option bags mutated through `with` or
the `Func<Options,Options>` configure lambdas. `SepColNotSetOption` (`Throw`/`Empty`/`Skip`)
binds through `SepWriterOptions.ColNotSetOption` only — there is no reader unset knob.
`SepTrim` (`None=0`/`Outer=1`/`AfterUnescape=2`/`All=3`, `[Flags]`-combinable) is the trim
classifier — `Outer` strips surrounding whitespace, `AfterUnescape` trims post-unescape,
`All` = both. `SepCreateToString` is the
`(SepReaderHeader?, int colCount) -> SepToString` pool-factory delegate.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]    | [CAPABILITY]                                      |
| :-----: | :------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `Sep`                | separator root    | `readonly record struct`; selects separator       |
|  [02]   | `SepSpec`            | culture spec      | `readonly record struct`; binds `Sep` + culture   |
|  [03]   | `SepDefaults`        | default anchors   | static default separator/culture policy           |
|  [04]   | `SepReaderOptions`   | reader options    | `readonly record struct`; parse policy bag        |
|  [05]   | `SepWriterOptions`   | writer options    | `readonly record struct`; emit policy bag         |
|  [06]   | `SepTrim`            | trim classifier   | `[Flags] enum byte` `None`/`Outer`/`AfterUnescape`/`All` |
|  [07]   | `SepColNotSetOption` | unset classifier  | `enum byte` `Throw`/`Empty`/`Skip` (writer only)  |
|  [08]   | `SepToString`        | string-pool root  | abstract pool; `Direct` + 4 pool factories        |
|  [09]   | `SepCreateToString`  | pool delegate     | `(header,colCount) -> SepToString` factory         |

[READER_TYPES]: read surfaces
- rail: interchange-codec

`SepReader` is the synchronous enumerator (`IEnumerable<Row>`/`IEnumerator<Row>`) AND the
async-enumerable source (`IAsyncEnumerable<Row>`); rows/cols are `ref struct` projections.

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]                                           |
| :-----: | :------------------------------------ | :--------------- | :---------------------------------------------------- |
|  [01]   | `SepReader`                           | reader root      | sync + async row enumeration over one buffer          |
|  [02]   | `SepReader.Row`                       | row ref struct   | `ColCount` live width; column indexer; `Span`/`Join`/`Select` |
|  [03]   | `SepReader.Col`                       | col ref struct   | `Span`/`Parse<T>`/`TryParse<T>`                        |
|  [04]   | `SepReader.Cols`                      | cols ref struct  | column-set `Parse`/`Select`/`Join` projections        |
|  [05]   | `SepReader.RowFunc<T>`                | projection delegate | `(Row) -> T where T : allows ref struct`           |
|  [06]   | `SepReader.RowTryFunc<T>`             | try-projection delegate | `(Row, out T) -> bool`; skips on false        |
|  [07]   | `SepReader.ColFunc<T>`                | col-projection delegate | `(Col) -> T where T : allows ref struct`      |
|  [08]   | `SepReader.AsyncEnumerator`           | async surface    | `IAsyncEnumerator<Row>`/`IAsyncDisposable`            |
|  [09]   | `SepReaderHeader`                     | header surface   | name/span index resolution + prefix window            |
|  [10]   | `SepReaderExtensions`                 | reader factory   | option builders, `From*`/`From*Async`, `Enumerate*`   |

[WRITER_TYPES]: write surfaces
- rail: interchange-codec

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]   | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `SepWriter`                               | writer root      | `IDisposable`/`IAsyncDisposable` row emitter         |
|  [02]   | `SepWriter.Row`                           | row ref struct   | `IDisposable`/`IAsyncDisposable`; commits on dispose  |
|  [03]   | `SepWriter.Col`                           | col ref struct   | `Set`/`Format`; interpolated-string handler          |
|  [04]   | `SepWriter.Col.FormatInterpolatedStringHandler` | format handler | `[InterpolatedStringHandler]` zero-alloc col write |
|  [05]   | `SepWriter.Cols`                          | cols ref struct  | column-set `Set`                                     |
|  [06]   | `SepWriterHeader`                         | header surface   | owns written header                                  |
|  [07]   | `SepWriterExtensions`                     | writer factory   | option builders, `To*`, `Strict`                     |
|  [08]   | `SepReaderWriterExtensions`               | copy bridge      | `NewRow(readerRow)` + `CopyTo` reader->writer         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: option construction (functional-config preferred)
- rail: interchange-codec

The functional-config form (`Func<Options,Options> configure`) is the canonical seam — it
composes with the repo's options-builder discipline; the bare form plus `with` is the
fallback. `Sep.Auto` returns a `Sep?` of `null`, which `Reader(this Sep?)` reads as
"auto-detect separator from the first row".

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]        | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------- | :------------------ | :------------------------------------------------- |
|  [01]   | `Sep.New(char)` / `new Sep(char)`              | factory             | builds an explicit separator                       |
|  [02]   | `Sep.Default`                                  | static property     | `SepDefaults.Separator` (`;`)                      |
|  [03]   | `Sep.Auto`                                     | static property     | `Sep?` = `null`; routes to auto-detect             |
|  [04]   | `Sep.Reader()` / `sep.Reader()`                | options factory     | starts `SepReaderOptions`                          |
|  [05]   | `Sep.Reader(Func<…>)` / `sep.Reader(Func<…>)`  | options factory     | starts + configures reader options in one call     |
|  [06]   | `spec.Reader(…)`                               | options factory     | reader options from a `SepSpec` (culture-bound)    |
|  [07]   | `Strict`                                        | options extension   | `in SepReaderOptions`/`in SepWriterOptions`; hardens col-count + quoting |
|  [08]   | `Sep.Writer()` / `sep.Writer(Func<…>)`         | options factory     | starts + configures writer options                 |

[ENTRYPOINT_SCOPE]: reader options surface (`readonly record struct SepReaderOptions`)
- rail: interchange-codec

The full parse-policy bag — the catalog-level reader is configured entirely through these
`init` members, not a thin culture/trim subset.

| [INDEX] | [MEMBER]                          | [TYPE]                  | [NOTES]                                             |
| :-----: | :-------------------------------- | :---------------------- | :------------------------------------------------- |
|  [01]   | `Sep`                             | `Sep?`                  | `null` = auto-detect separator                     |
|  [02]   | `CultureInfo`                     | `CultureInfo?`          | parse/format culture; `null` = invariant fast path |
|  [03]   | `HasHeader`                       | `bool`                  | default true; header drives name indexing          |
|  [04]   | `ColNameComparer`                 | `IEqualityComparer<string>` | header name lookup comparer                    |
|  [05]   | `Trim`                            | `SepTrim`               | `None`/`Outer`/`AfterUnescape`/`All` (`[Flags]`)   |
|  [06]   | `Unescape`                        | `bool`                  | unescape quoted columns in place                   |
|  [07]   | `DisableFastFloat`                | `bool`                  | opt out of the vectorized float parser             |
|  [08]   | `DisableColCountCheck`            | `bool`                  | allow ragged rows                                  |
|  [09]   | `DisableQuotesParsing`            | `bool`                  | treat quotes as literal characters                 |
|  [10]   | `InitialBufferLength`             | `int`                   | initial read buffer sizing                         |
|  [11]   | `CreateToString`                  | `SepCreateToString`     | per-column string-pool factory (see pool table)    |
|  [12]   | `AsyncContinueOnCapturedContext`  | `bool`                  | async `From*Async` continuation context            |

[ENTRYPOINT_SCOPE]: reader sources — sync + async mirror
- rail: interchange-codec

Every source has a sync `From*` and an async `From*Async`/`ValueTask<SepReader>` mirror;
the `name + factory` overloads admit a named stream/reader for diagnostics, with a
`leaveOpen` variant. `From*` are `in SepReaderOptions` extensions.

| [INDEX] | [SURFACE]                                              | [RETURNS]               | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :---------------------- | :-------------------------------------------- |
|  [01]   | `FromText(string)`                                     | `SepReader`             | string source                                 |
|  [02]   | `FromFile(string)`                                     | `SepReader`             | file source                                   |
|  [03]   | `From(byte[])` / `From(Stream[, leaveOpen])`           | `SepReader`             | buffer / stream source                        |
|  [04]   | `From(TextReader[, leaveOpen])`                        | `SepReader`             | reader source                                 |
|  [05]   | `From(name, Func<string,Stream>[, leaveOpen])`         | `SepReader`             | named-stream source                           |
|  [06]   | `FromTextAsync` / `FromFileAsync` / `FromAsync(…)`     | `ValueTask<SepReader>`  | async mirror of [01]-[05]                      |

[ENTRYPOINT_SCOPE]: row / column access (ref-struct, scope-bound)
- rail: interchange-codec

`Parse`/`TryParse` constrain to `ISpanParsable<T>`; the `struct`-constrained `TryParse<T>`
returns `T?`. `Cols.Parse<T>(Span<T>)` parses a whole column set into a caller buffer
(zero alloc), and `Cols.Select<T>(ColFunc<T>)` projects each column through a delegate.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]    | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `MoveNext` / `MoveNextAsync`                     | reader call     | advances row (sync / `ValueTask<bool>`)            |
|  [02]   | `Current`                                        | reader property | current `Row` ref struct                           |
|  [03]   | `Row[name]` / `Row[index]` / `Row[range]`        | row indexer     | selects `Col`/`Cols`                               |
|  [04]   | `Row.ColCount` / `Row.Span` / `Row.Join(sep)` / `Row.Select<T>` | row projection | live column count (`int`) / whole-row span / joined span / delegate project |
|  [05]   | `Col.Span`                                       | col property    | `ReadOnlySpan<char>` column chars                  |
|  [06]   | `Col.Parse<T>()`                                 | col call        | `where T : ISpanParsable<T>`                       |
|  [07]   | `Col.TryParse<T>()`                              | col call        | `where T : struct, ISpanParsable<T>` -> `T?`        |
|  [08]   | `Cols.Parse<T>()` / `Cols.Parse<T>(Span<T>)`     | cols call       | parse set -> `Span<T>` or into caller buffer       |
|  [09]   | `Cols.TryParse<T>()` / `Cols.TryParse<T>(Span<T?>)` | cols call    | nullable struct set parse (`T : struct`)           |
|  [10]   | `Cols.Select<T>(ColFunc<T>)`                     | cols call       | delegate-project each column -> `Span<T>`          |
|  [11]   | `Header.IndexOf(string)` / `IndexOf(ReadOnlySpan<char>)` | header call | resolve column index (string / span)            |
|  [12]   | `Header.TryIndexOf(…, out int)`                  | header call     | non-throwing index resolution                      |
|  [13]   | `Header.IndicesOf(IReadOnlyList<string>` / `params` / `Span<string>, Span<int>)` | header call | resolve many; the `Span<int>` form writes into a caller buffer |
|  [14]   | `Header.NamesStartingWith(prefix[, StringComparison])` | header call | prefixed column-name window (default `Ordinal`) |
|  [15]   | `Header.ColNames` / `Header.IsEmpty` / `Header.Empty` | header members | name roster / emptiness / sentinel              |

[ENTRYPOINT_SCOPE]: row enumeration projections — sync, async, parallel
- rail: interchange-codec

Each projector has a `RowFunc<T>` (project) and a `RowTryFunc<T>` (project-or-skip)
overload; `ParallelEnumerate` adds a `degreeOfParallelism` overload. These are the
canonical seam into `Query/lane` — `Enumerate` yields `IEnumerable<T>` of materialized
records out of ref-struct rows.

| [INDEX] | [SURFACE]                                        | [RETURNS]                 | [CAPABILITY]                              |
| :-----: | :----------------------------------------------- | :------------------------ | :---------------------------------------- |
|  [01]   | `Enumerate(RowFunc<T>)` / `Enumerate(RowTryFunc<T>)` | `IEnumerable<T>`       | project / project-or-skip rows            |
|  [02]   | `EnumerateAsync(RowFunc<T>)` / `(RowTryFunc<T>)`  | `IAsyncEnumerable<T>`     | async mirror                              |
|  [03]   | `ParallelEnumerate(RowFunc<T>)` / `(RowTryFunc<T>)` | `IEnumerable<T>`        | parallel project; preserves row order     |
|  [04]   | `ParallelEnumerate(RowFunc<T>, int degreeOfParallelism)` | `IEnumerable<T>`  | parallel project, bounded fan             |

[ENTRYPOINT_SCOPE]: writer options + sinks + column writes
- rail: interchange-codec

`SepWriterOptions` `init` members: `Sep`, `CultureInfo`, `WriteHeader`, `Escape`,
`DisableColCountCheck`, `ColNotSetOption`, `AsyncContinueOnCapturedContext`. `Col.Set`
accepts a span or an interpolated string via `FormatInterpolatedStringHandler`;
`Col.Format` writes an `ISpanFormattable` with a format/culture.

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]      | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `Sep.Writer()` / `sep.Writer(Func<…>)`           | options factory   | starts writer options                           |
|  [02]   | `ToText()` / `ToText(int capacity)`              | options extension | string sink (capacity-presized)                 |
|  [03]   | `ToFile(string)`                                 | options extension | file sink                                       |
|  [04]   | `To(Stream[, leaveOpen])` / `To(TextWriter[, leaveOpen])` / `To(StringBuilder)` / `To(name, factory[, leaveOpen])` | options extension | stream / writer / builder / named sink |
|  [05]   | `writer.NewRow()`                                | writer call       | starts a writer row (commit on dispose)         |
|  [06]   | `Col.Set(span)` / `Col.Set($"…")`                | col call          | sets span or interpolated value                 |
|  [07]   | `Col.Format(value[, format, provider])`          | col call          | formats `ISpanFormattable`                      |
|  [08]   | `writer.Flush()`                                 | writer call       | flushes pending rows to the sink                |

[ENTRYPOINT_SCOPE]: reader->writer copy bridge
- rail: interchange-codec

The transform-passthrough seam: open a reader and a writer, then per row either copy the
whole row (`NewRow(readerRow)` / `CopyTo`) or selectively re-emit columns — the basis for
redaction-pass and column-projection egress without materializing strings.

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]      | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `writer.NewRow(SepReader.Row)`                   | copy extension    | new writer row seeded from a reader row         |
|  [02]   | `writer.NewRow(SepReader.Row, CancellationToken)`| copy extension    | cancellable seeded row                          |
|  [03]   | `readerRow.CopyTo(writerRow)`                    | copy extension    | copies reader row columns into a writer row     |

[ENTRYPOINT_SCOPE]: string-pool factories (`SepToString`)
- rail: interchange-codec

`SepReaderOptions.CreateToString` selects how repeated column text is interned. For
catalog/identity columns with low cardinality this collapses string allocation to a pooled
lookup; the thread-safe variants are required when rows are consumed under
`ParallelEnumerate`.

| [INDEX] | [SURFACE]                                              | [THREAD_SAFE] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :-----------: | :-------------------------------------------- |
|  [01]   | `SepToString.Direct`                                   | n/a           | no pooling; new string per column read        |
|  [02]   | `SepToString.PoolPerCol(maxLen, initCap, maxCap)`      | no            | per-column pool                               |
|  [03]   | `SepToString.PoolPerColThreadSafe(maxLen, initCap, maxCap)` | yes      | per-column pool for parallel consumers        |
|  [04]   | `SepToString.PoolPerColThreadSafeFixedCapacity(maxLen, capacity)` | yes  | fixed-capacity per-column pool                |
|  [05]   | `SepToString.OnePool(maxLen, initCap, maxCap)`         | no            | single shared pool across all columns         |

## [04]-[IMPLEMENTATION_LAW]

[INTERCHANGE_PROFILE]:
- namespace: `nietras.SeparatedValues`
- entry root: `Sep` `readonly record struct`
- reader root: `SepReader` ref-struct rows over column spans (sync + `IAsyncEnumerable`)
- writer root: `SepWriter` ref-struct rows over column sinks
- pooling root: `SepToString` string-pool family on `SepReaderOptions.CreateToString`
- projection: `RowFunc<T>`/`RowTryFunc<T>`/`ColFunc<T>` delegates carry `allows ref struct`

[LOCAL_ADMISSION]:
- Sep is the separated-value codec for tabular interchange profiles; profile options
  (`Sep`, culture, trim, unset policy, pool factory) are declared, never inlined ad hoc.
- Rows, cols, and headers are `ref struct` projections; they never escape the read scope.
  Materialization is a single `Enumerate(RowFunc<T>)` boundary that lifts ref-struct rows
  into an `IEnumerable<T>`/`IAsyncEnumerable<T>` of domain records.
- Typed parse uses `ISpanParsable<T>` — never `string`-materialize-then-`Parse`. Nullable
  optionality rides `TryParse<T>() -> T?` (struct-constrained), folded into the LanguageExt
  `Fin`/`Validation` rail at the row boundary.
- `Cols.Parse<T>(Span<T>)`/`Header.IndicesOf(Span<string>, Span<int>)` write into caller
  buffers; prefer them in hot paths to keep the row scope allocation-free.

[STACKING_LAW]:
- Wire converters: a parsed `Col` span feeds the NodaTime STJ converters
  (`api-nodatime-stj`) and the Thinktecture value-object/smart-enum factories — Sep parses
  the raw ordinate/identifier span, the wire rail validates it into the semantic-time or
  value-object type. CSV columns carrying `Instant`/`LocalDate`/value-objects parse through
  `Col.Parse<T>` where `T : ISpanParsable<T>` directly when the type implements the
  interface; otherwise the column span is handed to the converter's read path.
- Bulk ingress: `Enumerate(RowFunc<T>)` -> `IEnumerable<T>` is the source for
  `LinqToDBForEFTools.BulkCopy<T>`/`BulkCopyAsync<T>` (`api-linq2db-ef`); `EnumerateAsync`
  feeds the `IAsyncEnumerable<T>` bulk overloads, streaming a CSV file into PostgreSQL
  binary COPY without buffering the file.
- Columnar edge: Sep is the text-CSV codec; Arrow/DuckDB own the binary columnar path.
  Sep reads the schedule/cost CSV interchange, `Enumerate` projects rows into the record
  shape, and the columnar rail materializes the Arrow batch — Sep never re-implements a
  columnar reader.
- Redaction/egress: the reader->writer `CopyTo`/`NewRow` bridge re-emits rows column-by-
  column, applying `Microsoft.Extensions.Compliance.Redaction` per column before
  `Col.Set`, so redacted CSV egress streams without string materialization.

[RAIL_LAW]:
- Package: `Sep`
- Owns: separated-value interchange — typed span parsing, header indexing, ref-struct row
  projection, parallel enumeration, pooled string interning, interpolated-string writes
- Accept: profile-declared tabular reads/writes; `ISpanParsable<T>` typed columns;
  `RowFunc<T>` materialization at the scope boundary
- Reject: hand-rolled CSV split/parse pipelines; `string.Split`; ref-struct rows escaping
  the read scope; per-column `string`-materialize-then-parse in hot paths; a bespoke
  columnar reader where Arrow/DuckDB owns the binary path
