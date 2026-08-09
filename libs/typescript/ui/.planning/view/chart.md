# [UI_CHART]

Chart owns declared statistics, streaming series, and user-driven pivots behind one data-shape discriminant. Observable Plot and visx render declared charts, uPlot renders streaming columns, and Perspective owns pivot aggregation. One Arrow table is their columnar bus; each surface brackets one engine, derives specs from atoms, and resolves color through tokens. Module: `ui/src/view/chart.ts`.

## [01]-[INDEX]

- [02]-[REGIME_LAW]: the regime row table, the Arrow columnar bus, the fault family, the one panel measurement; `Chart`, `ChartFault`.
- [03]-[DECLARED_SURFACE]: the Plot grammar bracket, legend and pointer readbacks, the visx bespoke lane, the d3 fold substrate; `Chart`.
- [04]-[SERIES_SURFACE]: the uplot scoped instance — the one imperative write, the options value, the two-source feed; `Chart`.
- [05]-[PIVOT_SURFACE]: the perspective engine — the origin family, client/table lifecycle, expression gate, derived feed; `Chart`.

## [02]-[REGIME_LAW]

[REGIME_LAW]:
- Owner: `Chart` — one owner whose members are the three regime brackets and the columnar bus fold; regime selection is a `Chart.regime` row, never a component fork: DECLARED (the chart states a statistical claim — distribution, regression, facet, small multiple) renders through `[3]`; STREAMING (a telemetry/sensor/simulation series where point count breaks SVG) renders through `[4]`'s canvas; PIVOT (the USER drives group/split/aggregate/filter over a live feed) renders through `[5]`'s engine.
- Law: the regime table carries the decisions a consumer otherwise re-derives — `mount` names the bracket member the surface calls, `bus` names the Arrow projection it consumes, `canvas` decides the color path (a canvas engine reads resolved values, an SVG surface reads classes through `cn`), `addressable` decides whether `[3]`'s bespoke lane is earned, and `summary` states which regimes owe the accessible summary row beside the chart; a conditional over regime names re-derives a column the row already carries.
- Law: Arrow is the inter-engine bus — `Chart.columns` projects an `apache-arrow` `Table` OR one `RecordBatch` into uplot's aligned columns through the shared `getChild(...).toArray()` spelling (`RecordBatch.getChild` carries the `Table` projection batch-direct, so the continuous-body reader lane never materializes a per-frame `Table`), Plot marks take the `Table` directly with column-name channels, and perspective ingests the SAME frame's IPC bytes with `format: "arrow"`; a JSON re-materialization between Arrow-capable engines is the named defect. A named column absent from the source folds the whole projection to `Option.none` — the consumer renders no chart; a fabricated flat series standing in for a missing column is the named defect.
- Law: arity is the projection's own modality — one source projects directly, and a NON-EMPTY set of sources disagreeing on x outer-joins through `uPlot.join` inside the same member, discriminated by the `isArrowTable`/`isArrowRecordBatch` evidence each value already carries; a `join` sibling export beside `columns` is the arity twin this entrypoint deletes.
- Law: color obeys the token split — series strokes, categorical palettes, and axis inks resolve from `Theme.Palette.ramp`/`Theme` rows (canvas engines take resolved values rebuilt on theme flip; SVG takes classes through `cn`), the regime row's `canvas` column being the read that selects between them; `d3-scale-chromatic` colormaps appear ONLY where the color IS the datum's value (`scaleSequential(interpolateViridis)` density/heat), and a `scheme*` categorical array standing in for the token palette is the split-brain defect.
- Law: `d3` is substrate, never surface — `rollup`/`bin`/`extent` folds prepare data beside a spec, scale/curve/format vocabularies pass through, and the DOM-coupled modules (`d3-selection`/`d3-zoom`/`d3-axis`) never appear; React owns chart DOM, `system/act` owns gesture.
- Law: measurement flows one way — `Chart.useFrame(sizing)` is the ONE producer, one `useParentSize` observer per panel whose `Chart.Panel` hands back the callback `parentRef` the panel spreads and the `Chart.Frame` every resident chart takes as a parameter: `Chart.plot` passes it into the Plot options value, `Chart.write` hands it to uplot's `setSize`, `Chart.bespoke` divides it by the margin into scale ranges. Debounce arrives as a `Chart.Sizing` policy row the composing panel supplies, so no chart holds a window literal; a chart calling the observer itself, or taking bare `width`/`height` scalars with no producer, is the named defect.
- Packages: `apache-arrow` (`Table`/`RecordBatch` `getChild`, `isArrowTable`/`isArrowRecordBatch` narrowing); `uplot` (`AlignedData`, `join`, `setSize`); `@visx/responsive` (`useParentSize` — the one observer, `debounceTime`/`enableDebounceLeadingCall` as its policy row).
- Boundary: `Grid` (`view/table`) owns fixed-shape interactive collections at DOM scale; `viewer/geo` owns the live basemap (Plot's `geo` mark serves statistical maps only); `viewer/probe` and `viewer/panel` render their metric and telemetry boards THROUGH this owner; `view/export` serializes what these brackets render and holds no engine of its own.
- Growth: a new chart need selects a regime row; a new regime is one row with its bracket member on the one owner — never a sibling chart component family.

```typescript
import { Fault } from "@rasm/ts/core"
import { useParentSize } from "@visx/responsive"
import { isArrowRecordBatch, isArrowTable, type RecordBatch, type Table } from "apache-arrow"
import { Array, Option, Schema } from "effect"
import uPlot from "uplot"

const _regimes = ["declared", "streaming", "pivot"] as const

const _regimeRows = {
  declared: { mount: "plot", bus: "table", canvas: false, addressable: true, summary: false },
  streaming: { mount: "series", bus: "aligned", canvas: true, addressable: false, summary: true },
  pivot: { mount: "pivot", bus: "ipc", canvas: false, addressable: false, summary: true },
} as const

declare namespace Chart {
  type Regimes = typeof _regimes
  type Regime = keyof typeof _regimeRows
  type RegimeRow = {
    readonly mount: "plot" | "series" | "pivot"
    readonly bus: "table" | "aligned" | "ipc"
    readonly canvas: boolean // canvas cannot read a custom property, so this column selects resolved values over classes
    readonly addressable: boolean // per-datum DOM addressability: the one condition [3]'s bespoke lane is earned by
    readonly summary: boolean // the regime owes an accessible summary row beside the chart
  }
  type Aligned = uPlot.AlignedData
  type Column = uPlot.AlignedData[number]
  type Frame = { readonly width: number; readonly height: number }
  type Sizing = { readonly debounceTime: number; readonly enableDebounceLeadingCall: boolean }
  type Panel = { readonly parentRef: (node: HTMLDivElement | null) => void; readonly frame: Chart.Frame }
  type Source = RecordBatch | Table // both carry getChild(name) — the batch lane projects with zero Table construction
  type _Rows<T extends Record<Regimes[number], RegimeRow> = typeof _regimeRows> = T // row guard: a missing regime or a malformed column fails at the declaration with zero widening
  type _Keys<K extends Regimes[number] = Regime> = K // key guard: a regime row outside the tuple fails here
}

// One row per reason carrying the core kind alone: severity, blame, retryability, and quarantine are the core
// Fault.Class row table's, so a rank or retry column here would fork the branch lattice per folder.
const _family = Fault.Class.family(["engine-lost", "frame-refused", "expression-refused", "view-lost"] as const, {
  "engine-lost": { class: "unavailable" },
  "frame-refused": { class: "malformed" },
  "expression-refused": { class: "invalid" },
  "view-lost": { class: "unavailable" },
})

class ChartFault extends Schema.TaggedError<ChartFault>()("ChartFault", {
  reason: _family.schema,
  feed: Schema.String,
  detail: Schema.String,
}) {
  static readonly roster: typeof _family.reasons = _family.reasons // the metric word census reads the family's own ordered tuple
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<chart:${this.reason}> ${this.feed}: ${this.detail}`
  }
}

