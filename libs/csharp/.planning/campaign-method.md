# [CAMPAIGN_METHOD]

The doctrinal approach behind the app-package planning corpora and every library campaign that follows (including `libs/typescript`). A future session loads this file first: it states why these libraries are built this way, the method that produces them, the bar they are held to, and the staged work that remains before and during implementation. Folder-specific entry points live in each package charter (`Rasm.<Pkg>/.planning/README.md`); open work lives in `TASKLOG.md`.

## [1]-[THE_BAR]

These are lib-grade foundations, not app scaffolding. The bar for every package, current and future:

- World-class library code a company would adopt if it were public: the library internalizes the heavy logic, boilerplate, host quirks, native lifetimes, and failure handling so downstream apps compose capability instead of re-deriving it.
- No wrappers, no wrapper-faces, no rename adapters, no grab-bag abstractions. Every surface adds real functionality: high-value, higher-order capability built FROM the admitted packages, never a thinner face OVER them.
- Effortless complex apps without sacrifice: single entry points and unified rails reduce downstream agent overhead, and they do it by absorbing complexity internally — never by limiting tooling, hiding capability, or simplifying away power.
- Dense, optimized, polymorphic code at 25-35% of naive LOC: one owner per axis, one entrypoint family per rail, one fault family per package, total Switch dispatch, fold algebras, table-driven logic. Flat code, parallel rails, and near-duplicate shapes are defects.
- Full doctrine adherence: `docs/stacks/csharp/` (shapes, rails-and-effects, surfaces-and-dispatch, boundaries, language, system-apis) is law, composed as given, never restated.

## [2]-[UNIVERSAL_PRINCIPLES]

- One unified body of functionality, not loose feature arms. A new capability is a row, case, or column on an existing axis — never a sibling surface. The pressure to add a second surface is the signal to deepen the first.
- Modality completeness from the root: every axis that varies by where code runs carries the full row set — in-process (rhino-plugin, gh2-plugin), standalone, paired (standalone attached to a running Rhino), companion, sidecar, headless service, web service, test host. Apps load and offload compute, data, and UI across these rows without new surfaces; the topology vocabulary lives in `libs/csharp/.planning/FEATURES.md`.
- Parameterization as a universal need: anything that can vary is a row value, never a hardcode. A new data source, asset source, theme probe, transport, store engine, or schedule is one row on the owning axis. Host-agnostic sourcing is the default (a UI host is rhino, empty, custom, or any admitted package; the same law applies to every axis).
- External-lib maximization, truthfully: every admitted package is mined to its full capability and integrated through the owning axis; first-party companions and extensions (database extensions included — PG contrib, PostGIS/GIS lanes, SQLite compile surface, DuckDB extensions) are first-class rows from the root. An admitted capability no page exploits is a named gap.
- Plug-in-play composition, glove-in-hand integration: packages are complete in isolation and integrate only through the AppHost ports and the ledger seam laws (one clock seam, one cache owner, one retry owner per hop, one correlation spine, one drain conductor, one wire vocabulary, the HLC stamp as the only cross-process causal primitive). Coupling beyond the ports is a defect; so is a seam re-taught instead of consumed.
- Unified telemetry/logging/observability from the source: every package contributes through the contributor and receipt-sink ports; signals are plug-in-play at composition, never bolted on per app.

## [3]-[THE_METHOD]

The repeatable campaign shape that produced this corpus, binding for every future library campaign:

1. INFRA TRUTH FIRST — root config, manifests, lockfiles, tool pins, and classification are made honest before anything else (the P0a pattern); admissions execute with restore proof; every admitted package gets a decompile-grounded catalogue page; every existing catalogue is re-verified against decompiled surfaces (phantom APIs are the named defect class).
2. RESEARCH RESERVOIR — design lanes, adversarial verifiers, gap-hunt lenses, deep-read lanes, decompile probes, and web currency checks run BEFORE authoring; binding precedence is locked decisions > verifier corrections > synthesis closures > design JSON.
3. IDEATION BEFORE DESIGN — exhaustive concept enumeration (isolation + in-concert, all topologies, domain-specific: speckle-class sync hubs, GIS pipelines from heterogeneous sources, compute farms, dataset products) drives the pages; imagine everything, then prune bloat ruthlessly; every survivor rides an axis row. The refinement blueprint (per-package, Fable-authored) converts ideation into per-page cluster plans, binding corrections, density guidance, and anti-bloat vetoes.
4. AUTHOR-THEN-REPAIR — parallel page authors transcribe the blueprint under the suite standard (`README.md` here: page grammar, signature law, language rules, the review law); consolidated review-and-repair waves find AND fix in one pass; the orchestrator is the only ledger writer.
5. CROSS-FOLDER COHESION — ledger consolidation (owner symbols, seam splits, adjudications), then a final deep pass per folder: under-utilization hunt, heavy code critique on every fence, doc integrity, research rows answerable by decompile get answered, zero-guessing validation.
6. EVERYTHING DURABLE — working material persists under `.artifacts/planning-briefs/` (gitignored); law lands in charters, pages, the ledger, atlases, and ROADMAPs; versions live only in charter ADMISSIONS_RECORD tables.

## [4]-[STAGED_WORK_BEFORE_IMPLEMENTATION]

The sessions that precede implementation, in order; each is its own campaign run with the method above:

1. PER-FOLDER EXTREME-DETAIL DEEPENING — each package in isolation: re-read every catalogue against every page, push capabilities/features/considerations further (the AppUi mined-no-gaps treatment as the bar for all), close TASKLOG rows owned by the folder, deepen thin catalogue pages (named in TASKLOG), and re-run the code critique until any world-class app is buildable from the pages alone.
2. COMPREHENSIVE APP IDEATION — a dedicated ideation campaign across all dimensionalities (local, web-linked, data-sourcing, domain-specific, multi-process) producing new concept rows integrated root-up into pages and atlases; advanced complexity is the target (GIS ingestion pipelines, distributed solve fabrics, dataset curation products, collaboration hubs), always with the dual mandate: universal standalone usage AND first-class rhino/gh2 operation.
3. SIBLING LIBRARY CAMPAIGNS — `libs/typescript` (entry: `libs/typescript/.planning/README.md`) and any further libs, each made lib-grade in isolation with its own infra truth, catalogue extraction, ideation, pages, charter, and ledger — integrating with these packages only through the wire contracts, exactly as these packages integrate with each other only through ports.
4. FINAL ARCHITECTURE PASS — the closing pre-implementation session: per-package file plan finalized against the charter BUILD_ORDER, logic-flow walkthrough of every entrypoint rail end-to-end, cross-package boot/drain/wire choreography rehearsed on paper, implementation-start gates executed (bridge spikes, native probes), and the proof-gate matrix confirmed green-able. Exit: implementation begins with transcription only.

## [5]-[QUALITY_MACHINERY]

- The suite standard (this directory's `README.md`) graded cold per its review law; zero hedge/meta/citation tokens; research rows carry executable routes only when a local decompile cannot answer them.
- The ledger (`region-map/`) is the single registry of owner symbols and seam laws; consult before naming any cross-package type; the orchestrating session is its only writer.
- Bleeding-edge posture is verified, not assumed: newest stable of every package at admission time with restore proof; performance laws (FrozenDictionary/SearchValues/span formatting/zero-alloc capsules/GC posture per profile) are page rows that specs encode.
- Compute/threading/tensors are first-class architecture: one CPU budget across lane readers, ORT pools, and parallel partitions; allocation classes with receipts; solve-path isolation; benchmark claims gated by host fingerprint.
- Proof gates per charter: locked restore, api doctor/resolve, static plan/build, test --target, bridge scenarios for host seams, local mermaid render.

## [6]-[SESSION_ENTRY_PROTOCOL]

1. Read this file, then `TASKLOG.md`, then the target folder's charter (`Rasm.<Pkg>/.planning/README.md` — PAGE_INDEX, BUILD_ORDER, PROOF_GATES, PROHIBITIONS, ADMISSIONS_RECORD, REFINEMENT_HORIZON).
2. Read the suite standard and ledger; doctrine (`docs/stacks/csharp/`) arrives settled.
3. Orchestrate with the method in [3]: Fable for synthesis, charters, adjudication, and grading judgment; parallel agents for reading, authoring, and repair; the orchestrator owns the ledger and the commits.
4. Never re-derive: catalogues are decompile truth; pages are decision-complete; the binding precedence resolves conflicts; anything genuinely new lands as rows through the same machinery.
