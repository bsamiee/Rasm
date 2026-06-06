# [CSHARP_01_LARGE_CONTEXT]

## [TRANSCRIPT]

I acted as context / CSharp Large Folders Context agent for the C# library overlays. I did not edit active repository files. I read the governing chain first: `CLAUDE.md`, root `AGENTS.md` from the user-provided project instructions, `docs/standards/README.md`, `docs/standards/agents-md.md`, and `docs/standards/AGENTS.md`. I also loaded the `coding-csharp` skill because the task reviews C# production-folder behavior.

I then read the four requested overlays in full with line numbers:

- `libs/csharp/AGENTS.md`
- `libs/csharp/Rasm/AGENTS.md`
- `libs/csharp/Rasm.Rhino/AGENTS.md`
- `libs/csharp/Rasm.Grasshopper/AGENTS.md`

I inspected folder shape and representative owner files rather than running validation, because this is a read-only _reports/report task. Source reads focused on:

- `libs/csharp/Rasm/Analysis/Analyze.cs`
- `libs/csharp/Rasm/Domain/Geometry.cs`
- `libs/csharp/Rasm/Vectors/Intent.cs`
- `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md`
- `libs/csharp/Rasm.Rhino/Events.cs`
- `libs/csharp/Rasm.Rhino/Capture.cs`
- `libs/csharp/Rasm.Rhino/Commands/Input.cs`
- `libs/csharp/Rasm.Rhino/Commands/Document.cs`
- `libs/csharp/Rasm.Rhino/Blocks/State.cs`
- `libs/csharp/Rasm.Rhino/Blocks/Operations.cs`
- `libs/csharp/Rasm.Rhino/UI/Intent.cs`
- `libs/csharp/Rasm.Rhino/UI/Overlay.cs`
- `libs/csharp/Rasm.Rhino/Construction/ROADMAP.md`
- `libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj`
- `libs/csharp/Rasm.Grasshopper/Components/Component.cs`
- `libs/csharp/Rasm.Grasshopper/Components/Port.cs`
- `libs/csharp/Rasm.Grasshopper/UI/Ui.cs`
- `libs/csharp/Rasm.Grasshopper/UI/Document.cs`
- `libs/csharp/Rasm.Grasshopper/UI/Motion.cs`
- `libs/csharp/Rasm.Grasshopper/UI/Wire.cs`

I used memory only as a targeting aid for known Rhino/GH2/Vectors owner names, then verified findings against current files. The report findings below cite current repository files.

## [FOLDER_CONTEXT]

The three requested project folders are large enough that vague category guidance is insufficient. Current production C# inventory under `libs/csharp/Rasm`, `libs/csharp/Rasm.Rhino`, and `libs/csharp/Rasm.Grasshopper` is `45,323` lines.

Folder fanout from direct inventory:

| [INDEX] | [FOLDER]                                  | [CS_FILES] | [OBSERVED_ROLE]                                                               |
| :-----: | :---------------------------------------- | ---------: | :---------------------------------------------------------------------------- |
|   [1]   | `libs/csharp/Rasm/Vectors`                |         13 | Dense vector/numerics intent, projection, receipts, spectral substrate.       |
|   [2]   | `libs/csharp/Rasm/Analysis`               |         12 | Analysis aspect algebra over `Operation<TGeometry,TOut>` and `Analyze.Run`.   |
|   [3]   | `libs/csharp/Rasm.Grasshopper/UI`         |         11 | GH2 UI intent surface, mutation rails, layout, motion, paint, wire internals. |
|   [4]   | `libs/csharp/Rasm.Rhino/UI`               |          8 | Rhino/Eto UI intent, overlay, interaction, panel, paint, motion.              |
|   [5]   | `libs/csharp/Rasm.Rhino/Exchange`         |          7 | File/archive/sheet/publish import-export owner.                               |
|   [6]   | `libs/csharp/Rasm.Rhino/Commands`         |          6 | Command input, selection, document mutation and receipts.                     |
|   [7]   | `libs/csharp/Rasm.Rhino/Camera`           |          5 | Camera/viewport state and operations.                                         |
|   [8]   | `libs/csharp/Rasm.Grasshopper/Components` |          5 | Component spec, port, binding, bridge, plugin catalog.                        |
|   [9]   | `libs/csharp/Rasm/Domain`                 |          4 | Shared context, validation, geometry kind/coercion, stats.                    |
|  [10]   | `libs/csharp/Rasm.Rhino/Blocks`           |          4 | Block definitions, linked archive lifecycle, operations, state.               |

