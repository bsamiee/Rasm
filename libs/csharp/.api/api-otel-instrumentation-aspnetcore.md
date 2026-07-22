# [RASM_API_OTEL_INSTRUMENTATION_ASPNETCORE]

`OpenTelemetry.Instrumentation.AspNetCore` owns the inbound server leg of a hosting root: request spans carrying filter, enrichment, and exception policy on the trace verb, and the ASP.NET Core server meter family on the metric verb. Every policy an options delegate reaches is a public slot; the gRPC-attribute and query-redaction switches bind from configuration alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.AspNetCore`
- package: `OpenTelemetry.Instrumentation.AspNetCore` (Apache-2.0, OpenTelemetry Authors)
- assembly: `OpenTelemetry.Instrumentation.AspNetCore`
- namespace: `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one policy carrier and the two builder-extension owners that admit it

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `AspNetCoreTraceInstrumentationOptions`                    | class         | per-request filter enrichment and protocol policy |
|  [02]   | `AspNetCoreInstrumentationTracerProviderBuilderExtensions` | class         | trace-verb admission over `TracerProviderBuilder` |
|  [03]   | `AspNetCoreInstrumentationMeterProviderBuilderExtensions`  | class         | metric-verb admission over `MeterProviderBuilder` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AddAspNetCoreInstrumentation` — one admission verb overloaded by receiver and argument shape, each overload returning its receiver

| [INDEX] | [SURFACE]                                                                        | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `(TracerProviderBuilder)`                                                        | static  | default-name admission at package policy |
|  [02]   | `(TracerProviderBuilder, Action<AspNetCoreTraceInstrumentationOptions>)`         | static  | policy delegate on the default name      |
|  [03]   | `(TracerProviderBuilder, string, Action<AspNetCoreTraceInstrumentationOptions>)` | static  | policy scoped to one registration name   |
|  [04]   | `(MeterProviderBuilder)`                                                         | static  | subscribes the ASP.NET Core meter family |

[ENTRYPOINT_SCOPE]: `AspNetCoreTraceInstrumentationOptions` policy slots the trace delegate writes

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Filter -> Func<HttpContext, bool>?`                        | property | per-request collection gate            |
|  [02]   | `EnrichWithHttpRequest -> Action<Activity, HttpRequest>?`   | property | request-side span enrichment           |
|  [03]   | `EnrichWithHttpResponse -> Action<Activity, HttpResponse>?` | property | response-side span enrichment          |
|  [04]   | `EnrichWithException -> Action<Activity, Exception>?`       | property | exception-side span enrichment         |
|  [05]   | `RecordException -> bool`                                   | property | projects exceptions as `ActivityEvent` |
|  [06]   | `EnableAspNetCoreSignalRSupport -> bool`                    | property | SignalR hub activity recording         |
|  [07]   | `EnableRazorComponentsSupport -> bool`                      | property | Razor component activity recording     |

- `Filter`: a `false` return or a thrown delegate drops the request; SignalR and Razor recording start on, exception recording off.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Trace admission subscribes the ASP.NET Core hosting diagnostic events and resolves options per registration name, so a named registration carries its own filter and enrich policy.
- Metric admission subscribes the whole ASP.NET Core meter family and mints no instrument, so a hand-added `AddMeter` row under-subscribes what the verb already covers.
- gRPC RPC attributes and URL-query redaction bind from `IConfiguration` at options construction through `OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_ENABLE_GRPC_INSTRUMENTATION` and `OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_DISABLE_URL_QUERY_REDACTION`; the options delegate reaches neither.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): inbound extraction reads the composite W3C propagator `Sdk.SetDefaultTextMapPropagator` seats at the root, and the request activity parents every span the trace opens downstream.
- `OpenTelemetry.Instrumentation.Http`(`api-otel-instrumentation-http.md`): outbound client spans nest inside the request span, so one server-root sampler verdict decides the whole fan-out and neither leg doubles the other.
- `Grpc.AspNetCore`(`api-grpc-aspnetcore.md`): server endpoints gain spans through this subscription, never a service-side interceptor shim.
- `OpenTelemetry.Instrumentation.GrpcNetClient`(`api-otel-instrumentation-grpcnetclient.md`): the outbound counterpart partitions gRPC by direction, its `SuppressDownstreamInstrumentation` collapsing the client HTTP leg alone.
- `SignalGovernance.Govern`: AppHost's service-root fold composes the trace verb inside its `WithTracing` delegate beside the baggage and profile processors, one registration covering HTTP and gRPC server spans.

[LOCAL_ADMISSION]:
- Service roots terminating HTTP or gRPC admit the package; a plugin or desktop profile hosts no server surface and never references it.
- `Filter` rejects health-probe and scrape-shaped paths at span creation, ahead of the sampler.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AspNetCore`
- Owns: inbound request spans and the ASP.NET Core server meter family at the service root
- Accept: one trace registration and one metric registration per hosting root, policy carried on options
- Reject: hand-written server-span middleware; a `DiagnosticListener` subscription beside the verb
