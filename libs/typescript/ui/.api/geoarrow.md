# [API_CATALOGUE] @geoarrow/deck.gl-geoarrow

`@geoarrow/deck.gl-geoarrow` supplies deck.gl `CompositeLayer` wrappers that bind directly to GeoArrow-encoded Apache Arrow `RecordBatch` columns, rendering millions of features without per-feature JavaScript iteration: `GeoArrowScatterplotLayer`, `GeoArrowPathLayer`, `GeoArrowSolidPolygonLayer`, `GeoArrowPolygonLayer`, plus the arc, column, point-cloud, trips, heatmap, and cell-index (`H3HexagonLayer`, `S2Layer`, `A5Layer`, `GeohashLayer`) variants. Every layer infers its geometry column from the GeoArrow extension type (`geoarrow.point`, `geoarrow.linestring`, `geoarrow.polygon`, and their multi forms) or takes an explicit `ga.data.*` accessor, and exposes the Arrow-aware `ColorAccessor`/`FloatAccessor`/`NormalAccessor`/`TimestampAccessor` vocabulary. All layers extend `@deck.gl/core`'s `CompositeLayer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@geoarrow/deck.gl-geoarrow`
- package: `@geoarrow/deck.gl-geoarrow`
- module: `@geoarrow/deck.gl-geoarrow`
- asset: `dist/index.d.ts`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: GeoArrow layer classes
- rail: viewport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [RAIL]                                                     |
| :-----: | :-------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `GeoArrowScatterplotLayer`  | composite class | points from `geoarrow.point`/`geoarrow.multipoint`         |
|  [02]   | `GeoArrowPathLayer`         | composite class | polylines from `geoarrow.linestring` columns               |
|  [03]   | `GeoArrowSolidPolygonLayer` | composite class | filled/extruded polygons via worker-pool earcut            |
|  [04]   | `GeoArrowPolygonLayer`      | composite class | filled + stroked polygons (wraps solid-polygon + path)     |
|  [05]   | `GeoArrowArcLayer`          | composite class | arcs between source/target point columns                   |
|  [06]   | `GeoArrowColumnLayer`       | composite class | 3D columns at point positions                              |
|  [07]   | `GeoArrowPointCloudLayer`   | composite class | 3D point cloud from point columns                          |
|  [08]   | `GeoArrowTripsLayer`        | composite class | animated time-windowed trip paths                          |
|  [09]   | `GeoArrowHeatmapLayer`      | composite class | density heatmap from point columns                         |
|  [10]   | `GeoArrowH3HexagonLayer`    | composite class | H3 cell hexagons from an index column                      |
|  [11]   | `GeoArrowS2Layer`           | composite class | S2 cell polygons from an index column                      |
|  [12]   | `GeoArrowA5Layer`           | composite class | A5 pentagonal cell polygons from an index column           |
|  [13]   | `GeoArrowGeohashLayer`      | composite class | geohash cell polygons from an index column                 |
|  [14]   | `_GeoArrowTextLayer`        | composite class | text labels at point positions (experimental, underscored) |

[PUBLIC_TYPE_SCOPE]: GeoArrow layer props interfaces
- rail: viewport

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `GeoArrowScatterplotLayerProps`  | type alias    | `Omit<ScatterplotLayerProps, …> & CompositeLayerProps`  |
|  [02]   | `GeoArrowPathLayerProps`         | type alias    | `Omit<PathLayerProps, …> & CompositeLayerProps`         |
|  [03]   | `GeoArrowSolidPolygonLayerProps` | type alias    | `Omit<SolidPolygonLayerProps, …> & CompositeLayerProps` |
|  [04]   | `GeoArrowPolygonLayerProps`      | type alias    | `Omit<PolygonLayerProps, …> & CompositeLayerProps`      |
|  [05]   | `GeoArrowArcLayerProps`          | type alias    | `Omit<ArcLayerProps, …> & CompositeLayerProps`          |
|  [06]   | `GeoArrowColumnLayerProps`       | type alias    | `Omit<ColumnLayerProps, …> & CompositeLayerProps`       |
|  [07]   | `GeoArrowPointCloudLayerProps`   | type alias    | `Omit<PointCloudLayerProps, …> & CompositeLayerProps`   |
|  [08]   | `GeoArrowTripsLayerProps`        | type alias    | extends the deck.gl trips props with Arrow accessors    |
|  [09]   | `GeoArrowHeatmapLayerProps`      | type alias    | extends the deck.gl heatmap props with Arrow accessors  |
|  [10]   | `GeoArrowH3HexagonLayerProps`    | type alias    | cell-index hexagon props                                |
|  [11]   | `GeoArrowS2LayerProps`           | type alias    | cell-index S2 props                                     |
|  [12]   | `GeoArrowA5LayerProps`           | type alias    | cell-index A5 props                                     |
|  [13]   | `GeoArrowGeohashLayerProps`      | type alias    | cell-index geohash props                                |

