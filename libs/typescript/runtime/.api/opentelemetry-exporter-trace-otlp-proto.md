# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_TRACE_OTLP_PROTO]

`@opentelemetry/exporter-trace-otlp-proto` binds `ProtobufTraceSerializer` (`@opentelemetry/otlp-transformer`) into the shared `OTLPTraceExporter`, POSTing `ReadableSpan` batches to an OTLP/HTTP collector as protobuf — the binary sibling of `.api/opentelemetry-exporter-trace-otlp-http.md`, which owns the exporter/config/lifecycle surface and binds the JSON serializer instead. A `BatchSpanProcessor` wraps it and feeds the facade `Configuration.spanProcessor`; this row completes the `[OTLP_SDK]` block's protobuf trace leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-proto`
- package: `@opentelemetry/exporter-trace-otlp-proto` (Apache-2.0)
- otel-peer: `@opentelemetry/api`, `@opentelemetry/core` (the `ExportResult` rail), `@opentelemetry/sdk-trace-base` (the `SpanExporter`/`ReadableSpan` contract + the `BatchSpanProcessor` wrapper)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies `OTLPExporterConfigBase`/`OTLPExporterNodeConfigBase` and `CompressionAlgorithm`; `@opentelemetry/otlp-transformer` supplies the `ProtobufTraceSerializer` this row binds
- consumed-by: `otel/emit` SDK-bridge trace leg via the facade `Configuration.spanProcessor`, on the protobuf-wire selection
- catalog-verdict: KEEP as the protobuf half of the SDK-bridge trace pair; `[OTLP_SDK]` protobuf leg, `[OTEL_PIN_BLOCK]`-collapse member
- runtime: dual — the package `browser` field remaps `platform/index` to `platform/node` (`http`/`https`, `OTLPExporterNodeConfigBase`) or `platform/browser` (`fetch`, `OTLPExporterConfigBase`); one class name, a build-time platform selection
- modules: `OTLPTraceExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one protobuf `SpanExporter`
- rail: observability/export/trace
- `OTLPTraceExporter extends OTLPExporterBase<ReadableSpan[]> implements SpanExporter`; the `export`/`forceFlush`/`shutdown` lifecycle and the `OTLPExporterConfigBase`/`OTLPExporterNodeConfigBase` config live in `.api/opentelemetry-exporter-trace-otlp-http.md` and are reused here — the one member this package adds is the constructor that binds `ProtobufTraceSerializer`.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `OTLPTraceExporter`              | class         | protobuf span exporter a `BatchSpanProcessor` wraps     |
|  [02]   | `new OTLPTraceExporter(config?)` | ctor          | binds `ProtobufTraceSerializer` into `OTLPExporterBase` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf trace export composition
- rail: observability/export/trace
- Exporter is never a leaf: construct it, wrap it in `BatchSpanProcessor` (buffered, production) or `SimpleSpanProcessor` (synchronous, dev), hand the processor to the facade `Configuration.spanProcessor`. `url`/`headers`/`compression`/`timeoutMillis` source from config or `OTEL_EXPORTER_OTLP_*` env via core's readers; the protobuf wire is fixed by the package, never a config toggle.

| [INDEX] | [SURFACE]                                                          | [SHAPE]     | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :---------- | :-------------------------------------------- |
|  [01]   | `new OTLPTraceExporter(config?)`                                   | ctor        | node/browser OTLP/HTTP protobuf span exporter |
|  [02]   | `new BatchSpanProcessor(exporter)` → `Configuration.spanProcessor` | composition | exporter → processor → `NodeSdk`/`WebSdk`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Wire-encoding is a serializer binding, never a fork: JSON versus protobuf picks the `.api/opentelemetry-exporter-trace-otlp-http.md` row or this one, node versus browser transport a `browser`-field remap, endpoint a config value — a backend or encoding change is a composition-root selection, never a second exporter type.
- `.api/effect-opentelemetry.md` owns the native-first `[OTEL_PIN_BLOCK]`-collapse dual-lane law this leg composes under.

[STACKING]:
- `.api/opentelemetry-exporter-trace-otlp-http.md`: JSON sibling that owns the `OTLPTraceExporter`/config/lifecycle surface; this row is its protobuf half — one per span-export lane, the collector's accepted encoding decides, both feed the same processor and facade seams unchanged.
- `.api/opentelemetry-sdk-trace-base.md`: `new OTLPTraceExporter(cfg)` wraps in `new BatchSpanProcessor(exporter)` or `SimpleSpanProcessor`; the processor owns batching, retry, and the `forceFlush` drain over this `SpanExporter`.
- `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk`: the wrapped processor is handed to `Configuration.spanProcessor` (node/bun `sdk-trace-node`; browser `sdk-trace-web`) alongside the one `AppIdentity`-derived `Resource`; the facade owns provider lifecycle, this package owns protobuf wire serialization.
- `.api/opentelemetry-core.md`: `export()` reports terminal disposition through `ExportResult`/`ExportResultCode`, the outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced, and `timeoutMillis`/`url`/`compression` default from `OTEL_EXPORTER_OTLP_*` env via core's typed readers.
- `.api/effect-platform.md`: this exporter carries its own `http`/`fetch` transport with an internal retry wrapper, never the `net/client` `HttpClient` retry/proxy policy the native lane inherits — the transport-policy gap that marks this row `[OTEL_PIN_BLOCK]`.
- `otel/emit` (within-lib): the export-boundary owner constructs this exporter at the composition root and applies the egress-redaction rows before a span leaves the browser RUM leg (`otel/vital`/`otel/crash`).

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger); construct at the composition root, and reach past the native `OtlpTracer` default to this exporter only for protobuf-wire SDK-only processor/exporter capability as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-proto`
- Owns: OTLP/HTTP protobuf span serialization — `ProtobufTraceSerializer` bound into one `OTLPTraceExporter` (`SpanExporter`) over a node or browser transport
- Accept: `new OTLPTraceExporter(cfg)` wrapped in a `BatchSpanProcessor`/`SimpleSpanProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.spanProcessor`; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: a hand-rolled protobuf span serializer, both the JSON and protobuf rows on one span-export lane, a subclass per backend/compression, an unwrapped exporter handed straight to the facade
