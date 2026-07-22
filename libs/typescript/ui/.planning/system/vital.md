# [UI_VITAL]

Vital owns browser performance evidence. Core Web Vitals, LoAF/event/long-task entries, React commits, and compiler diagnostics project to one `label`/`value`/`unit` row. Runtime callbacks fold through bounded windows and publish on `rasm.ui.vital.row`; app taps own OTLP egress, while probe and chart surfaces render the same rows. Module: `ui/src/system/vital.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                   | [PUBLIC] |
| :-----: | :--------------- | :----------------------------------------------------------------------- | :------- |
|  [01]   | `VITAL_CAPTURE`  | the five web-vitals capture rows and the latest-by-name row fold         | `Vital`  |
|  [02]   | `FRAME_OBSERVER` | scoped `PerformanceObserver` rows — LoAF, event timing, long tasks       | `Vital`  |
|  [03]   | `COMMIT_FOLD`    | the React `Profiler` commit window and its seed projections              | `Vital`  |
|  [04]   | `COMPILE_LANE`   | the build-lane counterpart — react-compiler diagnostics as evidence rows | `Vital`  |

## [02]-[VITAL_CAPTURE]

[VITAL_CAPTURE]:
- Owner: `Vital.capture(registry, report, mode)` — the one scoped registration: all five capture functions (`onLCP`, `onCLS`, `onINP`, `onFCP`, `onTTFB`) register once at composition, each callback folds its `MetricType` into one row (`vital-lcp`/`vital-cls`/`vital-inp`/`vital-fcp`/`vital-ttfb`, value in the metric's own unit), publishes that row through the replay point, and hands it to the local report sink; publication rides the one `FiberSet.makeRuntime` publisher every lane acquires, so the composing scope owns each forked publish — close interrupts in-flight publications and a post-close callback publish interrupts on arrival instead of reaching the registry. `terminal` is the default report mode; `stream` is the explicit non-production capture row. `Vital.latest` keys by `name`, carries the metric instance `id`, and sums `delta` per report into a session total; a bfcache restore mints a new `id`, replaces the name-keyed latest report, and continues the whole-session delta fold. `Vital.board` projects each latest row beside its `-session` twin.
- Packages: `web-vitals` (`onLCP`/`onCLS`/`onINP`/`onFCP`/`onTTFB`, `MetricType`, `ReportOpts`/`INPReportOpts`, `MetricRatingThresholds` with the five `*Thresholds` cutoff pairs); `@rasm/ts/core` (`Claim` — the row shape IS its metric vocabulary); `effect` (`Array`, `Effect`, `FiberSet`, `HashMap`, `Option`, `Scope`).
- Law: rows are probe rows — `label`/`value`/`unit` derives from `Claim` itself, CLS carries the unitless `"1"` and every timing vital carries `"ms"`, so vital evidence joins the claim board and the chart cohort as ordinary rows with zero shape adaptation.
- Law: rating maps to tone, never to a row field — a live report's shipped `rating` pre-buckets against its `*Thresholds` pair, a session row re-buckets through `Vital.rating` over the `_CUTOFF` table (the five shipped pairs, one anchor), and both key the `[05]` tone table at presentation; a threshold re-derived beside the shipped cutoffs is the named defect.
- Law: report cadence is one policy row — `reportAllChanges` selects streaming versus terminal reporting and `durationThreshold` floors the INP entry stream; the closed `_CAPTURE` registration table applies that row to every vital, and a per-vital bespoke opt bag or registration branch is the named defect.
- Law: capture is idempotent and composition-owned — the functions self-dedupe per page, registration runs once where the app composes the plane, and a component registering a vital is the named defect.
- Boundary: OTLP egress is the app tap's through the hook rail; the attribution build (`web-vitals/attribution`) is an app-plane diagnostic choice — the row shape is unchanged, so admitting it swaps the import and widens no surface.

```typescript
import type { Claim } from "@rasm/ts/core"
import { Array, Effect, FiberSet, HashMap, Option, type Scope } from "effect"
import {
  CLSThresholds, FCPThresholds, INPThresholds, LCPThresholds, TTFBThresholds,
  type INPReportOpts, type MetricRatingThresholds, type MetricType, onCLS, onFCP, onINP, onLCP, onTTFB,
} from "web-vitals"
import { Hook } from "./hook.ts"

type Row = Claim["metrics"][number]

declare module "./hook.ts" {
  interface Points {
    readonly "rasm.ui.vital.row": { readonly modality: "replay"; readonly payload: Row }
  }
}

const _REPORT = {
  terminal: { reportAllChanges: false, durationThreshold: 40 },
  stream: { reportAllChanges: true, durationThreshold: 40 },
} as const

type _ReportMode = keyof typeof _REPORT

