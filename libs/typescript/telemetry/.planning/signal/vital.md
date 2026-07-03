# [TELEMETRY_VITAL]

Browser RUM is a vocabulary table and one scoped observer bridge — zero `web-vitals`, zero polling: the six vital kinds (LCP, CLS, INP, FCP, TTFB, long task) are policy rows carrying their Performance-Timeline entry type, budget thresholds, and accumulation semantics; one `PerformanceObserver` bracket lifts the platform's push callbacks into a typed `Stream` of vital facts; a `mapAccum` step folds the accumulation semantics each kind declares (CLS sums shifts, INP tracks the interaction crest, paint kinds settle once); and emission is two bounded instruments per fact — the current level as a tagged gauge, the graded occurrence as a tagged counter — named by `Convention` rows. Grading is derived from the row thresholds, so a budget change is a row edit that moves the grade fold, the metrics, and every dashboard panel built on the same rows at once. This module is `runtime:browser` — the `./browser` subpath alone resolves it.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                     |
| :-----: | :--------- | :-------------------------------------------------------------------------- |
|  [01]   | [BUDGETS]  | the vital-kind vocabulary: entry types, thresholds, accumulation semantics   |
|  [02]   | [OBSERVER] | the scoped `PerformanceObserver` bridge and the accumulation fold            |
|  [03]   | [EMISSION] | bounded instruments, the drain Layer, and the span-enrichment boundary       |

## [2]-[BUDGETS]

[BUDGETS]:
- Owner: the interior `_rows` anchor — one row per vital kind: `entry` (the Performance-Timeline `entryType` the observer subscribes), `good`/`poor` (the web-standard budget thresholds the grade fold reads), `unit`, and `fold` (the accumulation semantic: `sum` for CLS's session accumulation, `crest` for INP's worst-interaction tracking and long-task ceilings, `last` for the settle-once paint and navigation kinds).
- Law: the thresholds are the Core Web Vitals standard grades as data — LCP 2500/4000 ms, CLS 0.1/0.25, INP 200/500 ms, FCP 1800/3000 ms, TTFB 800/1800 ms, long task 50/200 ms — and the three-grade verdict (`good`/`mid`/`poor`) derives from the row, so no consumer ever compares against a literal.
- Law: the kind union, the grade union, and the fold union all derive — `keyof typeof _rows`, the `_GRADES` tuple, the row's `fold` column — and the guard pair on the merged hub closes the table in both directions.
- Growth: a new vital (a soft-navigation metric, a custom mark budget) is one row — the observer, the fold, the grade, the instruments, and the board panels all follow from it.

```typescript
import type { Duration } from "effect"

const _GRADES = ["good", "mid", "poor"] as const

const _rows = {
  cls: { entry: "layout-shift", fold: "sum", good: 0.1, poor: 0.25, unit: "score" },
  fcp: { entry: "paint", fold: "last", good: 1800, poor: 3000, unit: "ms" },
  inp: { entry: "event", fold: "crest", good: 200, poor: 500, unit: "ms" },
  lcp: { entry: "largest-contentful-paint", fold: "last", good: 2500, poor: 4000, unit: "ms" },
  longtask: { entry: "longtask", fold: "crest", good: 50, poor: 200, unit: "ms" },
  ttfb: { entry: "navigation", fold: "last", good: 800, poor: 1800, unit: "ms" },
} as const

declare namespace Vital {
  type Kind = keyof typeof _rows
  type Grade = (typeof _GRADES)[number]
  type Row = { readonly entry: string; readonly fold: "crest" | "last" | "sum"; readonly good: number; readonly poor: number; readonly unit: string }
  type Fact = { readonly kind: Kind; readonly value: number }
  type Ledger = { readonly [K in Kind]?: number }
  type Tempo = { readonly settle: Duration.DurationInput }
  type _Rows<T extends Record<Kind, Row> = typeof _rows> = T
  type _Keys<K extends Kind = keyof typeof _rows> = K
}

const _grade = (kind: Vital.Kind, value: number): Vital.Grade =>
  value <= _rows[kind].good ? "good" : value <= _rows[kind].poor ? "mid" : "poor"
```

## [3]-[OBSERVER]

[OBSERVER]:
- Owner: the `_observed` bridge — one `Stream.asyncScoped` whose acquisition constructs a single `PerformanceObserver`, subscribes one `observe({ type, buffered: true })` per row, and whose release disconnects; `buffered: true` replays entries recorded before the observer attached, so a late boot still sees the paint vitals.
- Law: the raw entry maps to a `Fact` at the seam — `layout-shift` entries contribute their `value` (input-excluded shifts only), `event` entries their `duration`, `navigation` entries their `responseStart` as TTFB, paint entries their `startTime` discriminated by `name` — and the accumulation fold is one `mapAccum` over the row's `fold` column threading the per-kind ledger, so the emitted stream carries the current accounted value per kind, never a raw sample downstream consumers must re-fold.
- Exemption: the observer callback is the platform-forced statement seam — emissions are `void`-discarded inside the listener, the one place the pipeline cannot be an expression; the entry projection beside it is the marked admission kernel for the untyped `PerformanceEntry` records.
- Boundary: URL-bearing span enrichment (resource timing folded onto fetch/XHR spans) is the `sdk-trace-web` RUM toolkit riding `otlp/export`'s `web` lane — this bridge reads the Performance Timeline for vitals only and is never a second span source.
- Growth: a new entry projection is one arm in the admission kernel keyed by the row's `entry`.

