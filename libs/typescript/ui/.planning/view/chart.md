# [UI_CHART]

Chart owns declared statistics, streaming series, and user-driven pivots behind one data-shape discriminant. Observable Plot and visx render declared charts, uPlot renders streaming columns, and Perspective owns pivot aggregation. One Arrow table is their columnar bus; each surface brackets one engine, derives specs from atoms, and resolves color through tokens. Module: `ui/src/view/chart.ts`.

## [01]-[INDEX]

- [02]-[REGIME_LAW]: regime row table, Arrow columnar bus, fault family, one panel measurement; `Chart`, `ChartFault`, `ChartCensus`.
- [03]-[DECLARED_SURFACE]: Plot grammar bracket, decoded pointer readback, per-series bespoke marks under motion, d3 fold substrate; `Chart`.
- [04]-[SERIES_SURFACE]: uplot scoped instance — one imperative write, options value, two-source feed; `Chart`.
- [05]-[PIVOT_SURFACE]: perspective engine — bitness boot, origin family, workspace grain, expression and window gates, derived and borrowed lanes; `Chart`.

## [02]-[REGIME_LAW]

[REGIME_LAW]:
- Owner: `Chart` — one owner whose members are the three regime brackets and the columnar bus fold; regime selection is a `Chart.regime` row, never a component fork: DECLARED (the chart states a statistical claim — distribution, regression, facet, small multiple) renders through `[3]`; STREAMING (a telemetry/sensor/simulation series where point count breaks SVG) renders through `[4]`'s canvas; PIVOT (the USER drives group/split/aggregate/filter over a live feed) renders through `[5]`'s engine.
- Law: the regime table carries the decisions a consumer otherwise re-derives — `mount` names the bracket member the surface calls, `bus` names the Arrow projection it consumes, `ink` names the color path the surface reads (resolved values, `cn` classes, or a loaded theme name), `addressable` decides whether `[3]`'s bespoke lane is earned, and `summary` states which regimes owe the accessible summary row beside the chart; a conditional over regime names re-derives a column the row already carries.
- Law: Arrow is the inter-engine bus — `Chart.columns` projects an `apache-arrow` `Table` OR one `RecordBatch` into uplot's aligned columns through the shared `getChild(...).toArray()` spelling (`RecordBatch.getChild` carries the `Table` projection batch-direct, so the continuous-body reader lane never materializes a per-frame `Table`; a 64-bit lane's `BigInt64Array` widens to `Float64Array` at that one seam, because neither uplot's arithmetic nor the ring kernel admits a bigint), Plot marks take the `Table` directly with column-name channels, and perspective ingests the SAME frame's IPC bytes with `format: "arrow"`; a JSON re-materialization between Arrow-capable engines is the named defect. `Chart.columns` folds the whole projection to `Option.none` wherever a named column is absent from the source — the consumer renders no chart; a fabricated flat series standing in for a missing column is the named defect.
- Law: arity is the projection's own modality — one source projects directly, and a NON-EMPTY set of sources disagreeing on x outer-joins through `uPlot.join` inside the same member, discriminated by the `isArrowTable`/`isArrowRecordBatch` evidence each value already carries; a `join` sibling export beside `columns` is the arity twin this entrypoint deletes.
- Law: color obeys the token split — series strokes, categorical palettes, and axis inks resolve from `Theme.Palette.ramp`/`Theme` rows (a canvas engine takes resolved values rebuilt on theme flip, an SVG surface takes classes through `cn`, and a shadow-DOM element takes a theme NAME the token stylesheet already loaded), the regime row's `ink` column being the read that selects among them; `d3-scale-chromatic` colormaps appear ONLY where the color IS the datum's value (`scaleSequential(interpolateViridis)` density/heat), and a `scheme*` categorical array standing in for the token palette is the split-brain defect.
- Law: `d3` is substrate, never surface — `rollup`/`bin`/`extent` folds prepare data beside a spec, scale/curve/format vocabularies pass through, and the DOM-coupled modules (`d3-selection`/`d3-zoom`/`d3-axis`) never appear; React owns chart DOM, `system/act` owns gesture.
- Law: measurement flows one way — `Chart.useFrame(sizing)` is the ONE producer, one `useParentSize` observer per panel whose `Chart.Panel` hands back the callback `parentRef` the panel spreads and the `Chart.Frame` every resident chart takes as a parameter: `Chart.plot` passes it into the Plot options value, `Chart.write` hands it to uplot's `setSize`, `Chart.bespoke` divides it by the margin into scale ranges. Debounce arrives as a `Chart.Sizing` policy row the composing panel supplies, so no chart holds a window literal; a chart calling the observer itself, or taking bare `width`/`height` scalars with no producer, is the named defect.
- Packages: `apache-arrow` (`Table`/`RecordBatch` `getChild`, `isArrowTable`/`isArrowRecordBatch` narrowing); `uplot` (`AlignedData`, `join`, `setSize`); `@visx/responsive` (`useParentSize` — the one observer, `debounceTime`/`enableDebounceLeadingCall` as its policy row).
- Boundary: `Grid` (`view/table`) owns fixed-shape interactive collections at DOM scale; `viewer/geo` owns the live basemap (Plot's `geo` mark serves statistical maps only); `viewer/probe` and `viewer/panel` render their metric and telemetry boards THROUGH this owner; `view/export` serializes what these brackets render and holds no engine of its own.
- Growth: a new chart need selects a regime row; a new regime is one row with its bracket member on the one owner — never a sibling chart component family.

```typescript
import { Fault } from "@rasm/core"
import { useParentSize } from "@visx/responsive"
import { isArrowRecordBatch, isArrowTable, type RecordBatch, type Table } from "apache-arrow"
import { Array, Option, Schema } from "effect"
import uPlot from "uplot"

const _regimes = ["declared", "streaming", "pivot"] as const

const _regimeRows = {
  declared: { mount: "plot", bus: "table", ink: "class", addressable: true, summary: false },
  streaming: { mount: "series", bus: "aligned", ink: "resolved", addressable: false, summary: true },
  pivot: { mount: "pivot", bus: "ipc", ink: "themed", addressable: false, summary: true },
} as const

declare namespace Chart {
  type Regimes = typeof _regimes
  type Regime = keyof typeof _regimeRows
  type RegimeRow = {
    readonly mount: "plot" | "series" | "pivot"
    readonly bus: "table" | "aligned" | "ipc"
    readonly ink: "resolved" | "class" | "themed"
    readonly addressable: boolean
    readonly summary: boolean
  }
  type Aligned = uPlot.AlignedData
  type Column = uPlot.AlignedData[number]
  type Frame = { readonly width: number; readonly height: number }
  type Sizing = { readonly debounceTime: number; readonly enableDebounceLeadingCall: boolean }
  type Panel = { readonly parentRef: (node: HTMLDivElement | null) => void; readonly frame: Chart.Frame }
  type Source = RecordBatch | Table
  type _Rows<T extends Record<Regimes[number], RegimeRow> = typeof _regimeRows> = T
  type _Keys<K extends Regimes[number] = Regime> = K
}

const _named = (alias: Option.Option<string>): string => Option.getOrElse(Option.map(alias, (held) => ` ${held}`), () => "")

const _family = Fault.Class.family(
  ["engine-lost", "frame-refused", "expression-refused", "window-refused", "view-lost"] as const,
  {
    "engine-lost": Fault.Class.row({
      class: "unavailable",
      leg: "engine",
      detail: Schema.Struct({ feed: Schema.String, cause: Schema.String }),
      render: ({ cause, feed }) => `${feed} lost its engine: ${cause}`,
    }),
    "frame-refused": Fault.Class.row({
      class: "malformed",
      leg: "frame",
      detail: Schema.Struct({ feed: Schema.String, cause: Schema.String }),
      render: ({ cause, feed }) => `${feed} frame refused: ${cause}`,
    }),
    "expression-refused": Fault.Class.row({
      class: "invalid",
      leg: "expression",
      detail: Schema.Struct({
        feed: Schema.String,
        alias: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
        cause: Schema.String,
      }),
      render: ({ alias, cause, feed }) => `${feed} expression${_named(alias)} refused: ${cause}`,
    }),
    "window-refused": Fault.Class.row({
      class: "invalid",
      leg: "window",
      detail: Schema.Struct({
        feed: Schema.String,
        alias: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
        cause: Schema.String,
      }),
      render: ({ alias, cause, feed }) => `${feed} window${_named(alias)} refused: ${cause}`,
    }),
    "view-lost": Fault.Class.row({
      class: "unavailable",
      leg: "view",
      detail: Schema.Struct({ feed: Schema.String, cause: Schema.String }),
      render: ({ cause, feed }) => `${feed} lost its view: ${cause}`,
    }),
  },
)

declare namespace ChartFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class ChartFault extends Schema.TaggedError<ChartFault>()("ChartFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const ChartCensus = _family.census("ChartCensus")
type ChartCensus = InstanceType<typeof ChartCensus>

const _numeric = (column: ArrayLike<number> | ArrayLike<bigint>): Chart.Column =>
  column.length > 0 && typeof column[0] === "bigint"
    ? Float64Array.from(column as ArrayLike<bigint>, Number)
    : column as Chart.Column

const _project = (source: Chart.Source, x: string, series: ReadonlyArray<string>): Option.Option<Chart.Aligned> =>
  Option.map(
    Option.all(Array.map([x, ...series], (name) => Option.fromNullable(source.getChild(name)))),
    (children) => Array.map(children, (child) => _numeric(child.toArray() as ArrayLike<number> | ArrayLike<bigint>)) as Chart.Aligned,
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
  return isArrowTable(input) || isArrowRecordBatch(input)
    ? _project(input, x, series)
    : Option.map(Option.all(Array.map(input, (one) => _project(one, x, series))), (lanes) => uPlot.join([...lanes]))
}

const _useFrame = (sizing: Chart.Sizing): Chart.Panel => {
  const measured = useParentSize(sizing)
  return { parentRef: measured.parentRef, frame: { width: measured.width, height: measured.height } }
}
```

## [03]-[DECLARED_SURFACE]

[DECLARED_SURFACE]:
- Owner: `Chart.plot(container, frame, build)` — the grammar bracket: `build` takes the panel frame and derives a `Plot.plot(options)` element from decoded inputs (marks over channels, transforms as option rewriters — `binX`/`group`/`stackY`/`windowY` in the options value, never a pre-shaped copy of the data), the bracket mounts it through `replaceChildren` and removes it on release; rebuild-per-change is the model — the grammar rebuilds cheaply, which is exactly why streaming series live on `[4]` instead.
- Owner: `Chart.bespoke(frame, data, lens, held)` — the visx spec fold: ONE `Chart.Lens` policy row carries accessors, margin, tick count, and slot count; `lens.series` cuts the data into the MARK ROSTER the member answers — one `LinePath` prop record and one motion target per series key, in first-appearance order — and the whole fold answers `Option.none` on data whose span is empty rather than minting a forged domain. `Chart.fold(data, lens)` is the d3 substrate beside it, and both are pure — the render site spreads the returned prop records onto the shape components and holds no scale of its own.
- Packages: `@observablehq/plot` (`plot`, the mark roster — `dot`/`lineY`/`areaY`/`barY`/`rectY`/`cell`/`boxY`/`linearRegressionY`/`density`/`raster`/`contour`/`tree`/`geo` — the transform roster, `tip`/`pointer`/`crosshair` interaction, `facet`, named projections); `@visx/scale` (`scaleLinear`, `updateScale`) + `@visx/shape` (`LinePath`) + `@visx/axis` (`AxisBottom`, `AxisLeft`) + `@visx/group` (`Group`) — the bespoke lane; `motion` (`motion.create` over the shape component, `MotionConfig`'s `isValidProp` forwarding gate); `d3` (`extent`, `bin`, `rollup`, `scaleSequential`, `interpolateViridis` — the fold substrate); `system/act` (`Motion.Spring` — the temperament rows a target names, `Motion.springs` their physics).
- Law: interaction writes back through the store DECODED — `tip: true` renders channel values, and `Chart.pointed(figure, schema, sink)` is the scoped `input` subscription that decodes the figure's own `value` (the nearest-datum row a `pointer`/`crosshair` mark writes, which the package types `unknown` and nulls the moment the pointer leaves) through the Schema the readback is admitted by, ONCE at this seam: the consuming atom therefore takes the decoded shape and both absences — pointer-out and a row the schema refuses — arrive as the same `Option.none`. `Chart.pointed`'s subscription releases with the bracket that mounted the figure, chart-as-input state never lands in component state, and a sink taking `unknown` defers a decode that then has no owner, exactly as a `pointer` mark rendered with no listener claims an input seam it never opened.
- Law: the legend is a second detached element, never a hand-drawn key — `Chart.legend(figure, scale, options)` reads `figure.legend(name, options)` for a scale the spec already inferred, and the consumer mounts it wherever the layout wants it; a swatch row rebuilt from the palette beside a chart restates the scale the figure carries.
- Law: the visx lane is earned by per-element addressability — RAC-adjacent handlers, per-datum a11y, custom hit logic on React-owned SVG elements: the panel frame divides by margin into scale ranges, `scaleLinear` takes a config object, `updateScale` re-domains the HELD scale on a data change so axes and every mark keep one reference, and the axes and the marks read those same two instances inside one margin-translated `Group`. Rows materialize here and nowhere else on the page, because per-datum addressability IS the lane's earning; a chart needing none of that is a Plot spec, and a hand-built `d` string where a shape component exists is the named defect.
- Law: one mark per SERIES, and `Chart.Lens.series` is the ONE partition the page cuts on — `Chart.fold`'s census counts by it and `Chart.bespoke` cuts the mark roster by it, so a lens accessor no mark honors is the dead-column defect and a second grouping key beside it forks the partition. Each `Chart.Mark` carries its series key (the React identity that survives a re-domain), its `LinePath` prop record, and its `Chart.Target` together, so no render site re-groups the data a fold already grouped.
- Law: motion arrives as a WRAPPER, never a second renderer — the mounted shape is `motion.create(LinePath)` under the subtree `MotionConfig` `Chart.motion` supplies, whose `isValidProp` is the SOLE gate deciding which props the wrapper forwards to the DOM, so one predicate at the provider covers every wrapped visx shape beneath it. Hand-animating an attribute, reading reduced motion per mark, or mirroring a `MotionValue` into React or the atom store is the engine's own named defect.
- Law: the accelerated set decides what a target may hold — `opacity`, `transform`, `filter`, `clipPath`, and `backgroundColor` hand off to WAAPI on SVG exactly as on HTML, and EVERY other property (the path `d` included) generates on rAF. `Chart.Target` therefore carries the opacity and the inner-frame `clipPath` wipe alone; the geometry change rides `updateScale` re-domaining the held instances, and interpolating `d` is rejected because it pays a per-frame rAF cost to restate a path `LinePath` already regenerates from the re-domained scale.
- Law: enter, re-domain, and selection echo are ONE continuous plane — each target names a `Motion.Spring` temperament row (`system/act#CONTINUOUS_MOTION`) and the engine interpolates toward it, so a mark appearing, a domain widening under it, and a selection dimming its siblings all resolve on the springs that plane owns; a tween table, a CSS transition, or a second animation engine beside them is the split-brain the class-row/physical-plane split already forecloses.
- Law: prop records lift, never restate — every returned record is `ComponentProps` of the component it feeds (`typeof AxisBottom<Chart.Scale>` pre-solves the axis generic at the owner), so a visx prop rename breaks at this declaration; visx props take mutable arrays and tuples, and this fold is the one seam the readonly interior copies at.
- Law: `d3` enters through the fold members alone and every partial or foreign answer converts here — `extent` answers `[undefined, undefined]` on an empty read, so a span lifts through `Option` and an empty chart is `Option.none`; `rollup` answers d3's own `InternMap`, so the census converts to `HashMap` at the seam and no JS map crosses into the interior; a `Bin` carries `x0`/`x1` as possibly-absent edges, so an edgeless slot drops instead of rendering at zero width. `scaleSequential(interpolateViridis)` inks the density slots because the color IS the slot's count — no other d3 scale reaches a rendered surface.
- Law: Arrow plots directly — `Plot.dot(table, { x: "<column-a>", y: "<column-b>" })` consumes the bus `Table` with column-name shorthand and Arrow date detection; rows never materialize for a declared chart, and a `build` that pre-shapes the table into rows before a mark reads it forfeits the whole reason the bus exists.
- Law: `Plot.plot` answers `SVGSVGElement | HTMLElement` — a caption, a title, or a legend option wraps the svg in a figure — so every consumer reading the element as SVG discriminates on the tag it holds; `view/export`'s serializer row is the one place that discrimination is spelled, and an `as SVGSVGElement` at a mount site is the named defect.
- Growth: a new declared chart is a spec value — marks, transforms, facets as data; a new bespoke shape is one prop record on `Chart.Mark` reading the same scale pair, a new motion behaviour is one field on `Chart.Target`, and a new fold statistic is one field on `Chart.Fold` — never a d3-rendered surface beside any of them.

```typescript
import * as Plot from "@observablehq/plot"
import { AxisBottom, AxisLeft } from "@visx/axis"
import { Group } from "@visx/group"
import { scaleLinear, updateScale } from "@visx/scale"
import { LinePath } from "@visx/shape"
import { bin, extent, interpolateViridis, rollup, scaleSequential } from "d3"
import { Array, Effect, HashMap, Option, pipe, Record, Schema } from "effect"
import { motion, MotionConfig } from "motion/react"
import type { ComponentProps } from "react"
import type { Motion } from "../system/act.ts"

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
  type Target = {
    readonly initial: { readonly opacity: number; readonly clipPath: string }
    readonly animate: { readonly opacity: number; readonly clipPath: string }
    readonly spring: Motion.Spring
  }
  type Mark<Datum> = {
    readonly key: string
    readonly path: ComponentProps<typeof LinePath<Datum>>
    readonly target: Chart.Target
  }
  type Bespoke<Datum> = {
    readonly group: ComponentProps<typeof Group>
    readonly bottom: ComponentProps<typeof AxisBottom<Chart.Scale>>
    readonly left: ComponentProps<typeof AxisLeft<Chart.Scale>>
    readonly marks: ReadonlyArray<Chart.Mark<Datum>>
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
): Option.Option<HTMLElement | SVGSVGElement> => Option.fromNullable(figure.legend(scale, options))

const _pointed = <A, I>(
  figure: ReturnType<typeof Plot.plot>,
  schema: Schema.Schema<A, I>,
  sink: (row: Option.Option<A>) => void,
) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const decode = Schema.decodeUnknownOption(schema)
      const listen = (): void => sink(decode(figure.value))
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
    onSome: (scale) => updateScale(scale, span),
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

const _MARK = motion.create(LinePath)

const _ENGINE: ReadonlyArray<string> = [
  "initial", "animate", "exit", "variants", "transition", "layout", "layoutId",
  "drag", "whileHover", "whileTap", "whileFocus", "whileDrag", "whileInView",
]

const _MOTION = {
  reducedMotion: "user",
  isValidProp: (key: string): boolean => !_ENGINE.includes(key),
} as const satisfies ComponentProps<typeof MotionConfig>

const _target = (inner: Chart.Frame): Chart.Target => ({
  initial: { opacity: 0, clipPath: `inset(0 ${inner.width}px 0 0)` },
  animate: { opacity: 1, clipPath: "inset(0 0 0 0)" },
  spring: "glide",
})

const _marked = <Datum,>(
  data: ReadonlyArray<Datum>,
  lens: Chart.Lens<Datum>,
  x: Chart.Scale,
  y: Chart.Scale,
  inner: Chart.Frame,
): ReadonlyArray<Chart.Mark<Datum>> =>
  Array.map(
    Record.toEntries(Array.groupBy(data, lens.series)),
    ([key, rows]): Chart.Mark<Datum> => ({
      key,
      path: {
        data: rows,
        x: (datum: Datum) => x(lens.x(datum)),
        y: (datum: Datum) => y(lens.y(datum)),
        defined: (datum: Datum) => Number.isFinite(lens.y(datum)),
      },
      target: _target(inner),
    }),
  )

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
    const y = _scaled(Option.map(held, (spec) => spec.left.scale), { domain: up, range: [inner.height, 0], nice: true, clamp: true })
    return {
      group: { top: lens.margin.top, left: lens.margin.left },
      bottom: { scale: x, top: inner.height, numTicks: lens.ticks },
      left: { scale: y, numTicks: lens.ticks },
      marks: _marked(data, lens, x, y, inner),
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
  new ChartFault({ case: { reason: "frame-refused", feed: feed.name, cause: String(defect) } })

const _sourced = (
  feed: Chart.Feed,
  source: Stream.Stream<Uint8Array, ChartFault> | ReadableStream<Uint8Array>,
): Stream.Stream<Option.Option<Chart.Aligned>, ChartFault> =>
  !(source instanceof ReadableStream)
    ? Stream.mapEffect(source, (frame) =>
      Effect.map(
        Effect.try({ try: () => tableFromIPC(frame), catch: _refused(feed) }),
        (table) => _columns(table, feed.x, feed.series),
      ))
    : Stream.unwrap(
      Effect.map(
        Effect.tryPromise({ try: () => RecordBatchReader.from(source), catch: _refused(feed) }),
        (reader) =>
          Stream.map(
            Stream.fromAsyncIterable(reader, _refused(feed)),
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
  return Stream.mapAccum(_sourced(feed, source), feed.seed, (held, projected) =>
    Option.match(projected, {
      onNone: () => [held, held] as const,
      onSome: (columns) => {
        const next = _tail(held, columns, feed.points)
        return [next, next] as const
      },
    }))
}
```

## [05]-[PIVOT_SURFACE]

[PIVOT_SURFACE]:
- Owner: `Chart.boot(bitness)` — the engine registration cell: ONE idempotent `init_server` fold over a bitness THUNK PAIR, run once at composition ahead of every acquisition bracket. Registration is module-global engine state, so its owner is explicit — never an import side effect, and never inside `Chart.pivot`, where a second element mounting re-registers the engine under a live client. Each source rides an `Option` so an unregistered bitness omits its key rather than writing `undefined`, and a registration carrying neither key throws at the package.
- Owner: `Chart.pivot(element, origin, feed, config)` — the engine bracket: `perspective.worker()` spawns the WASM engine off the UI thread, the `Chart.Origin` case decides how the NAMED table arrives, the `<perspective-viewer>` element (`HTMLPerspectiveViewerElement`, the package's own exported type) `load`s it, the decoded `Chart.Config` lands through `restoreWorkspace`, and release runs `element.delete()`, `table.delete()`, then `client.terminate()` — every handle INCLUDING the worker engine is a scoped resource, and a bracket that frees the table while the worker thread lives on is the named leak.
- Packages: `@perspective-dev/client` (`worker`, `init_server` over its `ServerWasmSource`/`ServerWasmRegistration` pair, `host_supports_memory64`, `Client`/`Table`/`View`, `TableInitOptions`, `JoinOptions`, `ViewConfigUpdate` with its `windows` column, `TypedArrayWindow`, `Features.window_aggregates`); `@perspective-dev/viewer` + `-datagrid` + `-charts` (`HTMLPerspectiveViewerElement`, the panel and workspace round trips, `PerspectiveConfigUpdateEventDetail`); `@effect-atom/atom-react` (`Atom.kvs` — the persisted row `system/atom#STORE_ROOT` owns); `effect` (`Data`, `HashSet`, `Match`, `Record`, `Schema`, `Stream`).
- Law: bitness is a REGISTRATION the boot cell owns, never a probe at a call site — sources register as thunks, so SELECTION AND DOWNLOAD defer to the first `worker()` call and the losing binary is never fetched. `wasm64` wins wherever it is registered and either no `wasm32` stands beside it or `host_supports_memory64()` holds — a memoized `WebAssembly.validate` over the `(module (memory i64 1))` encoding — so a LONE `wasm64` registration is honored regardless of the probe and fails at instantiation, which is the correct error for an explicit opt-in; a selected `wasm64` that fails to load with a `wasm32` registered falls back to it behind a console warning. Memory64 rides a REAL second binary (`perspective-server.memory64.wasm`) whose memory maximum is 262144 pages against the wasm32 engine's 65536 — a 16GB heap ceiling against 4GB, at engine cost — so a feed whose frames outgrow 4GB registers both and a feed that never will registers `wasm32` alone. `/inline` embeds the wasm32 engine ONLY and boots itself on import, so a heap-ceiling surface can never ride it.
- Law: the persisted atom holds the WORKSPACE grain — `Chart.Config` is the decoded whole-element token (`version`, `active`, the recursive `layout` tree, the `panels` map, `global_filters`, `masters`), `Chart.CONFIG` is its storage key minted through `system/atom#STORE_ROOT`'s `Store.key` and held for the grain's life, `Store.sealed` carries the generation inside the stored bytes so a config-schema bump refuses the superseded token on content under the `discard` disposition, and `Chart.config(runtime, seed)` is the ONE persisted atom; `Chart.workspace` applies it through `restoreWorkspace`, which is strict by construction because every `panels` entry mints a NEW panel whose `table` is required by type. Per-panel edits flow `Chart.panel(element, feed, panel, update)` instead — `restore` under the `{panel}` selector, carrying `suppress_errors` because a programmatic patch's failure is this rail's fault value, not the viewer's visible error state. `restore` and `restoreWorkspace` NEVER substitute: `restore` handed a workspace token silently IGNORES its `panels` and `layout`, while `restoreWorkspace` handed a viewer token REJECTS, `panels` being non-defaulted.
- Law: the round trip is asymmetric in exactly three places, and each is folded at its own seam — `saveWorkspace()` emits `panels` in `BTreeMap` key order so consecutive saves stay byte-stable but emits `layout: null` for an unlaid element where the restore field is absent-or-present, so the null folds away at `Chart.saved` and never crosses back as an empty tree; restoring EJECTS every pre-existing panel and REMAPS the saved layout's panel ids onto the newly minted ones, which is why a tab node's ids are never a stable handle; and `restore({ panel })` naming an absent panel UPSERTS it, a table-less upsert yielding a DEFERRED panel the next `load()` binds — it renders, and `save()` refuses it until the binding lands.
- Law: `global_filters` and `masters` have NO JS setter — a master panel's selection contributes the clauses, `saveWorkspace`/`restoreWorkspace` carry them, and nothing else writes them. `global_filters` is a transient overlay every DETAIL panel reads and no panel config records, restored as ONE unattributed bucket the next master selection replaces; a `masters` id absent from `panels` warns and drops, and a restored master re-enters its row-tree selection edit mode. Keeping a cross-filter beside the workspace token mints the second state the overlay exists to foreclose.
- Law: every panel-scoped read takes the SAME selector — `PanelOptions` rides `getView`, `getViewConfig`, `getSelection`, `setSelection`, `getEditPort`, `getRenderStats`, `reset`, and `toggleColumnSettings`, defaulting to the active panel, so addressing a panel is one options field and never a second element handle. `Chart.Move` closes the panel-MOVE family — `Add` a configured panel, `Drop` one by id, `Focus` one by id, dispatching through `Match.valueTags` — and every arm answers the same `Chart.Board` read, because `Drop` against the LAST remaining panel resolves as a no-op and the roster after the move is the only honest evidence of what happened.
- Law: `perspective-config-update` narrows the echo seam to ONE synchronous turn — the event hands a `getConfig()` thunk whose closure releases the instant dispatch returns, and the thunk answers the PANEL patch, so the listener calls it AND reads `getActivePanel()` inside that same turn, then folds the pair into the workspace atom's own `panels` entry. Stashing the thunk for a later read throws, a config read outside a handler goes to `save()`/`saveWorkspace()`, and an attribute poke or DOM scrape beside the config value is the named defect.
- Law: where the data lives is a case on one closed origin family, never an API fork — every arm NAMES its table, because a panel resolves its `table` field against the element's default client and an unnamed table is a panel that can never bind: `Ingest` hands the bus frame to `client.table(frame, { format: "arrow", name, … })` (`index` makes updates upserts, `limit` ring-buffers a stream, `page_to_disk` spills past the memory ceiling, `list_flatten` decides how a list-bearing frame expands — the table modes every feed chooses between, each an `Option` folded into the options value at the boundary so an unset mode omits its key rather than writing `undefined`), `Hosted` attaches to a host-published name through `open_table`, and `Joined` opens the LIVE reactive `client.join` re-deriving on either side's update under its own name. Three arms dispatch through `Match.valueTags` on the family's own tag, so a fourth origin is one case and one arm; a hand-maintained merged copy beside a `Joined` origin is the named defect.
- Law: deltas stream, never poll — engine updates land through `table.update(arrowBuffer)` and repaint every dependent view incrementally; `View.on_update({ mode: "row" })` deltas ARE Arrow buffers feeding derived consumers, and a hand-maintained aggregate copy beside a live `View`/`join` is the named defect.
- Law: a derived feed is a scoped view lane — `Chart.derive(pivot, feed, config)` opens `table.view(config)`, emits the `to_arrow` seed frame then every row-mode delta, and release runs `view.delete()`; each emitted frame is exactly `Chart.stream`'s discrete input, so pivot-derived series feed the streaming regime with no re-materialization.
- Law: this owner brackets every view the engine opens, in exactly two lanes — `Chart.derive` is the LIVE lane a subscriber consumes as a stream, and `Chart.snapshot(pivot, feed, config, read)` is the ONE-SHOT lane whose `read` parameter is the only thing that varies, so a serializer chooses `to_arrow`, `to_csv`, or `to_json` without owning a bracket. `view/export` composes the snapshot lane for every tabular parcel; a consumer calling `table.view` outside these two members opens a view no scope releases, which is the named leak whichever engine it belongs to.
- Law: the borrow lane LENDS, it never yields — `Chart.borrow(pivot, feed, config, window, consume)` is the snapshot lane's zero-copy read, handing the consumer `(names, values, validities, dictionaries)` decoded straight off the Arrow buffer for exactly the borrow's life: it ends at the callback's synchronous return, or — when the callback answers a `Promise` — at its SETTLEMENT, so an awaited canvas write or GPU upload EXTENDS the borrow legally while a view stashed past settlement reads freed memory. `Chart.borrow` therefore brackets ITS CONSUMER and never becomes a stream element, because a borrowed array crossing a `Stream` boundary outlives the callback by construction — the exact dangle this seam exists to foreclose.
- Law: the borrow window carries three traps the type does not state — `float32: true` narrows Float64 AND Int64 columns to `Float32Array` while DELIBERATELY leaving Date32 and Timestamp on `Float64Array`, because epoch milliseconds in f32 quantize to roughly a quarter second, so the temporal exemption is correctness rather than an omission and a consumer expecting narrowed dates reads the wrong array class. `float32` is NON-OPTIONAL on the window type though the engine defaults it, so the value spells it always. Null slots arrive ZERO-FILLED in place, making `validities` the ONLY absence evidence and a zero read as a datum the named defect. And the call FORCES `emit_legacy_row_path_names` to `false` whatever the window says, so a group-by lane reads `__ROW_PATH_N__` here while a `to_*` serializer over the SAME view still defaults to the legacy `"colname (Group by N)"` spelling.
- Law: expression columns validate before shipping — `Chart.expressions(pivot, feed, exprs)` runs `table.validate_expressions(exprs)` and DECODES its report, because the package declares the return `unknown` and the engine answers a verdict record rather than throwing: refusals ride an `errors` map keyed by expression alias whose value carries the engine's own `error_message`, so a non-empty map fails the gate as `expression-refused` carrying each refused column beside its message, and a broken ExprTK column can never reach a `restore`. Aggregate vocabulary (`sum`/`distinct count`/`weighted mean`/`min by`/…) rides the engine's roster referenced as data in the config value, and a rolling computation is the config's own `windows` column keyed by output alias — the engine maintains a moving average, cumulative sum, or rank incrementally against every update, so a hand-folded rolling series beside the view is the same defect a hand-maintained aggregate is.
- Law: windows have NO pre-ship validator, so their gate is LOCAL and cannot borrow the expression one — `validate_expressions` deserializes `Expressions` alone and answers nothing about `windows`, while an alias collision refuses ENGINE-side only at view build. `Chart.windows(pivot, feed, exprs, rows)` therefore folds the alias set BEFORE any restore, against the `Table` schema, the same config's expression aliases, its own sibling keys, and the frame trio. `Chart.windows` takes ORDERED ENTRIES and answers the record: a `Windows` object literal has already collapsed its duplicate keys through JSON last-wins, so the entry list is the one shape where a duplicate is still evidence. `rows`, `range`, and `cumulative` are mutually exclusive and NOT type-encoded, so exclusivity is the gate's own count, and `alpha` lives in `(0, 1]`.
- Law: the engine refuses its own roster at view build, and the page names those refusals as data — `ema` accepts the cumulative frame alone, `rate` REQUIRES a `range` frame, `lag`/`lead`/`diff` take an `offset` and no frame at all, a `range` frame needs a numeric or temporal `order_by`, and `order_by` orders rows WITHIN the frame without reordering the `View`, which `sort` alone does. Virtual sources declare their own roster instead — `Features.window_aggregates` keyed by `ColumnType`, each `WindowAggSpec` naming the `frames` it accepts, whether it takes an `offset` or an `alpha`, and its `result_type` — reached through the handler's `getFeatures()`, so a source-specific refusal is a read, never a fork of this gate.
- Law: React reaches the element by ref only — mount runs the bracket in the effect seam, props never flow inside, config does; the element is the boundary.
- Law: the bracket is woven — acquisition carries `Effect.withSpan("rasm.ui.chart.pivot")` with the feed name as a log annotation, and every derived frame feeds `1` through `Effect.withMetric` into `_FRAMES`, so engine spin-up latency and delta throughput reach the app bridge with zero collector import; feed names stay log material, never metric tags.
- Growth: a new exploration surface is one bracket call over the ONE workspace atom; a new panel is a `Chart.Move` case value, a new engine bitness a `Chart.Bitness` key, and a headless consumer (export, alert, derived feed) rides `Chart.derive`'s view lane or `Chart.borrow`'s lend — never a second engine.

```typescript
import { Atom } from "@effect-atom/atom-react"
import type { KeyValueStore } from "@effect/platform"
import perspective from "@perspective-dev/client"
import "@perspective-dev/viewer"
import "@perspective-dev/viewer-datagrid"
import "@perspective-dev/viewer-charts"
import type { Client, JoinOptions, Table as PerspectiveTable, TableInitOptions, TypedArrayWindow, View, ViewConfigUpdate } from "@perspective-dev/client"
import type { HTMLPerspectiveViewerElement, PerspectiveConfigUpdateEventDetail, ViewerConfigUpdate, WorkspaceConfigUpdate } from "@perspective-dev/viewer"
import { Convention } from "@rasm/core"
import { Data, Effect, HashSet, Match, Record, Schema, type Scope, Stream } from "effect"
import { Store } from "../system/atom.ts"

declare namespace Chart {
  type Wasm = () => Promise<ArrayBuffer | Response | WebAssembly.Module>
  type Bitness = { readonly wasm32: Option.Option<Chart.Wasm>; readonly wasm64: Option.Option<Chart.Wasm> }
  type Windows = NonNullable<ViewConfigUpdate["windows"]>
  type Window = NonNullable<Chart.Windows[string]>
  type Initial = Parameters<HTMLPerspectiveViewerElement["addPanel"]>[0]
  type Json = null | boolean | number | string | Array<Chart.Json> | { [key: string]: Chart.Json }
  type Layout =
    | { readonly type: "split-layout"; children: Array<Chart.Layout>; sizes: Array<number>; readonly orientation: "horizontal" | "vertical" }
    | { readonly type: "tab-layout"; tabs: Array<string>; readonly selected?: number }
  type Config = Schema.Schema.Type<typeof _Config>
  type Board = { readonly panels: ReadonlyArray<string>; readonly active: Option.Option<string> }
  type Lend = (
    names: ReadonlyArray<string>,
    values: ReadonlyArray<ArrayLike<number> | ArrayLike<bigint>>,
    validities: ReadonlyArray<Uint8Array | null>,
    dictionaries: ReadonlyArray<ReadonlyArray<string> | null>,
  ) => void | Promise<void>
  type Move = Data.TaggedEnum<{
    Add: { readonly config: Chart.Initial }
    Drop: { readonly panel: string }
    Focus: { readonly panel: string }
  }>
  type Origin = Data.TaggedEnum<{
    Ingest: {
      readonly frame: ArrayBuffer
      readonly name: string
      readonly index: Option.Option<string>
      readonly limit: Option.Option<number>
      readonly spill: boolean
      readonly lists: Option.Option<NonNullable<TableInitOptions["list_flatten"]>>
    }
    Hosted: { readonly name: string }
    Joined: {
      readonly name: string
      readonly left: string
      readonly right: string
      readonly on: string
      readonly kind: NonNullable<JoinOptions["join_type"]>
    }
  }>
  type Pivot = {
    readonly client: Client
    readonly table: PerspectiveTable
    readonly element: HTMLPerspectiveViewerElement
    readonly append: (delta: ArrayBuffer) => Effect.Effect<void, ChartFault>
  }
  type Shape = {
    readonly Census: typeof ChartCensus
    readonly Fault: typeof ChartFault
    readonly Origin: typeof _Origin
    readonly Move: typeof _Move
    readonly Config: typeof _Config
    readonly CONFIG: typeof _CONFIG
    readonly regime: typeof _regimeRows
    readonly regimes: Chart.Regimes
    readonly useFrame: typeof _useFrame
    readonly columns: typeof _columns
    readonly plot: typeof _plot
    readonly legend: typeof _legend
    readonly pointed: typeof _pointed
    readonly bespoke: typeof _bespoke
    readonly fold: typeof _fold
    readonly mark: typeof _MARK
    readonly motion: typeof _MOTION
    readonly series: typeof _series
    readonly options: typeof _options
    readonly write: typeof _write
    readonly stream: typeof _stream
    readonly boot: typeof _boot
    readonly config: typeof _config
    readonly pivot: typeof _pivot
    readonly workspace: typeof _workspace
    readonly panel: typeof _panel
    readonly saved: typeof _saved
    readonly moved: typeof _moved
    readonly echo: typeof _echo
    readonly expressions: typeof _expressions
    readonly windows: typeof _windows
    readonly derive: typeof _derive
    readonly snapshot: typeof _snapshot
    readonly borrow: typeof _borrow
  }
}

const _Origin = Data.taggedEnum<Chart.Origin>()
const _Move = Data.taggedEnum<Chart.Move>()

let _registered = false

const _boot = (bitness: Chart.Bitness): Effect.Effect<void> =>
  Effect.sync(() => {
    if (_registered) {
      return
    }
    _registered = true
    perspective.init_server({
      ...Option.match(bitness.wasm32, { onNone: () => ({}), onSome: (wasm32) => ({ wasm32 }) }),
      ...Option.match(bitness.wasm64, { onNone: () => ({}), onSome: (wasm64) => ({ wasm64 }) }),
    })
  })

const _ingest = (origin: Extract<Chart.Origin, { readonly _tag: "Ingest" }>): TableInitOptions => ({
  format: "arrow",
  name: origin.name,
  ...Option.match(origin.index, { onNone: () => ({}), onSome: (index) => ({ index }) }),
  ...Option.match(origin.limit, { onNone: () => ({}), onSome: (limit) => ({ limit }) }),
  ...Option.match(origin.lists, { onNone: () => ({}), onSome: (list_flatten) => ({ list_flatten }) }),
  ...(origin.spill && { page_to_disk: true }),
})

const _opened = (client: Client, origin: Chart.Origin): Promise<PerspectiveTable> =>
  Match.valueTags(origin, {
    Ingest: (row) => client.table(row.frame, _ingest(row)),
    Hosted: ({ name }) => client.open_table(name),
    Joined: ({ name, left, right, on, kind }) => client.join(left, right, on, { join_type: kind, name }),
  })

// --- [WORKSPACE_CODEC]

const _Scalar = Schema.Union(Schema.Number, Schema.String, Schema.Boolean, Schema.Null)

const _Strings = Schema.mutable(Schema.Array(Schema.String))
const _Filter = Schema.mutable(
  Schema.Tuple(Schema.String, Schema.String, Schema.Union(_Scalar, Schema.mutable(Schema.Array(_Scalar)))),
)
const _Filters = Schema.mutable(Schema.Array(_Filter))
const _Sorts = Schema.mutable(Schema.Array(Schema.mutable(Schema.Tuple(
  Schema.String,
  Schema.Literal("none", "desc", "asc", "col desc", "col asc", "desc abs", "asc abs", "col desc abs", "col asc abs"),
))))

const _Json: Schema.Schema<Chart.Json> = Schema.suspend(() =>
  Schema.Union(
    Schema.Null,
    Schema.Boolean,
    Schema.Number,
    Schema.String,
    Schema.mutable(Schema.Array(_Json)),
    Schema.mutable(Schema.Record({ key: Schema.String, value: _Json })),
  )
)

const _Window = Schema.Struct({
  column: Schema.String,
  aggregate: Schema.String,
  partition_by: Schema.optional(_Strings),
  order_by: Schema.optional(Schema.NullOr(Schema.mutable(Schema.Tuple(Schema.String, Schema.Literal("asc", "desc"))))),
  rows: Schema.optional(Schema.NullOr(Schema.Number)),
  range: Schema.optional(Schema.NullOr(Schema.Number)),
  cumulative: Schema.optional(Schema.NullOr(Schema.Boolean)),
  offset: Schema.optional(Schema.NullOr(Schema.Number)),
  alpha: Schema.optional(Schema.NullOr(Schema.Number)),
})

const _Panel = Schema.Struct({
  table: Schema.String,
  version: Schema.optional(Schema.String),
  plugin: Schema.optional(Schema.String),
  title: Schema.optional(Schema.String),
  theme: Schema.optional(Schema.String),
  plugin_config: Schema.optional(Schema.mutable(Schema.Record({ key: Schema.String, value: _Json }))),
  columns_config: Schema.optional(Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.mutable(Schema.Record({ key: Schema.String, value: _Json })) })),),
  group_by: Schema.optional(_Strings),
  split_by: Schema.optional(_Strings),
  columns: Schema.optional(Schema.mutable(Schema.Array(Schema.NullOr(Schema.String)))),
  filter: Schema.optional(_Filters),
  filter_op: Schema.optional(Schema.Literal("and", "or")),
  sort: Schema.optional(_Sorts),
  expressions: Schema.optional(Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.String }))),
  windows: Schema.optional(Schema.mutable(Schema.Record({ key: Schema.String, value: _Window }))),
  aggregates: Schema.optional(Schema.mutable(Schema.Record({
    key: Schema.String,
    value: Schema.Union(Schema.String, Schema.mutable(Schema.Tuple(Schema.String, _Strings))),
  }))),
  group_by_depth: Schema.optional(Schema.Number),
  group_rollup_mode: Schema.optional(Schema.Literal("rollup", "flat", "total")),
  split_rollup_mode: Schema.optional(Schema.Literal("flat", "rollup")),
})

const _Layout: Schema.Schema<Chart.Layout> = Schema.suspend(() =>
  Schema.Union(
    Schema.Struct({
      type: Schema.Literal("split-layout"),
      children: Schema.mutable(Schema.Array(_Layout)),
      sizes: Schema.mutable(Schema.Array(Schema.Number)),
      orientation: Schema.Literal("horizontal", "vertical"),
    }),
    Schema.Struct({
      type: Schema.Literal("tab-layout"),
      tabs: _Strings,
      selected: Schema.optional(Schema.Number),
    }),
  )
)

const _Config = Schema.Struct({
  version: Schema.optional(Schema.String),
  active: Schema.optional(Schema.String),
  layout: Schema.optional(_Layout),
  panels: Schema.mutable(Schema.Record({ key: Schema.String, value: _Panel })),
  global_filters: Schema.optional(_Filters),
  masters: Schema.optional(_Strings),
})

const _CONFIG = Store.key({ domain: "chart", grain: "workspace" })
const _sealed = Store.sealed(_Config, { generation: 1, residue: "discard" })

const _config = (
  runtime: Atom.AtomRuntime<KeyValueStore.KeyValueStore, never>,
  seed: Chart.Config,
): Atom.Writable<Chart.Config> => Atom.kvs({ runtime, key: _CONFIG, schema: _sealed, defaultValue: () => seed })

// --- [ELEMENT_SEAM]

const _workspace = (
  element: HTMLPerspectiveViewerElement,
  feed: string,
  config: Chart.Config,
): Effect.Effect<void, ChartFault> =>
  Effect.tryPromise({
    try: (): Promise<void> => element.restoreWorkspace(config satisfies WorkspaceConfigUpdate),
    catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
  })

const _panel = (
  element: HTMLPerspectiveViewerElement,
  feed: string,
  panel: string,
  update: ViewerConfigUpdate,
): Effect.Effect<void, ChartFault> =>
  Effect.tryPromise({
    try: (): Promise<void> => element.restore(update, { panel, suppress_errors: true }),
    catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
  })

const _saved = (element: HTMLPerspectiveViewerElement, feed: string): Effect.Effect<Chart.Config, ChartFault> =>
  Effect.flatMap(
    Effect.tryPromise({
      try: () => element.saveWorkspace(),
      catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
    }),
    (token) =>
      Effect.mapError(
        Schema.decodeUnknown(_Config)({ ...token, layout: token.layout ?? undefined }),
        (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
      ),
  )

const _board = (element: HTMLPerspectiveViewerElement): Chart.Board => ({
  panels: element.getPanelNames() as ReadonlyArray<string>,
  active: Option.fromNullable(element.getActivePanel() as string | null),
})

const _moved = (
  element: HTMLPerspectiveViewerElement,
  feed: string,
  move: Chart.Move,
): Effect.Effect<Chart.Board, ChartFault> =>
  Effect.map(
    Effect.tryPromise({
      try: () =>
        Match.valueTags(move, {
          Add: ({ config }) => element.addPanel(config),
          Drop: ({ panel }) => element.removePanel(panel),
          Focus: ({ panel }) => element.setActivePanel(panel),
        }),
      catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
    }),
    () => _board(element),
  )

const _echo = (
  element: HTMLPerspectiveViewerElement,
  commit: (fold: (config: Chart.Config) => Chart.Config) => void,
) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const listen = (event: Event): void => {
        const patch = (event as CustomEvent<PerspectiveConfigUpdateEventDetail>).detail.getConfig()
        const panel = element.getActivePanel() as string | null
        if (panel === null) {
          return
        }
        commit((config) =>
          Option.match(Record.get(config.panels, panel), {
            onNone: () => config,
            onSome: (held) => ({ ...config, panels: { ...config.panels, [panel]: { ...held, ...patch } } }),
          })
        )
      }
      element.addEventListener("perspective-config-update", listen)
      return listen
    }),
    (listen) => Effect.sync(() => element.removeEventListener("perspective-config-update", listen)),
  )

const _pivot = (
  element: HTMLPerspectiveViewerElement,
  origin: Chart.Origin,
  feed: string,
  config: Chart.Config,
) =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const client = yield* Effect.tryPromise({
        try: () => perspective.worker(),
        catch: (defect) => new ChartFault({ case: { reason: "engine-lost", feed, cause: String(defect) } }),
      })
      const table = yield* Effect.tryPromise({
        try: () => _opened(client, origin),
        catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
      })
      yield* Effect.tryPromise({
        try: () => element.load(table),
        catch: (defect) => new ChartFault({ case: { reason: "engine-lost", feed, cause: String(defect) } }),
      })
      yield* _workspace(element, feed, config)
      return {
        client,
        table,
        element,
        append: (delta: ArrayBuffer) =>
          Effect.asVoid(Effect.tryPromise({
            try: () => table.update(delta),
            catch: (defect) => new ChartFault({ case: { reason: "frame-refused", feed, cause: String(defect) } }),
          })),
      } satisfies Chart.Pivot
    }).pipe(
      Effect.withSpan("rasm.ui.chart.pivot"),
      Effect.annotateLogs({ pivot: feed }),
    ),
    (pivot) =>
      Effect.promise(async () => {
        await element.delete()
        await pivot.table.delete()
        pivot.client.terminate()
      }),
  )

const _Validated = Schema.Struct({
  errors: Schema.Record({ key: Schema.String, value: Schema.Struct({ error_message: Schema.String }) }),
})

const _expressions = (
  pivot: Chart.Pivot,
  feed: string,
  exprs: Record.ReadonlyRecord<string, string>,
): Effect.Effect<void, ChartFault | ChartCensus> =>
  Effect.tryPromise({
    try: () => pivot.table.validate_expressions({ ...exprs }),
    catch: (defect) =>
      new ChartFault({ case: { reason: "expression-refused", feed, alias: Option.none(), cause: String(defect) } }),
  }).pipe(
    Effect.flatMap(Schema.decodeUnknown(_Validated)),
    Effect.mapError((defect) =>
      new ChartFault({ case: { reason: "expression-refused", feed, alias: Option.none(), cause: String(defect) } })),
    Effect.flatMap((report) => {
      const refused = Record.toEntries(report.errors)
      return Array.isNonEmptyReadonlyArray(refused)
        ? Effect.fail(new ChartCensus({
          issues: Array.map(refused, ([alias, issue]): ChartFault.Case => ({
            reason: "expression-refused",
            feed,
            alias: Option.some(alias),
            cause: issue.error_message,
          })),
        }))
        : Effect.void
    }),
  )

const _framed = (spec: Chart.Window): number =>
  (spec.rows === undefined || spec.rows === null ? 0 : 1) +
  (spec.range === undefined || spec.range === null ? 0 : 1) +
  (spec.cumulative === true ? 1 : 0)

const _refusal = (taken: HashSet.HashSet<string>, alias: string, spec: Chart.Window): Option.Option<string> => {
  const alpha = spec.alpha === undefined || spec.alpha === null ? 1 : spec.alpha
  return HashSet.has(taken, alias)
    ? Option.some("collides with a table column, an expression alias, or a sibling window key")
    : _framed(spec) > 1
    ? Option.some("rows, range, and cumulative are mutually exclusive")
    : alpha <= 0 || alpha > 1
    ? Option.some("alpha lies outside (0, 1]")
    : Option.none()
}

const _windows = (
  pivot: Chart.Pivot,
  feed: string,
  exprs: Record.ReadonlyRecord<string, string>,
  rows: ReadonlyArray<readonly [string, Chart.Window]>,
): Effect.Effect<Chart.Windows, ChartFault | ChartCensus> =>
  Effect.flatMap(
    Effect.tryPromise({
      try: () => pivot.table.schema(),
      catch: (defect) =>
        new ChartFault({ case: { reason: "window-refused", feed, alias: Option.none(), cause: String(defect) } }),
    }),
    (schema) => {
      const gated = Array.reduce(
        rows,
        {
          taken: HashSet.union(HashSet.fromIterable(Record.keys(schema)), HashSet.fromIterable(Record.keys(exprs))),
          issues: Array.empty<ChartFault.Case>(),
        },
        (held, [alias, spec]) => ({
          taken: HashSet.add(held.taken, alias),
          issues: Option.match(_refusal(held.taken, alias, spec), {
            onNone: () => held.issues,
            onSome: (cause) =>
              Array.append<ChartFault.Case>(held.issues, {
                reason: "window-refused",
                feed,
                alias: Option.some(alias),
                cause,
              }),
          }),
        }),
      )
      return Array.isNonEmptyReadonlyArray(gated.issues)
        ? Effect.fail(new ChartCensus({ issues: gated.issues }))
        : Effect.succeed(Record.fromEntries(rows))
    },
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
        catch: (defect) => new ChartFault({ case: { reason: "view-lost", feed, cause: String(defect) } }),
      }),
      (view) => Effect.promise(() => view.delete()),
    ),
    (view) =>
      Effect.tryPromise({
        try: () => read(view),
        catch: (defect) => new ChartFault({ case: { reason: "view-lost", feed, cause: String(defect) } }),
      }),
  )

const _FRAMES = Convention.mount(Convention.metric.chartFrames)

const _derive = (pivot: Chart.Pivot, feed: string, config: ViewConfigUpdate): Stream.Stream<Uint8Array, ChartFault> =>
  Stream.asyncScoped<Uint8Array, ChartFault>((emit) =>
    Effect.acquireRelease(
      Effect.tryPromise({
        try: async (): Promise<View> => {
          const view = await pivot.table.view(config)
          void emit.single(new Uint8Array(await view.to_arrow()))
          await view.on_update(({ delta }) => {
            if (delta !== undefined) void emit.single(new Uint8Array(delta))
          }, { mode: "row" })
          return view
        },
        catch: (defect) => new ChartFault({ case: { reason: "view-lost", feed, cause: String(defect) } }),
      }),
      (view) => Effect.promise(() => view.delete()),
    ),
  ).pipe(Stream.tap(() => Effect.asVoid(Effect.withMetric(Effect.succeed(1), _FRAMES))))

const _borrow = (
  pivot: Chart.Pivot,
  feed: string,
  config: ViewConfigUpdate,
  window: TypedArrayWindow,
  consume: Chart.Lend,
): Effect.Effect<void, ChartFault, Scope.Scope> =>
  _snapshot(pivot, feed, config, (view) => view.with_typed_arrays(window, consume))

const Chart: Chart.Shape = {
  Fault: ChartFault,
  Census: ChartCensus,
  Origin: _Origin,
  Move: _Move,
  Config: _Config,
  CONFIG: _CONFIG,
  regime: _regimeRows,
  regimes: _regimes,
  useFrame: _useFrame,
  columns: _columns,
  plot: _plot,
  legend: _legend,
  pointed: _pointed,
  bespoke: _bespoke,
  fold: _fold,
  mark: _MARK,
  motion: _MOTION,
  series: _series,
  options: _options,
  write: _write,
  stream: _stream,
  boot: _boot,
  config: _config,
  pivot: _pivot,
  workspace: _workspace,
  panel: _panel,
  saved: _saved,
  moved: _moved,
  echo: _echo,
  expressions: _expressions,
  windows: _windows,
  derive: _derive,
  snapshot: _snapshot,
  borrow: _borrow,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chart, ChartCensus, ChartFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
