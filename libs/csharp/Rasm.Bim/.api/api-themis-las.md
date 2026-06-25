# [RASM_BIM_API_THEMIS_LAS]

`Themis.Las` is the managed ASPRS LAS point-cloud reader/writer backing the scan-to-BIM ingest seam. It streams every ASPRS LAS point data record format (0-10) into a `LasPoint` carrying position, intensity, ASPRS classification, return index/count, scan angle, GPS time, RGB + near-infrared, and the full waveform packet, and exposes the LAS public header (scale/offset, bounding extrema, point counts by return) plus the VLR/EVLR record surface that carries the CRS WKT/GeoTIFF georeferencing. It is pure-managed (`Themis.Las.dll`, AnyCPU IL) over a single native-free dependency — `MathNet.Numerics`, the same dense-linear-algebra owner `csharp:Rasm.Compute/Tensor/blas` carries — so a decoded `LasPoint.Position` is already a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and the Compute substrate consume without a copy. It decodes/encodes UNCOMPRESSED LAS only; LAZ has no admissible non-copyleft managed port. The `reconstruct#RECONSTRUCTION` fold reads the raw LAS bytes through this reader as the front of the scan-to-BIM pipeline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Themis.Las`
- package: `Themis.Las` (single assembly, version 2025.3.5, direct pin)
- license: MIT (`Clodge-Scientific/Themis`)
- assembly: `Themis.Las` → the `net10.0` consumer binds `lib/net8.0/Themis.Las.dll` (the only `lib/` TFM; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset)
- namespace: `Themis.Las`, `Themis.Las.Structs`, `Themis.Las.Builders`, `Themis.Las.Stream`, `Themis.Las.Time`
- transitive: `MathNet.Numerics` 5.0.0 (the package floor; the central `csharp:Rasm.Compute` pin wins resolution) — `LasPoint.Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>`, so the point geometry rides the Compute MathNet substrate directly
- scope: ASPRS LAS 1.0-1.4, point data record formats 0-10; UNCOMPRESSED `.las` only (no LAZ/LASzip)
- rail: `reconstruct#RECONSTRUCTION` (scan-to-BIM ingest front)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream codec roots
- rail: reconstruct#INGEST

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [RAIL]                                                    |
| :-----: | :--------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `LasReader` / `ILasReader`   | streaming reader   | `IDisposable` forward point reader over a LAS stream     |
|  [02]   | `LasWriter` / `ILasWriter`   | streaming writer   | `IDisposable` point writer authoring a LAS file          |
|  [03]   | `LasHeader` / `ILasHeader`   | header value       | the ASPRS public header block (scale/offset/extrema)     |
|  [04]   | `LasVariableLengthRecord` / `ILasVariableLengthRecord` | VLR record | header-resident VLR/EVLR (CRS WKT, GeoTIFF keys, classification lookup) |
|  [05]   | `Stream.IStreamHandler` / `Stream.AsyncStreamHandler`  | stream handler | buffered LAS stream the reader/writer drives            |
|  [06]   | `Stream.IStreamBuffer` / `Stream.AsyncStreamBuffer`    | point buffer   | the bounded point staging buffer behind the handler     |

[PUBLIC_TYPE_SCOPE]: point value model
- rail: reconstruct#POINT
- note: `LasPoint` implements the full facet interface set; each interface names one ASPRS field group, so a consumer that needs only position/classification depends on the narrow facet rather than the concrete record.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :------------------ | :----------------- | :---------------------------------------------------------- |
|  [01]   | `LasPoint`          | point record       | the decoded point — implements every facet below            |
|  [02]   | `IPosition`         | position facet     | `Vector<double> Position` + `X`/`Y`/`Z` (MathNet vector)    |
|  [03]   | `ILasPointBase`     | core-field facet   | `Classification`, `Intensity`, `ScanAngle`, `FlightLine`, `UserData`, `GlobalEncoding` |
|  [04]   | `ILasTime`          | GPS-time facet     | `double Timestamp` (GPS adjusted-standard or week seconds)  |
|  [05]   | `ILasRgb`           | color facet        | `R`/`G`/`B` (formats 2/3/5/7/8/10)                          |
|  [06]   | `ILas4Band`         | NIR facet          | `ILasRgb` + `NIR` near-infrared band (formats 8/10)         |
|  [07]   | `ILasWaveform`      | waveform facet     | `WavePacketDescriptorIndex`, `ByteOffsetToWaveformData`, `ReturnPointWaveformLocation`, `X_t`/`Y_t`/`Z_t` |
|  [08]   | `ILasPoint`         | composite facet    | the union facet over `IPosition`+base+time+rgb+4band+waveform |

