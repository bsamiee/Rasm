# [RASM_API_OTEL_INSTRUMENTATION_ASPNETCORE]

`OpenTelemetry.Instrumentation.AspNetCore` owns the inbound server leg where AppHost terminates HTTP and gRPC: request spans with filter/enrich/exception shaping on the trace verb, and the server request-duration meter on the metric verb. Server-side gRPC rides this package — the gRPC client package covers only outbound channels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.AspNetCore`
- package: `OpenTelemetry.Instrumentation.AspNetCore`
- assembly: `OpenTelemetry.Instrumentation.AspNetCore`
- namespace: `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- asset: runtime library
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[OPTION_TYPES]: trace shaping
- rail: transport instrumentation

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]                                 |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `AspNetCoreTraceInstrumentationOptions` | trace options  | inbound filter, enrichment, protocol toggles |

Options members: `Filter`, `EnrichWithHttpRequest`, `EnrichWithHttpResponse`, `EnrichWithException`, `RecordException`, `DisableUrlQueryRedaction`, and the protocol toggles `EnableGrpcAspNetCoreSupport`, `EnableAspNetCoreSignalRSupport`, `EnableRazorComponentsSupport`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission
- rail: transport instrumentation

| [INDEX] | [SURFACE]                      | [KIND]           | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `AddAspNetCoreInstrumentation` | trace admission  | `TracerProviderBuilder`, optional `Action<AspNetCoreTraceInstrumentationOptions>` |
|  [02]   | `AddAspNetCoreInstrumentation` | metric admission | `MeterProviderBuilder`; subscribes the built-in server meters                     |

## [04]-[IMPLEMENTATION_LAW]

[SERVER_TOPOLOGY]:
- trace root: `AddAspNetCoreInstrumentation` with `EnableGrpcAspNetCoreSupport = true` inside the AppHost `WithTracing` delegate — one registration covers HTTP and gRPC server spans
- metric root: the built-in `Microsoft.AspNetCore.Hosting` request-duration meter, admitted by name or through the metric verb

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): inbound extraction joins the W3C composite propagator the root registered; the request span is the parent every downstream client span nests under.
- `Grpc.AspNetCore`(`api-grpc-aspnetcore.md`): measured-execution service endpoints gain server spans through this package, never a service-side interceptor shim.
- `OpenTelemetry.Instrumentation.GrpcNetClient`(`api-otel-instrumentation-grpcnetclient.md`): the outbound counterpart — the two packages partition gRPC by direction.

[LOCAL_ADMISSION]:
- Admitted only in AppHost service roots that terminate HTTP/gRPC; plugin processes host no server surface and never load it.
- `Filter` rejects health-probe and scrape-shaped paths at span-creation time, ahead of the sampler.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AspNetCore`
- Owns: inbound request spans and server request metrics at the service root
- Accept: one trace + one metric registration per hosting root
- Reject: hand-written server-span middleware; loading in any process without a server surface
