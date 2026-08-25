# [TS_RUNTIME_API_EFFECT_OPENTELEMETRY]

`@effect/opentelemetry` bridges Effect `Tracer`/`Metric`/`Logger` signals to OTLP export in two lanes over one `AppIdentity`-derived `Resource`: the native `Otlp` lane serializes every signal to the endpoint over the platform `HttpClient` with zero `@opentelemetry/sdk-*`, and the `NodeSdk`/`WebSdk` bridge lane wraps SDK processors, readers, and exporters only where the SDK carries the capability. `Tracer` owns the W3C span-context bridge every ingress extends.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: native OTLP export composition — one `Otlp.layer` covers all three signals over `HttpClient` + `OtlpSerialization`

| [INDEX] | [SURFACE]                                                         | [SHAPE]       | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `Otlp.layer(opts)`                                                | layer         | full trace+metric+log export; serialization separate |
|  [02]   | `Otlp.layerJson(opts)` / `Otlp.layerProtobuf(opts)`               | layer         | JSON / protobuf serialization bundled                |
|  [03]   | `OtlpTracer.layer` / `OtlpMetrics.layer` / `OtlpLogger.layer`     | exporter      | single-signal export; each has a `.make` twin        |
|  [04]   | `OtlpResource.make` / `OtlpResource.fromConfig(...)`              | resource      | native resource; `fromConfig` reads a `Config`       |
|  [05]   | `OtlpSerialization.layerJson` / `OtlpSerialization.layerProtobuf` | serialization | frame selector `Otlp.layer` requires                 |

- One option bag serves all three `Otlp` layers: `baseUrl` required; `resource?` (`{ serviceName?, serviceVersion?, attributes? }`), `headers?` (`Headers.Input`), `maxBatchSize?`, `replaceLogger?` (`Logger.Logger<any, any>`), `tracerContext?` (`<X>(f: () => X, span: Tracer.AnySpan) => X`), `loggerExportInterval?`, `loggerExcludeLogSpans?`, `metricsExportInterval?`, `tracerExportInterval?`, and `shutdownTimeout?` — every interval a `Duration.DurationInput`.
- That bag carries NO compression, no protocol, and no per-signal endpoint: the native lane posts to `<baseUrl>/v1/<signal>` uncompressed over the ambient `HttpClient`, so wire compression is an `HttpClient` middleware row on the net-client policy, never an exporter option. Compositions demanding gzip on the native lane install it at the client, or select the SDK-bridge exporter carrying `compression`.
- `layer` leaves `OtlpSerialization` in the layer's requirements; `layerJson`/`layerProtobuf` bundle it, so all three require `HttpClient` alone past that choice.
- One bag configures all three signals — a per-signal batch or interval divergence composes `OtlpTracer`/`OtlpMetrics`/`OtlpLogger` separately, never a second `Otlp.layer`.

[ENTRYPOINT_SCOPE]: SDK-bridge composition — `NodeSdk`/`WebSdk` wire concrete `@opentelemetry/sdk-*` rows, selected only for an SDK-only exporter

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `NodeSdk.layer(config)`                                                  | layer   | node/bun SDK bridge (`sdk-trace-node`)       |
|  [02]   | `WebSdk.layer(config)`                                                   | layer   | browser SDK bridge (`sdk-trace-web`)         |
|  [03]   | `NodeSdk.layerTracerProvider` / `WebSdk.layerTracerProvider`             | layer   | trace-only SDK provider; `.layerEmpty` empty |
|  [04]   | `Metrics.makeProducer` / `.registerProducer` / `.layer`                  | bridge  | feed Effect metrics to SDK `MetricReader`    |
|  [05]   | `Logger.layerLoggerAdd` / `.layerLoggerReplace` / `.layerLoggerProvider` | bridge  | route / replace / provide the SDK logger     |

