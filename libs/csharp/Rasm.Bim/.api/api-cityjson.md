# [RASM_BIM_API_CITYJSON]

`bertt.CityJSON` is the pure-managed read+write codec for CityJSON — the OGC CityGML JSON
encoding for 3D city/urban-context models. It owns the `CityJsonDocument` object graph
(transform-quantized integer-indexed vertices, a typed `CityObject` dictionary, the boundary-
referenced LoD geometry hierarchy, appearance/textures, and metadata) plus both the single-
document writer and the CityJSONSeq (newline-delimited streaming) reader/writer. The vertex
encoding is the CityJSON-canonical form: geometry boundaries are nested arrays of INTEGER
INDICES into the document `Vertices` list, and real coordinates are recovered by applying the
`Transform` scale+translate — the codec preserves this losslessly, it does not tessellate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bertt.CityJSON`
- package: `bertt.CityJSON`
- license: MIT
- assembly: `cityjson`
- namespace: `CityJSON`
- namespace: `CityJSON.Geometry`
- asset: netstandard2.0 only; the net10.0 consumer binds the `lib/netstandard2.0` asset (single TFM, binds forward)
- serialization: Newtonsoft.Json (`JToken`/`JObject` appear on the model — `CityObject.Address`, attribute bags)
- transitive-floor: `NetTopologySuite.Features` (a separate package id from the NTS core, stacking on the NTS algebra)
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and quantization
- package: `bertt.CityJSON`
- namespace: `CityJSON`
- rail: geospatial

| [INDEX] | [SYMBOL]           | [RAIL]     | [CAPABILITY]                                                                                                                                                                                                                                      |
| :-----: | :----------------- | :--------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `CityJsonDocument` | geospatial | the root: `Type` (`"CityJSON"`), `Version`, `Transform`, `CityObjects` (`Dictionary<string, CityObject>` keyed by object id), `Vertices` (`List<Vertex>`), `Appearance`, `Metadata`; `GetVerticesEnvelope` → `(Envelope, float minZ, float maxZ)` |
|  [02]   | `Transform`        | geospatial | the quantization transform: `Scale` / `Translate` (`double[]`) with `ScaleVector3()` / `TranslateVector3()`; real coord = index-vertex × Scale + Translate                                                                                        |
|  [03]   | `Vertex`           | geospatial | a quantized integer-or-real coordinate: `X`/`Y`/`Z` (`double`), `ToVector3()`                                                                                                                                                                     |
|  [04]   | `Metadata`         | geospatial | `GeographicalExtent` (`float[]`, the bbox), `Identifier`, `PointOfContact`, `ReferenceDate`, `ReferenceSystem` (the CRS URN), `Title`                                                                                                             |
|  [05]   | `Pointofcontact`   | geospatial | the dataset point-of-contact (name/role/org/email/address) `Metadata.PointOfContact` carries                                                                                                                                                      |
|  [06]   | `Address`          | geospatial | a structured xAL-style address attached to a `CityObject`                                                                                                                                                                                         |

[PUBLIC_TYPE_SCOPE]: city objects
- package: `bertt.CityJSON`
- namespace: `CityJSON`
- rail: geospatial

| [INDEX] | [SYMBOL]         | [RAIL]     | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| :-----: | :--------------- | :--------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `CityObject`     | geospatial | one urban feature: `Type` (`CityObjectType`), `Attributes` (`Dictionary<string, object>`), `Geometry` (`List<CityJSON.Geometry.Geometry>` — multiple LoDs), `Address` (`JToken`)                                                                                                                                                                                                                                                                    |
|  [02]   | `CityObjectType` | geospatial | the CityGML feature taxonomy enum: `Building`/`BuildingPart`/`BuildingInstallation`/`BuildingStorey`/`BuildingRoom`/`BuildingUnit`/`BuildingFurniture`/`BuildingsConstructiveElement`, `Bridge`+parts, `Tunnel`+parts, `Railway`/`TransportationSquare`, `Waterbody`/`WaterWay`, `CityFurniture`, `CityObjectGroup`, `LandUse`, `PlantCover`/`SolitaryVegetationObject`, `TINRelief`, `GenericCityObject`, `OtherConstruction`, `GroundSurface`,... |

[PUBLIC_TYPE_SCOPE]: LoD geometry hierarchy
- package: `bertt.CityJSON`
- namespace: `CityJSON.Geometry`
- rail: geospatial

`Boundaries` is the CityJSON index encoding — nested `int[]` arrays referencing the document
`Vertices` list, with one more nesting level per dimension (surface = ring×vertex,
solid = shell×surface×ring×vertex). `Texture` is the parallel per-boundary UV-index map
(`Dictionary<string, int?[]…>` keyed by appearance theme).

| [INDEX] | [SYMBOL]                                            | [RAIL]     | [CAPABILITY]                                                                                                                           |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Geometry`                                          | geospatial | `abstract` base: `Type` (`GeometryType`), `Lod` (the level-of-detail string, e.g. `""`)                                                |
|  [02]   | `GeometryType`                                      | geospatial | enum: `MultiPoint`, `MultiLineString`, `MultiSurface`, `CompositeSurface`, `Solid`, `MultiSolid`, `CompositeSolid`, `GeometryInstance` |
|  [03]   | `MultiSurfaceGeometry` / `CompositeSurfaceGeometry` | geospatial | `Boundaries` (`int[][][]` — surface×ring×vertex) + per-theme `Texture` (`int?[][][]`)                                                  |
|  [04]   | `SolidGeometry`                                     | geospatial | `Boundaries` (`int[][][][]` — shell×surface×ring×vertex) + `Texture` (`int?[][][][]`)                                                  |
|  [05]   | `MultiSolidGeometry` / `CompositeSolidGeometry`     | geospatial | the multi-solid / composite-solid boundary nesting (one level above `SolidGeometry`)                                                   |

