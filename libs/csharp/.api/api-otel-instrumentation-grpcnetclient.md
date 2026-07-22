# [RASM_API_OTEL_INSTRUMENTATION_GRPCNETCLIENT]

`OpenTelemetry.Instrumentation.GrpcNetClient` adds gRPC client-span semantics over `Grpc.Net.Client` channels — RPC method, service, and canonical status on every outbound call. Server-side gRPC is the ASP.NET Core package's; this one covers the client leg alone, with a suppression knob that collapses the underlying HTTP span when both instrumentations are live.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.GrpcNetClient`
- package: `OpenTelemetry.Instrumentation.GrpcNetClient`
- assembly: `OpenTelemetry.Instrumentation.GrpcNetClient`
- namespace: `OpenTelemetry.Instrumentation.GrpcNetClient`, `OpenTelemetry.Trace`
- asset: runtime library
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[OPTION_TYPES]: trace shaping
- rail: transport instrumentation

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `GrpcClientTraceInstrumentationOptions` | trace options  | HTTP-message enrichment + downstream suppression |
|  [02]   | `GrpcClientInstrumentation`             | subscription   | disposable listener behind the registration      |

Options members: `SuppressDownstreamInstrumentation`, `EnrichWithHttpRequestMessage`, `EnrichWithHttpResponseMessage`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission
- rail: transport instrumentation

| [INDEX] | [SURFACE]                      | [KIND]          | [CAPABILITY]                                                         |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `AddGrpcClientInstrumentation` | trace admission | `TracerProviderBuilder`; bare, named, and options-delegate overloads |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- root: `AddGrpcClientInstrumentation(static grpc => grpc.SuppressDownstreamInstrumentation = true)` beside `AddHttpClientInstrumentation` — the RPC span owns the call, the HTTP-transport span suppresses instead of doubling

[STACKING]:
- `Grpc.Net.Client`(`api-grpc-client.md`): retry and hedging attempts surface as sibling client spans under one parent, so channel policy is trace-visible with zero interceptor code.
- `OpenTelemetry`(`api-opentelemetry.md`): W3C context injects into gRPC metadata through the root's composite propagator; this package subscribes the channel's emission by name.
- `OpenTelemetry.Instrumentation.AspNetCore`(`api-otel-instrumentation-aspnetcore.md`): the inbound counterpart on service roots — direction partitions the pair.

[LOCAL_ADMISSION]:
- Composition-root-only, wherever `Grpc.Net.Client` channels leave the process — the AppHost wire root and measured-execution callers.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.GrpcNetClient`
- Owns: outbound gRPC client spans at the composition root
- Accept: one registration with downstream suppression beside HTTP client instrumentation
- Reject: hand-written client tracing interceptors; double-spanned RPC+HTTP legs
