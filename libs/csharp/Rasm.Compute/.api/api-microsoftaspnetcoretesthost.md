# [RASM_COMPUTE_API_MICROSOFTASPNETCORETESTHOST]

`Microsoft.AspNetCore.TestHost` owns the in-memory ASP.NET Core server whose `TestServer.CreateHandler` mints an `HttpMessageHandler` over the request pipeline with no socket. That handler is the source the `RemoteTransport.InProcess` row hands to `GrpcChannelOptions.HttpHandler`, so `GrpcChannel.ForAddress` dials the suite gRPC services against no live remote — the test seam proving cross-process hand-off in-process. Production transport, socket bind, and every non-test reference stay outside package source.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.TestHost`
- package: `Microsoft.AspNetCore.TestHost` (MIT, .NET Foundation)
- assembly: `Microsoft.AspNetCore.TestHost`
- namespace: `Microsoft.AspNetCore.TestHost`
- rail: in-process-transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: server and handler contracts

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `TestServer`             | class         | owns the pipeline and its handler source            |
|  [02]   | `TestServerOptions`      | class         | seeds base address, sync-IO, execution-context flow |
|  [03]   | `ClientHandler`          | class         | `HttpMessageHandler` dispatching over the pipeline  |
|  [04]   | `RequestBuilder`         | class         | builds one in-memory request                        |
|  [05]   | `WebSocketClient`        | class         | opens an in-memory web socket                       |
|  [06]   | `HttpResetTestException` | class         | carries an HTTP/2 reset `ErrorCode`                 |

[PUBLIC_TYPE_SCOPE]: composition and entry-point contracts

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `WebHostBuilderExtensions`        | static class  | registers the server, overrides its graph     |
|  [02]   | `HostBuilderTestServerExtensions` | static class  | reads the server off an `IHost`               |
|  [03]   | `WebHostBuilderFactory`           | static class  | builds a builder from an assembly entry point |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server lifecycle and handler operations

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `TestServer(IServiceProvider)`                                        | ctor     | build over a composed provider           |
|  [02]   | `TestServer(IServiceProvider, IFeatureCollection)`                    | ctor     | build over a provider with seed features |
|  [03]   | `TestServer(IServiceProvider, IOptions<TestServerOptions>)`           | ctor     | build under explicit server policy       |
|  [04]   | `TestServer.CreateHandler() -> HttpMessageHandler`                    | factory  | handler over the pipeline, no socket     |
|  [05]   | `TestServer.CreateHandler(Action<HttpContext>) -> HttpMessageHandler` | factory  | context-tuned handler                    |
|  [06]   | `TestServer.CreateClient() -> HttpClient`                             | factory  | client over the handler                  |
|  [07]   | `TestServer.CreateWebSocketClient() -> WebSocketClient`               | factory  | in-memory socket client                  |
|  [08]   | `TestServer.CreateRequest(string) -> RequestBuilder`                  | factory  | shape one in-memory request              |
|  [09]   | `TestServer.SendAsync(Action<HttpContext>) -> Task<HttpContext>`      | instance | run one raw context through the pipeline |
|  [10]   | `TestServer.Dispose()`                                                | instance | tear the in-memory server down           |

[ENTRYPOINT_SCOPE]: server and options policy properties

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `TestServer.BaseAddress`              | property | request base `Uri`, defaults `http://localhost/` |
|  [02]   | `TestServer.Services`                 | property | the running `IServiceProvider`                   |
|  [03]   | `TestServer.Features`                 | property | the running `IFeatureCollection`                 |
|  [04]   | `TestServer.AllowSynchronousIO`       | property | permit synchronous body access                   |
|  [05]   | `TestServer.PreserveExecutionContext` | property | flow the ambient execution context               |
|  [06]   | `TestServerOptions.BaseAddress`       | property | seed the server base `Uri`                       |

