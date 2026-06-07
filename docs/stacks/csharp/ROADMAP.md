# [STACKS_CSHARP_ROADMAP]

Roadmap state: ACTIVE. Sequence type: Hardening sequence. This roadmap sequences `docs/stacks/csharp/` from the current decision atlas into the C# coding center of gravity for language policy, platform truth, first-class package capability, and proof routing. Mutable implementation status stays in the owning source, manifest, or project-local roadmap; this file carries durable milestone order, exit proof, and documentation handoffs.

## [1][SCOPE]

Mission: make `docs/stacks/csharp/` the first route for C# coding decisions, with concept pages that internalize approved package capability instead of exposing package-shaped documentation.

Included: C# stack README, language policy, rails/effects, generated domain shapes, numeric algorithms, sparse factorization, platform/build pages, testing pages, and future concept pages named here.

Excluded: project-local implementation sequence for `libs/csharp/*`, host SDK member truth, generated API catalogs, package version tables outside central manifests, and portable skill authoring.

Live planning route: this Markdown roadmap.

Proof route: `Directory.Build.props`, `Directory.Packages.props`, `Workspace.slnx`, local project files, source, lockfiles, generated contracts, and targeted docs checks.

## [2][PLANNING_TRUTH]

Repository source and manifests own current package and build truth. Project-local README, ARCHITECTURE, ROADMAP, and AGENTS files own exact planned package behavior for their package. C# stack concept pages own reusable implementation policy once the behavior is source-backed or accepted as an adoption gate.

When a package is admitted centrally, conditioned by build logic, bundled by a host, owned by a tool, or accepted by a local roadmap gate, treat it as first-class planning material. Do not wait for a production call site to define the target policy when the owner route already proves the capability belongs in the platform.

## [3][SEQUENCE_TYPE]

Hardening sequence:
    Scope: documentation and policy surfaces that drive C# implementation.
    Proof: route existence, source-backed package state, concept-owner fit, stale-route scan, link check, and standards re-read.

## [4][STATUS_VOCABULARY]

Milestone statuses use `QUEUED`, `ACTIVE`, `BLOCKED`, `DEFERRED`, `COMPLETE`, `DROPPED`, and `CANCELED`.

## [5][ID_PROGRESS_RULES]

Milestone IDs are stable `M<N>` anchors. Progress is unweighted checked exit criteria; proof must agree with each checked criterion before the milestone can move to `COMPLETE`.

## [6][EXIT_PROOF_RULES]

Exit criteria close only through current source, manifests, generated contracts, local project files, maintained provider documentation, targeted link checks, stale-route scans, and standards re-read. A package candidate can close planning criteria through an accepted owner route, but implementation claims need graph or source proof.

## [7][MILESTONES]

M1:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: add `runtime-execution.md`.
    Mission contribution: captures runtime records, cancellation, deadlines, channels, progress streams, execution receipts, allocation measurement, and observability identity as one C# execution policy.
    Code scope: `docs/stacks/csharp/runtime-execution.md`, `docs/stacks/csharp/README.md`, `docs/stacks/csharp/AGENTS.md`, `docs/usage/`.
    Deliverables: concept page and README route.
    Exit criteria:
        - [ ] runtime-record and `Eff<RT,T>` ownership rules are stated without service-location wrappers
        - [ ] channel/progress/receipt rules route AppHost, AppUi, Compute, and Persistence concerns without duplicating project-local rails
        - [ ] observability identity and `[LoggerMessage]` routing are included without creating a telemetry package catalog
    Proof surface: AppHost, AppUi, Compute, Persistence local manuals; build/package manifests; link check.
    Proof map: 0/3.

M2:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: add `reactive-ui-surfaces.md`.
    Mission contribution: treats retained UI, reactive projection, live data, drawing, charting, controls, icons, dialogs, and diagnostics as one UI capability surface.
    Code scope: `docs/stacks/csharp/reactive-ui-surfaces.md`, `docs/stacks/csharp/platform/build-and-packages.md`, AppUi local docs.
    Deliverables: concept page and platform graph updates where needed.
    Exit criteria:
        - [ ] UI scheduler, command, live projection, chart, visual, and diagnostic boundaries are described as product concepts
        - [ ] native asset and lockfile/package-matrix facts route to platform/build truth
        - [ ] viewport-overlay behavior routes to host UI owners instead of toolkit wrappers
    Proof surface: AppUi project file, lockfile, local architecture, central package manifest.
    Proof map: 0/3.

