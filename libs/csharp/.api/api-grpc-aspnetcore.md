# [RASM_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` owns the gRPC ASP.NET server rail: service registration, endpoint mapping, the global and per-service policy pair with its ordered interceptor pipeline, and the service-model seam that registers methods without a generated base. `Grpc.AspNetCore.Web` folds `application/grpc-web[-text]` traffic onto that rail, and `Grpc.AspNetCore.HealthChecks` projects the registered `Microsoft.Extensions.Diagnostics.HealthChecks` results onto `grpc.health.v1.Health`. This rail terminates calls and never dials them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.AspNetCore`
- package: `Grpc.AspNetCore` (Apache-2.0, The gRPC Authors)
- assembly: `Grpc.AspNetCore.Server`
- namespace: `Grpc.AspNetCore.Server`, `Grpc.AspNetCore.Server.Model`, `Grpc.Core`, `Microsoft.AspNetCore.Builder`, `Microsoft.Extensions.DependencyInjection`
- rail: remote-server

[PACKAGE_SURFACE]: `Grpc.AspNetCore.Web`
- package: `Grpc.AspNetCore.Web` (Apache-2.0, The gRPC Authors)
- assembly: `Grpc.AspNetCore.Web`
- namespace: `Grpc.AspNetCore.Web`, `Microsoft.AspNetCore.Builder`
- rail: remote-server

[PACKAGE_SURFACE]: `Grpc.AspNetCore.HealthChecks`
- package: `Grpc.AspNetCore.HealthChecks` (Apache-2.0, The gRPC Authors)
- assembly: `Grpc.AspNetCore.HealthChecks`
- namespace: `Grpc.AspNetCore.HealthChecks`, `Microsoft.AspNetCore.Builder`, `Microsoft.Extensions.DependencyInjection`
- rail: remote-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: server hosting, policy, and service model — `Grpc.AspNetCore`

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `GrpcServiceOptions`                      | class         | global server policy                           |
|  [02]   | `GrpcServiceOptions<TService>`            | class         | per-service override of the global policy      |
|  [03]   | `IGrpcServerBuilder`                      | interface     | registration chain over `IServiceCollection`   |
|  [04]   | `InterceptorCollection`                   | class         | interceptor registrations, executed in order   |
|  [05]   | `InterceptorRegistration`                 | class         | one interceptor type with its ctor arguments   |
|  [06]   | `GrpcServiceEndpointConventionBuilder`    | class         | `IEndpointConventionBuilder` over mapped roots |
|  [07]   | `GrpcMethodMetadata`                      | class         | endpoint metadata: service type and `IMethod`  |
|  [08]   | `IServerCallContextFeature`               | interface     | call context off the HTTP feature collection   |
|  [09]   | `IGrpcServiceActivator<TService>`         | interface     | service instance creation and release          |
|  [10]   | `IGrpcInterceptorActivator`               | interface     | interceptor instance creation and release      |
|  [11]   | `IGrpcInterceptorActivator<TInterceptor>` | interface     | typed activator resolved per registration      |
|  [12]   | `GrpcActivatorHandle<T>`                  | struct        | activation result carrying ownership state     |
|  [13]   | `IServiceMethodProvider<TService>`        | interface     | per-service method-discovery hook              |
|  [14]   | `ServiceMethodProviderContext<TService>`  | class         | method registration surface                    |

[SERVER_METHOD_DELEGATES]: `UnaryServerMethod` `ServerStreamingServerMethod` `ClientStreamingServerMethod` `DuplexStreamingServerMethod`

[PUBLIC_TYPE_SCOPE]: server-side grpc-web — `Grpc.AspNetCore.Web`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `GrpcWebOptions`          | class         | host-wide default for unmarked endpoints |
|  [02]   | `EnableGrpcWebAttribute`  | class         | per-endpoint opt-in metadata             |
|  [03]   | `DisableGrpcWebAttribute` | class         | per-endpoint opt-out metadata            |
|  [04]   | `IGrpcWebEnabledMetadata` | interface     | metadata contract the middleware reads   |

[PUBLIC_TYPE_SCOPE]: gRPC health service — `Grpc.AspNetCore.HealthChecks`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `GrpcHealthChecksOptions`  | class         | health policy and the service-name map      |
|  [02]   | `ServiceMappingCollection` | class         | `ICollection<ServiceMapping>` keyed by name |
|  [03]   | `ServiceMapping`           | class         | one service name bound to a check predicate |
|  [04]   | `HealthCheckMapContext`    | class         | predicate input: a check's name and tags    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, endpoint mapping, and call-context access

