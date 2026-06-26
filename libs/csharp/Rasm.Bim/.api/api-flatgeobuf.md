# [RASM_BIM_API_FLATGEOBUF]

`FlatGeobuf` is the cloud-optimized streaming single-file vector codec backing the
`Semantics/geospatial#VECTOR_INGEST` long-tail: a FlatBuffers-encoded `.fgb` carrying a
Packed-Hilbert-R-tree bbox spatial index after the header so a bounding-box query reads only
the matching feature run, and a forward-streaming body so a continental dataset materializes
incrementally rather than parsed whole. It is NTS-NATIVE end to end — the consumer-facing
`FlatGeobuf.NTS` namespace serializes an NTS `FeatureCollection`/`IEnumerable<IFeature>` to
bytes and deserializes back to NTS `IFeature` rows directly (no intermediate vendor feature
type), so it stacks on the admitted `NetTopologySuite` (`api-nettopologysuite`) as a managed
codec PEER of `NetTopologySuite.IO.Esri.Shapefile`, not a GDAL/OGR driver. The
`PackedRTree.StreamSearch` random-access node fetch (a `ReadNode(offset, length) -> Stream`
delegate) is the HTTP-range / object-store streaming read leg: a remote `.fgb` is queried by
bbox over byte-range requests without downloading the whole file. The bound `lib/netstandard2.1`
asset binds under net10 and does NOT pull the `netstandard2.0`-only
`Microsoft.Bcl.AsyncInterfaces` (its async types are in-box under net10).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FlatGeobuf`
- package: `FlatGeobuf`
- version: `3.26.0`
- license: BSD-2-Clause (`flatgeobuf/flatgeobuf`; the nuspec ships a `licenseUrl`, no embedded SPDX expression)
- assembly: `FlatGeobuf` → the `net10.0` consumer binds `lib/netstandard2.1/FlatGeobuf.dll` (the higher of the two `lib/` TFMs; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset)
- namespace: `FlatGeobuf` (the FlatBuffers schema structs — `Header`/`Feature`/`Geometry`/`Column`/`Crs`/`GeometryType`/`ColumnType`), `FlatGeobuf.NTS` (the consumer-facing NTS conversion layer), `FlatGeobuf.Index` (the `PackedRTree` Hilbert spatial index), `Google.FlatBuffers` (the bundled FlatBuffers runtime)
- dependency: `NetTopologySuite` >= 2.5.0, `NetTopologySuite.Features` >= 2.1.0, `NetTopologySuite.IO.GeoJSON` >= 4.0.0, `Nito.AsyncEx` >= 5.1.2 (the `netstandard2.1` group; the central `NetTopologySuite` 2.6.0 pin wins resolution); the `netstandard2.0` group additionally pulls `Microsoft.Bcl.AsyncInterfaces` 8.0.0 — NOT pulled by the bound ns2.1 asset
- asset: emits/admits the geometry through `NetTopologySuite.Geometries.Implementation.FlatGeobufCoordinateSequence`, a `CoordinateSequence` over the FlatBuffers packed ordinate buffer (zero-copy into the NTS geometry)
- scope: streaming FGB read/write over NTS features, the Packed-Hilbert-R-tree bbox spatial filter, async enumeration, and the GeoJSON-string↔FGB direct convert
- rail: `Semantics/geospatial#VECTOR_INGEST` (the cloud-optimized streaming managed vector arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: NTS conversion layer (the consumer surface)
- namespace: `FlatGeobuf.NTS`
- rail: geospatial
- note: this is the ONLY namespace domain code touches — it exchanges the canonical NTS `IFeature`/`FeatureCollection` shape directly, so the `GeoVector` fold reads/writes FGB exactly as it reads/writes shapefile, with no vendor feature type to translate.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [RAIL]                                                            |
| :-----: | :-------------------------------- | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `FeatureCollectionConversions`    | codec entrypoint    | the static `Serialize`/`Deserialize` (+ async) family — NTS `FeatureCollection`/`IEnumerable<IFeature>` ↔ FGB bytes/stream |
|  [02]   | `FeatureConversions`              | per-feature codec   | one `IFeature` ↔ a FlatBuffers `Feature` (the row-level leg the collection fold calls) |
|  [03]   | `GeometryConversions`            | geometry codec      | NTS `Geometry` ↔ FlatBuffers geometry; `ToGeometryType(Geometry)`, `FromFlatbuf(factory, seqFactory, ref geom, header)`, `GetCoordinateSequence` |
|  [04]   | `AsyncFeatureEnumerator`          | async streaming reader | `IAsyncEnumerator<IFeature>` over a stream with `rect` bbox filter; carries `Extent`/`SRID`/`Crs`/`NumFeatures`/`Title` header metadata |
|  [05]   | `GeoJsonConversions`              | GeoJSON bridge      | static `Serialize(string geojson)`/`Deserialize(byte[]) → string` — GeoJSON-text ↔ FGB direct (no NTS round-trip) |
|  [06]   | `LayerMeta`                       | layer schema        | `Name`/`GeometryType`/`Dimensions`/`Columns` — the FGB layer header descriptor |
|  [07]   | `ColumnMeta`                      | column schema       | one attribute-column descriptor (name + `ColumnType`) the header carries |
|  [08]   | `FlatGeobufCoordinateSequence(Factory)` | packed sequence | `NetTopologySuite.Geometries.Implementation.CoordinateSequence` over the FlatBuffers ordinate buffer — the dense interop layout the NTS geometry maps onto without per-point boxing |

