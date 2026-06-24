# [RASM_APPUI_API_HASHING]

`System.IO.Hashing` is the BCL non-cryptographic hashing family — the `XxHash3`/`XxHash64`/`XxHash128`/`XxHash32` xxHash algorithms plus the `Crc32`/`Crc64` checksums — supplying snapshot identity, content-addressed cache keys, receipt fingerprints, benchmark indexes, and support-bundle correlation. Every algorithm derives from `NonCryptographicHashAlgorithm`, which is itself a writable `System.IO.Stream`, so a payload is fingerprinted three ways off one instance: one-shot through the static `Hash`/`HashToUInt*` allocation-free entrypoints, incrementally through `Append(...)`+`GetCurrentHashAsUInt64()` over a span/stream, or sink-style by `CopyToAsync`-ing a producer stream straight into the algorithm. The 128-bit `XxHash128.HashToUInt128(ReadOnlySpan<byte>)` is the single content-address mint the AppUi geometry-residency wire, the Persistence content-addressed blob key, and the reality-capture splat tile all share — the raw `UInt128` rendered to the `:x32` 32-hex wire string at the marshal seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: managed runtime library (no native dependency)
- tfm: `net10.0` (consumer-bound; the package multi-targets, the workspace binds the `net10.0` asset)
- license: `MIT`
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[HASH_TYPES]: hashing surfaces — every algorithm is a `NonCryptographicHashAlgorithm`, i.e. a writable `Stream`
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]                                                       |
| :-----: | :------------------------------ | :------------------ | :----------------------------------------------------------------- |
|  [01]   | `NonCryptographicHashAlgorithm` | abstract base       | `Stream`-derived hash lifecycle; `Append`/`GetCurrentHash`/`Reset` |
|  [02]   | `XxHash3`                       | hash algorithm      | 64-bit xxHash3 — the fastest small-to-mid input fingerprint        |
|  [03]   | `XxHash64`                      | hash algorithm      | 64-bit xxHash64 — wide-input streaming fingerprint                 |
|  [04]   | `XxHash128`                     | hash algorithm      | 128-bit xxHash128 — content-address identity (`UInt128`)           |
|  [05]   | `XxHash32`                      | hash algorithm      | 32-bit xxHash32 — compact cache/index key                          |
|  [06]   | `Crc32`                         | checksum algorithm  | 32-bit CRC — frame/wire integrity check                            |
|  [07]   | `Crc64`                         | checksum algorithm  | 64-bit CRC — large-payload integrity check                         |

## [03]-[ENTRYPOINTS]

[STATIC_ENTRYPOINTS]: one-shot allocation-free hashing — static per concrete algorithm, returning bytes or the native-width integer
- rail: snapshot-identity

| [INDEX] | [SURFACE]                                                         | [SURFACE_ROOT]                              | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------ | :--------------------------------------------- |
|  [01]   | `Hash(ReadOnlySpan<byte>, long seed = 0)` / `Hash(byte[])`        | `XxHash3`/`XxHash64`/`XxHash128`            | seeded one-shot hash bytes (xxHash takes seed) |
|  [02]   | `Hash(ReadOnlySpan<byte>)` / `Hash(byte[])`                       | `Crc32`/`Crc64`/`XxHash32`                  | one-shot checksum/hash bytes (no seed)         |
|  [03]   | `Hash(ReadOnlySpan<byte> source, Span<byte> dest, long seed = 0)` | concrete algorithm                          | one-shot into caller buffer, returns int count |
|  [04]   | `TryHash(ReadOnlySpan<byte>, Span<byte>, out int, long seed = 0)` | concrete algorithm                          | non-throwing one-shot into buffer              |
|  [05]   | `HashToUInt32(ReadOnlySpan<byte>)`                                | `XxHash32` / `Crc32`                        | one-shot 32-bit value, zero allocation         |
|  [06]   | `HashToUInt64(ReadOnlySpan<byte>, long seed = 0)`                 | `XxHash3` / `XxHash64` / `Crc64`            | one-shot 64-bit value, zero allocation         |
|  [07]   | `HashToUInt128(ReadOnlySpan<byte>, long seed = 0)`                | `XxHash128`                                 | one-shot 128-bit content-address `UInt128`     |