[PUBLIC_TYPE_SCOPE]: GeoArrow accessor and picking vocabulary
- rail: viewport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                                  |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `FloatAccessor`             | type alias    | `Data<arrow.Float> \| Accessor<RecordBatch, number>`                    |
|  [02]   | `ColorAccessor`             | type alias    | `Data<FixedSizeList<Uint8>> \| Accessor<RecordBatch, Color \| Color[]>` |
|  [03]   | `NormalAccessor`            | type alias    | `Data<FixedSizeList<Float32>> \| Accessor<RecordBatch, …>`              |
|  [04]   | `TimestampAccessor`         | type alias    | `arrow.Data<arrow.List<arrow.Float>>`                                   |
|  [05]   | `Accessor<In, Out>`         | type alias    | `Out \| AccessorFunction<In, Out>`                                      |
|  [06]   | `AccessorFunction<In, Out>` | type alias    | `(objectInfo: AccessorContext<In>) => Out`                              |
|  [07]   | `GeoArrowPickingInfo`       | type alias    | `PickingInfo & { object?: arrow.StructRowProxy }`                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GeoArrowScatterplotLayer accessors
- rail: viewport

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RAIL]                                                              |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `data` prop             | data prop      | `arrow.RecordBatch` — one GeoArrow batch                            |
|  [02]   | `getPosition` accessor  | geometry prop  | `ga.data.PointData \| ga.data.MultiPointData` (inferred if omitted) |
|  [03]   | `getRadius` accessor    | accessor prop  | `FloatAccessor` (default `1`)                                       |
|  [04]   | `getFillColor` accessor | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)                          |
|  [05]   | `getLineColor` accessor | accessor prop  | `ColorAccessor` — stroke color (default `[0, 0, 0, 255]`)           |
|  [06]   | `getLineWidth` accessor | accessor prop  | `FloatAccessor` — stroke width (default `1`)                        |
|  [07]   | `_validate` prop        | toggle prop    | validate array chunk lengths (default `true`)                       |

[ENTRYPOINT_SCOPE]: GeoArrowPathLayer accessors
- rail: viewport

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `data` prop         | data prop      | `arrow.RecordBatch`                                     |
|  [02]   | `getPath` accessor  | geometry prop  | `ga.data.LineStringData \| ga.data.MultiLineStringData` |
|  [03]   | `getColor` accessor | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)              |
|  [04]   | `getWidth` accessor | accessor prop  | `FloatAccessor` (default `1`)                           |
|  [05]   | `_validate` prop    | toggle prop    | validate array chunk lengths (default `true`)           |

[ENTRYPOINT_SCOPE]: GeoArrowSolidPolygonLayer accessors and earcut pool
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :---------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `data` prop                   | data prop      | `arrow.RecordBatch`                                                   |
|  [02]   | `getPolygon` accessor         | geometry prop  | `ga.data.PolygonData \| ga.data.MultiPolygonData`                     |
|  [03]   | `getElevation` accessor       | accessor prop  | `FloatAccessor` — extrusion height (default `1000`)                   |
|  [04]   | `getFillColor` accessor       | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)                            |
|  [05]   | `getLineColor` accessor       | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)                            |
|  [06]   | `earcutWorkerPool` prop       | resource prop  | `Pool<FunctionThread> \| null` — shared triangulation pool            |
|  [07]   | `earcutWorkerUrl` prop        | resource prop  | `string \| URL \| null` — earcut worker source (jsDelivr CDN default) |
|  [08]   | `earcutWorkerPoolSize` prop   | uniform prop   | worker count for the earcut pool                                      |
|  [09]   | `metrics` prop                | toggle prop    | log triangulation timing via `console.time`                           |
|  [10]   | `initEarcutPool(url?, opts?)` | factory        | `Promise<Pool<FunctionThread> \| null>` — module-level pool builder   |

