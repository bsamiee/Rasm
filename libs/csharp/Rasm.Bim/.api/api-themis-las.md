# [RASM_BIM_API_THEMIS_LAS]

`Themis.Las` owns managed uncompressed ASPRS LAS decode/encode — every point data record format (0-10) streamed into a `LasPoint` facet carrier over a `MathNet.Numerics` `Vector<double>` position, with the public header and the CRS-WKT-carrying VLR/EVLR surface. It decodes UNCOMPRESSED `.las` only; the `.laz` compressed leg is the separate-assembly `Unofficial.laszip.netstandard` peer (`api-laszip`), and `reconstruct#LAS_INGEST` `LasIngest.Decode` folds the two as one dual-engine ingest front dispatched once by `LasCompression.Sniff`, this reader owning the uncompressed leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Themis.Las`
- package: `Themis.Las` (MIT, `Clodge-Scientific/Themis`)
- assembly: `Themis.Las` → the `net10.0` consumer binds `lib/net8.0/Themis.Las.dll` (sole `lib/` TFM; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset)
- namespace: `Themis.Las`, `Themis.Las.Structs`, `Themis.Las.Builders`, `Themis.Las.Stream`, `Themis.Las.Time`
- depends: `MathNet.Numerics` — `LasPoint.Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>`, so the point geometry rides the Compute MathNet substrate directly
- scope: ASPRS LAS point data record formats 0-10; UNCOMPRESSED `.las` only — the `.laz` compressed leg is the `Unofficial.laszip.netstandard` peer (`api-laszip`)
- rail: `reconstruct#LAS_INGEST` — the uncompressed leg of the dual-engine scan-to-BIM ingest front

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream codec roots

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [CAPABILITY]                                                                   |
| :-----: | :-------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `LasReader`                 | streaming reader | `IDisposable` forward point reader over a LAS stream                           |
|  [02]   | `LasWriter`                 | streaming writer | `IDisposable` point writer authoring a LAS file                                |
|  [03]   | `LasHeader`                 | header value     | the ASPRS public header block (scale/offset/extrema)                           |
|  [04]   | `LasVariableLengthRecord`   | VLR record       | header-resident VLR/EVLR carrying CRS WKT, GeoTIFF keys, classification lookup |
|  [05]   | `Stream.AsyncStreamHandler` | stream handler   | the buffered LAS stream the reader/writer drives — PATH-BOUND admission        |
|  [06]   | `Stream.AsyncStreamBuffer`  | point buffer     | the bounded point staging buffer (`IStreamBuffer`) behind the handler          |

- [04]-[VLR]: `RecordID` (`ushort`; the `ProjectionRecordID` const `2112` is the OGC WKT CRS record) with `Data` (`byte[]`, UTF-8 WKT for record `2112`), `UserID`, `Description`, and `RecordLengthAfterHeader`.
- [05]-[STREAM_HANDLER]: `AsyncStreamHandler` is the sole shipped `IStreamHandler`, constructing from `(string, uint)`, so byte admission is PATH-BOUND to one temp path; `new LasReader(IStreamHandler)` is the stream growth seam.

[PUBLIC_TYPE_SCOPE]: point value model

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                                                           |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `LasPoint`      | point rec     | the decoded point — implements every facet below                                                       |
|  [02]   | `IPosition`     | position      | `Vector<double> Position` with `X`/`Y`/`Z` projections                                                 |
|  [03]   | `ILasPointBase` | core fields   | `Classification`, `Intensity`, `ScanAngle`, `FlightLine`, `UserData`, `GlobalEncoding`                 |
|  [04]   | `ILasTime`      | GPS-time      | `double Timestamp` (GPS adjusted-standard or week seconds)                                             |
|  [05]   | `ILasRgb`       | color         | `R`/`G`/`B` (formats 2/3/5/7/8/10)                                                                     |
|  [06]   | `ILas4Band`     | NIR           | `ILasRgb` with the `NIR` near-infrared band (formats 8/10)                                             |
|  [07]   | `ILasWaveform`  | waveform      | `WavePacketDescriptorIndex`/`ByteOffsetToWaveformData`/`ReturnPointWaveformLocation`/`X_t`/`Y_t`/`Z_t` |
|  [08]   | `ILasPoint`     | composite     | the union facet over `IPosition`+base+time+rgb+4band+waveform                                          |

