# [TS_UI_API_LOADERS_GL_CORE]

`@loaders.gl/core` owns the worker-pool decode engine the `ui/viewer/geo` plane drives: `parse` and `load` are one polymorphic pattern over a format `Loader` or a sniffed `Loader[]`, discriminating on source shape — in-memory `ArrayBuffer`/`Blob` against URL fetch-then-parse — with the `*InBatches` pair streaming `AsyncIterable` output. Direct loader arguments preserve return types and isolate every consumer; format loaders arrive as separate descriptors this core parses through, and `scope:viewer` admits it only in the `ui/viewer` Nx project, compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@loaders.gl/core`
- package: `@loaders.gl/core` (MIT)
- abi: isomorphic browser/node; binary parse is Web Worker-backed (`options.worker`), main-thread when workers are absent
- deps (loaders.gl framework substrate, transitive): `@loaders.gl/loader-utils` (`Loader`/`LoaderOptions`/`FileSystem` types, `RequestScheduler`, `JSONLoader`, path resolution), `@loaders.gl/schema` + `@loaders.gl/schema-utils` (mesh/table schema), `@loaders.gl/worker-utils` (worker pool), `@probe.gl/log`
- runtime: `scope:viewer` project-local; `setLoaderOptions`/`getLoaderOptions` mutate host-global defaults, so reusable consumers pass loaders and options directly
- modules: decode (`parse`/`load` + `*InBatches`), selection (`selectLoader`/`selectLoaderSync`), host-global option defaults (`setLoaderOptions`), acquisition/encode/stream adapters, `createDataSource`, `RequestScheduler`, and the loader/writer/filesystem types
- absent (not admitted): format loaders are separate descriptors; the browser target excludes `@loaders.gl/polyfills`

## [02]-[DECODE_SURFACE]

[TYPE_SCOPE]: the format-agnostic decode surface — one `Loader` (or a `Loader[]`) over a `DataType` source, worker-parsed to the loader's typed output. `parse` and `load` discriminate on source shape (in-memory vs URL), one vocabulary, never two.
- `Loader<DataT, BatchT, OptionsT>` is the format descriptor deck props accept; `LoaderWithParser` adds the synchronous parser the batch path needs.
- One `Loader` types the return exactly (`LoaderReturnType<LoaderT>`); a `Loader[]` sniffs content and selects — the multi-format ingest deck reaches through `loadOptions`.
- Every `parse`/`load`/`select` entry takes trailing `options?, context?`; the `*InBatches` pair returns `Promise<AsyncIterable<Batch>>`.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `parse(DataType, Loader(s)) -> Promise<Out>`                   | in-memory decode — `ArrayBuffer`/`Blob` to points   |
|  [02]   | `parseSync(DataType, Loader) -> Out`                           | main-thread sync decode; small text/JSON only       |
|  [03]   | `parseInBatches(BatchableDataType, Loader)`                    | streaming decode — point clouds too large to hold   |
|  [04]   | `load(string\|DataType, Loader(s)) -> Promise<Out>`            | URL fetch-then-parse — `.las`/`.laz` scan by href   |
|  [05]   | `loadInBatches(FileType, Loader)`                              | streaming URL/File ingest for the viewer drop lane  |
|  [06]   | `selectLoader(DataType, Loader[]?) -> Promise<Loader \| null>` | content-sniff resolution when the format is unknown |
|  [07]   | `selectLoaderSync(DataType, Loader[]?) -> Loader \| null`      | sync sniff; header-bytes available in memory        |
|  [08]   | `setLoaderOptions(LoaderOptions) -> void`                      | host-global defaults; single-owner only             |
|  [09]   | `getLoaderOptions() -> LoaderOptions`                          | current merged defaults                             |

## [03]-[FETCH_ENCODE_STREAM]

[TYPE_SCOPE]: source acquisition, the inverse `encode` writers, and the iterator/stream adapters bridging loaders.gl batches into Web Streams — the whole-file egress and streaming interop the `data` bus and viewer drop lane compose. Every entry takes a trailing `options?`.

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `fetchFile(string\|Blob) -> Promise<Response>`                  | static  | fetch honoring `setPathPrefix`/CDN roots               |
|  [02]   | `readArrayBuffer(File, number, number) -> Promise<ArrayBuffer>` | static  | range read for header-sniff before a full parse        |
|  [03]   | `FetchError`                                                    | class   | typed fetch failure mapped to `codec-absent`           |
|  [04]   | `encode(data, Writer) -> Promise<ArrayBuffer>`                  | static  | inverse egress — a decoded table/mesh back to bytes    |
|  [05]   | `encodeSync(data, Writer) -> ArrayBuffer`                       | static  | sync encode for small text writers                     |
|  [06]   | `encodeTable(data, Writer) -> Promise<ArrayBuffer>`             | static  | columnar table encode for the Arrow egress seam        |
|  [07]   | `encodeInBatches(data, Writer) -> AsyncIterable<ArrayBuffer>`   | static  | streaming egress feeding `makeStream` back to a stream |
|  [08]   | `makeIterator(source) -> AsyncIterable<ArrayBuffer>`            | static  | wraps a `Response`/`ReadableStream`/`Blob` as chunks   |
|  [09]   | `makeStream(iterator) -> ReadableStream`                        | static  | batch `AsyncIterable` back to a Web `ReadableStream`   |
|  [10]   | `createDataSource(url, sources) -> DataSource`                  | static  | tiled-source construction for the 3D-tiles/MVT engines |
|  [11]   | `new RequestScheduler({maxRequests, throttleRequests})`         | ctor    | request throttling across the tile fetch fanout        |
|  [12]   | `NullLoader: Loader`                                            | value   | no-op sink loader for pipeline probes                  |
|  [13]   | `JSONLoader: LoaderWithParser`                                  | value   | built-in JSON parser re-exported from `loader-utils`   |

## [04]-[TYPES]

[TYPE_SCOPE]: the framework type surface re-exported from `@loaders.gl/loader-utils` — the vocabulary a loader and its options thread through.
- `Loader` / `LoaderWithParser` — format descriptor; `LoaderWithParser` carries the sync `parse` the batch iterators require.
- `LoaderOptions` / `LoaderContext` — the merged options bag (`worker`, `fetch`, `CDN`, per-format sub-keys) and the parse context a nested loader receives.
- `DataType` / `SyncDataType` / `BatchableDataType` — the accepted source shapes per entry point; `BatchableDataType` narrows to what `*InBatches` streams.
- `Writer` / `WriterOptions` — the encode-side descriptor pair mirroring `Loader`.
- `FileSystem` / `RandomAccessFileSystem` / `ReadableFile` / `WritableFile` / `Stat` — the virtual-filesystem abstraction tiled sources read through.

## [05]-[IMPLEMENTATION_LAW]

[REGISTRY_TOPOLOGY]:
- direct descriptors own isolation: `parse`/`load` receive a loader or loader array, and deck layers receive `loaders`/`loadOptions`. `setLoaderOptions`/`getLoaderOptions` mutate host-global defaults; only a single-owner host admits them.
- worker-backed by default: `options.worker` is true unless set false; binary point-cloud and tile parse runs in the loaders.gl worker pool off the main thread. Self-hosted worker URLs resolve through the `[ASSET_IDENTITY]` served-asset roster, so CSP stays airtight.

[INTEGRATION_LAW]:
- Stack under `@deck.gl/core` (`.api/deck.gl-core.md`): loaders reach deck through layer `loaders`/`loadOptions` props — `PointCloudLayer` receives `LASLoader`, `Tile3DLayer` receives `Tiles3DLoader`. Deck owns the luma.gl/math.gl substrate transitively.
- Stack with `@deck.gl/geo-layers` (`.api/deck.gl-geo-layers.md`): `Tile3DLayer` tile parse and MVT/terrain decode run in loaders.gl worker pools; this admission makes the point-cloud and 3D-tile loaders explicit registrations the layer rows name.
- Stack with `viewer/geo` layer rows: `PointCloudLayer` receives a `load(url, LASLoader)` promise; loaders exposing a batch parser feed `parseInBatches`, and decoded columnar payloads feed deck's binary `LayerDataSource`.
- Stack with the `data` Arrow bus: `encode`/`encodeTable` write a decoded mesh/table back to bytes, and `encodeInBatches` into `makeStream` bridges a batch `AsyncIterable` to a Web `ReadableStream` for the resumable upload lane.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it, so heavy loader/worker deps stay compile-time excluded from non-spatial apps.
- pass loaders and options directly per call or deck layer; host-global defaults require one process owner, and library composition never mutates them.
- format loaders are separate descriptors passed into this core; core alone parses nothing beyond JSON.
- luma.gl/math.gl/mjolnir.js stay deck-owned transitive substrate; reach GPU decode through deck props, never a direct import.

[RAIL_LAW]:
- Package: `@loaders.gl/core`
- Owns: the `parse`/`load` decode pair, batch decoders, host-global option defaults, acquisition and encode surfaces, Web-Stream interop, request throttling, and loader types
- Accept: direct loader arguments, worker-backed parse with self-hosted worker URLs, `load(url, LASLoader)` feeding deck's binary `LayerDataSource`, loaders reaching deck through `loaders`/`loadOptions` props
- Reject: a `parseLas`/`parseTiles` family, host-global option mutation in a multi-owner process, direct deck-substrate imports, foreign-CDN workers, hand-parsed binary formats
