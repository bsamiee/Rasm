# [RASM_API_OTEL_INSTRUMENTATION_HTTP]

`OpenTelemetry.Instrumentation.Http` owns the outbound client leg: SDK context propagation onto every `HttpClient` call, and the filter, enrichment, and exception-recording seat no runtime-native attribute exposes. Runtime-native span and metric attributes stand as emitted — this surface layers over them and overwrites nothing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Http`
- package: `OpenTelemetry.Instrumentation.Http` (Apache-2.0, OpenTelemetry Authors)
- assembly: `OpenTelemetry.Instrumentation.Http`
- namespace: `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one options seat and the two builder-extension owners

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `HttpClientTraceInstrumentationOptions`                    | class         | the whole trace-shaping seat           |
|  [02]   | `HttpClientInstrumentationTracerProviderBuilderExtensions` | class         | trace verb in `OpenTelemetry.Trace`    |
|  [03]   | `HttpClientInstrumentationMeterProviderBuilderExtensions`  | class         | metric verb in `OpenTelemetry.Metrics` |

[METER_ROSTER]: the built-in instruments the metric verb admits (runtime-owned; this catalog is the roster's repo carrier)
- shared tags: every `System.Net.Http` row carries `url.scheme` `server.address` `server.port`
- request tags: `http.client.request.duration` adds `http.request.method` `http.response.status_code` `network.protocol.version` `error.type`
- connection tags: the connection pair adds `network.protocol.version` `network.peer.address` `http.connection.state`; `dns.lookup.duration` keys on `dns.question.name` `error.type`

| [INDEX] | [INSTRUMENT]                        | [UNIT]         | [CAPABILITY]                       |
| :-----: | :---------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `http.client.request.duration`      | `s`            | semconv request histogram          |
|  [02]   | `http.client.active_requests`       | `{request}`    | in-flight request level            |
|  [03]   | `http.client.open_connections`      | `{connection}` | pool occupancy by connection state |
|  [04]   | `http.client.connection.duration`   | `s`            | established-connection lifetime    |
|  [05]   | `http.client.request.time_in_queue` | `s`            | wait for a free pool connection    |
|  [06]   | `dns.lookup.duration`               | `s`            | name-resolution histogram          |

## [03]-[ENTRYPOINTS]

[TRACE_ADMISSION]: every `TracerProviderBuilder` overload admits the `System.Net.Http` `ActivitySource`; the options slot alone varies

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :-------------------------------- |
|  [01]   | `AddHttpClientInstrumentation()`                                                      | static  | package-default span shaping      |
|  [02]   | `AddHttpClientInstrumentation(Action<HttpClientTraceInstrumentationOptions>)`         | static  | shapes the default-named options  |
|  [03]   | `AddHttpClientInstrumentation(string, Action<HttpClientTraceInstrumentationOptions>)` | static  | keys one options instance by name |

[METRIC_ADMISSION]: `MeterProviderBuilder.AddHttpClientInstrumentation()`
- shape: bare static verb carrying no options seat
- admits: `System.Net.Http` `System.Net.NameResolution`

[TRACE_OPTIONS]: `HttpClientTraceInstrumentationOptions` members in invocation order; every enrichment delegate fires only where `Activity.IsAllDataRequested` holds

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `FilterHttpRequestMessage`      | property | `Func<HttpRequestMessage, bool>?` collection gate     |
|  [02]   | `EnrichWithHttpRequestMessage`  | property | `Action<Activity, HttpRequestMessage>?` at span start |
|  [03]   | `EnrichWithException`           | property | `Action<Activity, Exception>?` on the failure leg     |
|  [04]   | `EnrichWithHttpResponseMessage` | property | `Action<Activity, HttpResponseMessage>?` at span end  |
|  [05]   | `RecordException`               | property | `bool` projecting the exception as an `ActivityEvent` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `FilterHttpRequestMessage` runs after the sampler verdict, and a `false` return or a thrown exception both drop collection.
- `Activity.Duration` and `http.client.request.duration` close at response-headers read, so response-body time falls outside every span and every bucket.
- Activity status flips to error at a response code of 400 or above with no option set; `RecordException` adds the exception as an `ActivityEvent` beside it.
- Metric-side custom tags ride `HttpMetricsEnrichmentContext.AddCallback` from a `DelegatingHandler` and land only where `http.client.request.duration` is enabled — the trace options carry no metric knob.
- Both built-in meters admit by name through `AddMeter`, so a root already naming them takes the metric leg with no package reference — one spelling per root.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): both verbs are builder rows under `WithTracing`/`WithMetrics`; the root's composite propagator supplies the W3C headers this surface injects, and the root sampler's verdict gates every enrichment delegate.
- `OpenTelemetry.Instrumentation.GrpcNetClient`(`api-otel-instrumentation-grpcnetclient.md`): `GrpcClientTraceInstrumentationOptions.SuppressDownstreamInstrumentation` collapses the transport span this package raises under each RPC, so the pair spans one leg.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): the client resolves its meter through `IMeterFactory.Create("System.Net.Http")`, so a factory-scoped handler isolates same-named meters across co-resident plugin contexts.
- `Microsoft.Extensions.Http.Resilience`(`Rasm.AppHost/.api/api-resilience.md`): pipeline retry and hedging attempts surface as sibling client spans under one parent, so fan-out is trace-visible with no extra registration.
- within-library: one options delegate owns the whole shaping pass — the filter drops a request ahead of enrichment, the request and response delegates bracket the call, and the exception pair splits caller-shaped tags from the semconv event; a named registration gives a co-resident root its own pass on one provider.

[LOCAL_ADMISSION]:
- `url.full` query values redact to `Redacted` behind no public property; the `OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION` environment row is the whole disable seam.
- Metric admission takes both meters whole behind no per-instrument selector, so an unwanted stream drops through an `AddView` row after registration.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.Http`
- Owns: outbound client-span propagation and shaping at the composition root
- Accept: one trace registration per root with its options pass; the built-in meters admitted by name or by the metric verb
- Reject: a hand-written `DelegatingHandler` minting client spans or duration tags
