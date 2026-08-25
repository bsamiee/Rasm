# [UI_VITAL]

Vital owns the browser evidence the interface plane can see from inside itself: long-animation-frame and event-timing entries, React commit windows, and react-compiler diagnostics, each projected to one `label`/`value`/`unit` row. Runtime callbacks fold through bounded windows and publish on `rasm.ui.vital.row`; probe and chart surfaces render the same rows, and an app tap carries them outward. Module: `ui/src/system/vital.ts`.

Core Web Vitals are not measured here. `runtime:otel/vital` owns every CWV capture, the shipped cutoff pairs, and both instruments, so this floor grades nothing, mints no instrument, and publishes raw windowed evidence onto the replay point where the app bridge republishes that owner's graded facts alongside. Reading a Performance-Timeline family those registrars also read costs one more consumer of one browser buffer; one accounting per vital kind is the invariant, and the `event-timing` floor is the one policy value both readers share.

## [01]-[INDEX]

- [02]-[EVIDENCE_RAIL]: `Points` contributes the replay row, `_publisher` owns every forked publish, `_tone` keys the grade table onto the token roster, `Vital.policy` admits the window and floor `Vital.Policy` carries, and `Vital.window`/`fold`/`rows` is the one bounded-window measure algebra every evidence surface reads.
- [03]-[FRAME_OBSERVER]: one observer brackets every `_ENTRY` row, folding each family's bounded window into probe rows.
- [04]-[COMMIT_FOLD]: `Vital.committed` folds the React `Profiler` commit window into its seed projections.
- [05]-[COMPILE_LANE]: `Vital.compiled` folds react-compiler diagnostics into the same evidence rows.

## [02]-[EVIDENCE_RAIL]

