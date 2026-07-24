# [TS_UI_API_UPLOT]

`uPlot` renders aligned columnar series to one canvas at extreme point counts, carrying interactive time-series UX — zoom-drag, cursor crosshair, live legend, and cross-chart cursor sync.

One columnar contract feeds the chart, one declarative options tree configures it, and one hook-array bus carries every extension; a plugin contributes hook rows, never a subclass, and data streams through `setData`, never React reconciliation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uplot`
- package: `uplot` (MIT)
- module: ESM `dist/uPlot.esm.js`, CJS `dist/uPlot.cjs.js`, IIFE global `uPlot`; types `dist/uPlot.d.ts`, no `exports` map, no subpaths.
- runtime: browser canvas 2D; zero dependencies, no React coupling.
- plane: `plane:runtime` (W4 `ui`), the telemetry/sensor/simulation time-series lane.
- rail: `ui` data-viz — the high-frequency time-series owner.

## [02]-[ALIGNED_DATA_MODEL]

`AlignedData` is columnar and immutable-by-convention: one x column, N y columns, typed arrays first-class. `null` marks an author gap (`undefined` is only a `uPlot.join` alignment artifact); `spanGaps` bridges nulls per series and `gaps` refines the computed gap list.

[ALIGNED_DATA]: `AlignedData = TypedArray[]|[xValues:number[]|TypedArray,...yValues:((number|null|undefined)[]|TypedArray)[]]`
[U_PLOT]: `uPlot(uPlot.Options,uPlot.AlignedData?,HTMLElement|((self:uPlot,init:Function)=>void)?)` `uPlot.setData(uPlot.AlignedData,boolean?) -> void` `uPlot.join(AlignedData[],JoinNullMode[][]?) -> AlignedData`

- `Options.mode`: `1` aligned (ordered, shared x) | `2` faceted (unordered per-series points via `Series.facets: { scale, auto?, sorted? }[]`).
- Timestamps: x in seconds by default; `ms: 1` switches to milliseconds; `tzDate: uPlot.tzDate(date, tzName)` renders a fixed zone.

## [03]-[OPTIONS_TREE]

`Options` is one declarative value configuring the whole chart — `width`/`height` and `series` required, every other branch a policy row. Axis ticks are data (`incrs`/`splits`/`values` functions), never string formats; the live legend reads the cursor idx and `isolate` solos a series; `select`/`bands`/`focus`/`padding`/`drawOrder`/`pxAlign` are the remaining render leaves, and `hooks`/`plugins` are the extension bus.

| [INDEX] | [BRANCH]                            | [KEY_FIELDS]                                                                                      |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `series` (render)                   | `label` `scale` `stroke` `fill` `fillTo` `width` `dash` `points` `paths` `alpha`                  |
|  [02]   | `series` (data)                     | `value`/`values` `spanGaps` `gaps` `sorted` `facets` (mode-2 per-point)                           |
|  [03]   | `scales` (transform)                | `distr` (1 lin·2 ord·3 log·4 asinh·100 custom) `log` `asinh` `fwd`/`bwd` `clamp`                  |
|  [04]   | `scales` (domain)                   | `time` `auto` `range` `from` (+ readback `min`/`max`/`dir`/`ori`)                                 |
|  [05]   | `axes` (layout)                     | `side` (0 top·1 right·2 bot·3 left) `scale` `space` `size` `gap` `rotate` `align`                 |
|  [06]   | `axes` (ticks)                      | `values` `incrs` `splits` `filter` `ticks` `grid` `border` `stroke` `font` `label`                |
|  [07]   | `cursor` (core)                     | `x` `y` `points` `focus` (`prox`/`bias`) `hover` `dataIdx` `move` `bind` `lock`                   |
|  [08]   | `cursor.drag`                       | `x` `y` `setScale` `dist` `uni` `click` — zoom-drag                                               |
|  [09]   | `cursor.sync`                       | `key` `setSeries` `scales` `match` `filters` — cross-chart sync                                   |
|  [10]   | `legend`                            | `show` `live` `isolate` `markers` `mount` `values`                                                |
|  [11]   | `select` / `bands` / `focus`        | selection box; hi/lo `Band` pairs; series-focus `alpha`                                           |
|  [12]   | `padding` / `drawOrder` / `pxAlign` | box `padding`; `["axes","series"]` order; `pxAlign`                                               |
|  [13]   | `hooks` / `plugins`                 | `Hooks.Arrays`; `Plugin = { opts?: (self, opts) => void \| Options; hooks: Hooks.ArraysOrFuncs }` |

## [04]-[INSTANCE_AND_STATICS]

`uPlot` instance methods are imperative: every mutation is a `set*` call and `batch(txn)` coalesces a transaction; the statics build path geometry, ranges, formatters, and cohort sync.

| [INDEX] | [SURFACE]                                                                          | [FAMILY]         | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------- | :--------------- | :------------------------------- |
|  [01]   | `setData(data, resetScales?)` `setScale(key, {min,max})` `setSize({width,height})` | imperative write | multi-writes via `batch(txn)`    |
|  [02]   | `setCursor` `setSelect` `setSeries(idx, {show,focus})` `setLegend({idx})`          | imperative write | every mutation is a `set*`       |
|  [03]   | `addSeries`/`delSeries` `addBand`/`setBand`/`delBand`                              | lifecycle        | dynamic series/band membership   |
|  [04]   | `redraw(rebuildPaths?, recalcAxes?)` `destroy()`                                   | lifecycle        | `destroy()` teardown bracket     |
|  [05]   | `posToVal` `valToPos` `posToIdx` `valToIdx` `syncRect(defer?)`                     | mapping          | pixel↔value↔index projection     |
|  [06]   | `root` `over` `under` `ctx` `bbox` `rect`                                          | readback         | `over`/`under` draw layers       |
|  [07]   | `series` `scales` `axes` `cursor` `select` `legend` `data` `status`                | readback         | live instance state snapshots    |
|  [08]   | `uPlot.paths.{linear,spline,stepped,bars,points}`                                  | path builders    | swaps `Series.paths` geometry    |
|  [09]   | `uPlot.sync(key)` `uPlot.assign` `orient` `addGap` `clipGaps` `pxRatio`            | statics          | `sync(key)` links cursor cohorts |
|  [10]   | `fmtNum` `fmtDate(tpl)` `tzDate` `rangeNum` `rangeLog` `rangeAsinh`                | statics          | format + range helpers           |

## [05]-[HOOK_BUS]

Hooks compose as arrays: the closed lifecycle roster is `init` `addSeries` `delSeries` `setScale` `setCursor` `setLegend` `setSelect` `setSeries` `setData` `setSize` `drawClear` `drawAxes` `drawSeries` `draw` `ready` `destroy` `syncRect`, each an array (`Hooks.Arrays`) so N plugins compose without wrapper towers.

Each plugin optionally rewrites options at construction (`opts`) then contributes hook rows; custom rendering draws into `u.ctx` between `drawClear` and `draw`, or mounts DOM into `u.over`/`u.under` at `init` — annotations, threshold shading, and tooltips are hook rows, never draw-loop forks.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every extension rides the hook-array bus and every per-tick update rides `setData`; options rebuild on a config change and the fold streams data ticks, so the chart is one canvas the fold owns, never a React-reconciled tree.

[STACKING]:
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): a component mounts `new uPlot(opts, data, el)` in an effect bracket (`destroy()` on cleanup) and a telemetry `Atom` subscription lands each tick via `u.setData(next)`; high-frequency feeds coalesce to animation frames before `setData`, so the fold owns cadence and React never reconciles points.
- `system/token`: `dist/uPlot.min.css` imports once in the token stylesheet — no JS entry references it and nothing auto-injects — and series `stroke`/`fill` and axis/grid strokes take resolved token values (canvas cannot read CSS custom properties), so a theme flip rebuilds options from the token authority and the OKLCH ramp feeds multi-series stroke assignment.
- within-lib cohorts: `uPlot.sync(key)` with `cursor.sync: { key, setSeries, scales }` links crosshair, focus, and zoom across a panel cohort; the sync key is a chart-group value from the owning fold, never a literal per chart.

[LOCAL_ADMISSION]:
- uplot owns the extreme-point-count time-series regime — telemetry, sensor, and simulation traces streamed on canvas through `AlignedData`/`setData`; one chart binds one engine, an SVG-scale series routes to `@visx/*` or `@observablehq/plot`, and geospatial GPU scale routes to deck.gl. `.api/observablehq-plot.md` LOCAL_ADMISSION owns the full engine-selection matrix.
- uplot's canvas carries no per-point DOM or ARIA; the accessible summary beside the chart is the consumer's obligation.

[RAIL_LAW]:
- Package: `uplot`
- Owns: canvas time-series rendering at extreme point counts — the `AlignedData` columnar contract, the options tree (series/scales/axes/cursor/legend/bands/select), path-builder geometry swaps, the hook-array plugin bus, cursor-sync cohorts, pixel↔value mapping, and mode-2 faceted scatter.
- Accept: typed-array columns with `null` gaps; `setData` as the only per-tick write inside an effect-bracketed instance; `batch` around multi-writes; plugins as hook rows; `uPlot.paths.*` geometry; `distr`/`range`/`incrs` policy functions over hardcoded ticks; token-resolved colors rebuilt on theme flip; `uPlot.sync` cohorts.
- Reject: rebuilding the instance per data tick; React state mirroring chart data; SVG-scale charts routed here; a second time-series engine beside it; subclassing where a hook row exists; hand-linked CSS per chart; `undefined` as an authored gap.
