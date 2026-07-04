# [RASM_BIM_API_LASZIP]

`Unofficial.laszip.netstandard` is the pure-managed C# port of LASzip (Martin Isenburg /
rapidlasso) — the `LASzip.Net` namespace — backing the `Exchange/reconstruct#LAS_INGEST`
LAZ-capable arm. It is the full LASzip C-API mirror: a `laszip` object that OPENS a `.las`
or `.laz` stream (reporting `is_compressed` so one reader handles both), reads the public
header + VLR/EVLR records (the OGC WKT / GeoTIFF CRS keys), streams points through the
arithmetic-coded decompressor into a `laszip_point`, extracts real-world coordinates through
`get_coordinates`, and writes the symmetric compressed/uncompressed leg. It carries the
LASzip differentiators the BCL-managed uncompressed reader cannot: arithmetic-coded LAZ
DECODE/ENCODE (point formats 0-10 over the `LASreadItemCompressed_*`/`LASwriteItemCompressed_*`
item set), SELECTIVE decompression (`LASZIP_DECOMPRESS_SELECTIVE` — read only the channels a
fit needs, skipping RGB/waveform/extra-bytes), and the `.lax` SPATIAL-INDEX query
(`has_spatial_index`/`inside_rectangle`/`exploit_spatial_index`/`read_inside_point` —
bbox-windowed reads over a built spatial index). The `Exchange/reconstruct#LAS_INGEST` fold
composes it as the LAZ leg PEER of the admitted `Themis.Las` (`api-themis-las`) uncompressed
reader: `Themis.Las` owns the `MathNet.Numerics`-vector `LasPoint` facet model the kernel
registration consumes, this owns the compressed-stream decode that produces the raw point set
to lift into that model. LGPL-2.1 — a WEAK-COPYLEFT separate-assembly `PackageReference`, never
ILMerged into a Rasm assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Unofficial.laszip.netstandard`
- package: `Unofficial.laszip.netstandard`
- version: `5.6.2`
- license: LGPL-2.1 (verified from the repo `LICENSE` — "GNU LESSER GENERAL PUBLIC LICENSE Version 2.1"; the nuspec ships only a `licenseUrl`, NO embedded SPDX expression, so the catalog records the verified file identity); weak copyleft — admitted as a separate-assembly `PackageReference`, never statically merged into a Rasm assembly
- assembly: `Unofficial.laszip.netstandard` → the `net10.0` consumer binds `lib/netstandard2.0/Unofficial.laszip.netstandard.dll` (the sole `lib/` TFM, binds forward under net10; pure-managed AnyCPU IL, ALC-safe, no per-RID native asset — the LASzip codec is C#, not a native libLASzip binding)
- namespace: `LASzip.Net`
- dependency: zero managed dependencies (the nuspec declares no `<dependency>` group)
- attribution: a C# port (Shinta, 2014-2017) of the C++ LASzip (Isenburg / rapidlasso); the arithmetic coder and the per-format compressed-item decode/encode are ported in managed C#, NOT P/Invoked
- scope: ASPRS LAS 1.0-1.4 point formats 0-10 read AND write, LAZ arithmetic-coded compression/decompression, selective-channel decompression, the `.lax` spatial-index bbox query, and the full header/VLR/EVLR/geokey model
- rail: `Exchange/reconstruct#LAS_INGEST` (the LAZ-capable point-cloud decode leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec root and selective-decompress vocabulary
- namespace: `LASzip.Net`
- rail: reconstruct
- note: `laszip` is the one codec object — created through the static `create()`, then opened for read or write; it holds a single `header` and a single `point` the read cursor fills in place (no per-point allocation). One object decodes EVERY point format by the header's `point_data_format`, never a per-format reader family.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [RAIL]                                                              |
| :-----: | :-------------------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `laszip`                          | LAS/LAZ codec      | the C-API codec object (`create()`/`open_reader`/`read_point`/`open_writer`/`write_point`/`close_*`); holds `header` + `point` |
|  [02]   | `LASZIP_DECOMPRESS_SELECTIVE`     | channel-select enum | `enum : uint` flags — `CHANNEL_RETURNS_XY`/`Z`/`CLASSIFICATION`/`FLAGS`/`INTENSITY`/`SCAN_ANGLE`/`USER_DATA`/`POINT_SOURCE`/`GPS_TIME`/`RGB`/`NIR`/`WAVEPACKET`/`BYTE0..7`/`EXTRA_BYTES`/`ALL` — the per-channel decode mask |
|  [03]   | `U64I64F64`                       | reinterpret union  | the `[StructLayout(Explicit)]` 8-byte u64/i64/f64 the arithmetic coder reinterprets through |

