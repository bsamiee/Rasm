# [CSHARP_OVERLAY_REFINER]

## [TRANSCRIPT]

I acted as refinement / CSharp Overlay Refiner for Rasm `AGENTS.md` work. I did not edit active repository files. I read and used:

- `CLAUDE.md`
- root project instructions supplied in the user message
- `docs/standards/README.md`
- `docs/standards/agents-md.md`
- `docs/standards/AGENTS.md`
- `docs/standards/proof.md`
- runtime `coding-csharp` skill summary
- memory registry entries for recent Rasm C# overlay, AppUi/package, Rhino, Grasshopper, Vectors, tests/bridge, and docs-standards context
- context reports:
  - `.reports/agents-md-050626/track-source-scans/02-csharp-large-context.md`
  - `.reports/agents-md-050626/track-source-scans/03-csharp-platform-context.md`
  - `.reports/agents-md-050626/track-source-scans/06-poly-csharp.md`
  - `.reports/agents-md-050626/track-source-scans/12-tests-bridge-context.md`
- Current overlays:
  - `libs/csharp/AGENTS.md`
  - `libs/csharp/Rasm/AGENTS.md`
  - `libs/csharp/Rasm.Rhino/AGENTS.md`
  - `libs/csharp/Rasm.Grasshopper/AGENTS.md`
  - `libs/csharp/Rasm.AppHost/AGENTS.md`
  - `libs/csharp/Rasm.AppUi/AGENTS.md`
  - `libs/csharp/Rasm.Compute/AGENTS.md`
  - `libs/csharp/Rasm.Persistence/AGENTS.md`
  - `libs/csharp/Rasm.Materials/AGENTS.md`

I built on context instead of repeating basic inventory. context already established folder size, current source owners, project graph status, package facts, and test/bridge boundaries. This report refines that into patch-ready overlay changes.

No static, test, bridge, or docs validation gates were run before this chat report because this task is a read-only planning/report task plus one requested `.reports/` write.

## [REFINEMENT_POSITION]

The current C# overlays are compact and mostly shaped correctly for `docs/standards/agents-md.md`: they define scope, read order, owner contracts, boundary rules, rejections, and route-away behavior without becoming command catalogs. The problem is not broad noncompliance. The problem is precision.

The patch should make each overlay self-executing for a fresh agent by replacing concern nouns with local trigger grammar:

- trigger: when changing or adding a specific behavior;
- owner: the exact local rail, algebra, receipt, runtime record, or projection to extend;
- replacement: the action that prevents helpers, wrappers, package facades, public knobs, compatibility surfaces, or sibling rails;
- route-away: where exact versions, package details, architecture state, implementation sequence, or runtime proof belong.

The future-only standard should remain explicit: current implementation lag is not a ceiling. No overlay should preserve baseline caveats, compatibility/deprecation surfaces, or scaffold disclaimers as design constraints. Present-state claims still route to README, architecture, roadmap, manifests, source, and proof gaps.

## [NESTED_OVERLAY_DECISION]

Nested `AGENTS.md` files would be technically justified in several large subtrees:

| [INDEX] | [SUBTREE]                            | [WHY_NESTED_COULD_HELP]                                                                                                           |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `libs/csharp/Rasm/Vectors/`          | Dense projection, sampling, spectral, receipt, and native-proof rail with its own `Vectors/_ARCHITECTURE.md`.                    |
|   [2]   | `libs/csharp/Rasm/Analysis/`         | Singular execution rail around `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run`.                                         |
|   [3]   | `libs/csharp/Rasm.Rhino/Commands/`   | Command input, point event, document transaction, mutation, and receipt rail are high-risk and locally specific.                  |
|   [4]   | `libs/csharp/Rasm.Rhino/UI/`         | Rhino/Eto dispatch, panels, overlays, motion, callbacks, and retained state form a full boundary family.                          |
|   [5]   | `libs/csharp/Rasm.Rhino/Blocks/`     | Block operations, lifecycle, archive/link semantics, attributes, graph, and native obsolete projections need exact owner grammar. |
|   [6]   | `libs/csharp/Rasm.Grasshopper/UI/`   | GH2 UI is large enough to own intent, mutation, document, layout, motion, paint, wire, and reflective host internals.             |
|   [7]   | `libs/csharp/Rasm.Materials/Bricks/` | Brick catalogue and scalar layout already have a roadmap and bounded-context grammar.                                             |

