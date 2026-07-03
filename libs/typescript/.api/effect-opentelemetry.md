# [@effect/opentelemetry] — the OTLP export family telemetry owns and every folder emits through

`@effect/opentelemetry` bridges Effect's built-in `Tracer`/`Metric`/`Logger` signals to OpenTelemetry export in two lanes over ONE `Resource`. The NATIVE lane — `Otlp`/`OtlpTracer`/`OtlpMetrics`/`OtlpLogger`/`OtlpResource`/`OtlpSerialization` — serializes spans/metrics/logs to an OTLP endpoint directly over a `@effect/platform` `HttpClient` (JSON or protobuf), with zero `@opentelemetry/sdk-*` dependency; this is the `[R3]`-collapse target that retires the SDK block once native parity closes. The SDK-BRIDGE lane — `NodeSdk`/`WebSdk`/`Tracer`/`Metrics`/`Logger`/`Resource` — wraps concrete `@opentelemetry/sdk-*` processors/readers/exporters when SDK-only capability is required, and carries the heavy `@opentelemetry/*` peer block that the `edge/live` ledger fences to `scope:telemetry`. The `Tracer` module owns the W3C span-context bridge (`makeExternalSpan`/`withSpanContext`/`currentOtelSpan`) every ingress uses to extract-and-continue. The `Resource` derives from the same `AppIdentity` value `browser` boot and `StoreHandle` use, so hundreds of apps emit through one identity spine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/opentelemetry`
- package: `@effect/opentelemetry`
- version: `0.63.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` (`HttpClient` for the native lane; `.api/effect.md`, `.api/effect-platform.md`)
- otel-peer: `@opentelemetry/api`, `@opentelemetry/resources`, `@opentelemetry/sdk-trace-base`, `@opentelemetry/sdk-trace-node`, `@opentelemetry/sdk-trace-web`, `@opentelemetry/sdk-metrics`, `@opentelemetry/sdk-logs`, `@opentelemetry/semantic-conventions` — the SDK-bridge peer block; `semantic-conventions` survives, the rest collapses at `[R3]`
- catalog-verdict: KEEP; edge-ledger fences `@opentelemetry/*` to `scope:telemetry` only
- runtime: dual — native `Otlp` + `WebSdk` are browser-safe; `NodeSdk` is node/bun (`sdk-trace-node`)
- modules: `Otlp`, `OtlpTracer`, `OtlpMetrics`, `OtlpLogger`, `OtlpResource`, `OtlpSerialization`, `NodeSdk`, `WebSdk`, `Tracer`, `Metrics`, `Logger`, `Resource`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: native OTLP export lane
- rail: observability/native
- The self-contained path: `Otlp.layer` produces the trace + metric + log exporter wiring over an injected `HttpClient` + `OtlpSerialization` (JSON or protobuf frame). No `@opentelemetry/sdk-*` — Effect's own tracer/meter/logger feed the OTLP serializer directly. This is the `telemetry/otlp/export` primary rail and the `[R3]` future-default.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `Otlp.layer` / `Otlp.layerJson` / `Otlp.layerProtobuf` | layer     | `telemetry/otlp/export` unified trace+metric+log exporter   |
|  [02]   | `OtlpTracer.make` / `OtlpTracer.layer`          | span exporter  | trace-only OTLP export                                      |
|  [03]   | `OtlpMetrics.make` / `OtlpMetrics.layer`        | metric exporter| metric-only OTLP export                                     |
|  [04]   | `OtlpLogger.make` / `OtlpLogger.layer`          | log exporter   | log-only OTLP export; replaces the Effect `Logger`          |
|  [05]   | `OtlpResource.Resource` / `OtlpResource.make` / `fromConfig` | resource | native OTLP resource attributes                           |
|  [06]   | `OtlpResource.KeyValue` / `AnyValue` / `ArrayValue` / `KeyValueList` | wire | OTLP protobuf attribute value tree                         |
|  [07]   | `OtlpSerialization.OtlpSerialization`           | `Context.Tag`  | JSON/protobuf frame selector; `layerJson` / `layerProtobuf` |

[PUBLIC_TYPE_SCOPE]: SDK-bridge lane `[R3]`
- rail: observability/sdk-bridge
- `NodeSdk.layer` / `WebSdk.layer` wire concrete `@opentelemetry/sdk-*` `SpanProcessor`/`MetricReader`/`LogRecordProcessor` into the Effect runtime, for SDK-only exporters (OTLP-gRPC, vendor exporters, batch processors) the native lane does not yet cover. Carries the full `@opentelemetry` peer block that collapses at `[R3]`.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | `NodeSdk.Configuration` / `NodeSdk.layer` / `layerTracerProvider` / `layerEmpty` | layer | `telemetry` NodeSdk row (node/bun); `sdk-trace-node`       |
|  [02]   | `WebSdk.Configuration` / `WebSdk.layer` / `layerTracerProvider` | layer | `telemetry` WebSdk row (browser RUM); `sdk-trace-web`       |
|  [03]   | `Metrics.makeProducer` / `Metrics.registerProducer` / `Metrics.layer` | metric bridge | feed Effect metrics into an SDK `MetricReader`         |
|  [04]   | `Logger.OtelLoggerProvider` / `Logger.layerLoggerAdd` / `layerLoggerReplace` / `layerLoggerProvider` | log bridge | route Effect logs into an SDK `LoggerProvider`  |

