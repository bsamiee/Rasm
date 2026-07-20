# [RASM_APPHOST_API_OTEL_INSTRUMENTATION]

`OpenTelemetry.Instrumentation.Runtime` and `OpenTelemetry.Instrumentation.Http` supply meter-provider admission of .NET runtime metrics and tracer/meter-provider admission of `HttpClient` request telemetry with filter, enrichment, and exception recording policy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Runtime`
- package: `OpenTelemetry.Instrumentation.Runtime`
- assembly: `OpenTelemetry.Instrumentation.Runtime`
- namespace: `OpenTelemetry.Metrics`
- namespace: `OpenTelemetry.Instrumentation.Runtime`
- asset: runtime library
- rail: observability

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Http`
- package: `OpenTelemetry.Instrumentation.Http`
- assembly: `OpenTelemetry.Instrumentation.Http`
- namespace: `OpenTelemetry.Trace`
- namespace: `OpenTelemetry.Metrics`
- namespace: `OpenTelemetry.Instrumentation.Http`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime instrumentation family
- rail: observability

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]    | [CAPABILITY]                        |
| :-----: | :------------------------------- | :---------------- | :---------------------------------- |
|  [01]   | `MeterProviderBuilderExtensions` | builder extension | runtime meter admission             |
|  [02]   | `RuntimeInstrumentationOptions`  | options value     | runtime metrics policy (memberless) |

[PUBLIC_TYPE_SCOPE]: HttpClient instrumentation family
- rail: observability

| [INDEX] | [SYMBOL]                                                   | [PACKAGE_ROLE]    | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `HttpClientInstrumentationTracerProviderBuilderExtensions` | builder extension | `HttpClient` trace admission                |
|  [02]   | `HttpClientInstrumentationMeterProviderBuilderExtensions`  | builder extension | `HttpClient` and DNS meter admission        |
|  [03]   | `HttpClientTraceInstrumentationOptions`                    | options value     | filter, enrichment, exception-record policy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration
- rail: observability

`AddRuntimeInstrumentation` subscribes the built-in `System.Runtime` meter where the runtime emits it; runtimes without that meter register the package-local `RuntimeMetrics` meter and honor `RuntimeInstrumentationOptions`.

| [INDEX] | [SURFACE]                      | [BUILDER_SIGNAL]     | [OPTIONS]                               | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------------- | :-------------------------------------- | :------------------------------------- |
|  [01]   | `AddRuntimeInstrumentation`    | metrics runtime      | —                                       | sub: `System.Runtime` meter            |
|  [02]   | `AddRuntimeInstrumentation`    | metrics runtime      | `RuntimeInstrumentationOptions`         | package-local meter policy             |
|  [03]   | `AddHttpClientInstrumentation` | tracing `HttpClient` | —                                       | sub: `System.Net.Http` activity source |
|  [04]   | `AddHttpClientInstrumentation` | tracing `HttpClient` | `HttpClientTraceInstrumentationOptions` | filter and enrichment policy           |
|  [05]   | `AddHttpClientInstrumentation` | tracing `HttpClient` | name + trace options                    | named-options trace policy             |
|  [06]   | `AddHttpClientInstrumentation` | metrics HTTP + DNS   | —                                       | sub: HTTP and name-resolution meters   |

[ENTRYPOINT_SCOPE]: HttpClient trace options
- rail: observability

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                             | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :--------------------------------------- | :------------------------------------------------ |
|  [01]   | `FilterHttpRequestMessage`      | `Func<HttpRequestMessage, bool>?`        | per-request collection gate; false or throw drops |
|  [02]   | `EnrichWithHttpRequestMessage`  | `Action<Activity, HttpRequestMessage>?`  | request-side activity enrichment                  |
|  [03]   | `EnrichWithHttpResponseMessage` | `Action<Activity, HttpResponseMessage>?` | response-side activity enrichment                 |
|  [04]   | `EnrichWithException`           | `Action<Activity, Exception>?`           | exception enrichment on all runtimes              |
|  [05]   | `FilterHttpWebRequest`          | `Func<HttpWebRequest, bool>?`            | .NET Framework collection gate                    |
|  [06]   | `EnrichWithHttpWebRequest`      | `Action<Activity, HttpWebRequest>?`      | .NET Framework request enrichment                 |
|  [07]   | `EnrichWithHttpWebResponse`     | `Action<Activity, HttpWebResponse>?`     | .NET Framework response enrichment                |
|  [08]   | `RecordException`               | `bool` property, default false           | records exceptions as `ActivityEvent`             |