Recommended actionable route: do not add nested C# overlays in this patch. Densify the existing project-root overlays instead.

Reason: the project overlays are still short, already loaded by the root read order, and can carry precise trigger rules without exceeding the `AGENTS.md` role. Adding nested overlays now would create more instruction surfaces before the project-root trigger grammar has been fully exploited. Revisit nested overlays only after a project overlay grows past useful scan length or a subfolder gains an independent runtime/proof hazard that project-root rules cannot state cleanly.

Exception to watch: `libs/csharp/Rasm.Grasshopper/UI/` is the first likely nested-overlay candidate if future work keeps expanding GH2 UI rails. It has enough internal operation families that a future `UI/AGENTS.md` could be better than a dense project-root table.

## [PATCH_PLAN_PARENT_LIBS_CSHARP]

Target: `libs/csharp/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Lead and read order are good. Lines 3-11 keep root ownership and project overlay discovery clear.
- Library contract is strong but does not yet translate external-library-first into local C# library grammar.
- Extension grammar is too generic for package internalization and large-folder pressure.
- Project routing table is useful and should stay.
- Rejections are correct but should add package-facade and public-knob language.

### [PATCH_RECOMMENDATION]

In `[2][LIBRARY_CONTRACT]`, after the current typed FP/ROP boundary paragraph, add a platform/package internalization sentence:

- Approved external libraries are implementation surfaces that must disappear into the owning Rasm rail: operation algebra, runtime record, typed receipt, projection, capability record, source-owned table, or boundary capsule. Public APIs expose Rasm concepts, not package facades, provider knobs, option bags, or renamed native calls.

In `[3][EXTENSION_GRAMMAR]`, add two bullets:

- Package adoption: bind the package to the local operation algebra, runtime record, projection, or receipt before exposing a public surface; route exact version and package proof to manifests, project files, and architecture.
- Large owner folder: when a folder has multiple operation families or a source architecture/roadmap owner, name the singular rail to extend before adding public types, files, or sibling entrypoints.

In `[6][REJECTIONS]`, add one bullet:

- No public package-forwarding facade, provider selector, option bag, or wrapper API when a typed owner rail can internalize the dependency.

### [CONFIDENCE]

High. This is a family-level rule that is not duplicated by root because it translates root external-library-first policy into C# library behavior.

## [PATCH_PLAN_RASM]

Target: `libs/csharp/Rasm/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Scope is correct: foundational geometry kernel and concern library.
- Read order names `Domain/` and `Vectors/_ARCHITECTURE.md`, but misses the concrete `Analysis/Analyze.cs` execution rail and `Vectors/Intent.cs`.
- Extension grammar uses broad nouns: "analysis owner", "vector intent", "support-space projection". Fresh agents need exact rails.

### [PATCH_RECOMMENDATION]

In `[2][READ_ORDER]`, replace or extend the analysis and vector bullets:

- When changing `Analysis/`, read `Analysis/Analyze.cs` first; preserve `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run` as the singular execution rail.
- When changing `Vectors/` projection, sampling, receipt, or algorithm entrypoints, read `Vectors/_ARCHITECTURE.md` and `Vectors/Intent.cs` before adding public surface.

In `[3][EXTENSION_GRAMMAR]`, replace broad analysis/vector bullets with exact trigger grammar:

- Analysis behavior: extend `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run`; import `Domain` validation, statistics, coercion, and kind logic instead of adding analysis-local copies.
- Vector capability: extend `VectorIntent` factories and `Project<TOut>` dispatch unless `Vectors/_ARCHITECTURE.md` names a narrower owner rail such as field, cloud, mesh, matrix, sampling, flow, alignment, spectral, or atom projection.
- New numerical or symbolic behavior: use approved MathNet/CSparse/native BCL numeric surfaces directly inside the owning algorithm rail; do not expose library knobs or wrapper-only numeric APIs.
- Future concern category: create one category with one consumer surface and one typed rail over `Domain`; do not start with helper folders, caller-side switches, or parameter bags.

In `[5][REJECTIONS]`, add:

- No Vectors UI, preview, bake, GH2 parameter, command, product receipt, or host-specific public surface.
- No public fixed-kernel/provider knob unless it executes a materially different algorithm and is represented in typed intent or receipt data.

### [CONFIDENCE]