[PUBLIC_TYPE_SCOPE]: point data record structs and format map
- rail: reconstruct#FORMAT
- note: `Structs.LasPointRecordFormat0..10` are `[StructLayout]` blittable records matching the eleven ASPRS on-disk layouts; `PointTypeMap` is the byte↔`Type` dispatch table the reader resolves on, never an enumerated switch.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]    | [RAIL]                                                  |
| :-----: | :------------------------------------ | :--------------- | :----------------------------------------------------- |
|  [01]   | `Structs.LasPointRecordFormat0..10`   | on-disk record   | the eleven blittable ASPRS point data record formats   |
|  [02]   | `Structs.LasPointConverter`           | record converter | marshals an on-disk record ↔ `LasPoint`                |
|  [03]   | `Structs.FieldUpdater`                | field projector   | copies a facet group between records on format change  |
|  [04]   | `PointTypeMap`                        | format table     | `TypeByPointDataFormat` / `PointDataFormatByType` / `SizeByType` byte↔`Type`↔size maps |
|  [05]   | `Builders.ILasHeaderBuilder` / `Builders.LasHeaderBuilder` | fluent builder | composes an `ILasHeader` for the writer (`IFluentBuilder<ILasHeader>`) |
|  [06]   | `Time.GpsTime` / `Time.LeapSeconds`   | GPS-time helper  | converts the LAS GPS timestamp ↔ UTC `DateTime` with leap-second tables |
|  [07]   | `LasHelper` / `Extensions`            | header helper    | `GetGlobalEncoding(useGpsStandardTime, useProjWkt)` global-encoding bit composition |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: forward point read
- rail: reconstruct#INGEST
- note: `LasReader` is the streaming front of the ingest; the `ref` overload fills a caller-owned `LasPoint` so a tight decode loop allocates no per-point garbage.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :-------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new LasReader(string lasFilePath, uint pointsToBuffer = 250000)` | ctor          | opens a LAS file and reads/validates the header  |
|  [02]   | `new LasReader(IStreamHandler stream)`                          | ctor            | reads from a caller-supplied stream handler      |
|  [03]   | `LasPoint GetNextPoint()`                                       | read            | decodes and returns the next point               |
|  [04]   | `void GetNextPoint(ref LasPoint lpt)`                           | read (no-alloc) | decodes into a caller-owned point                |
|  [05]   | `bool EOF` / `ulong PointCount`                                 | cursor          | end-of-stream flag and total point count         |
|  [06]   | `ILasHeader Header` / `IList<LasVariableLengthRecord> VLRs`     | metadata        | the public header and the VLR/EVLR set (CRS WKT) |
|  [07]   | `void Dispose()`                                                | lifetime        | releases the underlying stream handle            |

[ENTRYPOINT_SCOPE]: point write
- rail: reconstruct#EMIT
- note: `LasWriter` authors a LAS file; `Initialize` writes the header, `WritePoints` streams the body, the header is composed through `LasHeaderBuilder`.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new LasWriter(string lasFile, ILasHeader header, IEnumerable<LasVariableLengthRecord>? vlrs = null)` | ctor | binds the output file, header, and VLR set |
|  [02]   | `ILasWriter Initialize()`                                                          | open           | writes the header + VLRs, positions at body  |
|  [03]   | `void WritePoint(LasPoint point)` / `void WritePoints(IEnumerable<LasPoint>)`      | write          | streams one or a sequence of points          |
|  [04]   | `ulong PointsWritten` / `long Position` / `string OutputFile`                      | cursor         | write progress, byte position, target path   |

