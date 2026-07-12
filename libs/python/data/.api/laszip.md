# [PY_DATA_API_LASZIP]

`laszip` supplies the LASzip compressed point-cloud backend for the data point-cloud rail: a pybind11 binding (`laszip_core`, re-exported flat through `import laszip`) that streams LAZ chunks through `LasUnZipper` for read and `LasZipper` for write against a Python file object, plus a `LasZipDll`/`LasZipHeader`/`LasZipPoint` per-point path and `DECOMPRESS_SELECTIVE_*` field-selection constants. The package owner composes `LasUnZipper`/`LasZipper` into the `LAZRS_LASZIP_ADMIT` backend so `laspy` resolves `LazBackend.Laszip` when `lazrs` is absent or fails; it never re-implements the LASzip arithmetic codec the binding already owns and never hand-rolls a parallel LAZ reader.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laszip`
- package: `laszip`
- version: `0.3.0`
- license: MIT
- module: `laszip`
- owner: `data`
- rail: point-cloud
- entry points: none; library use is import-only. `import laszip` re-exports the compiled `laszip_core` extension flat (`from .laszip_core import *`)
- capability: LASzip LAZ stream compression/decompression over a Python file object via `LasUnZipper.decompress_into`/`LasZipper.compress`, point-index `seek`, selective-field decompression through `DECOMPRESS_SELECTIVE_*` masks, per-point `LasZipDll` read/write with `LasZipHeader`/`LasZipPoint`, and `LaszipError` failure signaling — the LAZ backend `laspy` invokes as `LazBackend.Laszip`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream codecs, per-point DLL, and header/point records
- rail: point-cloud

`LasUnZipper` and `LasZipper` are the streaming LAZ codecs `laspy` constructs around a source/destination file object; `LasZipDll` drives the per-point read/write path exposing `LasZipHeader` and `LasZipPoint` by reference. `LaszipError` is raised on any LASzip codec failure (stream open, chunk overflow, malformed header).

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [RAIL]                                                                                 |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `LasUnZipper`  | stream codec  | LAZ decompressor over a source file object; `decompress_into`/`seek`/`header`          |
|  [02]   | `LasZipper`    | stream codec  | LAZ compressor over a destination file object; `compress`/`done`/`header`              |
|  [03]   | `LasZipDll`    | per-point IO  | per-point reader/writer exposing `LasZipHeader`/`LasZipPoint` by reference             |
|  [04]   | `LasZipHeader` | record        | mutable LAS header; `point_data_format`/`point_data_record_length`/scale/offset/extent |
|  [05]   | `LasZipPoint`  | record        | mutable LAS point record (legacy and extended fields)                                  |
|  [06]   | `LaszipError`  | error         | LASzip codec failure raised across stream open, chunk overflow, malformed header       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `LasUnZipper` decode
- rail: point-cloud

`LasUnZipper(source)` opens the LAZ stream; the second-arity constructor accepts a `decompression_selection` mask built from the `DECOMPRESS_SELECTIVE_*` constants to skip fields. `decompress_into` fills a caller-provided `bytearray(n * point_size)` buffer; `seek` repositions to a point index; `header` exposes the parsed `LasZipHeader` whose `point_data_format`/`point_data_record_length` the `laspy` backend asserts against `LasHeader.point_format.id`/`.size` before reading.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                        | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `LasUnZipper.__init__`        | `LasUnZipper(file_object)`                          | open LAZ decompressor over a source file object |
|  [02]   | `LasUnZipper.__init__`        | `LasUnZipper(file_object, decompression_selection)` | open with a `DECOMPRESS_SELECTIVE_*` field mask |
|  [03]   | `LasUnZipper.decompress_into` | `decompress_into(buffer)`                           | decompress points into the provided buffer      |
|  [04]   | `LasUnZipper.seek`            | `seek(index)`                                       | reposition the stream to a point index          |
|  [05]   | `LasUnZipper.header`          | property -> `LasZipHeader`                          | parsed LAS header                               |
|  [06]   | `LasUnZipper.close`           | `close()`                                           | close the decompressor stream                   |

[ENTRYPOINT_SCOPE]: `LasZipper` encode
- rail: point-cloud

`LasZipper(file_object, bytes)` opens the LAZ stream over a destination file object seeded with the serialized LAS header `bytes`. `compress` writes a chunk of packed point bytes; `done` finalizes the stream (flushing the chunk table and EOF).

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                    | [CAPABILITY]                                     |
| :-----: | :------------------- | :------------------------------ | :----------------------------------------------- |
|  [01]   | `LasZipper.__init__` | `LasZipper(file_object, bytes)` | open LAZ compressor seeded with LAS header bytes |
|  [02]   | `LasZipper.compress` | `compress(points_bytes)`        | compress and write a chunk of packed point bytes |
|  [03]   | `LasZipper.done`     | `done()`                        | finalize the stream (chunk table and EOF)        |
|  [04]   | `LasZipper.header`   | property -> `LasZipHeader`      | LAS header bound to the stream                   |

[ENTRYPOINT_SCOPE]: `LasZipDll` per-point IO
- rail: point-cloud

`LasZipDll()` constructs the per-point engine. `open_reader` is polymorphic over a filesystem path or a Python file object; `header`/`point` return the live `LasZipHeader`/`LasZipPoint` by reference for in-place field reads and writes across the `read_point`/`write_point` loop.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :---------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `LasZipDll.__init__`                | `LasZipDll()`                                               | construct the per-point engine                      |
|  [02]   | `LasZipDll.open_reader`             | `open_reader(path)` \| `open_reader(file_object)` -> `bool` | open a reader over a path or file object            |
|  [03]   | `LasZipDll.header`                  | `header()` -> `LasZipHeader` (by reference)                 | live header for in-place field access               |
|  [04]   | `LasZipDll.point`                   | `point()` -> `LasZipPoint` (by reference)                   | live point record for in-place field access         |
|  [05]   | `LasZipDll.read_point`              | `read_point()`                                              | advance the reader by one point                     |
|  [06]   | `LasZipDll.close_reader`            | `close_reader()`                                            | close the reader                                    |
|  [07]   | `LasZipDll.set_header`              | `set_header(header)`                                        | bind a `LasZipHeader` for writing                   |
|  [08]   | `LasZipDll.set_point_type_and_size` | `set_point_type_and_size(point_type, point_size)`           | set the writer point format and record length       |
|  [09]   | `LasZipDll.set_point`               | `set_point(point)`                                          | bind the current `LasZipPoint` for writing          |
|  [10]   | `LasZipDll.write_point`             | `write_point()`                                             | write the current point                             |
|  [11]   | `LasZipDll.update_inventory`        | `update_inventory()`                                        | recompute header counts/extents from written points |
|  [12]   | `LasZipDll.close_writer`            | `close_writer()`                                            | close the writer                                    |
|  [13]   | `LasZipDll.get_warning`             | `get_warning()`                                             | return the last LASzip warning string               |

[ENTRYPOINT_SCOPE]: module-level function and constants
- rail: point-cloud

`get_version` returns the bound LASzip version tuple. The `DECOMPRESS_SELECTIVE_*` constants are `int` masks combined bitwise to build the `decompression_selection` argument; `laspy.DecompressionSelection.to_laszip()` produces this same `int`.

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                                   | [CAPABILITY]                                   |
| :-----: | :---------------------------------------- | :--------------------------------------------- | :--------------------------------------------- |
|  [01]   | `get_version`                             | `get_version()` -> `tuple[int, int, int, int]` | bound LASzip `(major, minor, revision, build)` |
|  [02]   | `DECOMPRESS_SELECTIVE_ALL`                | `int` mask                                     | decompress every field (default)               |
|  [03]   | `DECOMPRESS_SELECTIVE_CHANNEL_RETURNS_XY` | `int` mask                                     | base x/y, return counts, scanner channel only  |
|  [04]   | `DECOMPRESS_SELECTIVE_Z`                  | `int` mask                                     | z field                                        |
|  [05]   | `DECOMPRESS_SELECTIVE_CLASSIFICATION`     | `int` mask                                     | classification field                           |
|  [06]   | `DECOMPRESS_SELECTIVE_FLAGS`              | `int` mask                                     | classification flags                           |
|  [07]   | `DECOMPRESS_SELECTIVE_INTENSITY`          | `int` mask                                     | intensity field                                |
|  [08]   | `DECOMPRESS_SELECTIVE_SCAN_ANGLE`         | `int` mask                                     | scan angle field                               |
|  [09]   | `DECOMPRESS_SELECTIVE_USER_DATA`          | `int` mask                                     | user data field                                |
|  [10]   | `DECOMPRESS_SELECTIVE_POINT_SOURCE`       | `int` mask                                     | point source ID field                          |
|  [11]   | `DECOMPRESS_SELECTIVE_GPS_TIME`           | `int` mask                                     | GPS time field                                 |
|  [12]   | `DECOMPRESS_SELECTIVE_RGB`                | `int` mask                                     | RGB field                                      |
|  [13]   | `DECOMPRESS_SELECTIVE_NIR`                | `int` mask                                     | NIR field                                      |
|  [14]   | `DECOMPRESS_SELECTIVE_WAVEPACKET`         | `int` mask                                     | wave packet field                              |
|  [15]   | `DECOMPRESS_SELECTIVE_BYTE0`..`BYTE7`     | `int` mask                                     | individual extra-byte slots 0..7               |
|  [16]   | `DECOMPRESS_SELECTIVE_EXTRA_BYTES`        | `int` mask                                     | all extra-byte fields                          |

## [04]-[IMPLEMENTATION_LAW]

[POINTCLOUD_LASZIP]:
- import: `import laszip` at boundary scope only; module-level import is banned by the manifest import policy. The flat surface comes from `from .laszip_core import *`, so every symbol resolves directly under `laszip.*` with no submodule reach.
- codec axis: one `LasUnZipper`/`LasZipper` pair owns LAZ stream decode/encode over a Python file object; chunked `decompress_into(buffer)`/`compress(points_bytes)` are the only streaming calls, never a per-chunk codec type — `seek(index)` and `header` carry random access and metadata.
- selection axis: `decompression_selection` is one `int` mask folded from `DECOMPRESS_SELECTIVE_*` constants on the second-arity `LasUnZipper` constructor; `laspy.DecompressionSelection.to_laszip()` is the only producer — field skipping is a mask row, never a parallel reader.
- per-point axis: `LasZipDll` owns the per-point path; `header()`/`point()` return live `LasZipHeader`/`LasZipPoint` references mutated in place across `read_point`/`write_point`, with `set_header`/`set_point_type_and_size`/`set_point`/`update_inventory` driving the writer — never duplicated header/point structs per direction.
- failure axis: `LaszipError` is the single raised failure for stream open, chunk overflow, and malformed header; it crosses the boundary into the data rail's typed error, never silently swallowed.
- append axis: the `laspy` `LaszipBackend.supports_append` is `False` and `create_appender` raises `LaspyException("Laszip backend does not support appending")`; LAZ append routes to `LazBackend.Lazrs`/`LazrsParallel`, never to this codec — the per-point `LasZipDll` writer path is a full rewrite, not an in-place append.
- evidence: each codec run captures point-data format, record length, point count, selective mask, bound LASzip version from `get_version()`, and compressed/decompressed byte length as a point-cloud receipt.
- stack: `laspy.DecompressionSelection.to_laszip() -> int` is the only mask producer; the `laspy` reader binds `LasUnZipper(source, selection)` then drives `decompress_into(bytearray(n * point_size))` per chunk and asserts the binding `header.point_data_format`/`point_data_record_length` against `LasHeader.point_format`. The writer seeds `LasZipper(dest, header_bytes)` from a `set_compressed(False)` header serialized via `LasHeader.write_to`, streams `compress(np.frombuffer(record.array, np.uint8))`, then `done()`; EVLR-count fixups rewrite the LAS header out-of-band, never through this codec. laszip stacks UNDER `laspy` as one `LazBackend.Laszip` selection — never invoked directly by domain code.
- boundary: laszip owns LAZ stream and per-point LASzip codec only; `laspy` selects it as `LazBackend.Laszip` when `lazrs` is unavailable or errors. The package reads/writes against the file object `laspy` supplies; LAS/LAZ container framing, CRS, and array assembly route to `laspy`, never re-implemented here.

[RAIL_LAW]:
- Package: `laszip`
- Owns: LASzip LAZ stream compression/decompression over a Python file object, selective-field decompression masks, per-point `LasZipDll` read/write with header/point records, and `LaszipError` signaling
- Accept: LAZ decode/encode feeding the `laspy` point-cloud owner as the `LazBackend.Laszip` backend
- Reject: wrapper-renames of `LasUnZipper`/`LasZipper`; a hand-rolled LASzip arithmetic codec or LAZ chunk reader; a parallel codec type per chunk or per field; duplicated header/point structs per IO direction; LAS container/CRS framing the `laspy` owner holds; selective decompression rebuilt outside the `DECOMPRESS_SELECTIVE_*` mask; any LAZ append path through this backend (unsupported — route append to `LazBackend.Lazrs`); direct `laszip.*` calls from domain code bypassing the `laspy` `LazBackend.Laszip` selection
