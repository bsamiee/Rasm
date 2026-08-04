# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from `IDEAS.md`; live-host probes and tool-gated arms track as blocked tasks against the substrate that unblocks them.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[SURFACE_LADDER_ROWS]-[QUEUED]: The two-ramp twelve-step dark surface ladder lands as paint rows on the frozen catalog.
- Capability: Surface identity as data — panel and overlay ramps, border alpha rungs, recessed input wells, a text-emphasis ladder, and the split accent pair, each a role-keyed row the resolve fold already knows how to serve.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Theme/tokens.md` `tokens#TOKEN_CATALOG` — new `Paint` anchor rows (panel/overlay surface anchors, border ladder, well, text-emphasis, accent-fill beside accent-text) with their ramp `Toward` columns, the dark-surround generation law on the `Boundary`, and `ThemeRail.ContrastCandidates`/`CvdCandidates` rows for every new pair in the same edit.
- Unlocks: `IDEAS.md [DARK_IDENTITY_RAMP]` — every downstream theme row resolves a real ladder key; the access sweep gates the new pairs because candidates land with them.
- Anchors: kernel `PerceptualColor.Ramp`/`Tone` with `BlendPath`/`GamutPolicy`; the `Anchored` two-endpoint span and accent tonal ladder already settled; Semi numbered-ramp slots; the host-matched-panel vs near-black-overlay calibration.
- Ripple: precedes [SEMI_SLOT_COMPLETION], [DEPTH_STACK_REBUILD], [CONTROL_THEME_ROWS]; mirrors [CHART_PAINT_RESOLVER].

[SEMI_SLOT_COMPLETION]-[QUEUED]: Every shipped Semi slot the resolve leaves untouched gains its correspondence row or its stated exclusion.
- Capability: Total slot custody — shadow, overlay, nav, focus-border, highlight, link, disabled-background/border, the remaining numbered background/fill rungs, secondary/tertiary role families, and the radius/width/height extents all mint from the resolve or carry an explicit shipped-pigment verdict, so boot coverage proves the whole correspondence.
- Shape: `tokens#TOKEN_CATALOG` `SemiSlots` rows plus `SemiCovered` consequences; exclusions stated on the cluster `Boundary`, never silently absent.
- Unlocks: `IDEAS.md [DARK_IDENTITY_RAMP]` — a theme swap re-skins the full Semi and Ursa control estate because the Ursa Semi theme binds the same slots; no shipped key lingers light-mode under a re-seeded neighbor.
- Anchors: the verified Semi key families (semantic roles with state rungs, numbered ramps, spacing/thickness/radius/height/icon-width extents, the single elevated-shadow token); the settled semantic-brush-layer-only ruling; `SemiSlot.Pigment` variant-inverting rungs.
- Ripple: follows [SURFACE_LADDER_ROWS]; precedes [DEPTH_STACK_REBUILD].

[DEPTH_STACK_REBUILD]-[QUEUED]: Depth rows become layered shadow stacks that actually reach controls.
- Capability: An elevation tier as one row carrying an ordered shadow-layer stack (offset, blur, spread, inset, color per layer) plus its z-rank, minting a `BoxShadows` resource every control theme binds — deleting the dead tuple that today reaches nothing.
- Shape: `tokens#TOKEN_CATALOG` — the `Depth` case widens to a layer sequence with tier rows spanning card, raised control, flyout, toast/floating, dialog/palette; `Resources` mints the `BoxShadows` value; the Semi per-control shadow keys re-emit from tiers in `SemiSlots`.
- Unlocks: `IDEAS.md [ELEVATION_MATERIAL_SYSTEM]` — dialogs/flyouts/toasts/rails bind tiers; elevation animates through the shadows transition without per-control literals.
- Anchors: `BoxShadow.Parse` grammar with spread and inset; `Border.BoxShadow`; `BoxShadowsTransition`; the ring-layer-first law; doubled dark alphas with the inset top-highlight rim recipe.
- Ripple: follows [SEMI_SLOT_COMPLETION]; precedes [DIALOG_CHROME_COLUMNS], [CONTROL_THEME_ROWS]; mirrors [VFX_FOLDER_PAGES].

[MATERIAL_WASH_ROWS]-[QUEUED]: Material and ambient-wash tokens land with an honest embedded story.
- Capability: A material row family — tint, tint opacity, noise amount, opaque fallback — plus scrim rows and the module-keyed ambient wash policy (hue per module, coverage fraction, luminance ceiling, crossfade span), all data the effects plane executes; the embedded truth is stated law: composited tint-plus-noise, never a hole punched through the host.
- Shape: `tokens#TOKEN_CATALOG` material/scrim rows and a wash policy row family; execution seam named toward `Vfx/material`.
- Unlocks: `IDEAS.md [ELEVATION_MATERIAL_SYSTEM]` — panels and overlays request materials by row; the wash gives each mode a live identity with zero accent spend.
- Anchors: `ExperimentalAcrylicMaterial` tint/material-opacity semantics with the noise-fill truth and `Digger` hazard; `TopLevel.TransparencyLevelHint` list semantics for the owned-window enhancement; the reduce-transparency preference gate; Unicolour `Blend` scrim math.
- Tension: the wash must never brighten past its luminance ceiling or it fights text contrast — the ceiling is a row value the contrast candidates gate.
- Ripple: follows [DEPTH_STACK_REBUILD]; precedes [VFX_FOLDER_PAGES].

