# [PY_ARTIFACTS_API_LZ4]

`lz4` supplies the LZ4 compression surface for the artifacts compression rail across two submodules: `lz4.frame` for self-describing framed streams (one-shot, incremental, file-like, and streaming-context paths in both directions) and `lz4.block` for raw block codecs with selectable speed/ratio mode. The package owner composes the frame one-shot/streaming/context-manager surface and the block one-shot surface into the compression owner; it never re-implements the LZ4 codec the native core already owns. lz4 is the low-latency hot-path codec on the same rail as `zstandard` (high-ratio archival), `brotli` (web-transport), and `py7zr` (archive containers) — a level/mode row choice, never a parallel codec owner per profile.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `lz4.frame` / `lz4.block`
- owner: `artifacts`
- rail: compression
- installed: `4.4.5` (PyPI) — manifest-gated `python_version<'3.15'` (no CPython 3.15 wheel declared in the lock; dispatched onto the runtime subprocess lane, never the cp315 core)
- license: BSD-3-Clause (Python bindings); bundled native LZ4 library is BSD-2-Clause
- ABI: C-extension over the vendored LZ4 library `1.9.4` (`lz4.library_version_string()` -> `'1.9.4'`, `lz4.library_version_number()` -> `10904`); the binding version is `lz4.__version__` / `lz4.VERSION` (`'4.4.5'`)
- entry points: none (library only)
- capability: LZ4 frame (self-describing, interoperable, one-shot/incremental/streaming-context/file-like) and block (raw, fastest, mode-selectable, dictionary-primable) compression and decompression with frame-header inspection and incremental decode-state introspection (`eof`/`unused_data`/`needs_input`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types
- rail: compression

`LZ4FrameCompressor`/`LZ4FrameDecompressor` are context-manager objects holding the native streaming context (begin/compress/flush, reset, has_context/started state); the decompressor exposes `eof`/`unused_data`/`needs_input` so a stream concatenation boundary is detected from state, never from a re-parse. `LZ4FrameFile` is the file-like read/write endpoint `lz4.frame.open` returns, exposing the full `BufferedIOBase` surface (`read`/`read1`/`peek`/`readinto`/`write`/`seek`/`tell`/`detach`) for buffer-protocol stacking.

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]                                                  |
| :-----: | :------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `lz4.frame.LZ4FrameCompressor`   | frame compressor   | context manager: `begin()`/`compress(data)`/`flush()`/`reset()` + `has_context`/`started` state |
|  [02]   | `lz4.frame.LZ4FrameDecompressor` | frame decompressor | context manager: incremental `decompress(data, max_length=-1)`/`reset()` + `eof`/`unused_data`/`needs_input` decode-state |
|  [03]   | `lz4.frame.LZ4FrameFile`         | file codec         | file-like framed read/write endpoint (returned by `open`); `BufferedIOBase` (`read`/`read1`/`peek`/`readinto`/`write`/`seek`) |
|  [04]   | `lz4.block.LZ4BlockError`        | block fault        | raw block codec failure (raised when `uncompressed_size` is too small or data is invalid) |

[PUBLIC_TYPE_SCOPE]: frame constants
- rail: compression

The block-size axis fixes the frame max block; the level axis bounds the compression level. These are the exact bound names a frame call passes by reference.

