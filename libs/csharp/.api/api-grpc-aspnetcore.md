# [RASM_COMPUTE_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` is a meta-package with no DLL assets — its `lib/<tfm>` folders carry `_._`
placeholders. Server hosting code surfaces ship in its transitive `Grpc.AspNetCore.Server`
(via `Grpc.AspNetCore.Server.ClientFactory`); `AddGrpc` / `MapGrpcService<T>` / `GrpcServiceOptions`
/ interceptor wiring live there. This catalogue is the server-hosting owner for the whole gRPC ASP.NET
rail: it pins the transitive `Grpc.AspNetCore.Server` options surface plus the two SEPARATE direct
admissions `Grpc.AspNetCore.Web` (server-side grpc-web middleware — `UseGrpcWeb` / `EnableGrpcWeb`)
and `Grpc.AspNetCore.HealthChecks` (the gRPC `Health` service — `AddGrpcHealthChecks` /
`MapGrpcHealthChecksService`). The decompile-verifiable in-process test surface Compute consumes is
owned by the directly-admitted `Grpc.Net.Client` (channel + stub invoker), `Grpc.Core.Api`
(`CallInvoker`, `ChannelBase`, `Metadata`, `Status`), `Microsoft.AspNetCore.TestHost` (`TestServer` +
`UseTestServer`), and `Grpc.Net.Client.Web` (`GrpcWebHandler` for in-process grpc-web — the CLIENT
mirror of the `Grpc.AspNetCore.Web` server middleware).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.AspNetCore`
- package: `Grpc.AspNetCore` `2.80.0` (meta-package; no DLL assets, `lib/<tfm>/_._`)
- license: Apache-2.0
- transitive bundle (net10.0 dependency group): `Grpc.AspNetCore.Server.ClientFactory` 2.80.0
  (pulls `Grpc.AspNetCore.Server` — the `lib/net10.0/Grpc.AspNetCore.Server.dll` that owns `AddGrpc`,
  `MapGrpcService<T>`, `GrpcServiceOptions`, `InterceptorCollection`), `Google.Protobuf`
  (`exclude=Build,Analyzers`), `Grpc.Tools` (`include=All`, build-only). `Grpc.AspNetCore.Web` and
  `Grpc.AspNetCore.HealthChecks` are NOT in this bundle — they are SEPARATE direct admissions, both
  catalogued here (§2/§3).
- direct admissions catalogued here: `Grpc.AspNetCore.Web` 2.80.0
  (`lib/net10.0/Grpc.AspNetCore.Web.dll` — server-side grpc-web middleware) and
  `Grpc.AspNetCore.HealthChecks` 2.80.0 (`lib/net10.0/Grpc.AspNetCore.HealthChecks.dll` — gRPC health
  service). Both bind the consumer net10.0 TFM directly (not the meta-package placeholder).
- in-process surface owned by (direct admissions): `Grpc.Net.Client` 2.80.0 (`GrpcChannel`),
  `Grpc.Core.Api` 2.80.0 (`CallInvoker`/`ChannelBase`/`Metadata`/`Status`),
  `Microsoft.AspNetCore.TestHost` 10.0.9 (`TestServer`), `Grpc.Net.Client.Web` 2.80.0 (`GrpcWebHandler`)
- build-floor: net8.0 (meta-package dependency groups net8.0/net9.0/net10.0; consumer binds net10.0)
- note: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<T>`) is NOT admitted — the
  in-process server is stood up with `Microsoft.AspNetCore.TestHost.UseTestServer()` directly.
- asset: meta-package
- rail: grpc

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: in-process channel + stub invocation — source: `Grpc.Net.Client` / `Grpc.Core.Api`
- rail: grpc

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [RAIL]                                            |
| :-----: | :-------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `GrpcChannel`                     | channel owner   | client channel + `CallInvoker` factory           |
|  [02]   | `GrpcChannelOptions`              | channel options | handler, message-size caps, service config, creds |
|  [03]   | `CallInvoker` (`Grpc.Core.Api`)   | invoker         | constructor arg for generated `TClient` stubs    |
|  [04]   | `ChannelBase` (`Grpc.Core.Api`)   | channel base    | alternate stub constructor arg                   |
|  [05]   | `Metadata` / `Status` / `CallOptions` (`Grpc.Core.Api`) | call surface | headers / status / per-call options |

