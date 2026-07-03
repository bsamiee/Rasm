# [API_CATALOGUE] @deck.gl/geo-layers

`@deck.gl/geo-layers` supplies the out-of-core and geospatial layer catalogue over the `@deck.gl/core` WebGL2/luma.gl-v9 engine: `TileLayer` (viewport-driven tile streaming), `MVTLayer` (Mapbox Vector Tiles over `TileLayer`), `Tile3DLayer` (OGC 3D Tiles / `i3s` scene streaming), `TerrainLayer` (height-tile mesh), `TripsLayer` (animated trip paths), the deprecated `GreatCircleLayer`, and the cell-index family — `H3HexagonLayer`, `H3ClusterLayer`, `S2Layer`, `QuadkeyLayer`, `GeohashLayer`, `A5Layer` — each one `_GeoCellLayer` (=`PolygonLayerProps`) plus a single cell-token accessor. It exports the `Tileset2D`/`Tile2DHeader` indexing model and the `GeoBoundingBox`/`TileLoadProps` tile vocabulary consumed when subclassing the indexer. `TileLayer`/`MVTLayer`/`Tile3DLayer`/`TerrainLayer` and the cell-index layers are `CompositeLayer`s; `TripsLayer` extends `PathLayer`, `GreatCircleLayer` extends `ArcLayer`. The `render/geo.md` `GeoSeriesLayer` `$match` drives this package's `tile`/`cell-index`/`tile-3d` arms; Arrow-native feature and cell rendering is `@geoarrow/deck.gl-geoarrow`, not this package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/geo-layers`
- package: `@deck.gl/geo-layers`
- module: `@deck.gl/geo-layers`
- asset: `dist/index.d.ts`
- baseline: deck.gl v9 (luma.gl v9, WebGL2 default context; interleave requires a shared WebGL2 context)
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: out-of-core and geospatial layer classes
- rail: viewport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                                     |
| :-----: | :----------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `TileLayer`        | composite class | viewport-driven 2D tile streaming base                     |
|  [02]   | `MVTLayer`         | composite class | Mapbox Vector Tiles (`extends TileLayer<ParsedMvtTile>`)   |
|  [03]   | `Tile3DLayer`      | composite class | OGC 3D Tiles / `i3s` scene streaming (`@loaders.gl/3d-tiles`)|
|  [04]   | `TerrainLayer`     | composite class | terrain `SimpleMeshLayer` from RGB-encoded height tiles    |
|  [05]   | `TripsLayer`       | layer class     | animated time-windowed trip paths (`extends PathLayer`)    |
|  [06]   | `GreatCircleLayer` | layer class     | great-circle arcs (`extends ArcLayer`; **`@deprecated`** — use `ArcLayer` with `greatCircle: true`) |
|  [07]   | `_WMSLayer`        | composite class | WMS/WMTS raster imagery (experimental, underscored)        |

[PUBLIC_TYPE_SCOPE]: cell-index layer family — one `_GeoCellLayer` base + one cell-token accessor per row
- rail: viewport
- collapse: every row is `_GeoCellLayer` (a `PolygonLayer` producing filled/extruded polygons) discriminated only by which cell-token accessor maps a datum to a boundary; the accessor column is the entire per-layer delta, so a new cell scheme is one row, never a new base.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                                          |
| :-----: | :----------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `_GeoCellLayer`    | composite class | base cell-index polygon layer (experimental); subclass contract |
|  [02]   | `H3HexagonLayer`   | composite class | H3 cell hexagons (`getHexagon`; adds `highPrecision`/`coverage`/`centerHexagon`) |
|  [03]   | `H3ClusterLayer`   | composite class | merged H3 cell-set polygons (`getHexagons` — a set per datum)   |
|  [04]   | `S2Layer`          | composite class | S2 cell polygons (`getS2Token`)                                |
|  [05]   | `QuadkeyLayer`     | composite class | quadkey cell polygons (`getQuadkey`)                           |
|  [06]   | `GeohashLayer`     | composite class | geohash cell polygons (`getGeohash`)                           |
|  [07]   | `A5Layer`          | composite class | A5 pentagonal cell polygons (`getPentagon`)                    |