High for Analysis and Vectors rail naming because context source reads directly established the current rails. Medium for exact wording of the narrower Vectors owner list because the overlay should avoid becoming a copied architecture table.

## [PATCH_PLAN_RASM_RHINO]

Target: `libs/csharp/Rasm.Rhino/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Scope correctly identifies `Rasm.Rhino` as the canonical RhinoWIP boundary.
- Read order has host API proof routing.
- Extension grammar says "case-row rail" and "existing receipt vocabulary"; those are too memory-dependent.
- Routing table is useful but close to inventory; project-root overlay should turn it into action grammar.
- UI rules are directionally correct but do not name the root UI rails.
- Rejections ban stale public names but need an exact native-boundary exception for native obsolete values emitted by Rhino documents.

### [PATCH_RECOMMENDATION]

In `[3][EXTENSION_GRAMMAR]`, replace the command/mutation bullets with named rails:

- New command input, option, prompt, transform, selection, point event, gumball, or text mode: extend `CommandInputPolicy`, `CommandInputRequest<T>`, `CommandPointEventPhase`, `CommandOption`, or the existing command operation rail before adding a command-local helper.
- New document mutation: extend `DocumentOp`, `DocumentTransaction`, `DocumentEdit.Commit`, and `DocumentReceiptSlot`; apply redraw, commit, disposal, and UI-thread protection at the boundary edge.
- New block definition, instance, link, archive, attribute, graph, or preview behavior: extend `BlockOp`, `BlockInstanceTask`, `LinkLifecycle`, block attribute task, mutation receipt, and existing snapshot/cache owners before adding a facade.
- New UI behavior: extend `UiIntent<T>`, viewport preview/interaction, motion owner, panel/dialog owner, or drawing-resource owner before adding a second executor.
- New native return conversion: map nullable to `Option` or `Fin`, bool failure to typed failure, resource lifetime to scoped projection, and native document mutation to the owning receipt vocabulary.

In `[4][ROUTING]`, keep the table but make the command owner cells more exact, or replace the table with trigger bullets. If keeping the table, change the owner descriptions:

- Commands: `Commands/` command input, point events, `DocumentOp`, transactions, and document receipts.
- UI: `UI/` `UiIntent<T>`, Rhino/Eto dispatch, panels, overlays, motion, callbacks, and drawing lifetimes.
- Blocks: `Blocks/` `BlockOp`, lifecycle, archive/link, attributes, graph, preview, native boundary projection.

In `[5][UI_RULES]`, add exact rails:

- Rhino UI requests extend the existing UI intent and executor edge; callbacks, retained state, motion, and resource lifetimes stay inside `UI/` owner rails.
- Native UI events become typed events with native identity and scoped disposal; no public callback surface receives raw native lifecycle choreography.

In `[6][REJECTIONS]`, add the native-obsolete exception:

- Obsolete native values may be projected only when live Rhino documents can emit them; keep the projection inside the owning boundary rail with canonical public naming, never as compatibility aliases, stale public vocabulary, or deprecation wrappers.

### [CONFIDENCE]

High for Commands, UI, Blocks, and Construction precision. Medium-high for keeping the routing table: a table remains acceptable if each row is action-oriented and not an architecture summary.

## [PATCH_PLAN_RASM_GRASSHOPPER]

Target: `libs/csharp/Rasm.Grasshopper/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Scope is strong and already states downstream intent clearly.
- Read order says "typed UI rail" but does not name `IUiOp<TResult>`, `GrasshopperUiIntent<T>`, `GhUi`, `DocumentMutation`, or `WireRepositoryRail`.
- Boundary rules are useful but broad.
- Rejections are correct; they need exact replacements for duplicate GH2 UI/wire/mutation paths.

### [PATCH_RECOMMENDATION]

In `[2][READ_ORDER]`, split UI reads by trigger:

- When changing GH2 UI operation dispatch, scope resolution, or UI-thread behavior, read the `IUiOp<TResult>`, `GrasshopperUiIntent<T>`, `GhUi`, and `GrasshopperUi.Use` owners in `UI/`.
- When changing document mutation, undo, repaint, action commit, snapshot, placement, clipboard, or wire mutation behavior, read `DocumentMutation`, `DocumentMutationReceipt`, `DocumentMutationDelta`, and the shared mutation runner in `UI/`.
- When changing wire query, edit, route, overlay, diagnostics, or reflective GH2 wire internals, read `WireOp` and `WireRepositoryRail`.

