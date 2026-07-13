# [RASM_BIM_API_THEMIS_LAS]

`Themis.Las` is the managed ASPRS LAS point-cloud reader/writer backing the scan-to-BIM ingest seam. It streams every ASPRS LAS point data record format (0-10) into a `LasPoint` carrying position, intensity, ASPRS classification, return index/count, scan angle, GPS time, RGB + near-infrared, and the full waveform packet, and exposes the LAS public header (scale/offset, bounding extrema, point counts by return) plus the VLR/EVLR record surface that carries the CRS WKT/GeoTIFF georeferencing. It is pure-managed (`Themis.Las.dll`, AnyCPU IL) over a single native-free dependency — `MathNet.Numerics`, the same dense-linear-algebra owner `csharp:Rasm.Compute/Tensor/blas` carries — so a decoded `LasPoint.Position` is already a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and the Compute substrate consume without a copy. It decodes/encodes UNCOMPRESSED LAS only; the `.laz` compressed leg is the admitted separate-assembly `Unofficial.laszip.netstandard` peer (`api-laszip`, LGPL-2.1, pure-managed, never ILMerged), and the `reconstruct#LAS_INGEST` `LasIngest.Decode` fold composes the two as ONE dual-engine ingest front dispatched once by `LasCompression.Sniff` (the public-header point-data-format byte at offset 104, high bit = LASzip), this reader owning the uncompressed leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Themis.Las`
- package: `Themis.Las` (single assembly, version, direct pin)
- license: MIT (`Clodge-Scientific/Themis`)
- assembly: `Themis.Las` → the `net10.0` consumer binds `lib/net8.0/Themis.Las.dll` (the only `lib/` TFM; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset)
- namespace: `Themis.Las`, `Themis.Las.Structs`, `Themis.Las.Builders`, `Themis.Las.Stream`, `Themis.Las.Time`
- transitive: `MathNet.Numerics` (the package floor; the central `csharp:Rasm.Compute` pin wins resolution) — `LasPoint.Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>`, so the point geometry rides the Compute MathNet substrate directly
- scope: ASPRS LAS, point data record formats 0-10; UNCOMPRESSED `.las` only — the `.laz` compressed leg is the `Unofficial.laszip.netstandard` peer (`api-laszip`)
- rail: `reconstruct#LAS_INGEST` (the uncompressed leg of the dual-engine scan-to-BIM ingest front)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream codec roots
- rail: reconstruct#LAS_INGEST
- note: each codec root ships a concrete class plus its `I*` interface (`LasReader`/`ILasReader`, `LasWriter`/`ILasWriter`, `LasHeader`/`ILasHeader`, `LasVariableLengthRecord`/`ILasVariableLengthRecord`; `Stream.AsyncStreamHandler`/`IStreamHandler`, `Stream.AsyncStreamBuffer`/`IStreamBuffer`)

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [CAPABILITY]                                                                   |
| :-----: | :-------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `LasReader`                 | streaming reader | `IDisposable` forward point reader over a LAS stream                           |
|  [02]   | `LasWriter`                 | streaming writer | `IDisposable` point writer authoring a LAS file                                |
|  [03]   | `LasHeader`                 | header value     | the ASPRS public header block (scale/offset/extrema)                           |
|  [04]   | `LasVariableLengthRecord`   | VLR record       | header-resident VLR/EVLR carrying CRS WKT, GeoTIFF keys, classification lookup |
|  [05]   | `Stream.AsyncStreamHandler` | stream handler   | the buffered LAS stream the reader/writer drives — PATH-BOUND admission        |
|  [06]   | `Stream.AsyncStreamBuffer`  | point buffer     | the bounded point staging buffer (`IStreamBuffer`) behind the handler          |

- [04]-[VLR]: `RecordID` (`ushort`; `LasVariableLengthRecord.ProjectionRecordID` const `2112` = the OGC WKT CRS record) + `Data` (`byte[]` payload, UTF-8 WKT for record `2112`), plus `UserID`/`Description`/`RecordLengthAfterHeader`.
- [05]-[STREAM_HANDLER]: `AsyncStreamHandler` is the ONLY shipped `IStreamHandler` impl and constructs from `(string, uint)` only, so byte admission is PATH-BOUND (one temp path); `new LasReader(IStreamHandler)` is the stream growth seam.

