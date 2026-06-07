# [STACKS_CSHARP_ROADMAP]

Roadmap state: [ACTIVE]. Sequence type: Hardening sequence. Mission: sequence `docs/stacks/csharp/` from the current decision atlas into the C# coding center of gravity for language policy, platform truth, first-class package capability, and proof routing. Code scope: C# stack concept pages, README routes, platform/build pages, testing pages, and cross-stack usage handoffs named here. Live planning route: this Markdown roadmap. Proof route: source, manifests, generated contracts, local project files, lockfiles, and targeted docs checks.

## [1][SCOPE]

Mission: make `docs/stacks/csharp/` the first route for C# coding decisions, with concept pages that internalize approved package capability instead of exposing package-shaped documentation.

Included: C# stack README, language policy, rails/effects, generated domain shapes, numeric algorithms, sparse factorization, platform/build pages, testing pages, and future concept pages named here.

Excluded: project-local implementation sequence for `libs/csharp/*`, host SDK member truth, generated API catalogs, package version tables outside central manifests, portable skill authoring, and product roadmap commitments.

Live planning route: this Markdown roadmap.

Proof route: `Directory.Build.props`, `Directory.Packages.props`, `Workspace.slnx`, local project files, source, lockfiles, generated contracts, and targeted docs checks.

## [2][ROADMAP_CONTRACT]

Repository source and manifests own current package and build truth. Project-local README, ARCHITECTURE, ROADMAP, and AGENTS files own exact planned package behavior for their package. C# stack concept pages own reusable implementation policy once the behavior is source-backed or accepted as an adoption gate.

When a package is admitted centrally, conditioned by build logic, bundled by a host, owned by a tool, or accepted by a local roadmap gate, treat it as first-class planning material. Do not wait for a production call site to define the target policy when the owner route already proves the capability belongs in the platform.

Task status is the roadmap's executable state. A task reaches `[COMPLETE]` only when `Exit` is satisfied and `Proof required` agrees with the owning source, command, artifact, link check, or review. A milestone reaches `[COMPLETE]` only when every included task is `[COMPLETE]` and required handoffs are closed or routed away.

## [3][LIFECYCLE_IDS]

Task statuses use `[QUEUED]`, `[ACTIVE]`, `[BLOCKED]`, `[DEFERRED]`, `[COMPLETE]`, `[DROPPED]`, and `[CANCELED]`.

Milestone IDs use `CSH-<AREA>-000`. Task IDs use `CSH-<AREA>-010`, `CSH-<AREA>-020`, and later multiples of 10 so new tasks can be inserted without renumbering. Original IDs are never reused after completion, deferral, drop, cancellation, or replacement.

## [4][BOUNDARIES]

- Keep `numeric-algorithms.md` and `sparse-factorization.md` separate; iterative sparse algorithms and direct sparse factorization answer different implementation questions.
- Do not add standalone Symbolics, FSharp, or FParsec pages until production source or an accepted owner route gives them concept ownership beyond graph closure.
- Do not create package folders, provider catalogs, compatibility aliases, link farms, or roadmap-only stubs.
- Package versions and graph state stay in `platform/build-and-packages.md`; concept pages state coding decisions.
- Project-local package roadmaps stay in `libs/csharp/*/ROADMAP.md`; C# stack pages only promote reusable implementation policy.

## [5][MILESTONES]

### [5.1][CSH_RUNTIME]

ID: CSH-RUNTIME-000
Outcome: runtime, composition, and execution-boundary policy becomes one coherent C# stack sequence.
Why now: runtime execution, composition roots, cross-stack usage, and route admission currently split the next C# stack work into page-shaped tasks without stable child task identity.
Scope: `docs/stacks/csharp/runtime-execution.md`, `docs/stacks/csharp/composition-roots.md`, `docs/usage/composition.md`, `docs/stacks/csharp/README.md`, `docs/stacks/csharp/AGENTS.md`.
Included tasks: CSH-RUNTIME-010, CSH-RUNTIME-020, CSH-RUNTIME-030, CSH-RUNTIME-040.
Completion rule: all included tasks are `[COMPLETE]`, and required README, AGENTS, and usage handoffs are closed or routed away.

Task:
ID: CSH-RUNTIME-010
Status: [QUEUED]
Milestone: CSH-RUNTIME-000
Work: author runtime execution policy.
Target: `docs/stacks/csharp/runtime-execution.md`, `docs/stacks/csharp/README.md`.
Reference material: AppHost, AppUi, Compute, and Persistence local manuals; build/package manifests; `docs/stacks/csharp/rails-and-effects.md`; `docs/stacks/csharp/platform/system-apis.md`.
Exit: runtime records, `Eff<RT,T>` ownership, channel/progress/receipt routing, cancellation/deadline behavior, allocation measurement, and observability identity are stated as C# execution policy without service-location wrappers or telemetry package catalogs.
Proof required: targeted Markdown link check plus source-owner review against referenced manuals and manifests.
Handoff: README route only when the new concept page is admitted.

