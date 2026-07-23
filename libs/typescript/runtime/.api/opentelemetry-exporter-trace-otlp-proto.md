# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_TRACE_OTLP_PROTO]

`@opentelemetry/exporter-trace-otlp-proto` is the concrete `SpanExporter` that POSTs a batch of `ReadableSpan`s to an OTLP/HTTP collector as protobuf — the binary-encoded sibling of `.api/opentelemetry-exporter-trace-otlp-http.md`, which serializes the same wire as JSON. Both share ONE `OTLPTraceExporter` class name, ONE base transport (`@opentelemetry/otlp-exporter-base`), and ONE config surface; the only divergence is the `ProtobufTraceSerializer` (`@opentelemetry/otlp-transformer`) this row binds versus the JSON serializer the http row binds — encoding is a serializer binding, never a second exporter concept. It is not composed directly: a `BatchSpanProcessor`/`SimpleSpanProcessor` (from `@opentelemetry/sdk-trace-base`) wraps it, and that processor is handed to the facade's `NodeSdk`/`WebSdk` `Configuration.spanProcessor`. Inside Rasm it completes the `[OTLP_SDK]` wire law — the protobuf leg of `otel/emit`'s SDK-bridge trace lane, selected when the collector rejects JSON or payload compactness matters; the native `Otlp` lane (`@effect/opentelemetry` `OtlpTracer`) stays the `[OTEL_PIN_BLOCK]`-preferred default that serializes spans directly over a `@effect/platform` `HttpClient` with no SDK. Edge ledger fences `@opentelemetry/*` to `scope:runtime`; this exporter is an `[OTEL_PIN_BLOCK]`-collapse member of the `[OTLP_SDK]` block (retires when native `OtlpTracer` reaches protobuf-wire parity; `semantic-conventions` survives, this does not).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-proto`
- package: `@opentelemetry/exporter-trace-otlp-proto` (Apache-2.0)
- otel-peer: `@opentelemetry/api ^catalog`, `@opentelemetry/core ^catalog` (the `ExportResult` rail), `@opentelemetry/sdk-trace-base ^catalog` (the `SpanExporter`/`ReadableSpan` contract + the `BatchSpanProcessor` that wraps it)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies the constructor config type (`OTLPExporterNodeConfigBase` / `OTLPExporterConfigBase`), `OTLPExporterBase`, and `CompressionAlgorithm`; `@opentelemetry/otlp-transformer` supplies the `ProtobufTraceSerializer` that makes this row protobuf — both are peers, not roster rows
- consumed-by: `otel/emit` SDK-bridge trace leg via the facade's `NodeSdk`/`WebSdk` `Configuration.spanProcessor`, on the protobuf-wire selection
- catalog-verdict: KEEP as SDK-bridge protobuf peer completing the JSON/protobuf wire pair; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[OTEL_PIN_BLOCK]`-collapse member (native `OtlpTracer` supersedes)
- runtime: dual — the package `browser` field remaps `platform/index` to select `platform/node` (uses `http`/`https`, config `OTLPExporterNodeConfigBase`) or `platform/browser` (uses `fetch`, config `OTLPExporterConfigBase`); ONE `OTLPTraceExporter` name, a build-time platform selection, never a fork
- modules: `OTLPTraceExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `SpanExporter` and its config
- rail: observability/export/trace
- One exporter class, `OTLPExporterBase<ReadableSpan[]> implements SpanExporter` — the endpoint, headers, compression, and timeout are CONFIG values on one class, never a subclass per backend or per compression. Node/browser split is the package `browser` field remap; the node config extends the browser+base config by node-only transport fields, and `CompressionAlgorithm` resolves `"none" | "gzip"`. Protobuf encoding is a fixed serializer binding inside the package, invisible at the constructor surface.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                                    |
| :-----: | :---------------------------------------- | :------------------ | :--------------------------------------------------------------------- |
|  [01]   | `OTLPTraceExporter`                       | span exporter class | concrete exporter a `BatchSpanProcessor` wraps                         |
|  [02]   | `export(items: ReadableSpan[], cb): void` | export method       | inherited from `OTLPExporterBase`; `cb` receives core's `ExportResult` |
|  [03]   | `forceFlush(): Promise<void>`             | lifecycle           | drain-on-exit flush the SDK provider invokes                           |
|  [04]   | `shutdown(): Promise<void>`               | lifecycle           | terminal release the SDK provider invokes                              |
|  [05]   | `OTLPExporterConfigBase`                  | base config         | `url?`, `headers?`, `concurrencyLimit?`, `timeoutMillis?`              |
|  [06]   | `OTLPExporterNodeConfigBase`              | node config         | adds `keepAlive?`, `compression?`, `httpAgentOptions?`, `userAgent?`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf trace export composition
- rail: observability/export/trace
- Exporter is never a leaf: construct it, wrap it in a processor, hand the processor to the facade. `new OTLPTraceExporter(config?)` selects its platform config type by the package `browser` remap. `BatchSpanProcessor` (production, buffered) or `SimpleSpanProcessor` (dev, synchronous) from `sdk-trace-base` is the wrapper; `NodeSdk`/`WebSdk` `Configuration.spanProcessor` is the sink. `url`/`headers`/`compression`/`timeoutMillis` are policy values sourced from config or `OTEL_EXPORTER_OTLP_*` env (via core's readers), never forks; the protobuf wire is fixed by the package, not a config toggle.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new OTLPTraceExporter(config?: OTLPExporterNodeConfigBase)`       | node ctor      | node OTLP/HTTP protobuf span exporter          |
|  [02]   | `new OTLPTraceExporter(config?: OTLPExporterConfigBase)`           | browser ctor   | browser OTLP/HTTP protobuf span exporter (RUM) |
|  [03]   | `new BatchSpanProcessor(exporter)` → `Configuration.spanProcessor` | composition    | exporter → processor → `NodeSdk`/`WebSdk`      |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the `NodeSdk`/`WebSdk` (`[OTEL_PIN_BLOCK]`) protobuf trace leg; the native `Otlp`/`OtlpTracer` lane is the `[OTEL_PIN_BLOCK]`-preferred default that serializes spans over a `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. Reach for this exporter only when the collector demands protobuf on the wire while the SDK processor semantics or a co-resident SDK-only exporter are also required.
- wire-encoding is a serializer binding, never a fork: OTLP/HTTP JSON vs protobuf is the choice between the `.api/opentelemetry-exporter-trace-otlp-http.md` row and this row — two package selections binding two `@opentelemetry/otlp-transformer` serializers, one `OTLPTraceExporter` shape. Node vs browser transport resolves to the package `browser` remap; endpoint/headers resolve to a config value. A backend or encoding change is a composition-root selection, never a second exporter type in design code.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-exporter-trace-otlp-http.md` (the JSON sibling): identical `OTLPTraceExporter`/config/lifecycle surface; this row is the protobuf half of the pair. Choose ONE per span-export lane — the collector's accepted encoding decides; both feed the same processor and facade seams unchanged.
- Stack with `.api/opentelemetry-sdk-trace-base.md` (the wrapping seam): `new OTLPTraceExporter(cfg)` is wrapped in `new BatchSpanProcessor(exporter)` (buffered, production) or `SimpleSpanProcessor` (synchronous, dev) — the exporter is a `SpanExporter`; the processor owns batching, retry, and `forceFlush` drain.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped processor is handed to `NodeSdk.Configuration.spanProcessor` (node/bun; `sdk-trace-node`) or `WebSdk.Configuration.spanProcessor` (browser RUM; `sdk-trace-web`), alongside the one `AppIdentity`-derived `Resource`. Facade owns provider lifecycle; this package owns protobuf wire serialization.
- Stack with `.api/opentelemetry-core.md`: `export()` reports terminal disposition through core's `ExportResult`/`ExportResultCode`; the exporter's own outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced; `timeoutMillis`/`url`/`compression` default from `OTEL_EXPORTER_OTLP_*` env via core's typed readers.
- Stack with `.api/effect-platform.md` posture (the divergence to record): this exporter carries its OWN `http`/`fetch` transport with an internal retrying wrapper — it does NOT ride the `net/client` `HttpClient` retry/proxy policy the native `Otlp` lane inherits. That transport-policy gap is a concrete reason `otel/emit` prefers the native lane and marks this row `[OTEL_PIN_BLOCK]`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); the exporter is constructed at the composition root only. Instrumentation code uses Effect's native `Effect.withSpan` and never imports this package.
- prefer the native `Otlp`/`OtlpTracer` lane; reach for this SDK exporter only for protobuf-wire SDK-only processor/exporter capability, and record it as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- Browser exporter is the RUM egress leg (`otel/vital`/`otel/crash`) on collectors demanding protobuf; apply the export-boundary redaction policy rows before the span leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-proto`
- Owns: OTLP/HTTP protobuf span serialization — one `OTLPTraceExporter` (`SpanExporter`) over a node or browser transport binding the `ProtobufTraceSerializer`, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPTraceExporter(cfg)` wrapped in a `BatchSpanProcessor`/`SimpleSpanProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.spanProcessor`; endpoint/runtime as config + platform-remap selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpTracer` suffices, both the JSON and protobuf exporter rows on one span-export lane, a subclass per backend/compression (that is config), treating the node/browser platform remap as a fork, an unwrapped exporter handed straight to the facade (it needs a processor)