[PUBLIC_TYPE_SCOPE]: point value model
- rail: reconstruct#LAS_INGEST
- note: `LasPoint` implements the full facet interface set; each interface names one ASPRS field group, so a consumer that needs only position/classification depends on the narrow facet rather than the concrete record. Two decode-correctness laws bind every consumer: the reader's ref-fill `Update` MUTATES the ONE `Position` vector in place, so a collected position detaches via `Clone()` (a bare `Position` read aliases every slot onto the last decoded point); and `Classification` surfaces the RAW on-disk byte, so formats 0-5 mask `& 0x1F` off the synthetic/key-point/withheld flag bits (bits 5-7) while formats 6-10 carry a full dedicated class byte.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                                                           |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `LasPoint`      | point rec     | the decoded point — implements every facet below                                                       |
|  [02]   | `IPosition`     | position      | `Vector<double> Position` + `X`/`Y`/`Z`; collect `Position.Clone()` per slot                           |
|  [03]   | `ILasPointBase` | core fields   | `Classification`, `Intensity`, `ScanAngle`, `FlightLine`, `UserData`, `GlobalEncoding`                 |
|  [04]   | `ILasTime`      | GPS-time      | `double Timestamp` (GPS adjusted-standard or week seconds)                                             |
|  [05]   | `ILasRgb`       | color         | `R`/`G`/`B` (formats 2/3/5/7/8/10)                                                                     |
|  [06]   | `ILas4Band`     | NIR           | `ILasRgb` + `NIR` near-infrared band (formats 8/10)                                                    |
|  [07]   | `ILasWaveform`  | waveform      | `WavePacketDescriptorIndex`/`ByteOffsetToWaveformData`/`ReturnPointWaveformLocation`/`X_t`/`Y_t`/`Z_t` |
|  [08]   | `ILasPoint`     | composite     | the union facet over `IPosition`+base+time+rgb+4band+waveform                                          |

[PUBLIC_TYPE_SCOPE]: point data record structs and format map
- rail: reconstruct#LAS_INGEST
- note: `Structs.LasPointRecordFormat0..10` are `[StructLayout]` blittable records matching the eleven ASPRS on-disk layouts; `PointTypeMap` is the byte↔`Type` dispatch table the reader resolves on, never an enumerated switch.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Structs.LasPointRecordFormat0..10` | on-disk record   | the eleven blittable ASPRS point data record formats                         |
|  [02]   | `Structs.LasPointConverter`         | record converter | marshals an on-disk record ↔ `LasPoint`                                      |
|  [03]   | `Structs.FieldUpdater`              | field projector  | copies a facet group between records on format change                        |
|  [04]   | `PointTypeMap`                      | format table     | `TypeByPointDataFormat`/`PointDataFormatByType`/`SizeByType` dispatch maps   |
|  [05]   | `Builders.LasHeaderBuilder`         | fluent builder   | composes an `ILasHeader` (`IFluentBuilder<ILasHeader>`, `ILasHeaderBuilder`) |
|  [06]   | `Time.GpsTime` / `Time.LeapSeconds` | GPS-time helper  | LAS GPS timestamp ↔ UTC `DateTime` with leap-second tables                   |
|  [07]   | `LasHelper` / `Extensions`          | header helper    | `GetGlobalEncoding(useGpsStandardTime, useProjWkt)` bit composition          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: forward point read
- rail: reconstruct#LAS_INGEST
- note: `LasReader` is the streaming front of the uncompressed ingest leg; the `ref` overload fills ONE caller-owned `LasPoint` so a tight decode loop allocates no per-point garbage — and because its `Update` mutates the one `Position` vector in place, the loop collects `point.Position.Clone()` per slot. Byte admission span-writes one `try/finally`-scoped temp path: the sole shipped `IStreamHandler` (`AsyncStreamHandler`) is path-ctor'd.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new LasReader(string lasFilePath, uint pointsToBuffer = 250000)` | ctor            | opens a LAS file, reads/validates the header     |
|  [02]   | `new LasReader(IStreamHandler stream)`                            | ctor            | stream growth seam; no shipped handler           |
|  [03]   | `LasPoint GetNextPoint()`                                         | read (alloc)    | decodes/returns a new point (per-point alloc)    |
|  [04]   | `void GetNextPoint(ref LasPoint lpt)`                             | read (no-alloc) | fills caller point in place; `Position.Clone()`  |
|  [05]   | `bool EOF` / `ulong PointCount`                                   | cursor          | end-of-stream flag and total point count         |
|  [06]   | `ILasHeader Header` / `IList<LasVariableLengthRecord> VLRs`       | metadata        | the public header and the VLR/EVLR set (CRS WKT) |
|  [07]   | `void Dispose()`                                                  | lifetime        | releases the underlying stream handle            |

