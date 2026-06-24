# [RASM_PERSISTENCE_API_NTS_IO]

`NetTopologySuite.IO.GeoJSON4STJ` converts NetTopologySuite geometries, features,
and attribute tables through `System.Text.Json`; `NetTopologySuite.IO.GeoPackage`
codecs the GeoPackage geometry blob (GPB header plus WKB body) for SQLite-backed
GeoPackage stores; and the core `NetTopologySuite` assembly carries WKB/WKT
binary and text IO in `NetTopologySuite.IO`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- version: `4.0.0`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters` (converters + attribute tables), `NetTopologySuite.Features`
- geometry package: `NetTopologySuite`, `NetTopologySuite.Features`
- serializer package: `System.Text.Json`
- bound asset: `lib/netstandard2.0` (single TFM; binds on the net10.0 consumer via netstandard2.0)
- asset: runtime library
- rail: spatial-values

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage`
- version: `2.0.0`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- geometry package: `NetTopologySuite`
- bound asset: `lib/netstandard2.0` (single TFM; binds on net10.0 via netstandard2.0)
- asset: runtime library
- rail: spatial-values

[PACKAGE_SURFACE]: `NetTopologySuite` (IO surface)
- package: `NetTopologySuite`
- version: `2.6.0` (transitive floor pulled by the IO packages and Npgsql.NetTopologySuite)
- license: BSD-3-Clause
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite.IO`, `NetTopologySuite.Geometries`
- bound asset: `lib/netstandard2.1` (binds netstandard2.1 over 2.0 on net10.0)
- asset: transitive runtime library
- rail: spatial-values

## [02]-[PUBLIC_TYPES]

[CONVERTER_TYPES]: STJ GeoJSON converter admission
- rail: spatial-values

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :----------------- | :-------------------------------------------------- |
|  [01]   | `GeoJsonConverterFactory`      | converter factory  | admits all GeoJSON converters on STJ options        |
|  [02]   | `RingOrientationOption`        | orientation policy | selects polygon ring orientation on write           |
|  [03]   | `StjAttributesTableExtensions` | obsolete extension | forwards to `IPartiallyDeserializedAttributesTable` |

`RingOrientationOption` cases: `DoNotModify`, `EnforceRfc9746`.
`GeoJsonConverterFactory` constructor overloads: `()`, `(factory)`, `(writeGeometryBBox)`, `(factory, writeGeometryBBox)`, `(writeGeometryBBox, idPropertyName)`, `(factory, writeGeometryBBox, idPropertyName)`, `(writeGeometryBBox, idPropertyName, ringOrientationOption)`, `(factory, writeGeometryBBox, idPropertyName, ringOrientationOption)`, `(writeGeometryBBox, idPropertyName, ringOrientationOption, allowModifyingAttributesTables)`, `(factory, writeGeometryBBox, idPropertyName, ringOrientationOption, allowModifyingAttributesTables)`.

[ATTRIBUTE_TYPES]: feature attribute projection (namespace `NetTopologySuite.IO.Converters`)
- rail: spatial-values

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]     | [CAPABILITY]                                     |
| :-----: | :-------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `IPartiallyDeserializedAttributesTable` | attribute contract | `: IAttributesTable`; typed table/property reify |
|  [02]   | `JsonElementAttributesTable`            | read-only adapter  | adapts `JsonElement` as `IAttributesTable`       |
|  [03]   | `JsonObjectAttributesTable`             | mutable adapter    | adapts `JsonObject` with `Add`/`DeleteAttribute` |

`IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>(JsonSerializerOptions, out T)` reifies the whole table to a typed CLR value and `TryGetJsonObjectPropertyValue<T>(name, JsonSerializerOptions, out T)` reifies one property — both take the same `JsonSerializerOptions` carrying the `GeoJsonConverterFactory`, so a feature's properties deserialize under the same converter graph as its geometry.

[GEOPACKAGE_TYPES]: GeoPackage geometry blob codec
- rail: spatial-values

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                         |
| :-----: | :----------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `GeoPackageGeoReader`    | blob decoder   | reads GPB header plus WKB body to `Geometry`         |
|  [02]   | `GeoPackageGeoWriter`    | blob encoder   | writes `Geometry` to GPB header plus WKB body        |
|  [03]   | `GeoPackageBinaryHeader` | header value   | carries magic, version, flags, SRID, and extent data |

`GeoPackageBinaryHeader` exposes `byte[] Magic` (`GP`), `byte Version`, `byte Flags`, `Ordinates Ordinates` (derived from the envelope-kind flag bits), `bool IsEmpty`, `int Endianess` (0/1 from the low flag bit), `int SrsId`, `Envelope Extent`, `Interval ZRange`, `Interval MRange`; the codec is static: `static GeoPackageBinaryHeader Read(BinaryReader)` and `static void Write(BinaryWriter, GeoPackageBinaryHeader)`. `ZRange`/`MRange` are NTS `Interval`, `Extent` is an NTS `Envelope` — not generic ranges.

[WKB_IO_TYPES]: binary geometry codec (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                       |
| :-----: | :---------- | :------------- | :--------------------------------- |
|  [01]   | `WKBReader` | binary decoder | reads WKB byte arrays and streams  |
|  [02]   | `WKBWriter` | binary encoder | writes WKB byte arrays and streams |

`WKBReader` policy properties: `HandleSRID`, `HandleOrdinates`, `AllowedOrdinates` (`Ordinates.XYZM & factory.Ordinates`), `IsStrict`. `RepairRings` is `[Obsolete("Use !IsStrict")]` and delegates to `!IsStrict` — use `IsStrict` directly. Constructors: `()`, `(NtsGeometryServices)`. Read: `Read(byte[])`, `Read(Stream)`. Static: `HexToBytes(string) → byte[]`.
`WKBWriter` policy properties: `EncodingType`, `Strict`, `HandleSRID`, `HandleOrdinates`. Constructors: `()`, `(ByteOrder)`, `(ByteOrder, handleSRID)`, `(ByteOrder, handleSRID, emitZ)`, `(ByteOrder, handleSRID, emitZ, emitM)`. Write: `Write(Geometry)` → `byte[]`, `Write(Geometry, Stream)`. Static: `ToHex(byte[])`. Constant: `AllowedOrdinates = Ordinates.XYZM`.

[WKT_IO_TYPES]: text geometry codec (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                               |
| :-----: | :---------- | :------------- | :----------------------------------------- |
|  [01]   | `WKTReader` | text decoder   | reads WKT strings, streams, and TextReader |
|  [02]   | `WKTWriter` | text encoder   | writes WKT with ordinate and format policy |

`WKTReader` policy properties: `Factory`, `DefaultSRID`, `IsStrict`, `FixStructure`, `IsOldNtsCoordinateSyntaxAllowed`, `IsOldNtsMultiPointSyntaxAllowed`. Constructors: `()`, `(NtsGeometryServices)`, `(GeometryFactory)`. Read: `Read(string)`, `Read(Stream)`, `Read(TextReader)`.
`WKTWriter` policy properties: `Formatted`, `MaxCoordinatesPerLine`, `Tab`, `OutputOrdinates`, `PrecisionModel`. Constructors: `()`, `(outputDimension)`. Static factory: `ForMicrosoftSqlServer()`. Write: `Write(Geometry)` → `string`, `Write(Geometry, Stream)`, `Write(Geometry, TextWriter)`, `WriteFormatted(Geometry)`, `WriteFormatted(Geometry, TextWriter)`. Static helpers: `ToPoint(Coordinate)`, `ToLineString(CoordinateSequence)`, `ToLineString(Coordinate[])`, `ToLineString(Coordinate, Coordinate)`.

[PRECISION_AND_ORDINATE_TYPES]: factory precision and ordinate policy (NetTopologySuite core)
- rail: spatial-values

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :-------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `PrecisionModel`      | precision policy | carries Floating/Fixed scale and grid size |
|  [02]   | `Ordinates`           | ordinate flags   | selects X/Y/Z/M ordinate dimensions        |
|  [03]   | `NtsGeometryServices` | service root     | creates factories with precision and SRID  |

`PrecisionModel` static presets: `Floating`, `FloatingSingle`, `Fixed` (all `Lazy<PrecisionModel>`). Constructors: `()`, `(PrecisionModels)`, `(double scale)`, `(PrecisionModel)`. Methods: `MakePrecise(double)`, `MakePrecise(Coordinate)`.
`Ordinates` key composites: `XY = 3`, `XYZ = 7`, `XYM`, `XYZM`.
`NtsGeometryServices` constructors accept combinations of `CoordinateSequenceFactory`, `PrecisionModel`, SRID, `GeometryOverlay`, `GeometryRelate`, and `CoordinateEqualityComparer`; `CreateGeometryFactory(...)` overloads carry each combination.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GeoJSON serializer admission
- rail: spatial-values

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]        | [CAPABILITY]                     |
| :-----: | :------------------------------------- | :------------------ | :------------------------------- |
|  [01]   | `GeoJsonConverterFactory`              | factory constructor | carries GeoJSON converter policy |
|  [02]   | `JsonSerializerOptions.Converters.Add` | options admission   | enables GeoJSON converters       |
|  [03]   | `DefaultIdPropertyName`                | policy constant     | names the feature `id` attribute |
|  [04]   | `CanConvert` / `CreateConverter`       | factory overrides   | resolves per-type converters     |

[ENTRYPOINT_SCOPE]: attribute value projection
- rail: spatial-values

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]       | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :----------------- | :------------------------------------------ |
|  [01]   | `TryDeserializeJsonObject<T>`                              | contract method    | converts a whole table to a typed CLR value |
|  [02]   | `TryGetJsonObjectPropertyValue<T>`                         | contract method    | converts one property to a typed CLR value  |
|  [03]   | `GetOptionalValue` / `GetNames` / `GetValues`              | table read         | reads loosely typed attribute values        |
|  [04]   | `StjAttributesTableExtensions.TryDeserializeJsonObject<T>` | obsolete forwarder | extension on `IAttributesTable`             |

[ENTRYPOINT_SCOPE]: GeoPackage blob codec
- rail: spatial-values

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]       | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `GeoPackageGeoReader.Read`                       | byte[] or `Stream` | decodes a GeoPackage blob to `Geometry`           |
|  [02]   | `GeoPackageGeoWriter.Write`                      | byte[] or `Stream` | encodes a `Geometry` to a GeoPackage blob         |
|  [03]   | `GeoPackageGeoReader.{HandleOrdinates,HandleSRID,RepairRings}` | reader policy | caps ordinates, stamps header SRID, repairs rings |
|  [04]   | `GeoPackageGeoWriter.HandleOrdinates`            | writer policy      | caps written ordinates within `AllowedOrdinates`  |
|  [05]   | `GeoPackageBinaryHeader.Read`                    | static decoder     | parses the GPB header from `BinaryReader`         |
|  [06]   | `GeoPackageBinaryHeader.Write`                   | static encoder     | serializes the GPB header to `BinaryWriter`       |

[ENTRYPOINT_SCOPE]: WKB binary codec
- rail: spatial-values

| [INDEX] | [SURFACE]              | [CALL_SHAPE]         | [CAPABILITY]                       |
| :-----: | :--------------------- | :------------------- | :--------------------------------- |
|  [01]   | `WKBReader.Read`       | byte[] or `Stream`   | decodes standard WKB to `Geometry` |
|  [02]   | `WKBWriter.Write`      | `Geometry` → byte[]  | encodes `Geometry` to standard WKB |
|  [03]   | `WKBWriter.Write`      | `Geometry`, `Stream` | streams WKB encoding               |
|  [04]   | `WKBWriter.ToHex`      | `byte[]` → `string`  | hex-encodes a WKB byte array       |
|  [05]   | `WKBReader.HexToBytes` | `string` → `byte[]`  | decodes a hex WKB string           |

[ENTRYPOINT_SCOPE]: WKT text codec
- rail: spatial-values

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]             | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :----------------------- | :---------------------------------- |
|  [01]   | `WKTReader.Read`                  | string/Stream/TextReader | decodes WKT text to `Geometry`      |
|  [02]   | `WKTWriter.Write`                 | `Geometry` → `string`    | encodes `Geometry` to WKT           |
|  [03]   | `WKTWriter.WriteFormatted`        | `Geometry` → `string`    | indented WKT with coordinate policy |
|  [04]   | `WKTWriter.ForMicrosoftSqlServer` | static factory           | configures SQL Server WKT dialect   |

## [04]-[IMPLEMENTATION_LAW]

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

[INTEGRATION_LAW]:
- The four codecs share one `NtsGeometryServices`/`GeometryFactory` precision+SRID configuration: `GeoJsonConverterFactory`, `GeoPackageGeoReader`, `WKBReader`, and `WKTReader` all bind a factory carrying the same `PrecisionModel` and SRID, so a geometry surviving a GeoJSON->WKB->GeoPackage round trip keeps one precision grid. A per-codec ad hoc factory is the rejected form.
- GeoJSON conversion stacks onto the `System.Text.Json` boundary the rest of Persistence uses: `GeoJsonConverterFactory` is added to the SAME `JsonSerializerOptions` instance that carries the document's other converters, and `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>(options, out _)` reifies feature properties under that one options graph — a geometry and its typed properties never deserialize under two disjoint converter sets.
- The GeoPackage blob codec and the WKB core codec meet the Npgsql spatial seam (`api-nts-ef`, `api-npgsql`): a PostgreSQL/PostGIS geometry column round-trips through Npgsql's NTS plugin, a SQLite GeoPackage column through `GeoPackageGeoReader`/`Writer`, and both produce the same NTS `Geometry` CLR type — the spatial-values rail is provider-agnostic at the `Geometry` boundary, codec-specific only at the wire.
- WKB is the canonical interchange binary: `WKBWriter.Write(Geometry) → byte[]` keyed by `XxHash128.HashToUInt128` (`api-hashing`) gives a geometry a content-stable identity independent of the storage codec, so the same geometry stored as GeoPackage blob, PostGIS column, or GeoJSON text shares one content key.

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