[PUBLIC_TYPE_SCOPE]: in-process server handler — source: `Microsoft.AspNetCore.TestHost`
- rail: grpc

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]     | [RAIL]                                              |
| :-----: | :------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `TestServer`                     | in-process server | owns `CreateHandler()` / `CreateClient()` / `Services` |
|  [02]   | `ClientHandler`                  | message handler   | `public HttpMessageHandler` returned by `CreateHandler()` |
|  [03]   | `WebHostBuilderExtensions`       | host wiring       | `UseTestServer()`, `GetTestServer()`, `ConfigureTestServices()` |
|  [04]   | `HostBuilderTestServerExtensions` | generic-host wiring | `IHost.GetTestServer()` / `GetTestClient()`      |
|  [05]   | `TestServerOptions`              | server options    | base address, request feature config              |

[PUBLIC_TYPE_SCOPE]: grpc-web in-process wrapping — source: `Grpc.Net.Client.Web`
- rail: grpc

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                                                  |
| :-----: | :---------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `GrpcWebHandler`  | message handler | wraps an inner handler to speak grpc-web / grpc-web-text |
|  [02]   | `GrpcWebMode`     | mode enum       | `GrpcWeb` / `GrpcWebText` framing select               |

[PUBLIC_TYPE_SCOPE]: server hosting options — source: `Grpc.AspNetCore.Server` (transitive)
- rail: grpc

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :-------------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `GrpcServiceOptions`                    | server options     | `CompressionProviders` / message-size caps / detailed errors / interceptors |
|  [02]   | `GrpcServiceOptions<TService>`          | per-service options | typed subclass for `AddServiceOptions<TService>`           |
|  [03]   | `IGrpcServerBuilder`                    | builder            | returned by `AddGrpc`; chains `AddServiceOptions<T>`        |
|  [04]   | `InterceptorCollection`                 | interceptor list   | `Collection<InterceptorRegistration>`; per-call pipeline    |
|  [05]   | `GrpcServiceEndpointConventionBuilder`  | endpoint builder   | `IEndpointConventionBuilder` returned by `MapGrpcService<T>` |

[PUBLIC_TYPE_SCOPE]: server-side grpc-web middleware — source: `Grpc.AspNetCore.Web` (direct admission)
- rail: grpc

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]    | [RAIL]                                                  |
| :-----: | :---------------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `GrpcWebApplicationBuilderExtensions`     | middleware wiring | `UseGrpcWeb()` registers the grpc-web translation middleware |
|  [02]   | `GrpcWebEndpointConventionBuilderExtensions` | endpoint metadata | `EnableGrpcWeb<TBuilder>()` / `DisableGrpcWeb<TBuilder>()` per endpoint |
|  [03]   | `GrpcWebOptions`                          | middleware options | `DefaultEnabled` opts every endpoint into grpc-web globally |
|  [04]   | `EnableGrpcWebAttribute` / `DisableGrpcWebAttribute` | endpoint metadata | per-service/method grpc-web opt-in/out marker         |