[ENTRYPOINT_SCOPE]: point write
- rail: reconstruct#LAS_INGEST (the symmetric uncompressed emit — no current fold composes it; the LAZ-compressed emit is the `api-laszip` writer leg)
- note: `LasWriter` authors a LAS file; `Initialize` writes the header, `WritePoints` streams the body, the header is composed through `LasHeaderBuilder`.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `new LasWriter(string, ILasHeader, IEnumerable<LasVariableLengthRecord>?)` | ctor           | output file, header, VLR set            |
|  [02]   | `ILasWriter Initialize()`                                                  | open           | writes header + VLRs, positions at body |
|  [03]   | `WritePoint(LasPoint)` / `WritePoints(IEnumerable<LasPoint>)`              | write          | one or a sequence of points             |
|  [04]   | `ulong PointsWritten` / `long Position` / `string OutputFile`              | cursor         | write progress, byte position, path     |

[ENTRYPOINT_SCOPE]: header build and field model
- rail: reconstruct#LAS_INGEST
- note: `LasHeaderBuilder` is the fluent `IFluentBuilder<ILasHeader>`; its `Set*` methods (the builder prefix elided below) compose the header the writer emits, `ILasHeader` exposes the matching read surface, and `LasHelper.GetGlobalEncoding` composes the global-encoding bit field

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `SetVersion(byte major, byte minor)` / `SetPointDataFormat(byte)`       | header build   | LAS version and point data record format      |
|  [02]   | `SetScale(double x, y, z)` / `SetOrigin(double x, y, z)`                | header build   | scale/offset for integer↔double position      |
|  [03]   | `SetMinima(double,double,double)` / `SetMaxima(double,double,double)`   | header build   | bounding-box extrema                          |
|  [04]   | `SetPointCount(ulong)` / `SetCreationDate(ushort year, ushort doy)`     | header build   | point count and creation date                 |
|  [05]   | `ILasHeader` read surface                                               | header read    | public header value surface; `LasCloud` facts |
|  [06]   | `ILasHeader.CheckExtrema(IEnumerable<double>)` / `SetScale(ILasHeader)` | header derive  | recompute extrema / copy scale                |
|  [07]   | `LasHelper.GetGlobalEncoding(bool, bool)`                               | bit compose    | GPS-standard-time + WKT-CRS bits              |
|  [08]   | `GpsTime.Parse(double timestamp)` (via `LasPoint.DateTime`)             | time decode    | LAS GPS timestamp → UTC `DateTime`            |

- [05]-[HEADER_READ]: `ScaleX`/`OriginX`/`MinX`/`MaxX`/…, `PointDataFormat`, `PointCount`, `NumPointRecordsByReturn` (`ulong[]`) with the `LegacyNumPointRecordsByReturn` (`uint[]`) pair.

## [04]-[IMPLEMENTATION_LAW]

