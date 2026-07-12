# [TS_RUNTIME_API_PAPAPARSE]

`papaparse` is the RFC-418 catalog CSV codec `runtime/src/work/report.ts` internalizes for every tabular ingress and egress: one polymorphic `parse` that discriminates on input shape — a `string` decodes synchronously to a `ParseResult`, a DOM `File`/URL streams asynchronously to callbacks, the `NODE_STREAM_INPUT` token returns a Node `Duplex`, and `worker: true` offloads to a Web Worker — plus `unparse` for serialization with formula-injection defense, a `Parser` control handle (`abort`/`pause`/`resume`) exposed inside the `step` callback for backpressure, a bounded `ParseError` code family, and a `ParseMeta` evidence record (`delimiter`, `linebreak`, `cursor`, `renamedHeaders`). It never assigns interior types — `dynamicTyping` is refused so the one `Schema` owns row typing — and it never throws on malformed rows; errors accumulate in `result.errors` for lift into the `Effect` channel. The library is wrapped once behind the report owner: `parse` feeds `Schema.decodeUnknown` per row, `unparse` emits a `string` that `Encoding`/`Buffer` turns into the `Uint catalogArray` egress body, and the Node `Duplex` bridges to an Effect `Stream` through `NodeStream.fromReadable` for unbounded CSV that never buffers in memory. papaparse is the admitted CSV owner `exceljs.csv` (fast-csv) defers to and one arm of the report output-format policy (PDF via `jspdf`, XLSX via `exceljs`, CSV here); the `unparse` `Uint catalogArray` is a shared `deliver` artifact `jszip` bundles and `nodemailer` attaches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papaparse`
- package: `papaparse` (MIT, © Matthew Holt); declarations via `@types/papaparse` (MIT, DefinitelyTyped) — the runtime ships no `.d.ts`
- module format: UMD single-file (`papaparse.js`, `main` only, no `module`/`exports` map); the `Papa` global namespace under `export as namespace Papa`; no deep-import subpaths
- runtime target: isomorphic, zero runtime dependencies, no native addon. Node uses the synchronous `string` path or the `NODE_STREAM_INPUT` `Duplex`; the `File`/`FileReader` local path and the `download`/`XMLHttpRequest` remote path are browser-only and never enter a Node durable job
- asset: JS runtime + external DefinitelyTyped surface; the `Duplex` return threads `node:stream`, so `@types/node` is a transitive type dependency
- rail: document egress (folder-tier; internalized once at `runtime/src/work/report.ts`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration shapes — parse modalities and the serializer
- rail: boundaries

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]    | [CONSUMER]                                                                                                                                                                                                                         |
| :-----: | :------------------------------------------------- | :--------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ParseConfig<T, TInput>`                           | parse policy     | the base row config — `header`, `delimiter`, `newline`, `quoteChar`, `escapeChar`, `comments`, `skipEmptyLines`, `preview`, `fastMode`, `delimitersToGuess`, `transformHeader`, `transform`, `skipFirstNLines`, `beforeFirstChunk` |
|  [02]   | `ParseWorkerConfig<T>` / `ParseAsyncConfigBase<T>` | streaming policy | `worker: true` off-thread; `chunkSize`, `chunk`, `error`, `step` — the incremental modalities `report` uses for large exports                                                                                                      |
|  [03]   | `ParseLocalConfig<T>` / `ParseRemoteConfig<T>`     | boundary policy  | browser-only — `encoding` over `FileReader`; `download`/`downloadRequestHeaders`/`downloadRequestBody`/`withCredentials` over `XMLHttpRequest`; excluded from Node jobs                                                            |
|  [04]   | `UnparseConfig`                                    | serialize policy | the egress formatter — `quotes`, `quoteChar`, `escapeChar`, `delimiter`, `header`, `newline`, `columns`, and `escapeFormulae` (the CSV-injection defense)                                                                          |
|  [05]   | `UnparseObject<T>`                                 | serialize input  | the explicit `{ fields, data }` column form when object keys do not fix column order                                                                                                                                               |

