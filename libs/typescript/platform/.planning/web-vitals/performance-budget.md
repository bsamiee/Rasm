# [PLATFORM_PERFORMANCE_BUDGET]

One page owns the browser performance-budget and web-vitals observability — `PerformanceBudget`, the Core Web Vitals (LCP/INP/CLS/TTFB/FCP) capture over one `Schema.Literal` vital axis folded into the `observability` `MetricRegistry` Core-Web-Vitals instrument rows, the data-driven `BudgetThreshold` `Record` gating a budget-exceeded fold, the long-task and resource-timing `PerformanceObserver` marshalled into Effect `Stream` signals, and the budget-breach span shipped over the `SelfTelemetry` collector edge. The per-soft-navigation reset keyed off the `routing` location cell and the `visibilityState`-hidden terminal CLS/INP flush are the SOFT_NAV_VITALS growth. `MetricRegistry` declares the Core Web Vitals instrument family and owns the `span` export; this page owns the CAPTURE and budget-gating mechanics, not a parallel metric construction. Capture is native `PerformanceObserver` only — ZERO `web-vitals` package, a deliberate no-admission. The page composes the `observability` `MetricRegistry` edge, holds no domain state, and authors no decode.

## [1]-[INDEX]

[PERFORMANCE_BUDGET]: the vital capture, the budget thresholds, and the breach fold.

## [2]-[PERFORMANCE_BUDGET]

- Owner: `PerformanceBudget`, the single web-vitals capture and budget owner — the vital capture over the closed `VitalKind` `Data.TaggedEnum` performance-signal axis feeding the `MetricRegistry` Core-Web-Vitals instrument rows, the `BudgetThreshold` data-driven `Record` per budgeted vital, the long-task/resource-timing `PerformanceObserver` `Stream` ingress folded onto the SAME `VitalKind` surface, and the budget-breach `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` edge; `VitalMetric`, the `Schema.Literal` budgeted-vital axis (`LCP`/`INP`/`CLS`/`TTFB`/`FCP`); and `BudgetThreshold`, the threshold `Record` gating the breach fold. The instrument rows and the `span` export live on `MetricRegistry` and a parallel `Metric.histogram` construction here is the named defect. The capture binds the native `PerformanceObserver` exclusively — the `web-vitals` npm package is a deliberate no-admission.
- Cases: `VitalKind` is one closed `Data.TaggedEnum` whose arms are the five budgeted vitals (`Lcp`/`Inp`/`Cls`/`Ttfb`/`Fcp`) plus the two diagnostic performance signals (`LongTask`/`Resource`), each arm carrying a `VitalBehavior` row (the `PerformanceObserver` `entryType`, the projector that reads the value off the native entry shape, the optional `MetricRegistry` `VitalKey` gauge it records into, and an `accumulate` flag), so a captured signal is one tagged arm and one registry record, never an ad-hoc metric and never a parallel observer family; `PerformanceBudget` binds one `PerformanceObserver` per `VitalKind` arm (`largest-contentful-paint`, `event` for INP via `PerformanceEventTiming.processingStart - startTime` with a 40ms `durationThreshold` so the native observer does not silently drop sub-104ms interactions, `layout-shift` for CLS via a typed `LayoutShiftEntry` refinement, `navigation` for TTFB via `PerformanceNavigationTiming.responseStart`, `paint` filtered to `first-contentful-paint` for FCP, `longtask`, `resource`) into one Effect `Stream` through `Stream.asyncScoped`, so the observer registration and `disconnect()` teardown are scoped via `Effect.acquireRelease`; each entry folds to a `VitalSample` keyed by its `VitalKind` `_tag` and records into the matching `MetricRegistry` `VitalKey` gauge; the CLS accumulator sums `layout-shift` `value`s in a capture-fiber-local `Ref` per the Core Web Vitals definition and the `event`-type INP arm folds to the maximum interaction delay observed.
- Auto: the budget-exceeded fold is data-driven — `BudgetThreshold` is a `Record<VitalMetric, number>` carrying the "good" threshold per budgeted vital (LCP 2500ms, INP 200ms, CLS 0.1, TTFB 800ms, FCP 1800ms), the diagnostic `LongTask`/`Resource` arms carry no threshold and feed only their gauges, and each budgeted `VitalSample` folds against its threshold to a `BudgetOutcome` `Data.TaggedEnum` (`Within`/`Exceeded`), an `Exceeded` outcome lifting a typed `BudgetBreach` and shipping a `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` collector edge carrying the vital, the value, the threshold, and the route, so the budget gate is one `Match.tagsExhaustive` fold over a threshold table rather than scattered `if (lcp > 2500)` checks; the `"web.vital.breach"` `SpanName` is one literal on the closed `MetricRegistry` `SpanName` axis `observability` owns; a breach ships at most once per `(vital, route)` within the page lifetime through a capture-fiber-local `Ref<HashSet<string>>` dedupe.
- Lifecycle: the CLS accumulator sums `layout-shift` values and the INP arm holds the running-max interaction delay in capture-fiber-local `Ref`s for the page lifetime, each captured signal stamping the route off `window.location.pathname` at capture time; the `(vital, route)` breach dedupe holds for the page lifetime in a capture-fiber-local `Ref<HashSet>`. The per-soft-navigation accumulator/dedupe reset keyed off the `routing` location cell and the `visibilityState`-hidden terminal CLS/INP flush are the SOFT_NAV_VITALS growth — both add the `routing` `AppRouter` location read as the one coupling seam and the `visibilitychange` ingress, never a parallel route tracker.
- Packages: `effect` for the `Schema.Literal` vital axis, the `Data.TaggedEnum` `VitalKind`/`BudgetOutcome` unions, the `Stream.asyncScoped` observer ingress, the `Match.tagsExhaustive` budget fold, the `Ref`/`HashSet` capture state, and the threshold `Record`; the native `PerformanceObserver` API for the capture (ZERO `web-vitals` package); the `observability` `MetricRegistry` for the instrument rows and the `span` breach ship. The `routing` `AppRouter` location cell is the SOFT_NAV_VITALS growth dependency, not a current read.
- Growth: a new budgeted vital lands as one literal on the `VitalMetric` axis, one `VitalKind` arm, one `BudgetThreshold` `Record` entry, and one `MetricRegistry` instrument row; a new diagnostic performance signal lands as one `VitalKind` arm with a `null` instrument or a new gauge row, never a parallel observer or a second metric construction; a new value projector lands as one `VitalBehavior.project` closure on its arm; the SOFT_NAV_VITALS soft-navigation reset lands as a `routing` location-cell subscription resetting the CLS/INP `Ref`s and the dedupe set on each guard-admitted commit plus a `visibilitychange`-to-hidden terminal flush, never a parallel route tracker.
- Boundary: the Core Web Vitals instrument rows live on the `observability` `MetricRegistry` and this page owns only the CAPTURE and budget-gating mechanics, so a `Metric.histogram` constructed here outside the registry is the named two-owner-one-concern defect; capture is native `PerformanceObserver` only — the `web-vitals` npm package is a deliberate no-admission recorded at the folder README registry, so its `onLCP`/`onCLS`/`onINP`/`onTTFB`/`onFCP` callback surface is NEVER imported; the `MetricRegistry` `VitalKey`/`GaugeKey`/`SpanName` axes are consumed verbatim from `observability` and never re-declared here; the budget-breach span ships over the `SelfTelemetry` collector edge, the only telemetry path, so a direct collector POST is the named defect; the SOFT_NAV_VITALS reset reads the `routing` location cell and never mutates it; `PerformanceBudget` emits no command, dials no transport, and `ui` never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { VitalKey } from "../observability/metric-registry.ts";
import type { Scope } from "effect";
import { Data, Effect, HashSet, Match, Metric, Option, Ref, Schema, Stream } from "effect";
import { MetricRegistry } from "../observability/metric-registry.ts";

