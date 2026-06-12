# [RASM_PERSISTENCE_API_NTS_IO]

`NetTopologySuite.IO.GeoJSON4STJ` converts NetTopologySuite geometries, features,
and attribute tables through `System.Text.Json`, and `NetTopologySuite.IO.GeoPackage`
codecs the GeoPackage geometry blob (GPB header plus WKB body) for SQLite-backed
GeoPackage stores.

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

## [2]-[PUBLIC_TYPES]

[CONVERTER_TYPES]: STJ GeoJSON converter admission
- rail: spatial-values

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :----------------- | :-------------------------------------------------- |
|   [1]   | `GeoJsonConverterFactory`      | converter factory  | admits all GeoJSON converters on STJ options        |
|   [2]   | `RingOrientationOption`        | orientation policy | selects polygon ring orientation on write           |
|   [3]   | `StjAttributesTableExtensions` | obsolete extension | forwards to `IPartiallyDeserializedAttributesTable` |

[ATTRIBUTE_TYPES]: feature attribute projection
- rail: spatial-values

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]     | [CAPABILITY]                                     |
| :-----: | :-------------------------------------- | :----------------- | :----------------------------------------------- |
|   [1]   | `IPartiallyDeserializedAttributesTable` | attribute contract | deserializes table or property to typed values   |
|   [2]   | `JsonElementAttributesTable`            | read-only adapter  | adapts `JsonElement` as `IAttributesTable`       |
|   [3]   | `JsonObjectAttributesTable`             | mutable adapter    | adapts `JsonObject` with `Add`/`DeleteAttribute` |

[GEOPACKAGE_TYPES]: GeoPackage geometry blob codec
- rail: spatial-values

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE] | [CAPABILITY]                                  |
| :-----: | :-------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `GeoPackageGeoReader` | blob decoder   | reads GPB header plus WKB body to `Geometry`  |
|   [2]   | `GeoPackageGeoWriter` | blob encoder   | writes `Geometry` to GPB header plus WKB body |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GeoJSON serializer admission
- rail: spatial-values

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]        | [CAPABILITY]                                                                        |
| :-----: | :---------------------------------------------- | :------------------ | :---------------------------------------------------------------------------------- |
|   [1]   | `new GeoJsonConverterFactory(...)`              | factory constructor | carries `GeometryFactory`, bbox, id, ring, mutability policy                        |
|   [2]   | `JsonSerializerOptions.Converters.Add`          | options admission   | enables GeoJSON for `Geometry`, `IFeature`, `FeatureCollection`, `IAttributesTable` |
|   [3]   | `GeoJsonConverterFactory.DefaultIdPropertyName` | policy constant     | names the attribute carrying a feature `id` (`_NetTopologySuite_id`)                |

[ENTRYPOINT_SCOPE]: attribute value projection
- rail: spatial-values

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]    | [CAPABILITY]                                |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------------------ |
|   [1]   | `TryDeserializeJsonObject<T>`                 | contract method | converts a whole table to a typed CLR value |
|   [2]   | `TryGetJsonObjectPropertyValue<T>`            | contract method | converts one property to a typed CLR value  |
|   [3]   | `GetOptionalValue` / `GetNames` / `GetValues` | table read      | reads loosely typed attribute values        |

[ENTRYPOINT_SCOPE]: GeoPackage blob codec
- rail: spatial-values

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]       | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------- | :----------------- | :------------------------------------------------ |
|   [1]   | `GeoPackageGeoReader.Read`                       | byte[] or `Stream` | decodes a GeoPackage blob to `Geometry`           |
|   [2]   | `GeoPackageGeoWriter.Write`                      | byte[] or `Stream` | encodes a `Geometry` to a GeoPackage blob         |
|   [3]   | `HandleOrdinates` / `HandleSRID` / `RepairRings` | codec policy       | caps ordinates, stamps header SRID, repairs rings |

## [4]-[IMPLEMENTATION_LAW]

[GEOJSON_PROFILE]:
- profile: one converter factory owns the full GeoJSON conversion family; per-type converters are internal and reached only through `CreateConverter`
- convertible set: `Geometry` and its seven concrete subtypes, `IFeature` implementations, `FeatureCollection`, `IAttributesTable`
- geometry policy: default `GeometryFactory` carries SRID 4326; polygon rings default to `RingOrientationOption.EnforceRfc9746` (RFC 7946 exterior counter-clockwise)
- attribute policy: deserialization yields read-only `JsonElementAttributesTable` unless `allowModifyingAttributesTables` admits `JsonObjectAttributesTable`
- id policy: an attribute named `DefaultIdPropertyName` lifts to the feature `id` instead of `properties`

[GEOPACKAGE_PROFILE]:
- profile: blob layout is the GPB header (magic `GP`, version, flags for endianness, envelope kind, and emptiness, SRID, envelope) followed by a little-endian WKB body
- writer policy: `HandleOrdinates` restricts written ordinates within XYZM and selects the header envelope kind; empty points encode as NaN ordinates
- reader policy: `HandleSRID` stamps the header SRID onto the decoded geometry; NaN-coded empty points decode to an empty `Point`

[LOCAL_ADMISSION]:
- GeoJSON conversion enters only through `GeoJsonConverterFactory` on serializer options; never instantiate `Stj*` converters.
- GeoPackage geometry columns pass through `GeoPackageGeoReader`/`GeoPackageGeoWriter`; raw WKB readers do not understand the GPB header.
- Typed attribute access casts to `IPartiallyDeserializedAttributesTable`; the static extension methods are obsolete forwarders.

[RAIL_LAW]:
- Packages: `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`
- Own: GeoJSON text interchange and GeoPackage blob coding for NetTopologySuite spatial values
- Accept: NetTopologySuite geometry and feature contracts per the spatial-values rail
- Reject: hand-rolled GeoJSON shaping or raw WKB columns standing in for GeoPackage geometry blobs