[PUBLIC_TYPE_SCOPE]: Packed-Hilbert-R-tree spatial index
- namespace: `FlatGeobuf.Index`
- rail: geospatial
- note: the FGB format embeds this index between the header and the feature body; `StreamSearch` is the random-access query that makes a remote `.fgb` cloud-optimized — only the index nodes and matching feature runs are fetched.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                                      |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `PackedRTree`       | spatial index   | the Packed-Hilbert-R-tree the FGB body is sorted by; the bbox filter index |
|  [02]   | `PackedRTree.StreamSearch` | range query | `(ulong numItems, ushort nodeSize, Envelope rect, ReadNode readNode) → IEnumerable<(ulong Offset, ulong Index)>` — bbox query over random-access node fetch |
|  [03]   | `PackedRTree.ReadNode` | fetch delegate | `delegate Stream ReadNode(ulong offset, ulong length)` — the byte-range / HTTP-range read the streaming query drives |
|  [04]   | `PackedRTree.CalcSize` | sizing       | `(ulong numItems, ushort nodeSize) → ulong` — the index byte size to skip to the feature body |

[PUBLIC_TYPE_SCOPE]: FlatBuffers schema structs (the on-wire layout)
- namespace: `FlatGeobuf`
- rail: geospatial
- note: these are the generated FlatBuffers accessors over the raw buffer; domain code never constructs them directly (the `NTS` layer owns the round-trip), but `GeometryType`/`ColumnType` are the discriminant enums the `LayerMeta`/`ColumnMeta` carry and the `Header` exposes the CRS + bbox + feature count.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                                                          |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------- |
|  [01]   | `Header` / `HeaderT`       | header struct      | feature count, `Envelope` bbox, `Crs`, `GeometryType`, column schema, index node size |
|  [02]   | `Feature` / `FeatureT`     | feature struct     | one encoded geometry + property buffer |
|  [03]   | `Geometry` / `GeometryT`   | geometry struct    | the encoded ordinate/part buffer the `GeometryConversions` leg decodes |
|  [04]   | `Column` / `ColumnT`       | column struct      | one attribute-column definition |
|  [05]   | `Crs` / `CrsT`             | CRS struct         | the coordinate reference system (`Code` → the NTS SRID `AsyncFeatureEnumerator.SRID` reads) |
|  [06]   | `GeometryType`             | geometry-kind enum | `enum : byte` — Point/LineString/Polygon/Multi*/GeometryCollection, the `Serialize` discriminant |
|  [07]   | `ColumnType`               | column-type enum   | `enum : byte` — the attribute scalar type (Byte/Int/Long/Double/String/…) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialize an NTS feature set to FGB
- namespace: `FlatGeobuf.NTS`
- rail: geospatial
- note: the `Serialize` family writes the header (with bbox + CRS + column schema), builds the Packed R-tree over the feature envelopes, and streams the sorted body — `geometryType` is the homogeneous-layer geometry kind, `dimensions` is 2 (XY) or 3 (XYZ), and `columns` is the explicit attribute schema (inferred from the first feature when omitted).