[PUBLIC_TYPE_SCOPE]: gRPC health service — source: `Grpc.AspNetCore.HealthChecks` (direct admission)
- rail: grpc

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]       | [RAIL]                                                  |
| :-----: | :--------------------------------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `GrpcHealthChecksServiceExtensions`            | DI wiring           | `AddGrpcHealthChecks()` binds the `Health` service to `IHealthChecksBuilder` |
|  [02]   | `GrpcHealthChecksEndpointRouteBuilderExtensions` | endpoint wiring   | `MapGrpcHealthChecksService()` maps the gRPC `Health` endpoint |
|  [03]   | `GrpcHealthChecksOptions`                      | health options      | `Services` map / `UseHealthChecksCache` / `SuppressCompletionOnShutdown` |
|  [04]   | `ServiceMappingCollection` / `ServiceMapping`  | service-name map     | maps a gRPC service name to a `HealthCheck` tag predicate |
|  [05]   | `HealthCheckMapContext`                        | predicate context   | `Name` / `Tags` of a registered health check (predicate input) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: in-process channel construction
- rail: grpc

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `GrpcChannel.ForAddress(string address, GrpcChannelOptions options)` / `(Uri, options)` | factory call | channel bound to address + handler |
|  [02]   | `GrpcChannel.ForAddress(string)` / `(Uri)`                            | factory call    | channel with default options                  |
|  [03]   | `GrpcChannelOptions.HttpHandler { get; set; }`                        | option property | accepts `HttpMessageHandler?` from `TestServer` |
|  [04]   | `GrpcChannelOptions.HttpClient { get; set; }`                         | option property | alternate: a `TestServer.CreateClient()` `HttpClient` |
|  [05]   | `GrpcChannelOptions.MaxReceiveMessageSize` / `MaxSendMessageSize` (`int?`) | option       | lift caps for large mesh/tensor payloads      |
|  [06]   | `GrpcChannelOptions.ServiceConfig` (`ServiceConfig?`)                 | option property | retry / hedging / load-balancing policy        |
|  [07]   | `GrpcChannelOptions.CompressionProviders` / `Credentials` / `LoggerFactory` / `DisposeHttpClient` / `ThrowOperationCanceledOnCancellation` | option | codec / creds / telemetry / lifetime |
|  [08]   | `GrpcChannel.CreateCallInvoker()`                                     | invoker factory | `CallInvoker` for `new TClient(invoker)`       |
|  [09]   | `GrpcChannel.ConnectAsync(ct)` / `WaitForStateChangedAsync(state, ct)` | connectivity   | await channel readiness in a test before calls |
|  [10]   | `GrpcChannel.Dispose()`                                               | disposal        | channel + connection teardown                 |

[ENTRYPOINT_SCOPE]: in-process server lifecycle (`Microsoft.AspNetCore.TestHost`)
- rail: grpc

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `IWebHostBuilder.UseTestServer()` / `UseTestServer(Action<TestServerOptions>)` | host wiring   | register the in-process server                 |
|  [02]   | `IWebHostBuilder.ConfigureTestServices(Action<IServiceCollection>)`          | host wiring      | override/seam services for the test            |
|  [03]   | `IHost.GetTestServer()` / `IWebHost.GetTestServer()`                         | server access    | resolve the `TestServer` after build           |
|  [04]   | `TestServer.CreateHandler()` / `CreateHandler(Action<HttpContext>)`         | handler factory  | `HttpMessageHandler` (+ per-request context cfg) |
|  [05]   | `TestServer.CreateClient()` / `IHost.GetTestClient()`                        | client factory   | `HttpClient` over the in-process handler        |
|  [06]   | `TestServer.Services` / `TestServer.BaseAddress`                             | DI / address     | hosted `IServiceProvider` / in-process base URI |

[ENTRYPOINT_SCOPE]: grpc-web in-process wrapping (`Grpc.Net.Client.Web`)
- rail: grpc

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                         |
| :-----: | :---------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `new GrpcWebHandler(GrpcWebMode mode, HttpMessageHandler innerHandler)` | ctor      | wrap `TestServer.CreateHandler()` for grpc-web |
|  [02]   | `GrpcWebHandler.GrpcWebMode` / `HttpVersion`                      | property        | framing mode / forced HTTP version            |

[ENTRYPOINT_SCOPE]: server hosting registration + options
- source: `Grpc.AspNetCore.Server` 2.80.0 (transitive) decompile
- rail: grpc

