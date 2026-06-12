# [RASM_APPHOST_API_OTEL_INSTRUMENTATION]

`OpenTelemetry.Instrumentation.Runtime` and `OpenTelemetry.Instrumentation.Http`
supply meter-provider admission of .NET runtime metrics and tracer/meter-provider
admission of `HttpClient` request telemetry with filter, enrichment, and exception
recording policy.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime instrumentation family
- rail: observability

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]    | [CAPABILITY]                        |
| :-----: | :------------------------------- | :---------------- | :---------------------------------- |
|   [1]   | `MeterProviderBuilderExtensions` | builder extension | runtime meter admission             |
|   [2]   | `RuntimeInstrumentationOptions`  | options value     | runtime metrics policy (memberless) |

[PUBLIC_TYPE_SCOPE]: HttpClient instrumentation family
- rail: observability

| [INDEX] | [SYMBOL]                                                   | [PACKAGE_ROLE]    | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :---------------- | :------------------------------------------ |
|   [1]   | `HttpClientInstrumentationTracerProviderBuilderExtensions` | builder extension | `HttpClient` trace admission                |
|   [2]   | `HttpClientInstrumentationMeterProviderBuilderExtensions`  | builder extension | `HttpClient` and DNS meter admission        |
|   [3]   | `HttpClientTraceInstrumentationOptions`                    | options value     | filter, enrichment, exception-record policy |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration
- rail: observability

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                  | [CAPABILITY]                                                        |
| :-----: | :----------------------------- | :------------------------------------------------------------ | :------------------------------------------------------------------ |
|   [1]   | `AddRuntimeInstrumentation`    | `MeterProviderBuilder` extension                              | subscribes `System.Runtime` meter on .NET 9+                        |
|   [2]   | `AddRuntimeInstrumentation`    | `Action<RuntimeInstrumentationOptions>?` configurator         | pre-9 internal `RuntimeMetrics` registration                        |
|   [3]   | `AddHttpClientInstrumentation` | `TracerProviderBuilder` extension                             | subscribes `System.Net.Http` activity source                        |
|   [4]   | `AddHttpClientInstrumentation` | `Action<HttpClientTraceInstrumentationOptions>?` configurator | options-configured trace admission                                  |
|   [5]   | `AddHttpClientInstrumentation` | name plus options configurator                                | named-options trace admission                                       |
|   [6]   | `AddHttpClientInstrumentation` | `MeterProviderBuilder` extension                              | subscribes `System.Net.Http` and `System.Net.NameResolution` meters |

[ENTRYPOINT_SCOPE]: HttpClient trace options
- rail: observability

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                             | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :--------------------------------------- | :------------------------------------------------ |
|   [1]   | `FilterHttpRequestMessage`      | `Func<HttpRequestMessage, bool>?`        | per-request collection gate; false or throw drops |
|   [2]   | `EnrichWithHttpRequestMessage`  | `Action<Activity, HttpRequestMessage>?`  | request-side activity enrichment                  |
|   [3]   | `EnrichWithHttpResponseMessage` | `Action<Activity, HttpResponseMessage>?` | response-side activity enrichment                 |
|   [4]   | `EnrichWithException`           | `Action<Activity, Exception>?`           | exception enrichment on all runtimes              |
|   [5]   | `FilterHttpWebRequest`          | `Func<HttpWebRequest, bool>?`            | .NET Framework collection gate                    |
|   [6]   | `EnrichWithHttpWebRequest`      | `Action<Activity, HttpWebRequest>?`      | .NET Framework request enrichment                 |
|   [7]   | `EnrichWithHttpWebResponse`     | `Action<Activity, HttpWebResponse>?`     | .NET Framework response enrichment                |
|   [8]   | `RecordException`               | `bool` property, default false           | records exceptions as `ActivityEvent`             |

