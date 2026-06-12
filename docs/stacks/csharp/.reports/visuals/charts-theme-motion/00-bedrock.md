# charts-theme-motion — bedrock

## chart law — the series-spec table

- One charting surface, two consumption profiles: the retained UI controls (Cartesian/Pie/Polar/Map families) and the in-memory headless family — the render projection owner is selected by process modality.
- The headless family (`InMemorySkiaSharpChart`: `GetImage()` → `SKImage`, `SaveImage(stream, format, quality)`, `DrawOnCanvas(SKCanvas)`, `Background`, `Width`/`Height`) renders the same series/axis values with zero UI shell — chart evidence feeds the render-hash law through one code path.
- `SKCartesianChart`/`SKPieChart`/`SKPolarChart`/`SKGeoMap` are the headless rows mirroring the control rows one-to-one — modality is a constructor choice, never a chart rewrite.
- Both profiles stand on source-generated chart bases: the declarative property surface is generated onto shared bases, so control rows and headless rows expose the same property vocabulary by construction — profile parity is generated, not maintained.
- Gauges decompose into series rows — value, background, needle, ticks are each series-table data — gauge chrome is data on the one table, and a custom gauge is row composition, not a control.
- Overlay visual elements carry their own pointer event row — annotation interaction is an event on the element collection, never canvas-level hit math.
- The series-spec table is the collapse: a closed series-kind vocabulary (column, row, line, step-line, scatter, candlestick, box, heat, pie, polar-line, stacked-area, stacked-column, gauge rows) dispatches onto four canvas families via one key column.
- One spec record — (kind, values source, value projection, paint token keys, geometry knobs) — materializes the concrete series row; per-chart wrapper controls and any second charting package are the deleted forms.
- Data-driven series generation deletes per-series code-behind: the source+template rows generate series from a bound collection of spec records, so "add a series" is a data change.
- The runtime generic series (model type plus geometry type parameters) is the typed row under the template; `ValuesMap` projects arbitrary domain models into chart coordinates without intermediate point lists.
- Gauge rows ride the same table: gauge value, background, and needle are series rows with an `Invalidate` refresh hook — a gauge is not a separate control concept.
- Overlay annotations are data too: the visual-elements collection and section rows declare highlight bands, threshold lines, and callouts as bound values — hand-drawn overlay painting on a chart is the rejected form.
- Axis law: one axis row owns labeling and scale policy — `Labeler` (`Func<double, string>`) is the single label projection; formatting and locale arrive through the token algebra, never inline format strings.
- `MinLimit`/`MaxLimit` are nullable for auto-scale; `MinStep` plus `ForceStepToMin` pin tick density; `Position` places the axis; named axis rows (numeric, date-time, time-span, logarithmic, polar) are vocabulary rows over one axis shape.
- `UnitWidth` declares the domain width of one unit — bar sizing on time scales depends on it, and the classic invisible-bars defect is an unset `UnitWidth` on a date axis.
- Axis paints (`NamePaint`/`LabelsPaint`/`SeparatorsPaint`/`SubseparatorsPaint`/`CrosshairLabelsPaint`) all arrive as resolved paint tokens — five paint slots, one token source.
- Global policy declares once: `Configure(Action<settings>)` at the composition root sets default animation speed, easing, legend/tooltip position and paints, zoom mode/speed, and finding strategy.
- Per-chart and per-axis values are explicit override rows on top of the declared root — scattering chart styling across instances is the rejected form, and the settings record is the policy-values law applied to charting.
- Diagnostics posture is root-declared too: a logging gate and a global rendering-settings record sit beside the default settings — turning chart diagnostics on is a root edit, never instance instrumentation.
- Zoom speed is a scalar policy row on the same record — wheel-step aggressiveness is tuned once for every zoomable chart in the process.
- The headless legend and tooltip exist as drawable rows beside the headless charts — evidence renders include chrome when the spec says so, because chrome is part of what users saw.
- Point-level entities (observable value, date-time point, observable point rows) implement change notification and the chart-entity contract — mutating an entity's value in place animates the transition without collection churn.
- The two mutation grains — entity mutation versus collection swap — have different costs and different animation semantics; the binding law picks the grain per feed class and never mixes them on one series.
- `SyncContext` is the chart-side concurrency contract: one shared sync object serializes data mutation against the motion-canvas render loop — binding a concurrently-mutated collection without it corrupts frames.
- The chart consumes the marshaled output of the stream lane; `SyncContext` is the seam where that guarantee is declared, not where it is implemented.
- Chart paint objects are token-materialized: solid/gradient paint rows constructed once at resolve (color plus stroke width) and shared across series — a literal color constructed at a chart call site traces to no token and is the defect.
- Legend and tooltip are policy rows, not custom draws: position vocabularies plus paint tokens; the finding-strategy row declares how pointer position maps to data points — interactive semantics are settings data.
- The settings record also carries legend/tooltip text sizes, background paints, a label-width ceiling, and the polar initial rotation — chrome styling is exhaustively settings-resident, which is what makes "restyle every chart" a one-record edit.
- Axis-name presentation rows (`NameTextSize`, `NamePadding`, `InLineNamePlacement`) live on the axis row beside the paints — axis chrome and axis scale are one declaration, not a control template concern.
- `DrawMargin` and `DrawMarginFrame` pin the plot rectangle explicitly — auto-margin reflows when label widths change, so dashboards that must align plot areas across charts declare the margin and accept clipped labels as the visible failure mode instead of silent misalignment.
- Entity rows carry `Coordinate` and `MetaData` slots the chart fills — domain models stay clean because chart-internal placement state lives on the entity contract's slots, not on domain fields.
- The wrapped-series row on declarative series types exposes the runtime series beneath the markup surface — escape-hatch access for capabilities the declarative row has not surfaced, used at composition, never in binding paths.
- Zoom policy is a flags algebra, not a mode enum: pan and zoom compose per axis (`PanX | ZoomX | PanY | ZoomY` with `X`/`Y` composites, `None`, plus behavior bits `NoFit`, `NoZoomBySection`, an inverted-pan trigger) — a chart's interaction posture is one flags value, and a new posture is a flags composition, never new handler code.
- Pointer-to-point mapping is the `FindingStrategy` vocabulary: automatic, compare-all, axis-constrained, take-closest variants, and exact-match rows — tooltip behavior, hover highlighting, and programmatic point queries all read the same declared strategy, so interactive and tested behavior cannot diverge.
- Legend visibility is a position row: the position vocabulary includes a hidden row — no parallel boolean, which keeps "where is the legend" a single-axis question.
- The series geometry is a type parameter: the runtime series family takes (model, visual geometry, label geometry) type arguments with defaulted convenience rows — swapping marker shape is a type-argument edit on the spec row, zero new series code.
- Token paints are safe to share across series because the chart clones paints per draw task internally — the sharing law costs nothing at the chart seam, and per-series paint construction defends against nothing.
- One retained motion canvas underlies every chart family — the same animation pump, the same invalidation, every family; chart-kind choice never changes the render-loop contract.
- The headless chart accepts a live chart view at construction — an interactive chart's exact configuration renders to evidence by handing the view to the headless row, which is the parity bridge between what a user saw and what a test hashes.