[PUBLIC_TYPE_SCOPE]: layer props and picking-info aliases
- rail: viewport

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `TileLayerProps<DataT>`              | type alias    | `CompositeLayerProps & _TileLayerProps<DataT>`     |
|  [02]   | `MVTLayerProps<FeaturePropertiesT>`  | type alias    | `_MVTLayerProps & Omit<TileLayerProps, 'data'>`    |
|  [03]   | `Tile3DLayerProps<DataT>`            | type alias    | `CompositeLayerProps & _Tile3DLayerProps<DataT>`   |
|  [04]   | `TerrainLayerProps`                  | type alias    | `_TerrainLayerProps & TileLayerProps<MeshAndTexture> & CompositeLayerProps` (rides the `TileLayer` spine) |
|  [05]   | `TripsLayerProps<DataT>`             | type alias    | `PathLayerProps<DataT> & _TripsLayerProps<DataT>`  |
|  [06]   | `GreatCircleLayerProps<DataT>`       | type alias    | `= ArcLayerProps<DataT>` (no added props)          |
|  [07]   | `H3HexagonLayerProps<DataT>`         | type alias    | `_GeoCellLayerProps<DataT>` + hexagon accessor/opts |
|  [08]   | `H3ClusterLayerProps<DataT>`         | type alias    | `_GeoCellLayerProps<DataT>` + `getHexagons`        |
|  [09]   | `S2LayerProps<DataT>`                | type alias    | `_GeoCellLayerProps<DataT>` + `getS2Token`         |
|  [10]   | `QuadkeyLayerProps<DataT>`           | type alias    | `_GeoCellLayerProps<DataT>` + `getQuadkey`         |
|  [11]   | `GeohashLayerProps<DataT>`           | type alias    | `_GeoCellLayerProps<DataT>` + `getGeohash`         |
|  [12]   | `A5LayerProps<DataT>`                | type alias    | `_GeoCellLayerProps<DataT>` + `getPentagon`        |
|  [13]   | `_GeoCellLayerProps<DataT>`          | type alias    | `= PolygonLayerProps<DataT>` (shared cell base)    |
|  [14]   | `WMSLayerProps`                      | type alias    | `CompositeLayerProps & _WMSLayerProps`             |
|  [15]   | `TileLayerPickingInfo<DataT, Sub>`   | type alias    | `Sub & { tile?, sourceTile, sourceTileSubLayer }`  |
|  [16]   | `MVTLayerPickingInfo<FeaturePropsT>` | type alias    | `TileLayerPickingInfo<ParsedMvtTile, PickingInfo<Feature<Geometry, FeaturePropsT>>>` |

