# [RASM_PERSISTENCE_API_NTS_IO]

`NetTopologySuite.IO.GeoJSON4STJ` and `NetTopologySuite.IO.GeoPackage` own the two satellite wire formats on the spatial-values boundary: GeoJSON text over `System.Text.Json`, and the GeoPackage geometry blob — a GPB header over a WKB body — for SQLite stores. Each decodes its format to one NTS `Geometry` currency and binds the same precision-and-SRID configuration the core WKB and WKT codecs carry, so a value crossing all three keeps one grid.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`, `NetTopologySuite.Features`
- depends: `NetTopologySuite`, `System.Text.Json`
- abi: netstandard2.0
- rail: spatial-values

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- depends: `NetTopologySuite`
- abi: netstandard2.0
- rail: spatial-values

- Registers `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the `Geometry` model both satellites decode to, the `WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter` core codecs, and `NtsGeometryServices` with its `PrecisionModel` and `Ordinates` policy all resolve there.

## [02]-[PUBLIC_TYPES]

[CONVERTER_TYPES]: STJ GeoJSON converter admission

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------ | :------------ | :------------------------------------------ |
|  [01]   | `GeoJsonConverterFactory` | class         | admits every GeoJSON converter onto options |
|  [02]   | `RingOrientationOption`   | enum          | selects polygon ring orientation on write   |

[RingOrientationOption]: `DoNotModify` `EnforceRfc9746` `NtsGeoJsonV2`

[ATTRIBUTE_TYPES]: feature attribute projection (namespace `NetTopologySuite.IO.Converters`)

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `IPartiallyDeserializedAttributesTable` | interface     | typed table/property reify over table contract   |
|  [02]   | `JsonElementAttributesTable`            | class         | read-only `JsonElement` adapter                  |
|  [03]   | `JsonObjectAttributesTable`             | class         | mutable `JsonObject` adapter with `Add`/`Delete` |

[GEOPACKAGE_TYPES]: GeoPackage geometry blob codec

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :-------------------- | :------------ | :------------------------------- |
|  [01]   | `GeoPackageGeoReader` | class         | decodes a GPB blob to `Geometry` |
|  [02]   | `GeoPackageGeoWriter` | class         | encodes `Geometry` to a GPB blob |

## [03]-[ENTRYPOINTS]

[GEOJSON_ADMISSION]: GeoJSON serializer admission

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `GeoJsonConverterFactory(GeometryFactory, bool, string, RingOrientationOption, bool)` | ctor     | carries GeoJSON converter policy   |
|  [02]   | `JsonSerializerOptions.Converters.Add`                                                | instance | admits the converters onto options |
|  [03]   | `DefaultIdPropertyName`                                                               | static   | names the feature `id` attribute   |

[ATTRIBUTE_PROJECTION]: `IPartiallyDeserializedAttributesTable` typed reify, cast from `IAttributesTable`

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `TryDeserializeJsonObject<T>(JsonSerializerOptions, out T) -> bool`              | instance | reifies the whole table to a typed value |
|  [02]   | `TryGetJsonObjectPropertyValue<T>(string, JsonSerializerOptions, out T) -> bool` | instance | reifies one property to a typed value    |

[GEOPACKAGE_CODEC]: GeoPackage blob codec

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `GeoPackageGeoReader.Read(byte[]\|Stream) -> Geometry` | instance | decodes a GeoPackage blob      |
|  [02]   | `GeoPackageGeoWriter.Write(Geometry) -> byte[]`        | instance | encodes a `Geometry` to a blob |

- Reader/writer policy: `HandleOrdinates` caps ordinates within `AllowedOrdinates`, `HandleSRID` stamps the header SRID, `RepairRings` fixes invalid rings on read.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GeoJsonConverterFactory` owns the whole conversion family — `Geometry` and its concrete subtypes, `IFeature`, `FeatureCollection`, `IAttributesTable` — with per-type converters internal and reached only through `CreateConverter`; its factory defaults `GeometryFactory` SRID to 4326 and polygon rings to `RingOrientationOption.EnforceRfc9746` (RFC 7946 exterior counter-clockwise).
- GeoJSON deserialization yields read-only `JsonElementAttributesTable` unless `allowModifyingAttributesTables` admits `JsonObjectAttributesTable`, and an attribute named `DefaultIdPropertyName` lifts to the feature `id` rather than `properties`.
- GeoPackage blob layout is the GPB header (magic `GP`, version, endianness/envelope-kind/emptiness flags, SRID, envelope, Z/M ranges) followed by a WKB body; `HandleOrdinates` restricts written ordinates within XYZM and selects the header envelope kind, empty points encode as NaN ordinates, and `HandleSRID` stamps the header SRID onto the decoded geometry.
- GeoPackage reuses the core WKB body under its own header, so the blob's ordinate set is the intersection of the writer's `HandleOrdinates` and the branch codec's `AllowedOrdinates` factory cap; a store column widened past that cap silently drops the extra dimension.

[STACKING]:
- `api-nts-ef`(`api-nts-ef.md`), `api-npgsql`(`api-npgsql.md`): a PostGIS geometry column round-trips through Npgsql's NTS plugin and a SQLite GeoPackage column through `GeoPackageGeoReader`/`GeoPackageGeoWriter`, both producing one NTS `Geometry` — the spatial-values rail stays provider-agnostic at the `Geometry` boundary and codec-specific only at the wire.
- `api-hashing`(`../../.api/api-hashing.md`): `WKBWriter.Write(Geometry) -> byte[]` keyed by `XxHash128.HashToUInt128` gives a geometry one content-stable identity across GeoPackage blob, PostGIS column, and GeoJSON text.
- within-lib: the four codecs bind one `NtsGeometryServices`/`GeometryFactory` precision-and-SRID configuration, so a geometry surviving a GeoJSON to WKB to GeoPackage round trip keeps one precision grid; `GeoJsonConverterFactory` adds to the same `JsonSerializerOptions` carrying the document's other converters, and `TryDeserializeJsonObject<T>(options, out _)` reifies feature properties under that one converter graph.

[LOCAL_ADMISSION]:
- GeoJSON conversion enters only through `GeoJsonConverterFactory` on serializer options.
- GeoPackage geometry columns pass through `GeoPackageGeoReader`/`GeoPackageGeoWriter`.
- Typed attribute access casts to `IPartiallyDeserializedAttributesTable` and calls its instance methods.
- Raw WKB and WKT IO takes the branch core codecs directly; a satellite package never re-wraps them.

[RAIL_LAW]:
- Packages: `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`
- Owns: GeoJSON text interchange and GeoPackage blob coding for NetTopologySuite spatial values
- Accept: NetTopologySuite geometry and feature contracts on the spatial-values rail
- Reject: hand-rolled GeoJSON shaping, raw WKB columns standing in for GeoPackage geometry blobs, WKT strings parsed via `string.Split`, and a folder-local re-tabling of the branch core codecs