[PUBLIC_TYPE_SCOPE]: header, point, and record model
- namespace: `LASzip.Net`
- rail: reconstruct
- note: the `laszip_point` is the decoded point the cursor fills; the `laszip_header` is the public header carrying scale/offset/extrema, the legacy + extended point counts, and the VLR/EVLR list where the CRS WKT lives. These are the raw ASPRS field carriers — the `Themis.Las` `LasPoint`/`ILasHeader` facet model is the canonical wrapper the kernel reads.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                                  |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `laszip_point`      | point record    | `X`/`Y`/`Z` (raw ints), `intensity`, `classification` (the LEGACY 5-bit getter — masks `& 0x1F`, TRUNCATING a format-6-10 record) / `extended_classification` (the full class byte; `extended_point_type` is set on reader open for formats 6-10, so the format-correct read is `extended_point_type != 0 ? extended_classification : classification`), `gps_time`, `rgb[4]` (RGB + NIR), `wave_packet[29]`, `extra_bytes`, the return/scan/flag bit-fields |
|  [02]   | `laszip_header`     | public header   | `version_*`, `point_data_format`, `x/y/z_scale_factor`/`offset`, `min/max_*`, `number_of_point_records` + `extended_number_of_point_records`, `number_of_points_by_return`, the VLR/EVLR offsets, `vlrs` (`List<laszip_vlr>`) |
|  [03]   | `laszip_vlr`        | VLR record      | `user_id[16]`/`record_id`/`record_length_after_header`/`description[32]`/`data` — the CRS WKT (record_id 2112) / GeoTIFF keys / classification lookup |
|  [04]   | `laszip_evlr`       | extended VLR    | the 64-bit-length EVLR (`record_length_after_header` is `ulong`) — large CRS / waveform records past the header |
|  [05]   | `laszip_geokey`     | GeoTIFF geokey  | one GeoTIFF CRS geokey entry (`key_id`/`tiff_tag_location`/`count`/`value_offset`) the geokey VLR carries |
|  [06]   | `LASattribute` / `LASattributer` / `LAS_ATTRIBUTE` | extra-bytes attribute | the "extra bytes" (ASPRS attribute) descriptor + the typed-attribute reader for per-point custom dimensions |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and stream-decode a LAS/LAZ source
- namespace: `LASzip.Net`
- rail: reconstruct
- note: `create()` mints the codec, `open_reader`/`open_reader_stream` opens a file/stream and reports `is_compressed` (one reader handles `.las` AND `.laz`), the forward loop calls `read_point()` filling `point`, and `get_coordinates` extracts the real-world XYZ (`X * scale + offset`). The `Stream` overload is the in-memory `ReadOnlyMemory<byte>`-backed read the object-store transport supplies. Every method returns an `int` status (0 = success); `get_error()`/`get_warning()` carry the message — the boundary traps these onto `Fin<T>`.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :----------------------------------------------- |
|  [01]   | `laszip.create()`                                                               | static → `laszip`         | mint the codec object                            |
|  [02]   | `open_reader(string file_name, out bool is_compressed)`                          | → `int`                   | open a `.las`/`.laz` file (reports compression)  |
|  [03]   | `open_reader_stream(Stream streamin, out bool is_compressed, bool leaveOpen = false)` | → `int`             | open from a caller stream (the in-memory transport read) |
|  [04]   | `get_header_pointer()` / `header`                                               | → `laszip_header`         | the public header (scale/offset/extrema, VLRs)   |
|  [05]   | `get_point_count(out long count)` / `get_number_of_point(out long npoints)`      | → `int`                   | the total point count (legacy or extended)       |
|  [06]   | `read_point()`                                                                  | → `int`                   | decode the next point into `point` (no allocation) |
|  [07]   | `get_point_pointer()` / `point`                                                 | → `laszip_point`          | the decoded point the cursor filled              |
|  [08]   | `get_coordinates(double[] coordinates)`                                         | → `int`                   | the real-world XYZ of the current point (`raw * scale + offset`) |
|  [09]   | `seek_point(long index)`                                                        | → `int`                   | random-access seek to a point index              |
|  [10]   | `read_evlrs()` / `header.vlrs`                                                  | → `int` / `List<laszip_vlr>` | the VLR/EVLR set carrying the CRS WKT (record_id 2112) |
|  [11]   | `close_reader()`                                                                | → `int`                   | release the reader and the stream                |
|  [12]   | `get_error()` / `get_warning()`                                                 | → `string`                | the last error / warning message (the rail traps onto `Fin<T>`) |

