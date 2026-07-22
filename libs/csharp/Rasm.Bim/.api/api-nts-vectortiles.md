# [RASM_BIM_API_NTS_VECTORTILES]

`NetTopologySuite.IO.VectorTiles` owns the in-memory Mapbox-Vector-Tile authoring model over the NTS `Feature` shape and the WebMercator XYZ tile algebra its slicing keys on: `VectorTileTree.Add` folds a feature stream into a `{z}/{x}/{y}` tile pyramid under a fixed or per-feature `(zoom, layerName)` policy. It is the format-neutral half of the MVT pair — the protobuf wire crosses to `NetTopologySuite.IO.VectorTiles.Mapbox` (`.api/api-nts-vectortiles-mapbox`) — feeding the `Semantics/geospatial` web-tile leg, distinct from the 3D-Tiles glTF stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.VectorTiles`
- package: `NetTopologySuite.IO.VectorTiles` (MIT)
- assembly: `NetTopologySuite.IO.VectorTiles`
- namespace: `NetTopologySuite.IO.VectorTiles` (the tile model and `Add` folds)
- namespace: `NetTopologySuite.IO.VectorTiles.Tiles` (the WebMercator XYZ tile algebra)
- namespace: `NetTopologySuite.IO.VectorTiles.Tiles.WebMercator` (`WebMercatorHandler` EPSG:3857 math)
- namespace: `NetTopologySuite.IO.VectorTiles.Tilers` (internal clip tilers, not consumer surface)
- asset: netstandard2.0 managed AnyCPU assembly, IL-only, no P/Invoke; the net10.0 consumer binds `lib/netstandard2.0`
- dependency: `NetTopologySuite` (the `Geometry` algebra + `GeometryFactory`/`NtsGeometryServices` the tilers cut against), `NetTopologySuite.Features` (the `IFeature`/`FeatureCollection` rows `Add` consumes; `.api/api-nettopologysuite`)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the tile model and its WebMercator XYZ tile algebra

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `VectorTile`         | class         | one MVT tile keyed by the WebMercator `Tile.Id`; `IList<Layer> Layers`   |
|  [02]   | `Layer`              | class         | a named `IFeature` group inside a tile                                   |
|  [03]   | `VectorTileTree`     | class         | the `ulong`-tile-id-keyed pyramid; `this[ulong]`, `TryGet`, `GetTileIds` |
|  [04]   | `Tile`               | class         | one XYZ tile; the id↔(x,y,zoom) bijection, EPSG:4326 bbox, navigation    |
|  [05]   | `TileRange`          | class         | an `(xMin,yMin,xMax,yMax,zoom)` block, center-first enumerable           |
|  [06]   | `WebMercatorHandler` | static class  | EPSG:3857 lat/lon↔meters↔pixels math                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the feature→tile slicing fold (`VectorTileTreeExtensions`)

`VectorTileTree.Add` slices features into per-`{z}/{x}/{y}` tiles; `zoom` defaults to 14, `layerName` to `"default"`. `ToFeatureZoomAndLayerFunc` is `delegate IEnumerable<(IFeature feature, int zoom, string layerName)>(IFeature feature)` — the per-feature LOD/layer discriminator a `[SmartEnum]`/match over an element `IfcDomain`/scale folds onto. `GetExtents` reads the populated pyramid's geographic envelope and zoom span for the `VectorTileSource` TileJSON descriptor.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Add(FeatureCollection, int, string)`                       | fold     | fixed-zoom ingest under one layer name         |
|  [02]   | `Add(IEnumerable<IFeature>, int, string)`                   | fold     | the `IEnumerable` mirror                       |
|  [03]   | `Add(FeatureCollection, ToFeatureZoomAndLayerFunc)`         | fold     | per-feature LOD + layer routing                |
|  [04]   | `Add(IEnumerable<IFeature>, ToFeatureZoomAndLayerFunc)`     | fold     | the routed-ingest `IEnumerable` mirror         |
|  [05]   | `Add(IEnumerable<(IFeature, int, string)>)`                 | fold     | already-tupled triples, the router bypassed    |
|  [06]   | `Add(IFeature, int, string)`                                | fold     | the single-feature leaf the batch folds onto   |
|  [07]   | `VectorTileTree.GetExtents(out double[], out int, out int)` | instance | envelope + zoom span for the TileJSON manifest |

[ENTRYPOINT_SCOPE]: the WebMercator XYZ tile algebra (`Tile`, `TileRange`, `WebMercatorHandler`)