[PUBLIC_TYPE_SCOPE]: tileset indexing model and tile vocabulary
- rail: viewport
- root export: rows [1]–[6] re-export from the package root under the underscore alias; `RefinementStrategy`/`ZRange`/`URLTemplate` reach consumers only as `refinementStrategy`/`zRange`/`data` prop value types — the root does not re-export them as standalone symbols.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `_Tileset2D`                          | class         | viewport→tile-index strategy; subclass and pass via `TilesetClass`          |
|  [02]   | `_Tile2DHeader<DataT>`                | class         | per-tile `content`/`bbox`/`index` + `isLoaded`/`needsReload` load state     |
|  [03]   | `_Tileset2DProps`                     | type alias    | `Tileset2D` constructor props                                               |
|  [04]   | `_TileLoadProps`                      | type alias    | `{ index, id, bbox, url?, signal?, zoom?, userData? }`                      |
|  [05]   | `GeoBoundingBox`, `NonGeoBoundingBox` | type aliases  | `{ west, north, east, south }` / `{ left, top, right, bottom }`             |
|  [06]   | `_getURLFromTemplate(template, tile)` | function      | substitutes `{x}/{y}/{z}`/`{-y}`/`{q}` into a `URLTemplate` (or array)      |
|  [07]   | `RefinementStrategy`                  | prop-value    | `'never' \| 'no-overlap' \| 'best-available' \| RefinementStrategyFunction` |
|  [08]   | `ZRange`, `URLTemplate`               | prop-values   | `[minZ, maxZ]` tuple / `string \| string[] \| null`                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TileLayer core props (the streaming/cache/refinement spine every tile layer inherits)
- rail: viewport

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------------------- |
|  [01]   | `data` prop                                                   | data prop      | `URLTemplate` — tile URL template `{x}/{y}/{z}` or URL array    |
|  [02]   | `getTileData` prop                                            | loader prop    | `(props: TileLoadProps) => Promise<DataT> \| DataT` per tile    |
|  [03]   | `renderSubLayers` prop                                        | render prop    | `(props & { id, data, _offset, tile }) => Layer \| LayersList`  |
|  [04]   | `tileSize` prop                                               | uniform prop   | tile pixel dimension, usually a power of 2                      |
|  [05]   | `minZoom`, `maxZoom` props                                    | uniform props  | data zoom range (`minZoom` default `0`, `maxZoom` `null`)       |
|  [06]   | `extent` prop                                                 | uniform prop   | `number[] \| null` — data bounding box for culling             |
|  [07]   | `TilesetClass` prop                                           | dispatch prop  | `typeof Tileset2D` — custom viewport→index scheme              |
|  [08]   | `refinementStrategy` prop                                     | dispatch prop  | `RefinementStrategy` (default `'best-available'`)               |
|  [09]   | `zRange` prop                                                 | uniform prop   | `ZRange \| null` — min/max height for 3D viewport culling       |
|  [10]   | `maxCacheSize`, `maxCacheByteSize`                            | uniform props  | tile cache count and byte ceilings (LRU eviction)              |
|  [11]   | `maxRequests`, `debounceTime`                                 | uniform props  | concurrent `getTileData` cap (default `6`) and fetch debounce   |
|  [12]   | `zoomOffset`, `visibleMinZoom`, `visibleMaxZoom`              | uniform props  | fetch-zoom offset and per-zoom visibility clamp                 |
|  [13]   | `onTileLoad`, `onTileUnload`, `onTileError`, `onViewportLoad` | callback props | `(tile: Tile2DHeader<DataT>)` hooks; `onViewportLoad(tiles[])` on full-viewport resolution |

[ENTRYPOINT_SCOPE]: MVTLayer added props and viewport-feature methods
- rail: viewport

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                                                      |
| :-----: | :----------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `data` prop                          | data prop      | `TileJson \| URLTemplate` — TileJSON descriptor or URL template             |
|  [02]   | `binary` prop                        | toggle prop    | parse tiles as `BinaryFeatureCollection` (default `true`)                   |
|  [03]   | `loaders` prop                       | loader prop    | `Loader[]` — defaults to `MVTWorkerLoader` from `@loaders.gl/mvt`           |
|  [04]   | `uniqueIdProperty` prop              | accessor prop  | feature property keying highlight across tile boundaries                    |
|  [05]   | `highlightedFeatureId` prop          | accessor prop  | `string \| number \| null` — feature to highlight                          |
|  [06]   | `onDataLoad` prop                    | callback prop  | `(tilejson: TileJson \| null) => void` on TileJSON fetch                    |
|  [07]   | GeoJSON accessor props               | accessor props | the full `GeoJsonLayerProps` set minus `data` (fill/stroke/extrusion/point) |
|  [08]   | `getRenderedFeatures(max?)`          | method         | `Feature[]` — deduplicated features drawn in the current viewport          |
|  [09]   | `getSubLayerPropsByTile(tile)`       | method         | `Record<string, any>` — per-tile sub-layer prop override hook              |

