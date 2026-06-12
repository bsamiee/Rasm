# [RASM_PERSISTENCE_API_SEP]

`Sep` supplies span-based separated-value reading and writing, typed column
parsing, header-indexed column access, row enumeration projections, and
parallel enumeration.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sep`
- package: `Sep`
- assembly: `Sep`
- namespace: `nietras.SeparatedValues`
- asset: runtime library
- rail: interchange-codec

## [2]-[PUBLIC_TYPES]

[ENTRY_TYPES]: specification and options
- rail: interchange-codec

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :------------------- | :--------------- | :------------------------ |
|   [1]   | `Sep`                | separator root   | selects separator         |
|   [2]   | `SepSpec`            | specification    | carries separator culture |
|   [3]   | `SepDefaults`        | default anchors  | states default policy     |
|   [4]   | `SepReaderOptions`   | reader options   | configures parsing        |
|   [5]   | `SepWriterOptions`   | writer options   | configures emission       |
|   [6]   | `SepTrim`            | trim classifier  | selects trimming          |
|   [7]   | `SepColNotSetOption` | unset classifier | selects unset handling    |
|   [8]   | `SepToString`        | string pool root | pools column strings      |
|   [9]   | `SepCreateToString`  | pool factory     | creates string pools      |

[READER_TYPES]: read surfaces
- rail: interchange-codec

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]               |
| :-----: | :-------------------------- | :-------------- | :------------------------- |
|   [1]   | `SepReader`                 | reader root     | enumerates rows            |
|   [2]   | `SepReader.Row`             | row ref struct  | indexes columns            |
|   [3]   | `SepReader.Col`             | col ref struct  | exposes column span        |
|   [4]   | `SepReader.Cols`            | cols ref struct | projects column sets       |
|   [5]   | `SepReaderHeader`           | header surface  | resolves column indices    |
|   [6]   | `SepReader.AsyncEnumerator` | async surface   | enumerates rows async      |
|   [7]   | `SepReaderExtensions`       | reader factory  | opens readers and projects |

[WRITER_TYPES]: write surfaces
- rail: interchange-codec

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :-------------------------- | :-------------- | :---------------------- |
|   [1]   | `SepWriter`                 | writer root     | emits rows              |
|   [2]   | `SepWriter.Row`             | row ref struct  | indexes write columns   |
|   [3]   | `SepWriter.Col`             | col ref struct  | sets and formats values |
|   [4]   | `SepWriter.Cols`            | cols ref struct | sets column sets        |
|   [5]   | `SepWriterHeader`           | header surface  | owns written header     |
|   [6]   | `SepWriterExtensions`       | writer factory  | opens writers           |
|   [7]   | `SepReaderWriterExtensions` | copy surface    | copies reader rows      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reader construction
- rail: interchange-codec

| [INDEX] | [SURFACE]       | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :-------------- | :---------------- | :------------------------ |
|   [1]   | `Sep.Reader`    | options factory   | starts reader options     |
|   [2]   | `Sep.Auto`      | static property   | detects separator         |
|   [3]   | `Strict`        | options extension | hardens parsing           |
|   [4]   | `FromText`      | options extension | reads string source       |
|   [5]   | `FromFile`      | options extension | reads file source         |
|   [6]   | `From`          | options extension | reads stream or reader    |
|   [7]   | `FromTextAsync` | options extension | reads string source async |
|   [8]   | `FromFileAsync` | options extension | reads file source async   |
|   [9]   | `FromAsync`     | options extension | reads stream async        |

[ENTRYPOINT_SCOPE]: row and column access
- rail: interchange-codec

| [INDEX] | [SURFACE]          | [CALL_SHAPE]    | [CAPABILITY]                          |
| :-----: | :----------------- | :-------------- | :------------------------------------ |
|   [1]   | `MoveNext`         | reader call     | advances row                          |
|   [2]   | `MoveNextAsync`    | reader call     | advances row async                    |
|   [3]   | `Current`          | reader property | exposes current row                   |
|   [4]   | `Row[..]`          | row indexer     | selects cols by index, name, or range |
|   [5]   | `Col.Span`         | col property    | exposes column chars                  |
|   [6]   | `Col.Parse`        | col call        | parses `ISpanParsable`                |
|   [7]   | `Col.TryParse`     | col call        | parses optionally                     |
|   [8]   | `Cols.Parse`       | cols call       | parses column set                     |
|   [9]   | `Cols.Select`      | cols call       | projects column set                   |
|  [10]   | `Header.IndexOf`   | header call     | resolves column index                 |
|  [11]   | `Header.IndicesOf` | header call     | resolves column indices               |

[ENTRYPOINT_SCOPE]: row enumeration
- rail: interchange-codec

| [INDEX] | [SURFACE]           | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :------------------ | :--------------- | :------------------------ |
|   [1]   | `Enumerate`         | reader extension | projects rows to values   |
|   [2]   | `EnumerateAsync`    | reader extension | projects rows async       |
|   [3]   | `ParallelEnumerate` | reader extension | projects rows in parallel |

[ENTRYPOINT_SCOPE]: writer operations
- rail: interchange-codec

| [INDEX] | [SURFACE]    | [CALL_SHAPE]      | [CAPABILITY]                    |
| :-----: | :----------- | :---------------- | :------------------------------ |
|   [1]   | `Sep.Writer` | options factory   | starts writer options           |
|   [2]   | `ToText`     | options extension | writes string sink              |
|   [3]   | `ToFile`     | options extension | writes file sink                |
|   [4]   | `To`         | options extension | writes stream or writer         |
|   [5]   | `NewRow`     | writer call       | starts row, copies reader rows  |
|   [6]   | `Col.Set`    | col call          | sets span or interpolated value |
|   [7]   | `Col.Format` | col call          | formats `ISpanFormattable`      |
|   [8]   | `CopyTo`     | row extension     | copies reader row to writer row |
|   [9]   | `Flush`      | writer call       | flushes written rows            |

## [4]-[IMPLEMENTATION_LAW]

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
