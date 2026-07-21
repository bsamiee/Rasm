# [CSHARP_BRANCH_IDEAS]

Cross-package C# concert ideas — concepts coupling two or more C# packages into one capability, distilled from the folder pools. A concept living inside one folder stays in that folder's pool; a concept spanning C#, Python, and TypeScript at once lives at the cross-libs tier and is referenced as a wire seam, never restated. Each idea is a card: a bracketed slug leader with the capability, what it unlocks, and the gap or technique it draws on.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[ANALYTICS_LAKE_CONCERT]-[QUEUED]: One columnar analytics plane across the strata — every producer hands Arrow record batches, Persistence lands them, the estate queries one lake.
- Capability: kernel encoded-geometry wires, Compute screening corpora, Element analytic tables, and Materials catalogues share one record-batch discipline — the producer owns batch shape and content-key metadata, Persistence owns writers, residence, slots, and the Flight SQL serving plane — so cross-package analytics is one query surface, never per-package files.
- Shape: producer→landing pairs land folder-side (kernel `[COLUMNAR_WIRE_SCHEMA]`, Compute `[DOE_LAKE_EGRESS]`, Element `[ANALYTIC_TABLE_PROJECTION]`, Materials `[CATALOGUE_ANALYTICS_EGRESS]`, Persistence `[PERS_L1]`); the `[BATCH_SEAM_LEDGER]` seam table owns the pair roster — a new producer is one ledger row and this card consumes every registered row; the branch tier owns the shared law — schema identity keyed by content hash, batch metadata preserved across landing, one serving plane.
- Unlocks: estate dashboards and `python:data` notebooks query every C# producer through one Flight SQL endpoint; a new producer is one schema handoff, zero new storage code.
- Anchors: the Persistence-tier `api-arrow.md` and `api-parquetsharp.md` catalogs, the kernel `ContentHash` law, the DuckDB residence lanes.

[BENCH_PROOF_CONCERT]-[BLOCKED]: One verdict grammar spans every branch bench claim — the task-named claim-family map closes over capture truth.
- Capability: claim families map onto `BenchmarkReceipt` through one verdict grammar at the AppHost benchmarks owner, every branch's bench claim reading the same verdict rows.
- Shape: claim-family map rows on `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md`.
- Unlocks: cross-branch bench verdicts compare on one grammar, no per-family verdict forks.
- Anchors: `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md` verdict grammar; `libs/csharp/Rasm.Grasshopper/.planning/Platform/capture.md` `CaptureBreach`.
- Tension: armed when `Platform/capture.md` `CaptureBreach` carries the bound that produced its breach, so the row maps without re-deriving capture policy.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SIGNAL_CONCERT]-[COMPLETE]: receipt-projected fabric landed — `TelemetryIdentity`/`SignalGovernance` spine (AppHost `Observability/telemetry`), `InstrumentFan.Mount` with the `[CONTRIBUTED_ARMS]` contributor roster (AppHost `Observability/instruments`), Persistence `SlotRegistry`/`StoreInstruments` contribution (`Store/observability`), OTLP egress at service roots; remaining emitters mount per the `[CONTRIBUTED_ARMS]` roster rows; the cross-libs counterpart stays `libs` `[UNIFIED_SIGNAL_FABRIC]`.