[ENTRYPOINT_SCOPE]: selective decompression and `.lax` spatial-index query
- namespace: `LASzip.Net`
- rail: reconstruct
- note: `decompress_selective` (set BEFORE the read loop) skips the channels a fit does not need — a plane/cylinder fit reading only `CHANNEL_RETURNS_XY | Z | CLASSIFICATION` skips RGB/waveform/extra-bytes decode entirely; the `.lax` spatial-index path (`has_spatial_index` → `inside_rectangle` → `exploit_spatial_index` → `read_inside_point`) reads only points inside a bbox window over a built index — the LAZ counterpart of the FGB Packed-R-tree bbox filter.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]   | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `decompress_selective(LASZIP_DECOMPRESS_SELECTIVE decompress_selective)`         | → `int`        | mask the per-channel decode (skip RGB/waveform/extra-bytes a fit ignores) |
|  [02]   | `has_spatial_index(out bool is_indexed, out bool is_appended)`                   | → `int`        | test for a `.lax` spatial index                  |
|  [03]   | `inside_rectangle(double min_x, double min_y, double max_x, double max_y, out bool is_empty)` | → `int` | set the bbox query window                        |
|  [04]   | `exploit_spatial_index(bool exploit)`                                            | → `int`        | enable index-accelerated windowed reads          |
|  [05]   | `read_inside_point(out bool is_done)`                                           | → `int`        | read the next point inside the bbox window (`is_done` = exhausted) |
|  [06]   | `create_spatial_index(bool create, bool append)`                                | → `int`        | build/append a `.lax` index on the writer leg    |

[ENTRYPOINT_SCOPE]: write a LAS/LAZ file (symmetric egress)
- namespace: `LASzip.Net`
- rail: reconstruct
- note: the writer leg authors a `.las`/`.laz` — `set_header` + `set_point_type_and_size` configure, `open_writer(file, compress)` opens (the `compress` flag selects LAZ), the loop fills `point` and calls `write_point`, and `set_geokeys`/`add_vlr` author the CRS. Distinct from the `Themis.Las` writer — admitted here for the LAZ-compressed emit Themis cannot produce.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]   | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `set_header(laszip_header header)` / `set_point_type_and_size(byte point_type, ushort point_size)` | → `int` | configure the output header and point format |
|  [02]   | `open_writer(string file_name, bool compress)` / `open_writer_stream(Stream streamout, bool compress, bool do_not_write_header, bool leaveOpen = false)` | → `int` | open the writer (`compress` = LAZ) |
|  [03]   | `set_point(laszip_point point)` / `point` + `set_coordinates(double[] coordinates)` | → `int`   | set the next point's fields / real-world XYZ     |
|  [04]   | `write_point()` / `write_indexed_point()`                                       | → `int`        | encode the current point (indexed variant feeds `.lax`) |
|  [05]   | `set_geokeys(ushort number, laszip_geokey[] key_entries)` / `add_vlr(laszip_vlr vlr)` | → `int`  | author the GeoTIFF CRS geokeys / a VLR           |
|  [06]   | `update_inventory()` / `close_writer()`                                         | → `int`        | finalize the header counts/extrema and close     |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_TOPOLOGY]:
- `laszip` is the one C-API codec object: `create()` mints it, `open_reader`/`open_reader_stream` opens a source and reports `is_compressed` so ONE reader decodes both `.las` and `.laz` (the compressed path runs the arithmetic decoder, the uncompressed path a raw read) — never a per-format or per-compression reader family
- the read cursor fills a single `point` (`laszip_point`) and a single `header` (`laszip_header`) in place; `read_point()` decodes into `point` with no per-point allocation, and `get_coordinates(double[])` lifts the raw `X`/`Y`/`Z` ints to real-world doubles through the header `scale`/`offset` into ONE reused caller buffer — the collecting lift is the COPYING `Vector<double>.Build.DenseOfArray(xyz)`, never the array-WRAPPING `Dense(double[])` that would alias every position onto the one mutating buffer (the laszip mirror of the Themis `Position.Clone()` law)
- the class channel is FORMAT-KEYED: `extended_point_type` is set on reader open for a LAS 1.4 format-6-10 record, whose full class byte lives in `extended_classification` — the legacy `classification` getter masks `& 0x1F` and TRUNCATES an extended record to garbage, so the one correct read is `extended_point_type != 0 ? extended_classification : classification`
- `decompress_selective(LASZIP_DECOMPRESS_SELECTIVE)` is set BEFORE the loop to mask the per-channel decode: a primitive fit reading only position + classification sets `CHANNEL_RETURNS_XY | Z | CLASSIFICATION` and the arithmetic decoder SKIPS the RGB/waveform/extra-bytes channels — a decode-time cost reduction the uncompressed reader has no analog for
- the `.lax` spatial index (`has_spatial_index` → `inside_rectangle(bbox)` → `exploit_spatial_index(true)` → `read_inside_point`) is the bbox-windowed read: only points inside the query rectangle are decoded over the built index — the LAZ counterpart of the FGB Packed-R-tree bbox filter and the GDAL `Layer.SetSpatialFilterRect` push-down
- the VLR/EVLR records (`header.vlrs`, `read_evlrs`) carry the OGC WKT CRS (record_id 2112), the GeoTIFF geokeys, and the classification lookup — read from the package surface, never a re-minted CRS parser

