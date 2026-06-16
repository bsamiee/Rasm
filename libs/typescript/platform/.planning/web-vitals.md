# [PLATFORM_WEB_VITALS]

One page owns the browser performance-budget and web-vitals observability concern — `PerformanceBudget`, the Core Web Vitals (LCP/INP/CLS/TTFB/FCP) capture over one `Schema.Literal` vital-metric axis folded into the `platform` `platform-substrate` `MetricRegistry` Core-Web-Vitals instrument rows, the data-driven `BudgetThreshold` `Record` gating a budget-exceeded fold, the long-task and resource-timing `PerformanceObserver` marshalled into Effect `Stream` signals, and the budget-breach span shipped as a `MetricRegistry.span` over the `SelfTelemetry` collector edge. `MetricRegistry` already declares the Core Web Vitals instrument family and owns the `span` export; this page owns the CAPTURE and budget-gating mechanics, not a parallel metric construction. Capture is native `PerformanceObserver` only — ZERO `web-vitals` package, a deliberate no-admission. The page composes the existing `MetricRegistry` edge, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                              |
| :-----: | :---------- | :----------------------------------------------------------------- |
|   [1]   | WEB_VITALS  | the vital capture, the budget thresholds, and the breach fold      |

## [2]-[WEB_VITALS]

- Owner: `PerformanceBudget`, the single web-vitals capture and budget owner — the vital capture over the `VitalMetric` `Schema.Literal` axis feeding the existing `MetricRegistry` Core-Web-Vitals instrument rows, the `BudgetThreshold` data-driven `Record` per vital, the long-task/resource-timing `PerformanceObserver` `Stream` ingress, and the budget-breach `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` edge; `VitalMetric`, the `Schema.Literal` vital axis (`LCP`/`INP`/`CLS`/`TTFB`/`FCP`); and `BudgetThreshold`, the threshold `Record` gating the breach fold. The instrument rows and the `span` export live on `MetricRegistry` and a parallel `Metric.histogram` construction here is the named defect.
- Cases: `VitalMetric` is one `Schema.Literal` union (`LCP`/`INP`/`CLS`/`TTFB`/`FCP`) with a `VitalBehavior` `Record` mapping each vital to its `PerformanceObserver` `entryType` and the `MetricRegistry` Core-Web-Vitals gauge (the `VitalKey` instrument the substrate axis declares) it records into, so a captured vital is one observer arm and one registry record, never an ad-hoc metric; `PerformanceBudget` binds one `PerformanceObserver` per observed `entryType` (`largest-contentful-paint`, `event`/`first-input` for INP, `layout-shift` for CLS, `navigation` for TTFB, `paint` for FCP, `longtask`, `resource`) into one Effect `Stream` through `Stream.asyncScoped`, so the observer registration and teardown are scoped to the capture fiber and the entries enter the Effect world as a `Stream` rather than imperative callbacks; each entry folds to a `VitalSample` keyed by its `VitalMetric` and the sample records into the matching `MetricRegistry` `VitalKey` gauge so the existing Core-Web-Vitals instrument rows carry the live distribution; the CLS accumulator sums layout-shift values across the session per the Core Web Vitals definition rather than recording each shift independently.
- Auto: the budget-exceeded fold is data-driven — `BudgetThreshold` is a `Record<VitalMetric, number>` carrying the "good" threshold per vital (LCP 2500ms, INP 200ms, CLS 0.1, TTFB 800ms, FCP 1800ms), and each `VitalSample` folds against its threshold to a `BudgetOutcome` (`within`/`exceeded`), an `exceeded` outcome shipping a `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` collector edge carrying the vital, the value, the threshold, and the route, so the budget gate is one fold over a threshold table rather than scattered `if (lcp > 2500)` checks; the `"web.vital.breach"` `SpanName` is one literal added to the closed `MetricRegistry` `SpanName` axis the `platform-substrate.md` page owns; a breach ships at most once per `(vital, route)` within the session so a repeated regression does not flood the collector.
- Packages: `effect` for the `Schema.Literal` vital axis, the `Stream.asyncScoped` observer ingress, the `Match` budget fold, and the threshold `Record`; the native `PerformanceObserver` API for the capture (ZERO `web-vitals` package); the existing `platform` `MetricRegistry` for the instrument rows and the `span` breach ship over the `SelfTelemetry` edge.
- Growth: a new vital lands as one literal on the `VitalMetric` axis, one `VitalBehavior` `Record` row, and one `MetricRegistry` instrument row; a new budget lands as one `BudgetThreshold` `Record` entry; a new performance signal (long-task, resource-timing) lands as one observer arm, never a parallel observer or a second metric construction.
- Boundary: the Core Web Vitals instrument rows live on the `platform-substrate` `MetricRegistry` and this page owns only the CAPTURE and budget-gating mechanics, so a `Metric.histogram` constructed here outside the registry is the named two-owner-one-concern defect; capture is native `PerformanceObserver` only — the `web-vitals` npm package is NOT catalogued and `catalogMode: strict` rejects it, recorded in the charter ADMISSIONS_RECORD as a deliberate no-admission exactly like `AppRouter`; the budget-breach span ships as a `MetricRegistry.span("web.vital.breach", ...)` over the `SelfTelemetry` collector edge, the only telemetry path the `platform-substrate` page fixes, so a direct collector POST is the named defect; `PerformanceBudget` emits no command, dials no transport, and `ui` never imports `platform`.

