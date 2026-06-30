# [PY_ARTIFACTS_API_LZ4]

`lz4` supplies the LZ4 compression surface for the artifacts compression rail across two submodules: `lz4.frame` for self-describing framed streams (one-shot, incremental-context, context-manager, and file-like paths in both directions) and `lz4.block` for raw block codecs with selectable speed/ratio mode and an optional shared dictionary. The package owner composes the frame one-shot/streaming-context/context-manager/file-like surface and the block one-shot surface into the `package/codec#CODEC` `LZ4` arm; it never re-implements the LZ4 codec the native core already owns. lz4 is the low-latency hot-path codec on the same `CompressionAlgo` rail as `zstandard` (high-ratio archival), `brotli` (`Content-Encoding: br` / WOFF2 transport), `zlib-ng` (gzip-container interop), and the `py7zr`/`stream-zip`/`stream-unzip` container arms — an `Lz4Knobs` level/block-size/checksum row choice on one union, never a parallel codec owner per profile. The worker `LZ4` arm runs on the `package/codec` subprocess lane (`to_process.run_sync` bounded by `_CODEC_LIMITER`), never on the event loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `lz4.frame` / `lz4.block`
- owner: `artifacts`
- rail: compression
- installed: `4.4.5`
- license: BSD-3-Clause (Python bindings); bundled native LZ4 library is BSD-2-Clause
- target: `cp315-cp315` native wheel (`lz4/frame/_frame.cpython-315-darwin.so` + `lz4/block/_block.cpython-315-darwin.so` C extensions; not pure-Python, not abi3 — one wheel per interpreter minor); bundled native LZ4 is `1.9.4` (`library_version_number() == 10904`)
- entry points: none (library only)
- capability: LZ4 frame (self-describing, interoperable, one-shot/incremental-context/context-manager/file-like) and block (raw, fastest, mode-selectable, dictionary-primable) compression and decompression with frame-header inspection, incremental decode-state introspection (`eof`/`unused_data`/`needs_input`), and native-vs-binding version probes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types
- rail: compression

`LZ4FrameCompressor`/`LZ4FrameDecompressor` are context-manager objects holding the native streaming context; the compressor exposes `begin`/`compress`/`flush`/`reset` plus `has_context()`/`started()` state probes (and clears all state on `__exit__`), and the decompressor exposes `eof`/`unused_data`/`needs_input` so a concatenated-frame boundary is detected from state, never from a re-parse. `LZ4FrameFile` is the file-like read/write endpoint `lz4.frame.open` returns over `compression._common._streams.BaseStream` (3.14+; legacy `_compression.BaseStream` on 3.9-3.13), exposing the full binary `BufferedIOBase` surface for buffer-protocol stacking. `LZ4BlockError` is the single raw-block fault (subclasses `Exception`).

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]                                                  |
| :-----: | :------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `lz4.frame.LZ4FrameCompressor`   | frame compressor   | context manager `LZ4FrameCompressor(block_size=BLOCKSIZE_DEFAULT, block_linked=True, compression_level=0, content_checksum=False, block_checksum=False, auto_flush=False, return_bytearray=False)`; `begin(source_size=0)`/`compress(data)`/`flush()`/`reset()` + `has_context()`/`started()` state |
|  [02]   | `lz4.frame.LZ4FrameDecompressor` | frame decompressor | context manager `LZ4FrameDecompressor(return_bytearray=False)`; incremental `decompress(data, max_length=-1)`/`reset()` + `eof`/`unused_data`/`needs_input` decode-state attributes |
|  [03]   | `lz4.frame.LZ4FrameFile`         | file codec         | file-like framed read/write endpoint (returned by `open`); binary `BufferedIOBase`: `read`/`read1`/`readline`/`readall`/`peek`/`write`/`seek`/`tell`/`flush`/`close`/`closed`/`fileno`/`seekable`/`readable`/`writable` |
|  [04]   | `lz4.block.LZ4BlockError`        | block fault        | raw block codec failure (`Exception` subclass; raised when `uncompressed_size` is too small or data is invalid — catch, grow the size, retry) |

[PUBLIC_TYPE_SCOPE]: frame constants
- rail: compression