[EVIDENCE_RAIL]:
- Owner: the `rasm.ui.vital.row` contribution and `_publisher` — the replay point every evidence lane on this page publishes onto, and the one scope-owned forked publish `FiberSet.makeRuntime` binds to the composing lifecycle: scope close interrupts in-flight publications and a post-close callback publish interrupts on arrival instead of reaching the registry, so no callback publication outlives or retains its registry composition.
- Law: the point carries evidence in both directions — this floor and `viewer/probe` publish local rows, and the app bridge republishes the runtime plane's graded vital rows onto the same replay window, so a board mounted mid-session reads web vitals and local evidence from one retained source without this package importing the runtime plane.
- Law: the grade vocabulary is `runtime:otel/vital`'s, spelled here field-for-field for presentation alone — `_tone` keys the three grades every vital surface renders through, and a cutoff table or rating fold beside them is the forked-semantics defect this floor exists without.
- Law: the tone column is a KEY onto `system/token#TONE_VOCABULARY`'s closed roster, never a color and never a local union — the row names the semantic and the token authority owns every hue, slot, and contrast gate, so restyling every graded surface is one row edit there.
- Owner: `Vital.window`/`Vital.fold`/`Vital.rows` — the ONE bounded-window measure algebra this floor owns and every evidence surface consumes: `window` caps an arriving batch into the held `Chunk` at `policy.samples`, `fold` runs a lane's sample reader over that window in a SINGLE `Chunk.reduce` pass accumulating one fixed `{ total, peak, latest }` triple per named measure, and `rows` projects the fold against the lane's own `Vital.Measures` table — one `<label>-count` plus one `<label>-<measure>-<projection>` row per declared projection, each carrying the measure's own UCUM unit. A lane contributes a measure table and a reader; a per-page window constant, seed, accumulator, or projection is the re-derivation this owner deletes.
- Law: the statistic a measure publishes is a declared COLUMN, never a shape — `projects` names one or more rows of the closed `sum`/`mean`/`peak`/`latest` vocabulary and the accumulator carries all three fields for every measure regardless, so adding `latest` to a gauge or `peak` to a counter is one table edit with no seed, fold, or consumer change; a lane that hard-codes which statistics it emits freezes its board's questions at authoring time.
- Law: a count is a measure with the `sum` projection, never a second concept — a commit phase, an entry family, or an error class reads `1` for the sample it names and `0` otherwise, so occurrence counting and magnitude measurement ride ONE table, ONE accumulator, and ONE pass; a parallel tally record beside the parts record is the split this collapse deletes.
- Law: means divide by the window's WHOLE sample count, so a measure some samples omit reads low by exactly the fraction that dropped it rather than silently re-basing; a measure the browser leaves at zero is a measured zero, a measure the window never saw at all emits nothing, and an empty window carries no rows because a zero-sample mean is fabricated evidence.
- Law: `peak` seeds from the first sample rather than from zero — the accumulator takes an unseen measure's first value whole, so a window of negative deltas peaks at its real maximum instead of at a floor no sample produced.
- Law: `Vital.Policy` is the one deployment row this floor reads and `Vital.policy` is the schema that admits it — the composing root decodes `samples` as a positive integer for every bounded window and `interaction` as the non-negative `event-timing` reporting floor, both branded so no bare literal reaches a window bound, and `interaction` is the SAME number `runtime:otel/vital`'s capture policy hands `web-vitals`, so both readers of that buffer admit one interaction set; a module-level window constant or a floor literal in a measure row is a compile-time assumption about a consumer this package never meets, and an unadmitted cap is that same assumption arriving at runtime as an empty window no board can tell from a quiet browser.
- Boundary: OTLP egress is the app tap's and the point carries it both ways — one bridge folds `viewer/probe`'s render rows onto `runtime:otel/vital`'s `Vital.Report` intake, whose closed carrier set is exactly the render kinds that owner does not measure itself, and republishes the graded facts the same service streams back onto this point; label vocabularies stay surface-local, so the bridge resolves each row onto its carrier kind and supplies the producer's own phases and subject, passing the value unconverted at the scale that kind's UCUM row declares; that intake answers a typed parse rail rather than a bare void, so the bridge carries an explicit refusal arm — a sample the fact constraint rejects stays local evidence on this point and is never re-offered, because discarding the rail leaves a mis-scaled producer row as a silent hole in the graded series while letting it escape kills the capture whose graded facts the same registration streams back.
- Boundary: this floor's own rows stay display and hook-rail evidence — the telemetry owner already grades the jank ceiling from its `longtask` row and the interaction headline from `web-vitals`, so a second carrier kind for either fact mints a rival series two boards then disagree about; this package imports no collector and mints no instrument.

