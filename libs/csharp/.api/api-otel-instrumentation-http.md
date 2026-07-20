# [RASM_API_OTEL_INSTRUMENTATION_HTTP]

`OpenTelemetry.Instrumentation.Http` enriches outbound `HttpClient`/`HttpWebRequest` client spans — filter, enrichment, and exception-recording knobs over the trace leg. Metrics need no package: the runtime emits the `System.Net.Http` and `System.Net.NameResolution` meters natively, admitted by name with `AddMeter`; this package's metric verb subscribes the same built-ins for composition symmetry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Http`
- package: `OpenTelemetry.Instrumentation.Http`
- assembly: `OpenTelemetry.Instrumentation.Http`
- namespace: `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- asset: runtime library
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[OPTION_TYPES]: trace shaping
- rail: transport instrumentation

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]                        |
| :-----: | :-------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `HttpClientTraceInstrumentationOptions` | trace options  | filter, enrich, and redaction knobs |

Options members: `FilterHttpRequestMessage`, `EnrichWithHttpRequestMessage`, `EnrichWithHttpResponseMessage`, `EnrichWithException`, `RecordException`, `DisableUrlQueryRedaction`, and the `HttpWebRequest` mirror trio `FilterHttpWebRequest`/`EnrichWithHttpWebRequest`/`EnrichWithHttpWebResponse`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission
- rail: transport instrumentation

| [INDEX] | [SURFACE]                      | [KIND]           | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `AddHttpClientInstrumentation` | trace admission  | `TracerProviderBuilder`, optional `Action<HttpClientTraceInstrumentationOptions>` |
|  [02]   | `AddHttpClientInstrumentation` | metric admission | `MeterProviderBuilder`; subscribes the built-in HTTP meters                       |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- trace root: `AddHttpClientInstrumentation(static http => http.RecordException = true)` inside the root's `WithTracing` delegate
- metric root: `AddMeter("System.Net.Http")` / `AddMeter("System.Net.NameResolution")` — runtime-native meters, no shim instruments

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): both verbs are builder rows; enrichment delegates run at span start/end under the root sampler's verdict.
- `Microsoft.Extensions.Http.Resilience`: resilience-pipeline retries appear as sibling client spans under one parent, so retry fan-out is trace-visible without extra wiring.

[LOCAL_ADMISSION]:
- Composition-root-only: the package subscribes emission the BCL client already produces; libraries never reference it.
- URL query redaction stays on — `DisableUrlQueryRedaction` flips only where a store's query string is proven identifier-free under the redaction taxonomy.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.Http`
- Owns: outbound HTTP client-span enrichment at the composition root
- Accept: trace-leg admission with options shaping; built-in meters admitted by name
- Reject: a library reference; a metric shim where the runtime meter already emits
