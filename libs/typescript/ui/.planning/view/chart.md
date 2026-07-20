# [UI_CHART]

The one analytic-visualization owner: three rendering regimes behind one `Chart` surface, discriminated by data shape and interaction class — DECLARED statistical charts through `@observablehq/plot`'s grammar with `@visx/*` as the bespoke accessible-SVG lane and `d3` as the headless math substrate, STREAMING time-series through `uplot`'s canvas engine at 100k+ points, and USER-DRIVEN pivot/aggregation through the `@perspective-dev` WASM engine with `<perspective-viewer>` as its face. One `apache-arrow` `Table` is the columnar bus every regime consumes zero-copy — the same frame `geo` decodes fans to a GeoArrow layer, a perspective table, and an aligned series with no JSON detour. Every chart is a scoped resource behind an effect bracket, every spec derives from atom state, every color resolves from the token authority, and one surface runs exactly one engine — a fixed-shape interactive grid stays `view/table`'s `Grid`, the live basemap stays `viewer/geo`'s. The module is `ui/src/view/chart.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                           | [PUBLIC] |
| :-----: | :----------------- | :------------------------------------------------------------------------------- | :------- |
|  [01]   | `REGIME_LAW`       | the three-regime discriminant, the Arrow columnar bus, the engine boundary table | `Chart`  |
|  [02]   | `DECLARED_SURFACE` | the Plot grammar bracket and the visx bespoke accessible-SVG lane                | `Chart`  |
|  [03]   | `SERIES_SURFACE`   | the uplot scoped instance — aligned columns, `setData` streaming, cursor cohorts | `Chart`  |
|  [04]   | `PIVOT_SURFACE`    | the perspective engine — client/table lifecycle, config fold-echo, derived feed  | `Chart`  |

## [02]-[REGIME_LAW]

[REGIME_LAW]:
- Owner: `Chart` — one owner whose members are the three regime brackets and the columnar bus fold; regime selection is a decision row, never a component fork: DECLARED (the chart states a statistical claim — distribution, regression, facet, small multiple) renders through `[3]`; STREAMING (a telemetry/sensor/simulation series where point count breaks SVG) renders through `[4]`'s canvas; PIVOT (the USER drives group/split/aggregate/filter over a live feed) renders through `[5]`'s engine.
- Law: Arrow is the inter-engine bus — `Chart.columns(source, x, series)` projects an `apache-arrow` `Table` OR one `RecordBatch` into uplot's aligned columns through the shared `getChild(...).toArray()` spelling (`RecordBatch.getChild` carries the `Table` projection batch-direct, so the continuous-body reader lane never materializes a per-frame `Table`), Plot marks take the `Table` directly with column-name channels, and perspective ingests the SAME frame's IPC bytes with `format: "arrow"`; a JSON re-materialization between Arrow-capable engines is the named defect. A named column absent from the source folds the whole projection to `Option.none` — the consumer renders no chart; a fabricated flat series standing in for a missing column is the named defect.
- Law: color obeys the token split — series strokes, categorical palettes, and axis inks resolve from `Theme.ramp`/`Theme` rows (canvas engines take resolved values rebuilt on theme flip; SVG takes classes through `cn`); `d3-scale-chromatic` colormaps appear ONLY where the color IS the datum's value (`scaleSequential(interpolateViridis)` density/heat), and a `scheme*` categorical array standing in for the token palette is the split-brain defect.
- Law: `d3` is substrate, never surface — `rollup`/`bin`/`extent` folds prepare data beside a spec, scale/curve/format vocabularies pass through, and the DOM-coupled modules (`d3-selection`/`d3-zoom`/`d3-axis`) never appear; React owns chart DOM, `system/act` owns gesture.
- Law: measurement flows one way — a panel measures ONCE through `useParentSize` (`debounceTime` as policy) and fans `{ width, height }` to every resident chart: Plot receives them in its options, uplot through `setSize`, visx through scale ranges; a chart measuring itself mid-render is the named defect.
- Boundary: `Grid` (`view/table`) owns fixed-shape interactive collections at DOM scale; `viewer/geo` owns the live basemap (Plot's `geo` mark serves statistical maps only); `viewer/probe` and `viewer/panel` render their metric and telemetry boards THROUGH this owner.
- Growth: a new chart need selects a regime row; a new regime is a fourth bracket member on the one owner — never a sibling chart component family.

```typescript
import type { RecordBatch, Table } from "apache-arrow"
import { Array, Option } from "effect"
import type uPlot from "uplot"

declare namespace Chart {
  type Aligned = uPlot.AlignedData
  type Source = RecordBatch | Table // both carry getChild(name) — the batch lane projects with zero Table construction
}

