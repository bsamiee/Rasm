# [API_CATALOGUE] jszip

`jszip` supplies in-memory ZIP archive creation and reading: the `JSZip` class owns file/folder mutation (`file`, `folder`, `remove`, `forEach`, `filter`), asynchronous serialization (`generateAsync`, `generateNodeStream`, `generateInternalStream`), and ZIP loading (`loadAsync`). Input and output types are keyed through the `InputType`/`OutputType` unions, and per-file options control compression, encoding, and permissions.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jszip`
- package: `jszip`
- module: `jszip`
- asset: `JSZip` class, file/generator option interfaces, stream helper, support flags
- rail: archive

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive and entry family
- rail: archive

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :---------------------------- | :------------ | :--------------------------------------- |
|   [1]   | `JSZip`                       | class         | archive owner; file tree + serialization |
|   [2]   | `JSZip.JSZipObject`           | interface     | single archived entry (file or folder)   |
|   [3]   | `JSZip.JSZipFileOptions`      | interface     | per-file add options                     |
|   [4]   | `JSZip.JSZipObjectOptions`    | interface     | stored per-entry options                 |
|   [5]   | `JSZip.JSZipGeneratorOptions` | interface     | output serialization options             |
|   [6]   | `JSZip.JSZipLoadOptions`      | interface     | load / parse options                     |
|   [7]   | `JSZip.JSZipStreamHelper`     | interface     | streaming output helper                  |
|   [8]   | `JSZip.JSZipMetadata`         | interface     | progress metadata from update callbacks  |

[PUBLIC_TYPE_SCOPE]: input/output type family
- rail: archive

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [RAIL]                                                                                                        |
| :-----: | :----------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------ |
|   [1]   | `JSZip.InputType`        | string union key | keys of `InputByType` (`"base64"`, `"string"`, `"uint8array"`, `"arraybuffer"`, `"blob"`, `"stream"`, …)      |
|   [2]   | `JSZip.OutputType`       | string union key | keys of `OutputByType` (`"base64"`, `"string"`, `"uint8array"`, `"arraybuffer"`, `"blob"`, `"nodebuffer"`, …) |
|   [3]   | `Compression`            | string union     | `"STORE" \| "DEFLATE"`                                                                                        |
|   [4]   | `CompressionOptions`     | interface        | `{ level: number }` for DEFLATE                                                                               |
|   [5]   | `InputFileFormat`        | union type       | any `InputByType` value or its `Promise`                                                                      |
|   [6]   | `JSZip.OnUpdateCallback` | function type    | `(metadata: JSZipMetadata) => void`                                                                           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive mutation
- rail: archive

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------- |
|   [1]   | `file(path: string)`              | read           | returns `JSZipObject \| null` by path    |
|   [2]   | `file(path: RegExp)`              | read           | returns `JSZipObject[]` matching pattern |
|   [3]   | `file<T>(path, data, options?)`   | write          | adds or replaces a file entry            |
|   [4]   | `file(path, null, { dir: true })` | write          | adds an explicit directory entry         |
|   [5]   | `folder(name: string)`            | navigate       | returns new `JSZip` rooted at folder     |
|   [6]   | `folder(name: RegExp)`            | navigate       | returns `JSZipObject[]` matching folders |
|   [7]   | `remove(path)`                    | mutate         | removes file or folder from archive      |
|   [8]   | `forEach(callback)`               | iterate        | iterates all entries at current root     |
|   [9]   | `filter(predicate)`               | iterate        | returns filtered `JSZipObject[]`         |

[ENTRYPOINT_SCOPE]: serialization and loading
- rail: archive

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `generateAsync<T>(options?, onUpdate?)`   | serialize      | `Promise<OutputByType[T]>` — full async gen  |
|   [2]   | `generateNodeStream(options?, onUpdate?)` | serialize      | `NodeJS.ReadableStream` for streaming output |
|   [3]   | `generateInternalStream<T>(options?)`     | serialize      | `JSZipStreamHelper<OutputByType[T]>` control |
|   [4]   | `loadAsync(data, options?)`               | parse          | `Promise<JSZip>` — deserializes a ZIP buffer |

[ENTRYPOINT_SCOPE]: entry async access
- rail: archive

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `JSZipObject.async<T>(type, onUpdate?)`    | read entry     | `Promise<OutputByType[T]>` for one file |
|   [2]   | `JSZipObject.nodeStream(type?, onUpdate?)` | read entry     | `NodeJS.ReadableStream` for one file    |

## [4]-[IMPLEMENTATION_LAW]

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
