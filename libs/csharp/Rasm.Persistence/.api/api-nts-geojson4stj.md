# [RASM_PERSISTENCE_API_NTS_GEOJSON4STJ]

`NetTopologySuite.IO.GeoJSON4STJ` owns GeoJSON text over `System.Text.Json` on the spatial-values boundary: one `GeoJsonConverterFactory` admits the whole conversion family onto a `JsonSerializerOptions`, and feature attributes reify through the partially-deserialized table contract. It decodes to the one NTS `Geometry` currency under the same precision-and-SRID configuration the core WKB and WKT codecs carry, so a value crossing GeoJSON, WKB, and the GeoPackage blob keeps one grid. It serves the `Ingest/geospatial#FEATURE_ROWS` `GeoContainer`/`GeoWire` seam; `Rasm.Bim/.api/api-nts-geojson4stj.md` is the peer partition serving the `Semantics/geospatial#GEOSPATIAL_SEAM` codec leg, one catalogue per package on both tiers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`, `NetTopologySuite.Features`
- depends: `NetTopologySuite`, `System.Text.Json`
- abi: netstandard2.0
- rail: spatial-values

- Registers `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the `Geometry` model this codec decodes to, the `WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter` core codecs, and `NtsGeometryServices` with its `PrecisionModel` and `Ordinates` policy all resolve there.

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GeoJsonConverterFactory` owns the whole conversion family — `Geometry` and its concrete subtypes, `IFeature`, `FeatureCollection`, `IAttributesTable` — with per-type converters internal and reached only through `CreateConverter`; its factory defaults `GeometryFactory` SRID to 4326 and polygon rings to `RingOrientationOption.EnforceRfc9746` (RFC 7946 exterior counter-clockwise).
- Deserialization yields a read-only `JsonElementAttributesTable` unless `allowModifyingAttributesTables` admits `JsonObjectAttributesTable`, and an attribute named `DefaultIdPropertyName` lifts to the feature `id` rather than `properties`.

[STACKING]:
- `api-nts-geopackage`(`.api/api-nts-geopackage.md`): the sibling satellite on the same rail — both decode to one `Geometry` under one `NtsGeometryServices` precision-and-SRID configuration, so a value surviving a GeoJSON to WKB to GeoPackage round trip keeps one grid.
- `api-nts-ef`(`.api/api-nts-ef.md`), `api-npgsql`(`.api/api-npgsql.md`): a PostGIS geometry column persists as binary and the SAME `Geometry` serializes to GeoJSON at the web egress boundary through these converters on the shared `JsonSerializerOptions` — the rail stays provider-agnostic at the `Geometry` boundary and codec-specific only at the wire.
- `api-hashing`(`../../.api/api-hashing.md`): `WKBWriter.Write(Geometry) -> byte[]` keyed by `XxHash128.HashToUInt128` gives a geometry one content-stable identity across GeoJSON text, the GeoPackage blob, and the PostGIS column.
- within-lib: the factory adds to the same `JsonSerializerOptions` carrying the document's other converters, and `TryDeserializeJsonObject<T>(options, out _)` reifies feature properties under that one converter graph.

[LOCAL_ADMISSION]:
- GeoJSON conversion enters only through `GeoJsonConverterFactory` on serializer options.
- Typed attribute access casts to `IPartiallyDeserializedAttributesTable` and calls its instance methods.
- Raw WKB and WKT IO takes the branch core codecs directly; a satellite package never re-wraps them.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoJSON4STJ`
- Owns: GeoJSON text interchange for NetTopologySuite spatial values on the `Ingest/geospatial#FEATURE_ROWS` seam
- Accept: NetTopologySuite geometry and feature contracts on the spatial-values rail
- Reject: hand-rolled GeoJSON shaping, a folder-local re-tabling of the branch core codecs, and the GeoPackage blob layout the sibling satellite owns
