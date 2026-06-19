# [RASM_COMPUTE_API_MICROSOFTASPNETCORETESTHOST]

`Microsoft.AspNetCore.TestHost` supplies an in-memory ASP.NET Core server whose
`CreateHandler` mints an `HttpMessageHandler` over the request pipeline with no
socket, the handler source the `RemoteTransport.InProcess` row injects to dial a
`GrpcChannel` against the suite gRPC services without a live remote — the
test-only seam that proves cross-process hand-off in-process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.TestHost`
- package: `Microsoft.AspNetCore.TestHost`
- assembly: `Microsoft.AspNetCore.TestHost`
- namespace: `Microsoft.AspNetCore.TestHost`
- asset: runtime library (test-only)
- rail: in-process-transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: server and handler contracts
- rail: in-process-transport

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                           |
| :-----: | :----------------------- | :--------------- | :------------------------------------- |
|  [01]   | `TestServer`             | in-memory server | owns the pipeline and handler source   |
|  [02]   | `TestServerOptions`      | server policy    | configures the in-memory server        |
|  [03]   | `ClientHandler`          | message handler  | `HttpMessageHandler` over the pipeline |
|  [04]   | `RequestBuilder`         | request shaper   | builds a single in-memory request      |
|  [05]   | `WebSocketClient`        | socket client    | opens an in-memory web socket          |
|  [06]   | `HttpResetTestException` | reset signal     | carries an HTTP/2 reset error code     |

[PUBLIC_TYPE_SCOPE]: composition and entry-point contracts
- rail: in-process-transport

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `WebHostBuilderExtensions`        | builder seam    | registers the test server            |
|  [02]   | `HostBuilderTestServerExtensions` | host seam       | reads the test server off an `IHost` |
|  [03]   | `WebHostBuilderFactory`           | builder factory | builds from an assembly entry point  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server lifecycle and handler operations
- rail: in-process-transport

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `TestServer(IServiceProvider)`                  | constructor   | builds over a provider                |
|  [02]   | `TestServer(IWebHostBuilder)`                   | constructor   | builds over a configured host builder |
|  [03]   | `TestServer.CreateHandler()`                    | factory call  | returns `HttpMessageHandler`          |
|  [04]   | `TestServer.CreateHandler(Action<HttpContext>)` | factory call  | returns a context-tuned handler       |
|  [05]   | `TestServer.CreateClient()`                     | factory call  | returns `HttpClient` over the handler |
|  [06]   | `TestServer.CreateWebSocketClient()`            | factory call  | returns `WebSocketClient`             |
|  [07]   | `TestServer.SendAsync`                          | dispatch call | runs one context through the pipeline |
|  [08]   | `TestServer.Dispose`                            | lifetime call | tears the in-memory server down       |

[ENTRYPOINT_SCOPE]: server policy properties
- rail: in-process-transport

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]    | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `TestServer.BaseAddress`              | option property | sets the request base `Uri`           |
|  [02]   | `TestServer.Services`                 | accessor        | reads the server `IServiceProvider`   |
|  [03]   | `TestServer.Features`                 | accessor        | reads the server `IFeatureCollection` |
|  [04]   | `TestServer.AllowSynchronousIO`       | option property | permits synchronous body access       |
|  [05]   | `TestServer.PreserveExecutionContext` | option property | flows the ambient execution context   |
|  [06]   | `TestServerOptions.BaseAddress`       | option property | seeds the server base `Uri`           |

[ENTRYPOINT_SCOPE]: builder and host registration operations
- rail: in-process-transport

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :----------- | :---------------------------------- |
|  [01]   | `IWebHostBuilder.UseTestServer()`                          | builder call | registers the test server           |
|  [02]   | `IWebHostBuilder.UseTestServer(Action<TestServerOptions>)` | builder call | registers with policy               |
|  [03]   | `IWebHostBuilder.ConfigureTestServices`                    | builder call | overrides registered services       |
|  [04]   | `IWebHostBuilder.ConfigureTestContainer<TContainer>`       | builder call | overrides the container             |
|  [05]   | `IWebHost.GetTestServer()`                                 | accessor     | reads the server off a built host   |
|  [06]   | `IWebHost.GetTestClient()`                                 | accessor     | reads an `HttpClient` off the host  |
|  [07]   | `IHost.GetTestServer()`                                    | accessor     | reads the server off a generic host |
|  [08]   | `IHost.GetTestClient()`                                    | accessor     | reads an `HttpClient` off the host  |

[ENTRYPOINT_SCOPE]: request, socket, and entry-point operations
- rail: in-process-transport

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `RequestBuilder.AddHeader`                           | builder call  | appends a request header            |
|  [02]   | `RequestBuilder.And`                                 | builder call  | tunes the `HttpRequestMessage`      |
|  [03]   | `RequestBuilder.SendAsync(string)`                   | dispatch call | sends with a named HTTP method      |
|  [04]   | `WebSocketClient.ConnectAsync`                       | dispatch call | opens an in-memory socket           |
|  [05]   | `WebSocketClient.SubProtocols`                       | accessor      | reads negotiated sub-protocols      |
|  [06]   | `WebHostBuilderFactory.CreateFromAssemblyEntryPoint` | factory call  | builds from an assembly entry point |

## [04]-[IMPLEMENTATION_LAW]

[IN_PROCESS_HANDLER]:
- namespace: `Microsoft.AspNetCore.TestHost`
- handler source: `TestServer.CreateHandler()` returns a `ClientHandler` (`HttpMessageHandler`) that dispatches `HttpRequestMessage` through the in-memory pipeline with no socket bind
- channel seam: the handler enters `GrpcChannel.ForAddress(endpoint.Address, new GrpcChannelOptions { HttpHandler = handler() })` as the `RemoteTransport.InProcess` row's dial
- base address: `TestServer.BaseAddress` defaults to `http://localhost/`; the gRPC endpoint address composes against it
- synchronous IO: `AllowSynchronousIO` stays false so the handler matches the production async pipeline posture