Project manifests confirm the intended boundary layering: `Rasm.Grasshopper` references only `Rasm` and declares GH2 usings in `Rasm.Grasshopper.csproj:1-17`; `Rasm.Rhino` references only `Rasm` and declares Rhino command/display/input usings in `Rasm.Rhino.csproj:1-19`; `Rasm` is the RhinoCommon-aware geometry foundation and owns MathNet/CSparse dependencies in `Rasm.csproj:1-24`.

The source already uses the future-forward implementation posture the overlays should defend:

- `Rasm.Analysis` has a singular aspect/execution rail: `IAspect` returns `Operation<TGeometry,TOut>` in `Analysis/Analyze.cs:5-17`, `Operation<TGeometry,TOut>` owns rejected/per-item/aggregate execution in `Analysis/Analyze.cs:31-86`, and `Analyze.Run` converts that rail into `Validation<Error, Seq<TOut>>` in `Analysis/Analyze.cs:89-160`.
- `Rasm.Domain` owns reusable geometry shape and coercion truth: `Kind` maps native geometry families in `Domain/Geometry.cs:84-119`, `GeometryKernel` centralizes capability predicates in `Domain/Geometry.cs:241-265`, and owned/borrowed curve/surface/brep coercion lives in `Domain/Geometry.cs:330-405`.
- `Rasm.Vectors` has a single consumer rail: `VectorIntent` owns many capability cases and dispatches through `Project<TOut>` in `Vectors/Intent.cs:3-80`; `_ARCHITECTURE.md` states `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail and `ExtractionDomain + SampleKind` is the only sampling/extraction rail in `Vectors/_ARCHITECTURE.md:217-220`.
- `Rasm.Rhino` root owners are real, not just prose: `Events.cs` owns `WatchPayload`, `WatchPhase`, `EventDispatcher`, and `WatchIdle` in `Events.cs:8-120` and `Events.cs:334-428`; `Capture.cs` owns `CaptureArea`, `CaptureCodec`, `CaptureResult`, and `CaptureRecipe` configuration in `Capture.cs:9-120` and `Capture.cs:300-368`.
- `Rasm.Rhino.Commands` is not just "commands": `CommandInputPolicy`, `CommandPointConstraint`, point-event binding, script/native input, and read-loop behavior are in `Commands/Input.cs:107-120`, `Commands/Input.cs:527-599`, and `Commands/Input.cs:880-1040`; document mutations and receipts are in `Commands/Document.cs:6-148` and `Commands/Document.cs:536-707`.
- `Rasm.Rhino.Blocks` is a full operation algebra: `BlockOp`, `BlockInstanceTask`, `LinkLifecycle`, and `TableMutation` are in `Blocks/Operations.cs:12-120`; native definition mutation, content indexing, linked archive placeholder creation, and attribute field operations live in `Blocks/Operations.cs:438-540` and `Blocks/Operations.cs:1420-1568`.
- `Rasm.Grasshopper.UI` has a singular UI intent surface: `GrasshopperUiIntent<T>` owns policy, bind, repaint, and `IUiOp<T>` conversion in `UI/Ui.cs:281-330`; `GhUi` is the public typed dispatcher in `UI/Ui.cs:451-502`; `GrasshopperUi.Use` resolves scope and marshals execution onto the UI thread in `UI/Ui.cs:510-644`.
- `Rasm.Grasshopper.UI` has a singular mutation and receipt rail: `DocumentMutationDelta` and `UndoGroup` are in `UI/Ui.cs:225-253`; `DocumentMutation` owns target/compose/drop/place/clipboard/wire/document mutation cases in `UI/Document.cs:945-1030`; `RunDocumentMutation` is the common rail called across `Document.cs` and `Wire.cs` (`rg` hits include `UI/Canvas.cs:1156`, `UI/Document.cs:299`, `UI/Document.cs:1434`, `UI/Wire.cs:864`).
- GH2 wire internals are deliberately one capsule, not open host poking: `WireOp` owns query/select/edit/route/install/overlay/diagnostics in `UI/Wire.cs:595-641`, and `WireRepositoryRail` owns the reflective `WireDrawCache`/`WireData` boundary in `UI/Wire.cs:1139-1279`.

## [FINDINGS]

[HIGH] `libs/csharp/Rasm/AGENTS.md` under-specifies the real `Analysis` and `Vectors` singular rails.

The overlay says analysis behavior should extend "the analysis owner, operation rail, and aspects" in `libs/csharp/Rasm/AGENTS.md:21-24`, but it does not name the actual rail: `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run`. The source shows these are the stable extension points in `Analysis/Analyze.cs:5-17`, `Analysis/Analyze.cs:31-86`, and `Analysis/Analyze.cs:89-160`. The same issue exists for Vectors: `libs/csharp/Rasm/AGENTS.md:23` lists many vector concern nouns, but the folder architecture says `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail and `ExtractionDomain + SampleKind` is the only sampling/extraction rail in `Vectors/_ARCHITECTURE.md:217-220`.

