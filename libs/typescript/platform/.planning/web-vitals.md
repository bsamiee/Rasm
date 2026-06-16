# [PLATFORM_WEB_VITALS]

One page owns the browser performance-budget and web-vitals observability concern — `PerformanceBudget`, the Core Web Vitals (LCP/INP/CLS/TTFB/FCP) capture over one `Schema.Literal` vital-metric axis folded into the `platform` `platform-substrate` `MetricRegistry` Core-Web-Vitals instrument rows, the data-driven `BudgetThreshold` `Record` gating a budget-exceeded fold, the long-task and resource-timing `PerformanceObserver` marshalled into Effect `Stream` signals, and the budget-breach span shipped as a `MetricRegistry.span` over the `SelfTelemetry` collector edge. `MetricRegistry` already declares the Core Web Vitals instrument family and owns the `span` export; this page owns the CAPTURE and budget-gating mechanics, not a parallel metric construction. Capture is native `PerformanceObserver` only — ZERO `web-vitals` package, a deliberate no-admission. The page composes the existing `MetricRegistry` edge, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                        |
| :-----: | :--------- | :------------------------------------------------------------ |
|   [1]   | WEB_VITALS | the vital capture, the budget thresholds, and the breach fold |

## [2]-[WEB_VITALS]

- Owner: `PerformanceBudget`, the single web-vitals capture and budget owner — the vital capture over the closed `VitalKind` `Data.TaggedEnum` performance-signal axis feeding the existing `MetricRegistry` Core-Web-Vitals instrument rows, the `BudgetThreshold` data-driven `Record` per budgeted vital, the long-task/resource-timing `PerformanceObserver` `Stream` ingress folded onto the SAME `VitalKind` surface, and the budget-breach `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` edge; `VitalMetric`, the `Schema.Literal` budgeted-vital axis (`LCP`/`INP`/`CLS`/`TTFB`/`FCP`); and `BudgetThreshold`, the threshold `Record` gating the breach fold. The instrument rows and the `span` export live on `MetricRegistry` and a parallel `Metric.histogram` construction here is the named defect. The capture binds the native `PerformanceObserver` exclusively — the `web-vitals` npm package is a deliberate no-admission and its `onLCP`/`onCLS`/`onINP`/`onTTFB` callback surface is NEVER imported (`catalogMode: strict` rejects it).
- Cases: `VitalKind` is one closed `Data.TaggedEnum` whose arms are the five budgeted vitals (`Lcp`/`Inp`/`Cls`/`Ttfb`/`Fcp`) plus the two diagnostic performance signals (`LongTask`/`Resource`), each arm carrying a `VitalBehavior` row (the `PerformanceObserver` `entryType`, the projector that reads the value off the native entry shape, the optional `MetricRegistry` `VitalKey` gauge it records into, and an `accumulate` flag), so a captured signal is one tagged arm and one registry record, never an ad-hoc metric and never a parallel observer family; `PerformanceBudget` binds one `PerformanceObserver` per `VitalKind` arm (`largest-contentful-paint`, `event` for INP via `PerformanceEventTiming.processingStart - startTime` with a 40ms `durationThreshold` on the observe options so the native observer does not silently drop sub-104ms interactions, `layout-shift` for CLS via a typed `LayoutShiftEntry` refinement, `navigation` for TTFB via `PerformanceNavigationTiming.responseStart`, `paint` filtered to `first-contentful-paint` for FCP, `longtask`, `resource`) into one Effect `Stream` through `Stream.asyncScoped`, so the observer registration and `disconnect()` teardown are scoped to the capture fiber via `Effect.acquireRelease` and the entries enter the Effect world as a `Stream` rather than imperative callbacks; each entry folds to a `VitalSample` keyed by its `VitalKind` `_tag` and the sample records into the matching `MetricRegistry` `VitalKey` gauge so the existing Core-Web-Vitals instrument rows carry the live distribution; the CLS accumulator sums `layout-shift` `value`s across the session in a capture-fiber-local `Ref` per the Core Web Vitals definition rather than recording each shift independently, and the `event`-type INP arm folds to the maximum interaction delay observed rather than the last.
- Auto: the budget-exceeded fold is data-driven — `BudgetThreshold` is a `Record<VitalMetric, number>` carrying the "good" threshold per budgeted vital (LCP 2500ms, INP 200ms, CLS 0.1, TTFB 800ms, FCP 1800ms), the diagnostic `LongTask`/`Resource` arms carry no threshold and feed only their gauges, and each budgeted `VitalSample` folds against its threshold to a `BudgetOutcome` `Data.TaggedEnum` (`Within`/`Exceeded`), an `Exceeded` outcome lifting a typed `BudgetBreach` and shipping a `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` collector edge carrying the vital, the value, the threshold, and the route, so the budget gate is one `Match.tagsExhaustive` fold over a threshold table rather than scattered `if (lcp > 2500)` checks; the `"web.vital.breach"` `SpanName` is one literal on the closed `MetricRegistry` `SpanName` axis the `platform-substrate.md` page owns; a breach ships at most once per `(vital, route)` within the session through a capture-fiber-local `Ref<HashSet<string>>` dedupe so a repeated regression does not flood the collector.
- Packages: `effect` for the `Schema.Literal` vital axis, the `Data.TaggedEnum` `VitalKind`/`BudgetOutcome` unions, the `Stream.asyncScoped` observer ingress, the `Match.tagsExhaustive` budget fold, the `Ref`/`HashSet` capture state, and the threshold `Record`; the native `PerformanceObserver` API for the capture (ZERO `web-vitals` package — no `onLCP`/`onCLS`/`onINP`/`onTTFB` import); the existing `platform` `MetricRegistry` for the instrument rows and the `span` breach ship over the `SelfTelemetry` edge.
- Growth: a new budgeted vital lands as one literal on the `VitalMetric` axis, one `VitalKind` arm, one `BudgetThreshold` `Record` entry, and one `MetricRegistry` instrument row; a new diagnostic performance signal (long-task, resource-timing, element-timing) lands as one `VitalKind` arm with a `null` instrument or a new gauge row, never a parallel observer or a second metric construction; a new value projector lands as one `VitalBehavior.project` closure on its arm.
- Boundary: the Core Web Vitals instrument rows live on the `platform-substrate` `MetricRegistry` and this page owns only the CAPTURE and budget-gating mechanics, so a `Metric.histogram` constructed here outside the registry is the named two-owner-one-concern defect; capture is native `PerformanceObserver` only — the `web-vitals` npm package is NOT catalogued and `catalogMode: strict` rejects it, recorded in the charter ADMISSIONS_RECORD as a deliberate no-admission exactly like `AppRouter`, so the package's `onLCP`/`onCLS`/`onINP`/`onTTFB`/`onFCP` callback surface is NEVER imported and the native `PerformanceObserver` `entryType` arms own the capture; the `MetricRegistry` `VitalKey`/`GaugeKey`/`SpanName` axes (`web_vital_lcp_ms`/.../`web_vital_cls`, `web.vital.breach`) are consumed verbatim from `platform-substrate.md` and never re-declared here; the budget-breach span ships as a `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` collector edge, the only telemetry path the `platform-substrate` page fixes, so a direct collector POST is the named defect; `PerformanceBudget` emits no command, dials no transport, and `ui` never imports `platform`.

