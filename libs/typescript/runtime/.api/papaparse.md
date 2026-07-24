# [TS_RUNTIME_API_PAPAPARSE]

`papaparse` owns RFC 4180 CSV: one polymorphic `parse` picks sync-`string`, async-callback, Node-`Duplex`, or Web-Worker decode by input shape, and `unparse` serializes with formula-injection defense; bad rows accumulate in `result.errors`, never throw.

`work/report.ts` internalizes it once as the output-format policy's CSV arm — `parse` feeds `Schema.decodeUnknown` per row, the `NODE_STREAM_INPUT` `Duplex` bridges to an Effect `Stream`, and encoded bytes ride the shared `deliver` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papaparse`
- package: `papaparse` (MIT)
- module: UMD single-file (`papaparse.js`, `main` only, no `module`/`exports` map); the `Papa` namespace under `export as namespace Papa`; no deep-import subpaths
- runtime: isomorphic, zero runtime dependencies, no native addon; Node rides the sync `string` path or the `NODE_STREAM_INPUT` `Duplex`, the `File`/`FileReader` and `download`/`XMLHttpRequest` paths bind browser only; `@types/papaparse` supplies the surface and threads `@types/node` for the `Duplex`
- rail: the CSV codec and egress owner for `work/report`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the parse and serialize configuration shapes; each field roster rides the token lines below the grid.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `ParseConfig<T, TInput>`                       | interface     | base parse policy; fields below                   |
|  [02]   | `ParseWorkerConfig<T>`                         | interface     | `worker: true` off-thread parse                   |
|  [03]   | `ParseLocalConfig<T>` / `ParseRemoteConfig<T>` | union         | browser `File` / `download` policy, Node-excluded |
|  [04]   | `UnparseConfig`                                | interface     | egress serializer policy; fields below            |
|  [05]   | `UnparseObject<T>`                             | interface     | explicit `{ fields, data }` column form           |

- `ParseConfig`: `delimiter`, `newline`, `quoteChar`, `escapeChar`, `header`, `transformHeader`, `transform`, `dynamicTyping`, `preview`, `comments`, `skipEmptyLines`, `fastMode`, `delimitersToGuess`, `skipFirstNLines`, `beforeFirstChunk`, `step`, `complete`.
- `ParseLocalConfig` adds `chunkSize`/`chunk`/`error`/`encoding` over `FileReader`; `ParseRemoteConfig` adds `download: true`/`downloadRequestHeaders`/`downloadRequestBody`/`withCredentials` over `XMLHttpRequest`.
- `UnparseConfig`: `quotes`, `quoteChar`, `escapeChar`, `delimiter`, `header`, `newline`, `skipEmptyLines`, `columns`, `escapeFormulae` (the CSV-injection defense).

[PUBLIC_TYPE_SCOPE]: the result, evidence, fault, and control shapes.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `ParseResult<T>`     | interface     | `{ data: T[], errors, meta }` — sync return and `complete` payload     |
|  [02]   | `ParseStepResult<T>` | interface     | `{ data: T, errors, meta }` — one row per `step`/`chunk`               |
|  [03]   | `ParseError`         | interface     | `{ type, code, message, row?, index? }`; accumulated, never thrown     |
|  [04]   | `ParseMeta`          | interface     | parse-evidence receipt; fields below                                   |
|  [05]   | `Parser`             | class         | streaming-control handle passed into `step`/`chunk`                    |
|  [06]   | `LocalFile`          | type          | `File \| NodeJS.ReadableStream`; only the stream arm is Node-reachable |