In `[3][EXTENSION_GRAMMAR]`, replace broad UI/mutation bullets:

- New GH2 UI request family: implement an `IUiOp<TResult>` case and surface it through `GrasshopperUiIntent<T>` and `GhUi`; do not add a separate public executor, thread-marshalling path, or caller-side host sequence.
- New document mutation: extend `DocumentMutation` and run it through the shared mutation rail so undo, repaint, action commit, `DocumentMutationReceipt`, `DocumentMutationDelta`, and snapshots stay one owner.
- New wire behavior: extend `WireOp`, wire query/edit/result cases, route synthesis, overlay, diagnostics, or `WireRepositoryRail`; do not add a second reflection capsule for GH2 wire internals.
- New component capability: extend `ComponentSpec`, spec builder, `PortKind`, `OutputBinding`, conversion, diagnostics, and ownership transfer before adding one-off parameter or component code.
- New motion/layout/paint behavior: extend the existing motion, layout, paint, and resource-lifetime owners; avoid per-feature timers, event polls, and paint hooks.

In `[4][BOUNDARY_RULES]`, keep the table but add rail names to the UI and Components rows:

- Components row should name `ComponentSpec`, spec builder, ports, output binding, conversion, diagnostics, ownership transfer.
- UI row should name `IUiOp<TResult>`, `GrasshopperUiIntent<T>`, `GhUi`, `DocumentMutation`, `WireOp`, `WireRepositoryRail`, undo, repaint, snapshots.

In `[5][REJECTIONS]`, add:

- No public GH2 executor beside `GhUi` and the existing UI intent rail.
- No direct host-internal wire reflection outside `WireRepositoryRail`.
- No mutation path that bypasses the shared mutation runner and receipt/delta rail.
- No bridge scenario that drives raw `Grasshopper2.*` when the wrapper route can express the host behavior.

### [CONFIDENCE]

High. context established the exact GH2 UI and wire rails, and the current overlay only needs precision upgrades.

## [PATCH_PLAN_RASM_APPHOST]

Target: `libs/csharp/Rasm.AppHost/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Lead routes state and sequence correctly, but it does not put the highest-risk invariant at the opening edge.
- Owner contract is strong: one runtime record and one capability boundary.
- Extension grammar is good but needs package internalization for runtime platform dependencies.
- Rejections forbid public `IServiceProvider` and companion packages in-process, but stop behavior is missing.

### [PATCH_RECOMMENDATION]

In the lead or `[2][OWNER_CONTRACT]`, add the highest-risk invariant:

- `Rasm.AppHost` is an in-process runtime-record rail; Generic Host, DI container roots, Scrutor scanning, Serilog sinks, OpenTelemetry SDK/exporters, and companion service packages are companion/test/bridge composition only unless architecture and host proof move them into the graph.

In `[3][EXTENSION_GRAMMAR]`, add package internalization:

- Platform package behavior: internalize Channels/Dataflow, LanguageExt schedules, TimeProvider/NodaTime, logging abstractions, DiagnosticSource/OpenTelemetry API, HTTP resilience, and cancellation/drain behavior into `RasmRuntime`, lifecycle rails, scheduler policy, and typed receipts before adding service registries, config knobs, or facade methods.
- Cross-capability contract: represent runtime handoffs as typed records and receipts; do not leak AppUi, Compute, or Persistence package types through AppHost public surface.

Add `[6][STOP_RULES]`:

- If proceeding requires in-process Generic Host, public `IServiceProvider`, OTel SDK/exporter, Serilog sink, raw host SDK call, unproved shutdown/drain behavior, or companion package adoption without architecture and host proof, stop and route to `ARCHITECTURE.md`, `docs/host-libraries.md`, and manifest proof.

### [CONFIDENCE]

High. This follows context platform context and directly aligns AppHost with the position-ring requirement that highest-risk constraints lead or close.

## [PATCH_PLAN_RASM_APPUI]

Target: `libs/csharp/Rasm.AppUi/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Lead and owner contract correctly route package state and implementation sequence.
- Package internalization is present but too general for AppUi's package-heavy surface.
- Host/native embedding hazards are present in architecture/roadmap but not captured as local stop behavior.
- Rejections are good but should explicitly reject public toolkit settings bags.