```typescript
import { Effect, HashMap, Number, Option, Stream } from "effect"

const _fact = (entry: PerformanceEntry): Option.Option<Vital.Fact> => {
  const record = entry as PerformanceEntry & {
    readonly duration: number
    readonly hadRecentInput?: boolean
    readonly name: string
    readonly responseStart?: number
    readonly value?: number
  }
  switch (entry.entryType) {
    case "layout-shift":
      return record.hadRecentInput === true || record.value === undefined
        ? Option.none()
        : Option.some({ kind: "cls", value: record.value })
    case "event":
      return Option.some({ kind: "inp", value: record.duration })
    case "largest-contentful-paint":
      return Option.some({ kind: "lcp", value: record.startTime })
    case "longtask":
      return Option.some({ kind: "longtask", value: record.duration })
    case "navigation":
      return record.responseStart === undefined ? Option.none() : Option.some({ kind: "ttfb", value: record.responseStart })
    case "paint":
      return record.name === "first-contentful-paint" ? Option.some({ kind: "fcp", value: record.startTime }) : Option.none()
    default:
      return Option.none()
  }
}

const _accounted = (
  ledger: HashMap.HashMap<Vital.Kind, number>,
  fact: Vital.Fact,
): readonly [HashMap.HashMap<Vital.Kind, number>, Vital.Fact] => {
  const held = Option.getOrElse(HashMap.get(ledger, fact.kind), () => 0)
  const next = {
    crest: Number.max(held, fact.value),
    last: fact.value,
    sum: held + fact.value,
  }[_rows[fact.kind].fold]
  return [HashMap.set(ledger, fact.kind, next), { kind: fact.kind, value: next }]
}

const _observed: Stream.Stream<Vital.Fact> = Stream.asyncScoped<Vital.Fact>(
  (emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            Option.match(_fact(entry), {
              onNone: () => undefined,
              onSome: (fact) => void emit.single(fact),
            })
          }
        })
        for (const row of Object.values(_rows)) {
          observer.observe({ buffered: true, type: row.entry })
        }
        return observer
      }),
      (observer) => Effect.sync(() => observer.disconnect()),
    ),
  { bufferSize: 32, strategy: "sliding" },
).pipe(Stream.mapAccum(HashMap.empty<Vital.Kind, number>(), _accounted))
```

## [4]-[EMISSION]

[EMISSION]:
- Owner: the assembled `Vital` export — the row table spread in, the grade fold, the live fact stream, and the drain Layer under one name.
- Law: two instruments serve every kind, both bounded — `Convention.metric.vitalLevel` is a gauge written through `Metric.set` and tagged by the kind row (six series), `Convention.metric.vitalObserved` is an incremental counter tagged by kind and derived grade (eighteen series) — and the exact-value distribution beyond the gauge is an export-lane concern, never a third instrument here.
- Law: `Vital.live(tempo)` is the registration node — a `Layer.scopedDiscard` forking the observed stream through `Stream.changesWith` on the accounted value (a repeat of an unchanged accounting emits nothing) and `Stream.debounce(tempo.settle)` so a layout-shift burst settles before it stamps, then draining each fact into the two instruments; the Layer merges at the browser composition root beside `Export.live`.
- Receipt: each drained fact annotates the current span with the kind and grade rows, so a session trace carries its vitals inline.
- Entry: `Vital.live(tempo)`; `Vital.stream` for a consumer that folds its own view; `Vital.grade(kind, value)` for board threshold reuse.
- Growth: an instrument axis is closed — new analysis lands as board queries over the same two instruments.

```typescript
import { Effect, Layer, Metric, Stream } from "effect"
import { Convention } from "@rasm/ts/telemetry"

const _level = Metric.gauge(Convention.metric.vitalLevel)
const _observedCount = Metric.counter(Convention.metric.vitalObserved, { incremental: true })

const _drained = (fact: Vital.Fact): Effect.Effect<void> =>
  Effect.all(
    [
      Metric.set(Metric.tagged(_level, Convention.rasm.vitalKind, fact.kind), fact.value),
      Metric.increment(
        Metric.tagged(
          Metric.tagged(_observedCount, Convention.rasm.vitalKind, fact.kind),
          Convention.rasm.vitalGrade,
          _grade(fact.kind, fact.value),
        ),
      ),
      Effect.annotateCurrentSpan(Convention.rasm.vitalKind, fact.kind),
    ],
    { discard: true },
  )

const Vital: {
  readonly grade: (kind: Vital.Kind, value: number) => Vital.Grade
  readonly live: (tempo: Vital.Tempo) => Layer.Layer<never>
  readonly rows: typeof _rows
  readonly stream: Stream.Stream<Vital.Fact>
} = {
  grade: _grade,
  live: (tempo) =>
    Layer.scopedDiscard(
      Effect.forkScoped(
        Stream.runForEach(
          _observed.pipe(
            Stream.changesWith((prior, next) => prior.kind === next.kind && prior.value === next.value),
            Stream.debounce(tempo.settle),
          ),
          _drained,
        ),
      ),
    ),
  rows: _rows,
  stream: _observed,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Vital }
```