```ts owner
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { VitalKey } from "./platform-substrate.ts";
import type { Scope } from "effect";
import { Data, Effect, HashSet, Match, Metric, Option, Ref, Schema, Stream } from "effect";
import { MetricRegistry } from "./platform-substrate.ts";

// --- [TYPES] ---------------------------------------------------------------------------
// The budgeted-vital literal axis. ZERO web-vitals package: no onLCP/onCLS/onINP/onTTFB
// import — the native PerformanceObserver entryType arms own the capture end-to-end.
const VitalMetric = Schema.Literal("LCP", "INP", "CLS", "TTFB", "FCP");
type VitalMetric = typeof VitalMetric.Type;

// One closed performance-signal axis: the five budgeted vitals plus the two diagnostic
// signals (LongTask/Resource), each arm a tagged member carrying its captured value and
// the route. Adding a signal is one arm, never a parallel observer family.
type VitalKindShape = Data.TaggedEnum<{
  readonly Lcp: { readonly value: number; readonly route: string };
  readonly Inp: { readonly value: number; readonly route: string };
  readonly Cls: { readonly value: number; readonly route: string };
  readonly Ttfb: { readonly value: number; readonly route: string };
  readonly Fcp: { readonly value: number; readonly route: string };
  readonly LongTask: { readonly value: number; readonly route: string };
  readonly Resource: { readonly value: number; readonly route: string };
}>;
const VitalKind = Data.taggedEnum<VitalKindShape>();
type VitalSignal = VitalKindShape["_tag"];

// The closed PerformanceObserver entryType vocabulary — lib.dom types
// PerformanceObserverInit["type"] as `string | undefined`, so the row table binds this
// seven-member union instead, giving the VitalBehavior.entryType field an exhaustively
// typed observe() input rather than an open string that admits `undefined`.
type VitalEntryType =
  | "largest-contentful-paint"
  | "event"
  | "layout-shift"
  | "navigation"
  | "paint"
  | "longtask"
  | "resource";

// The native entry shapes lib.dom under-types: layout-shift carries `value`, the paint
// entry carries `name`, the event entry carries `processingStart`. Narrowed at the edge,
// never assumed on the base PerformanceEntry. lib.dom's PerformanceObserverInit also omits
// the `durationThreshold` option the `event` arm requires, so the observe-options shape is
// the closed entryType plus the optional buffered/durationThreshold pair, narrowed here.
type LayoutShiftEntry = PerformanceEntry & { readonly value: number; readonly hadRecentInput: boolean };
type FirstInputEntry = PerformanceEventTiming;
type VitalObserveInit = { readonly type: VitalEntryType; readonly buffered: true; readonly durationThreshold?: number };

// --- [MODELS] --------------------------------------------------------------------------
// One row per VitalKind arm: the observed entryType, the projector reading the value off
// the (narrowed) native entry, the MetricRegistry gauge it records into (null on the
// diagnostic arms), and the session-accumulate flag.
// The diagnostic arms (LongTask/Resource) carry a `null` instrument: the MetricRegistry
// GaugeKey axis the platform-substrate.md page owns declares only the five Core-Web-Vitals
// VitalKey gauges, so these signals are captured for the budget/breach concern without a
// parallel gauge construction here. A diagnostic gauge lands as one new GaugeKey row on the
// substrate axis, never an inline Metric.gauge at this sink.
// The `durationThreshold` field carries the per-arm observe()-options floor: native
// `event`-type observation silently drops interactions below 104ms unless an explicit
// durationThreshold is set, so the INP arm owns the 40ms web-vitals floor on its row
// rather than under-capturing. Every other arm leaves it `undefined` (no floor applies).
interface VitalBehavior {
  readonly entryType: VitalEntryType;
  readonly durationThreshold: number | undefined;
  readonly project: (entry: PerformanceEntry) => number;
  readonly instrument: VitalKey | null;
  readonly accumulate: boolean;
}

const vitalBehavior: { readonly [K in VitalSignal]: VitalBehavior } = {
  Lcp: { entryType: "largest-contentful-paint", durationThreshold: undefined, project: (e) => e.startTime, instrument: "web_vital_lcp_ms", accumulate: false },
  Inp: { entryType: "event", durationThreshold: 40, project: (e) => Math.max(0, (e as FirstInputEntry).processingStart - e.startTime), instrument: "web_vital_inp_ms", accumulate: false },
  Cls: { entryType: "layout-shift", durationThreshold: undefined, project: (e) => ((e as LayoutShiftEntry).hadRecentInput ? 0 : (e as LayoutShiftEntry).value), instrument: "web_vital_cls", accumulate: true },
  Ttfb: { entryType: "navigation", durationThreshold: undefined, project: (e) => (e as PerformanceNavigationTiming).responseStart, instrument: "web_vital_ttfb_ms", accumulate: false },
  Fcp: { entryType: "paint", durationThreshold: undefined, project: (e) => e.startTime, instrument: "web_vital_fcp_ms", accumulate: false },
  LongTask: { entryType: "longtask", durationThreshold: undefined, project: (e) => e.duration, instrument: null, accumulate: false },
  Resource: { entryType: "resource", durationThreshold: undefined, project: (e) => e.duration, instrument: null, accumulate: false },
};

// The data-driven good-threshold table — only the five budgeted vitals carry a budget; the
// diagnostic arms feed gauges but never gate. Folded, never an `if (lcp > 2500)` ladder.
const budgetThreshold: Record<VitalMetric, number> = { LCP: 2500, INP: 200, CLS: 0.1, TTFB: 800, FCP: 1800 };

type BudgetOutcomeShape = Data.TaggedEnum<{
  readonly Within: { readonly vital: VitalMetric; readonly value: number };
  readonly Exceeded: { readonly vital: VitalMetric; readonly value: number; readonly threshold: number; readonly route: string };
}>;
const BudgetOutcome = Data.taggedEnum<BudgetOutcomeShape>();

// The breach payload IS the Exceeded arm — one shape owns the vital+value+threshold+route
// breach concept, never a parallel Schema.Struct re-encoding already-typed in-domain data.
// The span ships this typed projection by direct construction; the page authors no decode.
type BudgetBreach = Extract<BudgetOutcomeShape, { readonly _tag: "Exceeded" }>;

// --- [SERVICES] ------------------------------------------------------------------------
class PerformanceBudget extends Effect.Service<PerformanceBudget>()("platform/PerformanceBudget", {
  scoped: Effect.gen(function* () {
    const registry = yield* MetricRegistry;
    return { observe: makeObserve(registry) } as const;
  }),
  dependencies: [MetricRegistry.Default],
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------
// One PerformanceObserver per VitalKind arm marshalled into a scoped Stream: the observe()
// registration is acquired and disconnect() released on the capture fiber, so teardown is
// structural. Session-accumulating arms (CLS) fold into a fiber-local Ref before emit; the
// INP arm folds to the running max interaction delay. Every other arm emits per entry.
const observeKind = (
  kind: VitalSignal,
): Stream.Stream<VitalKindShape, never, never> =>
  Stream.unwrapScoped(
    Ref.make(0).pipe(
      Effect.map((acc) =>
        Stream.asyncScoped<VitalKindShape>((emit) =>
          Effect.acquireRelease(
            Effect.sync(() => {
              const behavior = vitalBehavior[kind];
              const observer = new PerformanceObserver((list) => {
                for (const entry of list.getEntries()) {
                  if (kind === "Fcp" && entry.name !== "first-contentful-paint") continue;
                  const raw = behavior.project(entry);
                  const route = window.location.pathname;
                  if (behavior.accumulate || kind === "Inp") {
                    void emit.fromEffect(
                      Ref.modify(acc, (prev) => {
                        const next = kind === "Inp" ? Math.max(prev, raw) : prev + raw;
                        return [VitalKind[kind]({ value: next, route }), next];
                      }),
                    );
                  } else {
                    void emit.single(VitalKind[kind]({ value: raw, route }));
                  }
                }
              });
              const init: VitalObserveInit =
                behavior.durationThreshold === undefined
                  ? { type: behavior.entryType, buffered: true }
                  : { type: behavior.entryType, buffered: true, durationThreshold: behavior.durationThreshold };
              observer.observe(init);
              return observer;
            }),
            (observer) => Effect.sync(() => observer.disconnect()),
          ),
        ),
      ),
    ),
  );

// Fold a captured arm against its budget. Diagnostic arms (LongTask/Resource) yield no
// outcome; budgeted arms gate value <= threshold. Match.tagsExhaustive enforces the closed
// VitalKind axis — adding an arm without a fold case is a type error.
const gateBudget = (signal: VitalKindShape): BudgetOutcomeShape | null =>
  Match.value(signal).pipe(
    Match.tagsExhaustive({
      LongTask: () => null,
      Resource: () => null,
      Lcp: (s) => outcomeFor("LCP", s.value, s.route),
      Inp: (s) => outcomeFor("INP", s.value, s.route),
      Cls: (s) => outcomeFor("CLS", s.value, s.route),
      Ttfb: (s) => outcomeFor("TTFB", s.value, s.route),
      Fcp: (s) => outcomeFor("FCP", s.value, s.route),
    }),
  );

const outcomeFor = (vital: VitalMetric, value: number, route: string): BudgetOutcomeShape => {
  const threshold = budgetThreshold[vital];
  return value <= threshold
    ? BudgetOutcome.Within({ vital, value })
    : BudgetOutcome.Exceeded({ vital, value, threshold, route });
};

// The capture pipeline: merge every arm's scoped observer Stream, record each sample into
// its MetricRegistry gauge, fold to a budget outcome, and on an Exceeded outcome ship one
// breach span per (vital, route) through the SelfTelemetry edge — deduped in a fiber-local
// HashSet so a sustained regression never floods the collector. Forked into the scope.
const makeObserve = (
  registry: MetricRegistry,
): Effect.Effect<void, never, Scope.Scope> =>
  Ref.make(HashSet.empty<string>()).pipe(
    Effect.flatMap((seen) =>
      Stream.mergeAll(
        (Object.keys(vitalBehavior) as ReadonlyArray<VitalSignal>).map(observeKind),
        { concurrency: "unbounded" },
      ).pipe(
        Stream.tap((signal) => {
          const instrument = vitalBehavior[signal._tag].instrument;
          return instrument === null
            ? Effect.void
            : Metric.update(registry.gauges[instrument], signal.value);
        }),
        Stream.filterMap((signal) =>
          Option.fromNullable(gateBudget(signal))),
        Stream.mapEffect((outcome) =>
          Match.value(outcome).pipe(
            Match.tagsExhaustive({
              Within: () => Effect.void,
              Exceeded: (breach) =>
                shipBreach(registry, seen, breach),
            }),
          )),
        Stream.runDrain,
        Effect.forkScoped,
      )),
    Effect.asVoid,
  );

const shipBreach = (
  registry: MetricRegistry,
  seen: Ref.Ref<HashSet.HashSet<string>>,
  breach: BudgetBreach,
): Effect.Effect<void> => {
  const key = `${breach.vital}:${breach.route}`;
  return Ref.modify(seen, (set) =>
    HashSet.has(set, key) ? [false, set] as const : [true, HashSet.add(set, key)] as const).pipe(
    Effect.flatMap((fresh) =>
      fresh
        ? registry.span(
            "web.vital.breach",
            Effect.annotateCurrentSpan({
              vital: breach.vital,
              value: breach.value,
              threshold: breach.threshold,
              route: breach.route,
            }),
          ).pipe(Effect.asVoid)
        : Effect.void),
  );
};

// --- [EXPORTS] -------------------------------------------------------------------------
export type { BudgetBreach };
export { BudgetOutcome, PerformanceBudget, VitalKind, VitalMetric };
```