`Tile` carries the id↔(x,y,zoom) bijection every tiler walks: ctors `Tile(ulong)` / `Tile(int, int, int)`, fields `X`/`Y`/`Zoom`, the `Top`/`Bottom`/`Left`/`Right` + `CenterLat`/`CenterLon` EPSG:4326 bbox, `Id`, `Parent`, `IsValid`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Tile.CreateAroundLocation(double, double, int)`             | static   | the tile at a lat/lon and zoom (`…Id` gives the id)       |
|  [02]   | `Tile.CalculateTileId(int, int, int)`/`CalculateTile(ulong)` | static   | the id ↔ (x, y, zoom) bijection                           |
|  [03]   | `Tile.IsDirectNeighbour(ulong, ulong)`                       | static   | edge adjacency of two tile ids                            |
|  [04]   | `Tile.GetSubTiles(int)`                                      | instance | the child `TileRange` one+ levels down (LOD descent)      |
|  [05]   | `Tile.GetSubTileIdFor(double, double)`                       | instance | the sub-tile id covering a lat/lon                        |
|  [06]   | `Tile.InvertX`/`InvertY`                                     | instance | TMS↔XYZ Y-axis flip                                       |
|  [07]   | `Tile.ToGeoJson()`                                           | instance | the tile footprint as a GeoJSON polygon                   |
|  [08]   | `TileRange(int, int, int, int, int)`                         | ctor     | an `(xMin,yMin,xMax,yMax,zoom)` block; `Count`/`Contains` |
|  [09]   | `TileRange.EnumerateInCenterFirst()`                         | instance | center-out tile emission for progressive streaming        |
|  [10]   | `WebMercatorHandler.LatLonToMeters`/`MetersToLatLon`         | static   | EPSG:4326 ↔ 3857 meters                                   |
|  [11]   | `WebMercatorHandler.FromMetersToPixels`/`FromPixelsToMeters` | static   | meters ↔ pixels at a zoom/`tileSize`                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- format-neutral: the `VectorTile`/`Layer`/`VectorTileTree` object model builds and holds tile-local geometry but emits NO bytes; the protobuf wire crosses to `NetTopologySuite.IO.VectorTiles.Mapbox`, so this package alone yields a model that never reaches disk.
- `VectorTileTree` is a `Dictionary<ulong, VectorTile>` over WebMercator XYZ `Tile.Id`s, each `VectorTile` one `Layer` per `Name`; `Add` resolves a feature's overlapping tiles at the zoom, gets-or-creates the named `Layer`, and adds the per-tile-clipped `Feature` — one polygon spanning many tiles becomes one clipped feature per tile.
- `PolygonTiler`/`LineStringTiler`/`PointTiler` clip features `internal`ly, invoked transitively by `Add`; naming any tiler as a callable entrypoint documents a phantom.

[STACKING]:
- `NetTopologySuite.IO.VectorTiles.Mapbox`(`.api/api-nts-vectortiles-mapbox`): `MapboxTileWriter.Write` emits the `VectorTileTree` as a `{z}/{x}/{y}.mvt` pyramid, `GetExtents` feeds the `VectorTileSource` TileJSON, and `MapboxTileReader.Read(stream, new Tile(z, x, y))` round-trips a stored tile — the pair is always composed, this package never reaches bytes alone.
- `NetTopologySuite`/`NetTopologySuite.Features`(`.api/api-nettopologysuite`): internal tilers cut against the global `NtsGeometryServices` EPSG:4326 `GeometryFactory`, so the tile clip inherits the canonical `PrecisionModel`; features arrive as `IFeature`/`FeatureCollection` rows.
- `Semantics/geospatial`: `GeoFeature` geometry+attributes rows the `GeoModel` `STRtree` already indexes feed `VectorTileTree.Add` under a `ToFeatureZoomAndLayerFunc` routing zoom/layer by `IfcClass`/scale; `ProjNET`/OSR reprojects to EPSG:4326 BEFORE tiling, never inside it.
- 3D-Tiles is orthogonal: the 2D `.mvt` pyramid (MapLibre/Mapbox-GL basemaps) and the 3D-Tiles glTF tileset (`subtree` + `SharpGLTF.Ext.3DTiles`) coexist as distinct delivery stacks.

[LOCAL_ADMISSION]:
- `VectorTileTree` admits the pyramid through `new VectorTileTree()` + the `Add` fold over the `GeoFeature` `IFeature` stream; a `ToFeatureZoomAndLayerFunc` on canonical `GeoFeature` attributes carries the zoom/layer policy, never a per-zoom imperative loop.
- `Tile`/`TileRange`/`WebMercatorHandler` admit tile id/bbox/sub-tile computation.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.VectorTiles` (+ transitive `NetTopologySuite`/`NetTopologySuite.Features`)
- Owns: the in-memory MVT object model (`VectorTile`/`Layer`/`VectorTileTree`), the feature→tile slicing fold (`Add` + `ToFeatureZoomAndLayerFunc`), and the WebMercator XYZ tile algebra (`Tile`/`TileRange`/`WebMercatorHandler`)
- Accept: building a tile pyramid from NTS `IFeature` rows, per-feature LOD/layer routing, tile id/bbox/sub-tile computation, pyramid extent and zoom-span query
- Reject: the MVT protobuf bytes (`NetTopologySuite.IO.VectorTiles.Mapbox` owns encode/decode), the geodetic reprojection into EPSG:4326 (the `Semantics/georeference` `ProjNET`/OSR leg runs first), the planar `Geometry`/`Feature` algebra (`NetTopologySuite` owns it), the 3D-Tiles glTF tileset (`subtree` + `SharpGLTF.Ext.3DTiles` own it)