const _columns = (source: Chart.Source, x: string, series: ReadonlyArray<string>): Option.Option<Chart.Aligned> =>
  Option.map(
    Option.all(Array.map([x, ...series], (name) => Option.fromNullable(source.getChild(name)))),
    (children) => {
      // BOUNDARY ADAPTER
      return Array.map(children, (child) => child.toArray() as Float64Array)
    },
  )
```

## [03]-[DECLARED_SURFACE]

[DECLARED_SURFACE]:
- Owner: `Chart.plot(container, build)` — the grammar bracket: `build` derives a `Plot.plot(options)` element from decoded inputs (marks over channels, transforms as option rewriters — `binX`/`group`/`stackY`/`windowY` in the options value, never a pre-shaped copy of the data), the bracket mounts it through `replaceChildren` and removes it on release; rebuild-per-change is the model — the grammar rebuilds cheaply, which is exactly why streaming series live on `[4]` instead.
- Packages: `@observablehq/plot` (`plot`, the mark roster — `dot`/`lineY`/`areaY`/`barY`/`rectY`/`cell`/`boxY`/`linearRegressionY`/`density`/`raster`/`contour`/`tree`/`geo` — the transform roster, `tip`/`pointer`/`crosshair` interaction, `facet`, named projections); `@visx/scale` + `@visx/shape` + `@visx/axis` + `@visx/group` + `@visx/responsive` (the bespoke lane); `d3` (the fold substrate).
- Law: interaction writes back through the store — `tip: true` renders channel values, and a `pointer` mark's `input` event carries the root element's `value` to an atom write; chart-as-input state never lands in component state.
- Law: the visx lane is earned by per-element addressability — RAC-adjacent handlers, per-datum a11y, custom hit logic on React-owned SVG elements: one chart = `useParentSize` dimensions → config-object scales (`scaleLinear({ domain, range, nice })`, `updateScale` on data change) → shapes and axes reading the SAME scale instances inside one margin-translated `Group`; a chart needing none of that is a Plot spec, and a hand-built `d` string where a shape component exists is the named defect.
- Law: Arrow plots directly — `Plot.dot(table, { x: "<column-a>", y: "<column-b>" })` consumes the bus `Table` with column-name shorthand and Arrow date detection; rows never materialize for a declared chart.
- Growth: a new declared chart is a spec value — marks, transforms, facets as data; a new bespoke interaction is one visx composition — never a d3-rendered surface beside either.

```typescript
import * as Plot from "@observablehq/plot"
import type { Table } from "apache-arrow"
import { Effect } from "effect"

const _plot = (container: HTMLElement, build: () => ReturnType<typeof Plot.plot>) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const figure = build()
      container.replaceChildren(figure)
      return figure
    }),
    (figure) => Effect.sync(() => figure.remove()),
  )

const _distribution = (table: Table, field: string, width: number): ReturnType<typeof Plot.plot> =>
  Plot.plot({
    width,
    grid: true,
    marks: [
      Plot.rectY(table, Plot.binX({ y: "count" }, { x: field, tip: true })),
      Plot.ruleY([0]),
    ],
  })
```

## [04]-[SERIES_SURFACE]

[SERIES_SURFACE]:
- Owner: `Chart.series(container, options, seed)` — the canvas bracket: `new uPlot(options, seed, container)` acquires, `destroy()` releases, and the ONLY per-tick write is `Chart.feed(chart, next)` — `setData` inside an atom subscription with the fold owning cadence (high-frequency feeds coalesce to animation frames before the call); React never reconciles a point, and rebuilding the instance per data tick is the named defect.
- Packages: `uplot` (the `uPlot` class, `AlignedData`, the options tree — `series`/`scales`/`axes`/`cursor`/`legend`/`bands` — `uPlot.sync`, `uPlot.paths.{linear,spline,stepped,bars,points}`, the hook-array plugin bus, `setSize`/`batch`); `system/token` (resolved stroke values — canvas reads no custom property); `apache-arrow` (`tableFromIPC` per-frame decode, `RecordBatchReader.from` for a single continuous IPC body).
- Law: the data contract is aligned columns — one x column, N y columns, typed arrays first-class, `null` the one gap marker with `spanGaps` per series; `Chart.columns` feeds it from the Arrow bus, and `uPlot.join` outer-joins tables that disagree on x.
- Law: an unbounded series streams frame by frame — `Chart.stream(x, series)` decodes each incoming Arrow IPC frame through `tableFromIPC`, projects it through `Chart.columns`, and appends the columns to a bounded aligned ring (the `_STREAM.points` cap), handing the next window to `Chart.feed`; the transport chunks the series into frames, so a single continuous body decodes incrementally through `RecordBatchReader.from` instead — each yielded `RecordBatch` projects through the SAME `Chart.columns` batch-direct — and rebuilding a whole `Table` over an unbounded series per frame is the named defect.
- Law: dashboard cohorts sync by key — `uPlot.sync(key)` + `cursor.sync: { key }` link crosshair, focus, and zoom across a panel cohort; the key is a chart-group value from the owning fold, never a literal per chart.
- Law: extension is a hook row — annotations, threshold shading, and tooltips ride the closed hook roster (`draw`/`drawSeries`/`setCursor`/…) as plugin hook arrays drawing into `u.ctx` or mounting into `u.over`; a fork of the draw loop is the named defect.
- Law: the stylesheet imports once — `uPlot.min.css` rides the token stylesheet; theme flips rebuild the options value from `Theme.ramp`-resolved strokes, and the canvas's missing per-point ARIA is compensated by an accessible summary row beside the chart (the consumer's obligation this card names).
- Growth: a new series is one options row; a new geometry is a `paths` builder swap — never a second time-series engine.

```typescript
import { tableFromIPC } from "apache-arrow"
import { Array, Effect, Option } from "effect"
import uPlot from "uplot"

