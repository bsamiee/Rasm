# [PY_BRANCH_API_LZ4]

`lz4` owns the branch LZ4 compression rail: `lz4.frame` mints self-describing framed compression for artifact packages, and `lz4.block` mints raw-block compression/decompression and the blocked runtime `CRDT_OPLOG_LZ4_DECODE` `DecompressFn` seam over the C# `MessagePackCompression.Lz4BlockArray` envelope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4` (BSD-3-Clause)
- module: `lz4.frame` / `lz4.block`
- namespaces: `lz4`, `lz4.frame`, `lz4.block`
- asset: native wheel; `lz4/frame/_frame` and `lz4/block/_block` C extensions
- rail: compression and wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `lz4.frame.LZ4FrameCompressor`   | compressor    | streaming frame compressor context   |
|  [02]   | `lz4.frame.LZ4FrameDecompressor` | decompressor  | streaming frame decompressor context |
|  [03]   | `lz4.frame.LZ4FrameFile`         | file codec    | file-like framed read/write endpoint |
|  [04]   | `lz4.block.LZ4BlockError`        | fault         | raw block decode failure             |

[PUBLIC_TYPE_SCOPE]: frame constants and identity probes
- [BLOCK_SIZE]: `BLOCKSIZE_DEFAULT` `BLOCKSIZE_MAX64KB` `BLOCKSIZE_MAX256KB` `BLOCKSIZE_MAX1MB` `BLOCKSIZE_MAX4MB`
- [LEVEL]: `COMPRESSIONLEVEL_MIN` `COMPRESSIONLEVEL_MINHC` `COMPRESSIONLEVEL_MAX`
- [NATIVE_ID]: `lz4.library_version_number` `lz4.library_version_string`
- [BINDING_ID]: `lz4.VERSION` `lz4.__version__`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming context, and file IO

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `lz4.frame.compress(...)`                                                           | static   | one-shot frame compression      |
|  [02]   | `lz4.frame.decompress(...)`                                                         | static   | one-shot frame decompression    |
|  [03]   | `compress_begin` / `compress_chunk` / `compress_flush`                              | static   | incremental frame compression   |
|  [04]   | `create_decompression_context` / `decompress_chunk` / `reset_decompression_context` | static   | incremental frame decompression |
|  [05]   | `lz4.frame.open(...)`                                                               | factory  | file-like framed read/write     |
|  [06]   | `lz4.frame.get_frame_info(frame)`                                                   | static   | parse frame metadata            |
|  [07]   | `LZ4FrameDecompressor.decompress`                                                   | instance | bounded frame peel              |

- `LZ4FrameDecompressor.decompress(data, max_length)`: `eof`/`unused_data` mark completion and tail; `needs_input` splits the non-`eof` states — `True` is input-starved (truncated), `False` is `max_length`-capped mid-frame (the bomb disposition); `unused_data` stays `None` until completion, so a tail consumer reads `unused_data or b""`.

[ENTRYPOINT_SCOPE]: raw block codec

| [INDEX] | [SURFACE]                                                                                         | [SHAPE] | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------------------ | :------ | :---------------------- |
|  [01]   | `lz4.block.compress(source, mode, acceleration, compression, store_size, return_bytearray, dict)` | static  | raw block compression   |
|  [02]   | `lz4.block.decompress(source, uncompressed_size, return_bytearray, dict)`                         | static  | raw block decompression |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `lz4.frame` is the self-describing, interoperable, streaming-capable default for artifact package compression; `lz4.block` is the raw-block fast path, `mode` selecting `default`, `fast`, or `high_compression`.
- Block `dict` primes both encode and decode with a shared initial dictionary for dense small-payload families.
- Each codec op records a typed receipt discriminating route, mode, compression level, sizing, and checksum evidence.
- Imports stay at boundary scope; consumers compose the public `lz4.frame` and `lz4.block` modules alone.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): the boundary encodes canonical payload `bytes` via `msgspec.msgpack.encode` / `json.encode` first, then `lz4.frame` (self-describing bundles) or `lz4.block` (the raw `Lz4BlockArray` runtime seam) compresses the encoded buffer — `msgspec` owns payload structure, `lz4` owns the compressed frame.
- artifacts `Lz4Knobs`: the `LZ4` codec arm carries frame/block selection, compression level, block size, checksum flags, linked-block policy, and bytearray-output policy as row data.

[LOCAL_ADMISSION]:
- artifacts `LZ4` defaults to `lz4.frame` for self-describing bundles; raw `lz4.block` admits only where a codec row selects raw-block payloads.
- runtime `CRDT_OPLOG_LZ4_DECODE` binds one injected `DecompressFn` port; the runtime wire seam admits `lz4.block.decompress` alone, the C# `MessagePackCompression.Lz4BlockArray` frame being a raw LZ4 block rather than a self-describing `lz4.frame` payload.
- corrupt-block `LZ4BlockError` is terminal, folds to the decode boundary fault, and is never retried by the `stamina` owner.

[RAIL_LAW]:
- Package: `lz4`
- Owns: branch LZ4 frame compression, raw block compression/decompression, version probes, raw block faults, and the blocked runtime `CRDT_OPLOG_LZ4_DECODE` `DecompressFn` seam over `Lz4BlockArray`
- Accept: artifacts frame codec rows, raw-block codec rows, injected runtime decode port, terminal `LZ4BlockError`
- Reject: wrapper-renames of `compress`/`decompress`, a second codec owner, `frame.*` on the `Lz4BlockArray` channel, runtime hardwiring instead of an injected `DecompressFn`, folder-level package-surface duplication
