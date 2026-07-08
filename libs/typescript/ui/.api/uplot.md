# [TS_UI_API_UPLOT]

[PACKAGE_SURFACE]:
- package: `uplot` · license `MIT`
- module: triple build — `dist/uPlot.esm.js` (`module`), `dist/uPlot.cjs.js` (`main`), `dist/uPlot.iife.js` (global `uPlot`); declarations at `dist/uPlot.d.ts` (the `typings` field — one hand-written file, no `exports` map, no subpaths).
- asset: `dist/uPlot.min.css` is a REQUIRED separate stylesheet — no JS entry references it and nothing auto-injects; the token stylesheet imports it once and overrides its custom look there, never per-chart.
- runtime: browser canvas 2D; zero dependencies, zero React coupling — the React seam is a ref + effect bracket the consumer owns.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, the telemetry/sensor/simulation time-series lane.
- rail: `ui` data-viz — the high-frequency time-series owner.

`uplot` is the raw-throughput end of the ui charting spectrum: a single `uPlot` class rendering aligned columnar series to one canvas at 100k+ points with classic time-series UX (zoom-drag, cursor, crosshair, legend, multi-chart cursor sync) in ~50KB. The design is ONE columnar contract (`AlignedData`), ONE declarative options tree (series/scales/axes/cursor/legend/bands), and ONE hook bus every extension rides — a plugin is a bag of hook arrays, never a subclass. It exists for the point-count regime where SVG (visx, Plot) collapses and where deck.gl's geospatial paths are the wrong UX; a chart that fits SVG never lands here, and a chart that lands here never re-renders through React — data flows through `setData`, not reconciliation.

## [01]-[ALIGNED_DATA_MODEL]

The data contract is columnar and immutable-by-convention: one x column, N y columns, typed arrays first-class. `null` is the one author-supplied gap marker (`undefined` appears only as a `uPlot.join` alignment artifact); `spanGaps` bridges nulls per series and `gaps` refines the computed gap list.

```ts contract
type AlignedData = TypedArray[] | [xValues: number[] | TypedArray, ...yValues: ((number | null | undefined)[] | TypedArray)[]]
declare class uPlot {
  constructor(opts: uPlot.Options, data?: uPlot.AlignedData, targ?: HTMLElement | ((self: uPlot, init: Function) => void)) // fn targ defers mount — attach self.root, then call init()
  setData(data: uPlot.AlignedData, resetScales?: boolean): void      // the streaming ingress — never reconstruct the chart per frame
  static join(tables: AlignedData[], nullModes?: JoinNullMode[][]): AlignedData  // outer-join tables on table[0]'s x
}
// mode: 1 (Aligned, default) = single-x, y-per-series; mode: 2 (Faceted) = per-point x/y/size/… via Series.facets — scatter/bubble regime.
```

- `Options.mode`: `1` aligned (ordered, shared x) | `2` faceted (unordered per-series points via `Series.facets: { scale, auto?, sorted? }[]`).
- Timestamps: x in seconds by default; `ms: 1` switches to milliseconds; `tzDate: uPlot.tzDate(date, tzName)` renders a fixed zone.

## [02]-[OPTIONS_TREE]

The whole chart is one declarative `Options` value — `width`/`height` + `series` required, everything else policy rows.

| [INDEX] | [BRANCH] | [KEY_FIELDS] | [CAPABILITY] |
|:-----: |:-------- |:-------------------------------------------------------------------------------------------------------------- |:------------------------------------------------------ |
| [01] | `series` | `label` `scale` `stroke` `fill` `fillTo` `width` `dash` `points` `paths` `value`/`values` `spanGaps` `gaps` `sorted` `alpha` `facets` | per-column render policy; `paths` swaps the path builder |
| [02] | `scales` | `time` `auto` `range` `from` `distr` `log` `asinh` `clamp` `fwd`/`bwd` (+ readback `min`/`max`/`dir`/`ori`) | `distr`: 1 linear · 2 ordinal · 3 log · 4 asinh · 100 custom (`fwd`/`bwd` transforms) |
| [03] | `axes` | `side` (0 top·1 right·2 bottom·3 left) `scale` `values` `space` `incrs` `splits` `filter` `size` `gap` `label` `rotate` `align` `grid` `ticks` `border` `stroke` `font` | tick generation is data (`incrs`/`splits`/`values` functions), never string formats |
| [04] | `cursor` | `x`/`y` `points` `drag` (`x`/`y`/`setScale`/`dist`/`uni`/`click`) `sync` (`key`/`setSeries`/`scales`/`match`/`filters`) `focus` (`prox`/`bias`) `hover` `dataIdx` `move` `bind` `lock` | zoom-drag + hover proximity + cross-chart sync in one branch |
| [05] | `legend` | `show` `live` `isolate` `markers` `mount` `values` | live legend reads the cursor idx; `isolate` = click-to-solo |
| [06] | `select` / `bands` / `focus` / `padding` / `drawOrder` / `pxAlign` | selection box state; hi/lo band pairs (`Band`); series-focus alpha; `["axes","series"]` order | the remaining policy leaves |
| [07] | `hooks` / `plugins` | `Hooks.Arrays`; `Plugin = { opts?: (self, opts) => void \| Options; hooks: Hooks.ArraysOrFuncs }` | the extension bus — see [04] |