The block-size axis fixes the frame max block; the level axis bounds the compression level. These are the exact bound names a frame call passes by reference (real native values shown). `compression_level` below 0 enables proportional "fast acceleration"; above 16 is clamped to 16.

| [INDEX] | [SYMBOL]                                                                                  | [PACKAGE_ROLE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `BLOCKSIZE_DEFAULT` / `BLOCKSIZE_MAX64KB` / `BLOCKSIZE_MAX256KB` / `BLOCKSIZE_MAX1MB` / `BLOCKSIZE_MAX4MB` | block-size axis | frame max-block-size selector (`BLOCKSIZE_DEFAULT` == `BLOCKSIZE_MAX64KB`) |
|  [02]   | `COMPRESSIONLEVEL_MIN` (0) / `COMPRESSIONLEVEL_MINHC` (3) / `COMPRESSIONLEVEL_MAX` (16)    | level axis      | compression-level bounds (HC floor at `MINHC`; levels 4-9 recommended) |

[PUBLIC_TYPE_SCOPE]: identity probes
- rail: compression — the native-library version and the binding version are DISTINCT objects; a consuming page must not treat the binding `VERSION`/`__version__` string as a native-library build probe (mirrors the `brotli` `version` caveat)

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]        | [CAPABILITY]                                                              |
| :-----: | :------------------------------ | :-------------------- | :----------------------------------------------------------------------- |
|  [01]   | `lz4.library_version_number`    | native version (int)  | bundled native LZ4 version as a packed int (`10904` for 1.9.4); the real feature gate — `block_checksum=True` requires `>= 10800` |
|  [02]   | `lz4.library_version_string`    | native version (str)  | bundled native LZ4 version string (`'1.9.4'`)                            |
|  [03]   | `lz4.VERSION` / `lz4.__version__` | binding version (str) | Python-binding/package version (`'4.4.5'`, same object); NOT the native-library version |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming-context, and file
- rail: compression

Frame rows share compression level, block size, content/block checksum, linked-block, stored-size, bytearray-output, and file-mode policy. The compress-side context (`compress_begin`/`compress_chunk`/`compress_flush` over `create_compression_context`) and the decompress-side context (`decompress_chunk` over `create_decompression_context`/`reset_decompression_context`) are the two halves of the same streaming surface — never a single-direction stream.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                  | [CAPABILITY]                          |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `lz4.frame.compress`                       | `compress(data, compression_level=0, block_size=0, content_checksum=0, block_linked=True, store_size=True, return_bytearray=False)` | one-shot framed compress |
|  [02]   | `lz4.frame.decompress`                     | `decompress(data, return_bytearray=False, return_bytes_read=False)`          | one-shot framed decompress (`return_bytes_read=True` -> `(data, bytes_read)`) |
|  [03]   | `lz4.frame.compress_begin`                 | `compress_begin(context, source_size=0, compression_level=0, block_size=0, content_checksum=0, content_size=1, block_linked=0, frame_type=0, auto_flush=1)` | start a streaming frame (header bytes); `block_checksum` is applied via `LZ4FrameCompressor`, not this native kwarg set |
|  [04]   | `lz4.frame.compress_chunk`                 | `compress_chunk(context, data, return_bytearray=False)`                       | feed a streaming compress chunk       |
|  [05]   | `lz4.frame.compress_flush`                 | `compress_flush(context, end_frame=True, return_bytearray=False)`            | finalize the frame (footer); `end_frame=False` flushes a decodable prefix and continues |
|  [06]   | `lz4.frame.create_compression_context`     | `create_compression_context()`                                              | allocate a streaming compress context |
|  [07]   | `lz4.frame.create_decompression_context`   | `create_decompression_context()`                                            | allocate a streaming decompress context |
|  [08]   | `lz4.frame.decompress_chunk`               | `decompress_chunk(context, data, max_length=-1, return_bytearray=False) -> (data, bytes_read, end_of_frame)` | feed a streaming decompress chunk; the `end_of_frame` bool gates the concat loop |
|  [09]   | `lz4.frame.reset_decompression_context`    | `reset_decompression_context(context)`                                      | reset a decompress context for reuse  |
|  [10]   | `lz4.frame.open`                           | `open(filename, mode='rb', encoding=None, errors=None, newline=None, block_size=0, block_linked=True, compression_level=0, content_checksum=False, block_checksum=False, auto_flush=False, return_bytearray=False, source_size=0)` | file-like framed endpoint; text modes (`'rt'`/`'wt'`/`'xt'`/`'at'`) wrap the binary `LZ4FrameFile` in an `io.TextIOWrapper` with `encoding`/`errors`/`newline` (all rejected in binary mode) |
|  [11]   | `lz4.frame.get_frame_info`                 | `get_frame_info(frame)`                                                     | parse a frame header -> dict (`block_size`/`block_size_id`/`content_checksum`/`content_size`/`block_linked`/`block_checksum`/`skippable`) |

