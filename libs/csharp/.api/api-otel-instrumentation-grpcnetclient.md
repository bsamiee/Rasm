# [RASM_API_OTEL_INSTRUMENTATION_GRPCNETCLIENT]

`OpenTelemetry.Instrumentation.GrpcNetClient` decorates the `Grpc.Net.Client.GrpcOut` activity each outbound call raises, folding the channel's transitional tags into RPC method, peer address, and canonical gRPC status. Registration mints no span: the client leg keeps the trace identity the channel opened, and enrichment reaches the transport message pair.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.GrpcNetClient`
- package: `OpenTelemetry.Instrumentation.GrpcNetClient` (`Apache-2.0`, OpenTelemetry Authors)
- assembly: `OpenTelemetry.Instrumentation.GrpcNetClient`
- namespace: `OpenTelemetry.Instrumentation.GrpcNetClient`, `OpenTelemetry.Trace`
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace shaping, resolved per registration name through `IOptionsMonitor`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `GrpcClientTraceInstrumentationOptions` | class         | downstream suppression and message enrichment |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tracer-provider admission

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :----------------------------------- |
|  [01]   | `AddGrpcClientInstrumentation(string, Action<GrpcClientTraceInstrumentationOptions>)` | static  | bare, named, and configure overloads |

[ENTRYPOINT_SCOPE]: `GrpcClientTraceInstrumentationOptions` settable shaping

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `SuppressDownstreamInstrumentation` | property | collapse the transport span, take over W3C injection |
|  [02]   | `EnrichWithHttpRequestMessage`      | property | `Action<Activity, HttpRequestMessage>` at call start |
|  [03]   | `EnrichWithHttpResponseMessage`     | property | `Action<Activity, HttpResponseMessage>` at call stop |

- `SuppressDownstreamInstrumentation`: suppression and injection precede the sampler verdict, so an unsampled call still collapses its transport span and propagates.
- `EnrichWithHttpRequestMessage`: both delegates run only while `Activity.IsAllDataRequested` holds, and a throw inside either lands on the instrumentation event source, never the call.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Stamping runs on `Activity.Current` rather than a fresh span: the listener re-seats source and `Client` kind on the `GrpcOut` bracket, folds `grpc.method` and `grpc.status_code` into `rpc.method`, `rpc.response.status_code`, and `error.type`, and leaves a caller-set `Activity.Status` standing.
- One RPC span owns the call while the transport span suppresses: `SuppressDownstreamInstrumentation` enters the suppression scope at call start and takes over W3C injection, so every attempt the channel's retry or hedging policy spends folds into that one span.

[STACKING]:
- `Grpc.Net.Client`(`api-grpc-client.md`): spans ride the channel's own `GrpcOut` bracket, so `CallInvokerExtensions.Intercept` stays free for domain policy and `RpcException.StatusCode` names the fault `rpc.response.status_code` already carries.
- `OpenTelemetry.Instrumentation.Http`(`api-otel-instrumentation-http.md`): both packages claim one outbound call — suppression voids the transport span and injects `traceparent` from this leg; left off, the transport span survives inside the RPC span under `HttpClientTraceInstrumentationOptions` shaping.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddGrpcClientInstrumentation` is a `WithTracing` builder row, and injection resolves `Propagators.DefaultTextMapPropagator`, so the root's `Sdk.SetDefaultTextMapPropagator` composite decides the wire headers with `Baggage.Current` riding along.
- `OpenTelemetry.Instrumentation.AspNetCore`(`api-otel-instrumentation-aspnetcore.md`): `traceparent` injected here arrives as the extracted parent of that package's server span, joining both legs of one call across the process boundary.
- Within-library: `EnrichWithHttpRequestMessage` stamps request-side domain tags off the transport headers and `EnrichWithHttpResponseMessage` closes the same `Activity` with response facts, so `SetTag`, `AddEvent`, and `AddLink` reach the RPC span with no interceptor or `DelegatingHandler` in the call path.

[LOCAL_ADMISSION]:
- Composition-root-only, wherever `Grpc.Net.Client` channels leave the process; a library emits through the channel bracket and never references this package.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.GrpcNetClient`
- Owns: outbound gRPC client-span semantics on the channel's own activity bracket
- Accept: one registration per tracer provider, shaped by the options delegate
- Reject: a hand-written client tracing interceptor; a double-spanned RPC-over-transport leg