Every `MapGrpcService` overload returns `GrpcServiceEndpointConventionBuilder`; both generic arms constrain `TService : class`.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `AddGrpc() -> IGrpcServerBuilder`                                   | static   | admit the rail on global defaults  |
|  [02]   | `AddGrpc(Action<GrpcServiceOptions>) -> IGrpcServerBuilder`         | static   | admit it with configured policy    |
|  [03]   | `AddServiceOptions<TService>(Action<GrpcServiceOptions<TService>>)` | static   | override policy for one service    |
|  [04]   | `IGrpcServerBuilder.Services -> IServiceCollection`                 | property | reach the underlying collection    |
|  [05]   | `MapGrpcService<TService>()`                                        | static   | map a generated service base       |
|  [06]   | `MapGrpcService(ServerServiceDefinition)`                           | static   | map a pre-built definition         |
|  [07]   | `MapGrpcService(Func<IServiceProvider, ServerServiceDefinition>)`   | static   | map a provider-resolved definition |
|  [08]   | `GetHttpContext() -> HttpContext`                                   | static   | reach request state from a call    |
|  [09]   | `IServerCallContextFeature.ServerCallContext`                       | property | read the call context off features |
|  [10]   | `GrpcMethodMetadata.ServiceType -> Type`                            | property | the mapped service's CLR type      |
|  [11]   | `GrpcMethodMetadata.Method -> IMethod`                              | property | the mapped method descriptor       |

[ENTRYPOINT_SCOPE]: server policy and the interceptor pipeline

`GrpcServiceOptions<TService>` inherits every property below and overrides the global instance for its service; `InterceptorCollection.Add<TInterceptor>` constrains `TInterceptor : Interceptor`.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `GrpcServiceOptions.MaxSendMessageSize -> int?`                          | property | cap outbound message bytes       |
|  [02]   | `GrpcServiceOptions.MaxSendMessageSizeSpecified -> bool`                 | property | mark the send cap explicit       |
|  [03]   | `GrpcServiceOptions.MaxReceiveMessageSize -> int?`                       | property | cap inbound message bytes        |
|  [04]   | `GrpcServiceOptions.MaxReceiveMessageSizeSpecified -> bool`              | property | mark the receive cap explicit    |
|  [05]   | `GrpcServiceOptions.CompressionProviders -> IList<ICompressionProvider>` | property | register server encodings        |
|  [06]   | `GrpcServiceOptions.ResponseCompressionAlgorithm -> string?`             | property | default response `grpc-encoding` |
|  [07]   | `GrpcServiceOptions.ResponseCompressionLevel -> CompressionLevel?`       | property | default response level           |
|  [08]   | `GrpcServiceOptions.EnableDetailedErrors -> bool?`                       | property | send exception detail to peers   |
|  [09]   | `GrpcServiceOptions.IgnoreUnknownServices -> bool?`                      | property | pass unmatched calls downstream  |
|  [10]   | `GrpcServiceOptions.Interceptors -> InterceptorCollection`               | property | the per-call server pipeline     |
|  [11]   | `GrpcServiceOptions.SuppressCreatingService -> bool`                     | property | skip service activation          |
|  [12]   | `InterceptorCollection.Add<TInterceptor>(params object[])`               | instance | register by type argument        |
|  [13]   | `InterceptorCollection.Add(Type, params object[])`                       | instance | register by runtime type         |
|  [14]   | `InterceptorRegistration.Type -> Type`                                   | property | the registered interceptor type  |
|  [15]   | `InterceptorRegistration.Arguments -> IReadOnlyList<object>`             | property | the stored ctor arguments        |

- `GrpcServiceOptions.MaxSendMessageSize`: assignment flips `MaxSendMessageSizeSpecified` true, and clearing that flag nulls the size; the receive pair behaves identically.
- `GrpcServiceOptions.CompressionProviders`: a read materializes an empty list, so the getter never returns null and appending needs no assignment.
- `GrpcServiceOptions.ResponseCompressionAlgorithm`: compression applies only where the request's `grpc-accept-encoding` advertises this algorithm.

[ENTRYPOINT_SCOPE]: service model and activation

