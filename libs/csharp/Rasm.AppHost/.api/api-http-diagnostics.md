# [RASM_APPHOST_API_HTTP_DIAGNOSTICS]

`Microsoft.Extensions.Http.Diagnostics` extends the `HttpClient` factory pipeline with a per-request latency breakdown, extended structured request/response logging with per-header and per-parameter redaction classes, and downstream-dependency request metadata. Its extensions bind onto `IServiceCollection` and `IHttpClientBuilder`; the latency and logging surfaces layer measurement and log records over the outbound handler pipeline the resilience and service-discovery handlers already occupy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Diagnostics`
- package: `Microsoft.Extensions.Http.Diagnostics`
- assembly: `Microsoft.Extensions.Http.Diagnostics`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http.Diagnostics`, `Microsoft.Extensions.Http.Latency`, `Microsoft.Extensions.Http.Logging`
- asset: runtime library
- rail: transport diagnostics

## [02]-[PUBLIC_TYPES]

[DI_EXTENSIONS]: registration surfaces (namespace `Microsoft.Extensions.DependencyInjection`)
- rail: transport diagnostics

| [INDEX] | [SYMBOL]                                       | [TARGET]             | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------- | :------------------- | :----------------------------------------------- |
|  [01]   | `HttpClientLatencyTelemetryExtensions`         | `IServiceCollection` | latency-breakdown telemetry admission            |
|  [02]   | `HttpClientLoggingServiceCollectionExtensions` | `IServiceCollection` | extended logging + log-enricher admission        |
|  [03]   | `HttpClientLoggingHttpClientBuilderExtensions` | `IHttpClientBuilder` | extended logging bound to one named client       |
|  [04]   | `HttpDiagnosticsServiceCollectionExtensions`   | `IServiceCollection` | downstream-dependency request-metadata admission |

[OPTIONS]: bound option records
- rail: transport diagnostics

| [INDEX] | [SYMBOL]                            | [NAMESPACE]                         | [KNOBS]                                                  |
| :-----: | :---------------------------------- | :---------------------------------- | :------------------------------------------------------- |
|  [01]   | `LoggingOptions`                    | `Microsoft.Extensions.Http.Logging` | body, header, query, route, and path-mode logging policy |
|  [02]   | `HttpClientLatencyTelemetryOptions` | `Microsoft.Extensions.Http.Latency` | `EnableDetailedLatencyBreakdown` checkpoint toggle       |

`LoggingOptions` carries `LogRequestStart`, `LogBody`, `BodySizeLimit` (default `32768`), `BodyReadTimeout` (default one second), `RequestBodyContentTypes`/`ResponseBodyContentTypes` allowed-media sets, `LogContentHeaders`, `RequestPathLoggingMode`, and `RequestPathParameterRedactionMode`. Header, query-parameter, and route redaction ride four `IDictionary<string, DataClassification>` maps — `RequestHeadersDataClasses`, `ResponseHeadersDataClasses`, `RequestQueryParametersDataClasses`, `RouteParameterDataClasses` — where a `DataClassification.None` entry means no redaction.

[LOGGING_SUPPORT]: log-shape contracts (namespace `Microsoft.Extensions.Http.Logging`)
- rail: transport diagnostics

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :-------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `IHttpClientLogEnricher`    | enricher hook   | `Enrich(IEnrichmentTagCollector, request, response?, exception?)` |
|  [02]   | `HttpClientLoggingTagNames` | tag catalog     | const log-tag keys + `TagNames` read-only list                    |
|  [03]   | `OutgoingPathLoggingMode`   | path-shape enum | `Formatted` / `Structured`                                        |

`HttpClientLoggingTagNames` fixes the semantic log-tag keys: `server.address`, `http.request.method`, `url.path`, `url.query`, the `http.request.header.` and `http.response.header.` prefixes, and `http.response.status_code`; `Duration`, `RequestBody`, and `ResponseBody` name the timing and body records.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline admission
- rail: transport diagnostics

| [INDEX] | [SURFACE]                         | [KIND]             | [CAPABILITY]                                                      |
| :-----: | :-------------------------------- | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `AddHttpClientLatencyTelemetry`   | latency admission  | bare, `IConfigurationSection`, and `Action<...Options>` overloads |
|  [02]   | `AddExtendedHttpClientLogging`    | logging admission  | service-collection and client-builder overload families           |
|  [03]   | `AddHttpClientLogEnricher<T>`     | enricher admission | registers one `IHttpClientLogEnricher` implementation             |
|  [04]   | `AddDownstreamDependencyMetadata` | metadata admission | instance and generic `IDownstreamDependencyMetadata` overloads    |

`AddExtendedHttpClientLogging` on `IHttpClientBuilder` adds a `wrapHandlersPipeline` overload pair that positions the logger around, rather than inside, the delegating-handler chain; the `IServiceCollection` form applies the extended logger to every named client. Each family also accepts an `IConfigurationSection` and an `Action<LoggingOptions>` configuration overload.

## [04]-[IMPLEMENTATION_LAW]

[DIAGNOSTICS_TOPOLOGY]:
- latency root: `AddHttpClientLatencyTelemetry` installs a handler that records fine-grained checkpoints into the latency context; `EnableDetailedLatencyBreakdown` (default `true`) toggles the per-stage breakdown against one coarse duration.
- logging root: `AddExtendedHttpClientLogging` replaces the framework's default `HttpClient` logging with a redaction-aware logger emitting the `HttpClientLoggingTagNames` keys; enrichers registered through `AddHttpClientLogEnricher` append tags per request.
- redaction root: header, query, and route values stay unlogged unless a `DataClassification` map assigns a redactor-backed class; unclassified members carry `DataClassification.None` and log verbatim.

[STACKING]:
- `OpenTelemetry.Instrumentation.Http`(`api-otel-instrumentation-http.md`): that package owns the outbound client SPAN; this package's latency checkpoints are a stage BREAKDOWN inside the request, not a second span — the two never emit duplicate client traces.
- `Microsoft.Extensions.Telemetry`(`api-telemetry.md`): supplies the latency-context ledger the breakdown checkpoints write into and the `IEnrichmentTagCollector` the log enricher fills.
- `Microsoft.Extensions.Compliance.Redaction`(`api-telemetry-abstractions.md`): binds the redactor the `DataClassification` maps resolve, so classified headers and parameters redact before any sink observes them.
- `Microsoft.Extensions.Http.Resilience`(`api-resilience.md`): resilience, service-discovery, and these diagnostics handlers share one named-client pipeline; the `wrapHandlersPipeline` flag decides whether logging observes pre- or post-retry attempts.

[LOCAL_ADMISSION]:
- composition-root-only, on the named `HttpClient` the outbound boundary owns; latency and extended logging register once per client factory.
- extended logging supersedes the built-in client logging — enabling both double-logs, so the extended form is the sole logger on an instrumented client.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Http.Diagnostics`
- Owns: outbound-request latency breakdown, redaction-aware extended logging, and downstream-dependency request metadata
- Accept: factory-pipeline admission through `AddHttpClientLatencyTelemetry` / `AddExtendedHttpClientLogging` with `DataClassification`-mapped redaction
- Reject: the built-in client logger left active beside the extended logger; span-shaped latency records duplicating the OpenTelemetry HTTP client span
