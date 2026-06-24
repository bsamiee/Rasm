# [RASM_PERSISTENCE_API_FASTCDC]

`FastCDC.Net` supplies the FastCDC normalized gear-hash content-defined chunker over an
in-memory `byte[]`: one stateful one-shot `FastCdc` instance lazily yields an
`IEnumerable<Chunk>` of `(Hash, Offset, Length)` cut descriptors whose boundaries survive an
interior insertion that would shift every fixed-window boundary, so the snapshot/versioning rail
re-stores only the chunks an edit actually changes. The gear-hash `Chunk.Hash` is a 32-bit
boundary marker, never a dedup identity — the content key is `XxHash128` over the chunk bytes,
computed downstream by `System.IO.Hashing`. This is the opaque-byte chunker for snapshot frames
and the object-store multipart window; it owns no stream or file IO and no content addressing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FastCDC.Net`
- package: `FastCDC.Net`
- version: `1.0.1`
- license: `MIT OR Apache-2.0`
- assembly: `FastCdc.Net` (single-target `net7.0`; the `net10.0` consumer binds the one `lib/net7.0` asset)
- namespace: `FastCdc.Net`
- asset: runtime library
- rail: chunking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: CDC family — the entire public surface is two types
- rail: chunking

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `FastCdc` | `sealed class` chunker root | stateful one-shot content-defined chunker over a held `byte[]` |
|  [02]   | `Chunk`   | `class` cut value           | `Hash`/`Offset`/`Length` cut descriptor, get-only             |

`Chunk` is a reference type with a three-arg ctor `Chunk(uint hash, uint offset, uint length)` and
three get-only `uint` properties; `FastCdc.GetChunks()` yields `Chunk` instances and signals
exhaustion with a `null` reference, so a consumer reads `Hash`/`Offset`/`Length` and never mutates.

`FastCdc` exposes six public size-bound constants (all `const uint`): `MinimumMin = 64`,
`MinimumMax = 67108864`, `AverageMin = 256`, `AverageMax = 268435456`, `MaximumMin = 1024`,
`MaximumMax = 1073741824`. A `ChunkPolicy` row sources its `min/avg/max` window from these floors
so the gear-hash mask is a policy value, never a free literal.

`Cut`, `Next`, `CenterSize`, `Logarithm2`, `CeilDiv`, and `Mask` are `internal`/`private`
implementation members of the gear-hash kernel; only `GetChunks()` and the constructor are public.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and chunking — one ctor, one enumeration method
- rail: chunking

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `FastCdc(byte[] source, uint minSize, uint avgSize, uint maxSize, bool eof = true)` | ctor   | one constructor; `eof` is an optional parameter (default `true`), not a second overload |
|  [02]   | `IEnumerable<Chunk> GetChunks()`                                           | enumeration    | lazy iterator over `Next()`; yields `Chunk` rows until a `null` cut ends the stream |

The ctor validates eagerly: `null`/empty `source` throw `ArgumentNullException`/`ArgumentException`;
`minSize`/`avgSize`/`maxSize` outside their constant bounds throw `ArgumentOutOfRangeException`;
`minSize > avgSize` or `avgSize > maxSize` throw `ArgumentException`. It derives the two gear masks
once — `_maskS = Mask(Logarithm2(avgSize) + 1)`, `_maskL = Mask(Logarithm2(avgSize) - 1)` — and
captures the `source` reference for the instance lifetime.

## [04]-[IMPLEMENTATION_LAW]

[CDC_TOPOLOGY]:
- the public surface is exactly `FastCdc` (sealed) and `Chunk`; everything else is `internal`/`private`
- `Chunk` carries `Hash` (uint, the gear-hash boundary marker), `Offset` (uint), and `Length` (uint)
- size bounds: `minSize` in `[64, 67108864]`, `avgSize` in `[256, 268435456]`, `maxSize` in `[1024, 1073741824]`, with `minSize <= avgSize <= maxSize`
- `eof = true` (default): a final remainder `<= minSize` is emitted as a last chunk; `eof = false`: the remainder is withheld (`(num4, 0u)` cut → `Next()` returns `null`), so a streamed source can resume with a fresh instance over the buffered tail
- the cut walks `[minSize, centerSize)` under the strict `_maskS` mask, then `[centerSize, sourceSize)` under the relaxed `_maskL` mask, capping a run at `maxSize` — the normalized two-mask gear-hash that makes the average chunk size hold
- `GetChunks()` is a lazy `yield` iterator; the `byte[]` is held for the chunker's lifetime

[STATEFUL_ONE_SHOT]:
- a `FastCdc` instance mutates `_bytesProcessed`/`_bytesRemaining` as `GetChunks()` advances, so it chunks a source exactly once — a re-chunk requires a fresh instance (`ChunkPolicy.Over(source)` mints one per payload)
- offsets and lengths are `uint`, so the addressable window is `< 4 GiB`; a payload above the `uint`-offset window partitions upstream before chunking — the package owns no stream or file IO and no large-object windowing
- the chunker is in-process and synchronous; it surfaces no async mirror — the snapshot fold runs it inside the codec write under the Persistence `IO`/`Fin` rail

[INTEGRATION_STACK]:
- `Version/snapshots#CONTENT_CHUNKING` is the sole consumer: `ChunkPolicy.Over(byte[]) => new FastCdc(source, MinSize, AvgSize, MaxSize, eof: true)`, then `toSeq(policy.Over(source).GetChunks())` folds the cut sequence into a `ChunkManifest`
- each `Chunk` is keyed by `XxHash128.HashToUInt128(chunkBytes)` (`api-hashing` `HashPolicy.Identity`) for the dedup content address, and stamped with a `XxHash3.HashToUInt64` 64-bit `ShortTag` (`HashPolicy.Content`) — `Novel` probes the cheap `ShortTag` bloom/sketch pre-filter before the authoritative 128-bit `holds` compare, so a tag-miss proves a chunk novel without the full lookup and a `ShortTag` false positive only costs one fall-through compare
- the manifest content keys dedup against `Query/cache#ARTIFACT_BLOB_INDEX`; the `Store/remote#MULTIPART_TRANSFER` window spans a whole number of content-defined chunks rather than a fixed `PartSize` slice, so a re-uploaded artifact skips the chunks the index already holds
- this owner aligns with `Compute/interchange#GEOMETRY_DELTA` `DeltaCodec` at the rolling-hash technique level only — Compute owns the geometry-aware structural delta over geometry columns, this owns the opaque-byte content-defined chunk; the two meet at the technique, never at the code

[LOCAL_ADMISSION]:
- callers pass the full segment as a `byte[]`; the chunker does not stream or partial-fill the buffer
- chunk dedup identity is the downstream `XxHash128` content key, never `Chunk.Hash` (the gear-hash boundary marker)
- size policy (min/avg/max/eof) is a `ChunkPolicy` row, not a per-call concern
- `Chunk.Offset`/`Length` is a `uint` window the manifest reconstructs in chunk order; a torn or reordered manifest is a typed reassembly rejection

[RAIL_LAW]:
- Package: `FastCDC.Net`
- Owns: content-defined chunk boundary detection over an in-memory `byte[]`
- Accept: `byte[]` source with a `ChunkPolicy` size window
- Reject: hand-rolled gear-shift CDC; a fixed-window framing; a per-edit full re-store; a second content-defined chunker beside this one; treating `Chunk.Hash` as a content address
