# [RASM_PERSISTENCE_API_E57]

`Aardvark.Data.E57` owns the managed ASTM E57 (E2807-11) read decode: the 48-byte binary file header, the XML `E57Root` metadata tree with one `E57Data3D` record per scan setup, and the CRC-paged CompressedVector point section unpacked chunk-by-chunk out of its bit-packed per-property byte streams. `Ingest/pointcloud#SCAN_SOURCE` composes it as the E57 leg of the reality-capture codec pair — header and `Data3D` metadata rows for the durable header, streamed point chunks for the chunked-blob residence and the per-region cell fold. Decode only: the assembly carries no writer.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: importer facade and the point-property vocabulary

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :----------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Aardvark.Data.Points.Import.E57`          | static class  | file/stream importer facade over the spec surface         |
|  [02]   | `E57.E57Chunk`                             | decoded chunk | one point block: positions plus the raw property arrays   |
|  [03]   | `E57.CartesianInvalidState`                | `byte` enum   | `Valid` \| `OnlyDirectionIsValid` \| `Invalid`            |
|  [04]   | `E57.SphericalInvalidState`                | `byte` enum   | `Valid` \| `InvalidSphericalRange` \| `Invalid`           |
|  [05]   | `Aardvark.Data.E57.PointPropertySemantics` | enum          | the per-point channel vocabulary every stream keys on     |
|  [06]   | `Aardvark.Data.E57.ASTM_E57`               | static class  | the spec surface: header parse, element model, CRC verify |

- [01]-[POINT_SEMANTICS]: `CartesianX` `CartesianY` `CartesianZ` `SphericalRange` `SphericalAzimuth` `SphericalElevation` `RowIndex` `ColumnIndex` `ReturnCount` `ReturnIndex` `TimeStamp` `Intensity` `ColorRed` `ColorGreen` `ColorBlue` `CartesianInvalidState` `SphericalInvalidState` `IsTimeStampInvalid` `IsIntensityInvalid` `IsColorInvalid` `Classification` `NormalX` `NormalY` `NormalZ` `Reflectance` `Amplitude` — the row name matches the E57 prototype child's `Semantic` string case-insensitively, and an unmapped semantic throws at `Data3D` parse.
- [02]-[SEMANTIC_ELEMENT_TYPE]: each semantic fixes its raw array type — `double[]` for the cartesian, spherical, and timestamp channels, `int[]` for `Intensity`/`Classification`, `byte[]` for the three colour channels and every invalid-state/flag channel, `uint[]` for `RowIndex`/`ColumnIndex`/`ReturnCount`/`ReturnIndex`, `float[]` for the three normal channels and `Reflectance`/`Amplitude`.

[PUBLIC_TYPE_SCOPE]: header, root, and scan-setup model

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [CAPABILITY]                                               |
| :-----: | :------------------------------- | :----------------- | :--------------------------------------------------------- |
|  [01]   | `ASTM_E57.E57FileHeader`         | binary file header | signature, version, file length, XML offset/length, page   |
|  [02]   | `ASTM_E57.E57Root`               | XML root structure | format name, guid, library version, `Data3D`/`Images2D`    |
|  [03]   | `ASTM_E57.E57Data3D`             | scan setup         | points, pose, bounds, limits, stamps, sensor, environment  |
|  [04]   | `ASTM_E57.E57CompressedVector`   | point section      | file offset, record count, prototype, codecs, the read leg |
|  [05]   | `ASTM_E57.E57RigidBodyTransform` | pose               | `Rot3d` rotation, `V3d` translation, composed `Trafo3d`    |
|  [06]   | `ASTM_E57.E57CartesianBounds`    | bounds             | `Box3d Bounds` over the setup's local frame                |
|  [07]   | `ASTM_E57.E57SphericalBounds`    | bounds             | `Range1d` `Range`/`Elevation`/`Azimuth`                    |
|  [08]   | `ASTM_E57.E57IndexBounds`        | bounds             | `Range1i?` `Row`/`Column`/`Return` grid extents            |
|  [09]   | `ASTM_E57.E57IntensityLimits`    | limits             | `Range1d Intensity` the stream normalizes against          |
|  [10]   | `ASTM_E57.E57ColorLimits`        | limits             | `Range1d` `Red`/`Green`/`Blue` producible ranges           |
|  [11]   | `ASTM_E57.E57DateTime`           | stamp              | GPS-epoch seconds, atomic-clock flag, `DateTimeOffset`     |
|  [12]   | `ASTM_E57.E57Image2D`            | camera image       | pinhole/spherical/cylindrical/visual representation family |

- [03]-[DATA3D_FIELDS]: `Guid` `Points` `Pose` `OriginalGuids` `PointGroupingSchemes` `Name` `Description` `CartesianBounds` `SphericalBounds` `IndexBounds` `IntensityLimits` `ColorLimits` `AcquisitonStart` `AcquisitonEnd` `SensorVendor` `SensorModel` `SensorSerialNumber` `SensorHardwareVersion` `SensorSoftwareVersion` `SensorFirmwareVersion` `Temperature` `RelativeHumidity` `AtmosphericPressure` — the acquisition pair spells `Acquisiton`, matching the element names the parser reads.
- [04]-[PROTOTYPE_ELEMENTS]: `IE57Element` (`E57ElementType E57Type`) roots the tree and `IBitPack` (`NumberOfBitsForBitPack`, `Semantic`) marks a prototype child; the concrete family is `E57Integer` `E57ScaledInteger` `E57Float` `E57String` `E57Blob` `E57Structure` `E57Vector` `E57CompressedVector` `E57Codec`, with `E57PointRecord` `E57PointGroupingSchemes` `GroupingByLine` `E57LineGroupRecord` carrying the grouping schemes.
- [05]-[OFFSETS_AND_PACKETS]: `E57PhysicalOffset`/`E57LogicalOffset` are the two addressing spaces the CRC paging separates, `+` crossing between them; `E57CompressedVectorHeader` `E57IndexPacketHeader` `E57IndexPacketAddressEntry` `E57DataPacketHeader` are the section's on-disk packet structs.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the spec surface — header parse and page-CRC verification
- note: `E57FileHeader.Parse` seeks to `0`, reads the 48-byte binary header, gates the `ASTM-E57` signature, version `1.0`, and the `1024` page size, refuses a header file length disagreeing with the caller's actual byte count, then reads the XML at `XmlOffset` through the CRC-skipping logical reader and parses `E57Root`. It needs the WHOLE seekable source, never a prefix.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `ASTM_E57.E57FileHeader.Parse(Stream, long actualFileSizeInBytes, bool verbose)` | static   | binary header plus the parsed `E57Root`  |
|  [02]   | `E57FileHeader.FileSignature` / `VersionMajor` / `VersionMinor`                  | property | `ASTM-E57`, `1`, `0`                     |
|  [03]   | `E57FileHeader.FileLength` / `XmlOffset` / `XmlLength` / `PageSize`              | property | `ulong` extents plus the physical offset |
|  [04]   | `E57FileHeader.RawXml` / `E57Root`                                               | property | the `XElement` tree and its parsed root  |
|  [05]   | `ASTM_E57.VerifyChecksums(Stream, long streamLengthInBytes)`                     | static   | page-size gate over the 1024-byte pages  |

[ENTRYPOINT_SCOPE]: the metadata tree
- note: `E57Root` and `E57Data3D` expose public FIELDS, not properties; `Data3D`/`Images2D` are null when the source declares no such vector.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `E57Root.FormatName` / `Guid` / `VersionMajor` / `VersionMinor`               | field    | the required root identity                  |
|  [02]   | `E57Root.E57LibraryVersion` / `CreationDateTime`                              | field    | writing library and creation stamp          |
|  [03]   | `E57Root.CoordinateMetadata`                                                  | field    | the file-level CRS declaration (WKT string) |
|  [04]   | `E57Root.Data3D` / `Images2D`                                                 | field    | `E57Data3D[]` scans, `E57Image2D[]` images  |
|  [05]   | `E57Data3D.Points` → `E57CompressedVector.RecordCount`                        | field    | the setup's declared point count            |
|  [06]   | `E57Data3D.Pose` → `Rotation` / `Translation` / `RigidBodyTransform`          | property | local-to-file-level rigid transform         |
|  [07]   | `E57Data3D.CartesianBounds.Bounds` → `Box3d.Min` / `Max` → `V3d.X`/`Y`/`Z`    | field    | the setup's local-frame extent              |
|  [08]   | `E57Data3D.Sem2Index` / `Has(sem)` / `HasAllOrNoneOf(params)`                 | member   | the declared channel set of one setup       |
|  [09]   | `E57Data3D.HasCartesianCoordinates` / `HasSphericalCoordinates` / `HasColors` | property | the three coordinate/colour probes          |
|  [10]   | `E57DateTime.DateTimeValue` / `IsAtomicClockReferenced` / `DateTime`          | member   | GPS-epoch seconds and its `DateTimeOffset`  |

[ENTRYPOINT_SCOPE]: point streaming
- note: three depths reach the points and each drags a different dependency set — `E57CompressedVector.ReadDataFull` yields BCL arrays alone, `E57Data3D.StreamPointsFull` adds `V3d` positions with the setup pose APPLIED, and the `E57.Chunks`/`ChunksFull`/`E57Info` facade adds the `Aardvark.Data.Points.Base` carriers.
- note: the two stream depths take their full argument lists as `ReadDataFull(int maxChunkPointCount, ImmutableDictionary<PointPropertySemantics, int> sem2idx, E57IntensityLimits intensityLimits, bool verbose, bool verboseDetail)` and `StreamPointsFull(int maxChunkPointCount, bool verbose, ImmutableHashSet<PointPropertySemantics> exclude)`; `maxChunkPointCount` caps one yielded block and `exclude`/`sem2idx` are the channel masks the bit-unpacker honours. Each facade row spells `(Stream stream, long streamLengthInBytes, ParseConfig config[, bool verifyChecksums])` or `(string filename, ParseConfig config)`.

| [INDEX] | [SURFACE]                                           | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------- | :-------- | :----------------------------------------------- |
|  [01]   | `E57CompressedVector.ReadDataFull(…)`               | instance  | raw per-semantic arrays, chunk by chunk          |
|  [02]   | `E57Data3D.StreamPointsFull(…)`                     | instance  | `(V3d[] Positions, …Array Properties)` per chunk |
|  [03]   | `E57CompressedVector.ByteStreamsCount`              | property  | the prototype's byte-stream count                |
|  [04]   | `E57.ChunksFull(Stream, long, ParseConfig[, bool])` | extension | `IEnumerable<E57Chunk>`, every property          |
|  [05]   | `E57.ChunksFull(string, ParseConfig)`               | static    | the path form of the same stream                 |
|  [06]   | `E57.Chunks(Stream, long, ParseConfig[, bool])`     | extension | `IEnumerable<Chunk>`, index/flag lanes cut       |
|  [07]   | `E57.Chunks(string, ParseConfig)`                   | static    | the path form of the same stream                 |
|  [08]   | `E57.E57Info(string, ParseConfig)`                  | static    | `PointFileInfo<ASTM_E57.E57FileHeader>`          |
|  [09]   | `E57.E57Format`                                     | static    | the `PointCloudFileFormat` registry row          |

[ENTRYPOINT_SCOPE]: the decoded chunk
- note: every `E57Chunk` accessor is a projection over `RawData`; an absent channel returns `null`, never an empty array, and `Colors`/`Normals` throw when a partner channel is missing.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `E57Chunk.RawData` → `ImmutableDictionary<PointPropertySemantics, Array>` | property | every decoded channel, unprojected      |
|  [02]   | `E57Chunk.Positions` / `Count` / `Data3D`                                 | property | `V3d[]` positions, count, owning setup  |
|  [03]   | `E57Chunk.Colors` / `HasNormals` / `Normals`                              | property | `C3b[]` colours, `V3f[]` normals        |
|  [04]   | `E57Chunk.Intensities` / `Classification`                                 | property | `int[]` intensity and class channels    |
|  [05]   | `E57Chunk.Timestamps`                                                     | property | `DateTimeOffset[]` off the setup epoch  |
|  [06]   | `E57Chunk.RowIndex` / `ColumnIndex` / `ReturnCount` / `ReturnIndex`       | property | `uint[]` grid and multi-return channels |
|  [07]   | `E57Chunk.CartesianInvalidState` / `SphericalInvalidState`                | property | the two per-point validity enum lanes   |
|  [08]   | `E57Chunk.IsTimeStampInvalid` / `IsIntensityInvalid` / `IsColorInvalid`   | property | `bool[]` per-channel validity flags     |
|  [09]   | `E57Chunk.NormalsTransformInPlace(Rot3d)`                                 | instance | rotates the held normal lane in place   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- E57 addresses in TWO spaces: `E57PhysicalOffset` counts raw bytes and `E57LogicalOffset` counts payload bytes, because every 1024-byte page ends in a 4-byte CRC the logical reader skips — 1020 payload bytes per page — so a hand-rolled seek over raw offsets reads CRC bytes as payload, which is exactly what the internal logical reader exists to prevent
- `E57FileHeader.Parse` gates the `ASTM-E57` signature, major `1`, minor `0`, and page size `1024`, and throws when its own `FileLength` disagrees with the caller-supplied byte count, so the caller hands the TRUE source length and a truncated or concatenated file refuses at the header rather than mid-stream
- one file carries N `Data3D` scan setups and the point count is the SUM of each setup's `Points.RecordCount`; each setup declares its OWN channel set through `Sem2Index`, so a file's setups can disagree on which properties exist and a fold reading a channel present in one setup and absent in another gets `null` from the chunk accessor
- `E57Data3D.StreamPointsFull` is the pose-APPLYING depth: a cartesian setup interleaves `CartesianX/Y/Z`, a spherical-only setup converts range/azimuth/elevation to cartesian, and a present `Pose` maps every position through `Rot.Transform(Pose.Rotation, p) + Pose.Translation` before yield — so the streamed positions are already in the FILE-LEVEL frame and re-applying the pose double-transforms the cloud
- `E57CompressedVector.ReadDataFull` is the dependency-lightest depth: its yield is `ImmutableDictionary<PointPropertySemantics, Array>` over BCL arrays alone, no Aardvark value type in the payload, so a consumer that interleaves its own positions never binds `Aardvark.Data.Points.Base`
- `NumberOfBitsForBitPack: 0` marks a prototype child as a CONSTANT channel: `StreamPointsFull` materializes it as a filled `int[]` for the ten index/flag/class semantics and throws for every other semantic and for a zero-bit `E57ScaledInteger`
- `E57Chunk.Colors` and `Normals` throw when one partner channel of an RGB or XYZ triple is missing, while every single-channel accessor returns `null` on absence — so a triple channel is probed through `RawData.ContainsKey` or `Data3D.HasColors`/`HasAllOrNoneOf`, never by catching
- `E57DateTime.DateTime` is `GpsStartEpoch + DateTimeValue` seconds (GPS epoch 1980-01-06T00:00:00Z); `E57Chunk.Timestamps` GUESSES the epoch when the setup declares no `AcquisitonStart`, choosing GPS or Unix by which yields a past instant, so a durable stamp reads `Data3D.AcquisitonStart.DateTime` rather than the per-point projection
- `VerifyChecksums` walks 1024-byte pages and refuses a stream whose length is not a page multiple; the `verifyChecksums: true` overloads run it BEFORE the header parse, so it is an O(n) pre-pass over the whole file, not a per-page gate inside the read
- decode is the whole surface — no writer, no encoder, no `E57Writer` — and `E57.E57Format` registers itself with the `Aardvark.Data.Points.Base` `PointCloudFileFormat` registry at type initialization, so touching the facade class triggers that global registration

[STACKING]:
- `Ingest/pointcloud#SCAN_SOURCE`: sole consumer — `ScanFormat.Sniff` reads the `ASTM-E57` signature at byte 0 without an open, `E57FileHeader.Parse` fills the `ScanHeader` row (summed `RecordCount`, `E57Root.CoordinateMetadata` as the CRS WKT, `Data3D.SensorVendor`/`SensorModel`, `AcquisitonStart.DateTime` as the capture stamp), and `E57Data3D.StreamPointsFull` feeds the one ingest fold that chunks the raw bytes and accumulates the per-cell region rows
- `Element/codec#CONTENT_CHUNKING`(`.api/api-fastcdc.md`): the scan's identity is the FastCDC manifest's whole-artifact `ContentAddress` over the RAW E57 bytes, so the durable key is codec-independent and this reader never participates in identity
- `Element/identity#ELEMENT_IDENTITY`(`.api/api-h3-pg.md`): each streamed position's `V3d.X`/`Y` mints the region cell through `IdentityStore.Cell`, and `V3d.Z` folds the region's vertical band — the same `bigint` cell vocabulary the LAS/LAZ leg and the in-database index share
- `Unofficial.laszip.netstandard`(`libs/dotnet/.api/api-laszip.md`): the reality-capture pair's other leg — `ScanFormat` dispatches E57 here and `.las`/`.laz` there, both folding into ONE `ScanBatch` currency, so no format sees a per-codec point model
- `Rasm.Bim/Exchange/reconstruct#LAS_INGEST`: that scan-to-BIM owner reads LAS/LAZ alone and never this assembly; the two owners join on the raw-bytes content key, never on a shared carrier

[LOCAL_ADMISSION]:
- `E57FileHeader.Parse` receives a SEEKABLE stream and the true byte count; a non-seekable `Origin.FromStream` buffers first, because the header read seeks to `XmlOffset` and every point read seeks to its section offset
- streaming enters at `E57Data3D.StreamPointsFull` per setup, never at the `E57.Chunks`/`ChunksFull` facade — the facade's `ParseConfig` defaults fabricate colour, normal, intensity, and classification lanes a durable row must not inherit, and `Chunks` silently drops the index, flag, and return channels
- raw `ASTM_E57.*` and `E57Chunk` types never leave the decode leg: internal code holds the canonical `ScanHeader`/`ScanRegion`/`ScanBatch` rows
- every thrown parse refusal traps at the fold boundary onto `ScanFault.CodecReject`; a setup whose `CoordinateMetadata` names a CRS the spec cannot admit rails `ScanFault.CrsUnsupported`
- AGPL-3.0 custody rides a SEPARATE assembly reference (`PackageReference`), never an ILMerge into a Rasm assembly; the pure-managed ns2.0 IL binds forward and the plugin ALC firebreak holds
