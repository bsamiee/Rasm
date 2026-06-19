# [RASM_PERSISTENCE_API_SEP]

`Sep` supplies span-based separated-value reading and writing, typed column
parsing, header-indexed column access, row enumeration projections, and
parallel enumeration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sep`
- package: `Sep`
- assembly: `Sep`
- namespace: `nietras.SeparatedValues`
- asset: runtime library
- rail: interchange-codec

## [02]-[PUBLIC_TYPES]

[ENTRY_TYPES]: specification and options
- rail: interchange-codec

`SepColNotSetOption` values are `Throw`, `Empty`, and `Skip`; the knob binds through `SepWriterOptions.ColNotSetOption`, not reader options.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]   | [CAPABILITY]          |
| :-----: | :------------------- | :--------------- | :-------------------- |
|  [01]   | `Sep`                | separator root   | selects separator     |
|  [02]   | `SepSpec`            | specification    | carries culture       |
|  [03]   | `SepDefaults`        | default anchors  | states default policy |
|  [04]   | `SepReaderOptions`   | reader options   | configures parsing    |
|  [05]   | `SepWriterOptions`   | writer options   | configures emission   |
|  [06]   | `SepTrim`            | trim classifier  | selects trimming      |
|  [07]   | `SepColNotSetOption` | unset classifier | handles unset columns |
|  [08]   | `SepToString`        | string pool root | pools column strings  |
|  [09]   | `SepCreateToString`  | pool delegate    | selects string pool   |

[READER_TYPES]: read surfaces
- rail: interchange-codec

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]               |
| :-----: | :-------------------------- | :-------------- | :------------------------- |
|  [01]   | `SepReader`                 | reader root     | enumerates rows            |
|  [02]   | `SepReader.Row`             | row ref struct  | indexes columns            |
|  [03]   | `SepReader.Col`             | col ref struct  | exposes column span        |
|  [04]   | `SepReader.Cols`            | cols ref struct | projects column sets       |
|  [05]   | `SepReaderHeader`           | header surface  | resolves column indices    |
|  [06]   | `SepReader.AsyncEnumerator` | async surface   | enumerates rows async      |
|  [07]   | `SepReaderExtensions`       | reader factory  | opens readers and projects |

[WRITER_TYPES]: write surfaces
- rail: interchange-codec

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :-------------------------- | :-------------- | :---------------------- |
|  [01]   | `SepWriter`                 | writer root     | emits rows              |
|  [02]   | `SepWriter.Row`             | row ref struct  | indexes write columns   |
|  [03]   | `SepWriter.Col`             | col ref struct  | sets and formats values |
|  [04]   | `SepWriter.Cols`            | cols ref struct | sets column sets        |
|  [05]   | `SepWriterHeader`           | header surface  | owns written header     |
|  [06]   | `SepWriterExtensions`       | writer factory  | opens writers           |
|  [07]   | `SepReaderWriterExtensions` | copy surface    | copies reader rows      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reader construction
- rail: interchange-codec

| [INDEX] | [SURFACE]       | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :-------------- | :---------------- | :------------------------ |
|  [01]   | `Sep.Reader`    | options factory   | starts reader options     |
|  [02]   | `Sep.Auto`      | static property   | detects separator         |
|  [03]   | `Strict`        | options extension | hardens parsing           |
|  [04]   | `FromText`      | options extension | reads string source       |
|  [05]   | `FromFile`      | options extension | reads file source         |
|  [06]   | `From`          | options extension | reads stream or reader    |
|  [07]   | `FromTextAsync` | options extension | reads string source async |
|  [08]   | `FromFileAsync` | options extension | reads file source async   |
|  [09]   | `FromAsync`     | options extension | reads stream async        |

[ENTRYPOINT_SCOPE]: row and column access
- rail: interchange-codec

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]    | [CAPABILITY]                         |
| :-----: | :------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `MoveNext`                 | reader call     | advances row                         |
|  [02]   | `MoveNextAsync`            | reader call     | advances row async                   |
|  [03]   | `Current`                  | reader property | exposes current row                  |
|  [04]   | `Row[..]`                  | row indexer     | selects columns                      |
|  [05]   | `Col.Span`                 | col property    | exposes column chars                 |
|  [06]   | `Col.Parse`                | col call        | parses `ISpanParsable`               |
|  [07]   | `Col.TryParse`             | col call        | parses optionally                    |
|  [08]   | `Cols.Parse`               | cols call       | parses column set                    |
|  [09]   | `Cols.Select`              | cols call       | projects column set                  |
|  [10]   | `Header.IndexOf`           | header call     | resolves column index                |
|  [11]   | `Header.IndicesOf`         | header call     | resolves column indices              |
|  [12]   | `Header.NamesStartingWith` | header call     | resolves prefixed column-name window |

`Header.NamesStartingWith` accepts a prefix and optional `StringComparison`, defaulting to `StringComparison.Ordinal`.

[ENTRYPOINT_SCOPE]: row enumeration
- rail: interchange-codec

| [INDEX] | [SURFACE]           | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :------------------ | :--------------- | :------------------------ |
|  [01]   | `Enumerate`         | reader extension | projects rows to values   |
|  [02]   | `EnumerateAsync`    | reader extension | projects rows async       |
|  [03]   | `ParallelEnumerate` | reader extension | projects rows in parallel |

[ENTRYPOINT_SCOPE]: writer operations
- rail: interchange-codec

| [INDEX] | [SURFACE]    | [CALL_SHAPE]      | [CAPABILITY]                    |
| :-----: | :----------- | :---------------- | :------------------------------ |
|  [01]   | `Sep.Writer` | options factory   | starts writer options           |
|  [02]   | `ToText`     | options extension | writes string sink              |
|  [03]   | `ToFile`     | options extension | writes file sink                |
|  [04]   | `To`         | options extension | writes stream or writer         |
|  [05]   | `NewRow`     | writer call       | starts row, copies reader rows  |
|  [06]   | `Col.Set`    | col call          | sets span or interpolated value |
|  [07]   | `Col.Format` | col call          | formats `ISpanFormattable`      |
|  [08]   | `CopyTo`     | row extension     | copies reader row to writer row |
|  [09]   | `Flush`      | writer call       | flushes written rows            |

## [04]-[IMPLEMENTATION_LAW]

[INTERCHANGE_PROFILE]:
- namespace: `nietras.SeparatedValues`
- entry root: `Sep`
- reader root: `SepReader` rows over column spans
- writer root: `SepWriter` rows over column sinks
- pooling root: `SepToString` string pools

[LOCAL_ADMISSION]:
- Sep is the separated-value codec for tabular interchange profiles.
- Rows and columns are ref-struct projections and never escape the read scope.
- Separator, culture, trim, and unset policy are profile-declared options.
- Typed column parsing uses `ISpanParsable` values, not string materialization.

[RAIL_LAW]:
- Package: `Sep`
- Owns: separated-value interchange
- Accept: profile-declared tabular reads and writes
- Reject: hand-rolled CSV parsing or string-split pipelines