- `NodeSdk.layer` / `WebSdk.layer`: output a `Layer<Resource>` concealing the tracer provider behind `Layer.provide`; `layerTracerProvider` is the leg exposing the `Tracer.OtelTracerProvider` Tag for instrumentation registration. Each overloads on a `LazyArg<Configuration>` and on an `Effect<Configuration, E, R>`, so a config resolved from `Config` or a service composes without a synchronous escape.
- `NodeSdk.Configuration` fields, every one optional: `spanProcessor?` (`SpanProcessor | ReadonlyArray<SpanProcessor>`), `tracerConfig?` (`Omit<TracerConfig, "resource">`), `metricReader?` (`MetricReader | ReadonlyArray<MetricReader>`), `logRecordProcessor?` (`LogRecordProcessor | ReadonlyArray<LogRecordProcessor>`), `loggerProviderConfig?` (`Omit<LoggerProviderConfig, "resource">`), `resource?` (`{ serviceName, serviceVersion?, attributes? }` — `serviceName` required inside it), and `shutdownTimeout?` (`DurationInput`). Each of the three signal seats takes the array form, so one config mounts a redaction processor beside the exporting one on any signal.
- `layerTracerProvider(processor, config?)` takes a `SpanProcessor | NonEmptyReadonlyArray<SpanProcessor>` positionally and its config is `Omit<TracerConfig, "resource"> & { shutdownTimeout? }` — the drain bound rides the trace-only leg too. `WebSdk.layerTracerProvider` takes the same processor shape with a config lacking `shutdownTimeout`, so the browser trace leg's flush bound is the batch record's own timeout alone.
- `Logger.layerLoggerProvider(processor, config?)` mirrors that shape — `LogRecordProcessor | NonEmptyReadonlyArray<LogRecordProcessor>` positionally, `Omit<LoggerProviderConfig, "resource"> & { shutdownTimeout? }` for the config — and outputs the `Logger.OtelLoggerProvider` Tag over `Resource`. Consuming it through `Layer.provide` under `layerLoggerAdd` conceals that Tag, so a graph binding instrumentation log records publishes it with `Layer.provideMerge` instead.
- Both provider legs and `registerProducer` take NON-EMPTY readonly arrays: a plain `T[]` built by spreading contributed rows is not assignable, so an assembled processor or reader list lands as a leading-element tuple.
- `Metrics.layer(evaluate, options?)` takes a `LazyArg<MetricReader | NonEmptyReadonlyArray<MetricReader>>` beside `{ shutdownTimeout? }`, so the reader constructs inside the layer's scope; `Metrics.makeProducer` is the `Effect<MetricProducer, never, Resource>` feeding `registerProducer(producer, readers)` where the reader set is already mounted.
- `Metrics.layer` is exactly `makeProducer` piped into `registerProducer`, and `registerProducer` calls `MetricReader.setMetricProducer` — so no `MeterProvider` exists on the Effect metric path and every SDK knob seated on a provider or reader (views, `aggregationSelector`, `aggregationTemporalitySelector`, `cardinalitySelector`) is inert for Effect-minted series. That producer stamps a fixed `@effect/opentelemetry/Metrics` scope carrying no version and no schema URL, `AggregationTemporality.CUMULATIVE` on every data point, and explicit-bucket histograms read off Effect's own `MetricBoundaries`; descriptor `unit` reads `tags.unit ?? tags.time_unit ?? "1"`, so a `Metric.tagged("unit", …)` row is how a UCUM unit reaches the wire. Governing those series means decomposing the layer into `makeProducer -> wrap -> registerProducer` and projecting the `CollectionResult`.
- Both metric lanes fan a `Frequency` state into one monotonic-sum data point per occurrence word and append that word under the HARDCODED attribute key `key` (`internal/metrics.js` on the producer leg, `OtlpMetrics.js` on the native leg), so an attribute allow-list omitting it collapses every frequency series into one undifferentiated sum.
- Those same folds push the `unit`/`time_unit` label into the attribute set as well as reading it for the descriptor, so the UCUM carrier survives as an exported attribute on any lane whose processors never run.
- `Otlp.layer`/`layerJson`/`layerProtobuf` carry no compression key, so the native lane posts uncompressed; `@effect/platform` ships no request-encoding middleware either, which leaves gzip on the native lane unreachable at these pins.
- `resource.serviceName` is required once `resource` is present, so an identity-less bridge omits the key whole rather than passing an empty string.

