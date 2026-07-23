# [TS_UI_API_GEOARROW_DECK_GL_GEOARROW]

`@geoarrow/deck.gl-geoarrow` is ONE parameterized layer pattern instanced 14 ways: a deck.gl `CompositeLayer` whose accessors bind GPU attributes straight from GeoArrow-encoded Arrow columns (an `arrow.Data<…>` column, or a function over the `arrow.RecordBatch`), never from per-row JS objects. The geometry column is auto-inferred by GeoArrow extension type (`geoarrow.point`/`.linestring`/`.polygon` + `multi*`), so a decoded WKB→GeoArrow `RecordBatch` renders with zero row materialization. As of catalog-bound every layer takes a single `RecordBatch` as `data` (not a chunked `Table`); a shared `threads` earcut pool triangulates polygons off-thread. The roster below is SEED DATA for the one pattern — a new geometry family is a row, never a new mechanism.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@geoarrow/deck.gl-geoarrow`
- package: `@geoarrow/deck.gl-geoarrow` (MIT)
- peer: `@deck.gl/core|layers|geo-layers catalog` (admitted `[VIEWER_GEO]`) + `@deck.gl/aggregation-layers catalog` and `@math.gl/polygon catalog` (verified peers NOT in the roster — the admission gap below), `apache-arrow catalog` (installed `catalog`, `.api/apache-arrow.md`)
- deps: `@geoarrow/geoarrow-js catalog` (`ga.data.*` extension-typed columns + `worker/earcut`), `threads catalog` (earcut `Pool<FunctionThread>`)
- catalog-verdict: KEEP
- runtime: `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core
- entry: single `.` barrel (`dist/index.d.ts`) — 14 `GeoArrow*Layer` classes + matching `*Props`, `initEarcutPool`, and the accessor/picking type family; `GeoArrowTextLayer` is `_`-prefixed (`_GeoArrowTextLayer`), the overlay/unstable row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared accessor + picking type family (bound once, read by every layer)
- rail: viewer/geo
- Each accessor is a discriminated union: an Arrow column (`arrow.Data<…>`, GPU-bound directly, zero JS) OR a per-feature `AccessorFunction` over the `RecordBatch`. This union is the collapse point — the 14 layers differ only in which columns they bind, never in accessor mechanism.
- `GeoArrowLayerData<T>` is the internal carrier `{ data: T; length: number; attributes?: Record<string, TypedArray | BinaryAttribute> }`.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                                            |
| :-----: | :-------------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `Accessor<In, Out>`         | accessor union  | `Out` value OR the per-feature `AccessorFunction`                              |
|  [02]   | `AccessorFunction<In, Out>` | accessor fn     | `(ctx: AccessorContext<In>) => Out`; `ctx` carries `index`/`data`/`target`     |
|  [03]   | `FloatAccessor`             | scalar column   | `arrow.Data<arrow.Float>` OR fn — radius/width/elevation/weight                |
|  [04]   | `ColorAccessor`             | color column    | `arrow.Data<FixedSizeList<Uint8>>` (RGBA) OR a `Color \| Color[]` fn           |
|  [05]   | `NormalAccessor`            | normal column   | `arrow.Data<FixedSizeList<Float32>>` — `GeoArrowPointCloudLayer.getNormal`     |
|  [06]   | `TimestampAccessor`         | temporal column | `arrow.Data<List<Float>>` — `GeoArrowTripsLayer.getTimestamps` animated paths  |
|  [07]   | `GeoArrowPickingInfo`       | pick receipt    | `PickingInfo & { object?: arrow.StructRowProxy }` — zero-copy picked Arrow row |
|  [08]   | `GeoArrowLayerData<T>`      | data envelope   | `{ data, length, attributes? }` — the binary-attribute carrier                 |
|  [09]   | `TypedArray`                | binary buffer   | the GPU attribute buffer element type                                          |

[PUBLIC_TYPE_SCOPE]: the layer-props family — the ONE pattern, 14 instances
- rail: viewer/geo
- Every `GeoArrow<Name>LayerProps` is `Omit<Deck<Name>LayerProps<RecordBatch>, "data" | geometry-accessors> & { data: arrow.RecordBatch; get…?: ga.data.<Geom>Data \| <Accessor>; _validate?: boolean } & CompositeLayerProps`. The layer subclasses the deck base layer and re-binds its accessors to columns; `_validate` (default `true`) gates chunk-length agreement.
- Base layer is `@deck.gl/layers` unless the `[WRAPS]` cell marks `geo-layers`/`aggregation-layers`; `_GeoArrowTextLayer` is the `_`-prefixed overlay class.

