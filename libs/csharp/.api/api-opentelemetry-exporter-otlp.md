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

- `ApplyConfigurationUsingSpecificationEnvVars` parses the endpoint, protocol, headers, timeout, and compression keys during OPTIONS CONSTRUCTION, so a value the registration delegate assigns lands after the parse and outranks it; the type's own remark omits the compression key while the body binds it, and every option left unassigned falls to its shipped default — `Grpc` at 4317 and `None` — so a wire pin carried by an env key nothing publishes ships gRPC and uncompressed.
- Setting `Endpoint` disables per-signal path appending, so an explicit endpoint carries its own `/v1/<signal>` suffix.
- `HttpClientFactory` is the one seam a persistent-queue or custom-certificate handler installs through, and the SHIPPED factory a set value displaces is the sole application point for both `HttpClient.Timeout` and the mutual-auth client — a replacement carries both or loses both.
- mTLS carriers stay internal with no public option, armed from `OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE` and `OTEL_EXPORTER_OTLP_CLIENT_KEY` inside that shipped factory alone, so a deployment configuring mutual auth through environment loses it the moment a composition seats its own factory.
- `TimeoutMilliseconds` still bounds the transmission handler's retry deadline whatever factory runs, so a supplied client left at `HttpClient`'s own default outlives the entire retry envelope on one hung request; a replacement factory sets that timeout from the same option or the drain band stalls past its window.
- Export sends run SYNCHRONOUSLY off `HttpClient.Send` wherever the transport is not http/2 and the platform admits a synchronous send; that call reaches `HttpMessageHandler.Send` and never `SendAsync`, so a `DelegatingHandler` installed through the factory overrides BOTH legs or drops out of the whole http/protobuf path.
- Exporter shutdown only cancels pending requests and never disposes the supplied client, so a handler-owned resource reaches no release seat and its lifetime belongs to the composition supplying the factory.
- Both shipped request contents are memory-backed and override the synchronous serializer — raw payload content and gzip wrapper alike, the latter carrying `Content-Encoding: gzip` on its own content headers — so a failed batch re-materializes from the live request with no async hop and a stored copy replays byte-identically under the compression already applied.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: egress registration; every `AddOtlpExporter` family carries a `string? name` prefix overload, and the metric and log families a second leg carrying `MetricReaderOptions` or `LogRecordExportProcessorOptions`.

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `IOpenTelemetryBuilder.UseOtlpExporter()`                                  | static  | all three signals on one hosted root     |
|  [02]   | `IOpenTelemetryBuilder.UseOtlpExporter(OtlpExportProtocol, Uri)`           | static  | protocol and base URL inline             |
|  [03]   | `TracerProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`       | static  | span egress with its batch square        |
|  [04]   | `MeterProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`        | static  | metric egress on exporter options alone  |
|  [05]   | `MeterProviderBuilder.AddOtlpExporter(Action<Otlp…, MetricReaderOptions>)` | static  | metric egress with temporality + cadence |
|  [06]   | `LoggerProviderBuilder.AddOtlpExporter(Action<OtlpExporterOptions>)`       | static  | log egress on exporter options alone     |
|  [07]   | `LoggerProviderBuilder.AddOtlpExporter(Action<Otlp…, LogRecordExport…>)`   | static  | log egress with its processor shape      |
|  [08]   | `OpenTelemetryLoggerOptions.AddOtlpExporter(Action<OtlpExporterOptions>)`  | static  | log egress on the `ILogger` bridge seat  |
|  [09]   | `OtlpTraceExporter(OtlpExporterOptions)`                                   | ctor    | exporter instance for a custom processor |

- Rows [05] and [07] are the ONLY public seats for reader temporality, reader cadence, and the log-record batch square: the cross-signal claim verb routes each through `OtlpExporterBuilder`, `OtlpExporterBuilderOptions`, and `IOtlpExporterOptions`, all three `internal` at this pin, so no composition behind row [01] or [02] can set them.

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
- claim root: the two registrations are mutually exclusive — `UseOtlpExporter()` binds once per hosted root, and a second call or a per-signal `AddOtlpExporter` beside it throws `NotSupportedException` at provider build.
- per-signal root: `AddOtlpExporter` seats each builder on both paths, hosted and hostless alike, and it is the form a root reaching any per-signal policy value must take because the claim verb exposes reader, processor, and transport options through internal types alone.
- protocol row: `OtlpExporterOptions.Protocol` pins `HttpProtobuf`, and each signal path appends to the base endpoint.
- batch square: peak rate times batch delay fits the `BatchExportProcessorOptions<Activity>` queue, and the drain window is the provider `ForceFlush` timeout.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): exporter I/O runs inside `SuppressInstrumentationScope.Begin`, and every processor this package registers joins the provider's own drain pair.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `UseOtlpExporter` extends the `IOpenTelemetryBuilder` that `AddOpenTelemetry()` mints.
- `Microsoft.Extensions.Options`(`Rasm.AppHost/.api/api-options.md`): `OtlpExporterOptions` resolves as named options through `IOptionsMonitor<OtlpExporterOptions>`, so the `string? name` prefix on each `AddOtlpExporter` family selects one policy record.
- instrumentation packages(`api-otel-instrumentation-*.md`): each subscribed source and meter produces the spans and metrics this surface drains, so an admitted instrumentation row and an egress row are one decision.
- AppHost observability root(`Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`): `SignalGovernance.Govern` binds the three per-signal seats — `SpanBatch`, delta temporality beside `ReaderCadence`, `LogBatch` — and `SignalGovernance.Egress` binds each arm's wire, stamping `Protocol` and `Compression` off `WireProtocol`/`WireCompression` and taking `HttpClientFactory` for the queue handler a durable profile arms; `api-otel-persistent-storage.md` owns that handler's lifetime.
- BCL transport trust(`api-bcl-net-http.md`, `api-bcl-cryptography.md`): `OtlpTrust.Mount` reads the `_CERTIFICATE`/`_CLIENT_CERTIFICATE`/`_CLIENT_KEY` rows into `SocketsHttpHandler.SslOptions` — `X509Certificate2.CreateFromPemFile` onto `ClientCertificates`, an `X509ChainPolicy` at `CustomRootTrust` onto `CertificateChainPolicy` — so a replacement factory presents the mutual-auth identity the internal `OtlpMtlsOptions` leg owns.

[LOCAL_ADMISSION]:
- Egress and trust bind from `OTEL_EXPORTER_OTLP_*` — the endpoint, protocol, headers, timeout, and compression keys, their `_TRACES_`/`_METRICS_`/`_LOGS_` per-signal overrides, and the `_CERTIFICATE`/`_CLIENT_CERTIFICATE`/`_CLIENT_KEY` triple; a deployment-plane coordinate rides its key and an estate wire pin rides the options delegate, because that delegate runs after the parse and wins.
- Direct `OtlpTraceExporter`/`OtlpMetricExporter`/`OtlpLogExporter` construction rides a custom `BaseProcessor<T>` seat alone; every ordinary root registers through the extension verbs.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Owns: OTLP egress for traces, metrics, and logs — protocol, endpoint, batch, compression, and transport trust
- Accept: per-signal `AddOtlpExporter` wherever any reader, processor, or transport policy value binds; the one-call claim verb only where every signal takes package defaults
- Reject: Prometheus exporter packages and any second export registration beside the one claim
