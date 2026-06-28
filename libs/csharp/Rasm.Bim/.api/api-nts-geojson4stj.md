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
- version: `4.0.0`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters` (`GeoJsonConverterFactory`, `RingOrientationOption`, the `Stj*Converter` family)
- namespace: `NetTopologySuite.Features` (`IPartiallyDeserializedAttributesTable`, `JsonElementAttributesTable`, `JsonObjectAttributesTable` — the lazy read-side attribute tables)
- asset: netstandard2.0 ONLY (single TFM); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no P/Invoke, no native binaries
- dependency: `NetTopologySuite` `[2.0.0, 3.0.0)` (the `Geometry`/`GeometryFactory`/`NtsGeometryServices` algebra), `NetTopologySuite.Features` `[2.1.0, 3.0.0)` (the `Feature`/`FeatureCollection`/`IAttributesTable` shape), `System.Text.Json` `6.0.3` (the consumer net10.0 in-box `System.Text.Json` binds forward)
- consumer: dual-owner central pin — `libs/csharp/Rasm.Bim` (geospatial site context + the `wire`/web GeoJSON projection) + `libs/csharp/Rasm.Persistence` (PostGIS/`Npgsql.NetTopologySuite` geometry-column to GeoJSON API response)
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory and orientation policy
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`
- rail: geospatial

