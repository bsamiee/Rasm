# [TS_RUNTIME_API_JSZIP]

`jszip` assembles and reads a ZIP container as one mutable tree — a polymorphic `file` that adds, fetches, or pattern-matches an entry by argument shape, `folder` scoping, and a type-indexed `generateAsync<T>`/`loadAsync` serialization narrowing through `OutputByType`. `report` folds artifacts into one archive inside `Effect.sync`, crosses to the rail once via `Effect.tryPromise`, and streams a large bundle through `generateInternalStream` rather than buffering.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jszip`
- package: `jszip` (MIT)
- module: CJS (`main: lib/index`, no `exports` map); `JSZip` is `export =` — import as `import JSZip = require("jszip")` or an esModuleInterop default; no deep-import subpaths
- runtime: isomorphic, no native addon; DEFLATE is pure-JS `pako`, CPU-bound, off the main path for a large archive
- asset: JS runtime + bundled `.d.ts`; static `support` (`JSZipSupport`) reports the runtime's available `arraybuffer`/`uint8array`/`blob`/`nodebuffer` output types
- rail: document egress — folder-tier, internalized once at `runtime/src/work/report.ts`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the container tree and its entries — `InputByType` keys span `string`/`uint8array`/`arraybuffer`/`blob`/`nodebuffer`/`base64`/`array`/`binarystring`/`stream` (direct or `Promise`); `OutputByType` keys span `uint8array`/`nodebuffer`/`blob`/`base64`/`arraybuffer`/`string`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `JSZip`                      | interface     | archive tree — `files` map, static `support`/`version`/`external`                      |
|  [02]   | `JSZip.JSZipObject`          | interface     | entry: `name`, `dir`, `date`, `comment`, `unixPermissions`/`dosPermissions`, `options` |
|  [03]   | `InputByType`                | interface     | `file` data codomain; `InputByType[K]` selects the input type; alias `JSZip.InputType` |
|  [04]   | `OutputByType`               | interface     | `generateAsync<T>` codomain; alias `JSZip.OutputType`; Node vs browser/transport arm   |
|  [05]   | `JSZip.JSZipMetadata`        | interface     | `{ percent, currentFile }` progress payload folded into a `Ref`/`Metric`               |
|  [06]   | `JSZip.JSZipStreamHelper<T>` | interface     | event source — `on`, `accumulate`, `pause`, `resume`                                   |

[PUBLIC_TYPE_SCOPE]: compression and load policy; `*Options` field rosters keyed below by table index

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Compression`              | union         | `"STORE" \| "DEFLATE"` — the per-entry and archive-wide codec discriminant |
|  [02]   | `CompressionOptions`       | interface     | `{ level }` — DEFLATE `1` (speed) → `9` (ratio); ignored under `STORE`     |
|  [03]   | `JSZipFileOptions`         | interface     | per-`file` write metadata                                                  |
|  [04]   | `JSZipGeneratorOptions<T>` | interface     | archive-wide egress policy                                                 |
|  [05]   | `JSZipLoadOptions`         | interface     | inbound-archive decode policy                                              |
|  [06]   | `JSZip.OnUpdateCallback`   | delegate      | `(metadata) => void` — streamed generation-progress hook                   |
|  [07]   | `DataEventCallback<T>`     | delegate      | the `data`/`end`/`error` callbacks on the stream helper                    |

- [03]-[JSZIPFILEOPTIONS]: `compression`/`compressionOptions`, `date`, `comment`, `base64`, `binary`, `optimizedBinaryString`, `createFolders`, `dir`, `unixPermissions`/`dosPermissions`.
- [04]-[JSZIPGENERATOROPTIONS]: `type`, `compression`/`compressionOptions`, `mimeType`, `comment`, `platform` (`DOS`/`UNIX`), `streamFiles`, `encodeFileName`.
- [05]-[JSZIPLOADOPTIONS]: `checkCRC32`, `base64`, `optimizedBinaryString`, `createFolders`, `decodeFileName`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build, traverse, and read the tree

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `new JSZip()` / `JSZip()`                                | ctor     | the bundle root; one tree per durable job                   |
|  [02]   | `zip.file<T>(path, data, options?)`                      | instance | add → `this`; chainable write of `InputByType[T]`/`Promise` |
|  [03]   | `zip.file(path)` / `zip.file(re)`                        | instance | read; `string` → one or `null`, `RegExp` → `JSZipObject[]`  |
|  [04]   | `zip.folder(name)`                                       | instance | nested-root scoping → `JSZip \| null`                       |
|  [05]   | `zip.remove(path)` / `zip.forEach(cb)` / `zip.filter(p)` | instance | delete, iterate, predicate-select over `files`              |
|  [06]   | `zip.generateAsync<T>(options?, onUpdate?)`              | instance | serialize → `Promise<OutputByType[T]>`; the primary egress  |
|  [07]   | `zip.generateNodeStream(options?, onUpdate?)`            | instance | serialize stream → `NodeJS.ReadableStream`                  |
|  [08]   | `zip.generateInternalStream<T>(options?)`                | instance | serialize events → `JSZipStreamHelper<OutputByType[T]>`     |
|  [09]   | `zip.loadAsync(data, options?)`                          | instance | deserialize → `Promise<JSZip>`; `checkCRC32` integrity gate |