## windowed/downsampled stream binding

- The chart binds to a fold artifact, never a producer: a fixed-capacity window or downsampled projection is computed in the stream lane, and chart binding is exactly one of — entity mutation inside a stable window, or atomic window swap.
- The chart never observes raw event cadence; stream mechanics (lanes, backpressure, drop receipts) arrive settled from the concurrency layer.
- Downsampling is not chart-owned: no decimation exists on the chart surface, so the projection is explicit upstream — at most ~2 points per rendered pixel column, min/max pair per bucket to preserve envelope extremes that averaging destroys.
- `MinStep` and `UnitWidth` make bucket width a declared chart-side fact the stream fold reads — the downsample ratio derives from declared geometry, never from guessing the render width.
- High-frequency feeds mutate entities in place at a bounded cadence: a window of pre-allocated observable entities whose values rotate is allocation-free at steady state, with per-entity animated interpolation.
- Structural changes — window length, series set — swap collections and re-measure; rebuilding collections per tick is the canonical live-chart performance defect, and the grain split makes it visible in review.
- Live-feed charts disable motion: the disable row is a one-millisecond sentinel duration, not zero — `TimeSpan.Zero` does not disable transitions, the dedicated constant does.
- Evidence charts ride the disable row unconditionally: animated interpolation between frames is nondeterminism in a hash gate.
- The global frame ceiling (`MaxFps`) caps the motion canvas independently of data cadence — feed rate and render rate are decoupled by construction, and the ceiling is a root policy value.
- Axis limits under streaming are policy rows, not derived per tick: auto-scale on a sliding window re-fits every frame and makes scale jitter the dominant visual noise — pin limits or pin a hysteresis band computed in the stream fold, so the frame-to-frame diff is data motion, not scale motion.
- Axis transitions carry their own motion rows: the axis row holds animation speed and easing independently of series — limit changes animate under the axis token while data animates under the series token, and the two cadences are deliberately decoupled.
- Window artifacts are value snapshots: a saved window plus its spec plus a catalog generation re-renders bit-identical evidence later — live-chart forensics is replay of values, never reproduction of timing.
- Heat series pair with the heat-legend drawable row — the legend renders the same ramp table the series samples, so legend and data cannot disagree about color meaning.

