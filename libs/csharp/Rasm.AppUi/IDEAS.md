# [APPUI_IDEAS]

The forward concept pool for the product UI engine. Open ideas are higher-order folder concepts grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research; each drives one or more task cards in `TASKLOG.md`. A finished or dropped idea moves to closed with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CONTROL_MATERIALIZATION_SYSTEM]-[COMPLETE]: the `Shell/Controls` sub-domain (`Shell/controls.md`) is the generative substrate — one `ControlIntent` `[Union]` over the eighteen-case control family materialized by the one `ControlFactory.Materialize` fold through the single `BehaviorRail.Intent` bridge, `Token`-resolved visuals, total `AutomationProperties.SetName`, `MaterializePool` recycling over the `VirtualWindow`, and the `ControlIntentWire` projection; `Shell/screens.md` `[06]-[CONTROL_STREAM]` shifts screen bodies to control-intent streams; Ripple: csharp:Rasm.AppUi/Controls [ONE_UI_INTENT_WIRE].
[CONSTRAINT_LAYOUT_ENGINE]-[COMPLETE]: the `Shell/Layout` sub-domain (`Shell/solver.md`) owns the `LayoutConstraint` Cassowary algebra, the flex/grid/auto-layout `LayoutPreset` rows, and the one `LayoutSolver` custom Avalonia `Panel` folding the `Kiwi` dual-simplex into measure/arrange via the guarded `Try*` family, with the ordered `LayoutProgramWire` carrying the full constraint program (introduction order + edit set + suggestions) the `@lume/kiwi` head re-solves; Ripple: csharp:Rasm.AppUi/Layout [ONE_UI_INTENT_WIRE].
[GENERIC_VIRTUALIZATION_FABRIC]-[COMPLETE]: `Shell/virtualization.md` realizes the one `VirtualWindow<TItem,TKey>` owner — range-to-realized fold over `DynamicData.Virtualise`/`Page`, the `ExtentLedger` O(log n) measured-extent prefix tree, `StickyProjection` group/ancestor overlay, and the one `HierarchyFlatten` `TransformToTree` bridge — every windowed surface (tables, notebook, dashboard, canvas, `ControlFactory` grid/tree/panel) consumes it.
[FORMS_TRANSACTION_AND_SELECTION]-[COMPLETE]: `Editing/forms.md` (`FormSchema`/`WizardFold` over `ControlFactory` + applicative `Validation<Error,T>`, `Selection` over `ICheckedList`, `BatchEdit` folding to one `CommandReceipt` via `CommandExecution.Combine`) and `Editing/history.md` (`RevertibleOp` forward/inverse algebra over `CancelableCommandRecorder` + durable op-log `RevertScope`) realize all three legs; Ripple: csharp:Rasm.AppHost [ONE_REVERT_VOCABULARY].
[RICH_CONTENT_AND_MEDIA]-[COMPLETE]: `Editing/media.md` realizes the `MarkdownInlineRenderer` retained materialization over the `Theme/typography` `MarkdownRow`/`InlineRun` rows, the `MediaSurface` `[Union]` codec rows (image via `AsyncImageLoader` single cache, video/audio via `HanumanInstitute.LibMpv.Avalonia` OpenGL path on the one `SurfaceSeam`), and the `PlaybackTransport` rail over typed `MpvContext` members.
[INPUT_DEVICE_FABRIC]-[COMPLETE]: `Shell/input.md` `[05]-[INPUT_FABRIC]`/`[06]-[DEVICE_DRIVERS]`/`[07]` realize the `InputDevice`/`DeviceOutput` `[Union]` + `InputFabric` device-to-intent and intent-to-device fold over the one `CommandIntent` table, with the four admitted SDK boundary capsules (`HidSharp`/`Silk.NET.Input`/`Silk.NET.SDL`/`Melanchall.DryWetMidi`) normalizing to `DeviceAxis` at the edge.
[GPU_SHADING_AND_IMMERSIVE]-[COMPLETE]: `Render/shading.md` (`ShaderAsset` per-`GpuBackend` cache + `ShaderShade` consuming Materials `LayeredBsdf`) and `Render/immersive.md` (`ImmersiveSession`/`XrFrame`/`XrInput`/`Passthrough` over `Silk.NET.OpenXR`+FB sharing the one `Wgpu` device, flat-fold fallback) realized, and the 750-line `Render/viewport` god-page split into `Render/{pipeline,meshlets,pathtrace}` (viewport.md a redirect stub) with the codemap conserved.
