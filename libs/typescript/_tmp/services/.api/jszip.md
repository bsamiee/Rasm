# [API_CATALOGUE] jszip

`jszip` is the in-memory ZIP codec the `persistence/object#OBJECT_STORE` `AssetCodec` `archive` arm rides: one `JSZip` instance owns the entry tree (`file`/`folder`/`remove`/`forEach`/`filter`), and one polymorphic serializer discriminates output on a single `OutputType` key — `generateAsync<T>({ type })` for a `Promise<OutputByType[T]>`, `generateNodeStream()` for a piped `node:stream.Readable`, `generateInternalStream<T>()` for a pull-controlled `JSZipStreamHelper`. `InputByType`/`OutputByType` are the two keyed vocabularies every read/write/serialize resolves through, so there is one `file`/`async`/`generateAsync` entry per direction, never a per-encoding method family. In `services` it is the bundling codec: sibling `AssetCodec` outputs (`jspdf` `output("arraybuffer")`, `exceljs` `xlsx.writeBuffer()`, `papaparse` `unparse`, `sharp` buffers) enter as `file(path, bytes)` entries and leave as one archive streamed straight into `ObjectStore.put` under the `XxHash128` content-address, never a disk round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jszip`
- package: `jszip` (target 3.10.1, dual `MIT OR GPL-3.0-or-later`, © Stuart Knightley)
- module format: UMD (`lib/index.js`, `main`) + bundled types `index.d.ts` (`export = JSZip`, DefinitelyTyped-lineage "JSZip 3.1"); pure-JS, zero native ABI — no `@types/jszip` (self-typed)
- runtime target: runtime-neutral — node + browser; `nodebuffer`/`stream` outputs are node-only, `blob` is browser-only, gated by the `JSZip.support` flags
- asset: the `JSZip` factory/class, `JSZip.*` namespace (entry, option, stream, metadata types), the `support`/`external`/`version` statics
- consumer: `persistence/object#OBJECT_STORE` — the `AssetCodec` `archive` literal (`generateAsync`/`generateNodeStream` → `ObjectStore.put`) and the import-side `loadAsync` round-trip
- rail: asset-codec / archive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive and entry family
The `JSZip` instance IS the folder-rooted tree; `folder(name)` returns a new instance re-rooted at that path so nested writes stay relative. `JSZipObject` is one archived entry carrying its own metadata plus a per-entry async reader.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                              |
| :-----: | :---------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `JSZip`                       | interface+ctor | the archive: `files` map, mutation, serialization; callable + `new`-able |
|  [02]   | `JSZip.files`                 | record         | `{ [path]: JSZipObject }` — the live flat entry index                    |
|  [03]   | `JSZip.JSZipObject`           | interface      | one entry: `name`/`dir`/`date`/`comment`/`unixPermissions`/`dosPermissions`/`options` + `async`/`nodeStream` |
|  [04]   | `JSZip.JSZipFileOptions`      | interface      | per-file add options (compression, dates, permissions, `dir`, base64/binary) |
|  [05]   | `JSZip.JSZipObjectOptions`    | interface      | stored per-entry options — `{ compression: Compression }`                |
|  [06]   | `JSZip.JSZipGeneratorOptions<T>` | interface   | output serialization options; `T extends OutputType` selects the return  |
|  [07]   | `JSZip.JSZipLoadOptions`      | interface      | parse options — `base64`/`checkCRC32`/`createFolders`/`decodeFileName`   |
|  [08]   | `JSZip.JSZipMetadata`         | interface      | progress carrier — `{ percent: number; currentFile: string \| null }`    |
|  [09]   | `JSZip.OnUpdateCallback`      | function type  | `(metadata: JSZipMetadata) => void` — the streamed-progress hook         |