[PUBLIC_TYPE_SCOPE]: point data record structs and format map
- note: `Structs.LasPointRecordFormat0..10` are `[StructLayout]` blittable records matching the eleven ASPRS on-disk layouts.

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
- note: `LasReader` is the streaming front of the uncompressed leg; the `ref` overload fills one caller-owned `LasPoint` for a garbage-free decode loop.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new LasReader(string lasFilePath, uint pointsToBuffer = 250000)` | ctor     | opens a LAS file, reads/validates the header     |
|  [02]   | `new LasReader(IStreamHandler stream)`                            | ctor     | stream growth seam; no shipped handler           |
|  [03]   | `LasPoint GetNextPoint()`                                         | instance | decodes/returns a new point (per-point alloc)    |
|  [04]   | `void GetNextPoint(ref LasPoint lpt)`                             | instance | fills caller point in place, no per-point alloc  |
|  [05]   | `bool EOF` / `ulong PointCount`                                   | property | end-of-stream flag and total point count         |
|  [06]   | `ILasHeader Header` / `IList<LasVariableLengthRecord> VLRs`       | property | the public header and the VLR/EVLR set (CRS WKT) |
|  [07]   | `void Dispose()`                                                  | instance | releases the underlying stream handle            |

[ENTRYPOINT_SCOPE]: point write
- note: `LasWriter` authors a LAS file — `Initialize` writes header+VLRs, `WritePoints` streams the body, the header composed through `LasHeaderBuilder`; the LAZ-compressed emit is the `api-laszip` writer leg.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `new LasWriter(string, ILasHeader, IEnumerable<LasVariableLengthRecord>?)` | ctor     | output file, header, VLR set            |
|  [02]   | `ILasWriter Initialize()`                                                  | instance | writes header + VLRs, positions at body |
|  [03]   | `WritePoint(LasPoint)` / `WritePoints(IEnumerable<LasPoint>)`              | instance | one or a sequence of points             |
|  [04]   | `ulong PointsWritten` / `long Position` / `string OutputFile`              | property | write progress, byte position, path     |

[ENTRYPOINT_SCOPE]: header build and field model
- note: `LasHeaderBuilder` is the fluent `IFluentBuilder<ILasHeader>`; its `Set*` methods (builder prefix elided below) compose the header the writer emits, `ILasHeader` exposes the matching read surface.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `SetVersion(byte major, byte minor)` / `SetPointDataFormat(byte)`       | instance | LAS version and point data record format      |
|  [02]   | `SetScale(double x, y, z)` / `SetOrigin(double x, y, z)`                | instance | scale/offset for integer↔double position      |
|  [03]   | `SetMinima(double,double,double)` / `SetMaxima(double,double,double)`   | instance | bounding-box extrema                          |
|  [04]   | `SetPointCount(ulong)` / `SetCreationDate(ushort year, ushort doy)`     | instance | point count and creation date                 |
|  [05]   | `ILasHeader` read surface                                               | property | public header value surface; `LasCloud` facts |
|  [06]   | `ILasHeader.CheckExtrema(IEnumerable<double>)` / `SetScale(ILasHeader)` | instance | recompute extrema / copy scale                |
|  [07]   | `LasHelper.GetGlobalEncoding(bool, bool)`                               | static   | GPS-standard-time + WKT-CRS bits              |
|  [08]   | `GpsTime.Parse(double timestamp)` (via `LasPoint.DateTime`)             | static   | LAS GPS timestamp → UTC `DateTime`            |

- [05]-[HEADER_READ]: `ScaleX`/`OriginX`/`MinX`/`MaxX`/…, `PointDataFormat`, `PointCount`, `NumPointRecordsByReturn` (`ulong[]`) with the `LegacyNumPointRecordsByReturn` (`uint[]`) pair.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `LasPoint` is the one decoded point carrier implementing `ILasPoint: IPosition, ILasPointBase, ILasTime, ILasRgb, ILas4Band, ILasWaveform`; `Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>` (`X`/`Y`/`Z` project its components) and `DateTime` lifts `Timestamp` through `GpsTime.Parse`
- eleven `Structs.LasPointRecordFormat0..10` blittable records own the on-disk layouts, and `PointTypeMap.TypeByPointDataFormat`/`PointDataFormatByType`/`SizeByType` is the byte↔`Type`↔record-length dispatch table the reader resolves on (`Marshal.SizeOf<T>` per format), so a format is one table row, never a switch arm; `LasPointConverter` marshals record ↔ `LasPoint` and `FieldUpdater` copies a facet group when re-encoding to a different format
- IN-PLACE MUTATION law: the ref-fill `GetNextPoint(ref LasPoint)` writes the scale+offset XYZ onto the SAME `Position` vector every iteration, so a bulk loop collects `point.Position.Clone()` — a bare `point.Position` read aliases every collected slot onto the last decoded point
- RAW CLASSIFICATION law: `Classification` passes the on-disk byte through unmasked — format-0-5 records pack synthetic/key-point/withheld flags into bits 5-7 (a withheld ground point reads `130`, not `2`), so a format-0-5 consumer masks `& 0x1F` while formats 6-10 carry a full dedicated class byte
- `ILasHeader` carries LAS version, point data format, scale/offset, bounding extrema, standard + extended per-return counts, and waveform/EVLR offsets, composed through the fluent `LasHeaderBuilder`; VLR/EVLR records (`LasVariableLengthRecord`) carry the OGC WKT/GeoTIFF CRS keys (record `2112`) and the classification lookup, read from the surface rather than a re-minted parser

[STACKING]:
- `Unofficial.laszip.netstandard`(`.api/api-laszip`): `LasIngest.Decode` composes this reader as the UNCOMPRESSED leg with the laszip compressed leg as one front — `LasCompression.Sniff` (public-header point-data-format byte at offset 104, high bit = LASzip) selects the engine once without a full open, `ReadLas` folds the no-alloc `GetNextPoint(ref LasPoint)` loop into one `LasCloud` (positions `Clone()`-detached, classes `& 0x1F`-masked below format 6), and the kernel registration is agnostic to which engine decoded
- `csharp:Rasm.Compute/Tensor/blas`(`#DENSE_ALGEBRA`): `LasPoint.Position` is the `MathNet.Numerics` `Vector<double>` the Compute dense-LA substrate consumes with no re-wrap, so a point batch crosses into covariance/PCA normal estimation and ICP transform directly, each collected instance a `Clone()` off the reader's one mutating vector
- content identity: the source-cloud bytes content-key as the `reconstruct#RECONSTRUCTION` `ReconstructionLineage` `[ValueObject<UInt128>]` through the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` composed via the seam `CanonicalWriter`, never a second hashing scheme and never the upper-stratum `Rasm.Compute` `InterchangeIdentity` (that reference inverts the strata DAG)
- classification seed: `LasPoint.Classification` (`& 0x1F`-masked below format 6 at ingest) folds into `LasCloud.ClassHistogram` and reduces to the `SegmentedCloud.DominantClass` the `reconstruct#RECONSTRUCTION` `AsprsBias` policy and `ElementClassifier` `(PrimitiveShape, IfcDomain, orientation)` table key on
- georeference: the header scale/offset and the `ProjectionRecordID` `2112` CRS WKT VLR land on `LasCloud.CrsWkt`, lowered onto the `Header.Reference` `GeoReference` through the `csharp:Rasm.Bim/Semantics/georeference#GEO_PROJECTION` `ProjNET` datum leg
- time: the capture `Instant` rides `LasCloud.At` (`NodaTime`-typed, supplied at `LasIngest.Decode`); the per-point GPS `Timestamp` decodes to a `DateTime` through `GpsTime`/`LeapSeconds` for a temporal scan slice