`IServiceMethodProvider<TService>` discovers a service's methods, receiving the `ServiceMethodProviderContext<TService>` that owns the `Add*` registrars below; each opens on a `Method<TRequest,TResponse>` descriptor and carries an `IList<object>` endpoint-metadata list before its invoker, and `AddMethod` inserts a `RoutePattern` after the descriptor. Both activators return a `GrpcActivatorHandle<T>` from `Create` and a `ValueTask` from `ReleaseAsync`.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `OnServiceMethodDiscovery(ServiceMethodProviderContext<TService>)`            | instance | discover a service's methods     |
|  [02]   | `AddUnaryMethod(UnaryServerMethod)`                                           | instance | register a unary method          |
|  [03]   | `AddServerStreamingMethod(ServerStreamingServerMethod)`                       | instance | register a server stream         |
|  [04]   | `AddClientStreamingMethod(ClientStreamingServerMethod)`                       | instance | register a client stream         |
|  [05]   | `AddDuplexStreamingMethod(DuplexStreamingServerMethod)`                       | instance | register a duplex stream         |
|  [06]   | `AddMethod(RoutePattern, RequestDelegate)`                                    | instance | map a raw route to a delegate    |
|  [07]   | `ServiceMethodProviderContext<TService>.Argument -> object?`                  | property | the provider-supplied argument   |
|  [08]   | `IGrpcServiceActivator<TService>.Create(IServiceProvider)`                    | instance | mint a service instance          |
|  [09]   | `IGrpcServiceActivator<TService>.ReleaseAsync(GrpcActivatorHandle<TService>)` | instance | release a service instance       |
|  [10]   | `IGrpcInterceptorActivator.Create(IServiceProvider, InterceptorRegistration)` | instance | mint an interceptor instance     |
|  [11]   | `IGrpcInterceptorActivator.ReleaseAsync(GrpcActivatorHandle<Interceptor>)`    | instance | release an interceptor           |
|  [12]   | `GrpcActivatorHandle<T>(T, bool, object?)`                                    | ctor     | carry an instance with its state |

[GRPC_ACTIVATOR_HANDLE]: `Instance` `Created` `State`

[ENTRYPOINT_SCOPE]: server-side grpc-web

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `UseGrpcWeb() -> IApplicationBuilder`               | static   | install translation on host defaults |
|  [02]   | `UseGrpcWeb(GrpcWebOptions) -> IApplicationBuilder` | static   | install with an explicit default     |
|  [03]   | `EnableGrpcWeb<TBuilder>() -> TBuilder`             | static   | stamp opt-in metadata on an endpoint |
|  [04]   | `DisableGrpcWeb<TBuilder>() -> TBuilder`            | static   | stamp opt-out metadata               |
|  [05]   | `GrpcWebOptions.DefaultEnabled -> bool`             | property | flip the host-wide default           |

[ENTRYPOINT_SCOPE]: gRPC health service

`ServiceMappingCollection` is an `ICollection<ServiceMapping>` whose `Map` and `Remove` key by service name.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `AddGrpcHealthChecks() -> IHealthChecksBuilder`                             | static   | admit the health service         |
|  [02]   | `AddGrpcHealthChecks(Action<GrpcHealthChecksOptions>)`                      | static   | admit it with configured mapping |
|  [03]   | `MapGrpcHealthChecksService() -> GrpcServiceEndpointConventionBuilder`      | static   | map `Check` and `Watch`          |
|  [04]   | `GrpcHealthChecksOptions.Services -> ServiceMappingCollection`              | property | the service-name map             |
|  [05]   | `GrpcHealthChecksOptions.UseHealthChecksCache -> bool`                      | property | serve the published snapshot     |
|  [06]   | `GrpcHealthChecksOptions.SuppressCompletionOnShutdown -> bool`              | property | hold status through shutdown     |
|  [07]   | `ServiceMappingCollection.Map(string, Func<HealthCheckMapContext, bool>)`   | instance | bind a name to a predicate       |
|  [08]   | `ServiceMappingCollection.Remove(string)`                                   | instance | drop one service-name row        |
|  [09]   | `ServiceMapping(string, Func<HealthCheckMapContext, bool>)`                 | ctor     | mint a mapping row               |
|  [10]   | `ServiceMapping.Name -> string`                                             | property | the mapped service name          |
|  [11]   | `ServiceMapping.HealthCheckPredicate -> Func<HealthCheckMapContext, bool>?` | property | the bound predicate              |
|  [12]   | `HealthCheckMapContext(string, IEnumerable<string>)`                        | ctor     | mint a predicate input           |
|  [13]   | `HealthCheckMapContext.Name -> string`                                      | property | the check's registered name      |
|  [14]   | `HealthCheckMapContext.Tags -> IEnumerable<string>`                         | property | the check's tags                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `AddGrpc()` registration owns the rail: `GrpcServiceOptions` carries global policy, `GrpcServiceOptions<TService>` overrides it per service, and `Interceptors` is the single per-call pipeline, executed in registration order.
- `ServerCallContext.GetHttpContext()` is the one bridge from a gRPC call to ASP.NET request state, and `IServerCallContextFeature` reads that same context back off the feature collection.
- One `ICompressionProvider` row set registers on both `GrpcServiceOptions.CompressionProviders` and the client `GrpcChannelOptions.CompressionProviders`, so `grpc-encoding`/`grpc-accept-encoding` negotiates one axis end to end.
- `UseGrpcWeb` resolves `IGrpcWebEnabledMetadata` off the matched endpoint, so it runs after routing and falls back to `GrpcWebOptions.DefaultEnabled` for an endpoint carrying none; a translated request enters the pipeline as HTTP/2 `application/grpc` and its response reframes on start.
- `AddGrpcHealthChecks` maps the empty service name to every registered check; `Check` runs only the checks a mapping's predicate selects, while `Watch` runs every check and filters its results.

