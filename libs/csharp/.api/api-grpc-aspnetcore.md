# [RASM_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` is a meta-package whose `lib/<tfm>` folders carry `_._` placeholders; `Grpc.AspNetCore.Server.ClientFactory` brings the `Grpc.AspNetCore.Server` surface that owns `AddGrpc`, `MapGrpcService<T>`, `GrpcServiceOptions`, and interceptor registration. The same gRPC ASP.NET rail admits `Grpc.AspNetCore.Web` for server-side gRPC-Web and `Grpc.AspNetCore.HealthChecks` for the gRPC `Health` service. In-process integration composes `Grpc.Net.Client`, `Grpc.Core.Api`, `Microsoft.AspNetCore.TestHost`, and `Grpc.Net.Client.Web` against that server rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.AspNetCore`

- package: `Grpc.AspNetCore` (meta-package; no DLL assets, `lib/<tfm>/_._`)
- license: Apache-2.0
- transitive bundle (net10.0 dependency group): `Grpc.AspNetCore.Server.ClientFactory`
  (pulls `Grpc.AspNetCore.Server` — the `lib/net10.0/Grpc.AspNetCore.Server.dll` that owns `AddGrpc`,
  `MapGrpcService<T>`, `GrpcServiceOptions`, `InterceptorCollection`), `Google.Protobuf`
  (`exclude=Build,Analyzers`), `Grpc.Tools` (`include=All`, build-only). `Grpc.AspNetCore.Web` and
  `Grpc.AspNetCore.HealthChecks` are NOT in this bundle — they are SEPARATE direct admissions, both
  catalogued here (§2/§3).
- direct admissions catalogued here: `Grpc.AspNetCore.Web`
  (`lib/net10.0/Grpc.AspNetCore.Web.dll` — server-side grpc-web middleware) and
  `Grpc.AspNetCore.HealthChecks` (`lib/net10.0/Grpc.AspNetCore.HealthChecks.dll` — gRPC health
  service). Both bind the consumer net10.0 TFM directly (not the meta-package placeholder).
- in-process surface owned by (direct admissions): `Grpc.Net.Client` (`GrpcChannel`),
  `Grpc.Core.Api` (`CallInvoker`/`ChannelBase`/`Metadata`/`Status`),
  `Microsoft.AspNetCore.TestHost` (`TestServer`), `Grpc.Net.Client.Web` (`GrpcWebHandler`)