```typescript
import { Array, Chunk, Effect, FiberSet, HashMap, Number, Option, Record, Schema, type Scope, pipe } from "effect"
import { Hook } from "./hook.ts"
import type { Theme } from "./token.ts"

type Row = { readonly label: string; readonly unit: string; readonly value: number }

declare module "./hook.ts" {
  interface Points {
    readonly "rasm.ui.vital.row": { readonly modality: "replay"; readonly payload: Row }
  }
}

const _Policy = Schema.Struct({
  interaction: Schema.Number.pipe(Schema.nonNegative(), Schema.brand("VitalInteraction")),
  samples: Schema.Int.pipe(Schema.positive(), Schema.brand("VitalSamples")),
})

declare namespace Vital {
  type Policy = Schema.Schema.Type<typeof _Policy>
  type Projection = (typeof _projections)[number]
  type Measure = { readonly projects: Array.NonEmptyReadonlyArray<Vital.Projection>; readonly unit: string }
  type Measures = Record.ReadonlyRecord<string, Vital.Measure>
}

const _tone = {
  good: { tone: "success" },
  "needs-improvement": { tone: "caution" },
  poor: { tone: "danger" },
} as const satisfies Record<string, { readonly tone: Theme.Tone }>

const _vitalHook: Hook.Row<"rasm.ui.vital.row"> = { modality: "replay", depth: 128, source: Option.none() }

type _Publish = (row: Row) => void

const _publisher = (registry: Hook.Registry): Effect.Effect<_Publish, never, Scope.Scope> =>
  Effect.map(FiberSet.makeRuntime<never>(), (fork) => (row) => {
    void fork(Effect.asVoid(Hook.publish(registry, "rasm.ui.vital.row", row)))
  })

const _deliver = (publish: _Publish, report: (row: Row) => void, row: Row): void => {
  publish(row)
  report(row)
}

const _projections = ["sum", "mean", "peak", "latest"] as const

type _Held = { readonly total: number; readonly peak: number; readonly latest: number }
type _Window = { readonly count: number; readonly parts: Readonly<Record<string, _Held>> }

const _PROJECT: { readonly [K in Vital.Projection]: (held: _Held, count: number) => number } = {
  sum: (held) => held.total,
  mean: (held, count) => held.total / count,
  peak: (held) => held.peak,
  latest: (held) => held.latest,
}

const _SEED: _Window = { count: 0, parts: {} }

const _window = <A>(held: Chunk.Chunk<A>, arrived: Iterable<A>, samples: Vital.Policy["samples"]): Chunk.Chunk<A> =>
  Chunk.takeRight(Chunk.appendAll(held, Chunk.fromIterable(arrived)), samples)

const _fold = <A>(trace: Chunk.Chunk<A>, read: (sample: A) => Readonly<Record<string, number>>): _Window =>
  Chunk.reduce(trace, _SEED, (acc, sample) =>
    pipe(read(sample), (taken) => ({
      count: acc.count + 1,
      parts: Record.union(
        acc.parts,
        Record.map(taken, (value): _Held => ({ total: value, peak: value, latest: value })),
        (prior, next): _Held => ({
          total: prior.total + next.total,
          peak: Number.max(prior.peak, next.peak),
          latest: next.latest,
        }),
      ),
    })))

const _rows = (label: string, measures: Vital.Measures, window: _Window): ReadonlyArray<Row> =>
  window.count === 0
    ? []
    : [
        { label: `${label}-count`, value: window.count, unit: "1" },
        ...Array.flatMap(Record.toEntries(measures), ([name, measure]) =>
          Array.filterMap(measure.projects, (project) =>
            Option.map(Record.get(window.parts, name), (held) => ({
              label: `${label}-${name}-${project}`,
              value: _PROJECT[project](held, window.count),
              unit: measure.unit,
            })))),
      ]
```

## [03]-[FRAME_OBSERVER]

[FRAME_OBSERVER]:
- Owner: `Vital.observe(registry, policy, report)` — ONE scoped `PerformanceObserver` over the whole `_ENTRY` measure table: acquisition asks `PerformanceObserver.supportedEntryTypes` for the roster the platform serves, `observe({ type, buffered: true })` per surviving row replays already-buffered entries into the first fold, a row declaring `floored` carries `policy.interaction` as its `durationThreshold`, and one `disconnect()` releases with the composition scope; the callback is total — every observed family folds its own bounded window out of the shared batch, projects `Vital.entryRows`, and publishes each row through the replay point before the local report sink. `long-animation-frame` reads the frame's own span, `blockingDuration`, the task and render-prologue spans its render coordinates carve, and the three sums its `scripts` rows carry; `event` reads the whole interaction latency beside the same input-delay, processing, and presentation split `runtime:otel/vital` reports for INP.
- Law: growth is one row and nothing else — the supported roster, the registration loop, the per-family window map, and the callback's dispatch all derive from `_ENTRY`, so a new entry family costs one row here and zero edits at any composing root; a bracket per entry type pushes the enumeration onto every caller, which is the shape this owner deletes.
- Law: the returned roster IS the platform's answer — a family the browser withholds never registers and never appears, so a board renders the evidence it has instead of waiting on a dead observer, and an empty roster reads as a browser carrying neither family.
- Law: the telemetry owner's registrars read these families too — `web-vitals` observes `event` for the INP estimate and `long-animation-frame` for its attribution, and the browser serves every registered observer from one buffer, so a second reader forks no accounting while this floor grades nothing and mints no instrument; the one coupled value is `policy.interaction`, the same floor that owner hands `web-vitals`, because two floors over one buffer let a graded interaction name an event this window never received.
- Law: the long-task ceiling is `runtime:otel/vital`'s row, not this floor's — `long-animation-frame` supersedes the bare `longtask` entry for the jank fact and carries the script attribution it cannot, both families ship Chromium-first so no fallback is lost, and a `longtask` row here mints the second accounting of one ceiling.
- Law: entry streams ride `[02]`'s window algebra unchanged — `Vital.window` bounds the batch, `Vital.fold` runs the family's own `read` as the sample reader, and `Vital.rows` projects; this lane contributes a reader row and nothing else, so a second traversal, a local seed, or a lane-private projection is the re-derivation the algebra deletes.
- Law: every measure arm is total in its own key set — a part the browser leaves at zero reads as a measured zero and never as an absent key, so the shared mean stays honest for the family.
- Law: the phase vocabulary is the telemetry owner's, spelled here for the raw entry — the event row's `input`/`processing`/`presentation` keys are the INP subparts that owner reports from the library's attribution, so a drill-in from a graded INP fact onto this floor's windowed events reads one decomposition rather than two dialects of it.
- Law: script attribution splits by cardinality, not by owner — the graded fact carries the ONE worst script's source, function, invoker, and subpart from the telemetry owner's attribution, while this floor sums every script's execution, forced style-and-layout, and paused durations into bounded parts and leaves the per-script `scripts` rows as unbounded drill-in evidence beside the row.
- Law: observers are passive — no forced layout, no synthetic events, no `takeRecords` polling loop; an idle document reports idle numbers truthfully.
- Packages: `web-vitals` — the types build augments the DOM lib with `PerformanceLongAnimationFrameTiming` and `PerformanceScriptTiming`, so raw entries type without a second `@types` package; `effect` (`Array`, `Chunk`, `Effect`, `HashMap`, `Number`, `Option`, `Record`, `pipe`, `Scope`).