[SERVER_LIFECYCLE]:
- construction: `TestServer(IServiceProvider)` over a composed provider, or `TestServer(IWebHostBuilder)` over a `UseTestServer`-configured builder
- services: `TestServer.Services` and `TestServer.Features` expose the live provider and feature collection for the running pipeline
- disposal: `TestServer.Dispose` tears the in-memory host down; the handler and any minted clients stop dispatching
- request shaping: `CreateRequest`/`RequestBuilder` and `SendAsync(Action<HttpContext>)` drive the pipeline directly without the handler when a raw context is the unit

[COMPOSITION_SEAM]:
- builder: `IWebHostBuilder.UseTestServer` registers the server; `ConfigureTestServices` and `ConfigureTestContainer<TContainer>` override the registered graph for the scenario
- host read: `GetTestServer` and `GetTestClient` extensions read the server and a ready `HttpClient` off a built `IWebHost` or `IHost`
- entry point: `WebHostBuilderFactory.CreateFromAssemblyEntryPoint` builds from a target assembly's entry point when the host is not composed inline

[LOCAL_ADMISSION]:
- The handler enters Compute through `GrpcChannelOptions.HttpHandler` on the `RemoteTransport.InProcess` row only; no Compute production code references the test server.
- `TestServer` composition lives in the spec project's test root, never in package source; the in-process row's `endpoint.Handler` slot carries the handler factory from the test boundary.
- Web socket, request-builder, and entry-point surfaces stay test-composition tools and are not Compute extension points.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.TestHost`
- Owns: in-memory ASP.NET Core pipeline and its `HttpMessageHandler` source
- Accept: the `RemoteTransport.InProcess` hand-off proof without a live remote
- Reject: production transport, socket bind, and any non-test reference from package source
