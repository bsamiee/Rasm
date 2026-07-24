# [TS_UI_API_LOADERS_GL_LAS]

`@loaders.gl/las` owns LAS/LAZ point-cloud decoding for the `ui/viewer/geo` plane: stateless loader-descriptor consts pass to `@loaders.gl/core` or a deck layer and decode to `LASMesh` columnar buffers or an Arrow table. Shared format identity across every descriptor blocks content-sniff selection, so a caller passes one descriptor explicitly, and `scope:viewer` admission compile-time excludes the package from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@loaders.gl/las`
- package: `@loaders.gl/las` (MIT)
- module: esm barrel of loader-descriptor consts and the `LASLoaderOptions` type
- runtime: isomorphic browser/node, `scope:viewer`; LAZ decompression runs worker-backed in laz-perf (C++) or laz-rs (Rust) WASM through core's parse engine (`options.worker`)
- rail: LAS/LAZ decoder descriptors `PointCloudLayer` receives through its loader props

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the option bag threaded to the loaders and the decoded shapes they return.
- `LASLoaderOptions` — `LoaderOptions & { las?, onProgress? }`; the base bag (`worker`/`fetch`/`CDN`) lives in `.api/loaders.gl-core.md`, and `las` carries `shape` (declared `'mesh' | 'columnar-table' | 'arrow-table'`, unread — descriptor choice owns output), `skip` decimation stride for LOD ingest, `fp64` 64-bit position precision, `colorDepth` `COLOR_0` RGB scale, and `workerUrl` self-hosted worker bundle.
- `LASMesh` — `Mesh & { loader: 'las'; loaderData: LASHeader; topology: 'point-list'; mode: 0 }`; `attributes` (`POSITION`/`COLOR_0`/`NORMAL`, intensity, classification) are the columnar buffers deck binds directly.
- `LASHeader` — `pointsCount`/`pointsFormatId`/`pointsStructSize`, `scale`/`offset` triples, optional `mins`/`maxs` bounds, `hasColor`/`isCompressed`, `versionAsString`, and `totalRead`/`totalToRead` progress ride the parsed `loaderData`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every export is a stateless loader-descriptor const passed to core or deck, or called through its own `parse`.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `LASLoader`                                                    | value    | `= LAZPerfLoader`, the default full loader        |
|  [02]   | `LAZPerfLoader.parse/parseSync(ArrayBuffer, opts?) -> LASMesh` | property | laz-perf C++ WASM; sync-capable; declines LAS 1.4 |
|  [03]   | `LAZRsLoader.parse(ArrayBuffer, opts?) -> Promise<LASMesh>`    | property | laz-rs Rust WASM; LAS 1.4 extended formats; async |
|  [04]   | `LASArrowLoader.parse(ArrayBuffer) -> Promise<ArrowTable>`     | property | async Arrow egress                                |
|  [05]   | `LASWorkerLoader`                                              | value    | `worker: true`, parserless; core worker delegate  |
|  [06]   | `LASFormat`                                                    | value    | format identity for sniff registries; no parser   |

- `LASArrowLoader.parseSync`: spread-inherited, returns `LASMesh` not `ArrowTable`; Arrow consumers never call it.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Shared format identity across every descriptor blocks `selectLoader`; a caller passes one descriptor explicitly, and unknown input takes one caller-owned dispatcher that reads the LAS header and branches.
- LAZ decompression runs worker-backed in the core pool; the default `las.workerUrl` points at unpkg.com, so `viewer` overrides it to the `[ASSET_IDENTITY]` served-asset worker and holds CSP.

[STACKING]:
- `@loaders.gl/core`(`.api/loaders.gl-core.md`): `parse`/`load` receive the selected descriptor directly — `load(url, LASLoader)` fetches and decodes; `batchType: never` makes `parseInBatches` buffer the whole input into one fallback batch.
- `@deck.gl/layers` `PointCloudLayer`(`.api/deck.gl-layers.md`): `LASMesh` attributes bind deck's binary `LayerDataSource`, and `viewer/geo` passes the descriptor through the layer's `loaders` prop with `data` a `load(url, LASLoader)` promise.
- `data` Arrow bus: explicit `LASArrowLoader.parse` emits a `@loaders.gl/schema` `ArrowTable` for the columnar `data` lane.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it, keeping the WASM decoder and worker deps out of non-spatial apps.
- pick laz-perf for the sync-capable common path, laz-rs for the LAS 1.4 extended formats, and `LASArrowLoader.parse` for Arrow egress; pass one descriptor per call and never register into core's host-global registry.

[RAIL_LAW]:
- Package: `@loaders.gl/las`
- Owns: the LAS/LAZ decoder descriptors, their mesh and Arrow outputs, the worker delegate, format identity, `LASLoaderOptions`, and the `LASMesh`/`LASHeader` shapes
- Accept: explicit descriptor selection by point-record format and output, `LASMesh` attributes feeding deck's binary `LayerDataSource`, async `LASArrowLoader.parse` feeding Arrow consumers, a self-hosted `las.workerUrl`
- Reject: host-global registration in reusable code, `selectLoader` as backend/output dispatch, `LASArrowLoader.parseSync`, streaming claims over the whole-file batch fallback, reliance on `las.shape`, foreign worker URLs, hand-parsed LAS/LAZ bytes
