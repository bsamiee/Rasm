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

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :---------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OtlpExporterOptions`   | class         | one egress policy record per registration |
|  [02]   | `OtlpExportProtocol`    | enum          | `Grpc` / `HttpProtobuf`                   |
|  [03]   | `OtlpExportCompression` | enum          | `None` / `GZip`                           |
|  [04]   | `OtlpTraceExporter`     | class         | `BaseExporter<Activity>` span frames      |
|  [05]   | `OtlpMetricExporter`    | class         | `BaseExporter<Metric>` metric frames      |
|  [06]   | `OtlpLogExporter`       | class         | `BaseExporter<LogRecord>` log frames      |

[OtlpExporterOptions]: `Endpoint` `Protocol` `Headers` `TimeoutMilliseconds` `Compression` `ExportProcessorType` `BatchExportProcessorOptions` `HttpClientFactory` `UserAgentProductIdentifier`

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- claim root: `UseOtlpExporter()` binds once per hosted root; a second call, or a per-signal `AddOtlpExporter` beside it, throws `NotSupportedException` at provider build.
- plugin root: per-signal `AddOtlpExporter` seats each `Sdk.Create*ProviderBuilder`, the hostless path carrying no `IOpenTelemetryBuilder`.
- protocol row: `OtlpExporterOptions.Protocol` pins `HttpProtobuf`, and each signal path appends to the base endpoint.
- batch square: peak rate times batch delay fits the `BatchExportProcessorOptions<Activity>` queue, and the drain window is the provider `ForceFlush` timeout.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): exporter I/O runs inside `SuppressInstrumentationScope.Begin`, and every processor this package registers joins the provider's own drain pair.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `UseOtlpExporter` extends the `IOpenTelemetryBuilder` that `AddOpenTelemetry()` mints.
- AppHost observability root: one named `OtlpExporterOptions` serves every signal seat — the `string? name` prefix selects it per `AddOtlpExporter` family, and the metric and log second legs shape `MetricReaderOptions` and `LogRecordExportProcessorOptions` against that one policy.

[LOCAL_ADMISSION]:
- Egress and trust bind from `OTEL_EXPORTER_OTLP_*` — the endpoint, headers, timeout, and compression keys, their `_TRACES_`/`_METRICS_`/`_LOGS_` per-signal overrides, and the `_CERTIFICATE`/`_CLIENT_CERTIFICATE`/`_CLIENT_KEY` triple; source literals carry neither.
- Direct `OtlpTraceExporter`/`OtlpMetricExporter`/`OtlpLogExporter` construction rides a custom `BaseProcessor<T>` seat alone; every ordinary root registers through the extension verbs.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP egress for traces, metrics, and logs — protocol, endpoint, batch, compression, and transport trust
- Accept: one `UseOtlpExporter` per hosted root; per-signal `AddOtlpExporter` on hostless plugin builders
- Reject: Prometheus exporter packages and any second export registration beside the one claim