## theme-token algebra — the master shape

- Five concerns are one algebra differing only in row payload: theme paints, typography roles, motion rows, locale rows, icon rows each instantiate the same shape.
- The shape: a frozen role/variant catalog; orthogonal axes composed at the resolve boundary (variant × density for paints, role × font-chain for typography, motion-class × reduced-motion for durations); one pure resolve fold; one resolved-artifact record; one atomic swap; one changed-key diff receipt.
- This is the largest deletion in the corpus: five bespoke theming/styling/formatting subsystems collapse into five instantiations of one generic owner.
- Token keys resolve at mount, artifacts live at call sites: a consumer mounts against keys and holds the resolved artifact (paint object, role row, duration+easing pair).
- A literal paint, font, duration, or easing at a call site traces to no token and is the defect — the audit is grep-shaped because the only legal constructors of these payloads live inside the resolve fold.
- The grid is frozen and total: every (role, variant, density) cell exists at catalog freeze — a missing cell is a construction-time rejection, never a draw-time fallback chain.
- Freezing fixes the key set, so the diff receipt's key space is closed and consumers can pre-register per-key reactions.
- The resolve fold is pure: frozen grid plus current axis values map to one immutable resolved-artifact record — same inputs, same record, no ambient reads.
- Publication is one cell swap emitting (generation, changed keys, causing axis); cell mechanics are rails-owned supporting material — the algebra owns fold purity, the generation stamp, and key-equality diffing over resolved payloads.
- Consumers re-mount only changed keys; a variant flip that leaves a key's payload identical emits no change for that key — diff is computed on resolved payloads, not on axis values.
- Theme application is the consumer's concern and lands in the interaction layer; the algebra's deliverables end at the resolved record and the diff receipt.
- Resolved paint payloads carry the full pigment row: float color with colorspace tag, stroke metrics, effect slots — raw-canvas paints and chart paint objects materialize from one paint-token row, so a theme swap moves every rendered surface through one receipt.
- Density is an axis, not a scale factor: compact/comfortable rows carry distinct spacing, stroke, and typography-size payloads — scaling one theme by a scalar produces optically wrong results at density extremes, because stroke weights and type sizes do not scale linearly.
- The grid pays the cell count to keep each cell optically tuned; cell count is the honest price of correct density, and the freeze-time totality check is what makes the price safe to pay.
- The rejected theming substrate for canvas surfaces is cascade resolution: resource-dictionary lookup chains and style cascades resolve per element per pass — the algebra replaces every cascade walk with one fold whose output is held, which is both the determinism win and the performance win.
- Token keys obey the naming law: role keys are canonical semantic nouns of the bounded context — a key named for its current value (a color name, a size number) is the naming defect that breaks the variant axis the moment values diverge per variant.
- Locale rows ride the same algebra: number/date/measure formatting policies are token payloads resolved at mount — the axis-label `Labeler` and every formatted string in a visual trace to a locale token, not an inline culture call.
- The catalog is one process-wide instance: multi-window and multi-surface processes share the resolved record and its swap channel — per-window catalogs fork the generation stamp and desynchronize derived caches, which is the multi-surface defect the single instance forecloses.
- Root chart configuration runs inside the catalog's publication: the configure call materializes from resolved tokens, so the chart-settings record is itself a derived artifact — a theme swap re-publishes chart defaults through the same receipt that moves every other surface.

## perceptual color interpolation

