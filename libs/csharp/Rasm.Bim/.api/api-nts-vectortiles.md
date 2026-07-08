# [RASM_BIM_API_NTS_VECTORTILES]

`NetTopologySuite.IO.VectorTiles` is the in-memory Mapbox-Vector-Tile authoring model over the
NTS `Feature` shape: a `VectorTile` carries named `Layer`s of `IFeature`, a `VectorTileTree`
is the `ulong`-tile-id-keyed pyramid those tiles live in, and the `VectorTileTreeExtensions.Add`
fold slices a `FeatureCollection` / `IEnumerable<IFeature>` into per-`{z}/{x}/{y}` tiles by a
fixed or per-feature `(zoom, layerName)` policy. It owns the WebMercator XYZ tile algebra
(`Tile`/`TileRange`/`WebMercatorHandler`) the slicing keys on, and is the format-neutral half of
the MVT pair — the wire encode/decode is `NetTopologySuite.IO.VectorTiles.Mapbox`
(`.api/api-nts-vectortiles-mapbox`). It is the 2D web-tile-pyramid leg of the
`Semantics/geospatial#GEOSPATIAL_SEAM`: the `GeoFeature` NTS `Geometry`/`AttributesTable` rows
the site model produces feed `VectorTileTree.Add`, the tree encodes to an `{z}/{x}/{y}.mvt`
pyramid for MapLibre/Mapbox-GL, distinct from the 3D-Tiles glTF stack (`subtree` +
`SharpGLTF.Ext.3DTiles`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.VectorTiles`
- package: `NetTopologySuite.IO.VectorTiles`
- license: MIT
- assembly: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles` (`VectorTile`, `Layer`, `VectorTileTree`, the `VectorTileTreeExtensions`/`VectorTileExtensions` folds)
- namespace: `NetTopologySuite.IO.VectorTiles.Tiles` (`Tile`, `TileRange`, the WebMercator XYZ tile algebra)
- namespace: `NetTopologySuite.IO.VectorTiles.Tiles.WebMercator` (`WebMercatorHandler` lat/lon↔meters↔pixels)
- namespace: `NetTopologySuite.IO.VectorTiles.Tilers` (`PolygonTiler`/`LineStringTiler`/`PointTiler`/`Shared` — `internal`, invoked transitively by `Add`; not consumer surface)
- asset: netstandard2.0 single managed AnyCPU assembly; the net10.0 consumer binds `lib/netstandard2.0`
- asset: IL-only, no P/Invoke, no native dependency
- dependency: `NetTopologySuite` (the `Geometry` algebra and `GeometryFactory`/`NtsGeometryServices` the tilers cut against) and `NetTopologySuite.Features` (`IFeature`/`Feature`/`IAttributesTable`/`FeatureCollection` — the row shape `Add` consumes; settled at `.api/api-nettopologysuite`)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the in-memory tile model
- package: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles`
- rail: geometry

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:------------------ |:------- |:------------------------------------------------------------------------------------------------------- |
| [01] | `VectorTile` | geometry | one MVT tile; `ulong TileId` (the WebMercator `Tile.Id`), `IList<Layer> Layers`, `bool IsEmpty` (all layers empty). The unit `MapboxTileReader.Read` produces and `MapboxTileWriter.Write` consumes |
| [02] | `Layer` | geometry | a named feature group inside a tile; `string Name`, `IList<IFeature> Features`, `virtual bool IsEmpty`. The MVT layer the renderer styles by `Name` |
| [03] | `VectorTileTree` | geometry | the tile pyramid; `IEnumerable<ulong>` of populated tile ids, `this[ulong tileId]` get/set, `bool TryGet(ulong, out VectorTile)`, `List<ulong> GetTileIds()`, `void GetExtents(out double[] bounds, out int minZoom, out int maxZoom)`. The dictionary-backed multi-zoom tile set `Add` populates and `Write` walks |

[PUBLIC_TYPE_SCOPE]: the WebMercator tile algebra
- package: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles.Tiles`, `NetTopologySuite.IO.VectorTiles.Tiles.WebMercator`
- rail: geometry

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------- |:------- |:------------------------------------------------------------------------------------------------------- |
| [01] | `Tile` | geometry | one XYZ tile; `int X`/`Y`/`Zoom`, `double Top`/`Bottom`/`Left`/`Right`/`CenterLat`/`CenterLon` (EPSG:4326 bbox), `ulong Id`, `Tile Parent`, `bool IsValid`. Ctors `Tile(ulong id)` / `Tile(int x, int y, int zoom)` — the tile definition `MapboxTileReader.Read(stream, tile)` decodes against |
| [02] | `Tile` (static) | geometry | `Tile? CreateAroundLocation(double lat, double lon, int zoom)` / `ulong CreateAroundLocationId(...)` (the tile a coordinate falls in), `ulong CalculateTileId(int zoom, int x, int y)`, `(int x, int y, int zoom) CalculateTile(ulong id)`, `bool IsDirectNeighbour(ulong, ulong)` — the id↔(x,y,zoom) bijection the tilers walk |
| [03] | `Tile` (instance) | geometry | `TileRange GetSubTiles(int zoom)` (the child range one level down), `ulong GetSubTileIdFor(double lat, double lon)`, `Tile InvertX()` / `InvertY()` (the TMS↔XYZ Y-flip), `string ToGeoJson()` (the tile footprint as GeoJSON) |
| [04] | `TileRange` | geometry | a rectangular `(XMin,YMin,XMax,YMax,Zoom)` block of tiles; `IEnumerable<Tile?>`, ctor `TileRange(int xMin, int yMin, int xMax, int yMax, int zoom)`, `long Count`, `bool Contains(Tile)`, `IEnumerable<Tile?> EnumerateInCenterFirst()` (center-out emission for progressive tile streaming) |
| [05] | `WebMercatorHandler` | geometry | static EPSG:3857 projection math; `(double x, double y) LatLonToMeters(double lat, double lon)` / `MetersToLatLon`, `(long x, long y) FromMetersToPixels((double,double) m, int zoom, int tileSize = 512)` / `FromPixelsToMeters` — the spherical-mercator leg the tile bbox derives from (distinct from the `ProjNET`/OSR geodetic reprojection; this is the web-tile pixel grid only). The `MetersToPixels`/`PixelsToMeters` `(double,double)` overloads are `[Obsolete]` — bind the `From…`/`long` forms |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: feature-to-tile slicing (`VectorTileTreeExtensions`)
- package: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles`
- rail: geometry

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------- |:--------------------------------------------------------------------------------------- |:----------------------------------------------------------- |
| [01] | `VectorTileTree.Add` | `(FeatureCollection features, int zoom = 14, string layerName = "default")` | fixed-zoom ingest — every feature sliced into its zoom-14 tile(s) under one layer name |
| [02] | `VectorTileTree.Add` | `(IEnumerable<IFeature> features, int zoom = 14, string layerName = "default")` | the `IEnumerable` mirror — the `GeoFeature` row stream straight into the pyramid |
| [03] | `VectorTileTree.Add` | `(FeatureCollection features, ToFeatureZoomAndLayerFunc toFeatureZoomAndLayer)` | per-feature LOD + layer routing — the func returns `IEnumerable<(IFeature, int zoom, string layerName)>` so one feature lands at multiple zooms / multiple named layers (the LOD pyramid policy) |
| [04] | `VectorTileTree.Add` | `(IEnumerable<IFeature> features, ToFeatureZoomAndLayerFunc toFeatureZoomAndLayer)` | the `IEnumerable` mirror of the routed ingest |
| [05] | `VectorTileTree.Add` | `(IEnumerable<(IFeature feature, int zoom, string layerName)> featuresZoomAndLayer)` | the already-tupled form — the explicit `(feature, zoom, layer)` triples bypassing the router |
| [06] | `VectorTileTree.Add` | `(IFeature feature, int zoom, string layerName)` | the single-feature leaf the batch forms fold onto |
| [07] | `VectorTileTreeExtensions.ToFeatureZoomAndLayerFunc` | `delegate IEnumerable<(IFeature feature, int zoom, string layerName)>(IFeature feature)` | the per-feature LOD/layer policy delegate — the discriminator a `[SmartEnum]`/match over an element `IfcDomain`/scale folds the zoom-and-layer assignment onto |

[ENTRYPOINT_SCOPE]: pyramid bounds and tile geometry
- package: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles`, `NetTopologySuite.IO.VectorTiles.Tiles`
- rail: geometry

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------ |:---------------------------------------------------------------- |:----------------------------------------------------------- |
| [01] | `VectorTileTree.GetExtents` | `(out double[] bounds, out int minZoom, out int maxZoom)` | the populated pyramid's geographic envelope and zoom span — the `bounds`/`minzoom`/`maxzoom` feeding the `VectorTileSource` TileJSON the Mapbox catalog authors |
| [02] | `Tile.ToGeoJson` | `()` → `string` | one tile footprint as a GeoJSON polygon — the tile-grid overlay/debug geometry |
| [03] | `Tile.GetSubTiles` | `(int zoom)` → `TileRange` | the child tile range one or more levels down — the LOD descent the pyramid build walks |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- a single managed `NetTopologySuite.IO.VectorTiles.dll` (netstandard2.0); no P/Invoke, no native dependency — the net10.0 consumer binds `lib/netstandard2.0` directly.
- the package is FORMAT-NEUTRAL: it builds and holds the `VectorTile`/`Layer`/`VectorTileTree` object model but emits NO bytes. The protobuf wire encode/decode is `NetTopologySuite.IO.VectorTiles.Mapbox` (`MapboxTileReader`/`MapboxTileWriter`) — this package is paired with it, never standalone for I/O.
- the actual geometry tilers (`PolygonTiler`/`LineStringTiler`/`PointTiler`, the `Tile.ToPolygon`/`LineString.Cut` clip) are `internal`: a feature added through `Add` is clipped to each overlapping tile by these tilers transitively. They are NOT public consumer surface — the consumer composes `Add` and the tile algebra, never the clip primitives. A catalog or design page that names `PolygonTiler.Tiles` as a callable entrypoint documents a phantom.

[TILE_MODEL]:
- the tile id is the WebMercator XYZ `Tile.Id` `ulong` (a `zoom`/`x`/`y` interleave through `Tile.CalculateTileId`); `VectorTileTree` is a `Dictionary<ulong, VectorTile>` over those ids. A `VectorTile` holds one `Layer` per `Name`, each `Layer` an `IList<IFeature>` of the clipped, tile-local feature geometry.
- `Add` resolves each feature's overlapping tiles at the requested zoom (`Tile.CreateAroundLocationId` for a point, the internal `LineStringTiler`/`PolygonTiler` walk for lines/polygons), gets-or-creates the named `Layer` in each tile, and adds the per-tile-clipped `Feature` — so one large polygon spanning many tiles is split into one clipped feature per tile, the standard MVT tiling.
- the `ToFeatureZoomAndLayerFunc` overload is the LOD authoring rail: the func emits `(feature, zoom, layerName)` triples, so a feature appears at every zoom it must (a coarse simplification at low zoom, the full geometry at high zoom) and routes to a layer by attribute — the per-feature policy a fixed `(zoom, layerName)` cannot express.

[STACK_INTEGRATION]:
- geospatial seam: the `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature` carries an NTS `Geometry` + `AttributesTable` (the `IFeature` shape). The web-tile pyramid is one fold: `var tree = new VectorTileTree(); tree.Add(geoFeatures.Select(f => f.AsNtsFeature()), router)` where `router` is a `ToFeatureZoomAndLayerFunc` assigning zoom/layer by the `GeoFeature` `IfcClass`/scale — then the Mapbox writer encodes `tree` to the `{z}/{x}/{y}.mvt` pyramid. The features the `GeoModel` `STRtree` already indexes are the same `IFeature` rows the tree slices, so the broad-phase index and the tile pyramid read one feature set.
- precision seam: the internal tilers cut against `NtsGeometryServices.Instance.CreateGeometryFactory(4326)` — they read the SAME global factory singleton `GeoServices.Configure` sets at module init (`GeometryOverlay.NG` + `PackedCoordinateSequenceFactory`). The tile clip therefore inherits the canonical EPSG:4326 `PrecisionModel`, and a feature must be in geographic coordinates (the `ProjNET`/OSR reprojection happens BEFORE tiling, never inside it).
- Mapbox encode seam: `VectorTileTree` → `MapboxTileWriter.Write(tree, path, minLinealExtent, minPolygonalExtent, extent, idAttributeName)` writes the `{z}/{x}/{y}.mvt` directory, and `VectorTileTree.GetExtents` supplies the `bounds`/`minzoom`/`maxzoom` the `VectorTileSource` TileJSON manifest carries — the pyramid plus its catalog descriptor. The `MapboxTileReader.Read(stream, new Tile(z,x,y))` round-trips a stored tile back to a `VectorTile`.
- distinct from 3D-Tiles: this is the 2D raster-map vector-tile leg (MapLibre/Mapbox-GL basemap overlays). The 3D-Tiles tileset (`subtree` `.subtree` availability bitstream + `SharpGLTF.Ext.3DTiles` glTF content, `.api/api-subtree`/`api-sharpgltf-3dtiles`) is the orthogonal 3D streaming stack — the two coexist, never collapsed.

[LOCAL_ADMISSION]:
- the tile pyramid enters through `new VectorTileTree()` + the `Add` fold over the `GeoFeature` `IFeature` stream; the zoom/layer policy enters through a `ToFeatureZoomAndLayerFunc` discriminating on the canonical `GeoFeature` attributes, never a per-zoom imperative loop.
- the WebMercator tile id/bbox enters through `Tile`/`TileRange`/`WebMercatorHandler`; the `[Obsolete]` `MetersToPixels`/`PixelsToMeters` `double` overloads are the rejected forms — bind `FromMetersToPixels`/`FromPixelsToMeters`.
- the wire encode/decode is NOT here — it crosses to `MapboxTileReader`/`MapboxTileWriter` (`.api/api-nts-vectortiles-mapbox`); composing this package without the Mapbox pair yields an object model that never reaches bytes.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.VectorTiles` (+ transitive `NetTopologySuite` / `NetTopologySuite.Features`)
- Owns: the in-memory MVT object model (`VectorTile`/`Layer`/`VectorTileTree`), the feature→tile slicing fold (`Add` + `ToFeatureZoomAndLayerFunc`), and the WebMercator XYZ tile algebra (`Tile`/`TileRange`/`WebMercatorHandler`)
- Accept: building a tile pyramid from NTS `IFeature` rows, per-feature LOD/layer routing, tile id/bbox/sub-tile computation, pyramid extent and zoom-span query
- Reject: the MVT protobuf bytes (`NetTopologySuite.IO.VectorTiles.Mapbox` owns encode/decode), the geodetic reprojection into EPSG:4326 (the `Semantics/georeference` `ProjNET`/OSR leg runs before tiling), the planar `Geometry` algebra and `Feature` shape (`NetTopologySuite` owns them), the 3D-Tiles glTF tileset (`subtree` + `SharpGLTF.Ext.3DTiles` own it)