```typescript
type _Entry = {
  readonly "long-animation-frame": PerformanceLongAnimationFrameTiming
  readonly event: PerformanceEventTiming
}

type _Parts = Readonly<Record<string, number>>

const _ENTRY = {
  "long-animation-frame": {
    floored: false,
    label: "loaf",
    read: (entry: PerformanceLongAnimationFrameTiming): _Parts => ({
      blocking: entry.blockingDuration,
      frame: entry.duration,
      ...(entry.renderStart > 0
        ? { render: Number.max(0, entry.styleAndLayoutStart - entry.renderStart), task: entry.renderStart - entry.startTime }
        : { render: 0, task: entry.duration }),
      ...entry.scripts.reduce(
        (held, script) => ({
          forced: held.forced + script.forcedStyleAndLayoutDuration,
          paused: held.paused + script.pauseDuration,
          script: held.script + script.duration,
        }),
        { forced: 0, paused: 0, script: 0 },
      ),
    }),
  },
  event: {
    floored: true,
    label: "event",
    read: (entry: PerformanceEventTiming): _Parts => ({
      input: entry.processingStart - entry.startTime,
      latency: entry.duration,
      presentation: entry.startTime + entry.duration - entry.processingEnd,
      processing: entry.processingEnd - entry.processingStart,
    }),
  },
} as const satisfies { readonly [K in keyof _Entry]: { readonly floored: boolean; readonly label: string; readonly read: (entry: _Entry[K]) => _Parts } }

declare namespace Vital {
  type Entry = keyof typeof _ENTRY
}

const _READ = _ENTRY as Readonly<
  Record<Vital.Entry, { readonly floored: boolean; readonly label: string; readonly read: (entry: PerformanceEntry) => _Parts }>
>

const _supported = (): ReadonlyArray<Vital.Entry> =>
  Array.filter(Record.keys(_ENTRY), (type) => PerformanceObserver.supportedEntryTypes.includes(type))

const _observe = (
  registry: Hook.Registry,
  policy: Vital.Policy,
  report: (row: Row) => void,
): Effect.Effect<ReadonlyArray<Vital.Entry>, never, Scope.Scope> =>
  Effect.flatMap(_publisher(registry), (publish) =>
    Effect.map(
      Effect.acquireRelease(
        Effect.sync(() => {
          const observed = _supported()
          let windows = HashMap.empty<Vital.Entry, Chunk.Chunk<PerformanceEntry>>()
          const observer = new PerformanceObserver((list) =>
            Array.forEach(observed, (type) =>
              Array.match(Array.filter(list.getEntries(), (entry) => entry.entryType === type), {
                onEmpty: () => undefined,
                onNonEmpty: (arrived) => {
                  const trace = _window(
                    Option.getOrElse(HashMap.get(windows, type), () => Chunk.empty<PerformanceEntry>()),
                    arrived,
                    policy.samples,
                  )
                  windows = HashMap.set(windows, type, trace)
                  Array.forEach(_entryRows(type, trace), (projected) => _deliver(publish, report, projected))
                },
              })))
          Array.forEach(observed, (type) =>
            observer.observe({ buffered: true, type, ...(_READ[type].floored ? { durationThreshold: policy.interaction } : {}) }))
          return { observed, observer }
        }),
        ({ observer }) => Effect.sync(() => observer.disconnect()),
      ),
      ({ observed }) => observed,
    ))

const _durations = (names: Array.NonEmptyReadonlyArray<string>): Vital.Measures =>
  Record.fromEntries(Array.map(names, (name) => [name, { projects: ["mean", "peak"], unit: "ms" }] as const))

const _ENTRY_MEASURES: Readonly<Record<Vital.Entry, Vital.Measures>> = {
  "long-animation-frame": _durations(["blocking", "frame", "render", "task", "forced", "paused", "script"]),
  event: _durations(["input", "latency", "presentation", "processing"]),
}

const _entryRows = (kind: Vital.Entry, trace: Chunk.Chunk<PerformanceEntry>): ReadonlyArray<Row> =>
  _rows(_READ[kind].label, _ENTRY_MEASURES[kind], _fold(trace, _READ[kind].read))
```

