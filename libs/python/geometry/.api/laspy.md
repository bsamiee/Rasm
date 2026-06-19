# [PY_GEOMETRY_API_LASPY]

`laspy` supplies LAS/LAZ point-cloud file IO through `LasReader`, `LasWriter`, `LasAppender`, and `CopcReader` for streaming or in-memory read/write of LAS 1.0–1.4 and LAS 2.0 files, including compressed LAZ via `LazBackend`, COPC spatial indexing, extra-dimension management, CRS handling, and VLR/EVLR access for the geometry scan-processing rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy`
- module: `laspy`
- asset: pure-Python wheel, cp315 available
- rail: scan-processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IO handles
- rail: scan-processing

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------ | :------------ | :--------------------------------------------------- |
|  [01]   | `LasReader`   | file reader   | streaming chunk read, seek, evlr access              |
|  [02]   | `LasWriter`   | file writer   | streaming point write, evlr write                    |
|  [03]   | `LasAppender` | file appender | append points to existing LAS/LAZ file               |
|  [04]   | `CopcReader`  | COPC reader   | spatial/level query against Cloud-Optimized PC files |

[PUBLIC_TYPE_SCOPE]: data containers
- rail: scan-processing

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                               |
| :-----: | :---------------------- | :---------------- | :--------------------------------------------------------- |
|  [01]   | `LasData`               | in-memory file    | `points`, `header`, `vlrs`, `xyz`, `write(dest)`           |
|  [02]   | `LasHeader`             | header record     | version, point format, scale/offset, bounds, CRS           |
|  [03]   | `PackedPointRecord`     | point buffer      | contiguous packed per-point storage                        |
|  [04]   | `ScaleAwarePointRecord` | scaled buffer     | per-point storage with scale/offset applied                |
|  [05]   | `PointFormat`           | format descriptor | standard/extra dimensions, dtype, size                     |
|  [06]   | `VLR`                   | variable record   | `user_id`, `record_id`, `description`, `record_data_bytes` |
|  [07]   | `DimensionInfo`         | dimension meta    | kind, dtype, name, min/max, scales/offsets                 |
|  [08]   | `ExtraBytesParams`      | extra dim spec    | parameter object for `add_extra_dim`                       |
|  [09]   | `Bounds`                | spatial bounds    | `overlaps(other)`, `ensure_3d()`                           |

[PUBLIC_TYPE_SCOPE]: enumerations
- rail: scan-processing

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `LazBackend`             | LAZ engine     | `Laszip`, `Lazrs`, `LazrsParallel`                              |
|  [02]   | `DimensionKind`          | dimension type | `BitField`, `FloatingPoint`, `SignedInteger`, `UnsignedInteger` |
|  [03]   | `DecompressionSelection` | channel mask   | bitfield controlling which channels are decompressed            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level IO functions
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `read(source, closefd=True, laz_backend=(), decompression_selection=...) -> LasData`               | eager read     | load full file into `LasData`    |
|  [02]   | `open(source, mode='r', closefd=True, laz_backend=None, ...) -> LasReader\|LasWriter\|LasAppender` | streaming      | context-manager IO handle        |
|  [03]   | `create(*, point_format=None, file_version=None) -> LasData`                                       | creation       | new empty `LasData`              |
|  [04]   | `convert(source_las, *, point_format_id=None, file_version=None) -> LasData`                       | conversion     | re-format point record           |
|  [05]   | `mmap(filename) -> LasData`                                                                        | mmap read      | memory-mapped LAS file access    |
|  [06]   | `supported_point_formats() -> Set[int]`                                                            | query          | supported format IDs             |
|  [07]   | `supported_versions() -> Set[str]`                                                                 | query          | supported LAS version strings    |
|  [08]   | `lost_dimensions(point_fmt_in, point_fmt_out)`                                                     | query          | dimensions lost by format change |

[ENTRYPOINT_SCOPE]: LasReader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------ |
|  [01]   | `reader.read() -> LasData`                            | read           | read full file            |
|  [02]   | `reader.read_points(n) -> PackedPointRecord`          | streaming      | read next n points        |
|  [03]   | `reader.chunk_iterator(points_per_chunk) -> iterator` | streaming      | iterate fixed-size chunks |
|  [04]   | `reader.seek(point_index)`                            | navigation     | seek to point index       |
|  [05]   | `reader.read_evlrs()`                                 | metadata       | read extended VLRs        |
|  [06]   | `reader.close()`                                      | lifecycle      | close file handle         |

[ENTRYPOINT_SCOPE]: LasWriter, LasAppender, CopcReader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :-------------------------------- |
|  [01]   | `writer.write_points(points: PackedPointRecord)`                                | write          | write point block                 |
|  [02]   | `writer.write_evlrs(evlrs)`                                                     | metadata       | write extended VLRs               |
|  [03]   | `writer.close()`                                                                | lifecycle      | flush and close                   |
|  [04]   | `appender.append_points(points: PackedPointRecord)`                             | append         | add points to existing file       |
|  [05]   | `appender.close()`                                                              | lifecycle      | flush and close                   |
|  [06]   | `CopcReader.open(source, ...) -> CopcReader`                                    | creation       | open COPC file or HTTP URL        |
|  [07]   | `copc.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord` | spatial        | combined spatial+resolution query |
|  [08]   | `copc.spatial_query(bounds) -> ScaleAwarePointRecord`                           | spatial        | bounding-box query                |
|  [09]   | `copc.level_query(level) -> ScaleAwarePointRecord`                              | spatial        | octree level query (int or range) |

[ENTRYPOINT_SCOPE]: LasHeader operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `LasHeader.read_from(stream) -> LasHeader`          | construction   | read header from file stream      |
|  [02]   | `header.add_crs(crs)`                               | metadata       | attach CRS to header              |
|  [03]   | `header.parse_crs()`                                | metadata       | parse CRS from VLRs               |
|  [04]   | `header.add_extra_dim(params: ExtraBytesParams)`    | schema         | register extra dimension          |
|  [05]   | `header.set_version_and_point_format(version, fmt)` | mutation       | change version and format         |
|  [06]   | `header.grow(other_header)`                         | mutation       | expand bounds to include other    |
|  [07]   | `header.update(points)`                             | mutation       | recompute stats from point record |

## [04]-[IMPLEMENTATION_LAW]

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
