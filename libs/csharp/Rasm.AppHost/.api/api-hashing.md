# [RASM_APPHOST_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hashing algorithms for snapshot
identity, cache keys, receipt fingerprints, benchmark indexes, and support
bundle correlation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: runtime library
- rail: snapshot-identity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hashing surfaces
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :----------------- | :----------------------- |
|  [01]   | `NonCryptographicHashAlgorithm` | algorithm base     | defines hash lifecycle   |
|  [02]   | `XxHash32`                      | hash algorithm     | computes 32-bit hash     |
|  [03]   | `XxHash64`                      | hash algorithm     | computes 64-bit hash     |
|  [04]   | `XxHash3`                       | hash algorithm     | computes 64-bit hash     |
|  [05]   | `XxHash128`                     | hash algorithm     | computes 128-bit hash    |
|  [06]   | `Crc32`                         | checksum algorithm | computes 32-bit checksum |
|  [07]   | `Crc64`                         | checksum algorithm | computes 64-bit checksum |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hash operations
- rail: snapshot-identity

| [INDEX] | [SURFACE]         | [CALL_SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------- | :------------ | :--------------------- |
|  [01]   | `Hash`            | static call   | computes hash bytes    |
|  [02]   | `HashToUInt32`    | static call   | computes 32-bit value  |
|  [03]   | `HashToUInt64`    | static call   | computes 64-bit value  |
|  [04]   | `HashToUInt128`   | static call   | computes 128-bit value |
|  [05]   | `Append`          | instance call | appends payload bytes  |
|  [06]   | `GetCurrentHash`  | instance call | reads current hash     |
|  [07]   | `GetHashAndReset` | instance call | finalizes and resets   |
|  [08]   | `Reset`           | instance call | resets hash state      |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm`
- fast root: XxHash algorithms
- checksum root: CRC algorithms
- receipt root: hash algorithm, input class, and output width

[LOCAL_ADMISSION]:
- CONTENT-IDENTITY CARVE-OUT (`[V18]`): a 128-bit CONTENT digest is NEVER a per-call-site `XxHash128` here — every content hash in the federation composes the kernel `Rasm.Domain.ContentHash.Of(ReadOnlySpan<byte>)` entry (seed-zero `XxHash128.HashToUInt128` → `UInt128`, one algorithm/one seed). A second hashing path forks identity and is the deleted form; AppHost's determinism chain, orchestration/reasoning/capability/isolation digests all route through that kernel entry.
- ALLOCATION-HASHING CARVE-OUT: `XxHash3` 64-bit hashing is NOT content identity and stays raw `System.IO.Hashing` — the `features.md` sticky bucketing and `determinism.md` stream-key derivation compose `XxHash3.HashToUInt64` directly, never the kernel entry (allocation ≠ identity).
- `Crc32`/`Crc64` are checksum/correlation only (transport frame integrity, support-bundle correlation), never a content-identity or security claim.
- Redaction, security, and tamper evidence use separate declared rails; hash algorithm, output width, and input domain are receipt facts that cannot hide codec, compression, schema, or retention policy.

[STACK]:
- kernel identity entry (`[V18]`): content digests compose `Rasm/Domain/identity#CONTENT_KEY` `ContentHash.Of` → `UInt128`; `UInt128` is the identity currency and hex/two-lane-`ulong` encodings are boundary projections at the consuming seam, never a re-mint. This catalog's `XxHash128` row is the ALGORITHM the kernel entry wraps, not a direct AppHost call site.
- allocation seam: `Runtime/features` (sticky bucketing) and `Runtime/determinism` (stream-key derivation) compose `XxHash3.HashToUInt64` for non-identity partition/seed hashing — the one raw-`System.IO.Hashing` AppHost path.
- chain re-verify: the `Runtime/determinism` `[V10]` durable read-back recomputes each chain digest through the same kernel `ContentHash.Of` entry, so cross-package/cross-runtime chain verification is one algorithm by construction (`ChainBroken` on breach).

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic allocation/checksum hashing (`XxHash3`, `Crc32`/`Crc64`); the `XxHash128` algorithm the kernel content-identity entry wraps
- Accept: `XxHash3` allocation/partition hashing and CRC correlation as raw AppHost calls
- Reject: a per-call-site `XxHash128` content digest (the `[V18]` deleted form — content identity composes the kernel `ContentHash.Of` entry), or a security claim from a non-cryptographic hash
