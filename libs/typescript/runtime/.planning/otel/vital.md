# [RUNTIME_VITAL]

Browser RUM is a vocabulary table, one scoped observer bridge, and one report intake — zero `web-vitals`, zero polling: every vital kind is a policy row carrying budget thresholds and accumulation semantics, the web family (LCP, CLS, INP, FCP, TTFB, long task) adding its Performance-Timeline entry type and the render family (`frame`, `gpumem`, `capture`) entering through the intake an app-composed tap feeds; one `PerformanceObserver` bracket lifts the platform's push callbacks into a typed `Stream` of vital facts, and a `mapAccum` step folds the accumulation semantics each kind declares — CLS folds session windows to their crest per the web standard, INP tracks the interaction crest, paint kinds settle once.

Emission is two bounded instruments per fact — the current level as a tagged gauge, the graded occurrence as a tagged counter — named by `Convention` rows; grading derives from the row thresholds, so a budget change is a row edit that moves the grade fold, the metrics, and every dashboard panel at once. This module is `runtime:browser` — the `./browser` subpath alone resolves it. Its module is `runtime/src/otel/vital.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                     | [PUBLIC] |
| :-----: | :--------- | :------------------------------------------------------------------------- | :------- |
|  [01]   | `BUDGETS`  | the vital-kind vocabulary: entry types, thresholds, accumulation semantics | `Vital`  |
|  [02]   | `OBSERVER` | the scoped `PerformanceObserver` bridge and the accumulation fold          | `Vital`  |
|  [03]   | `EMISSION` | bounded instruments, the report intake, the drain Layer, the span-enrichment boundary | `Vital`  |

## [02]-[BUDGETS]

[BUDGETS]:
- Owner: the interior `_rows` anchor — one row per vital kind: `entry` (the Performance-Timeline `entryType` the observer subscribes — absent on the render rows, which enter through the report intake alone), `good`/`poor` (the budget thresholds the grade fold reads), `unit`, `fold` (the accumulation semantic: `session` for CLS's session-window law, `crest` for INP's worst-interaction tracking, long-task ceilings, and render peaks, `last` for the settle-once paint, navigation, and verdict kinds), and the optional `threshold` (the observer's `durationThreshold` floor where the entry family admits one — the INP row prunes sub-40ms events at the platform, before any callback fires).
- Law: the thresholds are the Core Web Vitals standard grades as data — LCP 2500/4000 ms, CLS 0.1/0.25, INP 200/500 ms, FCP 1800/3000 ms, TTFB 800/1800 ms, long task 50/200 ms — and the three-grade verdict derives from the row, so no consumer ever compares against a literal.
- Law: render rows grade by the same discipline — `frame` against the 60/30 fps frame budget (17/33 ms) at its crest, `gpumem` against the byte-budget peak, `capture` against the capture-hash verdict (0 match, 1 mismatch) — so a render budget change is the same row edit as a web budget change.
- Law: CLS session accumulation is the standard's own window law as one policy row — shifts group into a session window while the gap to the previous shift stays under `_SESSION.gap` (1 s) and the window's span under `_SESSION.cap` (5 s), the page's CLS is the CREST of its session windows — so the fold state carries the temporal dimensions the semantics demand and separate interaction bursts can never inflate one score; a page-lifetime sum is the rejected accounting.
- Law: the kind union, the grade union, and the fold union all derive — `keyof typeof _rows` against the `_KINDS` key tuple, the `_GRADES` tuple, the row's `fold` column — and the guard pair on the merged hub ties tuple and table closed in both directions.
- Growth: a new vital (a soft-navigation metric, a custom mark budget) is one row — the observer where the row carries `entry`, the fold, the grade, the instruments, and the board panels all follow from it; a new accumulation semantic is one `_folds` arm the column selects.

```typescript
import { Context, Effect, HashMap, Layer, Metric, Number, Option, Predicate, Queue, Schema, Stream, pipe } from "effect"
import type { HrTime, Span } from "@opentelemetry/api"
import { addSpanNetworkEvents, getResource, normalizeUrl } from "@opentelemetry/sdk-trace-web"
import { Convention } from "@rasm/ts/core"

const _KINDS = ["capture", "cls", "fcp", "frame", "gpumem", "inp", "lcp", "longtask", "ttfb"] as const // key tuple: the spread below holds Schema.Literal's non-empty overload; derived keys would demote it to the widened array
const _GRADES = ["good", "mid", "poor"] as const

const _SESSION = { gap: 1_000, cap: 5_000 } as const