[POINT_TOPOLOGY]:
- `LasPoint` is the one decoded point carrier implementing `ILasPoint: IPosition, ILasPointBase, ILasTime, ILasRgb, ILas4Band, ILasWaveform`; `Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>` (`X`/`Y`/`Z` project its components) and `DateTime` lifts `Timestamp` through `GpsTime.Parse`
- the on-disk format is the eleven `Structs.LasPointRecordFormat0..10` blittable records; `PointTypeMap.TypeByPointDataFormat`/`PointDataFormatByType`/`SizeByType` is the byte↔`Type`↔record-length dispatch table the reader resolves on (`Marshal.SizeOf<T>` per format), so a format is one table row, never a switch arm
- `LasPointConverter` marshals an on-disk record ↔ `LasPoint`; `FieldUpdater` copies a facet group when re-encoding to a different format
- the header (`ILasHeader`) carries LAS version, point data format, scale/offset, bounding extrema, standard + extended point counts (by return), waveform-packet and extended-VLR offsets; `LasHeaderBuilder` is the fluent `IFluentBuilder<ILasHeader>` the writer composes a header through
- `LasReader.GetNextPoint(ref LasPoint)` is the no-allocation decode path; the buffered `Stream.AsyncStreamHandler`/`AsyncStreamBuffer` stage the body for the forward cursor
- IN-PLACE MUTATION law: the ref-fill `Update` writes the scale+offset XYZ onto the SAME `Position` vector instance every iteration, so a bulk loop collects `point.Position.Clone()` — a bare `point.Position` read aliases every collected slot onto the last decoded point, collapsing the cloud to N copies of one coordinate
- RAW CLASSIFICATION law: `Classification` passes the on-disk byte through unmasked — the format-0-5 records pack synthetic/key-point/withheld flags into bits 5-7 (a withheld ground point reads `130`, not `2`), so a format-0-5 consumer masks `& 0x1F` (the exact mask the laszip format-0-5 getter applies internally) while formats 6-10 carry a full dedicated class byte
- VLR/EVLR records (`LasVariableLengthRecord`) carry the OGC WKT / GeoTIFF CRS keys and the classification lookup; the georeferencing leg reads them, not a re-minted CRS parser

[INTEGRATION_STACK]:
- ingest front: `reconstruct#LAS_INGEST` `LasIngest.Decode` composes this reader as the UNCOMPRESSED leg of the dual-engine front — `LasCompression.Sniff` selects the engine once (Themis for `.las`, the `api-laszip` `laszip` arithmetic decoder for `.laz`), `ReadLas` folds the no-alloc `GetNextPoint(ref LasPoint)` loop into the one `LasCloud` carrier (positions detached via `Position.Clone()`, classes flag-masked `& 0x1F` below format 6), and the decoded cloud feeds the kernel `csharp:Rasm/Processing/register#ALIGN` cloud-ICP registration and the `csharp:Rasm/Spatial/cloud#SEGMENTATION` plane/cylinder segmentation, which produce the `reconstruct#RECONSTRUCTION` `SegmentedCloud` rows — Themis owns the decode, the kernel owns the fit, and no scan engine is re-minted in Bim
- MathNet seam: `LasPoint.Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>` over the same `MathNet.Numerics` owner `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA` carries, so a point batch crosses into the Compute dense-LA substrate (covariance/PCA normal estimation, ICP transform) with no vector re-wrap — each collected instance a `Clone()` off the reader's one mutating vector
- content identity: the source-cloud bytes content-key as the `reconstruct#RECONSTRUCTION` `ReconstructionLineage` `[ValueObject<UInt128>]` — the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` composed through the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter`, never a second hashing scheme over the LAS bytes and never the upper-stratum `Rasm.Compute` `InterchangeIdentity` (a `Rasm.Bim`→`Rasm.Compute` reference inverts the strata DAG — the leak the reconstruct rebuild closed)
- classification seed: `LasPoint.Classification` (the RAW ASPRS byte, flag-masked `& 0x1F` below format 6 at the ingest) folds into the `LasCloud.ClassHistogram` and reduces to the `SegmentedCloud.DominantClass` the `reconstruct#RECONSTRUCTION` `AsprsBias` policy and `ElementClassifier` `(PrimitiveShape, IfcDomain, orientation)` table key on — a ground/building/vegetation ASPRS class biases the fitted-primitive-to-`IfcClass` projection, never an enumerated per-class branch
- georeference: the LAS header scale/offset and the `ProjectionRecordID` (2112) CRS WKT VLR land on `LasCloud.CrsWkt`, which the app lowers onto the `Header.Reference` `GeoReference` through the `csharp:Rasm.Bim/Semantics/georeference#GEO_PROJECTION` `ProjNET` datum leg — wiring is app-owned, and a Themis-local reprojection is the rejected form
- time: the capture `Instant` rides `reconstruct#LAS_INGEST` `LasCloud.At` (`NodaTime`-typed, supplied at `LasIngest.Decode`); the per-point LAS GPS `Timestamp` decodes to a `DateTime` through `GpsTime`/`LeapSeconds` when a temporal scan slice is queried

