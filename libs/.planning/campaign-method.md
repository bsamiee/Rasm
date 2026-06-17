# [CAMPAIGN_METHOD]

The planning loop that drives the three peer library branches — `libs/csharp`, `libs/python`, `libs/typescript` — to a decision-complete, implementation-ready bar. One method, three language doctrines, aligned never coupled, run through workflows. The topology (strata, per-language roles, wire seams) is `architecture.md`; the authoring standard (doc-set, the four index-doc templates, the design-page grammar) is `README.md`; form is `docs/standards/information-structure.md`, `formatting.md`, and `style-guide.md`; the per-language code doctrine and the universal density bar is `docs/stacks/<lang>/`, where `docs/stacks/csharp/` is the parity bar every branch meets. This file owns only what those do not: the two modes, the bar, the universal principles, the three passes, the workflow orchestration, the sibling-branch campaign treatment, and the loop.

## [1]-[TWO_MODES]

PLANNING and SOURCE-TRANSCRIPTION never interleave. PLANNING — this method — does ALL discovery (research, package mining, ideation, gap-hunt) and authors a decision-complete design corpus in which every owner is a transcription-complete CODE FENCE: the real declaration with members, cases, fields, signatures, and bodies where logic lives, in exact decompile-verified spellings, that an implementer copies verbatim. SOURCE-TRANSCRIPTION is a later run that writes the real source from the finalized plan and discovers nothing new. The measure of a planning item is the diff to a code fence, never the prose around it; a card, a bare idea or task line, or a framing paragraph never substitutes for the fence it frames. Workflows drive BOTH modes: planning workflows author the corpus to extreme exhaustiveness, and the later source-transcription run dispatches workflows that transcribe the finalized plan and find nothing new. A plan is implementation-ready by one test — a transcription agent needs ZERO external lookups: every type, member, package spelling, fault, literal, and integration point is already on the page or its `.api` catalogue. Implementation is never the time for broad research or investigation; that work is exhausted in planning. A planning task or idea targets a design page, a code fence, a package or tooling admission, or an architecture/doc refinement — NEVER writing a source file (`.cs`/`.py`/`.ts` and the like); a task that names source-file creation or code transcription is out of scope, and the REFINE pass removes it.

## [2]-[THE_BAR]

These are lib-grade foundations, not app scaffolding. The bar for every package, current and future:

- World-class library code a company would adopt if it were public: the library internalizes the heavy logic, boilerplate, host quirks, native lifetimes, and failure handling so downstream apps compose capability instead of re-deriving it.
- No wrappers, no rename adapters, no grab-bag abstractions. Every surface adds real, higher-order capability built FROM the admitted packages, never a thinner face OVER them.
- Effortless complex apps without sacrifice: single entrypoints and unified rails reduce downstream overhead by absorbing complexity internally, never by limiting tooling or hiding capability.
- Dense, optimized, polymorphic code: one owner per axis, one entrypoint family per rail, one fault family per package, total dispatch, fold algebras, table-driven logic. Density is anti-redundancy and polymorphic collapse, not a line budget; flat code, parallel rails, and near-duplicate shapes are defects.
- Files are as large as the owned concern requires; size follows the concern, never a line ceiling.
- Full doctrine adherence: the route-owned code doctrine (`docs/stacks/<lang>/`) is law, composed as given, never restated.

## [3]-[UNIVERSAL_PRINCIPLES]

- One unified body of functionality, not loose feature arms. A new capability is a row, case, or column on an existing axis; the pressure to add a second surface is the signal to deepen the first.
- Modality completeness from the root: every axis that varies by where code runs carries the full row set — host modalities, process topologies, deployment placements — so apps load and offload across those rows without new surfaces.
- Parameterization as a universal need: anything that can vary is a row value, never a hardcode.
- External-lib maximization, truthfully: every admitted package is mined to its full capability and integrated through the owning axis; an admitted capability no owner exploits is a named gap.
- Plug-in-play composition, glove-in-hand integration: packages are complete in isolation and integrate only through the declared ports and the wire/companion seams. Coupling beyond the ports is a defect.
- Unified telemetry, logging, and observability from the source: signals contribute through the receipt-sink and contributor ports, plug-in-play at composition, never bolted on per app.

## [4]-[THE_PASSES]

The campaign runs three pass-modes; a session selects one, routed by the matching `.claude/prompts/` kickoff:

- IDEATE — build the forward pool top-down (cross-`libs/` then language then folder), distill ideas into tasks, and run the coverage audit (defined under workflow orchestration) into tasks. It grows the WIP folder set through a domain-frontier hunt: a whole conceptual domain a branch or folder lacks becomes a new sub-domain (the common case) or, at a high bar, a new top-level folder candidate with its charter and stratum placement — always a genuine full higher-order domain validated against the strata, never a mini sibling. Adds ideas and tasks; implements nothing.
- IMPLEMENT — the meat: realize the tasks and ideas into deep code fences within the folder -> language -> repo framework, mine every admitted package's `.api` to its full capability, extend existing owners and add new files and folders, and crush surface sprawl into fewer, richer, optimized owners with zero functionality loss. An idea closes once its tasks exist and are realized.
- REFINE — cleanup and architecture/quality hygiene: split god pages, restructure mini folder/file pairs that lack growth potential, correct stale or wrong `.api` catalogue entries (missing catalogues are an IDEATE coverage finding), strip comment litter from fences, and enforce the density bar and the doc-craft form. Implements fixes; emits a task or idea only to DEFER a structure with genuine growth potential it preserves rather than merges.

## [5]-[WORKFLOW_ORCHESTRATION]

Every substantive pass runs as workflows, never one-off agents, fanned out across three altitudes and refined at each:

- Tiers: per-folder, then a per-language pass, then the cross-`libs/` master. IDEATE runs top-down so a master concept seeds language ideas that seed folder tasks; IMPLEMENT and REFINE run bottom-up so each higher tier works a settled lower one. Each higher tier is a FULL pass at broader scope, never a thin reconcile or aggregation: it runs the same research/author/critique/red-team depth as the folder tier, adds the ideas, tasks, and fences that only emerge at the language or cross-`libs/` altitude, and reviews, refines, and massages the lower tier's items into cross-folder and cross-language alignment — widening scope or integration where it raises value and correcting or removing any item formulated in isolation of its folder's ground-up code-doctrine integration. A cross-language idea lands at its right touchpoint the same turn and is never stranded at one tier.
- Stages: research (modern, current, grounded in the monorepo purpose), then implement, then critique (constructive, full-view), then red-team (ultra-harsh adversarial). Ambitious synthesis is producer -> red-team -> merge, never a single agent.
- Dual-axis review at critique and red-team: fence code-quality against the route-owned doctrine — fold parallel shapes into one polymorphic owner, unify rails, the densest form that stays transcription-complete, capability conserved with no functionality removed, correctness and optimization in performance and sophistication, not only line count; and doc-craft against the authoring standard and the form standards — placement, card and page shape, signature truthfulness, zero-provenance discipline. The guard: fence-level rail-unification and density terms never surface as an `ARCHITECTURE` sub-domain name.
- Boundary integrity at critique and red-team: grade every concern against the hierarchy boundaries `architecture.md` owns — a concern found owned twice within a runtime, a folder mixing unrelated concerns, a concern scattered across three or four folders, or a cross-folder or cross-language touchpoint coupled to a sibling's interior rather than recorded as an aligned wire/boundary, is a defect the pass fixes or records.
- Coverage audit (IDEATE): read the central managers (`Directory.Packages.props`, `pyproject.toml`, `pnpm-workspace.yaml`) and each README registry, and emit tasks for every mismatch (a package claimed but absent from the manager, or present but unregistered), every missing `.api/<package>.md` catalogue, every newer or stronger package, and the per-language testing and tooling gaps.
- Discipline: the orchestrating session is the sole committer and sole ledger writer; agents author content and return it, and never run git. Commit by explicit pathspec, never `git add -A` over a foreign path. Keep concurrent heavy workflows at two or three. Assign a capable model to authoring, red-team, and synthesis and a lighter model to exploration and decompile reads; never the smallest tier. Never run an unattended probe that raises an OS prompt (keychain, biometric, credential store, `docker login`).

## [6]-[SIBLING_BRANCHES]

The campaign treats the three branches as first-class peers, additive never subtractive: each receives its own concurrent allocation, never effort diverted from a sibling, and each is re-derived from the finalized owner set every campaign rather than mirroring a sibling's layout. Each branch runs its own aggressive ideation into the tiered `IDEAS.md`/`TASKLOG.md` the authoring standard owns (folder root and branch `.planning/`), reading the sibling corpora for cross-pollination; the cross-language pool and the tri-language concert live in `libs/.planning/`. The branch roles, the parity bar, and the wire/companion seams that bound this cross-pollination are `architecture.md`.

## [7]-[THE_LOOP]

The campaign cycles IDEATE -> IMPLEMENT -> REFINE and deepens every turn. An idea closes only when its tasks exist and are realized in fences; a closed idea proven un-realized re-opens. Nothing plateaus: a finished owner is the floor for the next idea, never the ceiling, and a turn whose ideation only re-confirms existing capability has failed its most important job.