const _project = (source: Chart.Source, x: string, series: ReadonlyArray<string>): Option.Option<Chart.Aligned> =>
  Option.map(
    Option.all(Array.map([x, ...series], (name) => Option.fromNullable(source.getChild(name)))),
    // BOUNDARY ADAPTER: Vector.toArray answers the column's own backing array, which AlignedData admits element-wise
    (children) => Array.map(children, (child) => child.toArray() as Chart.Column) as Chart.Aligned,
  )

function _columns(source: Chart.Source, x: string, series: ReadonlyArray<string>): Option.Option<Chart.Aligned>
function _columns(
  sources: Array.NonEmptyReadonlyArray<Chart.Source>,
  x: string,
  series: ReadonlyArray<string>,
): Option.Option<Chart.Aligned>
function _columns(
  input: Chart.Source | Array.NonEmptyReadonlyArray<Chart.Source>,
  x: string,
  series: ReadonlyArray<string>,
): Option.Option<Chart.Aligned> {
  // the container evidence each value already carries decides the arity: no mode flag, no sibling join export
  return isArrowTable(input) || isArrowRecordBatch(input)
    ? _project(input, x, series)
    : Option.map(Option.all(Array.map(input, (one) => _project(one, x, series))), (lanes) => uPlot.join([...lanes]))
}

