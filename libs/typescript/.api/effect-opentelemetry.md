# [API_CATALOGUE] @effect/opentelemetry

Grounded from installed `node_modules` type declarations (`@effect/opentelemetry` 0.63.0, peers
`@opentelemetry/api` ^1.9, `@opentelemetry/resources` ^2.0.0, `@opentelemetry/sdk-logs`
`>=0.203.0 <0.300.0`, `@opentelemetry/sdk-metrics` ^2.0.0, `@opentelemetry/sdk-trace-base` ^2.0.0,
`@opentelemetry/sdk-trace-node` ^2.0.0, `@opentelemetry/sdk-trace-web` ^2.0.0,
`@opentelemetry/semantic-conventions` ^1.33.0, `@effect/platform` ^0.96.0, `effect` ^3.21.0). Every
surface below is reflected from `dist/dts/*.d.ts` with exact spellings. The package barrel
`@effect/opentelemetry` re-exports each module under its own namespace
(`import { NodeSdk } from "@effect/opentelemetry"`); per-module deep imports
(`@effect/opentelemetry/Tracer`) resolve the same declarations. This is the OpenTelemetry export
bridge for the Effect runtime: it adapts the `effect/Tracer`, `effect/Logger`, and `effect/Metric`
channels onto OTLP wire export — either through the official OpenTelemetry SDK (`NodeSdk` / `WebSdk`
over `@opentelemetry/sdk-*`) or through the dependency-light direct-OTLP module family (`Otlp` +
`OtlpTracer` / `OtlpLogger` / `OtlpMetrics` over `@effect/platform`'s `HttpClient`). All effect-side
spans/logs/metrics produced inside the runtime export through one of these two paths; nothing here
adds an alternative telemetry vocabulary.

---

## [01]-[PACKAGE_SURFACE]

```ts
// @effect/opentelemetry — namespace re-exports (index.d.ts)
export * as Logger from "./Logger.js"
export * as Metrics from "./Metrics.js"
export * as NodeSdk from "./NodeSdk.js"
export * as Otlp from "./Otlp.js"
export * as OtlpLogger from "./OtlpLogger.js"
export * as OtlpMetrics from "./OtlpMetrics.js"
export * as OtlpResource from "./OtlpResource.js"
export * as OtlpSerialization from "./OtlpSerialization.js"
export * as OtlpTracer from "./OtlpTracer.js"
export * as Resource from "./Resource.js"
export * as Tracer from "./Tracer.js"
export * as WebSdk from "./WebSdk.js"
```

| [INDEX] | [MODULE]            | [SECTION] | [PRIMARY ROLE]                                                   |
| :-----: | :------------------ | :-------- | :--------------------------------------------------------------- |
|  [01]   | `Resource`          | [02]      | OTel `Resource` tag + service-identity layers (SDK path)         |
|  [02]   | `Tracer`            | [03]      | `effect/Tracer` -> OTel tracer bridge, span-context propagation  |
|  [03]   | `Logger`            | [04]      | `effect/Logger` -> OTel `LoggerProvider` bridge                  |
|  [04]   | `Metrics`           | [05]      | `effect/Metric` -> OTel `MetricProducer`/`MetricReader` bridge   |
|  [05]   | `NodeSdk`           | [06]      | composite node SDK layer (tracer + metrics + logs in one config) |
|  [06]   | `WebSdk`            | [07]      | composite web SDK layer (browser tracer provider)                |
|  [07]   | `Otlp`              | [08]      | dependency-light direct-OTLP composite layer over `HttpClient`   |
|  [08]   | `OtlpTracer`        | [09]      | direct-OTLP tracer constructor/layer                             |
|  [09]   | `OtlpLogger`        | [10]      | direct-OTLP logger constructor/layer                             |
|  [10]   | `OtlpMetrics`       | [11]      | direct-OTLP metrics constructor/layer                            |
|  [11]   | `OtlpSerialization` | [12]      | tree-shakable JSON/protobuf `HttpBody` encoder service           |
|  [12]   | `OtlpResource`      | [13]      | OTLP wire resource/attribute model + attribute coercion          |

Two distinct export paths share the resource/attribute vocabulary but not the layers:
- SDK path (`Resource` + `Tracer`/`Logger`/`Metrics` + `NodeSdk`/`WebSdk`) requires the official
  `@opentelemetry/sdk-*` peers and an OTel `Resource` tag in context.
- Direct-OTLP path (`Otlp` + `OtlpTracer`/`OtlpLogger`/`OtlpMetrics` + `OtlpSerialization` +
  `OtlpResource`) requires only `@effect/platform`'s `HttpClient` + an `OtlpSerialization` layer;
  it carries its own resource model and never touches `@opentelemetry/sdk-*`.

---

## [02]-[RESOURCE]

OTel-SDK service-identity resource. The `Resource` tag holds an `@opentelemetry/resources`
`Resource` and is the shared requirement of every SDK-path layer.

```ts
// @effect/opentelemetry/Resource
import * as OtelApi from "@opentelemetry/api"
import * as Resources from "@opentelemetry/resources"

export interface Resource { readonly _: unique symbol }
export const Resource: Context.Tag<Resource, Resources.Resource>

export const layer: (config: {
  readonly serviceName: string
  readonly serviceVersion?: string
  readonly attributes?: OtelApi.Attributes
}) => Layer.Layer<Resource, never, never>

export const configToAttributes: (options: {
  readonly serviceName: string
  readonly serviceVersion?: string
  readonly attributes?: OtelApi.Attributes
}) => Record<string, string>

export const layerFromEnv: (additionalAttributes?: OtelApi.Attributes | undefined) => Layer.Layer<Resource>
export const layerEmpty: Layer.Layer<Resource, never, never>
```

- `layer` is the explicit service-identity binding; `layerFromEnv` reads `OTEL_*` environment
  resource attributes; `layerEmpty` (since 2.0.0) is the no-attribute identity for tests.

---

## [03]-[TRACER]

Bridges the host `effect/Tracer` channel onto an OpenTelemetry tracer. `OtelTracer`,
`OtelTracerProvider`, `OtelTraceFlags`, `OtelTraceState` are `Context.Tag`s wrapping the raw
`@opentelemetry/api` handles.

```ts
// @effect/opentelemetry/Tracer
import * as Otel from "@opentelemetry/api"
import { NoSuchElementException } from "effect/Cause"
import { ExternalSpan, ParentSpan, Tracer as EffectTracer } from "effect/Tracer"
import { Resource } from "./Resource.js"

export const make: Effect<EffectTracer, never, OtelTracer>

export const makeExternalSpan: (options: {
  readonly traceId: string
  readonly spanId: string
  readonly traceFlags?: number | undefined
  readonly traceState?: string | Otel.TraceState | undefined
}) => ExternalSpan

export const currentOtelSpan: Effect<Otel.Span, NoSuchElementException>

export const layerWithoutOtelTracer: Layer<never, never, OtelTracer>
export const layer: Layer<OtelTracer, never, Resource | OtelTracerProvider>
export const layerGlobal: Layer<OtelTracer, never, Resource>
export const layerTracer: Layer<OtelTracer, never, Resource | OtelTracerProvider>
export const layerGlobalTracer: Layer<OtelTracer, never, Resource>

export interface OtelTracerProvider { readonly _: unique symbol }
export const OtelTracerProvider: Tag<OtelTracerProvider, Otel.TracerProvider>
export interface OtelTracer { readonly _: unique symbol }
export const OtelTracer: Tag<OtelTracer, Otel.Tracer>
export interface OtelTraceFlags { readonly _: unique symbol }
export const OtelTraceFlags: Tag<OtelTraceFlags, Otel.TraceFlags>
export interface OtelTraceState { readonly _: unique symbol }
export const OtelTraceState: Tag<OtelTraceState, Otel.TraceState>

export const withSpanContext: {
  (spanContext: Otel.SpanContext): <A, E, R>(effect: Effect<A, E, R>) => Effect<A, E, Exclude<R, ParentSpan>>
  <A, E, R>(effect: Effect<A, E, R>, spanContext: Otel.SpanContext): Effect<A, E, Exclude<R, ParentSpan>>
}
```

- `layer` / `layerTracer` need a `Resource` + an `OtelTracerProvider` (supplied by
  `NodeSdk.layerTracerProvider` / `WebSdk.layerTracerProvider`); `layerGlobal` /
  `layerGlobalTracer` read the globally-registered OTel provider instead, needing only `Resource`.
- `withSpanContext` is the seam for attaching an effect to a parent span created outside Effect;
  `makeExternalSpan` builds an `ExternalSpan` from raw ids.

---

## [04]-[LOGGER]

Bridges `effect/Logger` onto an OTel `LoggerProvider`. `OtelLoggerProvider` is a `TagClass`
wrapping `@opentelemetry/sdk-logs` `LoggerProvider`.

```ts
// @effect/opentelemetry/Logger
import * as Otel from "@opentelemetry/sdk-logs"
import { NonEmptyReadonlyArray } from "effect/Array"
import { DurationInput } from "effect/Duration"
import { Resource } from "./Resource.js"

export declare class OtelLoggerProvider extends Context.TagClass<
  OtelLoggerProvider, "@effect/opentelemetry/Logger/OtelLoggerProvider", Otel.LoggerProvider
>() {}

export const make: Effect.Effect<Logger.Logger<unknown, void>, never, OtelLoggerProvider>
export const layerLoggerAdd: Layer.Layer<never, never, OtelLoggerProvider>
export const layerLoggerReplace: Layer.Layer<never, never, OtelLoggerProvider>
export const layerLoggerProvider: (
  processor: Otel.LogRecordProcessor | NonEmptyReadonlyArray<Otel.LogRecordProcessor>,
  config?: Omit<Otel.LoggerProviderConfig, "resource"> & { readonly shutdownTimeout?: DurationInput | undefined }
) => Layer.Layer<OtelLoggerProvider, never, Resource>
```

- `layerLoggerAdd` appends the OTel logger alongside the existing effect loggers;
  `layerLoggerReplace` swaps it in. `layerLoggerProvider` builds the `OtelLoggerProvider` from a
  `LogRecordProcessor` (needs `Resource`).

---

## [05]-[METRICS]

Bridges `effect/Metric` onto an OTel `MetricProducer`/`MetricReader`. Module imports are all
type-only against `@opentelemetry/sdk-metrics`.

```ts
// @effect/opentelemetry/Metrics
import { MetricProducer, MetricReader } from "@opentelemetry/sdk-metrics"
import { NonEmptyReadonlyArray } from "effect/Array"
import { DurationInput } from "effect/Duration"
import { LazyArg } from "effect/Function"
import { Resource } from "./Resource.js"

export const makeProducer: Effect.Effect<MetricProducer, never, Resource>
export const registerProducer: (
  self: MetricProducer,
  metricReader: LazyArg<MetricReader | NonEmptyReadonlyArray<MetricReader>>
) => Effect.Effect<Array<any>, never, Scope.Scope>
export const layer: (
  evaluate: LazyArg<MetricReader | NonEmptyReadonlyArray<MetricReader>>,
  options?: { readonly shutdownTimeout?: DurationInput | undefined }
) => Layer<never, never, Resource>
```

- `layer` is the production entry: it registers a `MetricProducer` (from effect metrics) against
  the supplied `MetricReader`(s). `makeProducer` + `registerProducer` are the unbundled steps.

---

## [06]-[NODESDK]

Composite node SDK layer: one `Configuration` wires tracer, metrics, and logs together over the
official `@opentelemetry/sdk-*` peers. Produces a `Resource.Resource` layer.

```ts
// @effect/opentelemetry/NodeSdk
import * as OtelApi from "@opentelemetry/api"
import { LoggerProviderConfig, LogRecordProcessor } from "@opentelemetry/sdk-logs"
import { MetricReader } from "@opentelemetry/sdk-metrics"
import { SpanProcessor, TracerConfig } from "@opentelemetry/sdk-trace-base"
import { NonEmptyReadonlyArray } from "effect/Array"
import { DurationInput } from "effect/Duration"
import { LazyArg } from "effect/Function"
import * as Resource from "./Resource.js"
import * as Tracer from "./Tracer.js"

export interface Configuration {
  readonly spanProcessor?: SpanProcessor | ReadonlyArray<SpanProcessor> | undefined
  readonly tracerConfig?: Omit<TracerConfig, "resource"> | undefined
  readonly metricReader?: MetricReader | ReadonlyArray<MetricReader> | undefined
  readonly logRecordProcessor?: LogRecordProcessor | ReadonlyArray<LogRecordProcessor> | undefined
  readonly loggerProviderConfig?: Omit<LoggerProviderConfig, "resource"> | undefined
  readonly resource?: {
    readonly serviceName: string
    readonly serviceVersion?: string
    readonly attributes?: OtelApi.Attributes
  } | undefined
  readonly shutdownTimeout?: DurationInput | undefined
}

export const layerTracerProvider: (
  processor: SpanProcessor | NonEmptyReadonlyArray<SpanProcessor>,
  config?: Omit<TracerConfig, "resource"> & { readonly shutdownTimeout?: DurationInput | undefined }
) => Layer.Layer<Tracer.OtelTracerProvider, never, Resource.Resource>

export const layer: {
  (evaluate: LazyArg<Configuration>): Layer.Layer<Resource.Resource>
  <R, E>(evaluate: Effect.Effect<Configuration, E, R>): Layer.Layer<Resource.Resource, E, R>
}
export const layerEmpty: Layer.Layer<Resource.Resource>
```

- `layer` is the one-call node telemetry root; the `Configuration.resource` field carries
  service-identity inline, so `NodeSdk.layer` supplies the `Resource.Resource` requirement that
  `Tracer.layer` / `Metrics.layer` / `Logger.layerLoggerProvider` consume.

---

## [07]-[WEBSDK]

Composite web SDK layer; mirrors `NodeSdk` but `resource` is required (not optional) and there is
no `shutdownTimeout`. Produces a `Resource.Resource` layer.

```ts
// @effect/opentelemetry/WebSdk
import * as OtelApi from "@opentelemetry/api"
import { LoggerProviderConfig, LogRecordProcessor } from "@opentelemetry/sdk-logs"
import { MetricReader } from "@opentelemetry/sdk-metrics"
import { SpanProcessor, TracerConfig } from "@opentelemetry/sdk-trace-base"
import { NonEmptyReadonlyArray } from "effect/Array"
import { LazyArg } from "effect/Function"
import * as Resource from "./Resource.js"
import * as Tracer from "./Tracer.js"

export interface Configuration {
  readonly spanProcessor?: SpanProcessor | ReadonlyArray<SpanProcessor> | undefined
  readonly tracerConfig?: Omit<TracerConfig, "resource">
  readonly metricReader?: MetricReader | ReadonlyArray<MetricReader> | undefined
  readonly logRecordProcessor?: LogRecordProcessor | ReadonlyArray<LogRecordProcessor> | undefined
  readonly loggerProviderConfig?: Omit<LoggerProviderConfig, "resource"> | undefined
  readonly resource: {
    readonly serviceName: string
    readonly serviceVersion?: string
    readonly attributes?: OtelApi.Attributes
  }
}

export const layerTracerProvider: (
  processor: SpanProcessor | NonEmptyReadonlyArray<SpanProcessor>,
  config?: Omit<TracerConfig, "resource">
) => Layer.Layer<Tracer.OtelTracerProvider, never, Resource.Resource>

export const layer: {
  (evaluate: LazyArg<Configuration>): Layer.Layer<Resource.Resource>
  <E, R>(evaluate: Effect.Effect<Configuration, E, R>): Layer.Layer<Resource.Resource, E, R>
}
```

---

## [08]-[OTLP]

Dependency-light direct-OTLP composite. One `layer` exports tracer + logger + metrics over an
`HttpClient` (+ an `OtlpSerialization` layer); `layerJson` / `layerProtobuf` bundle their own
serializer so they need only `HttpClient`.

```ts
// @effect/opentelemetry/Otlp
import { Headers } from "@effect/platform/Headers"
import { HttpClient } from "@effect/platform/HttpClient"
import { Duration } from "effect/Duration"
import { Logger } from "effect/Logger"
import { Tracer } from "effect/Tracer"
import * as OtlpSerialization from "./OtlpSerialization.js"

type OtlpOptions = {
  readonly baseUrl: string
  readonly resource?: {
    readonly serviceName?: string | undefined
    readonly serviceVersion?: string | undefined
    readonly attributes?: Record<string, unknown>
  }
  readonly headers?: Headers.Input | undefined
  readonly maxBatchSize?: number | undefined
  readonly replaceLogger?: Logger.Logger<any, any> | undefined
  readonly tracerContext?: (<X>(f: () => X, span: Tracer.AnySpan) => X) | undefined
  readonly loggerExportInterval?: Duration.DurationInput | undefined
  readonly loggerExcludeLogSpans?: boolean | undefined
  readonly metricsExportInterval?: Duration.DurationInput | undefined
  readonly tracerExportInterval?: Duration.DurationInput | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
}

export const layer: (options: OtlpOptions) =>
  Layer.Layer<never, never, HttpClient.HttpClient | OtlpSerialization.OtlpSerialization>
export const layerJson: (options: OtlpOptions) => Layer.Layer<never, never, HttpClient.HttpClient>
export const layerProtobuf: (options: OtlpOptions) => Layer.Layer<never, never, HttpClient.HttpClient>
```

- `layer` leaves serializer selection open (requires an external `OtlpSerialization`);
  `layerJson` folds `OtlpSerialization.layerJson` and `layerProtobuf` folds
  `OtlpSerialization.layerProtobuf`, so each needs only an `HttpClient`. `baseUrl` is the collector
  root; the module derives `/v1/traces`, `/v1/logs`, `/v1/metrics` sub-paths internally.

---

## [09]-[OTLP_TRACER]

```ts
// @effect/opentelemetry/OtlpTracer
import { Headers } from "@effect/platform/Headers"
import { HttpClient } from "@effect/platform/HttpClient"
import { Duration } from "effect/Duration"
import * as Tracer from "effect/Tracer"
import { OtlpSerialization } from "./OtlpSerialization.js"

type Options = {
  readonly url: string
  readonly resource?: {
    readonly serviceName?: string | undefined
    readonly serviceVersion?: string | undefined
    readonly attributes?: Record<string, unknown>
  } | undefined
  readonly headers?: Headers.Input | undefined
  readonly exportInterval?: Duration.DurationInput | undefined
  readonly maxBatchSize?: number | undefined
  readonly context?: (<X>(f: () => X, span: Tracer.AnySpan) => X) | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
}

export const make: (options: Options) =>
  Effect.Effect<Tracer.Tracer, never, HttpClient.HttpClient | OtlpSerialization | Scope.Scope>
export const layer: (options: Options) =>
  Layer.Layer<never, never, HttpClient.HttpClient | OtlpSerialization>
```

---

## [10]-[OTLP_LOGGER]

```ts
// @effect/opentelemetry/OtlpLogger
import { Headers } from "@effect/platform/Headers"
import { HttpClient } from "@effect/platform/HttpClient"
import { Duration } from "effect/Duration"
import { Logger } from "effect/Logger"
import { OtlpSerialization } from "./OtlpSerialization.js"

export const make: (options: {
  readonly url: string
  readonly resource?: { readonly serviceName?: string | undefined; readonly serviceVersion?: string | undefined; readonly attributes?: Record<string, unknown> } | undefined
  readonly headers?: Headers.Input | undefined
  readonly exportInterval?: Duration.DurationInput | undefined
  readonly maxBatchSize?: number | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
  readonly excludeLogSpans?: boolean | undefined
}) => Effect.Effect<Logger.Logger<unknown, void>, never, HttpClient.HttpClient | OtlpSerialization | Scope.Scope>

export const layer: (options: {
  readonly url: string
  readonly resource?: { readonly serviceName?: string | undefined; readonly serviceVersion?: string | undefined; readonly attributes?: Record<string, unknown> } | undefined
  readonly replaceLogger?: Logger.Logger<any, any> | undefined
  readonly headers?: Headers.Input | undefined
  readonly exportInterval?: Duration.DurationInput | undefined
  readonly maxBatchSize?: number | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
  readonly excludeLogSpans?: boolean | undefined
}) => Layer.Layer<never, never, HttpClient.HttpClient | OtlpSerialization>
```

- `make` produces a `Logger.Logger`; `layer` registers it (with optional `replaceLogger` swap).
  `excludeLogSpans` drops span-event log records to avoid double export.

---

## [11]-[OTLP_METRICS]

```ts
// @effect/opentelemetry/OtlpMetrics
import { Headers } from "@effect/platform/Headers"
import { HttpClient } from "@effect/platform/HttpClient"
import { Duration } from "effect/Duration"
import { OtlpSerialization } from "./OtlpSerialization.js"

export const make: (options: {
  readonly url: string
  readonly resource?: { readonly serviceName?: string | undefined; readonly serviceVersion?: string | undefined; readonly attributes?: Record<string, unknown> } | undefined
  readonly headers?: Headers.Input | undefined
  readonly exportInterval?: Duration.DurationInput | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
}) => Effect.Effect<void, never, HttpClient.HttpClient | OtlpSerialization | Scope.Scope>

export const layer: (options: {
  readonly url: string
  readonly resource?: { readonly serviceName?: string | undefined; readonly serviceVersion?: string | undefined; readonly attributes?: Record<string, unknown> } | undefined
  readonly headers?: Headers.Input | undefined
  readonly exportInterval?: Duration.DurationInput | undefined
  readonly shutdownTimeout?: Duration.DurationInput | undefined
}) => Layer.Layer<never, never, HttpClient.HttpClient | OtlpSerialization>
```

- No `maxBatchSize` (metrics export on `exportInterval` only); `make` yields `void` (registers the
  effect-metrics reader as a side effect of acquisition).

---

## [12]-[OTLP_SERIALIZATION]

Tree-shakable encoder service abstracting OTLP `HttpBody` encoding. `OtlpSerialization` is a
`TagClass`; the two `layer*` constants select JSON vs protobuf wire format.

```ts
// @effect/opentelemetry/OtlpSerialization
import * as HttpBody from "@effect/platform/HttpBody"

export declare class OtlpSerialization extends Context.TagClass<
  OtlpSerialization, "@effect/opentelemetry/OtlpSerialization", {
    readonly traces: (data: unknown) => HttpBody.HttpBody
    readonly metrics: (data: unknown) => HttpBody.HttpBody
    readonly logs: (data: unknown) => HttpBody.HttpBody
  }
>() {}

export const layerJson: Layer.Layer<OtlpSerialization>
export const layerProtobuf: Layer.Layer<OtlpSerialization>
```

- This is the swap point that lets `Otlp.layer` / `OtlpTracer.layer` / `OtlpLogger.layer` /
  `OtlpMetrics.layer` stay format-agnostic: provide `layerJson` or `layerProtobuf` to satisfy the
  `OtlpSerialization` requirement.

---

## [13]-[OTLP_RESOURCE]

Plain OTLP wire resource/attribute model (independent of `@opentelemetry/resources`) plus
attribute-coercion helpers used by the direct-OTLP path.

```ts
// @effect/opentelemetry/OtlpResource
export interface Resource {
  attributes: Array<KeyValue>
  droppedAttributesCount: number
}
export const make: (options: {
  readonly serviceName: string
  readonly serviceVersion?: string | undefined
  readonly attributes?: Record<string, unknown> | undefined
}) => Resource
export const fromConfig: (options?: {
  readonly serviceName?: string | undefined
  readonly serviceVersion?: string | undefined
  readonly attributes?: Record<string, unknown> | undefined
} | undefined) => Effect.Effect<Resource>
export const unsafeServiceName: (resource: Resource) => string
export const entriesToAttributes: (entries: Iterable<[string, unknown]>) => Array<KeyValue>
export const unknownToAttributeValue: (value: unknown) => AnyValue

export interface KeyValue { key: string; value: AnyValue }
export interface AnyValue {
  stringValue?: string | null
  boolValue?: boolean | null
  intValue?: number | null
  doubleValue?: number | null
  arrayValue?: ArrayValue
  kvlistValue?: KeyValueList
  bytesValue?: Uint8Array
}
export interface ArrayValue { values: Array<AnyValue> }
export interface KeyValueList { values: Array<KeyValue> }
export interface LongBits { low: number; high: number }
export type Fixed64 = LongBits | string | number
```

- `unknownToAttributeValue` is the canonical JS-value -> OTLP `AnyValue` coercion; `Fixed64` is the
  protobuf 64-bit wire encoding (`LongBits` low/high pair or string/number).

---

## [14]-[IMPLEMENTATION_LAW]

[EXPORT_PATH_CHOICE]:
- Telemetry export is exactly two layer families and one shared resource vocabulary. The SDK path
  (`NodeSdk.layer` / `WebSdk.layer`) is chosen when the official `@opentelemetry/sdk-*` collectors,
  span processors, or interop with non-Effect OTel instrumentation are required; it supplies the
  `Resource.Resource` tag that `Tracer.layer`, `Metrics.layer`, and `Logger.layerLoggerProvider`
  consume. The direct-OTLP path (`Otlp.layerJson` / `layerProtobuf`, or the per-signal
  `OtlpTracer`/`OtlpLogger`/`OtlpMetrics` layers) is chosen when only `@effect/platform`'s
  `HttpClient` is admissible (no `@opentelemetry/sdk-*` peers) — a single `baseUrl` collector. A
  hand-rolled OTLP HTTP exporter outside these layers is the deleted form.

[SERIALIZER_SEAM]:
- `OtlpSerialization` is the one wire-format swap. `Otlp.layer` and every per-signal `*.layer` /
  `*.make` on the direct path require an `OtlpSerialization` in context; `Otlp.layerJson` /
  `layerProtobuf` fold the serializer in so the only residual requirement is `HttpClient`. Format
  selection lives in the layer wiring, never in a parallel encoder type.

[SPAN_PROPAGATION]:
- `Tracer.currentOtelSpan` reads the active OTel span under both paths (the OTLP path returns an
  OTel-`Span`-conformant wrapper). `Tracer.withSpanContext` attaches an effect to a parent span
  created outside Effect; `Tracer.makeExternalSpan` builds an `ExternalSpan` from raw
  trace/span ids. These are the only seams to non-Effect OTel context — no manual `traceparent`
  parsing.

[TIER_LAW]:
- `NodeSdk` is the node-bundle composite (`@opentelemetry/sdk-trace-node` peer); `WebSdk` is the
  browser-bundle composite (`@opentelemetry/sdk-trace-web` peer) with a required `resource`. The
  direct-OTLP family and `OtlpResource`/`OtlpSerialization` are neutral (only `@effect/platform`
  + `effect`), composable in either bundle wherever an `HttpClient` binding exists.
- `Logger`/`Metrics` SDK bridges depend on `@opentelemetry/sdk-logs` / `@opentelemetry/sdk-metrics`
  type surfaces and ride the SDK path; the direct-OTLP `OtlpLogger`/`OtlpMetrics` are the neutral
  equivalents.
