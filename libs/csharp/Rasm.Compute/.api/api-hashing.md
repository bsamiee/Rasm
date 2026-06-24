# [RASM_COMPUTE_API_HASHING]

`System.IO.Hashing` is the suite's single non-cryptographic identity monopoly:
`XxHash128`/`XxHash3` for whole-artifact and content-address identity, `Crc32`/`Crc64`
for per-frame integrity. Every snapshot key, cache key, receipt fingerprint, and
content-address seed routes through this one owner — a second value-digest helper
(`CommunityToolkit.HighPerformance` `HashCode<T>.Combine`) is rejected at the
staging catalog so the content-address identity never fragments.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- version: `10.0.9`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- license: MIT (.NET Foundation)
- bound asset: `lib/net10.0/System.IO.Hashing.dll` (multi-target; consumer `net10.0` binds the `net10.0` lib, XML docs shipped)
- asset: runtime library
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hashing surfaces
- rail: snapshot-identity
- The public roster is exactly these 7 types plus the nested `XxHash128.Hash128` value struct; `XxHashShared`/`VectorHelper`/per-type `State` are package-internal and never enter Compute vocabulary.

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                   |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------------------ |
|  [01]   | `NonCryptographicHashAlgorithm` | algorithm base     | incremental `Append`/`GetCurrentHash`/`Reset` lifecycle root  |
|  [02]   | `XxHash3`                       | hash algorithm     | fastest 64-bit digest (`HashToUInt64`); seedable; the suite default value-digest |
|  [03]   | `XxHash128`                     | hash algorithm     | 128-bit content-address digest (`HashToUInt128` → `UInt128`); seedable |
|  [04]   | `XxHash64`                      | hash algorithm     | legacy 64-bit digest (`HashToUInt64`)                         |
|  [05]   | `XxHash32`                      | hash algorithm     | 32-bit digest (`HashToUInt32`); `int` seed                    |
|  [06]   | `Crc32`                         | checksum algorithm | 32-bit frame integrity checksum (`HashToUInt32`)              |
|  [07]   | `Crc64`                         | checksum algorithm | 64-bit frame integrity checksum (`HashToUInt64`)              |
|  [08]   | `XxHash128.Hash128`             | value struct       | nested `(ulong Low64, ulong High64)` 128-bit carrier; consumers prefer `UInt128` via `HashToUInt128` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot static hash (the content-address rail)
- rail: snapshot-identity
- Each concrete type carries the same static one-shot family; the `seed` parameter is the cross-machine identity knob the `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key`/`Seed` rail composes (a re-tessellation at a different deflection keys distinctly because the seed mixes the canonical policy scalars). The `ReadOnlySpan<byte>` overloads are the zero-copy path off `GetReadOnlySequence`/`MemoryOwner<byte>` slices — never a `ToArray` flatten before hashing.

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]    | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `Hash(byte[])` / `Hash(byte[], long seed)`                   | static `byte[]` | allocating digest from an array; seed overload on XxHash types     |
|  [02]   | `Hash(ReadOnlySpan<byte>, long seed = 0)`                    | static `byte[]` | allocating digest from a span; zero-copy source, seedable          |
|  [03]   | `Hash(ReadOnlySpan<byte> source, Span<byte> dest, long seed = 0)` | static `int` | allocation-free digest into a caller buffer; returns bytes written |
|  [04]   | `TryHash(ReadOnlySpan<byte>, Span<byte>, out int, long seed = 0)` | static `bool` | non-throwing span sink; `false` when `dest` too short              |
|  [05]   | `HashToUInt32(ReadOnlySpan<byte>, int seed = 0)`             | static `uint`   | direct primitive (XxHash32/Crc32); no `byte[]` allocation          |
|  [06]   | `HashToUInt64(ReadOnlySpan<byte>, long seed = 0)`            | static `ulong`  | direct primitive (XxHash3/XxHash64/Crc64); the suite value-key form |
|  [07]   | `HashToUInt128(ReadOnlySpan<byte>, long seed = 0)`           | static `UInt128`| direct 128-bit primitive (XxHash128); the content-key form         |