const _useFrame = (sizing: Chart.Sizing): Chart.Panel => {
  const measured = useParentSize<HTMLDivElement>(sizing)
  return { parentRef: measured.parentRef, frame: { width: measured.width, height: measured.height } } // the observer's own top/left/node/resize stay behind this seam: a chart reads extent, never the panel's box or its ref handle
}
```

## [03]-[DECLARED_SURFACE]

[DECLARED_SURFACE]:
- Owner: `Chart.plot(container, frame, build)` — the grammar bracket: `build` takes the panel frame and derives a `Plot.plot(options)` element from decoded inputs (marks over channels, transforms as option rewriters — `binX`/`group`/`stackY`/`windowY` in the options value, never a pre-shaped copy of the data), the bracket mounts it through `replaceChildren` and removes it on release; rebuild-per-change is the model — the grammar rebuilds cheaply, which is exactly why streaming series live on `[4]` instead.
- Owner: `Chart.bespoke(frame, data, lens, held)` — the visx spec fold: ONE `Chart.Lens` policy row carries accessors, margin, tick count, and slot count, and the member answers `Option.none` on data whose span is empty rather than minting a forged domain; `Chart.fold(data, lens)` is the d3 substrate beside it, and both are pure — the render site spreads the returned prop records onto the shape components and holds no scale of its own.
- Packages: `@observablehq/plot` (`plot`, the mark roster — `dot`/`lineY`/`areaY`/`barY`/`rectY`/`cell`/`boxY`/`linearRegressionY`/`density`/`raster`/`contour`/`tree`/`geo` — the transform roster, `tip`/`pointer`/`crosshair` interaction, `facet`, named projections); `@visx/scale` (`scaleLinear`, `updateScale`) + `@visx/shape` (`LinePath`) + `@visx/axis` (`AxisBottom`, `AxisLeft`) + `@visx/group` (`Group`) — the bespoke lane; `d3` (`extent`, `bin`, `rollup`, `scaleSequential`, `interpolateViridis` — the fold substrate).
- Law: interaction writes back through the store — `tip: true` renders channel values, and `Chart.pointed(figure, sink)` is the scoped `input` subscription carrying the figure's own `value` (the nearest-datum row a `pointer`/`crosshair` mark writes) to an atom set, released with the bracket that mounted the figure; chart-as-input state never lands in component state, and a `pointer` mark rendered with no listener is a spec claiming an input seam it never opened.
- Law: the legend is a second detached element, never a hand-drawn key — `Chart.legend(figure, scale, options)` reads `figure.legend(name, options)` for a scale the spec already inferred, and the consumer mounts it wherever the layout wants it; a swatch row rebuilt from the palette beside a chart restates the scale the figure carries.
- Law: the visx lane is earned by per-element addressability — RAC-adjacent handlers, per-datum a11y, custom hit logic on React-owned SVG elements: the panel frame divides by margin into scale ranges, `scaleLinear` takes a config object, `updateScale` re-domains the HELD scale on a data change so axes and path keep one reference, and the axes and `LinePath` read those same two instances inside one margin-translated `Group`. Rows materialize here and nowhere else on the page, because per-datum addressability IS the lane's earning; a chart needing none of that is a Plot spec, and a hand-built `d` string where a shape component exists is the named defect.
- Law: prop records lift, never restate — every returned record is `ComponentProps` of the component it feeds (`typeof AxisBottom<Chart.Scale>` pre-solves the axis generic at the owner), so a visx prop rename breaks at this declaration; visx props take mutable arrays and tuples, and this fold is the one seam the readonly interior copies at.
- Law: `d3` enters through the fold members alone and every partial or foreign answer converts here — `extent` answers `[undefined, undefined]` on an empty read, so a span lifts through `Option` and an empty chart is `Option.none`; `rollup` answers d3's own `InternMap`, so the census converts to `HashMap` at the seam and no JS map crosses into the interior; a `Bin` carries `x0`/`x1` as possibly-absent edges, so an edgeless slot drops instead of rendering at zero width. `scaleSequential(interpolateViridis)` inks the density slots because the color IS the slot's count — no other d3 scale reaches a rendered surface.
- Law: Arrow plots directly — `Plot.dot(table, { x: "<column-a>", y: "<column-b>" })` consumes the bus `Table` with column-name shorthand and Arrow date detection; rows never materialize for a declared chart, and a `build` that pre-shapes the table into rows before a mark reads it forfeits the whole reason the bus exists.
- Law: `Plot.plot` answers `SVGSVGElement | HTMLElement` — a caption, a title, or a legend option wraps the svg in a figure — so every consumer reading the element as SVG discriminates on the tag it holds; `view/export`'s serializer row is the one place that discrimination is spelled, and an `as SVGSVGElement` at a mount site is the named defect.
- Growth: a new declared chart is a spec value — marks, transforms, facets as data; a new bespoke shape is one prop record on `Chart.Bespoke` reading the same scale pair, and a new fold statistic is one field on `Chart.Fold` — never a d3-rendered surface beside either.

```typescript
import * as Plot from "@observablehq/plot"
import { AxisBottom, AxisLeft } from "@visx/axis"
import { Group } from "@visx/group"
import { scaleLinear, updateScale } from "@visx/scale"
import { LinePath } from "@visx/shape"
import { bin, extent, interpolateViridis, rollup, scaleSequential } from "d3"
import { Array, Effect, HashMap, Option, pipe } from "effect"
import type { ComponentProps } from "react"

declare namespace Chart {
  type Margin = { readonly top: number; readonly right: number; readonly bottom: number; readonly left: number }
  type Scale = ReturnType<typeof scaleLinear<number>>
  type Span = { readonly domain: [number, number]; readonly range: [number, number]; readonly nice: boolean; readonly clamp: boolean }
  type Slot = { readonly from: number; readonly to: number; readonly count: number }
  type Lens<Datum> = {
    readonly x: (datum: Datum) => number
    readonly y: (datum: Datum) => number
    readonly series: (datum: Datum) => string
    readonly margin: Chart.Margin
    readonly ticks: number
    readonly slots: number
  }
  type Fold = {
    readonly census: HashMap.HashMap<string, number>
    readonly slots: ReadonlyArray<Chart.Slot>
    readonly ink: Option.Option<(count: number) => string>
  }
  type Bespoke<Datum> = {
    readonly group: ComponentProps<typeof Group>
    readonly bottom: ComponentProps<typeof AxisBottom<Chart.Scale>>
    readonly left: ComponentProps<typeof AxisLeft<Chart.Scale>>
    readonly path: ComponentProps<typeof LinePath<Datum>>
  }
}

