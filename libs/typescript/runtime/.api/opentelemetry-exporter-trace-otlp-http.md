# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_TRACE_OTLP_HTTP]

`@opentelemetry/exporter-trace-otlp-http` is the concrete `SpanExporter` POSTing a `ReadableSpan[]` batch to an OTLP/HTTP collector as protobuf or JSON. A `BatchSpanProcessor`/`SimpleSpanProcessor` wraps it and `NodeSdk`/`WebSdk` `Configuration.spanProcessor` sinks it — `otel/emit`'s SDK-bridge trace leg, an `[OTEL_PIN_BLOCK]`-collapse member reached only for SDK-only processor or exporter capability.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-http`
- package: `@opentelemetry/exporter-trace-otlp-http` (Apache-2.0)
- module: dual platform export — `platform/node` (`http`/`https`, config `OTLPExporterNodeConfigBase`) or `platform/browser` (`XMLHttpRequest`/`sendBeacon`, config `OTLPExporterConfigBase`); one `OTLPTraceExporter` selected at build time by the export condition
- runtime: node/bun or browser; peers `@opentelemetry/sdk-trace-base` (`SpanExporter`/`ReadableSpan` + the wrapping `BatchSpanProcessor`), `@opentelemetry/core` (`ExportResult` rail), `@opentelemetry/api`, config types from `@opentelemetry/otlp-exporter-base`
- rail: observability/export/trace — the SDK-bridge trace leg
- consumed-by: `otel/emit` via `NodeSdk`/`WebSdk` `Configuration.spanProcessor`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `SpanExporter` class and its transport config

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OTLPTraceExporter`          | class         | concrete `SpanExporter` a processor wraps |
|  [02]   | `OTLPExporterConfigBase`     | interface     | browser transport config                  |
|  [03]   | `OTLPExporterNodeConfigBase` | interface     | node transport config, extends base       |

- config knobs — `OTLPExporterConfigBase`: `url` `headers` `concurrencyLimit` `timeoutMillis`; `OTLPExporterNodeConfigBase` adds `keepAlive` `compression` `httpAgentOptions` `userAgent`
- `OTLPTraceExporter`: wrapping processor and SDK provider drive its `SpanExporter` members `export` (terminal disposition via core's `ExportResult`), `forceFlush`, and `shutdown`; design code never calls them

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge trace export composition — construct, wrap in a processor, hand to the facade

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `new OTLPTraceExporter(OTLPExporterNodeConfigBase?)`              | ctor    | node OTLP/HTTP span exporter       |
|  [02]   | `new OTLPTraceExporter(OTLPExporterConfigBase?)`                  | ctor    | browser exporter, RUM egress leg   |
|  [03]   | `new BatchSpanProcessor(exporter) -> Configuration.spanProcessor` | fold    | exporter → processor → facade sink |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Endpoint, serialization, and runtime are config, never a fork: OTLP/HTTP vs gRPC, JSON vs protobuf, node vs browser transport resolve to a config value or the package's platform export condition — a backend change is a `url`/`headers` value at the composition root, never a second exporter type.
- SDK-bridge trace leg reached only when the SDK processor's batching/retry semantics or a co-resident SDK-only exporter are required; `effect-opentelemetry.md` `[04]` owns the native-first dual-lane doctrine and the `[OTEL_PIN_BLOCK]` collapse.

[STACKING]:
- `opentelemetry-sdk-trace-base`(`.api/opentelemetry-sdk-trace-base.md`): `new OTLPTraceExporter(cfg)` wraps in `new BatchSpanProcessor(exporter)` (buffered, production) or `SimpleSpanProcessor` (synchronous, dev); the processor owns batching, retry, and `forceFlush` drain.
- `effect-opentelemetry`(`.api/effect-opentelemetry.md`): the wrapped processor feeds `NodeSdk`/`WebSdk` `Configuration.spanProcessor` alongside the one `AppIdentity`-derived `Resource` — the facade owns provider lifecycle, this package owns wire serialization.
- `opentelemetry-core`(`.api/opentelemetry-core.md`): `export()` reports through core's `ExportResult`/`ExportResultCode`; the outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced; `timeoutMillis`/`url` default from `OTEL_EXPORTER_OTLP_*` env via core's readers.
- `effect-platform`(`.api/effect-platform.md`): this exporter carries its OWN `http`/`XMLHttpRequest` transport and does NOT ride the `net/client` `HttpClient` retry/proxy policy the native `Otlp` lane inherits — the concrete transport-policy gap pinning this row `[OTEL_PIN_BLOCK]`.
- `otel/emit` (within-lib): composes exporter → `BatchSpanProcessor` → `NodeSdk`/`WebSdk` at the composition root only for SDK-only capability; the browser exporter is the RUM egress leg (`otel/vital`/`otel/crash`) under the export-boundary redaction rows.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger), constructed at the composition root; instrumentation code emits through Effect's `Effect.withSpan` and never imports this package.
- This exporter admits as an `[OTEL_PIN_BLOCK]`-collapse dependency, selected only for SDK-only processor or exporter capability.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-http`
- Owns: OTLP/HTTP span serialization — one `OTLPTraceExporter` (`SpanExporter`) over a node or browser transport, configured by endpoint, headers, compression, timeout
- Accept: `new OTLPTraceExporter(cfg)` wrapped in a `BatchSpanProcessor`/`SimpleSpanProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.spanProcessor`; endpoint and runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` outside `scope:runtime`, this SDK exporter where the native `OtlpTracer` suffices, a subclass per backend or compression (config owns it), treating the platform export as a fork, an unwrapped exporter handed straight to the facade