- Color motion and ramp generation interpolate in a perceptual space, never componentwise sRGB: sRGB-component lerp desaturates midpoints and bends hue — the gray-dead-zone defect in any blue→yellow tween.
- The cube-root LMS opponent space is the interpolation default; its cylindrical form serves hue-preserving tweens — chroma and lightness linear, hue by shortest arc.
- Achromatic endpoints carry no hue: a tween from gray adopts the chromatic endpoint's hue for the whole arc — the undefined-hue rule, applied at ramp construction.
- Gamut mapping after interpolation: perceptually interpolated stops can exit the display gamut; the correct projection reduces chroma at constant lightness and hue until in-gamut.
- RGB clamping shifts both hue and lightness and reintroduces exactly the artifacts perceptual interpolation removed — clamping is the rejected gamut strategy.
- The pipeline seat is the resolve fold: interpolated ramps and tween endpoints are computed at token resolution into precomputed stop tables, converted once to tagged colors at the boundary.
- Per-frame perceptual conversion is waste — motion samples the table, and the table is part of the resolved-artifact record, so a theme swap rebuilds ramps through the standard receipt.
- Alpha interpolates separately from color: tweening premultiplied values darkens midpoints, so transitions interpolate straight color in the perceptual space and alpha linearly, premultiplying at the boundary — the premultiplication seam is the same one the render lane's alpha-type law declares.
- Sequential data ramps (heat series, density maps) carry a correctness predicate: lightness monotonic over the ramp — non-monotonic lightness creates false boundaries in continuous data.
- Diverging ramps pin the midpoint to the neutral axis; ramp construction asserts both predicates at resolve, making a perceptually broken palette a construction failure instead of a chart-reading hazard.
- Interpolation space is itself a policy row: near-hue gradients may ride the rectangular form, long-hue-distance tweens the cylindrical form with a declared hue path — the row names space and path so two surfaces tweening the same pair cannot diverge.
- The near-neutral instability edge: hue is numerically meaningless at very low chroma, so cylindrical interpolation near the neutral axis amplifies noise into visible hue flicker — the resolve fold clamps sub-epsilon chroma to neutral before interpolating, making the edge unreachable at sample time.
- Ramp tables are sized by perceptual step, not frame count: enough stops that adjacent samples differ below a just-noticeable threshold — a fixed generous stop count is cheaper than adaptive sizing and amortizes to zero because tables build at resolve, not per tween.
- Contrast obligations bind ramp endpoints: text-bearing roles pair foreground and background tokens whose resolved contrast is asserted at freeze — a variant whose interpolated surface color breaks the paired text contrast fails catalog construction, not an accessibility audit.

## motion rows and reduced-motion

- Motion tokens are (duration, easing) rows in the same frozen algebra; easing payloads are `Func<float, float>` values consumed wherever motion runs.
- The chart-side easing vocabulary — cubic-bezier builders for ease/ease-in/ease-out/ease-in-out, polynomial, elastic, bounce, back, circle, exponential, sine families, a four-point bezier constructor, and a keyframe-table constructor — doubles as the system-wide easing table.
- One easing vocabulary serves chart transitions and visual motion, so a motion token resolves identically wherever it is consumed — two easing tables in one system is the rejected form.
- The vocabulary distinguishes unset from linear: the settings default easing is an explicit unset row meaning inherit, while the linear row (`Lineal`) is a declared function — code that treats a missing easing as linear has silently overridden inheritance.
- Easing rows are pure functions and therefore testable as data: a motion token's curve is property-checkable (monotonic where declared, endpoint-exact, bounded overshoot for back/elastic families) at catalog freeze — broken curves fail construction, not review.
- Reduced motion is an axis, not a flag check: the motion grid carries a reduced variant whose rows degrade duration to the disable sentinel and easing to linear.
- State changes that must remain perceivable degrade to opacity-only rows rather than disappearing — reduced motion is a re-resolution, not a feature switch-off.
- The platform preference is admitted once as an axis value at the boundary; the swap republishes resolved motion artifacts through the standard diff receipt — zero call-site conditionals, because call sites hold resolved rows.
- Override altitude is declarative: animation speed and easing exist at root-settings, chart, axis, and series altitudes — overrides are permitted only as catalog rows (a series-class row, an axis-class row), never inline values.
- Duration semantics carry a sentinel trap: the unset marker is a max-value duration meaning inherit-from-root, and the disable row is the one-millisecond constant — treating zero as "off" or max-value as "very slow" both misread the vocabulary.
- Motion rows never store raw durations from user input without projecting through the vocabulary — admission normalizes to the sentinel system.
- The keyframe-table easing constructor makes bespoke motion data: a nonstandard curve is a keyframe row set in the catalog, not a hand-written interpolation function — custom motion stays inside the token algebra and swaps atomically with everything else.
- Parameterized easing constructors (the overshoot-taking back-easing builder, the four-point bezier builder) make easing families data-generable: a motion class declares its parameters and the catalog materializes the function — easing tuning is a parameter edit with an unchanged key.
- The easing function contract is normalized progress to normalized progress over the unit interval: any function honoring the contract enters the vocabulary, which is how physics-flavored curves (spring approximations as keyframe tables) coexist with analytic rows under one type.

