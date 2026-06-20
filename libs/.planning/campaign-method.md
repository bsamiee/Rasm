# [CAMPAIGN_METHOD]

The planning loop that drives the three peer library branches — `libs/csharp`, `libs/python`, `libs/typescript` — to a decision-complete, implementation-ready bar. One method, three language doctrines, aligned never coupled, run through workflows. The topology (strata, per-language roles, wire seams) is `libs/.planning/architecture.md`; the authoring standard (doc-set, the four index-doc templates, the design-page grammar) is `libs/.planning/README.md`; form is `docs/standards/information-structure.md`, `docs/standards/formatting.md`, and `docs/standards/style-guide.md`; the per-language code doctrine and the universal density bar is `docs/stacks/<lang>/`, where `docs/stacks/csharp/` is the parity bar every branch meets. This file owns only what those do not: the campaign scope and its single planning mode, the bar, the universal principles and procedures when working on planning folders and projects.

## [01]-[MODES_AND_CAMPAIGN_SCOPE]

PLANNING does ALL discovery — research, package mining, ideation, gap-hunt — and authors a decision-complete design corpus in which every owner is a transcription-complete CODE FENCE: the real declaration with members, cases, fields, signatures, and bodies where logic lives, in exact decompile-verified spellings, that an implementer copies verbatim.