[ENTRYPOINT_SCOPE]: incremental + streaming hash (the framed/chunked rail)
- rail: snapshot-identity
- The base class drives streaming and chunked accumulation; `AppendAsync` is the async mirror for IO-bound sources. `Clone` forks accumulated state (a shared prefix hashed once, then forked per branch). `GetCurrentHashAs*` reads the primitive without a `byte[]` round-trip.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<byte>)` / `Append(byte[])`    | instance call  | feeds a chunk into the running computation                   |
|  [02]   | `Append(Stream)`                                   | instance call  | drains a stream synchronously into the computation           |
|  [03]   | `AppendAsync(Stream, CancellationToken)`           | instance async | async mirror: drains a stream into the computation           |
|  [04]   | `GetCurrentHash()` / `GetCurrentHash(Span<byte>)`  | instance call  | reads the digest without resetting; span overload allocation-free |
|  [05]   | `TryGetCurrentHash(Span<byte>, out int)`           | instance call  | non-throwing current-hash read into a caller buffer          |
|  [06]   | `GetCurrentHashAsUInt32/64/128()`                  | instance call  | reads the primitive directly (per concrete type's width)     |
|  [07]   | `GetHashAndReset()` / `GetHashAndReset(Span<byte>)`| instance call  | finalizes and resets in one call; span overload allocation-free |
|  [08]   | `TryGetHashAndReset(Span<byte>, out int)`          | instance call  | non-throwing finalize-and-reset into a caller buffer         |
|  [09]   | `Reset()`                                          | instance call  | clears accumulated state for reuse                           |
|  [10]   | `Clone()`                                          | instance call  | forks the running state (shared-prefix-then-branch hashing)  |
|  [11]   | `HashLengthInBytes`                                | instance prop  | digest width (4/8/16) for sizing a destination span          |

## [04]-[IMPLEMENTATION_LAW]

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
- CONTIGUOUS vs FRAGMENTED grounding (the protobuf/RMS content-key seam — `api-protobuf` / `api-recyclable-stream`): the verified static one-shots are span-only — `Crc32.HashToUInt32(ReadOnlySpan<byte> source)` (no seed) and `XxHash128.HashToUInt128(ReadOnlySpan<byte> source, long seed = 0)`. A per-frame `Crc32` keys directly off a contiguous payload (`Crc32.HashToUInt32(frame.Payload.Span)` over the `ByteString`/segment span — the `Runtime/channels#ARTIFACT_FRAMES` `FrameEdge` realized form). A whole-artifact `XxHash128` over a MULTI-SEGMENT `RecyclableMemoryStream.GetReadOnlySequence()` view does NOT fit the one-shot span overload: drain it incrementally — `foreach (var seg in seq) hasher.Append(seg.Span);` then `hasher.GetCurrentHashAsUInt128()` — and reserve `XxHash128.HashToUInt128(seq.FirstSpan)` for the single-segment (`seq.IsSingleSegment`) fast case. The hash reads the pooled sequence in place; a `ToArray()` flatten before hashing is the rejected drift.

[LOCAL_ADMISSION]:
- Hashing mints non-cryptographic identity, cache, and correlation values only; redaction, security, and tamper evidence use separate declared rails — a non-cryptographic digest never carries a security claim.
- Hash algorithm, output width, input domain, and seed are receipt facts; snapshot identity cannot hide codec, compression, schema, or retention policy.
- `GetHashCode()` is `[Obsolete(error: true)]` on every type and throws `NotSupportedException` — a hash-algorithm instance is never a dictionary key; the digest primitive (`HashToUInt64`/`HashToUInt128`) is the key, never the algorithm object.
- The allocation-free `Hash(source, dest, seed)` / `HashToUInt*` forms are the default; an allocating `Hash(...) → byte[]` is admitted only at a boundary that already owns a `byte[]`.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity, content-address keys, frame-integrity checksums
- Accept: seeded `XxHash128`/`XxHash3` content keys, `Crc32`/`Crc64` frame integrity, incremental + streaming + forked accumulation, the `UInt128`/`ulong`/`uint` primitive sinks; the wire content-key seam — a `Crc32.HashToUInt32` per-frame integrity over a contiguous `ByteString`/segment span and a `XxHash128` whole-artifact identity over the pooled `RecyclableMemoryStream.GetReadOnlySequence()` view (`api-recyclable-stream`), keying the same bytes the `api-protobuf` `IBufferMessage` fast path stages (`Runtime/codecs#CONTENT_ADDRESSING` / `Runtime/channels#ARTIFACT_FRAMES`)
- Reject: security claims from non-cryptographic hashes; a second value-digest owner (`HashCode<T>.Combine` stays out at the staging catalog); a raw interpolated-string seed beside the canonical-scalar seed digest; a hash-algorithm instance used as a dictionary key (`GetHashCode` throws)
