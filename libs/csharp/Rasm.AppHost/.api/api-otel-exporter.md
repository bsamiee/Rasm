# [RASM_APPHOST_API_OTEL_EXPORTER]

`OpenTelemetry.Exporter.OpenTelemetryProtocol` owns OTLP export of traces, metrics, and logs over gRPC or HTTP/Protobuf. `UseOtlpExporter` wires all three signals onto one `IOpenTelemetryBuilder` in a single call, and `OtlpExporterOptions` binds endpoint, protocol, compression, headers, and timeout. A service composition root is the only site that instantiates the exporter; lower branch code emits `ILogger` and minted `ActivitySource`/`Meter` pairs this surface projects to the collector.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- assembly: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- namespace: `OpenTelemetry.Exporter`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: OTLP exporter surface

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `OtlpTraceExporter`                          | class         | `BaseExporter<Activity>` sink                     |
|  [02]   | `OtlpMetricExporter`                         | class         | `BaseExporter<Metric>` sink                       |
|  [03]   | `OtlpLogExporter`                            | class         | `BaseExporter<LogRecord>` sink                    |
|  [04]   | `OtlpExporterOptions`                        | class         | endpoint, protocol, compression, headers, timeout |
|  [05]   | `OtlpExportProtocol`                         | enum          | `Grpc` / `HttpProtobuf`                           |
|  [06]   | `OtlpExportCompression`                      | enum          | `None` / `GZip`                                   |
|  [07]   | `OpenTelemetryBuilderOtlpExporterExtensions` | class         | `UseOtlpExporter` unified wire-up                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified signal registration

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                             |
| :-----: | :----------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `UseOtlpExporter()`                        | static  | wires logging, metrics, and tracing OTLP |
|  [02]   | `UseOtlpExporter(OtlpExportProtocol, Uri)` | static  | protocol and base-URL override           |

[ENTRYPOINT_SCOPE]: `OtlpExporterOptions` get/set properties

| [INDEX] | [SURFACE]                     | [CAPABILITY]                                       |
| :-----: | :---------------------------- | :------------------------------------------------- |
|  [01]   | `Endpoint`                    | `Uri`; default `localhost:4317` gRPC / `4318` HTTP |
|  [02]   | `Protocol`                    | `Grpc` default                                     |
|  [03]   | `Headers`                     | `key=value,...` header string                      |
|  [04]   | `TimeoutMilliseconds`         | default `10000`                                    |
|  [05]   | `Compression`                 | `None` default                                     |
|  [06]   | `UserAgentProductIdentifier`  | token prepended to the exporter `User-Agent`       |
|  [07]   | `HttpClientFactory`           | custom `Func<HttpClient>`, `HttpProtobuf` only     |
|  [08]   | `ExportProcessorType`         | `Batch` default, traces only                       |
|  [09]   | `BatchExportProcessorOptions` | batch processor tuning, traces only                |

[ENTRYPOINT_SCOPE]: environment keys, parsed during `OtlpExporterOptions` construction

| [INDEX] | [ENV_VAR]                                                  | [SCOPE]    | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------- | :--------- | :---------------------------------------------- |
|  [01]   | `OTEL_EXPORTER_OTLP_ENDPOINT`                              | all-signal | base endpoint; signal path appended             |
|  [02]   | `OTEL_EXPORTER_OTLP_PROTOCOL`                              | all-signal | `grpc` or `http/protobuf`                       |
|  [03]   | `OTEL_EXPORTER_OTLP_HEADERS`                               | all-signal | `k=v,k=v` header string                         |
|  [04]   | `OTEL_EXPORTER_OTLP_TIMEOUT`                               | all-signal | integer milliseconds                            |
|  [05]   | `OTEL_EXPORTER_OTLP_COMPRESSION`                           | all-signal | `none` or `gzip`                                |
|  [06]   | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`                       | traces     | per-signal endpoint override                    |
|  [07]   | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT`                      | metrics    | per-signal endpoint override                    |
|  [08]   | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`                         | logs       | per-signal endpoint override                    |
|  [09]   | `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`        | metrics    | cumulative / delta / lowmemory temporality      |
|  [10]   | `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` | metrics    | explicit-bucket or base2-exponential histograms |
|  [11]   | `OTEL_EXPORTER_OTLP_CERTIFICATE`                           | mTLS       | CA certificate path                             |
|  [12]   | `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE`                    | mTLS       | client certificate path                         |
|  [13]   | `OTEL_EXPORTER_OTLP_CLIENT_KEY`                            | mTLS       | client private key path                         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UseOtlpExporter` runs at most once; a second call or a mix with per-signal `AddOtlpExporter` raises `NotSupportedException`.
- `UseOtlpExporter` activates the tracing, metric, and logging provider legs in one pass.
- Setting `Endpoint` disables `HttpProtobuf` signal-path appending; a per-signal env var overrides the base set and disables appending on its own endpoint.
- mTLS binds only through `OTEL_EXPORTER_OTLP_CERTIFICATE`, `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE`, and `OTEL_EXPORTER_OTLP_CLIENT_KEY`; the TLS carriers stay internal with no public option.

[STACKING]:
- `api-otel.md`(`.api/api-otel.md`): the `Otlp{Trace,Metric,Log}Exporter` sinks extend the core `BaseExporter<T>` and terminate a provider's `AddProcessor` / `AddReader` rail; `ExportProcessorType` and `BatchExportProcessorOptions` tune the trace batch stage.
- `api-otel-hosting.md`(`.api/api-otel-hosting.md`): `UseOtlpExporter` composes on the `IOpenTelemetryBuilder` that `AddOpenTelemetry` opens, so one call installs the OTLP sink across every hosted provider leg.
- `api-otel-aspnetcore.md`(`.api/api-otel-aspnetcore.md`) + `api-otel-instrumentation.md`(`.api/api-otel-instrumentation.md`): the inbound-request, `HttpClient`, and runtime instrumentation packages produce the spans and metrics this surface drains from every subscribed source and meter.
- `api-options.md`(`.api/api-options.md`): `OtlpExporterOptions` binds as named options resolved through `IOptionsMonitor<OtlpExporterOptions>` at composition.

[LOCAL_ADMISSION]:
- One `UseOtlpExporter` at the service composition root exports all signals to one endpoint; per-signal `AddOtlpExporter` is the alternative only when signals need distinct endpoints or protocols.
- Endpoint, protocol, compression, and headers drive through env vars or `OtlpExporterOptions` properties, never call-site literals.
- `HttpClientFactory` overrides for custom certificate handling or `HttpClient` lifetime control on `HttpProtobuf`.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP signal export for traces, metrics, and logs
- Accept: `UseOtlpExporter` on `IOpenTelemetryBuilder`; `OtlpExporterOptions` property and env-var configuration
- Reject: direct use of `Implementation.*` internal types or hand-rolled protobuf serialization
