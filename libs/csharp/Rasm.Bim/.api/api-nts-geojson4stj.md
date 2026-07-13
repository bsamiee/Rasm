# [RASM_BIM_API_NTS_GEOJSON4STJ]

`NetTopologySuite.IO.GeoJSON4STJ` is the `System.Text.Json`-native GeoJSON (RFC 7946) codec
for `NetTopologySuite`. It owns a single `JsonConverterFactory` (`GeoJsonConverterFactory`)
that, once registered in `JsonSerializerOptions.Converters`, makes `Geometry`, `Feature`, and
`FeatureCollection` first-class to `JsonSerializer.Serialize`/`Deserialize` — the BBox member,
the feature `id`<->property mapping, RFC ring-orientation enforcement, and a lazy
partially-deserialized attribute table that pulls strongly-typed property bags out of feature
JSON on demand. It is the STJ GeoJSON leg of the `Semantics/georeference#GEOSPATIAL_SEAM`,
distinct from the Newtonsoft-based `NetTopologySuite.IO.GeoJSON` (`api-flatgeobuf` closure):
the design uses the STJ factory at every modern boundary (web payloads, PostGIS column
projection) and reserves the Newtonsoft codec for the FlatGeobuf dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoJSON4STJ`
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters` — the public surface is `GeoJsonConverterFactory`, `RingOrientationOption`, and the `[Obsolete]` `StjAttributesTableExtensions`; the `Stj*Converter` per-kind converters and `GeoJsonObjectType` are `internal` (produced by the factory)
- namespace: `NetTopologySuite.Features` (`IPartiallyDeserializedAttributesTable`, `JsonElementAttributesTable`, `JsonObjectAttributesTable` — the lazy read-side attribute tables)
- asset: netstandard2.0 ONLY (single TFM); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no P/Invoke, no native binaries
- dependency: `NetTopologySuite` `[,)` (the `Geometry`/`GeometryFactory`/`NtsGeometryServices` algebra), `NetTopologySuite.Features` `[,)` (the `Feature`/`FeatureCollection`/`IAttributesTable` shape), `System.Text.Json` (the consumer net10.0 in-box `System.Text.Json` binds forward)
- consumer: dual-owner central pin — `libs/csharp/Rasm.Bim` (geospatial site context + the `wire`/web GeoJSON projection) + `libs/csharp/Rasm.Persistence` (PostGIS/`Npgsql.NetTopologySuite` geometry-column to GeoJSON API response)
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory and orientation policy
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`
- rail: geospatial

`GeoJsonObjectType` and the per-kind converters (`StjGeometryConverter`, `StjFeatureConverter`, `StjFeatureCollectionConverter`, `StjAttributesTableConverter`) are `internal`, produced BY the factory; the only registration entry is `GeoJsonConverterFactory`.

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                                                        |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `GeoJsonConverterFactory` | the `JsonConverterFactory` on `JsonSerializerOptions.Converters`; hands out the per-kind converters |
|  [02]   | `RingOrientationOption`   | enum `DoNotModify` / `EnforceRfc9746` (default, GeoJSON right-hand rule) / `NtsGeoJsonV2`           |

[PUBLIC_TYPE_SCOPE]: lazy attribute-table read surface
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.Features`
- rail: geospatial

`StjAttributesTableExtensions` is declared in `NetTopologySuite.IO.Converters`, not `.Features`. The typed-pull methods `TryDeserializeJsonObject<T>` / `TryGetJsonObjectPropertyValue<T>` are `[03]`'s and `[04]`'s.

