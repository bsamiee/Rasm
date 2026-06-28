# [RASM_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hashing over the
`NonCryptographicHashAlgorithm` `Stream`-derived incremental base — XxHash3/64/32/128
and CRC-32/64 — for snapshot identity, cache keys, receipt fingerprints, benchmark
indexes, and support-bundle correlation. Every concrete algorithm exposes three
discriminated call shapes: a static one-shot (`Hash`/`HashToUIntNN`/`TryHash`), an
allocation-free incremental loop (`Append` + `GetCurrentHashAsUIntNN`/`TryGetHashAndReset`),
and the inherited `Stream.Write`/`AppendAsync(Stream)` sink that hashes a payload in
flight. ABI: net10.0 BCL inbox-shaped NuGet (10.0.9), MIT, no native dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: runtime library (managed, no native dependency; license MIT)
- floor: net10.0 (multi-target; the net10 consumer binds `lib/net10.0`)
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[HASH_TYPES]: hashing surfaces
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                        |
| :-----: | :------------------------------ | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `NonCryptographicHashAlgorithm` | `Stream` base      | incremental lifecycle; write-only `Stream` sink; `Try*`/span reads |
|  [02]   | `XxHash3`                       | hash algorithm     | 64-bit; default fast root; seeded ctor + `GetCurrentHashAsUInt64`  |
|  [03]   | `XxHash64`                      | hash algorithm     | 64-bit; `long` seed; `GetCurrentHashAsUInt64`                      |
|  [04]   | `XxHash32`                      | hash algorithm     | 32-bit; `int` seed; `GetCurrentHashAsUInt32`                       |
|  [05]   | `XxHash128`                     | hash algorithm     | 128-bit; `Low64`/`High64` fields; `GetCurrentHashAsUInt128`        |
|  [06]   | `Crc32`                         | checksum algorithm | 32-bit checksum; `GetCurrentHashAsUInt32`                          |
|  [07]   | `Crc64`                         | checksum algorithm | 64-bit checksum; `GetCurrentHashAsUInt64`                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static one-shot
- rail: snapshot-identity

Width-typed statics live on the concrete algorithm, never on the base; each algorithm exposes only its own width. `XxHash3` exposes `HashToUInt64` (no `HashToUInt32`); `XxHash128` exposes `HashToUInt128`; CRCs expose no 128-bit form. The `seed` overload is `int` on `XxHash32`, `long` elsewhere; CRCs are unseeded.

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]   | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `XxHashNN.Hash(ReadOnlySpan<byte> source, seed = 0)`           | static `byte[]`| one-shot digest bytes from a span             |
|  [02]   | `XxHash3.HashToUInt64(ReadOnlySpan<byte>, long seed = 0)`      | static `ulong` | one-shot 64-bit value, zero allocation        |
|  [03]   | `XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed = 0)`   | static `UInt128`| one-shot 128-bit value                       |
|  [04]   | `XxHash32.HashToUInt32(ReadOnlySpan<byte>, int seed = 0)`      | static `uint`  | one-shot 32-bit value                         |
|  [05]   | `Crc32.HashToUInt32(ReadOnlySpan<byte>)` / `Crc64.HashToUInt64`| static value   | one-shot checksum value                       |
|  [06]   | `TryHash(ReadOnlySpan<byte>, Span<byte>, out int, seed = 0)`   | static `bool`  | digest into caller buffer; `false` if too short |
|  [07]   | `Hash(ReadOnlySpan<byte>, Span<byte>, seed = 0)`               | static `int`   | digest into caller buffer; returns bytes written |

[ENTRYPOINT_SCOPE]: incremental lifecycle (`NonCryptographicHashAlgorithm`)
- rail: snapshot-identity