| [INDEX] | [SURFACE]                                                                                        | [CALL_SHAPE]                              | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------------- | :---------------------------------------- | :------------------------------------------- |
|  [01]   | `FeatureCollectionConversions.Serialize(FeatureCollection fc, GeometryType geometryType, byte dimensions = 2, IList<ColumnMeta> columns = null)` | → `byte[]` | whole-collection FGB emit |
|  [02]   | `FeatureCollectionConversions.Serialize(Stream output, IEnumerable<IFeature> features, GeometryType geometryType, byte dimensions = 2, IList<ColumnMeta> columns = null)` | → `void` | streaming FGB emit to a stream (no whole-set buffer) |
|  [03]   | `FeatureCollectionConversions.SerializeAsync(Stream output, IEnumerable<IFeature> features, GeometryType, byte, IList<ColumnMeta>)` | → `Task` | async streaming emit |
|  [04]   | `GeometryConversions.ToGeometryType(Geometry geometry)`                                          | → `GeometryType`                          | resolve the layer geometry kind from a sample geometry |
|  [05]   | `GeoJsonConversions.Serialize(string geojson)` / `SerializeAsync(string geojson)`                | → `byte[]` / `Task<byte[]>`               | GeoJSON-text → FGB direct (skips the NTS object round-trip) |

[ENTRYPOINT_SCOPE]: read FGB with a bbox spatial filter
- namespace: `FlatGeobuf.NTS`
- rail: geospatial
- note: the `Deserialize(Stream, Envelope rect)` overload is the spatial-filter read — `rect` drives the Packed R-tree so only features whose bbox overlaps are decoded, the managed-codec equivalent of the GDAL OGR `Layer.SetSpatialFilterRect` push-down; passing `null` reads every feature.

| [INDEX] | [SURFACE]                                                                | [CALL_SHAPE]                                   | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------- | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | `FeatureCollectionConversions.Deserialize(byte[] bytes)`                 | → `FeatureCollection`                          | whole-set decode from a buffer               |
|  [02]   | `FeatureCollectionConversions.Deserialize(Stream stream, Envelope rect = null)` | → `IEnumerable<IFeature>`               | streaming decode with the bbox spatial filter (`rect` = the clip envelope) |
|  [03]   | `AsyncFeatureEnumerator.Create(Stream stream, PrecisionModel pm = null, Envelope rect = null, CancellationToken? token = null)` | → `Task<AsyncFeatureEnumerator>` | open an async streaming reader with the bbox filter and the canonical precision model |
|  [04]   | `AsyncFeatureEnumerator.MoveNextAsync()` / `.Current`                    | → `ValueTask<bool>` / `IFeature`               | the forward async cursor (`await foreach`)   |
|  [05]   | `AsyncFeatureEnumerator.Extent` / `.SRID` / `.Crs` / `.NumFeatures` / `.Title` | → `Envelope` / `int` / `CrsT` / `int` / `string` | the header metadata read before iterating |
|  [06]   | `GeoJsonConversions.Deserialize(byte[] bytes)`                           | → `string`                                     | FGB → GeoJSON-text direct                     |