## divergent — series-spec-binding

- The maximal chart law: chart = fold(series-spec table, window artifacts, resolved tokens) — every chart in a system is reproducible from three data inputs.
- Reproducibility makes chart state diffable, snapshotable, and headless-renderable by construction; a chart that cannot be reproduced from the three inputs has leaked imperative state and fails review structurally.
- Spec records absorb the foreseeable family along three independent axes: new series kind = one vocabulary row plus one dispatch arm (compile-break if missing); new visual treatment = paint-token rows; new data shape = one values-map projection.
- No foreseeable chart requirement opens a new surface — the table's growth axes cover kind, style, and shape without touching the fold.
- The headless family is the proof seam: spec plus window plus tokens render via the canvas-draw row onto the standard evidence surface, so chart regressions gate on render-hash like every other visual.
- The UI control is the interactive projection of an already-proven scene — visual chart bugs become reproducible artifacts instead of screenshot folklore.
- Finding/tooltip strategy and zoom mode are spec-level policy values: an embedded chart and its evidence twin differ only in interactivity rows — behavior parity between modalities is a record diff, not a test campaign.
- Stream-to-spec coupling is one-directional: window artifacts reference spec identity, specs never reference streams — charts re-bind to replayed windows for forensics without touching live producers.
- Viewport state is data: the zoom flags row plus current axis limits fully describe a viewport — save/restore of a chart view is two values, and "share this exact view" is serialization of the pair, not screenshot exchange.
- The finding strategy doubles as the evidence probe: programmatic point queries in tests use the same declared strategy as interactive tooltips, so a passing point-query test certifies the interactive behavior — one vocabulary, two consumers, no parity gap.

## divergent — theme-token-algebra

- The five instantiations share one generic owner: catalog of (key, row payload, artifact) with (freeze, resolve, swap, diff) — concern-specific code reduces to the row payload type and the resolve arm.
- A sixth tokenized concern (spacing scale, elevation, sound) is one new instantiation with zero new mechanism — the absorption proof for the whole algebra.
- Generation stamps unify staleness across the system: shaped-text caches, SVG variant matrices, ramp tables, and chart paints all key on catalog generation — one swap advances one number, and every derived cache invalidates by comparison, not enumeration.
- Cross-cache coherence reduces to a single integer's monotonicity — the strongest possible simplification of multi-cache invalidation.
- The diff receipt is an optimization contract, not telemetry: changed-key sets bound re-mount work, so the cost of a theme swap is proportional to actual visual change.
- Receipts double as behavioral evidence: an unexpectedly large changed-set on a small axis flip is a catalog-design smell surfaced as data, before any user sees jank.
- Axis causality in the receipt enables partial subscription: consumers depending only on density ignore variant-caused swaps entirely — the causing-axis field turns "theme changed, re-render everything" into routed invalidation.
- Token-key discipline composes with symbolic reference: keys are typed vocabulary members, never strings — a key that fails to resolve is a compile error or a freeze-time rejection, and the runtime has no unresolved-key path.

## divergent — motion-perceptual

- Motion and color converge in one tween row: (duration, easing, interpolation-space) — a color transition is fully specified by one row, and the tween engine is one generic sampler over easing composed with space interpolation.
- Bespoke per-property animation code collapses into row-driven sampling over precomputed ramp tables — the table is the artifact, the sampler is the only code.
- Reduced motion interacts with perceptual color: when duration degrades to the disable sentinel, color state changes lose their tween but keep endpoint correctness — the reduced variant's rows still resolve through gamut mapping.
- Accessibility mode never ships out-of-gamut or hue-shifted endpoint colors; the degenerate tween is a one-sample ramp, not a separate code path.
- Easing functions compose as function algebra: the function payload composes (reverse, mirror, clamp, segment) without new vocabulary entries — derived easings are declared as compositions in the catalog, keeping the row set small while the expressible family stays large.
- Frame-budget honesty: the motion canvas's global frame ceiling plus per-row durations yield a computable worst-case redraw budget per swap — motion cost is auditable from the catalog alone, before any frame renders.
- The declared-cost posture matches the render lane's op-count probes: scene cost and motion cost are both readable from declarations, which is what keeps performance review structural instead of empirical.
- Reduced-motion evidence parity: the reduced variant renders through the same headless rows as the standard variant, so accessibility-mode visuals carry their own hash baselines — reduced mode is tested as a first-class variant column, never assumed equivalent.
