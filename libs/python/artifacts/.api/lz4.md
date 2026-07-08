# [PY_ARTIFACTS_API_LZ4]

Full surface and stacking: `libs/python/.api/lz4.md` (shared-tier canonical owner).

`lz4` enters artifacts as the `package/codec` low-latency compression row. Artifacts owns frame-vs-block policy, package evidence, and artifact worker placement; the branch canonical owns the package surface.

## [01]-[ARTIFACT_CODEC]

[LOCAL_ADMISSION]:
- The `package/codec#CODEC` `LZ4` arm defaults to `lz4.frame` for self-describing artifact bundles.
- `Lz4Knobs` carries frame/block selection, compression level, block size, checksum flags, linked-block policy, and bytearray-output policy as row data.
- Raw `lz4.block` is admitted only when a codec row explicitly selects raw-block payloads; it is never the default artifact interchange format.
- The boundary encodes canonical payload bytes first, then compresses; payload structure stays with `msgspec` or the package owner.
- Each artifact call contributes frame-vs-block, mode, compression level, block size, checksum flags, compressed size, uncompressed size, native version, and binding version.

[RAIL_LAW]:
- Package: `lz4` (artifacts overlay)
- Owns: artifact-specific LZ4 codec policy and package evidence over the shared branch package surface
- Accept: `package/codec#CODEC` `LZ4` rows through `lz4.frame` or explicit raw-block policy
- Reject: a parallel codec owner, wrapper-renames of codec functions, raw-block defaulting for portable artifacts, and package-surface duplication