[ENTRYPOINT_SCOPE]: runtime metric families (`System.Runtime` meter)
- rail: observability

The `System.Runtime` meter emits these families under the `dotnet.*` semantic convention.

| [INDEX] | [DOMAIN]                    | [INSTRUMENT_KIND]          | [CAPABILITY]                        |
| :-----: | :-------------------------- | :------------------------- | :---------------------------------- |
|  [01]   | GC collections              | observable counter         | collections per generation          |
|  [02]   | GC heap size                | observable up-down counter | live heap bytes per generation      |
|  [03]   | GC allocations              | observable counter         | allocated bytes since process start |
|  [04]   | GC committed memory         | observable up-down counter | committed GC virtual memory         |
|  [05]   | GC heap fragmentation       | observable up-down counter | fragmentation per generation        |
|  [06]   | GC pause                    | observable counter         | total GC pause time                 |
|  [07]   | JIT compiled IL             | observable counter         | compiled IL bytes                   |
|  [08]   | JIT compiled methods        | observable counter         | JIT-compiled method count           |
|  [09]   | JIT compilation time        | observable counter         | JIT compilation time                |
|  [10]   | monitor lock contention     | observable counter         | monitor lock contention count       |
|  [11]   | thread pool threads         | observable up-down counter | thread pool thread count            |
|  [12]   | thread pool completed items | observable counter         | completed thread pool work items    |
|  [13]   | thread pool queue length    | observable up-down counter | pending thread pool work items      |
|  [14]   | active timers               | observable up-down counter | active timer count                  |
|  [15]   | loaded assemblies           | observable up-down counter | loaded assembly count               |
|  [16]   | exceptions                  | counter                    | thrown exception count              |

## [04]-[IMPLEMENTATION_LAW]

[INSTRUMENTATION_TOPOLOGY]:
- runtime dispatch: runtimes that emit `System.Runtime` metrics subscribe it via `AddMeter("System.Runtime")`; runtimes without that meter register the package-local `RuntimeMetrics` meter named `OpenTelemetry.Instrumentation.Runtime`
- runtime options: `RuntimeInstrumentationOptions` is memberless; the configurator exists for forward policy only and is ignored when the built-in runtime meter is active
- http trace dispatch: runtimes that emit the `System.Net.Http` activity source subscribe it directly; runtimes without that source add the package source plus the legacy `System.Net.Http.HttpRequestOut` source
- http meter dispatch: meter admission is `AddMeter` over runtime-emitted `System.Net.Http` and `System.Net.NameResolution` meters; `OpenTelemetry.Instrumentation.Http` defines no package-local HTTP instruments for admitted target frameworks
- options retrieval: trace options resolve through named `IOptionsMonitor<HttpClientTraceInstrumentationOptions>`
- redaction: URL query values are redacted by default; `OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION` binds from environment configuration to disable it

[LOCAL_ADMISSION]:
- Instrumentation registers at composition through provider builders; no manual listener wiring.
- Filter and enrichment delegates are policy values on options, not call-site instrumentation code.
- A throwing filter drops the request silently; filters stay total and side-effect free.
- Runtime metric identity follows the runtime-emitter boundary; dashboards bind to one convention per deployment.

[RAIL_LAW]:
- Packages: `OpenTelemetry.Instrumentation.{Runtime,Http}`
- Owns: runtime and `HttpClient` instrumentation admission
- Accept: builder-registered instrumentation with options-bound policy
- Reject: hand-rolled `DiagnosticListener` subscriptions and ad hoc runtime gauges