- `ParseError.type`: `"Quotes"` / `"Delimiter"` / `"FieldMismatch"`; `ParseError.code`: `"MissingQuotes"` / `"UndetectableDelimiter"` / `"TooFewFields"` / `"TooManyFields"` / `"InvalidQuotes"` — one closed literal union.
- `ParseMeta`: `delimiter`, `linebreak`, `aborted`, `fields?`, `truncated`, `cursor`, `renamedHeaders?`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the polymorphic codec and its streaming-control surface — every top-level surface is a `Papa.` member.

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Papa.parse<T>(string, config?) -> ParseResult<T>`                                   | static   | sync string decode                   |
|  [02]   | `Papa.parse<T>(File\|url, config) -> void`                                           | static   | async callback decode                |
|  [03]   | `Papa.parse(NODE_STREAM_INPUT, config?) -> Duplex`                                   | static   | Node row-stream decode               |
|  [04]   | `Papa.unparse<T>(T[]\|UnparseObject<T>, config?) -> string`                          | static   | serialize; formula-injection guard   |
|  [05]   | `parser.abort()` / `.pause()` / `.resume()` / `.getCharIndex()`                      | instance | in-`step` control; cursor read       |
|  [06]   | `Papa.NODE_STREAM_INPUT`                                                             | property | `unique symbol` `Duplex` selector    |
|  [07]   | `Papa.BAD_DELIMITERS` / `RECORD_SEP` / `UNIT_SEP` / `BYTE_ORDER_MARK`                | property | reserved delimiters, ASCII seps, BOM |
|  [08]   | `Papa.LocalChunkSize` / `RemoteChunkSize` / `DefaultDelimiter` / `WORKERS_SUPPORTED` | property | tunables; worker-support gate        |

- `Papa.parse(File\|url)`: browser binds the local `File`/`FileReader` and remote `download`/`XMLHttpRequest` arms; neither enters a Node durable job.
- `Papa.parse(string)`: `worker: true` forks a Web Worker, returning `void` through callbacks rather than a `ParseResult`.
- `RECORD_SEP` is `\x1E`, `UNIT_SEP` is `\x1F`, `BYTE_ORDER_MARK` is `﻿`; `LocalChunkSize`/`RemoteChunkSize`/`DefaultDelimiter` are reassignable module `let` bindings.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `parse` is one polymorphic entry: the first argument's shape selects the modality — a `string` returns a `ParseResult` synchronously, a `File`/URL drives async callbacks and returns `void`, the `NODE_STREAM_INPUT` symbol returns a `Duplex`, and `worker: true` forks a Web Worker — one `decodeCsv` seam discriminates internally, so downstream never picks a per-source function.
- Errors are data: a malformed row populates `result.errors: ParseError[]` while parsing continues, so the owner reads `errors` after the sync call and lifts a non-empty set onto the `Effect` error channel; there is no `try`/`catch` because `parse` does not throw on bad CSV.
- `dynamicTyping` is refused: `parse` yields `string` cells (or header-keyed `string` records under `header: true`) and the one `Schema` performs every decode, brand, and coercion; `dynamicTyping` forks typing authority off `Schema` and silently drops precision above `2^53`.
- `escapeFormulae` is the egress control: any exported cell opening `=`/`+`/`-`/`@` is prefixed so a spreadsheet consumer cannot execute it, and the owner sets it on every `unparse` because CSV egress is untrusted-sink output.

[STACKING]:
- `effect` (`../../.api/effect.md`): `Effect.sync` wraps the synchronous `parse`/`unparse`; `Schema.decodeUnknown(RowSchema)` decodes each `ParseResult.data` element so the interior types once; `ParseError.code` lowers to a `Data.taggedEnum` caught on the tag by `Effect.catchTag`; `Effect.withSpan` tags the parse with `meta.cursor`/`meta.aborted`.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): `NodeStream.fromReadable` lifts the `parse(NODE_STREAM_INPUT)` `Duplex` into a backpressured `Stream<ParseStepResult<T>>`; `NodeStream.pipeThroughDuplex` feeds a source `Stream<string>` back through the parse `Duplex` for round-trip re-serialization.
- `@effect/platform` (`../../.api/effect-platform.md`): the `unparse` `string` encodes to a `Uint8Array` once, then lands via `FileSystem.writeFile`/`FileSystem.sink` or rides the wire as `HttpBody.uint8Array(bytes, "text/csv")`.
- `exceljs` (`./exceljs.md`) / `jspdf` (`./jspdf.md`): output-format peers over one decoded row set — `papaparse` owns standalone CSV, so `exceljs.csv` reserves for re-projecting an existing `Worksheet` and `jspdf.output` renders the PDF arm, one policy row selecting the format.
- `jszip` (`./jszip.md`) / `nodemailer` (`./nodemailer.md`): the shared `deliver` channels — the encoded CSV `Uint8Array` folds into a `jszip` entry via `zip.file(name, bytes)` or attaches as a nodemailer `Mail.Attachment` `{ content: bytes, contentType: "text/csv" }`; papaparse mints the bytes, the container and mail owners carry them.
- `@effect/workflow` (`./effect-workflow.md`): a CSV export is a durable `Activity` under a `Schedule` retry budget, so a transient sink failure replays serialization without re-decoding upstream state.
- `work/report` (within-library): folds every report through the output-format policy, running `parse` -> `Schema.decodeUnknown` on ingress and `unparse` on egress, and pinning the `NODE_STREAM_INPUT` `Duplex` for any unbounded export so the CSV arm is a policy row, never a bespoke pipeline.

[LOCAL_ADMISSION]:
- One `parse` seam discriminates on input shape, then `Schema.decodeUnknown` types each row; `dynamicTyping` and per-source parse functions are refused.
- String ingest runs `Effect.sync` + `result.errors` inspection before `result.data`; streaming ingest runs `NodeStream.fromReadable` over the `Duplex`.
- `unparse` carries `escapeFormulae: true` on every egress to an untrusted spreadsheet sink.
- `NODE_STREAM_INPUT` carries any export whose row count is unbounded, never a whole result set buffered into one `unparse` string.
- Browser config (`File`/`download`) binds `browser` alone, never a `work` Node durable job.

[RAIL_LAW]:
- Package: `papaparse` (+ `@types/papaparse`)
- Owns: RFC 4180 CSV decode/encode — the polymorphic `parse`, `unparse` with formula-injection defense, the `Parser` streaming-control handle, the `ParseError` code union, the `ParseMeta` receipt, and the `NODE_STREAM_INPUT` Node-`Duplex` rail; the CSV arm of the report output-format policy that `exceljs.csv` defers to
- Accept: `Effect.sync`-wrapped `parse`/`unparse`, per-row `Schema.decodeUnknown`, `ParseError.code` as a `Data.taggedEnum`, `unparse` with `escapeFormulae`, `NodeStream.fromReadable` over the `Duplex`, the encoded `Uint8Array` through `FileSystem`/`HttpBody` or as a shared `jszip`/`nodemailer` deliver artifact
- Reject: `dynamicTyping` in place of `Schema`, untyped consumption of `result.data`, `unparse` without the formula guard on an untrusted sink, the browser `File`/`download` paths in a Node durable job, whole-result buffering where the row count is unbounded