[VFX_FOLDER_PAGES]-[QUEUED]: The Vfx sub-domain lands as three decision-complete pages with folder registry rows.
- Capability: Owned effects operations — a backdrop capsule (lease, save-layer backdrop blur, tint and grain draw, restore), SkSL shader owners (glow, noise/grain, wash gradient with per-frame uniforms), and the composition adapter (element visual acquisition, keyframe and implicit animation mint, duration floor, reduced-motion collapse) — each page a blueprint with signature fences over verified members.
- Shape: new `libs/csharp/Rasm.AppUi/.planning/Vfx/material.md`, `Vfx/shader.md`, `Vfx/compose.md`; router rows in `libs/csharp/Rasm.AppUi/README.md` `[01]-[ROUTER]` and codemap nodes in `libs/csharp/Rasm.AppUi/ARCHITECTURE.md` in the same pass.
- Unlocks: `IDEAS.md [VFX_PLANE]` — the completeness bar: every glow/wash/backdrop/pulse consumer in the corpus names a Vfx owner and no sibling page carries raw Skia effect code.
- Anchors: `ISkiaSharpApiLeaseFeature` lease members; `SKCanvasSaveLayerRec`/`SKRuntimeEffect`/`SKMaskFilter`/`SKShader` verified rosters; `Compositor` factories with `CompositionCustomVisualHandler`; the settled `ICustomDrawOperation` precedent rows in the SVG and basemap catalogs.
- Route: live render-hash probe of backdrop sampling under partial repaint before `material.md` finalizes; the opaque tinted floor ships beside the SPIKE marker.
- Ripple: follows [MATERIAL_WASH_ROWS]; mirrors [MOTION_BINDING_ROWS].

[CONTROL_VARIANT_COLUMNS]-[QUEUED]: Button and Slider intents gain variant, icon, and pending axes riding the one binding shape.
- Capability: Emphasis variants (quiet, secondary, primary, danger, inverted), an optional icon slot resolved through the one asset rail, and a pending column that renders in-control progress — as intent columns, so the wire carries them and both heads materialize identically; the fill-slider archetype (value-in-fill, hover steppers, range) is a slider variant row, not a new control.
- Shape: `Shell/controls#CONTROL_INTENT` column widening on `Button`/`Slider`, `Shell/controls#MATERIALIZE_FOLD` `Apply` gaining theme-by-variant resolution and an icon resolver context column, `Shell/controls#TS_PROJECTION` field additions; `Refresh` arms re-dress the new columns.
- Unlocks: `IDEAS.md [CONTROL_VARIANT_ALGEBRA]` — CTAs, rail buttons, and form sliders are rows over one entry point; no `GetPrimaryButton` proliferation ever exists.
- Anchors: `StyledElement.Theme` resolution from the theme dictionary; `IconSurface.Resolve` with the FluentIcons size/variant axes; the pending-state calibration (width-stable spinner replacing the leading visual, aria-disabled semantics).
- Ripple: follows [SURFACE_LADDER_ROWS]; precedes [CONTROL_CASE_ADDITIONS], [FORM_SECTION_ROWS].

[CONTROL_CASE_ADDITIONS]-[QUEUED]: Label, Segmented, and EmptyState join the closed union with total arms.
- Capability: A static label case (role-keyed text, the one non-interactive intent), a segmented mode-switch case (two-to-N segments, icon-or-label, slab/chips/thumb presentation modes, roving focus, immediate-apply semantics distinct from the form-value radio), and an empty-state case (icon, title, hint, one primary action) — each with visual arm, pooling verdict, refresh arm where poolable, wire literal, and automation identity derived from the key.
- Shape: `Shell/controls#CONTROL_INTENT` three cases; `Shell/controls#MATERIALIZE_FOLD` arms; `ControlTypeOf` verdicts; `Shell/controls#TS_PROJECTION` kind literals.
- Unlocks: `IDEAS.md [CONTROL_VARIANT_ALGEBRA]` — form label/value rows, mode toolbars, toggle-pairs, and zero-data panels compose from cases; the completeness bar is that no target pattern needs a twenty-second case.
- Anchors: the union's compile-time totality (a new case breaks every switch); the segmented thumb anatomy (padded track one step down, thumb one step up, indicator slide); Ursa `SelectionList` indicator part and `ButtonGroup` as context-column candidates; the stroke-only empty-state grammar.
- Ripple: follows [CONTROL_VARIANT_COLUMNS]; precedes [CHROME_RAIL_ROWS], [PALETTE_PRESENTATION].

