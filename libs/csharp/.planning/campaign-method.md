# [CAMPAIGN_METHOD]

The doctrinal approach behind the app-package planning corpora and every library campaign that follows (including `libs/typescript`). Sessions load this file first: it states why these libraries are built this way, the method that produces them, the bar they are held to, and the staged work that remains before and during implementation. Folder-specific entry points live in each package charter (`Rasm.<Pkg>/.planning/README.md`); open work lives in `TASKLOG.md`.

## [1]-[THE_BAR]

These are lib-grade foundations, not app scaffolding. The bar for every package, current and future:

- World-class library code a company would adopt if it were public: the library internalizes the heavy logic, boilerplate, host quirks, native lifetimes, and failure handling so downstream apps compose capability instead of re-deriving it.
- No wrappers, no wrapper-faces, no rename adapters, no grab-bag abstractions. Every surface adds real functionality: high-value, higher-order capability built FROM the admitted packages, never a thinner face OVER them.
- Effortless complex apps without sacrifice: single entry points and unified rails reduce downstream agent overhead, and they do it by absorbing complexity internally — never by limiting tooling, hiding capability, or simplifying away power.
- Dense, optimized, polymorphic code at 25-35% of naive LOC: one owner per axis, one entrypoint family per rail, one fault family per package, total Switch dispatch, fold algebras, table-driven logic. Flat code, parallel rails, and near-duplicate shapes are defects.
- Full doctrine adherence: `docs/stacks/csharp/` is law, composed as given, never restated.

## [2]-[UNIVERSAL_PRINCIPLES]

- One unified body of functionality, not loose feature arms. A new capability is a row, case, or column on an existing axis — never a sibling surface. The pressure to add a second surface is the signal to deepen the first.
- Modality completeness from the root: every axis that varies by where code runs carries the full row set — host modalities on the AppHost host-profiles axis, process topologies in `FEATURES.md`. Apps load and offload compute, data, and UI across those rows without new surfaces.
- Parameterization as a universal need: anything that can vary is a row value, never a hardcode. A new data source, asset source, theme probe, transport, store engine, or schedule is one row on the owning axis. Host-agnostic sourcing is the default (a UI host is rhino, empty, custom, or any admitted package; the same law applies to every axis).
- External-lib maximization, truthfully: every admitted package is mined to its full capability and integrated through the owning axis; first-party companions and extensions — database extension surfaces included — are first-class rows from the root. An admitted capability no page exploits is a named gap.
- Plug-in-play composition, glove-in-hand integration: packages are complete in isolation and integrate only through the AppHost ports and the ledger seam laws (`region-map/seam-splits.md`; one-owner summary in `FEATURES.md`). Coupling beyond the ports is a defect; so is a seam re-taught instead of consumed.
- Unified telemetry/logging/observability from the source: every package contributes through the contributor and receipt-sink ports; signals are plug-in-play at composition, never bolted on per app.

## [3]-[THE_METHOD]

The repeatable campaign shape that produced this corpus, binding for every future library campaign:

1. INFRA TRUTH FIRST — root config, manifests, lockfiles, tool pins, and classification are made honest before anything else (the P0a pattern); admissions execute with restore proof; every admitted package gets a decompile-grounded catalogue page; every existing catalogue is re-verified against decompiled surfaces (phantom APIs are the named defect class).
2. RESEARCH RESERVOIR — design lanes, adversarial verifiers, gap-hunt lenses, deep-read lanes, decompile probes, and web currency checks run BEFORE authoring; conflicts resolve by the suite standard's binding precedence.
3. IDEATION BEFORE DESIGN — exhaustive concept enumeration (isolation + in-concert, across every topology and domain) drives the pages; imagine everything, then prune bloat ruthlessly; every survivor rides an axis row, and the concert concepts land in `FEATURES.md`. The per-package refinement blueprint converts ideation into per-page cluster plans, binding corrections, density guidance, and anti-bloat vetoes.
4. AUTHOR-THEN-REPAIR — parallel page authors transcribe the blueprint under the suite standard (`README.md` here); consolidated review-and-repair waves find AND fix in one pass; the orchestrator is the only ledger writer.
5. CROSS-FOLDER COHESION — ledger consolidation (owner symbols, seam splits, adjudications), then a final deep pass per folder: under-utilization hunt, heavy code critique on every fence, doc integrity, research items answerable by decompile get answered, zero-guessing validation.
6. EVERYTHING DURABLE — working material persists under `.artifacts/planning-briefs/` (gitignored); law lands in charters, pages, the ledger, atlases, and ROADMAPs; versions live only in charter ADMISSIONS_RECORD tables.

## [4]-[STAGED_WORK_BEFORE_IMPLEMENTATION]

The sessions that precede implementation, in order; each is its own campaign run with the method above:

1. PER-FOLDER EXTREME-DETAIL DEEPENING — each package in isolation: re-read every catalogue against every page, push capabilities/features/considerations further (the AppUi mined-no-gaps treatment as the bar for all), close TASKLOG rows owned by the folder, deepen thin catalogue pages (named in TASKLOG), and re-run the code critique until any world-class app is buildable from the pages alone.
2. COMPREHENSIVE APP IDEATION — a dedicated ideation campaign across all dimensionalities (local, web-linked, data-sourcing, domain-specific, multi-process) producing new concept rows integrated root-up into pages and atlases; advanced complexity is the target, always with the dual mandate: universal standalone usage AND first-class rhino/gh2 operation.
3. SIBLING LIBRARY CAMPAIGNS — `libs/typescript` (entry: `libs/typescript/.planning/README.md`) and any further libs, each made lib-grade in isolation with its own infra truth, catalogue extraction, ideation, pages, charter, and ledger — integrating with these packages only through the wire contracts, exactly as these packages integrate with each other only through ports.
4. FINAL ARCHITECTURE PASS — the closing pre-implementation session: per-package file plan finalized against the charter BUILD_ORDER, logic-flow walkthrough of every entrypoint rail end-to-end, cross-package boot/drain/wire choreography rehearsed on paper, implementation-start gates executed (bridge spikes, native probes), and the proof-gate matrix confirmed green-able. Exit: implementation begins with transcription only.

## [5]-[QUALITY_MACHINERY]

- Every page graded cold per the suite standard's review law; research sections carry unresolved fact statements only — the executable proof rails live once in each charter PROOF_GATES.
- The ledger (`region-map/`) is the single registry of owner symbols and seam laws; consult before naming any cross-package type; the orchestrating session is its only writer.
- Bleeding-edge posture is verified, not assumed: newest stable of every package at admission time with restore proof; performance laws (FrozenDictionary/SearchValues/span formatting/zero-alloc capsules/GC posture per profile) are page rows that specs encode.
- Compute/threading/tensors are first-class architecture: one CPU budget across lane readers, ORT pools, and parallel partitions; allocation classes with receipts; solve-path isolation; benchmark claims gated by host fingerprint.
- Proof gates per charter: locked restore, api doctor/resolve, static plan/build, test --target, bridge scenarios for host seams, local mermaid render.
- Spike resolution ([8]): every `SPIKE` is driven to `FINALIZED` during planning by tier-1 decompile and tier-2 throwaway-harness proofs against the real packages, native libraries, and servers; only live-integrated-host probes remain open for the implementation session.

## [6]-[SESSION_ENTRY_PROTOCOL]

1. Read this file, then `TASKLOG.md`, then the target folder's charter (`Rasm.<Pkg>/.planning/README.md`) end-to-end.
2. Read the suite standard and ledger; doctrine (`docs/stacks/csharp/`) arrives settled.
3. Orchestrate with the method in [3]: ambitious synthesis is opus producer→red-team→merge (never a single agent), with parallel agents for reading, authoring, and repair — per-folder lanes parallel across folders and sequential within each. The orchestrator is the ledger's sole writer. The campaign never commits: uncommitted working-tree work is sacrosanct because agents and the operator edit concurrently, so the only permitted git is read-only inspection (status/diff/log).
4. Never re-derive: catalogues are decompile truth; pages are decision-complete; the binding precedence resolves conflicts; anything genuinely new lands as rows through the same machinery.

## [7]-[DEPTH_AND_GAP_HUNT]

The standing enrichment lens binding every per-folder deepening and enrichment pass, beyond the session's starting register. Each session reads every folder in full depth and grades each owner against a world-class library that must power many diverse, ambitious applications at once; a surface-level owner, an under-exploited admitted package, or a missing capability category is a defect, not an acceptable baseline.

- World-class-or-defect: grade every owner at the bar of "nothing a flagship app dev would hand-roll," not "present." For each page, name what an ambitious app would still build by hand from this folder's domain — a query shape, a store engine, a codec, a data type, an import/export path, a chart kind, a transport, a streaming or chunking path, a viewport binding, a data-source adapter — and own it now. An owner that stops at table-stakes is a deepening target.
- Categorical gap-hunt, not row top-up: the hunt finds whole concerns the folder overlooks, not only rows on existing axes. Persistence selects ANY store — Postgres, SQLite, DuckDB, embedded, remote — each fully realized with import/export, the full data-type surface, and streaming/chunking, no engine half-built; Compute streams and chunks at performance; every folder connects to remote, web, and backend so data flows from a source and back to a source; typography, charts, and visuals reach world-class breadth, power, and live-data feeding; rhino/gh2 viewport and panel integration pulls live data from any source. A missing category is a new owned concern, surfaced and integrated this session.
- External-lib maximization: every admitted package is mined to its deepest useful capability from multiple angles — operator, combinator, extension point, generated surface, zero-alloc and performance path — and integrated at the deepest form the package reaches; an admitted capability no owner exploits is a named defect.
- Topology completeness: every modality an app can take — backend, server, remote, on-host, companion, sidecar, standalone, web-fed, hub, paired — is equally and fully realized through the host-modality row set, never one face built and the rest stubbed; a capability present in-process but absent companion, sidecar, or remote is a gap.
- Root-up integration, never flat spam: a found gap lands by deepening the canonical owner with a row, case, column, fold, or policy value, or by a NEW design doc when a genuinely new owned concern warrants one — authored ground-up, woven into the densest owner, integrated where it belongs, never a loose arm, a parallel surface, or a tacked-on helper. Adding a spec page is welcome; adding flat or loose code is the defect. Every addition resolves through `docs/stacks/csharp/` in full and the suite standard.
- Additive and recurring: the gap-hunt runs every session as design pressure on top of the starting register, and each session closes by distilling the next frontier — new capability, feature, and category lists per file and folder — so the corpus deepens every cycle and never plateaus.

