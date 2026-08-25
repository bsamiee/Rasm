# [RUNTIME_VITAL]

Browser RUM is one vital-kind table, one capture bracket, and one graded emission: `web-vitals` measures the Core Web Vitals family whole — session-windowed CLS, interaction-grouped INP, input-finalized LCP, activation-corrected TTFB, FCP — and every kind it leaves enters the same table through a raw Performance-Timeline row or the app-composed report intake. Grading rides the cutoff pairs the library ships, so a budget edit moves the grade fold, both instruments, and every dashboard panel at once.

Capture belongs to the browser condition and the table does not — every platform touch sits inside a body `Vital.live` reaches, so a process plane folding `Vital.rows` opens no observer. This owner holds every Core Web Vital in the estate: the ui plane measures none, mints no instrument, and reaches these two through the `Vital.Report` intake. `Vital.rows` is the budget table `otel/meter#BOARD` folds into the deploy feed, `Vital.enrich` is the projection `browser/fetch` lays over its dial spans, and `Convention` owns every name stamped here. Its module is `runtime/src/otel/vital.ts`.

## [01]-[INDEX]

- [02]-[BUDGETS]: `_rows` fixes every vital kind — capture source, shipped cutoff pair, accumulation column, accrual flag, UCUM unit.
- [03]-[CAPTURE]: one bracket registers three capture sources and projects each arrival into one sample shape.
- [04]-[CONTEXT]: `_context` resolves document RUM identity, and `Vital.enrich` projects resource timing onto a caller's span.
- [05]-[EMISSION]: two bounded instruments, one accounting ledger, one drain Layer, one report intake.

## [02]-[BUDGETS]