| [INDEX] | [MEMBER]                                       | [SIGNATURE]                                                                                                  |
| :-----: | :--------------------------------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `GrpcServicesExtensions.AddGrpc`               | `static IGrpcServerBuilder AddGrpc(this IServiceCollection services)`                                        |
|  [02]   | `GrpcServicesExtensions.AddGrpc`               | `static IGrpcServerBuilder AddGrpc(this IServiceCollection services, Action<GrpcServiceOptions> configureOptions)` |
|  [03]   | `GrpcServicesExtensions.AddServiceOptions`     | `static IGrpcServerBuilder AddServiceOptions<TService>(this IGrpcServerBuilder grpcBuilder, Action<GrpcServiceOptions<TService>> configure) where TService : class` |
|  [04]   | `GrpcEndpointRouteBuilderExtensions.MapGrpcService` | `static GrpcServiceEndpointConventionBuilder MapGrpcService<TService>(this IEndpointRouteBuilder builder) where TService : class` |
|  [05]   | `GrpcServiceOptions.CompressionProviders`      | `IList<ICompressionProvider> CompressionProviders { get; }` — server registers `Grpc.Net.Common` providers here (server mirror of `GrpcChannelOptions.CompressionProviders`) |
|  [06]   | `GrpcServiceOptions.ResponseCompressionAlgorithm` | `string? ResponseCompressionAlgorithm { get; set; }` — default `grpc-encoding` for responses               |
|  [07]   | `GrpcServiceOptions.ResponseCompressionLevel`  | `CompressionLevel? ResponseCompressionLevel { get; set; }`                                                  |
|  [08]   | `GrpcServiceOptions.MaxReceiveMessageSize` / `MaxSendMessageSize` | `int?` — server-side caps (mirror the channel-side caps for large mesh/tensor payloads)    |
|  [09]   | `GrpcServiceOptions.EnableDetailedErrors`      | `bool? EnableDetailedErrors { get; set; }` — surface exception detail in `Status` (dev only)                |
|  [10]   | `GrpcServiceOptions.IgnoreUnknownServices`     | `bool? IgnoreUnknownServices { get; set; }`                                                                 |
|  [11]   | `GrpcServiceOptions.Interceptors`              | `InterceptorCollection Interceptors { get; }` — per-call server pipeline                                    |
|  [12]   | `InterceptorCollection.Add`                    | `void Add<TInterceptor>(params object[] args) where TInterceptor : Interceptor` / `void Add(Type interceptorType, params object[] args)` |

[ENTRYPOINT_SCOPE]: server-side grpc-web middleware
- source: `Grpc.AspNetCore.Web` 2.80.0 decompile
- rail: grpc

| [INDEX] | [MEMBER]                                                | [SIGNATURE]                                                                                          |
| :-----: | :------------------------------------------------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `GrpcWebApplicationBuilderExtensions.UseGrpcWeb`        | `static IApplicationBuilder UseGrpcWeb(this IApplicationBuilder builder)` — registers `GrpcWebMiddleware` with default options |
|  [02]   | `GrpcWebApplicationBuilderExtensions.UseGrpcWeb`        | `static IApplicationBuilder UseGrpcWeb(this IApplicationBuilder builder, GrpcWebOptions options)`     |
|  [03]   | `GrpcWebEndpointConventionBuilderExtensions.EnableGrpcWeb` | `static TBuilder EnableGrpcWeb<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder` — adds `EnableGrpcWebAttribute` metadata |
|  [04]   | `GrpcWebEndpointConventionBuilderExtensions.DisableGrpcWeb` | `static TBuilder DisableGrpcWeb<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder` |
|  [05]   | `GrpcWebOptions.DefaultEnabled`                         | `bool DefaultEnabled { get; set; }` — opt every endpoint into grpc-web without per-endpoint metadata |

[ENTRYPOINT_SCOPE]: gRPC health service
- source: `Grpc.AspNetCore.HealthChecks` 2.80.0 decompile
- rail: grpc