[ENTRYPOINT_SCOPE]: cloud-optimized streaming query (HTTP-range / object-store)
- namespace: `FlatGeobuf.Index`
- rail: geospatial
- note: this is the remote-source leg — `StreamSearch` queries the R-tree over a `ReadNode(offset, length)` delegate the consumer backs with HTTP byte-range requests (or an object-store random read), returning the `(Offset, Index)` of each matching feature so only the matching runs are fetched. The bytes the Persistence `ObjectStore.Fetch` transport supplies a whole-file `.fgb`; the range-read variant is the streaming escalation.

| [INDEX] | [SURFACE]                                                                                  | [CALL_SHAPE]                                          | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :--------------------------------------------------- | :------------------------------------------- |
|  [01]   | `PackedRTree.StreamSearch(ulong numItems, ushort nodeSize, Envelope rect, PackedRTree.ReadNode readNode)` | → `IEnumerable<(ulong Offset, ulong Index)>` | bbox query over random-access node fetch (the matching feature offsets) |
|  [02]   | `PackedRTree.CalcSize(ulong numItems, ushort nodeSize)`                                     | → `ulong`                                            | the index byte size — the offset to seek to the feature body |
|  [03]   | `PackedRTree.ReadNode` delegate                                                             | `Stream ReadNode(ulong offset, ulong length)`        | the consumer-supplied byte-range read (HTTP range / object-store seek) |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_TOPOLOGY]:
- `FlatGeobuf.NTS.FeatureCollectionConversions` is the codec root — `Serialize` writes the header (feature count, bbox `Envelope`, `Crs`, the `ColumnMeta` schema, the R-tree node size), builds the `PackedRTree` over the feature envelopes, sorts the body by Hilbert order, and streams it; `Deserialize` reads the header, optionally walks the R-tree for the `rect` filter, and yields NTS `IFeature` rows
- the geometry decodes through `GeometryConversions.FromFlatbuf` into a `Geometry` whose `CoordinateSequence` is the `FlatGeobufCoordinateSequence` over the FlatBuffers packed ordinate buffer — a zero-copy interop layout the NTS geometry wraps without per-point `Coordinate` boxing, the same density posture as the `PackedCoordinateSequenceFactory` the `GeoServices` global installs
- `GeometryType`/`ColumnType` are the discriminant enums the `LayerMeta`/`ColumnMeta` header carries; a homogeneous FGB layer is one `GeometryType` (the `ToGeometryType` of a sample geometry), and the attribute schema is the `ColumnMeta` list
- `AsyncFeatureEnumerator` exposes the header `Extent`/`SRID`/`Crs`/`NumFeatures` BEFORE iterating, so a reader knows the dataset extent and CRS without scanning the body — the metadata-first posture a streaming clip relies on
- the `PackedRTree` lives in the file between the header and the body; `StreamSearch` over a `ReadNode(offset, length)` delegate is the random-access query that fetches only the index nodes and matching feature runs — the property that makes a remote `.fgb` cloud-optimized

