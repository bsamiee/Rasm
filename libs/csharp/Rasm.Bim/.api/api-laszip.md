# [RASM_BIM_API_LASZIP]

`Unofficial.laszip.netstandard` owns the managed LASzip C-API codec — arithmetic-coded LAZ decode and encode backing the `Exchange/reconstruct#LAS_INGEST` LAZ-capable leg. One `laszip` object opens a `.las` or `.laz` stream and reports `is_compressed`, so a single reader decodes both, carrying the differentiators the BCL-managed reader lacks: selective per-channel decompression and the `.lax` spatial-index bbox query. `LasIngest.Decode` folds it as the compressed-stream PEER of the uncompressed `Themis.Las` reader (`api-themis-las`), dispatched by `LasCompression.Sniff`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Unofficial.laszip.netstandard`
- package: `Unofficial.laszip.netstandard` (LGPL-2.1 weak copyleft; a managed C# port of the rapidlasso LASzip, arithmetic coder and per-format compressed-item decode/encode ported in C#, not P/Invoked)
- assembly: `Unofficial.laszip.netstandard` → the `net10.0` consumer binds `lib/netstandard2.0/Unofficial.laszip.netstandard.dll` (sole `lib/` TFM; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset)
- namespace: `LASzip.Net`
- depends: none — zero managed dependencies
- scope: ASPRS LAS point formats 0-10 read and write, LAZ arithmetic-coded compression/decompression, selective-channel decompression, the `.lax` spatial-index bbox query, and the full header/VLR/EVLR/geokey model
- rail: `Exchange/reconstruct#LAS_INGEST` (the LAZ-capable point-cloud decode leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec root and channel-select vocabulary

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [CAPABILITY]                                                 |
| :-----: | :---------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `laszip`                      | LAS/LAZ codec      | the one C-API codec object; holds one `header` + one `point` |
|  [02]   | `LASZIP_DECOMPRESS_SELECTIVE` | `[Flags]` enum     | per-channel decode mask set before the read loop             |
|  [03]   | `U64I64F64`                   | reinterpret struct | 8-byte u64/i64/f64 explicit-layout union the coder reads     |

- [01]-[DECOMPRESS_MASK]: `CHANNEL_RETURNS_XY` `Z` `CLASSIFICATION` `FLAGS` `INTENSITY` `SCAN_ANGLE` `USER_DATA` `POINT_SOURCE` `GPS_TIME` `RGB` `NIR` `WAVEPACKET` `BYTE0`..`BYTE7` `EXTRA_BYTES` `ALL` — OR the channels a fit reads.

[PUBLIC_TYPE_SCOPE]: header, point, and record model

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]         | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :-------------------- | :---------------------------------------------------- |
|  [01]   | `laszip_point`                                     | point record          | the decoded point the cursor fills in place           |
|  [02]   | `laszip_header`                                    | public header         | scale/offset/extrema, point counts, the VLR/EVLR list |
|  [03]   | `laszip_vlr`                                       | VLR record            | CRS WKT (record_id 2112), GeoTIFF keys, class lookup  |
|  [04]   | `laszip_evlr`                                      | extended VLR          | 64-bit-length EVLR — large CRS/waveform records       |
|  [05]   | `laszip_geokey`                                    | GeoTIFF geokey        | one GeoTIFF CRS geokey entry the geokey VLR carries   |
|  [06]   | `LASattribute` / `LASattributer` / `LAS_ATTRIBUTE` | extra-bytes attribute | ASPRS extra-bytes descriptor + typed per-point reader |

