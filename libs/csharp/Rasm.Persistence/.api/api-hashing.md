# [RASM_PERSISTENCE_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hashing algorithms for snapshot
identity, content keys, cache keys, receipt fingerprints, benchmark indexes, and
support-bundle correlation. Both the one-shot static fast path and the incremental
`Append`/`GetCurrentHash` accumulator path are first-class, and each algorithm exposes
its width-typed scalar projection (`HashToUInt32/64/128`, `GetCurrentHashAsUInt32/64/128`)
so a fingerprint folds straight into a `uint`/`ulong`/`UInt128` receipt field without an
intermediate `byte[]`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- version: `10.0.9`
- license: MIT
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- bound asset: `lib/net10.0` (consumer-bound; package multi-targets net462/netstandard2.0/net8.0/net9.0/net10.0)
- asset: runtime library
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[HASH_TYPES]: hashing surfaces
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------ |
|  [01]   | `NonCryptographicHashAlgorithm` | algorithm base     | abstract accumulator lifecycle (Append/finalize)  |
|  [02]   | `XxHash32`                      | hash algorithm     | 32-bit xxHash; `HashToUInt32`                      |
|  [03]   | `XxHash64`                      | hash algorithm     | 64-bit xxHash; `HashToUInt64`                      |
|  [04]   | `XxHash3`                       | hash algorithm     | 64-bit XXH3 (seedable, fastest); `HashToUInt64`   |
|  [05]   | `XxHash128`                     | hash algorithm     | 128-bit XXH3; `HashToUInt128` content-identity     |
|  [06]   | `Crc32`                         | checksum algorithm | 32-bit CRC; `HashToUInt32`/`GetCurrentHashAsUInt32` |
|  [07]   | `Crc64`                         | checksum algorithm | 64-bit CRC; `HashToUInt64`/`GetCurrentHashAsUInt64` |

`NonCryptographicHashAlgorithm` is NOT a `Stream`; it is the shared accumulator base. Per-instance state: `int HashLengthInBytes`. Incremental ingest: `Append(ReadOnlySpan<byte>)`, `Append(byte[])`, `Append(Stream)`, `Task AppendAsync(Stream, CancellationToken)`. Finalize-non-destructive: `byte[] GetCurrentHash()`, `int GetCurrentHash(Span<byte>)`, `bool TryGetCurrentHash(Span<byte>, out int)`. Finalize-and-reset: `byte[] GetHashAndReset()`, `int GetHashAndReset(Span<byte>)`, `bool TryGetHashAndReset(Span<byte>, out int)`. Restart: `Reset()`.

`XxHash3`/`XxHash128` seedable: `new XxHash3(long seed)` / `new XxHash128(long seed)` salt the digest for keyed identity domains. Every concrete algorithm carries `Clone()` returning a same-type snapshot of accumulator state, so a partially-fed hasher forks into N continuations (common-prefix fan-out) without re-ingesting the prefix.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot static fast path
- rail: snapshot-identity

Static members live per concrete algorithm (no static dispatch on the base). `XxHash128.HashToUInt128` is the content-identity primitive; the `byte[] Hash(...)` allocating forms exist but the span `TryHash`/scalar projections are the zero-alloc receipt path.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                              | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :---------------------------------------- | :---------------------------------------------- |
|  [01]   | `HashToUInt128`                        | `ReadOnlySpan<byte> → UInt128`            | `XxHash128` content key (the `ContentKey` rail) |
|  [02]   | `HashToUInt64`                         | `ReadOnlySpan<byte> → ulong`              | 64-bit cache/index key (`XxHash3`/`64`/`Crc64`) |
|  [03]   | `HashToUInt32`                         | `ReadOnlySpan<byte> → uint`               | 32-bit checksum (`XxHash32`/`Crc32`)            |
|  [04]   | `TryHash`                              | `(src, Span<byte> dest, out int)`         | zero-alloc digest into a caller buffer          |
|  [05]   | `Hash`                                 | `(src, Span<byte> dest) → int`            | digest into a buffer, returns bytes written     |
|  [06]   | `Hash`                                 | `byte[]`/`ReadOnlySpan<byte> → byte[]`    | allocating one-shot                             |
|  [07]   | `XxHash3.Hash` / `XxHash128.Hash`      | `(ReadOnlySpan<byte>, long seed)`         | seeded one-shot                                 |