[ENTRYPOINT_SCOPE]: cell-index parameterized surface — shared base props + one token accessor per layer
- rail: viewport
- law: all six cell layers share the `_GeoCellLayer`=`PolygonLayerProps` fill/stroke/extrusion base; the only per-layer prop is the cell-token accessor (and `H3HexagonLayer`'s three tessellation extras). Read the accessor row for the layer, then compose the shared base row for styling.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                                                      |
| :-----: | :------------------------------------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `_GeoCellLayer` base props                   | shared props     | `getFillColor`/`getLineColor`/`getLineWidth`/`getElevation` + `filled`/`stroked`/`extruded`/`wireframe`/`elevationScale`/`material` (from `PolygonLayerProps`) |
|  [02]   | `H3HexagonLayer.getHexagon`                  | cell-token prop  | `AccessorFunction<DataT, string>` — one H3 index → hexagon                 |
|  [03]   | `H3HexagonLayer` tessellation opts           | uniform props    | `highPrecision?: boolean \| 'auto'`, `coverage?: number`, `centerHexagon?: H3Index \| null` |
|  [04]   | `H3ClusterLayer.getHexagons`                 | cell-token prop  | `AccessorFunction<DataT, H3IndexInput[]>` — a set merged into one polygon per datum |
|  [05]   | `S2Layer.getS2Token`                         | cell-token prop  | `AccessorFunction<DataT, string>` — S2 token/id                            |
|  [06]   | `QuadkeyLayer.getQuadkey`                    | cell-token prop  | `AccessorFunction<DataT, string>` — quadkey string                        |
|  [07]   | `GeohashLayer.getGeohash`                    | cell-token prop  | `AccessorFunction<DataT, string>` — geohash string                        |
|  [08]   | `A5Layer.getPentagon`                        | cell-token prop  | `AccessorFunction<DataT, string \| bigint>` — A5 cell id                   |

[ENTRYPOINT_SCOPE]: Tile3DLayer / TerrainLayer / TripsLayer distinct props
- rail: viewport

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                                                      |
| :-----: | :------------------------------------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Tile3DLayer.data`                           | data prop        | `string` — 3D-Tiles tileset URL (`tileset.json`) or `i3s` scene URL        |
|  [02]   | `Tile3DLayer` point props                    | accessor/uniform | `getPointColor?: Accessor<DataT, Color>`, `pointSize?: number` (point-cloud tiles) |
|  [03]   | `Tile3DLayer` lifecycle callbacks            | callback props   | `onTilesetLoad(tileset: Tileset3D)`, `onTileLoad`/`onTileUnload`/`onTileError(tile: Tile3D, …)` |
|  [04]   | `Tile3DLayer.loadOptions` / `_subLayerProps` | loader/override  | `@loaders.gl` load options (Cesium ion token etc.) and per-format sub-layer prop override |
|  [05]   | `TerrainLayer` height props                  | data props       | `elevationData: URLTemplate`, `texture?: URLTemplate`, `elevationDecoder?: ElevationDecoder` (`{ rScaler, gScaler, bScaler, offset }`) |
|  [06]   | `TerrainLayer` mesh props                    | uniform props    | `meshMaxError?: number`, `bounds?: Bounds \| null`, `material?`, `color?`, `wireframe?` |
|  [07]   | `TripsLayer` animation props                 | accessor/uniform | `getTimestamps: AccessorFunction<DataT, NumericArray>`, `currentTime?: number`, `trailLength?: number`, `fadeTrail?: boolean` |

[ENTRYPOINT_SCOPE]: instance operations and Tileset2D override surface
- rail: viewport

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                                                    |
| :-----: | :------------------------------------ | :------------- | :-------------------------------------------------------- |
|  [01]   | `TileLayer.isLoaded` getter           | accessor       | `boolean` — all viewport tiles resolved                  |
|  [02]   | `Tileset2D.getTileIndices({ viewport, minZoom, maxZoom, zRange, modelMatrix })` | override | tiles a viewport needs; override for a non-standard scheme |
|  [03]   | `Tileset2D.getTileId/getTileZoom/getTileMetadata/getParentIndex(index)` | override | index→id/zoom/metadata/parent, the LOD refinement contract |
|  [04]   | `Tileset2D.isTileVisible(tile, cullRect?)` / `update(viewport, opts)` | override | per-tile cull and per-frame update pass                  |
|  [05]   | `Tile2DHeader.isLoaded` / `needsReload` getters | accessor | per-tile load and reload state                           |

## [04]-[IMPLEMENTATION_LAW]

[GEO_LAYERS_TOPOLOGY]:
- `TileLayer` is a `CompositeLayer<DataT>` fetching only viewport-visible tiles; supply a `data` `URLTemplate` (or a `getTileData` prop) and a `renderSubLayers` prop turning each loaded tile into one or more `Layer` instances.
- `renderSubLayers` receives the full `TileLayerProps<DataT>` plus `{ id, data, _offset, tile: Tile2DHeader<DataT> }`; the injected `id` keeps sub-layers reconciled per tile, and `_offset` handles the 180° meridian wrap.
- `getTileData` receives a `TileLoadProps` (`{ index: { x, y, z }, id, bbox, url?, signal?, zoom? }`) and returns `Promise<DataT> | DataT`; `signal` is the `AbortSignal` fired when a tile leaves the viewport — thread it into the fetch so off-screen requests cancel.
- `MVTLayer` extends `TileLayer<ParsedMvtTile>` where `ParsedMvtTile = Feature[] | BinaryFeatureCollection`; with `binary: true` (default) it yields a `BinaryFeatureCollection` straight to `GeoJsonLayer.data`, bypassing per-feature JavaScript iteration. `getRenderedFeatures(max?)` returns the deduplicated `Feature[]` currently drawn — viewport feature inspection without re-fetching tiles.
- The cell-index layers collapse to `_GeoCellLayer` (a `PolygonLayer`): each layer maps its cell token to a boundary polygon via one accessor (`getHexagon`/`getHexagons`/`getS2Token`/`getQuadkey`/`getGeohash`/`getPentagon`), then inherits every fill/stroke/extrusion prop from `PolygonLayerProps`. `H3ClusterLayer` differs only in taking a *set* (`H3IndexInput[]`) and merging it into one polygon; `H3HexagonLayer` adds `highPrecision`/`coverage`/`centerHexagon` for GPU-instanced vs. per-hexagon tessellation.
- `Tile3DLayer` streams OGC 3D Tiles / `i3s` via `@loaders.gl/3d-tiles`, spawning `ScenegraphLayer`/`PointCloudLayer`/`SimpleMeshLayer` sub-layers by tile content type; `data` is the tileset URL, and Cesium ion access rides `loadOptions`.
- `TerrainLayer` reconstructs a mesh from RGB-encoded height tiles through `elevationDecoder`, draping `texture` over a `SimpleMeshLayer`; a single-tile `elevationData` URL is non-tiled, a `{z}/{x}/{y}` template is tiled through the same `Tileset2D` spine.
- `Tileset2D` owns the viewport→tile-index strategy; subclass it, override `getTileIndices`/`getParentIndex`/`getTileId`, and pass via `TilesetClass` for a non-Web-Mercator or non-quadtree scheme. `Tile2DHeader<DataT>` carries each tile's `content`, `bbox`, and `isLoaded`/`needsReload` state.
- `GeoBoundingBox` (`{ west, north, east, south }`) is the geographic tile bbox; `NonGeoBoundingBox` (`{ left, top, right, bottom }`) is the cartesian form. Discriminate on `west`/`north` key presence — the root does not re-export the internal `isGeoBoundingBox` guard.
- `refinementStrategy` governs load-time visibility: `'best-available'` shows parent/child tiles while loading, `'no-overlap'` hides until exact tiles land, `'never'` disables refinement.

[STACKING_LAW]:
- `TileLayer`/`MVTLayer`/`Tile3DLayer`/`TerrainLayer` and the `_GeoCellLayer` cell family subclass `@deck.gl/core`'s `CompositeLayer`; `TripsLayer`/`GreatCircleLayer` extend `@deck.gl/layers`' `PathLayer`/`ArcLayer` (themselves `@deck.gl/core` `Layer` subclasses, `deck.gl-core.md`) — drawn primitives, never a renderer. Each `renderSubLayers`/`renderLayers(): Layer | null | LayersList` emits a `LayersList` a `Deck` rasterizes under a `View`/`Viewport` from core; the streaming spine is a core-owned camera concern — `Tileset2D.getTileIndices({ viewport })` culls against core's `Viewport`, so a pan/zoom is a `Deck` event, not a package one.
- This package is the `tile`/`cell-index`/`tile-3d` arm of the `render/geo.md` `GeoSeriesLayer` `Data.TaggedEnum` `$match` (`effect` universal tier: `Data.taggedEnum`, `Match.value(featureKind).pipe(Match.when(...))`). Layer instances are constructed inside the match arms; the `overlayMode` discriminant routes them into `@deck.gl/mapbox` `MapboxOverlay` (`interleaved`) or a standalone canvas.
- Out-of-core data enters through the `interchange` `GeometryRail` on `decode-rail#TS_PROJECTION`: the `tile` arm binds an `MVTLayer`/`TileLayer` URL, never an in-memory `GeoJsonLayer` for a bounded collection. The maplibre `Map` substrate is held as an `Effect.acquireRelease` resource under the `platform` `BrowserPlatform`, and view state reads/writes through the `binding/atom.md` `AtomBinding`.
- The cell-index law is a two-source split: `h3`/`s2`/`geohash`/`a5`/`trips` bind the **Arrow-native** `@geoarrow/deck.gl-geoarrow` layers reading the same `RecordBatch` index column the `geoarrow` arm projects (no second decode); `h3-cluster` (set-merge) and `quadkey` — the two schemes with no Arrow-native variant — bind *this* package's `H3ClusterLayer`/`QuadkeyLayer` over the bounded JS cell-id projection (`{ token, weight }[]`). Feeding an Arrow `RecordBatch` into a `_GeoCellLayer` here is the named defect (it forces per-feature materialization); use the GeoArrow cell layer for Arrow columns.
- `Tile3DLayer` streams OGC-3D-Tiles/`i3s` city context under the existing `MapboxOverlay` interleave — a `tile-3d` arm, never a parallel `Deck`. `TilesetClass` is the extension point for a `platform`-supplied non-Mercator indexer.

[LOCAL_ADMISSION]:
- Drive `TileLayer`/`MVTLayer` from the `GeoSeriesLayer` `tile` arm for out-of-core feature sets; reserve the in-memory `GeoJsonLayer` (`@deck.gl/layers`) for bounded collections.
- Thread `TileLoadProps.signal` into every `getTileData` fetch so off-viewport requests abort cleanly; memoize `renderSubLayers`/`getTileData` references to avoid full tileset rebuilds on re-render.
- Prefer `binary: true` on `MVTLayer` for high feature counts; disable only when a sub-layer accessor needs JavaScript `Feature` objects.
- For the two JS-cell arms, bind the layer's native cell-id data prop (`getQuadkey`/`getHexagons`) against `{ token, weight }[]`; style through the shared `_GeoCellLayer` fill/stroke/extrusion props, not a per-layer restyle.
- Subclass `Tileset2D` (override `getTileIndices`/`getParentIndex`) and pass via `TilesetClass` for a non-standard indexing scheme rather than pre-computing tile lists outside the layer.

[RAIL_LAW]:
- Package: `@deck.gl/geo-layers`
- Owns: out-of-core tile streaming (`TileLayer`/`MVTLayer`/`Tile3DLayer`/`TerrainLayer`), the `_GeoCellLayer` cell-index family (H3/S2/quadkey/geohash/A5), `TripsLayer`, the deprecated `GreatCircleLayer`, and the `Tileset2D` indexing model
- Accept: `TileLayer` as the base for any viewport-driven tile source; `MVTLayer` for Mapbox Vector Tiles; `Tile3DLayer` for 3D-Tiles/`i3s`; the JS-cell `H3ClusterLayer`/`QuadkeyLayer` for the two schemes with no Arrow-native layer; `Tileset2D` subclassing for custom indexing
- Reject: loading an entire feature dataset into one `GeoJsonLayer` when tile streaming applies; hand-rolling tile fetch/cache/viewport-cull loops outside `TileLayer`; feeding an Arrow `RecordBatch` into a `_GeoCellLayer` when the `@geoarrow/deck.gl-geoarrow` cell layer renders the column directly; `GreatCircleLayer` (use `ArcLayer` with `greatCircle: true`)