- [02]-[POINT_FIELDS]: `laszip_point` decodes in place — `X`/`Y`/`Z` int (`X * scale + offset` → real-world XYZ), `intensity`, `gps_time`, `rgb` (`ushort[4]` = RGB+NIR), `wave_packet`, `extra_bytes`; the class byte reads `classification` (format 0-5) or `extended_classification` (format 6-10, selected by `extended_point_type`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and stream-decode a LAS/LAZ source
- note: `create()` mints the codec, `open_reader`/`open_reader_stream` opens a file or `ReadOnlyMemory<byte>`-backed stream and reports `is_compressed`, the forward loop calls `read_point()` filling `point`, and `get_coordinates` lifts real-world XYZ. Every method returns an `int` status (0 = success); `get_error()`/`get_warning()` carry the message, trapped onto `Fin<T>` at the boundary.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `laszip.create() -> laszip`                                                  | static   | mint the codec object                 |
|  [02]   | `open_reader(string, out bool is_compressed)`                                | instance | open a `.las`/`.laz` file             |
|  [03]   | `open_reader_stream(Stream, out bool is_compressed, bool leaveOpen = false)` | instance | open from a caller stream             |
|  [04]   | `get_header_pointer() -> laszip_header` / `header`                           | property | the public header (scale/offset/VLRs) |
|  [05]   | `get_point_count(out long)` / `get_number_of_point(out long)`                | instance | total point count, standard/extended  |
|  [06]   | `read_point()`                                                               | instance | decode the next point into `point`    |
|  [07]   | `get_point_pointer() -> laszip_point` / `point`                              | property | the decoded point the cursor filled   |
|  [08]   | `get_coordinates(double[])`                                                  | instance | real-world XYZ of the current point   |
|  [09]   | `seek_point(long index)`                                                     | instance | random-access seek to a point index   |
|  [10]   | `read_evlrs()` / `header.vlrs`                                               | instance | the VLR/EVLR set (CRS WKT id 2112)    |
|  [11]   | `close_reader()`                                                             | instance | release the reader and the stream     |
|  [12]   | `get_error() -> string` / `get_warning() -> string`                          | instance | the last error/warning message        |

[ENTRYPOINT_SCOPE]: selective decompression and `.lax` spatial-index query
- note: `decompress_selective` set before the loop skips channels a fit ignores; the `.lax` path `has_spatial_index` → `inside_rectangle` → `exploit_spatial_index` → `read_inside_point` decodes only points inside a bbox window over a built index.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `decompress_selective(LASZIP_DECOMPRESS_SELECTIVE)`                      | instance | mask the per-channel decode      |
|  [02]   | `has_spatial_index(out bool is_indexed, out bool is_appended)`           | instance | test for a `.lax` spatial index  |
|  [03]   | `inside_rectangle(double min_x, min_y, max_x, max_y, out bool is_empty)` | instance | set the bbox query window        |
|  [04]   | `exploit_spatial_index(bool exploit)`                                    | instance | index-accelerated windowed reads |
|  [05]   | `read_inside_point(out bool is_done)`                                    | instance | next point in the bbox window    |
|  [06]   | `create_spatial_index(bool create, bool append)`                         | instance | build/append a `.lax` index      |

[ENTRYPOINT_SCOPE]: write a LAS/LAZ file (symmetric egress)
- note: `set_header` + `set_point_type_and_size` configure, `open_writer(file, compress)` opens (`compress` selects LAZ), the loop fills `point` and calls `write_point`, and `set_geokeys`/`add_vlr` author the CRS — the LAZ-compressed emit `Themis.Las` cannot produce.

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `set_header(laszip_header)`                                                         | instance | configure the output header    |
|  [02]   | `set_point_type_and_size(byte point_type, ushort point_size)`                       | instance | set the output point format    |
|  [03]   | `open_writer(string, bool compress)`                                                | instance | open the file writer           |
|  [04]   | `open_writer_stream(Stream, bool compress, bool do_not_write_header, bool = false)` | instance | open a stream writer           |
|  [05]   | `set_point(laszip_point)` / `point`                                                 | instance | set the next point's fields    |
|  [06]   | `set_coordinates(double[])`                                                         | instance | set the real-world XYZ         |
|  [07]   | `write_point()`                                                                     | instance | encode the current point       |
|  [08]   | `write_indexed_point()`                                                             | instance | encode + feed the `.lax` index |
|  [09]   | `set_geokeys(ushort number, laszip_geokey[])`                                       | instance | author the GeoTIFF CRS geokeys |
|  [10]   | `add_vlr(laszip_vlr)`                                                               | instance | author a VLR                   |
|  [11]   | `update_inventory()`                                                                | instance | finalize header counts/extrema |
|  [12]   | `close_writer()`                                                                    | instance | flush and close                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `laszip` is the one C-API codec object: `create()` mints it, `open_reader`/`open_reader_stream` reports `is_compressed` so ONE reader decodes both `.las` and `.laz` (arithmetic decoder vs raw read), never a per-format or per-compression reader family
- `read_point()` fills one `point` and one `header` in place with no per-point allocation, and `get_coordinates(double[])` lifts the raw `X`/`Y`/`Z` ints to real-world doubles through header scale/offset into one reused buffer — the collecting lift is the COPYING `Vector<double>.Build.DenseOfArray(xyz)`, never the array-WRAPPING `Dense(double[])` that aliases every position onto the mutating buffer
- `extended_point_type` keys the class channel: a nonzero value selects `extended_classification` (the format-6-10 full class byte), while the format-0-5 `classification` getter masks `& 0x1F` and truncates an extended record, so the one correct read is `extended_point_type != 0 ? extended_classification : classification`
- `decompress_selective(LASZIP_DECOMPRESS_SELECTIVE)` set before the loop masks the per-channel decode: a fit reading position + classification sets `CHANNEL_RETURNS_XY | Z | CLASSIFICATION` and the arithmetic decoder SKIPS RGB/waveform/extra-bytes — a decode-time cost reduction the uncompressed reader has no analog for
- `has_spatial_index` → `inside_rectangle(bbox)` → `exploit_spatial_index(true)` → `read_inside_point` is the bbox-windowed read: only points inside the query rectangle decode over the built `.lax` index — the LAZ counterpart of the FGB Packed-R-tree bbox filter
- `header.vlrs`/`read_evlrs` carry the OGC WKT CRS (record_id 2112), the GeoTIFF geokeys, and the classification lookup, read from the surface, never a re-minted CRS parser

[STACKING]:
- `Themis.Las`(`.api/api-themis-las`): `reconstruct#LAS_INGEST` `LasIngest.Decode` folds this compressed leg and the Themis uncompressed leg as one ingest front — `LasCompression.Sniff` (public-header point-data-format byte at offset 104, high bit = LASzip) selects the engine once without a full open, `get_coordinates` lifts the decoded point into the same `MathNet.Numerics` `Vector<double>` the Themis `LasPoint.Position` already is; one `LasCloud`, two engines, the kernel registration agnostic to which decoded
- `csharp:Rasm.Compute/Tensor/blas`(`#DENSE_ALGEBRA`): the lifted `Vector<double>` enters the Compute dense-LA substrate — the `csharp:Rasm/Processing/register#ALIGN` cloud-ICP registration and covariance/PCA normal estimation — with no second point model
- content identity: the `.laz`/`.las` bytes content-key as the `reconstruct#RECONSTRUCTION` `ReconstructionLineage` `[ValueObject<UInt128>]` through the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` composed via the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter`, never the upper-stratum `Rasm.Compute` `InterchangeIdentity` (that reference inverts the strata DAG)
- classification seed: the format-correct class read folds into `LasCloud.ClassHistogram` and reduces to the `SegmentedCloud.DominantClass` the `reconstruct#RECONSTRUCTION` `AsprsBias` policy and `ElementClassifier` `(PrimitiveShape, IfcDomain, orientation)` table key on, feeding the same classifier as the Themis leg
- georeference: the `header.vlrs` CRS WKT (record_id 2112) and GeoTIFF geokeys land on `LasCloud.CrsWkt`, lowered onto the `Header.Reference` `GeoReference` through the `csharp:Rasm.Bim/Semantics/georeference#GEO_PROJECTION` `ProjNET` datum leg
- spatial-index seam: the `.lax` `inside_rectangle` windowed read decodes only the points inside the active building footprint — the bbox push-down projected to the point cloud

[LOCAL_ADMISSION]:
- `laszip.create()` + `open_reader` is the only LAZ codec root; one reader decodes every point format by the header `point_data_format` and both compression states by `is_compressed`
- `get_coordinates` + the copying `Vector<double>.Build.DenseOfArray` lift the decoded point into the kernel `MathNet.Numerics` vector, the class channel reads `extended_point_type != 0 ? extended_classification : classification`, and the raw `laszip_point`/`laszip_header` types never leak past the `LasIngest` fold — internal code holds the canonical `LasCloud`/`SegmentedCloud`
- a fit reading only position + classification sets `decompress_selective` to skip the unneeded channels
- a tiled/windowed ingest enters through the `.lax` `inside_rectangle` + `exploit_spatial_index` path when an index exists
- `int` status returns and `get_error()`/`get_warning()` messages trap onto `Fin<T>`/`Model/faults#FAULT_BAND` `BimFault.CodecReject` at the boundary
- `Unofficial.laszip.netstandard` is referenced as a SEPARATE assembly (`PackageReference`), never ILMerged into a Rasm assembly; the pure-managed ns2.0 IL binds forward and the in-Rhino plugin ALC firebreak holds

[RAIL_LAW]:
- Package: `Unofficial.laszip.netstandard` (LGPL-2.1 weak copyleft, pure-managed `lib/netstandard2.0` AnyCPU IL, zero managed dependencies)
- Owns: the managed LASzip C-API codec — `.las`/`.laz` arithmetic-coded read AND write (point formats 0-10), selective-channel decompression, the `.lax` spatial-index bbox query, and the full ASPRS header/point/VLR/EVLR/geokey/extra-bytes model
- Accept: the `reconstruct#LAS_INGEST` LAZ-capable decode leg lifting the raw point set through the copying `Vector<double>.Build.DenseOfArray` into the kernel `MathNet.Numerics` vector, the format-correct class channel, every `int` status gated onto the `Fin<T>` funnel, selective decode via `decompress_selective`, windowed reads via the `.lax` `inside_rectangle` path, and the CRS WKT VLR threaded onto `Semantics/georeference#GEO_PROJECTION`
- Reject: a re-minted point-cloud scan/segmentation/registration engine (the kernel owns it); a second point model beside the `Themis.Las` `LasPoint`/`LasCloud`; a hand-rolled LAZ arithmetic decoder or a native libLASzip binding; the format-0-5 `classification` getter on a format-6-10 record or the wrapping `Dense(double[])` lift over the reused buffer; a second hashing scheme or the upper-stratum `Rasm.Compute` `InterchangeIdentity` beside the kernel `ContentHash`/seam `CanonicalWriter` lineage; a laszip-local CRS reprojection beside `ProjNET`; decoding channels `decompress_selective` masks
