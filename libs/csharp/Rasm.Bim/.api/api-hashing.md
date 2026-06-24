# [RASM_BIM_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hashing algorithms (XxHash family +
CRC) for snapshot identity, cache keys, receipt fingerprints, benchmark indexes, and
support-bundle correlation. The unifying primitive is the seedable, resettable,
`Clone`-able, span-and-stream-fed `NonCryptographicHashAlgorithm` base — every concrete
algorithm stacks the same incremental `Append` -> finalize contract, so a multi-part
snapshot (geometry buffer + property set + clock fact) folds into one rolling hash
without intermediate allocation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- version: `10.0.9`
- license: MIT (.NET runtime libraries)
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: net10.0, net9.0, net8.0, net462, netstandard2.0; the net10.0 consumer binds the `lib/net10.0` asset (in-box-equivalent; the package is the out-of-band redistribution of the BCL hashing surface)
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hashing surfaces
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :----------------- | :----------------------- |
|  [01]   | `NonCryptographicHashAlgorithm` | algorithm base     | incremental hash lifecycle: `Append`, `GetCurrentHash`/`GetHashAndReset`/`Reset`, `TryGetCurrentHash`, `HashLengthInBytes`, `AsStream()` write-only `Stream` adapter |
|  [02]   | `XxHash32`                      | hash algorithm     | 32-bit (`HashLengthInBytes`=4); seeded ctor `(uint seed)`; `Clone()`; `GetCurrentHashAsUInt32` |
|  [03]   | `XxHash64`                      | hash algorithm     | 64-bit (8); seeded ctor `(long seed)`; `Clone()`; `GetCurrentHashAsUInt64` |
|  [04]   | `XxHash3`                       | hash algorithm     | 64-bit (8), fastest; seeded ctor `(long seed)`; `Clone()`; `GetCurrentHashAsUInt64` |
|  [05]   | `XxHash128`                     | hash algorithm     | 128-bit (16); seeded ctor `(long seed)`; `Clone()`; `GetCurrentHashAsUInt128` |
|  [06]   | `Crc32`                         | checksum algorithm | 32-bit (4); `GetCurrentHashAsUInt32`; little-endian byte order (gzip/zip-compatible) |
|  [07]   | `Crc64`                         | checksum algorithm | 64-bit (8); `GetCurrentHashAsUInt64`; ECMA-182 polynomial |

Every concrete algorithm exposes static one-shot helpers (`Hash`, `HashToUInt32`/`UInt64`/`UInt128`, `Hash(ReadOnlySpan<byte> source, Span<byte> destination, [seed])`, `TryHash(... out int bytesWritten ...)`) AND the inherited incremental instance surface; the seeded `HashToUIntXX(ReadOnlySpan<byte>, long seed)` static is the allocation-free domain-separated key form.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hash operations
- rail: snapshot-identity

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                            | [CAPABILITY]                       |
| :-----: | :---------------------- | :----------------------------------------------------- | :--------------------------------- |
|  [01]   | `Hash`                  | `(ReadOnlySpan<byte>[, seed])` → `byte[]`              | one-shot hash bytes                |
|  [02]   | `Hash`                  | `(ReadOnlySpan<byte> source, Span<byte> dest[, seed])` → `int` | one-shot into caller buffer (no alloc) |
|  [03]   | `TryHash`               | `(ReadOnlySpan<byte> source, Span<byte> dest, out int bytesWritten[, seed])` → `bool` | bounded one-shot                   |
|  [04]   | `HashToUInt32`/`UInt64`/`UInt128` | `(ReadOnlySpan<byte>[, long seed])` → integer | one-shot typed value; seeded form is the domain-separated key |
|  [05]   | `Append`                | `(ReadOnlySpan<byte>)`                                 | appends a payload chunk to the rolling state |
|  [06]   | `GetCurrentHash`        | `()` → `byte[]` / `GetCurrentHashAsUIntXX()` → integer | reads current hash without resetting |
|  [07]   | `TryGetCurrentHash`     | `(Span<byte> dest, out int bytesWritten)` → `bool`    | reads current hash into a caller buffer |
|  [08]   | `GetHashAndReset`       | `()` → `byte[]` / `(Span<byte>) → int`                | finalizes and resets in one step   |
|  [09]   | `Reset`                 | `()`                                                  | resets rolling state for reuse     |
|  [10]   | `Clone`                 | `()` → same algorithm                                 | forks rolling state — finalize a prefix while continuing to append |
|  [11]   | `AsStream`              | `()` → `Stream`                                       | write-only `Stream` over `Append` — feeds a serializer/`CopyToAsync` directly into the hash |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm`
- fast root: XxHash algorithms (`XxHash3` is the fastest 64-bit; `XxHash128` for collision-resistant 128-bit identity)
- checksum root: CRC algorithms (`Crc32` gzip/zip-compatible, `Crc64` ECMA-182)
- receipt root: hash algorithm, input class, and output width

[ROLLING_COMPOSE]:
- multi-part identity: a snapshot fingerprint over an ordered set of parts (vertex buffer, property-set bytes, georeference frame, clock fact) is one `XxHash3` instance with sequential `Append(part)` calls, finalized once by `GetCurrentHashAsUInt64` — never per-part hashes XORed together (order-blind and collision-weak), never an intermediate concatenated buffer.
- buffer feed: a geometry/property part already in a `Span<byte>`/`Memory<byte>` (e.g. a SharpGLTF `MemoryAccessor` region or a glTF buffer) feeds `Append` directly with zero copy; a serializer (STJ writer over a receipt) feeds `AsStream()` so the wire bytes hash as they are produced.
- text part determinism: a NodaTime clock fact is appended as its invariant `InstantPattern.ExtendedIso` UTF-8 bytes, never a culture-ambient `ToString`, so the fingerprint is machine- and culture-stable.
- fork-finalize: `Clone()` snapshots the rolling state to emit a prefix fingerprint (e.g. geometry-only identity) while the same stream continues appending the semantic parts for the full-snapshot fingerprint — one pass, two receipts.
- seed as domain tag: distinct identity domains (cache key vs. support-bundle correlation vs. benchmark index) use distinct constant seeds via the `(long seed)` ctor or `HashToUIntXX(span, seed)` so the same bytes never collide across domains.

[LOCAL_ADMISSION]:
- Hashing creates non-cryptographic identity, cache, and correlation values only.
- Redaction, security, and tamper evidence use separate declared rails.
- Hash algorithm, output width, and input domain are receipt facts.
- Snapshot identity cannot hide codec, compression, schema, or retention policy.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity
- Accept: cache and receipt fingerprints
- Reject: security claims from non-cryptographic hashes