const _plot = (container: HTMLElement, frame: Chart.Frame, build: (frame: Chart.Frame) => ReturnType<typeof Plot.plot>) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const figure = build(frame)
      container.replaceChildren(figure)
      return figure
    }),
    (figure) => Effect.sync(() => figure.remove()),
  )

const _legend = (
  figure: ReturnType<typeof Plot.plot>,
  scale: Parameters<ReturnType<typeof Plot.plot>["legend"]>[0],
  options: Parameters<ReturnType<typeof Plot.plot>["legend"]>[1],
): Option.Option<HTMLElement | SVGSVGElement> => Option.fromNullable(figure.legend(scale, options)) // a scale the spec never inferred answers nothing: absence is the legend's own verdict

const _pointed = (figure: ReturnType<typeof Plot.plot>, sink: (value: unknown) => void) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      // BOUNDARY ADAPTER: the figure's input event is the platform push seam; the nearest-datum row re-enters
      // as unknown and the consuming atom decodes it through the same Schema its source table was admitted by
      const listen = (): void => sink(figure.value)
      figure.addEventListener("input", listen)
      return listen
    }),
    (listen) => Effect.sync(() => figure.removeEventListener("input", listen)),
  )

const _spanned = <Datum,>(data: ReadonlyArray<Datum>, read: (datum: Datum) => number): Option.Option<[number, number]> =>
  pipe(extent(data, read), ([low, high]) =>
    Option.zipWith(Option.fromNullable(low), Option.fromNullable(high), (from, to): [number, number] => [from, to]),
  )

const _scaled = (held: Option.Option<Chart.Scale>, span: Chart.Span): Chart.Scale =>
  Option.match(held, {
    onNone: () => scaleLinear<number>(span),
    onSome: (scale) => updateScale(scale, span), // the instance survives the data change: the axes and the path hold one reference, so no consumer re-reads a fresh scale
  })

const _fold = <Datum,>(data: ReadonlyArray<Datum>, lens: Chart.Lens<Datum>): Chart.Fold =>
  pipe(
    Array.filterMap(bin<Datum, number>().value(lens.x).thresholds(lens.slots)(data), (slot) =>
      Option.zipWith(Option.fromNullable(slot.x0), Option.fromNullable(slot.x1), (from, to): Chart.Slot => ({ from, to, count: slot.length })),
    ),
    (slots) => ({
      census: HashMap.fromIterable(rollup(data, (rows) => rows.length, lens.series)),
      slots,
      ink: Option.map(_spanned(slots, (slot) => slot.count), (counts) => scaleSequential(counts, interpolateViridis)),
    }),
  )

const _inner = (frame: Chart.Frame, margin: Chart.Margin): Chart.Frame => ({
  width: frame.width - margin.left - margin.right,
  height: frame.height - margin.top - margin.bottom,
})

const _bespoke = <Datum,>(
  frame: Chart.Frame,
  data: ReadonlyArray<Datum>,
  lens: Chart.Lens<Datum>,
  held: Option.Option<Chart.Bespoke<Datum>>,
): Option.Option<Chart.Bespoke<Datum>> =>
  Option.gen(function* () {
    const across = yield* _spanned(data, lens.x)
    const up = yield* _spanned(data, lens.y)
    const inner = _inner(frame, lens.margin)
    const x = _scaled(Option.map(held, (spec) => spec.bottom.scale), { domain: across, range: [0, inner.width], nice: true, clamp: true })
    const y = _scaled(Option.map(held, (spec) => spec.left.scale), { domain: up, range: [inner.height, 0], nice: true, clamp: true }) // the inverted range IS the SVG y flip, stated once at the owner
    return {
      group: { top: lens.margin.top, left: lens.margin.left },
      bottom: { scale: x, top: inner.height, numTicks: lens.ticks },
      left: { scale: y, numTicks: lens.ticks },
      path: {
        data: [...data],
        x: (datum: Datum) => x(lens.x(datum)),
        y: (datum: Datum) => y(lens.y(datum)),
        defined: (datum: Datum) => Number.isFinite(lens.y(datum)),
      },
    }
  })
