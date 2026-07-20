# [RASM_API_OPENTELEMETRY_EXPORTER_OTLP]

`OpenTelemetry.Exporter.OpenTelemetryProtocol` is the estate's sole telemetry egress: one `UseOtlpExporter()` call claims traces, metrics, and logs together, pushing OTLP over `HttpProtobuf` to the collector gateway. Per-signal `AddOtlpExporter` overloads exist for the hostless plugin path where no cross-cutting builder is available; mixing the two registration styles in one root throws at build.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- assembly: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- namespace: `OpenTelemetry`, `OpenTelemetry.Exporter`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`
- asset: runtime library
- rail: telemetry egress

## [02]-[PUBLIC_TYPES]

[EXPORTER_TYPES]: exporters, options, and the cross-cutting builder
- rail: telemetry egress

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]      | [CAPABILITY]                                             |
| :-----: | :----------------------------------- | :------------------ | :------------------------------------------------------- |
|  [01]   | `OtlpTraceExporter`                  | span exporter       | OTLP trace frames                                        |
|  [02]   | `OtlpMetricExporter`                 | metric exporter     | OTLP metric frames                                       |
|  [03]   | `OtlpLogExporter`                    | log exporter        | OTLP log frames                                          |
|  [04]   | `OtlpExporterOptions`                | egress options      | `Endpoint`, `Protocol`, `Headers`, `TimeoutMilliseconds` |
|  [05]   | `OtlpExportProtocol`                 | protocol vocabulary | `Grpc` / `HttpProtobuf`                                  |
|  [06]   | `OtlpExportCompression`              | compression axis    | payload compression selection                            |
|  [07]   | `OtlpExporterBuilder`                | composite builder   | per-signal shaping inside `UseOtlpExporter`              |
|  [08]   | `OtlpTlsOptions` / `OtlpMtlsOptions` | transport trust     | TLS and mutual-TLS material                              |

`OtlpExporterOptions` carries `ExportProcessorType`, `BatchExportProcessorOptions`, `Compression`, `UserAgentProductIdentifier`, and `HttpClientFactory` beside the endpoint quartet; `IOtlpExporterOptions` is the read contract the composite builder shapes per signal.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration
- rail: telemetry egress

| [INDEX] | [SURFACE]                                      | [KIND]              | [CAPABILITY]                                                                                           |
| :-----: | :--------------------------------------------- | :------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `UseOtlpExporter()`                            | cross-cutting claim | all three signals on `IOpenTelemetryBuilder`, once                                                     |
|  [02]   | `UseOtlpExporter(OtlpExportProtocol, Uri)`     | configured claim    | protocol + endpoint inline                                                                             |
|  [03]   | `UseOtlpExporter(Action<OtlpExporterBuilder>)` | shaped claim        | per-signal option shaping                                                                              |
|  [04]   | `UseOtlpExporter(IConfiguration)`              | bound claim         | endpoint/headers/protocol from configuration                                                           |
|  [05]   | `AddOtlpExporter` (trace)                      | per-signal          | `TracerProviderBuilder`, optional named options                                                        |
|  [06]   | `AddOtlpExporter` (metric)                     | per-signal          | `MeterProviderBuilder`, optional reader-options leg                                                    |
|  [07]   | `AddOtlpExporter` (log)                        | per-signal          | log seats, optional processor-options leg                                                              |

Log registration lands on both `LoggerProviderBuilder` and `OpenTelemetryLoggerOptions`; the `LogRecordExportProcessorOptions` and `MetricReaderOptions` legs shape the processor and reader per signal.

## [04]-[IMPLEMENTATION_LAW]

[EGRESS_TOPOLOGY]:
- claim root: `UseOtlpExporter()` exactly once per hosted root — a second call, or mixing with per-signal `AddOtlpExporter`, throws at provider build
- plugin root: per-signal `AddOtlpExporter` on each `Sdk.Create*ProviderBuilder`, since the hostless path carries no `IOpenTelemetryBuilder`
- protocol row: `OtlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf` — the one estate egress protocol
- binding row: endpoint, headers, and protocol bind from `OTEL_EXPORTER_OTLP_*` configuration, never source literals
- batch square: `BatchExportProcessorOptions` — peak rate times batch delay fits the queue, and the drain window is the provider `ForceFlush` timeout

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): exporter I/O runs inside `SuppressInstrumentationScope.Begin`; the batch processors this package registers drain through the provider `ForceFlush`/`Dispose` pair.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `UseOtlpExporter` extends the `IOpenTelemetryBuilder` that `AddOpenTelemetry()` mints.

[LOCAL_ADMISSION]:
- OTLP push is the only telemetry egress — no in-process scrape endpoint exists anywhere in the estate; the collector re-exposes Prometheus downstream.
- Plugin unload forces the batch tail: `AssemblyLoadContext.Unloading` runs `ForceFlush` before `Dispose`, or the last batch drops with the context.
- TLS material rides `OtlpTlsOptions`/`OtlpMtlsOptions` rows bound from configuration, never inline certificate handling.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP egress for all three signals, its protocol/endpoint/batch policy, and transport trust
- Accept: one `UseOtlpExporter` per hosted root; per-signal `AddOtlpExporter` on hostless plugin builders
- Reject: Prometheus exporter packages and any second export registration beside the one claim
