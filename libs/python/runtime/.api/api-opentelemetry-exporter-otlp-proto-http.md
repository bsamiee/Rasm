# [PY_RUNTIME_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_HTTP]

`opentelemetry-exporter-otlp-proto-http` supplies the OTLP/HTTP exporters for spans, metrics, and logs: protobuf-over-HTTP transmission, endpoint/header/timeout/compression configuration, and the environment-variable knobs that drive them. The runtime catalogues the exporter so the host composition root attaches it; the runtime never configures export itself.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-http`
- package: `opentelemetry-exporter-otlp-proto-http`
- import: `opentelemetry.exporter.otlp.proto.http`
- version: `1.42.1`
- owner: `runtime`
- rail: observability
- namespaces: `opentelemetry.exporter.otlp.proto.http.trace_exporter`, `opentelemetry.exporter.otlp.proto.http.metric_exporter`, `opentelemetry.exporter.otlp.proto.http._log_exporter`
- capability: OTLP/HTTP span/metric/log exporters, endpoint/header/timeout/compression configuration, environment-variable knobs

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `trace_exporter.OTLPSpanExporter` | exporter | OTLP/HTTP span exporter |
| [2] | `metric_exporter.OTLPMetricExporter` | exporter | OTLP/HTTP metric exporter |
| [3] | `_log_exporter.OTLPLogExporter` | exporter | OTLP/HTTP log exporter |
| [4] | `Compression` | enum | gzip/none payload compression |
| [5] | `trace_exporter.SpanExportResult` | result | export success/failure value |

[PUBLIC_TYPE_SCOPE]: configuration constant family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `trace_exporter.DEFAULT_ENDPOINT` | constant | default collector base URL |
| [2] | `trace_exporter.DEFAULT_TRACES_EXPORT_PATH` | constant | default traces path |
| [3] | `trace_exporter.DEFAULT_TIMEOUT` | constant | default export timeout |
| [4] | `trace_exporter.DEFAULT_COMPRESSION` | constant | default compression |
| [5] | `OTEL_EXPORTER_OTLP_ENDPOINT` | env knob | collector endpoint override |
| [6] | `OTEL_EXPORTER_OTLP_HEADERS` | env knob | auth/header override |
| [7] | `OTEL_EXPORTER_OTLP_TIMEOUT` | env knob | timeout override |
| [8] | `OTEL_EXPORTER_OTLP_COMPRESSION` | env knob | compression override |
| [9] | `OTEL_EXPORTER_OTLP_CERTIFICATE` | env knob | TLS CA certificate |
| [10] | `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE` | env knob | mTLS client certificate |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exporter operations
- rail: observability

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `OTLPSpanExporter(endpoint=..., headers=..., timeout=..., compression=...)` | build | construct span exporter |
| [2] | `OTLPMetricExporter(...)` | build | construct metric exporter |
| [3] | `OTLPLogExporter(...)` | build | construct log exporter |
| [4] | `OTLPSpanExporter.export` | export | transmit a span batch |
| [5] | `OTLPSpanExporter.shutdown` | drain | stop the exporter |
| [6] | `OTLPSpanExporter.force_flush` | drain | flush pending spans |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- composition law: the exporter is constructed once at the host composition root and attached to the SDK `BatchSpanProcessor`/`BatchLogRecordProcessor`; the runtime emits against the API and holds no exporter handle.
- configuration law: endpoint, headers, timeout, and compression are supplied explicitly at construction from the host settings model; the env-var knobs are documented as the override surface, not read directly in package source.
- transport law: this exporter uses HTTP/protobuf; it shares the `httpx`/`requests` transport posture and the gzip `Compression` knob, never a hand-rolled HTTP client.
- drain law: shutdown/force-flush participate in the host drain choreography; the drain receipt records exporter flush completion.

[LOCAL_ADMISSION]:
- The runtime planning boundary forbids exporter ownership inside the runtime; this catalogue exists so the host composition root attaches the exporter with explicit configuration.
- Span/metric/log signals originate from the runtime API surface; this exporter is the egress the host wires, never invoked from runtime domain logic.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-http`
- Owns: OTLP/HTTP export of spans, metrics, and logs for the host composition root
- Accept: host-constructed exporters with explicit endpoint/headers/timeout/compression, batch-processor attachment, drain participation
- Reject: exporter construction inside the runtime, direct env-var reads in package source, a hand-rolled HTTP transmitter
