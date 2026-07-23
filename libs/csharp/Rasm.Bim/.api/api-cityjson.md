# [RASM_BIM_API_CITYJSON]

`bertt.CityJSON` reads and writes CityJSON — the OGC CityGML JSON encoding for 3D city models — as a managed codec over a `CityJsonDocument` graph of a transform-quantized vertex pool, a typed `CityObject` taxonomy, and an index-referenced LoD geometry hierarchy, across single-document and CityJSONSeq forms. Its `CityJSON.Extensions` rail dequantizes the geometry into NetTopologySuite `Polygon` and `Feature` sets, landing a dataset on the geospatial NTS algebra. Codec ownership stops at the `.city.json` round-trip and the NTS handoff — no reprojection, raster ingest, or IFC semantics.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bertt.CityJSON`
- package: `bertt.CityJSON` (MIT)
- assembly: `cityjson`
- namespace: `CityJSON`, `CityJSON.Geometry`, `CityJSON.Extensions`, `CityJSON.IO`
- asset: `netstandard2.0` only; the `net10.0` consumer binds `lib/netstandard2.0` (single TFM, binds forward)
- serialization: Newtonsoft.Json — `JToken`/`JObject` ride the model (`CityObject.Address`, attribute bags), and read is Newtonsoft deserialization since no `CityJsonReader` ships
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and quantization (`CityJSON`)

`CityJsonDocument` roots the graph — `Metadata`, `Transform`, `CityObjects` (`Dictionary<string,CityObject>` keyed by object id), `Vertices` (`List<Vertex>`), `Appearance`, and `Type`/`Version` — with `GetVerticesEnvelope()` returning the dequantized AABB.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `CityJsonDocument` | class         | document root, vertex pool, envelope accessor                       |
|  [02]   | `Transform`        | class         | `Scale`/`Translate` (`double[]`), `ScaleVector3`/`TranslateVector3` |
|  [03]   | `Vertex`           | class         | `X`/`Y`/`Z` (`double`), `ToVector3`                                 |
|  [04]   | `Metadata`         | class         | dataset metadata record                                             |
|  [05]   | `Pointofcontact`   | class         | `Metadata.PointOfContact` record                                    |
|  [06]   | `Address`          | class         | xAL-style address record                                            |

[METADATA]: `GeographicalExtent` (`float[]`) `Identifier` `PointOfContact` `ReferenceDate` `ReferenceSystem` `Title`
[POINTOFCONTACT]: `ContactName` `ContactType` `Role` `Phone` `EmailAddress` `Website` `Address`
[ADDRESS]: `ThoroughfareNumber` `ThoroughfareName` `Locality` `Postcode` `Country`

[PUBLIC_TYPE_SCOPE]: city objects (`CityJSON`)

`CityObject` is one urban feature — `Type` (`CityObjectType`), `Attributes` (`Dictionary<string,object>`), `Geometry` (`List<Geometry>`, per-LoD), and `Address` (`JToken`).

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------- | :------------ | :------------------------------------- |
|  [01]   | `CityObject`     | class         | one typed urban feature                |
|  [02]   | `CityObjectType` | enum          | CityGML feature taxonomy (cases below) |

[CITYOBJECTTYPE]: `GroundSurface` `Bridge` `BridgePart` `BridgeInstallation` `BridgeConstructiveElement` `BridgeRoom` `BridgeFurniture` `Building` `BuildingPart` `BuildingInstallation` `BuildingsConstructiveElement` `BuildingFurniture` `BuildingStorey` `BuildingRoom` `BuildingUnit` `CityFurniture` `CityObjectGroup` `GenericCityObject` `LandUse` `OtherConstruction` `PlantCover` `SolitaryVegetationObject` `TINRelief` `TransportationSquare` `Railway` `Tunnel` `TunnelPart` `TunnelInstallation` `TunnelConstructionElement` `TunnelHollowSpace` `TunnelFurniture` `Waterbody` `WaterWay`

[PUBLIC_TYPE_SCOPE]: LoD geometry hierarchy (`CityJSON.Geometry`)

`Boundaries` are nested `int[]` indices into `CityJsonDocument.Vertices`, one nesting level per dimension (surface = ring×vertex, solid = shell×surface×ring×vertex); `Texture` is the parallel per-theme UV-index map (`Dictionary<string, int?[]…>`).

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                                                         |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Geometry`                 | abstract class | base — `Type` (`GeometryType`), `Lod` (level-of-detail string)       |
|  [02]   | `GeometryType`             | enum           | geometry-kind vocabulary (cases below)                               |
|  [03]   | `MultiSurfaceGeometry`     | class          | `Boundaries` `int[][][]` (surface×ring×vertex) + per-theme `Texture` |
|  [04]   | `CompositeSurfaceGeometry` | class          | `Boundaries` `int[][][]` (surface×ring×vertex) + per-theme `Texture` |
|  [05]   | `SolidGeometry`            | class          | `Boundaries` `int[][][][]` (shell×surface×ring×vertex) + `Texture`   |
|  [06]   | `MultiSolidGeometry`       | class          | `Boundaries` `int[][][][][]` + `Texture`                             |
|  [07]   | `CompositeSolidGeometry`   | class          | `Boundaries` `int[][][][][]` + `Texture`                             |

