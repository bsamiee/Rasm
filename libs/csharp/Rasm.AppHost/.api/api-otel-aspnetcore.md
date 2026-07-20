# [RASM_APPHOST_API_OTEL_ASPNETCORE]

`OpenTelemetry.Instrumentation.AspNetCore` admits inbound ASP.NET Core request telemetry onto the provider builders: one tracer-builder extension subscribing the hosting activity source with filter, enrichment, and exception-record policy, and one meter-builder extension subscribing the runtime-emitted hosting meters. Service app roots register it beside the wire host; plugin and desktop profiles never load it.

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
- rail: observability

| [INDEX] | [SYMBOL]                                                   | [PACKAGE_ROLE]    | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `AspNetCoreInstrumentationTracerProviderBuilderExtensions` | builder extension | inbound request trace admission             |
|  [02]   | `AspNetCoreInstrumentationMeterProviderBuilderExtensions`  | builder extension | hosting meter admission                     |
|  [03]   | `AspNetCoreTraceInstrumentationOptions`                    | options value     | filter, enrichment, exception-record policy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration
- rail: observability

| [INDEX] | [SURFACE]                      | [BUILDER_SIGNAL] | [OPTIONS]                               | [CAPABILITY]                        |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------- | :---------------------------------- |
|  [01]   | `AddAspNetCoreInstrumentation` | tracing inbound  | —                                       | sub: hosting activity source        |
|  [02]   | `AddAspNetCoreInstrumentation` | tracing inbound  | `AspNetCoreTraceInstrumentationOptions` | filter and enrichment policy        |
|  [03]   | `AddAspNetCoreInstrumentation` | tracing inbound  | name + trace options                    | named-options trace policy          |
|  [04]   | `AddAspNetCoreInstrumentation` | metrics hosting  | —                                       | sub: runtime-emitted hosting meters |

[ENTRYPOINT_SCOPE]: trace options
- rail: observability

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                      | [CAPABILITY]                                      |
| :-----: | :------------------------------- | :-------------------------------- | :------------------------------------------------ |
|  [01]   | `Filter`                         | `Func<HttpContext, bool>?`        | per-request collection gate; false or throw drops |
|  [02]   | `EnrichWithHttpRequest`          | `Action<Activity, HttpRequest>?`  | request-side activity enrichment                  |
|  [03]   | `EnrichWithHttpResponse`         | `Action<Activity, HttpResponse>?` | response-side activity enrichment                 |
|  [04]   | `EnrichWithException`            | `Action<Activity, Exception>?`    | exception enrichment                              |
|  [05]   | `RecordException`                | `bool` property, default false    | records exceptions as `ActivityEvent`             |
|  [06]   | `EnableGrpcAspNetCoreSupport`    | `bool` property                   | gRPC server-span semconv on inbound calls         |
|  [07]   | `EnableAspNetCoreSignalRSupport` | `bool` property                   | SignalR hub span shaping                          |
|  [08]   | `EnableRazorComponentsSupport`   | `bool` property                   | Razor component span shaping                      |
|  [09]   | `DisableUrlQueryRedaction`       | `bool` property, default false    | opts out of URL-query redaction                   |

## [04]-[IMPLEMENTATION_LAW]

[INSTRUMENTATION_TOPOLOGY]:
- trace dispatch: subscribes the runtime-emitted hosting activity source; options resolve through named `IOptionsMonitor<AspNetCoreTraceInstrumentationOptions>`
- meter dispatch: meter admission subscribes the runtime-emitted hosting meters; the package defines no package-local hosting instruments on admitted target frameworks
- redaction: URL query values redact by default; `DisableUrlQueryRedaction` is the explicit opt-out policy value

[STACKING]:
- Registers at the service-root `SignalGovernance` composition beside `AddHttpClientInstrumentation` and `AddGrpcClientInstrumentation`, so inbound server spans, outbound client spans, and gRPC client spans compose into one trace under the one sampler verdict.
- Inbound gRPC calls served by the companion `ControlService` ride `EnableGrpcAspNetCoreSupport` on this package — server-side gRPC semconv is this package's concern, and the client side rides `api-otel-grpcnetclient.md`.
- Route-parameter redaction posture composes with the estate `RequestMetadata`/`HttpRouteParameterRedactionMode` prevention row, so an inbound span never carries an unredacted route segment.

[LOCAL_ADMISSION]:
- Registration is composition-root-only at service app roots; plugin and desktop profiles carry no ASP.NET host and never reference the package.
- Filter and enrichment delegates are policy values on options, total and side-effect free; a throwing filter drops the request silently.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AspNetCore`
- Owns: inbound ASP.NET Core trace and hosting-meter admission
- Accept: builder-registered instrumentation with options-bound policy at service roots
- Reject: hand-rolled `DiagnosticListener` subscriptions; per-handler span creation beside the subscribed source
