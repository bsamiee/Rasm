# [PY_DATA_API_LAZRS]

`lazrs` owns the Rust-backed LAZ/COPC point-record codec for the data point-cloud rail: PyO3 bindings over the `laz-rs` crate that decompress and compress LASzip chunked point data with no C++ `laszip` linkage. Sequential and Rayon-parallel decompressor, compressor, and appender families stream chunked codecs over VLR-keyed buffers and file-like sources, and block functions codec raw record-data buffers one-shot. `laspy` selects it as the `LazBackend.Lazrs`/`LazrsParallel` codec backend; it never re-implements the arithmetic range coding `laz-rs` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lazrs`
- package: `lazrs`
- module: `lazrs`
- owner: `data`
- rail: point-cloud

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots — one `LazVlr` descriptor keys every reader, writer, and appender.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :----------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `LazVlr`                 | codec descriptor | parses/mints LASzip VLR record data; keys readers/writers |
|  [02]   | `LasZipDecompressor`     | reader           | sequential chunked decompressor                           |
|  [03]   | `ParLasZipDecompressor`  | reader           | Rayon-parallel chunked decompressor                       |
|  [04]   | `LasZipCompressor`       | writer           | sequential chunked compressor                             |
|  [05]   | `ParLasZipCompressor`    | writer           | Rayon-parallel chunked compressor                         |
|  [06]   | `LasZipAppender`         | writer           | sequential appender to an existing compressed stream      |
|  [07]   | `ParLasZipAppender`      | writer           | Rayon-parallel appender to an existing stream             |
|  [08]   | `DecompressionSelection` | selection mask   | selective-field decompression bitmask                     |
|  [09]   | `LazrsError`             | error            | malformed-VLR / codec / I/O failure                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec constructors — `source` is a file-like read at the first compressed point, `dest` a writable stream, `vlr` the keying `LazVlr`; both appenders open an existing stream and continue at `point_count`, and `new_for_compression` accepts an optional `use_variable_size_chunks` flag.

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `LazVlr(record_data)`                                            | ctor    | parse an existing LASzip VLR buffer       |
|  [02]   | `LazVlr.new_for_compression(point_format_id, num_extra_bytes)`   | factory | mint a compression VLR for a point format |
|  [03]   | `LasZipDecompressor(source, record_data, selection=None)`        | ctor    | sequential decompressor over a source     |
|  [04]   | `ParLasZipDecompressor(source, vlr_record_data, selection=None)` | ctor    | Rayon-parallel decompressor               |
|  [05]   | `LasZipCompressor(dest, vlr)`                                    | ctor    | sequential compressor to a dest           |
|  [06]   | `ParLasZipCompressor(dest, vlr)`                                 | ctor    | Rayon-parallel compressor                 |
|  [07]   | `LasZipAppender(dest, laz_vlr_record_data, point_count)`         | ctor    | sequential appender continuing at count   |
|  [08]   | `ParLasZipAppender(dest, laz_vlr_record_data, point_count)`      | ctor    | Rayon-parallel appender                   |
|  [09]   | `DecompressionSelection(value)`                                  | ctor    | wrap a `SELECTIVE_DECOMPRESS_*` bitmask   |

Each reader, writer, and appender carries its chunked members; `decompress_many` fills a caller-supplied writable buffer sized to a point-count multiple of `item_size`, and `done` flushes the final chunk and backpatches the chunk table.

[LazVlr]: `uses_variable_size_chunks() -> bool` `chunk_size() -> int` `item_size() -> int` `record_data() -> bytes`
[LasZipDecompressor]: `decompress_many(dest)` `seek(point_idx)` `vlr() -> LazVlr` `read_chunk_table_only() -> list` `read_raw_bytes_into(bytes)`
[ParLasZipDecompressor]: `decompress_many(points)` `seek(point_idx)` `read_raw_bytes_into(bytes)`
[LasZipCompressor]: `reserve_offset_to_chunk_table()` `compress_many(points)` `compress_chunks(chunks)` `finish_current_chunk()` `done()`
[ParLasZipCompressor]: `reserve_offset_to_chunk_table()` `compress_many(points)` `compress_chunks(chunks)` `done()`
[LasZipAppender]: `compress_many(points)` `compress_chunks(chunks)` `done()`
[ParLasZipAppender]: `compress_many(points)` `compress_chunks(chunks)` `done()`

[ENTRYPOINT_SCOPE]: block codec functions — module-level one-shot codecs over raw buffers keyed by VLR record data, the `parallel` flag selecting the sequential or Rayon path.

| [INDEX] | [SURFACE]                                                                        | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `decompress_points(compressed_points_data, ..., decompression_output, parallel)` | block decompress into a buffer      |
|  [02]   | `compress_points(laszip_vlr, uncompressed_points, parallel) -> bytes`            | block compress to bytes             |
|  [03]   | `decompress_points_with_chunk_table(..., py_chunk_table, selection=None)`        | block decompress with a chunk table |
|  [04]   | `read_chunk_table(source, vlr) -> list`                                          | read and decode the chunk table     |
|  [05]   | `read_chunk_table_only(source, vlr) -> list`                                     | read the chunk table, no decode     |
|  [06]   | `write_chunk_table(dest, py_chunk_table, vlr)`                                   | write a chunk table to a stream     |

[ENTRYPOINT_SCOPE]: selective-decompression flags — module `int` flags each prefixed `SELECTIVE_DECOMPRESS_`, OR-composed into a `DecompressionSelection(value)` mask; `ALL` is the all-fields default and `ALL_EXTRA_BYTES` every extra-byte channel, so a COPC reader decodes only the dimensions the consumer keeps.

| [INDEX] | [FLAG]               | [SELECTS]                           |
| :-----: | :------------------- | :---------------------------------- |
|  [01]   | `ALL`                | every field (default)               |
|  [02]   | `XY_RETURNS_CHANNEL` | X/Y, return number, scanner channel |
|  [03]   | `Z`                  | Z coordinate                        |
|  [04]   | `CLASSIFICATION`     | classification byte                 |
|  [05]   | `FLAGS`              | classification/scan flags           |
|  [06]   | `INTENSITY`          | intensity                           |
|  [07]   | `SCAN_ANGLE`         | scan angle                          |
|  [08]   | `USER_DATA`          | user-data byte                      |
|  [09]   | `POINT_SOURCE_ID`    | point source id                     |
|  [10]   | `GPS_TIME`           | GPS time                            |
|  [11]   | `RGB`                | RGB color channels                  |
|  [12]   | `NIR`                | near-infrared channel               |
|  [13]   | `WAVEPACKET`         | full-waveform wavepacket            |
|  [14]   | `ALL_EXTRA_BYTES`    | every extra-byte channel            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `LazVlr` keys every reader, writer, and appender as the single codec descriptor, parsed once and threaded, never re-parsed per call; `LazVlr.new_for_compression` mints one for a write, `LazVlr(record_data)` parses one for a read.
- Sequential and Rayon-parallel pairs are one codec over a `parallel` axis: block `decompress_points`/`compress_points` own buffer-in/buffer-out work and the streaming `*Decompressor`/`*Compressor` classes own chunked file-like work — the buffer-vs-stream split, never duplicated logic.
- `DecompressionSelection(value)` composes the `SELECTIVE_DECOMPRESS_*` bitmask as the only field-narrowing surface, fed to a decompressor or `decompress_points_with_chunk_table`, never a post-decode drop.
- `read_chunk_table`/`read_chunk_table_only`/`write_chunk_table` and `decompress_points_with_chunk_table` own COPC chunk access; the chunk table reads once and threads into selective block decode, never re-walked per point.
- Each codec run captures point-format id, extra-byte count, chunk size, item size, point count, `parallel` flag, and selective mask as a point-cloud receipt the data owner folds into its `MeshPayload` receipt stream.

[STACKING]:
- `laspy`(`.api/laspy.md`): `lazrs` binds as the `LazBackend.Lazrs`/`LazrsParallel` codec backend that `laspy.open`/`read`/`CopcReader.open` select; `laspy` owns LAS container, header, dimensions, and CRS while `lazrs` decodes the compressed record stream, and `laspy.DecompressionSelection.to_laszip()` lowers to the `int` mask this surface wraps.
- point-cloud owner: a COPC reader composes one dense rail — `laspy` exposes the octree hierarchy and per-node chunk byte ranges, `read_chunk_table_only` reads the table once, consumer dimensions become a `DecompressionSelection` OR of `SELECTIVE_DECOMPRESS_*`, and `decompress_points_with_chunk_table(..., selection=)` decodes only the requested nodes' fields into the NumPy record buffer; spatial-window filter, field narrowing, and Rayon parallelism are one pipeline.

[LOCAL_ADMISSION]:
- Reach `lazrs` only through `laspy`'s `LazBackend` selector at boundary scope; block functions codec raw buffers, the streaming classes codec file-like sources, and a raised `LazrsError` maps to the data rail's typed failure at the boundary.

[RAIL_LAW]:
- Package: `lazrs`
- Owns: chunked LASzip/COPC point-record decompression and compression — sequential and Rayon-parallel decompressor, compressor, and appender families, selective-field decompression, and chunk-table read/write over VLR-keyed buffers and file-like streams
- Accept: LAZ/COPC compressed point-cloud ingestion and emission as the `laspy` `LazBackend.Lazrs`/`LazrsParallel` codec backend feeding the point-cloud owner
- Reject: a hand-rolled LASzip range coder; the C++ `laszip` backend where `lazrs` is admitted; LAS header/container parsing pulled into this package; a parallel reader type minted per `parallel` flag; post-decode field dropping the selective mask owns