`Append` accepts span, `byte[]`, or `Stream`; the no-alloc value read is the per-type `GetCurrentHashAsUIntNN`, not a base-level `HashToUIntNN`. `Clone()` (concrete) snapshots running state to fork an independent continuation.

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE]   | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<byte>)` / `Append(byte[])`            | instance `void`| feed payload chunk into running state           |
|  [02]   | `Append(Stream)`                                            | instance `void`| drain a stream synchronously into the hash      |
|  [03]   | `AppendAsync(Stream, CancellationToken)`                   | instance `Task`| drain a stream asynchronously                   |
|  [04]   | `GetCurrentHashAsUIntNN()`                                  | instance value | read current digest as `uint`/`ulong`/`UInt128` |
|  [05]   | `GetCurrentHash()` / `GetCurrentHash(Span<byte>)`          | instance bytes | digest without resetting state                  |
|  [06]   | `TryGetCurrentHash(Span<byte>, out int)`                  | instance `bool`| no-alloc current digest into caller buffer      |
|  [07]   | `GetHashAndReset()` / `TryGetHashAndReset(Span<byte>, out)`| instance       | finalize, emit digest, reset for reuse          |
|  [08]   | `Reset()`                                                  | instance `void`| clear state for a fresh accumulation            |
|  [09]   | `Clone()`                                                  | instance self  | fork independent algorithm at current state     |
|  [10]   | `Write(ReadOnlySpan<byte>)` / `WriteAsync(ReadOnlyMemory)`| `Stream` sink  | use the hash directly as a write-only `Stream`  |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm` is a `Stream` (`CanWrite=true`, read/seek throw) — a hash is a write-only sink, so a payload streams straight in via `Write`/`CopyToAsync(hash)` without staging bytes
- call-shape discriminant: ONE algorithm owns three modes — static one-shot for a buffer in hand, incremental `Append`+`GetCurrentHashAsUIntNN` for a chunked or streamed payload, and `AppendAsync(Stream)` for an async source; selection is the input shape, never a parallel algorithm
- width root: each concrete type exposes exactly its native width's `HashToUIntNN` static and `GetCurrentHashAsUIntNN` instance read; a generic base-level `HashToUIntNN` does not exist
- snapshot root: `Clone()` forks a running accumulation so a shared prefix is hashed once and continued into divergent suffixes (per-variant snapshot identity over a common header)

[STACK]:
- The content-identity rail composes `XxHash128.HashToUInt128` at SEED ZERO as the kernel's one `ContentHash.Of(ReadOnlySpan<byte>) → UInt128` entry (`Rasm.Domain.ContentHash`); the geometry `Geometry/Spatial/reconciliation`+`naming`+`Drawing/pack` GeometryHash, the `Rasm.Element` seam `NodeId`/`ContentAddress`, the Persistence snapshot spine, and the Python/TypeScript wire peers ALL compose this ONE entry — one algorithm, one seed, no second hasher — and the `UInt128` flows into the content-key value objects with no intermediate byte allocation
- An incremental digest over a structured value stacks under a `Span`/buffer-writer pipeline: serialize each field into a pooled buffer, `Append` the span, finalize with `GetHashAndReset` — the hash never owns a result store, the caller's buffer is the only allocation
- A no-throw fingerprint stacks onto a LanguageExt `Fin`/`Option` rail through `TryGetHashAndReset(buffer, out written)`: the `bool` maps to `Fin.Succ`/`Fin.Fail` (or `Option`) at the boundary, so a destination-too-short is a typed failure row, never an exception in the receipt path
- A hashed `Stream` payload (snapshot blob, support bundle) stacks the algorithm as the sink of `payloadStream.CopyToAsync(hashAsStream, ct)` or `AppendAsync(payloadStream, ct)`, threading the same `CancellationToken` the surrounding anyio/Task scope carries

[LOCAL_ADMISSION]:
- Hashing creates non-cryptographic identity, cache, and correlation values only; redaction, security, and tamper evidence use separate declared rails
- Hash algorithm, output width, seed, and input domain are receipt facts; a snapshot identity cannot hide codec, compression, schema, or retention policy
- The instance value read is `GetCurrentHashAsUIntNN`; `HashToUIntNN` is the static one-shot — a per-call-site mix of the two over the same algorithm is the named defect (one accumulation owns one finalize)

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity, cache/receipt fingerprints, stream-fed digests
- Accept: span/array/stream payloads, seeded variants, no-alloc `Try*`/`GetCurrentHashAsUIntNN` reads, `Clone` continuation
- Reject: security claims from non-cryptographic hashes, a base-level `HashToUIntNN` phantom, a hand-rolled FNV/Murmur beside the inbox algorithms
