# [RASM_PERSISTENCE_API_NTS_IO]

`NetTopologySuite.IO.GeoJSON4STJ` converts NetTopologySuite geometries, features,
and attribute tables through `System.Text.Json`; `NetTopologySuite.IO.GeoPackage`
codecs the GeoPackage geometry blob (GPB header plus WKB body) for SQLite-backed
GeoPackage stores; and the core `NetTopologySuite` assembly carries WKB/WKT
binary and text IO in `NetTopologySuite.IO`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`, `NetTopologySuite.Features`
- geometry package: `NetTopologySuite`, `NetTopologySuite.Features`
- serializer package: `System.Text.Json`
- asset: runtime library
- rail: spatial-values

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage`
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- geometry package: `NetTopologySuite`
- asset: runtime library
- rail: spatial-values

[PACKAGE_SURFACE]: `NetTopologySuite` (IO surface)
- package: `NetTopologySuite`
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite.IO`, `NetTopologySuite.Geometries`
- asset: transitive runtime library
- rail: spatial-values

## [2]-[PUBLIC_TYPES]

[CONVERTER_TYPES]: STJ GeoJSON converter admission
- rail: spatial-values

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :----------------- | :-------------------------------------------------- |
|   [1]   | `GeoJsonConverterFactory`      | converter factory  | admits all GeoJSON converters on STJ options        |
|   [2]   | `RingOrientationOption`        | orientation policy | selects polygon ring orientation on write           |
|   [3]   | `StjAttributesTableExtensions` | obsolete extension | forwards to `IPartiallyDeserializedAttributesTable` |

`RingOrientationOption` cases: `DoNotModify`, `EnforceRfc9746`.
`GeoJsonConverterFactory` constructor overloads: `()`, `(factory)`, `(writeGeometryBBox)`, `(factory, writeGeometryBBox)`, `(writeGeometryBBox, idPropertyName)`, `(factory, writeGeometryBBox, idPropertyName)`, `(writeGeometryBBox, idPropertyName, ringOrientationOption)`, `(factory, writeGeometryBBox, idPropertyName, ringOrientationOption)`, `(writeGeometryBBox, idPropertyName, ringOrientationOption, allowModifyingAttributesTables)`, `(factory, writeGeometryBBox, idPropertyName, ringOrientationOption, allowModifyingAttributesTables)`.

[ATTRIBUTE_TYPES]: feature attribute projection
- rail: spatial-values

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]     | [CAPABILITY]                                     |
| :-----: | :-------------------------------------- | :----------------- | :----------------------------------------------- |
|   [1]   | `IPartiallyDeserializedAttributesTable` | attribute contract | deserializes table or property to typed values   |
|   [2]   | `JsonElementAttributesTable`            | read-only adapter  | adapts `JsonElement` as `IAttributesTable`       |
|   [3]   | `JsonObjectAttributesTable`             | mutable adapter    | adapts `JsonObject` with `Add`/`DeleteAttribute` |

[GEOPACKAGE_TYPES]: GeoPackage geometry blob codec
- rail: spatial-values

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                         |
| :-----: | :----------------------- | :------------- | :--------------------------------------------------- |
|   [1]   | `GeoPackageGeoReader`    | blob decoder   | reads GPB header plus WKB body to `Geometry`         |
|   [2]   | `GeoPackageGeoWriter`    | blob encoder   | writes `Geometry` to GPB header plus WKB body        |
|   [3]   | `GeoPackageBinaryHeader` | header value   | carries magic, version, flags, SRID, and extent data |

`GeoPackageBinaryHeader` exposes `Magic`, `Version`, `Flags`, `Ordinates`, `IsEmpty`, `Endianess`, `SrsId`, `Extent`, `ZRange`, `MRange`; `Read(BinaryReader)` and `Write(BinaryWriter, header)` are static.

[WKB_IO_TYPES]: binary geometry codec (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                       |
| :-----: | :---------- | :------------- | :--------------------------------- |
|   [1]   | `WKBReader` | binary decoder | reads WKB byte arrays and streams  |
|   [2]   | `WKBWriter` | binary encoder | writes WKB byte arrays and streams |

`WKBReader` policy properties: `HandleSRID`, `HandleOrdinates`, `AllowedOrdinates`, `IsStrict`, `RepairRings`. Constructors: `()`, `(NtsGeometryServices)`. Read: `Read(byte[])`, `Read(Stream)`.
`WKBWriter` policy properties: `EncodingType`, `Strict`, `HandleSRID`, `HandleOrdinates`. Constructors: `()`, `(ByteOrder)`, `(ByteOrder, handleSRID)`, `(ByteOrder, handleSRID, emitZ)`, `(ByteOrder, handleSRID, emitZ, emitM)`. Write: `Write(Geometry)` → `byte[]`, `Write(Geometry, Stream)`. Static: `ToHex(byte[])`. Constant: `AllowedOrdinates = Ordinates.XYZM`.

[WKT_IO_TYPES]: text geometry codec (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                               |
| :-----: | :---------- | :------------- | :----------------------------------------- |
|   [1]   | `WKTReader` | text decoder   | reads WKT strings, streams, and TextReader |
|   [2]   | `WKTWriter` | text encoder   | writes WKT with ordinate and format policy |

`WKTReader` policy properties: `Factory`, `DefaultSRID`, `IsStrict`, `FixStructure`, `IsOldNtsCoordinateSyntaxAllowed`, `IsOldNtsMultiPointSyntaxAllowed`. Constructors: `()`, `(NtsGeometryServices)`, `(GeometryFactory)`. Read: `Read(string)`, `Read(Stream)`, `Read(TextReader)`.
`WKTWriter` policy properties: `Formatted`, `MaxCoordinatesPerLine`, `Tab`, `OutputOrdinates`, `PrecisionModel`. Constructors: `()`, `(outputDimension)`. Static factory: `ForMicrosoftSqlServer()`. Write: `Write(Geometry)` → `string`, `Write(Geometry, Stream)`, `Write(Geometry, TextWriter)`, `WriteFormatted(Geometry)`, `WriteFormatted(Geometry, TextWriter)`. Static helpers: `ToPoint(Coordinate)`, `ToLineString(CoordinateSequence)`, `ToLineString(Coordinate[])`, `ToLineString(Coordinate, Coordinate)`.

[PRECISION_AND_ORDINATE_TYPES]: factory precision and ordinate policy (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :-------------------- | :--------------- | :----------------------------------------- |
|   [1]   | `PrecisionModel`      | precision policy | carries Floating/Fixed scale and grid size |
|   [2]   | `Ordinates`           | ordinate flags   | selects X/Y/Z/M ordinate dimensions        |
|   [3]   | `NtsGeometryServices` | service root     | creates factories with precision and SRID  |

`PrecisionModel` static presets: `Floating`, `FloatingSingle`, `Fixed` (all `Lazy<PrecisionModel>`). Constructors: `()`, `(PrecisionModels)`, `(double scale)`, `(PrecisionModel)`. Methods: `MakePrecise(double)`, `MakePrecise(Coordinate)`.
`Ordinates` key composites: `XY = 3`, `XYZ = 7`, `XYM`, `XYZM`.
`NtsGeometryServices` constructors accept combinations of `CoordinateSequenceFactory`, `PrecisionModel`, SRID, `GeometryOverlay`, `GeometryRelate`, and `CoordinateEqualityComparer`; `CreateGeometryFactory(...)` overloads carry each combination.

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GeoJSON serializer admission
- rail: spatial-values

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]        | [CAPABILITY]                     |
| :-----: | :------------------------------------- | :------------------ | :------------------------------- |
|   [1]   | `GeoJsonConverterFactory`              | factory constructor | carries GeoJSON converter policy |
|   [2]   | `JsonSerializerOptions.Converters.Add` | options admission   | enables GeoJSON converters       |
|   [3]   | `DefaultIdPropertyName`                | policy constant     | names the feature `id` attribute |
|   [4]   | `CanConvert` / `CreateConverter`       | factory overrides   | resolves per-type converters     |

[ENTRYPOINT_SCOPE]: attribute value projection
- rail: spatial-values

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]       | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :----------------- | :------------------------------------------ |
|   [1]   | `TryDeserializeJsonObject<T>`                              | contract method    | converts a whole table to a typed CLR value |
|   [2]   | `TryGetJsonObjectPropertyValue<T>`                         | contract method    | converts one property to a typed CLR value  |
|   [3]   | `GetOptionalValue` / `GetNames` / `GetValues`              | table read         | reads loosely typed attribute values        |
|   [4]   | `StjAttributesTableExtensions.TryDeserializeJsonObject<T>` | obsolete forwarder | extension on `IAttributesTable`             |

[ENTRYPOINT_SCOPE]: GeoPackage blob codec
- rail: spatial-values

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]       | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------- | :----------------- | :------------------------------------------------ |
|   [1]   | `GeoPackageGeoReader.Read`                       | byte[] or `Stream` | decodes a GeoPackage blob to `Geometry`           |
|   [2]   | `GeoPackageGeoWriter.Write`                      | byte[] or `Stream` | encodes a `Geometry` to a GeoPackage blob         |
|   [3]   | `HandleOrdinates` / `HandleSRID` / `RepairRings` | codec policy       | caps ordinates, stamps header SRID, repairs rings |
|   [4]   | `GeoPackageBinaryHeader.Read`                    | static decoder     | parses the GPB header from `BinaryReader`         |
|   [5]   | `GeoPackageBinaryHeader.Write`                   | static encoder     | serializes the GPB header to `BinaryWriter`       |

[ENTRYPOINT_SCOPE]: WKB binary codec
- rail: spatial-values

| [INDEX] | [SURFACE]              | [CALL_SHAPE]         | [CAPABILITY]                       |
| :-----: | :--------------------- | :------------------- | :--------------------------------- |
|   [1]   | `WKBReader.Read`       | byte[] or `Stream`   | decodes standard WKB to `Geometry` |
|   [2]   | `WKBWriter.Write`      | `Geometry` → byte[]  | encodes `Geometry` to standard WKB |
|   [3]   | `WKBWriter.Write`      | `Geometry`, `Stream` | streams WKB encoding               |
|   [4]   | `WKBWriter.ToHex`      | `byte[]` → `string`  | hex-encodes a WKB byte array       |
|   [5]   | `WKBReader.HexToBytes` | `string` → `byte[]`  | decodes a hex WKB string           |

[ENTRYPOINT_SCOPE]: WKT text codec
- rail: spatial-values

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]             | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :----------------------- | :---------------------------------- |
|   [1]   | `WKTReader.Read`                  | string/Stream/TextReader | decodes WKT text to `Geometry`      |
|   [2]   | `WKTWriter.Write`                 | `Geometry` → `string`    | encodes `Geometry` to WKT           |
|   [3]   | `WKTWriter.WriteFormatted`        | `Geometry` → `string`    | indented WKT with coordinate policy |
|   [4]   | `WKTWriter.ForMicrosoftSqlServer` | static factory           | configures SQL Server WKT dialect   |

## [4]-[IMPLEMENTATION_LAW]

[GEOJSON_PROFILE]:
- profile: one converter factory owns the full GeoJSON conversion family; per-type converters are internal and reached only through `CreateConverter`
- convertible set: `Geometry` and its seven concrete subtypes, `IFeature` implementations, `FeatureCollection`, `IAttributesTable`
- geometry policy: default `GeometryFactory` carries SRID 4326; polygon rings default to `RingOrientationOption.EnforceRfc9746` (RFC 7946 exterior counter-clockwise)
- attribute policy: deserialization yields read-only `JsonElementAttributesTable` unless `allowModifyingAttributesTables` admits `JsonObjectAttributesTable`
- id policy: an attribute named `DefaultIdPropertyName` lifts to the feature `id` instead of `properties`

[GEOPACKAGE_PROFILE]:
- profile: blob layout is the GPB header (`GeoPackageBinaryHeader`: magic `GP`, version, flags for endianness, envelope kind, and emptiness, SRID, envelope, Z/M ranges) followed by a WKB body
- writer policy: `HandleOrdinates` restricts written ordinates within XYZM and selects the header envelope kind; empty points encode as NaN ordinates
- reader policy: `HandleSRID` stamps the header SRID onto the decoded geometry; NaN-coded empty points decode to an empty `Point`

[WKB_PROFILE]:
- profile: standard ISO WKB with optional SRID embedding and ordinate selection
- ordinate policy: `HandleOrdinates` gates which dimensions enter/leave the binary; `AllowedOrdinates` is the reader's factory cap
- byte order: `WKBWriter` defaults to little-endian; `ByteOrder` parameter on constructor selects endianness
- SRID: `HandleSRID` on the writer embeds the SRID in the EWKB extension; reader stamps the SRID from EWKB

[WKT_PROFILE]:
- profile: standard OGC WKT and extended WKT with ordinate, precision, and formatting policy
- ordinate policy: `OutputOrdinates` on the writer selects which dimensions appear; `PrecisionModel` controls decimal digits
- format policy: `Formatted` and `Tab`/`MaxCoordinatesPerLine` control indentation; `ForMicrosoftSqlServer()` uses legacy point syntax

[LOCAL_ADMISSION]:
- GeoJSON conversion enters only through `GeoJsonConverterFactory` on serializer options; never instantiate `Stj*` converters.
- GeoPackage geometry columns pass through `GeoPackageGeoReader`/`GeoPackageGeoWriter`; raw WKB readers do not understand the GPB header.
- Typed attribute access casts to `IPartiallyDeserializedAttributesTable`; the static extension methods are obsolete forwarders.
- Raw WKB/WKT IO uses `WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter` from the `NetTopologySuite` core assembly.

[RAIL_LAW]:
- Packages: `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `NetTopologySuite`
- Own: GeoJSON text interchange, GeoPackage blob coding, and WKB/WKT binary/text coding for NetTopologySuite spatial values
- Accept: NetTopologySuite geometry and feature contracts per the spatial-values rail
- Reject: hand-rolled GeoJSON shaping, raw WKB columns standing in for GeoPackage geometry blobs, or WKT strings parsed via `string.Split`
