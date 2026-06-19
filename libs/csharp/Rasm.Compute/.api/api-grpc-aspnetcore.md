# [RASM_COMPUTE_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` is a meta-package with no DLL assets. Its `lib/` folders contain `_._`
placeholders; all code surfaces ship in the sub-packages it bundles: `Grpc.AspNetCore.Server`,
`Grpc.AspNetCore.Web`, `Grpc.AspNetCore.HealthChecks`, and `Grpc.AspNetCore.Server.ClientFactory`.
The decompile-verifiable in-process test surface is owned by `Grpc.Net.Client` (admitted directly)
and `Microsoft.AspNetCore.TestHost` (admitted directly).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.AspNetCore`
- package: `Grpc.AspNetCore` (meta-package; no DLL assets)
- bundles: `Grpc.AspNetCore.Server`, `Grpc.AspNetCore.Web`, `Grpc.AspNetCore.HealthChecks`, `Grpc.AspNetCore.Server.ClientFactory`
- in-process surface verified via: `Grpc.Net.Client` + `Microsoft.AspNetCore.TestHost`
- namespace: `Grpc.AspNetCore.Server` (server routing), `Grpc.Net.Client` (channel), `Microsoft.AspNetCore.TestHost` (test handler)
- asset: meta-package
- rail: grpc

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: in-process channel construction — source: `Grpc.Net.Client`
- rail: grpc

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :------------------- | :-------------- | :------------------------------- |
|  [01]   | `GrpcChannel`        | channel owner   | client channel and stub factory  |
|  [02]   | `GrpcChannelOptions` | channel options | HTTP handler, credentials, codec |

[PUBLIC_TYPE_SCOPE]: test server handler — source: `Microsoft.AspNetCore.TestHost`
- rail: grpc

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                                             |
| :-----: | :-------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `TestServer`    | in-process server | owns `CreateHandler()` and `CreateClient()`        |
|  [02]   | `ClientHandler` | message handler   | `HttpMessageHandler` returned by `CreateHandler()` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: in-process channel construction
- rail: grpc

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :---------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `GrpcChannel.ForAddress(address, GrpcChannelOptions)` | factory call    | channel bound to address and handler          |
|  [02]   | `GrpcChannelOptions.HttpHandler`                      | option property | accepts `HttpMessageHandler?` from TestServer |
|  [03]   | `TestServer.CreateHandler()`                          | handler factory | returns `HttpMessageHandler` for in-process   |
|  [04]   | `GrpcChannel.CreateCallInvoker()`                     | invoker factory | `CallInvoker` for generated gRPC stubs        |
|  [05]   | `GrpcChannel.Dispose()`                               | disposal        | channel and connection teardown               |

[ENTRYPOINT_SCOPE]: test server lifecycle
- rail: grpc

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]   | [RAIL]                              |
| :-----: | :------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `WebApplicationFactory<TEntryPoint>`   | factory base     | test server backed by full startup  |
|  [02]   | `WebApplicationFactory.CreateClient()` | client factory   | `HttpClient` via in-process handler |
|  [03]   | `WebApplicationFactory.Services`       | DI access        | service provider from hosted app    |
|  [04]   | `TestServer.BaseAddress`               | address property | in-process base URI                 |

## [04]-[IMPLEMENTATION_LAW]

[IN_PROCESS_PATTERN]:
- canonical in-process gRPC test wiring:
  1. Create `WebApplicationFactory<T>` with gRPC services registered via `builder.Services.AddGrpc()` and endpoints mapped via `app.MapGrpcService<TService>()`
  2. Obtain `HttpMessageHandler handler = factory.Server.CreateHandler()` from `Microsoft.AspNetCore.TestHost`
  3. Build channel: `GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler })`
  4. Create stub: `new TClient(channel.CreateCallInvoker())`
- `GrpcChannelOptions.HttpHandler` accepts the test handler directly; no real TCP socket is opened
- `WebApplicationFactory` is in `Microsoft.AspNetCore.Mvc.Testing`, admitted separately; `TestServer` is in `Microsoft.AspNetCore.TestHost`

[SERVER_SURFACE_CONSTRAINT]:
- `Grpc.AspNetCore` has zero DLL assets; server registration (`AddGrpc`, `MapGrpcService<T>`) and interceptor wiring live in `Grpc.AspNetCore.Server.dll`, which is only indirectly pulled in
- all in-process surface types documented in §2 and §3 are decompile-verified against `Grpc.Net.Client` and `Microsoft.AspNetCore.TestHost`

[LOCAL_ADMISSION]:
- Compute test projects wire gRPC channels through `WebApplicationFactory` and `TestServer.CreateHandler()`; no real network is required.
- `GrpcChannelOptions.HttpHandler` is the sole injection point for the in-process handler; do not subclass `GrpcChannel`.
- Stub generation (`Grpc.Tools`) produces `TClient(CallInvoker)` constructors; stubs take `ChannelBase` or `CallInvoker`.

[RAIL_LAW]:
- Package: `Grpc.AspNetCore`
- Owns: gRPC server hosting meta-package (server registration via sub-packages)
- In-process test surface: `TestServer.CreateHandler()` → `GrpcChannelOptions.HttpHandler` → `GrpcChannel.ForAddress`
- Accept: in-process integration tests using `WebApplicationFactory` and `TestServer`
- Reject: real-network gRPC calls from unit tests; direct `GrpcChannel` without handler injection
