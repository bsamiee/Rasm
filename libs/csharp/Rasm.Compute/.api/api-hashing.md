# [RASM_COMPUTE_API_HASHING]

`System.IO.Hashing` is the suite's single non-cryptographic identity monopoly:
`XxHash128`/`XxHash3` for whole-artifact and content-address identity, `Crc32`/`Crc64`
for per-frame integrity. Every snapshot key, cache key, receipt fingerprint, and
content-address seed routes through this one owner — a second value-digest helper
(`CommunityToolkit.HighPerformance` `HashCode<T>.Combine`) is rejected at the
staging catalog so the content-address identity never fragments. The substrate
canonical member catalog is `libs/csharp/.api/api-hashing.md`; this overlay carries
only the Compute delta — the seeded-identity, streaming, and content-key law the
Compute pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-hashing.md`
- the full 7-type roster, the one-shot/incremental/streaming call-shape tables, and the package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: snapshot-identity

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- The public roster is exactly 7 types; the nested `XxHash128.Hash128` carrier is `private` (consume the digest as `UInt128` via `HashToUInt128`), and `XxHashShared`/`VectorHelper`/per-type `State` are package-internal and never enter Compute vocabulary.
- The XxHash `seed` is an XxHash-only partition knob (`int` on `XxHash32`, `long` on `XxHash3`/`XxHash64`/`XxHash128`); the `Crc32`/`Crc64` one-shots are seedless (unkeyed integrity). The seed is the cross-machine identity knob the `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key`/`Seed` rail composes — a re-tessellation at a different deflection keys distinctly because the seed mixes the canonical policy scalars.
- The `ReadOnlySpan<byte>` overloads are the zero-copy path off `GetReadOnlySequence`/`MemoryOwner<byte>` slices — never a `ToArray` flatten before hashing.
- `Solver/contract` `SolveProblem.Key(...)` and `Solver/discretization` canonical folds hash canonical little-endian bytes staged through `ArrayBufferWriter<byte>` + `BinaryPrimitives` (`ContentHash.Of(sink.WrittenSpan)`) — never a culture-bearing string render.

## [03]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm` (abstract; `HashLengthInBytes`, `Append`/`Reset`/`GetCurrentHashCore` lifecycle)
- content-address root: `XxHash128.HashToUInt128` → `UInt128` (the suite whole-artifact + content-key form)
- value-digest root: `XxHash3.HashToUInt64` (the fastest seedable value key)
- frame-integrity root: `Crc32`/`Crc64` `HashToUInt32`/`HashToUInt64`
- receipt facts: hash algorithm, output width (`HashLengthInBytes`), input domain, and the seed (when a policy seed partitions identity)

[SEEDED_IDENTITY]:
- The XxHash `seed` is the cross-machine partition knob: `InterchangeIdentity.Key` hashes canonical artifact bytes under `XxHash128.HashToUInt128(bytes, CanonicalForm.Seed(...))` and `CanonicalForm.Seed` itself folds the canonical tag and policy scalars through `XxHash3.HashToUInt64`. A raw interpolated-string seed (`$"{formatKey}|{deflection:R}|…"`) is the rejected drift defect — it keys distinctly across cultures and float renderings; the seed must be the little-endian canonical-scalar digest.
- Crc has no `seed` overload — frame integrity is unkeyed; only the XxHash family carries the policy seed.

[STREAMING_AND_FORKING]:
- A chunked artifact accumulates through `Append(ReadOnlySpan<byte>)` over each `GetReadOnlySequence` segment; the per-chunk identity is a one-shot `XxHash128.HashToUInt128(slice)` and the whole-artifact identity is the running accumulation — never two hashing passes over the same bytes.
- `Clone()` forks a shared-prefix accumulation so a base mesh hashed once branches per delta target without re-feeding the prefix.
- `AppendAsync(Stream, CancellationToken)` is the IO-edge async mirror; a synchronous `Append(Stream)` is the in-memory form.
- CONTIGUOUS vs FRAGMENTED grounding (the protobuf/RMS content-key seam — `api-protobuf` / `api-recyclable-stream`): the verified static one-shots are span-only — `Crc32.HashToUInt32(ReadOnlySpan<byte> source)` (no seed) and `XxHash128.HashToUInt128(ReadOnlySpan<byte> source, long seed = 0)`. A per-frame `Crc32` keys directly off a contiguous payload (`Crc32.HashToUInt32(frame.Payload.Span)` over the `ByteString`/segment span — the `Runtime/transport#ARTIFACT_FRAMES` `FrameEdge` realized form). A whole-artifact `XxHash128` over a MULTI-SEGMENT `RecyclableMemoryStream.GetReadOnlySequence()` view does NOT fit the one-shot span overload: drain it incrementally — `foreach (var seg in seq) hasher.Append(seg.Span);` then `hasher.GetCurrentHashAsUInt128()` — and reserve `XxHash128.HashToUInt128(seq.FirstSpan)` for the single-segment (`seq.IsSingleSegment`) fast case. The hash reads the pooled sequence in place; a `ToArray()` flatten before hashing is the rejected drift.

[LOCAL_ADMISSION]:
- Hashing mints non-cryptographic identity, cache, and correlation values only; redaction, security, and tamper evidence use separate declared rails — a non-cryptographic digest never carries a security claim.
- Hash algorithm, output width, input domain, and seed are receipt facts; snapshot identity cannot hide codec, compression, schema, or retention policy.
- `GetHashCode()` is `[Obsolete(error: true)]` on every type and throws `NotSupportedException` — a hash-algorithm instance is never a dictionary key; the digest primitive (`HashToUInt64`/`HashToUInt128`) is the key, never the algorithm object.
- The allocation-free `Hash(source, dest, seed)` / `HashToUInt*` forms are the default; an allocating `Hash(...) → byte[]` is admitted only at a boundary that already owns a `byte[]`.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity, content-address keys, frame-integrity checksums
- Accept: seeded `XxHash128`/`XxHash3` content keys, `Crc32`/`Crc64` frame integrity, incremental + streaming + forked accumulation, the `UInt128`/`ulong`/`uint` primitive sinks; the wire content-key seam — a `Crc32.HashToUInt32` per-frame integrity over a contiguous `ByteString`/segment span and a `XxHash128` whole-artifact identity over the pooled `RecyclableMemoryStream.GetReadOnlySequence()` view (`api-recyclable-stream`), keying the same bytes the `api-protobuf` `IBufferMessage` fast path stages (`Runtime/codecs#CONTENT_ADDRESSING` / `Runtime/transport#ARTIFACT_FRAMES`)
- Reject: security claims from non-cryptographic hashes; a second value-digest owner (`HashCode<T>.Combine` stays out at the staging catalog); a raw interpolated-string seed beside the canonical-scalar seed digest; a hash-algorithm instance used as a dictionary key (`GetHashCode` throws)