| [INDEX] | [SYMBOL]                                                                                  | [PACKAGE_ROLE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `BLOCKSIZE_DEFAULT` / `BLOCKSIZE_MAX64KB` / `BLOCKSIZE_MAX256KB` / `BLOCKSIZE_MAX1MB` / `BLOCKSIZE_MAX4MB` | block-size axis | frame max-block-size selector (default == 64KB) |
|  [02]   | `COMPRESSIONLEVEL_MIN` / `COMPRESSIONLEVEL_MAX` / `COMPRESSIONLEVEL_MINHC`                 | level axis      | compression-level bounds (HC floor at `MINHC`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming-context, and file
- rail: compression

Frame rows share compression level, block size, content/block checksum, linked-block, stored-size, bytearray-output, and file-mode policy. The compress-side context (`compress_begin`/`compress_chunk`/`compress_flush`) and the decompress-side context (`decompress_chunk` + `create_decompression_context`/`reset_decompression_context`) are the two halves of the same streaming surface — never a single-direction stream.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                  | [CAPABILITY]                          |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `lz4.frame.compress`                       | `compress(data, compression_level=0, block_size=0, content_checksum=0, block_linked=True, store_size=True, return_bytearray=False)` | one-shot framed compress |
|  [02]   | `lz4.frame.decompress`                     | `decompress(data, return_bytearray=False, return_bytes_read=False)`          | one-shot framed decompress            |
|  [03]   | `lz4.frame.compress_begin`                 | `compress_begin(context, source_size=0, compression_level=0, block_size=0, ...)` | start a streaming frame           |
|  [04]   | `lz4.frame.compress_chunk`                 | `compress_chunk(context, data)`                                              | feed a streaming compress chunk       |
|  [05]   | `lz4.frame.compress_flush`                 | `compress_flush(context, end_frame=True, return_bytearray=False)`            | finalize the frame                    |
|  [06]   | `lz4.frame.create_compression_context`     | `create_compression_context()`                                              | allocate a streaming compress context |
|  [07]   | `lz4.frame.create_decompression_context`   | `create_decompression_context()`                                            | allocate a streaming decompress context |
|  [08]   | `lz4.frame.decompress_chunk`               | `decompress_chunk(context, data, max_length=-1, return_bytearray=False) -> (data, bytes_read, end_of_frame)` | feed a streaming decompress chunk; the `end_of_frame` bool gates the concat loop |
|  [09]   | `lz4.frame.reset_decompression_context`    | `reset_decompression_context(context)`                                      | reset a decompress context for reuse  |
|  [10]   | `lz4.frame.open`                           | `open(filename, mode='rb', *, encoding=None, errors=None, newline=None, block_size=0, block_linked=True, compression_level=0, content_checksum=False, block_checksum=False, auto_flush=False, return_bytearray=False, source_size=0)` | file-like framed endpoint (text-mode kwargs wrap a `TextIOWrapper`) |
|  [11]   | `lz4.frame.get_frame_info`                 | `get_frame_info(frame)`                                                     | parse a frame header -> dict (`block_size`/`block_size_id`/`content_checksum`/`content_size`/`block_linked`/`block_checksum`/`skippable`) |

[ENTRYPOINT_SCOPE]: raw block codec
- rail: compression

Block rows carry the speed/ratio `mode`, the `fast`-mode `acceleration`, the `high_compression`-mode `compression` level (1-12, 9 default), the out-of-band `uncompressed_size`, and an optional `dict` initial dictionary that primes the codec for many small, similar payloads. The raw block format is not self-describing — the decoder needs `uncompressed_size` (or the encoder must `store_size=True`, the default, which prepends the size).

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                    | [CAPABILITY]                                       |
| :-----: | :--------------------- | :------------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `lz4.block.compress`   | `compress(source, mode='default', acceleration=1, compression=0, store_size=True, return_bytearray=False, dict=None)` | raw block compress (`default`/`fast`/`high_compression`), dictionary-primable |
|  [02]   | `lz4.block.decompress` | `decompress(source, uncompressed_size=-1, return_bytearray=False, dict=None)`   | raw block decompress (size required when not stored; same `dict` as compress) |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_LZ4]:
- import: `import lz4.frame` / `import lz4.block` at boundary scope only; module-level import is banned by the manifest import policy.
- frame vs block axis: `lz4.frame` is the default (self-describing, interoperable, streaming-capable, file-like); `lz4.block` is the raw speed path when the size is known out-of-band — a row choice, never parallel codec owners.
- modality axis: one-shot (`compress`/`decompress`), streaming-context (`compress_begin`/`compress_chunk`/`compress_flush` and the `decompress_chunk` mirror over `create_decompression_context`/`reset_decompression_context`), context-manager (`LZ4FrameCompressor`/`LZ4FrameDecompressor`), and file-like (`open`/`LZ4FrameFile`) are rows on the frame submodule, never separate codec owners; the decompress context is the symmetric half of the compress context, never a one-direction omission. The streaming decode loop reads `decompress_chunk`'s `end_of_frame` flag (and `LZ4FrameDecompressor.eof`/`unused_data`/`needs_input`) to detect a concatenated-frame boundary, never a length re-derivation.
- mode axis: block `mode` selects `default`/`fast`(`acceleration`)/`high_compression`(`compression` 1-12); frame `compression_level` rides `COMPRESSIONLEVEL_MIN..MAX` with the HC floor at `COMPRESSIONLEVEL_MINHC` and `block_size` rides the `BLOCKSIZE_*` axis — every tuning knob is a call argument, never a parallel codec type.
- dictionary axis: the block `dict` keyword primes both directions with a shared initial dictionary for many small, similar payloads (the same `dict` value MUST round-trip on decode); this is the block-codec ratio lever, not a parallel dictionary-codec owner — frame keeps its own block-linked ratio path.
- evidence: each codec call captures frame vs block, mode/compression level, block size, content/block checksum flags, dictionary presence, and input/output byte lengths as a compression receipt; frame inspection reads `get_frame_info` (full `block_size`/`block_size_id`/`content_checksum`/`content_size`/`block_linked`/`block_checksum`/`skippable` key set).
- boundary: lz4 owns the fast-codec path; high-ratio archival routes to `zstandard`; archive containers route to `py7zr`; web-transport codecs route to `brotli`; bounded-memory streaming zip routes to `stream-zip`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame (one-shot/streaming-context/context-manager/file-like, both directions, decode-state-introspectable) and block (raw, mode-selectable, dictionary-primable) compression and decompression with frame-header inspection
- Accept: low-latency codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a parallel codec owner where the frame/block submodule choice is a row; a compress-only streaming context that omits the `decompress_chunk` mirror; a length re-derivation where `decompress_chunk` already returns `end_of_frame` and the decompressor exposes `eof`/`needs_input`; a parallel dictionary-codec where `dict` is a block-call keyword; identity minting the runtime owns