[PUBLIC_TYPE_SCOPE]: appearance and textures
- package: `bertt.CityJSON`
- namespace: `CityJSON`
- rail: geospatial

| [INDEX] | [SYMBOL]                                               | [RAIL]     | [CAPABILITY]                                                                                                                                                   |
| :-----: | :----------------------------------------------------- | :--------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Appearance`                                           | geospatial | the document appearance store: `Textures` (`List<Texture>`), `VerticesTexture` (`float[][]` — the UV coordinate pool the geometry `Texture` indices reference) |
|  [02]   | `Texture`                                              | geospatial | `Image` (the texture file ref), `ImageType` (`TextureImageType`), `WrapMode` (`TextureWrapMode`), `TextureType`, `BorderColor` (`float[]`)                     |
|  [03]   | `TextureImageType` / `TextureWrapMode` / `TextureType` | geospatial | the image-format, wrap-mode, and texture-kind enums                                                                                                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read and write (single document)
- package: `bertt.CityJSON`
- namespace: `CityJSON`
- rail: geospatial

The codec is static and file/string-shaped. Reading is via Newtonsoft deserialization of a
`CityJsonDocument` from the JSON text; writing goes through `CityJsonWriter`.

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]                                | [CAPABILITY]                                                                                                                                                                                                                       |
| :-----: | :------------------------------------------------ | :------------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `JsonConvert.DeserializeObject<CityJsonDocument>` | `(string json)` → `CityJsonDocument`        | the read counterpart — `Newtonsoft.Json` deserialization of the CityJSON text into the document graph; the package ships NO single-document `CityJsonReader`, read IS Newtonsoft (the symmetric inverse of `CityJsonWriter.Write`) |
|  [02]   | `CityJsonWriter.Write`                            | `(CityJsonDocument)` → `string`             | serialize a document to a CityJSON string                                                                                                                                                                                          |
|  [03]   | `CityJsonWriter.WriteToFile`                      | `(CityJsonDocument, string filePath)`       | serialize a document to a `.city.json` file                                                                                                                                                                                        |
|  [04]   | `CityJsonDocument.GetVerticesEnvelope`            | `()` → `(Envelope, float minZ, float maxZ)` | compute the NTS planar envelope + Z-range of the dequantized vertices                                                                                                                                                              |
|  [05]   | `Transform.ScaleVector3` / `TranslateVector3`     | `()` → `Vector3`                            | the quantization vectors to dequantize an index-vertex                                                                                                                                                                             |
|  [06]   | `Vertex.ToVector3`                                | `()` → `Vector3`                            | a single vertex as a `System.Numerics.Vector3`                                                                                                                                                                                     |

[ENTRYPOINT_SCOPE]: CityJSONSeq streaming
- package: `bertt.CityJSON`
- namespace: `CityJSON`
- rail: geospatial

CityJSONSeq is the newline-delimited streaming form (one JSON object per line: a
first-line metadata "CityJSON" object followed by per-feature objects) for large datasets.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                   | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :--------------------------------------------- | :---------------------------------------- |
|  [01]   | `CityJsonSeqReader.ReadCityJsonSeq`  | `(string filePath)` → `List<CityJsonDocument>` | stream-read a `.city.jsonl` sequence file |
|  [02]   | `CityJsonSeqWriter.WriteCityJsonSeq` | `(List<CityJsonDocument>, string filePath)`    | stream-write a CityJSONSeq file           |

## [04]-[IMPLEMENTATION_LAW]

[QUANTIZATION_LAW]:
- a `CityJsonDocument`'s geometry never holds coordinates directly — `Geometry.Boundaries` are integer indices into `CityJsonDocument.Vertices`, and each `Vertex` is itself quantized; recover a real position as `vertex × Transform.ScaleVector3() + Transform.TranslateVector3()`.
- `Metadata.ReferenceSystem` is the CRS URN (e.g. an EPSG OGC URN); it is data, not a transform — reprojection is a separate leg, not part of decode.
- `GetVerticesEnvelope()` dequantizes and returns the NTS planar `Envelope` plus the Z-range; use it for the document AABB without walking the geometry tree.

[GEOMETRY_DISPATCH]:
- discriminate a `CityObject.Geometry[i]` on `Geometry.Type` (`GeometryType`) and downcast to the matching subtype (`SolidGeometry`/`MultiSurfaceGeometry`/...); the boundary nesting depth IS the geometry dimensionality, so the cast and the array rank agree.
- `CityObject.Geometry` is a LIST because one object carries multiple `Lod` representations; select by `Geometry.Lod` rather than assuming index 0.

[INTEGRATION_STACK]:
- planar-algebra leg: `GetVerticesEnvelope()` returns a `NetTopologySuite.Geometries.Envelope` and the transitive floor pulls `NetTopologySuite.Features`, so a CityJSON dataset lands directly on the `Semantics/georeference` + geospatial NTS algebra (`api-nettopologysuite`) — the same `Geometry`/`Envelope`/STRtree surface the shapefile and GeoPackage codecs (`api-nts-esri-shapefile`) feed; CityJSON is one more NTS-Feature source row, not a parallel geometry world.
- reprojection leg: `Metadata.ReferenceSystem` (the source CRS URN) drives the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` (`api-projnet`) datum/projection leg; the dequantized vertices are reprojected into the shared projected frame before the urban context is federated with the BIM model — `MaxRev.Gdal.Core` (`api-maxrev-gdal`) owns the heavier raster/vector site-context ingest, CityJSON owns the structured 3D-city encoding.
- canonical-carrier leg: a `CityObject` (typed by `CityObjectType`) maps onto a `BimElement` row with an `ElementPredicate`-classified `IfcClass` at the `Exchange/import` boundary (`Building`→`IfcBuilding`, `Bridge`→`IfcBridge`, etc.); `CityJSON.*` types never leak past the codec boundary, internal code holds canonical Bim shapes.
- identity leg: a `CityJsonWriter.Write(document)` string (UTF-8 bytes) feeds `System.IO.Hashing` `XxHash3`/`XxHash128` (substrate, `api-hashing`) for the urban-context snapshot content key — `XxHash3` is the fast in-process fingerprint, `XxHash128` (`GetCurrentHashAsUInt128`) the persisted, collision-resistant key the `Rasm.Persistence` artifact index is content-addressed by — joining the same XxHash128-keyed content-identity rail as the IFC/glTF exports.
- 3D-tiles seam leg: the dequantized solid/surface geometry is tessellated into the `Exchange/export` glTF/3D-Tiles pipeline (`api-sharpgltf-3dtiles`, `api-subtree`) for web delivery — CityJSON is the SOURCE encoding, the 3D-Tiles legs are the delivery encoding; the shared frame is the projected coordinate space the georeference leg established.

[LOCAL_ADMISSION]:
- CityJSON import enters through Newtonsoft deserialization into `CityJsonDocument` (or `CityJsonSeqReader.ReadCityJsonSeq` for the streaming form), then dequantizes via `Transform` and maps `CityObject`s onto canonical Bim carriers.
- CityJSON export enters through a canonical→`CityJsonDocument` build (vertex pooling + boundary index encoding) then `CityJsonWriter.Write*` / `CityJsonSeqWriter.WriteCityJsonSeq`.

[RAIL_LAW]:
- Package: `bertt.CityJSON`
- Owns: CityJSON / CityGML JSON read+write — the transform-quantized vertex pool, the typed `CityObject` taxonomy, the index-encoded LoD geometry hierarchy, appearance/textures, and the CityJSONSeq streaming form
- Accept: 3D urban/city-context model interchange, dataset envelope/CRS extraction, NTS-Feature handoff
- Reject: coordinate reprojection (ProjNET), raster/general-vector GIS ingest (MaxRev.Gdal), tessellation/mesh codecs (SharpGLTF / AssimpNetter), IFC semantics (GeometryGym), and leaking `CityJSON.*` types past the codec boundary
