# [H1][RASM_RHINO_COMMAND_RAIL_PLAN]
>**Dictum:** *Native-quality Rhino commands require one staged command rail, not manual prompt chains.*

<br>

[IMPORTANT] This plan was captured on `2026-05-19` against RhinoWIP `9.0.26132.12306`, `scripts/rhino.sh api doctor`, local `RhinoCommon.xml`, local source under `libs/csharp/Rasm.Rhino`, and McNeel command help. Future sessions must refresh `scripts/rhino.sh api doctor` and record every official help page URL used before implementation.

---
## [1][DECISION]
>**Dictum:** *One command graph absorbs prompt, option, preview, mutation, and result flow.*

<br>

Build a single staged command rail under `RasmCommand<TSelf>`. The public command lifecycle remains `RasmCommand<TSelf>`; do not add a second command base class, runner, manager, adapter, or compatibility layer.

The missing capability is a `CommandGraph<TState>` value rail with typed `PromptStage<TState, TValue>` stages and `DocumentEdit`-owned transaction semantics. It must remove this current downstream burden:

- Manual sequences of `context.Get<T>()`.
- Manual state carrying between prompts.
- Manual option persistence and option-driven re-prompting.
- Manual dynamic preview and gumball wiring.
- Manual stage undo/backtracking.
- Manual document mutation sequencing, undo records, redraws, and `Result.Success` conversion.

Keep the graph rail in `Commands/Command.cs` until the compressed command file would exceed `350` LOC or the graph declarations alone exceed `175` LOC. Create `Commands/Graph.cs` only when that threshold is crossed, and keep it to one public graph rail plus directly owned transition and event types. Document transaction, operation, and receipt types stay owned by `DocumentEdit` in `Commands/Document.cs`.

[CRITICAL] Do not create `MultiStageCommand`, `RasmStagedCommand`, `CommandWorkflow`, `CommandRunner`, `CommandService`, `StageInput`, or a second `RunStages` override.

---
## [2][CURRENT_STATE]
>**Dictum:** *Existing rails are strong primitives, not a native command framework.*

<br>

| [INDEX] | [AREA] | [CURRENT OWNER] | [STATUS] |
| :-----: | :----- | :-------------- | :------- |
| **1** | Command Lifecycle | `Commands/Command.cs` | `RunCommand -> Ready -> Run -> Complete -> FailureResult` is correct and stays canonical. |
| **2** | Context | `Commands/Context.cs` | `RhinoCommandContext` exposes `Document`, `Input`, `Edit`, `Ui`, view lookup, and active scope. |
| **3** | Single Input | `Commands/Input.cs` | `CommandInputs.Get<T>` covers object, point, text, scalar, native geometry, option, box, and color. |
| **4** | Options | `Commands/Options.cs` | `CommandOption` binds and captures per-getter options; it does not persist cross-stage state. |
| **5** | Selection | `Commands/Selection.cs` | `CommandSelection.Reference` preserves `ObjRef`, component, pick, view, and geometry access. |
| **6** | Mutation | `Commands/Document.cs` | `DocumentEdit` mutates object geometry/state under undo/protect, but lacks transaction receipts. |
| **7** | Preview | `UI/Overlay.cs` | `UiViewportPreview`, `UiGumball`, and overlay lifetimes exist but sit beside command input. |
| **8** | UI | `UI/Dialog.cs`, `UI/Panel.cs`, `UI/Pages.cs`, `UI/Ui.cs` | Common dialogs/status/progress/panels/pages exist, with specific missing native breadth below. |

Current code can implement multi-stage commands through manual `Fin` chains. It does not provide a no-boilerplate staged modal command surface.

---
## [3][TARGET_RAIL]
>**Dictum:** *Graph state drives every prompt, option, preview, transition, and commit.*

<br>

### [3.1][PUBLIC_SHAPE]

The target public authoring surface is one erased stage rail over the existing generic `CommandInputRequest<TValue>` / `CommandGet<TValue>` pair:

```csharp
public sealed record CommandGraph<TState>(
    TState Initial,
    Seq<CommandStage<TState>> Stages,
    Func<CommandCommitContext<TState>, Fin<Result>> Commit,
    CommandGraphEvents<TState> Events = default);

public abstract record CommandStage<TState>(string Name) {
    internal abstract Fin<PromptTransition<TState>> Run(CommandStageContext<TState> context);
}

public sealed record PromptStage<TState, TValue>(
    string Name,
    Func<TState, CommandInputRequest<TValue>> Input,
    Func<TState, Seq<CommandInputPolicy>> Policies,
    Func<CommandStageContext<TState>, CommandGet<TValue>, Fin<PromptTransition<TState>>> Receive,
    Func<PromptPreviewContext<TState>, Option<UiViewportPreview>> Preview,
    Func<PromptGumballContext<TState>, Fin<Option<PromptTransition<TState>>>> Gumball) : CommandStage<TState>(Name);
```

`CommandStage<TState>` may change from abstract record to interface only if analyzers require it. The invariant is fixed: `CommandGraph<TState>.Stages` stores one erased stage owner, and each concrete prompt stage preserves `TValue` through `Receive`. Document commit planning uses `DocumentEdit` transaction types; the graph invokes them, but does not own a second mutation rail.

### [3.2][TRANSITIONS]

Model command results as graph events:

| [INDEX] | [EVENT] | [GRAPH RESPONSE] |
| :-----: | :------ | :--------------- |
| **1** | `GetResult.Option` | Apply option state transition, then return `Stay`, `Next`, `Back`, or `Commit` by explicit stage policy. |
| **2** | `GetResult.Undo` | `Back` when history exists; otherwise `Stay` with prompt feedback or `Cancel` by stage policy. |
| **3** | `GetResult.Nothing` / `EnterWhenDone` | `Commit` only when stage state is complete. |
| **4** | Object / point / scalar / geometry result | `Next`, `Stay`, or `Commit` by stage transition. |
| **5** | `Cancel` / `ExitRhino` | `Fault.Cancelled`. |

### [3.3][INVARIANTS]

- Prompt stages never mutate `RhinoDoc`.
- All document writes pass through `DocumentEdit` transaction semantics.
- One command commit creates one undo boundary and one redraw policy.
- Prompt `Back` reverts graph state, option state, preview, and active getter state before commit; Rhino document undo/history begins only inside `DocumentEdit` transaction commit.
- Options remain `CommandOption` / `CommandOptionValue`.
- Selection remains `CommandSelection` / `CommandSelection.Reference`.
- Preview and gumball lifetimes are scoped to the active prompt stage.
- `Fin<T>` carries failures; `Option<T>` carries absence only.
- Scripted and interactive modes use the same graph; providers differ, graph semantics do not.

---
## [4][FINDING_MATRIX]
>**Dictum:** *Every reviewed gap has one owner and one rail.*

<br>

[CRITICAL] First implementation scope is findings `1-9`. Findings `10-23` remain mandatory backlog items, but they must wait until the graph, input/preview/options, selection target, transaction, and transform-session rails build cleanly.

