# [TS_RUNTIME_API_EFFECT_OPENTELEMETRY]

`@effect/opentelemetry` bridges Effect `Tracer`/`Metric`/`Logger` signals to OTLP export in two lanes over one `AppIdentity`-derived `Resource`: the native `Otlp` lane serializes every signal to the endpoint over the platform `HttpClient` with zero `@opentelemetry/sdk-*`, and the `NodeSdk`/`WebSdk` bridge lane wraps SDK processors, readers, and exporters only where the SDK carries the capability. `Tracer` owns the W3C span-context bridge every ingress extends.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/opentelemetry`
- package: `@effect/opentelemetry` (MIT)
- module: ESM, one subpath module per namespace — the native `Otlp*` export lane, the `NodeSdk`/`WebSdk` SDK bridge, and the shared `Tracer`/`Resource` bridge
- runtime: native `Otlp` and `WebSdk` are browser-safe; `NodeSdk` binds node/bun (`sdk-trace-node`)
- rail: observability — Effect signals to OTLP export over `HttpClient`, dual native/SDK-bridge lane
- peer: `effect`, `@effect/platform` (`HttpClient`, native lane); `@opentelemetry/api`, `resources`, `semantic-conventions` (native substrate) with `sdk-trace-base`/`-node`/`-web`, `sdk-metrics`, `sdk-logs` (SDK-bridge lane, `[OTEL_PIN_BLOCK]`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wire value types, SDK config, and the `Context.Tag`s both lanes resolve

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `OtlpResource.Resource`                                              | schema        | native OTLP resource value               |
|  [02]   | `OtlpResource.KeyValue` / `AnyValue` / `ArrayValue` / `KeyValueList` | wire          | OTLP protobuf attribute value tree       |
|  [03]   | `OtlpSerialization.OtlpSerialization`                                | `Context.Tag` | JSON / protobuf frame selector           |
|  [04]   | `NodeSdk.Configuration` / `WebSdk.Configuration`                     | config        | SDK processors, readers, and resource    |
|  [05]   | `Logger.OtelLoggerProvider`                                          | `Context.Tag` | SDK `LoggerProvider` handle              |
|  [06]   | `Tracer.OtelTracer` / `Tracer.OtelTracerProvider`                    | `Context.Tag` | Effect↔OTel tracer / provider bridge     |
|  [07]   | `Tracer.OtelTraceFlags` / `Tracer.OtelTraceState`                    | `Context.Tag` | W3C trace-flags / trace-state carriers   |
|  [08]   | `Resource.Resource`                                                  | `Context.Tag` | shared identity resource both lanes read |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: native OTLP export composition — one `Otlp.layer` covers all three signals over `HttpClient` + `OtlpSerialization`

| [INDEX] | [SURFACE]                                                         | [SHAPE]       | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `Otlp.layer(opts)`                                                | layer         | full trace+metric+log export; serialization separate |
|  [02]   | `Otlp.layerJson(opts)` / `Otlp.layerProtobuf(opts)`               | layer         | JSON / protobuf serialization bundled                |
|  [03]   | `OtlpTracer.layer` / `OtlpMetrics.layer` / `OtlpLogger.layer`     | exporter      | single-signal export; each has a `.make` twin        |
|  [04]   | `OtlpResource.make` / `OtlpResource.fromConfig(...)`              | resource      | native resource; `fromConfig` reads a `Config`       |
|  [05]   | `OtlpSerialization.layerJson` / `OtlpSerialization.layerProtobuf` | serialization | frame selector `Otlp.layer` requires                 |

[ENTRYPOINT_SCOPE]: SDK-bridge composition — `NodeSdk`/`WebSdk` wire concrete `@opentelemetry/sdk-*` rows, selected only for an SDK-only exporter

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `NodeSdk.layer(config)`                                                  | layer   | node/bun SDK bridge (`sdk-trace-node`)       |
|  [02]   | `WebSdk.layer(config)`                                                   | layer   | browser SDK bridge (`sdk-trace-web`)         |
|  [03]   | `NodeSdk.layerTracerProvider` / `WebSdk.layerTracerProvider`             | layer   | trace-only SDK provider; `.layerEmpty` empty |
|  [04]   | `Metrics.makeProducer` / `.registerProducer` / `.layer`                  | bridge  | feed Effect metrics to SDK `MetricReader`    |
|  [05]   | `Logger.layerLoggerAdd` / `.layerLoggerReplace` / `.layerLoggerProvider` | bridge  | route / replace / provide the SDK logger     |

- `NodeSdk.layer` / `WebSdk.layer`: output a `Layer<Resource>` concealing the tracer provider behind `Layer.provide`; `layerTracerProvider` is the leg exposing the `Tracer.OtelTracerProvider` Tag for instrumentation registration.

[ENTRYPOINT_SCOPE]: span-context bridge + shared `Resource` — inbound `traceparent` continuation and the one identity resource both lanes mint

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Tracer.make` / `.makeExternalSpan` / `.currentOtelSpan`                   | span    | build tracer; continue + read a remote span      |
|  [02]   | `Tracer.withSpanContext`                                                   | span    | set the effect's parent from a W3C `SpanContext` |
|  [03]   | `Tracer.layer` / `.layerGlobal` / `.layerTracer` / `.layerGlobalTracer`    | layer   | install the OTel tracer on `Resource`            |
|  [04]   | `Resource.layer` / `.layerFromEnv` / `.layerEmpty` / `.configToAttributes` | layer   | `AppIdentity` + `OTEL_RESOURCE_ATTRIBUTES`       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- native-first: `Otlp.layer` is the default export rail — Effect's `Tracer`/`Metric`/`Logger` serialize straight to the OTLP endpoint over `HttpClient`; `NodeSdk`/`WebSdk` recover only SDK-only exporters (OTLP-gRPC, vendor exporters, batch processors).
- one resource, one identity: both lanes consume one `Resource` derived from `AppIdentity`, so a per-app telemetry fork is structurally impossible.
- runtime rides the lane, never a fork: `WebSdk` binds `sdk-trace-web`, `NodeSdk` binds `sdk-trace-node`, and the native lane rides whichever `HttpClient` the runtime supplies — a node↔bun↔browser move is an `HttpClient`/SDK Layer selection at the app root.
- `[OTEL_PIN_BLOCK]`: native parity retires the `@opentelemetry` sdk/exporter machinery as one unit; `@opentelemetry/api`, `resources` (the shared `Resource`-identity substrate), `semantic-conventions`, and the `@opentelemetry/core` W3C propagation family persist as the native lane's substrate.

