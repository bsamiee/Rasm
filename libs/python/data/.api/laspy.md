# [PY_DATA_API_LASPY]

`laspy` owns LAS/LAZ point-cloud file IO and the COPC octree-subset read for the data scan-exchange rail. `laspy.open(source, mode)` is the one polymorphic IO entry, discriminating `mode` into a `LasReader`/`LasWriter`/`LasAppender`, and `laspy.read` is the eager whole-file load into a `LasData`. Compressed decode rides a companion `LazBackend`, so the LASzip range codec is never re-implemented here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy` (BSD-2-Clause)
- module: `laspy`
- namespaces: `laspy`, `laspy.copc`, `laspy.errors`
- rail: scan-exchange
- depends: `numpy` point buffer; `lazrs`/`laszip` LAZ backends; `pyproj` CRS round-trip; `requests` COPC-over-HTTP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: in-memory and streaming IO carriers

`LasData` is the eager whole-file carrier; `LasReader`/`LasWriter`/`LasAppender` are the streaming handles `laspy.open` returns per `mode`. All point storage is a `ScaleAwarePointRecord` (scale/offset applied) over a structured NumPy array; `LasAppender` and `LasMMAP` are returned-handle types, not top-level importable names.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [CAPABILITY]                                                                          |
| :-----: | :---------------------- | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `LasData`               | in-memory file     | eager whole-file point-cloud carrier                                                  |
|  [02]   | `LasReader`             | streaming reader   | `read_points(n)`, `chunk_iterator(n)`, `seek`, `read`, `read_evlrs`, `header`         |
|  [03]   | `LasWriter`             | streaming writer   | `write_points(record)`, `write_evlrs`, `close`; context-manager from `open(mode='w')` |
|  [04]   | `LasAppender`           | streaming appender | `append_points(record)`, `close`; context-manager from `open(mode='a')`               |
|  [05]   | `LasHeader`             | header record      | version, format, counts, scale/offset, bounds; CRS and VLR owner                      |
|  [06]   | `PointFormat`           | format descriptor  | `id`, `dimension_names`, `dtype`, standard/extra dims, `add_extra_dimension`          |
|  [07]   | `ScaleAwarePointRecord` | scaled buffer      | scale/offset-aware point record over a structured NumPy array                         |
|  [08]   | `VLR`                   | metadata block     | `user_id`/`record_id`/`description`/`record_data_bytes`; header VLR/EVLR payload      |
|  [09]   | `ExtraBytesParams`      | extra-dim spec     | `name`/`type`/`description`/`offsets`/`scales`/`no_data` for `add_extra_dim`          |
|  [10]   | `LaspyException`        | error              | `laspy.errors` family root                                                            |

- [01]-[LASDATA]: `x`/`y`/`z`/`xyz`, `point_format`, `header`, `points`, `vlrs`/`evlrs`, `add_extra_dim(s)`, `remove_extra_dim`, `write`.
- [05]-[LASHEADER]: `version`, `point_format`, `point_count`, scale/offset, bounds, `vlrs`, `add_crs`/`parse_crs`, `grow`.
- [07]-[SCALED_RECORD]: scaled `x`/`y`/`z`, `point_format`, `[name]` dimension access, `zeros`/`empty`/`from_buffer`/`change_scaling`/`resize`.
- [10]-[ERRORS]: `laspy.errors` refines `LaspyException` — `LazError`, `FileVersionNotSupported`, `PointFormatNotSupported`, `IncompatibleDataFormat`, `UnknownExtraType`.

[PUBLIC_TYPE_SCOPE]: backend and field-selection enums

`LazBackend` selects the native LAZ codec; `DecompressionSelection` is the field-mask flag enum whose `to_laszip() -> int` produces the mask the `laszip` binding consumes.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `LazBackend`             | backend enum  | `LazrsParallel`, `Lazrs`, `Laszip`; `is_available()`/`supports_append` gate selection |
|  [02]   | `DecompressionSelection` | field flags   | field-mask flag enum; `all()` builder, `to_laszip() -> int`                           |
|  [03]   | `DimensionKind`          | dtype kind    | `SignedInteger`/`UnsignedInteger`/`FloatingPoint`/`BitField`                          |

- [02]-[DECOMPRESSION_FLAGS]: `XY_RETURNS_CHANNEL`/`Z`/`CLASSIFICATION`/`FLAGS`/`INTENSITY`/`SCAN_ANGLE`/`USER_DATA`/`POINT_SOURCE_ID`/`GPS_TIME`/`RGB`/`NIR`/`WAVEPACKET`/`ALL_EXTRA_BYTES`.

[PUBLIC_TYPE_SCOPE]: COPC octree-subset carriers

`laspy.copc.CopcReader` reads a LAZ-1.4 COPC file organized as an octree, serving spatial-bounds and LOD queries that return a `ScaleAwarePointRecord` without decoding the whole file.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `laspy.copc.CopcReader` | COPC reader   | octree-indexed spatial/LOD reader over local path + HTTP |
|  [02]   | `laspy.copc.Bounds`     | bounds record | axis-aligned box; `overlaps`, `ensure_3d`                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: LAS/LAZ/COPC IO operations
- read/open carry: `laz_backend`, `decompression_selection` (default `DecompressionSelection.all()`), `encoding_errors`

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `laspy.read(source, closefd) -> LasData`                   | static   | load full LAS/LAZ into `LasData`              |
|  [02]   | `laspy.open(source, mode, *, header, read_evlrs)`          | static   | mode-discriminated reader/writer/appender     |
|  [03]   | `laspy.create(*, point_format, file_version) -> LasData`   | factory  | new empty `LasData` at a point format/version |
|  [04]   | `laspy.convert(source, *, point_format_id) -> LasData`     | static   | reformat a `LasData` to a new format/version  |
|  [05]   | `laspy.mmap(filename) -> LasMMAP`                          | static   | memory-mapped point access over a LAS file    |
|  [06]   | `LasReader.read_points(n)` / `chunk_iterator(n)`           | instance | bounded read / lazy chunk iteration           |
|  [07]   | `LasReader.seek(pos, whence) -> int` / `read_evlrs()`      | instance | reposition and pull deferred EVLRs            |
|  [08]   | `LasWriter.write_points(record)` / `write_evlrs(evlrs)`    | instance | append packed points / EVLRs to a stream      |
|  [09]   | `LasAppender.append_points(record)`                        | instance | append points to an open LAZ stream           |
|  [10]   | `LasData.x` / `y` / `z` / `xyz`                            | property | scaled coordinate dimension arrays            |
|  [11]   | `LasData.write(destination, *, do_compress, laz_backend)`  | instance | write LAS/LAZ to path or stream               |
|  [12]   | `LasData.add_extra_dim(s)` / `remove_extra_dim(name)`      | instance | declare/drop `ExtraBytesParams` dimensions    |
|  [13]   | `LasHeader.add_crs(crs, keep_compatibility)` / `parse_crs` | instance | write/read CRS through header VLRs            |
|  [14]   | `PointFormat.id` / `dimension_names` / `dimension_by_name` | property | point-format identity and dimension layout    |
|  [15]   | `CopcReader.open(source, http_num_threads) -> CopcReader`  | factory  | open COPC path, HTTP URL, or file object      |
|  [16]   | `CopcReader.query(bounds, resolution, level)`              | instance | combined bounds + resolution/LOD query        |
|  [17]   | `CopcReader.spatial_query(bounds)`                         | instance | bounds-only octree subset                     |
|  [18]   | `CopcReader.level_query(level)`                            | instance | LOD-only subset (`int` or `range`)            |
|  [19]   | `Bounds.overlaps(other) -> bool`                           | instance | axis-aligned overlap test                     |
|  [20]   | `Bounds.ensure_3d(mins, maxs) -> Bounds`                   | instance | promote 2D bounds to a 3D box                 |
|  [21]   | `CopcReader.copc_info` / `CopcReader.header`               | property | octree info (`spacing`, root) and `LasHeader` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `laspy.open(source, mode)` is the one IO entry: `mode='r'` yields a `LasReader` (eager `read()` or `chunk_iterator(n)` out-of-core), `mode='w'` a `LasWriter` (requires `header=`, `write_points` per chunk), `mode='a'` a `LasAppender`; `laspy.read` is `open('r').read()`. Compressed-LAZ append requires `LazBackend.Lazrs`/`LazrsParallel`, since `Laszip.supports_append` is `False`.
- `CopcReader.open(source)` reads the COPC header and root octree page eagerly, then serves `query` lazily. `query(bounds, resolution, level)` discriminates the read: `bounds` is a `laspy.copc.Bounds` (2D bounds skip Z filtering, promoted via `ensure_3d`); `resolution` and `level` are mutually exclusive, `resolution` deriving an octree level range from `copc_info.spacing` and `level` selecting an explicit `int`/`range`. `spatial_query`/`level_query` are the single-axis forms `query` dispatches into, and the result is always a `ScaleAwarePointRecord`.
- extra dimensions declare through `ExtraBytesParams(name, type, description, offsets, scales, no_data)` fed to `LasData.add_extra_dim(s)`/`PointFormat.add_extra_dimension`; CRS round-trips through `LasHeader.add_crs(pyproj.CRS, keep_compatibility)`/`parse_crs(prefer_wkt)`; VLR/EVLR payloads are `VLR(user_id, record_id, description)` carrying `record_data_bytes`, read off `LasHeader.vlrs`/`LasData.evlrs`.

[STACKING]:
- `lazrs`(`.api/lazrs.md`): every `.laz`/`.copc.laz` decode routes through the Rust `laz-rs` backend, laspy's default and required COPC codec; `LazBackend.{Lazrs, LazrsParallel}` selects it under `is_available()`.
- `laszip`(`.api/laszip.md`): the native-LASzip alternative codec; `DecompressionSelection.to_laszip() -> int` is the sole producer of the field mask the `laszip` binding consumes, and `lazrs` reads the same flag set — one mask vocabulary across both.
- `pyproj`(`.api/pyproj.md`): `LasHeader.add_crs(pyproj.CRS)`/`parse_crs` round-trips the point-cloud CRS through header VLRs.
- `data/spatial/mesh` `PointCloud` owner: threads one `decompression_selection` `Selection` fixed at `open` identically through `laspy.read` and `CopcReader.open`, crossing point records as a content-keyed `PointRecordTable` on the shared columnar Arrow rail.

[LOCAL_ADMISSION]:
- Admit `laspy` for LAS 1.0–2.0 / LAZ / COPC point-cloud file interchange on the scan rail; a compressed read or append requires `lazrs` or `laszip` present on the worker lane.

[RAIL_LAW]:
- Package: `laspy`
- Owns: LAS/LAZ read/write/append/convert, the `laspy.open` mode-discriminated streaming IO, extra-dimension/CRS/VLR management, and the COPC octree-subset spatial/LOD read
- Accept: LAS 1.0–2.0 files, LAZ through a `LazBackend`-selected codec, `DecompressionSelection` field masking, `ExtraBytesParams` extra dimensions, and COPC paths/HTTP URLs/file objects through `CopcReader`
- Reject: hand-rolled binary LAS parsing; a re-implemented LASzip range codec; a per-direction IO type where `laspy.open(mode)` discriminates; a raw NumPy column where `ExtraBytesParams` declares the dimension; a second field-mask vocabulary parallel to `DecompressionSelection`
