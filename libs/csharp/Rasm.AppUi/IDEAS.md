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

[CONTROL_MATERIALIZATION_SYSTEM]-[QUEUED]: one typed `ControlIntent` family materializes any interactive control from a declarative shape so a screen is authored as a control-intent stream rather than per-screen XAML literals.
- Capability: A `ControlIntent` `[Union]` whose single `ControlFactory` fold projects each intent — button, text/number/date/path input, select, slider, toggle, radio, grid, tree, menu, toolbar, tab, accordion, panel, dock, splitter — onto its Avalonia control, binds its `ReactiveCommand` exclusively through the existing `BehaviorRail.Intent` bridge, resolves visuals through `Theme/tokens` roles, and routes accessibility through the `Shell/accessibility` automation-identity rail, so one vocabulary materializes hundreds of controls and never a parallel control framework.
- Shape: A new `Shell/Controls` sub-domain (`Shell/controls.md`) becomes the generative substrate every screen body composes; `ScreenCatalog` rows shift from XAML-literal bodies to control-intent streams authored over `ScreenBase.Wire`.
- Unlocks: Screens authored declaratively; agent, remote, and wire control synthesis; one place to enforce binding, theming, accessibility, and command law across every control; the substrate the forms owner and the virtualization fabric consume.
- Anchors: `BehaviorRail.Intent` as the only C# binding bridge, the `CommandIntent`/`CommandDeck` rails, the `EditorFactory` eleven-row precedent already proving typed-shape-to-control in `PropertyGrid` cells, `Theme/tokens` `TokenRow`, `Shell/accessibility` automation identity, and Avalonia `ControlTemplate`/`DataTemplate`/`ControlTheme`; the `csharp:Rasm.AppUi/Controls` mint emits `ControlIntentWire` over the `ControlIntent` family that `typescript:interchange` decodes and `typescript:ui/render` materializes, so a web/remote caller materializes the same control vocabulary the desktop renders, and `Shell/accessibility` derives each materialized control's automation name from the one intent row.
- Ripple: counterpart of the cross-libs `[ONE_UI_INTENT_WIRE]` seam (`csharp:Rasm.AppUi/Controls` mints `ControlIntentWire` over the `ControlIntent` family the web head materializes).

[CONSTRAINT_LAYOUT_ENGINE]-[QUEUED]: a declarative constraint-layout engine replaces width-breakpoint knobs with a real solver so responsive, self-sizing, and adaptive layouts resolve from typed constraints across desktop, web, and immersive surfaces.
- Capability: A `LayoutConstraint` algebra of equalities, inequalities, and priorities over edge, size, and anchor variables, solved once per surface by a `Kiwi` owner that folds into Avalonia's measure/arrange pass through one custom `Panel`, so flex, grid-track, and auto-layout become constraint rows rather than parallel layout panels.
- Shape: A new `Shell/Layout` sub-domain (`Shell/solver.md`) owns the constraint vocabulary and the solver capsule; every adaptive screen consumes it.
- Unlocks: True responsive adaptation beyond breakpoints; self-sizing forms and dashboards; one layout language shared by desktop, web, and XR; design-tool-grade auto-layout.
- Anchors: The Avalonia `Panel`/`Layoutable` measure-arrange seam, `Theme/tokens` `Metric` rows for spacing, and the `CONTROL_MATERIALIZATION_SYSTEM` owner that emits the controls being laid out; `Kiwi` is the simplex solver admission, pinned in `Directory.Packages.props`, registered as the `README` `[LAYOUT]` group, and evidenced at `.api/api-kiwi.md`, internalized into the one `Shell/Layout` owner rather than a thin wrapper; the `typescript:ui/render` `LayoutConstraintWire` bands solve the same layout the desktop solves.
- Tension: An under-constrained Cassowary system admits many valid assignments, so the `Kiwi` desktop tableau and the `kiwi.js` web tableau converge to identical positions only when `LayoutConstraintWire` carries the full ordered constraint program — variable-introduction order, the edit-variable set, and the suggested-value sequence the desktop solver fed — not just the relation set; an order-free constraint dump is the silent per-viewport drift defect, so the `csharp:Rasm.AppUi/Layout` mint emits the ordered program and the web head re-solves it, matching the `ONE_UI_INTENT_WIRE` strength/relation-parity invariant.
- Ripple: counterpart of the cross-libs `[ONE_UI_INTENT_WIRE]` seam (`csharp:Rasm.AppUi/Layout` mints `LayoutConstraintWire` carrying the `Kiwi`-authored ordered constraint program the `kiwi.js` head re-solves).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[GENERIC_VIRTUALIZATION_FABRIC]-[COMPLETE]: `Shell/virtualization.md` realizes the one `VirtualWindow<TItem,TKey>` owner — range-to-realized fold over `DynamicData.Virtualise`/`Page`, the `ExtentLedger` O(log n) measured-extent prefix tree, `StickyProjection` group/ancestor overlay, and the one `HierarchyFlatten` `TransformToTree` bridge — every windowed surface (tables, notebook, dashboard, canvas, `ControlFactory` grid/tree/panel) consumes it.
[FORMS_TRANSACTION_AND_SELECTION]-[COMPLETE]: `Editing/forms.md` (`FormSchema`/`WizardFold` over `ControlFactory` + applicative `Validation<Error,T>`, `Selection` over `ICheckedList`, `BatchEdit` folding to one `CommandReceipt` via `CommandExecution.Combine`) and `Editing/history.md` (`RevertibleOp` forward/inverse algebra over `CancelableCommandRecorder` + durable op-log `RevertScope`) realize all three legs; Ripple: csharp:Rasm.AppHost [ONE_REVERT_VOCABULARY].
[RICH_CONTENT_AND_MEDIA]-[COMPLETE]: `Editing/media.md` realizes the `MarkdownInlineRenderer` retained materialization over the `Theme/typography` `MarkdownRow`/`InlineRun` rows, the `MediaSurface` `[Union]` codec rows (image via `AsyncImageLoader` single cache, video/audio via `HanumanInstitute.LibMpv.Avalonia` OpenGL path on the one `SurfaceSeam`), and the `PlaybackTransport` rail over typed `MpvContext` members.
[INPUT_DEVICE_FABRIC]-[COMPLETE]: `Shell/input.md` `[05]-[INPUT_FABRIC]`/`[06]-[DEVICE_DRIVERS]`/`[07]` realize the `InputDevice`/`DeviceOutput` `[Union]` + `InputFabric` device-to-intent and intent-to-device fold over the one `CommandIntent` table, with the four admitted SDK boundary capsules (`HidSharp`/`Silk.NET.Input`/`Silk.NET.SDL`/`Melanchall.DryWetMidi`) normalizing to `DeviceAxis` at the edge.
[GPU_SHADING_AND_IMMERSIVE]-[COMPLETE]: `Render/shading.md` (`ShaderAsset` per-`GpuBackend` cache + `ShaderShade` consuming Materials `LayeredBsdf`) and `Render/immersive.md` (`ImmersiveSession`/`XrFrame`/`XrInput`/`Passthrough` over `Silk.NET.OpenXR`+FB sharing the one `Wgpu` device, flat-fold fallback) realized, and the 750-line `Render/viewport` god-page split into `Render/{pipeline,meshlets,pathtrace}` (viewport.md a redirect stub) with the codemap conserved.
