# [API_CATALOGUE] papaparse

`papaparse` supplies `parse` and `unparse` functions with synchronous, streamed, web-worker, remote-URL, and Node.js readable-stream modes for consuming and producing delimiter-separated values; types are provided by `@types/papaparse`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papaparse`
- package: `papaparse`
- module: `papaparse` (UMD global `Papa`; named ESM/CJS exports via `@types/papaparse`)
- asset: runtime library
- rail: csv-parsing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parse configuration
- rail: csv-parsing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :------------------ | :------------ | :------------------------------------------- |
|  [01]   | `ParseConfig`       | interface     | base synchronous parse options               |
|  [02]   | `ParseWorkerConfig` | interface     | config requiring `worker: true` + `complete` |
|  [03]   | `ParseLocalConfig`  | type          | async local-file config (step or complete)   |
|  [04]   | `ParseRemoteConfig` | type          | remote-URL config with `download: true`      |
|  [05]   | `UnparseConfig`     | interface     | serialization options for `unparse`          |
|  [06]   | `UnparseObject`     | interface     | `{ fields: string[]; data: T[] }` shape      |

[PUBLIC_TYPE_SCOPE]: parse result types
- rail: csv-parsing

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `ParseResult`     | interface     | `{ data: T[]; errors: ParseError[]; meta: ParseMeta }`                |
|  [02]   | `ParseStepResult` | interface     | single-row result for `step` streaming callback                       |
|  [03]   | `ParseError`      | interface     | `type`, `code`, `message`, `row?`, `index?`                           |
|  [04]   | `ParseMeta`       | interface     | `delimiter`, `linebreak`, `aborted`, `fields?`, `truncated`, `cursor` |

[PUBLIC_TYPE_SCOPE]: utility types and constants
- rail: csv-parsing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]       | [RAIL]                                         |
| :-----: | :------------------ | :------------------ | :--------------------------------------------- |
|  [01]   | `Parser`            | class               | low-level parser with `abort`/`pause`/`resume` |
|  [02]   | `LocalFile`         | type                | `File \| NodeJS.ReadableStream`                |
|  [03]   | `NODE_STREAM_INPUT` | unique symbol       | sentinel to request a Node duplex stream       |
|  [04]   | `BAD_DELIMITERS`    | `readonly string[]` | disallowed delimiter characters                |
|  [05]   | `BYTE_ORDER_MARK`   | `"﻿"`               | UTF-8 BOM constant                             |
|  [06]   | `RECORD_SEP`        | `"\x1E"`            | ASCII record separator                         |
|  [07]   | `UNIT_SEP`          | `"\x1F"`            | ASCII unit separator                           |
|  [08]   | `WORKERS_SUPPORTED` | `boolean`           | runtime web-worker availability flag           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse overloads
- rail: csv-parsing

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]   | [RAIL]                                       |
| :-----: | :--------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `parse<T>(csvString, config?)`                       | synchronous      | returns `ParseResult<T>`                     |
|  [02]   | `parse<T>(csvString, config: ParseWorkerConfig<T>)`  | async worker     | void; results via `complete` callback        |
|  [03]   | `parse<T>(url, config: ParseRemoteConfig<T>)`        | async remote     | void; downloads then parses                  |
|  [04]   | `parse<T>(file, config: ParseLocalConfig<T, TFile>)` | async local file | void; reads `File` or `Readable` then parses |
|  [05]   | `parse(NODE_STREAM_INPUT, config?)`                  | Node stream      | returns `Duplex` for pipe-based streaming    |

[ENTRYPOINT_SCOPE]: unparse and configurable globals
- rail: csv-parsing

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :---------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `unparse<T>(data, config?)`         | serialization  | returns CSV string from array or object   |
|  [02]   | `LocalChunkSize` (writable `let`)   | global config  | file-chunk size in bytes (default 10 MB)  |
|  [03]   | `RemoteChunkSize` (writable `let`)  | global config  | remote-chunk size in bytes (default 5 MB) |
|  [04]   | `DefaultDelimiter` (writable `let`) | global config  | fallback delimiter when auto-detect fails |

## [04]-[IMPLEMENTATION_LAW]

[CSV_TOPOLOGY]:
- namespace: `papaparse` / `Papa`; two primary exports `parse` and `unparse` with five overload dispatch paths
- `NODE_STREAM_INPUT` is the Node.js streaming sentinel: `parse(Papa.NODE_STREAM_INPUT)` returns a `Duplex` that accepts piped CSV input and emits parsed rows
- `step` callback activates true streaming; `chunk` callback activates chunk-level streaming; both are mutually exclusive with the `complete`-only path that buffers the full file
- `ParseConfig.worker: true` spawns a Web Worker; the `complete` callback is required in that mode; `abort/pause/resume` on the `Parser` instance are unavailable inside a worker

[LOCAL_ADMISSION]:
- `ParseConfig.header: true` keys each row by header name and populates `ParseMeta.fields`; absent header cells produce `undefined` values.
- `ParseConfig.dynamicTyping: true` coerces numeric and boolean strings to their JS types before returning.
- `ParseConfig.skipEmptyLines: 'greedy'` skips rows that contain only whitespace after parsing, beyond fully empty lines.
- `UnparseConfig.escapeFormulae` prepends `'` to cells starting with `=`, `+`, `-`, or `@` to prevent injection in spreadsheet applications.

[RAIL_LAW]:
- Package: `papaparse` + `@types/papaparse`
- Owns: delimiter-separated value parsing and serialization — synchronous, streamed, worker, and remote modes
- Accept: CSV string, `File`, `NodeJS.ReadableStream`, remote URL, or `NODE_STREAM_INPUT`
- Reject: hand-rolled CSV tokenization or row splitting
