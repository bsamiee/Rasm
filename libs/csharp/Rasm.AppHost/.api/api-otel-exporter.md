# [RASM_APPHOST_API_OTEL_EXPORTER]

`OpenTelemetry.Exporter.OpenTelemetryProtocol` supplies OTLP gRPC and HTTP/Protobuf exporters for traces, metrics, and logs, a unified `UseOtlpExporter` builder extension that wires all three signals at once, typed options for endpoint, protocol, compression, and headers, and per-signal exporter and processor builder surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- assembly: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- namespace: `OpenTelemetry.Exporter`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter type family
- rail: telemetry

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                    |
| :-----: | :------------------- | :-------------- | :------------------------ |
|   [1]   | `OtlpTraceExporter`  | trace exporter  | `BaseExporter<Activity>`  |
|   [2]   | `OtlpMetricExporter` | metric exporter | `BaseExporter<Metric>`    |
|   [3]   | `OtlpLogExporter`    | log exporter    | `BaseExporter<LogRecord>` |

[PUBLIC_TYPE_SCOPE]: options and builder family
- rail: telemetry

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]     | [RAIL]                                      |
| :-----: | :--------------------------- | :---------------- | :------------------------------------------ |
|   [1]   | `IOtlpExporterOptions`       | options contract  | endpoint, protocol, headers, timeout        |
|   [2]   | `OtlpExporterOptions`        | options class     | `IOtlpExporterOptions` implementation       |
|   [3]   | `OtlpExporterBuilderOptions` | multi-signal opts | default + per-signal `IOtlpExporterOptions` |
|   [4]   | `OtlpExportProtocol`         | protocol enum     | `Grpc` / `HttpProtobuf`                     |
|   [5]   | `OtlpExportCompression`      | compression enum  | `None` / `GZip`                             |
|   [6]   | `OtlpMtlsOptions`            | mTLS options      | cert paths for mutual TLS                   |
|   [7]   | `OtlpTlsOptions`             | TLS options       | server cert path                            |
|   [8]   | `OtlpSignalType`             | signal enum       | `Traces` / `Metrics` / `Logs`               |

[PUBLIC_TYPE_SCOPE]: builder extension family
- namespace: `OpenTelemetry`
- rail: telemetry

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------- |
|   [1]   | `OpenTelemetryBuilderOtlpExporterExtensions` | builder ext   | `UseOtlpExporter` unified wire-up |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified signal registration
- rail: telemetry

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]  | [RAIL]                                   |
| :-----: | :-------------------------------------------------- | :-------------- | :--------------------------------------- |
|   [1]   | `UseOtlpExporter(this IOpenTelemetryBuilder)`       | all-signal wire | enables logging + metrics + tracing OTLP |
|   [2]   | `UseOtlpExporter(builder, OtlpExportProtocol, Uri)` | protocol+URL    | protocol and base URL override           |

[ENTRYPOINT_SCOPE]: options properties
- rail: telemetry

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------------------------- |
|   [1]   | `OtlpExporterOptions.Endpoint`                    | URI property     | default `localhost:4317` (gRPC) / `4318` (HTTP) |
|   [2]   | `OtlpExporterOptions.Protocol`                    | protocol prop    | `OtlpExportProtocol.Grpc` default               |
|   [3]   | `OtlpExporterOptions.Headers`                     | string property  | `key=value,...` header string                   |
|   [4]   | `OtlpExporterOptions.TimeoutMilliseconds`         | timeout          | default 10000 ms                                |
|   [5]   | `OtlpExporterOptions.Compression`                 | compression prop | `OtlpExportCompression.None` default            |
|   [6]   | `OtlpExporterOptions.HttpClientFactory`           | factory          | custom `Func<HttpClient>` override              |
|   [7]   | `OtlpExporterOptions.ExportProcessorType`         | processor        | `Batch` default (traces only)                   |
|   [8]   | `OtlpExporterOptions.BatchExportProcessorOptions` | batch            | batch processor tuning (traces)                 |

[ENTRYPOINT_SCOPE]: environment variable keys
- rail: telemetry

| [INDEX] | [ENV_VAR]                               | [SURFACE]    | [RAIL]                              |
| :-----: | :-------------------------------------- | :----------- | :---------------------------------- |
|   [1]   | `OTEL_EXPORTER_OTLP_ENDPOINT`           | all-signal   | base endpoint; signal path appended |
|   [2]   | `OTEL_EXPORTER_OTLP_PROTOCOL`           | all-signal   | `grpc` or `http/protobuf`           |
|   [3]   | `OTEL_EXPORTER_OTLP_HEADERS`            | all-signal   | `k=v,k=v` header string             |
|   [4]   | `OTEL_EXPORTER_OTLP_TIMEOUT`            | all-signal   | integer milliseconds                |
|   [5]   | `OTEL_EXPORTER_OTLP_COMPRESSION`        | all-signal   | `none` or `gzip`                    |
|   [6]   | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`    | traces only  | per-signal endpoint override        |
|   [7]   | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT`   | metrics only | per-signal endpoint override        |
|   [8]   | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`      | logs only    | per-signal endpoint override        |
|   [9]   | `OTEL_EXPORTER_OTLP_CERTIFICATE`        | mTLS         | CA certificate path                 |
|  [10]   | `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE` | mTLS         | client certificate path             |
|  [11]   | `OTEL_EXPORTER_OTLP_CLIENT_KEY`         | mTLS         | client private key path             |

## [4]-[IMPLEMENTATION_LAW]

[OTLP_TOPOLOGY]:
- primary public namespaces: `OpenTelemetry`, `OpenTelemetry.Exporter`
- internal namespaces (not for direct use): `...Implementation`, `...Implementation.ExportClient`, `...Implementation.Serializer`, `...Implementation.Transmission`
- signal exporters: `OtlpTraceExporter`, `OtlpMetricExporter`, `OtlpLogExporter` — all extend `BaseExporter<T>`
- `UseOtlpExporter` may be called at most once; subsequent calls or mixing with per-signal `AddOtlpExporter` raises `NotSupportedException`
- `UseOtlpExporter` automatically enables `WithTracing`, `WithMetrics`, and `WithLogging` on the builder
- protocol: `Grpc` default endpoint `localhost:4317`; `HttpProtobuf` default endpoint `localhost:4318` with signal path appended
- setting `Endpoint` directly disables signal-path appending for `HttpProtobuf`
- env vars are parsed during `OtlpExporterOptions` construction; per-signal env vars override the base set and disable signal-path appending on their endpoint

[LOCAL_ADMISSION]:
- Prefer `UseOtlpExporter()` at composition for all-signal OTLP export; use per-signal `AddOtlpExporter` only when signals require distinct endpoints or protocols.
- Drive endpoint, protocol, compression, and header configuration through env vars or `OtlpExporterOptions` properties in the composition root.
- `HttpClientFactory` override applies when custom certificate handling or `HttpClient` lifetime management is required.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP signal export (traces, metrics, logs)
- Accept: `UseOtlpExporter` on `IOpenTelemetryBuilder`; `OtlpExporterOptions` property and env-var configuration
- Reject: direct use of `Implementation.*` internal types or manual protobuf serialization
