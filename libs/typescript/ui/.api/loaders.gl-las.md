# [TS_UI_API_LOADERS_GL_LAS]

`@loaders.gl/las` is the LAS/LAZ point-cloud decoder family the `ui/viewer/geo` plane passes directly to `@loaders.gl/core` and deck layers. `LAZPerfLoader` (re-exported as `LASLoader`) drives laz-perf with async and synchronous mesh parsing, `LAZRsLoader` drives laz-rs for extended point-record formats, and `LASArrowLoader.parse` emits `ArrowTable`. Mesh loaders return `LASMesh`, whose columnar attributes feed deck's binary `LayerDataSource`. Every descriptor shares the same format identity, so selection cannot choose a backend or output shape; callers pass one descriptor explicitly. `scope:viewer` project-local: admitted only by the `ui/viewer` Nx project and compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@loaders.gl/las`
- package: `@loaders.gl/las` (MIT)
- abi: isomorphic browser/node; LAZ decompression runs in laz-perf (C++) or laz-rs (Rust) WASM, worker-backed through the core parse engine (`options.worker`)
- peer: `@loaders.gl/core` (`~catalog`, the `parse`/`load` surface receiving these descriptors, `.api/loaders.gl-core.md`)
- deps (loaders.gl framework substrate, transitive): `@loaders.gl/loader-utils` (`LoaderOptions`/`Loader` types), `@loaders.gl/schema` (`Mesh`/`MeshAttributes`/`Schema`/`ArrowTable`), `@loaders.gl/schema-utils`, `laz-perf` (laz-perf WASM decoder); the laz-rs Rust WASM decoder is vendored in the package, not a runtime dep
- catalog-verdict: KEEP — the LAS/LAZ decoder `PointCloudLayer` receives through its loader props
- runtime: `scope:viewer` project-local; descriptors are stateless and pass directly to core or deck
- modules: `LASLoader` (= `LAZPerfLoader`), `LAZPerfLoader`, `LAZRsLoader`, `LASArrowLoader`, `LASWorkerLoader`, `LASFormat`, `LASLoaderOptions` (type)
- absent (not admitted): sibling format packages and core's decode surface remain separate owners

## [02]-[DECODER_FAMILY]

[TYPE_SCOPE]: each LAS/LAZ descriptor passes directly to `@loaders.gl/core`. Backend and output shape select the descriptor; `las.shape` is declared but the laz-perf parser returns `LASMesh` without consulting it.
- every descriptor carries `name: 'LAS'`, `id: 'las'`, `extensions: ['las', 'laz']`, `mimeTypes: ['application/octet-stream']`, `tests: ['LASF']`, and `binary: true`; `selectLoader` cannot distinguish them.
- mesh descriptors carry `dataType: LASMesh`; `LASArrowLoader` carries `dataType: ArrowTable`. Every descriptor carries `batchType: never`, so core's batch fallback buffers the whole input and emits one batch.

| [INDEX] | [SYMBOL]          | [SIGNATURE]                                       | [CONSUMER_BOUNDARY]                                     |
| :-----: | :---------------- | :------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `LASLoader`       | `= LAZPerfLoader` — the default full loader       | core and deck accept this descriptor                    |
|  [02]   | `LAZPerfLoader`   | `parse`+`parseSync(ArrayBuffer,opts?) => LASMesh` | laz-perf C++ WASM; declines the extended point formats  |
|  [03]   | `LAZRsLoader`     | `parse(ArrayBuffer,opts?) => Promise<LASMesh>`    | laz-rs Rust WASM; reads extended formats; async-only    |
|  [04]   | `LASArrowLoader`  | `parse(ArrayBuffer) => Promise<ArrowTable>`       | async Arrow egress; no admitted sync call               |
|  [05]   | `LASWorkerLoader` | `Loader` — `worker: true`, no bundled parser      | parser-less; decode delegates to the core worker pool   |
|  [06]   | `LASFormat`       | `{ name, id, extensions, mimeTypes, tests }`      | format identity for sniff registries; carries no parser |

## [03]-[OPTIONS_AND_TYPES]