[LOCAL_ADMISSION]:
- `LasReader`/`LasWriter` are the only LAS codec roots; one streaming reader decodes every point data record format by `PointTypeMap` row, never a per-format `LasReaderFormat3`/`LasReaderFormat6` family
- the decoded point is the one `LasPoint`; a consumer narrows to the facet it needs (`IPosition` for geometry, `ILasPointBase` for classification, `ILasTime` for temporal) rather than a parallel point struct, and `LasPoint.Position` enters the kernel as the MathNet vector it already is
- byte admission is PATH-BOUND: the ingest span-writes the raw bytes to ONE `try/finally`-scoped temp path because the sole shipped `IStreamHandler` (`AsyncStreamHandler`) constructs from `(string, uint)` only; `new LasReader(IStreamHandler)` is the package-watch stream seam, and a hand-implemented `IStreamHandler` re-minting the LAS decode is the rejected form
- the bulk decode collects `Position.Clone()` per slot and flag-masks `Classification & 0x1F` below format 6 — the aliased bare-`Position` collection and the unmasked format-0-5 class byte are the two named decode-correctness defects
- the header is composed through `LasHeaderBuilder` and read through `ILasHeader`; a hand-rolled LAS header byte layout beside the builder is the rejected form
- LAS GPS time, CRS WKT VLR, and ASPRS classification are read from the package surface (`GpsTime`, `LasVariableLengthRecord`, `LasPoint.Classification`) and threaded onto the canonical owners (`georeference#GEO_PROJECTION`, `reconstruct#RECONSTRUCTION`), never re-derived
- LAZ is decoded by the PEER, never here — `LasIngest.Decode` routes a `LasCompression.Sniff`-detected compressed input onto the admitted separate-assembly `Unofficial.laszip.netstandard` leg (`api-laszip`, LGPL-2.1, pure-managed); a hand-rolled LAZ arithmetic decoder inside this leg or a NATIVE LASzip binding stays rejected (the ALC firebreak holds — both engines are pure-managed IL)

[RAIL_LAW]:
- Package: `Themis.Las` (, MIT, pure-managed `lib/net8.0` AnyCPU IL, `MathNet.Numerics` sole dependency)
- Owns: streaming uncompressed ASPRS LAS read/write (point data record formats 0-10), the `LasPoint` facet model, the LAS public header + VLR/EVLR surface, and the GPS-time conversion — the UNCOMPRESSED leg of the `reconstruct#LAS_INGEST` dual-engine front
- Accept: a raw LAS byte stream admitted through one scoped temp path (the sole shipped `IStreamHandler` is path-ctor'd) and decoded by the no-alloc `GetNextPoint(ref LasPoint)` loop into `Position.Clone()`-detached MathNet vectors and `& 0x1F`-masked format-0-5 classes the kernel registration and the Compute substrate consume, the source-cloud bytes content-keyed as the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` through the seam `CanonicalWriter` (`ReconstructionLineage`), and the ASPRS classification/CRS-WKT threaded onto `reconstruct#RECONSTRUCTION`/`georeference#GEO_PROJECTION`
- Reject: a re-minted point-cloud scan/segmentation/registration engine (the kernel owns it); a LAZ decode in this leg (the compressed leg is the admitted `Unofficial.laszip.netstandard` peer — `api-laszip`) or a native LASzip binding; a hand-implemented `IStreamHandler` beside the path admission; a bare `point.Position` collection (every slot aliases the last decoded point) or an unmasked format-0-5 classification byte; a second hashing scheme over the LAS bytes or the upper-stratum `Rasm.Compute` `InterchangeIdentity` beside the kernel `ContentHash`/seam `CanonicalWriter` lineage; a Themis-local CRS reprojection beside `ProjNET`; a parallel point struct or per-format reader family where one `LasPoint` and one `LasReader` discriminate by `PointTypeMap` row