[ENTRYPOINT_SCOPE]: entry byte access and capability probe

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------- | :------- | :---------------------------------------------------------------------------- |
|  [01]   | `entry.async<T>(type, onUpdate?)`    | instance | decompress one loaded entry on demand → `Promise<OutputByType[T]>`            |
|  [02]   | `entry.nodeStream(type?, onUpdate?)` | instance | stream one large entry without full decompress → `NodeJS.ReadableStream`      |
|  [03]   | `entry.unsafeOriginalName`           | property | the raw archived path that may contain `..`; validate against a resolved root |
|  [04]   | `JSZip.support` / `JSZip.version`    | static   | `support.uint8array`/`nodebuffer` gate the output `type`; `version` string    |
|  [05]   | `JSZip.external.Promise`             | static   | the promise-implementation slot, left at the native `Promise`                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `file` is one polymorphic method: `(path, data, options?)` adds and returns the tree for chaining, `(path)` fetches one entry or `null`, `(RegExp)` returns matches. `report` folds the artifact list through the add arm and reads through the accessor arm.
- Output is a type-indexed family: `generateAsync<T>` and `entry.async<T>` narrow through `OutputByType[T]`. `report`'s Node path pins `type: "uint8array"` once; `blob`/`base64` are browser/transport arms of the same call.
- Compression is a policy value: `{ compression: "DEFLATE", compressionOptions: { level } }` per entry or archive-wide, defaulting to `STORE`; `streamFiles: true` generates without holding the whole archive. `pako` is pure-JS, so DEFLATE is CPU-bound and a large archive belongs off the request path.
- `JSZip`'s tree is mutable and imperative but the seam stays pure: `report` folds it inside `Effect.sync` and crosses to the rail once at `generateAsync`/`loadAsync` via `Effect.tryPromise`; no archive state leaks across the boundary.
- `unsafeOriginalName` is the zip-slip control: a loaded entry's stored path may carry `..`; the owner resolves every name against a fixed root and rejects an escape before `FileSystem.writeFile`.
- Progress is a receipt stream: `onUpdate(JSZipMetadata)` reports `{ percent, currentFile }`, folded into a `Ref`/`SubscriptionRef` or a `Metric.gauge` for live job progress.

[STACKING]:
- `effect` (`../../.api/effect.md`): `Effect.tryPromise` lifts `generateAsync`/`loadAsync`/`entry.async`; `Effect.sync` owns the tree fold; `Stream.async` bridges `JSZipStreamHelper` events to a backpressured `Stream`; `Effect.forEach(artifacts, { concurrency })` populates entries; `Metric.gauge` folds the `percent` receipt; `Effect.acquireRelease` scopes a temp workspace.
- `@effect/platform` (`../../.api/effect-platform.md`): the `Uint8Array` archive lands via `FileSystem.writeFile(path, bytes)` or `FileSystem.sink`; wire egress is `HttpBody.uint8Array(bytes, "application/zip")` or `HttpBody.stream(byteStream)`; `Path.resolve`/`Path.join` validate `unsafeOriginalName` against the extraction root.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): `NodeStream.fromReadable` lifts `generateNodeStream`/`entry.nodeStream` to a `Stream<Uint8Array>`; `NodeSink.fromWritable` writes the archive stream into a Node `Writable`.
- `jspdf` (`./jspdf.md`) / `exceljs` (`./exceljs.md`) / `papaparse` (`./papaparse.md`): the artifact producers — `report` builds each document to `Uint8Array` under one output-format row, then folds them into one `jszip` tree keyed by filename; jszip is the container over the document owners, never a producer. `report` sends the `application/zip` `Uint8Array` down the shared `deliver` egress — `FileSystem.writeFile`, `HttpBody`, or a `nodemailer` (`./nodemailer.md`) attachment.
- `@effect/workflow` (`./effect-workflow.md`): a multi-artifact bundle is a durable `Activity` — the tree fold and `generateAsync` run under a `Schedule` retry budget, so a transient sink failure replays serialization without regenerating each source document.

[LOCAL_ADMISSION]:
- One polymorphic `file` and `generateAsync<T>` parameterized by output type own add/get/match and serialization; a per-format serializer or `addFile`/`getFile` variant is rejected.
- `Effect.tryPromise` bounds the `generateAsync`/`loadAsync` seam and `Effect.sync` the tree fold; the mutable tree never crosses the `Effect` boundary and domain code never `await`s a `JSZip` promise raw.
- `generateNodeStream`/`generateInternalStream` carry an unbounded bundle; `Path.resolve` against a fixed root validates every `unsafeOriginalName` before extraction.

[RAIL_LAW]:
- Package: `jszip`
- Owns: ZIP container assembly and reading — the polymorphic `file`/`folder`/`filter` tree, the type-indexed `generateAsync`/`generateNodeStream`/`generateInternalStream` egress, `loadAsync` with CRC integrity, per-entry lazy byte access, the `STORE`/`DEFLATE` policy, and the `JSZipMetadata` progress receipt
- Accept: `Effect.tryPromise`-lifted generate/load, `Effect.sync` tree folds, `Stream.async`/`NodeStream.fromReadable` over the stream helpers, `uint8array` bytes to `FileSystem`/`HttpBody`/a `nodemailer` attachment, compression as a policy value, `unsafeOriginalName` validated against a resolved root
- Reject: `addFile`/`getFile` proliferation, a serializer method per output format, the mutable tree crossing the `Effect` boundary, whole-archive buffering for unbounded bundles, unvalidated extraction of a loaded entry path
