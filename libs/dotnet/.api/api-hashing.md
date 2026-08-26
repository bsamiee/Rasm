# [RASM_API_HASHING]

`System.IO.Hashing` holds the branch's non-cryptographic digest monopoly: sealed XxHash and CRC algorithms folding through one `NonCryptographicHashAlgorithm` accumulator, each owning a static one-shot, an incremental append/finalize, and a stream drain. One accumulation owns one finalize.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: accumulator base and its sealed algorithms

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                       |
| :-----: | :------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `NonCryptographicHashAlgorithm` | abstract class | append lifecycle, byte-form reads  |
|  [02]   | `XxHash3`                       | sealed class   | 64-bit seeded digest, SIMD-blocked |
|  [03]   | `XxHash32`                      | sealed class   | 32-bit seeded digest               |
|  [04]   | `XxHash64`                      | sealed class   | 64-bit seeded digest               |
|  [05]   | `XxHash128`                     | sealed class   | 128-bit seeded content digest      |
|  [06]   | `Crc32`                         | sealed class   | 32-bit unseeded frame checksum     |
|  [07]   | `Crc64`                         | sealed class   | 64-bit unseeded frame checksum     |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static one-shot digests

`XxHash3`, `XxHash32`, `XxHash64`, and `XxHash128` carry the seeded static family; `Crc32` and `Crc64` carry the same shapes unseeded. Seed type is `int` on `XxHash32` and `long` elsewhere, and every span form has a `byte[]` twin.

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `Hash(ReadOnlySpan<byte>, seed) -> byte[]`                       | static  | digest bytes, one allocation             |
|  [02]   | `Hash(ReadOnlySpan<byte>, Span<byte>, seed) -> int`              | static  | digest into caller buffer, bytes written |
|  [03]   | `TryHash(ReadOnlySpan<byte>, Span<byte>, out int, seed) -> bool` | static  | `false` when the destination is short    |
|  [04]   | `XxHash3.HashToUInt64(ReadOnlySpan<byte>, long) -> ulong`        | static  | 64-bit value, zero allocation            |
|  [05]   | `XxHash32.HashToUInt32(ReadOnlySpan<byte>, int) -> uint`         | static  | 32-bit value, zero allocation            |
|  [06]   | `XxHash64.HashToUInt64(ReadOnlySpan<byte>, long) -> ulong`       | static  | 64-bit value, zero allocation            |
|  [07]   | `XxHash128.HashToUInt128(ReadOnlySpan<byte>, long) -> UInt128`   | static  | 128-bit value, zero allocation           |
|  [08]   | `Crc32.HashToUInt32(ReadOnlySpan<byte>) -> uint`                 | static  | 32-bit checksum value                    |
|  [09]   | `Crc64.HashToUInt64(ReadOnlySpan<byte>) -> ulong`                | static  | 64-bit checksum value                    |

[ENTRYPOINT_SCOPE]: accumulation lifecycle