| [INDEX] | [MEMBER]                                                       | [SIGNATURE]                                                                                          |
| :-----: | :------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks`        | `static IHealthChecksBuilder AddGrpcHealthChecks(this IServiceCollection services)` — registers `HealthServiceImpl` + `GrpcHealthChecksPublisher`, maps empty-name service to all checks |
|  [02]   | `GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks`        | `static IHealthChecksBuilder AddGrpcHealthChecks(this IServiceCollection services, Action<GrpcHealthChecksOptions> configure)` |
|  [03]   | `GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService` | `static GrpcServiceEndpointConventionBuilder MapGrpcHealthChecksService(this IEndpointRouteBuilder builder)` — maps the gRPC `Health` (`grpc.health.v1`) endpoint |
|  [04]   | `GrpcHealthChecksOptions.Services`                            | `ServiceMappingCollection Services { get; }` — service-name → health-check predicate map            |
|  [05]   | `GrpcHealthChecksOptions.UseHealthChecksCache`                | `bool UseHealthChecksCache { get; set; }` — serve last-published cache instead of running checks per probe |
|  [06]   | `GrpcHealthChecksOptions.SuppressCompletionOnShutdown`        | `bool SuppressCompletionOnShutdown { get; set; }`                                                    |
|  [07]   | `ServiceMappingCollection.Map`                                | `void Map(string name, Func<HealthCheckMapContext, bool> predicate)` — maps a named gRPC health service to the checks whose `Name`/`Tags` match the predicate |
|  [08]   | `ServiceMappingCollection.Add` / `Remove` / `Clear`          | `void Add(ServiceMapping)` / `void Remove(string name)` / `void Clear()`                             |
|  [09]   | `HealthCheckMapContext.Name` / `Tags`                        | `string Name { get; }` / `IEnumerable<string> Tags { get; }` — predicate input per registered check  |

## [04]-[IMPLEMENTATION_LAW]

[IN_PROCESS_PATTERN] — canonical in-process gRPC test wiring (all admitted packages):
1. Stand up the server with `Microsoft.AspNetCore.TestHost`:
   `new HostBuilder().ConfigureWebHost(web => web.UseTestServer().Configure(app => { app.UseRouting();
   app.UseEndpoints(e => e.MapGrpcService<TService>()); }).ConfigureServices(s => s.AddGrpc()))`,
   then `host.Start()` and `TestServer server = host.GetTestServer()`.
2. `HttpMessageHandler handler = server.CreateHandler()` (concrete `ClientHandler`, no TCP socket).
3. `GrpcChannel channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions {
   HttpHandler = handler })`.
4. `var client = new TService.TServiceClient(channel.CreateCallInvoker())` — the `Grpc.Tools`-generated
   stub takes a `Grpc.Core.Api` `CallInvoker` (or `ChannelBase`).
- `GrpcChannelOptions.HttpHandler` accepts the test handler directly; `HttpClient` is the alternate
  injection when a pre-built `TestServer.CreateClient()` is preferred. No real network is opened.
- grpc-web: wrap once — `HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, server.CreateHandler())`.

[SERVER_SURFACE_CONSTRAINT]:
- `Grpc.AspNetCore` has zero DLL assets; `AddGrpc`, `MapGrpcService<T>`, `GrpcServiceOptions`, and
  interceptor registration (`InterceptorCollection.Add<TInterceptor>`) live in
  `Grpc.AspNetCore.Server.dll`, pulled in transitively via `Grpc.AspNetCore.Server.ClientFactory`.
- the server compression axis is `GrpcServiceOptions.CompressionProviders`
  (`IList<ICompressionProvider> { get; }`) — the server-side mirror of the client
  `GrpcChannelOptions.CompressionProviders`; both register the same `Grpc.Net.Common` provider rows
  (`gzip`/`deflate`). `GrpcServiceOptions.ResponseCompressionAlgorithm` selects the default response
  encoding; per-write `WriteOptions.NoCompress` overrides it.
- in-process surface types in §2/§3 are decompile-verified against `Grpc.Net.Client`,
  `Grpc.Core.Api`, `Microsoft.AspNetCore.TestHost`, and `Grpc.Net.Client.Web`; the server-hosting,
  grpc-web-middleware, and health-service members are decompile-verified against the consumer-bound
  `lib/net10.0` `Grpc.AspNetCore.Server.dll`, `Grpc.AspNetCore.Web.dll`, and
  `Grpc.AspNetCore.HealthChecks.dll`.

[SERVER_GRPC_WEB] — server-side grpc-web translation (`Grpc.AspNetCore.Web`):
- `app.UseGrpcWeb()` (after `UseRouting`, before `UseEndpoints`) installs `GrpcWebMiddleware`, which
  unframes `application/grpc-web[-text]` requests into HTTP/2 gRPC and re-frames responses; a browser
  or `Grpc.Net.Client.Web` `GrpcWebHandler` client speaks grpc-web against the same service.
- opt-in is per-endpoint by default: `endpoints.MapGrpcService<T>().EnableGrpcWeb()` adds
  `EnableGrpcWebAttribute` metadata; `GrpcWebOptions.DefaultEnabled = true` flips the default so every
  service is grpc-web-enabled and `.DisableGrpcWeb()` opts a single endpoint out.
- the server middleware is the inbound counterpart of the client `GrpcWebHandler` (§2 grpc-web
  in-process wrapping): one grpc-web framing contract, server-decode and client-encode.

[HEALTH_SERVICE] — gRPC `Health` service (`Grpc.AspNetCore.HealthChecks` → transitive `Grpc.AspNetCore.Server` + `Grpc.HealthCheck`):
- `services.AddGrpcHealthChecks()` registers the `grpc.health.v1.Health` `HealthServiceImpl` (from the
  transitive `Grpc.HealthCheck`) as a singleton, registers `GrpcHealthChecksPublisher` as an
  `IHealthCheckPublisher` (the bridge that pushes `Microsoft.Extensions.Diagnostics.HealthChecks`
  results into the gRPC health protocol), and calls `services.AddHealthChecks()`;
  `endpoints.MapGrpcHealthChecksService()` maps the `Check`/`Watch` endpoint.
- the default registration maps the empty-string service name to every health check; narrow it with
  `options.Services.Map("compute.v1.Solver", ctx => ctx.Tags.Contains("solver"))` so a per-service
  health probe reports `SERVING` only when the matching tagged checks are healthy — `HealthCheckMapContext`
  carries the registered check `Name`/`Tags`. `UseHealthChecksCache` serves the last published snapshot
  instead of re-running checks on every probe.

[STACKING] — single dense remote/wire rail with sibling Compute libs:
- contracts compile with `Grpc.Tools` (build-only) from `.proto`; `Google.Protobuf` owns the
  `IMessage` wire types the stubs serialize. `NodaTime.Serialization.Protobuf` round-trips
  `Instant`/`Duration`/`LocalDate` through `google.protobuf.Timestamp`/`Duration` so Compute time
  fields cross the wire without bespoke converters.
- payload sizing: `Microsoft.IO.RecyclableMemoryStream` backs large request/response buffers; lift
  `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` for mesh/tensor payloads that exceed
  the 4 MB gRPC default rather than chunking by hand.
- resilience/telemetry: `GrpcChannelOptions.ServiceConfig` carries the retry/hedging policy and
  `LoggerFactory` threads the channel into the Compute logging/OTel surface.
- symmetric compression axis: the SAME `ICompressionProvider` rows (`gzip`/`deflate`, owned by
  `Grpc.Net.Common`, catalogued in `api-grpc-common.md`) register on BOTH the client
  `GrpcChannelOptions.CompressionProviders` and the server `GrpcServiceOptions.CompressionProviders`;
  `GrpcServiceOptions.ResponseCompressionAlgorithm` sets the server's default response encoding so the
  `grpc-encoding`/`grpc-accept-encoding` negotiation is one axis end to end.
- health + grpc-web on the same host: `MapGrpcHealthChecksService()` and `app.UseGrpcWeb()` compose on
  the same `AddGrpc()` host as the Compute services; the health publisher reflects the Compute
  `Microsoft.Extensions.Diagnostics.HealthChecks` registrations into `grpc.health.v1` without a bespoke
  status DTO, and grpc-web exposes the identical services to a browser/`GrpcWebHandler` client.

[LOCAL_ADMISSION]:
- Compute test projects stand up gRPC servers with `UseTestServer()` and consume
  `TestServer.CreateHandler()`; no real network is required and `WebApplicationFactory` is not used.
- `GrpcChannelOptions.HttpHandler` (or `HttpClient`) is the sole injection point for the in-process
  handler; do not subclass `GrpcChannel`.
- generated stubs take a `Grpc.Core.Api` `CallInvoker` or `ChannelBase`; build them from
  `GrpcChannel.CreateCallInvoker()`.
- server hosting configures through `AddGrpc(o => …)` and `AddServiceOptions<TService>(o => …)`; the
  server compression set is `GrpcServiceOptions.CompressionProviders`, never a parallel list — it
  mirrors the client `GrpcChannelOptions.CompressionProviders`. Server interceptors register through
  `GrpcServiceOptions.Interceptors.Add<TInterceptor>()`, not ad hoc DI.
- grpc-web is server policy via `app.UseGrpcWeb()` + per-endpoint `EnableGrpcWeb()`/`DisableGrpcWeb()`
  (or `GrpcWebOptions.DefaultEnabled`); the in-process test client mirrors it with `GrpcWebHandler`.
- the gRPC health surface is `AddGrpcHealthChecks()` + `MapGrpcHealthChecksService()`; service-name
  mapping uses `GrpcHealthChecksOptions.Services.Map(name, ctx => …)` over `HealthCheckMapContext`,
  never a hand-rolled health proto — the `Microsoft.Extensions.Diagnostics.HealthChecks` registrations
  are the single source of health truth.

[RAIL_LAW]:
- Package: `Grpc.AspNetCore` (meta) + `Grpc.AspNetCore.Web` + `Grpc.AspNetCore.HealthChecks` (direct)
- Owns: the gRPC ASP.NET server-hosting rail — `AddGrpc`/`GrpcServiceOptions`/`MapGrpcService<T>`
  (transitive `Grpc.AspNetCore.Server`), server-side grpc-web translation (`UseGrpcWeb`), and the
  gRPC `Health` service (`AddGrpcHealthChecks`/`MapGrpcHealthChecksService`)
- Server options: `AddGrpc(o => o.CompressionProviders…/ResponseCompressionAlgorithm/MaxReceiveMessageSize/
  Interceptors.Add<T>())`; `AddServiceOptions<TService>` for per-service overrides
- In-process test surface: `UseTestServer()` → `TestServer.CreateHandler()` →
  `GrpcChannelOptions.HttpHandler` → `GrpcChannel.ForAddress` → `CreateCallInvoker()` → `TClient`
- Accept: gRPC server hosting with `GrpcServiceOptions` configuration; server-side grpc-web via
  `UseGrpcWeb` + `EnableGrpcWeb`; the gRPC health service; in-process integration tests via
  `Microsoft.AspNetCore.TestHost`; grpc-web in tests via `GrpcWebHandler`
- Reject: real-network gRPC from unit tests; `GrpcChannel` without handler injection; a parallel
  server compression list beside `GrpcServiceOptions.CompressionProviders`; a hand-rolled health proto
  beside `Grpc.AspNetCore.HealthChecks`; `WebApplicationFactory` (unadmitted
  `Microsoft.AspNetCore.Mvc.Testing`)
