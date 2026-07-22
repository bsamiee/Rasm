# [TS_UI_API_LOADERS_GL_CORE]

`@loaders.gl/core` is the async loader framework the `ui/viewer/geo` plane drives directly: a worker-pool parse engine and a `parse`/`load` pair polymorphic over one format `Loader` or an explicitly supplied loader array. Deck consumes the same descriptors through its `loaders`/`loadOptions` props. `registerLoaders` mutates a host-global registry and is deprecated; direct loader arguments preserve type information and isolate every consumer. `parse` owns in-memory `ArrayBuffer`/`Blob`/`Response` data, `load` owns URL fetch-then-parse, and loaders with a batch parser return typed `AsyncIterable` output through the `*InBatches` pair. Deck's luma.gl/math.gl/mjolnir.js substrate stays transitive. `scope:viewer` project-local: admitted only by the `ui/viewer` Nx project and compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@loaders.gl/core`
- package: `@loaders.gl/core`
- license: `MIT`
- abi: isomorphic browser/node; binary parse is Web Worker-backed (`options.worker`), degrading to main-thread when workers are unavailable
- deps (loaders.gl framework substrate, transitive): `@loaders.gl/loader-utils` (`Loader`/`LoaderOptions`/`FileSystem` types, `RequestScheduler`, `JSONLoader`, path resolution), `@loaders.gl/schema` + `@loaders.gl/schema-utils` (mesh/table schema), `@loaders.gl/worker-utils` (worker pool), `@probe.gl/log`
- catalog-verdict: KEEP — deck's `loaders`/`loadOptions` props and direct `parse`/`load` calls consume the loader descriptors
- runtime: `scope:viewer` project-local; loader registry and default options are host-global mutable state, so reusable consumers pass loaders and options directly
- modules: `parse`/`parseSync`/`parseInBatches`, `load`/`loadInBatches`, `registerLoaders`/`_unregisterLoaders`, `selectLoader`/`selectLoaderSync`, `setLoaderOptions`/`getLoaderOptions`, acquisition, encode, stream, datasource, scheduler, null-loader, and JSON surfaces
- absent (not admitted): format loaders are separate descriptors passed to this core; the browser target does not consume `@loaders.gl/polyfills`

## [02]-[LOADER_REGISTRY]

[TYPE_SCOPE]: the format-agnostic decode surface — one `Loader` value (or an array) with a `DataType` source, worker-parsed to the loader's typed output. `parse` and `load` are one polymorphic pattern discriminating on source shape (in-memory vs URL), not two vocabularies.
- `Loader<DataT, BatchT, OptionsT>` is the format descriptor deck props and `registerLoaders` both accept; `LoaderWithParser` adds the synchronous parser the batch path needs.
- A single `Loader` argument types the return exactly (`LoaderReturnType<LoaderT>`); a `Loader[]` argument makes `parse`/`load` sniff the content and select — the multi-format ingest path deck uses through `loadOptions`.
- Every `parse`/`load`/`select` entry takes trailing `options?, context?`; the `*InBatches` pair returns `Promise<AsyncIterable<Batch>>`.

| [INDEX] | [SYMBOL]             | [SIGNATURE]                                             | [CONSUMER_BOUNDARY]                                 |
| :-----: | :------------------- | :------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `parse`              | `(data: DataType, loader(s)) => Promise<Out>`           | in-memory decode — `ArrayBuffer`/`Blob` to points   |
|  [02]   | `parseSync`          | `(data, loader) => Out`                                 | main-thread sync decode; small text/JSON only       |
|  [03]   | `parseInBatches`     | `(data: BatchableDataType, loader)`                     | streaming decode — point clouds too large to hold   |
|  [04]   | `load`               | `(url: string\|DataType, loader(s)) => Promise<Out>`    | URL fetch-then-parse — `.las`/`.laz` scan by href   |
|  [05]   | `loadInBatches`      | `(files: FileType, loader)`                             | streaming URL/File ingest for the viewer drop lane  |
|  [06]   | `registerLoaders`    | `(loaders: Loader[] \| Loader) => void`                 | deprecated host-global registration                 |
|  [07]   | `_unregisterLoaders` | `() => void`                                            | testing-only host-global reset                      |
|  [08]   | `selectLoader`       | `(data: DataType, loaders?) => Promise<Loader \| null>` | content-sniff resolution when the format is unknown |
|  [09]   | `selectLoaderSync`   | `(data, loaders?) => Loader \| null`                    | sync sniff; header-bytes available in memory        |
|  [10]   | `setLoaderOptions`   | `(options: LoaderOptions) => void`                      | host-global defaults; single-owner only             |
|  [11]   | `getLoaderOptions`   | `() => LoaderOptions`                                   | current merged defaults                             |

## [03]-[FETCH_ENCODE_STREAM]

[TYPE_SCOPE]: source acquisition, the inverse `encode` writers, and the iterator/stream adapters bridging loaders.gl batches into Web Streams — the whole-file egress and the streaming interop the `data` bus and viewer drop lane compose.
- Every acquisition, encode, and adapter entry takes a trailing `options?`.