[ENTRYPOINT_SCOPE]: runtime instruments (pre-.NET 9 meter `OpenTelemetry.Instrumentation.Runtime`)
- rail: observability

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]               | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :------------------------- | :---------------------------------- |
|   [1]   | `process.runtime.dotnet.gc.collections.count`              | observable counter         | GC collections per generation       |
|   [2]   | `process.runtime.dotnet.gc.objects.size`                   | observable up-down counter | live GC heap object bytes           |
|   [3]   | `process.runtime.dotnet.gc.allocations.size`               | observable counter         | allocated bytes since process start |
|   [4]   | `process.runtime.dotnet.gc.committed_memory.size`          | observable up-down counter | committed GC virtual memory         |
|   [5]   | `process.runtime.dotnet.gc.heap.size`                      | observable up-down counter | heap size per generation            |
|   [6]   | `process.runtime.dotnet.gc.heap.fragmentation.size`        | observable up-down counter | heap fragmentation per generation   |
|   [7]   | `process.runtime.dotnet.gc.duration`                       | observable counter         | total GC pause time                 |
|   [8]   | `process.runtime.dotnet.jit.il_compiled.size`              | observable counter         | compiled IL bytes                   |
|   [9]   | `process.runtime.dotnet.jit.methods_compiled.count`        | observable counter         | JIT-compiled method count           |
|  [10]   | `process.runtime.dotnet.jit.compilation_time`              | observable counter         | JIT compilation time                |
|  [11]   | `process.runtime.dotnet.monitor.lock_contention.count`     | observable counter         | monitor lock contention count       |
|  [12]   | `process.runtime.dotnet.thread_pool.threads.count`         | observable up-down counter | thread pool thread count            |
|  [13]   | `process.runtime.dotnet.thread_pool.completed_items.count` | observable counter         | completed thread pool work items    |
|  [14]   | `process.runtime.dotnet.thread_pool.queue.length`          | observable up-down counter | pending thread pool work items      |
|  [15]   | `process.runtime.dotnet.timer.count`                       | observable up-down counter | active timer count                  |
|  [16]   | `process.runtime.dotnet.assemblies.count`                  | observable up-down counter | loaded assembly count               |
|  [17]   | `process.runtime.dotnet.exceptions.count`                  | counter                    | first-chance exception count        |

## [4]-[IMPLEMENTATION_LAW]

[INSTRUMENTATION_TOPOLOGY]:
- runtime dispatch: .NET 9+ admission is `AddMeter("System.Runtime")` over runtime-emitted metrics; earlier runtimes register the internal `RuntimeMetrics` meter named `OpenTelemetry.Instrumentation.Runtime`
- runtime options: `RuntimeInstrumentationOptions` is memberless; the configurator exists for forward policy only and is ignored on .NET 9+
- exception counting: pre-9 exception counter subscribes `AppDomain.FirstChanceException` once per process, reference counted across instrumentation instances
- http trace dispatch: .NET 7+ admission subscribes the `System.Net.Http` activity source; earlier runtimes add the package source plus legacy source `System.Net.Http.HttpRequestOut`
- http meter dispatch: meter admission is `AddMeter` over runtime-emitted `System.Net.Http` and `System.Net.NameResolution` meters; no package-local HTTP instruments on current TFMs
- options retrieval: trace options resolve through named `IOptionsMonitor<HttpClientTraceInstrumentationOptions>`
- redaction: URL query values are redacted by default; `OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION` binds from environment configuration to disable it

[LOCAL_ADMISSION]:
- Instrumentation registers at composition through provider builders; no manual listener wiring.
- Filter and enrichment delegates are policy values on options, not call-site instrumentation code.
- A throwing filter drops the request silently; filters stay total and side-effect free.
- Runtime metric identity differs across the .NET 9 boundary; dashboards bind to one convention per deployment.

[RAIL_LAW]:
- Packages: `OpenTelemetry.Instrumentation.{Runtime,Http}`
- Owns: runtime and `HttpClient` instrumentation admission
- Accept: builder-registered instrumentation with options-bound policy
- Reject: hand-rolled `DiagnosticListener` subscriptions and ad hoc runtime gauges
