# [TS_UI_API_DECK_GL_GEO_LAYERS]

`@deck.gl/geo-layers` is the geospatial composite tier the `ui/viewer/geo/layers` plane composes over `@deck.gl/layers`: the `TileLayer` streaming engine (with its pluggable `_Tileset2D` indexer and `_Tile2DHeader` cache), its payload specializations (`MVTLayer` vector tiles, `TerrainLayer` Martini meshes, `Tile3DLayer` OGC 3D Tiles), the `TripsLayer` time-animated path, the `_WMSLayer` image-source binding, and the discrete-global-grid cell family. That cell family is one parameterized pattern, not five layers: `_GeoCellLayer` (abstract, `indexToBounds()` → `PolygonLayer`) is specialized only by the index accessor — `S2Layer.getS2Token`, `QuadkeyLayer.getQuadkey`, `GeohashLayer.getGeohash`, `A5Layer.getPentagon`, `H3ClusterLayer.getHexagons` — with `H3HexagonLayer` the high-precision GPU sibling. Most rows are `CompositeLayer` subclasses reusing the `@deck.gl/layers` marks (`TileLayer`/cell family/`Tile3DLayer` render sublayers); `TripsLayer` and the deprecated `GreatCircleLayer` are primitive-`Layer` subclasses (`PathLayer`/`ArcLayer`). This catalog documents the tiling/streaming machinery and the per-scheme accessors, deferring the base `Layer`/`Accessor`/`Viewport` surface to `.api/deck.gl-core.md`, the mark vocabulary to `.api/deck.gl-layers.md`, the `Tile3DLayer` glTF/mesh rendering to `.api/deck.gl-mesh-layers.md`, and the `extensions` pack to `.api/deck.gl-extensions.md`. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/geo-layers`
- package: `@deck.gl/geo-layers` (MIT)
- abi: browser WebGL2/WebGPU via `@deck.gl/core`; worker-backed tile parsing via loaders.gl worker pools
- peer (`~catalog` deck family, catalog loaders): the deck peers are admitted centrally with their own catalogs — `@deck.gl/core` (`CompositeLayer`/`Viewport`/`Accessor`, `.api/deck.gl-core.md`), `@deck.gl/layers` (`PolygonLayer`/`PathLayer`/`GeoJsonLayer`/`ScatterplotLayer` the composites render, `.api/deck.gl-layers.md`), `@deck.gl/mesh-layers` (`Tile3DLayer`'s glTF via `SimpleMeshLayer`/`ScenegraphLayer`, `.api/deck.gl-mesh-layers.md`), `@deck.gl/extensions` (the `LayerExtension` pack the `extensions` prop consumes, `.api/deck.gl-extensions.md`); the substrate stays transitive/deck-owned — `@luma.gl/core`+`/engine`+`/gltf`+`/shadertools`, `@loaders.gl/core`+`/mvt`+`/3d-tiles`+`/terrain`+`/wms`+`/tiles`+`/gis` (tile decoders), `@math.gl/core`+`/web-mercator`+`/culling` (tile-index + frustum math), `h3-js`/`a5-js`/`long` (DGGS index → boundary)
- catalog-verdict: KEEP — the tiling + DGGS + terrain + 3D-tiles tier; no lower-level substitute
- runtime: `scope:viewer` project-local; tile fetch/parse is async + worker-backed, layers are declarative
- modules: `TileLayer`, `MVTLayer`, `TerrainLayer`, `Tile3DLayer`, `TripsLayer`, `H3HexagonLayer`, `H3ClusterLayer`, `S2Layer`, `QuadkeyLayer`, `GeohashLayer`, `A5Layer`, `_GeoCellLayer`, `_WMSLayer`, `GreatCircleLayer` (deprecated), `_Tileset2D`/`_Tile2DHeader`

## [02]-[TILING_ENGINE]

[TYPE_SCOPE]: `TileLayer` — the generic viewport-driven tile streamer; one engine parameterized by tile-payload type `DataT`, the `getTileData` loader, and the `renderSubLayers` renderer.
- `TileLayer` computes visible tile indices from the `Viewport`, calls `getTileData(props: _TileLoadProps): Promise<DataT>` per tile (bounded by `maxRequests`/`debounceTime`), caches results in `_Tile2DHeader`s (`maxCacheSize`/`maxCacheByteSize`, `refinementStrategy` for load-in behavior), and renders each via `renderSubLayers({tile, data, _offset})`. `_Tileset2D` is swappable (`TilesetClass`) for non-XYZ schemes. Callbacks `onTileLoad`/`onTileUnload`/`onTileError`/`onViewportLoad` form one tile-lifecycle family.

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

[TYPE_SCOPE]: `TileLayer` specialized by payload — vector tiles, terrain meshes, 3D-tile hierarchies. Each fixes `DataT`, `getTileData`, and `renderSubLayers` for its format.
- `MVTLayer` decodes Mapbox Vector Tiles (binary by default) and renders through `GeoJsonLayer`, adding cross-tile feature highlight; `TerrainLayer` reconstructs a Martini mesh from an RGB-encoded elevation raster and drapes a texture; `Tile3DLayer` streams an OGC 3D Tiles / I3S hierarchy (its own `Tileset3D`, not `_Tileset2D`).

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

[TYPE_SCOPE]: the discrete-global-grid cell family — one `_GeoCellLayer` pattern discriminated by index scheme — plus the time-animated `TripsLayer` and the `_WMSLayer` image binding.
- `_GeoCellLayer` (abstract composite) implements `indexToBounds()` to map a cell index → polygon boundary and renders through `PolygonLayer`; each concrete cell layer supplies only the index accessor and the index→boundary decode. `S2`/`Quadkey`/`Geohash`/`A5`/`H3Cluster` are this one pattern with a different DGGS; `H3HexagonLayer` is the specialized high-precision GPU path (extends `PolygonLayer` directly, adds `highPrecision`/`coverage`/`centerHexagon`). Treat the cell family as one parameterized row keyed by index scheme, not five layers.

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
|  [10]   | `GreatCircleLayer<DataT>`         | geodesic arcs (superseded)                                           |

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
- [10]-[GREATCIRCLELAYER]: `@deprecated` → `ArcLayer{greatCircle:true}`.

## [05]-[IMPLEMENTATION_LAW]

[GEO_TOPOLOGY]:
- one tiling engine: `TileLayer` is the single viewport-driven streamer; MVT/Terrain/(WMS) are payload specializations fixing `getTileData` + `renderSubLayers`, never re-implemented tilers. A new tiled format is a `getTileData`/`renderSubLayers` pair (or a `TilesetClass`), not a new tile engine.
- one cell pattern: the DGGS family is `_GeoCellLayer.indexToBounds()` parameterized by index scheme; a new grid (e.g. rHEALPix) is one `_GeoCellLayer` subclass with an index accessor + boundary decode, never a fork of `PolygonLayer`. `H3HexagonLayer` is the sole high-precision exception because it owns a GPU path.
- cache + throttle are policy rows: `maxCacheByteSize`/`maxRequests`/`debounceTime`/`refinementStrategy` are the LOD/memory policy; tune them as values, never by subclassing the tileset.
- worker-parsed payloads: MVT binary decode, terrain Martini meshing, and 3D-tile parsing run in loaders.gl worker pools — the CPU-heavy decode stays off the main thread by construction (mirrors the `browser` GLB decode-worker port).

[INTEGRATION_LAW]:
- Stack on `@deck.gl/core` + `@deck.gl/layers`: every layer here is a `CompositeLayer` rendering the mark vocabulary (`PolygonLayer`/`PathLayer`/`GeoJsonLayer`/`ScatterplotLayer`); `renderSubLayers` and `getSubLayerProps` are the core seam. `data` accepts the core `LayerDataSource` (URL templates, TileJSON, tileset.json).
- Stack with `@deck.gl/mapbox`: a `TileLayer`/`MVTLayer` basemap composed into `MapboxOverlay` shares the maplibre camera automatically; the tiler reads the synced `Viewport` for visible-tile computation — no manual camera plumbing.
- Stack with `wire` + selection: `MVTLayer.getRenderedFeatures()` and pick `PickingInfo.object` carry the vector feature → `mark/selection` `GlobalId`; `uniqueIdProperty`/`highlightedFeatureId` drive cross-tile highlight of the selected `GlobalId`. Decoded WKB from `wire` also feeds these composites' `data` directly.
- Stack with `@geoarrow/deck.gl-geoarrow`: the geoarrow package mirrors the cell family (`GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer`) for Arrow-columnar cell data — reach for those when the index column is an Arrow batch, these when it is a JS object stream.
- Stack with an atom clock for `TripsLayer`: drive `currentTime` from a derived atom (rAF-fed time fold) and set `Deck._animate` (or `MapboxOverlay` `_animate`) so the trail advances every frame; `trailLength`/`fadeTrail` are the decay policy.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); tile fetch/parse is async and worker-backed.
- treat the DGGS cell family as one parameterized surface keyed by index scheme; adding a grid is one `_GeoCellLayer` subclass, never five parallel layers.
- `_`-prefixed exports (`_GeoCellLayer`, `_WMSLayer`, `_Tileset2D`, `_Tile2DHeader`) are overlay/advanced — instantiate the concrete cell layers and `TileLayer`; reach for `_Tileset2D`/`_Tile2DHeader` only to author a custom indexer.
- `GreatCircleLayer` is deprecated → `ArcLayer{greatCircle:true}`; `TerrainLayer.workerUrl` is deprecated → `loadOptions.terrain.workerUrl`.
- `Tile3DLayer` uses `Tileset3D` (3D-tiles), not `_Tileset2D` — its cache/LOD is governed by the tileset hierarchy, not the 2D tile props; it renders glTF/b3dm through the admitted `@deck.gl/mesh-layers` peer (`SimpleMeshLayer`/`ScenegraphLayer`; `.api/deck.gl-mesh-layers.md`), the peer that must resolve or `Tile3DLayer` and mesh tiles fail.
- `@deck.gl/mesh-layers`/`@deck.gl/extensions` peers are admitted centrally in `pnpm-workspace.yaml` (own catalogs `.api/deck.gl-mesh-layers.md`/`.api/deck.gl-extensions.md`), so `Tile3DLayer` mesh rendering and the `extensions` prop resolve concretely — no longer transitive-only.

[RAIL_LAW]:
- Package: `@deck.gl/geo-layers`
- Owns: the `TileLayer` streaming engine (`_Tileset2D`/`_Tile2DHeader`/refinement/cache), its payload specializations (`MVTLayer`/`TerrainLayer`/`Tile3DLayer`), the `_GeoCellLayer` DGGS family (`S2`/`Quadkey`/`Geohash`/`A5`/`H3Cluster` + `H3HexagonLayer`), the `TripsLayer` motion path, and the `_WMSLayer` image binding
- Accept: one `TileLayer` per tiled source with a `getTileData`/`renderSubLayers` pair, cache/throttle as policy props, the cell family as one index-scheme-parameterized pattern, `MVTLayer.getRenderedFeatures`/pick → `GlobalId`, GeoArrow cell mirrors for Arrow input, an atom clock + `_animate` for `TripsLayer`
- Reject: re-implementing a tiler instead of a `getTileData`/`TilesetClass`, five parallel cell layers where one `_GeoCellLayer` pattern owns the space, subclassing the tileset to change cache/LOD, the deprecated `GreatCircleLayer`/`workerUrl`, instantiating `_`-prefixed internals for ordinary rendering