`GeoJsonObjectType` is `internal` (the codec's own type discriminant) — NOT a public surface.
The per-kind converters (`StjGeometryConverter`, `StjFeatureConverter`,
`StjFeatureCollectionConverter`, `StjAttributesTableConverter`) are produced BY the factory;
the only registration entry is `GeoJsonConverterFactory`.

| [INDEX] | [SYMBOL]                    | [RAIL]     | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------- | :--------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `GeoJsonConverterFactory`   | geospatial | `JsonConverterFactory` (override `CanConvert`/`CreateConverter`); the single object added to `JsonSerializerOptions.Converters`. It hands out the `Geometry`/`Feature`/`FeatureCollection`/`AttributesTable` converters. `static readonly DefaultIdPropertyName = "_NetTopologySuite_id"` is the attribute key the feature `id` round-trips through |
|  [02]   | `RingOrientationOption`     | geospatial | enum: `DoNotModify` (pass polygon rings through unchanged), `EnforceRfc9746` (the default — enforce the GeoJSON right-hand rule: exterior ring CCW, holes CW), `NtsGeoJsonV2` (the legacy NTS-v2 orientation). The literal member name is `EnforceRfc9746` |

[PUBLIC_TYPE_SCOPE]: lazy attribute-table read surface
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.Features`
- rail: geospatial

| [INDEX] | [SYMBOL]                                | [RAIL]     | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------------------- | :--------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `IPartiallyDeserializedAttributesTable` | geospatial | `IAttributesTable` whose values stay as raw JSON until pulled typed. `TryDeserializeJsonObject<T>(JsonSerializerOptions, out T)` deserializes the whole `properties` object to `T`; `TryGetJsonObjectPropertyValue<T>(string propertyName, JsonSerializerOptions, out T)` deserializes one nested property — the form to lift a `BimElement` property bag out of a feature without an `object`-boxed intermediate |
|  [02]   | `JsonElementAttributesTable` / `JsonObjectAttributesTable` | geospatial | the two concrete `IPartiallyDeserializedAttributesTable` instances a deserialized `Feature.Attributes` carries (`JsonElement`-backed vs `JsonObject`-backed depending on the read mode); cast `feature.Attributes` to the interface to reach the typed-pull methods |
|  [03]   | `StjAttributesTableExtensions`          | geospatial | `[Obsolete]` static mirrors of the two interface methods on `IAttributesTable`; the live path is a direct cast to `IPartiallyDeserializedAttributesTable` + the instance call — the extensions exist only for source compatibility and must not be the documented call |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: register the factory
- package: `NetTopologySuite.IO.GeoJSON4STJ`
- namespace: `NetTopologySuite.IO.Converters`
- rail: geospatial

The factory carries the overload ladder so the design fixes the `GeometryFactory` (SRID/
precision), the BBox policy, the `id` property name, the ring orientation, and the attribute-
mutation policy once at composition. Defaults (the no-arg ctor): factory =
`NtsGeometryServices.Instance.CreateGeometryFactory(4326)` (EPSG:4326 lon/lat, the GeoJSON-
mandated CRS), `writeGeometryBBox = false`, `idPropertyName = DefaultIdPropertyName`,
`ringOrientationOption = EnforceRfc9746`, `allowModifyingAttributesTables = false`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new GeoJsonConverterFactory`   | `()`                                                                                                                     | the all-default factory (EPSG:4326, RFC orientation, no BBox) |
|  [02]   | `new GeoJsonConverterFactory`   | `(GeometryFactory factory)`                                                                                              | pin the SRID/precision — seed from `NtsGeometryServices.Instance` so parsed geometry matches the rest of the algebra |
|  [03]   | `new GeoJsonConverterFactory`   | `(GeometryFactory, bool writeGeometryBBox, string idPropertyName, RingOrientationOption ringOrientationOption, bool allowModifyingAttributesTables)` | the full ctor: BBox emit on each geometry, the feature-`id` attribute key, ring policy, and whether deserialization may mutate the source `AttributesTable` |
|  [04]   | `options.Converters.Add(factory)` | `JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory(...))`                                                | the registration that makes the NTS types serializable; all I/O after this is plain `JsonSerializer` |

[ENTRYPOINT_SCOPE]: serialize / deserialize through `System.Text.Json`
- package: `NetTopologySuite.IO.GeoJSON4STJ` (+ `System.Text.Json`)
- namespace: `System.Text.Json`, `NetTopologySuite.Features`
- rail: geospatial

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                                                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `JsonSerializer.Serialize`                  | `(Geometry \| Feature \| FeatureCollection, options)` → `string`/`Utf8`      | emit RFC 7946 GeoJSON (geometry, feature, or feature collection) with the factory's BBox/orientation policy |
|  [02]   | `JsonSerializer.Deserialize<FeatureCollection>` | `(string \| ReadOnlySpan<byte> \| Stream, options)` → `FeatureCollection`    | parse a GeoJSON document into NTS `Feature[]`; each `Feature.Geometry` is built with the factory's `GeometryFactory` |
|  [03]   | `JsonSerializer.DeserializeAsync<FeatureCollection>` | `(Stream, options, CancellationToken)` → `ValueTask<FeatureCollection>`      | the async streaming mirror over a UTF-8 stream (the web/fsspec ingest) |
|  [04]   | `((IPartiallyDeserializedAttributesTable)feature.Attributes).TryGetJsonObjectPropertyValue<T>` | `(string propertyName, options, out T)` → `bool`                             | lazily pull one typed property from a deserialized feature without materializing every attribute |

## [04]-[IMPLEMENTATION_LAW]

[FACTORY_REGISTRATION]:
- the entire surface routes through ONE `GeoJsonConverterFactory` added to `JsonSerializerOptions.Converters`; there is no `GeoJsonReader`/`GeoJsonWriter` pair here (that is the Newtonsoft `NetTopologySuite.IO.GeoJSON`). Configure the options object once and reuse it — `JsonSerializerOptions` is thread-safe after first use and cached by the runtime.
- seed the factory's `GeometryFactory` from `NtsGeometryServices.Instance` (`api-nettopologysuite`) so deserialized geometry carries the canonical `PackedCoordinateSequenceFactory`/`PrecisionModel`/SRID; the no-arg default uses EPSG:4326, which is correct for RFC 7946 but wrong if the pipeline works in a projected frame — pass an explicit factory then.

[RING_ORIENTATION]:
- `EnforceRfc9746` (default) rewrites polygon rings to the GeoJSON right-hand rule on write (exterior CCW, interior CW), so a `BimElement` footprint authored under an arbitrary winding still emits spec-valid GeoJSON. `DoNotModify` is the escape hatch when ring order is already canonical and the rewrite must be skipped; `NtsGeoJsonV2` reproduces the older NTS orientation for byte-compatible round-trips with legacy datasets.

[LAZY_ATTRIBUTES]:
- a deserialized `Feature.Attributes` is an `IPartiallyDeserializedAttributesTable`: the `properties` object is held as raw JSON and only the keys touched are deserialized. Cast and call `TryGetJsonObjectPropertyValue<T>` / `TryDeserializeJsonObject<T>` to lift a typed `T` (a `BimElement` property record, a nested object) directly — this avoids the `object`-boxed `AttributesTable` walk the eager path forces. The `[Obsolete]` `StjAttributesTableExtensions` mirrors are not the call; cast to the interface.
- the feature `id` member maps to/from the attribute named by `idPropertyName` (default `_NetTopologySuite_id`); set it to the domain key so a GeoJSON `id` survives the round-trip as a first-class attribute rather than a synthetic NTS key.

[STACK_INTEGRATION]:
- NTS seam: the codec serializes/parses `NetTopologySuite.Geometries.Geometry` and `NetTopologySuite.Features.Feature`/`FeatureCollection` (`api-nettopologysuite`) — a parsed feature flows straight into the planar predicate/overlay/`STRtree` algebra, and a `BimElement` footprint serializes out as RFC 7946 GeoJSON. It is the same `Feature` shape the shapefile (`api-nts-esri-shapefile`), GeoPackage (`api-nts-geopackage`), and GeoParquet (`api-gisblox-geoparquet`) codecs exchange.
- STJ-pipeline seam: the factory composes with the rest of the `System.Text.Json` boundary — the same `JsonSerializerOptions` that carries the `Thinktecture` value-object/smart-enum converters (`api-thinktecture-json`) also carries `GeoJsonConverterFactory`, so a DTO with a `Geometry` member and a `[ValueObject]` member serializes in one pass. This is the integration the Newtonsoft GeoJSON codec cannot offer.
- Persistence seam: the dual-consumer pin lets the Persistence owner project an `Npgsql.NetTopologySuite` PostGIS geometry column into a GeoJSON API response through the same factory the Bim owner uses for site context — one converter, both rails.
- reprojection seam: GeoJSON is CRS84 (lon/lat) by spec; a projected-frame geometry is reprojected through the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg (`api-projnet`) to EPSG:4326 BEFORE serialization, and the parsed lon/lat is reprojected into the project frame after — the codec never transforms coordinates.
- identity seam: a serialized GeoJSON string (UTF-8 bytes) feeds `System.IO.Hashing` `XxHash3` (`api-hashing`) for the content key, joining the same content-identity rail as the other interchange exports.

[LOCAL_ADMISSION]:
- the GeoJSON boundary enters through a composition-built `JsonSerializerOptions` carrying one `GeoJsonConverterFactory` seeded from `NtsGeometryServices.Instance`; serialize/parse is plain `JsonSerializer`.
- typed attribute access enters through a cast to `IPartiallyDeserializedAttributesTable`; the rejected forms are the Newtonsoft codec for a modern boundary, the `[Obsolete]` extension methods, and the default EPSG:4326 factory when the pipeline is in a projected frame.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoJSON4STJ`
- Owns: the `System.Text.Json`-native RFC 7946 GeoJSON converter factory for NTS `Geometry`/`Feature`/`FeatureCollection`, ring-orientation enforcement, feature-`id` mapping, optional BBox emit, and the lazy partially-deserialized attribute table
- Accept: STJ GeoJSON serialize/parse, async streaming GeoJSON, lazy typed attribute extraction, single-pipeline composition with the other STJ converters
- Reject: the geometry algebra (`NetTopologySuite` owns it), Newtonsoft-based GeoJSON (`NetTopologySuite.IO.GeoJSON` owns it, in the FlatGeobuf closure), datum/projection transformation (`ProjNET`/OSR own it), and the non-GeoJSON vector container formats (their own NTS IO codec packages own them)