| [INDEX] | [PRIORITY] | [FINDING] | [OWNER] | [ACTION] |
| :-----: | :--------: | :-------- | :------ | :------- |
| **1** | Critical | Multi-stage command prompting is manual. | `Command.cs` or `Commands/Graph.cs` with `RasmCommand<TSelf>` executor. | Add `CommandGraph<TState>` and stage interpreter. |
| **2** | Critical | Command-time dynamic preview/gumball input is detached from `GetPoint`. | `Input.cs`, `Overlay.cs`; `Mouse.cs` remains UI callback only. | Extend point input to host stage preview, dynamic draw, post-draw, mouse, and gumball events through existing preview/gumball rails. |
| **3** | Critical | Document mutation lacks transaction plan and receipt. | `Document.cs`. | Add transaction, operation, and receipt types inside `DocumentEdit`; graph commits invoke `context.Edit`. |
| **4** | Critical | Stage undo/history is shallow. | `CommandGraph`, `Document.cs`. | Add graph state history and transaction receipts; verify Rhino `HistoryRecord` XML before coding object history. |
| **5** | High | Options lack cross-stage persistence and dynamic visibility. | `Options.cs`, `Input.cs`, `CommandGraph`. | Treat options as state lenses; derive values, visibility, enabled state, bounds, and list contents from `TState`. |
| **6** | High | Prompt state is not first-class. | `CommandGraph`, `Input.cs`. | Each stage owns prompt, defaults, accept modes, constraints, options, preview, and next transition. |
| **7** | High | Subobject and grip selection are captured but not modeled. | `Selection.cs`, `Input.cs`, `Document.cs`. | Preserve `CommandSelection.Reference` metadata; add missing projection and mutation behavior without duplicating selection target cases. |
| **8** | High | Transform commands only apply finished transforms. | `Input.cs`, `Document.cs`, `Overlay.cs`. | Add transform session over `GetTransform`, target resolution, preview/gumball transform, copy policy, and final transaction. |
| **9** | Medium | Scripted mode parity is under-modeled. | `CommandGraph`, `Ui.cs`, `Dialog.cs`. | Add scripted token provider over the same graph; keep interactive UI blocked only where native UI is required. |
| **10** | Critical | Document table operations are missing. | `Document.cs`; optional `Document.Tables.cs` after LOC proof. | Extend `DocumentEdit` operation algebra for layers, groups, linetypes, materials, dim styles, and hatch patterns. |
| **11** | Critical | Blocks and instance definitions are missing. | `Document.cs`; optional `Document.Blocks.cs` after LOC proof. | Extend `DocumentEdit` operation algebra for instance definitions and instance object insertion. |
| **12** | Critical | Object attributes and metadata cannot be modified on existing targets. | `Document.cs`. | Add `DocumentEdit.Attributes(DocumentTarget, Func<ObjectAttributes, Fin<ObjectAttributes>>, bool quiet)`. |
| **13** | Critical | Query-driven document targets are missing. | `Document.cs`. | Add `DocumentTarget.Filter(ObjectEnumeratorSettings)` and query projection rail. |
| **14** | High | View/layout/viewport editing is missing. | `Document.cs` / `Context.cs`; optional `Document.Views.cs` after LOC proof. | Extend document operations for document layouts, named views, named CPlanes, viewport camera/projection, zoom, redraw control, and display modes. |
| **15** | High | File open/import/export/save is missing. | `Document.cs` / `Context.cs`; optional `Document.Files.cs` after LOC proof. | Extend document operations for open/import/export/save/write; keep `UiIntent.File` as picker and scripted token resolver only. |
| **16** | Medium | Static `RhinoGet` geometry bypasses option loop. | `Input.cs`. | Fold custom geometry getters into the existing getter loop where native getter classes exist. |
| **17** | Medium | Native distance/angle interactive input is missing. | `Input.cs`. | Add one scalar measure rail that can express distance and angle without parallel `CommandDistance` / `CommandAngle` wrappers. |
| **18** | Medium | Annotation and hatch creation are not first-class. | `Document.cs`, `Tables.cs`. | Add annotation/hatch document ops using table-aware styles. |
| **19** | Medium | Purge/undelete lifecycle is missing. | `Document.cs`. | Add one lifecycle operation rail, not separate purge/undelete helpers. |
| **20** | Medium | Layer dialog only supports single-layer selection. | `Dialog.cs`. | Extend `UiLayerSpec` into one layer dialog rail with single/multiple/material modes. |
| **21** | Medium | Panels miss selected-tab, dock ids, and events. | `Panel.cs`. | Extend `PanelSnapshot` and `PanelOp` without adding a second panel registry. |
| **22** | Medium | Pages are base classes only. | `Pages.cs`. | Add page registration metadata rail for plugin page collections. |
| **23** | Low | Additional named native dialogs are absent. | `Dialog.cs`. | Add only where they reduce ceremony through existing `UiIntent<T>`; generic `Dialog` remains escape hatch. |

---
## [5][IMPLEMENTATION_PHASES]
>**Dictum:** *Build the missing rail first, then widen capability through existing owners.*

<br>

### [5.1][PHASE_0_BASELINE]

Run before edits:

```bash
git diff --stat
git diff --numstat
wc -l libs/csharp/Rasm.Rhino/Commands/*.cs libs/csharp/Rasm.Rhino/UI/*.cs
rg -n "^(public|internal|file|private|protected).*" libs/csharp/Rasm.Rhino
scripts/rhino.sh api doctor
```

Record `RhinoCommon.xml` paths and exact API members touched. Do not run RhinoWIP runtime bridge checks.

Native API evidence gate:

1. Run `scripts/rhino.sh api doctor` and paste the RhinoWIP version plus XML status into the implementation notes.
2. For every RhinoCommon member touched, capture exact XML evidence with `scripts/rhino.sh api xml rhino-common "<member-or-type-pattern>"`.
3. For every Rhino.UI member touched, use `scripts/rhino.sh api xml rhino-ui "<member-or-type-pattern>"` when XML exists; otherwise use `scripts/rhino.sh api decompile rhino-ui "<fully-qualified-type>"`.
4. Required first-pass evidence: `Rhino.Input.Custom.GetPoint`, `Rhino.Input.Custom.GetTransform`, `Rhino.DocObjects.HistoryRecord`, `Rhino.DocObjects.ObjectEnumeratorSettings`, `Rhino.DocObjects.Tables.ObjectTable`, `Rhino.RhinoDoc` file operations, and any `Rhino.UI` dialog, panel, or gumball type changed.
5. Do not implement an API-backed rail until exact constructor, method, property, disposal, and return semantics are recorded.

### [5.2][PHASE_1_COMMAND_GRAPH]

Implement `CommandGraph<TState>`, `CommandStage<TState>`, `PromptStage<TState, TValue>`, `PromptTransition<TState>`, `CommandStageContext<TState>`, `PromptPreviewContext<TState>`, `PromptGumballContext<TState>`, `CommandCommitContext<TState>`, and `CommandGraphEvents<TState>`. Keep `DocumentTransaction`, `DocumentOp`, and `DocumentReceipt` in `Document.cs` under `DocumentEdit` ownership.

Instructions:

1. Keep `RasmCommand<TSelf>` as the sole lifecycle owner.
2. Add a protected graph execution method to `RasmCommand<TSelf>`.
3. Keep `Run(RhinoCommandContext context)` as the only override point.
4. Store state history for `Back`.
5. Map graph completion to existing `Complete` / `FailureResult` behavior.
6. Reject empty stage lists and invalid transitions through `Fin.Fail`.

### [5.3][PHASE_2_INPUT_PREVIEW_OPTIONS]

Refactor `Input.cs` so graph stages can consume one getter event without owning native getter disposal.

Instructions:

1. Keep `CommandInputs.Get<T>` as the native getter factory.
2. Expose a stage-level event result that preserves `CommandGet<T>`, option changes, undo, nothing, and cancel.
3. Add `PromptPreviewContext<TState>` carrying persistent graph state plus transient getter, current point, viewport, dynamic draw args, post-draw args, and candidate geometry.
4. Add point-stage support for `GetPoint` dynamic draw, mouse move/down, post-draw, `FullFrameRedrawDuringGet`, no-redraw-on-exit, `Constrain*`, `PermitTabMode`, `PermitFromOption`, `PermitElevatorMode`, `AddSnapPoint(s)`, and `AddConstructionPoint(s)` after XML verification.
5. Reuse `UiViewportPreview`, `OverlayDecision`, `UiGumball`, and `UiPreviewScope`; do not create callback files or route command-stage logic through `RasmMouseCallback`.
6. Add `PromptGumballContext<TState>` so pick, drag, change, accept, and cancel events can produce `PromptTransition<TState>` through the active stage.
7. Add option state lens behavior inside `CommandOption` / `CommandOptionPolicy`: stable option key, persistence seed, scripted token update, value projection, state update, visibility, enabled state, bounds, list derivation, and re-registration when dynamic option shape changes.

### [5.4][PHASE_3_SELECTION_TARGETS]

Extend selection without flattening into object IDs.

Instructions:

1. Preserve `CommandSelection.Reference.Use` as the `ObjRef` ownership boundary.
2. Do not add `DocumentTarget` cases that only restate selection metadata.
3. Thread `ObjRef` serial reconstruction, component index, grip owner/object identity, grip index/state, detail/page space, pick point, curve/surface parameters, view id, preselected state, and preselect-to-postselect transitions into graph state.
4. Add grip mutation only through `DocumentTarget` and `DocumentEdit`; do not expose raw grip mutation helpers.

### [5.5][PHASE_4_DOCUMENT_TRANSACTION]

Move command commits from manual edit chains to transaction plans.

Instructions:

1. Extend `DocumentEdit.Mutate` to execute a `Seq<DocumentOp>` under one undo/protect boundary.
2. Keep `DocumentOp`, `DocumentTransaction`, and `DocumentReceipt` owned by `DocumentEdit`; `CommandGraph` only invokes `context.Edit`.
3. Return `DocumentReceipt` with created, replaced, deleted, transformed, selected, hidden, locked, attribute-changed, table-changed, and lifecycle-changed IDs.
4. Add attribute mutation for existing targets.
5. Add lifecycle operations for delete, purge, undelete, hide, lock, reveal, and selection state.
6. Add history only after verifying native `HistoryRecord` and `TransformWithHistory` signatures in local XML.

### [5.6][PHASE_5_TRANSFORM_SESSION]

Build native transform behavior as graph specialization, not a second command rail.

Instructions:

1. Add `CommandInputs.Get<Transform>` through `GetTransform` when local XML confirms exact shape.
2. Add transform stage policy for source targets, base/reference frames, copy/delete-original, and preview transform.
3. If native `GetTransform` is narrower than required, build the session from existing point/object stages rather than adding a second transform rail.
4. Include source targets, base/reference/current frames, live preview transform, copy/delete-original, attributes/history policy, and final receipt shape.
5. Commit through `DocumentEdit` transaction operations and `DocumentEdit.Transform`.
6. Feed gumball snapshot transforms into the same transaction path.

### [5.7][PHASE_6_DOCUMENT_BREADTH]

Add structurally justified files only where a concern would otherwise bloat `Document.cs`.

Start by extending `DocumentEdit`, `DocumentTarget`, and the document operation algebra in `Document.cs`. Split only after the first implementation scope is green and only when `Document.cs` would exceed `350` LOC after compression.

Backlog split candidates:

| [INDEX] | [FILE] | [OWNER] | [ENTRY_CRITERIA] |
| :-----: | :----- | :------ | :--------------- |
| **1** | `Commands/Document.Tables.cs` | `DocumentEdit` / `DocumentOp` partial ownership for resource tables. | XML evidence for each table API and no viable compressed `Document.cs` ownership. |
| **2** | `Commands/Document.Blocks.cs` | `DocumentEdit` / `DocumentOp` partial ownership for instance definitions. | XML evidence for instance definition APIs and one document operation rail. |
| **3** | `Commands/Document.Views.cs` | `DocumentEdit` / `DocumentOp` partial ownership for document views, layouts, and display modes. | XML/decompile evidence for view/layout APIs and one document operation rail. |
| **4** | `Commands/Document.Files.cs` | `DocumentEdit` / `DocumentOp` partial ownership for file operations. | XML evidence for `RhinoDoc` read/import/export/write APIs and one document operation rail. |

Each file must expose no parallel public rail. Access remains through `RhinoCommandContext.Edit`. Do not add services, managers, helpers, adapters, registries, compatibility layers, or legacy surfaces.

### [5.8][PHASE_7_UI_GAPS]

Extend existing UI files only.

Instructions:

1. `Dialog.cs`: extend file picking with scripted picker token resolution only; document open/import/export/save belongs to `DocumentEdit` operations. Extend the layer dialog rail for single, multiple, and material modes. Add named native dialogs only when they reduce ceremony through `UiIntent<T>` and are used by graph/UI flows.
2. `Panel.cs`: add selected-tab state, dock id snapshot, panel show/close events, and dock-bar query through `PanelOp`.
3. `Pages.cs`: add plugin settings-page and properties-page registration metadata. Do not mix these UI pages with document layouts.
4. `Ui.cs`: keep scripted-mode gating centralized; do not duplicate interactive checks in each intent.