```

## [04]-[SERIES_SURFACE]

[SERIES_SURFACE]:
- Owner: `Chart.series(container, options, seed)` — the canvas bracket: `new uPlot(options, seed, container)` acquires, `destroy()` releases, and every post-mount mutation is `Chart.write` — the ONE imperative seam, whose payload shape selects the write: aligned columns are a data tick (`setData` inside an atom subscription with the fold owning cadence, high-frequency feeds coalescing to animation frames before the call), a frame is a resize (`setSize`, whose field pair IS `Chart.Frame`, so the `[2]` producer's value crosses with no projection), and the pair together coalesces inside `batch` so one repaint serves both. React never reconciles a point, rebuilding the instance per data tick is the named defect, and a `feed`/`resize` sibling pair is the arity twin this entrypoint deletes.
- Owner: `Chart.options(frame, cohort, series)` — the options value the bracket takes: the `[2]` frame spreads in as the required `width`/`height` pair, `cursor.sync` binds the cohort key, `cursor.drag` states the zoom posture, `legend.live`/`isolate` state the readout, and each `Chart.Series` row carries its resolved stroke beside a `uPlot.paths` geometry key, so a new series is a row and a new geometry is a key — never a second options builder.
- Packages: `uplot` (the `uPlot` class, `AlignedData`, the options tree — `series`/`scales`/`axes`/`cursor`/`legend`/`bands` — `uPlot.sync`, `uPlot.join`, `uPlot.paths.{linear,spline,stepped,bars,points}`, the hook-array plugin bus, `setData`/`setSize`/`batch`, the `ctx`/`over`/`under` readbacks); `system/token` (resolved stroke values — canvas reads no custom property); `apache-arrow` (`tableFromIPC` per-frame decode, `RecordBatchReader.from` for a single continuous IPC body).
- Law: the data contract is aligned columns — one x column, N y columns, typed arrays first-class, `null` the one gap marker with `spanGaps` per series; `Chart.columns` feeds it from the Arrow bus and outer-joins the multi-source arity through `uPlot.join`.
- Law: the feed is one entrypoint over two byte sources — `Chart.stream(feed, source)` answers `Stream<Chart.Aligned, ChartFault>` either way, discriminating on the platform class the value already is: a SINGLE continuous body is a `ReadableStream` and opens `RecordBatchReader.from` once, projecting each yielded `RecordBatch` batch-direct through the SAME `Chart.columns`, while every other source is a `Stream` of DISCRETE IPC frames each decoded through `tableFromIPC`. Both fold the projection into one bounded ring through one `Stream.mapAccum` step, so the window law has exactly one implementation; rebuilding a whole `Table` over an unbounded series per frame is the named defect, and a source-shaped parameter beside the source restates what the value answers.
- Law: the ring cap is policy, never a module constant — `points` arrives on the `Chart.Feed` row beside the column names, because a point ceiling is an assumption about a feed rate this owner never meets; the same law binds every window bound this folder holds.
- Law: dashboard cohorts sync by key — `uPlot.sync(key)` + `cursor.sync: { key }` link crosshair, focus, and zoom across a panel cohort; the key is a chart-group value the owning fold supplies as a parameter, never a literal held beside the options builder.
- Law: extension is a hook row — annotations, threshold shading, and tooltips ride the closed hook roster (`draw`/`drawSeries`/`setCursor`/…) as plugin hook arrays drawing into `u.ctx` or mounting into `u.over`; a fork of the draw loop is the named defect, and `u.ctx.canvas` is the only reach to the backing element (`view/export`'s raster row is its one consumer).
- Law: the stylesheet imports once — `uPlot.min.css` rides the token stylesheet; theme flips rebuild the options value from `Theme.Palette.ramp`-resolved strokes, and the canvas's missing per-point ARIA is compensated by the accessible summary row the regime's `summary` column already obliges.
- Growth: a new series is one options row; a new geometry is a `paths` builder key — never a second time-series engine.

```typescript
import { RecordBatchReader, tableFromIPC } from "apache-arrow"
import { Array, Effect, Option, Predicate, Stream } from "effect"
import uPlot from "uplot"

declare namespace Chart {
  type Series = { readonly label: string; readonly stroke: string; readonly geometry: keyof typeof uPlot.paths }
  type Feed = {
    readonly name: string
    readonly x: string
    readonly series: ReadonlyArray<string>
    readonly points: number
    readonly seed: Chart.Aligned
  }
}

const _series = (container: HTMLElement, options: uPlot.Options, seed: Chart.Aligned) =>
  Effect.acquireRelease(
    Effect.sync(() => new uPlot(options, seed, container)),
    (chart) => Effect.sync(() => chart.destroy()),
  )

function _write(chart: uPlot, payload: Chart.Aligned, frame: Chart.Frame): void
function _write(chart: uPlot, payload: Chart.Aligned): void
function _write(chart: uPlot, payload: Chart.Frame): void
function _write(chart: uPlot, payload: Chart.Aligned | Chart.Frame, frame?: Chart.Frame): void {
  // BOUNDARY ADAPTER: uPlot mutates through set* calls alone; the shape the payload already carries selects the write
  return Predicate.hasProperty(payload, "width")
    ? chart.setSize(payload)
    : frame === undefined
      ? chart.setData(payload)
      : chart.batch(() => {
        chart.setData(payload)
        chart.setSize(frame)
      })
}

const _options = (frame: Chart.Frame, cohort: string, series: ReadonlyArray<Chart.Series>): uPlot.Options => ({
  ...frame,
  ms: 1,
  cursor: { sync: { key: cohort }, drag: { x: true, y: false, setScale: true } },
  legend: { live: true, isolate: true },
  series: [
    {},
    ...Array.map(series, (row) => ({
      label: row.label,
      stroke: row.stroke,
      width: 1,
      spanGaps: true,
      paths: uPlot.paths[row.geometry](),
    })),
  ],
  axes: [{ side: 2 }, { side: 3 }],
})

