# [PY_DATA_API_LAZRS]

`lazrs` supplies the Rust-backed LAZ/COPC point-record codec for the data point-cloud rail: PyO3 bindings over the `laz-rs` crate that decompress and compress LASzip chunked point data without the C++ `laszip` dependency. The owner composes `LazVlr`, the `LasZipDecompressor`/`ParLasZipDecompressor` reader pair, and the `LasZipCompressor`/`ParLasZipCompressor` writer pair into the `laspy` `LazBackend.Lazrs`/`LazrsParallel` path; it admits compressed `.laz`/COPC ingestion through `laspy` and never re-implements the arithmetic range coding `laz-rs` already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lazrs`
- package: `lazrs`
- import: `lazrs`
- owner: `data`
- rail: point-cloud
- version: `0.8.1`
- license: MIT (PyO3 binding over the `laz-rs` Rust crate)
- asset: native extension (`lazrs/lazrs.cpython-<abi>-<plat>.so`); Rayon thread-pool backed; no C++ `laszip` linkage
- entry points: extension-module only; `import lazrs` is the sole entry; no console script
- capability: chunked LASzip decompression and compression for LAS point formats, sequential and Rayon-parallel reader/writer pairs, append-to-existing-LAZ writers, selective field decompression, chunk-table read/write, and one-shot block `decompress_points`/`compress_points` over raw record-data buffers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots
- rail: point-cloud

`LazVlr` parses or mints the LASzip VLR record data that keys every reader/writer. The decompressor and compressor families own the chunked streaming codec; the appender family extends an existing compressed stream. `DecompressionSelection` carries the selective-field bitmask. `LazrsError` is the raised failure for malformed VLRs, codec faults, and I/O errors at the boundary.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [RAIL]                                                    |
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

[ENTRYPOINT_SCOPE]: `LazVlr` descriptor
- rail: point-cloud

`LazVlr(record_data)` parses an existing LASzip VLR; `LazVlr.new_for_compression(point_format_id, num_extra_bytes, use_variable_size_chunks=False)` mints one for a target point format. `record_data` round-trips the serialized VLR bytes that the writer stores back into the LAS header; every surface below is a `LazVlr` member.

| [INDEX] | [SURFACE]                   | [ARGS]          | [CAPABILITY]                               |
| :-----: | :-------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `LazVlr`                    | `(record_data)` | parse an existing LASzip VLR record buffer |
|  [02]   | `new_for_compression`       | `(…)`           | mint a compression VLR for a point format  |
|  [03]   | `uses_variable_size_chunks` | `() -> bool`    | variable-size-chunk mode flag              |
|  [04]   | `chunk_size`                | `() -> int`     | fixed chunk point count                    |
|  [05]   | `item_size`                 | `() -> int`     | uncompressed bytes per point               |
|  [06]   | `record_data`               | `() -> bytes`   | serialized VLR record bytes for the header |

[ENTRYPOINT_SCOPE]: decompressors
- rail: point-cloud

`source` is a binary file-like object positioned at the first compressed point; `selection` is an optional `DecompressionSelection` restricting decoded fields. `decompress_many` fills a caller-supplied writable buffer (a `bytes`-like / NumPy array) sized to a point-count multiple of `item_size`.

| [INDEX] | [SURFACE]                                   | [ARGS]                                      | [CAPABILITY]                                 |
| :-----: | :------------------------------------------ | :------------------------------------------ | :------------------------------------------- |
|  [01]   | `LasZipDecompressor`                        | `(source, record_data, selection=None)`     | construct a sequential decompressor          |
|  [02]   | `LasZipDecompressor.decompress_many`        | `(dest)`                                    | decode points into a writable buffer         |
|  [03]   | `LasZipDecompressor.seek`                   | `(point_idx)`                               | seek to a point index                        |
|  [04]   | `LasZipDecompressor.vlr`                    | `() -> LazVlr`                              | the active VLR descriptor                    |
|  [05]   | `LasZipDecompressor.read_chunk_table_only`  | `() -> list`                                | read the chunk table without decoding points |
|  [06]   | `LasZipDecompressor.read_raw_bytes_into`    | `(bytes)`                                   | copy raw compressed bytes into a buffer      |
|  [07]   | `ParLasZipDecompressor`                     | `(source, vlr_record_data, selection=None)` | construct a parallel decompressor            |
|  [08]   | `ParLasZipDecompressor.decompress_many`     | `(points)`                                  | decode points in parallel into a buffer      |
|  [09]   | `ParLasZipDecompressor.seek`                | `(point_idx)`                               | seek to a point index                        |
|  [10]   | `ParLasZipDecompressor.read_raw_bytes_into` | `(bytes)`                                   | copy raw compressed bytes into a buffer      |

[ENTRYPOINT_SCOPE]: compressors and appenders
- rail: point-cloud

`dest` is a binary writable file-like object; `vlr` keys the codec. `reserve_offset_to_chunk_table` writes the placeholder offset before the first chunk; `done` flushes the final chunk and backpatches the chunk table. Both appenders construct `(dest, laz_vlr_record_data, point_count)`, opening an existing compressed stream and continuing at `point_count`.

| [INDEX] | [SURFACE]                                           | [ARGS]        | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `LasZipCompressor`                                  | `(dest, vlr)` | construct a sequential compressor      |
|  [02]   | `LasZipCompressor.reserve_offset_to_chunk_table`    | `()`          | reserve chunk-table offset placeholder |
|  [03]   | `LasZipCompressor.compress_many`                    | `(points)`    | encode a points buffer                 |
|  [04]   | `LasZipCompressor.compress_chunks`                  | `(chunks)`    | encode a list of pre-formed chunks     |
|  [05]   | `LasZipCompressor.finish_current_chunk`             | `()`          | close the in-progress chunk            |
|  [06]   | `LasZipCompressor.done`                             | `()`          | flush and backpatch the chunk table    |
|  [07]   | `ParLasZipCompressor`                               | `(dest, vlr)` | construct a parallel compressor        |
|  [08]   | `ParLasZipCompressor.reserve_offset_to_chunk_table` | `()`          | reserve chunk-table offset placeholder |
|  [09]   | `ParLasZipCompressor.compress_many`                 | `(points)`    | encode a points buffer in parallel     |
|  [10]   | `ParLasZipCompressor.compress_chunks`               | `(chunks)`    | encode a list of pre-formed chunks     |
|  [11]   | `ParLasZipCompressor.done`                          | `()`          | flush and backpatch the chunk table    |
|  [12]   | `LasZipAppender`                                    | `(…)`         | construct a sequential appender        |
|  [13]   | `LasZipAppender.compress_many`                      | `(points)`    | append-encode a points buffer          |
|  [14]   | `LasZipAppender.compress_chunks`                    | `(chunks)`    | append-encode pre-formed chunks        |
|  [15]   | `LasZipAppender.done`                               | `()`          | flush and backpatch the chunk table    |
|  [16]   | `ParLasZipAppender`                                 | `(…)`         | construct a parallel appender          |
|  [17]   | `ParLasZipAppender.compress_many`                   | `(points)`    | append-encode points buffer, parallel  |
|  [18]   | `ParLasZipAppender.compress_chunks`                 | `(chunks)`    | append-encode pre-formed chunks        |
|  [19]   | `ParLasZipAppender.done`                            | `()`          | flush and backpatch the chunk table    |

[ENTRYPOINT_SCOPE]: block functions and selection
- rail: point-cloud

`decompress_points`/`compress_points` are one-shot block codecs over raw buffers keyed by VLR record data with a `parallel` flag selecting the sequential or Rayon path; every block decompressor opens `(compressed_points_data, laszip_vlr_record_data, decompression_output, …)` and `compress_points(laszip_vlr, uncompressed_points, parallel)` returns `bytes`. `DecompressionSelection(value)` wraps a bitmask composed from the module `SELECTIVE_DECOMPRESS_*` constants. The chunk-table functions read or write the chunk-offset table directly against a stream.

| [INDEX] | [SURFACE]                            | [ARGS]                                | [CAPABILITY]                                  |
| :-----: | :----------------------------------- | :------------------------------------ | :-------------------------------------------- |
|  [01]   | `decompress_points`                  | `(…, parallel)`                       | one-shot block decompress into a buffer       |
|  [02]   | `compress_points`                    | `(…) -> bytes`                        | one-shot block compress to bytes              |
|  [03]   | `decompress_points_with_chunk_table` | `(…, py_chunk_table, selection=None)` | block decompress using a supplied chunk table |
|  [04]   | `read_chunk_table`                   | `(source, vlr) -> list`               | read and decode the chunk table               |
|  [05]   | `read_chunk_table_only`              | `(source, vlr) -> list`               | read the chunk table without decoding points  |
|  [06]   | `write_chunk_table`                  | `(dest, py_chunk_table, vlr)`         | write a chunk table to a stream               |
|  [07]   | `DecompressionSelection`             | `(value)`                             | wrap a `SELECTIVE_DECOMPRESS_*` bitmask       |

[ENTRYPOINT_SCOPE]: selective-decompression bitmask constants
- rail: point-cloud

Module-level `int` flags OR-composed into the `DecompressionSelection(value)` mask; `SELECTIVE_DECOMPRESS_ALL` is the all-fields default and `SELECTIVE_DECOMPRESS_ALL_EXTRA_BYTES` selects every extra-byte channel. The per-field flags map to the LAZ point-record field layers so a COPC reader decodes only the dimensions the consumer keeps.

| [INDEX] | [CONSTANT]                                | [SELECTS]                           |
| :-----: | :---------------------------------------- | :---------------------------------- |
|  [01]   | `SELECTIVE_DECOMPRESS_ALL`                | every field (default)               |
|  [02]   | `SELECTIVE_DECOMPRESS_XY_RETURNS_CHANNEL` | X/Y, return number, scanner channel |
|  [03]   | `SELECTIVE_DECOMPRESS_Z`                  | Z coordinate                        |
|  [04]   | `SELECTIVE_DECOMPRESS_CLASSIFICATION`     | classification byte                 |
|  [05]   | `SELECTIVE_DECOMPRESS_FLAGS`              | classification/scan flags           |
|  [06]   | `SELECTIVE_DECOMPRESS_INTENSITY`          | intensity                           |
|  [07]   | `SELECTIVE_DECOMPRESS_SCAN_ANGLE`         | scan angle                          |
|  [08]   | `SELECTIVE_DECOMPRESS_USER_DATA`          | user-data byte                      |
|  [09]   | `SELECTIVE_DECOMPRESS_POINT_SOURCE_ID`    | point source id                     |
|  [10]   | `SELECTIVE_DECOMPRESS_GPS_TIME`           | GPS time                            |
|  [11]   | `SELECTIVE_DECOMPRESS_RGB`                | RGB color channels                  |
|  [12]   | `SELECTIVE_DECOMPRESS_NIR`                | near-infrared channel               |
|  [13]   | `SELECTIVE_DECOMPRESS_WAVEPACKET`         | full-waveform wavepacket            |
|  [14]   | `SELECTIVE_DECOMPRESS_ALL_EXTRA_BYTES`    | every extra-byte channel            |

## [04]-[IMPLEMENTATION_LAW]

[POINT_CLOUD_LAZ]:
- import: `import lazrs` at boundary scope only; module-level import is banned by the manifest import policy; `lazrs` is reached through the `laspy` `LazBackend.Lazrs`/`LazrsParallel` selector, not a parallel reader type.
- backend axis: `laspy` owns the LAS/LAZ reader/writer surface and selects `lazrs` as the codec backend; the owner sets `LazBackend.Lazrs` (sequential) or `LazBackend.LazrsParallel` (Rayon) on `laspy.open`/`read`, never a hand-stitched chunk loop, and never the C++ `laszip` backend when `lazrs` is admitted.
- vlr axis: `LazVlr(record_data)` parses an existing LASzip VLR; `LazVlr.new_for_compression(point_format_id, num_extra_bytes, ...)` mints one for a write; the VLR descriptor is the single key for every reader/writer/appender, never a re-parsed buffer per call.
- codec axis: the sequential and parallel pairs are a `parallel` row over one codec, not separate concepts; the block `decompress_points`/`compress_points` functions carry the `parallel` flag for buffer-in/buffer-out work, and the streaming `*Decompressor`/`*Compressor` classes carry chunked file-like work — selection between them is the buffer-vs-stream axis, never duplicated logic.
- selection axis: `DecompressionSelection(value)` composes the `SELECTIVE_DECOMPRESS_*` bitmask to restrict decoded fields; the selective row is the only field-narrowing surface, fed to the decompressor or `decompress_points_with_chunk_table`, never a post-decode field drop.
- chunk-table axis: `read_chunk_table`/`read_chunk_table_only`/`write_chunk_table` and `decompress_points_with_chunk_table` own COPC/spatial-index chunk access; the chunk table is read once and threaded into selective block decompression, never re-walked per point.
- COPC stack: a COPC reader composes one dense rail — `laspy` exposes the COPC octree hierarchy and the per-node chunk byte ranges, the chunk table is read once via `read_chunk_table_only`, the consumer-selected dimensions become a `DecompressionSelection` OR of `SELECTIVE_DECOMPRESS_*` flags, and `decompress_points_with_chunk_table(..., selection=)` decodes only the requested nodes' fields directly into the laspy/NumPy record buffer; the spatial-window filter, the field narrowing, and the Rayon parallelism are one pipeline, never a full decode followed by a post-filter.
- evidence: each codec run captures point format id, extra-byte count, chunk size, item size, point count, parallel flag, and selective mask as a point-cloud receipt that the data owner folds into its `MeshPayload`/point-cloud receipt stream.
- boundary: `lazrs` owns LAZ/COPC compressed point-record codec only; LAS container structure, header parsing, dimension typing, and CRS routing stay in `laspy`; raised `LazrsError` is mapped to the data rail's typed failure at the boundary; no NumPy buffer typing or coordinate transform leaks into this admission.

[RAIL_LAW]:
- Package: `lazrs`
- Owns: chunked LASzip/COPC point-record decompression and compression, sequential and Rayon-parallel reader/writer/appender pairs, selective-field decompression, and chunk-table read/write over raw VLR-keyed buffers and file-like streams
- Accept: LAZ/COPC compressed point-cloud ingestion and emission as the `laspy` `LazBackend.Lazrs`/`LazrsParallel` codec backend feeding the data point-cloud owner
- Reject: a hand-rolled LASzip range coder; the C++ `laszip` backend where `lazrs` is admitted; a wrapper-rename of `LasZipDecompressor`/`LasZipCompressor`; LAS header/container parsing pulled into this package; a parallel reader type per `parallel` flag; post-decode field dropping that the selective mask already owns