[CONTROL_THEME_ROWS]-[QUEUED]: The control-theme table grows the professional-dark skin rows with a stated composition law.
- Capability: Theme rows for rail button, segmented item and indicator, the CTA variant family, form row, banner, palette row, status chip, and toast card — each naming pseudo-classes and token keys — plus the previously unstated composition law: target type, base-theme derivation, nested state styles, custom pseudo-class vocabulary, and the double-ring focus recipe rendered only on keyboard focus.
- Shape: `tokens#CONTROL_THEMES` table rows and `Boundary` law additions (ControlTheme `TargetType`/`BasedOn`/nested children; `PseudoClasses.Set` product states; focus ring gap painted in parent surface color).
- Unlocks: `IDEAS.md [DARK_IDENTITY_RAMP]` — every new control shape has its skin row before any fence lands; state deltas follow the settled hover/press/selected numeric table.
- Anchors: verified Semi named button-variant themes as `BasedOn` targets; the Ursa Semi icon-button theme keys; the state-delta calibration (pressed sinks, border deltas outsize fill deltas, disabled floor).
- Ripple: follows [DEPTH_STACK_REBUILD], [CONTROL_CASE_ADDITIONS]; mirrors [TOAST_BANNER_FAMILY].

[CHROME_RAIL_ROWS]-[QUEUED]: The floating rail and mode toolbar land as chrome rows over routes with a rail layout program.
- Capability: A rail chrome slot whose rows are intent keys with icon and badge columns, collapse geometry riding breakpoints, flyout-group markers, and overflow promotion; the mode toolbar as the same family rendering segment chips over catalog routes — mode state is navigation, presentation is chrome, commands stay on the deck.
- Shape: `Shell/navigation#SHELL_CHROME` slot case and row columns with visibility-matrix cells per mount; `Shell/solver#LAYOUT_PRESETS` rail program rows with gap metric keys; `Shell/controls` materialization through a composition-bound rail context column.
- Unlocks: `IDEAS.md [CHROME_RAIL_FAMILY]` — Rhino panel, GH2 companion, and standalone mount the identical rail; collapse and reveal read motion plan rows.
- Anchors: `ChromeRow` intent-key law; `ShellRoot.Freeze` route projection; `AdaptiveBehavior` breakpoint classes; Ursa `NavMenu`/`IconButton` verified properties; the dock-zone/orientation overlay parameter spine.
- Ripple: follows [CONTROL_CASE_ADDITIONS]; precedes [STATUS_FOOTER_PANES]; mirrors [MOTION_BINDING_ROWS].

[STATUS_FOOTER_PANES]-[QUEUED]: The quiet status footer lands as a bounded pane vocabulary with slot and kind metadata.
- Capability: A three-zone footer — left context facts, center transient report and running-task progress, right toggle panes — where each pane is a row carrying kind (readout, toggle, dropdown), zone, and its fact source, so panes never proliferate as bespoke widgets and the severity hue lands on glyphs alone.
- Shape: `Shell/navigation#SHELL_CHROME` status-slot pane rows; `Shell/screens` consumption in body composition; theme rows for the pane chip states.
- Unlocks: `IDEAS.md [CHROME_RAIL_FAMILY]` — every host shows the same honest status line; transient reports auto-dismiss through motion rows.
- Anchors: the one-fact-stream slot/kind manifest rule; the pane roster calibration (coordinate/unit/snap/filter archetypes); `ChromeSlot` growth law.
- Atomic: one slot case plus its pane row family and theme row.
- Ripple: follows [CHROME_RAIL_ROWS].

[HUD_CHIP_ROWS]-[QUEUED]: In-viewport HUD chips land as a corner-anchored chip row family with an injected camera seam.
- Capability: Quiet translucent chips — zoom, view name, coordinates, section state, stats — as rows carrying corner, plate style, and chip kind, with camera-fact content bound through injected observables so the chip family itself stays host-agnostic and viewport-free.
- Shape: `Shell/screens` chip row family beside the body composition; the camera seam named as a delegate column toward the render plane; theme chip rows.
- Unlocks: `IDEAS.md [CHROME_RAIL_FAMILY]` — viewport-adjacent surfaces read one chip vocabulary; the diagnostics HUD composes the same rows.
- Anchors: the eight-position corner grid; the fused-plate corner treatment; tabular numerals from the numeric typography role; elevation-integer layering.
- Atomic: one row family, one seam column, one theme row.
- Ripple: follows [STATUS_FOOTER_PANES]; mirrors `Diagnostics/devloop` HUD consumption.

