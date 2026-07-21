# [TS_RUNTIME_API_EFFECT_OPENTELEMETRY]

`@effect/opentelemetry` bridges Effect `Tracer`/`Metric`/`Logger` signals to OTLP export in two lanes over ONE `Resource`. Native `Otlp` lane serializes every signal to the endpoint over the platform `HttpClient`, zero `@opentelemetry/sdk-*` — the `[OTEL_PIN_BLOCK]` target that retires the SDK block. SDK-bridge lane `NodeSdk`/`WebSdk` wraps SDK processors, readers, and exporters where only the SDK carries the capability, its peer block fenced to `scope:runtime`. `Tracer` owns the W3C span-context bridge every ingress extends; `Resource` derives from the one `AppIdentity` value — one identity spine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/opentelemetry`
- package: `@effect/opentelemetry`
- license: `MIT`
- effect-peer: `effect catalog`, `@effect/platform catalog` (`HttpClient` for the native lane)
- otel-peer: `@opentelemetry/api`, `@opentelemetry/resources`, `@opentelemetry/sdk-trace-base`, `@opentelemetry/sdk-trace-node`, `@opentelemetry/sdk-trace-web`, `@opentelemetry/sdk-metrics`, `@opentelemetry/sdk-logs`, `@opentelemetry/semantic-conventions` — the SDK-bridge peer block; at `[OTEL_PIN_BLOCK]` the SDK MACHINERY (`sdk-trace-base`/`-node`/`-web`, `sdk-metrics`, `sdk-logs`) collapses, while `api` (signal API), `resources` (the `Resource`-identity substrate `Resource.layer` lowers to in both lanes), and `semantic-conventions` (convention vocabulary) persist
- catalog-verdict: KEEP; edge-ledger fences `@opentelemetry/*` to `scope:runtime` only
- runtime: dual — native `Otlp` + `WebSdk` are browser-safe; `NodeSdk` is node/bun (`sdk-trace-node`)
- modules: `Otlp`, `OtlpTracer`, `OtlpMetrics`, `OtlpLogger`, `OtlpResource`, `OtlpSerialization`, `NodeSdk`, `WebSdk`, `Tracer`, `Metrics`, `Logger`, `Resource`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: native OTLP export lane
- rail: observability/native
- Self-contained path: `Otlp.layer` produces the trace + metric + log exporter wiring over an injected `HttpClient` + `OtlpSerialization` (JSON or protobuf frame). No `@opentelemetry/sdk-*` — Effect's own tracer/meter/logger feed the OTLP serializer directly. This is the `otel/emit` primary rail and the `[OTEL_PIN_BLOCK]` future-default.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                          |
| :-----: | :------------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `Otlp.layer` / `Otlp.layerJson` / `Otlp.layerProtobuf`               | layer           | unified trace+metric+log exporter            |
|  [02]   | `OtlpTracer.make` / `OtlpTracer.layer`                               | span exporter   | trace-only OTLP export                       |
|  [03]   | `OtlpMetrics.make` / `OtlpMetrics.layer`                             | metric exporter | metric-only OTLP export                      |
|  [04]   | `OtlpLogger.make` / `OtlpLogger.layer`                               | log exporter    | log-only export; replaces `Logger`           |
|  [05]   | `OtlpResource.Resource` / `OtlpResource.make` / `fromConfig`         | resource        | native OTLP resource attributes              |
|  [06]   | `OtlpResource.KeyValue` / `AnyValue` / `ArrayValue` / `KeyValueList` | wire            | OTLP protobuf attribute value tree           |
|  [07]   | `OtlpSerialization.OtlpSerialization`                                | `Context.Tag`   | `layerJson` / `layerProtobuf` frame selector |

[PUBLIC_TYPE_SCOPE]: SDK-bridge lane `[OTEL_PIN_BLOCK]`
- rail: observability/sdk-bridge
- `NodeSdk.layer` / `WebSdk.layer` wire concrete `@opentelemetry/sdk-*` `SpanProcessor`/`MetricReader`/`LogRecordProcessor` into the Effect runtime, for SDK-only exporters (OTLP-gRPC, vendor exporters, batch processors) the native lane does cover. Carries the full `@opentelemetry` peer block that collapses at `[OTEL_PIN_BLOCK]`.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                       |
| :-----: | :-------------------------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `NodeSdk.Configuration` / `NodeSdk.layer`                             | layer         | `telemetry` NodeSdk row; `sdk-trace-node` |
|  [02]   | `NodeSdk.layerTracerProvider` / `layerEmpty`                          | layer         | node/bun trace-only provider              |
|  [03]   | `WebSdk.Configuration` / `WebSdk.layer` / `layerTracerProvider`       | layer         | `telemetry` WebSdk row; `sdk-trace-web`   |
|  [04]   | `Metrics.makeProducer` / `Metrics.registerProducer` / `Metrics.layer` | metric bridge | feed Effect metrics to SDK `MetricReader` |
|  [05]   | `Logger.OtelLoggerProvider` / `Logger.layerLoggerAdd`                 | log bridge    | route Effect logs to SDK `LoggerProvider` |
|  [06]   | `Logger.layerLoggerReplace` / `layerLoggerProvider`                   | log bridge    | replace/provide the SDK logger            |

[PUBLIC_TYPE_SCOPE]: span-context bridge + shared Resource
- rail: observability
- `Tracer` exposes the `OtelTracer`/`OtelTracerProvider` Tags and the W3C context bridge — the single seam where an inbound `traceparent` becomes an Effect parent span (`otel/emit` extract-and-continue at every ingress). `Resource` is the one identity carrier both lanes share, derived from `AppIdentity`.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                          |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Tracer.OtelTracer` / `Tracer.OtelTracerProvider`                       | `Context.Tag`  | Effect↔OTel tracer/provider bridge           |
|  [02]   | `Tracer.OtelTraceFlags` / `Tracer.OtelTraceState`                       | `Context.Tag`  | W3C trace-flags / trace-state carriers       |
|  [03]   | `Tracer.make` / `Tracer.makeExternalSpan` / `currentOtelSpan`           | span bridge    | continue remote span; read active OTel span  |
|  [04]   | `Tracer.withSpanContext` / `layer` / `layerGlobal` / `layerTracer`      | bridge layer   | `otel/emit` W3C continuation                 |
|  [05]   | `Resource.Resource`                                                     | `Context.Tag`  | shared identity resource for both lanes      |
|  [06]   | `Resource.layer` / `layerFromEnv` / `layerEmpty` / `configToAttributes` | resource layer | `AppIdentity`-derived resource; `OTEL_*` env |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: native OTLP export composition
- rail: observability/native
- One `Otlp.layer({ baseUrl, resource, headers, …intervals })` covers all three signals; it requires an `HttpClient` (from `net/client` policy or `BrowserHttpClient`) and an `OtlpSerialization`. `layerJson`/`layerProtobuf` bundle the serialization so only `HttpClient` remains outstanding. Per-signal export intervals and `maxBatchSize` are policy values, never forks.
- call: `Otlp.layer({ baseUrl, resource?, headers?, maxBatchSize?, tracerExportInterval?, metricsExportInterval?, loggerExportInterval?, shutdownTimeout? }): Layer<never, never, HttpClient | OtlpSerialization>`
- call: `Otlp.layerJson(opts): Layer<never, never, HttpClient>` / `Otlp.layerProtobuf(opts): Layer<never, never, HttpClient>`
- call: `OtlpResource.fromConfig({ serviceName?, serviceVersion?, attributes? })`

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                      |
| :-----: | :---------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `Otlp.layer(opts)`                                                | layer          | full OTLP export; serialization separate |
|  [02]   | `Otlp.layerJson(opts)` / `Otlp.layerProtobuf(opts)`               | layer          | JSON / protobuf serialization bundled    |
|  [03]   | `OtlpTracer.layer` / `OtlpMetrics.layer` / `OtlpLogger.layer`     | layer          | single-signal export                     |
|  [04]   | `OtlpResource.fromConfig(...)`                                    | resource       | resource from `Config` (native lane)     |
|  [05]   | `OtlpSerialization.layerJson` / `OtlpSerialization.layerProtobuf` | serialization  | frame selector `Otlp.layer` requires     |

[ENTRYPOINT_SCOPE]: SDK-bridge composition `[OTEL_PIN_BLOCK]`
- rail: observability/sdk-bridge
- `NodeSdk.layer` / `WebSdk.layer` take a `Configuration` of SDK processors/readers + resource; use only when an SDK-only exporter is required. `layer` is overloaded (`layer` + `layerEmpty`). Both conceal the tracer provider behind `Layer.provide` — a declared `Layer<Resource>` with no `Tracer.OtelTracerProvider` in its output; `layerTracerProvider` is the leg that exposes the provider Tag for consumers that must reach it (instrumentation registration).
- call: `NodeSdk.layer(LazyArg<Configuration> | Effect<Configuration>): Layer<Resource>` — `Configuration = { resource, spanProcessor?, metricReader?, logRecordProcessor?, tracerConfig?, shutdownTimeout? }` (`shutdownTimeout` is NodeSdk-only)
- call: `WebSdk.layer(LazyArg<Configuration> | Effect<Configuration>): Layer<Resource>` — `Configuration = { resource, spanProcessor?, metricReader?, logRecordProcessor?, loggerProviderConfig?, tracerConfig? }` (no `shutdownTimeout`)
- call: `NodeSdk.layerTracerProvider(processor, config?)` / `WebSdk.layerTracerProvider(processor, config?)`
- call: `Logger.layerLoggerProvider(processor, config?)`

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                   |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `NodeSdk.layer(config)`                                                | layer          | node/bun SDK bridge; `sdk-trace-node` |
|  [02]   | `WebSdk.layer(config)`                                                 | layer          | browser SDK bridge; `sdk-trace-web`   |
|  [03]   | `NodeSdk.layerTracerProvider(...)` / `WebSdk.layerTracerProvider(...)` | layer          | trace-only SDK provider               |
|  [04]   | `Metrics.layer(() => MetricReader \| MetricReader[], opts?)`           | layer          | Effect metrics → SDK reader           |
|  [05]   | `Logger.layerLoggerAdd` / `layerLoggerReplace` / `layerLoggerProvider` | layer          | Effect logs → SDK `LoggerProvider`    |

[ENTRYPOINT_SCOPE]: span-context bridge + shared Resource
- rail: observability
- `Tracer.makeExternalSpan({ traceId, spanId, … })` reconstructs a remote parent from an inbound `traceparent`; `withSpanContext` sets the effect's parent from an OTel `SpanContext`. `Resource.layer` / `layerFromEnv` mint the one identity resource both lanes consume.
- call: `Tracer.makeExternalSpan({ traceId, spanId, traceFlags?, traceState? })`
- call: `Tracer.withSpanContext(effect, spanContext)`
- call: `Resource.layer({ serviceName, serviceVersion?, attributes? })` / `layerFromEnv(additional?)`

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                        |
| :-----: | :------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Tracer.makeExternalSpan(...)`                                       | span bridge    | `otel/emit` inbound W3C continuation       |
|  [02]   | `Tracer.withSpanContext(...)` / `Tracer.currentOtelSpan`             | span bridge    | continue remote span; read OTel span       |
|  [03]   | `Resource.layer(...)` / `layerFromEnv(...)`                          | resource layer | `AppIdentity` + `OTEL_RESOURCE_ATTRIBUTES` |
|  [04]   | `Tracer.layer` / `layerGlobal` / `layerTracer` / `layerGlobalTracer` | bridge layer   | install OTel tracer on `Resource`          |

## [04]-[IMPLEMENTATION_LAW]

[DUAL_LANE_TOPOLOGY]:
- native-first: `Otlp.layer` is the default export rail — Effect's built-in `Tracer`/`Metric`/`Logger` serialize straight to the OTLP endpoint over `HttpClient`, no `@opentelemetry/sdk-*`; `NodeSdk`/`WebSdk` recover only SDK-only exporters.
- `[OTEL_PIN_BLOCK]` collapse: native parity retires the `@opentelemetry` sdk/exporter machinery, while `semantic-conventions` (convention vocabulary), `resources` (the shared `Resource`-identity substrate), and the directly-imported `@opentelemetry/core` W3C propagation family survive as the native lane's substrate.
- runtime split by lane, never by fork: `WebSdk` binds `sdk-trace-web`, `NodeSdk` binds `sdk-trace-node`; the native `Otlp` lane is runtime-neutral and rides whichever `HttpClient` the runtime supplies. A Node↔Bun↔browser change is an `HttpClient`/SDK Layer selection at the app root, not a second exporter.
- one resource, one identity: both lanes consume one `Resource` derived from `AppIdentity`; `core/observe/board` dashboards are `AppIdentity -> DashboardModel` total functions, so a per-app telemetry fork is structurally impossible.

[INTEGRATION_LAW]:
- Stack with `@effect/platform` `HttpClient`: the native `Otlp` lane requires `HttpClient` — satisfied by `net/client` default-policy rows (timeout/retry/proxy) on node/bun, or `BrowserHttpClient.layerXMLHttpRequest` in the browser. `otel/emit` composes the export layer onto the shared net-client policy, so OTLP egress inherits the same retry/proxy posture as every other outbound call.
- Stack with `core/value/identity` `AppIdentity`: `Resource.layer({ serviceName, serviceVersion, attributes })` is fed the `AppIdentity` value; the egress-redaction policy rows (`otel/emit`) scrub PII at the export boundary before serialization.
- Stack with `@effect/experimental` `Sse` / `edge`: inbound ingress (`edge/api/middleware`, `browser` boot, `work` entities) calls `Tracer.makeExternalSpan`/`withSpanContext` to continue the W3C `traceparent`; the `otel/emit` module owns this extract-and-continue at every entry.
- Stack with `effect` `Logger`: `OtlpLogger.layer` (native) or `Logger.layerLoggerReplace` (SDK) replaces the process `Logger`, so structured logs become OTLP log records on the same resource — one signal spine, never a parallel log sink.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); no other folder imports the SDK. Folders emit through Effect's built-in tracing/metrics/logging and `telemetry` owns the export boundary.
- exporters are constructed at the composition root; instrumentation code uses Effect's native `Effect.withSpan`/`Metric`/`Effect.log` and never imports this package.
- native `Otlp` lane is the default; `NodeSdk`/`WebSdk` bind only for an SDK-only exporter, recorded as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.

[RAIL_LAW]:
- Package: `@effect/opentelemetry`
- Owns: native OTLP trace/metric/log export over `HttpClient` (`Otlp`/`Otlp{Tracer,Metrics,Logger,Resource,Serialization}`), the `@opentelemetry/sdk-*` bridge (`NodeSdk`/`WebSdk`/`Metrics`/`Logger`), the W3C span-context bridge (`Tracer`), and the `AppIdentity`-derived `Resource`
- Accept: native `Otlp.layer` as the default export rail over the shared net-client `HttpClient`, `NodeSdk`/`WebSdk` only for SDK-only exporters `[OTEL_PIN_BLOCK]`, one `Resource` from `AppIdentity`, `Tracer.makeExternalSpan`/`withSpanContext` for W3C continuation, export composed at the composition root
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, SDK-bridge lanes where native `Otlp` suffices, per-app telemetry forks (dashboards are identity-derived data), a parallel log sink beside `OtlpLogger`, hand-rolled `traceparent` parsing
