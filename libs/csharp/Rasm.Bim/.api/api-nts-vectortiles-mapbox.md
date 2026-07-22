# [RASM_BIM_API_NTS_VECTORTILES_MAPBOX]

`NetTopologySuite.IO.VectorTiles.Mapbox` is the protobuf wire codec for the `NetTopologySuite.IO.VectorTiles` tile model (`.api/api-nts-vectortiles`): `MapboxTileWriter` encodes a `VectorTileTree`/`VectorTile` to MVT protobuf — an `{z}/{x}/{y}.mvt` directory pyramid or a single `Stream` — `MapboxTileReader` decodes one MVT `Stream` back to a `VectorTile` of `IFeature` rows, and `VectorTileSource`/`VectorLayer` are the TileJSON descriptor a renderer discovers the pyramid through. It is the byte-emitting half of the MVT pair the `Semantics/geospatial#GEOSPATIAL_SEAM` web-tile leg feeds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.VectorTiles.Mapbox`
- package: `NetTopologySuite.IO.VectorTiles.Mapbox` (MIT)
- assembly: `NetTopologySuite.IO.VectorTiles.Mapbox`
- namespace: `NetTopologySuite.IO.VectorTiles.Mapbox`
- dependency: `NetTopologySuite.IO.VectorTiles` (the `VectorTile`/`Layer`/`VectorTileTree` model it serializes; `.api/api-nts-vectortiles`)
- dependency: `protobuf-net` (`Serializer.Deserialize<Tile>`/`Serialize` over the generated MVT `Tile` DTOs)
- asset: netstandard2.0 managed AnyCPU assembly; IL-only, no P/Invoke
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the MVT wire codec

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------------- | :------------ | :------------------------------- |
|  [01]   | `MapboxTileReader` | class         | the MVT decode surface           |
|  [02]   | `MapboxTileWriter` | static class  | MVT encode over the tile model   |
|  [03]   | `VectorTileSource` | class         | the pyramid TileJSON descriptor  |
|  [04]   | `VectorLayer`      | class         | one per-layer zoom advertisement |

- `MapboxTileReader()` binds an EPSG:4326 `GeometryFactory`; `MapboxTileReader(GeometryFactory)` overrides it.
- `VectorTileSource` carries wire snake_case `tiles` `minzoom` `maxzoom` `bounds` `name` `description` `attribution` `format` `id` `basename` `version` `tilejson` `vector_layers` (`format` is `"pbf"`); each `VectorLayer` carries `id`/`minzoom`/`maxzoom`/`description`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pyramid and tile encode (`MapboxTileWriter` extensions)
- Write carry: the pyramid overloads take a directory `path`, the single-tile overload a `Stream`, all with `(uint minLinealExtent, uint minPolygonalExtent, uint extent = 4096, string idAttributeName = "id")` — `extent` is the tile-local integer grid, the two extents cull sub-pixel features.

| [INDEX] | [SURFACE]                       | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :------------------------------ | :-------- | :----------------------------------------------- |
|  [01]   | `VectorTileTree.Write`          | extension | the whole `{z}/{x}/{y}.mvt` directory pyramid    |
|  [02]   | `IEnumerable<VectorTile>.Write` | extension | the pyramid from an explicit tile sequence       |
|  [03]   | `VectorTile.Write`              | extension | one tile to a `Stream` for a server or store PUT |

[ENTRYPOINT_SCOPE]: tile decode (`MapboxTileReader`)
- Read carry: `Read(Stream, Tile[, string idAttributeName]) -> VectorTile` — the `Tile(z,x,y)` anchor is mandatory; MVT bytes carry only tile-local integer coordinates, never the geographic frame.

| [INDEX] | [SURFACE]               | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :---------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `MapboxTileReader.Read` | instance | decode one MVT stream against its `Tile` anchor                 |
|  [02]   | `MapboxTileReader.Read` | instance | the same decode threading the feature id onto `idAttributeName` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Tile`/`Tile.Layer`/`Tile.Feature`/`Tile.Value`/`Tile.GeomType` are the generated `[ProtoContract]` wire DTOs — command-integer geometry, the layer `Extent`, the de-duplicated `Keys`/`Values` tag tables — populated transitively by `Read`/`Write`; a catalog naming `Tile.Feature` as authoring surface documents a phantom.
- WRITE quantizes every feature's `Geometry` to the tile-local integer `extent` grid, de-duplicates attribute keys/values into the layer tag tables, and serializes the protobuf; `minLinealExtent`/`minPolygonalExtent` drop a line or polygon whose grid extent falls sub-pixel, and `idAttributeName` names the `IAttributesTable` key whose value becomes the MVT feature `id`.
- READ deserializes the `Tile`, builds a `TileGeometryTransform` from the supplied `Tile` definition and the layer `Extent`, and de-quantizes the integer geometry back to EPSG:4326, so decode REQUIRES the `Tile(z,x,y)` the stored path encodes.
- `MapboxTileWriter` lays the pyramid as `{path}/{Zoom}/{X}/{Y}.mvt`, creating the `{Zoom}` and `{X}` directories — the standard XYZ layout a `{z}/{x}/{y}` URL template fetches.

