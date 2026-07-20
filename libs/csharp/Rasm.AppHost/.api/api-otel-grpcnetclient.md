# [RASM_APPHOST_API_OTEL_GRPCNETCLIENT]

`OpenTelemetry.Instrumentation.GrpcNetClient` admits `Grpc.Net.Client` call telemetry onto the tracer builder: one extension subscribing the gRPC client activity source with enrichment policy and downstream-suppression control, so a gRPC hop emits one client span with rpc semconv instead of a bare HTTP/2 span pair.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.GrpcNetClient`
- package: `OpenTelemetry.Instrumentation.GrpcNetClient`
- assembly: `OpenTelemetry.Instrumentation.GrpcNetClient`
- namespace: `OpenTelemetry.Trace`
- namespace: `OpenTelemetry.Instrumentation.GrpcNetClient`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gRPC client instrumentation family
- rail: observability

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                                 |
| :-----: | :-------------------------------------- | :---------------- | :------------------------------------------- |
|  [01]   | `TracerProviderBuilderExtensions`       | builder extension | gRPC client trace admission                  |
|  [02]   | `GrpcClientTraceInstrumentationOptions` | options value     | enrichment and downstream-suppression policy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration
- rail: observability

| [INDEX] | [SURFACE]                      | [BUILDER_SIGNAL]    | [OPTIONS]                               | [CAPABILITY]                      |
| :-----: | :----------------------------- | :------------------ | :-------------------------------------- | :-------------------------------- |
|  [01]   | `AddGrpcClientInstrumentation` | tracing gRPC client | —                                       | sub: gRPC client activity source  |
|  [02]   | `AddGrpcClientInstrumentation` | tracing gRPC client | `GrpcClientTraceInstrumentationOptions` | enrichment and suppression policy |
|  [03]   | `AddGrpcClientInstrumentation` | tracing gRPC client | name + trace options                    | named-options trace policy        |

[ENTRYPOINT_SCOPE]: trace options
- rail: observability

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                             | [CAPABILITY]                                       |
| :-----: | :---------------------------------- | :--------------------------------------- | :------------------------------------------------- |
|  [01]   | `SuppressDownstreamInstrumentation` | `bool` property                          | suppresses the underlying HTTP-layer span per call |
|  [02]   | `EnrichWithHttpRequestMessage`      | `Action<Activity, HttpRequestMessage>?`  | request-side activity enrichment                   |
|  [03]   | `EnrichWithHttpResponseMessage`     | `Action<Activity, HttpResponseMessage>?` | response-side activity enrichment                  |

## [04]-[IMPLEMENTATION_LAW]

[INSTRUMENTATION_TOPOLOGY]:
- trace dispatch: subscribes the `Grpc.Net.Client` activity source; options resolve through named `IOptionsMonitor<GrpcClientTraceInstrumentationOptions>`
- status mapping: gRPC status codes resolve to span status through the package's canonical-code mapping, client-side and server-side rules distinct

[STACKING]:
- Registers at the `SignalGovernance` tracing chain with `SuppressDownstreamInstrumentation = true`, so the co-registered `AddHttpClientInstrumentation` row never double-traces the HTTP/2 leg beneath a gRPC call — one hop, one client span.
- `CallSpine` stamps `rasm-correlation` and `traceparent` on the same calls this package spans, so correlation metadata and the client span carry one context.
- Server-side gRPC spans ride `api-otel-aspnetcore.md` `EnableGrpcAspNetCoreSupport`; this package owns the client side only.

[LOCAL_ADMISSION]:
- Registration is composition-root-only; libraries dial channels through `Grpc.Net.Client` with zero instrumentation reference.
- Enrichment delegates are policy values on options, total and side-effect free.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.GrpcNetClient`
- Owns: gRPC client-span admission over `Grpc.Net.Client`
- Accept: builder-registered instrumentation with suppression-on policy beside the HTTP instrumentation row
- Reject: a hand-written client interceptor minting spans beside the subscribed source; stacking the HTTP and gRPC spans on one hop
