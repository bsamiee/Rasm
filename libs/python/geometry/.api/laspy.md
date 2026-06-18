# [PY_GEOMETRY_API_LASPY]

`laspy` supplies LAS/LAZ point-cloud file IO through `LasReader`, `LasWriter`, `LasAppender`, and `CopcReader` for streaming or in-memory read/write of LAS 1.0–1.4 and LAS 2.0 files, including compressed LAZ via `LazBackend`, COPC spatial indexing, extra-dimension management, CRS handling, and VLR/EVLR access for the geometry scan-processing rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy`
- module: `laspy`
- asset: pure-Python wheel, cp315 available
- rail: scan-processing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IO handles
- rail: scan-processing

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------ | :------------ | :--------------------------------------------------- |
|   [1]   | `LasReader`   | file reader   | streaming chunk read, seek, evlr access              |
|   [2]   | `LasWriter`   | file writer   | streaming point write, evlr write                    |
|   [3]   | `LasAppender` | file appender | append points to existing LAS/LAZ file               |
|   [4]   | `CopcReader`  | COPC reader   | spatial/level query against Cloud-Optimized PC files |

[PUBLIC_TYPE_SCOPE]: data containers
- rail: scan-processing

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                               |
| :-----: | :---------------------- | :---------------- | :--------------------------------------------------------- |
|   [1]   | `LasData`               | in-memory file    | `points`, `header`, `vlrs`, `xyz`, `write(dest)`           |
|   [2]   | `LasHeader`             | header record     | version, point format, scale/offset, bounds, CRS           |
|   [3]   | `PackedPointRecord`     | point buffer      | contiguous packed per-point storage                        |
|   [4]   | `ScaleAwarePointRecord` | scaled buffer     | per-point storage with scale/offset applied                |
|   [5]   | `PointFormat`           | format descriptor | standard/extra dimensions, dtype, size                     |
|   [6]   | `VLR`                   | variable record   | `user_id`, `record_id`, `description`, `record_data_bytes` |
|   [7]   | `DimensionInfo`         | dimension meta    | kind, dtype, name, min/max, scales/offsets                 |
|   [8]   | `ExtraBytesParams`      | extra dim spec    | parameter object for `add_extra_dim`                       |
|   [9]   | `Bounds`                | spatial bounds    | `overlaps(other)`, `ensure_3d()`                           |

[PUBLIC_TYPE_SCOPE]: enumerations
- rail: scan-processing

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :------------- | :-------------------------------------------------------------- |
|   [1]   | `LazBackend`             | LAZ engine     | `Laszip`, `Lazrs`, `LazrsParallel`                              |
|   [2]   | `DimensionKind`          | dimension type | `BitField`, `FloatingPoint`, `SignedInteger`, `UnsignedInteger` |
|   [3]   | `DecompressionSelection` | channel mask   | bitfield controlling which channels are decompressed            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level IO functions
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `read(source, closefd=True, laz_backend=(), decompression_selection=...) -> LasData`               | eager read     | load full file into `LasData`    |
|   [2]   | `open(source, mode='r', closefd=True, laz_backend=None, ...) -> LasReader\|LasWriter\|LasAppender` | streaming      | context-manager IO handle        |
|   [3]   | `create(*, point_format=None, file_version=None) -> LasData`                                       | creation       | new empty `LasData`              |
|   [4]   | `convert(source_las, *, point_format_id=None, file_version=None) -> LasData`                       | conversion     | re-format point record           |
|   [5]   | `mmap(filename) -> LasData`                                                                        | mmap read      | memory-mapped LAS file access    |
|   [6]   | `supported_point_formats() -> Set[int]`                                                            | query          | supported format IDs             |
|   [7]   | `supported_versions() -> Set[str]`                                                                 | query          | supported LAS version strings    |
|   [8]   | `lost_dimensions(point_fmt_in, point_fmt_out)`                                                     | query          | dimensions lost by format change |

[ENTRYPOINT_SCOPE]: LasReader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------ |
|   [1]   | `reader.read() -> LasData`                            | read           | read full file            |
|   [2]   | `reader.read_points(n) -> PackedPointRecord`          | streaming      | read next n points        |
|   [3]   | `reader.chunk_iterator(points_per_chunk) -> iterator` | streaming      | iterate fixed-size chunks |
|   [4]   | `reader.seek(point_index)`                            | navigation     | seek to point index       |
|   [5]   | `reader.read_evlrs()`                                 | metadata       | read extended VLRs        |
|   [6]   | `reader.close()`                                      | lifecycle      | close file handle         |

[ENTRYPOINT_SCOPE]: LasWriter, LasAppender, CopcReader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :-------------------------------- |
|   [1]   | `writer.write_points(points: PackedPointRecord)`                                | write          | write point block                 |
|   [2]   | `writer.write_evlrs(evlrs)`                                                     | metadata       | write extended VLRs               |
|   [3]   | `writer.close()`                                                                | lifecycle      | flush and close                   |
|   [4]   | `appender.append_points(points: PackedPointRecord)`                             | append         | add points to existing file       |
|   [5]   | `appender.close()`                                                              | lifecycle      | flush and close                   |
|   [6]   | `CopcReader.open(source, ...) -> CopcReader`                                    | creation       | open COPC file or HTTP URL        |
|   [7]   | `copc.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord` | spatial        | combined spatial+resolution query |
|   [8]   | `copc.spatial_query(bounds) -> ScaleAwarePointRecord`                           | spatial        | bounding-box query                |
|   [9]   | `copc.level_query(level) -> ScaleAwarePointRecord`                              | spatial        | octree level query (int or range) |

[ENTRYPOINT_SCOPE]: LasHeader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `LasHeader.read_from(stream) -> LasHeader`          | construction   | read header from file stream      |
|   [2]   | `header.add_crs(crs)`                               | metadata       | attach CRS to header              |
|   [3]   | `header.parse_crs()`                                | metadata       | parse CRS from VLRs               |
|   [4]   | `header.add_extra_dim(params: ExtraBytesParams)`    | schema         | register extra dimension          |
|   [5]   | `header.set_version_and_point_format(version, fmt)` | mutation       | change version and format         |
|   [6]   | `header.grow(other_header)`                         | mutation       | expand bounds to include other    |
|   [7]   | `header.update(points)`                             | mutation       | recompute stats from point record |

## [4]-[IMPLEMENTATION_LAW]

[IO_TOPOLOGY]:
- `open(source, mode='r')` returns `LasReader`; `mode='w'` returns `LasWriter`; `mode='a'` returns `LasAppender`.
- `LasData.xyz` is a convenience `(N, 3)` float64 array of `x`, `y`, `z` with scale/offset applied.
- Compressed LAZ requires a `LazBackend`: `Laszip` needs the `laszip` native library; `Lazrs`/`LazrsParallel` are pure-Rust via `lazrs-python`.
- `PackedPointRecord` is the raw per-format storage layout; `ScaleAwarePointRecord` wraps it with `x_scale`, `x_offset` semantics for COPC output.
- Extra dimensions are added via `ExtraBytesParams` and persist in VLRs; `PointFormat.extra_dimension_names` lists them.
- `DecompressionSelection` is a bitfield; pass a subset to decompress only needed channels and skip decompression of others.

[LOCAL_ADMISSION]:
- Scan data enters via `read(path)` for small files or `open(path)` context-manager for streaming large/LAZ files.
- COPC HTTP access uses `CopcReader.open(url)` and `query` for spatially bounded extraction without full download.
- Header scale/offset are read from `LasHeader.x_scale`/`x_offset` etc.; the consumer must apply them for metric coordinates unless using `LasData.xyz` or `ScaleAwarePointRecord`.

[RAIL_LAW]:
- Package: `laspy`
- Owns: LAS/LAZ point-cloud file read, write, append, format conversion, and COPC spatial query
- Accept: LAS 1.0–2.0 files, LAZ compressed files via LazBackend, COPC files or HTTP URLs
- Reject: hand-rolled binary LAS parsing, direct byte manipulation of point records