type _Register = (fold: (metric: MetricType) => void, options: INPReportOpts) => void

const _CAPTURE = [
  (fold, options) => onLCP(fold, options),
  (fold, options) => onCLS(fold, options),
  (fold, options) => onINP(fold, options),
  (fold, options) => onFCP(fold, options),
  (fold, options) => onTTFB(fold, options),
] as const satisfies ReadonlyArray<_Register>

const _UNIT = { CLS: "1", FCP: "ms", INP: "ms", LCP: "ms", TTFB: "ms" } as const

const _CUTOFF: { readonly [K in MetricType["name"]]: MetricRatingThresholds } = {
  CLS: CLSThresholds,
  FCP: FCPThresholds,
  INP: INPThresholds,
  LCP: LCPThresholds,
  TTFB: TTFBThresholds,
} // the shipped cutoff pairs are the one rating truth: session rows re-bucket through them, never a re-derived threshold

const _rating = (name: MetricType["name"], value: number): "good" | "needs-improvement" | "poor" =>
  value <= _CUTOFF[name][0] ? "good" : value <= _CUTOFF[name][1] ? "needs-improvement" : "poor"

const _row = (metric: MetricType): Row => ({
  label: `vital-${metric.name.toLowerCase()}`,
  value: metric.value,
  unit: _UNIT[metric.name],
})

const _vitalHook: Hook.Row<"rasm.ui.vital.row"> = { modality: "replay", depth: 128, source: Option.none() }

type _Publish = (row: Row) => void

// scope-owned publisher: FiberSet.makeRuntime binds every callback-forked publish to the composing
// lifecycle — scope close interrupts in-flight publishes and a post-close call interrupts on arrival,
// so no callback publication outlives or retains its registry composition
const _publisher = (registry: Hook.Registry): Effect.Effect<_Publish, never, Scope.Scope> =>
  Effect.map(FiberSet.makeRuntime<never>(), (fork) => (row) => {
    // BOUNDARY ADAPTER: browser and Profiler callbacks re-enter the Effect rail at the one point publisher
    void fork(Effect.asVoid(Hook.publish(registry, "rasm.ui.vital.row", row)))
  })

const _deliver = (publish: _Publish, report: (row: Row) => void, row: Row): void => {
  publish(row)
  report(row)
}

const _capture = (
  registry: Hook.Registry,
  report: (row: Row) => void,
  mode: _ReportMode = "terminal",
): Effect.Effect<void, never, Scope.Scope> =>
  Effect.map(_publisher(registry), (publish) => {
    const fold = (metric: MetricType): void => _deliver(publish, report, _row(metric))
    const options = _REPORT[mode]
    Array.forEach(_CAPTURE, (register) => register(fold, options))
  })

type _Held = { readonly row: Row; readonly id: string; readonly session: number }

const _latest = (held: HashMap.HashMap<MetricType["name"], _Held>, metric: MetricType): HashMap.HashMap<MetricType["name"], _Held> =>
  HashMap.set(held, metric.name, {
    row: _row(metric),
    id: metric.id, // metric-instance identity changes on bfcache restore; streaming reports retain it while delta advances
    session: Option.match(HashMap.get(held, metric.name), {
      onNone: () => metric.delta,
      onSome: (prior) => prior.session + metric.delta, // delta accumulates across reports and restore-minted ids into the whole-session total
    }),
  })

const _board = (held: HashMap.HashMap<MetricType["name"], _Held>): ReadonlyArray<Row> =>
  Array.flatMap(Array.fromIterable(HashMap.values(held)), (kept) => [
    kept.row,
    { label: `${kept.row.label}-session`, value: kept.session, unit: kept.row.unit },
  ])
