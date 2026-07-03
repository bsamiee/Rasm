# [API_CATALOGUE] papaparse

`papaparse` supplies two functions — `parse` (a 5-overload dispatch across synchronous string, web-worker, remote-URL, local-`File`/`Readable`, and node-`Duplex` modes) and `unparse` (row-array or `{fields,data}` → CSV string) — plus the `Parser` control class, three writable global config `let`s, six read-only constants, the rich parse/unparse config surface, and the result/error/meta model. In `services` the load-bearing role is the CSV codec in the `persistence/object#OBJECT_STORE AssetCodec` fan-out: `Papa.unparse(rows, { escapeFormulae: true })` is the `csv` export arm — one of four codecs (`papaparse`/`exceljs`/`jspdf`/`jszip`) collapsed onto one `Schema.Literal("csv","xlsx","pdf","archive")` axis folded by `Match`, each producing a byte source that streams straight into one `ObjectStore.put`. The symmetric ingress mirror is `parse` streaming — `NODE_STREAM_INPUT` yields a `Duplex` that pipes an `ObjectStore.get` body (or an `@effect/platform` `Multipart` part) into typed rows without buffering the whole file. Types are `@types/papaparse` (DefinitelyTyped, `export as namespace Papa`); the runtime ships UMD/CommonJS only — `import Papa from "papaparse"` default-interop, no ESM export map.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papaparse`
- package: `papaparse` (5.5.4, MIT, © Matthew Holt) + types `@types/papaparse` (5.5.2, MIT)
- module format: UMD/CommonJS runtime (`main: papaparse.js`, browser `papaparse.min.js`, UMD global `Papa`; NO ESM `module`/`exports`) — consume via default-import interop `import Papa from "papaparse"`; ambient types via `@types/papaparse`
- runtime target: browser + node; the `parse(NODE_STREAM_INPUT)` `Duplex` and `LocalFile = File | NodeJS.ReadableStream` paths pull `/// <reference types="node" />` (node-only), and the worker path is gated by `WORKERS_SUPPORTED`
- surface: `parse` (5 overloads) + `unparse`; the `Parser` control class; 3 writable global `let`s; 6 read-only constants; the `ParseConfig`/`ParseWorkerConfig`/`ParseLocalConfig`/`ParseRemoteConfig`/`UnparseConfig` config surface; the `ParseResult`/`ParseStepResult`/`ParseError`/`ParseMeta` model
- consumer: `persistence/object#OBJECT_STORE AssetCodec` (`csv` arm via `unparse`, streamed into `ObjectStore.put` beside `exceljs`/`jspdf`/`jszip`); a CSV-ingest consumer drives `parse` streaming into rows
- rail: csv-codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parse configuration (the discriminant is which callback + `worker`/`download` the shape carries)

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ParseConfig<T,TInput>` | interface | base — `delimiter`, `newline`, `quoteChar`, `escapeChar`, `header`, `transformHeader`, `dynamicTyping`, `preview`, `comments`, `skipEmptyLines`, `fastMode`, `transform`, `delimitersToGuess`, `step`, `complete`, `beforeFirstChunk`, `skipFirstNLines` |
|  [02]   | `ParseWorkerConfig<T>`  | interface | `extends ParseConfig` with `worker: true` + required `complete` — off-main-thread parse                        |
|  [03]   | `ParseLocalConfig<T,TFile>` | type   | `step \| complete` union over `ParseAsyncConfigBase` + `encoding`, `chunk`, `chunkSize`, `error` — `File`/`Readable` |
|  [04]   | `ParseRemoteConfig<T>`  | type      | `download: true` + `downloadRequestHeaders`, `downloadRequestBody`, `withCredentials`, `step \| complete` — URL fetch |

[PUBLIC_TYPE_SCOPE]: unparse configuration

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                                                    |
| :-----: | :--------------- | :------------ | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `UnparseConfig`  | interface     | `quotes` (`bool \| bool[] \| (v,i)=>bool`), `quoteChar`, `escapeChar`, `delimiter`, `header`, `newline`, `skipEmptyLines`, `columns`, `escapeFormulae` (`bool \| RegExp`) |
|  [02]   | `UnparseObject<T>` | interface   | `{ fields: string[]; data: T[] }` — explicit column order for object rows                                       |

[PUBLIC_TYPE_SCOPE]: result, error, and meta model (`ParseError` carries closed literal vocabularies)

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ParseResult<T>`    | interface     | `{ data: T[]; errors: ParseError[]; meta: ParseMeta }` — buffered (`complete`) result                          |
|  [02]   | `ParseStepResult<T>`| interface     | `{ data: T; errors: ParseError[]; meta: ParseMeta }` — single row in the `step` callback                       |
|  [03]   | `ParseError`        | interface     | closed `type`: `"Quotes" \| "Delimiter" \| "FieldMismatch"`; closed `code`: `"MissingQuotes" \| "UndetectableDelimiter" \| "TooFewFields" \| "TooManyFields" \| "InvalidQuotes"`; `message`, `row?`, `index?` |
|  [04]   | `ParseMeta`         | interface     | `delimiter`, `linebreak`, `aborted`, `fields?`, `truncated`, `cursor`, `renamedHeaders?` (dedup map on duplicate headers) |

