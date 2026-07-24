# [TS_UI_API_DECK_GL_GEO_LAYERS]

`@deck.gl/geo-layers` is the geospatial composite tier the `ui/viewer/geo/layers` plane drives over `@deck.gl/layers`: the `TileLayer` viewport-streaming engine, its vector/terrain/3D-tile payload specializations, the `_GeoCellLayer` discrete-global-grid family, and the `TripsLayer` motion path — every layer a `CompositeLayer` over the `@deck.gl/layers` marks, `TripsLayer` the one primitive. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/geo-layers`
- package: `@deck.gl/geo-layers` (MIT)
- abi: browser WebGL2/WebGPU via `@deck.gl/core`; worker-backed tile parsing via loaders.gl worker pools
- runtime: `scope:viewer` project-local; tile fetch/parse async + worker-backed, layers declarative
- modules: `TileLayer`, `MVTLayer`, `TerrainLayer`, `Tile3DLayer`, `TripsLayer`, `H3HexagonLayer`, `H3ClusterLayer`, `S2Layer`, `QuadkeyLayer`, `GeohashLayer`, `A5Layer`, `_GeoCellLayer`, `_WMSLayer`, `_Tileset2D`/`_Tile2DHeader`

## [02]-[TILING_ENGINE]

[TYPE_SCOPE]: `TileLayer` — the generic viewport-driven tile streamer, one engine parameterized by payload type `DataT`, the `getTileData` loader, and the `renderSubLayers` renderer, with `_Tileset2D` swappable via `TilesetClass` for a non-XYZ scheme.

| [INDEX] | [SYMBOL]                                                  | [CONSUMER_BOUNDARY]                                 |
| :-----: | :-------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `TileLayer<DataT>` (composite)                            | the streaming base; raster/vector/mesh tiles        |
|  [02]   | `TileLayerProps` cache/zoom axis                          | fetch throttling + cache + LOD policy               |
|  [03]   | tile-lifecycle callbacks                                  | callback family; `onViewportLoad` = all-loaded gate |
|  [04]   | `_Tileset2D` / `_Tileset2DProps`                          | custom index scheme via `TilesetClass`              |
|  [05]   | `_Tile2DHeader<DataT>`                                    | the per-tile cache node `renderSubLayers` receives  |
|  [06]   | `_TileLoadProps` / `GeoBoundingBox` / `NonGeoBoundingBox` | `getTileData` input + tile extent shapes            |
|  [07]   | `_getURLFromTemplate`                                     | `{x}/{y}/{z}` template expansion                    |
|  [08]   | `TileLayerPickingInfo` / `MVTLayerPickingInfo`            | pick payload; carries the `tile` node + feature     |

[KEY_PROPS] by row (signature + load-bearing props):
- [01]-[TILELAYER]: `data: URLTemplate`; `getTileData: (TileLoadProps) => Promise<DataT>`, `renderSubLayers: (props & {tile,data,_offset}) => Layer|LayersList`, `TilesetClass?`.
- [02]-[TILELAYERPROPS]: `tileSize`, `minZoom`/`maxZoom`, `zoomOffset`, `extent`, `zRange`, `maxCacheSize`/`maxCacheByteSize`, `maxRequests`, `debounceTime`, `refinementStrategy`, `visibleMin/MaxZoom`.
- [03]-[CALLBACKS]: `onTileLoad(tile)`, `onTileUnload(tile)`, `onTileError(err,tile)`, `onViewportLoad(tiles)`.
- [04]-[TILESET2D]: pluggable indexer via `TilesetClass`; `refinementStrategy` accepts `'best-available'|'no-overlap'|'never'|fn`, default `'best-available'`.
- [05]-[TILE2DHEADER]: `{index, boundingBox:[min,max], content:DataT, isVisible, isSelected, zoom, data, isLoaded, byteLength}`; `loadData`, `abort`, `setNeedsReload`.
- [06]-[TILELOADPROPS]: `{index, id, bbox, signal, …}`; `GeoBoundingBox` `{west,south,east,north}`, `NonGeoBoundingBox` `{left,top,right,bottom}`.

## [03]-[TILE_PAYLOAD_SPECIALIZATIONS]

[TYPE_SCOPE]: `TileLayer` specialized by payload — `MVTLayer` decodes Mapbox Vector Tiles and renders through `GeoJsonLayer`, `TerrainLayer` reconstructs a Martini mesh from an RGB elevation raster, `Tile3DLayer` streams an OGC 3D-Tiles/I3S hierarchy on its own `Tileset3D`; each fixes `DataT`, `getTileData`, and `renderSubLayers`.

| [INDEX] | [SYMBOL]              | [CONSUMER_BOUNDARY]                                     |
| :-----: | :-------------------- | :------------------------------------------------------ |
|  [01]   | `MVTLayer<FeatProps>` | vector basemaps/overlays; cross-tile feature highlight  |
|  [02]   | `TerrainLayer`        | 3D terrain surface; RGB height → mesh                   |
|  [03]   | `Tile3DLayer<DataT>`  | photogrammetry/BIM/point-cloud 3D tiles (b3dm/pnts/i3s) |

[DISTINCTIVE_SURFACE] by row:
- [01]-[MVTLAYER]: `data: TileJson | URLTemplate`; `binary` (default true), `uniqueIdProperty`, `highlightedFeatureId`, `loaders`; all `GeoJsonLayer` accessors; `getRenderedFeatures(maxFeatures?): Feature[]`, `MVTLayerPickingInfo` specializes `TileLayerPickingInfo` (the `tile` node) for the parsed `Feature`.
- [02]-[TERRAINLAYER]: `elevationData: URLTemplate`, `texture?`, `elevationDecoder: {rScaler,gScaler,bScaler,offset}`, `meshMaxError` (Martini tolerance), `bounds`, `color`, `wireframe`, `material`.
- [03]-[TILE3DLAYER]: `data: string` (tileset.json), `getPointColor`, `pointSize`, `onTilesetLoad(Tileset3D)`, `onTileLoad(Tile3D)`, `onTileUnload`, `onTileError`, `_getMeshColor`, `loaders`.

## [04]-[CELL_FAMILY_AND_MOTION]

[TYPE_SCOPE]: `_GeoCellLayer` fans the discrete-global-grid cell family into one `indexToBounds()` pattern discriminated by index scheme, rendered through `PolygonLayer`; `TripsLayer` adds the time-animated path and `_WMSLayer` the image binding, `H3HexagonLayer` the high-precision GPU path extending `PolygonLayer` directly.

| [INDEX] | [SYMBOL]                          | [CONSUMER_BOUNDARY]                                                  |
| :-----: | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `_GeoCellLayer<DataT>` (abstract) | the cell-family base; specialize the index decode                    |
|  [02]   | `S2Layer<DataT>`                  | S2 quadtree cells                                                    |
|  [03]   | `QuadkeyLayer<DataT>`             | Bing/quadkey tile cells                                              |
|  [04]   | `GeohashLayer<DataT>`             | geohash rectangles                                                   |
|  [05]   | `A5Layer<DataT>`                  | A5 pentagonal DGGS cells                                             |
|  [06]   | `H3ClusterLayer<DataT>`           | H3 region outlines                                                   |
|  [07]   | `H3HexagonLayer<DataT>`           | H3 hexagon fills; GPU high-precision path (not via `_GeoCellLayer`)  |
|  [08]   | `TripsLayer<DataT>`               | time-animated paths; `currentTime` from atom clock + `Deck._animate` |
|  [09]   | `_WMSLayer`                       | OGC WMS/image-source overlays                                        |

[INDEX_ACCESSOR] by row (index-scheme decode + distinctive props):
- [01]-[_GeoCellLayer]: `indexToBounds(): Partial<props>`; inherits all `PolygonLayer` accessors (`getFillColor`/`getLineColor`/`getElevation`/`extruded`/…).
- [02]-[S2LAYER]: `getS2Token: (d) => string`.
- [03]-[QUADKEYLAYER]: `getQuadkey: (d) => string`.
- [04]-[GEOHASHLAYER]: `getGeohash: (d) => string`.
- [05]-[A5LAYER]: `getPentagon: (d) => string | bigint`.
- [06]-[H3CLUSTERLAYER]: `getHexagons: (d) => H3IndexInput[]` (dissolves a set to one boundary).
- [07]-[H3HEXAGONLAYER]: `getHexagon: (d) => string`; `highPrecision:boolean|'auto'`, `coverage`, `centerHexagon`, `extruded`.
- [08]-[TRIPSLAYER]: `PathLayer` + `getTimestamps: (d) => NumericArray`, `currentTime`, `trailLength`, `fadeTrail`.
- [09]-[_WMSLayer]: `data: string | ImageSource`, `serviceType:'wms'|'auto'`, `layers:string[]`, `srs`, `onMetadataLoad`, `onImageLoad*`.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one tiling engine: `TileLayer` is the single viewport-driven streamer; a new tiled format is a `getTileData`/`renderSubLayers` pair or a `TilesetClass`, never a new tile engine.
- one cell pattern: the DGGS family is `_GeoCellLayer.indexToBounds()` parameterized by index scheme; a new grid is one subclass supplying an index accessor and a boundary decode, never a `PolygonLayer` fork, with `H3HexagonLayer` the sole GPU-path exception.
- cache and throttle are policy rows: `maxCacheByteSize`/`maxRequests`/`debounceTime`/`refinementStrategy` tune LOD and memory as values, never by subclassing the tileset.
- worker-parsed payloads: MVT binary decode, terrain Martini meshing, and 3D-tile parsing run in loaders.gl worker pools, keeping the CPU-heavy decode off the main thread by construction.

[STACKING]:
- `@deck.gl/core`+`@deck.gl/layers`(`.api/deck.gl-core.md`,`.api/deck.gl-layers.md`): every layer is a `CompositeLayer` whose `renderSubLayers`/`getSubLayerProps` render the `PolygonLayer`/`PathLayer`/`GeoJsonLayer`/`ScatterplotLayer` marks — the cell family and `MVTLayer` inherit the fill/stroke split, `TripsLayer` extends `PathLayer`; `data` takes the core `LayerDataSource` (URL templates, TileJSON, tileset.json).
- `@deck.gl/mapbox`(`.api/deck.gl-mapbox.md`): a `TileLayer`/`MVTLayer` basemap inside `MapboxOverlay` reads the synced maplibre `Viewport` for visible-tile computation — no manual camera plumbing.
- `@geoarrow/deck.gl-geoarrow`(`.api/geoarrow-deck.gl-geoarrow.md`): `GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer` mirror the cell family for an Arrow-columnar index column — reach for those on an Arrow batch, these on a JS object stream.
- `wire` + `mark/selection`: `MVTLayer.getRenderedFeatures()` and pick `PickingInfo.object` carry the vector feature to a `GlobalId`, and `uniqueIdProperty`/`highlightedFeatureId` drive cross-tile highlight of the selected `GlobalId`; decoded WKB from `wire` feeds `data` directly.
- `TripsLayer` motion: a derived atom (rAF-fed time fold) drives `currentTime` with `Deck._animate` (or `MapboxOverlay` `_animate`) advancing the trail every frame, `trailLength`/`fadeTrail` the decay policy.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`).
- `_`-prefixed exports (`_GeoCellLayer`, `_WMSLayer`, `_Tileset2D`, `_Tile2DHeader`) are advanced overlays — instantiate the concrete cell layers and `TileLayer`, reaching for `_Tileset2D`/`_Tile2DHeader` only to author a custom indexer.
- `Tile3DLayer` governs its cache and LOD through the `Tileset3D` hierarchy, not the 2D tile props, and renders glTF/b3dm through the `@deck.gl/mesh-layers` peer.

[RAIL_LAW]:
- Package: `@deck.gl/geo-layers`
- Owns: the `TileLayer` streaming engine (`_Tileset2D`/`_Tile2DHeader`/refinement/cache), its `MVTLayer`/`TerrainLayer`/`Tile3DLayer` payload specializations, the `_GeoCellLayer` DGGS family (`S2`/`Quadkey`/`Geohash`/`A5`/`H3Cluster` + `H3HexagonLayer`), the `TripsLayer` motion path, and the `_WMSLayer` image binding
- Accept: one `TileLayer` per source with a `getTileData`/`renderSubLayers` pair, cache and throttle as policy props, the cell family as one index-scheme-parameterized pattern, `MVTLayer.getRenderedFeatures`/pick → `GlobalId`, GeoArrow cell mirrors for Arrow input, an atom clock + `_animate` for `TripsLayer`
- Reject: re-implementing a tiler instead of a `getTileData`/`TilesetClass`, five parallel cell layers where one `_GeoCellLayer` pattern owns the space, subclassing the tileset to change cache or LOD, the deprecated `GreatCircleLayer`, instantiating `_`-prefixed internals for ordinary rendering
