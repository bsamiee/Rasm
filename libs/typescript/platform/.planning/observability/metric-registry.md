# [PLATFORM_METRIC_REGISTRY]

One page owns the self-telemetry export edge and the bounded instrument/span vocabulary — `SelfTelemetry`, the host instrumentation export edge over the browser OpenTelemetry web layer, and `MetricRegistry`, the closed `Metric` instrument and `Effect.withSpan` span vocabulary every host owner ships through. The collector is the only telemetry path; the registry declares every instrument as a named row, so an inline `Metric.counter` or a free-string span name authored at a sink is the deleted form. The page references no telemetry wire type and authors no decode.

## [1]-[INDEX]

[METRIC_REGISTRY]: the telemetry export edge and the closed instrument/span vocabulary.

## [2]-[METRIC_REGISTRY]

- Owner: `SelfTelemetry`, the host instrumentation export edge over the browser OpenTelemetry web layer, and `MetricRegistry`, the bounded instrument-and-span vocabulary the host edge ships.
- Cases: `SelfTelemetry` ships the host's own spans and metrics to the collector through the OTLP web export, the collector the only telemetry path and dashboards reading it through the `ui` `CollectorPanel`; `MetricRegistry` owns one `instrumentVocabulary` `as const satisfies` table mapping each instrument name to its `counter`/`histogram`/`gauge` kind (mirroring the C# `HostMetrics` names) and one `spanVocabulary` `as const` span axis (the `crash.report` row `fault-capture`'s `CrashTelemetry` ships and the `web.vital.breach` row `web-vitals`' `PerformanceBudget` ships are rows on this one axis, never free-string span names authored at the sink), with `CounterKey`/`HistogramKey`/`VitalKey`/`GaugeKey`/`SpanName` derived by `keyof typeof` over the table rather than five parallel literal unions; the Core Web Vitals instrument rows (`web_vital_*`) carry the `vital` axis flag the gauge projection reads, so `PerformanceBudget` records into a named gauge row and never a parallel metric construction.
- Auto: `MetricRegistry` binds the `@effect/opentelemetry` `WebSdk` `Layer` with its resource attributes and the OTLP trace-and-metric exporters reading the collector endpoint from `RuntimeConfig`, so the export edge is one layer over the one derived instrument set and an inline `Metric.counter` construction outside the vocabulary table is the deleted form.
- Packages: `effect` for the `Metric` and `Effect.withSpan` primitives; `@effect/platform-browser` for the browser OpenTelemetry layer; `@effect/opentelemetry` for the `WebSdk` exporter; `@opentelemetry/sdk-trace-web` as the browser trace SDK the `WebSdk` binds; `@opentelemetry/sdk-metrics`, `@opentelemetry/resources`, and `@opentelemetry/semantic-conventions` for the metric reader and resource attributes; `RuntimeConfig` for the collector endpoint.
- Growth: a new signal lands as one row on the `instrumentVocabulary` table carrying its kind; a new span lands as one row on the `spanVocabulary` axis, never a parallel telemetry construction and never a sixth key-union declaration.
- Boundary: the self-telemetry spans and metrics cross no wire contract — they ship from the host root to the collector, which is the only telemetry path; the observability stack that backs the collector is provisioned by `services` `provisioning`; `MetricRegistry` is the single instrument owner and an inline metric construction outside the vocabulary table is the named defect; the node OTel SDK never enters this browser edge; the replay-window span attribute (`session-replay`) lands on the derived `SpanName` axis, never a third telemetry path.

```ts contract
const instrumentVocabulary = {
  wire_calls_total: "counter",
  fault_total: "counter",
  redial_total: "counter",
  offline_drain_total: "counter",
  wire_call_duration_ms: "histogram",
  decode_duration_ms: "histogram",
  frame_reassemble_ms: "histogram",
  active_subscriptions: "gauge",
  offline_queue_depth: "gauge",
  web_vital_lcp_ms: "gauge",
  web_vital_inp_ms: "gauge",
  web_vital_cls: "gauge",
  web_vital_ttfb_ms: "gauge",
  web_vital_fcp_ms: "gauge",
} as const satisfies Record<string, "counter" | "histogram" | "gauge">;

const spanVocabulary = ["boot.spa", "route.transition", "worker.reassemble", "auth.refresh", "sw.activate", "crash.report", "web.vital.breach"] as const;

type InstrumentName = keyof typeof instrumentVocabulary;
type InstrumentKind = typeof instrumentVocabulary[InstrumentName];
type KeyOfKind<Kind extends InstrumentKind> = { readonly [K in InstrumentName]: typeof instrumentVocabulary[K] extends Kind ? K : never }[InstrumentName];
type CounterKey = KeyOfKind<"counter">;
type HistogramKey = KeyOfKind<"histogram">;
type GaugeKey = KeyOfKind<"gauge">;
type VitalKey = Extract<GaugeKey, `web_vital_${string}`>;
type SpanName = typeof spanVocabulary[number];

interface MetricRegistry {
  readonly counters: { readonly [K in CounterKey]: Metric.Metric.Counter<number> };
  readonly histograms: { readonly [K in HistogramKey]: Metric.Metric.Histogram<number> };
  readonly gauges: { readonly [K in GaugeKey]: Metric.Metric.Gauge<number> };
  readonly span: <A, E, R>(name: SpanName, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
  readonly webSdk: Layer.Layer<never, never, RuntimeConfig>;
}
```
