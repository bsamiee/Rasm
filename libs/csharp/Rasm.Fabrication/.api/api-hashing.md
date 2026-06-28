# [RASM_FABRICATION_API_HASHING]

`System.IO.Hashing` is the Fabrication nesting content-identity owner: the `Remnant`/`Stock` `XxHash128` content address and the `NoFitPolygon.PairKey` precompute-memo digest, plus `Crc32`/`Crc64` frame integrity for any wire handoff. Every nesting cache key, remnant-lineage seed, and NFP pair-dedup digest routes through this one owner — no cross-kernel `NAMING_HASH` owner consumes it, so the federation hash is wired wholly in-folder. The seeded `XxHash128`/`XxHash3` family is the cross-machine identity rail (the policy seed partitions identity by kerf, tolerance, and sheet-grade scalars); the `Crc` family is the unkeyed integrity rail. This is a non-cryptographic identity monopoly — a digest never carries a security claim, and `CommunityToolkit.HighPerformance` `HashCode<T>.Combine` is rejected as a second value-digest owner so the content address never fragments. The same package is copied at `Rasm.Compute/.api/api-hashing` (the snapshot-identity owner) under the no-shared-C#-tier law; the two catalogs document one verified surface against two folder rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- version: `10.0.9` (centrally pinned)
- license: `MIT` (.NET Foundation)
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: multi-target `net10.0` / `net9.0` / `net8.0` / `netstandard2.0` / `net462`; the `net10.0` consumer binds `lib/net10.0/System.IO.Hashing.dll`, XML docs shipped (pure-managed, vectorized intrinsics internal)
- rail: nesting-content-identity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hashing surfaces
- rail: nesting-content-identity
- The public roster is exactly these 7 types plus the nested `XxHash128.Hash128` value struct; `XxHashShared`/`VectorHelper`/per-type `State` are package-internal and never enter Fabrication vocabulary.

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                   |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------------------ |
|  [01]   | `NonCryptographicHashAlgorithm` | algorithm base     | incremental `Append`/`GetCurrentHash`/`Reset` lifecycle root  |
|  [02]   | `XxHash128`                     | hash algorithm     | 128-bit content-address digest (`HashToUInt128` → `UInt128`); seedable — the `Remnant`/`Stock` content key |
|  [03]   | `XxHash3`                       | hash algorithm     | fastest 64-bit digest (`HashToUInt64`); seedable — the `PairKey` value-digest form |
|  [04]   | `XxHash64`                      | hash algorithm     | legacy 64-bit digest (`HashToUInt64`)                         |
|  [05]   | `XxHash32`                      | hash algorithm     | 32-bit digest (`HashToUInt32`); `int` seed                    |
|  [06]   | `Crc32`                         | checksum algorithm | 32-bit frame-integrity checksum (`HashToUInt32`)              |
|  [07]   | `Crc64`                         | checksum algorithm | 64-bit frame-integrity checksum (`HashToUInt64`)              |
|  [08]   | `XxHash128.Hash128`             | value struct       | nested `(ulong Low64, ulong High64)` 128-bit carrier; consumers prefer `UInt128` via `HashToUInt128` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot static hash (the content-address rail)
- rail: nesting-content-identity
- Each concrete type carries the same static one-shot family; the `seed` parameter is the cross-machine partition knob the nesting policy folds (a re-nest at a different kerf/tolerance keys distinctly because the seed mixes the canonical policy scalars). The `ReadOnlySpan<byte>` overloads are the zero-copy path off a canonical-scalar buffer — never a `ToArray` flatten before hashing.

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]    | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `Hash(byte[])` / `Hash(byte[], long seed)`                   | static `byte[]` | allocating digest from an array; seed overload on XxHash types     |
|  [02]   | `Hash(ReadOnlySpan<byte>, long seed = 0)`                    | static `byte[]` | allocating digest from a span; zero-copy source, seedable          |
|  [03]   | `Hash(ReadOnlySpan<byte> source, Span<byte> dest, long seed = 0)` | static `int` | allocation-free digest into a caller buffer; returns bytes written |
|  [04]   | `TryHash(ReadOnlySpan<byte>, Span<byte>, out int, long seed = 0)` | static `bool` | non-throwing span sink; `false` when `dest` too short              |
|  [05]   | `HashToUInt32(ReadOnlySpan<byte>, int seed = 0)`             | static `uint`   | direct primitive (XxHash32/Crc32); no `byte[]` allocation          |
|  [06]   | `HashToUInt64(ReadOnlySpan<byte>, long seed = 0)`            | static `ulong`  | direct primitive (XxHash3/XxHash64/Crc64); the `PairKey` value-digest form |
|  [07]   | `HashToUInt128(ReadOnlySpan<byte>, long seed = 0)`           | static `UInt128`| direct 128-bit primitive (XxHash128); the `Remnant`/`Stock` content-key form |

