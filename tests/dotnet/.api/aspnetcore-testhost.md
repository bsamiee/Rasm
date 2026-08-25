# [DOTNET_TESTING_API_ASPNETCORE_TESTHOST]

`Microsoft.AspNetCore.TestHost` ships `TestServer`, the in-memory server that runs a real ASP.NET Core pipeline — routing, middleware, gRPC endpoints — without a socket: `CreateClient()` returns an `HttpClient` whose handler dispatches straight into the application, so a transport-boundary spec proves the wire contract in-process and stays inside the unit lane's no-socket law; the Compute suite composes it for the gRPC/http compute endpoints, and a real-socket proof belongs to the integration lane, never here.

## [01]-[PACKAGE_SURFACE]

- package: `Microsoft.AspNetCore.TestHost`
- license: `MIT`
- namespace: `Microsoft.AspNetCore.TestHost`
- asset: `lib/net10.0/Microsoft.AspNetCore.TestHost.dll`
- rail: harness — suite-owned row on the consuming test csproj (`Rasm.Compute.Tests`); the shared test stack never injects it

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                          | [KIND]    | [CAPABILITY]                                                       |
| :-----: | :-------------------------------- | :-------- | :----------------------------------------------------------------- |
|  [01]   | `TestServer`                      | host      | in-memory `IServer`; `CreateClient`/`CreateHandler`/`SendAsync`    |
|  [02]   | `TestServerOptions`               | policy    | `BaseAddress`, `AllowSynchronousIO`, `PreserveExecutionContext`    |
|  [03]   | `WebSocketClient`                 | client    | in-memory websocket connect against the pipeline                   |
|  [04]   | `RequestBuilder`                  | builder   | header/body composition for a one-shot in-memory request           |
|  [05]   | `ClientHandler`                   | handler   | the `HttpMessageHandler` bridging `HttpClient` into the host       |
|  [06]   | `HostBuilderTestServerExtensions` | extension | `UseTestServer()` / `GetTestServer()` / `GetTestClient()` on IHost |
|  [07]   | `WebHostBuilderExtensions`        | extension | `ConfigureTestServices` service substitution at host build         |
|  [08]   | `WebHostBuilderFactory`           | factory   | host builder off a target assembly entry point                     |
|  [09]   | `HttpResetTestException`          | fault     | typed HTTP/2 RST_STREAM surfaced to the client                     |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                   | [KIND]  | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------------------- | :------ | :--------------------------------------------------------------- |
|  [01]   | `builder.WebHost.UseTestServer()`                           | wiring  | swap Kestrel for the in-memory server on a real host build       |
|  [02]   | `builder.WebHost.UseTestServer(Action<TestServerOptions>)`  | wiring  | swap it in under explicit base-address and sync-IO policy        |
|  [03]   | `host.GetTestClient()`                                      | client  | `HttpClient` into the pipeline; feeds `GrpcChannel.ForAddress`   |
|  [04]   | `server.CreateHandler()`                                    | handler | raw handler for typed clients and gRPC channel composition       |
|  [05]   | `server.CreateHandler(Action<HttpContext>)`                 | handler | same handler, tuning every context before the pipeline runs      |
|  [06]   | `builder.ConfigureTestServices(Action<IServiceCollection>)` | seams   | per-spec service substitution without touching production wiring |
|  [07]   | `builder.UseSolutionRelativeContentRoot(string)`            | wiring  | resolve the content root against the solution, not the bin dir   |
|  [08]   | `server.SendAsync(Action<HttpContext>)`                     | probe   | direct `HttpContext` exercise for middleware-only laws           |
|  [09]   | `server.CreateRequest(string)`                              | builder | shape one in-memory request through `RequestBuilder`             |
|  [10]   | `server.CreateWebSocketClient()`                            | client  | websocket lane against the same pipeline                         |

- `TestServer`: the ctor takes an `IServiceProvider` alone, with an `IFeatureCollection` of seed features, or with an `IOptions<TestServerOptions>` policy; `Services` and `Features` expose the running provider and feature collection, `BaseAddress` defaults to `http://localhost/`, `AllowSynchronousIO` stays false to hold the production async posture, and `Dispose()` stops the handler and every client minted off it.
- `ConfigureTestContainer<TContainer>(Action<TContainer>)`: substitutes at the DI container behind the service collection, where `ConfigureTestServices` cannot reach.
- `RequestBuilder`: `AddHeader(string, string)` and `And(Action<HttpRequestMessage>)` compose one request; `GetAsync()`, `PostAsync()`, and `SendAsync(string)` send it under a named method.
- `WebSocketClient`: `ConnectAsync(Uri, CancellationToken)` opens the socket, with `SubProtocols` and `ConfigureRequest` tuning the handshake.
- `WebHostBuilderFactory`: `CreateFromAssemblyEntryPoint(Assembly, string[])` and `CreateFromTypesAssemblyEntryPoint<T>(string[])` build a host builder off a target assembly when the host is not composed inline.

```csharp
public class TestServer : IServer, IDisposable {
    public TestServer(IServiceProvider services);
    public HttpClient CreateClient();
    public HttpMessageHandler CreateHandler();
    public RequestBuilder CreateRequest(string path);
    public WebSocketClient CreateWebSocketClient();
    public Task<HttpContext> SendAsync(Action<HttpContext> configureContext, CancellationToken cancellationToken = default);
}
public static class HostBuilderTestServerExtensions {
    public static IWebHostBuilder UseTestServer(this IWebHostBuilder builder);
    public static TestServer GetTestServer(this IHost host);
    public static HttpClient GetTestClient(this IHost host);
}
```

## [04]-[IMPLEMENTATION_LAW]

[BOUNDARY]: the in-memory pipeline proves routing, middleware order, serialization, and gRPC contract shape; it never proves socket behavior, TLS, or backpressure — those are integration-lane facts behind the `network` boundary.

[STACKING]:
- `Grpc.Net.Client`: `GrpcChannel.ForAddress(server.BaseAddress, new GrpcChannelOptions { HttpHandler = server.CreateHandler() })` runs the full gRPC contract in-process; a `ClientHandler` is not a `SocketsHttpHandler`, so the channel forfeits `GrpcChannel.State`, `ConnectAsync`, and `WaitForStateChangedAsync` and a hand-off proof never reads them.
- `Rasm.Compute` `RemoteTransport.InProcess`: the handler factory rides that row's endpoint slot from the spec composition root, so no Compute production code references the test server.
- `Rasm.TestKit` (`Spec.cs`): responses and typed gRPC results fold through the kit rail gates; wire payloads round-trip through `Spec.RoundtripBytes`.
- `timeprovider-testing.md`: hosted pipeline takes the spec's `TimeProvider` through DI like any SUT.

[LOCAL_ADMISSION]:
- Suites proving hosted transport boundaries own the reference (`Rasm.Compute.Tests` today); adding it to a suite with no hosted surface is roster noise.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.TestHost`
- Owns: in-memory hosted-pipeline proof for HTTP and gRPC surfaces inside the unit lane.
- Accept: `UseTestServer` on the production composition root, `ConfigureTestServices` for seam substitution, handler-fed `GrpcChannel` composition.
- Reject: real sockets or ports in the unit lane; duplicating the production DI graph inside the spec instead of substituting seams; asserting transport facts the in-memory host cannot carry.