[PALETTE_PRESENTATION]-[QUEUED]: The command palette presentation lands over Federate with widened hits and an action sub-panel.
- Capability: Hit rows carrying icon, secondary text, gesture chips, and kind badges; a top-anchored overlay surface with search, grouped results, footer hints, and empty/loading states; a per-hit action sub-panel resolving contextual command keys — while ranking, scoping, and invocation stay exactly where they are.
- Shape: `Shell/commands#PALETTE_AND_REMOTE` hit-column widening plus one presentation cluster; `Shell/dialogs#SESSION_ALGEBRA` Ursa-overlay seating row; `tokens#CONTROL_THEMES` palette and keycap rows; the search-source badge closes the palette provider's dropped-source gap at `Document/search#RANKED_WINDOW`.
- Unlocks: `IDEAS.md [COMMAND_PALETTE_SURFACE]` — the full surface; the deck gesture column renders as keycaps with zero palette-local command state.
- Anchors: `CommandProjections.Federate`/`Score`; `PaletteHit`; `OverlayDialogHost` mask; `GesturePolicy` scope narrowing; the palette geometry archetype.
- Ripple: follows [CONTROL_CASE_ADDITIONS]; mirrors [DIALOG_CHROME_COLUMNS].

[DIALOG_CHROME_COLUMNS]-[QUEUED]: Dialog topology styling re-points to the elevation and material families and Confirm gains its typed destructive shape.
- Capability: Overlay scrim, corner, ring, and blur columns resolving tier and material rows; a verification-phrase column pair on the confirm intent whose affirmative disables until the typed phrase matches the target identifier; the drawer positioner variant and the peek verb ripple stated at both ends.
- Shape: `Shell/dialogs#SESSION_ALGEBRA` column re-pointing with per-row values across the six topology rows; `Shell/dialogs#DIALOG_INTENTS` Confirm widening; `Shell/navigation#ROUTING_SPINE` peek verb counterpart.
- Unlocks: `IDEAS.md [OVERLAY_MODALITY_CHROME]` — every modal surface reads tiers; destructive friction is typed, never a convention.
- Anchors: `DialogTopology` token-key columns; `DialogHostStyle` BoxShadow attachment; the dark scrim calibration; `NavRequest` verb growth law.
- Ripple: follows [DEPTH_STACK_REBUILD]; precedes [TOAST_BANNER_FAMILY].

[TOAST_BANNER_FAMILY]-[QUEUED]: The pending toast row, stack policy, and the banner owner land as one severity system.
- Capability: A pending row that morphs to its terminal severity in place under one correlation with the timer starting at morph; stack ceiling and hover-pause as presentation policy; a position column; and an inline banner family — severity, dismissibility, action key, placement — persistent by construction and materialized as a control arm, never a toast variant.
- Shape: `Shell/dialogs#NOTIFICATIONS` row and policy growth plus the banner family; `Shell/controls` banner materialization through a context column; `tokens#CONTROL_THEMES` toast and banner rows.
- Unlocks: `IDEAS.md [NOTIFICATION_SYSTEM]` — promise flows and persistent conditions each have an honest owner; receipts stay double-count-proof because the morph re-emits under the admission correlation.
- Anchors: `ToastRow` severity/linger columns with the sticky derivation; `IMessage` click/close contract; Ursa `Banner` severity type; the transient-vs-persistent decision rule.
- Ripple: follows [DIALOG_CHROME_COLUMNS]; mirrors [CONTROL_THEME_ROWS].

[FORM_SECTION_ROWS]-[QUEUED]: Form layout emits sectioned label/value trees and fields gain units and override state.
- Capability: A section row (title key plus field-key roster, the wizard-step shape without the gate) driving a nested emission — section headers, label/value rows at the settled ratio, intra-section rules — plus a unit column resolving through the quantity formatter and a field-state vocabulary (default, overridden, invalid) rendered as ink states; labels materialize through the new label case.
- Shape: `Editing/forms#FORM_SCHEMA` section rows and `Layout` rebuild; `Editing/forms#WIZARD_FLOW` step-chrome projection; counterpart columns landed at `Shell/controls#CONTROL_INTENT` per [CONTROL_CASE_ADDITIONS].
- Unlocks: `IDEAS.md [FORM_SECTION_GRAMMAR]` — one schema renders the professional sectioned look everywhere; the label column stops being dead data.
- Anchors: `FormField.LabelKey`; `FormSurface.Layout` as the one materialization; UnitsNet formatter members with the display-unit election law; the override-state ink calibration.
- Ripple: follows [CONTROL_VARIANT_COLUMNS], [CONTROL_CASE_ADDITIONS]; mirrors [EDITOR_INSPECTOR_CHROME].

[CHART_PAINT_RESOLVER]-[QUEUED]: Chart chrome keys resolve through one token fold and the dark theme registers at composition.
- Capability: A paint-resolver owner folding every chart string key — paint family, grid, label, crosshair, tooltip, legend — from the resolved theme on the typed rail, with the axis chrome widened past the single grid role (separators, subseparators, ticks, zero line, crosshair label chip) and the package dark theme plus series palette registered once at composition from the colormap vocabulary.
- Shape: `Charts/dashboards` policy and axis clusters plus one resolver owner; composition registration row; `tokens#TOKEN_CATALOG` chart chrome rows as the counterpart.
- Unlocks: `IDEAS.md [DARK_CHART_GRAMMAR]` — every chart surface re-tints on theme swap; the undeclared series palette and light package defaults die.
- Anchors: the verified LiveCharts theming rail and axis/crosshair paint members; `ChartPolicy` twelve-field row; `Colormap` qualitative rows; the dark chart grammar calibration.
- Ripple: follows [SURFACE_LADDER_ROWS]; precedes [CHART_THRESHOLD_FAMILY], [TILE_STATE_UNION].