const _series = (container: HTMLElement, options: uPlot.Options, seed: Chart.Aligned) =>
  Effect.acquireRelease(
    Effect.sync(() => new uPlot(options, seed, container)),
    (chart) => Effect.sync(() => chart.destroy()),
  )

const _feed = (chart: uPlot, next: Chart.Aligned): void => chart.setData(next)

const _COHORT = { key: "panel-telemetry" } as const

const _telemetry = (
  width: number,
  height: number,
  series: ReadonlyArray<{ readonly label: string; readonly stroke: string }>,
): uPlot.Options => ({
  width,
  height,
  ms: 1,
  cursor: { sync: { key: _COHORT.key } },
  series: [
    {},
    ...Array.map(series, (row) => ({ label: row.label, stroke: row.stroke, width: 1, spanGaps: true })),
  ],
  axes: [{ side: 2 }, { side: 3 }],
})

const _STREAM = { points: 100_000 } as const

const _tail = (held: Chart.Aligned, next: Chart.Aligned, points: number): Chart.Aligned =>
  Array.map(held, (col, rank) => {
    // BOUNDARY ADAPTER: concat the prior window with the frame's column, tail to the point cap
    const merged = Float64Array.from([...(col as Iterable<number>), ...(next[rank] as Iterable<number>)])
    return merged.length > points ? merged.subarray(merged.length - points) : merged
  }) as Chart.Aligned

const _stream = (x: string, series: ReadonlyArray<string>) =>
  (held: Chart.Aligned, frame: Uint8Array): Chart.Aligned =>
    Option.match(_columns(tableFromIPC(frame), x, series), {
      onNone: () => held,
      onSome: (cols) => _tail(held, cols, _STREAM.points),
    })
