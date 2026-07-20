# [CSHARP_BRANCH_IDEAS]

Cross-package C# concert ideas — concepts coupling two or more C# packages into one capability, distilled from the folder pools. A concept living inside one folder stays in that folder's pool; a concept spanning C#, Python, and TypeScript at once lives at the cross-libs tier and is referenced as a wire seam, never restated. Each idea is a card: a bracketed slug leader with the capability, what it unlocks, and the gap or technique it draws on.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[SIGNAL_CONCERT]-[ACTIVE]: One receipt-projected signal fabric across the C# strata.
- Capability: typed receipts and hook facts stay the single truth; instruments, logs, and spans project from them, never a parallel telemetry model.
- Shape: AppHost's Observability spine owns projection, Health, bundles, the hook rail, and benchmark receipts; every emitting package contributes one kind-arm partition at the fan mount — AppUi, Compute, and Persistence realized, every remaining emitter per the AppHost `[CONTRIBUTED_ARM_ROSTER]`, which owns the contributor census — Persistence's receipt-slot registry feeds the `store.<domain>.<verb>` series, and every root pushes OTLP.
- Unlocks: metric→trace→profile click-through through trace-based exemplars and the profile-id stamp, incident bundles replaying buffered logs, and one dashboard vocabulary the TypeScript iac stratum compiles.
- Anchors: branch observability catalogs under `libs/csharp/.api/`; the diagnostics doctrine; AppHost Observability pages; Persistence store observability; the AppHost contributed-arm roster card.
- Ripple: `libs` `[UNIFIED_SIGNAL_FABRIC]`.

[ANALYTICS_LAKE_CONCERT]-[QUEUED]: One columnar analytics plane across the strata — every producer hands Arrow record batches, Persistence lands them, the estate queries one lake.
- Capability: kernel encoded-geometry wires, Compute screening corpora, Element analytic tables, and Materials catalogues share one record-batch discipline — the producer owns batch shape and content-key metadata, Persistence owns writers, residence, slots, and the Flight SQL serving plane — so cross-package analytics is one query surface, never per-package files.
- Shape: producer→landing pairs land folder-side (kernel `[COLUMNAR_WIRE_SCHEMA]`, Compute `[DOE_LAKE_EGRESS]`, Element `[ANALYTIC_TABLE_PROJECTION]`, Materials `[CATALOGUE_ANALYTICS_EGRESS]`, Persistence `[PERS-L1]`); the `[BATCH_SEAM_LEDGER]` seam table owns the pair roster — a new producer is one ledger row and this card consumes every registered row; the branch tier owns the shared law — schema identity keyed by content hash, batch metadata preserved across landing, one serving plane.
- Unlocks: estate dashboards and `python:data` notebooks query every C# producer through one Flight SQL endpoint; a new producer is one schema handoff, zero new storage code.
- Anchors: the Persistence-tier `api-arrow.md` and `api-parquetsharp.md` catalogs, the kernel `ContentHash` law, the DuckDB residence lanes.

[BENCH_PROOF_CONCERT]-[QUEUED]: One benchmark-claim vocabulary across the branch — every folder claim family admits through the AppHost corpus gate, never a per-folder verdict grammar.
- Capability: every folder claim family — kernel `BenchClaim`, Bim `BimBenchReceipt`, Persistence corpus families, Fabrication bench cases, Materials workload rows, Grasshopper frame budgets, the Rhino in-host harvest — resolves to `BenchmarkReceipt` verdicts under `GatePolicy` with `HostEvidence` binding and Persistence claim custody; the `[CLAIM_FIELD_MAP]` table owns the family roster, a new family admits by one registered mapping row — one verdict grammar, folder families as data.
- Shape: the ingestion end lands AppHost-side (`[CORPUS_GATE_INGEST]`); the branch tier owns the vocabulary law — a folder claim family maps its fields onto `BenchmarkReceipt` and never mints a sibling verdict union.
- Unlocks: cross-folder performance comparison and regression dashboards read one receipt family; a new folder family is one mapping row.
- Anchors: the AppHost benchmarks gate fold, the Persistence `BENCHMARK_INDEX` custody, the folder claim-family cards.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
