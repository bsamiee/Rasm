# [API_CATALOGUE] @opentelemetry/core

`@opentelemetry/core` is the OpenTelemetry SDK utility layer — NOT a propagator-only package. Its DIRECT-consumed surface is the W3C propagation family (`W3CTraceContextPropagator`/`W3CBaggagePropagator` fanned through `CompositePropagator`, `parseTraceParent`, the `TRACE_PARENT_HEADER`/`TRACE_STATE_HEADER` constants, the W3C `TraceState` class) that `Observability/telemetry#TRACE_PROPAGATION` registers via `WebTracerProvider.register({ propagator })`. Underneath sits the substrate every SDK-bridge exporter/reader/processor rides: the `ExportResult`/`ExportResultCode` callback contract (the shape every `SpanExporter.export`/`PushMetricExporter.export` result flows through), the `suppressTracing`/`isTracingSuppressed` context guard that stops the OTLP egress from tracing itself, the `HrTime` clock algebra, the `OTEL_*` env readers backing `RuntimeConfig`, and the `TimeoutError`/`BindOnceFuture` lifecycle primitives shared with `sdk-metrics`. The propagators implement the `@opentelemetry/api` `TextMapPropagator` triple; registration is the consumer's seam choice (`WebSdk.Configuration` carries NO `propagator` field), realized on a SELF-CONSTRUCTED `sdk-trace-web` `WebTracerProvider.register({ propagator })` or `@opentelemetry/api` `propagation.setGlobalPropagator` — never a WebSdk-built provider handle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/core`
- package: `@opentelemetry/core`
- version: `2.8.0` (`build/src/version.d.ts` `VERSION = "2.8.0"`; central pin `pnpm-workspace.yaml`)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.9.x` — `Context`/`SpanContext`/`TextMapPropagator`/`TextMapGetter`/`TextMapSetter`/`HrTime`/`Attributes`/`Baggage`/`DiagLogLevel`/`Exception` are all api types
- module: `@opentelemetry/core` (barrel `build/src/index.d.ts`; the `browser` package.json field swaps `platform/index` -> `platform/browser/index` at bundle time)
- runtime: dual — browser + node; the `platform` split resolves per build, `SDK_INFO["telemetry.sdk.language"]` is `"webjs"` in the browser build
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: observability
- collapse-fence: `[R3]`-SURVIVOR (partial) — the W3C propagation family (`W3CTraceContextPropagator`/`W3CBaggagePropagator`/`CompositePropagator`/`TraceState`) is a DIRECT-import survivor registration needs regardless of lane, so it PERSISTS when the native `@effect/opentelemetry` `Otlp` lane retires the sdk-*/exporter machinery (`libs/typescript/.api/effect-opentelemetry.md`); the utility substrate consumed TRANSITIVELY via `sdk-*` rides that collapse; the `edge/live` ledger fences `@opentelemetry/*` to `scope:telemetry` only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: W3C propagation family (direct-consumed)
- rail: observability
- The `CompositePropagator` OWNS the propagator list; a new wire format (B3, Jaeger) is one more `propagators[]` entry, never a parallel registration. `TraceState` is the concrete W3C `tracestate` list the `@effect/opentelemetry` `Tracer.OtelTraceState` tag and `makeExternalSpan`'s `traceState` carry.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                              |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `W3CTraceContextPropagator` | class         | `TextMapPropagator` for `traceparent`/`tracestate`                |
|  [02]   | `W3CBaggagePropagator`      | class         | `TextMapPropagator` for the W3C `baggage` header                  |
|  [03]   | `CompositePropagator`       | class         | ordered fan-out of a `TextMapPropagator[]` as one handle          |
|  [04]   | `CompositePropagatorConfig` | interface     | `{ propagators?: TextMapPropagator[] }` — later entry wins on key |
|  [05]   | `TraceState`                | class         | W3C `tracestate`: `set`/`unset`/`get`/`serialize`, immutable-copy |
|  [06]   | `parseTraceParent`          | function      | `(traceParent: string) => SpanContext \| null`; never throws     |
|  [07]   | `TRACE_PARENT_HEADER`       | const         | `"traceparent"` — the canonical header name, never a literal      |
|  [08]   | `TRACE_STATE_HEADER`        | const         | `"tracestate"` — the canonical header name, never a literal       |
|  [09]   | `parseKeyPairsIntoRecord`   | function      | `(value?: string) => Record<string,string>` W3C baggage parse    |

[PUBLIC_TYPE_SCOPE]: export-result + lifecycle contract family
- rail: observability
- `ExportResult` is THE callback shape every sibling exporter reports through (`sdk-trace-base` `SpanExporter.export(spans, cb)`, `sdk-metrics` `PushMetricExporter.export(metrics, cb)`, `exporter-*-otlp-http`); catalog these here so the exporter catalogs point at one owner. `BindOnceFuture` makes `forceFlush`/`shutdown` idempotent; `TimeoutError`/`callWithTimeout` bound every async drain and are re-exported verbatim by `sdk-metrics`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                              |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ExportResult`     | interface     | `{ code: ExportResultCode; error?: Error }` — exporter callback   |
|  [02]   | `ExportResultCode` | enum          | `SUCCESS = 0`, `FAILED = 1`                                       |
|  [03]   | `TimeoutError`     | class         | `extends Error`; thrown on drain timeout (re-exported by metrics) |
|  [04]   | `callWithTimeout`  | function      | `<T>(promise: Promise<T>, timeout: number) => Promise<T>`        |
|  [05]   | `BindOnceFuture`   | class         | once-future: `isCalled`/`promise`/`call(...)` for flush/shutdown  |

[PUBLIC_TYPE_SCOPE]: context-key utility family
- rail: observability
- Two small families keyed onto the api `Context`. `suppressTracing` is load-bearing for the export edge: wrapping the OTLP `HttpClient` call in a suppressed context stops the exporter's own fetch from minting a span (the telemetry-of-telemetry loop). `RPCMetadata` carries the active HTTP route/span onto context for instrumentation enrichment.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                    |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `suppressTracing` / `unsuppressTracing`         | function      | `(context) => Context` toggle the no-trace context flag |
|  [02]   | `isTracingSuppressed`                            | function      | `(context) => boolean` guard read at span-start         |
|  [03]   | `RPCType`                                        | enum          | `HTTP = "http"` — the RPC-metadata discriminant         |
|  [04]   | `RPCMetadata`                                    | type          | `{ type: RPCType.HTTP; route?: string; span: Span }`    |
|  [05]   | `getRPCMetadata` / `setRPCMetadata` / `deleteRPCMetadata` | function | context RPC-metadata read/write/clear             |

[PUBLIC_TYPE_SCOPE]: time + attribute + env + error utility family
- rail: observability
- The `HrTime` clock algebra is one conversion family over the api `HrTime = [seconds, nanos]` pair — a new unit is a `hrTimeTo*` sibling, never an ad-hoc `Date.now()` at a sink. `AnchoredClock` prevents span end-before-start drift. The env readers are the typed `OTEL_*` layer `RuntimeConfig` mirrors; `sanitizeAttributes` is the boundary every raw attribute map crosses before it becomes a `ReadableSpan`/`MetricData` attribute set.

| [INDEX] | [SYMBOL]                                                                       | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :----------------------------------------------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `hrTime` / `millisToHrTime` / `timeInputToHrTime` / `hrTimeDuration` / `addHrTimes` | function | `TimeInput`/millis/perf-now -> api `HrTime`; duration + sum   |
|  [02]   | `hrTimeToTimeStamp` / `hrTimeToNanoseconds` / `hrTimeToMicroseconds` / `hrTimeToMilliseconds` / `hrTimeToSeconds` | function | project `HrTime` to string/number units |
|  [03]   | `isTimeInput` / `isTimeInputHrTime`                                             | type guard    | narrow `unknown` to `TimeInput` / `HrTime`                    |
|  [04]   | `AnchoredClock` / `Clock`                                                       | class / iface | `{ now(): number }` monotonic wall-clock anchored per trace   |
|  [05]   | `getStringFromEnv` / `getNumberFromEnv` / `getBooleanFromEnv` / `getStringListFromEnv` | function | typed `OTEL_*` env readers backing `RuntimeConfig`      |
|  [06]   | `SDK_INFO` / `_globalThis` / `otperformance`                                    | const         | `telemetry.sdk.*` identity map; global + performance handles  |
|  [07]   | `sanitizeAttributes` / `isAttributeValue`                                       | function      | attribute-map coercion / `AttributeValue` narrowing           |
|  [08]   | `setGlobalErrorHandler` / `globalErrorHandler` / `loggingErrorHandler` / `ErrorHandler` | function / type | global SDK exception sink; default logging handler   |
|  [09]   | `InstrumentationScope`                                                          | interface     | `{ name; version?; schemaUrl? }` scope on span/metric records |
|  [10]   | `merge` / `urlMatches` / `isUrlIgnored` / `diagLogLevelFromString`              | function      | deep merge; URL match/ignore for propagation targets; diag lvl |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: W3C trace-context propagator + tracestate
- rail: observability

```ts
// build/src/trace/W3CTraceContextPropagator.d.ts
export declare const TRACE_PARENT_HEADER = "traceparent";
export declare const TRACE_STATE_HEADER = "tracestate";

export declare function parseTraceParent(traceParent: string): SpanContext | null;

export declare class W3CTraceContextPropagator implements TextMapPropagator {
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}

// build/src/trace/TraceState.d.ts — immutable W3C tracestate list (mutators return a new TraceState)
export declare class TraceState {
  constructor(rawTraceState?: string);
  set(key: string, value: string): TraceState;
  unset(key: string): TraceState;
  get(key: string): string | undefined;
  serialize(): string;
}
```

[ENTRYPOINT_SCOPE]: W3C baggage propagator
- rail: observability

```ts
// build/src/baggage/propagation/W3CBaggagePropagator.d.ts
export declare class W3CBaggagePropagator implements TextMapPropagator {
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}
```

[ENTRYPOINT_SCOPE]: composite propagator — ordered fan-out
- rail: observability

```ts
// build/src/propagation/composite.d.ts — propagators run in list order; a later entry writing the same key wins
export interface CompositePropagatorConfig {
  propagators?: TextMapPropagator[];
}

export declare class CompositePropagator implements TextMapPropagator {
  constructor(config?: CompositePropagatorConfig);
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}
```

[ENTRYPOINT_SCOPE]: export-result contract + lifecycle primitives
- rail: observability

```ts
// build/src/ExportResult.d.ts — the callback shape every SpanExporter/PushMetricExporter reports through
export interface ExportResult { code: ExportResultCode; error?: Error; }
export declare enum ExportResultCode { SUCCESS = 0, FAILED = 1 }

// build/src/utils/timeout.d.ts, build/src/utils/callback.d.ts
export declare class TimeoutError extends Error { constructor(message?: string); }
export declare function callWithTimeout<T>(promise: Promise<T>, timeout: number): Promise<T>;
export declare class BindOnceFuture<R> { get isCalled(): boolean; get promise(): Promise<R>; call(...args: unknown[]): Promise<R>; }
```

[ENTRYPOINT_SCOPE]: context suppression + OTEL_* env readers
- rail: observability

```ts
// build/src/trace/suppress-tracing.d.ts — wrap the OTLP egress in a suppressed context so it never traces itself
export declare function suppressTracing(context: Context): Context;
export declare function unsuppressTracing(context: Context): Context;
export declare function isTracingSuppressed(context: Context): boolean;

// build/src/platform (browser build) — typed OTEL_* readers; RuntimeConfig mirrors these
export declare function getStringFromEnv(key: string): string | undefined;
export declare function getNumberFromEnv(key: string): number | undefined;
export declare function getBooleanFromEnv(key: string): boolean;
export declare function getStringListFromEnv(key: string): string[] | undefined; // ',' delimited
```

## [04]-[IMPLEMENTATION_LAW]

[PROPAGATION_TOPOLOGY]:
- a propagator implements the `@opentelemetry/api` `TextMapPropagator` triple — `inject` writes the active `Context` onto an outbound carrier, `extract` reads an inbound carrier into a `Context`, `fields` lists the header names it owns
- `CompositePropagator` runs its `propagators` list in order; a later propagator writing the same context/carrier key wins, so `[W3CTraceContextPropagator, W3CBaggagePropagator]` injects both `traceparent`/`tracestate` and `baggage` from one handle
- `parseTraceParent` decodes a `traceparent` string to a `SpanContext` or `null` on a malformed value and never throws; the `SpanContext.traceState` it yields is a `TraceState` instance
- `TraceState` mutators (`set`/`unset`) return a NEW `TraceState` per the W3C list-move rule (modified keys move to the head); never mutate in place

[UTILITY_TOPOLOGY]:
- `ExportResult` is the sole export-result shape across all signals; a `code === FAILED` result carries the `error`, and the SDK's batch/periodic processors decide retry from it — application code never constructs one
- `suppressTracing(context)` sets the no-trace flag the tracer reads at span start; the OTLP export path runs its `HttpClient` call under a suppressed context so exporter egress is not itself traced (the recursion guard)
- the `HrTime` algebra is closed over the api `[seconds, nanos]` pair; `AnchoredClock` computes wall time as `anchor + monotonic-delta` so a child span never predates its parent under a mid-trace system-clock correction; `getTimeOrigin`/`unrefTimer` are `@deprecated` and excluded from new code
- env readers return `undefined`/`false` for empty/whitespace values; `getStringListFromEnv` splits on `,` and drops empty entries — the exact `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` semantics the `resources` `envDetector` and `@effect/opentelemetry` `Resource.layerFromEnv` implement
- the `browser`-conditional subpath resolves `platform/browser` (`SDK_INFO["telemetry.sdk.language"] = "webjs"`, `otperformance = window.performance`); the `internal._export` helper and the `./semconv` subpath (core's own experimental `ATTR_*` subset) are SDK-internal — route attribute vocabulary through `@opentelemetry/semantic-conventions`, not `core/semconv`

[INTEGRATION_LAW]:
- Stack with `@effect/opentelemetry` `Tracer`: the `Tracer.OtelTraceState` tag and `makeExternalSpan({ traceId, spanId, traceState })` carry a `core` `TraceState`; `parseTraceParent` on an inbound `traceparent` yields the `SpanContext` `withSpanContext` continues, so the W3C extract-and-continue seam is `parseTraceParent` -> `SpanContext` -> `Tracer.withSpanContext`
- Stack with `sdk-trace-web` `WebTracerProvider`: `Observability/telemetry#TRACE_PROPAGATION` builds a `CompositePropagator([W3CTraceContextPropagator, W3CBaggagePropagator])` and installs it via `new WebTracerProvider().register({ propagator })` — the `register` config is `SDKRegistrationConfig` (propagator + contextManager ONLY), and the global propagator the `@effect/opentelemetry` `Tracer` global reads is exactly this handle
- Stack with the sibling exporters + `sdk-metrics`: `ExportResult`/`ExportResultCode`, `TimeoutError`, and `BindOnceFuture` are the shared contract `exporter-trace-otlp-http`/`exporter-metrics-otlp-http` `export(...)` callbacks and `PeriodicExportingMetricReader` drains flow through — the exporter catalogs cite these members as `@opentelemetry/core`-owned rather than redeclaring them
- Stack with `RuntimeConfig`: the `OTEL_*` env readers are the raw layer the platform `Config` boundary re-projects into typed `RuntimeConfig` cells (`collectorOtlpEndpoint`, `traceSampleRatio`); the design page reads `RuntimeConfig`, and the readers are the substrate proof that those keys are the OTel-canonical env surface

[LOCAL_ADMISSION]:
- `@opentelemetry/core` is admitted ONLY inside `scope:telemetry` (edge-ledger ban); it is imported DIRECTLY for the W3C propagators/`TraceState` and consumed TRANSITIVELY (via `sdk-*`) for the utility substrate — no other folder imports it
- `TRACE_PARENT_HEADER`/`TRACE_STATE_HEADER` are the header names a transport interceptor injects under, never a hand-written `"traceparent"` literal; the outbound inject leg is settled at `interchange/Transport/transport#WIRE_TRANSPORT`, so `telemetry` registers extract-and-continue only
- pair the propagator with a `ParentBasedSampler` so the extracted parent's sampling decision is honored and a root span samples at the configured ratio
- do not reimplement HrTime math, attribute sanitization, timeout wrapping, or env parsing at a sink — compose the `core` primitive

[RAIL_LAW]:
- Package: `@opentelemetry/core`
- Owns: W3C trace-context/baggage propagation + the `CompositePropagator` fan-out, the W3C `TraceState`, the `ExportResult`/`ExportResultCode` exporter-callback contract, context suppression + RPC metadata, the `HrTime` clock algebra, the `OTEL_*` env readers, attribute sanitization, and the shared `TimeoutError`/`BindOnceFuture` lifecycle primitives
- Accept: `W3CTraceContextPropagator`/`W3CBaggagePropagator` composed in a `CompositePropagator` registered on a self-constructed `WebTracerProvider.register({ propagator })` or `propagation.setGlobalPropagator`; `ExportResult` as the one exporter result shape; `suppressTracing` on the OTLP egress; the `HrTime`/env/attribute utilities composed at their point of use
- Reject: a hand-rolled `traceparent` build/parse, a `"traceparent"` literal in place of `TRACE_PARENT_HEADER`, a per-request fresh root where an inbound parent exists, a hand-rolled export-result shape or HrTime math, an un-suppressed OTLP export path that traces itself, `@opentelemetry/core` imports outside `scope:telemetry`