[GEOMETRYTYPE]: `CompositeSolid` `CompositeSurface` `GeometryInstance` `MultiLineString` `MultiPoint` `MultiSolid` `MultiSurface` `Solid`

[PUBLIC_TYPE_SCOPE]: appearance and textures (`CityJSON`)

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Appearance`       | class         | `Textures` (`List<Texture>`), `VerticesTexture` (`float[][]` UV pool)      |
|  [02]   | `Texture`          | class         | `Image`, `ImageType`, `WrapMode`, `TextureType`, `BorderColor` (`float[]`) |
|  [03]   | `TextureImageType` | enum          | image format — `PNG`, `JPG`                                                |
|  [04]   | `TextureWrapMode`  | enum          | wrap mode — `none`, `wrap`, `mirror`, `clamp`, `border`                    |
|  [05]   | `TextureType`      | enum          | texture kind — `unknown`, `specific`, `typical`                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: single-document read and write (`CityJSON`)

Read is Newtonsoft deserialization into `CityJsonDocument`; write and the dequant helpers are static.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `JsonConvert.DeserializeObject<CityJsonDocument>(string) -> CityJsonDocument` | static   | read a CityJSON string into the graph |
|  [02]   | `CityJsonWriter.Write(CityJsonDocument) -> string`                            | static   | serialize a document to CityJSON text |
|  [03]   | `CityJsonWriter.WriteToFile(CityJsonDocument, string)`                        | static   | serialize a document to a file path   |
|  [04]   | `CityJsonDocument.GetVerticesEnvelope() -> (Envelope, float, float)`          | instance | dequantized NTS envelope and Z-range  |
|  [05]   | `Transform.ScaleVector3() / TranslateVector3() -> Vector3`                    | instance | dequantization vectors                |
|  [06]   | `Vertex.ToVector3() -> Vector3`                                               | instance | a vertex as `System.Numerics.Vector3` |

[ENTRYPOINT_SCOPE]: CityJSONSeq streaming (`CityJSON`)

CityJSONSeq is newline-delimited — one JSON object per line, a metadata header line then per-feature lines — for large datasets.

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `CityJsonSeqReader.ReadCityJsonSeq(string) -> List<CityJsonDocument>` | static  | stream-read a `.city.jsonl` sequence |
|  [02]   | `CityJsonSeqWriter.WriteCityJsonSeq(List<CityJsonDocument>, string)`  | static  | stream-write a CityJSONSeq file      |

[ENTRYPOINT_SCOPE]: NTS projection rail (`CityJSON.Extensions`, `CityJSON.IO`)

`ToFeatures` folds the whole document into NTS `Feature`s, each carrying a `MultiPolygon` and an `AttributesTable` built from `CityObject.Attributes`; the rail applies `Transform` per coordinate so a caller never walks the geometry tree by hand.

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `CityJsonDocument.ToFeatures(Transform?, string?) -> List<Feature>`      | fold    | project a document to NTS features per LoD          |
|  [02]   | `CityObject.ToFeature(List<Vertex>, Transform, string?) -> Feature`      | fold    | one feature — `MultiPolygon` + `AttributesTable`    |
|  [03]   | `Geometry.ToPolygons(List<Vertex>, Transform, string?) -> List<Polygon>` | fold    | dequantize and triangulate boundaries to polygons   |
|  [04]   | `CoordinateZ.Transform(Transform) -> CoordinateZ`                        | static  | dequantize a single 3D coordinate                   |
|  [05]   | `PolygonCreator.GetPolygon(List<Vertex>, int[][], Transform) -> Polygon` | static  | build one NTS polygon (outer plus holes) from rings |

- `Geometry.ToPolygons` dispatches the per-subtype family: the surface and solid classes expose `ToPolygons`, the `MultiSolidGeometry`/`CompositeSolidGeometry` classes expose `ToPolys`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Geometry.Boundaries` hold no coordinates — they are integer indices into `CityJsonDocument.Vertices`, and each `Vertex` is itself quantized; a real position is `vertex × Transform.ScaleVector3() + Transform.TranslateVector3()`, and boundary nesting depth equals the geometry dimensionality so a `Geometry.Type` downcast and its array rank agree.
- `CityObject.Geometry` is a list of per-`Lod` representations; select by `Geometry.Lod`, never by index.
- `Metadata.ReferenceSystem` is the CRS URN — data, not a transform; reprojection is a separate leg.
- `GetVerticesEnvelope()` dequantizes to the NTS `Envelope` and Z-range for the document AABB without walking the geometry tree.