const _tail = (held: Chart.Aligned, next: Chart.Aligned, points: number): Chart.Aligned => {
  // BOUNDARY ADAPTER: measured ring kernel — the streaming regime's per-frame cost IS its reason to exist, so each
  // column copies once into a preallocated draft and every draft detaches immutable at the return
  const window: Array<Chart.Column> = []
  for (let rank = 0; rank < held.length; rank += 1) {
    const prior = held[rank] as ArrayLike<number>
    const arrival = next[rank] as ArrayLike<number> | undefined
    const arrived = arrival === undefined ? 0 : arrival.length
    const width = Math.min(points, prior.length + arrived)
    const fromArrival = Math.min(arrived, width)
    const fromPrior = width - fromArrival
    const draft = new Float64Array(width)
    for (let slot = 0; slot < fromPrior; slot += 1) draft[slot] = prior[prior.length - fromPrior + slot]!
    for (let slot = 0; slot < fromArrival; slot += 1) draft[fromPrior + slot] = arrival![arrived - fromArrival + slot]!
    window.push(draft)
  }
  return window as Chart.Aligned
}

const _refused = (feed: Chart.Feed) => (defect: unknown): ChartFault =>
  new ChartFault({ reason: "frame-refused", feed: feed.name, detail: String(defect) })

const _sourced = (
  feed: Chart.Feed,
  source: Stream.Stream<Uint8Array, ChartFault> | ReadableStream<Uint8Array>,
): Stream.Stream<Option.Option<Chart.Aligned>, ChartFault> =>
  // the platform class is the evidence: a continuous body IS a ReadableStream, and everything else is the frame lane
  !(source instanceof ReadableStream)
    ? Stream.mapEffect(source, (frame) =>
      Effect.map(
        Effect.try({ try: () => tableFromIPC(frame), catch: _refused(feed) }), // a malformed frame is admission failure, never a defect
        (table) => _columns(table, feed.x, feed.series),
      ))
    : Stream.unwrap(
      Effect.map(
        Effect.tryPromise({ try: () => RecordBatchReader.from(source), catch: _refused(feed) }),
        (reader) =>
          Stream.map(
            Stream.fromAsyncIterable(reader, _refused(feed)), // one reader over the whole body: each batch projects with zero Table construction
            (batch) => _columns(batch, feed.x, feed.series),
          ),
      ),
    )