```

## [03]-[FRAME_OBSERVER]

[FRAME_OBSERVER]:
- Owner: `Vital.observe(registry, type, report)` — the scoped observer row: registration first proves the type against `PerformanceObserver.supportedEntryTypes` and an unsupported kind (`long-animation-frame` ships Chromium-first) answers `Option.none`, never a dead observer; on a supported kind `new PerformanceObserver` acquires, `observe({ type, buffered: true })` replays already-buffered entries into the first fold, and `disconnect()` releases with the composition scope; the observer closure appends each delivery through `Vital.window.samples`, projects `Vital.entryRows`, and publishes every row through the replay point before calling the local report sink. Three entry kinds ride the one bracket AND one polymorphic seed fold over the `_ENTRY` measure table — `long-animation-frame` measures `PerformanceLongAnimationFrameTiming.blockingDuration` (the LoAF jank fact), `event` measures `PerformanceEventTiming.duration` over the `durationThreshold` floor (interaction latency beyond the INP headline), `longtask` measures main-thread occupancy — each kind projecting its `-count`/`-mean`/`-peak` rows from the same fold, so a new entry kind is one measure-table row on the same bracket, never a sibling fold.
- Packages: `web-vitals` (the types build augments the DOM lib with `PerformanceLongAnimationFrameTiming`, `PerformanceEventTiming`, and `PerformanceScriptTiming`, so raw entries type without a second `@types` package); `effect` (`Chunk`, `Effect`, `Number`, `Option`, `pipe`, `Scope`).
- Law: entry streams fold through the probe window law — samples append into a bounded `Chunk` window (`takeRight` at the cap) and projections run as ONE seed fold: raw sums accumulate in a single `Chunk.reduce` pass, means project at read, and a new statistic is one seed field and one row, never a second traversal.
- Law: script attribution stays entry-local — a LoAF entry's `scripts` rows (`invokerType`, `sourceURL`, `forcedStyleAndLayoutDuration`) render as drill-in evidence beside the row, never as per-script metric rows, because per-script labels are unbounded and rows are a bounded vocabulary.
- Law: observers are passive — no forced layout, no synthetic events, no `takeRecords` polling loop; an idle document reports idle numbers truthfully.

```typescript
import { Chunk, Effect, Number, Option, pipe, type Scope } from "effect"

const _WINDOW = { samples: 120 } as const

type _Entry = {
  readonly "long-animation-frame": PerformanceLongAnimationFrameTiming
  readonly event: PerformanceEventTiming
  readonly longtask: PerformanceEntry
}

const _ENTRY: { readonly [K in keyof _Entry]: { readonly label: string; readonly measure: (entry: _Entry[K]) => number } } = {
  "long-animation-frame": { label: "loaf", measure: (entry) => entry.blockingDuration }, // the LoAF jank fact
  event: { label: "event", measure: (entry) => entry.duration }, // interaction latency beyond the INP headline
  longtask: { label: "longtask", measure: (entry) => entry.duration }, // main-thread occupancy
}

const _entryWindow = <K extends keyof _Entry>(trace: Chunk.Chunk<_Entry[K]>, entries: ReadonlyArray<_Entry[K]>): Chunk.Chunk<_Entry[K]> =>
  Chunk.takeRight(Chunk.fromIterable([...trace, ...entries]), _WINDOW.samples)

// long-animation-frame ships Chromium-first: registration proves the type against the platform roster
const _supported = (type: keyof _Entry): boolean => PerformanceObserver.supportedEntryTypes.includes(type)

const _observe = <K extends keyof _Entry>(
  registry: Hook.Registry,
  type: K,
  report: (row: Row) => void,
): Effect.Effect<Option.Option<PerformanceObserver>, never, Scope.Scope> =>
  _supported(type)
    ? Effect.flatMap(_publisher(registry), (publish) =>
        Effect.map(
          Effect.acquireRelease(
            Effect.sync(() => {
              let trace = Chunk.empty<_Entry[K]>()
              const observer = new PerformanceObserver((list) => {
                // BOUNDARY ADAPTER: the typed observe registration proves the delivered entry subtype
                trace = _entryWindow(trace, list.getEntries() as ReadonlyArray<_Entry[K]>)
                Array.forEach(_entryRows(type, trace), (row) => _deliver(publish, report, row))
              })
              observer.observe({ type, buffered: true, ...(type === "event" && { durationThreshold: _REPORT.terminal.durationThreshold }) })
              return observer
            }),
            (observer) => Effect.sync(() => observer.disconnect()),
          ),
          Option.some,
        ))
    : Effect.succeedNone // an absent entry type is the exposed state, never a dead observer

type _EntrySums = { readonly count: number; readonly total: number; readonly peak: number }

const _ENTRY_SEED: _EntrySums = { count: 0, total: 0, peak: 0 }

const _entryRows = <K extends keyof _Entry>(kind: K, trace: Chunk.Chunk<_Entry[K]>): ReadonlyArray<Row> =>
  pipe(
    Chunk.reduce(trace, _ENTRY_SEED, (acc, entry): _EntrySums =>
      pipe(_ENTRY[kind].measure(entry), (cost) => ({
        count: acc.count + 1,
        total: acc.total + cost,
        peak: Number.max(acc.peak, cost), // the worst entry windows as its peak, never a mean
      }))),
    (sums) =>
      sums.count === 0
        ? [] // an empty window carries no rows — a zero-sample mean is fabricated evidence
        : [
            { label: `${_ENTRY[kind].label}-count`, value: sums.count, unit: "1" },
            { label: `${_ENTRY[kind].label}-mean`, value: sums.total / sums.count, unit: "ms" },
            { label: `${_ENTRY[kind].label}-peak`, value: sums.peak, unit: "ms" },
          ])
