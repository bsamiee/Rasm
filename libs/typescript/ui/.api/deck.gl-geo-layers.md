# [API_CATALOGUE] @deck.gl/geo-layers

`@deck.gl/geo-layers` supplies the geospatial and out-of-core layer catalogue: `TileLayer` (viewport-driven tile streaming), `MVTLayer` (Mapbox Vector Tiles over `TileLayer`), the cell-index layers (`H3HexagonLayer`, `H3ClusterLayer`, `S2Layer`, `QuadkeyLayer`, `GeohashLayer`, `A5Layer`), `GreatCircleLayer`, `TripsLayer`, `Tile3DLayer`, `TerrainLayer`, plus the `Tileset2D` indexing model and the `GeoBoundingBox`/`URLTemplate`/`TileLoadProps` tile vocabulary. `TileLayer` and `MVTLayer` extend `@deck.gl/core`'s `CompositeLayer`; the cell-index layers extend the private `_GeoCellLayer`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/geo-layers`
- package: `@deck.gl/geo-layers`
- module: `@deck.gl/geo-layers`
- asset: `dist/index.d.ts`
- rail: viewport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: out-of-core and geospatial layer classes
- rail: viewport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                         |
| :-----: | :----------------- | :-------------- | :--------------------------------------------- |
|   [1]   | `TileLayer`        | composite class | viewport-driven 2D tile streaming              |
|   [2]   | `MVTLayer`         | composite class | Mapbox Vector Tiles (extends `TileLayer`)      |
|   [3]   | `Tile3DLayer`      | composite class | OGC 3D Tiles / `i3s` scene streaming           |
|   [4]   | `TerrainLayer`     | composite class | terrain mesh from height tiles                 |
|   [5]   | `TripsLayer`       | layer class     | animated time-windowed trip paths              |
|   [6]   | `GreatCircleLayer` | layer class     | great-circle arcs between source/target        |
|   [7]   | `H3HexagonLayer`   | composite class | H3 cell hexagons                               |
|   [8]   | `H3ClusterLayer`   | composite class | merged H3 cell-set polygons                    |
|   [9]   | `S2Layer`          | composite class | S2 cell polygons                               |
|  [10]   | `QuadkeyLayer`     | composite class | quadkey cell polygons                          |
|  [11]   | `GeohashLayer`     | composite class | geohash cell polygons                          |
|  [12]   | `A5Layer`          | composite class | A5 pentagonal cell polygons                    |
|  [13]   | `_WMSLayer`        | composite class | WMS raster imagery (experimental, underscored) |
|  [14]   | `_GeoCellLayer`    | composite class | base cell-index polygon layer (experimental)   |

[PUBLIC_TYPE_SCOPE]: layer props and picking-info interfaces
- rail: viewport

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------- |
|   [1]   | `TileLayerProps<DataT>`              | type alias    | `CompositeLayerProps & _TileLayerProps<DataT>`     |
|   [2]   | `MVTLayerProps<FeaturePropertiesT>`  | type alias    | `_MVTLayerProps & Omit<TileLayerProps, 'data'>`    |
|   [3]   | `Tile3DLayerProps`                   | type alias    | extends `CompositeLayerProps`                      |
|   [4]   | `TerrainLayerProps`                  | type alias    | extends `CompositeLayerProps`                      |
|   [5]   | `TripsLayerProps`                    | type alias    | extends `PathLayerProps`                           |
|   [6]   | `GreatCircleLayerProps`              | type alias    | extends `ArcLayerProps`                            |
|   [7]   | `H3HexagonLayerProps`                | type alias    | extends `CompositeLayerProps`                      |
|   [8]   | `H3ClusterLayerProps`                | type alias    | extends `CompositeLayerProps`                      |
|   [9]   | `S2LayerProps`                       | type alias    | extends `CompositeLayerProps`                      |
|  [10]   | `QuadkeyLayerProps`                  | type alias    | extends `CompositeLayerProps`                      |
|  [11]   | `GeohashLayerProps`                  | type alias    | extends `CompositeLayerProps`                      |
|  [12]   | `A5LayerProps`                       | type alias    | extends `CompositeLayerProps`                      |
|  [13]   | `WMSLayerProps`                      | type alias    | `CompositeLayerProps & _WMSLayerProps`             |
|  [14]   | `_GeoCellLayerProps<DataT>`          | type alias    | `PolygonLayerProps<DataT>` (base cell-index props) |
|  [15]   | `TileLayerPickingInfo<DataT, Sub>`   | type alias    | `Sub & { tile?, sourceTile, sourceTileSubLayer }`  |
|  [16]   | `MVTLayerPickingInfo<FeaturePropsT>` | type alias    | `TileLayerPickingInfo<ParsedMvtTile, PickingInfo>` |