[LOCAL_ADMISSION]:
- `LasReader`/`LasWriter` are the only LAS codec roots; one streaming reader decodes every point data record format by `PointTypeMap` row
- one `LasPoint` carries the decoded point; a consumer narrows to the facet it needs (`IPosition` for geometry, `ILasPointBase` for classification, `ILasTime` for temporal), and `Position` enters the kernel as the `MathNet` vector it already is
- byte admission is PATH-BOUND: the ingest span-writes raw bytes to one `try/finally`-scoped temp path because the sole shipped `IStreamHandler` (`AsyncStreamHandler`) constructs from `(string, uint)`; `new LasReader(IStreamHandler)` is the stream growth seam
- `LasHeaderBuilder` composes the header and `ILasHeader` reads it
- LAS GPS time (`GpsTime`), CRS WKT VLR (`LasVariableLengthRecord`), and ASPRS classification (`LasPoint.Classification`) thread onto the canonical owners `georeference#GEO_PROJECTION` and `reconstruct#RECONSTRUCTION`
- LAZ decodes at the `api-laszip` peer, routed by a `LasCompression.Sniff`-detected compressed input; both engines are pure-managed IL, so the ALC firebreak holds

[RAIL_LAW]:
- Package: `Themis.Las` (MIT, pure-managed `lib/net8.0` AnyCPU IL, `MathNet.Numerics` dependency)
- Owns: streaming uncompressed ASPRS LAS read/write (point data record formats 0-10), the `LasPoint` facet model, the LAS public header + VLR/EVLR surface, and the GPS-time conversion — the UNCOMPRESSED leg of the `reconstruct#LAS_INGEST` dual-engine front
- Accept: a raw LAS byte stream admitted through one scoped temp path and decoded by the no-alloc `GetNextPoint(ref LasPoint)` loop into `Position.Clone()`-detached `MathNet` vectors and `& 0x1F`-masked format-0-5 classes, content-keyed via the kernel `ContentHash`/seam `CanonicalWriter` (`ReconstructionLineage`), the ASPRS classification and CRS WKT threaded onto `reconstruct#RECONSTRUCTION`/`georeference#GEO_PROJECTION`
- Reject: a re-minted point-cloud scan/segmentation/registration engine (the kernel owns it); a LAZ decode in this leg (the `Unofficial.laszip.netstandard` peer owns it) or a native LASzip binding; a hand-implemented `IStreamHandler` beside the path admission; a bare `point.Position` collection or an unmasked format-0-5 classification byte; a second hashing scheme or the upper-stratum `Rasm.Compute` `InterchangeIdentity` beside the kernel `ContentHash`/seam `CanonicalWriter` lineage; a Themis-local CRS reprojection beside `ProjNET`; a parallel point struct or per-format reader family