const _rows = {
  capture: { fold: "last", good: 0, poor: 0.5, unit: "1" },
  cls: { entry: "layout-shift", fold: "session", good: 0.1, poor: 0.25, unit: "score" },
  fcp: { entry: "paint", fold: "last", good: 1800, poor: 3000, unit: "ms" },
  frame: { fold: "crest", good: 17, poor: 33, unit: "ms" },
  gpumem: { fold: "crest", good: 536_870_912, poor: 1_073_741_824, unit: "By" },
  inp: { entry: "event", fold: "crest", good: 200, poor: 500, threshold: 40, unit: "ms" },
  lcp: { entry: "largest-contentful-paint", fold: "last", good: 2500, poor: 4000, unit: "ms" },
  longtask: { entry: "longtask", fold: "crest", good: 50, poor: 200, unit: "ms" },
  ttfb: { entry: "navigation", fold: "last", good: 800, poor: 1800, unit: "ms" },
} as const

declare namespace Vital {
  type Kind = keyof typeof _rows
  type Grade = (typeof _GRADES)[number]
  type Fold = (typeof _rows)[Kind]["fold"]
  type Row = {
    readonly entry?: string
    readonly fold: "crest" | "last" | "session"
    readonly good: number
    readonly poor: number
    readonly threshold?: number
    readonly unit: string
  }
  type Fact = _Fact
  type Policy = _Policy
  type _Rows<T extends Record<(typeof _KINDS)[number], Row> = typeof _rows> = T
  type _Keys<K extends (typeof _KINDS)[number] = Kind> = K
}

class _Fact extends Schema.Class<_Fact>("Vital/Fact")({
  at: Schema.Number.pipe(Schema.finite(), Schema.nonNegative()),
  kind: Schema.Literal(..._KINDS),
  value: Schema.Number.pipe(Schema.finite(), Schema.nonNegative()),
}) {}

class _Policy extends Schema.Class<_Policy>("Vital/Policy")({
  pulse: Schema.Int.pipe(Schema.positive()),
  settle: Schema.Duration,
}) {}

const _grade = (kind: Vital.Kind, value: number): Vital.Grade =>
  value <= _rows[kind].good ? "good" : value <= _rows[kind].poor ? "mid" : "poor"
```

## [03]-[OBSERVER]

[OBSERVER]:
- Owner: the `_observed` bridge — one `Stream.asyncScoped` whose acquisition constructs a single `PerformanceObserver`, intersects the table with `PerformanceObserver.supportedEntryTypes`, subscribes one `observe({ type, buffered: true })` per supported row (the row's `threshold` column riding `durationThreshold` where present), and whose release disconnects; `buffered: true` replays entries recorded before the observer attached, so a late boot still sees the paint vitals while a browser lacking an entry family degrades by data instead of defecting acquisition.
- Law: the raw entry maps to a `Fact` at the seam — `layout-shift` entries contribute their `value` with input-caused shifts (`hadRecentInput`) excluded, `event` entries contribute their `duration` only when a non-zero `interactionId` marks a real interaction, `navigation` entries their `responseStart` as TTFB, paint entries their `startTime` discriminated by `name` — every fact stamped with the entry's own `startTime` so the temporal folds read timeline coordinates, never wall clock.
- Law: the accumulation fold is one `mapAccum` over the row's `fold` column threading a per-kind cell ledger — the cell carries the emitted crest with the session dimensions (`window`, `opened`, `last`) the `session` arm consumes — so the emitted stream carries the current accounted value per kind, never a raw sample downstream consumers must re-fold, and the fold arms are one `_folds` record the column indexes.
- Exemption: the observer callback is the platform-forced statement seam — emissions are `void`-discarded inside the listener; the entry projection beside it is the marked admission kernel for the untyped `PerformanceEntry` records.
- Boundary: URL-bearing span enrichment composes through `Vital.enrich(span, request)`: `normalizeUrl` fixes the lookup identity, `getResource` selects the nearest unused main/preflight timing pair inside the supplied span range, and `addSpanNetworkEvents` projects both onto the caller-owned fetch/XHR span. This bridge never opens a span, so `browser/fetch` keeps request ownership while this module owns the browser Performance-Timeline projection.
- Growth: a new entry projection is one arm in the admission kernel keyed by the row's `entry`.

```typescript
declare namespace Vital {
  type Request = {
    readonly url: string
    readonly start: HrTime
    readonly end: HrTime
    readonly initiator: Option.Option<string>
    readonly used: WeakSet<PerformanceResourceTiming>
  }
}