## [04]-[COMMIT_FOLD]

[COMMIT_FOLD]:
- Owner: `Vital.committed(registry, policy, report)` — the React tree lane, minted as a scoped acquisition over the same publisher law as the observer: one `<Profiler id onRender>` per measured subtree feeds an id-keyed window owned by the callback closure, `onRender`'s full `(id, phase, actualDuration, baseDuration, startTime, commitTime)` tuple enters through `[02]`'s window, and the projections publish through the replay point before reaching the local report sink. `commit-<id>-actual-mean` against `commit-<id>-base-mean` reads whether the compiler's memoization is holding, the per-phase `-sum` rows read churn, the `-peak` rows read the worst commit in the window, and the `lag` measure (`commitTime - startTime`) reads the scheduling latency the tree paid beyond its own render cost.
- Packages: `react` (`Profiler`, the `ProfilerOnRenderCallback` contract); `effect` (`Array`, `Chunk`, `Effect`, `HashMap`, `Option`, `Scope`).
- Law: the profiled set is a bounded roster — measured subtrees are named policy rows (the view plane, the viewer canvas shell, an app-nominated surface), never a per-component wrap; `id` keys the label prefix so two subtrees never blur into one series.
- Law: phase is a MEASURE carrying the `sum` projection, never a row family — `mount`, `update`, and `nested-update` each read `1` for the sample they name, so churn counts and render durations ride one table and one pass; a per-phase window triple is the named defect, and declaring a phase under `mean` would emit a meaningless `mount-mean` the projection column makes visible at the table.
- Law: this lane contributes a measure table, a reader, and a label prefix — every window bound, accumulator, and projection is `[02]`'s, so the commit series and the entry series carry ONE row grammar and a board renders both through one reader.
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

const _COMMIT_MEASURES: Vital.Measures = {
  actual: { projects: ["mean", "peak"], unit: "ms" },
  base: { projects: ["mean"], unit: "ms" },
  lag: { projects: ["mean", "peak"], unit: "ms" },
  mount: { projects: ["sum"], unit: "1" },
  update: { projects: ["sum"], unit: "1" },
  nested: { projects: ["sum"], unit: "1" },
}

