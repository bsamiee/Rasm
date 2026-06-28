# [PY_RUNTIME_API_LZ4]

`lz4` (python-lz4) binds the LZ4 C library for high-throughput compression at the runtime wire seam: the `lz4.frame` interoperable frame codec (one-shot `compress`/`decompress`, streaming `LZ4FrameCompressor`/`LZ4FrameDecompressor`, and the file-like `open` reader/writer) and the raw `lz4.block` codec (one-shot `compress`/`decompress` of unframed blocks). It is the dependency-injected decompress owner the runtime transport composes for wire payloads and the deferred CRDT oplog decode path; it never hand-rolls LZ4 framing, block headers, or content checksums the C core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `import lz4.frame`, `import lz4.block`
- owner: `runtime`
- rail: wire
- namespaces: `lz4`, `lz4.frame`, `lz4.block`
- installed: `4.4.5`
- capability: LZ4 frame codec (one-shot, streaming, file-like) and raw LZ4 block codec for wire-payload compression and CRDT oplog decode

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame streaming codecs
- rail: wire
- namespace `lz4.frame`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :--------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `LZ4FrameCompressor`   | streaming encoder | incremental frame compressor (context-manager)   |
|  [02]   | `LZ4FrameDecompressor` | streaming decoder | incremental frame decompressor (context-manager) |
|  [03]   | `LZ4FrameFile`         | file object       | `BaseStream` file-like LZ4-frame reader/writer returned by `frame.open` |

[PUBLIC_TYPE_SCOPE]: frame constants and library version
- rail: wire
- namespace `lz4.frame`; the named caps replace magic numbers in compressor/file construction.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                                           |
| :-----: | :---------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `frame.BLOCKSIZE_DEFAULT` / `BLOCKSIZE_MAX64KB` / `BLOCKSIZE_MAX256KB` / `BLOCKSIZE_MAX1MB` / `BLOCKSIZE_MAX4MB` | block-size cap | maximum block size selector (`DEFAULT` == 64 KB) |
|  [02]   | `frame.COMPRESSIONLEVEL_MIN` / `COMPRESSIONLEVEL_MINHC` / `COMPRESSIONLEVEL_MAX` | level cap | compression-level anchors (MIN 0, MINHC 3, MAX 16) |
|  [03]   | `lz4.library_version_number()` / `library_version_string()` | version probe | bound LZ4 C-library version (gates `block_checksum` availability `>= 1.8.0`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot codec
- rail: wire
- namespace `lz4.frame`; one self-describing frame carrying block size, content checksum, and content-size header.

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------- | :------------- | :---------------------------- |
|  [01]   | `frame.compress(data, compression_level=0, block_size=BLOCKSIZE_DEFAULT, content_checksum=False, block_checksum=False, block_linked=True, store_size=True, return_bytearray=False)` | encode | bytes to a complete LZ4 frame; `compression_level` 0..16 (HC above 3), `store_size` writes the content-size header |
|  [02]   | `frame.decompress(data, return_bytearray=False, return_bytes_read=False)` | decode | complete LZ4 frame to bytes; `return_bytes_read=True` yields `(decompressed, bytes_consumed)` |
|  [03]   | `frame.get_frame_info(frame)`    | introspect     | parse the frame header to a dict (`block_size`, `block_size_id`, `content_checksum`, `block_checksum`, `block_linked`, `content_size`, `skippable`) without decompressing |

[ENTRYPOINT_SCOPE]: frame streaming codec
- rail: wire
- namespace `lz4.frame`; methods defined on `LZ4FrameCompressor` / `LZ4FrameDecompressor`, driven across chunked wire reads.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `LZ4FrameCompressor(block_size=BLOCKSIZE_DEFAULT, block_linked=True, compression_level=COMPRESSIONLEVEL_MIN, content_checksum=False, block_checksum=False, auto_flush=False, return_bytearray=False)` | encode codec | stateful frame compressor (context-manager); `auto_flush=False` lets the C core buffer across `compress` calls |
|  [02]   | `LZ4FrameCompressor.begin(source_size=0)`    | encode header  | emit the frame header bytes; `source_size` writes the content-size header |
|  [03]   | `LZ4FrameCompressor.compress(data)`          | encode chunk   | compress a data chunk into frame bytes      |
|  [04]   | `LZ4FrameCompressor.flush()`                 | encode finish  | emit the frame end-mark and trailing bytes  |
|  [05]   | `LZ4FrameDecompressor(return_bytearray=False)` | decode codec | stateful frame decompressor (context-manager) |
|  [06]   | `LZ4FrameDecompressor.decompress(data, max_length=-1)` | decode chunk | decompress frame bytes; `max_length` caps output and parks the remainder in `needs_input` |
|  [07]   | `LZ4FrameDecompressor.reset()`               | decode reset   | clear state to reuse the decompressor for a new frame |
|  [08]   | `LZ4FrameDecompressor.unused_data`           | decode residue | bytes past the frame end-mark               |
|  [09]   | `LZ4FrameDecompressor.needs_input`           | decode state   | whether more input is required for output   |
|  [10]   | `LZ4FrameDecompressor.eof`                   | decode state   | whether the frame end-mark has been reached |

[ENTRYPOINT_SCOPE]: frame file-like codec
- rail: wire
- namespace `lz4.frame`; returns a file object reading/writing a single LZ4 frame.

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `frame.open(filename, mode="rb", *, encoding=None, errors=None, newline=None, block_size=BLOCKSIZE_DEFAULT, block_linked=True, compression_level=COMPRESSIONLEVEL_MIN, content_checksum=False, block_checksum=False, auto_flush=False, return_bytearray=False, source_size=0)` | file codec | `LZ4FrameFile` reader/writer over a path or file object; text or binary mode |

[ENTRYPOINT_SCOPE]: block one-shot codec
- rail: wire
- namespace `lz4.block`; raw unframed blocks, no self-describing header — caller owns block size and length framing.

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :------------------------------- | :------------- | :----------------------- |
|  [01]   | `block.compress(source, mode="default", acceleration=1, compression=0, store_size=True, return_bytearray=False, dict=None)` | encode | raw LZ4 block; `mode` is `"default"`/`"fast"` (uses `acceleration`)/`"high_compression"` (uses `compression` 0..16); `store_size=True` prepends a little-endian uncompressed-size prefix; `dict` is an optional shared dictionary |
|  [02]   | `block.decompress(source, uncompressed_size=-1, return_bytearray=False, dict=None)` | decode | raw LZ4 block to bytes; `uncompressed_size` required when the block was compressed with `store_size=False`; `dict` must match the compress-time dictionary |
|  [03]   | `block.LZ4BlockError`            | error          | block codec failure (corrupt block, size mismatch) |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_TOPOLOGY]:
- frame law: the interoperable LZ4 frame (`lz4.frame`) is the canonical wire form — it self-describes block size, content checksum, and content size, so a frame round-trips across owners without out-of-band metadata; `frame.compress`/`frame.decompress` is the one-shot path and `LZ4FrameCompressor`/`LZ4FrameDecompressor` the streaming path over chunked reads. `frame.get_frame_info` reads the header (size, checksum flags, block layout) without decompressing, for routing/sizing decisions before the body decode.
- compression-level law: `compression_level=0` (`COMPRESSIONLEVEL_MIN`) is the fast default; levels `>= 3` (`COMPRESSIONLEVEL_MINHC`) engage LZ4HC for higher ratio at lower throughput, up to `COMPRESSIONLEVEL_MAX`. `block.mode="fast"` trades ratio for speed via `acceleration`; `mode="high_compression"` uses `compression`. The level/mode is a boundary policy on the compress side, never assumed by the decode side (the frame header carries what the decoder needs).
- block law: `lz4.block` is the raw, unframed codec for payloads whose length and block size the caller already frames; it carries no LZ4-frame header. With `store_size=True` it prepends a uncompressed-size prefix so `decompress` self-sizes; with `store_size=False` the caller must pass `uncompressed_size`. Block bytes are used only inside an outer envelope and never mixed with frame-codec bytes on the same channel.
- dictionary law: `block.compress`/`decompress` accept a shared `dict` for dictionary-trained compression of many small similar payloads; the same dictionary must be supplied on both sides, so it is a boundary-pinned parameter carried with the envelope, never an ambient global.
- streaming law: streaming decode advances through `decompress(data, max_length)` with `needs_input`/`eof` state and surfaces post-frame bytes via `unused_data`; chunk boundaries are never assumed to align with frame boundaries, and `auto_flush=False` lets the compressor buffer across `compress` calls for better ratio. `LZ4FrameDecompressor.reset()` reuses one decompressor across frames.
- decompress-seam law: the runtime transport composes `lz4` as a dependency-injected decompress owner, selected at the boundary, never hardwired into a codec; the same frame codec backs the deferred CRDT oplog decode path.

