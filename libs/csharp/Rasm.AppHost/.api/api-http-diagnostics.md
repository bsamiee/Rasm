# [RASM_APPHOST_API_HTTP_DIAGNOSTICS]

`Microsoft.Extensions.Http.Diagnostics` owns outbound-`HttpClient` diagnostics on the factory pipeline: a per-request latency breakdown, redaction-aware structured request/response logging, and downstream-dependency request metadata. Its registration extensions bind onto `IServiceCollection` and `IHttpClientBuilder`, layering measurement and redacted log records over the same named-client handler chain the resilience and service-discovery handlers occupy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Diagnostics`
- package: `Microsoft.Extensions.Http.Diagnostics`
- assembly: `Microsoft.Extensions.Http.Diagnostics`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http.Diagnostics`, `Microsoft.Extensions.Http.Latency`, `Microsoft.Extensions.Http.Logging`
- asset: runtime library
- rail: transport diagnostics

## [02]-[PUBLIC_TYPES]

[DI_EXTENSIONS]: registration classes, namespace `Microsoft.Extensions.DependencyInjection`

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CONSUMER]           | [CAPABILITY]                              |
| :-----: | :--------------------------------------------- | :------------ | :------------------- | :---------------------------------------- |
|  [01]   | `HttpClientLatencyTelemetryExtensions`         | DI extension  | `IServiceCollection` | latency-breakdown telemetry admission     |
|  [02]   | `HttpClientLoggingServiceCollectionExtensions` | DI extension  | `IServiceCollection` | extended logging + log-enricher admission |
|  [03]   | `HttpClientLoggingHttpClientBuilderExtensions` | DI extension  | `IHttpClientBuilder` | extended logging on one named client      |
|  [04]   | `HttpDiagnosticsServiceCollectionExtensions`   | DI extension  | `IServiceCollection` | downstream-dependency metadata admission  |

[OPTIONS]: bound option records

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [CAPABILITY]                                                        |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------------------------------------ |
|  [01]   | `LoggingOptions`                    | logging options | request/response logging, body capture, and redaction policy        |
|  [02]   | `HttpClientLatencyTelemetryOptions` | latency options | `EnableDetailedLatencyBreakdown` checkpoint toggle (default `true`) |

[LOGGING_KNOBS]: `LoggingOptions` policy surface
- `LoggingOptions` knobs: `LogRequestStart` `LogBody` `LogContentHeaders`, `BodySizeLimit` (default `32768`), `BodyReadTimeout` (default `1s`), `RequestPathLoggingMode` (default `Formatted`), `RequestPathParameterRedactionMode`, and the `RequestBodyContentTypes`/`ResponseBodyContentTypes` media allow-sets.
- `LoggingOptions` redaction maps: `RequestHeadersDataClasses` `ResponseHeadersDataClasses` `RequestQueryParametersDataClasses` `RouteParameterDataClasses` — each `IDictionary<string, DataClassification>`.

[LOGGING_SUPPORT]: log-shape contracts, namespace `Microsoft.Extensions.Http.Logging`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                                           |
| :-----: | :-------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `IHttpClientLogEnricher`    | enricher contract | `Enrich(IEnrichmentTagCollector, request, response?, exception?)` hook |
|  [02]   | `HttpClientLoggingTagNames` | tag catalog       | const emitted keys, read as the `TagNames` list                        |
|  [03]   | `OutgoingPathLoggingMode`   | path-mode enum    | `Formatted` / `Structured`                                             |

[LOG_TAG_KEYS]: `server.address` `http.request.method` `url.path` `url.query` `http.request.header.` `http.response.header.` `http.response.status_code` `Duration` `RequestBody` `ResponseBody`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline admission

| [INDEX] | [SURFACE]                         | [SHAPE] | [CAPABILITY]                                                                         |
| :-----: | :-------------------------------- | :------ | :----------------------------------------------------------------------------------- |
|  [01]   | `AddHttpClientLatencyTelemetry`   | static  | bare, `IConfigurationSection`, `Action<HttpClientLatencyTelemetryOptions>` overloads |
|  [02]   | `AddExtendedHttpClientLogging`    | static  | `IServiceCollection` and `IHttpClientBuilder` overload families                      |
|  [03]   | `AddHttpClientLogEnricher<T>`     | static  | admit one `IHttpClientLogEnricher` implementation                                    |
|  [04]   | `AddDownstreamDependencyMetadata` | static  | instance and generic `IDownstreamDependencyMetadata` overloads                       |

- `AddExtendedHttpClientLogging` on `IHttpClientBuilder` carries a `wrapHandlersPipeline` overload positioning the logger around, not inside, the delegating-handler chain; the `IServiceCollection` form applies it to every named client.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AddHttpClientLatencyTelemetry` installs a handler recording per-stage checkpoints into the latency context; `EnableDetailedLatencyBreakdown` selects the per-stage breakdown over one coarse duration.
- `AddExtendedHttpClientLogging` replaces the framework's default `HttpClient` logger with a redaction-aware one emitting the `HttpClientLoggingTagNames` keys; `AddHttpClientLogEnricher` implementations append tags per request.
- Header, query, and route values log verbatim unless a `DataClassification` map assigns a redactor-backed class; a `DataClassification.None` entry is the no-redaction default.

[STACKING]:
- `OpenTelemetry.Instrumentation.Http`: owns the outbound client SPAN; these latency checkpoints are a stage breakdown inside the request, never a second span, so the two never emit duplicate client traces.
- `Microsoft.Extensions.Telemetry`(`api-telemetry.md`): supplies the latency-context ledger the breakdown checkpoints write into.
- `Microsoft.Extensions.Telemetry.Abstractions`(`api-telemetry-abstractions.md`): owns the `IEnrichmentTagCollector` the enricher fills and the `HttpRouteParameterRedactionMode` and classified-tag redaction the `DataClassification` maps resolve against before any sink observes them.
- `Microsoft.Extensions.Http.Resilience`(`api-resilience.md`): resilience, service-discovery, and these diagnostics handlers share one named-client pipeline; `wrapHandlersPipeline` decides whether logging observes pre- or post-retry attempts.

[LOCAL_ADMISSION]:
- Latency and extended logging register once per named-client factory at the composition root the outbound boundary owns.
- Extended logging is the sole logger on an instrumented client; leaving the built-in client logger active double-logs every request.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Http.Diagnostics`
- Owns: outbound-request latency breakdown, redaction-aware extended logging, and downstream-dependency request metadata
- Accept: factory-pipeline admission through `AddHttpClientLatencyTelemetry` / `AddExtendedHttpClientLogging` with `DataClassification`-mapped redaction
- Reject: the built-in client logger left active beside the extended logger; span-shaped latency records duplicating the OpenTelemetry HTTP client span