const _commitRead = (commit: _Commit): Readonly<Record<string, number>> => ({
  actual: commit.actual,
  base: commit.base,
  lag: commit.commit - commit.start,
  mount: commit.phase === "mount" ? 1 : 0,
  update: commit.phase === "update" ? 1 : 0,
  nested: commit.phase === "nested-update" ? 1 : 0,
})

const _committed = (
  registry: Hook.Registry,
  policy: Vital.Policy,
  report: (row: Row) => void,
): Effect.Effect<ProfilerOnRenderCallback, never, Scope.Scope> =>
  Effect.map(_publisher(registry), (publish) => {
    let held = HashMap.empty<string, Chunk.Chunk<_Commit>>()
    return (id, phase, actualDuration, baseDuration, startTime, commitTime) => {
      const commit = { id, phase, actual: actualDuration, base: baseDuration, start: startTime, commit: commitTime }
      const trace = _window(Option.getOrElse(HashMap.get(held, id), () => Chunk.empty<_Commit>()), [commit], policy.samples)
      held = HashMap.set(held, id, trace)
      Array.forEach(_commitRows(id, trace), (row) => _deliver(publish, report, row))
    }
  })

const _commitRows = (id: string, trace: Chunk.Chunk<_Commit>): ReadonlyArray<Row> =>
  _rows(`commit-${id}`, _COMMIT_MEASURES, _fold(trace, _commitRead))
```

## [05]-[COMPILE_LANE]

[COMPILE_LANE]:
- Owner: `Vital.compiled(text, file)` — the build-lane counterpart row: `runBabelPluginReactCompiler` compiles a source under `noEmit`-grade options with a `logger` sink, the `LoggerEvent` census folds into the same row shape (`compile-success`, `compile-skip`, `compile-error` counts), and a CI gate renders the rows exactly like runtime evidence — one vocabulary spans field and build.
- Packages: `babel-plugin-react-compiler` (`runBabelPluginReactCompiler`, `LoggerEvent`, `PluginOptions`); `effect` (`Array`).
- Law: the lane is build-time only — the fence runs in tooling and CI, never in the browser bundle; the browser plane's compiler evidence is the dev-validator surface (`react-compiler-runtime`'s `renderCounterRegistry`), which an app tap reads and folds through the same rows when the emission flags are armed.
- Law: severity is the row split — a `CompileError` event counts by its category, a skip counts as deliberate opt-out, and a rising skip count is architecture pressure on the skipped components, never a threshold to tune away.
- Law: tone keys the `[02]` grade table alone — a graded row renders through `_tone`, while these census rows carry a count and no grade, so a board tones a rising error or skip count by its own threshold rather than by a fourth key minted here.
- Boundary: bundler wiring, `panicThreshold`, and gating are the build plane's config bag; probe's claim board and the chart cohort render these rows; the hook rail carries them to any app sink.

```typescript
import { type LoggerEvent, type PluginOptions, runBabelPluginReactCompiler } from "babel-plugin-react-compiler"

const _compiled = (text: string, file: string): ReadonlyArray<Row> => {
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
  type Held = _Held
  type Reading = Readonly<Record<string, number>>
  type Window = _Window
  type Shape = {
    readonly entry: typeof _ENTRY
    readonly policy: typeof _Policy
    readonly tone: typeof _tone
    readonly hook: typeof _vitalHook
    readonly projections: typeof _projections
    readonly project: typeof _PROJECT
    readonly window: typeof _window
    readonly fold: typeof _fold
    readonly rows: typeof _rows
    readonly observe: typeof _observe
    readonly entryRows: typeof _entryRows
    readonly committed: typeof _committed
    readonly commitRows: typeof _commitRows
    readonly compiled: typeof _compiled
  }
}

const Vital: Vital.Shape = {
  entry: _ENTRY,
  policy: _Policy,
  tone: _tone,
  hook: _vitalHook,
  projections: _projections,
  project: _PROJECT,
  window: _window,
  fold: _fold,
  rows: _rows,
  observe: _observe,
  entryRows: _entryRows,
  committed: _committed,
  commitRows: _commitRows,
  compiled: _compiled,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Vital }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
