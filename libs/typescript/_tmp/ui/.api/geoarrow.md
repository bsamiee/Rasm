# [API_CATALOGUE] @geoarrow/deck.gl-geoarrow

`@geoarrow/deck.gl-geoarrow` is ONE parameterized deck.gl `CompositeLayer` pattern instanced 14 ways: a layer whose accessors bind GPU attributes straight from GeoArrow-encoded Apache Arrow columns (an `arrow.Data<…>` column, or a function over the `arrow.RecordBatch`), never from per-row JS objects. The geometry column auto-infers by GeoArrow extension type (`geoarrow.point`/`.multipoint`/`.linestring`/`.polygon`/`.multipolygon`), so a decoded WKB→GeoArrow `RecordBatch` renders with zero row materialization. As of v0.4 every layer takes a single `arrow.RecordBatch` as `data` (not a chunked `Table`); a shared `threads` earcut pool triangulates polygons off-thread. The 14-layer roster is SEED DATA for the one pattern — a new geometry family is a row (geometry column, deck base layer), never a new mechanism. It is the `geoarrow`/`cell-index` source arms of the `render/geo.md` `GeoSeriesLayer` over the `interchange` GeometryRail Arrow projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@geoarrow/deck.gl-geoarrow`
- package: `@geoarrow/deck.gl-geoarrow`
- version: `0.4.1`
- license: `MIT`
- module: `dist/index.d.ts`; single `.` barrel export only (`exports: { ".": … }` — `types.d.ts`/`constants.d.ts` internals are not public)
- peer: `@deck.gl/core|layers|geo-layers ^9` (admitted, `deck.gl-core.md`/`deck.gl-layers.md`/`deck.gl-geo-layers.md`) + `@deck.gl/aggregation-layers ^9` and `@math.gl/polygon ^4.1` (declared peers NOT in the `render/geo.md` admitted deck set — the `Heatmap` layer + JS-polygon tessellation gap below), `apache-arrow >=15` (installed `21.1.0`, the columnar substrate — `apache-arrow.md`)
- deps: `@geoarrow/geoarrow-js ^0.3` (`ga.data.*` extension-typed columns + `worker/earcut`), `threads ^1.7` (earcut `Pool<FunctionThread>`)
- runtime: browser deck.gl layers; admitted by the `ui` `DATA_SURFACES` set, driven from `render/geo.md` — out-of-core (millions of features) with no per-feature JS iteration
- entry: 14 `GeoArrow*Layer` classes + matching `*Props`, `initEarcutPool`, and the 7 accessor/picking types; `GeoArrowTextLayer` is `_`-prefixed (`_GeoArrowTextLayer`), the experimental/unstable row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared accessor + picking type family (bound once, read by every layer)
- rail: viewer/geo
- Each accessor is a discriminated union: an Arrow column (`arrow.Data<…>`, GPU-bound directly, zero JS) OR a per-feature `AccessorFunction` over the `RecordBatch`. This union is the collapse point — the 14 layers differ only in which columns they bind, never in accessor mechanism. These 7 are the entire public type surface exported from the barrel.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                                                 |
| :-----: | :------------------------------------------- | :-------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Accessor<In, Out>` / `AccessorFunction<In, Out>` | accessor union | `Out` value OR `(ctx: AccessorContext<In>) => Out`; `AccessorContext = { index, data, target: number[] }` carries a reusable `target` buffer for GC-free fills (structural, reached through `AccessorFunction`) |
|  [02]   | `FloatAccessor`                              | scalar column   | `arrow.Data<arrow.Float>` OR `Accessor<RecordBatch, number>` — radius/width/elevation/weight |
|  [03]   | `ColorAccessor`                              | color column    | `arrow.Data<arrow.FixedSizeList<arrow.Uint8>>` (RGBA) OR `Accessor<RecordBatch, Color \| Color[]>` |
|  [04]   | `NormalAccessor`                             | normal column   | `arrow.Data<arrow.FixedSizeList<arrow.Float32>>` OR `Accessor<RecordBatch, …>` — `GeoArrowPointCloudLayer.getNormal` |
|  [05]   | `TimestampAccessor`                          | temporal column | `arrow.Data<arrow.List<arrow.Float>>` — `GeoArrowTripsLayer.getTimestamps` animated paths |
|  [06]   | `GeoArrowPickingInfo`                        | pick receipt    | `render/geo.md` — deck `PickingInfo & { object?: arrow.StructRowProxy }`; the picked feature as a zero-copy Arrow row proxy, never a materialized GeoJSON `Feature` |

[PUBLIC_TYPE_SCOPE]: the layer-props family — the ONE pattern, 14 instances
- rail: viewer/geo
- Every `GeoArrow<Name>LayerProps` is `Omit<Deck<Name>LayerProps<RecordBatch>, "data" | geometry-accessors> & { data: arrow.RecordBatch; get…?: ga.data.<Geom>Data \| <Accessor>; _validate?: boolean } & CompositeLayerProps`. The layer subclasses the deck base layer and re-binds its accessors to columns; `_validate` (default `true`) gates chunk-length agreement.

