# [PY_BRANCH_API_LZ4]

`lz4` supplies the branch LZ4 compression surface: `lz4.frame` owns self-describing framed compression for artifact packages, and `lz4.block` owns raw-block compression/decompression including the runtime `CRDT_OPLOG_LZ4_DECODE` `DecompressFn` seam over the C# `MessagePackCompression.Lz4BlockArray` envelope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `lz4.frame` / `lz4.block`
- owner: `shared`
- rail: compression and wire
- version: `4.4.5`
- license: `BSD-3-Clause` Python bindings; bundled native LZ4 library is `BSD-2-Clause`
- asset: native wheel with `lz4/frame/_frame` and `lz4/block/_block` C extensions
- namespaces: `lz4`, `lz4.frame`, `lz4.block`
- capability: self-describing frame compression/decompression, incremental frame contexts, file-like frame IO, raw block compression/decompression, mode-selectable block compression, native-version probes, and raw-block decode faults

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types
- rail: compression

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `lz4.frame.LZ4FrameCompressor`   | compressor    | streaming frame compressor context   |
|  [02]   | `lz4.frame.LZ4FrameDecompressor` | decompressor  | streaming frame decompressor context |
|  [03]   | `lz4.frame.LZ4FrameFile`         | file codec    | file-like framed read/write endpoint |
|  [04]   | `lz4.block.LZ4BlockError`        | fault         | raw block decode failure             |

[PUBLIC_TYPE_SCOPE]: frame constants and identity probes
- rail: compression

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------------------------------------------- | :------------ | :----------------------------- |
|  [01]   | `BLOCKSIZE_DEFAULT`                                                        | block size    | default frame max block size   |
|  [02]   | `BLOCKSIZE_MAX64KB`                                                        | block size    | 64 KB frame max block          |
|  [03]   | `BLOCKSIZE_MAX256KB`                                                       | block size    | 256 KB frame max block         |
|  [04]   | `BLOCKSIZE_MAX1MB`                                                         | block size    | 1 MB frame max block           |
|  [05]   | `BLOCKSIZE_MAX4MB`                                                         | block size    | 4 MB frame max block           |
|  [06]   | `COMPRESSIONLEVEL_MIN` / `COMPRESSIONLEVEL_MINHC` / `COMPRESSIONLEVEL_MAX` | level         | frame compression level bounds |
|  [07]   | `lz4.library_version_number` / `lz4.library_version_string`                | native id     | bundled native LZ4 version     |
|  [08]   | `lz4.VERSION` / `lz4.__version__`                                          | binding id    | Python binding version         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming context, and file IO
- rail: compression

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `lz4.frame.compress(...)`                                                           | encode         | one-shot frame compression      |
|  [02]   | `lz4.frame.decompress(...)`                                                         | decode         | one-shot frame decompression    |
|  [03]   | `compress_begin` / `compress_chunk` / `compress_flush`                              | encode stream  | incremental frame compression   |
|  [04]   | `create_decompression_context` / `decompress_chunk` / `reset_decompression_context` | decode stream  | incremental frame decompression |
|  [05]   | `lz4.frame.open(...)`                                                               | file           | file-like framed read/write     |
|  [06]   | `lz4.frame.get_frame_info(frame)`                                                   | inspect        | parse frame metadata            |
|  [07]   | `LZ4FrameDecompressor.decompress`                                                   | decode stream  | bounded frame peel              |

Full signature: `LZ4FrameDecompressor.decompress(data, max_length=-1)`; `eof` and `unused_data` expose completion and tail, and `needs_input` splits the two non-`eof` states — `True` means the frame is input-starved (truncated), `False` means `max_length` capped the output mid-frame (the bomb disposition); `unused_data` stays `None` until the frame completes with trailing bytes, so a tail consumer normalizes `unused_data or b""` (live-verified).

[ENTRYPOINT_SCOPE]: raw block codec
- rail: compression and wire

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :---------------------------------- | :------------- | :-------------------------- |
|  [01]   | `lz4.block.compress(source, ...)`   | encode         | raw block compression       |
|  [02]   | `lz4.block.decompress(source, ...)` | decode         | raw block decompression     |
|  [03]   | `lz4.block.LZ4BlockError`           | fault          | corrupt or mismatched block |

Full signatures for the `...`-abbreviated block codecs:
- [01]-[COMPRESS]: `lz4.block.compress(source, mode='default', acceleration=1, compression=0, store_size=True, return_bytearray=False, dict=None)`
- [02]-[DECOMPRESS]: `lz4.block.decompress(source, uncompressed_size=-1, return_bytearray=False, dict=None)`

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_LZ4]:
- frame axis: `lz4.frame` is the artifacts default for self-describing, interoperable, streaming-capable package compression.
- block axis: `lz4.block` is the raw-block fast path, with `mode` selecting `default`, `fast`, or `high_compression`.
- dictionary axis: the block `dict` keyword primes both encode and decode with a shared initial dictionary for dense small-payload families.
- evidence axis: each codec operation records frame-vs-block, mode, compression level, block size, checksum flags, native library version, binding version, compressed size, and uncompressed size.
- boundary axis: imports happen at boundary scope; package consumers compose the public `lz4.frame` and `lz4.block` modules only.
- artifacts binding: the `package/codec#CODEC` `LZ4` arm defaults to `lz4.frame` for self-describing bundles, `Lz4Knobs` carries frame/block selection, compression level, block size, checksum flags, linked-block policy, and bytearray-output policy as row data, raw `lz4.block` is admitted only where a codec row explicitly selects raw-block payloads, and the boundary encodes canonical payload bytes first — payload structure stays with `msgspec` or the package owner.

[RUNTIME_CRDT_OPLOG_DECODE]:
- blocked-seam law: `CRDT_OPLOG_LZ4_DECODE` names one injected `DecompressFn` port; runtime does not hardwire `lz4` into the codec while the seam remains blocked.
- envelope law: the C# `MessagePackCompression.Lz4BlockArray` frame is a raw LZ4 block, not a self-describing `lz4.frame` payload; the future decode path is `lz4.block.decompress`.
- fault law: corrupt-block `LZ4BlockError` is terminal and folds to the decode boundary fault; it is never retried by the `stamina` owner.
- non-admission law: frame one-shot, streaming, and file-like surfaces stay out of the runtime wire seam unless a separate runtime owner names them.

[RAIL_LAW]:
- Package: `lz4`
- Owns: branch LZ4 frame compression, raw block compression/decompression, version probes, raw block faults, and the blocked runtime `CRDT_OPLOG_LZ4_DECODE` `DecompressFn` seam over `Lz4BlockArray`
- Accept: artifacts frame codec rows, raw-block codec rows, injected runtime decode port, and terminal `LZ4BlockError`
- Reject: wrapper-renames of `compress`/`decompress`, a second codec owner, `frame.*` on the `Lz4BlockArray` channel, runtime hardwiring instead of an injected `DecompressFn`, and folder-level package-surface duplication