[ENTRYPOINT_SCOPE]: raw block codec
- rail: compression

Block rows carry the speed/ratio `mode`, the `fast`-mode `acceleration`, the `high_compression`-mode `compression` level (1-12, 9 default), the out-of-band `uncompressed_size`, and an optional `dict` initial dictionary that primes the codec for many small, similar payloads. The raw block format is not self-describing — the decoder needs `uncompressed_size` (or the encoder must `store_size=True`, the default, which prepends the size). The block submodule has no streaming/file modality (raw blocks only); framed/streaming/file work routes to `lz4.frame`.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                    | [CAPABILITY]                                       |
| :-----: | :--------------------- | :------------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `lz4.block.compress`   | `compress(source, mode='default', acceleration=1, compression=0, store_size=True, return_bytearray=False, dict=None)` | raw block compress (`'default'`/`'fast'`/`'high_compression'`), dictionary-primable |
|  [02]   | `lz4.block.decompress` | `decompress(source, uncompressed_size=-1, return_bytearray=False, dict=None)`   | raw block decompress (size required when not stored; the SAME `dict` as compress must round-trip) |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_LZ4]:
- import: `import lz4.frame` / `import lz4.block` at boundary scope only (`package/codec` declares `lazy import lz4.frame`); module-level import is banned by the manifest import policy.
- frame vs block axis: `lz4.frame` is the default (self-describing, interoperable, streaming-capable, file-like) and is the `package/codec#CODEC` `LZ4` arm's path; `lz4.block` is the raw speed path when the size is known out-of-band — a row choice, never parallel codec owners.
- modality axis: one-shot (`compress`/`decompress`), streaming-context (`compress_begin`/`compress_chunk`/`compress_flush` and the `decompress_chunk` mirror over `create_decompression_context`/`reset_decompression_context`), context-manager (`LZ4FrameCompressor`/`LZ4FrameDecompressor`), and file-like (`open`/`LZ4FrameFile`) are rows on the frame submodule, never separate codec owners; the decompress context is the symmetric half of the compress context, never a one-direction omission. The streaming decode loop reads `decompress_chunk`'s `end_of_frame` flag (and `LZ4FrameDecompressor.eof`/`unused_data`/`needs_input`) to detect a concatenated-frame boundary — the exact seam `package/codec` `_walked(blob, _lz4_frame)` reads to recover one bundle member per concatenated frame — never a length re-derivation.
- mode axis: block `mode` selects `'default'`/`'fast'`(`acceleration`)/`'high_compression'`(`compression` 1-12); frame `compression_level` rides `COMPRESSIONLEVEL_MIN..MAX` with the HC floor at `COMPRESSIONLEVEL_MINHC`, sub-zero values enabling proportional fast-acceleration, and `block_size` rides the `BLOCKSIZE_*` axis — every tuning knob is the `Lz4Knobs` (`compression_level`/`block_size`/`content_checksum`) frozen-struct call argument the `package/codec` `CodecProfile.lz4` case carries, never a parallel codec type. `block_checksum=True` requires native LZ4 `>= 1.8.0` (`library_version_number() >= 10800`, satisfied by the bundled `1.9.4`) and raises `RuntimeError` on an older library — gate it on the version probe, never assume.
- dictionary axis: the block `dict` keyword primes both directions with a shared initial dictionary for many small, similar payloads (the same `dict` value MUST round-trip on decode); this is the block-codec ratio lever, not a parallel dictionary-codec owner — frame keeps its own block-linked ratio path.
- integration: at the boundary, encode the canonical payload via `msgspec.msgpack.encode(...)`, frame-compress it with an `Lz4Knobs`-tuned `lz4.frame.compress(payload, compression_level=k.compression_level, block_size=block_size, content_checksum=k.content_checksum, store_size=True)` (the level/block-size/checksum knobs read as named fields off the `CodecProfile.lz4` case, never positional ints), wrap the producing boundary call in a `stamina`-retried block, and lift the result onto the universal `expression.Result[bytes, ArtifactError]` rail so an `LZ4BlockError`/`RuntimeError` becomes an `Error` case at the seam rather than a raised exception crossing the owner. The heavy codec body never runs on the event loop: `package/codec` `_compress`/`_recover` offload the `LZ4` arm through `to_process.run_sync` (the subprocess worker lane, since lz4 does not release the GIL like the in-wheel zstandard/zlib-ng arms) bounded by the one shared `_CODEC_LIMITER` `CapacityLimiter`. Stamp a `structlog` event / `opentelemetry` span carrying frame-vs-block, mode/compression-level, block size, content/block-checksum flags, dictionary presence, `library_version_string()`, and input/output byte lengths. For many small, similar block payloads, prime `dict=` once and thread the same dictionary through every per-record `compress`/`decompress`; for a streaming sink, drive `LZ4FrameFile`/the `compress_begin`/`compress_chunk`/`compress_flush` context so back-pressure stays in the codec state, not in an in-RAM accumulation buffer.
- evidence: each codec call captures frame-vs-block, mode/compression level, block size, content/block checksum flags (the `verified` count reads `get_frame_info(frame)["content_checksum"]`, never a hardcoded zero), dictionary presence, native `library_version_string()`, and input/output byte lengths as a compression receipt contributed through the runtime `ReceiptContributor` port onto the single `ArtifactReceipt` family — never a parallel lz4-only receipt shape. Frame inspection reads `get_frame_info` (full `block_size`/`block_size_id`/`content_checksum`/`content_size`/`block_linked`/`block_checksum`/`skippable` key set); the decompressed-size field reads `get_frame_info(frame)["content_size"]`.
- boundary: lz4 owns the fast-codec path (`package/codec#CODEC` `LZ4` arm; `package/delta#DELTA` reaches it only as a `detools`-framed `DeltaCompression` `lz4` payload row, never re-implementing the codec; `exchange/detect#CONTAINER` reaches it only as the `Container.LZ4` / `application/x-lz4` sniff token, detection-only). High-ratio archival routes to `zstandard`; archive containers route to `py7zr`; web/WOFF2-transport codecs route to `brotli`; gzip-container interop routes to the shared `zlib-ng` substrate; bounded-memory streaming zip routes to `stream-zip`/`stream-unzip`; recovery is bomb-bounded against `_DECOMPRESS_CEILING`; failures surface as `LZ4BlockError`/`RuntimeError` lifted to the `Result` rail; identity minting the runtime owns; live UI stays outside this package.

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame (one-shot/streaming-context/context-manager/file-like, both directions, decode-state-introspectable) and block (raw, mode-selectable, dictionary-primable) compression and decompression with frame-header inspection and native-vs-binding version probes
- Accept: low-latency codec service feeding the `package/codec#CODEC` `LZ4` arm via an `Lz4Knobs` row, running on the subprocess worker lane bounded by `_CODEC_LIMITER`, lifted onto the `expression.Result` rail
- Reject: wrapper-renames of `compress`/`decompress`; a parallel codec owner where the frame/block submodule choice or an `Lz4Knobs` field is a row; a compress-only streaming context that omits the `decompress_chunk` mirror; a length re-derivation where `decompress_chunk` already returns `end_of_frame` and the decompressor exposes `eof`/`unused_data`/`needs_input` (the `_walked`/`_lz4_frame` recovery seam); a parallel dictionary-codec where `dict` is a block-call keyword; treating the binding `VERSION`/`__version__` string as a native-library version probe; setting `block_checksum=True` without gating on `library_version_number() >= 10800`; running the GIL-holding codec body on the event loop instead of the `to_process` lane; a parallel lz4-only receipt shape where `ArtifactReceipt` already owns the case; identity minting the runtime owns