[PUBLIC_TYPE_SCOPE]: result, evidence, and fault shapes
- rail: boundaries

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]   | [CONSUMER]                                                                                                                                              |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `ParseResult<T>`                              | batch result    | `{ data: T[], errors: ParseError[], meta: ParseMeta }` — the synchronous-parse and `complete`-callback payload                                          |
|  [02]   | `ParseStepResult<T>`                          | streamed result | `{ data: T, errors, meta }` — one row per `step`/`chunk` tick, the element the Effect `Stream` carries                                                  |
|  [03]   | `ParseError`                                  | fault           | `{ type: "Quotes"\|"Delimiter"\|"FieldMismatch", code, message, row?, index? }` — accumulated, never thrown; `code` is the closed vocabulary below      |
|  [04]   | `ParseMeta`                                   | receipt         | `delimiter`, `linebreak`, `aborted`, `fields?`, `truncated`, `cursor`, `renamedHeaders?` — the sampling/dedup evidence retained as an algorithm receipt |
|  [05]   | `Parser`                                      | control handle  | passed into `step`/`chunk`; `abort()`, `pause()`, `resume()`, `getCharIndex()` — the backpressure + early-termination seam                              |
|  [06]   | `LocalFile` = `File \| NodeJS.ReadableStream` | input alias     | the async-parse source union; only the `ReadableStream` arm is Node-reachable                                                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the polymorphic codec and its control surface
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY]  | [CONSUMER]                                                                                                                      |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Papa.parse<T>(csv, config?)` → `ParseResult<T>`                                              | sync ingest     | `report` inbound decode — the string arm; wrap in `Effect.sync`, inspect `result.errors`, lift to the typed channel             |
|  [02]   | `Papa.parse<T>(file\|url, config)` → `void`                                                   | async ingest    | browser DOM `File`/remote URL through `step`/`complete`/`error` callbacks; bridged with `Effect.async`, excluded from Node jobs |
|  [03]   | `Papa.parse(Papa.NODE_STREAM_INPUT, config?)` → `Duplex`                                      | node stream     | the Node streaming rail — pipe an `fs` `ReadStream` in, lift the `Duplex` with `NodeStream.fromReadable` to an Effect `Stream`  |
|  [04]   | `Papa.unparse<T>(data\|UnparseObject<T>, config?)` → `string`                                 | serialize       | `report` outbound — `Effect.sync`; `escapeFormulae` prepends `'` to `=`/`+`/`-`/`@` cells before the bytes leave the process    |
|  [05]   | `Parser#abort()` / `#pause()` / `#resume()` / `#getCharIndex()`                               | stream control  | inside `step` — `abort` ends parse on a poison row, `pause`/`resume` gate against the downstream `Queue`                        |
|  [06]   | `Papa.NODE_STREAM_INPUT` (`unique symbol`)                                                    | stream token    | the discriminant selecting the `Duplex` return; the only Node-native streaming entry                                            |
|  [07]   | `Papa.BAD_DELIMITERS` / `RECORD_SEP` (`\x1E`) / `UNIT_SEP` (`\x1F`) / `BYTE_ORDER_MARK` (`﻿`) | constants       | reserved-delimiter guard and the ASCII record/unit separators for structured keys; BOM stripping at ingress                     |
|  [08]   | `Papa.LocalChunkSize` / `RemoteChunkSize` / `DefaultDelimiter` / `WORKERS_SUPPORTED`          | tunable / probe | mutable module tunables for streamed-file chunking and recovery delimiter; the worker-availability gate                         |

## [04]-[IMPLEMENTATION_LAW]

[PAPAPARSE_TOPOLOGY]:
- `parse` is one polymorphic entry, not a family: the first argument's shape selects the modality — a `string` returns a `ParseResult` synchronously, a `File`/URL drives async callbacks and returns `void`, the `NODE_STREAM_INPUT` symbol returns a `Duplex`, and `worker: true` forks a Web Worker. The owner exposes one `decodeCsv` seam that discriminates internally; downstream never picks a per-source function.
- Errors are data, not exceptions: malformed rows populate `result.errors: ParseError[]` while parsing continues, so the owner reads `errors` after `Effect.sync` and lifts a non-empty set into the `Effect` error channel — there is no `try`/`catch` because papaparse does not throw on bad CSV.
- The library never types the interior: `dynamicTyping` is refused. `parse` yields `string` cells (or header-keyed `string` records with `header: true`), and the one `Schema` performs the real decode, brand, and coercion. `dynamicTyping` does fork typing authority away from `Schema` and silently drop precision above 2^53.
- `escapeFormulae` is the egress security control: any exported cell beginning `=`/`+`/`-`/`@` is prefixed so a spreadsheet consumer cannot execute it as a formula. The report owner sets it on every `unparse` — CSV egress is untrusted-sink output.
- The `Duplex` path is the memory bound: `parse(NODE_STREAM_INPUT)` streams row-by-row, so a multi-gigabyte export never materializes as one `string`. `step`/`chunk` deliver `ParseStepResult<T>` (one row) with the `Parser` handle for `abort`/`pause`, mapping directly onto Effect `Stream` backpressure.