[PUBLIC_TYPE_SCOPE]: control class, constants, and writable globals

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]       | [CAPABILITY]                                                                    |
| :-----: | :------------------ | :------------------ | :----------------------------------------------------------------------------- |
|  [01]   | `Parser`            | class               | `constructor(config)`, `parse(input, baseIndex, ignoreLastRow)`, `abort()`, `getCharIndex()`, `pause()`, `resume()` — the streaming-control handle passed to `step` |
|  [02]   | `LocalFile`         | type                | `File \| NodeJS.ReadableStream` — the local-source union                        |
|  [03]   | `NODE_STREAM_INPUT` | `unique symbol`     | sentinel first-arg requesting a node `Duplex` stream                            |
|  [04]   | `BAD_DELIMITERS`    | `readonly string[]` | disallowed delimiters (`\r`, `\n`, `"`, `﻿`) — a chosen delimiter must avoid these |
|  [05]   | `BYTE_ORDER_MARK`   | `"﻿"`          | UTF-8 BOM constant                                                              |
|  [06]   | `RECORD_SEP` / `UNIT_SEP` | `"\x1E"` / `"\x1F"` | ASCII record/unit separators — the invisible canonical delimiters         |
|  [07]   | `WORKERS_SUPPORTED` | `boolean`           | runtime web-worker availability; `worker: true` is a no-op when false          |
|  [08]   | `LocalChunkSize` / `RemoteChunkSize` / `DefaultDelimiter` | writable `let` | file-chunk bytes (10 MB), remote-chunk bytes (5 MB), auto-detect-fallback delimiter |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse dispatch (arg-shape discriminated, 5 real overloads)

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `parse<T>(csvString, config?)`                         | synchronous      | returns `ParseResult<T>` — full buffer, main thread            |
|  [02]   | `parse<T>(csvString, ParseWorkerConfig & {download?:false})` | async worker | `void`; results via required `complete` off the main thread     |
|  [03]   | `parse<T>(url, ParseRemoteConfig)`                     | async remote     | `void`; downloads (`download: true`) then parses               |
|  [04]   | `parse<T,TFile>(file, ParseLocalConfig)`               | async local file | `void`; reads a `File` or `NodeJS.ReadableStream` then parses    |
|  [05]   | `parse(NODE_STREAM_INPUT, config?)`                    | node stream      | returns a `Duplex` — pipe CSV in, read parsed rows out          |

[ENTRYPOINT_SCOPE]: unparse and in-callback control

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `unparse<T>(data: T[] \| UnparseObject<T>, config?)`   | serialization  | returns a CSV string from an array-of-arrays, array-of-objects, or `{fields,data}` |
|  [02]   | `parser.abort()` / `pause()` / `resume()` / `getCharIndex()` | control  | drive streaming from inside `step` (unavailable inside a worker) |

## [04]-[IMPLEMENTATION_LAW]