| [INDEX] | [LAYER + PROPS] (`GeoArrow…Layer` / `…LayerProps`) | [GEOMETRY / KEY COLUMN]                              | [WRAPS]                                       |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Scatterplot`                                       | `getPosition` — `geoarrow.point`/`multipoint` (inferred) | `ScatterplotLayer` (`deck.gl-layers.md`) |
|  [02]   | `PointCloud`                                        | `getPosition` + `getNormal` — point                 | `PointCloudLayer` (layers)                    |
|  [03]   | `Column`                                            | `getPosition` — point, extruded                     | `ColumnLayer` (layers)                        |
|  [04]   | `Arc`                                               | `getSourcePosition` + `getTargetPosition` (+`getTilt`/`getHeight`) | `ArcLayer` (layers)            |
|  [05]   | `Path`                                              | `getPath` — `geoarrow.linestring`                   | `PathLayer` (layers)                          |
|  [06]   | `Polygon`                                           | `getPolygon` — `geoarrow.polygon` (composite fill+stroke) | `PolygonLayer` (layers)                 |
|  [07]   | `SolidPolygon`                                      | `getPolygon` — polygon, earcut-triangulated         | `SolidPolygonLayer` (layers)                  |
|  [08]   | `_Text` (`_GeoArrowTextLayer`, experimental)        | `getPosition` + `getText`/`getPixelOffset`          | `TextLayer` (layers)                          |
|  [09]   | `Heatmap` ⚠ unresolved peer                          | `getPosition` + `getWeight` — density aggregation   | `HeatmapLayer` (`@deck.gl/aggregation-layers`, NOT admitted) |
|  [10]   | `H3Hexagon`                                         | `getHexagon` — H3 cell-id column                    | `H3HexagonLayer` (`deck.gl-geo-layers.md`)    |
|  [11]   | `Geohash`                                           | `getGeohash` — geohash cell-id column               | `GeohashLayer` (geo-layers)                   |
|  [12]   | `A5`                                                | `getPentagon` — A5 pentagon cell-id column          | `A5Layer` (geo-layers)                        |
|  [13]   | `S2`                                                | `getS2Token` — S2 token cell-id column              | `S2Layer` (geo-layers)                        |
|  [14]   | `Trips`                                             | `getPath` + `getTimestamps` — animated trajectories | `TripsLayer` (geo-layers)                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: layer construction + the shared earcut pool
- rail: viewer/geo
- Construction is uniform: `new GeoArrow<Name>Layer({ id, data: recordBatch, pickable, get…: column | fn })`. The geometry column infers from the GeoArrow extension metadata when the accessor is omitted; a picked feature returns as a `GeoArrowPickingInfo` with an `arrow.StructRowProxy`.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                                    |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `new GeoArrow<Name>Layer<ExtraProps>(props: GeoArrow<Name>LayerProps & ExtraProps)` | construct | `render/geo.md` `overlayLayers`/`cellIndexLayers` — one `LayersList` over the decoded `RecordBatch`, fed to `MapboxOverlay.setProps({ layers })` |
|  [02]   | `Layer.renderLayers(): Layer \| LayersList \| null`                           | composite      | expands to per-geometry sublayers (`_renderPointLayer`/`_renderMultiPointLayer` split multi-geometry) |
|  [03]   | `static defaultProps: DefaultProps<…Props>` / `static layerName: string`      | layer identity | deck update-diff + prop merge; overridable per instance                 |
|  [04]   | `Layer.getPickingInfo(params): GeoArrowPickingInfo`                           | pick           | screen pixel → `arrow.StructRowProxy` feature; a selection/GlobalId resolution reads the picked proxy's key column directly, never a materialized object |
|  [05]   | `initEarcutPool(earcutWorkerUrl?: string \| URL \| null, optionsOrSize?: number \| PoolOptions): Promise<Pool<FunctionThread> \| null>` | worker pool | `render/geo.md` builds ONE pool at the surface boundary and passes it via `earcutWorkerPool` to every `SolidPolygon`/`Polygon` layer — a per-layer auto-pool triangulates once and dies under the v0.4 `RecordBatch` refactor |

## [04]-[IMPLEMENTATION_LAW]

[GEOARROW_TOPOLOGY]:
- one pattern, geometry as data: a layer is a deck `CompositeLayer` re-binding the base layer's accessors to GeoArrow columns; the roster is a table keyed by (geometry family, deck base layer), never 14 hand-authored surfaces. A new layer is a row.
- `RecordBatch`, not `Table` (v0.4): `data` is one `arrow.RecordBatch`; the caller iterates `Table.batches` and mounts one layer per batch, so a chunked table is a `LayersList`, and an internally-created earcut pool would run once and die — hoist it with `initEarcutPool`.
- extension-type inference: an omitted geometry accessor resolves by scanning columns for the GeoArrow extension name (`geoarrow.point`/`.multipoint`/`.linestring`/`.polygon`/`.multipolygon`); explicit `ga.data.*Data` columns override. Multi-geometry rows explode to single geometries and map back through `invertedGeomOffsets` so picking returns the original row.
- columnar accessors are zero-copy: an `arrow.Data<FixedSizeList<Uint8>>` color column or `Data<Float>` scalar binds as a GPU `BinaryAttribute` directly; the `AccessorFunction` fallback fills the reusable `AccessorContext.target` buffer to avoid per-feature allocation. `_validate` (default on) asserts chunk-length agreement before upload.

[STACKING_LAW]:
- `render/geo.md` `GeoSeriesLayer` is the sole consumer: the `overlay` case dispatches `source` under `Match.value` — the `geoarrow` arm binds `GeoArrowScatterplotLayer`/`GeoArrowPathLayer`/`GeoArrowPolygonLayer` (the polygon arm carries `earcutWorkerPool: src.earcutPool`), and the `cell-index` arm binds the Arrow-native `GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer`/`GeoArrowTripsLayer` over the SAME `src.batch`. The two JS-cell families (`H3ClusterLayer`/`QuadkeyLayer`, `deck.gl-geo-layers.md`) bind their JS cell-id projection `src.cells`, never the batch — feeding a `RecordBatch` into a `_GeoCellLayer` forces the per-feature materialization the no-second-decode law forbids.
- apache-arrow substrate (`apache-arrow.md`): `data` is an `arrow.RecordBatch`; GeoArrow columns are `arrow.Data<…>` typed by `FixedSizeList`/`List`/`Struct`; the `RecordBatch` arrives already-decoded from the `interchange` GeometryRail (`import type { RecordBatch } from "@rasm/ts/interchange"`) — the WKB→GeoArrow decode is `wire`-owned, never re-minted in `ui`. The picked `StructRowProxy` is Arrow's zero-copy row view — no JS object is ever built.
- `deck.gl-core.md` / `deck.gl-layers.md` / `deck.gl-geo-layers.md`: each GeoArrow layer subclasses a deck base layer (`CompositeLayer`) and returns a `LayersList`; `GeoArrowPickingInfo` extends `@deck.gl/core` `PickingInfo`. The vector family wraps `layers`, the cell/trip family wraps `geo-layers`.
- `deck.gl-mapbox.md` + `maplibre-gl.md`: the layer array rides a `MapboxOverlay` in interleaved mode over the MapLibre WebGL2 context via `setProps({ layers })`, sharing the `render/geo.md` camera/view state — deck GPU layers composite into the same frame as the basemap; one overlay owns every layer, never a parallel `Deck`.
- universal `libs/typescript/.api/effect.md`: the `RecordBatch` is a `Schema`-decoded wire projection; the `Deck`/`MapboxOverlay` is an `Effect.acquireRelease` scoped resource (`overlay.finalize()` in release); `render/geo.md` folds the batch stream to a `LayersList` under `Match`/`Effect.forEach`, and the `GeoSeriesLayer` is a `Data.TaggedEnum` dispatched by `$match`.
- universal `libs/typescript/.api/effect-platform-browser.md`: heavy Arrow IPC decode belongs off-main-thread on the `BrowserWorker.layer` decode pool `ui` declares as a port; the earcut `threads` pool (`initEarcutPool`) is this package's own vendored worker path for triangulation — one shared pool, hoisted at surface mount, never per-layer.

[LOCAL_ADMISSION]:
- Feed layers a decoded `arrow.RecordBatch` and bind accessors to columns; never materialize `RecordBatch → Array<object>` for a per-row accessor — that discards the whole point of GeoArrow.
- Create ONE `initEarcutPool` at surface mount and pass it via `earcutWorkerPool` to every polygon layer; never let each `SolidPolygonLayer` spin its own single-use pool under the `RecordBatch` refactor.
- Let extension-type inference pick the geometry column; supply explicit `ga.data.*` columns only when a batch carries multiple geometry columns.
- Treat `_GeoArrowTextLayer` as experimental — gate it behind a capability flag, not a load-bearing row.
- `GeoArrowHeatmapLayer` (needs `@deck.gl/aggregation-layers`) and the JS-polygon tessellation path (needs `@math.gl/polygon`) stay UNRESOLVED until those peers are admitted; the vector/cell/temporal families over the admitted `core`/`layers`/`geo-layers` are the resolved surface today.

[RAIL_LAW]:
- Package: `@geoarrow/deck.gl-geoarrow`
- Owns: the GeoArrow-column deck `CompositeLayer` roster (vector/cell/temporal families + the unresolved aggregation row), the columnar accessor union (`Float`/`Color`/`Normal`/`Timestamp`), extension-type geometry inference, `arrow.StructRowProxy` picking, and the shared earcut `threads` pool
- Accept: one `arrow.RecordBatch` as `data`, column-or-function accessors, inferred geometry columns, a hoisted `initEarcutPool`, per-batch layer fan-out for chunked tables, `MapboxOverlay` interleave over MapLibre
- Reject: row-materialized data feeding object accessors, per-layer auto earcut pools, a re-mint of the WKB→GeoArrow decode inside `ui` (it is `wire`-owned), the experimental text layer as a stable surface, feeding a `RecordBatch` into a JS-data `_GeoCellLayer`