Why this matters: future agents can add a new analysis convenience API or a parallel vector projection type while technically following the current broad wording. The overlay should make the correct extension rail explicit.

[HIGH] `libs/csharp/Rasm.Rhino/AGENTS.md` passively lists `Construction/ROADMAP.md` but does not trigger-read it for the work it owns.

The routing table says `Construction` is `Construction/ROADMAP.md` until production code lands in `libs/csharp/Rasm.Rhino/AGENTS.md:37-40`. The roadmap is much stronger than a placeholder: it defines `RhinoConstruction.Project<TOut>(ConstructionOp op, Context context)` as the future public rail in `Construction/ROADMAP.md:49-62`, names the three-file target shape in `Construction/ROADMAP.md:75-88`, and routes existing command/document/overlay/block/camera facts through that future owner in `Construction/ROADMAP.md:102-114`.

Why this matters: an agent adding geometry factory, annotation, framed-bounds, block-ready, document-ready, or preview-ready behavior could stay in `Commands/Document.cs`, `UI/Overlay.cs`, or `Blocks` because the overlay does not state a condition-driven read rule. That is a likely source of duplicated native construction adapters.

[HIGH] `libs/csharp/Rasm.Rhino/AGENTS.md` needs concrete command/document mutation rails, not just category labels.

The routing table points to "`Commands/` staged execution and document mutation" in `libs/csharp/Rasm.Rhino/AGENTS.md:30-35`, and the extension grammar has a vague "case-row rail" rule in `libs/csharp/Rasm.Rhino/AGENTS.md:24`. Source truth is more precise:

- Command input policy and outcome rail are in `Commands/Input.cs:107-120`.
- Point constraints and point-event phases are in `Commands/Input.cs:527-599` and `Commands/Input.cs:880-919`.
- Native/scripted input and the read loop are in `Commands/Input.cs:921-1040`.
- Document mutation is `DocumentOp` plus `DocumentEdit.Commit`, not ad hoc table calls, in `Commands/Document.cs:6-148` and `Commands/Document.cs:576-707`.
- `DocumentReceiptSlot` is the receipt vocabulary in `Commands/Document.cs:536-544`.

Why this matters: "case-row rail" is meaningful to someone with memory of prior refactors, but not self-executing for a fresh agent. The overlay should name `CommandInputPolicy`, `CommandInputRequest<T>`, `DocumentOp`, `DocumentTransaction`, `DocumentEdit.Commit`, and `DocumentReceiptSlot` as the rails to extend before adding new command/document mutation shapes.

[MEDIUM] `libs/csharp/Rasm.Rhino/AGENTS.md` bans compatibility nouns but does not distinguish forbidden compatibility APIs from necessary native-obsolete boundary projection.

The overlay bans stale names and compatibility nouns in `libs/csharp/Rasm.Rhino/AGENTS.md:50-56`. That is directionally correct, but `Blocks/State.cs` intentionally preserves Rhino's obsolete native `InstanceDefinitionUpdateType.Embedded` at the boundary: `UpdatePolicy.Embedded` is declared with a boundary comment in `Blocks/State.cs:395-405`, and `FromNative` maps the obsolete native value because old documents can still report it in `Blocks/State.cs:413-421`.

Why this matters: a ruthless cleanup pass could misread the overlay and delete a necessary native-document semantic. The rule should say obsolete native values may be projected only inside the owning boundary rail, with canonical public naming and no compatibility alias surface.

[HIGH] `libs/csharp/Rasm.Grasshopper/AGENTS.md` does not name the central `GrasshopperUiIntent<T>` / `IUiOp<TResult>` surface.

The overlay says UI work should read `UI/` to find the typed UI rail in `libs/csharp/Rasm.Grasshopper/AGENTS.md:13-18`, and says new GH2 UI request families extend the UI operation algebra and typed intent factory in `libs/csharp/Rasm.Grasshopper/AGENTS.md:21-25`. Current source has a specific shape: every operation implements `IUiOp<TResult>` and becomes a `GrasshopperUiIntent<TResult>` through `UI/Ui.cs:451-502`; `GrasshopperUiIntent<T>` owns policy merge, bind, repaint, and execution in `UI/Ui.cs:281-330`; `GrasshopperUi.Use` owns scope resolution and UI-thread marshalling in `UI/Ui.cs:586-619`.