[ENTRYPOINT_SCOPE]: incremental accumulator path
- rail: snapshot-identity

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                          | [CAPABILITY]                                      |
| :-----: | :----------------------- | :------------------------------------ | :------------------------------------------------ |
|  [01]   | `Append`                 | `ReadOnlySpan<byte>` / `byte[]`        | fold a chunk into accumulator state               |
|  [02]   | `Append` / `AppendAsync` | `Stream` / `(Stream, CancellationToken)` | digest a streamed snapshot body of unknown length |
|  [03]   | `GetCurrentHash`         | `() → byte[]` / `(Span<byte>) → int`   | non-destructive read for a running fingerprint    |
|  [04]   | `TryGetCurrentHash`      | `(Span<byte>, out int)`                | non-destructive zero-alloc read                   |
|  [05]   | `GetHashAndReset`        | `() → byte[]` / `(Span<byte>) → int`   | finalize one frame, reuse the hasher for the next |
|  [06]   | `TryGetHashAndReset`     | `(Span<byte>, out int)`                | finalize-and-reset, zero-alloc                    |
|  [07]   | `GetCurrentHashAsUInt32` | `Crc32 → uint`                         | width-typed running checksum projection           |
|  [08]   | `GetCurrentHashAsUInt64` | `Crc64`/`XxHash3` → `ulong`            | width-typed running digest projection             |
|  [09]   | `GetCurrentHashAsUInt128`| `XxHash128 → UInt128`                  | width-typed running content-identity projection   |
|  [10]   | `Clone`                  | `() → same algorithm`                  | fork accumulator state at a common prefix         |
|  [11]   | `Reset`                  | `()`                                   | clear accumulator state                           |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm` (accumulator lifecycle, not a `Stream`)
- content root: `XxHash128.HashToUInt128(ReadOnlySpan<byte>) → UInt128` is the canonical content-key derivation at `Schema/identity#IDENTITY_LADDER` (`ContentKey`); the `ContentHash` `IdentityPolicy` row carries `clrType: typeof(UInt128)`, so the hasher's scalar projection IS the persisted key — no `byte[]` round trip.
- streamed root: a snapshot body larger than one contiguous span feeds `XxHash128.Append(Stream)` / `AppendAsync(Stream)` then `GetCurrentHashAsUInt128()`, yielding the same `UInt128` identity as the one-shot path over the concatenated bytes.
- checksum root: `Crc32`/`Crc64` cover transport-frame and support-bundle integrity where a 32/64-bit CRC is the wire convention rather than a content key.
- receipt root: algorithm name, output width, seed presence, and input domain are receipt facts (`Store/quality` snapshot/benchmark receipts); the scalar projection lands directly in the typed receipt field.

[INTEGRATION_LAW]:
- The `UInt128` content key stacks under the snapshot codec: `MessagePack` (`api-messagepack`) serializes the snapshot body to bytes, `XxHash128.HashToUInt128` keys those bytes, and the resulting `UInt128` is the idempotent `ContentHash` identity (`Collision.ContentIdempotent`) — a re-serialized identical snapshot collides to the same key and dedupes.
- For a content-defined-chunked body (`api-fastcdc`) each chunk's `HashToUInt128` is the chunk key; `Clone()` lets a Merkle-style roll-up fork a running digest at a chunk boundary without re-reading the prefix.
- `seed` partitions identity domains: a seeded `XxHash128`/`XxHash3` keeps two logical content namespaces from colliding under a shared store without a separate salt column.
- A scalar key (`UInt128`/`ulong`) is the cache/index key feeding `HybridCache` (`api-hybrid-cache`) and the `Query/cache` key derivation; `XxHash3.HashToUInt64` is the fast 64-bit cache-key fast path.

[LOCAL_ADMISSION]:
- Hashing creates non-cryptographic identity, cache, and correlation values only.
- Redaction, security, and tamper evidence use separate declared rails; a non-cryptographic digest never carries an authenticity or anti-tamper claim.
- Algorithm, output width, seed presence, and input domain are receipt facts.
- Snapshot identity cannot hide codec, compression, schema, or retention policy — it keys the body, it does not describe it.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity, content keys, and integrity checksums
- Accept: content/cache/index keys and receipt fingerprints, one-shot or streamed
- Reject: security, authenticity, or tamper-evidence claims from a non-cryptographic hash
