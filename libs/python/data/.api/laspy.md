# [PY_DATA_API_LASPY]

`laspy` supplies LAS/LAZ point-cloud file IO for the data scan-exchange rail and owns the COPC octree-subset read. `laspy.open(source, mode)` is the single polymorphic IO entry that discriminates `mode` into a `LasReader`/`LasWriter`/`LasAppender` for chunked streaming; `laspy.read` is the eager whole-file load into a `LasData`. The package reads, writes, appends, and converts LAS 1.0–1.4 / LAS 2.0 records, manages extra dimensions (`ExtraBytesParams`) and CRS (`pyproj.CRS`), reads/writes VLRs and EVLRs, selects the LAZ backend through `LazBackend`, masks decode fields through `DecompressionSelection`, and runs spatial/level-of-detail queries against Cloud-Optimized Point Cloud files through `laspy.copc.CopcReader`; it never re-implements binary LAS parsing or the LASzip range codec. COPC is LAZ-1.4 by construction, so `CopcReader` and every `.laz`/`.copc.laz` read decode through the companion `lazrs` (default) or `laszip` backend (`.api/lazrs.md`, `.api/laszip.md`) selected via `LazBackend` — laspy never reimplements the range codec.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy`
- version: `2.7.0`
- license: BSD-2-Clause
- module: `laspy`
- owner: `data`
- rail: scan-exchange
- depends: `numpy` (core); optional `lazrs`/`laszip` (LAZ backends, `.api/lazrs.md`/`.api/laszip.md`), `pyproj` (CRS round-trip), `requests` (COPC over HTTP)
- entry points: `import laspy` plus the `laspy` console script (`laspy.cli`)
- evidence: assay-reflected — `laspy 2.7.0` (`api resolve laspy`); `LasAppender`/`LasMMAP` resolve through `laspy.open(mode='a')`/`laspy.mmap` (returned-handle types, not top-level importable names)
- capability: LAS/LAZ eager `read` and chunked-streaming `open` (reader/writer/appender), `create`/`convert`/`mmap`, extra-dimension (`ExtraBytesParams`) and CRS management, VLR/EVLR access, `LazBackend` selection, `DecompressionSelection` field masking, and the COPC octree-subset spatial/LOD read via `laspy.copc.CopcReader`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: in-memory and streaming IO carriers
- rail: scan-exchange

`LasData` is the eager whole-file carrier; `LasReader`/`LasWriter`/`LasAppender` are the streaming handles `laspy.open` returns per `mode`. All point storage is a `ScaleAwarePointRecord` (scale/offset applied) backed by a structured NumPy array.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [CAPABILITY]                                                                                                                                                                                          |
| :-----: | :---------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `LasData`               | in-memory file     | `x`/`y`/`z`/`xyz`, `point_format`, `header`, `points`, `vlrs`/`evlrs`, `add_extra_dim(s)`, `write`                                                                                                    |
|  [02]   | `LasReader`             | streaming reader   | `read_points(n)`, `chunk_iterator(n)`, `seek`, `read`, `read_evlrs`, `header`                                                                                                                         |
|  [03]   | `LasWriter`             | streaming writer   | `write_points(record)`, `write_evlrs`, `close`; context-manager from `open(mode='w')`                                                                                                                 |
|  [04]   | `LasAppender`           | streaming appender | `append_points(record)`, `close`; context-manager from `open(mode='a')`                                                                                                                               |
|  [05]   | `LasHeader`             | header record      | version, point format, `point_count`, scale/offset, bounds, VLRs, `add_crs`/`parse_crs`, `grow`                                                                                                       |
|  [06]   | `PointFormat`           | format descriptor  | `id`, `dimension_names`, `dtype`, standard/extra dims, `add_extra_dimension`                                                                                                                          |
|  [07]   | `ScaleAwarePointRecord` | scaled buffer      | scale/offset-aware record; scaled `x`/`y`/`z`, `point_format` (a `PointFormat`), `[name]` dimension access, `zeros`/`empty`/`from_buffer`/`change_scaling`/`resize`                                   |
|  [08]   | `VLR`                   | metadata block     | `user_id`/`record_id`/`description`/`record_data_bytes`; header VLR/EVLR payload                                                                                                                      |
|  [09]   | `ExtraBytesParams`      | extra-dim spec     | `name`/`type`/`description`/`offsets`/`scales`/`no_data` for `add_extra_dim`                                                                                                                          |
|  [10]   | `LaspyException`        | error              | base laspy failure; the `laspy.errors` family refines it — `LazError` (absent/failed LAZ backend), `FileVersionNotSupported`, `PointFormatNotSupported`, `IncompatibleDataFormat`, `UnknownExtraType` |

[PUBLIC_TYPE_SCOPE]: backend and field-selection enums
- rail: scan-exchange

`LazBackend` selects the native LAZ codec; `DecompressionSelection` is laspy's own field-mask flag enum whose `to_laszip()` produces the `int` mask the `laszip` binding consumes.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [MEMBERS_CAPABILITY]                                                                                                                                                                                  |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `LazBackend`             | backend enum  | `LazrsParallel`, `Lazrs`, `Laszip`; `is_available()`/`supports_append` gate selection (Laszip has no append)                                                                                          |
|  [02]   | `DecompressionSelection` | field flags   | `XY_RETURNS_CHANNEL`/`Z`/`CLASSIFICATION`/`FLAGS`/`INTENSITY`/`SCAN_ANGLE`/`USER_DATA`/`POINT_SOURCE_ID`/`GPS_TIME`/`RGB`/`NIR`/`WAVEPACKET`/`ALL_EXTRA_BYTES`; `all()` builder, `to_laszip() -> int` |
|  [03]   | `DimensionKind`          | dtype kind    | `SignedInteger`/`UnsignedInteger`/`FloatingPoint`/`BitField`                                                                                                                                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: LAS/LAZ IO operations
- rail: scan-exchange
- `laspy.open(source, mode)` is the single polymorphic IO surface; `mode` ('r'/'w'/'a') discriminates the returned `LasReader`/`LasWriter`/`LasAppender`. `laspy.read` is the eager whole-file shorthand for `open(mode='r').read()`.

| [INDEX] | [SURFACE]                                                                                                                                                                                                      | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `laspy.read(source, closefd=True, laz_backend=(), decompression_selection=DecompressionSelection.all(), encoding_errors='strict') -> LasData`                                                                  | eager read      | load full LAS/LAZ into `LasData`              |
|  [02]   | `laspy.open(source, mode='r', closefd=True, laz_backend=None, header=None, do_compress=None, encoding_errors='strict', read_evlrs=True, decompression_selection=...) -> LasReader \| LasWriter \| LasAppender` | streaming       | mode-discriminated reader/writer/appender     |
|  [03]   | `laspy.create(*, point_format=None, file_version=None) -> LasData`                                                                                                                                             | construct       | new empty `LasData` at a point format/version |
|  [04]   | `laspy.convert(source_las, *, point_format_id=None, file_version=None) -> LasData`                                                                                                                             | convert         | reformat a `LasData` to a new format/version  |
|  [05]   | `laspy.mmap(filename) -> LasMMAP`                                                                                                                                                                              | mmap            | memory-mapped point access over a LAS file    |
|  [06]   | `LasReader.read_points(n) -> ScaleAwarePointRecord` / `chunk_iterator(points_per_iteration) -> PointChunkIterator`                                                                                             | streaming read  | bounded read or lazy chunk iteration          |
|  [07]   | `LasReader.seek(pos, whence=0) -> int` / `read_evlrs()`                                                                                                                                                        | streaming read  | reposition and pull deferred EVLRs            |
|  [08]   | `LasWriter.write_points(record)` / `write_evlrs(evlrs)` / `LasAppender.append_points(record)`                                                                                                                  | streaming write | append packed points / EVLRs to a stream      |
|  [09]   | `LasData.x` / `LasData.y` / `LasData.z` / `LasData.xyz`                                                                                                                                                        | dimension       | scaled coordinate dimension arrays            |
|  [10]   | `LasData.write(destination, *, do_compress=None, laz_backend=None)`                                                                                                                                            | write           | write LAS/LAZ to path or stream               |
|  [11]   | `LasData.add_extra_dim(params)` / `add_extra_dims(params)` / `remove_extra_dim(name)`                                                                                                                          | extra-dim       | declare/drop `ExtraBytesParams` dimensions    |
|  [12]   | `LasHeader.add_crs(crs, keep_compatibility=True)` / `parse_crs(prefer_wkt=True) -> Optional[pyproj.CRS]`                                                                                                       | metadata        | write/read CRS through header VLRs            |
|  [13]   | `PointFormat.id` / `dimension_names` / `dtype` / `dimension_by_name(name)`                                                                                                                                     | descriptor      | point-format identity and dimension layout    |

## [04]-[COPC_SUBSET]

[COPC_SCOPE]: octree-subset read
- rail: scan-exchange
- engine: `laspy.copc.CopcReader` reads LAZ-1.4 COPC files organized as an octree, exposing spatial-bounds and level-of-detail queries that return a `ScaleAwarePointRecord` without decoding the whole file.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                                                                                                                                         |
| :-----: | :------------------------------------------------------------------ | :------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `laspy.copc.CopcReader`                                             | COPC reader   | octree-indexed spatial/LOD reader over local paths and HTTP URLs                                                                                     |
|  [02]   | `laspy.copc.Bounds(mins: np.ndarray, maxs: np.ndarray)`             | bounds record | `overlaps(other)`, `ensure_3d(mins, maxs)` axis-aligned query box                                                                                    |
|  [03]   | `laspy.ScaleAwarePointRecord(array, point_format, scales, offsets)` | scaled buffer | scale/offset-aware point record; scaled `x`/`y`/`z`, `point_format` (a `PointFormat`), `[name]` dimension access, `zeros(...)`/`empty(...)` builders |

| [INDEX] | [SURFACE]                                                                                                         | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `CopcReader.open(source, http_num_threads=80, _http_strategy='queue', decompression_selection=...) -> CopcReader` | creation       | open COPC path, HTTP URL, or file object           |
|  [02]   | `CopcReader.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord`                             | spatial        | combined bounds + resolution/LOD query             |
|  [03]   | `CopcReader.spatial_query(bounds) -> ScaleAwarePointRecord`                                                       | spatial        | bounds-only octree subset                          |
|  [04]   | `CopcReader.level_query(level) -> ScaleAwarePointRecord`                                                          | LOD            | LOD-only subset (`int` or `range`)                 |
|  [05]   | `Bounds.overlaps(other) -> bool`                                                                                  | predicate      | axis-aligned overlap test                          |
|  [06]   | `Bounds.ensure_3d(mins, maxs) -> Bounds`                                                                          | projection     | promote 2D bounds to 3D box                        |
|  [07]   | `CopcReader.copc_info` / `CopcReader.header`                                                                      | metadata       | COPC info (`spacing`, octree root) and `LasHeader` |

## [05]-[IMPLEMENTATION_LAW]

[COPC_TOPOLOGY]:
- `CopcReader.open(source)` accepts a local path, an HTTP/HTTPS URL (via `requests`), or a file-like object; it reads the COPC header and root octree page eagerly, then serves `query` calls lazily over the octree.
- `query(bounds, resolution, level)` discriminates the read: `bounds` is a `laspy.copc.Bounds` (2D bounds skip Z filtering, promoted via `ensure_3d`); `resolution` and `level` are mutually exclusive — `resolution` derives an octree level range from `copc_info.spacing`, `level` selects an explicit `int` or `range` LOD; the result is a `ScaleAwarePointRecord`. `spatial_query`/`level_query` are the single-axis forms `query` dispatches into; never re-derive an octree level range outside `copc_info.spacing`.
- `ScaleAwarePointRecord(array, point_format, scales, offsets)` extends `PackedPointRecord` with `scales`/`offsets` arrays of shape `(3,)`, exposing scaled `x`/`y`/`z` directly; build empties via `ScaleAwarePointRecord.zeros(...)`/`empty(...)` and grow via `resize`, never a raw NumPy buffer.

[IO_TOPOLOGY]:
- `laspy.open(source, mode)` is the one IO entry; `mode='r'` yields a `LasReader` (eager `read()` or `chunk_iterator(n)` for out-of-core streaming), `mode='w'` a `LasWriter` (requires `header=`, `write_points` per chunk), `mode='a'` a `LasAppender`. `laspy.read` is `open('r').read()`; never hand-thread a reader/writer per direction when `open` discriminates `mode`.
- extra dimensions are declared through `ExtraBytesParams(name, type, description='', offsets=None, scales=None, no_data=None)` fed to `LasData.add_extra_dim(s)` / `PointFormat.add_extra_dimension`; never append a raw column to the structured array.
- CRS round-trips through `LasHeader.add_crs(pyproj.CRS, keep_compatibility=True)` and `parse_crs(prefer_wkt=True)`; VLR/EVLR payloads are `VLR(user_id, record_id, description, record_data_bytes)` read off `LasHeader.vlrs`/`LasData.evlrs`.

[BACKEND_LAW]:
- Compressed COPC/LAZ decoding rides the companion backend: `lazrs` (`.api/lazrs.md`, Rust `laz-rs`) is the default and required COPC backend, with `laszip` (`.api/laszip.md`, native LASzip) the alternative; `laspy` selects between them through `LazBackend.{Lazrs, LazrsParallel, Laszip}`, gated by `LazBackend.is_available()`.
- field-skipping is one `DecompressionSelection` flag value threaded into `read`/`open`/`CopcReader.open`; `DecompressionSelection.to_laszip() -> int` is the only producer of the mask the `laszip` binding consumes (`.api/laszip.md`), and the `lazrs` backend reads the same flag set — never two parallel mask vocabularies.
- append is backend-sensitive: `LazBackend.Laszip.supports_append` is `False`, so a `LasAppender` over compressed LAZ requires `LazBackend.Lazrs`/`LazrsParallel`; route LAZ append accordingly.

[RAIL_LAW]:
- Package: `laspy`
- Owns: LAS/LAZ point-cloud file read/write/append/convert, the `laspy.open` mode-discriminated streaming IO, extra-dimension/CRS/VLR management, and the COPC octree-subset spatial query
- Accept: LAS 1.0–2.0 files, LAZ via `lazrs`/`laszip` backend selected through `LazBackend`, `DecompressionSelection` field masking, `ExtraBytesParams` extra dimensions, and COPC paths/HTTP URLs/file objects through `CopcReader`
- Reject: hand-rolled binary LAS parsing; direct byte manipulation of point records; re-implementing the LASzip range codec the companion backend owns; a per-direction IO type where `laspy.open(mode)` discriminates; a raw NumPy column where `ExtraBytesParams` declares the extra dimension; a second field-mask vocabulary parallel to `DecompressionSelection`
