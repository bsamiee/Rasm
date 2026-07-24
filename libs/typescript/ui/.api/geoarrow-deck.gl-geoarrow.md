# [TS_UI_API_GEOARROW_DECK_GL_GEOARROW]

`@geoarrow/deck.gl-geoarrow` owns one parameterized deck.gl `CompositeLayer` binding GPU attributes straight from GeoArrow-encoded Arrow columns — an `arrow.Data<…>` column or a function over the `arrow.RecordBatch`, never per-row JS objects — so a decoded WKB→GeoArrow batch renders with zero row materialization.

Extension-type metadata infers the geometry column and every layer takes a single `RecordBatch` as `data`; a new geometry family lands as a roster row, never a new mechanism.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@geoarrow/deck.gl-geoarrow`
- package: `@geoarrow/deck.gl-geoarrow` (MIT)
- rail: viewer/geo — the GeoArrow-column deck layer plane
- deps: `@geoarrow/geoarrow-js` (`ga.data.*` extension-typed columns + `worker/earcut`), `threads` (earcut `Pool<FunctionThread>`)
- runtime: `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core
- entry: single `.` barrel (`dist/index.d.ts`) — the `GeoArrow*Layer` classes + matching `*Props`, `initEarcutPool`, and the accessor/picking type family; `GeoArrowTextLayer` is `_`-prefixed (`_GeoArrowTextLayer`), the overlay/unstable row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared accessor + picking type family (bound once, read by every layer)
- Each accessor is a discriminated union — an Arrow column (`arrow.Data<…>`, GPU-bound directly, zero JS) OR a per-feature `AccessorFunction` over the `RecordBatch` — the collapse point where layers differ only in which columns they bind, never in accessor mechanism.

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

[PUBLIC_TYPE_SCOPE]: the layer-props family — one pattern across every instance
- Each `GeoArrow<Name>LayerProps` re-binds the deck base layer's props to columns: `Omit`s the base `data` + geometry accessors, swaps `data: arrow.RecordBatch`, and intersects `get…?: ga.data.<Geom>Data \| <Accessor>` with `CompositeLayerProps`; `_validate` (default `true`) gates chunk-length agreement.

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
- Construction is uniform — `new GeoArrow<Name>Layer({ id, data: recordBatch, get…: column | fn })` — the geometry column inferred from GeoArrow extension metadata when the accessor is omitted, a picked feature returning as a `GeoArrowPickingInfo` carrying an `arrow.StructRowProxy`.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                        |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `new GeoArrow<Name>Layer<ExtraProps>(props)`        | construct      | `viewer/geo/layers.md` — one layer array per `RecordBatch` |
|  [02]   | `Layer.renderLayers(): Layer \| LayersList \| null` | composite      | expands to per-geometry sublayers; multi-geometry splits   |
|  [03]   | `static defaultProps: DefaultProps<…Props>`         | layer identity | deck update-diff + prop merge                              |
|  [04]   | `static layerName: string`                          | layer identity | the class name; overridable per instance                   |
|  [05]   | `Layer.getPickingInfo(params): GeoArrowPickingInfo` | pick           | screen pixel → `arrow.StructRowProxy` feature              |

[ENTRYPOINT_SCOPE]: the shared earcut worker pool
- `initEarcutPool` builds one shared `threads` `Pool<FunctionThread>`, passed to every polygon layer via `earcutWorkerPool` and hoisted at viewer mount.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `initEarcutPool(workerUrl?, optionsOrSize?)` | worker pool    | share ONE `threads` pool across polygon layers via `earcutWorkerPool` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `data` is one `arrow.RecordBatch`, never a `Table`: the caller iterates `Table.batches` and mounts one layer per batch, so a chunked table becomes a `LayersList` and an internally-created earcut pool runs once and dies — hoist it with `initEarcutPool`.
- column scanning resolves an omitted geometry accessor by matching the GeoArrow extension name (`geoarrow.point`/`.multipoint`/`.linestring`/`.polygon`/`.multipolygon`); explicit `ga.data.*Data` columns override, and multi-geometry rows explode to single geometries mapping back through `invertedGeomOffsets` so picking returns the original row.
- columnar accessors bind zero-copy: an `arrow.Data<FixedSizeList<Uint8>>` color column or `Data<Float>` scalar becomes a GPU `BinaryAttribute` directly, the `AccessorFunction` recovery fills the reusable `AccessorContext.target` buffer, and `_validate` asserts chunk-length agreement before upload.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): layer `data` is an `arrow.RecordBatch`; GeoArrow columns are `arrow.Data<…>` typed by `FixedSizeList`/`List`/`Struct`; `tableFromIPC` decodes the wire IPC bytes and `Table.batches` fans to per-batch layers; the picked `StructRowProxy` is Arrow's zero-copy row view.
- `@deck.gl/core|layers|geo-layers|aggregation-layers`(`.api/deck.gl-*.md`): each layer subclasses a deck base layer and composes onto one `Deck`/`MapboxOverlay`; the roster spans `layers` (vector), `geo-layers` (cell/trip), and `aggregation-layers` (heatmap).
- `maplibre-gl`(`.api/maplibre-gl.md`): the layer array rides a `MapboxOverlay` in interleaved mode over the MapLibre WebGL context, sharing the `viewer/geo/project.md` camera/viewState so deck GPU layers composite into the same frame as the basemap.
- `effect`(`libs/typescript/.api/effect.md`): the `RecordBatch` arrives as a `Schema`-decoded wire projection (WKB→GeoArrow at `wire`, never re-minted in `ui`); the `Deck` is an `Effect.acquireRelease` scoped resource; `viewer/geo/layers.md` folds the batch `Stream` into a `LayersList` with `Effect.forEach`, and `@turf/turf` planar ops (`.api/turf-turf.md`) run over query-scale materialized GeoJSON, never the bulk Arrow columns.
- `effect-platform-browser`(`libs/typescript/.api/effect-platform-browser.md`): heavy Arrow IPC decode rides the `BrowserWorker` decode pool `ui/viewer` declares as a port; the earcut `threads` pool (`initEarcutPool`) is this package's own vendored triangulation worker — one shared pool, hoisted at mount, never per-layer.

[LOCAL_ADMISSION]:
- Gate `_GeoArrowTextLayer` behind a capability flag as an overlay, never a load-bearing row.
- `GeoArrowHeatmapLayer` (needs `@deck.gl/aggregation-layers`) and the polygon-tessellation path (needs `@math.gl/polygon`) stay UNRESOLVED until those peers admit to `[VIEWER_GEO]`; the vector/cell/temporal families over the admitted `core`/`layers`/`geo-layers` are the resolved surface.

[RAIL_LAW]:
- Package: `@geoarrow/deck.gl-geoarrow`
- Owns: the GeoArrow-column deck `CompositeLayer` roster (vector/cell/temporal/aggregation families), the columnar accessor union (`Float`/`Color`/`Normal`/`Timestamp`), extension-type geometry inference, `arrow.StructRowProxy` picking, and the shared earcut `threads` pool
- Accept: one `arrow.RecordBatch` as `data`, column-or-function accessors, inferred geometry columns, a hoisted `initEarcutPool`, per-batch layer fan-out for chunked tables, `MapboxOverlay` interleave over MapLibre
- Reject: row-materialized data feeding object accessors, per-layer auto earcut pools, a re-mint of the WKB→GeoArrow decode inside `ui` (it is `wire`-owned), the overlay text layer as a stable surface, importing this outside `scope:viewer`