[INTEGRATION_STACK]:
- with `NetTopologySuite` (`api-nettopologysuite`): FGB is a MANAGED codec PEER of `NetTopologySuite.IO.Esri.Shapefile` — `Serialize`/`Deserialize` exchange the canonical NTS `IFeature`/`FeatureCollection` directly, the `FlatGeobufCoordinateSequence` is an NTS `CoordinateSequence`, and the `AsyncFeatureEnumerator.Create(stream, pm, rect, token)` seeds the canonical `PrecisionModel` — so the `GeoVector` fold reads FGB exactly as it reads shapefile, producing the `GeoFeature` row the `GEOSPATIAL_SEAM` `GeoModel` indexes, with NO GDAL/OGR dependency
- spatial-filter seam: the `Deserialize(stream, rect)` / `AsyncFeatureEnumerator.Create(…, rect, …)` `Envelope rect` is the managed bbox push-down equivalent of the GDAL OGR `Layer.SetSpatialFilterRect` — a continental `.fgb` clips to the project extent over the Packed R-tree without decoding the skipped features, so the `clip` envelope the `GeoVector.Read` carries drives the filter at the codec, not client-side after a whole-file decode
- cloud-streaming seam: the `csharp:Rasm.Persistence/Store` `ObjectStore.Fetch` transport supplies the `.fgb` bytes the whole-file `Deserialize(byte[])` decodes; the `PackedRTree.StreamSearch` range-read leg is the escalation for a remote `.fgb` too large to fetch whole — the `ReadNode` delegate backed by the object-store random read fetches only the matching runs, the streaming counterpart of the GDAL `/vsicurl/` remote read
- wire seam: `GeoJsonConversions` bridges a GeoJSON string directly to/from FGB — orthogonal to the `data-interchange#GEO_INTERCHANGE` `GeoWire` GeoJSON-text wire (which serializes the canonical `GeoFeature` through `NetTopologySuite.IO.GeoJSON4STJ`); FGB is a STORAGE/transport codec, the canonical cross-runtime wire stays the GeoJSON-text `GeoWire` and the GeoPackage blob — emitting `.fgb` on the peer wire is the rejected form

[LOCAL_ADMISSION]:
- FGB read/write enters through `FlatGeobuf.NTS.FeatureCollectionConversions` producing/consuming the canonical NTS `IFeature`; the FlatBuffers `FlatGeobuf.*` schema structs never leak past the codec (the `NTS` layer owns the round-trip) per the boundary-mapping law
- a bbox-filtered read enters through the `Deserialize(stream, rect)` / `AsyncFeatureEnumerator` `rect` argument so the Packed R-tree pushes the clip down; a whole-file decode followed by a client-side `Envelope.Intersects` filter is the rejected form when `rect` is available
- a remote-source streaming read enters through `PackedRTree.StreamSearch` over a `ReadNode` byte-range delegate; downloading a large remote `.fgb` whole when only a bbox window is needed is the rejected form
- FGB is admitted as a MANAGED `GeoVectorSource` arm (like shapefile/GeoJSON), NOT routed through the GDAL OGR universal `Ogr.Open` path — admitting GDAL for a format the managed NTS-native codec reads is the rejected form the `VECTOR_INGEST` boundary deletes; the geometry crosses to the kernel only as the `CoordinateSequence`/WKB the planar-triangulation arm consumes

[RAIL_LAW]:
- Package: `FlatGeobuf` (3.26.0, BSD-2-Clause, pure-managed `lib/netstandard2.1` AnyCPU IL binding under net10; deps `NetTopologySuite`/`NetTopologySuite.Features`/`NetTopologySuite.IO.GeoJSON`/`Nito.AsyncEx`, the central NTS 2.6.0 pin winning; `Microsoft.Bcl.AsyncInterfaces` NOT pulled by the ns2.1 bind)
- Owns: the cloud-optimized streaming FGB vector codec over NTS features — header-bbox + Packed-Hilbert-R-tree spatial index, streaming/async feature read with bbox push-down, the `FlatGeobufCoordinateSequence` packed layout, random-access range query, and the GeoJSON-text↔FGB bridge
- Accept: a `Semantics/geospatial#VECTOR_INGEST` managed FGB arm producing/consuming the canonical NTS `GeoFeature`, a bbox-filtered read via the `rect` Packed-R-tree push-down, and a remote streaming read via `PackedRTree.StreamSearch` over a byte-range `ReadNode`
- Reject: routing FGB through the GDAL OGR universal driver where the managed NTS-native codec reads it; translating through a vendor feature type where `IFeature` is exchanged directly; a client-side `Envelope.Intersects` filter after a whole-file decode where the `rect` Packed-R-tree push-down applies; downloading a whole remote `.fgb` where `StreamSearch` range-reads the bbox window; emitting `.fgb` on the cross-runtime peer wire where the GeoJSON-text `GeoWire` is the canonical geometry wire; the planar-algebra owner is `NetTopologySuite` and a boolean op inside this codec is the deleted form