- build-floor: net8.0 (meta-package dependency groups net8.0/net9.0/net10.0; consumer binds net10.0)
- note: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<T>`) is NOT admitted — the
  in-process server is stood up with `Microsoft.AspNetCore.TestHost.UseTestServer()` directly.
- asset: meta-package
- rail: grpc

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: in-process channel + stub invocation — source: `Grpc.Net.Client` / `Grpc.Core.Api`

- rail: grpc

| [INDEX] | [SYMBOL]             | [SOURCE]          | [TYPE_FAMILY]   | [CAPABILITY]                |
| :-----: | :------------------- | :---------------- | :-------------- | :-------------------------- |
|  [01]   | `GrpcChannel`        | `Grpc.Net.Client` | channel owner   | client RPC channel          |
|  [02]   | `GrpcChannelOptions` | `Grpc.Net.Client` | channel options | channel policy              |
|  [03]   | `CallInvoker`        | `Grpc.Core.Api`   | invoker         | generated-client invocation |
|  [04]   | `ChannelBase`        | `Grpc.Core.Api`   | channel base    | generated-client channel    |
|  [05]   | `Metadata`           | `Grpc.Core.Api`   | call metadata   | header collection           |
|  [06]   | `Status`             | `Grpc.Core.Api`   | call status     | terminal RPC status         |
|  [07]   | `CallOptions`        | `Grpc.Core.Api`   | call options    | per-call policy             |

[PUBLIC_TYPE_SCOPE]: in-process server handler — source: `Microsoft.AspNetCore.TestHost`

- rail: grpc

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]            |
| :-----: | :-------------------------------- | :------------------ | :---------------------- |
|  [01]   | `TestServer`                      | in-process server   | hosted test transport   |
|  [02]   | `ClientHandler`                   | message handler     | in-process HTTP handler |
|  [03]   | `WebHostBuilderExtensions`        | host wiring         | web-host integration    |
|  [04]   | `HostBuilderTestServerExtensions` | generic-host wiring | generic-host access     |
|  [05]   | `TestServerOptions`               | server options      | request feature policy  |

[PUBLIC_TYPE_SCOPE]: grpc-web in-process wrapping — source: `Grpc.Net.Client.Web`

- rail: grpc

`GrpcWebMode.GrpcWeb` selects binary framing, and `GrpcWebMode.GrpcWebText` selects base64 text framing.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :--------------- | :------------ | :------------------ |
|  [01]   | `GrpcWebHandler` | handler       | gRPC-Web framing    |
|  [02]   | `GrpcWebMode`    | enum          | framing-mode choice |

[PUBLIC_TYPE_SCOPE]: server hosting options — source: `Grpc.AspNetCore.Server` (transitive)

- rail: grpc

`GrpcServiceOptions<TService>` derives from `GrpcServiceOptions`. `InterceptorCollection` derives from `Collection<InterceptorRegistration>`, and `GrpcServiceEndpointConventionBuilder` implements `IEndpointConventionBuilder`.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]       | [CAPABILITY]         |
| :-----: | :------------------------------------- | :------------------ | :------------------- |
|  [01]   | `GrpcServiceOptions`                   | server options      | global server policy |
|  [02]   | `GrpcServiceOptions<TService>`         | per-service options | service policy       |
|  [03]   | `IGrpcServerBuilder`                   | builder             | registration chain   |
|  [04]   | `InterceptorCollection`                | interceptor list    | per-call pipeline    |
|  [05]   | `GrpcServiceEndpointConventionBuilder` | endpoint builder    | endpoint conventions |

[PUBLIC_TYPE_SCOPE]: server-side grpc-web middleware — source: `Grpc.AspNetCore.Web` (direct admission)

- rail: grpc

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]      | [CAPABILITY]       |
| :-----: | :------------------------------------------- | :----------------- | :----------------- |
|  [01]   | `GrpcWebApplicationBuilderExtensions`        | middleware wiring  | translation policy |
|  [02]   | `GrpcWebEndpointConventionBuilderExtensions` | endpoint metadata  | endpoint policy    |
|  [03]   | `GrpcWebOptions`                             | middleware options | default policy     |
|  [04]   | `EnableGrpcWebAttribute`                     | endpoint metadata  | opt-in marker      |
|  [05]   | `DisableGrpcWebAttribute`                    | endpoint metadata  | opt-out marker     |

[PUBLIC_TYPE_SCOPE]: gRPC health service — source: `Grpc.AspNetCore.HealthChecks` (direct admission)

- rail: grpc

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :----------------------------------------------- | :---------------- | :-------------------- |
|  [01]   | `GrpcHealthChecksServiceExtensions`              | DI wiring         | health registration   |
|  [02]   | `GrpcHealthChecksEndpointRouteBuilderExtensions` | endpoint wiring   | health endpoint       |
|  [03]   | `GrpcHealthChecksOptions`                        | health options    | health policy         |
|  [04]   | `ServiceMappingCollection`                       | service-name map  | mapping registry      |
|  [05]   | `ServiceMapping`                                 | service-name row  | predicate binding     |
|  [06]   | `HealthCheckMapContext`                          | predicate context | check predicate input |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: in-process channel construction

- rail: grpc

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `GrpcChannel.ForAddress(string, GrpcChannelOptions)`                         | factory call    | configured string address        |
|  [02]   | `GrpcChannel.ForAddress(Uri, GrpcChannelOptions)`                            | factory call    | configured URI                   |
|  [03]   | `GrpcChannel.ForAddress(string)`                                             | factory call    | default string address           |
|  [04]   | `GrpcChannel.ForAddress(Uri)`                                                | factory call    | default URI                      |
|  [05]   | `GrpcChannelOptions.HttpHandler`                                             | option property | `HttpMessageHandler?` injection  |
|  [06]   | `GrpcChannelOptions.HttpClient`                                              | option property | `HttpClient?` injection          |
|  [07]   | `GrpcChannelOptions.MaxReceiveMessageSize`                                   | option property | `int?` receive-size cap          |
|  [08]   | `GrpcChannelOptions.MaxSendMessageSize`                                      | option property | `int?` send-size cap             |
|  [09]   | `GrpcChannelOptions.ServiceConfig`                                           | option property | `ServiceConfig?` policy          |
|  [10]   | `GrpcChannelOptions.CompressionProviders`                                    | option property | compression registry             |
|  [11]   | `GrpcChannelOptions.Credentials`                                             | option property | channel credentials              |
|  [12]   | `GrpcChannelOptions.LoggerFactory`                                           | option property | channel telemetry                |
|  [13]   | `GrpcChannelOptions.DisposeHttpClient`                                       | option property | client ownership                 |
|  [14]   | `GrpcChannelOptions.ThrowOperationCanceledOnCancellation`                    | option property | cancellation projection          |
|  [15]   | `GrpcChannelOptions.HttpVersion`                                             | option property | `Version?` override              |
|  [16]   | `GrpcChannelOptions.HttpVersionPolicy`                                       | option property | `HttpVersionPolicy?` negotiation |
|  [17]   | `GrpcChannel.CreateCallInvoker()`                                            | invoker factory | `CallInvoker` factory            |
|  [18]   | `GrpcChannel.ConnectAsync(CancellationToken)`                                | connectivity    | establish connection             |
|  [19]   | `GrpcChannel.WaitForStateChangedAsync(ConnectivityState, CancellationToken)` | connectivity    | await state change               |
|  [20]   | `GrpcChannel.Dispose()`                                                      | disposal        | channel teardown                 |

[ENTRYPOINT_SCOPE]: in-process server lifecycle (`Microsoft.AspNetCore.TestHost`)

- rail: grpc

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [OUTPUT]             | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------------ | :-------------- | :------------------- | :------------------- |
|  [01]   | `IWebHostBuilder.UseTestServer()`                                   | host wiring     | `IWebHostBuilder`    | server registration  |
|  [02]   | `IWebHostBuilder.UseTestServer(Action<TestServerOptions>)`          | host wiring     | `IWebHostBuilder`    | configured server    |
|  [03]   | `IWebHostBuilder.ConfigureTestServices(Action<IServiceCollection>)` | host wiring     | `IWebHostBuilder`    | service overrides    |
|  [04]   | `IHost.GetTestServer()`                                             | server access   | `TestServer`         | hosted server        |
|  [05]   | `TestServer.CreateHandler()`                                        | handler factory | `HttpMessageHandler` | default context      |
|  [06]   | `TestServer.CreateHandler(Action<HttpContext>)`                     | handler factory | `HttpMessageHandler` | configured context   |
|  [07]   | `TestServer.CreateClient()`                                         | client factory  | `HttpClient`         | hosted test client   |
|  [08]   | `IHost.GetTestClient()`                                             | client factory  | `HttpClient`         | resolved test client |
|  [09]   | `TestServer.Services`                                               | server property | `IServiceProvider`   | hosted services      |
|  [10]   | `TestServer.BaseAddress`                                            | server property | `Uri`                | client base address  |

`TestServerOptions` owns the server policy projected by `UseTestServer(Action<TestServerOptions>)`.

| [INDEX] | [PROPERTY]                 | [TYPE] | [ACCESS] | [CAPABILITY]           |
| :-----: | :------------------------- | :----- | :------- | :--------------------- |
|  [01]   | `AllowSynchronousIO`       | `bool` | get/set  | synchronous I/O        |
|  [02]   | `PreserveExecutionContext` | `bool` | get/set  | execution-context flow |
|  [03]   | `BaseAddress`              | `Uri`  | get/set  | client base address    |

[ENTRYPOINT_SCOPE]: grpc-web in-process wrapping (`Grpc.Net.Client.Web`)

- rail: grpc

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CAPABILITY]         |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `new GrpcWebHandler(HttpMessageHandler innerHandler)`                   | constructor    | binary wrapper       |
|  [02]   | `new GrpcWebHandler(GrpcWebMode mode, HttpMessageHandler innerHandler)` | constructor    | configured wrapper   |
|  [03]   | `GrpcWebHandler.GrpcWebMode`                                            | property       | framing-mode control |

[ENTRYPOINT_SCOPE]: server hosting registration + options

- source: `Grpc.AspNetCore.Server` (transitive) surface
- rail: grpc

Every `GrpcServicesExtensions` row is a static extension that returns `IGrpcServerBuilder`; `AddServiceOptions<TService>` constrains `TService` to `class`.

| [INDEX] | [RECEIVER]           | [MEMBER]                      | [ARG]                                            |
| :-----: | :------------------- | :---------------------------- | :----------------------------------------------- |
|  [01]   | `IServiceCollection` | `AddGrpc`                     | —                                                |
|  [02]   | `IServiceCollection` | `AddGrpc`                     | `Action<GrpcServiceOptions> configureOptions`    |
|  [03]   | `IGrpcServerBuilder` | `AddServiceOptions<TService>` | `Action<GrpcServiceOptions<TService>> configure` |

Every `GrpcEndpointRouteBuilderExtensions` row extends `IEndpointRouteBuilder` and returns `GrpcServiceEndpointConventionBuilder`.

| [INDEX] | [MEMBER]                   | [ARG]                                                           | [CONSTRAINT]       |
| :-----: | :------------------------- | :-------------------------------------------------------------- | :----------------- |
|  [01]   | `MapGrpcService<TService>` | —                                                               | `TService : class` |
|  [02]   | `MapGrpcService`           | `ServerServiceDefinition serviceDefinition`                     | —                  |
|  [03]   | `MapGrpcService`           | `Func<IServiceProvider, ServerServiceDefinition> mapDefinition` | —                  |

`GrpcServiceOptions` owns the server-policy properties.

| [INDEX] | [SURFACE]                        | [TYPE]                        | [ACCESS] | [CAPABILITY]           |
| :-----: | :------------------------------- | :---------------------------- | :------- | :--------------------- |
|  [01]   | `CompressionProviders`           | `IList<ICompressionProvider>` | get/set  | compression registry   |
|  [02]   | `ResponseCompressionAlgorithm`   | `string?`                     | get/set  | response encoding      |
|  [03]   | `ResponseCompressionLevel`       | `CompressionLevel?`           | get/set  | compression level      |
|  [04]   | `MaxReceiveMessageSize`          | `int?`                        | get/set  | receive-size cap       |
|  [05]   | `MaxSendMessageSize`             | `int?`                        | get/set  | send-size cap          |
|  [06]   | `EnableDetailedErrors`           | `bool?`                       | get/set  | error detail policy    |
|  [07]   | `IgnoreUnknownServices`          | `bool?`                       | get/set  | unknown-service policy |
|  [08]   | `Interceptors`                   | `InterceptorCollection`       | get      | server pipeline        |
|  [09]   | `MaxSendMessageSizeSpecified`    | `bool`                        | get/set  | send-limit override    |
|  [10]   | `MaxReceiveMessageSizeSpecified` | `bool`                        | get/set  | receive-limit override |
|  [11]   | `SuppressCreatingService`        | `bool`                        | get/set  | activation suppression |

`InterceptorCollection` returns `void` from both additions and constrains `TInterceptor` to `Interceptor`.

| [INDEX] | [MEMBER]            | [ARG_A]                | [ARG_B]                |
| :-----: | :------------------ | :--------------------- | :--------------------- |
|  [01]   | `Add<TInterceptor>` | `params object[] args` | —                      |
|  [02]   | `Add`               | `Type interceptorType` | `params object[] args` |

[ENTRYPOINT_SCOPE]: server-side grpc-web middleware

- source: `Grpc.AspNetCore.Web` surface
- rail: grpc

`GrpcWebApplicationBuilderExtensions` owns `UseGrpcWeb`, and `GrpcWebEndpointConventionBuilderExtensions` owns the endpoint metadata extensions. Every row is a static extension that returns its receiver; the endpoint extensions constrain `TBuilder` to `IEndpointConventionBuilder`.

| [INDEX] | [MEMBER]                   | [RECEIVER]            | [ARG]                    | [CAPABILITY]          |
| :-----: | :------------------------- | :-------------------- | :----------------------- | :-------------------- |
|  [01]   | `UseGrpcWeb`               | `IApplicationBuilder` | —                        | default middleware    |
|  [02]   | `UseGrpcWeb`               | `IApplicationBuilder` | `GrpcWebOptions options` | configured middleware |
|  [03]   | `EnableGrpcWeb<TBuilder>`  | `TBuilder`            | —                        | opt-in metadata       |
|  [04]   | `DisableGrpcWeb<TBuilder>` | `TBuilder`            | —                        | opt-out metadata      |

`GrpcWebOptions.DefaultEnabled` (`bool`, get/set) selects the default for endpoints without explicit opt-in or opt-out metadata.

[ENTRYPOINT_SCOPE]: gRPC health service

- source: `Grpc.AspNetCore.HealthChecks` surface
- rail: grpc

`GrpcHealthChecksServiceExtensions` owns the static registration overloads from `IServiceCollection` to `IHealthChecksBuilder`.

| [INDEX] | [MEMBER]              | [ARG]                                       |
| :-----: | :-------------------- | :------------------------------------------ |
|  [01]   | `AddGrpcHealthChecks` | —                                           |
|  [02]   | `AddGrpcHealthChecks` | `Action<GrpcHealthChecksOptions> configure` |

`GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService` is a static extension from `IEndpointRouteBuilder` to `GrpcServiceEndpointConventionBuilder`.

`GrpcHealthChecksOptions` owns the health-policy properties.

| [INDEX] | [PROPERTY]                     | [TYPE]                     | [ACCESS] |
| :-----: | :----------------------------- | :------------------------- | :------- |
|  [01]   | `Services`                     | `ServiceMappingCollection` | get      |
|  [02]   | `UseHealthChecksCache`         | `bool`                     | get/set  |
|  [03]   | `SuppressCompletionOnShutdown` | `bool`                     | get/set  |

`ServiceMappingCollection` mutates the service-name map through `void` operations.

| [INDEX] | [MEMBER] | [ARG_A]                  | [ARG_B]                                       |
| :-----: | :------- | :----------------------- | :-------------------------------------------- |
|  [01]   | `Map`    | `string name`            | `Func<HealthCheckMapContext, bool> predicate` |
|  [02]   | `Add`    | `ServiceMapping service` | —                                             |
|  [03]   | `Remove` | `string name`            | —                                             |
|  [04]   | `Clear`  | —                        | —                                             |

`ServiceMappingCollection.Count` (`int`, get) reports the mapping count.

`ServiceMapping(string, Func<HealthCheckMapContext, bool>)` constructs a mapping row.

`ServiceMapping` owns the row projection.

| [INDEX] | [PROPERTY]             | [TYPE]                               | [ACCESS] |
| :-----: | :--------------------- | :----------------------------------- | :------- |
|  [01]   | `Name`                 | `string`                             | get      |
|  [02]   | `HealthCheckPredicate` | `Func<HealthCheckMapContext, bool>?` | get      |

`HealthCheckMapContext` owns the predicate inputs.

| [INDEX] | [PROPERTY] | [TYPE]                | [ACCESS] |
| :-----: | :--------- | :-------------------- | :------- |
|  [01]   | `Name`     | `string`              | get      |
|  [02]   | `Tags`     | `IEnumerable<string>` | get      |

## [04]-[IMPLEMENTATION_LAW]

[IN_PROCESS_PATTERN] — canonical in-process gRPC test wiring (all admitted packages):

1. Stand up the server with `Microsoft.AspNetCore.TestHost`: `new HostBuilder().ConfigureWebHost(web => web.UseTestServer().Configure(app => { app.UseRouting();
app.UseEndpoints(e => e.MapGrpcService<TService>()); }).ConfigureServices(s => s.AddGrpc()))`, then `host.Start()` and `TestServer server = host.GetTestServer()`.
2. `HttpMessageHandler handler = server.CreateHandler()` (concrete `ClientHandler`, no TCP socket).
3. `GrpcChannel channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions {HttpHandler = handler })`.
4. `var client = new TService.TServiceClient(channel.CreateCallInvoker())` — the `Grpc.Tools`-generated stub takes a `Grpc.Core.Api` `CallInvoker` (or `ChannelBase`).

- `GrpcChannelOptions.HttpHandler` accepts the test handler directly; `HttpClient` is the alternate
  injection when a pre-built `TestServer.CreateClient()` is preferred. No real network is opened.
- grpc-web: wrap once — `HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, server.CreateHandler())`.

[SERVER_SURFACE_CONSTRAINT]:

- `Grpc.AspNetCore` has zero DLL assets; `AddGrpc`, `MapGrpcService<T>`, `GrpcServiceOptions`, and interceptor registration (`InterceptorCollection.Add<TInterceptor>`) live in `Grpc.AspNetCore.Server.dll`, pulled in transitively via `Grpc.AspNetCore.Server.ClientFactory`.
- the server compression axis is `GrpcServiceOptions.CompressionProviders` (`IList<ICompressionProvider> { get; set; }`) — the server-side mirror of the client `GrpcChannelOptions.CompressionProviders`; both register the same `Grpc.Net.Common` provider rows (`gzip`/`deflate`). `GrpcServiceOptions.ResponseCompressionAlgorithm` selects the default response encoding; per-write `WriteOptions.NoCompress` overrides it.
- in-process surface types in §2/§3 are member against `Grpc.Net.Client`, `Grpc.Core.Api`, `Microsoft.AspNetCore.TestHost`, and `Grpc.Net.Client.Web`; the server-hosting, grpc-web-middleware, and health-service members are member against the consumer-bound `lib/net10.0` `Grpc.AspNetCore.Server.dll`, `Grpc.AspNetCore.Web.dll`, and `Grpc.AspNetCore.HealthChecks.dll`.

[SERVER_GRPC_WEB] — server-side grpc-web translation (`Grpc.AspNetCore.Web`):

- `app.UseGrpcWeb()` (after `UseRouting`, before `UseEndpoints`) installs `GrpcWebMiddleware`, which unframes `application/grpc-web[-text]` requests into HTTP/2 gRPC and re-frames responses; a browser or `Grpc.Net.Client.Web` `GrpcWebHandler` client speaks grpc-web against the same service.
- opt-in is per-endpoint by default: `endpoints.MapGrpcService<T>().EnableGrpcWeb()` adds `EnableGrpcWebAttribute` metadata; `GrpcWebOptions.DefaultEnabled = true` flips the default so every service is grpc-web-enabled and `.DisableGrpcWeb()` opts a single endpoint out.
- the server middleware is the inbound counterpart of the client `GrpcWebHandler` (§2 grpc-web in-process wrapping): one grpc-web framing contract, server-decode and client-encode.

[HEALTH_SERVICE] — gRPC `Health` service (`Grpc.AspNetCore.HealthChecks` → transitive `Grpc.AspNetCore.Server` + `Grpc.HealthCheck`):

- `services.AddGrpcHealthChecks()` registers the `grpc.health.v1.Health` `HealthServiceImpl` (from the transitive `Grpc.HealthCheck`) as a singleton, registers `GrpcHealthChecksPublisher` as an `IHealthCheckPublisher` (the bridge that pushes `Microsoft.Extensions.Diagnostics.HealthChecks` results into the gRPC health protocol), and calls `services.AddHealthChecks()`; `endpoints.MapGrpcHealthChecksService()` maps the `Check`/`Watch` endpoint.
- the default registration maps the empty-string service name to every health check; narrow it with `options.Services.Map("compute.v1.Solver", ctx => ctx.Tags.Contains("solver"))` so a per-service health probe reports `SERVING` only when the matching tagged checks are healthy — `HealthCheckMapContext` carries the registered check `Name`/`Tags`. `UseHealthChecksCache` serves the last published snapshot instead of re-running checks on every probe.

[STACKING] — single dense remote/wire rail with sibling Compute libs:

- contracts compile with `Grpc.Tools` (build-only) from `.proto`; `Google.Protobuf` owns the `IMessage` wire types the stubs serialize. `NodaTime.Serialization.Protobuf` round-trips `Instant`/`Duration`/`LocalDate` through `google.protobuf.Timestamp`/`Duration` so Compute time fields cross the wire without bespoke converters.
- payload sizing: `Microsoft.IO.RecyclableMemoryStream` backs large request/response buffers; lift `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` for mesh/tensor payloads that exceed the 4 MB gRPC default rather than chunking by hand.
- resilience/telemetry: `GrpcChannelOptions.ServiceConfig` carries the retry/hedging policy and `LoggerFactory` threads the channel into the Compute logging/OTel surface.
- symmetric compression axis: the SAME `ICompressionProvider` rows (`gzip`/`deflate`, owned by `Grpc.Net.Common`, catalogued in `libs/csharp/Rasm.Compute/.api/api-grpc-common.md`) register on BOTH the client `GrpcChannelOptions.CompressionProviders` and the server `GrpcServiceOptions.CompressionProviders`; `GrpcServiceOptions.ResponseCompressionAlgorithm` sets the server's default response encoding so the `grpc-encoding`/`grpc-accept-encoding` negotiation is one axis end to end.
- health + grpc-web on the same host: `MapGrpcHealthChecksService()` and `app.UseGrpcWeb()` compose on the same `AddGrpc()` host as the Compute services; the health publisher reflects the Compute `Microsoft.Extensions.Diagnostics.HealthChecks` registrations into `grpc.health.v1` without a bespoke status DTO, and grpc-web exposes the identical services to a browser/`GrpcWebHandler` client.

[LOCAL_ADMISSION]:

- Compute test projects stand up gRPC servers with `UseTestServer()` and consume `TestServer.CreateHandler()`; no real network is required and `WebApplicationFactory` is not used.
- `GrpcChannelOptions.HttpHandler` (or `HttpClient`) is the sole injection point for the in-process handler; do not subclass `GrpcChannel`.
- generated stubs take a `Grpc.Core.Api` `CallInvoker` or `ChannelBase`; build them from `GrpcChannel.CreateCallInvoker()`.
- server hosting configures through `AddGrpc(o => …)` and `AddServiceOptions<TService>(o => …)`; the server compression set is `GrpcServiceOptions.CompressionProviders`, never a parallel list — it mirrors the client `GrpcChannelOptions.CompressionProviders`. Server interceptors register through `GrpcServiceOptions.Interceptors.Add<TInterceptor>()`, not ad hoc DI.
- grpc-web is server policy via `app.UseGrpcWeb()` + per-endpoint `EnableGrpcWeb()`/`DisableGrpcWeb()` (or `GrpcWebOptions.DefaultEnabled`); the in-process test client mirrors it with `GrpcWebHandler`.
- the gRPC health surface is `AddGrpcHealthChecks()` + `MapGrpcHealthChecksService()`; service-name mapping uses `GrpcHealthChecksOptions.Services.Map(name, ctx => …)` over `HealthCheckMapContext`, never a hand-rolled health proto — the `Microsoft.Extensions.Diagnostics.HealthChecks` registrations are the single source of health truth.

[RAIL_LAW]:

- Package: `Grpc.AspNetCore` (meta) + `Grpc.AspNetCore.Web` + `Grpc.AspNetCore.HealthChecks` (direct)
- Owns: the gRPC ASP.NET server-hosting rail — `AddGrpc`/`GrpcServiceOptions`/`MapGrpcService<T>` (transitive `Grpc.AspNetCore.Server`), server-side grpc-web translation (`UseGrpcWeb`), and the gRPC `Health` service (`AddGrpcHealthChecks`/`MapGrpcHealthChecksService`)
- Server options: `AddGrpc(o => o.CompressionProviders…/ResponseCompressionAlgorithm/MaxReceiveMessageSize/ Interceptors.Add<T>())`; `AddServiceOptions<TService>` for per-service overrides
- In-process test surface: `UseTestServer()` → `TestServer.CreateHandler()` → `GrpcChannelOptions.HttpHandler` → `GrpcChannel.ForAddress` → `CreateCallInvoker()` → `TClient`
- Accept: gRPC server hosting with `GrpcServiceOptions` configuration; server-side grpc-web via `UseGrpcWeb` + `EnableGrpcWeb`; the gRPC health service; in-process integration tests via `Microsoft.AspNetCore.TestHost`; grpc-web in tests via `GrpcWebHandler`
- Reject: real-network gRPC from unit tests; `GrpcChannel` without handler injection; a parallel server compression list beside `GrpcServiceOptions.CompressionProviders`; a hand-rolled health proto beside `Grpc.AspNetCore.HealthChecks`; `WebApplicationFactory` (unadmitted `Microsoft.AspNetCore.Mvc.Testing`)