[INTEGRATION_STACK]:
- with `Themis.Las` (`api-themis-las`): the `Exchange/reconstruct#LAS_INGEST` fold composes the TWO as one ingest front discriminated by compression — `Themis.Las.LasReader` decodes the `.las` uncompressed leg producing the `LasPoint` whose `Position` is already a `MathNet.Numerics.LinearAlgebra.Vector<double>`, and `LASzip.Net.laszip` decodes the `.laz` compressed leg producing a raw `laszip_point` the fold lifts into the SAME `MathNet` vector through `get_coordinates` + the copying `Vector<double>.Build.DenseOfArray(xyz)` (the array-WRAPPING `Dense(double[])` would alias every position onto the one reused coordinate buffer); one `LasCloud` carrier, two decode engines, the kernel registration agnostic to which decoded — the `reconstruct#LAS_INGEST` `LasIngest.Decode` dispatches ONCE on `LasCompression.Sniff` (the public-header point-data-format byte at offset 104, high bit = LASzip) without a full open, never two parallel ingest models and never an extension-string branch
- MathNet seam: the decoded `get_coordinates` `double[]` lifts to the `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel `csharp:Rasm/Processing/register#ALIGN` cloud-ICP registration and the `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA` covariance/PCA normal estimation consume — the LAZ point batch enters the Compute dense-LA substrate through the same vector the `Themis.Las` uncompressed point already is, no second point model
- content identity: the source-cloud `.laz`/`.las` bytes content-key as the `reconstruct#RECONSTRUCTION` `ReconstructionLineage` `[ValueObject<UInt128>]` — the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` composed through the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter`, never a second hashing scheme over the LAS/LAZ bytes and never the upper-stratum `Rasm.Compute` `InterchangeIdentity` (a `Rasm.Bim`→`Rasm.Compute` reference inverts the strata DAG — the leak the reconstruct rebuild closed)
- classification seed: the FORMAT-CORRECT class read `extended_point_type != 0 ? extended_classification : classification` (the legacy getter's `& 0x1F` mask truncates a LAS 1.4 format-6-10 record) folds into the `LasCloud.ClassHistogram` and reduces to the `SegmentedCloud.DominantClass` the `reconstruct#RECONSTRUCTION` `AsprsBias` policy and `ElementClassifier` `(PrimitiveShape, IfcDomain, orientation)` table key on, exactly as the Themis leg's `& 0x1F`-masked `LasPoint.Classification` does (the mask the legacy getter applies internally, Themis surfacing the raw byte) — the LAZ leg feeds the same classifier, never a per-codec classification branch
- georeference: the `header.vlrs` CRS WKT (record_id 2112) and the GeoTIFF geokeys land on `LasCloud.CrsWkt`, which the app lowers onto the `Header.Reference` `GeoReference` through the `csharp:Rasm.Bim/Semantics/georeference#GEO_PROJECTION` `ProjNET` datum leg — the same georeferencing seam the `Themis.Las` VLR reads, never a laszip-local reprojection
- spatial-index seam: the `.lax` `inside_rectangle` windowed read lets a tiled scan-to-BIM ingest decode only the points inside the active building footprint — the bbox push-down the FGB/GeoParquet web codecs run for vector, projected to the point cloud

[LOCAL_ADMISSION]:
- `laszip.create()` + `open_reader` is the only LAZ codec root; one reader decodes every point format by the header `point_data_format` and both compression states by `is_compressed`, never a per-format/per-compression reader family
- the decoded point lifts through `get_coordinates` + the copying `Vector<double>.Build.DenseOfArray` into the `MathNet.Numerics` vector the kernel consumes (the same vector `Themis.Las.LasPoint.Position` is; the wrapping `Dense(double[])` over the reused buffer is the rejected aliasing form), the class channel reads `extended_point_type != 0 ? extended_classification : classification`, and the raw `laszip_point`/`laszip_header` types never leak past the `LasIngest` fold — internal code holds the canonical `LasCloud`/`SegmentedCloud` per the boundary-mapping law
- a fit reading only position+classification sets `decompress_selective` to skip the unneeded channels; decoding RGB/waveform/extra-bytes a fit ignores is the rejected decode-cost form
- a tiled/windowed ingest enters through the `.lax` `inside_rectangle` + `exploit_spatial_index` path when an index exists; decoding the whole cloud to filter by bbox in-memory is the rejected form when a `.lax` index is present
- the `int` status returns and `get_error()`/`get_warning()` messages are trapped onto `Fin<T>`/`Model/faults#FAULT_BAND` `BimFault.CodecReject` at the boundary; a domain branch on a raw status code is the rejected form
- LGPL-2.1 weak copyleft holds: the package is referenced as a SEPARATE assembly (`PackageReference`), never ILMerged/ILRepacked into a Rasm assembly, and the in-Rhino plugin ALC firebreak is preserved (the pure-managed ns2.0 IL binds forward, no native binding)

[RAIL_LAW]:
- Package: `Unofficial.laszip.netstandard` (5.6.2, LGPL-2.1 weak copyleft, pure-managed `lib/netstandard2.0` AnyCPU IL binding under net10, zero managed dependencies)
- Owns: the managed LASzip C-API codec — `.las`/`.laz` arithmetic-coded read AND write (point formats 0-10), selective-channel decompression, the `.lax` spatial-index bbox query, and the full ASPRS header/point/VLR/EVLR/geokey/extra-bytes model
- Accept: the `Exchange/reconstruct#LAS_INGEST` LAZ-capable decode leg producing the raw point set lifted through the copying `Vector<double>.Build.DenseOfArray` into the `MathNet.Numerics` vector the kernel registration consumes, the format-correct class channel (`extended_point_type != 0 ? extended_classification : classification`), every `int` status gated onto the `Fin<T>` funnel, selective decode via `decompress_selective`, windowed reads via the `.lax` `inside_rectangle` path, and the CRS WKT VLR threaded onto `Semantics/georeference#GEO_PROJECTION`
- Reject: a re-minted point-cloud scan/segmentation/registration engine (the kernel owns it); a second point model beside the `Themis.Las` `LasPoint`/`LasCloud`; a hand-rolled LAZ arithmetic decoder or a native libLASzip binding; the legacy `classification` getter on a format-6-10 record (the 5-bit mask truncates it) or the wrapping `Dense(double[])` lift over the reused coordinate buffer; a second hashing scheme over the LAS/LAZ bytes or the upper-stratum `Rasm.Compute` `InterchangeIdentity` beside the kernel `ContentHash` seed-zero `XxHash128`/seam `CanonicalWriter` lineage; a laszip-local CRS reprojection beside `ProjNET`; decoding unneeded channels where `decompress_selective` masks them; ILMerging the LGPL-2.1 assembly into a Rasm assembly
