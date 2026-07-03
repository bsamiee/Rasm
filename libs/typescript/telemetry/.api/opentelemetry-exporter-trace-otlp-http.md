# [@opentelemetry/exporter-trace-otlp-http] — the SDK-bridge `SpanExporter` that serializes spans to OTLP/HTTP, wrapped in a processor and fed to `NodeSdk`/`WebSdk`

`@opentelemetry/exporter-trace-otlp-http` is the concrete `SpanExporter` that POSTs a batch of `ReadableSpan`s to an OTLP/HTTP collector (protobuf over binary, or JSON). It is not composed directly: a `BatchSpanProcessor`/`SimpleSpanProcessor` (from `@opentelemetry/sdk-trace-base`) wraps it, and that processor is handed to the facade's `NodeSdk`/`WebSdk` `Configuration.spanProcessor`. Inside Rasm it is the SDK-bridge trace leg of `telemetry/otlp/export` — the fallback lane used when the SDK processor's batching/retry semantics or a co-resident SDK-only exporter are required; the native `Otlp` lane (`@effect/opentelemetry` `OtlpTracer`) is the `[R3]`-preferred default that serializes spans directly over a `@effect/platform` `HttpClient` with no SDK. The edge ledger fences `@opentelemetry/*` to `scope:telemetry`; this exporter is an `[R3]`-collapse member of the `[OTLP_SDK]` block (retires when native `OtlpTracer` reaches parity; `semantic-conventions` survives, this does not).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-http`
- package: `@opentelemetry/exporter-trace-otlp-http`
- version: `0.219.0`
- license: `Apache-2.0`
- otel-peer: `@opentelemetry/api ^1.3.0`, `@opentelemetry/core ^2.8.0` (the `ExportResult` rail), `@opentelemetry/sdk-trace-base ^2.8.0` (the `SpanExporter`/`ReadableSpan` contract + the `BatchSpanProcessor` that wraps it)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies the constructor config type (`OTLPExporterNodeConfigBase` / `OTLPExporterConfigBase`) and `CompressionAlgorithm` — a peer, not one of the nine roster rows
- consumed-by: `telemetry/otlp/export` SDK-bridge trace leg via the facade's `NodeSdk`/`WebSdk` `Configuration.spanProcessor`
- catalog-verdict: KEEP as SDK-bridge peer; edge-ledger fences `@opentelemetry/*` to `scope:telemetry`; `[R3]`-collapse member (native `OtlpTracer` supersedes)
- runtime: dual — the package export map selects `platform/node` (uses `http`/`https`, config `OTLPExporterNodeConfigBase`) or `platform/browser` (uses `XMLHttpRequest`/`sendBeacon`, config `OTLPExporterConfigBase`); ONE `OTLPTraceExporter` name, a build-time platform selection, never a fork
- modules: `OTLPTraceExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `SpanExporter` and its config
- rail: observability/export/trace
- One exporter class, `OTLPExporterBase<ReadableSpan[]> implements SpanExporter` — the endpoint, headers, compression, and timeout are CONFIG values on one class, never a subclass per backend or per compression. The node/browser split is the package export map; the two config types differ only by node-only transport fields (`keepAlive`/`compression`/`httpAgentOptions`).

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                              |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `OTLPTraceExporter` (`extends OTLPExporterBase<ReadableSpan[]> implements SpanExporter`) | span exporter | the concrete exporter a `BatchSpanProcessor` wraps |
|  [02]   | `export(items: ReadableSpan[], cb: (r: ExportResult) => void): void` | export method | called by the processor; reports through core's `ExportResult` |
|  [03]   | `forceFlush(): Promise<void>` / `shutdown(): Promise<void>`     | lifecycle      | drain-on-exit and terminal release the SDK provider invokes        |
|  [04]   | `OTLPExporterConfigBase { url?; headers?; concurrencyLimit?; timeoutMillis? }` (transitive) | base config | endpoint + header + concurrency + deadline (browser + base) |
|  [05]   | `OTLPExporterNodeConfigBase` (extends base: `keepAlive?; compression?: CompressionAlgorithm; httpAgentOptions?; userAgent?`) | node config | node transport tuning; `CompressionAlgorithm = "none" \| "gzip"` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge trace export composition
- rail: observability/export/trace
- The exporter is never a leaf: construct it, wrap it in a processor, hand the processor to the facade. `BatchSpanProcessor` (production, buffered) or `SimpleSpanProcessor` (dev, synchronous) from `sdk-trace-base` is the wrapper; `NodeSdk`/`WebSdk` `Configuration.spanProcessor` is the sink. `url`/`headers`/`compression`/`timeoutMillis` are policy values sourced from config or `OTEL_EXPORTER_OTLP_*` env (via core's readers), never forks.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :----------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new OTLPTraceExporter(config?: OTLPExporterNodeConfigBase)`               | node ctor      | the node OTLP/HTTP span exporter                              |
|  [02]   | `new OTLPTraceExporter(config?: OTLPExporterConfigBase)`                   | browser ctor   | the browser OTLP/HTTP span exporter (RUM egress)             |
|  [03]   | `new BatchSpanProcessor(new OTLPTraceExporter(cfg))` → `Configuration.spanProcessor` | composition | the standing stack: exporter → processor → `NodeSdk`/`WebSdk` |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the `NodeSdk`/`WebSdk` (`[R3]`) trace leg; the native `Otlp`/`OtlpTracer` lane is the `[R3]`-preferred default that serializes spans over a `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. Reach for this exporter only when the SDK processor semantics or a co-resident SDK-only exporter are required.
- endpoint/runtime are config, never a fork: OTLP/gRPC vs OTLP/HTTP, JSON vs protobuf, node vs browser transport — all resolve to a config value or the package's platform export condition. A backend change is a `url`/`headers` value at the composition root, never a second exporter type in design code.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-sdk-trace-base.md` (the wrapping seam): `new OTLPTraceExporter(cfg)` is wrapped in `new BatchSpanProcessor(exporter)` (buffered, production) or `SimpleSpanProcessor` (synchronous, dev) — the exporter is a `SpanExporter`; the processor owns batching, retry, and `forceFlush` drain.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped processor is handed to `NodeSdk.Configuration.spanProcessor` (node/bun; `sdk-trace-node`) or `WebSdk.Configuration.spanProcessor` (browser RUM; `sdk-trace-web`), alongside the one `AppIdentity`-derived `Resource`. The facade owns provider lifecycle; this package owns wire serialization.
- Stack with `.api/opentelemetry-core.md`: `export()` reports terminal disposition through core's `ExportResult`/`ExportResultCode`; the exporter's own outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced; `timeoutMillis`/`url` default from `OTEL_EXPORTER_OTLP_*` env via core's typed readers.
- Stack with `.api/effect-platform.md` posture (the divergence to record): this exporter carries its OWN `http`/`XMLHttpRequest` transport — it does NOT ride the `host/net/client` `HttpClient` retry/proxy policy the native `Otlp` lane inherits. That transport-policy gap is a concrete reason `otlp/export` prefers the native lane and marks this row `[R3]`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:telemetry` (edge-ledger ban); the exporter is constructed at the composition root only. Instrumentation code uses Effect's native `Effect.withSpan` and never imports this package.
- prefer the native `Otlp`/`OtlpTracer` lane; reach for this SDK exporter only for SDK-only processor/exporter capability, and record it as an `[R3]` non-collapsed dependency.
- the browser exporter is the RUM egress leg (`signal/vital`/`signal/crash`); apply the export-boundary redaction policy rows before the span leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-http`
- Owns: OTLP/HTTP span serialization — one `OTLPTraceExporter` (`SpanExporter`) over a node or browser transport, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPTraceExporter(cfg)` wrapped in a `BatchSpanProcessor`/`SimpleSpanProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.spanProcessor`; endpoint/runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:telemetry`, this SDK exporter where the native `OtlpTracer` suffices, a subclass per backend/compression (that is config), treating the node/browser platform export as a fork, an unwrapped exporter handed straight to the facade (it needs a processor)