[ENTRYPOINT_SCOPE]: span-context bridge + shared `Resource` — inbound `traceparent` continuation and the one identity resource both lanes mint

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Tracer.make` / `.makeExternalSpan` / `.currentOtelSpan`                   | span    | build tracer; continue + read a remote span      |
|  [02]   | `Tracer.withSpanContext`                                                   | span    | set the effect's parent from a W3C `SpanContext` |
|  [03]   | `Tracer.layer` / `.layerGlobal` / `.layerTracer` / `.layerGlobalTracer`    | layer   | install the OTel tracer on `Resource`            |
|  [04]   | `Resource.layer` / `.layerFromEnv` / `.layerEmpty` / `.configToAttributes` | layer   | `AppIdentity` + `OTEL_RESOURCE_ATTRIBUTES`       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- native-first: `Otlp.layer` is the default export rail — Effect's `Tracer`/`Metric`/`Logger` serialize straight to the OTLP endpoint over `HttpClient`; `NodeSdk`/`WebSdk` recover only SDK-only exporters (OTLP-gRPC, vendor exporters, batch processors).
- one resource, one identity: both lanes consume one `Resource` derived from `AppIdentity`, so a per-app telemetry fork is structurally impossible.
- runtime rides the lane, never a fork: `WebSdk` binds `sdk-trace-web`, `NodeSdk` binds `sdk-trace-node`, and the native lane rides whichever `HttpClient` the runtime supplies — a node↔bun↔browser move is an `HttpClient`/SDK Layer selection at the app root.
- `[OTEL_PIN_BLOCK]`: native parity retires the `@opentelemetry` sdk/exporter machinery as one unit; `@opentelemetry/api`, `resources` (the shared `Resource`-identity substrate), `semantic-conventions`, and the `@opentelemetry/core` W3C propagation family persist as the native lane's substrate. This row is the block's one definition — a catalog or design page citing the token resolves here, and the criteria table below is the whole roster deciding when the block retires.

[PIN_BLOCK_PARITY]: criteria the block carries — each names the seat the pin withholds and what the estate loses until it opens; every criterion clears before the block retires

| [INDEX] | [CRITERION]         | [WITHHELD_SEAT]                                         | [COST_UNTIL_IT_OPENS]                                |
| :-----: | :------------------ | :------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | boundary scrub      | native lane exposes no span-attribute hook              | a scrub-mandating posture selects an SDK lane        |
|  [02]   | sender encoding     | native options carry no compression field               | the gzip pin holds on the SDK node sender alone      |
|  [03]   | producer governance | producer seats on the reader, past every view selector  | dimension governance rides the collection projection |
|  [04]   | exemplar seat       | `sdk-metrics` exports no `ExemplarFilter`, reaches none | click-through rides the gateway span-derived series  |

- Law: criteria state capability facts re-proved on the installed rail, never preferences — the browser exporter build declares neither `compression` nor `keepAlive` while the node build declares both, so criterion [02] is a SENDER column rather than a lane-wide gap.
- Law: criteria [03] and [04] compound — the producer emits data points carrying no exemplar slot, so opening the SDK filter alone leaves every `rasm.*` series exemplar-free and both seats gate one criterion clear.
- Law: every criterion projects onto the `TELEMETRY_CONVENTION` entry's capability-absence disposition, so a branch ceiling and the corpus digest carry one value and a pin bump opening a seat fails at that digest until the row re-values.

[STACKING]:
- `@effect/platform`(`.api/effect-platform.md`): the native lane demands `HttpClient` — satisfied by `net/client` default-policy rows (timeout/retry/proxy) on node/bun or `BrowserHttpClient.layerXMLHttpRequest` in the browser, so OTLP egress inherits the shared net-client retry/proxy posture.
- `effect`(`.api/effect.md`): `OtlpLogger.layer` (native) or `Logger.layerLoggerReplace` (SDK) replaces the process `Logger`, so structured logs land as OTLP log records on the same `Resource`; Effect's own tracer/meter/logger feed the serializer, never a parallel SDK meter.
- `otel/emit` (within-lib): the export-boundary owner composes the export Layer onto the net-client policy, feeds `Resource.layer` the `core/value/identity` `AppIdentity`, scrubs PII through egress-redaction rows before serialization, and owns W3C extract-and-continue (`Tracer.makeExternalSpan`/`withSpanContext`) at every ingress; `core/observe/board` dashboards are `AppIdentity -> DashboardModel` total functions.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` imports admit ONLY inside `scope:runtime` (edge-ledger); every other folder emits through Effect's built-in `Effect.withSpan`/`Metric`/`Effect.log` and never imports this package.
- exporters construct at the composition root; native `Otlp` is the default, and `NodeSdk`/`WebSdk` bind only for an SDK-only exporter as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
