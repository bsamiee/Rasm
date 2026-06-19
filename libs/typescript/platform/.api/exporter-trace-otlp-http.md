# [API_CATALOGUE] @opentelemetry/exporter-trace-otlp-http

`@opentelemetry/exporter-trace-otlp-http` supplies `OTLPTraceExporter`, the concrete browser `SpanExporter` that serializes `ReadableSpan[]` to OTLP/JSON and ships them to a collector over HTTP, satisfying the `SpanExporter` contract the `@effect/opentelemetry` `WebSdk` binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-http`
- package: `@opentelemetry/exporter-trace-otlp-http`
- module: `@opentelemetry/exporter-trace-otlp-http`
- asset: runtime library
- rail: tracing

## [02]-[PUBLIC_TYPES]

The package's only own export is `OTLPTraceExporter`; `OTLPExporterConfigBase` and `OTLPExporterBase` are re-listed from `@opentelemetry/otlp-exporter-base` because the constructor config shape and the export/lifecycle base define the consumed contract, and the `[SOURCE_PACKAGE]` column names each symbol's owning package.

[PUBLIC_TYPE_SCOPE]: exporter and configuration family
- rail: tracing

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [SOURCE_PACKAGE]                          | [RAIL]                                                |
| :-----: | :----------------------- | :------------ | :---------------------------------------- | :---------------------------------------------------- |
|  [01]   | `OTLPTraceExporter`      | class         | `@opentelemetry/exporter-trace-otlp-http` | browser `SpanExporter` over OTLP/HTTP                 |
|  [02]   | `OTLPExporterConfigBase` | interface     | `@opentelemetry/otlp-exporter-base`       | `url`, `headers`, `concurrencyLimit`, `timeoutMillis` |
|  [03]   | `OTLPExporterBase`       | class         | `@opentelemetry/otlp-exporter-base`       | shared export/forceFlush/shutdown delegate base       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPTraceExporter construction and lifecycle
- rail: tracing

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :--------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `new OTLPTraceExporter(config?)`         | exporter factory | browser trace exporter construction         |
|  [02]   | `exporter.export(spans, resultCallback)` | span export      | serialize `ReadableSpan[]` and POST to OTLP |
|  [03]   | `exporter.forceFlush()`                  | lifecycle        | resolve when pending exports drain          |
|  [04]   | `exporter.shutdown()`                    | lifecycle        | stop exporter and release the delegate      |

[ENTRYPOINT_SCOPE]: `OTLPExporterConfigBase` fields
- rail: tracing

| [INDEX] | [FIELD]            | [TYPE]                                     | [DEFAULT]                          |
| :-----: | :----------------- | :----------------------------------------- | :--------------------------------- |
|  [01]   | `url`              | `string`                                   | `—` (delegate appends `v1/traces`) |
|  [02]   | `headers`          | `Record<string, string> \| HeadersFactory` | `—`                                |
|  [03]   | `concurrencyLimit` | `number`                                   | `—`                                |
|  [04]   | `timeoutMillis`    | `number`                                   | `10000`                            |

## [04]-[IMPLEMENTATION_LAW]

[TRACE_EXPORT_TOPOLOGY]:
- `OTLPTraceExporter` extends `OTLPExporterBase<ReadableSpan[]>` and implements `SpanExporter`; the browser build wraps the OTLP network export delegate over the Fetch/Beacon transport, never the Node `http`/`https` agent path.
- The browser constructor accepts `OTLPExporterConfigBase`; the Node build accepts the wider `OTLPExporterNodeConfigBase` (keep-alive, compression, http-agent) — admit the browser config shape only.
- `export` is callback-style (`(result: ExportResult) => void`), not promise-returning; `BatchSpanProcessor` from `@opentelemetry/sdk-trace-web` drives it and owns batching and retry.
- `timeoutMillis` bounds each batch export at 10000 ms by default; `headers` accepts a static map or an async `HeadersFactory` for bearer-token rotation.

[LOCAL_ADMISSION]:
- Construct `OTLPTraceExporter` as the concrete `SpanExporter` passed into the `WebSdk` `spanProcessor`/`traceExporter` seam; the `WebSdk` binds it under `BatchSpanProcessor`.
- Set `url` to the collector OTLP/HTTP traces path and `headers` to the auth headers; leave transport batching to the SDK span processor.
- Do not import the Node platform entry in browser code; the package resolves the browser `OTLPTraceExporter` automatically via the conditional `exports` map.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-http`
- Owns: browser OTLP/HTTP span serialization and collector transport
- Accept: `OTLPTraceExporter` as the `SpanExporter` the `WebSdk` requires
- Reject: hand-rolled span POST clients; the Node exporter variant in browser code