[LOCAL_ADMISSION]:
- the runtime wire seam admits `lz4` as the LZ4 compress/decompress owner behind a dependency-injected decompress port; the transport composes `frame.decompress` (one-shot) or the `LZ4FrameDecompressor` cycle (streaming) for inbound payloads and `frame.compress` for outbound.
- integration rail: the canonical wire stack lowers a `msgspec` `Encoder` output (or a `Raw` op-log envelope) through `frame.compress`, optionally tagging the payload with an `xxhash` digest (see `xxhash.md`) for cheap integrity ahead of the LZ4 `content_checksum`, and the inbound leg runs `frame.decompress` -> `msgspec.Decoder` under a `stamina` `retry_context` scoped to transport faults only (a corrupt-frame `RuntimeError`/`LZ4BlockError` is terminal, never retried). `frame.get_frame_info` sizes the decode buffer before materializing the body.
- the C-extension internals (`lz4._version`, the compiled `_frame`/`_block` modules) are implementation detail; runtime owners compose the public `lz4.frame` / `lz4.block` surface, never the extension modules directly.

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame and raw-block compression/decompression at the runtime wire seam and the deferred CRDT oplog decode path
- Accept: `lz4.frame` one-shot `compress`/`decompress` with explicit `compression_level`/`block_size`/`content_checksum`, `frame.get_frame_info` header inspection, the `LZ4FrameCompressor`/`LZ4FrameDecompressor` streaming cycle with `reset`/`auto_flush`, `frame.open`/`LZ4FrameFile` file-like access, the `BLOCKSIZE_*`/`COMPRESSIONLEVEL_*` named caps, `lz4.block` raw codec (with `mode`/`acceleration`/`compression`/`store_size`/`dict`) inside an outer envelope, dependency-injected decompress-port composition