| [INDEX] | [SYMBOL]                                                   | [CAPABILITY]                                                             |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `IPartiallyDeserializedAttributesTable`                    | lazy read interface — `properties` as raw JSON, pulled typed on demand   |
|  [02]   | `JsonElementAttributesTable` · `JsonObjectAttributesTable` | concrete backing tables — `JsonElement`- vs `JsonObject`-backed          |
|  [03]   | `StjAttributesTableExtensions`                             | `[Obsolete]` `IAttributesTable` mirrors; live path is the interface cast |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: register the factory
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`
- rail: geospatial

The full ctor is `new GeoJsonConverterFactory(GeometryFactory factory, bool writeGeometryBBox, string idPropertyName, RingOrientationOption ringOrientationOption, bool allowModifyingAttributesTables)`. No-arg defaults: `factory` = `NtsGeometryServices.Instance.CreateGeometryFactory(4326)` (EPSG:4326 lon/lat), `writeGeometryBBox = false`, `idPropertyName = DefaultIdPropertyName` (`"_NetTopologySuite_id"`), `ringOrientationOption = EnforceRfc9746`, `allowModifyingAttributesTables = false`.

| [INDEX] | [ENTRYPOINT]                                   | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `new GeoJsonConverterFactory()`                | all-default factory: EPSG:4326, `EnforceRfc9746`, no BBox              |
|  [02]   | `new GeoJsonConverterFactory(GeometryFactory)` | pin SRID/precision from `NtsGeometryServices.Instance`                 |
|  [03]   | `new GeoJsonConverterFactory(…)` (full ctor)   | BBox emit, feature-`id` key, ring policy, source-table mutation toggle |
|  [04]   | `options.Converters.Add(factory)`              | the registration that makes the NTS types serializable                 |

[ENTRYPOINT_SCOPE]: serialize / deserialize through `System.Text.Json`
- package: `NetTopologySuite.IO.GeoJSON4STJ` (+ `System.Text.Json`)
- namespace: `System.Text.Json`, `NetTopologySuite.Features`
- rail: geospatial

`Serialize` takes a `Geometry`/`Feature`/`FeatureCollection`; `Deserialize`/`DeserializeAsync` take a `string`/`ReadOnlySpan<byte>`/`Stream` source; the lazy pull casts `feature.Attributes` to `IPartiallyDeserializedAttributesTable` first.

| [INDEX] | [CALL]                                                                                   | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `JsonSerializer.Serialize(value, options)` → `string`/`Utf8`                             | RFC 7946 emit with factory BBox/orientation |
|  [02]   | `JsonSerializer.Deserialize<FeatureCollection>(source, options)`                         | parse a GeoJSON document to NTS `Feature[]` |
|  [03]   | `JsonSerializer.DeserializeAsync<FeatureCollection>(Stream, options, CancellationToken)` | async UTF-8 ingest → `ValueTask`            |
|  [04]   | `TryGetJsonObjectPropertyValue<T>(string propertyName, options, out T)` → `bool`         | pull one typed property lazily              |

## [04]-[IMPLEMENTATION_LAW]

[FACTORY_REGISTRATION]:
- the entire surface routes through ONE `GeoJsonConverterFactory` added to `JsonSerializerOptions.Converters`; there is no `GeoJsonReader`/`GeoJsonWriter` pair here (that is the Newtonsoft `NetTopologySuite.IO.GeoJSON`). Configure the options object once and reuse it — `JsonSerializerOptions` is thread-safe after first use and cached by the runtime.
- seed the factory's `GeometryFactory` from `NtsGeometryServices.Instance` (`api-nettopologysuite`) so deserialized geometry carries the canonical `PackedCoordinateSequenceFactory`/`PrecisionModel`/SRID; the no-arg default uses EPSG:4326, which is correct for RFC 7946 but wrong if the pipeline works in a projected frame — pass an explicit factory then.

[RING_ORIENTATION]:
- `EnforceRfc9746` (default) rewrites polygon rings to the GeoJSON right-hand rule on write (exterior CCW, interior CW), so a `BimElement` footprint authored under an arbitrary winding still emits spec-valid GeoJSON. `DoNotModify` is the escape hatch when ring order is already canonical and the rewrite must be skipped; `NtsGeoJsonV2` reproduces the older NTS orientation for byte-compatible round-trips with NTS-v2 datasets.

[LAZY_ATTRIBUTES]:
- a deserialized `Feature.Attributes` is an `IPartiallyDeserializedAttributesTable`: the `properties` object is held as raw JSON and only the keys touched are deserialized. Cast and call `TryGetJsonObjectPropertyValue<T>` / `TryDeserializeJsonObject<T>` to lift a typed `T` (a `BimElement` property record, a nested object) directly — this avoids the `object`-boxed `AttributesTable` walk the eager path forces. The `[Obsolete]` `StjAttributesTableExtensions` mirrors are not the call; cast to the interface.
- the feature `id` member maps to/from the attribute named by `idPropertyName` (default `_NetTopologySuite_id`); set it to the domain key so a GeoJSON `id` survives the round-trip as a first-class attribute rather than a synthetic NTS key.

[STACK_INTEGRATION]:
- NTS seam: the codec serializes/parses `NetTopologySuite.Geometries.Geometry` and `NetTopologySuite.Features.Feature`/`FeatureCollection` (`api-nettopologysuite`) — a parsed feature flows straight into the planar predicate/overlay/`STRtree` algebra, and a `BimElement` footprint serializes out as RFC 7946 GeoJSON. It is the same `Feature` shape the shapefile (`api-nts-esri-shapefile`), GeoPackage (`api-nts-geopackage`), and GeoParquet (`api-gisblox-geoparquet`) codecs exchange.
- STJ-pipeline seam: the factory composes with the rest of the `System.Text.Json` boundary — the same `JsonSerializerOptions` that carries the `Thinktecture` value-object/smart-enum converters (`api-thinktecture-json`) also carries `GeoJsonConverterFactory`, so a DTO with a `Geometry` member and a `[ValueObject]` member serializes in one pass. This is the integration the Newtonsoft GeoJSON codec cannot offer.
- Persistence seam: the dual-consumer pin lets the Persistence owner project an `Npgsql.NetTopologySuite` PostGIS geometry column into a GeoJSON API response through the same factory the Bim owner uses for site context — one converter, both rails.
- reprojection seam: GeoJSON is CRS84 (lon/lat) by spec; a projected-frame geometry is reprojected through the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg (`api-projnet`) to EPSG:4326 BEFORE serialization, and the parsed lon/lat is reprojected into the project frame after — the codec never transforms coordinates.
- identity seam: a serialized GeoJSON string (UTF-8 bytes) feeds `System.IO.Hashing` — `XxHash3` for the fast in-process fingerprint, `XxHash128` for the collision-resistant persisted content key (`api-hashing`) — joining the same content-identity rail (the `Rasm.Persistence` artifact index) as the other interchange exports.

[LOCAL_ADMISSION]:
- the GeoJSON boundary enters through a composition-built `JsonSerializerOptions` carrying one `GeoJsonConverterFactory` seeded from `NtsGeometryServices.Instance`; serialize/parse is plain `JsonSerializer`.
- typed attribute access enters through a cast to `IPartiallyDeserializedAttributesTable`; the rejected forms are the Newtonsoft codec for a modern boundary, the `[Obsolete]` extension methods, and the default EPSG:4326 factory when the pipeline is in a projected frame.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoJSON4STJ`
- Owns: the `System.Text.Json`-native RFC 7946 GeoJSON converter factory for NTS `Geometry`/`Feature`/`FeatureCollection`, ring-orientation enforcement, feature-`id` mapping, optional BBox emit, and the lazy partially-deserialized attribute table
- Accept: STJ GeoJSON serialize/parse, async streaming GeoJSON, lazy typed attribute extraction, single-pipeline composition with the other STJ converters
- Reject: the geometry algebra (`NetTopologySuite` owns it), Newtonsoft-based GeoJSON (`NetTopologySuite.IO.GeoJSON` owns it, in the FlatGeobuf closure), datum/projection transformation (`ProjNET`/OSR own it), and the non-GeoJSON vector container formats (their own NTS IO codec packages own them)