## [03]-[INSTANCE_AND_STATICS]

| [INDEX] | [SURFACE] | [FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------------------------------------------ |:----------- |:------------------------------------------------------------ |
| [01] | `setData` `setScale(key, {min,max})` `setCursor` `setSelect` `setSize({width,height})` `setSeries(idx, {show,focus})` `setLegend({idx})` | imperative write | every mutation is a `set*`; wrap multi-writes in `batch(txn)` |
| [02] | `addSeries`/`delSeries` `addBand`/`setBand`/`delBand` `redraw(rebuildPaths?, recalcAxes?)` `destroy()` | lifecycle | dynamic series membership; `destroy` is the teardown bracket |
| [03] | `posToVal` `valToPos` `posToIdx` `valToIdx` `syncRect(defer?)` | mapping | pixel↔value↔index projection for custom draw/hit-test |
| [04] | `root` `over` `under` `ctx` `bbox` `rect` `series` `scales` `axes` `cursor` `select` `legend` `data` `status` | readback | `over`/`under` are the DOM layers custom hooks draw into |
| [05] | `uPlot.paths.{linear,spline,stepped,bars,points}` | path builders | factory per geometry; a `Series.paths` row swaps geometry — no `spline2` |
| [06] | `uPlot.sync(key)` · `uPlot.assign` `fmtNum` `fmtDate(tpl)` `tzDate` `rangeNum` `rangeLog` `rangeAsinh` `orient` `addGap` `clipGaps` `pxRatio` | statics | `sync(key)` + `cursor.sync.key` links cursors across charts |

## [04]-[HOOK_BUS]

The lifecycle roster is closed: `init` `addSeries` `delSeries` `setScale` `setCursor` `setLegend` `setSelect` `setSeries` `setData` `setSize` `drawClear` `drawAxes` `drawSeries` `draw` `ready` `destroy` `syncRect`. Every hook is an ARRAY (`Hooks.Arrays`), so N plugins compose without wrapper towers; a plugin optionally rewrites options at construction (`opts`) then contributes hook rows. Custom rendering draws into `u.ctx` between `drawClear` and `draw`, or mounts DOM into `u.over`/`u.under` at `init` — annotations, threshold shading, and tooltips are hook rows, never forks of the draw loop.

## [05]-[INTEGRATION]

[STACK: `setData` + the atom store (`.api/effect-atom-atom-react.md`)] — the chart is an external imperative surface behind ONE ref: a component mounts `new uPlot(opts, data, el)` in an effect bracket (`destroy()` on cleanup), and a telemetry atom's stream lands via `u.setData(next)` inside `Atom` subscription — React never reconciles points. Options are built once from decoded config; a config change rebuilds the instance, a data tick calls `setData`. High-frequency feeds coalesce to animation frames before `setData` — the fold owns cadence, the chart just draws.

[STACK: theming + `system/token`] — `dist/uPlot.min.css` imports once in the token stylesheet; series `stroke`/`fill` and axis/grid strokes take resolved token values (canvas cannot read CSS custom properties), so the chart re-skins by rebuilding options from the token authority on theme flip, and the OKLCH ramp feeds multi-series stroke assignment.

[STACK: `cursor.sync` + dashboard cohorts] — `uPlot.sync(key)` with `cursor.sync: { key, setSeries, scales }` links crosshair, focus, and zoom across a panel cohort; the sync key is a chart-group value from the owning fold, never a literal per chart.

[BOUNDARY: uplot vs the sibling viz owners] — uplot owns the extreme-point-count time-series regime (telemetry, sensors, simulation traces: canvas, `AlignedData`, `setData` streaming). `@observablehq/plot` (`.api/observablehq-plot.md`) owns exploratory-statistical grammar charts; the `@visx/*` set (`.api/visx-shape.md` …) owns bespoke accessible SVG composition; `@perspective-dev/*` (`.api/perspective-dev-viewer.md`) owns pivot/aggregation grids over streaming tables; deck.gl (`.api/deck.gl-core.md`) owns geospatial GPU layers. One chart, one engine — a series that fits SVG scale never pays the canvas a11y cost (uplot's canvas has no per-point DOM/ARIA; the accessible summary row beside it is the consumer's obligation).

## [06]-[RAIL_LAW]

- Owns: canvas time-series rendering at extreme point counts — the `AlignedData` columnar contract, the options tree (series/scales/axes/cursor/legend/bands/select), path-builder geometry swaps, the hook-array plugin bus, cursor sync cohorts, pixel↔value mapping, and mode-2 faceted scatter.
- Accept: typed-array columns with `null` gaps; `setData` as the only per-tick write path inside an effect-bracketed instance; `batch` around multi-writes; plugins as hook rows; `uPlot.paths.*` for geometry; `distr`/`range`/`incrs` policy functions over hardcoded ticks; token-resolved colors rebuilt on theme flip; `uPlot.sync` for cohort crosshairs.
- Reject: rebuilding the instance per data tick (stream through `setData`); React state mirroring chart data; SVG-scale charts routed here (that is visx/Plot); a second time-series engine beside it; subclassing or patching internals where a hook row exists; hand-linked CSS per chart (the token stylesheet owns the one import); `undefined` as an authored gap marker.