[PUBLIC_TYPE_SCOPE]: tileset indexing model and tile vocabulary
- rail: viewport
- root export: rows [1]–[5] re-export from the package root (underscore alias); rows [6]–[8] reach consumers only as `refinementStrategy`/`zRange`/`data` prop value types, since the package root does not re-export them as standalone symbols.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------------------- |
|   [1]   | `_Tileset2D`                          | class         | viewport-to-tile-index strategy (override for custom indexing)              |
|   [2]   | `_Tile2DHeader<DataT>`                | class         | per-tile load state, bounds, and content                                    |
|   [3]   | `_Tileset2DProps`                     | type alias    | `Tileset2D` constructor props                                               |
|   [4]   | `_TileLoadProps`                      | type alias    | `{ index, id, bbox, url?, signal?, zoom?, userData? }`                      |
|   [5]   | `GeoBoundingBox`, `NonGeoBoundingBox` | type aliases  | `{ west, north, east, south }` / `{ left, top, right, bottom }`             |
|   [6]   | `RefinementStrategy`                  | type alias    | `'never' \| 'no-overlap' \| 'best-available' \| RefinementStrategyFunction` |
|   [7]   | `ZRange`                              | type alias    | `[minZ, maxZ]` numeric tuple                                                |
|   [8]   | `URLTemplate`                         | type alias    | `string \| string[] \| null`                                                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TileLayer core props
- rail: viewport

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------------------- |
|   [1]   | `data` prop                                                   | data prop      | `URLTemplate` — tile URL template `{x}/{y}/{z}` or array        |
|   [2]   | `getTileData` prop                                            | loader prop    | `(props: TileLoadProps) => Promise<DataT> \| DataT` per tile    |
|   [3]   | `renderSubLayers` prop                                        | render prop    | `(props & { id, data, _offset, tile }) => Layer \| LayersList`  |
|   [4]   | `tileSize` prop                                               | uniform prop   | tile pixel dimension, usually a power of 2                      |
|   [5]   | `minZoom`, `maxZoom` props                                    | uniform props  | data zoom range (`minZoom` default `0`, `maxZoom` `null`)       |
|   [6]   | `extent` prop                                                 | uniform prop   | `number[] \| null` — data bounding box                          |
|   [7]   | `TilesetClass` prop                                           | dispatch prop  | `typeof Tileset2D` — custom indexing scheme                     |
|   [8]   | `refinementStrategy` prop                                     | dispatch prop  | `RefinementStrategy` (default `'best-available'`)               |
|   [9]   | `zRange` prop                                                 | uniform prop   | `ZRange \| null` — min/max height for 3D culling                |
|  [10]   | `maxCacheSize`, `maxCacheByteSize`                            | uniform props  | tile cache count and byte ceilings                              |
|  [11]   | `maxRequests`, `debounceTime`                                 | uniform props  | concurrent `getTileData` cap (default `6`) and request debounce |
|  [12]   | `zoomOffset`, `visibleMinZoom`, `visibleMaxZoom`              | uniform props  | fetch-zoom offset and per-zoom visibility clamp                 |
|  [13]   | `onTileLoad`, `onTileUnload`, `onTileError`, `onViewportLoad` | callback props | tile and viewport lifecycle hooks                               |

[ENTRYPOINT_SCOPE]: MVTLayer added props
- rail: viewport

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                                                      |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------------------------------------- |
|   [1]   | `data` prop                 | data prop      | `TileJson \| URLTemplate` — TileJSON descriptor or URL template             |
|   [2]   | `binary` prop               | toggle prop    | parse tiles as binary `BinaryFeatureCollection` (default `true`)            |
|   [3]   | `loaders` prop              | loader prop    | `Loader[]` — defaults to `MVTWorkerLoader` from `@loaders.gl/mvt`           |
|   [4]   | `uniqueIdProperty` prop     | accessor prop  | feature property keying highlight across tile boundaries                    |
|   [5]   | `highlightedFeatureId` prop | accessor prop  | `string \| number \| null` — feature to highlight                           |
|   [6]   | `onDataLoad` prop           | callback prop  | `(tilejson: TileJson \| null) => void` on TileJSON fetch                    |
|   [7]   | GeoJSON accessor props      | accessor props | the full `GeoJsonLayerProps` set minus `data` (fill/stroke/extrusion/point) |