[ENTRYPOINT_SCOPE]: incremental + streaming hash (the framed/chunked rail)
- rail: nesting-content-identity
- The base class drives streaming and chunked accumulation; `AppendAsync` is the async mirror for IO-bound sources. `Clone` forks accumulated state (a shared prefix hashed once, then forked per branch). `GetCurrentHashAs*` reads the primitive without a `byte[]` round-trip.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<byte>)` / `Append(byte[])`    | instance call  | feeds a chunk into the running computation                   |
|  [02]   | `Append(Stream)` / `AppendAsync(Stream, CancellationToken)` | instance | drains a stream synchronously / async into the computation   |
|  [03]   | `GetCurrentHash()` / `GetCurrentHash(Span<byte>)`  | instance call  | reads the digest without resetting; span overload allocation-free |
|  [04]   | `TryGetCurrentHash(Span<byte>, out int)`           | instance call  | non-throwing current-hash read into a caller buffer          |
|  [05]   | `GetCurrentHashAsUInt32/64/128()`                  | instance call  | reads the primitive directly (per concrete type's width)     |
|  [06]   | `GetHashAndReset()` / `GetHashAndReset(Span<byte>)`| instance call  | finalizes and resets in one call; span overload allocation-free |
|  [07]   | `TryGetHashAndReset(Span<byte>, out int)`          | instance call  | non-throwing finalize-and-reset into a caller buffer         |
|  [08]   | `Reset()` / `Clone()`                              | instance call  | clears state for reuse / forks running state (shared-prefix-then-branch) |
|  [09]   | `HashLengthInBytes`                                | instance prop  | digest width (4/8/16) for sizing a destination span          |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm` (abstract; `HashLengthInBytes`, `Append`/`Reset`/`GetCurrentHashCore` lifecycle)
- content-address root: `XxHash128.HashToUInt128` → `UInt128` (the `Remnant`/`Stock` content key)
- value-digest root: `XxHash3.HashToUInt64` (the fastest seedable value key — the `NoFitPolygon.PairKey`)
- frame-integrity root: `Crc32`/`Crc64` `HashToUInt32`/`HashToUInt64`
- receipt facts: hash algorithm, output width (`HashLengthInBytes`), input domain, and the policy seed are nesting-receipt evidence

[NESTING_IDENTITY]:
- `Remnant`/`Stock` content address: `XxHash128.HashToUInt128` over the canonical sheet-grade + outline + remnant-lineage bytes mints the stable cutting-stock identity; an identical sheet re-nested keys to the same `UInt128`, so the multi-sheet `Seq<Stock>` inventory and the kerf-inflated Boolean-difference remnant lineage share one content-address space.
- `NoFitPolygon.PairKey` memo: a part-pair NFP is keyed by `XxHash3.HashToUInt64` over the two canonical part outlines (orientation-normalized) so the `Clipper2` Minkowski geometry for identical pairs is computed ONCE across the placement scan — the pair-key memo (geometry dedup across pairs) composes with the `Clipper2` `ReuseableDataContainer64` (scanbeam dedup across positions of one pair), the two operating on disjoint reuse axes.
- the XxHash `seed` is the cross-machine partition knob: fold the canonical nesting-policy scalars (kerf, tolerance, sheet grade) into a little-endian seed digest via `XxHash3.HashToUInt64`, never a raw interpolated-string seed (`$"{kerf:R}|…"` keys distinctly across cultures and float renderings — the rejected drift defect). `Crc` carries no seed; frame integrity is unkeyed.

[STREAMING_AND_FORKING]:
- a multi-feature part accumulates through `Append(ReadOnlySpan<byte>)` over each canonical-segment span; the per-feature identity is a one-shot `XxHash128.HashToUInt128(slice)` and the whole-part identity is the running accumulation — never two hashing passes over the same bytes.
- `Clone()` forks a shared-prefix accumulation so a base stock outline hashed once branches per remnant delta without re-feeding the prefix.
- the verified static one-shots are span-only; a digest over a multi-segment pooled buffer drains incrementally (`foreach (var seg in seq) hasher.Append(seg.Span);` then `GetCurrentHashAsUInt128()`) and reserves `HashToUInt128(seq.FirstSpan)` for the single-segment fast case — a `ToArray()` flatten before hashing is the rejected drift.

[LOCAL_ADMISSION]:
- hashing mints non-cryptographic identity, cache, and memo keys only; a non-cryptographic digest never carries a security/tamper claim.
- `GetHashCode()` is `[Obsolete(error: true)]` on every type and throws `NotSupportedException` — a hash-algorithm instance is never a dictionary key; the digest primitive (`HashToUInt64`/`HashToUInt128`) is the key, never the algorithm object.
- the allocation-free `Hash(source, dest, seed)` / `HashToUInt*` forms are the default; an allocating `Hash(...) → byte[]` is admitted only at a boundary that already owns a `byte[]`.
- the federation hash is wholly in-folder: no cross-kernel `NAMING_HASH` owner consumes this rail. The `Rasm.Compute/.api/api-hashing` copy is the SAME package against the snapshot-identity rail (C# has no shared tier) — keep the two catalogs' verified surfaces identical and the rail framing folder-local.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic nesting content-identity, NFP pair-memo keys, remnant/stock content addresses, and frame-integrity checksums
- Accept: seeded `XxHash128` `Remnant`/`Stock` content keys, seeded `XxHash3` `PairKey` value digests, `Crc32`/`Crc64` frame integrity, incremental + streaming + forked accumulation, the `UInt128`/`ulong`/`uint` primitive sinks; the `Clipper2` Minkowski/NFP geometry seam (`api-clipper2`) the `PairKey` memoizes
- Reject: security claims from non-cryptographic hashes; a second value-digest owner (`HashCode<T>.Combine` stays out); a raw interpolated-string seed beside the canonical-scalar seed digest; a hash-algorithm instance used as a dictionary key (`GetHashCode` throws); a `ToArray` flatten before hashing a pooled multi-segment buffer