[PUBLIC_TYPE_SCOPE]: span-context bridge + shared Resource
- rail: observability
- `Tracer` exposes the `OtelTracer`/`OtelTracerProvider` Tags and the W3C context bridge — the single seam where an inbound `traceparent` becomes an Effect parent span (`telemetry/otlp/context` extract-and-continue at every ingress). `Resource` is the one identity carrier both lanes share, derived from `AppIdentity`.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | `Tracer.OtelTracer` / `Tracer.OtelTracerProvider` | `Context.Tag`  | Effect↔OTel tracer/provider bridge                         |
|  [02]   | `Tracer.OtelTraceFlags` / `Tracer.OtelTraceState` | `Context.Tag`  | W3C trace-flags / trace-state carriers                     |
|  [03]   | `Tracer.make` / `Tracer.makeExternalSpan` / `currentOtelSpan` | span bridge | continue a remote span; read the active OTel span      |
|  [04]   | `Tracer.withSpanContext` / `Tracer.layer` / `layerGlobal` / `layerTracer` | bridge layer | `telemetry/otlp/context` W3C continuation           |
|  [05]   | `Resource.Resource`                               | `Context.Tag`  | the shared service-identity resource both lanes require     |
|  [06]   | `Resource.layer` / `layerFromEnv` / `layerEmpty` / `configToAttributes` | resource layer | `AppIdentity`-derived resource; `OTEL_*` env       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: native OTLP export composition
- rail: observability/native
- One `Otlp.layer({ baseUrl, resource, headers, …intervals })` covers all three signals; it requires an `HttpClient` (from `host/net/client` policy or `BrowserHttpClient`) and an `OtlpSerialization`. `layerJson`/`layerProtobuf` bundle the serialization so only `HttpClient` remains outstanding. Per-signal export intervals and `maxBatchSize` are policy values, never forks.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Otlp.layer({ baseUrl, resource?, headers?, maxBatchSize?, tracerExportInterval?, metricsExportInterval?, loggerExportInterval?, shutdownTimeout? }): Layer<never, never, HttpClient \| OtlpSerialization>` | layer | full OTLP export; serialization injected separately |
|  [02]   | `Otlp.layerJson(opts): Layer<never, never, HttpClient>` / `Otlp.layerProtobuf(opts): Layer<never, never, HttpClient>` | layer | export with JSON / protobuf serialization bundled        |
|  [03]   | `OtlpTracer.layer(opts)` / `OtlpMetrics.layer(opts)` / `OtlpLogger.layer(opts)`                             | layer          | single-signal export when a signal is disabled           |
|  [04]   | `OtlpResource.fromConfig({ serviceName?, serviceVersion?, attributes? })`                                    | resource       | resource from `Config` for the native lane               |
|  [05]   | `OtlpSerialization.layerJson` / `OtlpSerialization.layerProtobuf`                                            | serialization  | the frame selector `Otlp.layer` requires                 |

[ENTRYPOINT_SCOPE]: SDK-bridge composition `[R3]`
- rail: observability/sdk-bridge
- `NodeSdk.layer` / `WebSdk.layer` take a `Configuration` of SDK processors/readers + resource; use only when an SDK-only exporter is required. `layer` is overloaded (`layer` + `layerEmpty`).

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `NodeSdk.layer(LazyArg<Configuration> \| Effect<Configuration>): Layer<Resource>`; `Configuration = { resource, spanProcessor?, metricReader?, logRecordProcessor?, tracerConfig?, shutdownTimeout? }` | layer | node/bun SDK bridge; `sdk-trace-node` |
|  [02]   | `WebSdk.layer(LazyArg<Configuration> \| Effect<Configuration>): Layer<Resource>`                             | layer          | browser SDK bridge; `sdk-trace-web`                      |
|  [03]   | `NodeSdk.layerTracerProvider(processor, config?)` / `WebSdk.layerTracerProvider(processor, config?)`         | layer          | trace-only SDK provider                                  |
|  [04]   | `Metrics.layer(() => MetricReader \| MetricReader[], opts?)`                                                  | layer          | Effect metrics → SDK reader                              |
|  [05]   | `Logger.layerLoggerAdd` / `Logger.layerLoggerReplace` / `layerLoggerProvider(processor, config?)`           | layer          | Effect logs → SDK `LoggerProvider`                       |

[ENTRYPOINT_SCOPE]: span-context bridge + shared Resource
- rail: observability
- `Tracer.makeExternalSpan({ traceId, spanId, … })` reconstructs a remote parent from an inbound `traceparent`; `withSpanContext` sets the effect's parent from an OTel `SpanContext`. `Resource.layer` / `layerFromEnv` mint the one identity resource both lanes consume.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Tracer.makeExternalSpan({ traceId, spanId, traceFlags?, traceState? })`                       | span bridge    | `telemetry/otlp/context` inbound W3C continuation         |
|  [02]   | `Tracer.withSpanContext(effect, spanContext)` / `Tracer.currentOtelSpan`                       | span bridge    | continue a remote span; read active OTel span            |
|  [03]   | `Resource.layer({ serviceName, serviceVersion?, attributes? })` / `layerFromEnv(additional?)`  | resource layer | `AppIdentity`-derived resource; `OTEL_RESOURCE_ATTRIBUTES` |
|  [04]   | `Tracer.layer` / `layerGlobal` / `layerTracer` / `layerGlobalTracer`                           | bridge layer   | install the OTel tracer against a `Resource`             |