## [8]-[SPIKE_RESOLUTION]

A `SPIKE` owner is a transcription-complete fence whose runtime correctness is gated on a probe. Spikes are driven to `FINALIZED` during planning, never deferred to implementation, by proving each probe against the real external packages, native libraries, servers, and runtimes in throwaway scaffolding, then folding the verified spelling and behavior back into the fence and flipping its `[STATE]`. The design corpus stays the deliverable; the proof harness is disposable. When implementation begins, every closable spike is already finalized and transcribes by copy-paste.

The closure tiers, cheapest first; a spike rides the lowest tier that proves it.
1. Decompile and reflection — `uv run python -m tools.assay api query` and the `.api` catalogues confirm member existence and exact spelling with no execution, closing every uncatalogued-member RESEARCH row: the placeholder spelling in the fence is replaced with the verified one and the dependent owner row leaves `SPIKE`.
2. Throwaway harness against the real package — a minimal standalone proof under the gitignored temp root `.artifacts/spikes/<spike-id>/`, referencing the admitted package versions from the manifests, compiled and run against the real runtime to confirm behavior: a native library `dlopen` on the target RID, a live Postgres or DuckDB response against a throwaway database, an ONNX session run, an Avalonia headless render, a serializer round-trip, a byte-identity or codec check. The harness proves exactly the named probe; its verified output — member shapes, return contracts, error taxonomy, performance class — folds into the fence and card, and the temp dir is deleted. This tier closes every native, server, library, and headless spike in-session.
3. Live integrated host — only probes that strictly require the running integrated host (Generic Host boot and unload inside the live Rhino or GH2 plugin ALC, NSView embedding, in-process host-seam behavior) stay `SPIKE` until the host exists, and even these are de-risked standalone first, so only the final in-host confirmation remains for implementation.

- Every closed spike updates the page (RESEARCH row removed or member confirmed), the `[STATE]` column (`SPIKE` to `FINALIZED`), and the charter `GAP_LEDGER` and ROADMAP; a probe that reveals a runtime divergence reshapes the fence root-up — the spike has done its job by surfacing the unknown before it cost the system.
- The throwaway harness never enters package source, the planning pages, or the region ledger; it lives only under `.artifacts/spikes/` (gitignored), is referenced by no durable artifact, and is deleted once its result is folded in. Versions stay in the charter `ADMISSIONS_RECORD`; the harness reads them and never re-declares them.
- A session closes with every tier-1 and tier-2 spike finalized; the residual `SPIKE` set is exactly the tier-3 live-host probes, each named in the page RESEARCH cluster and the ROADMAP close-out list. The `SPIKE` count at session exit measures only host-bridge surface, never unproven library, native, or server capability.

## [9]-[THE_LOOP]

The campaign is a repeatable cycle, not a one-shot; each session is one turn and the corpus deepens every turn.
1. Consume the prior session's distillation — the registers and per-folder next-loop capability and task lists — as the decision-complete work-list.
2. Deepen and gap-hunt per folder ([7]): integrate the work-list AND run the categorical world-class gap-hunt in one full-depth read, surfacing and owning every missing category and every surface-level owner, root-up and integrated.
3. Resolve spikes ([8]): drive every tier-1 and tier-2 probe to `FINALIZED` against the real packages and tooling in throwaway scaffolding.
4. Cohere cross-folder without coupling: one owner per axis, consumers reference and never re-mint, the ledger records every seam-split; clean drift, collapse duplication, never flatten a sanctioned altitude-split.
5. Distill forward: verify 100% of this turn's contained work is realized as fences (not mentioned), then run comprehensive and app-driven ideation for the next frontier and emit a refined capability, feature, and task set per file and folder for the next turn.

The next session begins at step 1 against that fresh distillation, and `campaign-method.md` records the loop so every turn launches from the same protocol.