| [INDEX] | [LAYER]           | [GEOMETRY_KEY_COLUMN]                                         | [WRAPS]                                           |
| :-----: | :---------------- | :------------------------------------------------------------ | :------------------------------------------------ |
|  [01]   | `Scatterplot`     | `getPosition` — `geoarrow.point`/`multipoint` (inferred)      | `ScatterplotLayer`                                |
|  [02]   | `PointCloud`      | `getPosition` + `getNormal` — point                           | `PointCloudLayer`                                 |
|  [03]   | `Column`          | `getPosition` — point, extruded                               | `ColumnLayer`                                     |
|  [04]   | `Arc`             | `getSourcePosition`/`getTargetPosition`/`getTilt`/`getHeight` | `ArcLayer`                                        |
|  [05]   | `Path`            | `getPath` — `geoarrow.linestring`                             | `PathLayer`                                       |
|  [06]   | `Polygon`         | `getPolygon` — `geoarrow.polygon` (composite fill+stroke)     | `PolygonLayer`                                    |
|  [07]   | `SolidPolygon`    | `getPolygon` — polygon, earcut-triangulated                   | `SolidPolygonLayer`                               |
|  [08]   | `_Text` (overlay) | `getPosition` + `getText`/`getPixelOffset`                    | `TextLayer`                                       |
|  [09]   | `Heatmap` ⚠       | `getPosition` + `getWeight` — density aggregation             | `HeatmapLayer` (`aggregation-layers`, unadmitted) |
|  [10]   | `H3Hexagon`       | `getHexagon` — H3 cell-id column                              | `H3HexagonLayer` (`geo-layers`)                   |
|  [11]   | `Geohash`         | `getGeohash` — geohash cell-id column                         | `GeohashLayer` (`geo-layers`)                     |
|  [12]   | `A5`              | `getPentagon` — A5 pentagon cell-id column                    | `A5Layer` (`geo-layers`)                          |
|  [13]   | `S2`              | S2 token cell-id column (`getFillColor`/`getElevation`)       | `S2Layer` (`geo-layers`)                          |
|  [14]   | `Trips`           | `getPath` + `getTimestamps` — animated trajectories           | `TripsLayer` (`geo-layers`)                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: layer construction
- rail: viewer/geo
- Construction is uniform: `new GeoArrow<Name>Layer({ id, data: recordBatch, get…: column | fn })`. The geometry column is inferred from the GeoArrow extension metadata when the accessor is omitted; a picked feature returns as a `GeoArrowPickingInfo` with an `arrow.StructRowProxy`.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                        |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `new GeoArrow<Name>Layer<ExtraProps>(props)`        | construct      | `viewer/geo/layers.md` — one layer array per `RecordBatch` |
|  [02]   | `Layer.renderLayers(): Layer \| LayersList \| null` | composite      | expands to per-geometry sublayers; multi-geometry splits   |
|  [03]   | `static defaultProps: DefaultProps<…Props>`         | layer identity | deck update-diff + prop merge                              |
|  [04]   | `static layerName: string`                          | layer identity | the class name; overridable per instance                   |
|  [05]   | `Layer.getPickingInfo(params): GeoArrowPickingInfo` | pick           | screen pixel → `arrow.StructRowProxy` feature              |

[ENTRYPOINT_SCOPE]: the shared earcut worker pool
- rail: viewer/geo
- `initEarcutPool(earcutWorkerUrl?: string | URL | null, optionsOrSize?: number | PoolOptions): Promise<Pool<FunctionThread> | null>`

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `initEarcutPool(workerUrl?, optionsOrSize?)` | worker pool    | share ONE `threads` pool across polygon layers via `earcutWorkerPool` |

## [04]-[IMPLEMENTATION_LAW]

[GEOARROW_TOPOLOGY]:
- one pattern, geometry as data: a layer is a deck `CompositeLayer` re-binding the base layer's accessors to GeoArrow columns; the roster is a table keyed by (geometry family, deck base layer), never 14 hand-authored surfaces. A new layer is a row.
- `RecordBatch`, not `Table` (catalog-bound): `data` is one `arrow.RecordBatch`; the caller iterates `Table.batches` and mounts one layer per batch, so a chunked table is a `LayersList`, and an internally-created earcut pool does run once and die — hoist it with `initEarcutPool`.
- extension-type inference: an omitted geometry accessor is resolved by scanning columns for the GeoArrow extension name (`geoarrow.point`/`.multipoint`/`.linestring`/`.polygon`/`.multipolygon`); explicit `ga.data.*Data` columns override. Multi-geometry rows explode to single geometries and map back through `invertedGeomOffsets` so picking returns the original row.
- columnar accessors are zero-copy: an `arrow.Data<FixedSizeList<Uint8>>` color column or `Data<Float>` scalar binds as a GPU `BinaryAttribute` directly; the `AccessorFunction` recovery fills the reusable `AccessorContext.target` buffer to avoid per-feature allocation. `_validate` (default on) asserts chunk-length agreement before upload.

