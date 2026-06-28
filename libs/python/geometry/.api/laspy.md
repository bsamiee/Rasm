# [PY_GEOMETRY_API_LASPY]

`laspy` supplies the ASPRS LAS/LAZ point-cloud file IO for the geometry scan-processing rail: `LasReader`/`LasWriter`/`LasAppender` for streaming chunked read/write/append, `CopcReader` for spatial+resolution+octree-level query against Cloud-Optimized Point Clouds (local path or HTTP), `LasData` as the in-memory file with scale-aware `xyz`, `LasHeader` for version/format/scale/offset/bounds/CRS, `PointFormat`/`ExtraBytesParams` for standard and extra-dimension schema, and `VLR`/EVLR access. Compressed LAZ rides a `LazBackend` (pure-Rust `lazrs`/`LazrsParallel` or native `Laszip`); CRS rides `pyproj`; HTTP COPC rides `requests`. The geometry owner streams scans into the registration/deviation pipeline through chunk iteration; it never hand-parses LAS binary records or applies scale/offset manually where `xyz`/`ScaleAwarePointRecord` own metric coordinates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy`
- import: `import laspy`
- owner: `geometry`
- rail: scan-processing
- installed: `2.7.0`
- capability: LAS 1.0–1.4 / LAS 2.0 read/write/append, compressed LAZ via `LazBackend`, COPC spatial+resolution+level query (local or HTTP), per-channel selective decompression, extra-dimension schema management, CRS handling via `pyproj`, and VLR/EVLR record access

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IO handles
- rail: scan-processing

`LasReader`/`LasWriter`/`LasAppender` are reached through `open(source, mode=...)`; `CopcReader` through its own `open`. Only `LasReader`/`LasWriter` are exported at top level — the appender is the `mode='a'` return.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `LasReader`   | file reader   | chunked/streamed read, seek, full read, EVLR + point-source access |
|  [02]   | `LasWriter`   | file writer   | streaming point-block write, EVLR write                            |
|  [03]   | `LasAppender` | file appender | append point blocks to an existing LAS/LAZ file                    |
|  [04]   | `CopcReader`  | COPC reader   | combined spatial/resolution/octree-level query (local or HTTP)     |

[PUBLIC_TYPE_SCOPE]: data containers
- rail: scan-processing

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                                          |
| :-----: | :---------------------- | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `LasData`               | in-memory file    | `points`, `point_format`, `vlrs`, `evlrs`, `xyz`, `write`, `change_scaling`, extra-dim mgmt |
|  [02]   | `LasHeader`             | header record     | version/format, scale/offset, bounds, CRS, VLRs, extra-dim schema     |
|  [03]   | `PackedPointRecord`     | raw point buffer  | contiguous per-format packed storage (write input)                    |
|  [04]   | `ScaleAwarePointRecord` | scaled buffer     | per-point storage with scale/offset applied (read/COPC output)        |
|  [05]   | `PointFormat`           | format descriptor | standard + extra dimensions, dtype, size, dimension-by-name lookup    |
|  [06]   | `VLR`                   | variable record   | `user_id`, `record_id`, `description`, `record_data_bytes`            |
|  [07]   | `DimensionInfo`         | dimension meta    | kind, dtype, name, min/max, scales/offsets                            |
|  [08]   | `ExtraBytesParams`      | extra-dim spec    | parameter object for `add_extra_dim`/`add_extra_dims`                 |
|  [09]   | `Bounds`                | spatial bounds    | `overlaps(other)`, `ensure_3d()` — the `CopcReader.query` filter      |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies
- rail: scan-processing

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                                                  |
| :-----: | :----------------------- | :------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `LazBackend`             | LAZ engine     | `Laszip` (native `laszip`), `Lazrs`/`LazrsParallel` (pure-Rust `lazrs`); `detect_available()` |
|  [02]   | `DimensionKind`          | dimension type | `BitField`, `FloatingPoint`, `SignedInteger`, `UnsignedInteger`                               |
|  [03]   | `DecompressionSelection` | channel mask   | per-channel bitfield (`XY_RETURNS_CHANNEL`/`Z`/`CLASSIFICATION`/...); `all()` decompresses everything |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level IO functions
- rail: scan-processing

`open` discriminates the handle by `mode`: `'r'`->`LasReader`, `'w'`->`LasWriter` (requires `header`), `'a'`->`LasAppender`. `read`/`open` default `laz_backend=LazBackend.detect_available()` and `read` defaults `decompression_selection=DecompressionSelection.all()`.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `read(source, closefd=True, laz_backend=detect_available(), decompression_selection=all()) -> LasData` | eager read | load full file into `LasData`            |
|  [02]   | `open(source, mode='r', *, header=None, do_compress=None, closefd=True, laz_backend=detect_available()) -> LasReader\|LasWriter\|LasAppender` | streaming | context-manager IO handle, mode-discriminated |
|  [03]   | `create(*, point_format=0, file_version=0) -> LasData`                                          | creation       | new empty `LasData`                           |
|  [04]   | `convert(source_las, *, point_format_id=None, file_version=None) -> LasData`                    | conversion     | re-format an in-memory point record           |
|  [05]   | `merge(las_files: Iterable[LasData]\|LasData) -> LasData`                                        | merge          | concatenate multiple `LasData` into one       |
|  [06]   | `mmap(filename) -> LasMMAP`                                                                      | mmap read      | memory-mapped LAS access                      |
|  [07]   | `supported_point_formats() -> set[int]` / `supported_versions() -> set[str]`                    | query          | supported format IDs / version strings        |
|  [08]   | `lost_dimensions(point_fmt_in, point_fmt_out)`                                                   | query          | dimensions dropped by a format change         |

[ENTRYPOINT_SCOPE]: LasReader streaming
- rail: scan-processing

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `reader.read() -> LasData`                                         | read           | read the remaining file into `LasData`         |
|  [02]   | `reader.read_points(n) -> ScaleAwarePointRecord`                  | streaming      | read next n points, scale/offset applied       |
|  [03]   | `reader.chunk_iterator(points_per_iteration) -> PointChunkIterator` | streaming    | iterate fixed-size scaled chunks               |
|  [04]   | `reader.seek(point_index)`                                        | navigation     | seek to an absolute point index                |
|  [05]   | `reader.read_evlrs()` / `reader.evlrs` / `reader.point_source`     | metadata       | read extended VLRs / EVLR list / point source  |
|  [06]   | `reader.close()`                                                  | lifecycle      | close the underlying handle                    |

[ENTRYPOINT_SCOPE]: LasWriter / LasAppender / CopcReader
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `writer.write_points(points: PackedPointRecord)` / `writer.write_evlrs(evlrs)`  | write          | write a point block / extended VLRs               |
|  [02]   | `appender.append_points(points: PackedPointRecord)`                             | append         | add points to an existing file                    |
|  [03]   | `writer.close()` / `appender.close()`                                           | lifecycle      | flush and close                                   |
|  [04]   | `CopcReader.open(source, http_num_threads=80, decompression_selection=all()) -> CopcReader` | creation | open a COPC file path or HTTP URL (via `requests`) |
|  [05]   | `copc.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord` | spatial        | combined spatial+resolution+octree-level query    |
|  [06]   | `copc.spatial_query(bounds: Bounds) -> ScaleAwarePointRecord`                   | spatial        | bounding-box query only                           |
|  [07]   | `copc.level_query(level: int\|range) -> ScaleAwarePointRecord`                  | spatial        | octree level (or level-range) query               |

[ENTRYPOINT_SCOPE]: LasHeader and LasData schema
- rail: scan-processing

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `LasHeader.read_from(stream) -> LasHeader` / `header.write_to(stream)` | construction | read/write a header from/to a stream      |
|  [02]   | `header.add_crs(crs)` / `header.parse_crs()`                  | metadata       | attach / parse CRS via `pyproj` (VLRs)            |
|  [03]   | `header.add_extra_dim(params)` / `header.add_extra_dims(params_list)` / `header.remove_extra_dim(name)` / `header.remove_extra_dims(names)` | schema | register / drop extra dimensions |
|  [04]   | `header.set_version_and_point_format(version, fmt)` / `header.set_compressed(bool)` | mutation | change version+format / mark compressed |
|  [05]   | `header.grow(other)` / `header.update(points)` / `header.partial_reset()` | mutation | expand bounds / recompute stats / reset counts |
|  [06]   | `header.x_scale`/`x_offset`/`x_min`/`x_max` (and y/z), `header.version`, `header.point_format`, `header.vlrs`, `header.max_point_count` | accessor | scale/offset/bounds/version/format/VLRs |
|  [07]   | `data.xyz` / `data.change_scaling(scales=, offsets=)` / `data.update_header()` / `data.write(dest)` | data | scaled `(N,3)` f64 coords / rescale / refresh header / write to path-or-stream |
|  [08]   | `data.add_extra_dim(params)` / `data.add_extra_dims(params_list)` / `data.remove_extra_dim(name)` / `data.remove_extra_dims(names)` | schema | extra-dimension management on the in-memory file |

## [04]-[IMPLEMENTATION_LAW]

[IO_TOPOLOGY]:
- import: `import laspy` at boundary scope only; module-level import is banned by the manifest import policy.
- read axis: small scans enter eagerly via `read(path)`; large/streamed scans iterate `open(path).chunk_iterator(points_per_iteration)` or pull `read_points(n)`. Both reader read paths return a `ScaleAwarePointRecord` (scale/offset already applied) — the writer/appender input is the raw `PackedPointRecord`. The boundary never multiplies by scale and adds offset by hand; `data.xyz` and `ScaleAwarePointRecord` own metric coordinates.
- COPC axis: `CopcReader.open(source)` accepts a local path or an HTTP URL (over `requests`, `http_num_threads`-parallel); `query(bounds, resolution, level)` is the single polymorphic spatial entry that `spatial_query`/`level_query` specialize — a spatially-bounded extraction never downloads the full file. `DecompressionSelection` (a per-channel bitfield, default `all()`) decompresses only the requested channels, skipping the rest for a cheaper scan-load.
- schema axis: extra per-point dimensions are declared through `ExtraBytesParams` and registered via `header.add_extra_dim(s)` / `data.add_extra_dim(s)`, persisting in VLRs; `PointFormat.extra_dimension_names` and `dimension_by_name` enumerate the layout. CRS attaches/parses through `add_crs`/`parse_crs` over `pyproj`.

[STACKING_LAW]:
- the chunked reader is the streaming source for the scan-processing pipeline: `chunk_iterator` feeds point blocks into the registration owner (`kiss_matcher` global + `small_gicp` fine) and the deviation/segmentation owners without materializing the whole cloud — the per-chunk `ScaleAwarePointRecord` carries metric `xyz` directly into the numpy buffers those estimators consume.
- COPC `query` is the partial-download remote source: a bounded `Bounds`+resolution query over an HTTP COPC URL streams only the in-frustum octree nodes, complementing the data-tier remote-pointcloud rail (the `SCAN_COPC_PARTIAL` / data `REMOTE_POINTCLOUD_LASPY` seam) rather than re-downloading.
- extra-dimension VLRs are the carrier for per-point analysis results (classification, deviation, segment labels) computed downstream and written back via `add_extra_dim` + `write`, so a processed scan persists its derived channels in-format instead of a sidecar file.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `laspy`
- Owns: LAS/LAZ point-cloud file read, streamed chunk read, write, append, format conversion, multi-file merge, COPC spatial/resolution/level query, extra-dimension schema, CRS handling, and VLR/EVLR access
- Accept: LAS 1.0–2.0 files, LAZ via a `LazBackend`, COPC files or HTTP URLs, in-memory `LasData`/`PackedPointRecord` blocks
- Reject: hand-rolled binary LAS parsing or direct byte manipulation of point records, manual scale/offset arithmetic where `xyz`/`ScaleAwarePointRecord` apply it, a full-download read where a COPC `query` extracts only the needed extent, and a sidecar channel store where extra-dimension VLRs persist derived per-point data

[CAPTURE_GAP]:
