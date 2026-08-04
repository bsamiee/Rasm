# [APPUI_IDEAS]

Forward concept pool for the product UI engine: each open idea is a higher-order folder concept grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research, and each drives one or more `TASKLOG.md` cards.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[DARK_IDENTITY_RAMP]-[QUEUED]: One professional dark identity every host inherits — two disciplined surface ramps replace the lone anchor pair.
- Capability: A perceptually generated two-ramp surface system — a host-matched panel ramp beside a near-black overlay ramp — carrying a twelve-step role ladder (canvas, panel, control rest/hover/active, subtle/interactive/strong border, solid, low/high text), borders as text-color alpha rungs, input wells recessed darker than their parent, a text-emphasis alpha ladder with a hard disabled floor, and a two-token accent law: the fill accent and the text/ring accent are distinct rungs, accent coverage stays marginal, and hue holds in the cool 210-235 band.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Theme/tokens.md` `tokens#TOKEN_CATALOG` paint rows, `tokens#VARIANT_AXIS`, the `SemiSlot` correspondence, and the `ThemeRail` contrast/CVD candidate rosters.
- Unlocks: every control theme, chart paint, chrome surface, and dialog reads one settled dark ladder; the garish-accent failure mode becomes structurally unspellable, and the access sweep gates every new pair because candidates land with the rows.
- Anchors: kernel `PerceptualColor` ramp/tone owners with the dark-surround viewing condition and chroma-reduction gamut mapping; Semi `SemiColorBackground0..4`/`SemiColorFill0..2`/`SemiColorText0..3` numbered ramps; Radix step-role semantics; measured Jifto/Arcol calibration — a docked body host-matches mid-grey while owned overlays go near-black.
- Tension: near-black everywhere reads as a foreign object inside a mid-grey host — two ramps in one system, never a per-host fork; the ramp is data, so a host adapter selects a ramp row and no host name enters the catalog.
- Ripple: precedes [ELEVATION_MATERIAL_SYSTEM], [CONTROL_VARIANT_ALGEBRA], [DARK_CHART_GRAMMAR], [CONTENT_SURFACE_DARKENING].

[ELEVATION_MATERIAL_SYSTEM]-[QUEUED]: Elevation and material become real algebra — layered shadow stacks, tier vocabulary, honest translucency.
- Capability: An elevation-tier vocabulary whose rows carry multi-layer shadow stacks (hairline ring first, ambient beside key layers, dark alphas doubled), the in-window law that elevation is one surface step plus a border while true shadows are reserved for surfaces leaving the window, inset top-highlight rims, scrim rows, and a material axis whose embedded truth is composited tint-plus-noise — real OS translucency exists only on owned windows and the fallback is a first-class row, never a degraded afterthought.
- Shape: `tokens#TOKEN_CATALOG` `Depth` rebuild plus material/scrim rows fused with the `Rank` z-order axis; `Vfx/material` counterpart page.
- Unlocks: dialogs, flyouts, toasts, palettes, rails, and cards each bind one tier row; the current dead-end depth tuple that reaches no control is deleted whole.
- Anchors: `BoxShadow`/`BoxShadows` (offset/blur/spread/inset, comma stacks) on `Border.BoxShadow` with `BoxShadowsTransition`; `SemiShadowElevated` beside the per-control Semi shadow keys; the Fluent dark shadow ramp and Primer negative-spread stack archetypes; `ExperimentalAcrylicMaterial` tint/noise truth and the `Digger` erase hazard; Unicolour `Blend` compositing as the scrim math.
- Tension: `Visual.Effect` rasterizes its subtree per push while `Border.BoxShadow` draws per shadow — elevation rides `BoxShadows` by default and `Effect` only on deliberate one-off surfaces.
- Ripple: follows [DARK_IDENTITY_RAMP]; precedes [VFX_PLANE]; mirrors [OVERLAY_MODALITY_CHROME].

[VFX_PLANE]-[QUEUED]: A rendering-effects plane — backdrop, shader, and composition owners as a new sub-domain.
- Capability: One effects plane owning in-tree backdrop sampling (blur of sibling content already painted), runtime SkSL shader surfaces (glow, grain, gradient wash), and the composition-visual animation adapter — so glow, acrylic-like panels, the ambient module wash, and pulsing emphasis are owned operations any surface composes, never per-control drawing code, and every effect degrades under reduced-motion and reduce-transparency by construction.
- Shape: new sub-domain `libs/csharp/Rasm.AppUi/.planning/Vfx/` — `material.md`, `shader.md`, `compose.md` — with router and codemap rows landing at the folder `README.md`/`ARCHITECTURE.md` in the same pass.
- Unlocks: the signature ambient wash and restrained CTA glow become one-row policies for any surface on any host; motion plan rows gain a composition execution lane.
- Anchors: `ICustomDrawOperation` with the `ISkiaSharpApiLeaseFeature` lease; `SKCanvasSaveLayerRec.Backdrop` under `InitializeWithPrevious`; `SKRuntimeEffect`/`SKRuntimeShaderBuilder` per-frame uniforms; `SKMaskFilter` outer-blur halo; `SKShader` perlin noise; additive `SKBlendMode` compositors; `ElementComposition.GetElementVisual`, `Compositor` keyframe/implicit factories, `CompositionCustomVisualHandler`; the host reduce-transparency preference gate.
- Tension: backdrop sampling reads siblings painted before the layer and never host pixels — the plane states that boundary as law instead of promising OS acrylic it cannot deliver embedded.
- Route: the backdrop layer's interaction with partial-repaint dirty rects proves on a live render-hash probe before `material.md` finalizes; the deterministic floor is the opaque tinted-noise fill.
- Ripple: follows [ELEVATION_MATERIAL_SYSTEM]; mirrors [MOTION_MICRO_INTERACTION].

[CONTROL_VARIANT_ALGEBRA]-[QUEUED]: The closed control union gains its missing axes — variant, icon, pending, and the absent cases — without a parallel name.
- Capability: Variant/emphasis, icon, and pending axes on the existing intent family plus the genuinely absent shapes — static label, segmented mode-switch, empty-state — so every professional control (quiet CTA, icon rail button, toggle-pair, labeled row, zero-data panel) is a shape on one vocabulary; materialization keeps riding the one fold and its composition-bound context columns, so hosts stay adapter rows.
- Shape: `Shell/controls#CONTROL_INTENT` case/column widening, `Shell/controls#MATERIALIZE_FOLD` arms and context columns, `Shell/controls#TS_PROJECTION` wire literals; `tokens#CONTROL_THEMES` table counterpart rows.
- Unlocks: an icon can finally reach a materialized control (the asset rail currently terminates unreachable); forms, chrome, palette, and dialogs compose variants instead of hand-rolling.
- Anchors: the `ControlIntent` growth law and the `MaterializeContext` delegate-column precedent; `StyledElement.Theme` `ControlTheme` resolution; FluentIcons `SymbolIcon`/`IconVariant`/`IconSize`; Ursa `IconButton` attached icon properties; `IconSurface.Resolve` as the one icon materializer.
- Tension: the fold is Avalonia-core-only by settled law — a third-party control enters only through a context column, and each new case answers that admission explicitly rather than eroding the wall.
- Ripple: follows [DARK_IDENTITY_RAMP]; precedes [CHROME_RAIL_FAMILY], [FORM_SECTION_GRAMMAR].

[CHROME_RAIL_FAMILY]-[QUEUED]: Floating chrome as one parameterized overlay family — rail, mode toolbar, HUD chips, status footer, collapse tab.
- Capability: One overlay contract (layout orientation, dock zone, collapse, overflow policy, translucency, elevation tier, pointer pass-through) behind the floating action rail, the segmented icon mode toolbar, quiet in-viewport HUD chips, the status footer pane vocabulary, and the panel collapse tab — pure-2D overlays are host-agnostic by construction and camera-fact consumers take injected observables through a declared seam.
- Shape: `Shell/navigation#SHELL_CHROME` slot/row growth, `Shell/controls` materialization arms, `Shell/solver#LAYOUT_PRESETS` rail programs, `tokens#CONTROL_THEMES` chrome rows.
- Unlocks: any host mounts the same rail and footer; a mode is a catalog route, never a mode enum; breakpoint collapse rides the adaptive rows already settled.
- Anchors: `ChromeSlot`/`ChromeRow` with the per-mount visibility matrix; `ShellRoot` route projection over `ScreenCatalog`; `LayoutPrograms.Rail`; `BreakpointRow` with `AdaptiveBehavior`; Ursa `NavMenu` collapse geometry, `ButtonGroup`, `ToolBar` overflow; the overlay parameter spine (layout, dock zone, min/max, priority) proven across canvas tools; elevation-integer layering with wrappers hit-test-invisible and leaves interactive.
- Ripple: follows [CONTROL_VARIANT_ALGEBRA]; mirrors [COMMAND_PALETTE_SURFACE].

[COMMAND_PALETTE_SURFACE]-[QUEUED]: The settled palette engine earns a world-class presentation surface.
- Capability: An overlay command surface over the settled federation — ranked rows carrying icon, secondary text, shortcut chips, and kind badges; a per-hit action sub-panel; honest empty and loading states — presentation only, because search, ranking, scoping, and invocation are settled law that no palette-local registry may re-mint.
- Shape: `Shell/commands#PALETTE_AND_REMOTE` hit-column widening plus a presentation cluster; `Shell/dialogs#SESSION_ALGEBRA` overlay seating; `tokens#CONTROL_THEMES` palette rows.
- Unlocks: one keystroke reaches every command, route, element, and issue on every host, with the deck's gesture column rendering as keycap chrome.
- Anchors: `CommandProjections.Federate`/`Search`/`Score` and `PaletteHit`; `ScreenCatalog` palette listings; Ursa `OverlayDialogHost` scrim slot; `GesturePolicy.Bindings` scope narrowing; the top-anchored palette geometry archetype (fixed-width surface, keycap rows, selection never wraps).
- Tension: two overlay stacks are admitted — DialogHost sessions and the Ursa overlay canvas — the palette seats on the Ursa stack and that seam becomes stated law instead of parallel ownership.
- Ripple: follows [CHROME_RAIL_FAMILY]; mirrors [OVERLAY_MODALITY_CHROME].

[OVERLAY_MODALITY_CHROME]-[QUEUED]: Dialogs, drawers, and peeks under one dark chrome and one modality law.
- Capability: Dialog chrome bound to elevation/material rows (scrim, ring, radius, blur as token-key columns), a typed destructive-confirmation shape whose verification phrase is the target identifier, and two additional modalities beside the six intents — a non-modal side-peek that keeps the underlying surface interactive and a modal drawer for forms — with dismissal-as-value preserved everywhere.
- Shape: `Shell/dialogs#SESSION_ALGEBRA` styling columns and `Shell/dialogs#DIALOG_INTENTS` Confirm widening; `Shell/navigation#ROUTING_SPINE` peek verb.
- Unlocks: destructive flows earn friction by tier — undo-toast, confirm, typed-confirm — and inspection stops stealing modality.
- Anchors: `DialogTopology` token-key columns with `DialogHostStyle` attached BoxShadow/CornerRadius; `AlignmentDialogPopupPositioner`; Ursa `Drawer`/`OverlayDialogHost` mask brush; `NavRequest` verb growth; dark scrim/elevation calibration — scrim alpha rises on dark, the surface lightens one step, the hairline ring swaps to white-alpha.
- Ripple: follows [ELEVATION_MATERIAL_SYSTEM]; mirrors [NOTIFICATION_SYSTEM].

[NOTIFICATION_SYSTEM]-[QUEUED]: Toasts and banners become one severity system with modern mechanics.
- Capability: A pending/promise toast row that morphs in place under one correlation, stack presentation policy (visible ceiling with collapse, hover pause), a position policy column, and an inline banner owner genuinely distinct from toasts — persistent, placed, never auto-expiring — under one decision rule: transient outcomes toast, unresolved conditions banner, decisions dialog.
- Shape: `Shell/dialogs#NOTIFICATIONS` row/policy growth plus a banner family; `Shell/controls` banner materialization; `tokens#CONTROL_THEMES` toast/banner rows.
- Unlocks: long operations report honestly from pending to sealed outcome on one receipt correlation; degraded states persist visibly instead of expiring silently.
- Anchors: `ToastRow`/`ToastGate` admission fold with the `Held` register and cause-bearing seal; Ursa `WindowToastManager` message contract and `Banner` severity; `MotionPlan.Toast`; the sticky-derives-from-zero-linger law extending to a pending row that carries no expiry until it morphs.
- Ripple: follows [OVERLAY_MODALITY_CHROME]; mirrors [DARK_CHART_GRAMMAR].

[FORM_SECTION_GRAMMAR]-[QUEUED]: Forms read as professionally sectioned surfaces on every host.
- Capability: Section grouping with materialized labels — label/value rows at a settled ratio with uniform row height — plus unit-aware numeric fields, an overridden/default/invalid field-state vocabulary, the fill-slider interaction archetype (value-in-fill, steppers on hover, range form), and wizard step chrome; all schema-driven, so one form definition renders sectioned identically on desktop, embedded panel, and the wire head.
- Shape: `Editing/forms#FORM_SCHEMA` layout rebuild and `Editing/forms#WIZARD_FLOW` section shape; `Shell/controls` label case and slider variant counterparts; `tokens#CONTROL_THEMES` form rows.
- Unlocks: the flat one-panel form emission and the carried-but-never-materialized label column are deleted; every schema field renders labeled, sectioned, unit-correct, and state-bearing.
- Anchors: `FormField`/`FormSchema`/`FormSurface.Layout` with the wizard step grouping shape; `ControlIntent` container cases; Ursa `Form`/`FormItem`/`FormGroup` label geometry; UnitsNet `QuantityFormatter`/`UnitMath` with the base-units partiality trap; the measured label-quarter row geometry calibration.
- Ripple: follows [CONTROL_VARIANT_ALGEBRA]; mirrors [CONTENT_SURFACE_DARKENING].

[DARK_CHART_GRAMMAR]-[QUEUED]: The chart plane joins the token system and reaches dashboard grade.
- Capability: A chart-paint resolver making every chart chrome key a token fold — axis, crosshair, tooltip, legend, series palette — plus a threshold step-list family rendered identically by sections, gauges, cells, and state regions, a per-tile state union where reload holds the prior frame, stat-tile anatomy with sparklines, and the two missing visual primitives (lane/region series, polar stacked sectors) that unlock status timelines, wind and radiation roses, and schedule lanes.
- Shape: `Charts/dashboards`, `Charts/custom`, `Charts/telemetry`, `Charts/basemap` owners with `tokens#TOKEN_CATALOG` counterpart rows.
- Unlocks: dashboards, telemetry boards, and basemaps read one dark grammar; the light-mode package defaults and the ownerless resolved-at-mount prose die.
- Anchors: `ChartPolicy`/`ChartSection`/`DashboardTile`/`StatFold`; the LiveCharts theming rail (settings-level dark theme, per-family builder lists, crosshair and tooltip chrome paints, per-control theme override); `XamlRectangularSection`; Mapsui gradient thematics and widget colour members; the `CustomVisual` layout algebra; the dark chart grammar calibration — hairline grids one step off surface, muted non-flipping axis ink, surface-gap mark separation, focus-dim.
- Ripple: follows [DARK_IDENTITY_RAMP]; mirrors [NOTIFICATION_SYSTEM].

[MOTION_MICRO_INTERACTION]-[QUEUED]: Motion tokens gain their binding rail and the micro-interaction tier.
- Capability: A declared transition-binding law — which retained transition carries each animated axis — plus micro-interaction plan rows (hover, press, indicator slide, disclosure), the floating-chrome motion law (fade only, position never animated, exits shorter than entrances), and the composition-animation execution seam; everything still resolves through the one easing adapter and the one reduced-motion switch.
- Shape: `Theme/motion#MOTION_AXIS`/`motion#MOTION_APPLICATION` rows and a binding table; `Vfx/compose` counterpart.
- Unlocks: disclosure, segmented-indicator slide, hover/press deltas, elevation changes, and glow pulses all read plan rows; unowned animation dies corpus-wide, including the currently unanimated Editing and Document disclosure surfaces.
- Anchors: `Animatable.Transitions` with `BoxShadowsTransition`/`TransformOperationsTransition`/`EffectTransition`/`BrushTransition`; `TransformOperations.Parse` as the interpolable transform authoring form; `MotionEasing` feeding composition keyframes through the shared easing interface; the keyframe duration-poison trap (a zero duration corrupts the animation object — instant tokens set terminal values instead); reduced pairs render opacity-only.
- Ripple: follows [VFX_PLANE]; mirrors [CHROME_RAIL_FAMILY].

[CONTENT_SURFACE_DARKENING]-[QUEUED]: Editors, inspectors, markdown, and icons land on the one token rail.
- Capability: The code-editor chrome bridge — the grammar-theme gui-color map projected from the one resolve with dark current-line, selection, link, and ruler overrides and indent guides as an owned background renderer — plus inspector section chrome (category cards, the label-cell replacement seam, per-row class tags, token-aligned color palettes), markdown callout/fence/quote/list arms over a widened styling record, and the shipped Semi icon-geometry family joining the asset vocabulary.
- Shape: `Editing/inspector#INSPECTOR_SURFACE`/`inspector#EDITOR_FACTORIES`/`inspector#CODE_EDITING`; `Document/media#MARKDOWN_INLINES`; `Theme/assets#ICON_AXIS` rows.
- Unlocks: every content surface follows a theme swap with zero per-editor literals; markdown renders documentation-grade dark; a second shipped icon source becomes reachable.
- Anchors: the grammar-registry gui-color dictionary and per-key chrome read; editor styling properties with the custom-layer insertion rail; the property-grid label-cell event seam, class-tag attribute, and the local-value pinning fact that forces a control-theme route; `MarkdownStyling` widening; the SVG CSS cascade; the Semi icon path-geometry dictionary.
- Ripple: follows [DARK_IDENTITY_RAMP]; mirrors [FORM_SECTION_GRAMMAR].

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[DOCUMENT_SEARCH]-[COMPLETE]: landed as `libs/csharp/Rasm.AppUi/.planning/Document/search.md` — the closed query shape, source-coverage rows over the landed owners (notebook cells, media prose, issue comments, evidence payloads), ranked source-attributed results through the `VirtualWindow` fabric, highlight navigation on the `SearchStrategyFactory.Create` mint, and `SearchFault` on `AppUiFaultBand.Search` (6430) — WITH the Persistence counterpart landed the same pass: `Query/retrieval.md` `[06]-[DOCUMENT_CORPUS]` bm25 index lane and the `DocumentQuery`/`DocumentHit` wire named at both ends.
[SUPPORT_BUNDLE]-[COMPLETE]: blocker fell — AppHost `SupportManifest.Entry` gained the post-redaction, post-cap `ContentKey` (`Option<string>` hex, `None` on every no-byte arm including the bundle-cap drop) across every construction, and the AppUi golden leg pins content identity as the third fact of the `ProofLaw.BundleShape` pair; the pre-redaction `BundleMember.ContentKey` and the archive key agree exactly where nothing was masked or truncated, an inequality naming redaction or a cap rather than corruption.
[RAY_CONE_CURVATURE]-[COMPLETE]: the producer counterpart landed (`Rasm.Compute` `ResidencyMeshlet.Curvature`, measured normal-variation bound on `ResidencyPayload.Clusters`) and the whole consumer chain realized — `ResidencyMeshletView` projects the column, `SurfaceAttributes.Curvature` fills at hit resolve, `RayCone.Advanced(distance, curvature)` widens spread by `2 * curvature * distance` beside the distance term, and `MeshletWire`/`MeshletWireOf` carry the column under the bumped `Schema = 3`; a planar hit reads a measured 0 and widens by distance alone.
[HOST_SPRING_PARITY]-[COMPLETE]: the parity-map table landed in `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]-[MOTION_AXIS]` — `SpringValue` members (`Response`, `DampingFraction`, `Shape`) beside the host surface classes mirroring them, a host preset and a shell token evaluating the SAME kernel `SpringShape.OfResponse` closed form; the host preset table itself seats at the composition root per the branch spring-parity ruling, AppUi the parity source with zero surface change.
[COLLAB_SESSION_GOVERNANCE]-[COMPLETE]: landed as `libs/csharp/Rasm.AppUi/.planning/Collab/session.md` — `SessionRole`/`SessionCapability` `[SmartEnum<string>]` vocabulary with delegate-column grants, `MembershipState` total lifecycle rows, `MembershipOp` landing durably as `EditIntent.Membership`, and `SessionGate.Required`/`Admit` the total admission fold the `sync.md` merge authority composes ahead of `LedgerAppend`; rejections derive through the new `AppUiFaultBand.Session` (6530) registry row and presence stays a projection over the landed awareness channel.
[COLLAB_WIRE_CONTEXT]-[COMPLETE]: blocker fell — AppHost landed `Wire/topics` `Topic.Collab`/`Topic.Presence` and the generic `TraceContext` `Inject`/`Extract`/`Continue` spine whose boundary law seats a domain carrier's concrete bodies beside the consuming egress leg; `Collab/sync.md` `CollabCarrier` now binds the frame's getter/setter pair with `TenantAdoption.Adopted`, and the named `[COLLAB_DELTA_FEED]` cluster never existed — the topic rows are the reciprocal.
[MATH_TYPESET_OWNER]-[COMPLETE]: landed as `MathStyle`/`MathBox`/`MathTypeset`/`MathRun`/`MathInlineVisual` on `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `[02]-[MARKDOWN_INLINES]` — one painter serves the measure and the draw, the block and inline arms both materialize through `MathStyle`, math composites into the leased canvas through the settled `ICustomDrawOperation` vehicle, and the typography projection stays typeset-free as ruled.
[PROFILE_FLAME_JOIN]-[COMPLETE]: the AppHost `ProfileCapture.Bind` producer landed at `Observability/benchmarks.md` `[04]-[PROFILE_CORRELATION]`, so `FlameNode.Of` now folds published `ProfileSample` values; the row widened with `ThreadId`, `Kind`, and the `ProfileFrameForm` posture, and `FromSamples` lane-groups by thread with the posture on the lane name.
[TELEMETRY_BOARD_PAGE]-[COMPLETE]: store-profile tiles read the landed `SeriesKind.Telemetry` projection through `StoreProfileTrack`'s `(domain, slot, measure)` facets and one injected read arrow, so every board tile binds a producer.
[MULTI_MEASUREMENT_LEVELS]-[COMPLETE]: keyed level families ride the kernel `InstrumentSpec.Levels` declaration over `LevelCells.Reader`, declared beside each producer and swapped by the `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]` fan arms.
[COLLAB_FORENSICS]-[COMPLETE]: landed as `LiveWire.TapPreCommit`/`ExportJson` with the devloop `CollabPreCommit`/`CollabJson` verbs sealing `PreCommitFact` evidence in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md`.
[FRAME_BENCH_LANE]-[COMPLETE]: landed as `BenchLane`/`ProofEngine.Bench` with `ProofLaw.FrameBench`/`Divergence` gating through the AppHost `BenchmarkGate` in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`.
[TENANT_USAGE_FOLD]-[COMPLETE]: landed as `TenantUsage`/`TenantUsageFold.Fold` beside `EvidenceJoin` with the `TenantUsageWire` crossing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[04]-[CORRELATION_JOIN]`.
[FRAME_SLO_FEED]-[COMPLETE]: landed as the `ViewportObjectives` latency indicators in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`, consumed by `SloTiles`/`BurnFeed` over the kernel burn table.