[ENTRYPOINT_SCOPE]: header build and field model
- rail: reconstruct#FORMAT
- note: `LasHeaderBuilder` is a fluent `IFluentBuilder<ILasHeader>`; `ILasHeader` exposes both the read surface and the `Set*` mutators the converter writes; `LasHelper.GetGlobalEncoding` composes the global-encoding bit field.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `LasHeaderBuilder.SetVersion(byte major, byte minor)` / `SetPointDataFormat(byte)` | header build  | LAS version and point data record format         |
|  [02]   | `LasHeaderBuilder.SetScale(double x, double y, double z)` / `SetOrigin(double x, double y, double z)` | header build | scale and offset for integer↔double position |
|  [03]   | `LasHeaderBuilder.SetMinima(double, double, double)` / `SetMaxima(double, double, double)` | header build | bounding-box extrema                       |
|  [04]   | `LasHeaderBuilder.SetPointCount(ulong)` / `SetCreationDate(ushort year, ushort doy)` | header build | point count and creation date                |
|  [05]   | `ILasHeader.{ScaleX, OriginX, MinX, MaxX, …}` / `PointDataFormat` / `PointCount` | header read     | the public header value surface                  |
|  [06]   | `ILasHeader.CheckExtrema(IEnumerable<double> pos)` / `SetScale(ILasHeader)`       | header derive   | recompute extrema / copy scale from a source     |
|  [07]   | `LasHelper.GetGlobalEncoding(bool useGpsStandardTime = true, bool useProjWkt = false)` | bit compose | GPS-standard-time and WKT-CRS global-encoding bits |
|  [08]   | `GpsTime.Parse(double timestamp)` (via `LasPoint.DateTime`)                       | time decode     | LAS GPS timestamp → UTC `DateTime`               |

## [04]-[IMPLEMENTATION_LAW]

[POINT_TOPOLOGY]:
- `LasPoint` is the one decoded point carrier implementing `ILasPoint : IPosition, ILasPointBase, ILasTime, ILasRgb, ILas4Band, ILasWaveform`; `Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>` (`X`/`Y`/`Z` project its components) and `DateTime` lifts `Timestamp` through `GpsTime.Parse`
- the on-disk format is the eleven `Structs.LasPointRecordFormat0..10` blittable records; `PointTypeMap.TypeByPointDataFormat`/`PointDataFormatByType`/`SizeByType` is the byte↔`Type`↔record-length dispatch table the reader resolves on (`Marshal.SizeOf<T>` per format), so a format is one table row, never a switch arm
- `LasPointConverter` marshals an on-disk record ↔ `LasPoint`; `FieldUpdater` copies a facet group when re-encoding to a different format
- the header (`ILasHeader`) carries LAS version, point data format, scale/offset, bounding extrema, legacy + extended point counts (by return), waveform-packet and extended-VLR offsets; `LasHeaderBuilder` is the fluent `IFluentBuilder<ILasHeader>` the writer composes a header through
- `LasReader.GetNextPoint(ref LasPoint)` is the no-allocation decode path; the buffered `Stream.AsyncStreamHandler`/`AsyncStreamBuffer` stage the body for the forward cursor
- VLR/EVLR records (`LasVariableLengthRecord`) carry the OGC WKT / GeoTIFF CRS keys and the classification lookup; the georeferencing leg reads them, not a re-minted CRS parser

