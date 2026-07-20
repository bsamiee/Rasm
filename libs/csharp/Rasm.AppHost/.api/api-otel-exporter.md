# [RASM_APPHOST_API_OTEL_EXPORTER]

`OpenTelemetry.Exporter.OpenTelemetryProtocol` supplies OTLP gRPC and HTTP/Protobuf exporters for traces, metrics, and logs, a unified `UseOtlpExporter` builder extension that wires all three signals at once, typed `OtlpExporterOptions` for endpoint, protocol, compression, headers, and timeout, and per-signal `BaseExporter<T>` exporter classes.

[APP_ROOT_RESERVED]: `[V15]` — the OTLP exporter is a composition-root instantiation only; the lib emits `ILogger` + minted `ActivitySource`/`Meter` pairs and `UseOtlpExporter` composes at service app roots. The central pin is retained; the row moves out of the lib csproj.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- assembly: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- namespace: `OpenTelemetry.Exporter`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter type family
- rail: telemetry

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                    |
| :-----: | :------------------- | :-------------- | :------------------------ |
|  [01]   | `OtlpTraceExporter`  | trace exporter  | `BaseExporter<Activity>`  |
|  [02]   | `OtlpMetricExporter` | metric exporter | `BaseExporter<Metric>`    |
|  [03]   | `OtlpLogExporter`    | log exporter    | `BaseExporter<LogRecord>` |

[PUBLIC_TYPE_SCOPE]: options and enum family
- rail: telemetry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [RAIL]                                            |
| :-----: | :---------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `OtlpExporterOptions`   | options class    | endpoint, protocol, compression, headers, timeout |
|  [02]   | `OtlpExportProtocol`    | protocol enum    | `Grpc` / `HttpProtobuf`                           |
|  [03]   | `OtlpExportCompression` | compression enum | `None` / `GZip`                                   |

[PUBLIC_TYPE_SCOPE]: builder extension family
- namespace: `OpenTelemetry`
- rail: telemetry

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `OpenTelemetryBuilderOtlpExporterExtensions` | builder ext   | `UseOtlpExporter` unified wire-up |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified signal registration
- rail: telemetry

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]  | [RAIL]                                   |
| :-----: | :-------------------------------------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `UseOtlpExporter(this IOpenTelemetryBuilder)`       | all-signal wire | enables logging + metrics + tracing OTLP |
|  [02]   | `UseOtlpExporter(builder, OtlpExportProtocol, Uri)` | protocol+URL    | protocol and base URL override           |

[ENTRYPOINT_SCOPE]: options properties
- rail: telemetry

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `OtlpExporterOptions.Endpoint`                    | URI property     | default `localhost:4317` (gRPC) / `4318` (HTTP) |
|  [02]   | `OtlpExporterOptions.Protocol`                    | protocol prop    | `OtlpExportProtocol.Grpc` default               |
|  [03]   | `OtlpExporterOptions.Headers`                     | string property  | `key=value,...` header string                   |
|  [04]   | `OtlpExporterOptions.TimeoutMilliseconds`         | timeout          | default 10000 ms                                |
|  [05]   | `OtlpExporterOptions.Compression`                 | compression prop | `OtlpExportCompression.None` default            |
|  [06]   | `OtlpExporterOptions.UserAgentProductIdentifier`  | string property  | product token appended to exporter `User-Agent` |
|  [07]   | `OtlpExporterOptions.HttpClientFactory`           | factory          | custom `Func<HttpClient>`, `HttpProtobuf` only  |
|  [08]   | `OtlpExporterOptions.ExportProcessorType`         | processor        | `Batch` default (traces only)                   |
|  [09]   | `OtlpExporterOptions.BatchExportProcessorOptions` | batch            | batch processor tuning (traces)                 |

[ENTRYPOINT_SCOPE]: environment variable keys
- rail: telemetry

| [INDEX] | [ENV_VAR]                                                  | [SURFACE]    | [RAIL]                                          |
| :-----: | :--------------------------------------------------------- | :----------- | :---------------------------------------------- |
|  [01]   | `OTEL_EXPORTER_OTLP_ENDPOINT`                              | all-signal   | base endpoint; signal path appended             |
|  [02]   | `OTEL_EXPORTER_OTLP_PROTOCOL`                              | all-signal   | `grpc` or `http/protobuf`                       |
|  [03]   | `OTEL_EXPORTER_OTLP_HEADERS`                               | all-signal   | `k=v,k=v` header string                         |
|  [04]   | `OTEL_EXPORTER_OTLP_TIMEOUT`                               | all-signal   | integer milliseconds                            |
|  [05]   | `OTEL_EXPORTER_OTLP_COMPRESSION`                           | all-signal   | `none` or `gzip`                                |
|  [06]   | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`                       | traces only  | per-signal endpoint override                    |
|  [07]   | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT`                      | metrics only | per-signal endpoint override                    |
|  [08]   | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`                         | logs only    | per-signal endpoint override                    |
|  [09]   | `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`        | metrics only | cumulative / delta / lowmemory temporality      |
|  [10]   | `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` | metrics only | explicit-bucket or base2-exponential histograms |
|  [11]   | `OTEL_EXPORTER_OTLP_CERTIFICATE`                           | mTLS         | CA certificate path                             |
|  [12]   | `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE`                    | mTLS         | client certificate path                         |
|  [13]   | `OTEL_EXPORTER_OTLP_CLIENT_KEY`                            | mTLS         | client private key path                         |

## [04]-[IMPLEMENTATION_LAW]

[OTLP_TOPOLOGY]:
- primary public namespaces: `OpenTelemetry`, `OpenTelemetry.Exporter`
- internal namespaces (not for direct use): `...Implementation`, `...Implementation.ExportClient`, `...Implementation.Serializer`, `...Implementation.Transmission`
- signal exporters: `OtlpTraceExporter`, `OtlpMetricExporter`, `OtlpLogExporter` — all extend `BaseExporter<T>`
- TLS and mutual-TLS configure through `OTEL_EXPORTER_OTLP_CERTIFICATE`, `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE`, and `OTEL_EXPORTER_OTLP_CLIENT_KEY`; the `OtlpTlsOptions`/`OtlpMtlsOptions` carriers are internal and expose no public option surface
- `UseOtlpExporter` may be called at most once; subsequent calls or mixing with per-signal `AddOtlpExporter` raises `NotSupportedException`
- `UseOtlpExporter` activates `WithTracing`, `WithMetrics`, and `WithLogging` on the builder
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
