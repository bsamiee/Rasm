# [RASM_FABRICATION_API_HASHING]

Full surface and stacking: `libs/csharp/.api/api-hashing.md` (shared-tier canonical owner).

## [01]-[FABRICATION_OVERLAY]

[SINGLE_MINT]:
- `ContentHash.Of` is the ONE mint site for every Fabrication content key; every egress artifact keys through it — `Posting/program` (`CutProgram`), `Nesting/nfp` (Placement/Remnant), `Additive/implicit` (`.cli`/grayscale), `Additive/production` (3MF), `Verify/removal` (`ResidualStock`/`StockSnapshot`), `Documentation/traveler`, `Tooling/magazine`, `Nesting/stock`. The content key is keyed by the `EgressKind` discriminant on `owner#atoms` (thirteen artifact families), federated to the Persistence `ArtifactKind` rows at the content-key boundary.
- the nesting `Remnant`/`Stock` content address and the `PairTable.Key`/`PairTable.InnerKey` pair identities route through `ContentHash.Of` over their canonical digests; `PairMemo` keys the `HybridCache` pair-polygon tier on the same `PairTable.Key` mint, so cache identity and pair identity are one spelling; `Stock` identity hashes the discriminant salt + ALL dimensions (never area-only), so an identical sheet keys to the same `UInt128`.
- The Persistence artifact index folds these keys into durable rows through one `ArtifactKind` enrollment per Fabrication egress family.

[RAIL_LAW]:
- Package: `System.IO.Hashing` (folder consumption via the kernel `ContentHash.Of`)
- Owns (folder scope): nothing net-new — the algorithm surface is the shared tier's; this folder consumes the ONE `ContentHash.Of` mint for all content identity
- Accept: `ContentHash.Of(canonicalBytes) → UInt128` at every egress; the `EgressKind`-keyed content address; the nesting `Remnant`/`Stock`/`PairTable.Key`/`PairTable.InnerKey` identities over their canonical digests
- Reject: a raw `XxHash128`/`XxHash3`/`GenerateHash` second hasher anywhere in the folder (the second-hasher defect); re-documenting the full package surface here (defer to the shared catalog); any security/tamper claim from a non-cryptographic digest