```ts contract
type VitalMetric = "LCP" | "INP" | "CLS" | "TTFB" | "FCP";

interface VitalBehavior {
  readonly entryType: string;
  readonly instrument: VitalKey;
  readonly accumulate: boolean;
}

const vitalBehavior: Record<VitalMetric, VitalBehavior> = {
  LCP: { entryType: "largest-contentful-paint", instrument: "web_vital_lcp_ms", accumulate: false },
  INP: { entryType: "event", instrument: "web_vital_inp_ms", accumulate: false },
  CLS: { entryType: "layout-shift", instrument: "web_vital_cls", accumulate: true },
  TTFB: { entryType: "navigation", instrument: "web_vital_ttfb_ms", accumulate: false },
  FCP: { entryType: "paint", instrument: "web_vital_fcp_ms", accumulate: false },
};

const budgetThreshold: Record<VitalMetric, number> = { LCP: 2500, INP: 200, CLS: 0.1, TTFB: 800, FCP: 1800 };

interface VitalSample {
  readonly vital: VitalMetric;
  readonly value: number;
  readonly route: string;
}

type BudgetOutcome =
  | { readonly _tag: "Within"; readonly vital: VitalMetric; readonly value: number }
  | { readonly _tag: "Exceeded"; readonly vital: VitalMetric; readonly value: number; readonly threshold: number };

interface PerformanceBudget {
  readonly observe: Effect.Effect<void, never, Scope.Scope | MetricRegistry>;
}

const gateBudget = (sample: VitalSample): BudgetOutcome => {
  const threshold = budgetThreshold[sample.vital];
  return sample.value <= threshold
    ? { _tag: "Within", vital: sample.vital, value: sample.value }
    : { _tag: "Exceeded", vital: sample.vital, value: sample.value, threshold };
};

const observeVital = (vital: VitalMetric): Stream.Stream<VitalSample, never, Scope.Scope> =>
  Stream.asyncScoped<VitalSample>((emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const cls = { value: 0 };
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            const value = vitalBehavior[vital].accumulate
              ? (cls.value += (entry as PerformanceEntry & { value: number }).value)
              : entry.startTime || entry.duration;
            void emit.single({ vital, value, route: window.location.pathname });
          }
        });
        observer.observe({ type: vitalBehavior[vital].entryType, buffered: true });
        return observer;
      }),
      (observer) => Effect.sync(() => observer.disconnect()),
    ).pipe(Effect.asVoid));

const makePerformanceBudget: PerformanceBudget = {
  observe: Effect.gen(function* () {
    const registry = yield* MetricRegistry;
    const breached = yield* Ref.make<ReadonlySet<string>>(new Set());
    yield* Stream.mergeAll(
      (["LCP", "INP", "CLS", "TTFB", "FCP"] as const).map(observeVital),
      { concurrency: "unbounded" },
    ).pipe(
      Stream.tap((sample) => Metric.update(registry.gauges[vitalBehavior[sample.vital].instrument], sample.value)),
      Stream.map(gateBudget),
      Stream.mapEffect((outcome) =>
        Match.value(outcome).pipe(
          Match.tag("Within", () => Effect.void),
          Match.tag("Exceeded", (e) => {
            const key = `${e.vital}:${window.location.pathname}`;
            return Ref.get(breached).pipe(
              Effect.flatMap((s) =>
                s.has(key)
                  ? Effect.void
                  : Ref.update(breached, (x) => new Set([...x, key])).pipe(
                      Effect.zipRight(registry.span("web.vital.breach", Effect.succeed(e))),
                      Effect.asVoid,
                    )),
            );
          }),
          Match.exhaustive,
        )),
      Stream.runDrain,
      Effect.forkScoped,
    );
  }),
};
```