[PUBLIC_TYPE_SCOPE]: input/output type vocabulary — the one parameterized axis
`InputByType`/`OutputByType` are the keyed maps every direction resolves through; `InputType`/`OutputType` are their exported `keyof`s. `Compression`/`CompressionOptions` are option-carried (reached through the option interfaces, not independently importable under `export = JSZip`).

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                                                     |
| :-----: | :----------------------- | :------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `JSZip.InputType`        | keyof union    | `"base64"\|"string"\|"text"\|"binarystring"\|"array"\|"uint8array"\|"arraybuffer"\|"blob"\|"stream"` |
|  [02]   | `JSZip.OutputType`       | keyof union    | `"base64"\|"string"\|"text"\|"binarystring"\|"array"\|"uint8array"\|"arraybuffer"\|"blob"\|"nodebuffer"` |
|  [03]   | `Compression`            | string union   | `"STORE" \| "DEFLATE"` — archive- or per-file compression selector                              |
|  [04]   | `CompressionOptions`     | interface      | `{ level: number }` — DEFLATE quality 1 (speed) … 9 (ratio); ignored under `STORE`             |
|  [05]   | `InputFileFormat`        | union          | `InputByType[keyof InputByType] \| Promise<…>` — the `loadAsync` accept type                    |
|  [06]   | `JSZip.JSZipStreamHelper<T>` | interface   | pull-controlled stream — `on('data'\|'end'\|'error')`, `accumulate`, `resume`, `pause`         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive mutation — `file` is the polymorphic read/write entry
`file(path)` discriminates on the path type: `string` → single entry or `null`, `RegExp` → matched array; a third arity with `data` writes. There is no `addFile`/`getFile` split.

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `file(path: string): JSZipObject \| null`                            | read           | one entry by exact path                                 |
|  [02]   | `file(path: RegExp): JSZipObject[]`                                  | read           | every entry matching the pattern                        |
|  [03]   | `file<T extends InputType>(path, data: InputByType[T] \| Promise<…>, options?): this` | write | add/replace a file entry; `data` may be a `Promise`     |
|  [04]   | `file<T>(path, null, options & { dir: true }): this`                 | write          | explicit directory entry                                |
|  [05]   | `folder(name: string): JSZip \| null` / `folder(name: RegExp): JSZipObject[]` | navigate | re-root at a folder / match folder entries              |
|  [06]   | `remove(path): JSZip`                                                | mutate         | drop a file or folder subtree, returns the instance     |
|  [07]   | `forEach((relativePath, file) => void)` / `filter((relativePath, file) => boolean): JSZipObject[]` | iterate | walk / predicate-select entries at the current root |

[ENTRYPOINT_SCOPE]: serialization and loading — one generic serializer, three egress shapes
All three serializers key the return on `JSZipGeneratorOptions.type`; the choice is buffered-Promise vs piped-`Readable` vs pull-`StreamHelper`, never a distinct method per output encoding.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `generateAsync<T extends OutputType>(options?, onUpdate?): Promise<OutputByType[T]>` | serialize | full buffered gen; `type` selects `Uint8Array`/`ArrayBuffer`/`Buffer`/`base64`/`Blob`/… |
|  [02]   | `generateNodeStream(options?: JSZipGeneratorOptions<'nodebuffer'>, onUpdate?): NodeJS.ReadableStream` | serialize | node piped output — the streaming-upload source |
|  [03]   | `generateInternalStream<T>(options?): JSZipStreamHelper<OutputByType[T]>`       | serialize      | pull-controlled stream with `pause`/`resume`/`accumulate` |
|  [04]   | `loadAsync(data: InputFileFormat, options?: JSZipLoadOptions): Promise<JSZip>`  | parse          | deserialize a ZIP buffer; `checkCRC32` validates on load |
|  [05]   | `JSZipObject.async<T extends OutputType>(type, onUpdate?): Promise<OutputByType[T]>` | read entry | one entry's bytes in the requested shape                |
|  [06]   | `JSZipObject.nodeStream(type?: 'nodebuffer', onUpdate?): NodeJS.ReadableStream` | read entry     | one entry as a node stream                              |

[ENTRYPOINT_SCOPE]: statics — capability probes, never construction knobs

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `JSZip.support`                        | probe          | `{ arraybuffer, uint8array, blob, nodebuffer }` runtime-capability flags |
|  [02]   | `JSZip.external.Promise`               | seam           | `PromiseConstructorLike` override for a custom Promise implementation |
|  [03]   | `JSZip.version`                        | probe          | the runtime library version string                                   |

