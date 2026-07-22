# [RASM_BIM_API_FLATGEOBUF]

`FlatGeobuf` owns the cloud-optimized streaming vector codec over `NetTopologySuite` features: a FlatBuffers `.fgb` carrying a Packed-Hilbert-R-tree bbox index after the header, so a bounding-box query decodes only the matching feature run and a continental dataset materializes incrementally. It reads and writes the canonical NTS `IFeature`/`FeatureCollection` directly, standing as a managed codec peer of the shapefile leg with no GDAL/OGR dependency; the planar algebra, the datum transform, and the cross-runtime GeoJSON wire stay with their own owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FlatGeobuf`
- package: `FlatGeobuf` (BSD-2-Clause)
- assembly: `FlatGeobuf` — the net10 consumer binds `lib/netstandard2.1`, pure-managed AnyCPU IL, ALC-safe, no per-RID native asset
- namespace: `FlatGeobuf.NTS` (the consumer-facing NTS conversion layer domain code touches)
- namespace: `FlatGeobuf.Index` (the `PackedRTree` Hilbert spatial index)
- namespace: `FlatGeobuf` (the generated FlatBuffers schema structs), `Google.FlatBuffers` (bundled runtime)
- depends: `NetTopologySuite` (`Feature`/`Geometry` algebra), `NetTopologySuite.Features`, `NetTopologySuite.IO.GeoJSON`
- rail: geospatial (the cloud-optimized streaming managed vector arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: NTS conversion layer — the only namespace domain code touches, exchanging the canonical NTS `IFeature`/`FeatureCollection` directly

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `FeatureCollectionConversions`          | static class  | NTS features ↔ FGB bytes/stream, sync and async               |
|  [02]   | `FeatureConversions`                    | static class  | one `IFeature` ↔ FlatBuffers `Feature`; `FromByteBuffer` leg  |
|  [03]   | `GeometryConversions`                   | static class  | NTS `Geometry` ↔ FlatBuffers ordinate/part buffer             |
|  [04]   | `AsyncFeatureEnumerator`                | class         | `IAsyncEnumerator<IFeature>` over a stream, `rect` filtered   |
|  [05]   | `GeoJsonConversions`                    | static class  | GeoJSON-text ↔ FGB direct                                     |
|  [06]   | `LayerMeta`                             | class         | `Name`/`GeometryType`/`Dimensions`/`Columns` layer header     |
|  [07]   | `ColumnMeta`                            | class         | attribute-column descriptor over `ColumnType`                 |
|  [08]   | `FlatGeobufCoordinateSequence(Factory)` | class         | NTS `CoordinateSequence` over the FlatBuffers ordinate buffer |

[PUBLIC_TYPE_SCOPE]: Packed-Hilbert-R-tree spatial index seated between the header and the feature body

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :--------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `PackedRTree`          | class         | the Hilbert R-tree the body sorts by; the bbox filter index      |
|  [02]   | `PackedRTree.ReadNode` | delegate      | `Stream (ulong offset, ulong length)` — consumer byte-range read |

[PUBLIC_TYPE_SCOPE]: FlatBuffers schema structs — the on-wire layout the `NTS` layer round-trips, never constructed by domain code

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `Header` / `HeaderT`     | struct        | feature count, `Envelope` bbox, `Crs`, `GeometryType`, columns, node size       |
|  [02]   | `Feature` / `FeatureT`   | struct        | one encoded geometry and property buffer                                        |
|  [03]   | `Geometry` / `GeometryT` | struct        | the encoded ordinate/part buffer `GeometryConversions` decodes                  |
|  [04]   | `Column` / `ColumnT`     | struct        | one attribute-column definition                                                 |
|  [05]   | `Crs` / `CrsT`           | struct        | coordinate reference system (`Code` → NTS SRID)                                 |
|  [06]   | `GeometryType`           | enum          | `enum: byte` — `Unknown`(=0)/Point/LineString/Polygon/Multi*/GeometryCollection |
|  [07]   | `ColumnType`             | enum          | `enum: byte` — attribute scalar type (Bool/Byte/Int/Long/Double/String/…)       |
|  [08]   | `Helpers`                | static class  | `ReadHeader`/`GetEnvelope`/`GetCrsCode` — the header decode leg                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialize an NTS feature set to FGB

| [INDEX] | [SURFACE]                                                                              | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `FeatureCollectionConversions.Serialize(FeatureCollection, GeometryType, …) -> byte[]` | static  | whole-collection FGB emit           |
|  [02]   | `FeatureCollectionConversions.Serialize(Stream, IEnumerable<IFeature>, …) -> void`     | static  | streaming emit, no whole-set buffer |
|  [03]   | `FeatureCollectionConversions.SerializeAsync(Stream, …) -> Task`                       | static  | async streaming emit                |
|  [04]   | `GeometryConversions.ToGeometryType(Geometry) -> GeometryType`                         | static  | resolve the homogeneous layer kind  |
|  [05]   | `GeoJsonConversions.Serialize(string) -> byte[]` / `SerializeAsync -> Task<byte[]>`    | static  | GeoJSON-text → FGB direct           |

- `Serialize`: `dimensions` is 2 (XY) or 3 (XYZ); `columns` is inferred from the first feature's attributes when omitted.

[ENTRYPOINT_SCOPE]: read FGB with a bbox spatial filter

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `FeatureCollectionConversions.Deserialize(byte[]) -> FeatureCollection`                | static   | whole-set decode from a buffer    |
|  [02]   | `FeatureCollectionConversions.Deserialize(Stream, Envelope?) -> IEnumerable<IFeature>` | static   | streaming decode, bbox push-down  |
|  [03]   | `AsyncFeatureEnumerator.Create(Stream, PrecisionModel?, Envelope?, …) -> Task<…>`      | static   | open the async reader (bbox + pm) |
|  [04]   | `AsyncFeatureEnumerator.MoveNextAsync() -> ValueTask<bool>` / `.Current -> IFeature`   | instance | the forward async cursor          |
|  [05]   | `AsyncFeatureEnumerator.Extent/SRID/Crs/NumFeatures/Title`                             | property | header metadata before iterating  |
|  [06]   | `GeoJsonConversions.Deserialize(byte[]) -> string`                                     | static   | FGB → GeoJSON-text direct         |

- `Deserialize(Stream, rect)`: a `null` `rect` reads every feature; a non-null `rect` drives the Packed R-tree so only overlapping features decode.

[ENTRYPOINT_SCOPE]: cloud-optimized streaming query over HTTP-range / object-store random reads

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `PackedRTree.StreamSearch(ulong, ushort, Envelope, ReadNode) -> IEnumerable<…>` | static   | bbox query over random-access fetch   |
|  [02]   | `PackedRTree.CalcSize(ulong, ushort) -> ulong`                                  | static   | index byte size to seek the body      |
|  [03]   | `PackedRTree.ReadNode`                                                          | delegate | consumer byte-range / HTTP-range read |
|  [04]   | `Helpers.ReadHeader(Stream, out int) -> Header`                                 | static   | decode the header off one head fetch  |
|  [05]   | `FeatureConversions.FromByteBuffer(…) -> IFeature`                              | static   | lower one prefix-stripped record      |

- `StreamSearch` yields `IEnumerable<(ulong Offset, ulong Index)>` hits, offsets index-relative and re-based onto `12 + headerSize`; `FromByteBuffer(GeometryFactory, FlatGeobufCoordinateSequenceFactory, ByteBuffer, HeaderT)` lowers each size-prefixed body record prefix-stripped.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `FeatureCollectionConversions` is the codec root: `Serialize` writes the header (feature count, bbox `Envelope`, `Crs`, `ColumnMeta` schema, R-tree node size), builds the `PackedRTree` over the feature envelopes, sorts the body by Hilbert order, and streams it; `Deserialize` reads the header, walks the R-tree for a `rect` filter, and yields NTS `IFeature` rows.
- Geometry decodes through `GeometryConversions.FromFlatbuf` into a `Geometry` whose `CoordinateSequence` is the `FlatGeobufCoordinateSequence` over the FlatBuffers packed ordinate buffer — a zero-copy layout the NTS geometry wraps without per-point `Coordinate` boxing.
- `GeometryType`/`ColumnType` are the discriminant enums the `LayerMeta`/`ColumnMeta` header carries: a homogeneous layer is one `GeometryType` (the `ToGeometryType` of a sample geometry), the attribute schema one `ColumnMeta` list.
- `AsyncFeatureEnumerator` exposes `Extent`/`SRID`/`Crs`/`NumFeatures` before iterating, so a reader knows extent and CRS without scanning the body — the metadata-first posture a streaming clip relies on.
- `PackedRTree` seats between the header and the body; `Helpers.ReadHeader(stream, out headerSize)` consumes magic, size prefix, and blob so the index starts at `12 + headerSize`, and `StreamSearch` over a `ReadNode(offset, length)` delegate fetches only the index nodes and matching feature runs — the cloud-optimized property. Each body hit reads its 4-byte record length and hands the prefix-stripped record to `FeatureConversions.FromByteBuffer`.

[STACKING]:
- `api-nettopologysuite`: FGB exchanges the canonical NTS `IFeature`/`FeatureCollection` directly and the `FlatGeobufCoordinateSequence` is an NTS `CoordinateSequence`, so the `GeoVector` fold reads FGB exactly as `NetTopologySuite.IO.Esri.Shapefile` reads a shapefile, producing the `GeoFeature` row the `GEOSPATIAL_SEAM` `GeoModel` indexes.
- `api-nts-esri-shapefile`: managed codec peer — `Deserialize(stream, rect)` / `AsyncFeatureEnumerator.Create(…, rect, …)` is the `Envelope` bbox push-down that clips a continental `.fgb` to the project extent over the Packed R-tree, no whole-file decode and no client-side filter.
- `csharp:Rasm.Persistence/Store`: `ObjectStore.Fetch` supplies the `.fgb` bytes `Deserialize(byte[])` decodes; `PackedRTree.StreamSearch` over an object-store `ReadNode` random read is the escalation for a remote `.fgb` too large to fetch whole, `Helpers.ReadHeader` decoding the head fetch and `FromByteBuffer` lowering each matched record.
- `data-interchange#GEO_INTERCHANGE`: `GeoJsonConversions` bridges a GeoJSON string to and from FGB, orthogonal to the canonical GeoJSON-text `GeoWire` — FGB is the storage/transport codec, the cross-runtime geometry wire stays `GeoWire`.

