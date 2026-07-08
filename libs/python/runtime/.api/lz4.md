# [PY_RUNTIME_API_LZ4]

Full surface and stacking: `libs/python/.api/lz4.md` (shared-tier canonical owner).

`lz4` enters runtime only as the blocked raw-block decode seam for the CRDT op-log. Runtime owns no frame codec, streaming codec, package codec, or compression policy.

## [01]-[CRDT_OPLOG_DECODE]

[LOCAL_ADMISSION]:
- `CRDT_OPLOG_LZ4_DECODE` names the injected `DecompressFn` port.
- The producer envelope is C# `MessagePackCompression.Lz4BlockArray`; it maps to `lz4.block.decompress`, not `lz4.frame.decompress`.
- The seam remains dependency-injected; runtime code does not import or bind `lz4` directly while the upstream decode path is blocked.
- `LZ4BlockError` is terminal and folds through the decode boundary as a `BoundaryFault`.

[RAIL_LAW]:
- Package: `lz4` (runtime overlay)
- Owns: the blocked `CRDT_OPLOG_LZ4_DECODE` `DecompressFn` seam over the C# `Lz4BlockArray` envelope
- Accept: an injected `lz4.block.decompress` adapter when the seam unblocks
- Reject: hardwired codec imports, `lz4.frame` on the `Lz4BlockArray` channel, retries for corrupt blocks, and package-surface duplication
