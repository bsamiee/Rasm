# [PLATFORM_METRIC_REGISTRY]

One page owns the self-telemetry export edge and the bounded instrument/span vocabulary — `MetricRegistry`, the one `Effect.Service` carrying the closed `Metric` instrument records, the `Effect.withSpan` span vocabulary every host owner ships through, and the `webSdk` `SelfTelemetry` browser-OpenTelemetry export layer the composition root composes. The collector is the only telemetry path; the registry declares every instrument as a named row built once at service construction, so an inline `Metric.counter` or a free-string span name authored at a sink is the deleted form. The service is provided once as `MetricRegistryLive` and resolved by the four host-owner consumers (`composition-root`, `crash-telemetry`, `performance-budget`, `session-recorder`) through `yield* MetricRegistry`; a bare interface no consumer can `provide` is the retired form. The page references no telemetry wire type and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          |
| :-----: | :-------------- | :-------------------------------------------------------------- |
|   [1]   | METRIC_REGISTRY | the `Effect.Service` export edge and the closed instrument/span vocabulary |
|   [2]   | RESEARCH        | the `MetricBoundaries` histogram-bucket constructor spelling    |

## [2]-[METRIC_REGISTRY]

- Owner: `MetricRegistry`, the one `Effect.Service` carrying the bounded instrument-and-span vocabulary the host edge ships, exposing the derived `counters`/`histograms`/`gauges` instrument records, the `span` `Effect.withSpan` projector over the closed `SpanName` axis, and the `webSdk` `SelfTelemetry` export layer; `SelfTelemetry` is the host instrumentation export edge the `webSdk` field IS — the `MetricRegistry.webSdk` `WebSdk.layer` over the browser OpenTelemetry web layer the composition root composes once. `MetricRegistry` is the service tag every host owner resolves through `yield* MetricRegistry`, and `MetricRegistryLive` is its `.Default` layer; a bare `interface` no consumer can `provide` is the retired form, a second instrument owner or a free `Metric.counter` at a sink the named defect.
- Cases: `MetricRegistry` is one `Effect.Service` whose constructor derives every instrument from one `instrumentVocabulary` `as const satisfies` table mapping each instrument name to its `counter`/`histogram`/`gauge` kind (mirroring the C# `HostMetrics` names) — `Metric.counter`/`Metric.histogram`/`Metric.gauge` constructed once per row at service build and frozen into the `counters`/`histograms`/`gauges` records consumers index by key, never per-sink — and one `spanVocabulary` `as const` span axis (the `crash.report` row `fault-capture`'s `CrashTelemetry` ships, the `web.vital.breach` row `web-vitals`' `PerformanceBudget` ships, and the `session.replay` row `session-replay`'s `SessionRecorder` ships are rows on this one axis, never free-string span names authored at the sink), with `CounterKey`/`HistogramKey`/`VitalKey`/`GaugeKey`/`SpanName` derived by `keyof typeof` over the table rather than five parallel literal unions; the `span(name, effect)` member is one `Effect.withSpan` over the `SpanName` literal so a sink ships a span by name not a free string; the Core Web Vitals instrument rows (`web_vital_*`) carry the `vital` axis flag the gauge projection reads, so `PerformanceBudget` records into a named gauge row and never a parallel metric construction.
- Auto: `MetricRegistry` binds the `@effect/opentelemetry` `WebSdk.layer` over a `Configuration` carrying the required `resource` identity, the `spanProcessor` as a `BatchSpanProcessor` wrapping the `@opentelemetry/exporter-trace-otlp-http` `OTLPTraceExporter` (the concrete browser `SpanExporter` over OTLP/HTTP), the `metricReader` as a `PeriodicExportingMetricReader` wrapping the `@opentelemetry/exporter-metrics-otlp-http` `OTLPMetricExporter` (the concrete `PushMetricExporter`), and one `tracerConfig.sampler` row — `ParentBasedSampler` over a `TraceIdRatioBasedSampler` so a child span inherits the parent's sampling decision and a root span samples at the one configured ratio — all reading the collector endpoint from `RuntimeConfig`, so the export edge is one layer over the one derived instrument set, the sampling decision is one declared row rather than an export-time per-span flag, and an inline `Metric.counter` construction outside the vocabulary table is the deleted form.
- Packages: `effect` for the `Metric` and `Effect.withSpan` primitives; `@effect/opentelemetry` `WebSdk.layer` for the browser SDK exporter composite (required `resource`, `spanProcessor`, `metricReader`, `tracerConfig`); `@opentelemetry/sdk-trace-web` `BatchSpanProcessor`/`ParentBasedSampler`/`TraceIdRatioBasedSampler` for the span processor and the one sampler row; `@opentelemetry/exporter-trace-otlp-http` `OTLPTraceExporter` for the concrete browser `SpanExporter`; `@opentelemetry/sdk-metrics` `PeriodicExportingMetricReader` and `@opentelemetry/exporter-metrics-otlp-http` `OTLPMetricExporter` for the push metric reader and exporter; `@opentelemetry/resources`/`@opentelemetry/semantic-conventions` for the resource attributes; `RuntimeConfig` for the collector endpoint.
- Growth: a new signal lands as one row on the `instrumentVocabulary` table carrying its kind; a new span lands as one row on the `spanVocabulary` axis; a sampling-policy change lands as one `tracerConfig.sampler` row swap, never a per-span sampling flag; never a parallel telemetry construction and never a sixth key-union declaration.
- Boundary: the self-telemetry spans and metrics cross no wire contract — they ship from the host root to the collector, which is the only telemetry path; the observability stack that backs the collector is provisioned by `services` `provisioning`; `MetricRegistry` is the single instrument owner provided as one `Effect.Service` and an inline metric construction outside the vocabulary table is the named defect; the `composition-root.md` graph provides `MetricRegistryLive` once under `Layer.mergeAll(AuthSessionLive, SelfTelemetryLive, MetricRegistryLive)` and the `crash-telemetry.md`/`performance-budget.md`/`session-recorder.md` owners resolve the one tag in their `R` channel, never re-declaring the instrument shape; the node OTel SDK never enters this browser edge; the replay-window span attribute (`session-replay`) lands on the derived `SpanName` axis, never a third telemetry path.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { Resource } from "@effect/opentelemetry/Resource";
import type { MetricBoundaries } from "effect/MetricBoundaries";
import { Effect, Layer, Metric } from "effect";
import { WebSdk } from "@effect/opentelemetry";
import { BatchSpanProcessor, ParentBasedSampler, TraceIdRatioBasedSampler } from "@opentelemetry/sdk-trace-web";
import { PeriodicExportingMetricReader } from "@opentelemetry/sdk-metrics";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { OTLPMetricExporter } from "@opentelemetry/exporter-metrics-otlp-http";
import { ATTR_SERVICE_NAMESPACE } from "@opentelemetry/semantic-conventions";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";

// --- [CONSTANTS] -----------------------------------------------------------------------
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

const spanVocabulary = ["boot.spa", "route.transition", "worker.reassemble", "auth.refresh", "sw.activate", "crash.report", "web.vital.breach", "session.replay"] as const;

type InstrumentName = keyof typeof instrumentVocabulary;
type InstrumentKind = typeof instrumentVocabulary[InstrumentName];
type KeyOfKind<Kind extends InstrumentKind> = { readonly [K in InstrumentName]: typeof instrumentVocabulary[K] extends Kind ? K : never }[InstrumentName];
type CounterKey = KeyOfKind<"counter">;
type HistogramKey = KeyOfKind<"histogram">;
type GaugeKey = KeyOfKind<"gauge">;
type VitalKey = Extract<GaugeKey, `web_vital_${string}`>;
type SpanName = typeof spanVocabulary[number];

// --- [MODELS] --------------------------------------------------------------------------
interface MetricRegistryShape {
  readonly counters: { readonly [K in CounterKey]: Metric.Metric.Counter<number> };
  readonly histograms: { readonly [K in HistogramKey]: Metric.Metric.Histogram<number> };
  readonly gauges: { readonly [K in GaugeKey]: Metric.Metric.Gauge<number> };
  readonly span: <A, E, R>(name: SpanName, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
  readonly webSdk: Layer.Layer<Resource, never, RuntimeConfig>;
}

const namesOfKind = <Kind extends InstrumentKind>(kind: Kind): ReadonlyArray<KeyOfKind<Kind>> =>
  (Object.keys(instrumentVocabulary) as ReadonlyArray<InstrumentName>).filter(
    (name): name is KeyOfKind<Kind> => instrumentVocabulary[name] === kind,
  );

const counters = Object.fromEntries(namesOfKind("counter").map((name) => [name, Metric.counter(name)])) as MetricRegistryShape["counters"];
const histograms = Object.fromEntries(namesOfKind("histogram").map((name) => [name, Metric.histogram(name, HISTOGRAM_BOUNDARIES)])) as MetricRegistryShape["histograms"];
const gauges = Object.fromEntries(namesOfKind("gauge").map((name) => [name, Metric.gauge(name)])) as MetricRegistryShape["gauges"];

const webSdkLayer = (endpoint: string, sampleRatio: number): Layer.Layer<Resource> =>
  WebSdk.layer(() => ({
    resource: { serviceName: "rasm-web", attributes: { [ATTR_SERVICE_NAMESPACE]: "rasm" } },
    spanProcessor: new BatchSpanProcessor(new OTLPTraceExporter({ url: `${endpoint}/v1/traces` })),
    metricReader: new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter({ url: `${endpoint}/v1/metrics` }) }),
    tracerConfig: { sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(sampleRatio) }) },
  }));

