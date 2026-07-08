# [RASM_BIM_API_HASHING]

Full surface and stacking: `libs/csharp/.api/api-hashing.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

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