[INTEGRATION_LAW]:
- Stack with `apache-arrow` (`.api/apache-arrow.md`): the layer `data` is an `arrow.RecordBatch`; GeoArrow columns are `arrow.Data<…>` typed by `FixedSizeList`/`List`/`Struct`; `tableFromIPC` decodes the wire IPC bytes and `Table.batches` fans to per-batch layers. The picked `StructRowProxy` is Arrow's zero-copy row view — no JS object is ever built.
- Stack with `@deck.gl/core|layers|geo-layers|aggregation-layers` (`.api/deck.gl-*.md`): each GeoArrow layer subclasses a deck base layer and composes onto one `Deck`/`MapboxOverlay`; the roster spans all three deck layer packages (`layers` vector, `geo-layers` cell/trip, `aggregation-layers` heatmap).
- Stack with `maplibre-gl` (`.api/maplibre-gl.md`): the layer array rides a `MapboxOverlay` in interleaved mode over the MapLibre WebGL context, sharing the `viewer/geo/project.md` camera/viewState — deck GPU layers composite into the same frame as the basemap.
- Stack with `effect` (`libs/typescript/.api/effect.md`): the `RecordBatch` arrives as a `Schema`-decoded wire projection (WKB→GeoArrow at `wire`, never re-minted in `ui`); the `Deck` instance is an `Effect.acquireRelease` scoped resource; the `viewer/geo/layers.md` GeoLayers row folds the incoming batch `Stream` into a layer `LayersList` with `Effect.forEach`, and `@turf/turf` planar ops (`.api/turf-turf.md`) run over query-scale materialized GeoJSON — a buffer/clip on a drawn query polygon, never the bulk Arrow columns geoarrow owns — the derived overlay rendered as its own GeoJSON layer.
- Stack with the browser worker rail (`libs/typescript/.api/effect-platform-browser.md`): heavy Arrow IPC decode belongs off-main-thread on the `BrowserWorker` decode pool `ui/viewer` declares as a port; the earcut `threads` pool (`initEarcutPool`) is this package's own vendored worker path for triangulation — one shared pool, hoisted at viewer mount, never per-layer.

[LOCAL_ADMISSION]:
- Feed layers a decoded `arrow.RecordBatch` and bind accessors to columns; never materialize `RecordBatch → Array<object>` for a per-row accessor — that discards the whole point of GeoArrow.
- Create ONE `initEarcutPool` at viewer mount and pass it via `earcutWorkerPool` to every polygon layer; never let each `SolidPolygonLayer` spin its own single-use pool under the `RecordBatch` refactor.
- Let extension-type inference pick the geometry column; supply explicit `ga.data.*` columns only when a batch carries multiple geometry columns.
- Treat `_GeoArrowTextLayer` as overlay — gate it behind a capability flag, not a load-bearing row.
- `GeoArrowHeatmapLayer` (needs `@deck.gl/aggregation-layers`) and the polygon-tessellation path (needs `@math.gl/polygon`) stay UNRESOLVED until those peers are admitted to `[VIEWER_GEO]`; the vector/cell/temporal families over the admitted `core`/`layers`/`geo-layers` are the resolved surface today.

[RAIL_LAW]:
- Package: `@geoarrow/deck.gl-geoarrow`
- Owns: the GeoArrow-column deck `CompositeLayer` roster (vector/cell/temporal/aggregation families), the columnar accessor union (`Float`/`Color`/`Normal`/`Timestamp`), extension-type geometry inference, `arrow.StructRowProxy` picking, and the shared earcut `threads` pool
- Accept: one `arrow.RecordBatch` as `data`, column-or-function accessors, inferred geometry columns, a hoisted `initEarcutPool`, per-batch layer fan-out for chunked tables, `MapboxOverlay` interleave over MapLibre
- Reject: row-materialized data feeding object accessors, per-layer auto earcut pools, a re-mint of the WKB→GeoArrow decode inside `ui` (it is `wire`-owned), the overlay text layer as a stable surface, importing this outside `scope:viewer`
