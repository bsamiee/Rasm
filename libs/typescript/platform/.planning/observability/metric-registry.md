# [PLATFORM_METRIC_REGISTRY]

One page owns the self-telemetry export edge and the bounded instrument/span vocabulary — `SelfTelemetry`, the host instrumentation export edge over the browser OpenTelemetry web layer, and `MetricRegistry`, the closed `Metric` instrument and `Effect.withSpan` span vocabulary every host owner ships through. The collector is the only telemetry path; the registry declares every instrument as a named row, so an inline `Metric.counter` or a free-string span name authored at a sink is the deleted form. The page references no telemetry wire type and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          |
| :-----: | :-------------- | :------------------------------------------------------------- |
|   [1]   | METRIC_REGISTRY | the telemetry export edge and the closed instrument/span vocabulary |

## [2]-[METRIC_REGISTRY]

- Owner: `SelfTelemetry`, the host instrumentation export edge over the browser OpenTelemetry web layer, and `MetricRegistry`, the bounded instrument-and-span vocabulary the host edge ships.
- Cases: `SelfTelemetry` ships the host's own spans and metrics to the collector through the OTLP web export, the collector the only telemetry path and dashboards reading it through the `ui` `CollectorPanel`; `MetricRegistry` owns the closed `Metric` instrument vocabulary mirroring the C# `HostMetrics` names and the closed `Effect.withSpan` span vocabulary — the `crash.report` row `fault-capture`'s `CrashTelemetry` ships and the `web.vital.breach` row `web-vitals`' `PerformanceBudget` ships are span literals on this one axis, never free-string span names authored at the sink — and the Core Web Vitals instrument family `PerformanceBudget` feeds (the registry declares the Core-Web-Vitals instrument rows; `PerformanceBudget` owns the capture and budget-gating, never a parallel metric construction), so every instrument is a named row rather than an ad-hoc construction.
- Auto: `MetricRegistry` binds the `@effect/opentelemetry` `WebSdk` `Layer` with its resource attributes and the OTLP trace-and-metric exporters reading the collector endpoint from `RuntimeConfig`, so the export edge is one layer over a named instrument set and an inline `Metric.counter` construction outside the registry is the deleted form.
- Packages: `effect` for the `Metric` and `Effect.withSpan` primitives; `@effect/platform-browser` for the browser OpenTelemetry layer; `@effect/opentelemetry` for the `WebSdk` exporter; `@opentelemetry/sdk-trace-web` as the browser trace SDK the `WebSdk` binds; `@opentelemetry/sdk-metrics`, `@opentelemetry/resources`, and `@opentelemetry/semantic-conventions` for the metric reader and resource attributes; `RuntimeConfig` for the collector endpoint.
- Growth: a new signal lands as one instrument row on `MetricRegistry`; a new span lands as one `Effect.withSpan` vocabulary row, never a parallel telemetry construction.
- Boundary: the self-telemetry spans and metrics cross no wire contract — they ship from the host root to the collector, which is the only telemetry path; the observability stack that backs the collector is provisioned by `services` `provisioning`; `MetricRegistry` is the single instrument owner and an inline metric construction outside it is the named defect; the node OTel SDK never enters this browser edge; the replay-window span attribute (`session-replay`) lands on this closed `SpanName`/span-attribute axis, never a third telemetry path.

```ts contract
type CounterKey = "wire_calls_total" | "fault_total" | "redial_total" | "offline_drain_total";
type HistogramKey = "wire_call_duration_ms" | "decode_duration_ms" | "frame_reassemble_ms";
type VitalKey = "web_vital_lcp_ms" | "web_vital_inp_ms" | "web_vital_cls" | "web_vital_ttfb_ms" | "web_vital_fcp_ms";
type GaugeKey = "active_subscriptions" | "offline_queue_depth" | VitalKey;
type SpanName = "boot.spa" | "route.transition" | "worker.reassemble" | "auth.refresh" | "sw.activate" | "crash.report" | "web.vital.breach";

interface MetricRegistry {
  readonly counters: { readonly [K in CounterKey]: Metric.Metric.Counter<number> };
  readonly histograms: { readonly [K in HistogramKey]: Metric.Metric.Histogram<number> };
  readonly gauges: { readonly [K in GaugeKey]: Metric.Metric.Gauge<number> };
  readonly span: <A, E, R>(name: SpanName, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
  readonly webSdk: Layer.Layer<never, never, RuntimeConfig>;
}
```