[BUDGETS]:
- Owner: the interior `_rows` anchor — one row per vital kind carrying `source` (which capture path mints it: `library` for a `web-vitals` registrar, `entry` for a Performance-Timeline entry type, `report` for the app-composed intake), the source's own capture column (`on` the registrar a library row calls, `entry` the entry type an entry row joins beside the `read` projecting a raw entry to its measure), `fold` (`level` where the producer already accounted the value, `crest` where the worst sample stands), `accrues` (whether the producer's `delta` is a per-report increment), `good`/`poor` (the cutoff pair the grade fold reads), and `unit` (the UCUM code selecting the kind's level instrument, which `otel/meter#BOARD` carries onto its panel).
- Law: one OTLP metric name carries one descriptor unit, so the level family spans one `Convention._instrument` row per code.
- Law: `_LEVELS` keys those rows by code, so a kind's unit selects its series and refuses when no series receives it.
- Law: a dimensionless gauge carrying milliseconds is the wire defect that split forecloses.
- Law: `web-vitals` owns the Core Web Vitals cutoffs — every web-family row projects its budget from the shipped `*Thresholds` pair, so a standard revision arrives with a version bump rather than a hand edit, and the render rows project through the same `_budget` shape so a render budget is the same row edit as a web budget: `frame` against the 60/30 fps frame budget, `gpumem` against the byte-budget peak, `capture` against the binary capture-hash verdict whose `[0, 0]` pair grades a match good and any mismatch poor.
- Law: the grade vocabulary is the library's own `good`/`needs-improvement`/`poor` triple — a library fact carries the `rating` the package computed against the same pair and this module never re-buckets it, while an entry or report fact grades through `_grade` off the row's pair; one vocabulary reaches `Convention.rasm.vitalGrade`, the ui evidence tone table, and the `otel/meter#BOARD` burn slice.
- Law: `accrues` reads whether the producer's `delta` is a per-report INCREMENT or a re-read of one level, never whether the fold crests — the library's `delta` chains across the instances a bfcache restore mints, a long-task increment totals main-thread occupancy, and a capture verdict totals mismatches, each beside its own fold, while a re-read level (the frame span, the gpu-memory peak) accrues nothing because summing one sampled level measures nothing; a non-accruing kind therefore carries NO session total rather than a zero one.
- Law: the kind union, the grade union, the navigation union, the fold union, and the report-only kind subset all derive — `keyof typeof _rows` against the `_KINDS` key tuple, the `_GRADES` and `_NAVIGATIONS` tuples, the row's `fold` column, and a mapped filter over the `source` column — so the intake refuses a library-owned kind at the type level rather than at a runtime check.
- Growth: a new vital is one row — its capture source selects the registration arm and supplies that arm's own column, its fold selects the accounting arm, and the grade, both instruments, the deploy-feed budget, and every board panel follow; a new accumulation semantic is one `_folds` arm the column selects.

```typescript
import { Array, Chunk, Context, Effect, HashMap, Layer, Metric, Number, Option, ParseResult, PubSub, Queue, Record, Schema, Stream, pipe } from "effect"
import type { HrTime, Span } from "@opentelemetry/api"
import { addSpanNetworkEvents, getElementXPath, getResource, normalizeUrl } from "@opentelemetry/sdk-trace-web"
import { Convention } from "@rasm/core"
import {
  CLSThresholds,
  FCPThresholds,
  INPThresholds,
  LCPThresholds,
  TTFBThresholds,
  type INPAttributionReportOpts,
  type MetricRatingThresholds,
  type MetricWithAttribution,
  onCLS,
  onFCP,
  onINP,
  onLCP,
  onTTFB,
} from "web-vitals/attribution"

const _KINDS = ["capture", "cls", "fcp", "frame", "gpumem", "inp", "lcp", "longtask", "ttfb"] as const
const _GRADES = ["good", "needs-improvement", "poor"] as const
const _NAVIGATIONS = ["navigate", "reload", "back-forward", "back-forward-cache", "prerender", "restore", "soft-navigation"] as const

const _budget = (pair: MetricRatingThresholds): { readonly good: number; readonly poor: number } => ({ good: pair[0], poor: pair[1] })

const _rows = {
  capture: { ..._budget([0, 0]), accrues: true, fold: "level", source: "report", unit: "1" },
  cls: { ..._budget(CLSThresholds), accrues: true, fold: "level", on: onCLS, source: "library", unit: "1" },
  fcp: { ..._budget(FCPThresholds), accrues: true, fold: "level", on: onFCP, source: "library", unit: "ms" },
  frame: { ..._budget([17, 33]), accrues: false, fold: "crest", source: "report", unit: "ms" },
  gpumem: { ..._budget([536_870_912, 1_073_741_824]), accrues: false, fold: "crest", source: "report", unit: "By" },
  inp: { ..._budget(INPThresholds), accrues: true, fold: "level", on: onINP, source: "library", unit: "ms" },
  lcp: { ..._budget(LCPThresholds), accrues: true, fold: "level", on: onLCP, source: "library", unit: "ms" },
  longtask: { ..._budget([50, 200]), accrues: true, entry: "longtask", fold: "crest", read: (entry: PerformanceEntry) => entry.duration, source: "entry", unit: "ms" },
  ttfb: { ..._budget(TTFBThresholds), accrues: true, fold: "level", on: onTTFB, source: "library", unit: "ms" },
} as const satisfies Record<(typeof _KINDS)[number], Vital.Row>

declare namespace Vital {
  type Fold = (typeof _rows)[Kind]["fold"]
  type Grade = (typeof _GRADES)[number]
  type Kind = keyof typeof _rows
  type Level = keyof typeof _LEVELS
  type Navigation = (typeof _NAVIGATIONS)[number]
  type Reported = { readonly [K in Kind]: (typeof _rows)[K]["source"] extends "report" ? K : never }[Kind]
  type Registrar = (report: (metric: MetricWithAttribution) => void, opts: INPAttributionReportOpts) => void
  type Row =
    & { readonly accrues: boolean; readonly good: number; readonly poor: number; readonly unit: Level }
    & (
      | { readonly fold: "level"; readonly on: Registrar; readonly source: "library" }
      | { readonly entry: string; readonly fold: "crest" | "level"; readonly read: (entry: PerformanceEntry) => number; readonly source: "entry" }
      | { readonly fold: "crest" | "level"; readonly source: "report" }
    )
  type Fact = _Fact
  type Policy = _Policy
  type Session = _Session
  type _Keys<K extends (typeof _KINDS)[number] = Kind> = K
}

const _grade = (kind: Vital.Kind, value: number): Vital.Grade =>
  value <= _rows[kind].good ? "good" : value <= _rows[kind].poor ? "needs-improvement" : "poor"
```

## [03]-[CAPTURE]

[CAPTURE]:
- Owner: `_watched(policy, navigation)` — one `Stream.asyncScoped` whose acquisition walks `_rows` once and dispatches on `source`: a `library` row calls its own registrar with the shared opt bag, an `entry` row joins the single `PerformanceObserver` when `PerformanceObserver.supportedEntryTypes` carries it, and a `report` row registers nothing because the intake feeds it; release disconnects the observer and closes the emission gate.
- Law: `web-vitals` registrations are page-lifetime and idempotent — the package exposes no unregister, so the release arm closes a gate the emitter reads instead of pretending to tear the callback down, and a late callback discards rather than emitting into a dead stream; the raw observer releases properly because `disconnect()` exists.
- Law: the registrars own their own entry families — `web-vitals` observes `layout-shift`, `paint`, `largest-contentful-paint`, `event`, `long-animation-frame`, and `soft-navigation` on this owner's behalf, so an `entry` row spells only what remains after those six and re-observing one of them here mints the second accounting this table exists to prevent; the display plane windowing `event` and `long-animation-frame` forks nothing because it grades nothing and mints no instrument, and the one value both readers share is `policy.interaction` — a second `event-timing` floor lets a graded interaction name an event the display window never received.
- Law: `buffered: true` replays entries recorded before the observer attached, and an entry family the platform withholds degrades by absent data instead of defecting acquisition — the library applies the same discipline per vital, so a browser missing an entry family reports the vitals it can.
- Law: report cadence, interaction floor, and soft-navigation reporting are one policy row applied to every registrar — `stream` selects streaming versus terminal reporting, `interaction` floors the `event-timing` stream the INP estimator consumes, and `soft` arms soft-navigation re-reporting so a client-routed view change re-reports against its own navigation identity; a per-vital opt bag is the named defect.
- Law: attribution rides the enriched build and lands as the fact's causal decomposition, both halves — `_phases` projects each metric's numeric subparts (INP input delay, processing, presentation, longest intersecting script, and the four LoAF-derived script/style-and-layout/paint/unattributed totals; LCP byte, resource-start, resource-load, and element-render delays; TTFB cache, dns, connection, request, and waiting durations; FCP byte and render halves; CLS largest shift) beside the discrete subject record naming WHAT the value fell on (the attributed element, the LCP resource, the interaction's input class, the document load state, and the worst script's own source, function, invoker, and the subpart it ran in), so a poor grade answers which phase spent the budget, which element spent it, and which script code path did the spending; the enriched build re-exports the standard registrars and cutoff pairs unchanged, so the choice costs one module specifier and widens no signature.
- Law: the attribution union narrows per arm — a metric's subpart roster correlates with its own `name`, so one exhaustive switch is the only form that reads it without erasing the correlation, and a shared indexed reader over the union types every roster as every other's.
- Law: each arm keys its causal records on `Convention._rasm` rows, so the drain spreads them and no mapping layer exists.
- Law: an undispatched subpart omits its key — `_present` is the one omission projection every optional-bearing arm returns through, so an absent CLS shift entry, an element gone from the DOM before the report, and an INP total no long frame intersects each drop their key instead of carrying a fabricated zero or an empty string, while an arm whose whole roster is unconditional states that by spelling its record directly.
- Law: interaction targets spell through `getElementXPath` and LAND — `generateTarget` answers the same XPath the DOM-event instrumentation rows stamp and the arm's `element` subject carries it onto the fact, so an attributed interaction and the span its click opened name one element on one trace; a target the registrar computes and the fact drops pays the selector cost for nothing.
- Exemption: the registrar and observer callbacks are the platform-forced statement seam — emissions are `void`-discarded inside them; the sample projections beside them are the marked admission kernel for the untyped `PerformanceEntry` records and the library's own `Metric` union.
- Growth: a new library-covered vital is one `_LIBRARY` row beside its `_rows` row; a new entry family is one `_rows` entry row carrying its own `entry` type and `read`, which `_ENTERED` indexes and the observer callback resolves with no arm edit.

```typescript
type _Hints = { readonly model?: string; readonly platformVersion?: string }

type _Parts = Readonly<Record<string, number>>

type _Subject = Readonly<Record<string, string>>

type _Causal = { readonly phases: _Parts; readonly subject: _Subject }

type _Sample = {
  readonly at: number
  readonly delta: number
  readonly instance: string
  readonly kind: Vital.Kind
  readonly navigation: Vital.Navigation
  readonly phases: _Parts
  readonly rated: Option.Option<Vital.Grade>
  readonly subject: _Subject
  readonly value: number
}

const _present = <A>(row: Readonly<Record<string, A | undefined>>): Readonly<Record<string, A>> =>
  Record.filterMap(row, Option.fromNullable)

const _LIBRARY = { CLS: "cls", FCP: "fcp", INP: "inp", LCP: "lcp", TTFB: "ttfb" } as const satisfies Record<MetricWithAttribution["name"], Vital.Kind>

const _NAVIGATED = { back_forward: "back-forward", navigate: "navigate", reload: "reload" } as const satisfies Record<NavigationTimingType, Vital.Navigation>

const _navigation = (): Vital.Navigation =>
  Option.match(Option.fromNullable(performance.getEntriesByType("navigation")[0]), {
    onNone: () => "navigate",
    onSome: (entry) => _NAVIGATED[entry.type],
  })

const _phases = (metric: MetricWithAttribution): _Causal => {
  switch (metric.name) {
    case "CLS":
      return {
        phases: _present({ [Convention.rasm.vitalPhaseLargestShift]: metric.attribution.largestShiftValue }),
        subject: _present({
          [Convention.rasm.vitalElement]: metric.attribution.largestShiftTarget,
          [Convention.rasm.vitalState]: metric.attribution.loadState,
        }),
      }
    case "FCP":
      return {
        phases: {
          [Convention.rasm.vitalPhaseFirstByte]: metric.attribution.timeToFirstByte,
          [Convention.rasm.vitalPhaseRender]: metric.attribution.firstByteToFCP,
        },
        subject: { [Convention.rasm.vitalState]: metric.attribution.loadState },
      }
    case "INP":
      return {
        phases: _present({
          [Convention.rasm.vitalPhaseInput]: metric.attribution.inputDelay,
          [Convention.rasm.vitalPhasePaint]: metric.attribution.totalPaintDuration,
          [Convention.rasm.vitalPhasePresentation]: metric.attribution.presentationDelay,
          [Convention.rasm.vitalPhaseProcessing]: metric.attribution.processingDuration,
          [Convention.rasm.vitalPhaseScript]: metric.attribution.totalScriptDuration,
          [Convention.rasm.vitalPhaseLongestScript]: metric.attribution.longestScript?.intersectingDuration,
          [Convention.rasm.vitalPhaseStyleAndLayout]: metric.attribution.totalStyleAndLayoutDuration,
          [Convention.rasm.vitalPhaseUnattributed]: metric.attribution.totalUnattributedDuration,
        }),
        subject: _present({
          [Convention.rasm.vitalElement]: metric.attribution.interactionTarget,
          [Convention.rasm.vitalInteraction]: metric.attribution.interactionType,
          [Convention.rasm.vitalScriptInvoker]: metric.attribution.longestScript?.entry.invokerType,
          [Convention.rasm.vitalScript]: metric.attribution.longestScript?.entry.sourceURL,
          [Convention.rasm.vitalScriptFunction]: metric.attribution.longestScript?.entry.sourceFunctionName,
          [Convention.rasm.vitalScriptPart]: metric.attribution.longestScript?.subpart,
          [Convention.rasm.vitalState]: metric.attribution.loadState,
        }),
      }
    case "LCP":
      return {
        phases: {
          [Convention.rasm.vitalPhaseElementRender]: metric.attribution.elementRenderDelay,
          [Convention.rasm.vitalPhaseFirstByte]: metric.attribution.timeToFirstByte,
          [Convention.rasm.vitalPhaseResourceLoad]: metric.attribution.resourceLoadDuration,
          [Convention.rasm.vitalPhaseResourceStart]: metric.attribution.resourceLoadDelay,
        },
        subject: _present({
          [Convention.rasm.vitalElement]: metric.attribution.target,
          [Convention.rasm.vitalResource]: metric.attribution.url,
        }),
      }
    case "TTFB":
      return {
        phases: {
          [Convention.rasm.vitalPhaseCache]: metric.attribution.cacheDuration,
          [Convention.rasm.vitalPhaseConnection]: metric.attribution.connectionDuration,
          [Convention.rasm.vitalPhaseDns]: metric.attribution.dnsDuration,
          [Convention.rasm.vitalPhaseRequest]: metric.attribution.requestDuration,
          [Convention.rasm.vitalPhaseWaiting]: metric.attribution.waitingDuration,
        },
        subject: {},
      }
  }
}

const _reported = (metric: MetricWithAttribution): _Sample =>
  pipe(_phases(metric), (causal) => ({
    at: performance.now(),
    delta: metric.delta,
    instance: metric.id,
    kind: _LIBRARY[metric.name],
    navigation: metric.navigationType,
    phases: causal.phases,
    rated: Option.some(metric.rating),
    subject: causal.subject,
    value: metric.value,
  }))

const _ENTERED: Readonly<Record<string, { readonly kind: Vital.Kind; readonly read: (entry: PerformanceEntry) => number }>> = pipe(
  Record.toEntries(_rows),
  Array.filterMap(([kind, row]) => (row.source === "entry" ? Option.some([row.entry, { kind, read: row.read }] as const) : Option.none())),
  Record.fromEntries,
)

const _entered = (entry: PerformanceEntry, session: string, navigation: Vital.Navigation): Option.Option<_Sample> =>
  Option.map(Record.get(_ENTERED, entry.entryType), (row) =>
    pipe(row.read(entry), (value) => ({
      at: entry.startTime,
      delta: value,
      instance: session,
      kind: row.kind,
      navigation,
      phases: {},
      rated: Option.none(),
      subject: {},
      value,
    })))

const _watched = (policy: Vital.Policy, navigation: Vital.Navigation): Stream.Stream<_Sample> =>
  Stream.asyncScoped<_Sample>(
    (emit) =>
      Effect.acquireRelease(
        Effect.sync(() => {
          const gate = { open: true }
          const push = (sample: _Sample): void => void (gate.open && emit.single(sample))
          const report = (metric: MetricWithAttribution): void => push(_reported(metric))
          const opts: INPAttributionReportOpts = {
            durationThreshold: policy.interaction,
            generateTarget: (node) => (node === null ? undefined : getElementXPath(node, true)),
            reportAllChanges: policy.stream,
            reportSoftNavs: policy.soft,
          }
          const observer = new PerformanceObserver((list) => {
            for (const entry of list.getEntries()) {
              Option.match(_entered(entry, policy.session.id, navigation), { onNone: () => undefined, onSome: push })
            }
          })
          const supported = new Set(PerformanceObserver.supportedEntryTypes)
          for (const row of Record.values(_rows)) {
            if (row.source === "library") row.on(report, opts)
            else if (row.source === "entry" && supported.has(row.entry)) observer.observe({ buffered: true, type: row.entry })
          }
          return { gate, observer }
        }),
        ({ gate, observer }) =>
          Effect.sync(() => {
            gate.open = false
            observer.disconnect()
          }),
      ),
    { bufferSize: policy.window, strategy: "sliding" },
  )
```

## [04]-[CONTEXT]

[CONTEXT]:
- Owner: `_context(session)` — the document RUM context resolved once at Layer construction into a `Convention.Attributes` record: browser brands, mobility, platform, and language; the device model behind the high-entropy client hint; the network connection type; and the session pair the composition root supplies.
- Law: `Convention.ValueOf` binds the connection key to the bounded spec union, so a raw browser word fails the stamp.
- Law: `_CONNECTED` maps the four Network Information API transports the spec rows name, at this one site.
- Law: every remaining browser answer folds onto the family's unknown row, so a new transport degrades rather than refuses.
- Law: this owner pins the `connection.type` refinement because dependency direction bars it from reading `browser/boot#SIGNAL_CELLS`'s profile cell — browser composes otel, never the reverse — so the transport WORD stamps here while the byte-budget axis (`effectiveType`, `saveData`) stays that cell's alone; one nonstandard surface, two refinements, one owner each.
- Law: session identity arrives as policy, never mints here — the crash signal and any app analytics fold join on the same id, so a second mint inside this module forks the correlation key; `previous` carries the prior session so a soft navigation or a resumed visit chains, and an absent dimension is omitted rather than sentinelled so a backend filter never matches an empty string.
- Law: the RUM context stamps spans, never metric tags — brands are an array and model, platform, and session are identifier-grade, so the whole record rides the evidence span while the two instruments keep exactly the kind and grade dimensions their cardinality budget admits.
- Law: `Navigator` members no shipped lib declares enter through one marked global augmentation beside the entry kernel — the same discipline `web-vitals` applies to the performance globals — and the high-entropy hint call answers an empty hint set when the browser refuses, so a permission denial degrades the context rather than the Layer.
- Law: the package's performance augmentation is composed, never re-guarded — its types build keys `Performance.getEntriesByType` by entry name, so the navigation and resource buffers arrive as their concrete entry types and a narrowing predicate over them is a hand-rolled guard that copies the buffer and admits every element it tested.
- Boundary: URL-bearing span enrichment composes through `Vital.enrich(span, request)` — `normalizeUrl` fixes the lookup identity, `getResource` selects the nearest unused main and preflight timing pair inside the supplied span range, and `addSpanNetworkEvents` projects both onto the caller-owned dial span under the new content-length semconv alone, dropping zeroed phases so an unused phase never reads as a measured zero. This bridge never opens a span, so `browser/fetch` keeps request ownership while this module owns the Performance-Timeline projection; the fetch instrumentation row leaves `clearTimingResources` off so the buffer this reads survives.
- Growth: a new context dimension is one projection line beside its `Convention` row.

```typescript
declare global {
  interface Navigator {
    readonly connection?: { readonly type?: string }
    readonly userAgentData?: {
      readonly brands: ReadonlyArray<{ readonly brand: string; readonly version: string }>
      readonly getHighEntropyValues: (hints: ReadonlyArray<string>) => Promise<_Hints>
      readonly mobile: boolean
      readonly platform: string
    }
  }
}

declare namespace Vital {
  type Request = {
    readonly url: string
    readonly start: HrTime
    readonly end: HrTime
    readonly initiator: Option.Option<string>
    readonly used: WeakSet<PerformanceResourceTiming>
  }
}

const _HINTS = ["model", "platformVersion"] as const

const _CONNECTED: Readonly<Record<string, Convention.ConnectionType>> = {
  cellular: Convention.value.connectionCell,
  ethernet: Convention.value.connectionWired,
  none: Convention.value.connectionUnavailable,
  wifi: Convention.value.connectionWifi,
}

const _connected = (transport: string): Convention.ConnectionType =>
  Option.getOrElse(Record.get(_CONNECTED, transport), () => Convention.value.connectionUnknown)

const _context = (session: Vital.Session): Effect.Effect<Convention.Attributes> =>
  Effect.map(
    Effect.orElseSucceed(
      Effect.tryPromise(() => globalThis.navigator.userAgentData?.getHighEntropyValues([..._HINTS]) ?? Promise.resolve<_Hints>({})),
      (): _Hints => ({}),
    ),
    (hints) => ({
      [Convention.incubating.browserLanguage]: globalThis.navigator.language,
      [Convention.incubating.sessionId]: session.id,
      ...Option.match(Option.fromNullable(globalThis.navigator.userAgentData), {
        onNone: () => ({}),
        onSome: (agent) => ({
          [Convention.incubating.browserBrands]: agent.brands.map((row) => `${row.brand} ${row.version}`),
          [Convention.incubating.browserMobile]: agent.mobile,
          [Convention.incubating.browserPlatform]: agent.platform,
        }),
      }),
      ...Option.match(Option.fromNullable(hints.model), {
        onNone: () => ({}),
        onSome: (model) => ({ [Convention.incubating.deviceModel]: model }),
      }),
      ...Option.match(Option.fromNullable(globalThis.navigator.connection?.type), {
        onNone: () => ({}),
        onSome: (transport) => ({ [Convention.incubating.connectionType]: _connected(transport) }),
      }),
      ...Option.match(session.previous, {
        onNone: () => ({}),
        onSome: (previous) => ({ [Convention.incubating.sessionPrevious]: previous }),
      }),
    }),
  )

const _enrich = (span: Span, request: Vital.Request): Option.Option<PerformanceResourceTiming> => {
  const resources = performance.getEntriesByType("resource")
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
    addSpanNetworkEvents(span, entry, false, true, true)
    return entry
  }
  Option.match(Option.fromNullable(timing.corsPreFlightRequest), { onNone: () => undefined, onSome: attach })
  return Option.map(Option.fromNullable(timing.mainRequest), attach)
}
```

## [05]-[EMISSION]

[EMISSION]:
- Owner: the assembled `Vital` export — the row table spread in, the grade fold, the level projection, the accounting ledger, the instruments, the report intake, and the drain Layer under one name; every instrument materializes through `Convention.mount`, so name, description, UCUM code, and wire form arrive from the row and no signal-site literal exists here.
- Law: the level plane is one level series per UCUM code, each written through `Metric.set` and tagged by kind.
- Law: `vitalObserved` is one incremental counter tagged by kind and grade, so the observation fan is the kind-grade product.
- Law: the exact-value distribution beyond a level gauge is an export-lane concern, never a third instrument here.
- Law: `Vital.level(kind)` is the one kind-to-level-series projection, so `otel/meter#BOARD` never re-derives a metric name.
- Law: `Vital.levels` carries the level-name tuple, so the board schema decodes to the closed union rather than a bare string.
- Law: the accounting fold is one `mapAccum` over a per-kind cell ledger holding the accounted level, the metric instance, and the whole-session delta total, and it suppresses exactly one thing — a crest sample that failed to rise — because a level sample IS the producer's own new accounting and dropping it on an equal value strands the element, phase split, and delta riding that report; suppression is per kind rather than per adjacent pair so interleaved kinds never mask each other, a restore-minted instance always emits and resets the crest while the session total keeps accumulating across instances, and a non-accruing kind projects no total at all because a zero one reads as a measured session.
- Law: `Vital.live(policy)` is the one registration node — a scoped Layer resolving the context, minting the intake queue and the replay hub, and forking one drain: the capture stream merged with the intake, the accounting fold, and `Stream.throttle` shaping the stamp rate through `settle` and `pulse`, weighing each chunk by its own size so a layout-shift burst and a whole intake drain both pay their real cost; every surviving fact publishes to the hub and drains into both instruments, and the Layer merges at the browser composition root beside `Export.live`.
- Law: one document runs one capture — `Vital.Report` carries both the intake and the accounted fact stream, so a consumer folding its own view subscribes the same registration rather than minting a second capture, and the intake admits exactly the report-source kinds because a library-owned kind arriving from an app tap double-counts the accounting the library already performed; the sample VALUE admits at that same intake against the fact's own constraint and refuses on the typed parse rail, since the accounted fold constructs its fact inside a stream accumulator where a malformed number raises and kills the capture whole.
- Law: render-vital emission is this module's — the ui component floor and viewer probe stay display surfaces and mint no instrument, and `ui:viewer/probe`'s render rows (per-frame render time, the gpu-memory peak, capture-hash verdicts) reach these instruments through the `Vital.Report` tap an app composition root composes over `ui:system/hook`'s `rasm.ui.vital.row` point; label vocabularies stay surface-local, so the tap resolves each row onto its `Vital.Reported` kind and supplies the producer's own phases and subject, and a render kind then answers the same phase and element questions the library answers for a web kind.
- Law: every report row carries a producing surface — the carrier set is closed at `Vital.Reported`, so a kind added here without a row minting it is a type admitting a wire nobody sends.
- Receipt: each throttled fact opens one short root evidence span carrying the document RUM context beside the kind and grade rows, so the trace signal is inhabited and exemplar click-through lands on the counter's own coordinate; the span roots explicitly because the drain fiber owns no ambient request span and an inherited parent attributes a page-lifecycle fact to whatever call opened last.
- Law: the evidence span carries the fact WHOLE — accounting identity, phase decomposition, and subject record ride it beside the context.
- Law: the two instruments hold exactly the kind and grade dimensions their cardinality budget admits; every other axis is span-only.
- Law: the phase plane's one reader is that span, so a `Vital.Report.facts` consumer folding phases duplicates a landed signal.
- Entry: `Vital.live(policy)` at the browser composition root; `Vital.Report` for the render tap and the accounted fact stream; `Vital.rows` beside `Vital.grade(kind, value)` for the deploy-feed budget fold — the table and its fold travel as one pair, because a holder of the cutoffs alone re-buckets them and that is the forked-standard defect this owner exists to foreclose.
- Growth: an instrument axis is closed — new analysis lands as board queries over the same two instruments.
- Packages: `effect` (`Array`, `Chunk`, `Context`, `Effect`, `HashMap`, `Layer`, `Metric`, `Number`, `Option`, `PubSub`, `Queue`, `Record`, `Schema`, `Stream`, `pipe`); `web-vitals/attribution` (the five registrars, the five cutoff pairs, the five attribution shapes through `MetricWithAttribution`, `MetricRatingThresholds`, `INPAttributionReportOpts`, and the performance-global types build); `@opentelemetry/sdk-trace-web` (`normalizeUrl`, `getResource`, `addSpanNetworkEvents`, `getElementXPath`); `@rasm/core` (`Convention`).

```typescript
class _Session extends Schema.Class<_Session>("Vital/Session")({
  id: Schema.NonEmptyString,
  previous: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

class _Fact extends Schema.Class<_Fact>("Vital/Fact")({
  at: Schema.Number.pipe(Schema.finite(), Schema.nonNegative()),
  delta: Schema.Number.pipe(Schema.finite()),
  grade: Schema.Literal(..._GRADES),
  instance: Schema.NonEmptyString,
  kind: Schema.Literal(..._KINDS),
  navigation: Schema.Literal(..._NAVIGATIONS),
  phases: Schema.Record({ key: Schema.String, value: Schema.Number }),
  session: Schema.optionalWith(Schema.Number.pipe(Schema.finite(), Schema.nonNegative()), { as: "Option" }),
  subject: Schema.Record({ key: Schema.String, value: Schema.String }),
  value: Schema.Number.pipe(Schema.finite(), Schema.nonNegative()),
}) {}

class _Policy extends Schema.Class<_Policy>("Vital/Policy")({
  interaction: Schema.Int.pipe(Schema.nonNegative()),
  pulse: Schema.Int.pipe(Schema.positive()),
  session: _Session,
  settle: Schema.Duration,
  soft: Schema.Boolean,
  stream: Schema.Boolean,
  window: Schema.Int.pipe(Schema.positive()),
}) {}

type _Cell = { readonly held: number; readonly instance: string; readonly session: number }

const _EMPTY: _Cell = { held: 0, instance: "", session: 0 }

const _folds = {
  crest: (held: number, value: number) => Number.max(held, value),
  level: (_held: number, value: number) => value,
} as const satisfies Record<Vital.Fold, (held: number, value: number) => number>

const _accounted = (
  ledger: HashMap.HashMap<Vital.Kind, _Cell>,
  sample: _Sample,
): readonly [HashMap.HashMap<Vital.Kind, _Cell>, Option.Option<Vital.Fact>] => {
  const prior = Option.getOrElse(HashMap.get(ledger, sample.kind), () => _EMPTY)
  const row = _rows[sample.kind]
  const restored = prior.instance !== "" && prior.instance !== sample.instance
  const held = _folds[row.fold](restored ? 0 : prior.held, sample.value)
  const grade = Option.getOrElse(sample.rated, () => _grade(sample.kind, held))
  const cell: _Cell = { held, instance: sample.instance, session: prior.session + (row.accrues ? sample.delta : 0) }
  const advanced = prior.instance === "" || restored || row.fold !== "crest" || held > prior.held
  return [
    HashMap.set(ledger, sample.kind, cell),
    advanced
      ? Option.some(
          new _Fact({
            at: sample.at,
            delta: sample.delta,
            grade,
            instance: sample.instance,
            kind: sample.kind,
            navigation: sample.navigation,
            phases: sample.phases,
            session: row.accrues ? Option.some(cell.session) : Option.none(),
            subject: sample.subject,
            value: held,
          }),
        )
      : Option.none(),
  ]
}

const _LEVELS = {
  [Convention.instrument.vitalDuration.unit]: Convention.instrument.vitalDuration,
  [Convention.instrument.vitalScore.unit]: Convention.instrument.vitalScore,
  [Convention.instrument.vitalSize.unit]: Convention.instrument.vitalSize,
} as const

const _LEVEL_NAMES = [
  Convention.instrument.vitalDuration.name,
  Convention.instrument.vitalScore.name,
  Convention.instrument.vitalSize.name,
] as const

const _level = Record.map(_LEVELS, (row) => Convention.mount(row.name))

const _observed = Convention.mount(Convention.metric.vitalObserved)

class _Report extends Context.Tag("Vital/Report")<_Report, {
  readonly facts: Stream.Stream<Vital.Fact>
  readonly report: (
    sample: { readonly kind: Vital.Reported; readonly phases?: _Parts; readonly subject?: _Subject; readonly value: number },
  ) => Effect.Effect<void, ParseResult.ParseError>
}>() {}

const _drained = (context: Convention.Attributes, fact: Vital.Fact): Effect.Effect<void> =>
  Effect.all(
    [
      Metric.set(Metric.tagged(_level[_rows[fact.kind].unit], Convention.rasm.vitalKind, fact.kind), fact.value),
      Metric.increment(
        Metric.tagged(Metric.tagged(_observed, Convention.rasm.vitalKind, fact.kind), Convention.rasm.vitalGrade, fact.grade),
      ),
    ],
    { discard: true },
  ).pipe(
    Effect.withSpan(Convention.metric.vitalObserved, {
      attributes: {
        ...context,
        ...fact.phases,
        ...fact.subject,
        [Convention.rasm.vitalDelta]: fact.delta,
        [Convention.rasm.vitalGrade]: fact.grade,
        [Convention.rasm.vitalInstance]: fact.instance,
        [Convention.rasm.vitalKind]: fact.kind,
        [Convention.rasm.vitalNavigation]: fact.navigation,
        ...Option.match(fact.session, {
          onNone: () => ({}),
          onSome: (session) => ({ [Convention.rasm.vitalSession]: session }),
        }),
      },
      root: true,
    }),
  )

const Vital: {
  readonly Fact: typeof _Fact
  readonly Policy: typeof _Policy
  readonly Report: typeof _Report
  readonly Session: typeof _Session
  readonly enrich: (span: Span, request: Vital.Request) => Option.Option<PerformanceResourceTiming>
  readonly grade: (kind: Vital.Kind, value: number) => Vital.Grade
  readonly level: (kind: Vital.Kind) => Convention.MetricName<"gauge">
  readonly levels: typeof _LEVEL_NAMES
  readonly live: (policy: Vital.Policy) => Layer.Layer<_Report>
  readonly rows: typeof _rows
} = {
  Fact: _Fact,
  Policy: _Policy,
  Report: _Report,
  Session: _Session,
  enrich: _enrich,
  grade: _grade,
  level: (kind) => _LEVELS[_rows[kind].unit].name,
  levels: _LEVEL_NAMES,
  live: (policy) =>
    Layer.scoped(
      _Report,
      Effect.gen(function* () {
        const context = yield* _context(policy.session)
        const navigation = _navigation()
        const intake = yield* Queue.sliding<_Sample>(policy.window)
        const hub = yield* PubSub.sliding<Vital.Fact>({ capacity: policy.window, replay: policy.window })
        yield* Effect.forkScoped(
          Stream.runForEach(
            Stream.merge(_watched(policy, navigation), Stream.fromQueue(intake)).pipe(
              Stream.mapAccum(HashMap.empty<Vital.Kind, _Cell>(), _accounted),
              Stream.filterMap((accounted) => accounted),
              Stream.throttle({ cost: Chunk.size, duration: policy.settle, strategy: "shape", units: policy.pulse }),
            ),
            (fact) => Effect.zipRight(PubSub.publish(hub, fact), _drained(context, fact)),
          ),
        )
        return {
          facts: Stream.fromPubSub(hub),
          report: (sample) =>
            pipe(
              Schema.decodeUnknown(_Fact.fields.value)(sample.value),
              Effect.flatMap((value) =>
                Queue.offer(intake, {
                  at: performance.now(),
                  delta: value,
                  instance: policy.session.id,
                  kind: sample.kind,
                  navigation,
                  phases: sample.phases ?? {},
                  rated: Option.none(),
                  subject: sample.subject ?? {},
                  value,
                })),
              Effect.asVoid,
            ),
        }
      }),
    ),
  rows: _rows,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Vital }
```

## [06]-[RESEARCH]

(none)
