# [API_CATALOGUE] jszip

`jszip` supplies in-memory ZIP archive creation and reading: the `JSZip` class owns file/folder mutation (`file`, `folder`, `remove`, `forEach`, `filter`), asynchronous serialization (`generateAsync`, `generateNodeStream`, `generateInternalStream`), and ZIP loading (`loadAsync`). Input and output types are keyed through the `InputType`/`OutputType` unions, and per-file options control compression, encoding, and permissions.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jszip`
- package: `jszip`
- module: `jszip`
- asset: `JSZip` class, file/generator option interfaces, stream helper, support flags
- rail: archive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive and entry family
- rail: archive

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :---------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `JSZip`                       | class         | archive owner; file tree + serialization |
|  [02]   | `JSZip.JSZipObject`           | interface     | single archived entry (file or folder)   |
|  [03]   | `JSZip.JSZipFileOptions`      | interface     | per-file add options                     |
|  [04]   | `JSZip.JSZipObjectOptions`    | interface     | stored per-entry options                 |
|  [05]   | `JSZip.JSZipGeneratorOptions` | interface     | output serialization options             |
|  [06]   | `JSZip.JSZipLoadOptions`      | interface     | load / parse options                     |
|  [07]   | `JSZip.JSZipStreamHelper`     | interface     | streaming output helper                  |
|  [08]   | `JSZip.JSZipMetadata`         | interface     | progress metadata from update callbacks  |

[PUBLIC_TYPE_SCOPE]: input/output type family
- rail: archive

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [RAIL]                                                                                                        |
| :-----: | :----------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `JSZip.InputType`        | string union key | keys of `InputByType` (`"base64"`, `"string"`, `"uint8array"`, `"arraybuffer"`, `"blob"`, `"stream"`, …)      |
|  [02]   | `JSZip.OutputType`       | string union key | keys of `OutputByType` (`"base64"`, `"string"`, `"uint8array"`, `"arraybuffer"`, `"blob"`, `"nodebuffer"`, …) |
|  [03]   | `Compression`            | string union     | `"STORE" \| "DEFLATE"`                                                                                        |
|  [04]   | `CompressionOptions`     | interface        | `{ level: number }` for DEFLATE                                                                               |
|  [05]   | `InputFileFormat`        | union type       | any `InputByType` value or its `Promise`                                                                      |
|  [06]   | `JSZip.OnUpdateCallback` | function type    | `(metadata: JSZipMetadata) => void`                                                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive mutation
- rail: archive

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `file(path: string)`              | read           | returns `JSZipObject \| null` by path    |
|  [02]   | `file(path: RegExp)`              | read           | returns `JSZipObject[]` matching pattern |
|  [03]   | `file<T>(path, data, options?)`   | write          | adds or replaces a file entry            |
|  [04]   | `file(path, null, { dir: true })` | write          | adds an explicit directory entry         |
|  [05]   | `folder(name: string)`            | navigate       | returns new `JSZip` rooted at folder     |
|  [06]   | `folder(name: RegExp)`            | navigate       | returns `JSZipObject[]` matching folders |
|  [07]   | `remove(path)`                    | mutate         | removes file or folder from archive      |
|  [08]   | `forEach(callback)`               | iterate        | iterates all entries at current root     |
|  [09]   | `filter(predicate)`               | iterate        | returns filtered `JSZipObject[]`         |

[ENTRYPOINT_SCOPE]: serialization and loading
- rail: archive

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `generateAsync<T>(options?, onUpdate?)`   | serialize      | `Promise<OutputByType[T]>` — full async gen  |
|  [02]   | `generateNodeStream(options?, onUpdate?)` | serialize      | `NodeJS.ReadableStream` for streaming output |
|  [03]   | `generateInternalStream<T>(options?)`     | serialize      | `JSZipStreamHelper<OutputByType[T]>` control |
|  [04]   | `loadAsync(data, options?)`               | parse          | `Promise<JSZip>` — deserializes a ZIP buffer |

[ENTRYPOINT_SCOPE]: entry async access
- rail: archive

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `JSZipObject.async<T>(type, onUpdate?)`    | read entry     | `Promise<OutputByType[T]>` for one file |
|  [02]   | `JSZipObject.nodeStream(type?, onUpdate?)` | read entry     | `NodeJS.ReadableStream` for one file    |

## [04]-[IMPLEMENTATION_LAW]

[JSZIP_TOPOLOGY]:
- `JSZip` is a class but also callable as a factory function and constructable with `new JSZip()`.
- `file(path, data)` keys are relative paths; folder separators are `/`; no leading slash.
- `generateAsync` options key `type` selects the `OutputType`; TypeScript infers the return type via the generic.
- `loadAsync` accepts any `InputFileFormat` value; `options.checkCRC32` enables CRC validation during load.
- `JSZipStreamHelper` exposes `on("data")`, `on("end")`, `on("error")`, `accumulate()`, `resume()`, and `pause()`.
- `JSZip.support` object carries boolean flags for `arraybuffer`, `uint8array`, `blob`, and `nodebuffer` availability.

[LOCAL_ADMISSION]:
- `JSZipFileOptions.compression` overrides the archive-level default per file; `compressionOptions.level` sets DEFLATE quality (1–9).
- `JSZipObject.unsafeOriginalName` may contain `..` path components from loaded archives; validate before use.
- For Node.js server-side use, prefer `generateNodeStream` or `type: "nodebuffer"` in `generateAsync`.

[RAIL_LAW]:
- Package: `jszip`
- Owns: in-memory ZIP archive construction, reading, and streaming
- Accept: `generateAsync` for single-shot serialization; `generateNodeStream` for piped output
- Reject: hand-rolled ZIP byte construction; synchronous file access patterns
