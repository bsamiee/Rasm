# [RASM_PERSISTENCE_API_FASTCDC]

`FastCDC.Net` owns content-defined chunk boundary detection over an in-memory `byte[]`: one stateful chunker lazily yields `(Hash, Offset, Length)` cut descriptors whose boundaries survive an interior insertion that shifts every fixed-window boundary, so the snapshot rail re-stores only the chunks an edit changed. `Chunk.Hash` is the 32-bit gear-hash marker the cut decision consumed, never a dedup identity — content addressing and every stream or file IO concern sit downstream.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FastCDC.Net`
- package: `FastCDC.Net` (MIT OR Apache-2.0)
- assembly: `FastCdc.Net`
- namespace: `FastCdc.Net`
- rail: chunking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the whole public surface — a chunker root and its cut value

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :-------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `FastCdc` | sealed class  | stateful chunker over a held `byte[]`, one source one pass |
|  [02]   | `Chunk`   | class         | get-only `uint` `Hash`/`Offset`/`Length` cut descriptor    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and lazy cut enumeration

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `FastCdc(byte[], uint, uint, uint, bool)`   | ctor     | binds source and size window, `eof` on |
|  [02]   | `FastCdc.GetChunks() -> IEnumerable<Chunk>` | instance | lazy cut iterator over the held source |
|  [03]   | `Chunk(uint, uint, uint)`                   | ctor     | hash, offset, and length of one cut    |

- `FastCdc(…)`: validation throws before any cut — `ArgumentNullException`/`ArgumentException` on a null or empty source, `ArgumentOutOfRangeException` on an out-of-bound size, `ArgumentException` on a `min > avg` or `avg > max` ordering; the codec boundary lifts all three onto its `Fin` rail.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every cut folds through the normalized two-mask gear hash: the walk from `minSize` to the center point runs the strict mask, the tail to the source end the relaxed mask, and a run caps at `maxSize` — the shape that holds the average chunk size while leaving boundaries past an interior edit stable. Both masks derive from `avgSize` once at construction.
- Size bounds bind at construction and each ceiling is a public `const uint`: `MinimumMin`/`MinimumMax` hold `minSize` in `[64, 67108864]`, `AverageMin`/`AverageMax` hold `avgSize` in `[256, 268435456]`, `MaximumMin`/`MaximumMax` hold `maxSize` in `[1024, 1073741824]`, ordered `minSize <= avgSize <= maxSize`.
- `eof: true` emits a trailing remainder at or under `minSize` as a final chunk; `eof: false` withholds it, so a streamed source resumes on a fresh instance over the buffered tail.
- One instance holds its source reference for its lifetime and chunks it once, so `ChunkPolicy.Over(source)` mints a chunker per payload and enumeration is single-pass.
- `Chunk.Offset`/`Length` are `uint` over a `byte[]`, so one pass spans one `int`-length array — a larger payload partitions upstream, and the fold runs synchronous in-process inside the codec write under the Persistence `IO`/`Fin` rail.

[STACKING]:
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): each cut span carries an `XxHash3.HashToUInt64` short tag (the `HashPolicy.Content` row) beside its 128-bit key, so `ContentChunker.Novel` probes the cheap tag sketch before the authoritative `holds` compare — a tag miss proves a chunk novel with no index lookup, a false positive costs one fall-through compare.
- `Element/codec#CONTENT_CHUNKING`: sole consumer, where `ChunkPolicy.Over(source)` mints the chunker, `toSeq(…GetChunks())` folds each cut span through the kernel `ContentHash.Of` 128-bit key (the `HashPolicy.Identity` row) into a `ChunkManifest`, and `ContentChunker.Reassemble` rebuilds the payload from that manifest.
- `Store/blobstore#MULTIPART_TRANSFER`: an upload window spans a whole number of content-defined chunks rather than a fixed `PartSize` slice, and the manifest keys dedup against `Query/cache#ARTIFACT_BLOB_INDEX`, so a re-uploaded artifact skips the chunks the index already holds.
- `Compute/Runtime/codecs#GEOMETRY_DELTA`: `DeltaCodec` chunks quantization-normalized geometry bytes under its own `DeltaPolicy` while this surface chunks opaque payload bytes; the two share the gear-hash technique and meet nowhere in code.

[LOCAL_ADMISSION]:
- Callers hand the whole segment as a `byte[]`, and `min`/`avg`/`max`/`eof` ride one `ChunkPolicy` row rather than a call-site literal.
- A `ChunkManifest` reassembles in chunk order; a torn or reordered manifest is a typed `CodecFault.ChunkManifestRejected`.

[RAIL_LAW]:
- Package: `FastCDC.Net`
- Owns: content-defined chunk boundary detection over an in-memory `byte[]`
- Accept: a `byte[]` source under a `ChunkPolicy` size window
- Reject: a hand-rolled gear-shift CDC, a fixed-window framing, a per-edit full re-store, a second content-defined chunker, or `Chunk.Hash` read as a content address
