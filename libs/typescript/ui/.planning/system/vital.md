# [UI_VITAL]

The browser performance-evidence owner: Core Web Vitals, `PerformanceObserver` long-task/LoAF/event-timing entries, and React `Profiler` commit folds captured as the SAME `label`/`value`/`unit` metric rows probe boards and chart series already render, so one board answers whether cost sits in the render loop, the React tree, or the interaction path. The five `web-vitals` capture functions register once at composition, observer registrations are scoped resources whose teardown rides the composition scope, commit and frame streams fold through one bounded seed window per probe's window law, and the compile lane mirrors the same row shape at build time through the react-compiler diagnostic rail. The plane mints no instrument and imports no collector — rows publish through the `rasm.ui.vital.row` hook point (`system/hook`, replay modality) and the app-composed tap carries them to the OTel spine, while probe boards and `view/chart#SERIES_SURFACE` cohorts render the same rows locally. The module is `ui/src/system/vital.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                   | [PUBLIC] |
| :-----: | :--------------- | :----------------------------------------------------------------------- | :------- |
|  [01]   | `VITAL_CAPTURE`  | the five web-vitals capture rows and the latest-by-name row fold         | `Vital`  |
|  [02]   | `FRAME_OBSERVER` | scoped `PerformanceObserver` rows — LoAF, event timing, long tasks       | `Vital`  |
|  [03]   | `COMMIT_FOLD`    | the React `Profiler` commit window and its seed projections              | `Vital`  |
|  [04]   | `COMPILE_LANE`   | the build-lane counterpart — react-compiler diagnostics as evidence rows | `Vital`  |

## [02]-[VITAL_CAPTURE]

