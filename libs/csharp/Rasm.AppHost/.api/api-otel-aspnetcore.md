# [RASM_APPHOST_API_OTEL_ASPNETCORE]

`OpenTelemetry.Instrumentation.AspNetCore` admits inbound ASP.NET Core request telemetry onto the provider builders: one tracer-builder extension subscribing the hosting activity source with filter, enrichment, and exception-record policy, and one meter-builder extension subscribing the runtime-emitted hosting meters.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.AspNetCore`
- package: `OpenTelemetry.Instrumentation.AspNetCore`
- assembly: `OpenTelemetry.Instrumentation.AspNetCore`
- namespace: `OpenTelemetry.Trace`
- namespace: `OpenTelemetry.Metrics`
- namespace: `OpenTelemetry.Instrumentation.AspNetCore`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: inbound request instrumentation family

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `AspNetCoreInstrumentationTracerProviderBuilderExtensions` | static class  | inbound request trace admission             |
|  [02]   | `AspNetCoreInstrumentationMeterProviderBuilderExtensions`  | static class  | hosting meter admission                     |
|  [03]   | `AspNetCoreTraceInstrumentationOptions`                    | class         | filter, enrichment, exception-record policy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `AddAspNetCoreInstrumentation(TracerProviderBuilder)`                          | static  | sub: hosting activity source        |
|  [02]   | `AddAspNetCoreInstrumentation(TracerProviderBuilder, Action<Options>)`         | static  | filter and enrichment policy        |
|  [03]   | `AddAspNetCoreInstrumentation(TracerProviderBuilder, string, Action<Options>)` | static  | named-options trace policy          |
|  [04]   | `AddAspNetCoreInstrumentation(MeterProviderBuilder)`                           | static  | sub: runtime-emitted hosting meters |

[ENTRYPOINT_SCOPE]: trace options

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Filter: Func<HttpContext, bool>?`                        | property | per-request gate; false or throw drops     |
|  [02]   | `EnrichWithHttpRequest: Action<Activity, HttpRequest>?`   | property | request-side activity enrichment           |
|  [03]   | `EnrichWithHttpResponse: Action<Activity, HttpResponse>?` | property | response-side activity enrichment          |
|  [04]   | `EnrichWithException: Action<Activity, Exception>?`       | property | exception activity enrichment              |
|  [05]   | `RecordException: bool`                                   | property | records exceptions as `ActivityEvent`; off |
|  [06]   | `EnableAspNetCoreSignalRSupport: bool`                    | property | SignalR hub span shaping; on               |
|  [07]   | `EnableRazorComponentsSupport: bool`                      | property | Razor component span shaping; on           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- trace dispatch subscribes the runtime-emitted hosting activity source; options resolve through named `IOptionsMonitor<AspNetCoreTraceInstrumentationOptions>`
- meter dispatch subscribes the runtime-emitted hosting meters; the package defines no package-local instruments
- URL query values redact by default; the env var `OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_DISABLE_URL_QUERY_REDACTION` is the sole opt-out

[STACKING]:
- Registers at the service-root `SignalGovernance` composition beside `AddHttpClientInstrumentation` and `AddGrpcClientInstrumentation`, so inbound server spans, outbound client spans, and gRPC client spans fold into one trace under one sampler verdict.
- Inbound gRPC calls served by the companion `ControlService` carry rpc semconv only under the env var `OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_ENABLE_GRPC_INSTRUMENTATION`; the client side rides `api-otel-grpcnetclient.md`.
- Route-parameter redaction composes with the estate `RequestMetadata`/`HttpRouteParameterRedactionMode` prevention row, so an inbound span never carries an unredacted route segment.

[LOCAL_ADMISSION]:
- Registration is composition-root-only at service app roots; plugin and desktop profiles carry no ASP.NET host and never reference the package.
- Filter and enrichment delegates are total side-effect-free policy values; a throwing filter drops the request.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AspNetCore`
- Owns: inbound ASP.NET Core trace and hosting-meter admission
- Accept: builder-registered instrumentation with options-bound policy at service roots
- Reject: hand-rolled `DiagnosticListener` subscriptions; per-handler span creation beside the subscribed source