Task:
ID: CSH-RUNTIME-020
Status: [QUEUED]
Milestone: CSH-RUNTIME-000
Work: author composition roots policy.
Target: `docs/stacks/csharp/composition-roots.md`, `docs/stacks/csharp/platform/build-and-packages.md`, `docs/usage/composition.md`.
Reference material: AppHost local manuals; central manifests; `docs/usage/composition.md`; `docs/stacks/csharp/platform/build-and-packages.md`.
Depends on: CSH-RUNTIME-010.
Exit: runtime-record mode, DI/container mode, scan registration, keyed services, decorator ordering, validation, options, Generic Host boundaries, and companion-root package state are represented as owner decisions without package-shaped pages.
Proof required: targeted Markdown link check plus source-owner review against referenced manuals, manifests, and usage owner.
Handoff: `docs/usage/composition.md` only when cross-stack composition ownership changes.

Task:
ID: CSH-RUNTIME-030
Status: [QUEUED]
Milestone: CSH-RUNTIME-000
Work: update cross-stack composition handoff.
Target: `docs/usage/composition.md`, `docs/stacks/csharp/README.md`.
Reference material: AppHost, AppUi, Compute, and Persistence local docs; `docs/usage/README.md`; central package manifests.
Depends on: CSH-RUNTIME-010, CSH-RUNTIME-020.
Exit: AppHost schedules, Compute executes, Persistence stores, AppUi observes, retry ownership, and Generic Host/DI boundaries route through usage and C# stack concept pages without copying project-local implementation sequence.
Proof required: targeted Markdown link check plus route review against usage docs, app-platform local docs, and central manifests.
Handoff: stack README route when the usage handoff changes reader navigation.

Task:
ID: CSH-RUNTIME-040
Status: [QUEUED]
Milestone: CSH-RUNTIME-000
Work: update C# stack routing for admitted runtime and composition pages.
Target: `docs/stacks/csharp/README.md`, `docs/stacks/csharp/AGENTS.md`.
Reference material: `docs/standards/reference/readme.md`, `docs/standards/agents-md.md`, `docs/standards/explanation/roadmap.md`, admitted runtime and composition pages.
Depends on: CSH-RUNTIME-010, CSH-RUNTIME-020.
Exit: README chooser and AGENTS sequencing trigger route admitted runtime/composition policy without copying sequence state into overlays or concept pages.
Proof required: roadmap standard reread, README standard reread, AGENTS standard reread, and targeted Markdown link check.

### [5.2][CSH_APP]

ID: CSH-APP-000
Outcome: application surface and durable-state policy becomes a coherent C# stack capability lane.
Why now: UI projection, reactive state, durable storage, and app-platform handoffs are related implementation policy but currently appear as separate page-shaped tasks.
Scope: `docs/stacks/csharp/reactive-ui-surfaces.md`, `docs/stacks/csharp/local-durable-state.md`, `docs/stacks/csharp/platform/build-and-packages.md`, AppUi/AppHost/Persistence/Compute local docs.
Included tasks: CSH-APP-010, CSH-APP-020, CSH-APP-030.
Completion rule: all included tasks are `[COMPLETE]`, and platform/build or app-platform handoffs are closed or routed away.

Task:
ID: CSH-APP-010
Status: [QUEUED]
Milestone: CSH-APP-000
Work: author reactive UI surfaces policy.
Target: `docs/stacks/csharp/reactive-ui-surfaces.md`, `docs/stacks/csharp/platform/build-and-packages.md`.
Reference material: AppUi project file, lockfile, local architecture, central package manifest, and `docs/stacks/csharp/platform/build-and-packages.md`.
Exit: UI scheduler, commands, live projection, retained UI, drawing, charting, controls, icons, dialogs, viewport overlays, and diagnostics are described as product concepts with toolkit, native asset, and lockfile/package-matrix facts routed to platform/build truth.
Proof required: targeted Markdown link check plus source-owner review against AppUi local docs, lockfile, and package manifest.
Handoff: platform/build package-state route only when graph admission changes.

Task:
ID: CSH-APP-020
Status: [QUEUED]
Milestone: CSH-APP-000
Work: author local durable state policy.
Target: `docs/stacks/csharp/local-durable-state.md`, `docs/stacks/csharp/platform/build-and-packages.md`.
Reference material: Persistence local manuals, central manifests, lockfiles when present, and `docs/stacks/csharp/platform/build-and-packages.md`.
Exit: `DbContext` lifetime, SQLite native initialization, PRAGMAs, migrations, downgrade, corruption, backup, snapshots, support artifacts, and live-state projection are represented as durable-state policy while preserving AppUi scheduler and Persistence ownership boundaries.
Proof required: targeted Markdown link check plus source-owner review against Persistence local manuals, manifests, and lockfiles.
Handoff: platform/build package-state route only when graph admission changes.