### [PATCH_RECOMMENDATION]

In `[2][OWNER_CONTRACT]`, add local package internalization:

- Avalonia, ReactiveUI, DynamicData, LiveCharts, Skia, DialogHost, icons, and behavior packages are internal implementation surfaces. Public AppUi surfaces expose product shell, screen, command, live-view, chart, visual, scheduler, and diagnostic concepts.

In `[3][EXTENSION_GRAMMAR]`, add:

- New package-backed UI behavior: encode it as product intent, `Screen<T>`, command receipt, diagnostic receipt, live-view projection, chart/visual projection, or `RasmUiScheduler`; do not expose toolkit settings bags or package-specific option records.
- New host embedding behavior: prove embedded top-level creation, native focus, software-rendering path, panel lifecycle, and disposal ordering in architecture/runtime proof before adding surface.

In `[5][REJECTIONS]`, add:

- No public toolkit settings bag, package facade, or per-package screen abstraction.
- No AppUi-owned host mutation, repaint, undo, panel lifecycle, or scheduler bypass.

Add `[6][STOP_RULES]`:

- If SkiaSharp native-major proof, GH2 panel-host API proof, Avalonia app-delegate disabling, embeddable top-level creation, native focus/disposal ordering, or software-rendering embedding proof is missing, stop and route to `ARCHITECTURE.md`, `ROADMAP.md`, manifest proof, or host runtime proof before expanding AppUi.

### [CONFIDENCE]

High. AppUi is the package-heavy project where external library internalization matters most, and context established the exact native hazards.

## [PATCH_PLAN_RASM_COMPUTE]

Target: `libs/csharp/Rasm.Compute/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Scope and owner contract correctly position Compute over `Rasm.Vectors`.
- Read order misses the shared-contracts route for `ComputeRequest`, `.proto`, gRPC codegen, and remote payloads.
- Extension grammar correctly rejects algorithm reimplementation but does not yet internalize ONNX/gRPC/reactive/performance packages into Compute rails.
- Stop behavior is missing.

### [PATCH_RECOMMENDATION]

In `[1][READ_ORDER]`, add:

- Before changing `ComputeRequest`, `.proto`, gRPC codegen, remote payloads, or companion remote contracts, read the shared-contracts owner where present; if no owner exists, stop and record the owner as a proof gap instead of defining the contract inside Compute.

In `[3][EXTENSION_GRAMMAR]`, add package internalization:

- ONNX Runtime/CoreML EP, gRPC, System.Reactive, CommunityToolkit.HighPerformance, UnitsNet, TensorPrimitives, and model/remote packages become substrate, model, remote, progress, allocation, tolerance, and receipt behavior inside `ComputeIntent` and `ExecutionReceipt`; do not expose provider knobs or package API shapes as the Compute API.
- New remote lane: use companion contracts and typed receipts; AppHost owns retry/scheduling/drain and Persistence owns durable artifacts.
- New model lane: preserve substrate selection, lifecycle lease, native resolver proof, model receipt, and benchmark receipt as typed fields.

In `[5][REJECTIONS]`, add:

- No `ComputeRequest` or gRPC schema owner inside Compute if the shared-contracts owner is absent.
- No public ONNX, CoreML, gRPC, reactive, benchmark, or provider selector knobs outside typed intent and receipt rails.

Add `[6][STOP_RULES]`:

- If native model loading, `libonnxruntime.dylib` resolution, CoreML fp16 equivalence, remote retry ownership, shared-contracts ownership, benchmark methodology, or cache artifact ownership is unproved, stop and route to architecture, AppHost, Persistence, or shared-contract proof.

### [CONFIDENCE]

Medium-high. The shared-contracts owner is a proof gap rather than a current file path, so the patch must phrase it as "where present" and stop on absence.

## [PATCH_PLAN_RASM_PERSISTENCE]

Target: `libs/csharp/Rasm.Persistence/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- This is the strongest platform overlay.
- Owner contract, boundary table, and stop rules are already good.
- Missing piece is explicit public-boundary rejection for provider APIs and option bags.
- Package internalization can be strengthened without listing versions.

### [PATCH_RECOMMENDATION]

In `[3][EXTENSION_GRAMMAR]`, add:

- Provider package behavior: internalize EF Core SQLite, Microsoft.Data.Sqlite, SQLitePCLRaw, source-generated JSON, MessagePack/LZ4, NodaTime, DynamicData/System.Reactive, and redaction packages inside lifecycle, query, snapshot, live-state, and support-artifact rails; public callers see store operations, queries, app-state projection, and receipts.

In `[5][REJECTIONS]`, replace or extend existing provider rejection:

- No public `DbContext`, `DbContextOptions`, `IQueryable`, `SqliteConnection`, serializer option bag, codec selector, provider selector, EF entity type, or migration helper crosses the Persistence boundary.
- No generic repository wrapper, long-lived context, provider facade, or public storage knob when `StoreLifecycleOp`, `StoreQuery<TResult>`, `StoreDispatch`, `AppState`, `SnapshotEnvelope`, and `StoreReceipt` can carry the behavior.

Keep `[6][STOP_RULES]` mostly as-is, with one optional addition:

- Include serializer/source-generation drift and native SQLite initialization in the same stop list if the architecture treats them as proof-sensitive.

### [CONFIDENCE]

High. The overlay already has the correct shape; this is a small precision patch.

## [PATCH_PLAN_RASM_MATERIALS]

Target: `libs/csharp/Rasm.Materials/AGENTS.md`.

### [CURRENT_SECTION_ASSESSMENT]

- Scope is strong: host-free catalogue plus scalar layout data.
- Read order routes source standards and `Bricks/ROADMAP.md`.
- Extension grammar already rejects parallel enum families and host geometry leakage.
- External-library internalization should not be forced here; Materials is source-standard-first and typed-catalogue-first.
- Stop behavior is missing for unproved source-standard values.

### [PATCH_RECOMMENDATION]

In `[2][READ_ORDER]`, refine the source-standard bullet:

- When changing catalogue values, regional policy, dimensional basis, layout constraints, or source routes, read the source standards of record and preserve typed citation routes in code documentation or catalogue records.

In `[3][EXTENSION_GRAMMAR]`, add:

- Source standards are evidence surfaces, not instruction authority. New source facts enter typed catalogue records, closed vocabularies, unions, scalar layout records, and code documentation with source routes; they do not enter overlay prose.
- New generated or imported catalogue input: translate it into the owning typed vocabulary and query rail; do not add generated schema, JSON, SQL, or stringly citation surfaces.

Add `[6][STOP_RULES]`:

- If source-standard proof is missing for a new regional policy, catalogue value, dimensional rule, layout constraint, or citation route, stop and route to source proof or code documentation instead of adding heuristic values.

### [CONFIDENCE]

High. Materials should remain host-free and package-light; the patch should resist importing platform package language that belongs to AppUi/AppHost/Compute/Persistence.

## [CROSS_OVERLAY_RULES_TO_APPLY]

Use these edits consistently, but do not paste a generic block into every file.

1. Replace broad nouns with exact rails.
   - Preferred: "extend `DocumentMutation` and run it through the shared mutation rail."
   - Rejected: "route through mutation rail."

2. Internalize external libraries.
   - Preferred: "package behavior becomes typed operation, receipt, runtime record, projection, source table, or boundary capsule."
   - Rejected: public package facades, provider knobs, option bags, renamed native APIs.

3. Preserve future-only posture.
   - Preferred: "the overlay states the target owner rail and stop condition."
   - Rejected: baseline caveats, partial-adoption excuses, deprecation wrappers, transitional aliases.

4. Keep exact facts out of overlays unless they change action.
   - Exact package versions, native API signatures, project state, and implementation sequence belong to manifests, source, README, architecture, roadmap, or proof routes.

5. Use boundary exceptions only when exact.
   - Obsolete native values may be projected only where the native runtime can emit them and only inside the owning boundary rail.
   - No compatibility APIs, public aliases, or stale names follow from that projection.

6. Add stop rules for runtime/native/package proof hazards.
   - AppHost: runtime composition and shutdown/drain.
   - AppUi: native embedding and package-native proof.
   - Compute: native model, remote contracts, benchmark/cache proof.
   - Persistence: provider/native/encryption/corruption/snapshot proof.
   - Materials: source-standard proof.
   - Rhino/Grasshopper: host API semantics and bridge-owned runtime behavior.

7. Do not add ordinary validation ladders.
   - Parent/root quality policy and tool READMEs already own command syntax.
   - Local overlays only name selector, proof hazard, route, or stop condition where parent guidance cannot infer it.