[TYPE_SCOPE]: the `las` sub-options bag threaded through `LASLoaderOptions`, and the decoded `LASMesh`/`LASHeader` shapes the loaders return — the vocabulary `PointCloudLayer` binary attributes and the Arrow egress consume.
- `LASLoaderOptions` — `LoaderOptions & { las?: {...}, onProgress? }`; the base bag (`worker`, `fetch`, `CDN`) lives in `.api/loaders.gl-core.md`.
- `las.shape` — declared as `'mesh' | 'columnar-table' | 'arrow-table'`; parsers do not dispatch on it, so descriptor choice owns output shape.
- `las.skip` — decimation stride; keep every Nth point for LOD ingest of oversized scans.
- `las.fp64` — 64-bit position precision toggle for georeferenced clouds exceeding fp32 range.
- `las.colorDepth` — `number | string` RGB scaling input for the `COLOR_0` attribute.
- `las.workerUrl` — self-hosted worker bundle URL; DEFAULT loads from unpkg.com, so `viewer` MUST set this to the served-asset worker to hold CSP.
- `LASMesh` — `Mesh & { loader: 'las'; loaderData: LASHeader; topology: 'point-list'; mode: 0 }`; the `attributes` (`POSITION`/`COLOR_0`/`NORMAL`/intensity/classification) are the columnar buffers deck binds directly.
- `LASHeader` — `pointsCount`/`pointsFormatId`/`pointsStructSize`, `scale`/`offset` triples, optional `mins`/`maxs` bounds, `hasColor`, `isCompressed`, `versionAsString`, `totalRead`/`totalToRead` progress; the parsed header on `loaderData`.

## [04]-[IMPLEMENTATION_LAW]

[REGISTRY_TOPOLOGY]:
- explicit descriptors own dispatch: `load`/`parse` and deck's `loaders` prop receive `LASLoader`, `LAZRsLoader`, or `LASArrowLoader` directly. Shared identity prevents `selectLoader` from choosing among them; unknown inputs require one caller-owned dispatcher that inspects the LAS header and branches internally.
- backend discriminant: `LAZPerfLoader`/`LASLoader` own the sync-capable common path, `LAZRsLoader` owns extended formats, and `LASArrowLoader.parse` owns Arrow output. `LASArrowLoader` inherits a mesh-returning `parseSync` through object spread, so Arrow consumers never call that member.
- worker-backed by default: LAZ decompression runs in the core worker pool (`options.worker`); the default `las.workerUrl` points at unpkg.com, so `viewer` overrides it to the `[ASSET_IDENTITY]` served-asset worker — never a foreign CDN, keeping CSP airtight the same way core does.

[INTEGRATION_LAW]:
- Stack under `@loaders.gl/core` (`.api/loaders.gl-core.md`): core owns `parse`/`load`; callers pass the selected LAS descriptor directly. `load(url, LASLoader)` fetches and decodes a LAS/LAZ resource. LAS descriptors expose no batch parser, so `parseInBatches` buffers the input and emits one fallback batch.
- Stack under `@deck.gl/layers` `PointCloudLayer` (`.api/deck.gl-layers.md`): `LASMesh` attributes bind to deck's binary `LayerDataSource`; `data` is a `load(url, LASLoader)` promise, and the descriptor reaches deck through `loaders`/`loadOptions`.
- Stack with the `data` Arrow bus: explicit `LASArrowLoader` selection emits a `@loaders.gl/schema` `ArrowTable` through async `parse`.
- Stack with `viewer/geo#LAYER_ROWS`: `PointCloudLayer` receives the selected LAS descriptor through its `loaders` prop.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it — heavy WASM decoder/worker deps stay compile-time excluded from non-spatial apps.
- pass one descriptor explicitly per call or layer; reusable library code never mutates core's deprecated host-global registry.
- pick laz-perf for the sync-capable common path, laz-rs for extended point formats, and `LASArrowLoader.parse` for Arrow egress; `las.shape` does not select parser output.

[RAIL_LAW]:
- Package: `@loaders.gl/las`
- Owns: the LAS/LAZ decoder descriptors, mesh and Arrow outputs, worker delegate, format identity, loader options, and `LASMesh`/`LASHeader` shapes
- Accept: explicit descriptor selection by point-record format and output, `LASMesh` attributes feeding deck's binary `LayerDataSource`, async `LASArrowLoader.parse` feeding Arrow consumers, a self-hosted `las.workerUrl`
- Reject: host-global registration in reusable code, `selectLoader` as backend/output dispatch, `LASArrowLoader.parseSync`, streaming claims over the whole-file batch fallback, reliance on `las.shape`, foreign worker URLs, hand-parsed LAS/LAZ bytes