[STACKING]:
- `@effect/platform`(`.api/effect-platform.md`): the native lane demands `HttpClient` — satisfied by `net/client` default-policy rows (timeout/retry/proxy) on node/bun or `BrowserHttpClient.layerXMLHttpRequest` in the browser, so OTLP egress inherits the shared net-client retry/proxy posture.
- `effect`(`.api/effect.md`): `OtlpLogger.layer` (native) or `Logger.layerLoggerReplace` (SDK) replaces the process `Logger`, so structured logs land as OTLP log records on the same `Resource`; Effect's own tracer/meter/logger feed the serializer, never a parallel SDK meter.
- `otel/emit` (within-lib): the export-boundary owner composes the export Layer onto the net-client policy, feeds `Resource.layer` the `core/value/identity` `AppIdentity`, scrubs PII through egress-redaction rows before serialization, and owns W3C extract-and-continue (`Tracer.makeExternalSpan`/`withSpanContext`) at every ingress; `core/observe/board` dashboards are `AppIdentity -> DashboardModel` total functions.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` imports admit ONLY inside `scope:runtime` (edge-ledger); every other folder emits through Effect's built-in `Effect.withSpan`/`Metric`/`Effect.log` and never imports this package.
- exporters construct at the composition root; native `Otlp` is the default, and `NodeSdk`/`WebSdk` bind only for an SDK-only exporter as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.

[RAIL_LAW]:
- Package: `@effect/opentelemetry`
- Owns: native OTLP trace/metric/log export over `HttpClient` (`Otlp`/`Otlp{Tracer,Metrics,Logger,Resource,Serialization}`), the `@opentelemetry/sdk-*` bridge (`NodeSdk`/`WebSdk`/`Metrics`/`Logger`), the W3C span-context bridge (`Tracer`), and the `AppIdentity`-derived `Resource`
- Accept: native `Otlp.layer` over the shared net-client `HttpClient` as the default rail, `NodeSdk`/`WebSdk` only for SDK-only exporters, one `Resource` from `AppIdentity`, `Tracer.makeExternalSpan`/`withSpanContext` for W3C continuation, export composed at the composition root
- Reject: `@opentelemetry/*` outside `scope:runtime`, SDK-bridge lanes where native `Otlp` suffices, per-app telemetry forks, a parallel log sink beside `OtlpLogger`, hand-rolled `traceparent` parsing