[ENTRYPOINT_SCOPE]: GeoArrowPolygonLayer accessors
- rail: viewport

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RAIL]                                                        |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `data` prop             | data prop      | `arrow.RecordBatch`                                           |
|  [02]   | `getPolygon` accessor   | geometry prop  | `ga.data.PolygonData \| ga.data.MultiPolygonData`             |
|  [03]   | `getFillColor` accessor | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)                    |
|  [04]   | `getLineColor` accessor | accessor prop  | `ColorAccessor` (default `[0, 0, 0, 255]`)                    |
|  [05]   | `getLineWidth` accessor | accessor prop  | `FloatAccessor` (default `1`)                                 |
|  [06]   | `getElevation` accessor | accessor prop  | `FloatAccessor` — used when `extruded: true` (default `1000`) |
|  [07]   | earcut worker props     | resource props | `earcutWorkerPool`/`earcutWorkerUrl`/`earcutWorkerPoolSize`   |

## [04]-[IMPLEMENTATION_LAW]

[GEOARROW_TOPOLOGY]:
- Each layer takes `data: arrow.RecordBatch` (one Arrow batch, not a `Table` as of v0.4) and infers its geometry column from the GeoArrow extension type metadata; an explicit `ga.data.*` accessor (`getPosition`, `getPath`, `getPolygon`) overrides inference.
- Accessors are Arrow-native: `ColorAccessor` accepts a packed `Data<FixedSizeList<Uint8>>` column, `FloatAccessor` accepts a `Data<arrow.Float>` column, and both fall back to a JavaScript `Accessor<RecordBatch, …>` function over `AccessorContext`.
- `GeoArrowScatterplotLayer` renders `geoarrow.point` columns through `_renderPointLayer` and `geoarrow.multipoint` through `_renderMultiPointLayer`, each producing a deck.gl `ScatterplotLayer` sub-layer.
- `GeoArrowSolidPolygonLayer` triangulates polygons with earcut on a `threads` worker pool; pass a shared `earcutWorkerPool` (built via `initEarcutPool`) when rendering many polygon layers, since each batch consumes a pool once.
- `GeoArrowPolygonLayer` is a `CompositeLayer` that wraps `GeoArrowSolidPolygonLayer` (fill/extrusion) and `GeoArrowPathLayer` (stroke); `getPolygonExterior`/`getMultiPolygonExterior` derive the stroke `MultiLineString` from the polygon column without changing row count.
- `getPickingInfo` returns a `GeoArrowPickingInfo` whose `object` is an `arrow.StructRowProxy` into the source batch row, not a materialized GeoJSON `Feature`.
- `_validate` (default `true`) checks chunk-length consistency across the GeoArrow arrays; disable only for trusted pre-validated batches in hot paths.

[LOCAL_ADMISSION]:
- Drive the GeoArrow layers from the `GeoSeriesLayer` `overlay` case when `source` is `geoarrow`, feeding the `interchange` `GeometryRail` Arrow projection directly without a GeoJSON round-trip.
- Build one shared earcut `Pool` via `initEarcutPool` at the surface boundary and pass it into every `GeoArrowSolidPolygonLayer`/`GeoArrowPolygonLayer` instance.
- Let geometry-column inference run by extension type; supply an explicit `ga.data.*` accessor only when a batch carries multiple geometry columns.
- Read picked rows through the `arrow.StructRowProxy` on `GeoArrowPickingInfo.object` rather than converting batches to JavaScript objects.

[RAIL_LAW]:
- Package: `@geoarrow/deck.gl-geoarrow`
- Owns: GeoArrow `RecordBatch`-bound deck.gl layers (scatterplot, path, solid-polygon, polygon, arc, column, point-cloud, trips, heatmap, H3/S2/A5/geohash) and the Arrow-native accessor vocabulary
- Accept: these layers for out-of-core feature rendering over Arrow columns; the worker-pool earcut path for large polygon sets; extension-type geometry inference
- Reject: converting Arrow batches to GeoJSON `Feature[]` for `@deck.gl/layers` when a GeoArrow layer renders the column directly; the deprecated `@geoarrow/deck.gl-layers` package name