## [04]-[IMPLEMENTATION_LAW]

[DUAL_LANE_TOPOLOGY]:
- native-first: `Otlp.layer` is the default export rail — Effect's built-in `Tracer`/`Metric`/`Logger` serialize straight to the OTLP endpoint over `HttpClient`, no `@opentelemetry/sdk-*`. `NodeSdk`/`WebSdk` are the fallback for SDK-only exporters. `[R3]` closes when native parity retires the entire `@opentelemetry` sdk/exporter peer block; `semantic-conventions` survives as the `telemetry/signal/convention` vocabulary source.
- runtime split by lane, never by fork: `WebSdk` binds `sdk-trace-web`, `NodeSdk` binds `sdk-trace-node`; the native `Otlp` lane is runtime-neutral and rides whichever `HttpClient` the runtime provides. A Node↔Bun↔browser change is an `HttpClient`/SDK Layer selection at the app root, not a second exporter.
- one resource, one identity: both lanes consume one `Resource` derived from `AppIdentity`; `telemetry/board` dashboards are `AppIdentity -> DashboardModel` total functions, so a per-app telemetry fork is structurally impossible.

[INTEGRATION_LAW]:
- Stack with `@effect/platform` `HttpClient`: the native `Otlp` lane requires `HttpClient` — satisfied by `host/net/client` default-policy rows (timeout/retry/proxy) on node/bun, or `BrowserHttpClient.layerXMLHttpRequest` in the browser. `telemetry/otlp/export` composes the export layer onto the shared net-client policy, so OTLP egress inherits the same retry/proxy posture as every other outbound call.
- Stack with `kernel/identity` `AppIdentity`: `Resource.layer({ serviceName, serviceVersion, attributes })` is fed the `AppIdentity` value; the egress-redaction policy rows (`telemetry/otlp/export`) scrub PII at the export boundary before serialization.
- Stack with `@effect/experimental` `Sse` / `edge`: inbound ingress (`edge/api/middleware`, `browser` boot, `work` entities) calls `Tracer.makeExternalSpan`/`withSpanContext` to continue the W3C `traceparent`; the `telemetry/otlp/context` module owns this extract-and-continue at every entry.
- Stack with `effect` `Logger`: `OtlpLogger.layer` (native) or `Logger.layerLoggerReplace` (SDK) replaces the process `Logger`, so structured logs become OTLP log records on the same resource — one signal spine, never a parallel log sink.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:telemetry` (edge-ledger ban); no other folder imports the SDK. Folders emit through Effect's built-in tracing/metrics/logging and `telemetry` owns the export boundary.
- exporters are constructed at the composition root; instrumentation code uses Effect's native `Effect.withSpan`/`Metric`/`Effect.log` and never imports this package.
- prefer the native `Otlp` lane; reach for `NodeSdk`/`WebSdk` only for an SDK-only exporter, and record it as an `[R3]` non-collapsed dependency.

[RAIL_LAW]:
- Package: `@effect/opentelemetry`
- Owns: native OTLP trace/metric/log export over `HttpClient` (`Otlp`/`Otlp{Tracer,Metrics,Logger,Resource,Serialization}`), the `@opentelemetry/sdk-*` bridge (`NodeSdk`/`WebSdk`/`Metrics`/`Logger`), the W3C span-context bridge (`Tracer`), and the `AppIdentity`-derived `Resource`
- Accept: native `Otlp.layer` as the default export rail over the shared net-client `HttpClient`, `NodeSdk`/`WebSdk` only for SDK-only exporters `[R3]`, one `Resource` from `AppIdentity`, `Tracer.makeExternalSpan`/`withSpanContext` for W3C continuation, export composed at the composition root
- Reject: `@opentelemetry/*` imports outside `scope:telemetry`, SDK-bridge lanes where native `Otlp` suffices, per-app telemetry forks (dashboards are identity-derived data), a parallel log sink beside `OtlpLogger`, hand-rolled `traceparent` parsing
