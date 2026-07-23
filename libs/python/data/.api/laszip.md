# [PY_DATA_API_LASZIP]

`laszip` owns the native LASzip codec for the data point-cloud rail: a pybind11 binding (`laszip_core`, re-exported flat under `laszip.*`) that streams LAZ chunks through `LasUnZipper` decode and `LasZipper` encode over a Python file object, drives a `LasZipDll` per-point read/write path over `LasZipHeader`/`LasZipPoint` records, and masks decode fields through the `DECOMPRESS_SELECTIVE_*` constants. It decodes and encodes the LAZ arithmetic stream; `laspy` selects it as `LazBackend.Laszip` and holds all LAS/LAZ container framing, CRS, and array assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laszip`
- package: `laszip` (MIT)
- module: `laszip` (flat re-export of the compiled `laszip_core` extension via `from .laszip_core import *`)
- owner: `data`
- rail: point-cloud

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream codecs, the per-point DLL, and header/point records

`LasUnZipper`/`LasZipper` are the streaming codecs `laspy` constructs around a file object; `LasZipDll` drives the per-point path, returning `LasZipHeader`/`LasZipPoint` by reference for in-place field access. `LaszipError` raises on any codec failure across stream open, chunk overflow, and malformed header.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `LasUnZipper`  | stream codec  | LAZ decompressor over a source file object; `decompress_into`/`seek`/`header`          |
|  [02]   | `LasZipper`    | stream codec  | LAZ compressor over a destination file object; `compress`/`done`/`header`              |
|  [03]   | `LasZipDll`    | per-point IO  | per-point reader/writer exposing `LasZipHeader`/`LasZipPoint` by reference             |
|  [04]   | `LasZipHeader` | record        | mutable LAS header; `point_data_format`/`point_data_record_length`/scale/offset/extent |
|  [05]   | `LasZipPoint`  | record        | mutable LAS point record over standard and extended point fields                       |
|  [06]   | `LaszipError`  | error         | LASzip codec failure raised across stream open, chunk overflow, malformed header       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `LasUnZipper` decode

`LasUnZipper(source)` opens the LAZ stream; the second-arity form threads a `decompression_selection` mask built from `DECOMPRESS_SELECTIVE_*` to skip fields. `decompress_into` fills a caller-provided `bytearray(n * point_size)`; `header` exposes the `LasZipHeader` whose `point_data_format`/`point_data_record_length` `laspy` asserts against `LasHeader.point_format` before reading.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `LasUnZipper(file_object)`                          | ctor     | open a LAZ decompressor over a source file object |
|  [02]   | `LasUnZipper(file_object, decompression_selection)` | ctor     | open with a `DECOMPRESS_SELECTIVE_*` field mask   |
|  [03]   | `decompress_into(buffer)`                           | instance | decompress points into the provided buffer        |
|  [04]   | `seek(index)`                                       | instance | reposition the stream to a point index            |
|  [05]   | `header -> LasZipHeader`                            | property | parsed LAS header                                 |
|  [06]   | `close()`                                           | instance | close the decompressor stream                     |

[ENTRYPOINT_SCOPE]: `LasZipper` encode

`LasZipper(file_object, header_bytes)` opens the stream over a destination seeded with the serialized LAS header; `compress` writes a chunk of packed point bytes and `done` finalizes the stream (chunk table and EOF).

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `LasZipper(file_object, header_bytes)` | ctor     | open a LAZ compressor seeded with LAS header bytes |
|  [02]   | `compress(points_bytes)`               | instance | compress and write a chunk of packed point bytes   |
|  [03]   | `done()`                               | instance | finalize the stream (chunk table and EOF)          |
|  [04]   | `header -> LasZipHeader`               | property | LAS header bound to the stream                     |

[ENTRYPOINT_SCOPE]: `LasZipDll` per-point IO

`LasZipDll()` constructs the per-point engine; `open_reader` is polymorphic over a filesystem path or a Python file object, and `header`/`point` return live `LasZipHeader`/`LasZipPoint` references mutated in place across the `read_point`/`write_point` loop.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `LasZipDll()`                                     | ctor     | construct the per-point engine                      |
|  [02]   | `open_reader(path \| file_object) -> bool`        | instance | open a reader over a path or file object            |
|  [03]   | `header() -> LasZipHeader`                        | instance | live header for in-place field access               |
|  [04]   | `point() -> LasZipPoint`                          | instance | live point record for in-place field access         |
|  [05]   | `read_point()`                                    | instance | advance the reader by one point                     |
|  [06]   | `close_reader()`                                  | instance | close the reader                                    |
|  [07]   | `set_header(header)`                              | instance | bind a `LasZipHeader` for writing                   |
|  [08]   | `set_point_type_and_size(point_type, point_size)` | instance | set the writer point format and record length       |
|  [09]   | `set_point(point)`                                | instance | bind the current `LasZipPoint` for writing          |
|  [10]   | `write_point()`                                   | instance | write the current point                             |
|  [11]   | `update_inventory()`                              | instance | recompute header counts/extents from written points |
|  [12]   | `close_writer()`                                  | instance | close the writer                                    |
|  [13]   | `get_warning() -> str`                            | instance | last LASzip warning string                          |

[ENTRYPOINT_SCOPE]: module-level function and constants

`get_version() -> tuple[int, int, int, int]` returns the bound LASzip `(major, minor, revision, build)`. Each `DECOMPRESS_SELECTIVE_*` constant below is an `int` mask combined bitwise into the `decompression_selection` argument, the same `int` `laspy.DecompressionSelection.to_laszip()` produces.

| [INDEX] | [CONSTANT]                                | [SELECTS]                           |
| :-----: | :---------------------------------------- | :---------------------------------- |
|  [01]   | `DECOMPRESS_SELECTIVE_ALL`                | every field (default)               |
|  [02]   | `DECOMPRESS_SELECTIVE_CHANNEL_RETURNS_XY` | x/y, return counts, scanner channel |
|  [03]   | `DECOMPRESS_SELECTIVE_Z`                  | z field                             |
|  [04]   | `DECOMPRESS_SELECTIVE_CLASSIFICATION`     | classification field                |
|  [05]   | `DECOMPRESS_SELECTIVE_FLAGS`              | classification flags                |
|  [06]   | `DECOMPRESS_SELECTIVE_INTENSITY`          | intensity field                     |
|  [07]   | `DECOMPRESS_SELECTIVE_SCAN_ANGLE`         | scan angle field                    |
|  [08]   | `DECOMPRESS_SELECTIVE_USER_DATA`          | user data field                     |
|  [09]   | `DECOMPRESS_SELECTIVE_POINT_SOURCE`       | point source ID field               |
|  [10]   | `DECOMPRESS_SELECTIVE_GPS_TIME`           | GPS time field                      |
|  [11]   | `DECOMPRESS_SELECTIVE_RGB`                | RGB field                           |
|  [12]   | `DECOMPRESS_SELECTIVE_NIR`                | NIR field                           |
|  [13]   | `DECOMPRESS_SELECTIVE_WAVEPACKET`         | wave packet field                   |
|  [14]   | `DECOMPRESS_SELECTIVE_BYTE0`..`BYTE7`     | individual extra-byte slots 0..7    |
|  [15]   | `DECOMPRESS_SELECTIVE_EXTRA_BYTES`        | all extra-byte fields               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `LasUnZipper`/`LasZipper` pair owns LAZ stream decode/encode over a Python file object; `decompress_into(buffer)`/`compress(points_bytes)` are the streaming calls, `seek(index)` and `header` carry random access and metadata.
- `decompression_selection` is one `int` mask folded from `DECOMPRESS_SELECTIVE_*` on the second-arity `LasUnZipper` constructor; field skipping is a mask value, never a per-field reader.
- `LasZipDll` owns the per-point path: `header()`/`point()` return live `LasZipHeader`/`LasZipPoint` references mutated in place across `read_point`/`write_point`, driven for writing by `set_header`/`set_point_type_and_size`/`set_point`/`update_inventory` — one header/point record per engine, not per IO direction.
- `LaszipError` is the single raised failure across stream open, chunk overflow, and malformed header, crossing the boundary into the data rail's typed error.
- append is a full rewrite: the `LasZipDll` writer path recomputes the stream, never an in-place append over an existing LAZ file.
- each codec run captures point-data format, record length, point count, selective mask, `get_version()` build, and compressed/decompressed byte length as a point-cloud receipt.

[STACKING]:
- `laspy`(`.api/laspy.md`): `laspy` selects this codec as `LazBackend.Laszip`, feeds `DecompressionSelection.to_laszip() -> int` into the second-arity `LasUnZipper(source, selection)`, and drives `decompress_into(bytearray(n * point_size))` per chunk after asserting `header.point_data_format`/`point_data_record_length` against `LasHeader.point_format`; the writer seeds `LasZipper(dest, header_bytes)` from a `LasHeader.write_to` serialization, streams `compress(...)`, then `done()`, with EVLR-count fixups rewritten out-of-band.
- data point-cloud rail: laszip composes only under the `laspy` `LazBackend.Laszip` selection, never a direct `laszip.*` call from domain code.

[LOCAL_ADMISSION]:
- import `laszip` at boundary scope; every symbol resolves flat under `laszip.*` through the `laszip_core` re-export, with no submodule reach.

[RAIL_LAW]:
- Package: `laszip`
- Owns: the native LASzip LAZ stream codec over a Python file object, `DECOMPRESS_SELECTIVE_*` field-selection masks, the `LasZipDll` per-point read/write over `LasZipHeader`/`LasZipPoint`, and `LaszipError` signaling
- Accept: LAZ decode/encode feeding the `laspy` point-cloud owner through the `LazBackend.Laszip` selection
- Reject: a hand-rolled LASzip arithmetic codec or LAZ chunk reader; a per-chunk or per-field codec type; duplicated header/point structs per IO direction; LAS container/CRS framing the `laspy` owner holds; selective decompression outside the `DECOMPRESS_SELECTIVE_*` mask; a LAZ append path through this codec; a direct `laszip.*` call from domain code bypassing `LazBackend.Laszip`