[ENTRYPOINT_SCOPE]: layer instance operations
- rail: viewport

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                                                    |
| :-----: | :------------------------------------ | :------------- | :-------------------------------------------------------- |
|   [1]   | `TileLayer.getTileData(tile)`         | method         | resolves one tile's data (override or `getTileData` prop) |
|   [2]   | `TileLayer.renderSubLayers(props)`    | method         | maps a loaded tile to renderable `Layer`/`LayersList`     |
|   [3]   | `TileLayer.isLoaded` getter           | accessor       | `boolean` — all viewport tiles resolved                   |
|   [4]   | `MVTLayer.getRenderedFeatures(max?)`  | method         | `Feature[]` — features rendered in current viewport       |
|   [5]   | `_getURLFromTemplate(template, tile)` | utility        | substitutes `{x}/{y}/{z}` into a `URLTemplate`            |

## [4]-[IMPLEMENTATION_LAW]

[GEO_LAYERS_TOPOLOGY]:
- `TileLayer` is a `CompositeLayer<DataT, ExtraPropsT>` that fetches only viewport-visible tiles; supply either a `getTileData` prop or a `data` `URLTemplate`, then a `renderSubLayers` prop to turn each loaded tile into one or more `Layer` instances.
- `renderSubLayers` receives the full `TileLayerProps<DataT>` plus `{ id, data, _offset, tile: Tile2DHeader<DataT> }`; the injected `id` keeps sub-layers reconciled per tile.
- `getTileData` receives a `TileLoadProps` (`{ index: { x, y, z }, id, bbox, url?, signal?, zoom? }`) and returns `Promise<DataT> | DataT`; the `signal` is the `AbortSignal` for tiles leaving the viewport.
- `MVTLayer` extends `TileLayer<ParsedMvtTile>` where `ParsedMvtTile = Feature[] | BinaryFeatureCollection`; with `binary: true` (default) it yields `BinaryFeatureCollection` for `GeoJsonLayer.data`, bypassing per-feature JavaScript iteration.
- `MVTLayer` accepts a `TileJson` descriptor or a raw `URLTemplate` as `data`; `onDataLoad` fires once on TileJSON resolution, and `uniqueIdProperty` plus `highlightedFeatureId` drive cross-tile feature highlight.
- `MVTLayer.getRenderedFeatures(maxFeatures?)` returns the deduplicated `Feature[]` currently drawn, the entrypoint for viewport feature inspection without re-fetching tiles.
- `Tileset2D` owns the viewport-to-tile-index strategy; subclass it and pass via `TilesetClass` to implement a non-standard indexing scheme. `Tile2DHeader<DataT>` carries each tile's bounds, content, and load state.
- `GeoBoundingBox` (`{ west, north, east, south }`) is the geographic tile bbox; `NonGeoBoundingBox` (`{ left, top, right, bottom }`) is the cartesian form, and `TileBoundingBox` is their union. Discriminate on the presence of `west`/`north` keys, since the package root does not re-export the internal `isGeoBoundingBox` helper.
- `refinementStrategy` controls visibility during refinement: `'best-available'` shows parent/child tiles while loading, `'no-overlap'` hides until exact tiles load, `'never'` disables refinement.

[LOCAL_ADMISSION]:
- Drive `TileLayer`/`MVTLayer` from the `GeoSeriesLayer` `overlay` case for out-of-core feature sets; reserve the in-memory `GeoJsonLayer` for bounded collections.
- Pass `AbortSignal` from `TileLoadProps.signal` into the `getTileData` fetch so off-viewport tile requests cancel cleanly.
- Prefer `binary: true` on `MVTLayer` for high feature counts; only disable it when a sub-layer accessor needs JavaScript `Feature` objects.
- Memoize `renderSubLayers` and `getTileData` references to avoid full tileset rebuilds on re-render.

[RAIL_LAW]:
- Package: `@deck.gl/geo-layers`
- Owns: out-of-core tile streaming (`TileLayer`, `MVTLayer`, `Tile3DLayer`, `TerrainLayer`), cell-index layers (H3/S2/quadkey/geohash/A5), great-circle and trips layers, and the `Tileset2D` indexing model
- Accept: `TileLayer` as the base for any viewport-driven tile source; `MVTLayer` for Mapbox Vector Tiles; `Tileset2D` subclassing for custom indexing
- Reject: loading entire feature datasets into a single `GeoJsonLayer` when tile streaming applies; hand-rolling tile fetch, cache, or viewport-cull loops outside `TileLayer`