[INTEGRATION_STACK]:
- ingest front: `reconstruct#RECONSTRUCTION` reads a capture's raw LAS bytes through `LasReader`; the decoded `LasPoint` stream feeds the kernel `csharp:Rasm/Vectors#ALIGN` cloud-ICP registration and the `csharp:Rasm/Geometry/spatial#SEGMENTATION` plane/cylinder segmentation, which produce the `reconstruct#RECONSTRUCTION` `SegmentedCloud` rows — Themis owns the decode, the kernel owns the fit, and no scan engine is re-minted in Bim
- MathNet seam: `LasPoint.Position` is a `MathNet.Numerics.LinearAlgebra.Vector<double>` over the same `MathNet.Numerics` owner `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA` carries, so a point batch crosses into the Compute dense-LA substrate (covariance/PCA normal estimation, ICP transform) with no vector re-wrap
- content identity: the source-cloud payload (`SegmentedCloud.CloudBytes`, `ReadOnlyMemory<byte>`) is content-keyed through the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key` (the `reconstruct#RECONSTRUCTION` `ReconstructionLineage` `[ValueObject<UInt128>]`), never a second hashing scheme over the LAS bytes
- classification seed: `LasPoint.Classification` (the ASPRS class byte) seeds the `reconstruct#RECONSTRUCTION` `ElementClassifier` `(PrimitiveShape, IfcDomain, orientation)` discipline-hint table — a ground/building/vegetation ASPRS class biases the fitted-primitive-to-`IfcClass` projection, never an enumerated per-class branch
- georeference: the LAS header scale/offset and the CRS WKT VLR feed the `csharp:Rasm.Bim/Semantics/georeference#GEOREFERENCE` `GeoReference` projection so a georeferenced capture lands in the canonical kernel frame through the `ProjNET` datum leg `georeference#GEOREFERENCE` owns, not a Themis-local reprojection
- time: the LAS GPS `Timestamp` decodes to a `DateTime` through `GpsTime`/`LeapSeconds`; the capture instant the `reconstruct#RECONSTRUCTION` `BimModel` carries is the `NodaTime` `ClockPolicy.Now`, with the per-point GPS time available as point metadata when a temporal scan slice is queried

[LOCAL_ADMISSION]:
- `LasReader`/`LasWriter` are the only LAS codec roots; one streaming reader decodes every point data record format by `PointTypeMap` row, never a per-format `LasReaderFormat3`/`LasReaderFormat6` family
- the decoded point is the one `LasPoint`; a consumer narrows to the facet it needs (`IPosition` for geometry, `ILasPointBase` for classification, `ILasTime` for temporal) rather than a parallel point struct, and `LasPoint.Position` enters the kernel as the MathNet vector it already is
- the header is composed through `LasHeaderBuilder` and read through `ILasHeader`; a hand-rolled LAS header byte layout beside the builder is the rejected form
- LAS GPS time, CRS WKT VLR, and ASPRS classification are read from the package surface (`GpsTime`, `LasVariableLengthRecord`, `LasPoint.Classification`) and threaded onto the canonical owners (`georeference#GEOREFERENCE`, `reconstruct#RECONSTRUCTION`), never re-derived
- LAZ is out of scope — a compressed input is rejected at the boundary; the codec is uncompressed-LAS-only, and no native LASzip binding is admitted (the ALC firebreak and the non-copyleft constraint both hold)

[RAIL_LAW]:
- Package: `Themis.Las` (2025.3.5, MIT, pure-managed `lib/net8.0` AnyCPU IL, `MathNet.Numerics` 5.0.0 sole dependency)
- Owns: streaming uncompressed ASPRS LAS read/write (point data record formats 0-10), the `LasPoint` facet model, the LAS public header + VLR/EVLR surface, and the GPS-time conversion
- Accept: a raw LAS byte stream decoded to `LasPoint` rows whose `Position` is the MathNet vector the kernel registration and the Compute substrate consume, the source-cloud bytes content-keyed by `InterchangeIdentity.Key`, and the ASPRS classification/CRS-WKT threaded onto `reconstruct#RECONSTRUCTION`/`georeference#GEOREFERENCE`
- Reject: a re-minted point-cloud scan/segmentation/registration engine (the kernel owns it); a LAZ/LASzip decode path or a native LASzip binding; a second hashing scheme over the LAS bytes beside `InterchangeIdentity.Key`; a Themis-local CRS reprojection beside `ProjNET`; a parallel point struct or per-format reader family where one `LasPoint` and one `LasReader` discriminate by `PointTypeMap` row
