# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Live-host probes and tool-gated arms are tracked as blocked tasks against the substrate that unblocks them.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[EMBED-PROBE]-[BLOCKED]: live Rhino embedding proves the shared AppUi GPU seam.
- Capability: live-host validation for `SurfaceHost.RhinoPanel`, `EmbedCapsule`, `SurfaceScheduler.Affinity`, and `RenderGraph.Lease` against the Rhino-owned AppKit run-loop and GPU pipeline.
- Shape: one `SurfaceSeam`-bound GPU lease carries `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi`, `GRMtlBackendContext`/`GRVkBackendContext`, and `SKRuntimeEffect` into the `GpuBackend` `RenderTargetFactory` rows without per-host render arms.
- Unlocks: meshlet, path-trace, live `Taa`/`Fsr`, and embedded-panel frame-receipt proof while preserving the CPU/2D-Skia fallback already de-risked against a windowed `GRContext`.
- Anchors: `.planning/Shell/hosts.md#07research`, `.planning/Render/viewport.md#09research`, `.api/api-avalonia-skia.md`, `.api/api-skiasharp.md`, `.api/api-avalonia-gpu-interop.md`, `SurfaceSeam.OnUiThread`, `GpuBackend`, and `RenderTargetFactory`.
- Tension: no integrated running RhinoWIP host surface exists yet, so pump coexistence, resize sync, render-backend contention, and host-owned shared-context spelling remain blocked.

[RENDER_LAYEREDBSDF_PATHTRACE]-[QUEUED]: Rasm.AppUi/Render owns the path tracer that shades FROM the LayeredBsdf.Sample/Evaluate/Pdf, the SlabStack lowering, and the SurfaceShade graph sink, never re-deriving lobe math — the renderer boundary the bsdf and graph pages name as the downstream consumer at the PATH_TRACE seam.
- Capability: The app-platform path-tracer consuming the one LayeredBsdf the SlabStack.ToLayered produces (post-split surface#OPENPBR_SLAB) and the SurfaceShade the MaterialGraph.Evaluate sink assembles, driving the per-bounce world ray through ShadingFrame.ToWorld and the MIS-balanced lobe sample, with the position-free multi-scatter random walk as the high-fidelity path over the Kulla-Conty fast path, and the SPECTRAL_REFLECTANCE_GROUNDING per-wavelength conductor curve as the high-fidelity conductor path.
- Shape: A Rasm.AppUi/Render integrator reading the Materials LayeredBsdf/SlabStack/SurfaceShade at the wire/runtime boundary and shading the scene, the consumer end of the Appearance/bsdf and Appearance/graph -> Render boundary seams.
- Unlocks: The renderer shades every material as a weighting of the closed seven-lobe set with zero per-material code, the OpenPBR slab stack lowers to one LayeredBsdf the integrator reads, and the position-free multi-scatter route admits as the high-fidelity path.
- Anchors: bsdf.md the renderer shades from LayeredBsdf and never re-derives lobe math (line 3), the path-tracer at the Rasm.AppUi/Render/viewport#PATH_TRACE seam; graph.md the SurfaceShade sink shaded by the path tracer; ARCHITECTURE.md [02]-[SEAMS] the bsdf/graph -> Render boundary (lines 51-52).
- Ripple: counterpart of `Rasm.Materials` `[SPECTRAL_REFLECTANCE_GROUNDING]` idea + `[BSDF_PAGE_SPLIT]` task (path tracer shades from `LayeredBsdf`/`SurfaceShade`).

[APPUI_VIEWPORT_SILHOUETTE_CONSUMPTION]-[QUEUED]: Confirm the AppUi Render drafting Viewport2D consumes the Fabrication HiddenLineResult silhouette edge set as a first-class style class once the CSG watertight-silhouette arm lands, so the two-sided consumption seam carries the silhouette set the projection page produces and the drafting page already tags as an emphasized EdgeStyle.
- Capability: AppUi Render/drafting Viewport2D composes the Fabrication Posting/projection HiddenLineSeam delegate running Hlr.Solve and reads the world-space (Visible, Hidden, Silhouette) Edge3 sets (owner.md FabricationResult.HiddenLineResult line 88), tagging each with an EdgeStyle; the drafting page already declares the silhouette-emphasized style. The ripple confirms the consumption contract for the silhouette set the CSG arm deepens: when the CSG_WATERTIGHT_SILHOUETTE arm composes the kernel arrangement to produce exact watertight-solid outlines, the Viewport2D silhouette-emphasized style consumes that improved set with no AppUi change, the seam staying a pure consumption boundary.
- Shape: A Rasm.AppUi Render/drafting.md page note confirming the Viewport2D HiddenLineSeam consumes the Silhouette Edge3 set as the emphasized EdgeStyle and that the Fabrication CSG silhouette deepening flows through the same delegate with no AppUi-side change, reconciling the drafting page's stated EdgeStyle tagging against the projection page's three-set output.
- Unlocks: A confirmed consumption contract so the Fabrication CSG silhouette improvement (exact arrangement outlines vs per-facet silhouettes) is read by the AppUi viewport with zero coupling change.
- Anchors: csharp:Rasm.AppUi/Render/drafting#PROJECTION Viewport2D/HiddenLineSeam/EdgeStyle; Fabrication/Posting/projection#PROJECTION_HIDDEN_LINE the (Visible/Hidden/Silhouette) sets (FabricationResult.HiddenLineResult owner.md line 88); the CSG silhouette arm; ARRANGEMENT_REALIZED_OWNER_LANDING upstream gate
- Tension: The CSG silhouette arm is forward-blocked on the realized ARRANGEMENT kernel .cs owner — the AppUi consumption contract confirms the seam shape but the silhouette-set deepening it consumes does not land until the kernel arrangement owner is implemented, so the ripple states the consumption contract as forward-ready, not active.
- Ripple: counterpart of `Rasm.Fabrication` Posting `projection` `HiddenLineResult` + `[CSG_SILHOUETTE]` idea (Viewport2D consumes the silhouette edge set).

[ARCHITECTURE_PROHIBITIONS_BLOCK]-[QUEUED]: add the missing `[05]-[PROHIBITIONS]` block to `Rasm.AppUi/ARCHITECTURE.md` so every UI seed cites a concrete, source-verified prohibition rather than flagging its absence.
- Capability: A declarative `[05]-[PROHIBITIONS]` section enumerating the inferred-and-verified UI laws — NEVER runtime XAML (compiled-only, HotAvalonia `DEV_LOOP` exception); NEVER per-host `GpuBackend`/`GRContext` construction (the `SurfaceSeam` + `GpuBackend` `RenderTargetFactory` only); NEVER a per-surface `AsyncImageLoader`/telemetry/receipt sink; NEVER an `SKSurface` outside the `Offscreen` capsule; NEVER `ReactiveUI` code-behind/`BindCommand` binding (`BehaviorRail.Intent` only); NEVER a second command/hotkey/palette registry; a control-generation or layout system is ONE polymorphic owner feeding the existing rails, never a parallel framework.
- Shape: A new `[05]-[PROHIBITIONS]` section in `ARCHITECTURE.md` alongside the existing `[01]-[DOMAIN_MAP]` and `[02]-[SEAMS]` blocks, the owner every downstream `prohibitionCheck` cites.
- Unlocks: Each subsequent seed's prohibition test resolves against a real block instead of flagging an absent section, sealing the UI prohibition vocabulary in one place.
- Anchors: Existing prohibition prose scattered across `Shell/commands`, the `Shell/input` `BehaviorRail.RejectViewBinding` XAML-only law, and the `Render/viewport` host-API-in-arm rejection clauses, lifted verbatim into the consolidated block.

[CONTROLS_INTENT_UNION_PAGE]-[QUEUED]: author the `Shell/controls` intent-vocabulary page naming the `ControlIntent` `[Union]` over the full interactive-control family with each kind's typed shape, binding column, and accessibility/token columns.
- Capability: A closed `ControlIntent` vocabulary — button, text/number/date/path input, select, slider, toggle, radio, grid, tree, menu, toolbar, tab, accordion, panel, dock, splitter — where arity, provider, and modality live in the intent shape rather than parallel control names, one polymorphic discriminant materializing hundreds of controls.
- Shape: A new `Shell/controls.md` design page spawned by the `CONTROL_MATERIALIZATION_SYSTEM` idea, naming the union and the per-kind row signatures it owns without authoring materialization code.
- Unlocks: The typed substrate the `ControlFactory` materialize fold and every screen body consume, and the basis for agent/remote/wire control synthesis over one vocabulary.
- Anchors: The `EditorFactory` eleven-row typed-shape->control precedent already proven in `PropertyGrid` cells, the `CommandIntent` row shape, `Theme/tokens` roles, `Shell/accessibility` automation identity, and the new `[05]-[PROHIBITIONS]` block; the control-intent projection rides the `typescript:interchange/transport` `ControlIntentWire` seam so a web/remote caller materializes the same vocabulary the desktop renders.

[CONTROLS_MATERIALIZE_FOLD_PAGE]-[QUEUED]: author the `ControlFactory` materialize-fold section projecting each `ControlIntent` onto its Avalonia control with command binding through `BehaviorRail.Intent`, token-resolved visuals, and total automation identity.
- Capability: One polymorphic fold (intent -> realized control) with recycling-aware materialization, the single `BehaviorRail.Intent` binding bridge, and total accessibility derivation — no per-kind materializer call sites and no runtime-XAML emission.
- Shape: A section of `Shell/controls.md` owning the `ControlFactory` fold and its boundary capsule, consuming the `GENERIC_VIRTUALIZATION_FABRIC` `VirtualWindow` owner for the grid/tree/canvas kinds.
- Unlocks: Declarative screen bodies materialized from intent streams and agent/remote control synthesis over compiled templates.
- Anchors: `BehaviorRail.Intent` as the only `ICommand` binding bridge, Avalonia `ControlTemplate`/`DataTemplate`/`ControlTheme` compiled templates, `Theme/tokens` `TokenRow` resolve, the `VirtualWindow` owner, and the `[05]-[PROHIBITIONS]` block; the `csharp:Rasm.AppUi/Shell/accessibility` seam derives each materialized control's automation name from the one intent row.

[SCREENS_CONTROL_STREAM_DEEPEN]-[QUEUED]: deepen `Shell/screens` so a `ScreenCatalogRow` body is authored as a control-intent stream materialized through `ControlFactory` rather than a per-screen XAML literal.
- Capability: `ScreenBase.Wire` composes a control-intent stream and the catalog row gains the generative body column, deleting per-screen XAML duplication across the `ScreenCatalog` frozen-row table.
- Shape: An extension to the existing `Shell/screens.md` activation page consuming the control-materialization owner, no second screen base family minted.
- Unlocks: Every screen authored declaratively over the one control vocabulary.
- Anchors: The `ScreenCatalog` frozen-row table, `ScreenBase.Wire`, the `ControlFactory` fold, and the `[05]-[PROHIBITIONS]` block.

[LAYOUT_CONSTRAINT_ALGEBRA_PAGE]-[QUEUED]: author the `Shell/solver` constraint-algebra page naming the `LayoutConstraint` vocabulary and the flex/grid/auto-layout rows expressed over it.
- Capability: A typed constraint vocabulary over edge/size/anchor variables with equalities, inequalities, and priorities, where flex, grid-track, and auto-layout are constraint-row presets rather than parallel layout panels.
- Shape: The algebra section of a new `Shell/solver.md` design page spawned by the `CONSTRAINT_LAYOUT_ENGINE` idea.
- Unlocks: One layout language shared across desktop, web, and immersive surfaces.
- Anchors: The Avalonia `Layoutable` measure-arrange seam, `Theme/tokens` `Metric` spacing rows, `Kiwi` (`.api/api-kiwi.md`), and the `[05]-[PROHIBITIONS]` block; the constraint bands project onto the `typescript:ui/render` `LayoutConstraintWire` seam carrying the full ordered constraint program — variable-introduction order, edit-variable set, and suggested-value sequence — so the `kiwi.js` web tableau re-solves the identical system the desktop solves rather than a baked frame.
- Ripple: counterpart of the cross-libs `[ONE_UI_INTENT_WIRE]` seam (`LayoutConstraintWire` carries the `Kiwi`-authored ordered constraint program the `kiwi.js` head re-solves).

[LAYOUT_SOLVER_PANEL_PAGE]-[QUEUED]: author the solver-capsule section folding the Kiwi solve into one custom Avalonia panel's measure/arrange pass.
- Capability: A single `LayoutSolver` panel that solves constraints once per surface and arranges children, the named boundary capsule carrying the solver statement body and folding into Avalonia's native layout pass rather than a parallel layout engine.
- Shape: A section of `Shell/solver.md` owning the solver capsule.
- Unlocks: Self-sizing and responsive arrangement resolved from typed constraints.
- Anchors: Avalonia `Panel`/`Layoutable`, `Kiwi`, the control-materialization owner emitting the laid-out controls, and the `[05]-[PROHIBITIONS]` block.

[VIRTUALIZATION_WINDOW_OWNER_PAGE]-[QUEUED]: author the `Shell/virtualization` `VirtualWindow` owner page — viewport-range to realized-item window, control recycling, variable-extent measurement, sticky headers, and hierarchical flatten — folding over DynamicData change-sets.
- Capability: One windowing owner every list/tree/grid/canvas surface consumes, where windowing is incremental over `IChangeSet` rather than a per-surface virtualizer, recycling realized controls over a data window keyed by scroll/viewport.
- Shape: A new `Shell/virtualization.md` design page spawned by the `GENERIC_VIRTUALIZATION_FABRIC` idea.
- Unlocks: Constant-cost rendering for arbitrarily large lists, deep model trees, dashboard tile grids, and the drafting canvas from one owner.
- Anchors: DynamicData `IChangeSet` operators as the existing `LiveData` spine, Avalonia `ItemsControl`/`Layoutable`, the `Editing/tables` tree-flatten fold, and the `[05]-[PROHIBITIONS]` block; the `csharp:Rasm.AppUi/Editing/tables` seam routes its tree-flatten projection through the one `VirtualWindow` rather than a tables-local virtualizer.

[VIRTUALIZATION_TREE_FLATTEN_DEEPEN]-[QUEUED]: deepen `Editing/tables` so its hierarchical tree-flatten projection routes through the one `VirtualWindow` owner rather than a tables-local virtualizer.
- Capability: Tables expose their flattened tree as a windowed item stream the `VirtualWindow` recycles, conserving the existing column-metadata family and tree-flatten fold while delegating windowing to the one fabric.
- Shape: An extension to the existing `Editing/tables.md` page consuming the virtualization fabric.
- Unlocks: Constant-cost rendering for deep model trees and large tabular projections.
- Anchors: The `Editing/tables` column-metadata family and tree-flatten fold, the `VirtualWindow` owner, and the `[05]-[PROHIBITIONS]` block.

[INPUT_FABRIC_DEVICE_FOLD]-[QUEUED]: deepen `Shell/input` with an `InputFabric` device-source axis folding the admitted SpaceMouse, gamepad, haptic, and MIDI SDKs onto the one `BehaviorRail.Intent` bridge.
- Capability: One `InputDevice` `[Union]` and a `DeviceFold` projecting `HidSharp` raw-HID SpaceMouse six-axis deltas, `Silk.NET.Input` `IGamepad`/`IJoystick` state, `Silk.NET.SDL` `SDL_Haptic` rumble effects, and `Melanchall.DryWetMidi` `NoteOnEvent`/control-change streams onto the `CommandIntent` vocabulary and the pan-zoom canvas algebra — no per-device command registry and no second input stack.
- Shape: An extension to the existing `Shell/input.md` page deepening the command-derived-hotkey and behavior trigger/action rows with the device-source axis, consuming the four admitted SDKs through the one `InputFabric` owner.
- Unlocks: SpaceMouse viewport navigation, gamepad design review, haptic feedback on clash/constraint events, and MIDI control-surface parameter binding over the one command rail.
- Anchors: The `Shell/input` `BehaviorRail.Intent`/`BehaviorRail.RejectViewBinding` law and pan-zoom canvas algebra, the `CommandIntent`/`CommandDeck` vocabulary, the admitted `HidSharp` (`.api/api-hidsharp.md`), `Silk.NET.Input` (`.api/api-silk-input.md`), `Silk.NET.SDL` (`.api/api-silk-sdl.md`), and `Melanchall.DryWetMidi` (`.api/api-drywetmidi.md`) surfaces, and the `[05]-[PROHIBITIONS]` block.

[FORMS_SCHEMA_WIZARD_PAGE]-[QUEUED]: author the `Editing/forms` schema-and-wizard page — a `FormSchema` of typed field rows materialized through `ControlFactory`, validated through the screens `ReactiveUI.Validation` lift, with multi-step wizard flow over PropertyModels condition/visibility annotations.
- Capability: Declarative forms and wizards as field-row schemas, validation as the one typed-rail `Validation<Error,T>` lift, and conditional visibility through admitted annotations — no settings-dialog or form framework minted.
- Shape: A new `Editing/forms.md` design page spawned by the `FORMS_TRANSACTION_AND_SELECTION` idea.
- Unlocks: Forms and wizard flows as a product capability over the one control vocabulary.
- Anchors: The `ControlFactory` fold, the screens `Validation<Error,T>` lift, the admitted PropertyModels `[ConditionTarget]`/`[PropertyVisibilityCondition]`/`[DependsOnProperty]` annotations, and the `[05]-[PROHIBITIONS]` block.

[SELECTION_BATCH_EDIT_PAGE]-[QUEUED]: author the multi-selection and batch-edit section — a `Selection` model over admitted `ICheckedList`/`ISelectableList` driving batch-edit intents that fold to one combined `CommandReceipt` through `CommandExecution.Combine`.
- Capability: One selection vocabulary and a batch-edit fold where N selected items edit in one transaction, batch verbs folding through the one `CommandExecution.Combine` algebra rather than a per-macro registry.
- Shape: A section of `Editing/forms.md` owning the selection model and the batch-edit fold.
- Unlocks: Batch editing of many selected items in one revertible transaction.
- Anchors: The admitted `ICheckedList`/`ISelectableList`, `CommandExecution.Combine`, the `EditReceipt` sink, and the `[05]-[PROHIBITIONS]` block.

[EDIT_HISTORY_REVERTIBLE_PAGE]-[QUEUED]: author the `Editing/history` page wrapping the admitted `CancelableCommandRecorder`/`CommandHistoryViewModel` so every edit is a revertible command sealed through the `EditReceipt` sink with `EditOutcome.Reverted`.
- Capability: Client-side undo/redo as a revertible command stack on the admitted recorder, surfaced as undo/redo command-table intents and sealing `EditReceipt.Reverted` — no per-screen undo stack and no undo package minted.
- Shape: A new `Editing/history.md` design page spawned by the `FORMS_TRANSACTION_AND_SELECTION` idea.
- Unlocks: Revertible client-side history across every edit surface.
- Anchors: The admitted `CancelableCommandRecorder` (`MaxCommand=20`), `CommandHistoryViewModel` `CanUndo`/`CanRedo`, the `EditReceipt`/`EditOutcome.Reverted` rail, the `CommandIntent` rows, and the `[05]-[PROHIBITIONS]` block; each edit records as a `RevertibleOp` carrying its forward and inverse delta, and an undo past the `MaxCommand=20` window resolves against the durable `csharp:Rasm.Persistence/Sync` `OpLogEntry` inverse stream keyed by `ContentIdentity` so client history and durable sync fold one inverse algebra.
- Ripple: counterpart of the `csharp:Rasm.AppHost` branch `[ONE_REVERT_VOCABULARY]` idea (AppUi records `RevertibleOp` deltas, Persistence replays them as a `SyncOpKind` row over the `Editing/forms → Persistence/Sync` revertible op-log seam).

[RICHTEXT_MARKDIG_RENDERER_PAGE]-[QUEUED]: author the `Editing/media` markdown-projection section — a Markdig `RendererBase` walking the block/inline AST via `Descendants<T>` into theme-token-styled Avalonia inlines and visuals.
- Capability: Markdown rendered to live visuals through the admitted Markdig AST and styled through `Theme/typography` roles — no HTML round-trip and no parallel markdown model.
- Shape: The markdown section of a new `Editing/media.md` design page spawned by the `RICH_CONTENT_AND_MEDIA` idea.
- Unlocks: Markdown documentation cells, help, and report prose rendered as visuals.
- Anchors: The admitted Markdig `RendererBase` plus block/inline AST and `Descendants<T>`, the `Theme/typography` `MarkdownProjection` hook already referenced by the notebook, the notebook markdown cells, and the `[05]-[PROHIBITIONS]` block; the `csharp:Rasm.AppUi/Theme/typography` seam owns the inline-styling roles the renderer resolves so markdown styling rides the one typography vocabulary.

[MEDIA_SURFACE_CODEC_PAGE]-[QUEUED]: author the `MediaSurface` codec-row section — a `MediaSurface` `[Union]` over image/video/audio codec rows mounting on the one `SurfaceSeam`, with `HanumanInstitute.LibMpv.Avalonia` driving video/audio and the existing `AsyncImageLoader` the image row.
- Capability: One media vocabulary where a new codec is one row, playback transport is one rail, and every media surface mounts on the single `SurfaceSeam` render host rather than a per-surface codec.
- Shape: A section of `Editing/media.md` owning the codec rows and the `SurfaceSeam` mount.
- Unlocks: Embedded video/audio playback beside images in one content rail.
- Anchors: The `HanumanInstitute.LibMpv.Avalonia` admission, the admitted `AsyncImageLoader`, the `SurfaceSeam` render-host axis, and the `[05]-[PROHIBITIONS]` block.

[RENDER_SHADING_ASSET_PAGE]-[QUEUED]: author the PLANNED `Render/shading` page — a `ShaderAsset` cache keyed per `GpuBackend` feeding a layered-BSDF `SurfaceShade` at the path tracer.
- Capability: One shader-asset owner with a per-backend pipeline-state cache (`SKRuntimeEffect` for Ganesh, wgpu pipeline-state for Wgpu) and a `SurfaceShade` pass consuming the Materials `LayeredBsdf`, sharing the one `Wgpu` device the viewport leases through the branch `ONE_WGPU_DEVICE` owner rather than a second GPU device.
- Shape: A newly realized `Render/shading.md` design page, currently a PLANNED codemap node.
- Unlocks: Material/BSDF shading at the GPU path tracer.
- Anchors: The `GpuBackend.Wgpu` `RenderTargetFactory`, `SKRuntimeEffect`, `Silk.NET.WebGPU` pipeline-state, the `Rasm.Materials/Appearance` `LayeredBsdf` seam, the shared `ONE_WGPU_DEVICE` `EMBED_CAPSULE` law, and the `[05]-[PROHIBITIONS]` block — the `SKSurface` stays in the `Offscreen` capsule and the runtime arm is SPIKE-gated while the design authors now; the `csharp:Rasm.Materials/Appearance` seam supplies `LayeredBsdf`->`SurfaceShade` so the shading owner consumes the materials BSDF rather than minting an appearance model.

[RENDER_IMMERSIVE_OPENXR_PAGE]-[QUEUED]: author the PLANNED `Render/immersive` page — an OpenXR stereo design-review surface plus `XR_FB_passthrough`, binding onto the same `Wgpu` device and folding to the flat viewport where the OpenXR loader is absent.
- Capability: One immersive owner over `Silk.NET.OpenXR` plus FB passthrough (stereo swapchain, predicted-display-time frame loop, action-set controller) sharing the viewport's `Wgpu` device through the branch `ONE_WGPU_DEVICE` owner, with a flat-fold fallback so the page ships even without an XR runtime.
- Shape: A newly realized `Render/immersive.md` design page, currently a PLANNED codemap node.
- Unlocks: On-site mixed-reality design review with passthrough.
- Anchors: `Silk.NET.OpenXR`/`Extensions.FB` (pinned 2.23.0), the shared `ONE_WGPU_DEVICE` `EMBED_CAPSULE` law, the `GpuBackend.Wgpu` `RenderTargetFactory`, and the `[05]-[PROHIBITIONS]` block — never a second GPU context, the runtime arm SPIKE-gated while the design authors now.

[VIEWPORT_GODPAGE_SPLIT]-[QUEUED]: split the 750-line `Render/viewport` god-page into `Render/{Pipeline, Meshlets, PathTrace}` so the render-graph, geometry-virtualization, and path-trace owners are authored at their true granularity, capability conserved.
- Capability: Three focused Render pages — Pipeline (`RenderGraph` pass-DAG + `RenderTargetFactory` + resolve ladder), Meshlets (`MeshletCluster` + residency), PathTrace (BVH + ReSTIR + denoise) — with no behavior change and denser ownership.
- Shape: A move-only structural relocation of the `Render/viewport.md` sections into three pages, with the codemap and `[02]-[SEAMS]` updated.
- Unlocks: The shading and immersive pages mount cleanly beside a viewport authored at real granularity.
- Anchors: The existing `Render/viewport.md` `[02]-[RENDER_GRAPH]`/`[03]-[GEOMETRY_VIRTUAL]`/`[04]-[PATH_TRACE]` sections, the `ARCHITECTURE.md` codemap, and the `[05]-[PROHIBITIONS]` block — capability conserved and one owner per concern preserved.

[ISSUES_RELOCATE_TO_BIM]-[QUEUED]: relocate the BCF issue-board DOMAIN semantics out of `Editing/issues` into `Rasm.Bim/coordination` so AppUi keeps only the board PROJECTION, enforcing the boundary the branch `FEDERATED_COORDINATION_COCKPIT` idea draws.
- Capability: The BCF 3.0 topic/component/comment semantics and GlobalId-stable diff move to `Rasm.Bim/coordination` as the single openBIM coordination owner, while AppUi retains only the viewpoint board projection over comment threads and tiles — no second BCF schema and no BCF-XML writer in AppUi.
- Shape: A move-only relocation lifting the issue-board domain model from `Editing/issues` into `Rasm.Bim/coordination`, leaving the AppUi `coordination` board as a pure projection consuming the Bim-owned semantics.
- Unlocks: The federated coordination cockpit composes one Bim-owned semantic model, the diff joins by GlobalId plus content key, and round-trip comments persist through the op-log changefeed with the board reading rather than re-modeling the issue domain.
- Anchors: `Rasm.Bim/coordination` as the BCF owner, the AppUi `coordination` board projection, BCF 3.0 and the GlobalId-stable diff, and the branch `[FEDERATED_COORDINATION_COCKPIT]` idea enforcing the boundary; `Editing/issues` is absent from the AppUi `[02]-[SEAMS]` map, so the relocation adds the `Editing/issues <- Rasm.Bim/coordination` consumption endpoint as the board's only inbound coordination seam.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[KIWI_ADMISSION]-[COMPLETE]: admitted `Kiwi` 3.0.0 (Apache-2.0, pure-managed incremental linear-arithmetic constraint solver) as the `Shell/Layout` constraint-layout solver; pinned + referenced; README `[LAYOUT]` group and `.api/api-kiwi.md` authored.
[LIBMPV_ADMISSION]-[COMPLETE]: admitted `HanumanInstitute.LibMpv` + `HanumanInstitute.LibMpv.Avalonia` 0.10.1 as the Avalonia-12 `MediaSurface` (OpenGL render path); pinned + referenced; README `[MEDIA]` group and `.api/api-libmpv.md` authored; the libmpv native is provisioned at the app-host distribution layer.
[INPUT-FABRIC-SDKS]-[COMPLETE]: admitted the cross-platform net10 input SDKs — `HidSharp` (SpaceMouse/HID), `Silk.NET.Input` (gamepad), `Silk.NET.SDL` (haptic/rumble), and `Melanchall.DryWetMidi` (MIDI); pinned + referenced; README `[INPUT_FABRIC]` group and `.api/api-hidsharp.md`, `api-silk-input.md`, `api-silk-sdl.md`, `api-drywetmidi.md` authored. Eye-gaze, switch-access, speech-to-text, translation, and robotics have no viable cross-platform net10 NuGet and stay out of scope.