## [SECTION_BY_SECTION_PATCH_ORDER]

Recommended implementation order for the future patch:

1. `libs/csharp/AGENTS.md`
   - Add package internalization family rule.
   - Add large-owner trigger rule.
   - Add package-facade rejection.

2. `libs/csharp/Rasm/AGENTS.md`
   - Name `Analysis/Analyze.cs`, `IAspect`, `Operation<TGeometry,TOut>`, `Analyze.Run`.
   - Name `Vectors/Intent.cs`, `VectorIntent`, `Project<TOut>`.
   - Add no-host/product-surface rejection for Vectors.

3. `libs/csharp/Rasm.Rhino/AGENTS.md`
   - Add construction read trigger.
   - Replace "case-row rail" with named command/document rails.
   - Add Blocks and UI rail precision.
   - Add native-obsolete projection exception.

4. `libs/csharp/Rasm.Grasshopper/AGENTS.md`
   - Name UI intent/op/public dispatcher.
   - Name document mutation and wire reflection rails.
   - Add component/motion precision.
   - Add duplicate executor/reflection/mutation bypass rejections.

5. `libs/csharp/Rasm.AppHost/AGENTS.md`
   - Put in-process runtime-record invariant at the lead.
   - Add runtime package internalization.
   - Add stop rules.

6. `libs/csharp/Rasm.AppUi/AGENTS.md`
   - Add package internalization.
   - Add native embedding/package proof stop rules.
   - Reject public toolkit bags/facades.

7. `libs/csharp/Rasm.Compute/AGENTS.md`
   - Add shared-contracts read/stop rule.
   - Add model/remote/substrate package internalization.
   - Add native model/remote/benchmark/cache stop rules.

8. `libs/csharp/Rasm.Persistence/AGENTS.md`
   - Add provider package internalization.
   - Add explicit no-public-provider-API rejection.
   - Optionally strengthen stop rules for serializer/source-gen/native initialization.

9. `libs/csharp/Rasm.Materials/AGENTS.md`
   - Add source-standard proof stop rule.
   - Add generated/imported source translation rule.
   - Keep host-free and package-light.

## [EXPECTED_PATCH_SHAPE]

The eventual patch should be sentence-level and section-level, not a wholesale rewrite. Estimated scope:

| [INDEX] | [FILE]                                   | [PATCH_SIZE] | [RISK] |
| :-----: | :--------------------------------------- | :----------- | :----- |
|   [1]   | `libs/csharp/AGENTS.md`                  | small        | low    |
|   [2]   | `libs/csharp/Rasm/AGENTS.md`             | medium       | low    |
|   [3]   | `libs/csharp/Rasm.Rhino/AGENTS.md`       | medium       | medium |
|   [4]   | `libs/csharp/Rasm.Grasshopper/AGENTS.md` | medium       | medium |
|   [5]   | `libs/csharp/Rasm.AppHost/AGENTS.md`     | medium       | low    |
|   [6]   | `libs/csharp/Rasm.AppUi/AGENTS.md`       | medium       | low    |
|   [7]   | `libs/csharp/Rasm.Compute/AGENTS.md`     | medium       | medium |
|   [8]   | `libs/csharp/Rasm.Persistence/AGENTS.md` | small        | low    |
|   [9]   | `libs/csharp/Rasm.Materials/AGENTS.md`   | small        | low    |

Validation for that future patch should be docs-only unless the patch changes source, manifests, links, diagrams, or tooling claims. The likely gate is `git diff --check -- libs/csharp/AGENTS.md libs/csharp/Rasm*/AGENTS.md`. Do not claim C# static, test, or bridge proof for instruction-only edits.

## [CONFIDENCE]

High confidence on the overall route: project-root overlay densification is the best next action, with no new nested overlays in this patch.

High confidence on `Rasm`, `Rhino`, `Grasshopper`, `AppHost`, `AppUi`, `Persistence`, and `Materials` recommendations because context source reads and current overlay lines directly support the gaps.

Medium-high confidence on `Compute` recommendations because the shared-contracts owner is identified as required by architecture but is not a current concrete overlay path. The patch must express that as a read/stop rule, not as a present-file claim.

Medium confidence on exact final wording because the eventual patch should preserve the concise local style of each overlay and avoid overfitting to this report. The recommendations above are precise enough to implement without another research pass.