| [INDEX] | [SYMBOL]           | [SIGNATURE]                                             | [CONSUMER_BOUNDARY]                                    |
| :-----: | :----------------- | :------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `fetchFile`        | `(url: string) => Promise<Response>`                    | fetch honoring `setPathPrefix`/CDN roots               |
|  [02]   | `readArrayBuffer`  | `(file, start, length) => Promise<ArrayBuffer>`         | range read for header-sniff before a full parse        |
|  [03]   | `FetchError`       | `class extends Error`                                   | typed fetch failure mapped to `codec-absent`           |
|  [04]   | `encode`           | `(data, writer: Writer) => Promise<ArrayBuffer>`        | inverse egress — a decoded table/mesh back to bytes    |
|  [05]   | `encodeSync`       | `(data, writer) => ArrayBuffer`                         | sync encode for small text writers                     |
|  [06]   | `encodeTable`      | `(data, writer) => Promise<ArrayBuffer>`                | columnar table encode for the Arrow egress seam        |
|  [07]   | `makeIterator`     | `(source) => AsyncIterable<ArrayBuffer>`                | wraps a `Response`/`ReadableStream`/`Blob` as chunks   |
|  [08]   | `makeStream`       | `(iterator) => ReadableStream`                          | batch `AsyncIterable` back to a Web `ReadableStream`   |
|  [09]   | `createDataSource` | `(url, sources) => DataSource`                          | tiled-source construction for the 3D-tiles/MVT engines |
|  [10]   | `RequestScheduler` | `new RequestScheduler({maxRequests, throttleRequests})` | request throttling across the tile fetch fanout        |
|  [11]   | `NullLoader`       | `Loader`                                                | no-op sink loader for pipeline probes                  |
|  [12]   | `JSONLoader`       | `LoaderWithParser`                                      | built-in JSON parser re-exported from `loader-utils`   |

## [04]-[TYPES]

[TYPE_SCOPE]: the framework type surface, re-exported from `@loaders.gl/loader-utils` — the vocabulary a registered loader and its options thread through.
- `Loader` / `LoaderWithParser` — format descriptor; `LoaderWithParser` carries the sync `parse` the batch iterators require.
- `LoaderOptions` / `LoaderContext` — the merged options bag (`worker`, `fetch`, `CDN`, per-format sub-keys) and the parse context a nested loader receives.
- `DataType` / `SyncDataType` / `BatchableDataType` — the accepted source shapes per entry point; `BatchableDataType` narrows to what `*InBatches` can stream.
- `Writer` / `WriterOptions` — the encode-side descriptor pair mirroring `Loader`.
- `FileSystem` / `RandomAccessFileSystem` / `ReadableFile` / `WritableFile` / `Stat` — the virtual-filesystem abstraction the tiled sources read through.

## [05]-[IMPLEMENTATION_LAW]

[REGISTRY_TOPOLOGY]:
- direct descriptors own isolation: `parse`/`load` receive a loader or loader array, and deck layers receive `loaders`/`loadOptions`. `registerLoaders` and `setLoaderOptions` mutate host-global state; only a single-owner host admits them. `_unregisterLoaders` is testing-only and clears every owner's registry.
- worker-backed by default: `options.worker` is true unless set false; binary point-cloud and tile parse runs in the loaders.gl worker pool off the main thread. Self-hosted worker URLs resolve through the `[ASSET_IDENTITY]` served-asset roster — never a foreign CDN, so CSP stays airtight.
- polymorphic source: `parse` owns in-memory `DataType`, `load` owns a URL; a `Loader[]` argument sniffs and selects. One decode vocabulary, discriminated by source shape and loader arity — never a `parseLas`/`parseTiles` family.

[INTEGRATION_LAW]:
- Stack under `@deck.gl/core` (`.api/deck.gl-core.md`): loaders reach deck through layer `loaders`/`loadOptions` props — `PointCloudLayer` receives `LASLoader`, and `Tile3DLayer` receives `Tiles3DLoader`. Deck owns the luma.gl/math.gl substrate transitively.
- Stack with `@deck.gl/geo-layers` (`.api/deck.gl-geo-layers.md`): `Tile3DLayer`'s tile parse and MVT/terrain decode already run in loaders.gl worker pools — this admission makes the point-cloud and 3D-tile loaders explicit registrations the layer rows name, closing the "loader the `Tile3DLayer` row already presumes" gap.
- Stack with `viewer/geo#LAYER_ROWS`: `PointCloudLayer` receives a `load(url, LASLoader)` promise; loaders exposing a batch parser feed `parseInBatches`. Decoded columnar payloads feed deck's binary `LayerDataSource`.
- Stack with the `data` Arrow bus: `encode`/`encodeTable` are the inverse egress writing a decoded mesh/table back to bytes; `makeStream` bridges a batch `AsyncIterable` to a Web `ReadableStream` for the resumable upload lane.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it — heavy loader/worker deps stay compile-time excluded from non-spatial apps.
- Pass loaders and options directly per call or deck layer. Host-global registration/defaults require one process owner; library composition never mutates or clears them.
- Format loaders are separate descriptors passed into this core; core alone parses nothing beyond JSON.
- luma.gl/math.gl/mjolnir.js stay deck-owned transitive substrate; reach GPU decode only through deck props, never a direct import.

[RAIL_LAW]:
- Package: `@loaders.gl/core`
- Owns: the host-global loader registry, the `parse`/`load` decode pair, batch decoders, acquisition and encode surfaces, Web-Stream interop, request throttling, and loader types
- Accept: direct loader arguments, worker-backed parse with self-hosted worker URLs, `load(url, LASLoader)` feeding deck's binary `LayerDataSource`, loaders reaching deck through `loaders`/`loadOptions` props
- Reject: a `parseLas`/`parseTiles` family, host-global mutation in a multi-owner process, `_unregisterLoaders` outside tests, direct deck-substrate imports, foreign-CDN workers, hand-parsed binary formats