[STACKING]:
- `api-nettopologysuite`(`libs/csharp/.api/api-nettopologysuite.md`): `ToFeatures`/`ToPolygons` return `NetTopologySuite.Features.Feature` and `Geometries.Polygon`, and `GetVerticesEnvelope()` an `Envelope` — a dataset lands on the NTS `Geometry`/`Envelope`/STRtree algebra as one more NTS-Feature source beside the shapefile and GeoPackage codecs (`api-nts-esri-shapefile`).
- `api-projnet`(`libs/csharp/.api/api-projnet.md`): `Metadata.ReferenceSystem` drives the ProjNET datum/projection leg, reprojecting dequantized vertices into the shared projected frame before urban context federates with the BIM model.
- `api-sharpgltf-3dtiles`(`libs/csharp/.api/api-sharpgltf-3dtiles.md`), `api-subtree`: dequantized solid/surface geometry tessellates into the `Exchange/export` glTF/3D-Tiles delivery pipeline, sharing the projected frame the georeference leg establishes.
- `api-hashing`(`libs/csharp/.api/api-hashing.md`): a `CityJsonWriter.Write` string's UTF-8 bytes mint the urban-context snapshot content key on the shared content-identity rail.
- within-lib: `CityJsonDocument.ToFeatures(lod)` composes the full decode in one fold — walking `CityObjects`, applying `Transform` per `CoordinateZ`, triangulating each `Geometry` through `PolygonCreator` into a `MultiPolygon`, and packing `CityObject.Attributes` into the NTS `AttributesTable`.

[LOCAL_ADMISSION]:
- import enters through Newtonsoft deserialization into `CityJsonDocument` (or `CityJsonSeqReader.ReadCityJsonSeq` for the streaming form), dequantizes via `Transform`, and maps each `CityObject` onto a canonical `BimElement` with an `ElementPredicate`-classified `IfcClass`; `CityJSON.*` types never cross the codec boundary.
- export enters through a canonical build into `CityJsonDocument` (vertex pooling and boundary index encoding) then `CityJsonWriter.Write*` or `CityJsonSeqWriter.WriteCityJsonSeq`.

[RAIL_LAW]:
- Package: `bertt.CityJSON`
- Owns: CityJSON / CityGML JSON read+write — the transform-quantized vertex pool, the typed `CityObject` taxonomy, the index-encoded LoD geometry hierarchy, appearance/textures, the CityJSONSeq streaming form, and the NTS `Feature`/`Polygon` projection rail
- Accept: 3D urban/city-context interchange, dataset envelope/CRS extraction, NTS-Feature handoff
- Reject: coordinate reprojection (ProjNET), raster/general-vector GIS ingest (MaxRev.Gdal), mesh/tessellation codecs (SharpGLTF/AssimpNetter), IFC semantics (GeometryGym), and leaking `CityJSON.*` types past the codec boundary