[ENTRYPOINT_SCOPE]: builder and host registration operations

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `IWebHostBuilder.UseTestServer()`                                        | static  | register the test server                  |
|  [02]   | `IWebHostBuilder.UseTestServer(Action<TestServerOptions>)`               | static  | register under policy                     |
|  [03]   | `IWebHostBuilder.ConfigureTestServices(Action<IServiceCollection>)`      | static  | override registered services              |
|  [04]   | `IWebHostBuilder.ConfigureTestContainer<TContainer>(Action<TContainer>)` | static  | override the DI container                 |
|  [05]   | `IWebHostBuilder.UseSolutionRelativeContentRoot(string)`                 | static  | resolve content root against the solution |
|  [06]   | `IHost.GetTestServer() -> TestServer`                                    | static  | read the server off a generic host        |
|  [07]   | `IHost.GetTestClient() -> HttpClient`                                    | static  | read a ready client off the host          |

[ENTRYPOINT_SCOPE]: request, socket, and entry-point operations

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `RequestBuilder.AddHeader(string, string)`                                | instance | append a request header                  |
|  [02]   | `RequestBuilder.And(Action<HttpRequestMessage>)`                          | instance | tune the `HttpRequestMessage`            |
|  [03]   | `RequestBuilder.SendAsync(string) -> Task<HttpResponseMessage>`           | instance | send under a named HTTP method           |
|  [04]   | `RequestBuilder.GetAsync() -> Task<HttpResponseMessage>`                  | instance | send a GET                               |
|  [05]   | `RequestBuilder.PostAsync() -> Task<HttpResponseMessage>`                 | instance | send a POST                              |
|  [06]   | `WebSocketClient.ConnectAsync(Uri, CancellationToken) -> Task<WebSocket>` | instance | open an in-memory socket                 |
|  [07]   | `WebSocketClient.SubProtocols`                                            | property | negotiated sub-protocols                 |
|  [08]   | `WebSocketClient.ConfigureRequest`                                        | property | tune the socket handshake request        |
|  [09]   | `WebHostBuilderFactory.CreateFromAssemblyEntryPoint(Assembly, string[])`  | static   | build from an assembly entry point       |
|  [10]   | `WebHostBuilderFactory.CreateFromTypesAssemblyEntryPoint<T>(string[])`    | static   | build from a type's assembly entry point |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `TestServer.CreateHandler()` returns a `ClientHandler` dispatching `HttpRequestMessage` through the in-memory pipeline with no socket bind; it is the sole handler source every remote dial threads.
- Construction takes an `IServiceProvider`; `Services` and `Features` expose the running pipeline's provider and feature collection, and `Dispose` tears the host down so the handler and minted clients stop dispatching.
- `BaseAddress` defaults to `http://localhost/` and the gRPC endpoint composes against it; `AllowSynchronousIO` stays false to hold the production async posture.
- `SendAsync(Action<HttpContext>)` drives a raw context through the pipeline directly when a context is the unit, bypassing the handler.
- `UseTestServer` registers the server; `ConfigureTestServices` and `ConfigureTestContainer<TContainer>` override the scenario graph; `IHost.GetTestServer`/`GetTestClient` read the server and a ready client off a built host; `WebHostBuilderFactory.CreateFromAssemblyEntryPoint` builds from a target assembly when the host is not composed inline.

[STACKING]:
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): `CreateHandler()`'s `HttpMessageHandler` enters `GrpcChannelOptions.HttpHandler`, so `GrpcChannel.ForAddress` dials in-process; a `ClientHandler` is not a `SocketsHttpHandler`, forfeiting `GrpcChannel.State`/`ConnectAsync` connectivity the hand-off proof never reads.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Timestamp` and `Duration` contract fields round-trip through `ToInstant`/`ToNodaDuration` over the in-memory pipeline, exercising the temporal seam without a live remote.
- Within-library: one `TestServer` per suite composed over the service graph, its `CreateHandler` handed once to the `RemoteTransport.InProcess` row's channel dial.

[LOCAL_ADMISSION]:
- `CreateHandler()`'s handler enters Compute through `GrpcChannelOptions.HttpHandler` on the `RemoteTransport.InProcess` row alone; no Compute production code references the test server.
- `TestServer` composition lives in the spec project's test root; the in-process row's `endpoint.Handler` slot carries the handler factory from the test boundary.
- Web socket, request-builder, and entry-point surfaces stay test-composition tools, never Compute extension points.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.TestHost`
- Owns: the in-memory ASP.NET Core pipeline and its `HttpMessageHandler` source
- Accept: the `RemoteTransport.InProcess` hand-off proof against no live remote
- Reject: production transport, socket bind, and any non-test reference from package source
