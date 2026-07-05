# [PY_RUNTIME_API_LZ4]

`lz4` (python-lz4) binds the LZ4 C library at one runtime seam only: the injected `DecompressFn` behind `CRDT_OPLOG_LZ4_DECODE`. The C# producer frames the CRDT op-log as MessagePack under a `MessagePackCompression.Lz4BlockArray` envelope; the Python decode path is UPSTREAM-BLOCKED on that envelope, so the seam stays a dependency-injected port and this catalog carries no live-consumed surface. The frame/block codec breadth, streaming compressors, and file-like access are unconsumed in runtime — a full re-catalog is a defect until the seam unblocks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `import lz4.block`
- owner: `runtime`
- rail: wire
- version: `4.4.5`
- license: `BSD-3-Clause`
- namespaces: `lz4`, `lz4.block`
- capability: BLOCKED — the injected `DecompressFn` decode owner for the CRDT op-log `Lz4BlockArray` envelope; no live-consumed frame/block/streaming surface in runtime

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: injected decode seam (`lz4.block`)
- rail: wire
- namespace `lz4.block`; the C# `Lz4BlockArray` envelope is a length-prefixed raw block, matched by `block.decompress` — the only member the seam binds when unblocked.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `block.decompress(source, uncompressed_size=-1, return_bytearray=False, dict=None)`        | decode         | raw LZ4 block to bytes; matches the `Lz4BlockArray` block frame (`uncompressed_size` supplied when the envelope omits the size prefix) |
|  [02]   | `block.LZ4BlockError`                                                                       | error          | block decode failure (corrupt block, size mismatch) — terminal, never retried |

## [03]-[IMPLEMENTATION_LAW]

[WIRE_TOPOLOGY]:
- blocked-seam law: `CRDT_OPLOG_LZ4_DECODE` names one injected `DecompressFn` port. The runtime transport composes an `lz4.block.decompress`-backed decoder ONLY through that port; no owner hardwires `lz4` into a codec, and the seam stays open (default-unbound) until the C# `Lz4BlockArray` envelope contract is pinned cross-language.
- envelope law: the C# `MessagePackCompression.Lz4BlockArray` frame is a raw LZ4 block, not a self-describing `lz4.frame` — the decode path is `block.decompress` (block codec), never `frame.decompress`; the two codecs never mix on this channel.
- non-admission law: the `lz4.frame` one-shot/streaming/file-like surface, the `BLOCKSIZE_*`/`COMPRESSIONLEVEL_*` caps, and dictionary-trained block compression are UNCONSUMED in runtime; admitting any of them requires a live fence, not a speculative re-catalog.

[LOCAL_ADMISSION]:
- The runtime admits `lz4` as the blocked `DecompressFn` decode owner behind `CRDT_OPLOG_LZ4_DECODE`; the port is injected, the C#-minted `Lz4BlockArray` op-log is honest ingress the runtime decodes and never re-mints.
- A corrupt-block `LZ4BlockError` is terminal (never retried by the `stamina` owner); the seam surfaces it as a `BoundaryFault` at the decode boundary.
- The compiled `_block` extension is implementation detail; the port composes the public `lz4.block` surface only.

[RAIL_LAW]:
- Package: `lz4`
- Owns: the BLOCKED `CRDT_OPLOG_LZ4_DECODE` decode seam — the injected `DecompressFn` port over the C# `Lz4BlockArray` op-log envelope
- Accept: `lz4.block.decompress` bound through the injected `DecompressFn` port when the seam unblocks, terminal `LZ4BlockError`
- Reject: hardwiring `lz4` into a codec instead of the injected port, `frame.*` on the `Lz4BlockArray` channel, cataloguing the unconsumed frame/streaming/file-like/dictionary surface ahead of a live fence
