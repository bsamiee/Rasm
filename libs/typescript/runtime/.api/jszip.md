# [TS_RUNTIME_API_JSZIP]

`jszip` is the ZIP container `runtime/src/work/report.ts` uses to bundle a multi-artifact report — several PDFs, spreadsheets, and CSVs — into one downloadable archive, and to read an inbound archive back into typed entries. Its surface is a single mutable `JSZip` tree: one polymorphic `file` that adds an entry when given `(path, data, options)`, fetches an entry when given `(path)`, and matches by pattern when given a `RegExp`; `folder` for nested roots; `forEach`/`filter`/`remove` for traversal; and a type-indexed serialization family — `generateAsync<T>` returns the container as any `OutputByType` member (`uint8array`, `nodebuffer`, `arraybuffer`, `blob`, `base64`, `string`), `generateNodeStream` returns a Node `Readable`, and `generateInternalStream` returns a `JSZipStreamHelper` event source with `on`/`accumulate`/`pause`/`resume`. `loadAsync` deserializes, exposing each `JSZipObject` with a lazy `async<T>` byte accessor, a `nodeStream`, and the `unsafeOriginalName` that flags a zip-slip path. Compression is a policy pair — `STORE` or `DEFLATE` with a `level`, plus `streamFiles` for low-memory generation — never a method per codec. The library is imperative and `Promise`-returning; the owner internalizes it once: `Effect.tryPromise` lifts `generateAsync`/`loadAsync`, the `Uint8Array` result flows to `FileSystem.writeFile` or a nodemailer attachment, and `generateInternalStream` bridges to an Effect `Stream` through `Stream.async` so a large bundle streams rather than buffering.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jszip`
- package: `jszip` (`MIT OR GPL-3.0-or-later` — take the MIT arm, © Stuart Knightley); declarations bundled (`index.d.ts`)
- module format: CJS (`main: lib/index`, no `module`/`exports` map); the `JSZip` value is `export =` (import as `import JSZip = require("jszip")` or esModuleInterop default); no deep-import subpaths
- runtime target: isomorphic. Runtime dependencies `pako` (DEFLATE), `lie` (Promise polyfill), `readable-stream`, `setimmediate`; no native addon — compression is pure-JS `pako`, so it is CPU-bound and belongs off the main path for large archives
- asset: JS runtime + bundled `.d.ts`; `JSZipSupport` (`arraybuffer`/`uint8array`/`blob`/`nodebuffer`) reports the runtime's available output types via the static `support` field
- rail: document egress (folder-tier; internalized once at `runtime/src/work/report.ts`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the container tree and its entries
- rail: boundaries

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `JSZip` | archive tree | the mutable owner — `files` map, `file`/`folder`/`forEach`/`filter`/`remove`, `generate*`, `loadAsync`, static `support`/`version`/`external` |
| [02] | `JSZip.JSZipObject` | entry | `name`, `dir`, `date`, `comment`, `unixPermissions`/`dosPermissions`, `options`; `async<T>(type)`, `nodeStream()`; `unsafeOriginalName` flags a zip-slip path |
| [03] | `InputByType` / `JSZip.InputType` | input codomain | the `file` data union — `string`, `uint8array`, `arraybuffer`, `blob`, `nodebuffer`, `base64`, `array`, `binarystring`, `stream`; each accepted directly or as a `Promise` |
| [04] | `OutputByType` / `JSZip.OutputType` | output codomain | the `generateAsync<T>` return index — `uint8array`/`nodebuffer` for Node egress, `blob`/`base64`/`arraybuffer` for browser/transport |
| [05] | `JSZip.JSZipMetadata` | progress receipt | `{ percent, currentFile }` — the `onUpdate` payload the owner folds into a `Ref`/`Metric` for durable-job progress |
| [06] | `JSZip.JSZipStreamHelper<T>` | event source | `generateInternalStream` return — `on("data"\|"end"\|"error")`, `accumulate`, `pause`, `resume`; the seam bridged to an Effect `Stream` |

[PUBLIC_TYPE_SCOPE]: compression and load policy
- rail: boundaries

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `Compression` = `"STORE" \| "DEFLATE"` | codec policy | the per-entry and per-archive compression discriminant; a policy value, never a method family |
| [02] | `CompressionOptions` = `{ level: number }` | codec tuning | DEFLATE level `1` (speed) → `9` (ratio); ignored under `STORE` |
| [03] | `JSZipFileOptions` | entry policy | `compression`/`compressionOptions`, `date`, `comment`, `base64`, `binary`, `createFolders`, `dir`, `unixPermissions`/`dosPermissions` — per-`file` metadata |
| [04] | `JSZipGeneratorOptions<T>` | serialize policy | `type`, `compression`, `mimeType`, `comment`, `platform` (`DOS`/`UNIX`), `streamFiles`, `encodeFileName` — the archive-wide egress policy |
| [05] | `JSZipLoadOptions` | deserialize policy | `checkCRC32` (integrity gate), `base64`, `createFolders`, `optimizedBinaryString`, `decodeFileName` — inbound-archive decode |
| [06] | `JSZip.OnUpdateCallback` / `DataEventCallback<T>` | progress hooks | `(metadata) => void` streamed generation progress; the `data`/`end`/`error` event callbacks on the stream helper |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build, traverse, and read the tree
- rail: boundaries

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `new JSZip()` / `JSZip()` | construct | `report` bundle root; the owner builds one tree per durable job inside `Effect.sync` |
| [02] | `zip.file<T>(path, data, options?)` → `this` | add entry | polymorphic write — `data` is any `InputByType[T]` or its `Promise`; chainable, folded over the artifact list |
| [03] | `zip.file(path)` → `JSZipObject \| null` / `zip.file(re)` → `JSZipObject[]` | read entry | the same name discriminates: `string` fetches one entry (`null`-absent), `RegExp` matches many — one polymorphic accessor |
| [04] | `zip.folder(name)` → `JSZip \| null` / `zip.remove(path)` / `zip.forEach(cb)` / `zip.filter(p)`| traverse | nested-root scoping, deletion, and iteration/predicate selection over `files` |
| [05] | `zip.generateAsync<T>(options?, onUpdate?)` → `Promise<OutputByType[T]>` | serialize | the primary egress — `{ type: "uint8array" }` yields the bytes; `Effect.tryPromise`-lifted; `onUpdate` folds progress |
| [06] | `zip.generateNodeStream(options?, onUpdate?)` → `NodeJS.ReadableStream` | serialize stream | the Node streaming egress; `NodeStream.fromReadable` lifts it to an Effect `Stream<Uint8Array>` for a memory-bounded bundle |
| [07] | `zip.generateInternalStream<T>(options?)` → `JSZipStreamHelper<OutputByType[T]>` | serialize events | the runtime-neutral streaming egress; `Stream.async` bridges `on("data"\|"end"\|"error")` to an Effect `Stream` |
| [08] | `zip.loadAsync(data, options?)` → `Promise<JSZip>` | deserialize | inbound-archive decode with `checkCRC32` integrity; `Effect.tryPromise`-lifted; entries read lazily via `JSZipObject.async` |

[ENTRYPOINT_SCOPE]: entry byte access and capability probe
- rail: system-apis

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `entry.async<T>(type, onUpdate?)` → `Promise<OutputByType[T]>` | lazy read | decompress one loaded entry to bytes on demand; `Effect.tryPromise`-lifted, decoded through the entry's `Schema` |
| [02] | `entry.nodeStream(type?, onUpdate?)` → `NodeJS.ReadableStream` | lazy stream | stream one large entry out without full decompression; bridged with `NodeStream.fromReadable` |
| [03] | `entry.unsafeOriginalName` | zip-slip flag | the raw archived path that may contain `..`; the owner validates it against a resolved root before any `FileSystem.writeFile` |
| [04] | `JSZip.support` / `JSZip.version` / `JSZip.external.Promise` | runtime probe | `support.uint8array`/`nodebuffer` gate the chosen output `type`; `external.Promise` is left at the native `Promise` |

## [04]-[IMPLEMENTATION_LAW]

[JSZIP_TOPOLOGY]:
- `file` is one polymorphic method, not three: `(path, data, options?)` adds and returns the tree for chaining, `(path)` fetches one entry or `null`, `(RegExp)` returns matches. The owner folds the artifact list with the add arm and reads with the accessor arm — no `addFile`/`getFile`/`matchFile` proliferation.
- Output is a type-indexed family, not a codec-per-format surface: `generateAsync<T>` and `entry.async<T>` are parameterized by an `OutputType` member, and the return narrows through `OutputByType[T]`. The Node durable job pins `type: "uint8array"` once; `blob`/`base64` are browser/transport arms of the same call.
- Compression is a policy value: `{ compression: "DEFLATE", compressionOptions: { level } }` set per entry or archive-wide, defaulting to `STORE`. `streamFiles: true` generates without holding the whole archive in memory — the low-RAM path for a large bundle. `pako` is pure-JS, so DEFLATE is CPU-bound and a large archive belongs in a `Worker` or off the request path.
- The tree is mutable and imperative but the seam is pure: the owner constructs and folds the tree inside `Effect.sync`, then crosses to the `Effect` rail exactly once at `generateAsync`/`loadAsync` via `Effect.tryPromise`. No archive state leaks across the boundary.
- `unsafeOriginalName` is the zip-slip control: a loaded entry's stored path may contain `..` traversal. The owner resolves every entry name against a fixed root and rejects an escape before `FileSystem.writeFile` — inbound archives are untrusted input.
- Progress is a receipt stream: `onUpdate(JSZipMetadata)` reports `{ percent, currentFile }`; the owner folds it into a `Ref`/`SubscriptionRef` or a `Metric.gauge` so a durable report job exposes live progress without re-reading the tree.

[STACKS_WITH]:
- `effect` (`../../.api/effect.md`): `Effect.tryPromise` lifts `generateAsync`/`loadAsync`/`entry.async`; `Effect.sync` owns the tree-building fold; `Stream.async` bridges `JSZipStreamHelper` events to a backpressured Effect `Stream`; `Effect.forEach(artifacts, { concurrency })` populates entries; `Metric.gauge` folds the `percent` progress receipt; `Effect.acquireRelease` scopes a temp workspace around the bundle.
- `@effect/platform` (`../../.api/effect-platform.md`): the `Uint8Array` archive lands via `FileSystem.writeFile(path, bytes)` or streams through `FileSystem.sink`; egress over the wire is `HttpBody.uint8Array(bytes, "application/zip")` or `HttpBody.stream(byteStream)`; `Path.resolve`/`Path.join` validate `unsafeOriginalName` against the extraction root.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): `NodeStream.fromReadable` lifts `generateNodeStream`/`entry.nodeStream` to an Effect `Stream<Uint8Array>`; `NodeSink.fromWritable` writes the archive stream into a Node `Writable` sink for a file target.
- `jspdf` (`./jspdf.md`) / `exceljs` (`./exceljs.md`) / `papaparse` (`./papaparse.md`): the artifact producers — `report` builds each document to `Uint8Array` (PDF/XLSX/CSV under one output-format policy row), then folds them into one `jszip` tree keyed by filename; jszip is the container over the sibling document owners, never a document producer itself. The generated `application/zip` `Uint8Array` then rides the shared `deliver` egress — `FileSystem.writeFile`, `HttpBody`, or a `nodemailer` (`./nodemailer.md`) attachment — so a multi-report bundle mails as one file.
- `@effect/workflow` (`./effect-workflow.md`): a multi-artifact bundle is a durable activity — the tree fold and `generateAsync` run inside an `Activity` with a `Schedule` retry budget, so a transient sink failure replays the serialization without regenerating each source document.

[LOCAL_ADMISSION]:
- Use the one polymorphic `file` for add/get/match and `generateAsync<T>` parameterized by output type; never add `addFile`/`getFile` variants or a serializer per format.
- Use `Effect.tryPromise` at the `generateAsync`/`loadAsync` boundary and `Effect.sync` for the tree fold; never leak the mutable tree across the `Effect` boundary or `await` a `JSZip` promise raw in domain code.
- Use `compression`/`streamFiles` as policy on the generator options; never branch code paths per codec.
- Use `generateNodeStream`/`generateInternalStream` for an unbounded bundle; never buffer a large archive into one `Uint8Array` where entry count or size is open-ended.
- Use `Path.resolve` against a fixed root on every `unsafeOriginalName` before extraction; never write a loaded entry to a path derived from its stored name unvalidated.

[RAIL_LAW]:
- Package: `jszip`
- Owns: ZIP container assembly and reading — the polymorphic `file`/`folder`/`filter` tree, the type-indexed `generateAsync`/`generateNodeStream`/`generateInternalStream` egress, `loadAsync` with CRC integrity, per-entry lazy byte access, the `STORE`/`DEFLATE` policy, and the `JSZipMetadata` progress receipt
- Accept: `Effect.tryPromise`-lifted generate/load, `Effect.sync` tree folds, `Stream.async`/`NodeStream.fromReadable` over the stream helpers, `type: "uint8array"` bytes to `FileSystem`/`HttpBody`/a `nodemailer` attachment, compression as a policy value, `unsafeOriginalName` validated against a resolved root
- Reject: `addFile`/`getFile` proliferation, a serializer method per output format, the mutable tree crossing the `Effect` boundary, whole-archive buffering for unbounded bundles, unvalidated extraction of a loaded entry path
