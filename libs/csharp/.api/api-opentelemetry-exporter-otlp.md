# [RASM_API_OPENTELEMETRY_EXPORTER_OTLP]

`OpenTelemetry.Exporter.OpenTelemetryProtocol` pushes every signal — traces, metrics, and logs — to the collector gateway as OTLP frames over gRPC or HTTP/protobuf, on hosted and hostless roots alike.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- assembly: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- namespace: `OpenTelemetry`, `OpenTelemetry.Exporter`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`
- rail: telemetry egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: egress policy, wire vocabularies, and the three signal exporters

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OtlpExporterOptions`                        | class         | one egress policy record per registration |
|  [02]   | `OtlpExportProtocol`                         | enum          | `Grpc` / `HttpProtobuf`                   |
|  [03]   | `OtlpExportCompression`                      | enum          | `None` / `GZip`                           |
|  [04]   | `OtlpTraceExporter`                          | class         | `BaseExporter<Activity>` span frames      |
|  [05]   | `OtlpMetricExporter`                         | class         | `BaseExporter<Metric>` metric frames      |
|  [06]   | `OtlpLogExporter`                            | class         | `BaseExporter<LogRecord>` log frames      |
|  [07]   | `OpenTelemetryBuilderOtlpExporterExtensions` | static class  | the `UseOtlpExporter` claim verb          |

[OTLP_OPTION_ROWS]: `OtlpExporterOptions` properties, each with its shipped default

| [INDEX] | [PROPERTY]                    | [DEFAULT_AND_SCOPE]                                                         |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Endpoint`                    | `Uri`; `localhost:4317` under `Grpc`, `localhost:4318` under `HttpProtobuf` |
|  [02]   | `Protocol`                    | `Grpc`                                                                      |
|  [03]   | `Headers`                     | `k=v,k=v` header string                                                     |
|  [04]   | `TimeoutMilliseconds`         | `10000`                                                                     |
|  [05]   | `Compression`                 | `None`                                                                      |
|  [06]   | `UserAgentProductIdentifier`  | token prepended to the exporter `User-Agent`                                |
|  [07]   | `HttpClientFactory`           | `Func<HttpClient>`, honored on `HttpProtobuf` alone                         |
|  [08]   | `ExportProcessorType`         | `Batch`, traces alone                                                       |
|  [09]   | `BatchExportProcessorOptions` | trace batch tuning, traces alone                                            |

- Setting `Endpoint` disables per-signal path appending, so an explicit endpoint carries its own `/v1/<signal>` suffix.
- `HttpClientFactory` is the one seam a persistent-queue or custom-certificate handler installs through; the TLS carriers stay internal with no public option.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: egress registration; every `AddOtlpExporter` family carries a `string? name` prefix overload, and the metric and log families a second leg carrying `MetricReaderOptions` or `LogRecordExportProcessorOptions`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------ | :--------------------------------------- |
|  [01]   | `IOpenTelemetryBuilder.UseOtlpExporter()`                                 | static  | all three signals on one hosted root     |
|  [02]   | `IOpenTelemetryBuilder.UseOtlpExporter(OtlpExportProtocol, Uri)`          | static  | protocol and base URL inline             |
|  [03]   | `TracerProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`      | static  | span egress on a hostless tracer root    |
|  [04]   | `MeterProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`       | static  | metric egress on a hostless meter root   |
|  [05]   | `LoggerProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`      | static  | log egress on a hostless logger root     |
|  [06]   | `OpenTelemetryLoggerOptions.AddOtlpExporter(Action<OtlpExporterOptions>)` | static  | log egress on the `ILogger` bridge seat  |
|  [07]   | `OtlpTraceExporter(OtlpExporterOptions)`                                  | ctor    | exporter instance for a custom processor |

[ENV_ENTRY]: environment keys parsed during `OtlpExporterOptions` construction — the one configuration door, source literals carrying none

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

- Per-signal endpoint overrides disable path appending on that signal alone, so the three keys set whole or not at all.
- Rows [09] and [10] are where the wire temporality and histogram-aggregation defaults bind, so a governance table pins them here rather than at each instrument.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- claim root: `UseOtlpExporter()` binds once per hosted root; a second call, or a per-signal `AddOtlpExporter` beside it, throws `NotSupportedException` at provider build.
- plugin root: per-signal `AddOtlpExporter` seats each `Sdk.Create*ProviderBuilder`, the hostless path carrying no `IOpenTelemetryBuilder`.
- protocol row: `OtlpExporterOptions.Protocol` pins `HttpProtobuf`, and each signal path appends to the base endpoint.
- batch square: peak rate times batch delay fits the `BatchExportProcessorOptions<Activity>` queue, and the drain window is the provider `ForceFlush` timeout.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): exporter I/O runs inside `SuppressInstrumentationScope.Begin`, and every processor this package registers joins the provider's own drain pair.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `UseOtlpExporter` extends the `IOpenTelemetryBuilder` that `AddOpenTelemetry()` mints.
- `Microsoft.Extensions.Options`(`Rasm.AppHost/.api/api-options.md`): `OtlpExporterOptions` resolves as named options through `IOptionsMonitor<OtlpExporterOptions>`, so the `string? name` prefix on each `AddOtlpExporter` family selects one policy record.
- instrumentation packages(`api-otel-instrumentation-*.md`): each subscribed source and meter produces the spans and metrics this surface drains, so an admitted instrumentation row and an egress row are one decision.
- AppHost observability root: one named `OtlpExporterOptions` serves every signal seat — the `string? name` prefix selects it per `AddOtlpExporter` family, and the metric and log second legs shape `MetricReaderOptions` and `LogRecordExportProcessorOptions` against that one policy.

[LOCAL_ADMISSION]:
- Egress and trust bind from `OTEL_EXPORTER_OTLP_*` — the endpoint, headers, timeout, and compression keys, their `_TRACES_`/`_METRICS_`/`_LOGS_` per-signal overrides, and the `_CERTIFICATE`/`_CLIENT_CERTIFICATE`/`_CLIENT_KEY` triple; source literals carry neither.
- Direct `OtlpTraceExporter`/`OtlpMetricExporter`/`OtlpLogExporter` construction rides a custom `BaseProcessor<T>` seat alone; every ordinary root registers through the extension verbs.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP egress for traces, metrics, and logs — protocol, endpoint, batch, compression, and transport trust
- Accept: one `UseOtlpExporter` per hosted root; per-signal `AddOtlpExporter` on hostless plugin builders
- Reject: Prometheus exporter packages and any second export registration beside the one claim