const _enrich = (span: Span, request: Vital.Request): Option.Option<PerformanceResourceTiming> => {
  const resources = performance.getEntriesByType("resource").filter(
    (entry): entry is PerformanceResourceTiming => "initiatorType" in entry,
  )
  const timing = getResource(
    normalizeUrl(request.url),
    request.start,
    request.end,
    resources,
    request.used,
    Option.getOrUndefined(request.initiator),
  )
  const attach = (entry: PerformanceResourceTiming): PerformanceResourceTiming => {
    request.used.add(entry)
    addSpanNetworkEvents(span, entry)
    return entry
  }
  Option.match(Option.fromNullable(timing.corsPreFlightRequest), { onNone: () => undefined, onSome: attach })
  return Option.map(Option.fromNullable(timing.mainRequest), attach)
}

const _fact = (entry: PerformanceEntry): Option.Option<Vital.Fact> => {
  switch (entry.entryType) {
    case "layout-shift":
      return (Predicate.hasProperty(entry, "hadRecentInput") && entry.hadRecentInput === true) ||
        !Predicate.hasProperty(entry, "value") || !Predicate.isNumber(entry.value)
        ? Option.none()
        : Option.some(new _Fact({ at: entry.startTime, kind: "cls", value: entry.value }))
    case "event":
      // interactionId 0 marks a non-interaction event: it never counts toward INP
      return !Predicate.hasProperty(entry, "interactionId") || !Predicate.isNumber(entry.interactionId) || entry.interactionId === 0
        ? Option.none()
        : Option.some(new _Fact({ at: entry.startTime, kind: "inp", value: entry.duration }))
    case "largest-contentful-paint":
      return Option.some(new _Fact({ at: entry.startTime, kind: "lcp", value: entry.startTime }))
    case "longtask":
      return Option.some(new _Fact({ at: entry.startTime, kind: "longtask", value: entry.duration }))
    case "navigation":
      return !Predicate.hasProperty(entry, "responseStart") || !Predicate.isNumber(entry.responseStart)
        ? Option.none()
        : Option.some(new _Fact({ at: entry.startTime, kind: "ttfb", value: entry.responseStart }))
    case "paint":
      return entry.name === "first-contentful-paint"
        ? Option.some(new _Fact({ at: entry.startTime, kind: "fcp", value: entry.startTime }))
        : Option.none()
    default:
      return Option.none()
  }
}

type _Cell = { readonly held: number; readonly window: number; readonly opened: number; readonly last: number }

const _EMPTY: _Cell = { held: 0, window: 0, opened: 0, last: 0 }

const _folds = {
  crest: (cell, fact) => ({ ...cell, held: Number.max(cell.held, fact.value) }),
  last: (cell, fact) => ({ ...cell, held: fact.value }),
  session: (cell, fact) => {
    // CWV window law: a >1s gap or a >5s span opens a fresh window; the score is the crest of windows
    const fresh = fact.at - cell.last > _SESSION.gap || fact.at - cell.opened > _SESSION.cap
    const window = fresh ? fact.value : cell.window + fact.value
    return { held: Number.max(cell.held, window), window, opened: fresh ? fact.at : cell.opened, last: fact.at }
  },
} as const satisfies Record<Vital.Fold, (cell: _Cell, fact: Vital.Fact) => _Cell>

const _accounted = (
  ledger: HashMap.HashMap<Vital.Kind, _Cell>,
  fact: Vital.Fact,
): readonly [HashMap.HashMap<Vital.Kind, _Cell>, Vital.Fact] => {
  const cell = _folds[_rows[fact.kind].fold](Option.getOrElse(HashMap.get(ledger, fact.kind), () => _EMPTY), fact)
  return [HashMap.set(ledger, fact.kind, cell), new _Fact({ at: fact.at, kind: fact.kind, value: cell.held })]
}

const _FLOW = { buffer: 32 } as const

const _watched: Stream.Stream<Vital.Fact> = Stream.asyncScoped<Vital.Fact>(
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
        const supported = new Set(PerformanceObserver.supportedEntryTypes)
        for (const row of Object.values(_rows)) {
          if ("entry" in row && supported.has(row.entry)) {
            observer.observe({
              buffered: true,
              type: row.entry,
              ...("threshold" in row && { durationThreshold: row.threshold }),
            })
          }
        }
        return observer
      }),
      (observer) => Effect.sync(() => observer.disconnect()),
    ),
  { bufferSize: _FLOW.buffer, strategy: "sliding" },
)