[STACKING]:
- `Grpc.Tools`(`.api/api-grpc-tools.md`): `GrpcServices=Server` emits the service base class `MapGrpcService<TService>` binds, and `Access` fixes that type's visibility.
- `Google.Protobuf`(`.api/api-protobuf.md`): generated `IMessage<T>` requests and responses are the payloads `GrpcServiceOptions.MaxReceiveMessageSize` bounds.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Instant` and `Duration` cross the server edge as `Timestamp` and `Duration` through its conversion extensions.
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): `GrpcServiceOptions.CompressionProviders` and the message-size pair mirror `GrpcChannelOptions`, so one policy row set configures both ends.
- `Grpc.Core.Api`(`Rasm.Compute/.api/api-grpc-common.md`): service methods take `ServerCallContext` and `IServerStreamWriter<T>`, and interceptors read `Metadata` off the same call.
- `Grpc.Net.Client.Web`(`Rasm.Compute/.api/api-grpc-client-web.md`): `GrpcWebHandler` frames what the `UseGrpcWeb` middleware unframes — one grpc-web contract, encode and decode.
- `Microsoft.AspNetCore.TestHost`(`Rasm.Compute/.api/api-microsoftaspnetcoretesthost.md`): `TestServer.CreateHandler()` feeds `GrpcChannelOptions.HttpHandler`, dialing a mapped endpoint with no socket.
- `OpenTelemetry.Instrumentation.AspNetCore`(`.api/api-otel-instrumentation-aspnetcore.md`): `AspNetCoreTraceInstrumentationOptions.EnableGrpcAspNetCoreSupport` shapes the inbound span for every mapped gRPC endpoint.
- AppHost composition: `AddGrpc` folds into `AddServiceOptions<TService>` and then `Interceptors.Add<TInterceptor>` for policy depth; `IServiceMethodProvider<TService>` registers methods no generated base declares, and `IGrpcServiceActivator<TService>` with `IGrpcInterceptorActivator` own both instance lifetimes.

[LOCAL_ADMISSION]:
- AppHost hosts every gRPC service through `AddGrpc()` and `MapGrpcService<TService>`; `GrpcServiceOptions` and its per-service subclass carry all server policy.
- Server interceptors register through `GrpcServiceOptions.Interceptors.Add<TInterceptor>()`.
- grpc-web is server policy: `app.UseGrpcWeb()` with per-endpoint `EnableGrpcWeb()`/`DisableGrpcWeb()`, or `GrpcWebOptions.DefaultEnabled` for a host-wide default.
- Health status derives from the `Microsoft.Extensions.Diagnostics.HealthChecks` registrations, narrowed per service by `GrpcHealthChecksOptions.Services.Map` over `HealthCheckMapContext`.

[RAIL_LAW]:
- Package: `Grpc.AspNetCore`, `Grpc.AspNetCore.Web`, `Grpc.AspNetCore.HealthChecks`
- Owns: gRPC service registration, endpoint mapping, server policy with its interceptor pipeline, server-side grpc-web translation, and the `grpc.health.v1.Health` service
- Accept: server-terminated calls under `GrpcServiceOptions` policy, browser-framed calls through `UseGrpcWeb`, and health probes projected from the registered checks
- Reject: a hand-rolled health proto, a second server compression list, or a per-service options bag beside `GrpcServiceOptions<TService>`