---
## [6][NATIVE_COMMAND_COVERAGE]
>**Dictum:** *Native command families define acceptance criteria.*

<br>

| [INDEX] | [COMMAND FAMILY] | [REQUIRED COVERAGE] |
| :-----: | :--------------- | :------------------ |
| **1** | `Line`, `Polyline` | Repeated point stages, conditional options, undo/back, rubber-band preview, tangent/ortho construction state. |
| **2** | `Circle`, `Arc`, `Ellipse` | Multi-point constructors, scalar options, constraints, curve/surface snap references, dynamic curve preview. |
| **3** | `ExtrudeCrv`, `Sweep1`, `Loft` | Ordered selections, profile/rail phases, seam adjustment, delete-input/history policy, preview/commit split. |
| **4** | `Fillet`, `Blend`, `Chamfer` | Pick-location semantics, curve parameter/end intent, radius/continuity options, trim/join commit. |
| **5** | `Move`, `Rotate`, `Scale`, `Orient` | Target selection, base/reference/current frames, copy policy, live transform preview, gumball transform commit. |
| **6** | `BooleanUnion`, `BooleanDifference` | Separate selection sets, delete/cutter policy, diagnostics for normals, open objects, nonmanifold topology. |
| **7** | `Offset` | Side-pick intent, approximate preview vs exact commit, corner/cap/tolerance policy. |
| **8** | `Trim`, `Split`, `Join` | Cutter/target phases, view projection, subobject chains/loops, join tolerance reporting. |
| **9** | `MatchSrf` | Edge-only selection, continuity options, target direction flip, topology-changing preview. |
| **10** | `SoftMove`, `CageEdit`, `FlowAlongSrf` | Grip/control-object lifecycle, falloff state, deformation preview, preserve-structure policy. |

Implementation is incomplete until the graph can express each family without manual state, option, preview, gumball, undo, and commit plumbing in command bodies.

---
## [7][VALIDATION]
>**Dictum:** *Static proof guards behavior without Rhino runtime claims.*

<br>

Run after each implementation phase:

```bash
dotnet build libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj --no-restore
pnpm check:cs
dotnet format Workspace.slnx --verify-no-changes --severity warn --no-restore
git diff --check
```

Run final residue checks:

```bash
rg -n "MultiStageCommand|RasmStagedCommand|CommandWorkflow|CommandRunner|CommandService|StageInput|Helper|Util|Manager|Adapter|Compat|Legacy" libs/csharp/Rasm.Rhino
rg -n "DocumentTables|DocumentBlocks|DocumentViews|DocumentFiles|CommandDistance|CommandAngle|PromptStageContext|parallel scripted|parallel gumball" libs/csharp/Rasm.Rhino
rg -n "public .*\\(" libs/csharp/Rasm.Rhino
git diff --stat -- libs/csharp/Rasm.Rhino
git diff --numstat -- libs/csharp/Rasm.Rhino
wc -l libs/csharp/Rasm.Rhino/Commands/*.cs libs/csharp/Rasm.Rhino/UI/*.cs
```

[CRITICAL] Do not run `scripts/rhino.sh bridge doctor`, `bridge check`, `bridge script`, `bridge load-smoke`, RhinoWIP runtime checks, or runtime test projects unless runtime validation is explicitly reopened.

---
## [8][REJECTION_RULES]
>**Dictum:** *The plan fails when a second rail appears.*

<br>

Reject implementation if any item appears:

- Second command base class.
- Separate option state model parallel to `CommandOption`.
- Raw `ObjRef` ownership outside `CommandSelection.Reference.Use`.
- Preview conduit duplicated outside `UiViewportPreview` / `RasmOverlay`.
- Document mutation outside `DocumentEdit`.
- Parallel scripted graph or parallel gumball event model.
- Table, block, view, file, or display-mode rails exposed outside `RhinoCommandContext.Edit`.
- One method per native command family.
- Helper, utility, manager, adapter, compatibility, or legacy-named types.
- Public overload families where one graph/stage/policy rail can infer behavior.
- Net LOC growth without explicit capability proof and subsequent collapse pass.
