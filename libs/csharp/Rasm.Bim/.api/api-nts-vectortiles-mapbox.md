# [RASM_BIM_API_NTS_VECTORTILES_MAPBOX]

`NetTopologySuite.IO.VectorTiles.Mapbox` is the protobuf-net wire codec for the
`NetTopologySuite.IO.VectorTiles` in-memory model (`.api/api-nts-vectortiles`): `MapboxTileWriter`
encodes a `VectorTileTree` / `VectorTile` to the Mapbox-Vector-Tile (MVT, spec) protobuf —
either an `{z}/{x}/{y}.mvt` directory pyramid or a single `Stream` — and `MapboxTileReader`
decodes one MVT `Stream` back to a `VectorTile` of NTS `IFeature` rows. `VectorTileSource` /
`VectorLayer` are the TileJSON catalog descriptor a renderer reads to discover the pyramid.
It is the byte-emitting half of the MVT pair the `Semantics/geospatial#GEOSPATIAL_SEAM` web-tile
leg composes: the `GeoFeature` rows the site model produces are sliced into a `VectorTileTree`
by the sibling package, then written here to the `.mvt` pyramid MapLibre/Mapbox-GL renders,
distinct from the 3D-Tiles glTF stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.VectorTiles.Mapbox`
- package: `NetTopologySuite.IO.VectorTiles.Mapbox`
- license: MIT
- assembly: `NetTopologySuite.IO.VectorTiles.Mapbox`
- namespace: `NetTopologySuite.IO.VectorTiles.Mapbox` (`MapboxTileReader`, `MapboxTileWriter`, `VectorTileSource`, `VectorLayer`; the generated `Tile`/`Tile.Layer`/`Tile.Feature`/`Tile.Value` protobuf DTOs are the on-wire shape, not consumer surface)
- asset: netstandard2.0 single managed AnyCPU assembly; the net10.0 consumer binds `lib/netstandard2.0`
- asset: IL-only, no P/Invoke, no native dependency
- dependency: `NetTopologySuite.IO.VectorTiles` (the `VectorTile`/`Layer`/`VectorTileTree`/`Tile` model it serializes; `.api/api-nts-vectortiles`)
- dependency: `protobuf-net` — the MVT protobuf serializer (`Serializer.Deserialize<Tile>` / `Serializer.Serialize`). The central manifest lifts the transitive `protobuf-net` to (the admitted protobuf-net.Core line) so the codec resolves through the admitted protobuf-net.Core line
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the MVT wire codec
- package: `NetTopologySuite.IO.VectorTiles.Mapbox`
- namespace: `NetTopologySuite.IO.VectorTiles.Mapbox`
- rail: geometry

| [INDEX] | [SYMBOL]           | [RAIL]   | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                                            |
| :-----: | :----------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `MapboxTileReader` | geometry | the MVT decode; ctors `MapboxTileReader()` (EPSG:4326 `GeometryFactory`) / `MapboxTileReader(GeometryFactory)`, `VectorTile Read(Stream, Tile tileDefinition)` / `Read(Stream, Tile, string idAttributeName)` — deserializes the protobuf and de-quantizes each layer's `Extent`-grid integer geometry back to EPSG:4326 against the `Tile` bbox                        |
|  [02]   | `MapboxTileWriter` | geometry | the MVT encode — a static extension class over `VectorTileTree`/`IEnumerable<VectorTile>`/`VectorTile` (see [03]); `const uint DefaultMinLinealExtent = 1`, `DefaultMinPolygonalExtent = 2`, `const string DefaultIdAttributeName = "id"`                                                                                                                               |
|  [03]   | `VectorTileSource` | geometry | the TileJSON source descriptor; `string[] tiles` (URL templates), `int minzoom`/`maxzoom`, `double[] bounds`, `string name`/`description`/`attribution`/`format` (`"pbf"`)/`id`/`basename`/`version` (`""`)/`tilejson` (`""`), `VectorLayer[] vector_layers` — the JSON catalog a MapLibre/Mapbox-GL style points its source at (field names are the wire `snake_case`) |
|  [04]   | `VectorLayer`      | geometry | one TileJSON `vector_layers[]` entry; `int minzoom`/`maxzoom`, `string id`, `string description` — the per-layer zoom advertisement the renderer reads                                                                                                                                                                                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pyramid and tile encode (`MapboxTileWriter` extensions)
- package: `NetTopologySuite.IO.VectorTiles.Mapbox`
- namespace: `NetTopologySuite.IO.VectorTiles.Mapbox`
- rail: geometry

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]                                                                                                        | [CAPABILITY]                                                                                                                                                                                                             |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `VectorTileTree.Write`                                          | `(string path, uint minLinealExtent, uint minPolygonalExtent, uint extent = 4096, string idAttributeName = "id")`   | writes the whole pyramid as an `{path}/{z}/{x}/{y}.mvt` directory tree (one file per populated tile); `extent` is the tile-local integer grid resolution, `minLinealExtent`/`minPolygonalExtent` drop sub-pixel features |
|  [02]   | `IEnumerable<VectorTile>.Write`                                 | `(string path, uint minLinealExtent, uint minPolygonalExtent, uint extent = 4096, string idAttributeName = "id")`   | the same directory-pyramid write driven by an explicit tile sequence                                                                                                                                                     |
|  [03]   | `VectorTile.Write`                                              | `(Stream stream, uint minLinealExtent, uint minPolygonalExtent, uint extent = 4096, string idAttributeName = "id")` | encodes ONE tile to a `Stream` — the form for a tile server / object-store PUT rather than a filesystem pyramid                                                                                                          |
|  [04]   | `VectorTileTree.Write` / `VectorTile.Write` (obsolete overload) | `(…, uint extent = 4096[, string idAttributeName = "id"])`                                                          | the `[Obsolete]` overloads WITHOUT the `minLineal`/`minPolygonalExtent` knobs — bind the full-arity forms above                                                                                                          |

[ENTRYPOINT_SCOPE]: tile decode (`MapboxTileReader`)
- package: `NetTopologySuite.IO.VectorTiles.Mapbox`
- namespace: `NetTopologySuite.IO.VectorTiles.Mapbox`
- rail: geometry

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                  | [CAPABILITY]                                                                                                                                                             |
| :-----: | :---------------------- | :---------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `MapboxTileReader.Read` | `(Stream stream, Tile tileDefinition)` → `VectorTile`                         | decodes the MVT protobuf and de-quantizes the integer tile-grid geometry to EPSG:4326 against `tileDefinition`'s bbox (the `Tile(z,x,y)` the stored tile id resolves to) |
|  [02]   | `MapboxTileReader.Read` | `(Stream stream, Tile tileDefinition, string idAttributeName)` → `VectorTile` | the same decode threading the feature `Id` onto an attribute named `idAttributeName` so the round-tripped `IFeature` carries the MVT feature id                          |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- a single managed `NetTopologySuite.IO.VectorTiles.Mapbox.dll` (netstandard2.0); no P/Invoke. The MVT protobuf goes through `protobuf-net` (`Serializer.Deserialize<Tile>(stream)` / `Serializer.Serialize`) — the central manifest pins the transitive `protobuf-net` to so the modern protobuf-net.Core line resolves, resolves through the admitted protobuf-net.Core line.
- the generated `Tile`/`Tile.Layer`/`Tile.Feature`/`Tile.Value`/`Tile.GeomType` types are the protobuf wire DTOs (the `[ProtoContract]` shapes mapping the MVT spec's command-integer geometry, the layer `Extent`, the de-duplicated `Keys`/`Values` tag tables). They are populated transitively by `Read`/`Write` — the consumer never constructs them. A catalog naming `Tile.Feature` as authoring surface documents a phantom.

[ENCODE_DECODE]:
- WRITE: `MapboxTileWriter.Write` walks each `VectorTile`'s `Layer`s, quantizes every feature's `Geometry` to the tile-local integer `extent` grid (default 4096, the MVT-standard resolution), de-duplicates attribute keys/values into the layer tag tables, and serializes the protobuf. `minLinealExtent`/`minPolygonalExtent` are the sub-pixel cull: a line/polygon whose extent on the integer grid falls below the threshold is dropped, so a low-zoom tile sheds geometry too small to render. `idAttributeName` names the `IAttributesTable` key whose value becomes the MVT feature `id`.
- READ: `MapboxTileReader.Read` deserializes the protobuf `Tile`, then for each layer builds a `TileGeometryTransform` from the supplied `Tile` definition and the layer `Extent` and de-quantizes the integer geometry back to EPSG:4326 — so the decode REQUIRES the `Tile(z,x,y)` the stored file's path encodes (`new Tile(zoom, x, y)`), because the MVT bytes carry only tile-local coordinates, never the geographic anchor.
- the directory pyramid layout is `{path}/{Zoom}/{X}/{Y}.mvt` (the writer creates the `{Zoom}` and `{X}` directories), the standard XYZ tile-server layout MapLibre/Mapbox-GL fetches with a `{z}/{x}/{y}` URL template.

[STACK_INTEGRATION]:
- the MVT pair: `NetTopologySuite.IO.VectorTiles` builds the `VectorTileTree` from `GeoFeature` `IFeature` rows (`tree.Add(features, router)`), this package writes it (`tree.Write(path, 1, 2, 4096)`). The two are ALWAYS composed together — the sibling owns the object model and tile algebra, this package owns the bytes. The `GeoModel` `STRtree` the site model already indexes holds the same `IFeature` rows the tree slices.
- TileJSON descriptor: after `tree.Write(path, …)`, `tree.GetExtents(out var bounds, out var minZoom, out var maxZoom)` populates a `VectorTileSource { tiles = ["…/{z}/{x}/{y}.mvt"], bounds = bounds, minzoom = minZoom, maxzoom = maxZoom, vector_layers = […] }` serialized (System.Text.Json) as the `tile.json` a renderer discovers the pyramid through — the pyramid plus its catalog in one emit.
- coordinate-frame law: the geometry MUST be EPSG:4326 (WGS84 lon/lat) before tiling — the sibling's internal tilers and this codec's quantization both assume geographic coordinates against the WebMercator tile grid. The `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET`/OSR leg reprojects the `GeoFeature` geometry to 4326 BEFORE `tree.Add`, never inside the codec.
- wire-store seam: `VectorTile.Write(Stream, …)` is the per-tile form the `csharp:Rasm.Persistence/Store` object-store PUT or the `csharp:Rasm.AppHost` tile endpoint streams, distinct from the filesystem-pyramid `tree.Write(path, …)` — one tile to a stream for a dynamic tile server, the whole tree to a directory for a static pyramid.
- distinct from 3D-Tiles: the `.mvt` 2D vector-tile pyramid (MapLibre basemap overlays, the `csharp:Rasm.AppUi/Charts` Mapsui map surface) is orthogonal to the 3D-Tiles glTF tileset (`subtree` + `SharpGLTF.Ext.3DTiles`); the two delivery stacks coexist.

[LOCAL_ADMISSION]:
- the pyramid encode enters through `VectorTileTree.Write(path, minLineal, minPolygonal, extent)`; a single tile to a stream through `VectorTile.Write(stream, …)`. The `[Obsolete]` overloads omitting the `minLineal`/`minPolygonalExtent` cull are the rejected forms.
- the decode enters through `MapboxTileReader.Read(stream, new Tile(z, x, y))` — the `Tile` definition is mandatory; decoding without the geographic tile anchor yields tile-local integer coordinates, never geography.
- the TileJSON descriptor enters through `VectorTileSource`/`VectorLayer` populated from `VectorTileTree.GetExtents`; hand-authoring the `bounds`/zoom span beside the pyramid is the drift form.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.VectorTiles.Mapbox` (+ `NetTopologySuite.IO.VectorTiles`, `protobuf-net`)
- Owns: the Mapbox-Vector-Tile protobuf encode (`MapboxTileWriter` directory pyramid + single-stream) and decode (`MapboxTileReader`), and the TileJSON source descriptor (`VectorTileSource`/`VectorLayer`)
- Accept: serializing a `VectorTileTree`/`VectorTile` to `.mvt` bytes, de-serializing one `.mvt` stream against a `Tile` definition, advertising a pyramid through TileJSON
- Reject: building the tile model and slicing features (`NetTopologySuite.IO.VectorTiles` owns `VectorTileTree`/`Add`/`Tile`), the geodetic reprojection to EPSG:4326 (the `Semantics/georeference` `ProjNET`/OSR leg runs first), the planar `Geometry`/`Feature` algebra (`NetTopologySuite` owns it), the 3D-Tiles glTF tileset bytes (`subtree` + `SharpGLTF.Ext.3DTiles` own them)