[CHART_THRESHOLD_FAMILY]-[QUEUED]: One threshold step-list model renders as bands, gauge fills, cell backgrounds, and state regions.
- Capability: An ordered step list (base plus steps, absolute-or-percentage mode) as one row family every renderer projects — axis bands with labels on both axes, gauge fill arcs, table cell backgrounds, alert state regions — so thresholds are declared once and read identically everywhere.
- Shape: `Charts/dashboards` section widening (both-axis coordinates, label columns) plus the step-list family beside `WatchRule`; `Charts/telemetry` tile consumption rows.
- Unlocks: `IDEAS.md [DARK_CHART_GRAMMAR]` — alert chrome, gauges, and bands agree by construction; a per-panel threshold block is unrepresentable.
- Anchors: `XamlRectangularSection` verified members; `WatchFold.Arm`; the six-arm band-rendering vocabulary calibration.
- Atomic: one row family plus consumption rows.
- Ripple: follows [CHART_PAINT_RESOLVER].

[TILE_STATE_UNION]-[QUEUED]: Every dashboard tile carries one state union and the stat tile gains its full anatomy.
- Capability: A per-tile state union — loading holds the prior frame at reduced opacity, no-data states the gap honestly, error dims with retry, too-small drops elements progressively — plus stat-tile anatomy (value, delta with polarity, trend caption, background sparkline) and the axis-less sparkline primitive shared by tiles and table cells.
- Shape: `Charts/dashboards` tile union widening and state rows; `Charts/telemetry` board tile consumption; the sparkline as a `Charts/custom` visual row.
- Unlocks: `IDEAS.md [DARK_CHART_GRAMMAR]` — boards read professionally under load, absence, and fault; the withheld burn-rate tick gains a declared visual.
- Anchors: `DashboardTile`/`StatFold` growth laws; the refetch-holds-frame and progressive-drop calibration; the delta-polarity axis.
- Ripple: follows [CHART_PAINT_RESOLVER]; mirrors [CHART_LANE_POLAR_PRIMITIVES].

[CHART_LANE_POLAR_PRIMITIVES]-[QUEUED]: The lane/region series and polar stacked sectors land as custom visual rows.
- Capability: Two chart-hosted primitives — horizontal state lanes over a time or ordinal axis (merge-equal-consecutive, value labels, null gaps) and polar stacked sectors (binned petals, calm center, sector hover) — as rows on the custom visual algebra, unlocking status timelines, uptime strips, wind and radiation roses, Gantt lanes, and sankey groundwork without a second chart rail.
- Shape: `Charts/custom` visual rows with payload cases and layout folds; `Charts/dashboards` series-spec rows referencing them.
- Unlocks: `IDEAS.md [DARK_CHART_GRAMMAR]` — the completeness bar: the named AEC and ops chart classes render from data rows with zero bespoke drawing code outside the custom plane.
- Anchors: `CustomVisual` fourteen-row precedent with `VisualPayload` union growth; the chart-hosted draw pass law (chart semantics stay data-driven on one rail); `Colormap` class discipline for petals and lanes.
- Ripple: follows [TILE_STATE_UNION].

[BASEMAP_DARK_ROWS]-[QUEUED]: The basemap gains dark rows and its widget construction defect falls.
- Capability: A dark tile-source row beside the light one, the map background color row, widget re-tints from the token resolve, and a choropleth seam projecting the colormap vocabulary through gradient thematics — plus the structural fix: widget source columns take the map argument their constructors demand.
- Shape: `Charts/basemap` layer-row additions, widget column signature fix (`Func` over the map), `MapNav` untouched; token counterpart rows.
- Unlocks: `IDEAS.md [DARK_CHART_GRAMMAR]` — the geo plane matches the dark identity instead of shipping light package defaults; overlays graduate from hand-built styles to ramp-driven thematics.
- Anchors: verified widget colour members and constructor arities; `GradientTheme`/`ColorBlend`; the no-tile-filter truth (a dark basemap is a source swap, never a post-effect).
- Ripple: follows [CHART_PAINT_RESOLVER].