```

## [05]-[PIVOT_SURFACE]

[PIVOT_SURFACE]:
- Owner: `Chart.pivot(element, frame, options)` — the engine bracket: `perspective.worker()` spawns the WASM engine off the UI thread (or `websocket(url)` + `open_table(name)` attaches to a host-published feed — where the data lives is wiring, not an API fork), `client.table(frame, { format: "arrow", index })` ingests the bus frame (`index` makes updates upserts, `limit` ring-buffers a stream, `page_to_disk` spills a feed past the memory ceiling — the table modes every feed chooses between), the `<perspective-viewer>` element (`HTMLPerspectiveViewerElement`, the package's own exported type) `load`s the table, and release runs `element.delete()`, `table.delete()`, then `client.terminate()` — every handle INCLUDING the worker engine is a scoped resource, and a bracket that frees the table while the worker thread lives on is the named leak.
- Packages: `@perspective-dev/client` (`worker`, `websocket`, `Client.table`/`open_table`/`join`, `Table.update`/`view`, `View.to_arrow`/`on_update`); `@perspective-dev/viewer` + `@perspective-dev/viewer-datagrid` + `@perspective-dev/viewer-charts` (registration-by-import — the import IS the API; the plugin pair is closed, `viewer-d3fc`/`viewer-openlayers` rejected); `system/token` (the `./themes/*.css` roster imports once through the token stylesheet).
- Law: the `ViewerConfig` is the ONE state value — `save()` emits it, `restore(update)` applies any subset, the config atom rides `Atom.kvs` with its schema, a `perspective-config-update` listener writes user-driven changes back through the atom, and atom-driven changes apply via `restore` — the same fold-echo law `Grid` follows for TanStack state; an attribute poke or DOM scrape beside the config value is the named defect.
- Law: deltas stream, never poll — engine updates land through `table.update(arrowBuffer)` and repaint every dependent view incrementally; `View.on_update({ mode: "row" })` deltas ARE Arrow buffers feeding derived consumers, and a hand-maintained aggregate copy beside a live `View`/`join` is the named defect.
- Law: a derived feed is a scoped view lane — `Chart.derive(pivot, config)` opens `table.view(config)`, emits the `to_arrow` seed frame then every row-mode delta, and release runs `view.delete()`; each emitted frame is exactly `Chart.stream`'s input, so pivot-derived series feed the streaming regime with no re-materialization.
- Law: expression columns validate before shipping — `table.validate_expressions(exprs)` gates an ExprTK column; the aggregate vocabulary (`sum`/`distinct count`/`weighted mean`/`min by`/…) is the engine's roster referenced as data in the config value.
- Law: React reaches the element by ref only — mount runs the bracket in the effect seam, props never flow inside, config does; the element is the boundary.
- Law: the bracket is woven — acquisition carries `Effect.withSpan("rasm.ui.chart.pivot")` with the feed name as a log annotation, and every derived frame ticks `_FRAMES`, so engine spin-up latency and delta throughput reach the app bridge (`system/atom#STORE_ROOT`'s seam) with zero collector import; feed names stay log material, never metric tags.
- Growth: a new exploration surface is one bracket call with its own config atom; a headless consumer (export, alert, derived feed) rides `Chart.derive`'s view lane — never a second engine.

```typescript
import perspective from "@perspective-dev/client"
import "@perspective-dev/viewer"
import "@perspective-dev/viewer-datagrid"
import "@perspective-dev/viewer-charts"
import type { ViewConfigUpdate } from "@perspective-dev/client"
import type { HTMLPerspectiveViewerElement } from "@perspective-dev/viewer"
import { Effect, Metric, Stream } from "effect"

declare namespace Chart {
  type PivotOptions = { readonly name?: string; readonly index?: string; readonly limit?: number; readonly pageToDisk?: boolean }
  type Pivot = {
    readonly client: Awaited<ReturnType<typeof perspective.worker>>
    readonly table: Awaited<ReturnType<Awaited<ReturnType<typeof perspective.worker>>["table"]>>
    readonly append: (delta: ArrayBuffer) => Effect.Effect<void>
  }
  type Shape = {
    readonly columns: typeof _columns
    readonly plot: typeof _plot
    readonly series: typeof _series
    readonly feed: typeof _feed
    readonly stream: typeof _stream
    readonly pivot: typeof _pivot
    readonly derive: typeof _derive
  }
}

const _pivot = (element: HTMLPerspectiveViewerElement, frame: ArrayBuffer, options: Chart.PivotOptions) =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const client = yield* Effect.promise(() => perspective.worker())
      const table = yield* Effect.promise(() =>
        client.table(frame, {
          format: "arrow",
          ...(options.name !== undefined && { name: options.name }),
          ...(options.index !== undefined && { index: options.index }),
          ...(options.limit !== undefined && { limit: options.limit }),
          ...(options.pageToDisk !== undefined && { page_to_disk: options.pageToDisk }), // OPFS spill for feeds past the memory ceiling
        }))
      yield* Effect.promise(() => element.load(table))
      return {
        client,
        table,
        append: (delta: ArrayBuffer) => Effect.promise(() => table.update(delta)),
      } satisfies Chart.Pivot
    }).pipe(
      Effect.withSpan("rasm.ui.chart.pivot"),
      Effect.annotateLogs({ pivot: options.name ?? "<inline>" }),
    ),
    (pivot) =>
      Effect.promise(async () => {
        await element.delete()
        await pivot.table.delete()
        pivot.client.terminate()
      }),
  )

const _FRAMES = Metric.counter("rasm.ui.chart.frames", { description: "pivot delta frames delivered", incremental: true })

const _derive = (pivot: Chart.Pivot, config: ViewConfigUpdate): Stream.Stream<Uint8Array> =>
  Stream.asyncScoped<Uint8Array>((emit) =>
    Effect.acquireRelease(
      Effect.promise(async () => {
        // BOUNDARY ADAPTER: on_update is the engine's push seam — the seed frame emits before the delta subscription arms
        const view = await pivot.table.view(config)
        void emit.single(new Uint8Array(await view.to_arrow()))
        await view.on_update(({ delta }) => {
          if (delta !== undefined) void emit.single(new Uint8Array(delta))
        }, { mode: "row" })
        return view
      }),
      (view) => Effect.promise(() => view.delete()),
    ),
  ).pipe(Stream.tap(() => Metric.increment(_FRAMES)))

const Chart: Chart.Shape = {
  columns: _columns,
  plot: _plot,
  series: _series,
  feed: _feed,
  stream: _stream,
  pivot: _pivot,
  derive: _derive,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Chart }
```
