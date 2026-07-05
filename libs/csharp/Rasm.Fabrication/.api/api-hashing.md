# [RASM_FABRICATION_API_HASHING]

`System.IO.Hashing` is a shared-substrate package; its full verified surface — the `NonCryptographicHashAlgorithm` accumulator base, the `XxHash3/64/32/128` and `Crc32/64` algorithms, and the three discriminated call shapes (static one-shot / incremental / stream sink) — lives in the shared catalog `libs/csharp/.api/api-hashing.md` and is NEVER re-documented here. This folder overlay records ONE fact: Fabrication reaches the package ONLY through the kernel `ContentHash.Of(ReadOnlySpan<byte>) → UInt128` seed-zero federation entry (the shared tier's `XxHash128.HashToUInt128` at seed zero, K9). `ContentHash.Of` is the SINGLE content-key mint for the whole folder; a raw `XxHash128`/`XxHash3`/`GenerateHash` call is the forbidden second-hasher defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- version: `10.0.9` (centrally pinned)
- license: `MIT` (.NET Foundation)
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- shared catalog: `libs/csharp/.api/api-hashing.md` (the full surface; this file is the folder overlay only)
- rail: fabrication content-identity (via `ContentHash.Of`)

## [02]-[FABRICATION_OVERLAY]

[SINGLE_MINT]:
- `ContentHash.Of` is the ONE mint site for every Fabrication content key; every egress artifact keys through it — `Posting/program` (`CutProgram`), `Nesting/nfp` (Placement/Remnant), `Additive/implicit` (`.cli`/grayscale), `Additive/production` (3MF), `Verify/removal` (`ResidualStock`/`StockSnapshot`), `Documentation/traveler`, `Tooling/magazine`, `Nesting/stock`. The content key is keyed by the `ArtifactKind` discriminant on `owner#atoms`.
- the nesting `Remnant`/`Stock` content address and the `NoFitPolygon.PairKey` precompute memo route through `ContentHash.Of` over their canonical digests; `Stock.Of` hashes the discriminant + ALL dimensions (never area-only), so an identical sheet keys to the same `UInt128`.
- the durable-row fold over these keys is the Fabrication-authored demand on the held-open `[ARTIFACT_CONTENT_KEY_FEDERATION]` blocker, never a composed Persistence contract.

[RAIL_LAW]:
- Package: `System.IO.Hashing` (folder consumption via the kernel `ContentHash.Of`)
- Owns (folder scope): nothing net-new — the algorithm surface is the shared tier's; this folder consumes the ONE `ContentHash.Of` mint for all content identity
- Accept: `ContentHash.Of(canonicalBytes) → UInt128` at every egress; the `ArtifactKind`-keyed content address; the nesting `Remnant`/`Stock`/`PairKey` identities over their canonical digests
- Reject: a raw `XxHash128`/`XxHash3`/`GenerateHash` second hasher anywhere in the folder (the second-hasher defect); re-documenting the full package surface here (defer to the shared catalog); any security/tamper claim from a non-cryptographic digest