[EDITOR_INSPECTOR_CHROME]-[QUEUED]: The code editor and property inspector land on the token rail with section-grade chrome.
- Capability: The editor chrome bridge — gui-color dictionary keys projected from the one resolve, dark current-line and selection overrides, link/ruler/caret inks, indent guides as an owned background renderer on the custom layer rail — and inspector section chrome: category cards through a control theme (the local-value pinning makes style setters lose), the label-cell seam bound, per-row class tags, and the picker palette family re-cut onto theme tokens.
- Shape: `Editing/inspector#CODE_EDITING` bridge rows and renderer owner; `Editing/inspector#INSPECTOR_SURFACE` chrome policy; `Editing/inspector#EDITOR_FACTORIES` palette alignment.
- Unlocks: `IDEAS.md [CONTENT_SURFACE_DARKENING]` — code and property surfaces follow every theme swap with zero per-editor literals; the light-tuned current-line defaults never render.
- Anchors: the verified editor styling properties and layer-insertion rail; the grammar-theme color dictionary and per-key read; the label-cell and operation-cell event seams; the class-tag attribute application point.
- Ripple: follows [SURFACE_LADDER_ROWS]; mirrors [FORM_SECTION_ROWS].

[MARKDOWN_DARK_ARMS]-[QUEUED]: The empty markdown arms materialize with a widened styling record.
- Capability: Callout, code-fence, quote, list, and rule arms render — callout tint keyed on its kind, fence on a recessed mono surface, quotes and lists with structural inks — over a styling record widened past its single ink to carry the surface, border, and per-kind tint slots the arms read.
- Shape: `Document/media#MARKDOWN_INLINES` arm materialization and `MarkdownStyling` widening; token counterpart rows for callout/fence inks.
- Unlocks: `IDEAS.md [CONTENT_SURFACE_DARKENING]` — notebook and document panels render documentation-grade dark; the retained-payload empty projections die.
- Anchors: the eleven-arm row dispatch with its explicit routing-verdict law; the callout kind discriminant; the code typography role; grammar-scope truth that fences admit only the registered grammars.
- Atomic: arm set plus one record widening.
- Ripple: follows [EDITOR_INSPECTOR_CHROME].

[MOTION_BINDING_ROWS]-[QUEUED]: The transition-binding law and micro-interaction plan rows complete the motion vocabulary.
- Capability: A binding table naming which retained transition carries each animated axis (brush, shadow stack, transform operations, effect), micro-interaction plan rows — hover, press, indicator slide, disclosure — with the floating-chrome law (fade only, position never animated, exits shorter), and the composition execution seam with its duration floor and instant-token terminal-value rule.
- Shape: `Theme/motion#MOTION_APPLICATION` plan rows and the binding table; `Theme/motion#MOTION_AXIS` boundary additions; `Vfx/compose` counterpart seam.
- Unlocks: `IDEAS.md [MOTION_MICRO_INTERACTION]` — every animated surface names its transition and plan row; the unowned Editing/Document disclosure animations gain their owner.
- Anchors: the verified transitions roster; `TransformOperations` authoring form; `MotionEasing` crossing into composition keyframes unchanged; the keyframe duration-poison trap; reduced pairs opacity-only.
- Ripple: follows [VFX_FOLDER_PAGES]; mirrors [CHROME_RAIL_ROWS], [CONTROL_THEME_ROWS].

[ICON_SOURCE_COMPLETION]-[QUEUED]: The shipped Semi icon geometries and the icon size axis join the asset vocabulary.
- Capability: The path-geometry dictionary the theme already ships becomes an addressable icon source with token-tinted materialization, and the icon size axis gains the FluentIcons size member so rail, palette, footer, and banner glyph needs resolve from registered rows instead of ad-hoc choices.
- Shape: `Theme/assets#ICON_AXIS` source registration and size-axis note; icon rows for the chrome vocabulary; `Shell/controls` icon resolver consumption per [CONTROL_VARIANT_COLUMNS].
- Unlocks: `IDEAS.md [CONTENT_SURFACE_DARKENING]` — two shipped icon sources serve every chrome surface through one resolve; no unregistered glyph source exists.
- Anchors: the ranked five-case icon union with its generated rank law; the geometry-resource extraction truth; the icon-size off-axis gate.
- Atomic: source rows plus the size-axis addition.
- Ripple: follows [CONTROL_VARIANT_COLUMNS].

[GRAPH_CANVAS_THEME_ROWS]-[QUEUED]: The node-editor canvas joins the dark identity through its variant theme keys and the undefined-key closure.
- Capability: Graph chrome — node, pin, connector, crossing, rejection, and guide inks — re-seeded per variant from the token resolve through the package's variant-keyed theme dictionaries, and the consumed-but-undefined resource keys (editor backgrounds and chrome icons) supplied from the token and asset vocabularies so the canvas never renders blank chrome.
- Shape: `libs/csharp/Rasm.AppUi/.planning/Editing/graph.md` canvas cluster theming rows; `tokens#TOKEN_CATALOG` counterpart inks; icon keys through `Theme/assets#ASSET_CATALOG` rows.
- Unlocks: `IDEAS.md [CONTENT_SURFACE_DARKENING]` — the parametric canvas follows every theme swap; the shared guide/selected-stroke key gets split so recoloring one never recolors the other.
- Anchors: the variant-keyed theme-dictionary override law (replace the type-keyed control theme or redefine the key — no code-level color API); the per-variant key families in the repaired catalog; the routing-policy row already owning geometry.
- Atomic: theme-key rows plus the undefined-key closure.
- Ripple: follows [SURFACE_LADDER_ROWS].