const webSdk: Layer.Layer<Resource, never, RuntimeConfig> = Layer.unwrapEffect(
  Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const endpoint = yield* config.collectorOtlpEndpoint.pipe(Effect.orElseSucceed(() => "http://localhost:4318"));
    const sampleRatio = yield* config.traceSampleRatio.pipe(Effect.orElseSucceed(() => 1));
    return webSdkLayer(endpoint, sampleRatio);
  }),
);

// --- [SERVICES] ------------------------------------------------------------------------
class MetricRegistry extends Effect.Service<MetricRegistry>()("@rasm/ts/platform/MetricRegistry", {
  sync: (): MetricRegistryShape => ({
    counters,
    histograms,
    gauges,
    span: (name, effect) => effect.pipe(Effect.withSpan(name)),
    webSdk,
  }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------
const MetricRegistryLive: Layer.Layer<MetricRegistry> = MetricRegistry.Default;

// --- [EXPORTS] -------------------------------------------------------------------------
export type { CounterKey, GaugeKey, HistogramKey, SpanName, VitalKey };
export { MetricRegistry, MetricRegistryLive };
```

## [3]-[RESEARCH]

- [HISTOGRAM_BOUNDARIES]: `Metric.histogram(name, boundaries)` requires a `MetricBoundaries` second argument the `.api` `effect.md` `[8]-[OBSERVABILITY]` `Metric` section confirms by type but whose constructor (`Metric.boundaries.exponential`/`Metric.boundaries.linear`/`Metric.boundaries.fromIterable`) is unverified against the catalogue; `HISTOGRAM_BOUNDARIES` is the one bucket-schedule row every `*_ms`/`*_duration` histogram reads, its constructor spelling resolved against the `effect/MetricBoundaries` module before transcription — a per-histogram inline boundary set is the rejected form. The instrument records, the `span` `Effect.withSpan` projector, the `webSdk` layer, and the `Effect.Service` surface are settled.