[VITAL_CAPTURE]:
- Owner: `Vital.capture(report)` — the one registration: all five capture functions (`onLCP`, `onCLS`, `onINP`, `onFCP`, `onTTFB`) register once at composition, each callback folds its `MetricType` into one row (`vital-lcp`/`vital-cls`/`vital-inp`/`vital-fcp`/`vital-ttfb`, value in the metric's own unit), and the fold keys by `name` with the metric's `id` as the dedupe fact — a bfcache restore mints a new `id`, so the keyed fold replaces the prior report instead of double-counting, and a session total sums `delta` per id where an app tap wants it.
- Packages: `web-vitals` (`onLCP`/`onCLS`/`onINP`/`onFCP`/`onTTFB`, `MetricType`, `ReportOpts`/`INPReportOpts`, the `*Thresholds` cutoff pairs); `@rasm/ts/core` (`Claim` — the row shape IS its metric vocabulary); `effect` (`Array`, `HashMap`).
- Law: rows are probe rows — `label`/`value`/`unit` derives from `Claim` itself, CLS carries the unitless `"1"` and every timing vital carries `"ms"`, so vital evidence joins the claim board and the chart cohort as ordinary rows with zero shape adaptation.
- Law: rating maps to tone, never to a row field — each metric's `rating` pre-buckets against its `*Thresholds` pair and keys the `[05]` tone table at presentation; a threshold re-derived beside the shipped cutoffs is the named defect.
- Law: report cadence is one policy row — `reportAllChanges` selects streaming versus terminal reporting and `durationThreshold` floors the INP entry stream; a per-vital bespoke opt bag is the named defect.
- Law: capture is idempotent and composition-owned — the functions self-dedupe per page, registration runs once where the app composes the plane, and a component registering a vital is the named defect.
- Boundary: OTLP egress is the app tap's through the hook rail; the attribution build (`web-vitals/attribution`) is an app-plane diagnostic choice — the row shape is unchanged, so admitting it swaps the import and widens no surface.

```typescript
import type { Claim } from "@rasm/ts/core"
import { Array, HashMap } from "effect"
import { type MetricType, onCLS, onFCP, onINP, onLCP, onTTFB } from "web-vitals"

type Row = Claim["metrics"][number]

const _REPORT = { reportAllChanges: true, durationThreshold: 40 } as const

const _UNIT = { CLS: "1", FCP: "ms", INP: "ms", LCP: "ms", TTFB: "ms" } as const

const _row = (metric: MetricType): Row => ({
  label: `vital-${metric.name.toLowerCase()}`,
  value: metric.value,
  unit: _UNIT[metric.name],
})

const _capture = (report: (row: Row, metric: MetricType) => void): void => {
  const fold = (metric: MetricType): void => report(_row(metric), metric)
  onLCP(fold, _REPORT)
  onCLS(fold, _REPORT)
  onINP(fold, _REPORT)
  onFCP(fold, _REPORT)
  onTTFB(fold, _REPORT)
}

const _latest = (held: HashMap.HashMap<string, Row>, metric: MetricType): HashMap.HashMap<string, Row> =>
  HashMap.set(held, metric.name, _row(metric)) // keyed replace: a bfcache restore's fresh id lands as the new report, never a double count

const _board = (held: HashMap.HashMap<string, Row>): ReadonlyArray<Row> => Array.fromIterable(HashMap.values(held))
```

## [03]-[FRAME_OBSERVER]

[FRAME_OBSERVER]:
- Owner: `Vital.observe(type, digest)` — the scoped observer row: `new PerformanceObserver` acquires, `observe({ type, buffered: true })` replays already-buffered entries into the first digest, and `disconnect()` releases with the composition scope; three entry rows ride the one bracket — `long-animation-frame` (`PerformanceLongAnimationFrameTiming.blockingDuration` — the LoAF jank fact), `event` (`PerformanceEventTiming.duration` over the `durationThreshold` floor — interaction latency beyond the INP headline), `longtask` (main-thread occupancy) — and a new entry kind is one row on the same bracket.
- Packages: `web-vitals` (the types build augments the DOM lib with `PerformanceLongAnimationFrameTiming`, `PerformanceEventTiming`, and `PerformanceScriptTiming`, so raw entries type without a second `@types` package); `effect` (`Chunk`, `Effect`, `Number`, `pipe`).
- Law: entry streams fold through the probe window law — samples append into a bounded `Chunk` window (`takeRight` at the cap) and projections run as ONE seed fold: raw sums accumulate in a single `Chunk.reduce` pass, means project at read, and a new statistic is one seed field and one row, never a second traversal.
- Law: script attribution stays entry-local — a LoAF entry's `scripts` rows (`invokerType`, `sourceURL`, `forcedStyleAndLayoutDuration`) render as drill-in evidence beside the row, never as per-script metric rows, because per-script labels are unbounded and rows are a bounded vocabulary.
- Law: observers are passive — no forced layout, no synthetic events, no `takeRecords` polling loop; an idle document reports idle numbers truthfully.

```typescript
import { Chunk, Effect, Number, pipe, type Scope } from "effect"

const _WINDOW = { samples: 120 } as const

const _observe = (
  type: "event" | "long-animation-frame" | "longtask",
  digest: (entries: ReadonlyArray<PerformanceEntry>) => void,
): Effect.Effect<PerformanceObserver, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const observer = new PerformanceObserver((list) => digest(list.getEntries()))
      observer.observe({ type, buffered: true, ...(type === "event" && { durationThreshold: _REPORT.durationThreshold }) })
      return observer
    }),
    (observer) => Effect.sync(() => observer.disconnect()),
  )

type _FrameSums = { readonly count: number; readonly blocking: number; readonly longest: number }

const _FRAME_SEED: _FrameSums = { count: 0, blocking: 0, longest: 0 }

const _frameStepped = (acc: _FrameSums, entry: PerformanceLongAnimationFrameTiming): _FrameSums => ({
  count: acc.count + 1,
  blocking: acc.blocking + entry.blockingDuration,
  longest: Number.max(acc.longest, entry.blockingDuration), // the worst frame windows as its peak, never a mean
})

const _frameRows = (trace: Chunk.Chunk<PerformanceLongAnimationFrameTiming>): ReadonlyArray<Row> =>
  pipe(Chunk.reduce(trace, _FRAME_SEED, _frameStepped), (sums) =>
    sums.count === 0
      ? [] // an empty window carries no rows — a zero-sample mean is fabricated evidence
      : [
          { label: "loaf-count", value: sums.count, unit: "1" },
          { label: "loaf-blocking", value: sums.blocking / sums.count, unit: "ms" },
          { label: "loaf-peak", value: sums.longest, unit: "ms" },
        ])
```

## [04]-[COMMIT_FOLD]

[COMMIT_FOLD]:
- Owner: `Vital.commit` — the React tree lane: one `<Profiler id onRender>` per measured subtree feeds the commit window, `onRender`'s `(id, phase, actualDuration, baseDuration, startTime, commitTime)` folds into the bounded seed, and the projections answer the memoization question directly — `commit-actual` against `commit-base` reads whether the compiler's memoization is holding, `commit-count` per phase reads churn, and the peak reads the worst commit in the window.
- Packages: `react` (`Profiler`, the `ProfilerOnRenderCallback` contract); `effect` (`Chunk`, `Number`, `pipe`).
- Law: the profiled set is a bounded roster — measured subtrees are named policy rows (the view plane, the viewer canvas shell, an app-nominated surface), never a per-component wrap; `id` keys the row labels so two subtrees never blur into one series.
- Law: phase is a fold discriminant, not a row family — `mount`, `update`, and `nested-update` advance their own counters inside ONE seed, and a per-phase window triple is the named defect.
- Law: the render loop stays out — GPU and frame-loop evidence is `viewer/probe#METRIC_FOLD`'s lane; this fold measures the React tree alone, and one board renders both lanes side by side because the rows share one shape.

```typescript
import type { ProfilerOnRenderCallback } from "react"

type _CommitSums = {
  readonly mounts: number
  readonly updates: number
  readonly nested: number
  readonly actual: number
  readonly base: number
  readonly peak: number
}

const _COMMIT_SEED: _CommitSums = { mounts: 0, updates: 0, nested: 0, actual: 0, base: 0, peak: 0 }

type _Commit = { readonly phase: "mount" | "update" | "nested-update"; readonly actual: number; readonly base: number }

const _commitStepped = (acc: _CommitSums, commit: _Commit): _CommitSums => ({
  mounts: acc.mounts + (commit.phase === "mount" ? 1 : 0),
  updates: acc.updates + (commit.phase === "update" ? 1 : 0),
  nested: acc.nested + (commit.phase === "nested-update" ? 1 : 0),
  actual: acc.actual + commit.actual,
  base: acc.base + commit.base,
  peak: Number.max(acc.peak, commit.actual),
})

const _committed = (sink: (commit: _Commit) => void): ProfilerOnRenderCallback =>
  (_id, phase, actualDuration, baseDuration) => sink({ phase, actual: actualDuration, base: baseDuration })

const _commitRows = (id: string, trace: Chunk.Chunk<_Commit>): ReadonlyArray<Row> =>
  pipe(Chunk.reduce(trace, _COMMIT_SEED, _commitStepped), (sums) =>
    pipe(sums.mounts + sums.updates + sums.nested, (count) =>
      count === 0
        ? []
        : [
            { label: `commit-count-${id}`, value: count, unit: "1" },
            { label: `commit-actual-${id}`, value: sums.actual / count, unit: "ms" },
            { label: `commit-base-${id}`, value: sums.base / count, unit: "ms" },
            { label: `commit-peak-${id}`, value: sums.peak, unit: "ms" },
          ]))
```

## [05]-[COMPILE_LANE]

[COMPILE_LANE]:
- Owner: `Vital.compiled(text, file)` — the build-lane counterpart row: `runBabelPluginReactCompiler` compiles a source under `noEmit`-grade options with a `logger` sink, the `LoggerEvent` census folds into the same row shape (`compile-success`, `compile-skip`, `compile-error` counts), and a CI gate renders the rows exactly like runtime evidence — one vocabulary spans field and build.
- Packages: `babel-plugin-react-compiler` (`runBabelPluginReactCompiler`, `LoggerEvent`, `PluginOptions`); `effect` (`Array`).
- Law: the lane is build-time only — the fence runs in tooling and CI, never in the browser bundle; the browser plane's compiler evidence is the dev-validator surface (`react-compiler-runtime`'s `renderCounterRegistry`), which an app tap reads and folds through the same rows when the emission flags are armed.
- Law: severity is the row split — a `CompileError` event counts by its category, a skip counts as deliberate opt-out, and a rising skip count is architecture pressure on the skipped components, never a threshold to tune away.
- Law: tone keys the shared table — matched-good renders success, `needs-improvement` renders caution, `poor` and error rows render danger; the table is one `as const` record beside the rows and every vital surface reads it.
- Boundary: bundler wiring, `panicThreshold`, and gating are the build plane's config bag; probe's claim board and the chart cohort render these rows; the hook rail carries them to any app sink.