## [04]-[IMPLEMENTATION_LAW]

[JSZIP_TOPOLOGY]:
- `JSZip` is callable (`JSZip()`), `new`-able (`new JSZip()`), and namespace-bearing; `file(path, data)` keys are `/`-separated relative paths with no leading slash. `createFolders` auto-materializes intermediate directory entries; without it the path is virtual.
- `generateAsync`/`async`/`generateInternalStream` infer the return type from the `type` generic — one polymorphic surface, so a `Uint8Array` vs `nodebuffer` vs `base64` choice is a `type` value, never a second method.
- `JSZipGeneratorOptions`: `type`, `compression` (archive default), `compressionOptions.level`, `comment`, `mimeType`, `encodeFileName`, `streamFiles`, `platform` (`"DOS"|"UNIX"`); `JSZipFileOptions` adds `base64`/`binary`/`date`/`dir`/`optimizedBinaryString`/`unixPermissions`/`dosPermissions` and per-file `compression`.
- `JSZipStreamHelper` is event-shaped (`on('data'|'end'|'error')`) with `accumulate()` collapsing the stream to a `Promise<T>` — the pull equivalent of `generateAsync`.

[LOCAL_ADMISSION]:
- `JSZipObject.unsafeOriginalName` may carry `..` path components from loaded archives (zip-slip); validate before mapping an entry name to a filesystem or key path.
- For node server serialization prefer `generateNodeStream()` or `generateAsync({ type: "nodebuffer" \| "uint8array", streamFiles: true })`; `blob` output requires `JSZip.support.blob` (browser only).
- Per-file `compression` overrides the archive default; set `STORE` for already-compressed members (images, PDFs) to skip redundant DEFLATE.

[STACKING]:
- Effect boundary: `generateAsync({ type })` is a `Promise` folded at `Effect.tryPromise` into the `persistence/object#OBJECT_STORE` `AssetTransferFault` rail (`format: "archive", stage: "encode"`); `generateNodeStream()` crosses into an Effect `Stream` via `@effect/platform-node` `NodeStream.fromReadable` (`.api/effect-platform-node.md`) so the archive streams straight into `ObjectStore.put` with backpressure, never a buffer-to-disk staging. `loadAsync` (import) is a symmetric `tryPromise`.
- Content-address: `generateAsync({ type: "uint8array" })` bytes feed `hash-wasm` `createXXHash128(0,0)` to mint the `ObjectKey` brand (the `interchange#CONTENT_KEY_PARITY` seed regime), so an archive that shares bytes with a stored object shares its content address and the store deduplicates.
- Multi-codec bundling: `jspdf` `output("arraybuffer")` (`.api/jspdf.md`), `exceljs` `xlsx.writeBuffer()`, `papaparse` `unparse`, and `sharp` (`.api/sharp.md`) buffers enter as `file(path, bytes)` entries; one `generateNodeStream` → one `put` emits a multi-asset export bundle. `JSZip.support`/`JSZip.external.Promise` need no wiring — Effect's runtime already owns the Promise.
- Telemetry: the `onUpdate(JSZipMetadata{ percent, currentFile })` progress hook feeds a `structlog`/OTLP span or gauge on the `telemetry` export spine (`.api/effect-opentelemetry.md`), so a large-archive export reports progress without a parallel counter.

[RAIL_LAW]:
- package: `jszip`
- owns: in-memory ZIP entry-tree construction, reading, CRC-validated loading, and the buffered/piped/pull serialization triad keyed by `InputType`/`OutputType`
- accept: one `JSZip` instance per archive; `generateNodeStream`/`streamFiles` bridged through `NodeStream.fromReadable` for streamed `ObjectStore.put`; `generateAsync({ type })` for single-shot bytes; `STORE` compression for already-compressed members
- reject: hand-rolled ZIP byte assembly; a `generateBase64`/`generateBuffer`-style method family where the `type` generic discriminates; mapping `unsafeOriginalName` to a path without zip-slip validation; buffering a generated archive to disk before upload where the node stream pipes directly