function _stream(feed: Chart.Feed, frames: Stream.Stream<Uint8Array, ChartFault>): Stream.Stream<Chart.Aligned, ChartFault>
function _stream(feed: Chart.Feed, body: ReadableStream<Uint8Array>): Stream.Stream<Chart.Aligned, ChartFault>
function _stream(
  feed: Chart.Feed,
  source: Stream.Stream<Uint8Array, ChartFault> | ReadableStream<Uint8Array>,
): Stream.Stream<Chart.Aligned, ChartFault> {
  // one window law, two byte sources: the discrete lane decodes a whole frame, the continuous lane pulls batches
  // off one reader, and both hand the SAME Option projection to the SAME ring step
  return Stream.mapAccum(_sourced(feed, source), feed.seed, (held, projected) =>
    Option.match(projected, {
      onNone: () => [held, held] as const, // a frame missing a named column advances nothing: the prior window re-emits
      onSome: (columns) => {
        const next = _tail(held, columns, feed.points)
        return [next, next] as const
      },
    }))
}
```

## [05]-[PIVOT_SURFACE]

[PIVOT_SURFACE]:
- Owner: `Chart.pivot(element, origin, feed)` — the engine bracket: `perspective.worker()` spawns the WASM engine off the UI thread, the `Chart.Origin` case decides how the table arrives, the `<perspective-viewer>` element (`HTMLPerspectiveViewerElement`, the package's own exported type) `load`s it, and release runs `element.delete()`, `table.delete()`, then `client.terminate()` — every handle INCLUDING the worker engine is a scoped resource, and a bracket that frees the table while the worker thread lives on is the named leak.
- Law: where the data lives is a case on one closed origin family, never an API fork — `Ingest` hands the bus frame to `client.table(frame, { format: "arrow", … })` (`index` makes updates upserts, `limit` ring-buffers a stream, `page_to_disk` spills past the memory ceiling — the table modes every feed chooses between, each an `Option` folded into the options value at the boundary so an unset mode omits its key rather than writing `undefined`), `Hosted` attaches to a host-published name through `open_table`, and `Joined` opens the LIVE reactive `client.join` re-deriving on either side's update. The three arms dispatch through `Match.valueTags` on the family's own tag, so a fourth origin is one case and one arm; a hand-maintained merged copy beside a `Joined` origin is the named defect.
- Law: the `ViewerConfig` is the ONE state value — `save()` emits it, `restore(update)` applies any subset, the config atom rides `Atom.kvs` with its schema, a `perspective-config-update` listener writes user-driven changes back through the atom, and atom-driven changes apply via `restore` — the same fold-echo law `Grid` follows for TanStack state; an attribute poke or DOM scrape beside the config value is the named defect.
- Law: deltas stream, never poll — engine updates land through `table.update(arrowBuffer)` and repaint every dependent view incrementally; `View.on_update({ mode: "row" })` deltas ARE Arrow buffers feeding derived consumers, and a hand-maintained aggregate copy beside a live `View`/`join` is the named defect.
- Law: a derived feed is a scoped view lane — `Chart.derive(pivot, feed, config)` opens `table.view(config)`, emits the `to_arrow` seed frame then every row-mode delta, and release runs `view.delete()`; each emitted frame is exactly `Chart.stream`'s discrete input, so pivot-derived series feed the streaming regime with no re-materialization.
- Law: this owner brackets every view the engine opens, in exactly two lanes — `Chart.derive` is the LIVE lane a subscriber consumes as a stream, and `Chart.snapshot(pivot, feed, config, read)` is the ONE-SHOT lane whose `read` parameter is the only thing that varies, so a serializer chooses `to_arrow`, `to_csv`, or `to_json` without owning a bracket. `view/export` composes the snapshot lane for every tabular parcel; a consumer calling `table.view` outside these two members opens a view no scope releases, which is the named leak whichever engine it belongs to.
- Law: expression columns validate before shipping — `Chart.expressions(pivot, exprs)` runs `table.validate_expressions(exprs)` and DECODES its report, because the package declares the return `unknown` and the engine answers a verdict record rather than throwing: refusals ride an `errors` map keyed by expression alias whose value carries the engine's own `error_message`, so a non-empty map fails the gate as `expression-refused` carrying each refused column beside its message, and a broken ExprTK column can never reach a `restore`. The aggregate vocabulary (`sum`/`distinct count`/`weighted mean`/`min by`/…) is the engine's roster referenced as data in the config value.
- Law: React reaches the element by ref only — mount runs the bracket in the effect seam, props never flow inside, config does; the element is the boundary.
- Law: the bracket is woven — acquisition carries `Effect.withSpan("rasm.ui.chart.pivot")` with the feed name as a log annotation, and every derived frame feeds `1` through `Effect.withMetric` into `_FRAMES`, so engine spin-up latency and delta throughput reach the app bridge with zero collector import; feed names stay log material, never metric tags.
- Growth: a new exploration surface is one bracket call with its own config atom; a headless consumer (export, alert, derived feed) rides `Chart.derive`'s view lane — never a second engine.

```typescript
import perspective from "@perspective-dev/client"
import "@perspective-dev/viewer"
import "@perspective-dev/viewer-datagrid"
import "@perspective-dev/viewer-charts"
import type { Client, JoinOptions, Table as PerspectiveTable, TableInitOptions, View, ViewConfigUpdate } from "@perspective-dev/client"
import type { HTMLPerspectiveViewerElement } from "@perspective-dev/viewer"
import { Convention } from "@rasm/ts/core"
import { Data, Effect, Match, Record, type Scope, Stream } from "effect"

declare namespace Chart {
  type Origin = Data.TaggedEnum<{
    Ingest: {
      readonly frame: ArrayBuffer
      readonly index: Option.Option<string>
      readonly limit: Option.Option<number>
      readonly spill: boolean // page_to_disk: the WASM worker's own OPFS-backed canonical store past the memory ceiling
    }
    Hosted: { readonly name: string }
    Joined: { readonly left: string; readonly right: string; readonly on: string; readonly kind: NonNullable<JoinOptions["join_type"]> }
  }>
  type Pivot = {
    readonly client: Client
    readonly table: PerspectiveTable
    readonly append: (delta: ArrayBuffer) => Effect.Effect<void, ChartFault>
  }
  type Shape = {
    readonly Fault: typeof ChartFault
    readonly Origin: typeof _Origin
    readonly regime: typeof _regimeRows
    readonly regimes: Chart.Regimes
    readonly useFrame: typeof _useFrame
    readonly columns: typeof _columns
    readonly plot: typeof _plot
    readonly legend: typeof _legend
    readonly pointed: typeof _pointed
    readonly bespoke: typeof _bespoke
    readonly fold: typeof _fold
    readonly series: typeof _series
    readonly options: typeof _options
    readonly write: typeof _write
    readonly stream: typeof _stream
    readonly pivot: typeof _pivot
    readonly expressions: typeof _expressions
    readonly derive: typeof _derive
    readonly snapshot: typeof _snapshot
  }
}

const _Origin = Data.taggedEnum<Chart.Origin>()

const _ingest = (origin: Extract<Chart.Origin, { readonly _tag: "Ingest" }>): TableInitOptions => ({
  format: "arrow",
  // each mode is Option-carried, so an unset mode omits its key rather than writing undefined into the options value
  ...Option.match(origin.index, { onNone: () => ({}), onSome: (index) => ({ index }) }),
  ...Option.match(origin.limit, { onNone: () => ({}), onSome: (limit) => ({ limit }) }),
  ...(origin.spill && { page_to_disk: true }),
})

const _opened = (client: Client, origin: Chart.Origin): Promise<PerspectiveTable> =>
  Match.valueTags(origin, {
    Ingest: (row) => client.table(row.frame, _ingest(row)),
    Hosted: ({ name }) => client.open_table(name),
    Joined: ({ left, right, on, kind }) => client.join(left, right, on, { join_type: kind }), // a LIVE reactive table: either side's update re-derives it
  })

