# [TS_UI_API_LOADERS_GL_LAS]

`@loaders.gl/las` owns LAS/LAZ point-cloud decoding for the `ui/viewer/geo` plane: stateless loader-descriptor consts pass to `@loaders.gl/core` or a deck layer and decode to `LASMesh` columnar buffers or an Arrow table. Shared format identity across every descriptor blocks content-sniff selection, so a caller passes one descriptor explicitly, and `scope:viewer` admission compile-time excludes the package from the `ui` core.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the option bag threaded to the loaders and the decoded shapes they return.
- `LASLoaderOptions` — `LoaderOptions & { las?, onProgress? }`; the base bag (`worker`/`fetch`/`CDN`) lives in `.api/loaders.gl-core.md`, and `las` carries `shape` (DEAD — see below), `skip` decimation stride for LOD ingest, `fp64` 64-bit position precision, `colorDepth` `COLOR_0` RGB scale, and `workerUrl` self-hosted worker bundle.
- `las.shape` is declared `'mesh' | 'columnar-table' | 'arrow-table'` and is DEAD in the shipped source: the parse body's mesh-to-table conversion is commented out, so every descriptor returns `LASMesh` unconditionally and a `shape: 'arrow-table'` request silently receives a mesh. Arrow egress rides `LASArrowLoader`, never this row.
- `LASMesh` — `Mesh & { loader: 'las'; loaderData: LASHeader; topology: 'point-list'; mode: 0 }`; `attributes` (`POSITION`/`COLOR_0`/`NORMAL`, intensity, classification) are the columnar buffers deck binds directly.
- `LASHeader` — `pointsCount`/`pointsFormatId`/`pointsStructSize`, `scale`/`offset` triples, optional `mins`/`maxs` bounds, `hasColor`/`isCompressed`, `versionAsString`, and `totalRead`/`totalToRead` progress ride the parsed `loaderData`.
- `ArrowTable` (`@loaders.gl/schema`) — `{ shape: 'arrow-table'; schema?: Schema; data: arrow.Table }`; `data` is a real `apache-arrow` `Table`, arrow being a hard dependency of the schema package rather than a structural stand-in, so a columnar consumer projects `.data` with no cast.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every export is a stateless loader-descriptor const passed to core or deck, or called through its own `parse`.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `LASLoader`                                                    | value    | `= LAZPerfLoader`, the default full loader        |
|  [02]   | `LAZPerfLoader.parse/parseSync(ArrayBuffer, opts?) -> LASMesh` | property | laz-perf C++ WASM; sync-capable; declines LAS 1.4 |
|  [03]   | `LAZRsLoader.parse(ArrayBuffer, opts?) -> Promise<LASMesh>`    | property | laz-rs Rust WASM; LAS 1.4 extended formats; async |
|  [04]   | `LASArrowLoader.parse(ArrayBuffer) -> Promise<ArrowTable>`     | property | main-thread Arrow egress; reads NO options        |
|  [05]   | `LASWorkerLoader`                                              | value    | `worker: true`, parserless; core worker delegate  |
|  [06]   | `LASFormat`                                                    | value    | format identity for sniff registries; no parser   |

- Rows [01]–[03] spread `LASWorkerLoader`, so each inherits `worker: true` and decodes in the core pool at the `las.workerUrl` bundle; each returns `LASMesh` unconditionally whatever `las.shape` asks for.
- `LASArrowLoader` spreads `LAZPerfLoader` and overrides `worker: false`, so Arrow egress is main-thread only and has no worker-side twin; its `parse` hardcodes the `'arrow-table'` shape.
- `LASArrowLoader.parse` declares the bare `ArrayBuffer` and DROPS the options argument core passes it, re-entering `LAZPerfLoader.parse` with the buffer alone — `las.skip`/`fp64`/`colorDepth` never reach the Arrow lane even through `load(url, LASArrowLoader, options)`, which decodes at the package defaults. Thinning an Arrow frame slices the decoded table rather than selecting a loader row.
- `LASArrowLoader.parseSync`: spread-inherited, returns `LASMesh` not `ArrowTable`; Arrow consumers never call it.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Shared format identity across every descriptor blocks `selectLoader`; a caller passes one descriptor explicitly, and unknown input takes one caller-owned dispatcher that reads the LAS header and branches.
- LAZ decompression runs worker-backed in the core pool; the default `las.workerUrl` points at unpkg.com, so `viewer` overrides it to the `[ASSET_IDENTITY]` served-asset worker and holds CSP.

[STACKING]:
- `@loaders.gl/core`(`.api/loaders.gl-core.md`): `parse`/`load` receive the selected descriptor directly — `load(url, LASLoader)` fetches and decodes; `batchType: never` makes `parseInBatches` buffer the whole input into one fallback batch.
- `@deck.gl/layers` `PointCloudLayer`(`.api/deck.gl-layers.md`): `LASMesh` attributes bind deck's binary `LayerDataSource`, and `viewer/geo` passes the descriptor through the layer's `loaders` prop with `data` a `load(url, LASLoader)` promise.
- `data` Arrow bus: explicit `LASArrowLoader` emits a `@loaders.gl/schema` `ArrowTable` whose `data` member is the `apache-arrow` `Table` the columnar lane consumes; the mesh lane and the Arrow lane are distinct loaders, never one call parameterized by `las.shape`.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it, keeping the WASM decoder and worker deps out of non-spatial apps.
- pick laz-perf for the sync-capable common path, laz-rs for the LAS 1.4 extended formats, and `LASArrowLoader` for Arrow egress; pass one descriptor per call and never register into core's host-global registry.
- mesh-lane loaders carry the `las` policy rows; the Arrow lane carries none, so a policy bag spread onto that call is a dead knob the reader mistakes for a live one.