```typescript
import { type LoggerEvent, type PluginOptions, runBabelPluginReactCompiler } from "babel-plugin-react-compiler"
import { Array } from "effect"

const _tone = {
  good: { tone: "success" },
  "needs-improvement": { tone: "caution" },
  poor: { tone: "danger" },
} as const

const _compiled = (text: string, file: string): ReadonlyArray<Row> => {
  // BOUNDARY ADAPTER: the logger sink is the plugin's callback contract — the census detaches immutable
  const events: Array<LoggerEvent> = []
  const options: PluginOptions = {
    target: "19",
    compilationMode: "infer",
    panicThreshold: "none",
    noEmit: true,
    logger: { logEvent: (_, event) => void events.push(event) },
  }
  runBabelPluginReactCompiler(text, file, "typescript", options)
  const census = (kind: LoggerEvent["kind"]): number => Array.filter(events, (event) => event.kind === kind).length
  return [
    { label: "compile-success", value: census("CompileSuccess"), unit: "1" },
    { label: "compile-skip", value: census("CompileSkip"), unit: "1" },
    { label: "compile-error", value: census("CompileError"), unit: "1" },
  ]
}

declare namespace Vital {
  type Shape = {
    readonly window: typeof _WINDOW
    readonly report: typeof _REPORT
    readonly tone: typeof _tone
    readonly capture: typeof _capture
    readonly latest: typeof _latest
    readonly board: typeof _board
    readonly observe: typeof _observe
    readonly frameRows: typeof _frameRows
    readonly committed: typeof _committed
    readonly commitRows: typeof _commitRows
    readonly compiled: typeof _compiled
  }
}

const Vital: Vital.Shape = {
  window: _WINDOW,
  report: _REPORT,
  tone: _tone,
  capture: _capture,
  latest: _latest,
  board: _board,
  observe: _observe,
  frameRows: _frameRows,
  committed: _committed,
  commitRows: _commitRows,
  compiled: _compiled,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Vital }
```
