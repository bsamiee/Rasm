# [TS_REGION_MAP]

Ownership ledger for the `libs/typescript` four-domain FLAT planning branch — ONE branch ledger with four per-domain blocks plus the branch non-domain block, mirroring the C# suite ledger shape. Protocol and row shapes mirror the [suite standard](../../../csharp/.planning/README.md) ledger-protocol section, minus the signature-regions file: the design-page fences carry the signatures inline and the wire signature regions are owned by the C# ledger. Every owner name is unique within the TS branch and grepped clean against the suite owner-symbol registry; anchors are domain-folder-qualified (`@rasm/<domain>/page#CLUSTER`, the `@rasm/<domain>` prefix a ledger qualification convention for the domain folder inside the one `@rasm/ts` package, never a published-package boundary). The per-domain DENSITY_BAR lives in each `<domain>/.planning/README.md`, never duplicated here — the single-state-surface law.

| [INDEX] | [FILE]                               | [OWNS]                                                                       |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------- |
|   [1]   | [page-regions.md](page-regions.md)   | one row per domain/non-domain page, four per-domain blocks, the one-sentence concern law per page; page-finalization state routes to the charter PAGE_INDEX |
|   [2]   | [owner-symbols.md](owner-symbols.md) | the TS-branch owner registry, domain-qualified, unique within and against the suite |
|   [3]   | [seam-splits.md](seam-splits.md)     | the eleven wire seams + the cross-domain + node-tier + inter-domain folder-stratum + cross-branch DAG seams |