[STREAMING_ENTRYPOINTS]: incremental + async accumulation off one instance — `NonCryptographicHashAlgorithm` is a `Stream`
- rail: snapshot-identity

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]                  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------- | :------------------------------ | :----------------------------------------------------- |
|  [01]   | `Append(ReadOnlySpan<byte>)`                         | `NonCryptographicHashAlgorithm` | feed a span chunk into the running hash                 |
|  [02]   | `Append(byte[])` / `Append(Stream)`                  | `NonCryptographicHashAlgorithm` | feed a buffer / drain a stream synchronously            |
|  [03]   | `AppendAsync(Stream, CancellationToken = default)`   | `NonCryptographicHashAlgorithm` | drain a stream into the hash asynchronously             |
|  [04]   | `Write(ReadOnlySpan<byte>)` / `WriteAsync(ROM<byte>)`| `Stream` override               | use the algorithm directly as a writable `Stream` sink  |
|  [05]   | `GetCurrentHashAsUInt32/64/128()`                    | concrete algorithm              | read running hash as native-width int, no reset, no alloc |
|  [06]   | `GetCurrentHash()` / `GetCurrentHash(Span<byte>)`    | `NonCryptographicHashAlgorithm` | snapshot running hash bytes (alloc / into buffer)       |
|  [07]   | `TryGetCurrentHash(Span<byte>, out int)`             | `NonCryptographicHashAlgorithm` | non-throwing running-hash snapshot into buffer          |
|  [08]   | `GetHashAndReset()` / `GetHashAndReset(Span<byte>)`  | `NonCryptographicHashAlgorithm` | finalize + reset for the next message                   |
|  [09]   | `TryGetHashAndReset(Span<byte>, out int)`            | `NonCryptographicHashAlgorithm` | non-throwing finalize + reset into buffer               |
|  [10]   | `Reset()`                                            | `NonCryptographicHashAlgorithm` | clear running state without reading                     |
|  [11]   | `Clone()`                                            | concrete algorithm              | fork the running hash state for a branch fingerprint    |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- base root: `NonCryptographicHashAlgorithm` (a `Stream` subclass) — the lifecycle every algorithm shares.
- fast root: `XxHash3` for the small-to-mid hot path, `XxHash64` for wide streaming, `XxHash32` for compact keys.
- content-address root: `XxHash128.HashToUInt128(ReadOnlySpan<byte>)` — the 128-bit `UInt128` the cross-language residency wire keys on.
- checksum root: `Crc32`/`Crc64` for frame and wire integrity where a polynomial checksum (not a hash) is the contract.
- receipt root: the receipt records hash algorithm, input class, and output width as facts; the value itself is the fingerprint.

[STACKING_LAW]:
- Content addressing collapses to one mint: `XxHash128.HashToUInt128(positionsSpan)` over the geometry byte span yields the `UInt128` `ContentKey`, rendered to the `:x32` 32-hex string at the wire/marshal seam (`KeyHex`). The Render geometry-residency wire, the Persistence content-addressed blob key, and the reality-capture splat tile all key off this one law — no second content-identity value object exists, and a name-addressed string key is the rejected form because consumers fetch bytes by content-address.
- Streaming receipts stack with the snapshot rail: `Append(span)` the codec output, then `GetCurrentHashAsUInt64()` reads the running fingerprint with zero allocation and zero finalize, so a multi-chunk snapshot accumulates one cache key while the codec writes — the typed-integer accessor feeds the receipt's `ulong`/`UInt128` field directly rather than re-hashing the assembled buffer.
- Stream-sink composition: because the algorithm is a `Stream`, a producer pipeline (`Stream.CopyToAsync(hash, ct)`, or a serializer writing into it) fingerprints in flight — the hash is the tee, not a second pass over a materialized buffer.
- `Clone()` forks the running state, so a shared prefix (header/preamble) hashes once and each branch finalizes its own suffix off the clone instead of re-feeding the prefix per branch.

[LOCAL_ADMISSION]:
- Hashing creates non-cryptographic identity, cache, and correlation values only.
- Redaction, security, and tamper evidence use separate declared rails — a non-cryptographic digest is never a security claim.
- Hash algorithm, output width, and input domain are receipt facts.
- Snapshot identity cannot hide codec, compression, schema, or retention policy.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity, content-address keys, and frame/wire checksums.
- Accept: cache and receipt fingerprints; the `XxHash128.HashToUInt128` content-address `:x32` shared with the cross-language wire; streaming `Append`+`GetCurrentHashAsUInt*` accumulation; CRC frame integrity.
- Reject: security or tamper claims from non-cryptographic hashes; a second content-identity value object beside the suite `ContentKey`; a name-addressed string key where a content-address is required; re-hashing an assembled buffer where streaming accumulation or `Clone()` already holds the running state.
