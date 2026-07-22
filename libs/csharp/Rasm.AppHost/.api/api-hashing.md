# [RASM_APPHOST_API_HASHING]

Full surface and stacking: `libs/csharp/.api/api-hashing.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Content identity composes the kernel `Rasm.Domain.ContentHash.Of(ReadOnlySpan<byte>)` entry (seed-zero `XxHash128.HashToUInt128` → `UInt128`); `UInt128` is the identity currency, and hex or two-lane-`ulong` forms are boundary projections at the consuming seam.
- Allocation and partition hashing calls `XxHash3.HashToUInt64` raw — the one non-identity `System.IO.Hashing` path.

[STACKING]:
- `Runtime/features` sticky bucketing and `Runtime/determinism` stream-key derivation compose `XxHash3.HashToUInt64` directly over the raw path.
- `Runtime/determinism` durable read-back recomputes each chain digest through `ContentHash.Of`, so cross-package and cross-runtime chain verification folds one algorithm by construction (`ChainBroken` on breach).

[LOCAL_ADMISSION]:
- `Crc32`/`Crc64` correlate transport frames and support bundles only, never a content-identity or security claim.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic allocation and checksum hashing (`XxHash3`, `Crc32`/`Crc64`); the `XxHash128` algorithm the kernel content-identity entry wraps
- Accept: `XxHash3` allocation/partition hashing and CRC correlation as raw AppHost calls
- Reject: a per-call-site `XxHash128` content digest routed outside the kernel `ContentHash.Of` entry, or a security claim from a non-cryptographic hash