M3:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: add `local-durable-state.md`.
    Mission contribution: captures adoption-gated durable store, native SQLite initialization, PRAGMAs, migrations, snapshots, support artifacts, and live-state projection as durable-state policy.
    Code scope: `docs/stacks/csharp/local-durable-state.md`, `docs/stacks/csharp/platform/build-and-packages.md`, Persistence local docs.
    Deliverables: concept page and package-state route updates when graph admission changes.
    Exit criteria:
        - [ ] `DbContext` lifetime is modeled as an effect boundary with one context per operation
        - [ ] SQLite native init, PRAGMA, migration, downgrade, corruption, backup, and snapshot rules are source-backed
        - [ ] live-state projection rules preserve AppUi scheduler and Persistence ownership boundaries
    Proof surface: Persistence local manuals, central manifests, lockfiles when present.
    Proof map: 0/3.

M4:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: add `composition-roots.md`.
    Mission contribution: captures scan/decorate composition, DI/container mode, Generic Host boundaries, keyed services, validation, options, and companion-root boundaries without making package folders.
    Code scope: `docs/stacks/csharp/composition-roots.md`, `docs/usage/composition.md`, AppHost local docs.
    Deliverables: C# composition concept page and usage handoff.
    Exit criteria:
        - [ ] runtime-record mode and DI/container mode are separated by composition boundary
        - [ ] scan, registration, keyed service, decorator ordering, and validation rules are stated as owner decisions
        - [ ] package graph and companion-only package state route to platform/build truth
    Proof surface: AppHost local manuals, central manifests, usage composition owner.
    Proof map: 0/3.

M5:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: add `performance-diagnostics.md`.
    Mission contribution: captures span/SIMD/SearchValues/regex/AOT/BenchmarkDotNet/profiling guidance as evidence-gated implementation policy.
    Code scope: `docs/stacks/csharp/performance-diagnostics.md`, `docs/stacks/csharp/platform/system-apis.md`, `docs/stacks/csharp/testing/specialized-rails.md`.
    Deliverables: concept page plus route cleanup for overloaded system API content.
    Exit criteria:
        - [ ] hot-path BCL guidance names the implementation owner and proof rail
        - [ ] diagnostics guidance explains source-generator failures, effect trace reading, and profiling route without command catalogs
        - [ ] BenchmarkDotNet and specialized proof rails route through testing docs
    Proof surface: existing C# stack pages, testing pages, source/tool owners.
    Proof map: 0/3.

M6:
    Status: QUEUED.
    Progress: [░░░░░░░░░░] 0%
    Goal: extend `docs/usage/composition.md` with the AppHost/AppUi/Compute/Persistence spine.
    Mission contribution: keeps cross-stack boot, handoff, drain, retry, persistence, progress, and UI observation ownership outside language stack pages.
    Code scope: `docs/usage/`, `docs/stacks/csharp/README.md`, project-local App platform docs.
    Deliverables: usage owner route and stack README handoff.
    Exit criteria:
        - [ ] AppHost schedules, Compute executes, Persistence stores, and AppUi observes are stated as cross-stack ownership
        - [ ] retry ownership and Generic Host/DI boundaries route to usage/composition and C# stack concept pages
        - [ ] project-local implementation sequence remains in package roadmaps
    Proof surface: App platform local manuals, docs usage route, central package manifests.
    Proof map: 0/3.

## [8][BOUNDARIES]

- Keep `numeric-algorithms.md` and `sparse-factorization.md` separate; iterative sparse algorithms and direct sparse factorization answer different implementation questions.
- Do not add standalone Symbolics, FSharp, or FParsec pages until production source or an accepted owner route gives them concept ownership beyond graph closure.
- Do not create package folders, provider catalogs, compatibility aliases, link farms, or roadmap-only stubs.
- Package versions and graph state stay in `platform/build-and-packages.md`; concept pages state coding decisions.
- Project-local package roadmaps stay in `libs/csharp/*/ROADMAP.md`; C# stack pages only promote reusable implementation policy.

## [9][VALIDATION]

Before closing a roadmap milestone:
- run `git diff --check -- docs/stacks/csharp docs/usage docs/README.md AGENTS.md`;
- run a stale-route scan for removed paths and renamed pages touched by the milestone;
- run or emulate a targeted Markdown link check over changed docs;
- re-read changed files against `docs/standards/style-guide.md`, `docs/standards/information-structure.md`, `docs/standards/formatting.md`, `docs/standards/reference/readme.md`, `docs/standards/explanation/roadmap.md`, and `docs/standards/agents-md.md`.