Seeded construction is `XxHash32(int)`, `XxHash3(long)`, `XxHash64(long)`, and `XxHash128(long)`; `Crc32()` and `Crc64()` construct unseeded. Each sealed algorithm declares its own `Clone()` with a covariant return.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<byte>)`                      | instance | fold a payload chunk into running state |
|  [02]   | `Append(Stream)`                                  | instance | drain a stream synchronously            |
|  [03]   | `AppendAsync(Stream, CancellationToken) -> Task`  | instance | drain a stream asynchronously           |
|  [04]   | `GetCurrentHash() -> byte[]`                      | instance | digest bytes, state untouched           |
|  [05]   | `GetCurrentHash(Span<byte>) -> int`               | instance | digest into caller buffer               |
|  [06]   | `TryGetCurrentHash(Span<byte>, out int) -> bool`  | instance | `false` when the destination is short   |
|  [07]   | `GetHashAndReset() -> byte[]`                     | instance | finalize, emit bytes, reset             |
|  [08]   | `GetHashAndReset(Span<byte>) -> int`              | instance | finalize into caller buffer, reset      |
|  [09]   | `TryGetHashAndReset(Span<byte>, out int) -> bool` | instance | `false` when the destination is short   |
|  [10]   | `Reset()`                                         | instance | clear state for a fresh accumulation    |
|  [11]   | `HashLengthInBytes -> int`                        | property | digest width the buffer must hold       |
|  [12]   | `XxHash3.GetCurrentHashAsUInt64() -> ulong`       | instance | 64-bit value, zero allocation           |
|  [13]   | `XxHash32.GetCurrentHashAsUInt32() -> uint`       | instance | 32-bit value, zero allocation           |
|  [14]   | `XxHash64.GetCurrentHashAsUInt64() -> ulong`      | instance | 64-bit value, zero allocation           |
|  [15]   | `XxHash128.GetCurrentHashAsUInt128() -> UInt128`  | instance | 128-bit value, zero allocation          |
|  [16]   | `Crc32.GetCurrentHashAsUInt32() -> uint`          | instance | 32-bit checksum value                   |
|  [17]   | `Crc64.GetCurrentHashAsUInt64() -> ulong`         | instance | 64-bit checksum value                   |
|  [18]   | `Clone()`                                         | instance | fork an independent running state       |

- `NonCryptographicHashAlgorithm.GetHashCode`: throws `NotSupportedException`, so an accumulator is never a dictionary key.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Input shape selects the mode: a static one-shot for a buffer in hand, `Append` with the instance value read for a chunked payload, `Append(Stream)`/`AppendAsync` for a streaming source.
- Width rides the concrete algorithm; the base owns the append lifecycle and the byte-form reads alone.
- `Append(Stream)` and `AppendAsync` copy the source through a write-only bridge, so the algorithm stages no bytes at any payload size.
- Byte-form digests are big-endian on every `XxHash` and on `Crc64`, little-endian on `Crc32`; the `UInt` value reads carry no encoding, so a cross-runtime wire pins the value form.
- `Clone()` forks a running accumulation, so a shared prefix hashes once and continues into divergent suffixes.
- Multi-part identity is one accumulator over ordered `Append(part)` calls finalized once — never per-part hashes XORed together (order-blind and collision-weak), never an intermediate concatenated buffer.
- Distinct identity domains partition by seed — the `(long seed)` ctor or the seeded one-shot — so identical bytes never collide across cache-key, correlation, and index domains; `Crc32`/`Crc64` carry no seed, so frame integrity stays unkeyed.
- `Append(seg.Span)` per segment drains a multi-segment `ReadOnlySequence<byte>` incrementally before the value read, with the span one-shot reserved for the `IsSingleSegment` fast case; the hash reads pooled bytes in place, never a `ToArray` flatten, and the accumulator is never treated as a `Stream` — the base exposes `Append`/`GetCurrentHash`, no public `Write`.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): `TryGetHashAndReset(Span<byte>, out int)` gates through `guard(written == HashLengthInBytes, error).ToFin()`, so a short destination lands as a `Fin` failure row rather than an exception on the result path.
- `CommunityToolkit.HighPerformance`(`.api/api-highperformance.md`): `Append(writer.WrittenSpan)` folds an `ArrayPoolBufferWriter<byte>`'s staged fields and `GetHashAndReset(Span<byte>)` finalizes into a rented span, so the pool owns every allocation.
- `Microsoft.IO.RecyclableMemoryStream`(`Rasm.Compute/.api/api-recyclable-stream.md`): `AppendAsync(pooledStream, ct)` drains a pooled staging stream straight into the accumulator on the caller's `CancellationToken`.
- within-library: one pooled span feeds two accumulators in a single pass — `XxHash128` for content identity beside `Crc32` for frame integrity — each finalizing through `GetHashAndReset(Span<byte>)` into its own slice of one output buffer.
- `Rasm.AppHost`: feature sticky bucketing and the schedule-key fleet spread call `XxHash3.HashToUInt64` raw over UTF-8 key bytes, because both need a cross-process-stable SPREAD value and neither carries identity; every IDENTITY digest instead folds ordered length-delimited chunks through the kernel `ContentHash.Of` accumulator overload (stream key, environment fingerprint, chain link, macro identity, credential bundle), so a record's synthesized `ToString()`, a culture-rendered number, and an unspecified `FrozenDictionary` order never reach a preimage, and the durable read-back re-mints each chain digest through that same fold (`ChainBroken` on breach); `Crc32`/`Crc64` correlate transport frames and support bundles alone.
- `NodaTime`(`.api/api-nodatime.md`): appends every clock fact as invariant `InstantPattern.ExtendedIso` UTF-8 bytes, never a culture-ambient `ToString`, so a fingerprint stays machine- and culture-stable.
- `Rasm.AppUi`: mints every decode-side consumer through the kernel `ContentHash.Of` entry — the capture `ContentHash` delegate binding, walkthrough per-frame proof, command payload digest, notebook replay-input hashes, and `Collab/sync.md` snapshot-accelerator key — while Compute-minted `ResidencyPayload.ContentKey` splat-tile keys arrive already content-addressed and are never re-hashed; a local `XxHash128` mint site forks the federation seed and breaks the TS/python seed-row reproduction, hex encodings (`:x32`) staying boundary projections.
- `Rasm.Bim`: folds vertex buffer, property-set bytes, georeference frame, and clock fact into one snapshot fingerprint through a single `XxHash3` accumulation finalized by `GetCurrentHashAsUInt64`; a SharpGLTF `MemoryAccessor` region or glTF buffer feeds `Append` zero-copy, and the STJ writer feeds `AsStream()` so wire bytes hash as produced.
- `Rasm.Compute`: `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key` hashes canonical artifact bytes under `XxHash128.HashToUInt128(bytes, CanonicalForm.Seed(...))`, `CanonicalForm.Seed` folding the canonical tag and policy scalars through `XxHash3.HashToUInt64` — a raw interpolated-string seed keys distinctly across cultures and float renderings, so the seed is the little-endian canonical-scalar digest; `SolveProblem.ContentKey` and the discretization folds hash canonical little-endian bytes staged through the kernel `CanonicalWriter` into `ContentHash.Of<TState>`, and per-frame `Crc32.HashToUInt32(frame.Payload.Span)` keys `Runtime/channels#ARTIFACT_FRAMES` integrity beside the whole-artifact `XxHash128` over the pooled sequence.
- `Rasm.Fabrication`: `ContentHash.Of` is the one mint for every egress content key, keyed by the `EgressKind` discriminant and federated to the Persistence `ArtifactKind` rows at the content-key boundary; the nesting `Remnant`/`Stock` addresses and `PairTable.Key`/`PairTable.InnerKey` identities route through the same entry — `PairMemo` keys the `HybridCache` pair-polygon tier on the `PairTable.Key` mint so cache identity and pair identity are one spelling, and `Stock` identity hashes the discriminant salt with every dimension, never area-only.
- `Rasm.Element` (`Projection/address`): every digest reaches XxHash128 through the KERNEL seed-zero entries alone — `ContentAddress.Of<TState>` streams the kernel `CanonicalWriter` at the model grid with zero byte materialization, `BlobKey.Of(ReadOnlySpan<byte>)` keys raw payloads, and no second hasher or seed exists on the boundary.

[LOCAL_ADMISSION]:
- Every non-cryptographic identity, cache, and correlation value in the branch comes from this package; redaction, security, and tamper evidence ride their own declared surfaces.
- Content identity routes through the kernel `Rasm.Domain.ContentHash` entry in one of its two shapes — `Of(ReadOnlySpan<byte>)` for canonical bytes already in hand, `Of<TState>(TState, Action<TState, CanonicalWriter>, double)` for a multi-part preimage the caller frames through the kernel writer, exact-grid by default and caller-grid through the tolerance — both seed-zero `XxHash128` minting the `UInt128` identity currency, with hex and the named `Halves` pair as boundary projections at the consuming boundary.
- Every digest crossing a durable boundary carries algorithm, width, seed, and input domain as fields.
- Allocation-free `Hash(source, destination, seed)`/`HashToUInt*` forms are the default — the nested `XxHash128.Hash128` carrier is `private`, so a digest consumes as `UInt128` — and an allocating `Hash(...) -> byte[]` is admitted only at a boundary that already owns a `byte[]`.