const _pivot = (element: HTMLPerspectiveViewerElement, origin: Chart.Origin, feed: string) =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const client = yield* Effect.tryPromise({
        try: () => perspective.worker(),
        catch: (defect) => new ChartFault({ reason: "engine-lost", feed, detail: String(defect) }),
      })
      const table = yield* Effect.tryPromise({
        try: () => _opened(client, origin),
        catch: (defect) => new ChartFault({ reason: "frame-refused", feed, detail: String(defect) }),
      })
      yield* Effect.tryPromise({
        try: () => element.load(table),
        catch: (defect) => new ChartFault({ reason: "engine-lost", feed, detail: String(defect) }),
      })
      return {
        client,
        table,
        append: (delta: ArrayBuffer) =>
          Effect.asVoid(Effect.tryPromise({
            try: () => table.update(delta),
            catch: (defect) => new ChartFault({ reason: "frame-refused", feed, detail: String(defect) }),
          })),
      } satisfies Chart.Pivot
    }).pipe(
      Effect.withSpan("rasm.ui.chart.pivot"),
      Effect.annotateLogs({ pivot: feed }),
    ),
    (pivot) =>
      // release is total by signature: teardown resolves its own faults so the primary outcome survives it
      Effect.promise(async () => {
        await element.delete()
        await pivot.table.delete()
        pivot.client.terminate()
      }),
  )

// Engine answers a verdict record rather than throwing, and each refusal entry carries `error_message` — the gate decodes before it reads.
const _Validated = Schema.Struct({
  errors: Schema.Record({ key: Schema.String, value: Schema.Struct({ error_message: Schema.String }) }),
})

const _expressions = (
  pivot: Chart.Pivot,
  feed: string,
  exprs: Record.ReadonlyRecord<string, string>,
): Effect.Effect<void, ChartFault> =>
  Effect.tryPromise({
    try: () => pivot.table.validate_expressions({ ...exprs }),
    catch: (defect) => new ChartFault({ reason: "expression-refused", feed, detail: String(defect) }),
  }).pipe(
    Effect.flatMap(Schema.decodeUnknown(_Validated)),
    Effect.mapError((defect) => new ChartFault({ reason: "expression-refused", feed, detail: String(defect) })),
    Effect.filterOrFail(
      (report) => Record.isEmptyRecord(report.errors),
      (report) =>
        new ChartFault({
          reason: "expression-refused",
          feed,
          detail: Array.join(Record.toEntries(report.errors).map(([alias, issue]) => `${alias}: ${issue.error_message}`), "; "),
        }),
    ),
    Effect.asVoid,
  )

const _snapshot = <A>(
  pivot: Chart.Pivot,
  feed: string,
  config: ViewConfigUpdate,
  read: (view: View) => Promise<A>,
): Effect.Effect<A, ChartFault, Scope.Scope> =>
  Effect.flatMap(
    Effect.acquireRelease(
      Effect.tryPromise({
        try: () => pivot.table.view(config),
        catch: (defect) => new ChartFault({ reason: "view-lost", feed, detail: String(defect) }),
      }),
      (view) => Effect.promise(() => view.delete()),
    ),
    (view) =>
      Effect.tryPromise({
        try: () => read(view), // the ONLY axis a one-shot serializer varies: the bracket never forks per format
        catch: (defect) => new ChartFault({ reason: "view-lost", feed, detail: String(defect) }),
      }),
  )

const _FRAMES = Convention.mount(Convention.metric.chartFrames)

const _derive = (pivot: Chart.Pivot, feed: string, config: ViewConfigUpdate): Stream.Stream<Uint8Array, ChartFault> =>
  Stream.asyncScoped<Uint8Array, ChartFault>((emit) =>
    Effect.acquireRelease(
      Effect.tryPromise({
        try: async (): Promise<View> => {
          // BOUNDARY ADAPTER: on_update is the engine's push seam — the seed frame emits before the delta subscription arms
          const view = await pivot.table.view(config)
          void emit.single(new Uint8Array(await view.to_arrow()))
          await view.on_update(({ delta }) => {
            if (delta !== undefined) void emit.single(new Uint8Array(delta))
          }, { mode: "row" })
          return view
        },
        catch: (defect) => new ChartFault({ reason: "view-lost", feed, detail: String(defect) }),
      }),
      (view) => Effect.promise(() => view.delete()),
    ),
  ).pipe(Stream.tap(() => Effect.asVoid(Effect.withMetric(Effect.succeed(1), _FRAMES))))

const Chart: Chart.Shape = {
  Fault: ChartFault,
  Origin: _Origin,
  regime: _regimeRows,
  regimes: _regimes,
  useFrame: _useFrame,
  columns: _columns,
  plot: _plot,
  legend: _legend,
  pointed: _pointed,
  bespoke: _bespoke,
  fold: _fold,
  series: _series,
  options: _options,
  write: _write,
  stream: _stream,
  pivot: _pivot,
  expressions: _expressions,
  derive: _derive,
  snapshot: _snapshot,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Chart, ChartFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