// --- [TYPES] ---------------------------------------------------------------------------
const VitalMetric = Schema.Literal("LCP", "INP", "CLS", "TTFB", "FCP");
type VitalMetric = typeof VitalMetric.Type;

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

type VitalEntryType =
  | "largest-contentful-paint"
  | "event"
  | "layout-shift"
  | "navigation"
  | "paint"
  | "longtask"
  | "resource";

type LayoutShiftEntry = PerformanceEntry & { readonly value: number; readonly hadRecentInput: boolean };
type FirstInputEntry = PerformanceEventTiming;
type VitalObserveInit = { readonly type: VitalEntryType; readonly buffered: true; readonly durationThreshold?: number };

// --- [MODELS] --------------------------------------------------------------------------
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

const budgetThreshold: Record<VitalMetric, number> = { LCP: 2500, INP: 200, CLS: 0.1, TTFB: 800, FCP: 1800 };

type BudgetOutcomeShape = Data.TaggedEnum<{
  readonly Within: { readonly vital: VitalMetric; readonly value: number };
  readonly Exceeded: { readonly vital: VitalMetric; readonly value: number; readonly threshold: number; readonly route: string };
}>;
const BudgetOutcome = Data.taggedEnum<BudgetOutcomeShape>();

type BudgetBreach = Extract<BudgetOutcomeShape, { readonly _tag: "Exceeded" }>;

// --- [SERVICES] ------------------------------------------------------------------------
class PerformanceBudget extends Effect.Service<PerformanceBudget>()("@rasm/ts/platform/PerformanceBudget", {
  scoped: Effect.gen(function* () {
    const registry = yield* MetricRegistry;
    return { observe: makeObserve(registry) } as const;
  }),
  dependencies: [MetricRegistry.Default],
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
