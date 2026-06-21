# [PY_RUNTIME_API_LZ4]

`lz4` (python-lz4) binds the LZ4 C library for high-throughput compression at the runtime wire seam: the `lz4.frame` interoperable frame codec (one-shot `compress`/`decompress`, streaming `LZ4FrameCompressor`/`LZ4FrameDecompressor`, and the file-like `open` reader/writer) and the raw `lz4.block` codec (one-shot `compress`/`decompress` of unframed blocks). It is the dependency-injected decompress owner the runtime transport composes for wire payloads and the deferred CRDT oplog decode path; it never hand-rolls LZ4 framing, block headers, or content checksums the C core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `import lz4.frame`, `import lz4.block`
- owner: `runtime`
- rail: wire
- namespaces: `lz4`, `lz4.frame`, `lz4.block`
- installed: `4.4.5`; license BSD-3-Clause; wheels `cp39`..`cp314`, no `cp315` wheel => GATED `; python_version<'3.15'`
- gate: no `cp315` wheel published for `4.4.5`; the environment marker `; python_version<'3.15'` admits the package on cp314 and below, and the consuming USAGE card runtime [CRDT_OPLOG_LZ4] is [BLOCKED] until a cp315 wheel lands
- capability: LZ4 frame codec (one-shot, streaming, file-like) and raw LZ4 block codec for wire-payload compression and CRDT oplog decode

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame streaming codecs
- rail: wire
- namespace `lz4.frame`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :--------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `LZ4FrameCompressor`   | streaming encoder | incremental frame compressor (context-manager)   |
|  [02]   | `LZ4FrameDecompressor` | streaming decoder | incremental frame decompressor (context-manager) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot codec
- rail: wire
- namespace `lz4.frame`; one self-describing frame carrying block size, content checksum, and content-size header.

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------- | :------------- | :---------------------------- |
|  [01]   | `frame.compress(data, **opts)`   | encode         | bytes to a complete LZ4 frame |
|  [02]   | `frame.decompress(data, **opts)` | decode         | complete LZ4 frame to bytes   |

[ENTRYPOINT_SCOPE]: frame streaming codec
- rail: wire
- namespace `lz4.frame`; methods defined on `LZ4FrameCompressor` / `LZ4FrameDecompressor`, driven across chunked wire reads.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `LZ4FrameCompressor.begin()`                 | encode header  | emit the frame header bytes                 |
|  [02]   | `LZ4FrameCompressor.compress(data)`          | encode chunk   | compress a data chunk into frame bytes      |
|  [03]   | `LZ4FrameCompressor.flush()`                 | encode finish  | emit the frame end-mark and trailing bytes  |
|  [04]   | `LZ4FrameDecompressor.decompress(data, max)` | decode chunk   | decompress frame bytes, optional output cap |
|  [05]   | `LZ4FrameDecompressor.unused_data`           | decode residue | bytes past the frame end-mark               |
|  [06]   | `LZ4FrameDecompressor.needs_input`           | decode state   | whether more input is required for output   |
|  [07]   | `LZ4FrameDecompressor.eof`                   | decode state   | whether the frame end-mark has been reached |

[ENTRYPOINT_SCOPE]: frame file-like codec
- rail: wire
- namespace `lz4.frame`; returns a file object reading/writing a single LZ4 frame.

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `frame.open(fp, mode, **opts)` | file codec     | file-like LZ4-frame reader/writer over a stream |

[ENTRYPOINT_SCOPE]: block one-shot codec
- rail: wire
- namespace `lz4.block`; raw unframed blocks, no self-describing header — caller owns block size and length framing.

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :------------------------------- | :------------- | :----------------------- |
|  [01]   | `block.compress(data, **opts)`   | encode         | bytes to a raw LZ4 block |
|  [02]   | `block.decompress(data, **opts)` | decode         | raw LZ4 block to bytes   |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_TOPOLOGY]:
- frame law: the interoperable LZ4 frame (`lz4.frame`) is the canonical wire form — it self-describes block size, content checksum, and content size, so a frame round-trips across owners without out-of-band metadata; `frame.compress`/`frame.decompress` is the one-shot path and `LZ4FrameCompressor`/`LZ4FrameDecompressor` the streaming path over chunked reads.
- block law: `lz4.block` is the raw, unframed codec for payloads whose length and block size the caller already frames; it carries no header, so it is used only inside an outer envelope and never mixed with frame-codec bytes on the same channel.
- streaming law: streaming decode advances through `decompress(data, max)` with `needs_input`/`eof` state and surfaces post-frame bytes via `unused_data`; chunk boundaries are never assumed to align with frame boundaries.
- decompress-seam law: the runtime transport composes `lz4` as a dependency-injected decompress owner, selected at the boundary, never hardwired into a codec; the same frame codec backs the deferred CRDT oplog decode path.

[LOCAL_ADMISSION]:
- the runtime wire seam admits `lz4` as the LZ4 compress/decompress owner behind a dependency-injected decompress port; the transport composes `frame.decompress` (one-shot) or the `LZ4FrameDecompressor` cycle (streaming) for inbound payloads and `frame.compress` for outbound.
- lz4 publishes no `cp315` wheel for `4.4.5`; admission is gated `; python_version<'3.15'` (cp315-DEFER), and the consuming USAGE card runtime [CRDT_OPLOG_LZ4] stays [BLOCKED] until a cp315 wheel lands — no source-build fallback and no companion lane is opened against the gate.
- the C-extension internals (`lz4._version`, the compiled `_frame`/`_block` modules) are implementation detail; runtime owners compose the public `lz4.frame` / `lz4.block` surface, never the extension modules directly.

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame and raw-block compression/decompression at the runtime wire seam and the deferred CRDT oplog decode path
- Accept: `lz4.frame` one-shot `compress`/`decompress`, the `LZ4FrameCompressor`/`LZ4FrameDecompressor` streaming cycle, `frame.open` file-like access, `lz4.block` raw codec inside an outer envelope, dependency-injected decompress-port composition
- Reject: hand-rolled LZ4 framing, block headers, or content checksums; raw `lz4.block` bytes on a frame channel or vice versa; hardwiring lz4 into a codec instead of injecting the decompress port; opening a source-build or companion lane against the `; python_version<'3.15'` gate; direct use of the compiled `_frame`/`_block` extension modules