The operating surface is exactly: the `.planning/` design corpus (the design pages and the transcription-complete code FENCES inside them), the four index docs per tier (`README.md`, `ARCHITECTURE.md`/`architecture.md`, `IDEAS.md`, `TASKLOG.md`), the central manifests (`Directory.Packages.props`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.config`, `pyproject.toml`, `pnpm-workspace.yaml`, `.config`), the per-folder `.api/` catalogues, and the Nix toolchain. To "implement" or "realize" is to author or deepen a code FENCE inside a design page, plus the manifest, `.api`, and tooling work realization requires — [NEVER] to land a source file or a source tree. A code FENCE is a markdown fenced block inside a design page: it IS the work product.

The measure of a planning item is the diff to a code fence, never the prose around it; a card, a bare idea or task line, or a framing paragraph never substitutes for the fence it frames. A plan is implementation-ready by one test: Once plan mode is left, and writing begins, the workflow agents need ZERO external lookups or further investigation, the work is defined, clear, and context is already defined, and integration points are already on the page or its `.api` catalogue. A planning task or idea targets a design page, a code fence, a package or tooling admission, or an architecture/doc refinement — NEVER writing a source file (`.cs`/`.py`/`.ts` and the like); a task that names source-file creation or code transcription is out of scope, MUST be removed.

## [02]-[THE_BAR]

These are lib-grade foundations, not app scaffolding. The bar for every package, current and future:
- MOST IMPORTANT: All .planning/ folder content is ALWAYS refactored/re-built as needed when new functionality is added to add new functionality/features/capabilities ROOT/GROUND-UP, never tacked-on, never flat code spam, THE PURPOSE OF PRE-PLANNING ALL THESE FOLDERS IS EXACTLY THAT; FLEXIBILITY TO CONTINUAL REBUILD ALL CODE FENCE CONTENT TO ENSURE THE MOST DENSE, COMPLEX, RICH, UNIFIED CODE AS NEW FUNCTIONALITY/IDEAS/CONCEPTS EMERGE, WITHOUT NEEDING TO WORRY ABOUT CODE AS CODE, BUT TO FREELY, AND AGGRESSIVELY REFACTOR/REBUILD/CHANGE ANYTHING INSIDE THE DESIGN DOCS.
- World-class library code a company would adopt if it were public: the library internalizes the heavy logic, boilerplate, host quirks, native lifetimes, and failure handling so downstream apps compose capability instead of re-deriving it.
- No wrappers, no rename adapters, no grab-bag abstractions. Every surface adds real, higher-order capability built FROM the admitted packages, never a thinner face OVER them.
- Effortless complex apps without sacrifice: single entrypoints and unified rails reduce downstream overhead by absorbing complexity internally, never by limiting tooling or hiding capability.
- Dense, optimized, polymorphic code: one owner per axis, one entrypoint family per rail, one fault family per package, total dispatch, fold algebras, table-driven logic. Density is anti-redundancy and polymorphic collapse, not a line budget; flat code, parallel rails, and near-duplicate shapes are defects.
- Files are as large as the owned concern requires; size follows the concern, never a line ceiling.
- Full doctrine adherence: the route-owned code doctrine (`docs/stacks/<lang>/`) is law, composed as given, never restated.

## [03]-[UNIVERSAL_PRINCIPLES]

- One unified body of functionality, not loose feature arms. A new capability is a row, case, or column on an existing axis; the pressure to add a second surface is the signal to deepen the first.
- Modality completeness from the root: every axis that varies by where code runs carries the full row set — host modalities, process topologies, deployment placements — so apps load and offload across those rows without new surfaces.
- Parameterization as a universal need: anything that can vary is a row value, never a hardcode.
- External-lib maximization, truthfully: every admitted package is mined to its full capability and integrated through the owning axis; an admitted capability no owner exploits is a named gap.
- Plug-in-play composition, glove-in-hand integration: packages are complete in isolation and integrate only through the declared ports and the wire/companion seams. Coupling beyond the ports is a defect.
- Unified telemetry, logging, wiring, ports, communication, integration, and observability from the source: signals contribute through the receipt-sink and contributor ports, plug-in-play at composition, never bolted on per app.

## [04]-[WORKFLOW_ORCHESTRATION]

NOTE: Workflow may be altered by prompt based on scope of `<TARGET>`, IF the target is the full `libs/`, or language specific, like `libs/csharp/`, then proceed with proper multi-tiered workflow instead of scoped/specific workflow.

Every substantive pass runs as workflows, never one-off agents, fanned out across three altitudes and refined at each:

- Tiers: per-folder, then a per-language pass, then the cross-`libs/` master. IDEATE runs top-down so a master concept seeds language ideas that seed folder tasks; IMPLEMENT and REFINE run bottom-up so each higher tier works a settled lower one. Each higher tier is a FULL pass at broader scope, never a thin reconcile or aggregation: it runs the same research/author/critique/red-team depth as the folder tier, adds the ideas, tasks, and fences that only emerge at the language or cross-`libs/` altitude, and reviews, refines, and massages the lower tier's items into cross-folder and cross-language alignment — widening scope or integration where it raises value and correcting or removing any item formulated in isolation of its folder's ground-up code-doctrine integration. A cross-language idea lands at its right touchpoint the same turn and is never stranded at one tier.
- Stages: research (modern, current, grounded in the monorepo purpose), then implement, then critique (constructive, full-view), then red-team (ultra-harsh adversarial). Ambitious synthesis is producer -> red-team -> merge, never a single agent.
- Dual-axis review at critique and red-team: fence code-quality against the route-owned doctrine — fold parallel shapes into one polymorphic owner, unify rails, the densest form that stays transcription-complete, capability conserved with no functionality removed, correctness and optimization in performance and sophistication, not only line count; and doc-craft against the authoring standard and the form standards — placement, card and page shape, signature truthfulness, zero-provenance discipline. The guard: fence-level rail-unification and density terms never surface as an `ARCHITECTURE` sub-domain name.
- Boundary integrity at critique and red-team: grade every concern against the hierarchy boundaries `architecture.md` owns — a concern found owned twice within a runtime, a folder mixing unrelated concerns, a concern scattered across three or four folders, or a cross-folder or cross-language touchpoint coupled to a sibling's interior rather than recorded as an aligned seam in the folder `ARCHITECTURE.md` `[2]-[SEAMS]` map, is a defect the pass fixes or records.
- Coverage audit (IDEATE): read the central managers (`Directory.Packages.props`, `pyproject.toml`, `pnpm-workspace.yaml`) and each README registry, and emit tasks for every mismatch (a package claimed but absent from the manager, or present but unregistered), every missing `.api/<package>.md` catalogue, every newer or stronger package, and the per-language testing and tooling gaps.
- Model policy: a lighter model for exploration, decompile reads, currency dossiers, and throwaway-harness authoring; a capable model for every authoring, red-team, merge, alignment, cross-cutting, and ideation lane, with ambitious synthesis run producer -> red-team -> merge and never a single agent. Never the smallest tier, and never over-granularize — assign by veracity need.
— Prefer inline `script` over `scriptPath`+`args`, pass file lists as real JSON arrays, and verify `args` is populated before using it for phase selection, since workflow globals are runtime state separate from shell, Nix, aliases, and PATH. Never run an unattended probe that raises an OS dialog — a keychain read (`SecItem*`/Security.framework), DPAPI, a biometric or credential-store read, a signal-to-self (SIGHUP), or `docker login` all block on a prompt the session cannot answer; provisioning stays clean and additive with operator containers and volumes sacrosanct.

## [05]-[SIBLING_BRANCHES]

The campaign treats the three branches as first-class peers, additive never subtractive: each receives its own concurrent allocation, never effort diverted from a sibling, and each is re-derived from the finalized owner set every campaign rather than mirroring a sibling's layout. Each branch runs its own aggressive ideation into the tiered `IDEAS.md`/`TASKLOG.md` the authoring standard owns (folder root and branch `.planning/`), reading the sibling corpora for cross-pollination; the cross-language pool and the tri-language concert live in `libs/.planning/`. The branch roles, the parity bar, and the wire/companion seams that bound this cross-pollination are `architecture.md`.

## [06]-[PLAN_MODE_DISCIPLINE]

Plan mode is left only when the plan is exhaustive. Before exit, every gate, fence, page, sub-domain, admission, catalogue, manifest edit, and refinement the campaign will touch is identified in decision-complete detail — no residual research deferred to an execution turn, no entry-gate assumption left unresolved, no finding, idea, gap, or suggestion truncated or tossed. Each finding lands at its right tier and target; a finding without a home is a defect, not a discard. Every addition — fence, card, page, admission, refinement — is critiqued and red-teamed as it lands, and the whole plan and corpus are critiqued and red-teamed as one body before the plan finalizes; a single un-adversaried author is never the final word. A red-team or verify stage applies its fixes rather than only reporting them: it verifies before flipping any status — a spike or gap is marked finalized only against a cited `.api` line or harness output — fixes true positives in place, refines false positives rather than deleting the finding, and never marks work done without evidence. The plan carries the full scope a 4+ page blueprint demands; a thin plan is an un-exhausted one.

## [07]-[THE_LOOP]

The campaign cycles IDEATE -> IMPLEMENT -> REFINE and deepens every turn. An idea closes only when its tasks exist and are realized in fences; a closed idea proven un-realized re-opens. Nothing plateaus: a finished owner is the floor for the next idea, never the ceiling, and a turn whose ideation only re-confirms existing capability has failed its most important job.

IDEATE, IMPLEMENT, REFINE, are all seperate prompts / isolated, not done within the SAME workflow, the PROMTPS are:
- `.claude/prompts/1-ideate-pass.md`
- `.claude/prompts/2-refine-pass.md`
- `.claude/prompts/3-implement-pass.md`

A SESSIONS IS ONLY EVER ONE OF THESE, NEVER MIXED, USER WILL PROVIDE INITIAL PROMPT TO START SESSION.