[STACKING]:
- `NetTopologySuite.IO.VectorTiles`(`.api/api-nts-vectortiles`): the sibling builds the `VectorTileTree` (`tree.Add(features, router)`) this codec writes (`tree.Write(path, 1, 2, 4096)`) — the sibling owns the object model and tile algebra, this package owns the bytes; `tree.GetExtents(out bounds, out minZoom, out maxZoom)` supplies the `VectorTileSource` TileJSON, the pyramid with its catalog in one emit.
- `protobuf-net`: `Serializer.Deserialize<Tile>`/`Serialize` drives the wire round-trip over the generated `Tile` DTOs.
- geospatial seam: the `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature` `IFeature` rows the site model produces feed the tree, and the `GeoModel` `STRtree` holds the same rows the tree slices.
- coordinate-frame law: geometry MUST be EPSG:4326 before tiling — the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET`/OSR leg reprojects `GeoFeature` geometry BEFORE `tree.Add`, never inside the codec.
- wire-store seam: `VectorTile.Write(Stream, …)` is the per-tile form the `csharp:Rasm.Persistence/Store` PUT or `csharp:Rasm.AppHost` tile endpoint streams, distinct from the filesystem pyramid `tree.Write(path, …)`.

[LOCAL_ADMISSION]:
- `VectorTileTree.Write(path, minLineal, minPolygonal, extent)` admits the pyramid encode, `VectorTile.Write(stream, …)` a single tile.
- `MapboxTileReader.Read(stream, new Tile(z, x, y))` admits the decode — decoding without the geographic tile anchor yields tile-local integer coordinates, never geography.
- `VectorTileSource`/`VectorLayer` populated from `VectorTileTree.GetExtents` admit the TileJSON descriptor; hand-authoring the `bounds`/zoom span beside the pyramid is the drift form.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.VectorTiles.Mapbox` (+ `NetTopologySuite.IO.VectorTiles`, `protobuf-net`)
- Owns: the Mapbox-Vector-Tile protobuf encode (`MapboxTileWriter` directory pyramid + single-stream) and decode (`MapboxTileReader`), and the TileJSON source descriptor (`VectorTileSource`/`VectorLayer`)
- Accept: serializing a `VectorTileTree`/`VectorTile` to `.mvt` bytes, de-serializing one `.mvt` stream against a `Tile` definition, advertising a pyramid through TileJSON
- Reject: building the tile model and slicing features (`NetTopologySuite.IO.VectorTiles` owns `VectorTileTree`/`Add`/`Tile`), the geodetic reprojection to EPSG:4326 (the `Semantics/georeference` `ProjNET`/OSR leg runs first), the planar `Geometry`/`Feature` algebra (`NetTopologySuite` owns it), the 3D-Tiles glTF tileset (`subtree` + `SharpGLTF.Ext.3DTiles` own them)