[PANANDZOOM_ALIAS_GUARD]-[QUEUED]: The duplicated pan-zoom type surface resolves to one addressable owner.
- Capability: The zoom-border type reaches consumers from exactly one assembly — the transitive copy under the node-editor package and the directly admitted package stop colliding, through an alias or exclusion row on the owning reference so every future fence names the type unambiguously.
- Shape: `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj` reference row; the boundary note at `Editing/graph.md` naming the resolved owner.
- Unlocks: `IDEAS.md [CONTENT_SURFACE_DARKENING]` — graph and dashboard pan-zoom fences compile against one type identity instead of arming a two-assembly ambiguity.
- Anchors: the extern-alias parity precedent for duplicated geometry packages; the repaired node-editor catalog's admission row naming the collision.
- Atomic: one reference-row change plus one boundary note.
- Ripple: mirrors [GRAPH_CANVAS_THEME_ROWS].

[DOCK_KEY_CLOSURE]-[QUEUED]: Dock chrome keys the skin consumes but never defines mint from the token resolve.
- Capability: The workbench-surface and separator brushes the dock skin binds without defining land as token-minted resources, and the dock key vocabulary joins the elevation and surface families so a variant flip re-tints dock chrome whole through the dynamic-resource re-resolution it already rides.
- Shape: `tokens#TOKEN_CATALOG` dock-key emission rows beside the `SemiSlots` correspondence; the dock-theme boundary note at `Shell/navigation#DOCK_LAYOUTS`.
- Unlocks: `IDEAS.md [ELEVATION_MATERIAL_SYSTEM]` — dock chrome never renders an unset brush; the palette override re-tints the full docking estate with zero dock-side edits.
- Anchors: the repaired dock catalog's skin-slot tables and consumed-but-undefined finding; the dynamic-resource binding law the skin already follows; the settled dock-theme-manager composition seam.
- Atomic: two key mints plus the vocabulary rows.
- Ripple: follows [DEPTH_STACK_REBUILD]; mirrors [GRAPH_CANVAS_THEME_ROWS].

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[INSTRUMENT_WRITER_CLOSURE]-[COMPLETE]: the card's six-page "unwritten" roster was stale for five of six (producers verified via the `Count` delegate and the evidence fan); the real remainder landed whole — `Editing/livedata.md` `Audit` fences the `CollectUpdateStats` -> `ChangeSummary` fold sealing `EvidenceReceipt.LiveData` per slot (the last producer-less instrument, sealing `Latest` never `Overall` so the fan cannot double-count), two same-class instances found and closed (`Shell/dialogs.md` toast admission/dismissal `InstrumentSpec` rows + `Observe(InstrumentSet, ToastReceipt)`; `Render/immersive.md` `Observe` pair beside its `TelemetryRow`), and the dimension-consistency half holds corpus-wide — `Shell/navigation.md` declares the verb dimension both `NavigateInstrument`/`RouteMissInstrument` writes tag through the dispatch state slot, and every touched page verified declaration slots == writer tag keys == description.
[SEARCH_QUERY_SHAPE]-[COMPLETE]: the closed `SearchQuery` shape, source-coverage rows, and the `DocumentQuery`/`DocumentHit` consumed wire landed in `libs/csharp/Rasm.AppUi/.planning/Document/search.md` `[02]-[QUERY_SHAPE]`/`[06]-[INDEX_WIRE]`, the seam ceiling admitted at BOTH ends against one constant and the local leg building the wire shape through the same decode gate the store's answers take.
[BUNDLE_MANIFEST]-[COMPLETE]: the AppHost manifest carries content identity — `SupportManifest.Entry.ContentKey` landed across `Written`/`Faulted`/`Capped` with absence spelled `None` on every no-byte arm, the `ManifestContract` carrying the column whole to `SupportCaptureWire.Entries`, and the AppUi `ProofLaw.BundleShape` golden pair now pins content identity through the extracted `manifest.json`.
[SPRING_PARITY_MAP]-[COMPLETE]: the three-row parity map landed in `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]-[MOTION_AXIS]` with the six `MotionToken` spring values as the parity values and the composition-root seating stated per the branch ruling.
[SESSION_ROLE_ROWS]-[COMPLETE]: the role/capability vocabulary, `MembershipState` lifecycle rows, and the `SessionGate` admission seam landed whole in `libs/csharp/Rasm.AppUi/.planning/Collab/session.md` with the gate folded into `sync.md` `IntentLedger.Project` ahead of `LedgerAppend`; rejections derive through `AppUiFaultBand.Session` (6530), not a `Collab` code — session governance is a distinct band from sync merge faults.
[SHADER_COMPILE_METER]-[COMPLETE]: the recording projection landed beside the compile fold — `Render/shading.md` `[02]-[SHADER_ASSET]` `Observe(InstrumentSet, ShaderReceipt)` writes the compiled-or-failed row under `AppUiTelemetry.BackendSlot` and drives the whole six-row `TelemetryRow` contribution, plane admissions fanned mint/reuse/refuse on `AppUiTelemetry.OutcomeSlot` so the byte-ceiling and layered-chain refusals count as a partition of one keyed series; the corpus-wide half of the claimed bar was false and `[INSTRUMENT_WRITER_CLOSURE]` owns the real remainder.
[MATH_ENGINE_VERIFY]-[COMPLETE]: the painter path is proven at fence depth — the `LaTeX` setter's `Result` parse rail probed through `ErrorMessage`, `Measure(float)`, the absolute-origin `Draw(SKCanvas, SKPoint)` into the `DrawSource.Borrowed` lease, and the headless `DrawAsStream(width, SKEncodedImageFormat.Png, quality, TextAlignment)` encode, every member verified against `.api/api-csharpmath-skia.md`; the two residual unknowns (the `LocalTypefaces` element-type bridge, the aligned-draw parameter semantics) carry as research rows on the owning page rather than blocking the arm.
[CAPTURE_ROW_FACTORY_CALLS]-[COMPLETE]: both twins re-key the proof owner's `RenderHashLane` and mint through `RenderHashLane.Row` — `CustomVisual.RenderTwin` and `ChartSeriesSpec.Baseline` now return `Fin<CaptureRow>` carrying the lane's gamut and tick policy, so the bare four-argument construction against the five-column private constructor is gone at both sites.
[BOARD_TILE_ROWS]-[COMPLETE]: tile-row table [01]-[10] and the `TelemetryBoard` spine landed in `libs/csharp/Rasm.AppUi/.planning/Charts/telemetry.md` `[02]-[BOARD_ROWS]`.
[LEVEL_KIND_ROW]-[COMPLETE]: keyed `LevelCells` families and their fan-arm swaps landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`; the kind roster and its `Levels` declaration home at the kernel instrument mechanism.
[WIRE_CONTEXT_FIELDS]-[COMPLETE]: carriage landed as `CollabWireContext`/`CollabFrame` frame metadata in `libs/csharp/Rasm.AppUi/.planning/Collab/sync.md` `[04]-[LIVE_WIRE]` with the ingest edge re-emitting envelope-carried context unchanged; the `CommitWith` origin-metadata claim was refuted — `CommitOptions` seals only change origin, timestamp, and message.
[PRECOMMIT_TAP]-[COMPLETE]: `SubscribePreCommit` tap and `ExportJsonUpdates` window export landed as `LiveWire.TapPreCommit`/`ExportJson` with the devloop `CollabPreCommit`/`CollabJson` verbs.
[BENCH_MATRIX_ROW]-[COMPLETE]: `BenchLane` derivation and the `FrameBench`/`Divergence` gate landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`, judged through the AppHost `BenchmarkGate` with the dashboards benchmark layout fed off the envelope stream.
[FLAME_SOURCE_PORT]-[COMPLETE]: `ProfileSampleSource` reads the AppHost `UiSchedulerPort.ProfileSamples` feed by correlation, and `FlameNode.Of` prefix-merges its AppHost-owned `ProfileSample` values in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md` `[02]-[DEV_LOOP]`.
[USAGE_RECEIPT_SHAPE]-[COMPLETE]: `TenantUsage` record, envelope-partition fold, and `TenantUsageWire` landed in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[04]-[CORRELATION_JOIN]`.
[SLO_COORD_PIN]-[COMPLETE]: superseded — the viewport indicators land as `ViewportObjectives` `Sli.Latency` rows against the composed frame budget in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`, and the window, factor, severity, and burn fold derive from the kernel SLO algebra.
[VIEWPORT_PACK_ON_PORT]-[COMPLETE]: viewport reliability policy rides the contributor port — `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]` collapsed `ViewportObjectives` onto one `Pack(FrameBudget)` entry binding panels beside objectives off one row table, `AppUiTelemetry.Contribute` gained its pack-bearing twin, `Render/pipeline.md` `RenderGraph.TelemetryRow(version, budget)` carries that pack on the port declaring the three viewport series, and `Charts/telemetry.md` `SloTiles.Rows` reads its objectives.
[SIGNAL_CAPSULE_COMPOSE]-[COMPLETE]: the folder `InstrumentKind`/`InstrumentSpec` twins and the `SloCoordinate` burn partial deleted against the kernel mechanism and SLO algebra; `AppUiTelemetry` keeps scope identity, the dimension-slot vocabulary, contribution, and mount, with every sibling `TelemetryRow` re-cut onto them. Kind vocabulary collapsed onto the `[JsonDerivedType]` roster behind `EvidenceOps.KindOf`/`Kinds`/`Probe`, `EvidenceFan` parks refused measurements on a composition cell, `TenantUsageFold` accrues off the decoded union, and `ViewportObjectives` dropped its window knob.