[CSV_TOPOLOGY]:
- 2 functions; `parse` dispatches on the first-arg shape (string / URL string / `File`|`Readable` / `NODE_STREAM_INPUT`) and the config discriminant (`worker: true`, `download: true`), so one polymorphic entry owns all five modalities — never a `parseString`/`parseFile`/`parseUrl` family.
- streaming is discriminated by callback: `step` fires per row (`ParseStepResult`, `data` is one row), `chunk` fires per parsed chunk, and the two are mutually exclusive with the `complete`-only path that buffers the whole file into `ParseResult`. `worker: true` requires `complete` and disables `Parser.abort/pause/resume`.
- `header: true` keys each row by field name, populates `ParseMeta.fields`, and records `ParseMeta.renamedHeaders` when a duplicate header is auto-suffixed; `dynamicTyping` coerces numeric/boolean strings (values beyond ±2^53 stay strings to preserve precision); `skipEmptyLines: 'greedy'` drops whitespace-only rows.
- `NODE_STREAM_INPUT` returns a node `Duplex`: write CSV bytes into it, read parsed rows off it — the pipe-based streaming path, distinct from the browser `File` path.

[STACKING]:
- egress (`persistence/object#OBJECT_STORE`): the `csv` `AssetCodec` arm is `new TextEncoder().encode(Papa.unparse(rows, { escapeFormulae: true }))` — `escapeFormulae` prepends `'` to any cell starting `=`/`+`/`-`/`@`, the spreadsheet-formula-injection defense — producing a `Uint8Array` byte source that streams into one `ObjectStore.put` alongside the sibling codecs `exceljs` (`.api/exceljs.md` `Workbook.xlsx.writeBuffer`), `jspdf` (`.api/jspdf.md` `output("arraybuffer")`), and `jszip` (`.api/jszip.md` `generateAsync`) on the one `Schema.Literal("csv","xlsx","pdf","archive")` axis folded by `Match.exhaustive`, never four parallel exporters or a disk round-trip. `unparse` is synchronous CPU work — wrap it in `Effect.try` mapping the throw to the owner's `AssetTransferFault { format: "csv", stage: "encode" }` rail.
- ingress mirror (a CSV-import consumer): decode with `parse` streaming rather than a whole-file buffer — `NODE_STREAM_INPUT` yields a `Duplex` piping an `ObjectStore.get` body `Readable` (or an `@effect/platform` `Multipart` upload part; `.api/effect-platform.md`) into rows, the streaming analog of the streamed `put`. Bridge the `Duplex`/`step` callback surface into an `effect` `Stream` at the boundary (`Stream.fromReadableStream` / `Stream.async`; `.api/effect.md`) so ingest rides the same `Stream<Uint8Array>` rail the store uses, with `header: true` + `dynamicTyping` landing typed rows and `step`/`chunk` bounding memory on large inputs.
- closed error axis: `ParseError.type`/`.code` are bounded literal vocabularies a decode fold discriminates — `FieldMismatch` with `TooFewFields`/`TooManyFields` is schema drift, `Quotes`/`Delimiter` is malformed input — mapped into the fault channel by `Match` on `code`, never a stringy `message` scrape.

[RAIL_LAW]:
- package: `papaparse` (5.5.4) + `@types/papaparse` (5.5.2)
- owns: delimiter-separated value parsing and serialization across synchronous, worker, remote, local-file, and node-stream modes, plus the `Parser` streaming control and the closed `ParseError` vocabulary
- accept: `unparse(rows, { escapeFormulae: true })` → `Uint8Array` → `ObjectStore.put` for the `csv` codec arm; `parse` streaming (`step`/`chunk`/`NODE_STREAM_INPUT`) bridged into an `effect` `Stream` for ingest; `File`, `NodeJS.ReadableStream`, remote URL, or `NODE_STREAM_INPUT` as the source
- reject: hand-rolled CSV tokenization or row splitting; a buffered `unparse` round-trip through disk where the encoded `Uint8Array` streams into `put`; `unparse` without `escapeFormulae` for spreadsheet-bound output; whole-file `parse` buffering where `step`/`chunk`/`NODE_STREAM_INPUT` streams; a raw `message`-string error scrape instead of the closed `type`/`code` fold