```

## [04]-[COMMIT_FOLD]

[COMMIT_FOLD]:
- Owner: `Vital.committed(registry, report)` — the React tree lane, minted as a scoped acquisition over the same publisher law as capture: one `<Profiler id onRender>` per measured subtree feeds an id-keyed window owned by the callback closure, `onRender`'s full `(id, phase, actualDuration, baseDuration, startTime, commitTime)` tuple appends under `Vital.window.samples`, and the projections publish through the replay point before reaching the local report sink. `commit-actual` against `commit-base` reads whether the compiler's memoization is holding, `commit-count` per phase reads churn, the peak reads the worst commit in the window, and `commit-lag` (`commitTime - startTime`) reads the scheduling latency the tree paid beyond its own render cost.
- Packages: `react` (`Profiler`, the `ProfilerOnRenderCallback` contract); `effect` (`Chunk`, `Number`, `pipe`).
- Law: the profiled set is a bounded roster — measured subtrees are named policy rows (the view plane, the viewer canvas shell, an app-nominated surface), never a per-component wrap; `id` keys the row labels so two subtrees never blur into one series.
- Law: phase is a fold discriminant, not a row family — `mount`, `update`, and `nested-update` advance their own counters inside ONE seed, and a per-phase window triple is the named defect.
- Law: the render loop stays out — GPU and frame-loop evidence is `viewer/probe#METRIC_FOLD`'s lane; this fold measures the React tree alone, and one board renders both lanes side by side because the rows share one shape.

```typescript
import type { ProfilerOnRenderCallback } from "react"

type _Commit = {
  readonly id: string
  readonly phase: "mount" | "update" | "nested-update"
  readonly actual: number
  readonly base: number
  readonly start: number
  readonly commit: number
}

type _CommitSums = {
  readonly mounts: number
  readonly updates: number
  readonly nested: number
  readonly actual: number
  readonly base: number
  readonly peak: number
  readonly lag: number
  readonly lagPeak: number
}

const _COMMIT_SEED: _CommitSums = { mounts: 0, updates: 0, nested: 0, actual: 0, base: 0, peak: 0, lag: 0, lagPeak: 0 }

const _commitStepped = (acc: _CommitSums, commit: _Commit): _CommitSums => ({
  mounts: acc.mounts + (commit.phase === "mount" ? 1 : 0),
  updates: acc.updates + (commit.phase === "update" ? 1 : 0),
  nested: acc.nested + (commit.phase === "nested-update" ? 1 : 0),
  actual: acc.actual + commit.actual,
  base: acc.base + commit.base,
  peak: Number.max(acc.peak, commit.actual),
  lag: acc.lag + (commit.commit - commit.start), // start-to-commit latency: scheduling pressure beyond the render cost itself
  lagPeak: Number.max(acc.lagPeak, commit.commit - commit.start),
})

const _committed = (registry: Hook.Registry, report: (row: Row) => void): Effect.Effect<ProfilerOnRenderCallback, never, Scope.Scope> =>
  Effect.map(_publisher(registry), (publish) => {
    let held = HashMap.empty<string, Chunk.Chunk<_Commit>>()
    return (id, phase, actualDuration, baseDuration, startTime, commitTime) => {
      // BOUNDARY ADAPTER: Profiler supplies the complete commit tuple at this callback seam
      const commit = { id, phase, actual: actualDuration, base: baseDuration, start: startTime, commit: commitTime }
      const trace = Chunk.takeRight(
        Chunk.append(Option.getOrElse(HashMap.get(held, id), () => Chunk.empty<_Commit>()), commit),
        _WINDOW.samples,
      )
      held = HashMap.set(held, id, trace)
      Array.forEach(_commitRows(id, trace), (row) => _deliver(publish, report, row))
    }
  })

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
            { label: `commit-lag-${id}`, value: sums.lag / count, unit: "ms" },
            { label: `commit-lag-peak-${id}`, value: sums.lagPeak, unit: "ms" },
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
    readonly cutoff: typeof _CUTOFF
    readonly tone: typeof _tone
    readonly rating: typeof _rating
    readonly capture: typeof _capture
    readonly hook: typeof _vitalHook
    readonly latest: typeof _latest
    readonly board: typeof _board
    readonly observe: typeof _observe
    readonly entryRows: typeof _entryRows
    readonly committed: typeof _committed
    readonly commitRows: typeof _commitRows
    readonly compiled: typeof _compiled
  }
}

const Vital: Vital.Shape = {
  window: _WINDOW,
  report: _REPORT,
  cutoff: _CUTOFF,
  tone: _tone,
  rating: _rating,
  capture: _capture,
  hook: _vitalHook,
  latest: _latest,
  board: _board,
  observe: _observe,
  entryRows: _entryRows,
  committed: _committed,
  commitRows: _commitRows,
  compiled: _compiled,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Vital }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