Task:
ID: CSH-APP-030
Status: [QUEUED]
Milestone: CSH-APP-000
Work: reconcile app-platform handoffs.
Target: AppUi, AppHost, Persistence, and Compute local docs plus `docs/stacks/csharp/README.md`.
Reference material: AppUi, AppHost, Persistence, and Compute local README, architecture, roadmap, and overlay files where present; `docs/stacks/csharp/README.md`.
Depends on: CSH-APP-010, CSH-APP-020.
Exit: reactive UI and durable-state pages point to app-platform owners only where reader action changes, and project-local implementation sequence remains in package roadmaps.
Proof required: targeted Markdown link check plus route review against app-platform local manuals and stack README.

### [5.3][CSH_PERF]

ID: CSH-PERF-000
Outcome: performance, diagnostics, and proof policy becomes an evidence-gated C# stack lane.
Why now: hot-path BCL guidance, profiling, source-generator diagnostics, and BenchmarkDotNet proof rails are currently candidates for overloaded system API or testing pages.
Scope: `docs/stacks/csharp/performance-diagnostics.md`, `docs/stacks/csharp/platform/system-apis.md`, `docs/stacks/csharp/testing/specialized-rails.md`.
Included tasks: CSH-PERF-010, CSH-PERF-020, CSH-PERF-030.
Completion rule: all included tasks are `[COMPLETE]`, and system API and testing proof-rail handoffs are closed or routed away.

Task:
ID: CSH-PERF-010
Status: [QUEUED]
Milestone: CSH-PERF-000
Work: author performance diagnostics policy.
Target: `docs/stacks/csharp/performance-diagnostics.md`.
Reference material: existing C# stack pages; `docs/stacks/csharp/platform/system-apis.md`; `docs/stacks/csharp/testing/specialized-rails.md`; source and tool owners.
Exit: span, SIMD, `SearchValues`, regex, AOT, source-generator failures, effect trace reading, allocation measurement, profiling guidance, and evidence-gated hot-path rules are represented as implementation policy without command catalogs.
Proof required: targeted Markdown link check plus source-owner review against existing stack pages and tool owners.

Task:
ID: CSH-PERF-020
Status: [QUEUED]
Milestone: CSH-PERF-000
Work: route overloaded system API content.
Target: `docs/stacks/csharp/platform/system-apis.md`, `docs/stacks/csharp/performance-diagnostics.md`.
Reference material: `docs/stacks/csharp/platform/system-apis.md`, `docs/stacks/csharp/performance-diagnostics.md`, `docs/standards/explanation/roadmap.md`.
Depends on: CSH-PERF-010.
Exit: BCL replacement guidance stays in system APIs, while performance-specific usage and proof policy move to or route through performance diagnostics.
Proof required: system API page reread, performance diagnostics page reread, and targeted Markdown link check.
Handoff: system API page only when content moves or route behavior changes.

Task:
ID: CSH-PERF-030
Status: [QUEUED]
Milestone: CSH-PERF-000
Work: connect BenchmarkDotNet and profiling proof rails.
Target: `docs/stacks/csharp/testing/specialized-rails.md`, `docs/stacks/csharp/performance-diagnostics.md`.
Reference material: `docs/stacks/csharp/testing/specialized-rails.md`, `docs/stacks/csharp/performance-diagnostics.md`, source and tool owners.
Depends on: CSH-PERF-010.
Exit: BenchmarkDotNet and specialized performance proof rails route through testing docs while performance diagnostics states only implementation-facing proof selection.
Proof required: testing specialized rails page, performance diagnostics page, source/tool owners, and targeted Markdown link check.
Handoff: testing specialized rails only when proof-rail mapping changes.

## [6][DEFERRED_DROPPED_CANCELED_WORK]

No terminal milestone or task records are currently retained.

## [7][VALIDATION]

[ROADMAP_SHAPE]:
- [ ] Boundaries appear before milestones; after milestones, only terminal lifecycle records and this validation section appear.
- [ ] Milestones use semantic `CSH-<AREA>-000` IDs and describe umbrella outcomes rather than executable page-writing tasks.
- [ ] Tasks use stable `CSH-<AREA>-NNN` IDs, bracketed `Status`, `Work`, `Target`, `Reference material` when task-specific context is required, `Exit`, and `Proof required`.
- [ ] `Reference material` points to context to read and does not imply target edits, dependency sequencing, completion proof, or handoff by itself.
- [ ] Acceptance checklists do not hide delegateable work; delegateable units have task IDs.

[PROOF_CLOSE]:
- [ ] A task moves to `[COMPLETE]` only when its `Exit` is satisfied and `Proof required` agrees with current source, command output, generated artifact, link check, review, or stated proof gap.
- [ ] A milestone moves to `[COMPLETE]` only when all included tasks are `[COMPLETE]`, and required handoffs are closed or routed away.
- [ ] `git diff --check -- docs/stacks/csharp docs/usage docs/README.md AGENTS.md` runs clean for changes that close a task.
- [ ] Removed or renamed paths touched by a task have a stale-route scan.
- [ ] Changed docs have a targeted Markdown link or anchor check, or the proof gap is stated.
- [ ] Changed roadmap, README, standards, and overlay files are re-read against their owning standards before task closure.