Why this matters: "typed UI rail" is not enough in an 11-file, 12k-plus-line UI folder. The overlay should prohibit new public UI entrypoints unless they become `IUiOp<TResult>` cases surfaced through `GhUi` or an existing intent factory.

[HIGH] `libs/csharp/Rasm.Grasshopper/AGENTS.md` underspecifies document mutation and wire ownership, which are the highest-risk duplicate rails.

The overlay says new mutation behavior routes through "the mutation rail with undo, repaint, action commit, and snapshot behavior" in `libs/csharp/Rasm.Grasshopper/AGENTS.md:21-24`, but does not name `DocumentMutation`, `DocumentMutationReceipt`, `UiRail.RunDocumentMutation`, or `DocumentMutationDelta`. Source shows the rail:

- `DocumentMutationDelta` and `UndoGroup` live in `UI/Ui.cs:225-253`.
- `DocumentMutation` owns mutation cases in `UI/Document.cs:945-1030`.
- `DocumentMutation.Apply` converts cases to `DocumentMutationReceipt` in `UI/Document.cs:983-1030`.
- `WireOp` routes wire edits, batch edits, route synthesis, overlays, and diagnostics through one union in `UI/Wire.cs:595-641`.
- `WireRepositoryRail` is the one reflective boundary for GH2 wire draw internals in `UI/Wire.cs:1139-1279`.

Why this matters: duplicate document mutations, separate wire repository reflection, or direct `DocumentMethods` calls outside the mutation rail would violate the one-owner posture but are not directly forbidden by the current overlay.

[MEDIUM] Parent `libs/csharp/AGENTS.md` is concise, but its project-routing table is too weak for large folders without nested overlays or stronger trigger rules.

`libs/csharp/AGENTS.md` correctly states root policy ownership and library-family behavior in `libs/csharp/AGENTS.md:1-18`, and its project table routes to local overlays in `libs/csharp/AGENTS.md:27-40`. The folder facts show multiple large owner subtrees: `Rasm/Vectors` has 13 files, `Rasm/Analysis` 12, `Rasm.Grasshopper/UI` 11, `Rasm.Rhino/UI` 8, `Rasm.Rhino/Exchange` 7, and `Rasm.Rhino/Commands` 6.

Why this matters: nearest overlays stop at project root. For these large folders, the project overlays must either carry concrete trigger-driven rails or delegate to nested `AGENTS.md` files. The current overlays do some of this, but the gaps above show the project-root overlay is not yet enough for fresh-agent large-folder work.

[LOW] Some overlay route-away wording could unintentionally preserve implementation facts in `AGENTS.md`.

`libs/csharp/AGENTS.md:40` says use co-located `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` for project state, package adoption, file architecture, and implementation sequence. That is correct route-away behavior. However, `libs/csharp/Rasm.Rhino/AGENTS.md:28-40` and `libs/csharp/Rasm.Grasshopper/AGENTS.md:27-35` still contain category tables that are close to architecture summaries. They are not wrong, but they should stay action-oriented: "When changing X, extend Y first" beats category inventory.

## [RECOMMENDED_OVERLAY_CHANGES]

`libs/csharp/AGENTS.md`:

- Add a large-folder rule: when a project subfolder has an architecture/roadmap/source owner or more than one operation family, the local project overlay must name the singular rail to extend before new public types or files.
- Add a project-level rejection: no folder-local "new API" without first proving it is not an extension of an existing `Operation`, `Intent`, `Op`, `Receipt`, `Policy`, `Spec`, or `Mutation` rail.

`libs/csharp/Rasm/AGENTS.md`:

- Add to read order: when changing `Analysis/`, read `Analysis/Analyze.cs` first to preserve `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run` as the singular execution rail.
- Add to read order: when changing `Vectors/`, read `Vectors/_ARCHITECTURE.md` plus `Vectors/Intent.cs` before adding projection, sampling, receipt, or algorithm entrypoints.
- Replace the broad vector extension bullet with a concrete rule: new vector capability extends `VectorIntent` factories and `Project<TOut>` dispatch unless the architecture names an owner-local rail such as `FieldNabla`, `SampleKind`, `CloudKernel.MassOf`, `LaplacianCache`, `SpectralFilter`, or `AtomProjection`.
- Add a rejection: no `Rasm.Vectors` UI, preview, command, GH2 parameter, bake, or product receipt surface; `_ARCHITECTURE.md:231-234` and `_ARCHITECTURE.md:346` already carry the product-boundary truth.

`libs/csharp/Rasm.Rhino/AGENTS.md`:

- Add a condition-driven construction read rule: when adding geometry creation, annotation, framed-bounds, transform/frame, block-ready, document-ready, or preview-ready output, read `Construction/ROADMAP.md` and route through its intended `RhinoConstruction.Project<TOut>` / `ConstructionOp` shape until production code supersedes the roadmap.
- Replace "case-row rail" with named command rails: `CommandInputPolicy`, `CommandInputRequest<T>`, `CommandPointEventPhase`, `CommandOption`, `DocumentOp`, `DocumentTransaction`, `DocumentEdit.Commit`, and `DocumentReceiptSlot`.
- Add a blocks rule: new block definition, instance, link, archive, attribute, graph, or preview behavior extends `BlockOp`, `BlockInstanceTask`, `LinkLifecycle`, `BlockAttributeTask`, `MutationReceipt`, and the existing snapshot/cache owners before adding another facade.
- Add a native-obsolete exception rule: preserve obsolete native values only as boundary projections when live Rhino documents can emit them; do not expose them as compatibility aliases or stale public vocabulary.
- Add a UI specificity rule: new overlay, panel, dialog, resource, motion, or gumball behavior extends `UiIntent<T>`, `UiViewportPreview`, `UiViewportInteraction<TState>`, `MotionSpec`/motion owner, or panel owner before adding a parallel UI executor.

`libs/csharp/Rasm.Grasshopper/AGENTS.md`:

- Add a UI surface rule: every new UI operation must extend an existing `IUiOp<TResult>` union/factory and flow through `GrasshopperUiIntent<T>` and `GhUi`; no separate public executor or direct thread-marshalling surface.
- Add a mutation rule: document mutations extend `DocumentMutation` and must run through `UiRail.RunDocumentMutation` so undo, repaint, `ActionList`, `DocumentMutationReceipt`, `DocumentMutationDelta`, and snapshots remain one rail.
- Add a wire rule: wire behavior extends `WireOp`, `WireEdit`, `WireQuery`, `WireResult`, or `WireRepositoryRail`; no second reflective GH2 wire repository reader and no direct host-internal wire access outside that capsule.
- Add a component rule: component capability extends `ComponentSpec`, `SpecBuilder`, `OutputBinding`, `PortKind`, and `Capability` before adding one-off component parameter code. Current source proves those owners in `Components/Component.cs:31-77`, `Components/Component.cs:200-319`, and `Components/Port.cs:11-95`.
- Add a motion rule: cosmetic animation, haptics, display-link pacing, spring configuration, and accessibility-driven motion reduction extend `UI/Motion.cs` owners. `MotionClock`, `MotionAccessibility`, haptic pattern, spring configuration, cosmetic animation, and `Pacer` are already owner-local in `UI/Motion.cs:25-130`, `UI/Motion.cs:377-445`, and `UI/Motion.cs:2030-2315`.

## [UNIVERSAL_PATTERNS_TO_CODIFY]

- Large C# overlays should name exact local rails, not concern nouns. "Extend `DocumentMutation` through `UiRail.RunDocumentMutation`" is useful; "route through mutation rail" is too memory-dependent.
- A category table is acceptable only when paired with trigger grammar: "When adding X, read Y and extend Z before creating W."
- One owner rail per concern should be stated as a positive extension rule and a rejection pair. Example: "Wire repository facts extend `WireRepositoryRail`; no other reflection capsule reads GH2 wire internals."
- Roadmaps can be active owner routes when production code has not landed. The overlay must name the condition that triggers the roadmap read, or agents will treat it as background.
- Boundary exceptions must be exact. Obsolete or internal host API values may be projected when the native runtime can emit them, but only inside the owner boundary and never as compatibility APIs, aliases, or stale vocabulary.
- Receipt families should remain owner-local and typed. Collapse repeated mutation buckets into existing slot/fact streams, but do not genericize algorithm receipts into ledgers or reported-value wrappers.
- Host SDK claims should stay evidence-gated: RhinoWIP/GH2 XML, decompile evidence, and the repo API rail precede implementation claims; bridge scenarios own native runtime behavior when static managed proof is insufficient.
- Large folder overlays should be reviewed for nested `AGENTS.md` need when a subfolder has 5+ production files and multiple durable operation families. If no nested overlay is added, the project overlay must carry stronger trigger-driven rails.

## [CONFIDENCE]

High for the source-backed owner facts and overlay gaps above. The current source directly proves the named rails, and the requested overlays do not name several of them.

Medium for the recommendation to add nested overlays. The folder size and rail complexity justify it, but the repo may prefer project-root overlays with denser trigger grammar instead.

No validation commands were run because this task was read-only research and report writing. The only file written is this `_reports/` report.