[LOCAL_ADMISSION]:
- FGB read/write enters through `FlatGeobuf.NTS.FeatureCollectionConversions` producing and consuming the canonical NTS `IFeature`; the FlatBuffers `FlatGeobuf.*` schema structs stay behind the codec under the boundary-mapping law.
- A bbox-filtered read enters through the `Deserialize(stream, rect)` / `AsyncFeatureEnumerator` `rect` argument so the Packed R-tree pushes the clip down at the codec.
- A remote-source streaming read enters through `PackedRTree.StreamSearch` over a `ReadNode` byte-range delegate.
- FGB is admitted as a managed `GeoVectorSource` arm alongside shapefile and GeoJSON; geometry crosses to the kernel only as the `CoordinateSequence`/WKB the planar-triangulation arm consumes.

[RAIL_LAW]:
- Package: `FlatGeobuf`
- Owns: the cloud-optimized streaming FGB vector codec over NTS features — header-bbox and Packed-Hilbert-R-tree index, streaming/async read with bbox push-down, the `FlatGeobufCoordinateSequence` packed layout, random-access range query, and the GeoJSON-text↔FGB bridge.
- Accept: a `Semantics/geospatial#VECTOR_INGEST` managed FGB arm over the canonical NTS `GeoFeature`, a `rect` Packed-R-tree bbox read, and a `StreamSearch` remote streaming read over a byte-range `ReadNode`.
- Reject: routing FGB through the GDAL OGR driver where the managed NTS-native codec reads it; a vendor feature type where `IFeature` exchanges directly; a client-side `Envelope.Intersects` filter after a whole-file decode where the `rect` push-down applies; downloading a whole remote `.fgb` where `StreamSearch` range-reads the window; emitting `.fgb` on the cross-runtime wire where `GeoWire` is canonical; a boolean op inside this codec where `NetTopologySuite` owns the planar algebra.