const _observed: Stream.Stream<Vital.Fact> = _watched.pipe(
  Stream.mapAccum(HashMap.empty<Vital.Kind, _Cell>(), _accounted),
)
```

## [04]-[EMISSION]

[EMISSION]:
- Owner: the assembled `Vital` export — the row table spread in, the grade fold, the live fact stream, the report intake, and the drain Layer under one name.
- Law: two instruments serve every kind, both bounded — `Convention.metric.vitalLevel` is a gauge written through `Metric.set` and tagged by the kind row, `Convention.metric.vitalObserved` is an incremental counter tagged by kind and derived grade — the series fan is the kind and grade product, and the exact-value distribution beyond the gauge is an export-lane concern, never a third instrument here.
- Law: `Vital.live(policy)` is the registration node — a scoped Layer providing the `Vital.Report` intake and forking one drain: the observer stream merged with the intake queue, the accumulation fold, `Stream.changesWith` on the accounted value (a repeat of an unchanged accounting emits nothing), and `Stream.throttle` shaping the stamp rate through the policy's `settle` and `pulse` axes so a layout-shift burst cannot flood the gauge, each surviving fact draining into the two instruments; the Layer merges at the browser composition root beside `Export.live`.
- Law: render-vital emission is this module's — the ui viewer probe stays display-only, and its local render rows (frame timing, the DeckMetrics counters with the gpu-memory peak, capture-hash verdicts) reach the two instruments only through the `Vital.Report` tap an app composition root composes.
- Receipt: each throttled fact opens one short `Convention.metric.vitalObserved` evidence span and annotates it with the kind and grade rows, so the trace signal is inhabited rather than depending on an ambient request span that the observer fiber cannot own.
- Entry: `Vital.live(policy)`; `Vital.Report` for the app-composed render tap; `Vital.stream` for a consumer that folds its own web-family view; `Vital.grade(kind, value)` for board threshold reuse.
- Growth: an instrument axis is closed — new analysis lands as board queries over the same two instruments.
- Packages: `effect` (`Metric`, `Layer`, `Stream`, `Queue`, `Context`), `@opentelemetry/sdk-trace-web` (`normalizeUrl`, `getResource`, `addSpanNetworkEvents`), `@rasm/ts/core` (`Convention`).

```typescript
const _level = Metric.gauge(Convention.metric.vitalLevel)
const _observedCount = Metric.counter(Convention.metric.vitalObserved, { incremental: true })

class _Report extends Context.Tag("Vital/Report")<_Report, (fact: Vital.Fact) => Effect.Effect<void>>() {}

const _drained = (fact: Vital.Fact): Effect.Effect<void> =>
  pipe(_grade(fact.kind, fact.value), (grade) =>
    Effect.all(
      [
        Metric.set(Metric.tagged(_level, Convention.rasm.vitalKind, fact.kind), fact.value),
        Metric.increment(
          Metric.tagged(
            Metric.tagged(_observedCount, Convention.rasm.vitalKind, fact.kind),
            Convention.rasm.vitalGrade,
            grade,
          ),
        ),
        Effect.annotateCurrentSpan(Convention.rasm.vitalKind, fact.kind),
        Effect.annotateCurrentSpan(Convention.rasm.vitalGrade, grade),
      ],
      { discard: true },
    ).pipe(Effect.withSpan(Convention.metric.vitalObserved)))

const Vital: {
  readonly Fact: typeof _Fact
  readonly Policy: typeof _Policy
  readonly Report: typeof _Report
  readonly enrich: (span: Span, request: Vital.Request) => Option.Option<PerformanceResourceTiming>
  readonly grade: (kind: Vital.Kind, value: number) => Vital.Grade
  readonly live: (policy: Vital.Policy) => Layer.Layer<_Report>
  readonly rows: typeof _rows
  readonly stream: Stream.Stream<Vital.Fact>
} = {
  Fact: _Fact,
  Policy: _Policy,
  Report: _Report,
  enrich: _enrich,
  grade: _grade,
  live: (policy) =>
    Layer.scoped(
      _Report,
      Effect.gen(function* () {
        const intake = yield* Queue.sliding<Vital.Fact>(_FLOW.buffer)
        yield* Effect.forkScoped(
          Stream.runForEach(
            Stream.merge(_watched, Stream.fromQueue(intake)).pipe(
              Stream.mapAccum(HashMap.empty<Vital.Kind, _Cell>(), _accounted),
              Stream.changesWith((prior, next) => prior.kind === next.kind && prior.value === next.value),
              Stream.throttle({ cost: () => 1, duration: policy.settle, strategy: "shape", units: policy.pulse }),
            ),
            _drained,
          ),
        )
        return (fact: Vital.Fact) => Queue.offer(intake, fact).pipe(Effect.asVoid)
      }),
    ),
  rows: _rows,
  stream: _observed,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Vital }
```

## [05]-[RESEARCH]

(none)
