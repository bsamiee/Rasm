# [RASM_BIM_API_NTS_GEOJSON4STJ]

`NetTopologySuite.IO.GeoJSON4STJ` is the `System.Text.Json`-native GeoJSON (RFC 7946) codec for `NetTopologySuite`: one `GeoJsonConverterFactory` on `JsonSerializerOptions.Converters` makes `Geometry`, `Feature`, and `FeatureCollection` first-class to `JsonSerializer`, carrying BBox emit, feature-`id`↔property mapping, ring-orientation enforcement, and a lazy attribute table that pulls typed property bags out of feature JSON on demand.

This factory is the STJ GeoJSON leg of the geospatial seam, distinct from the Newtonsoft `NetTopologySuite.IO.GeoJSON` (`api-flatgeobuf` closure): it holds every web-payload and PostGIS-projection boundary, the Newtonsoft codec serving only the FlatGeobuf dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters` — public `GeoJsonConverterFactory` and `RingOrientationOption`; the `Stj*Converter` per-kind converters and `GeoJsonObjectType` are `internal`, produced by the factory
- namespace: `NetTopologySuite.Features` — the lazy read-side tables `IPartiallyDeserializedAttributesTable`, `JsonElementAttributesTable`, `JsonObjectAttributesTable`
- asset: netstandard2.0 single TFM, bound by the net10.0 consumer; IL-only AnyCPU managed assembly, no P/Invoke
- dependency: `NetTopologySuite` (the `Geometry`/`GeometryFactory`/`NtsGeometryServices` algebra), `NetTopologySuite.Features` (the `Feature`/`FeatureCollection`/`IAttributesTable` shape), `System.Text.Json` (in-box under net10.0)
- consumer: `libs/csharp/Rasm.Bim` (site context, web GeoJSON projection) and `libs/csharp/Rasm.Persistence` (PostGIS geometry-column to GeoJSON API response)
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory and orientation policy

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `GeoJsonConverterFactory` | class         | `JsonConverterFactory` on `JsonSerializerOptions.Converters`; mints per-kind converters |
|  [02]   | `RingOrientationOption`   | enum          | `DoNotModify` / `EnforceRfc9746` (default, GeoJSON right-hand rule) / `NtsGeoJsonV2`    |

[PUBLIC_TYPE_SCOPE]: lazy attribute-table read surface

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `IPartiallyDeserializedAttributesTable` | interface     | lazy read — `properties` held as raw JSON, pulled typed on demand   |
|  [02]   | `JsonElementAttributesTable`            | class         | `JsonElement`-backed, read-only; number properties box as `decimal` |
|  [03]   | `JsonObjectAttributesTable`             | class         | `JsonObject`-backed, mutable; edits propagate to `RootObject`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: register the factory

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------------------------- |
|  [01]   | `new GeoJsonConverterFactory()`                   | ctor     | all-default factory: EPSG:4326, `EnforceRfc9746`, no BBox       |
|  [02]   | `new GeoJsonConverterFactory(GeometryFactory)`    | ctor     | pin SRID/precision from `NtsGeometryServices.Instance`          |
|  [03]   | `new GeoJsonConverterFactory(GeometryFactory, …)` | ctor     | full ctor: BBox, feature-`id`, ring policy, source-table toggle |
|  [04]   | `options.Converters.Add(factory)`                 | instance | the registration that makes the NTS types serializable          |

- `new GeoJsonConverterFactory()`: `factory` = `NtsGeometryServices.Instance.CreateGeometryFactory(4326)`, `writeGeometryBBox` = false, `idPropertyName` = `"_NetTopologySuite_id"`, `ringOrientationOption` = `EnforceRfc9746`, `allowModifyingAttributesTables` = false.

[ENTRYPOINT_SCOPE]: serialize / deserialize through `System.Text.Json`

Every serialize/parse call rides `JsonSerializer`: `Serialize` takes a `Geometry`/`Feature`/`FeatureCollection`, `Deserialize`/`DeserializeAsync` a `string`/`ReadOnlySpan<byte>`/`Stream`; a typed pull first casts `feature.Attributes` to `IPartiallyDeserializedAttributesTable`, whose instance methods lift the typed value.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `Serialize(value, options) -> string/Utf8`                                | static   | RFC 7946 emit with factory BBox/orientation |
|  [02]   | `Deserialize<FeatureCollection>(source, options)`                         | static   | parse a GeoJSON document to NTS `Feature[]` |
|  [03]   | `DeserializeAsync<FeatureCollection>(Stream, options, CancellationToken)` | static   | async UTF-8 ingest → `ValueTask`            |
|  [04]   | `TryGetJsonObjectPropertyValue<T>(string, options, out T) -> bool`        | instance | pull one typed property lazily              |
|  [05]   | `TryDeserializeJsonObject<T>(options, out T) -> bool`                     | instance | lift the whole table to a typed `T`         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `GeoJsonConverterFactory` on `JsonSerializerOptions.Converters` routes the entire surface, no `GeoJsonReader`/`GeoJsonWriter` pair (that is the Newtonsoft `NetTopologySuite.IO.GeoJSON`); configure the options object once and reuse it, `JsonSerializerOptions` being thread-safe after first use.
- seed the factory's `GeometryFactory` from `NtsGeometryServices.Instance` so deserialized geometry carries the canonical `PackedCoordinateSequenceFactory`/`PrecisionModel`/SRID; the no-arg EPSG:4326 default is RFC 7946-correct but wrong in a projected frame, so pass an explicit factory there.
- `EnforceRfc9746` (default) rewrites polygon rings to the GeoJSON right-hand rule on write (exterior CCW, interior CW), so a footprint authored under arbitrary winding still emits spec-valid GeoJSON; `DoNotModify` skips the rewrite when winding is already canonical; `NtsGeoJsonV2` reproduces the v2 orientation for byte-compatible round-trips with NTS-v2 datasets.
- a deserialized `Feature.Attributes` is an `IPartiallyDeserializedAttributesTable` holding `properties` as raw JSON with only touched keys deserialized; cast and call `TryGetJsonObjectPropertyValue<T>`/`TryDeserializeJsonObject<T>` to lift a typed `T` directly, skipping the `object`-boxed eager `AttributesTable` walk.
- feature `id` maps to/from the attribute named by `idPropertyName` (default `_NetTopologySuite_id`); set it to the domain key so a GeoJSON `id` survives the round-trip as a first-class attribute rather than a synthetic NTS key.

[STACKING]:
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the codec serializes/parses `Geometry` and `Feature`/`FeatureCollection`, so a parsed feature flows straight into the planar predicate/overlay/`STRtree` algebra and a footprint emits as RFC 7946 GeoJSON — the same `Feature` shape the shapefile (`api-nts-esri-shapefile`), GeoPackage (`api-nts-geopackage`), and GeoParquet (`api-gisblox-geoparquet`) codecs exchange.
- `Thinktecture.Json`(`libs/csharp/.api/api-thinktecture-json.md`): one `JsonSerializerOptions` carrying both the value-object/smart-enum converters and `GeoJsonConverterFactory` serializes a DTO with a `Geometry` member and a `[ValueObject]` member in one pass, the composition the Newtonsoft codec cannot offer.
- `ProjNET`(`api-projnet`): GeoJSON is CRS84 lon/lat by spec, so a projected-frame geometry reprojects to EPSG:4326 through the `ProjNET` geodetic transform BEFORE serialization and back to the project frame after — the codec never transforms coordinates.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): a serialized GeoJSON UTF-8 string feeds `XxHash3` for the in-process fingerprint and `XxHash128` for the persisted content key, joining the `Rasm.Persistence` artifact-index content-identity rail with the other interchange exports.
- `Rasm.Persistence`: this two-owner pin projects an `Npgsql.NetTopologySuite` PostGIS geometry column into a GeoJSON API response through the same factory `Rasm.Bim` uses for site context — one converter, both rails.

[LOCAL_ADMISSION]:
- GeoJSON entry binds a composition-built `JsonSerializerOptions` carrying one `GeoJsonConverterFactory` seeded from `NtsGeometryServices.Instance`; serialize/parse is plain `JsonSerializer`, and typed attribute access is a cast to `IPartiallyDeserializedAttributesTable`.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoJSON4STJ`
- Owns: the `System.Text.Json`-native RFC 7946 GeoJSON converter factory for NTS `Geometry`/`Feature`/`FeatureCollection` — ring-orientation enforcement, feature-`id` mapping, optional BBox emit, and the lazy partially-deserialized attribute table
- Accept: STJ GeoJSON serialize/parse, async streaming ingest, lazy typed attribute extraction, single-pipeline composition with the other STJ converters
- Reject: the geometry algebra (`NetTopologySuite` owns it), Newtonsoft-based GeoJSON (`NetTopologySuite.IO.GeoJSON`, the FlatGeobuf closure), datum/projection transformation (`ProjNET`/OSR own it), and the non-GeoJSON vector container formats (their own NTS IO codec packages own them)