[STACKS_WITH]:
- `effect` (`../../.api/effect.md`): `Effect.sync` wraps the synchronous `parse`/`unparse`; `Schema.decodeUnknown(RowSchema)` decodes each `ParseResult.data` element so the interior is typed once; the `ParseError.code` vocabulary lowers to a `Data.taggedEnum`/`Schema.Literal` union caught by `Effect.catchTag`; `Stream.async` bridges the `step` callback into a backpressured row stream; `Chunk` accumulates batch rows; `Effect.withSpan` tags the parse with `meta.cursor`/`meta.aborted` evidence.
- `@effect/platform` (`../../.api/effect-platform.md`): `unparse` output becomes bytes for `FileSystem.writeFile(path, Uint8Array)` or `FileSystem.sink` streaming; the CSV artifact rides `HttpBody.uint8Array(bytes, "text/csv")` to a webhook or `Multipart.toPersisted` on an upload edge.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): `NodeStream.fromReadable` lifts the `parse(NODE_STREAM_INPUT)` `Duplex` into an Effect `Stream` with backpressure; `NodeStream.toReadable` feeds an Effect `Stream<string>` back into papaparse's Node duplex for round-trip re-serialization.
- `jspdf` (`./jspdf.md`) / `exceljs` (`./exceljs.md`): the report output-format peers — the same decoded rows render to CSV (`papaparse`), XLSX (`exceljs`), or PDF (`jspdf`) selected by one output-format policy row, never a forked pipeline per format. papaparse is the admitted CSV owner: `exceljs`'s built-in `.csv` (fast-csv) DEFERS to it for standalone tabular CSV, `exceljs.csv` reserved only for re-projecting an existing `Worksheet`.
- `jszip` (`./jszip.md`) / `nodemailer` (`./nodemailer.md`): the shared `deliver` egress channels — the `unparse` `Uint8Array` is equally a deliver artifact `jszip` folds into a multi-artifact archive and `nodemailer` attaches as `{ content: bytes, contentType: "text/csv" }`; papaparse produces the CSV bytes, the container and mail owners carry them.
- `@effect/workflow` (`./effect-workflow.md`): a CSV export is a durable activity — `parse`/`unparse` run inside an `Activity` body with a `Schedule` retry budget from `core/value/fault`, so a transient sink failure replays without re-decoding upstream state.

[LOCAL_ADMISSION]:
- Use one `parse` seam that discriminates on input shape, then `Schema.decodeUnknown` per row; never call `parse` with `dynamicTyping` (typing belongs to `Schema`) and never expose per-source parse functions.
- Use `Effect.sync` + `result.errors` inspection for the string path and `Stream.async`/`NodeStream.fromReadable` for the streaming path; never treat papaparse as throwing, and never read `result.data` without lifting `result.errors` first.
- Use `unparse` with `escapeFormulae: true` on every egress; never emit a CSV cell to an untrusted spreadsheet sink without the formula guard.
- Use `NODE_STREAM_INPUT` for any export that can exceed memory; never buffer a whole result set into one `unparse` string when the row count is unbounded.
- Use the browser `File`/`download` config only in `browser`; never route a DOM-only parse modality through a `work` Node durable job.

[RAIL_LAW]:
- Package: `papaparse` (+ `@types/papaparse`)
- Owns: RFC-4180 CSV decode/encode — the polymorphic `parse`, `unparse` with formula-injection defense, the `Parser` streaming-control handle, the `ParseError` code vocabulary, the `ParseMeta` receipt, and the `NODE_STREAM_INPUT` Node-duplex rail; the admitted CSV owner `exceljs.csv` defers to, one arm of the report output-format policy peered with `jspdf`/`exceljs`
- Accept: `Effect.sync`/`Stream.async`-wrapped `parse`, per-row `Schema.decodeUnknown`, `ParseError.code` as a `Data.taggedEnum`, `unparse` with `escapeFormulae`, `NodeStream.fromReadable` over the Node `Duplex`, output bytes via `FileSystem`/`HttpBody` or as a shared deliver artifact for `jszip`/`nodemailer`
- Reject: `dynamicTyping` in place of `Schema`, untyped consumption of `result.data`, `unparse` without the formula guard on an untrusted sink, the browser `File`/`download` paths inside a Node durable job, whole-result buffering where the row count is unbounded
