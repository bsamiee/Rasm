# [RASM_APPHOST_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` is a meta-package aggregating `Grpc.AspNetCore.Server.ClientFactory`, `Google.Protobuf`, and `Grpc.Tools`; `Grpc.AspNetCore.Server` supplies DI registration, routing, service options, interceptor hosting, and the server-side model provider; `Grpc.AspNetCore.HealthChecks` adds a gRPC health-checking service backed by ASP.NET Core health checks; `Grpc.AspNetCore.Web` adds middleware and endpoint conventions for gRPC-Web browser transport.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.AspNetCore`
- package: `Grpc.AspNetCore`
- assembly: — (meta-package; no library DLL)
- namespace: — (aggregates `Grpc.AspNetCore.Server.ClientFactory`, `Google.Protobuf`, `Grpc.Tools`)
- asset: meta-package
- rail: grpc-server

[PACKAGE_SURFACE]: `Grpc.AspNetCore.HealthChecks`
- package: `Grpc.AspNetCore.HealthChecks`
- assembly: `Grpc.AspNetCore.HealthChecks`
- namespace: `Grpc.AspNetCore.HealthChecks`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: grpc-server

[PACKAGE_SURFACE]: `Grpc.AspNetCore.Web`
- package: `Grpc.AspNetCore.Web`
- assembly: `Grpc.AspNetCore.Web`
- namespace: `Grpc.AspNetCore.Web`, `Microsoft.AspNetCore.Builder`
- asset: runtime library
- rail: grpc-server

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: server core — `Grpc.AspNetCore.Server`
- rail: grpc-server

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `GrpcServiceOptions`                   | options class | global gRPC service configuration               |
|   [2]   | `GrpcServiceOptions<TService>`         | options class | per-service gRPC configuration, extends base    |
|   [3]   | `IGrpcServerBuilder`                   | interface     | builder contract for `AddGrpc` return value     |
|   [4]   | `IGrpcServiceActivator<TService>`      | interface     | service-instance lifecycle contract             |
|   [5]   | `IGrpcInterceptorActivator`            | interface     | interceptor-instance lifecycle contract         |
|   [6]   | `IGrpcInterceptorActivator<T>`         | interface     | typed interceptor activator, extends base       |
|   [7]   | `IServiceMethodProvider<TService>`     | interface     | service-method discovery contract               |
|   [8]   | `ServiceMethodProviderContext<T>`      | class         | method-binding context passed to providers      |
|   [9]   | `InterceptorCollection`                | class         | ordered `InterceptorRegistration` list          |
|  [10]   | `InterceptorRegistration`              | class         | one interceptor binding with type and arguments |
|  [11]   | `GrpcServiceEndpointConventionBuilder` | sealed class  | endpoint convention builder for mapped services |
|  [12]   | `GrpcMethodMetadata`                   | sealed class  | per-method metadata for routing                 |
|  [13]   | `IServerCallContextFeature`            | interface     | exposes `ServerCallContext` via HTTP features   |

[PUBLIC_TYPE_SCOPE]: health checks — `Grpc.AspNetCore.HealthChecks`
- rail: grpc-server

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `GrpcHealthChecksOptions`  | sealed class  | maps health-check entries to gRPC service names |
|   [2]   | `ServiceMappingCollection` | class         | ordered `ServiceMapping` entries                |
|   [3]   | `ServiceMapping`           | sealed class  | one gRPC service → health-check name mapping    |
|   [4]   | `HealthCheckMapContext`    | sealed class  | evaluation context for mapping predicates       |
|   [5]   | `HealthResult`             | sealed class  | resolved `ServingStatus` + entry count pair     |

[PUBLIC_TYPE_SCOPE]: gRPC-Web — `Grpc.AspNetCore.Web`
- rail: grpc-server

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------------ | :--------------- | :------------------------------------------------- |
|   [1]   | `GrpcWebOptions`          | class            | gRPC-Web middleware options; owns `DefaultEnabled` |
|   [2]   | `EnableGrpcWebAttribute`  | sealed attribute | enables gRPC-Web on a specific endpoint            |
|   [3]   | `DisableGrpcWebAttribute` | sealed attribute | disables gRPC-Web on a specific endpoint           |
|   [4]   | `IGrpcWebEnabledMetadata` | interface        | endpoint metadata contract for gRPC-Web activation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DI registration — `Microsoft.Extensions.DependencyInjection`
- rail: grpc-server

| [INDEX] | [SURFACE]                                                                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :-------------- | :---------------------------------------------------- |
|   [1]   | `GrpcServicesExtensions.AddGrpc(this IServiceCollection)`                                                           | DI registration | registers gRPC services, returns `IGrpcServerBuilder` |
|   [2]   | `GrpcServicesExtensions.AddGrpc(this IServiceCollection, Action<GrpcServiceOptions>)`                               | DI registration | registers gRPC with global options                    |
|   [3]   | `GrpcServicesExtensions.AddServiceOptions<TService>(this IGrpcServerBuilder, Action<GrpcServiceOptions<TService>>)` | builder fluent  | per-service options                                   |
|   [4]   | `GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks(this IServiceCollection)`                                    | DI registration | registers gRPC health service                         |
|   [5]   | `GrpcHealthChecksServiceExtensions.AddGrpcHealthChecks(this IServiceCollection, Action<GrpcHealthChecksOptions>)`   | DI registration | with options                                          |

[ENTRYPOINT_SCOPE]: routing — `Microsoft.AspNetCore.Builder`
- rail: grpc-server

| [INDEX] | [SURFACE]                                                                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `GrpcEndpointRouteBuilderExtensions.MapGrpcService<TService>(this IEndpointRouteBuilder)`                                        | routing        | maps generated service         |
|   [2]   | `GrpcEndpointRouteBuilderExtensions.MapGrpcService(this IEndpointRouteBuilder, Func<IServiceProvider, ServerServiceDefinition>)` | routing        | dynamic mapping                |
|   [3]   | `GrpcEndpointRouteBuilderExtensions.MapGrpcService(this IEndpointRouteBuilder, ServerServiceDefinition)`                         | routing        | static definition              |
|   [4]   | `GrpcHealthChecksEndpointRouteBuilderExtensions.MapGrpcHealthChecksService(this IEndpointRouteBuilder)`                          | routing        | maps health-checks endpoint    |
|   [5]   | `GrpcWebApplicationBuilderExtensions.UseGrpcWeb(this IApplicationBuilder)`                                                       | middleware     | enables gRPC-Web middleware    |
|   [6]   | `GrpcWebApplicationBuilderExtensions.UseGrpcWeb(this IApplicationBuilder, GrpcWebOptions)`                                       | middleware     | with options                   |
|   [7]   | `GrpcWebEndpointConventionBuilderExtensions.EnableGrpcWeb<TBuilder>(this TBuilder)`                                              | convention     | enables gRPC-Web per endpoint  |
|   [8]   | `GrpcWebEndpointConventionBuilderExtensions.DisableGrpcWeb<TBuilder>(this TBuilder)`                                             | convention     | disables gRPC-Web per endpoint |

[ENTRYPOINT_SCOPE]: service options surface
- rail: grpc-server

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------------------------- |
|   [1]   | `GrpcServiceOptions.MaxSendMessageSize`           | option property | nullable send limit in bytes                 |
|   [2]   | `GrpcServiceOptions.MaxReceiveMessageSize`        | option property | nullable receive limit in bytes              |
|   [3]   | `GrpcServiceOptions.EnableDetailedErrors`         | option property | includes exception detail in responses       |
|   [4]   | `GrpcServiceOptions.ResponseCompressionAlgorithm` | option property | compression algorithm name                   |
|   [5]   | `GrpcServiceOptions.ResponseCompressionLevel`     | option property | `CompressionLevel` for response compression  |
|   [6]   | `GrpcServiceOptions.CompressionProviders`         | option property | registered `ICompressionProvider` list       |
|   [7]   | `GrpcServiceOptions.Interceptors`                 | option property | ordered `InterceptorCollection`              |
|   [8]   | `GrpcServiceOptions.IgnoreUnknownServices`        | option property | ignores requests for unmapped services       |
|   [9]   | `GrpcHealthChecksOptions.Services`                | option property | `ServiceMappingCollection` for name mapping  |
|  [10]   | `GrpcHealthChecksOptions.UseHealthChecksCache`    | option property | enables periodic health report caching       |
|  [11]   | `GrpcWebOptions.DefaultEnabled`                   | option property | enables gRPC-Web on all endpoints by default |

[ENTRYPOINT_SCOPE]: server call context access
- rail: grpc-server
- surface: `ServerCallContextExtensions.GetHttpContext(this ServerCallContext)` — extension that retrieves `HttpContext` from `ServerCallContext`

## [4]-[IMPLEMENTATION_LAW]

[SERVER_TOPOLOGY]:
- meta-package `Grpc.AspNetCore` has no lib DLL — it selects `Grpc.AspNetCore.Server.ClientFactory` + `Google.Protobuf` + `Grpc.Tools` as runtime dependencies
- primary server assembly: `Grpc.AspNetCore.Server` version `2.80.0`; public surfaces live in `Grpc.AspNetCore.Server` and `Microsoft.Extensions.DependencyInjection` / `Microsoft.AspNetCore.Builder` extension namespaces
- service activation: `IGrpcServiceActivator<TService>` controls per-request service instantiation; default uses DI scoping
- interceptor order: `InterceptorCollection` is ordered; interceptors execute in list order on every call

[GRPC_WEB_TOPOLOGY]:
- middleware: `UseGrpcWeb()` must be placed between `UseRouting` and `MapGrpcService` in the pipeline
- per-endpoint control: `EnableGrpcWebAttribute` / `DisableGrpcWebAttribute` override `GrpcWebOptions.DefaultEnabled`
- content types handled: `application/grpc-web` and `application/grpc-web-text` (base64-framed)

[LOCAL_ADMISSION]:
- Registration entry: `services.AddGrpc()` → `IGrpcServerBuilder`; `AddServiceOptions<TService>()` on the builder sets per-service overrides
- Mapping entry: `endpoints.MapGrpcService<TService>()` — `TService` is the protoc-generated service class
- Health checks: `services.AddGrpcHealthChecks()` → configure `GrpcHealthChecksOptions.Services` mappings; `endpoints.MapGrpcHealthChecksService()` exposes the standard health protocol
- gRPC-Web: admitted only when non-browser clients require it; production gRPC-first traffic uses native HTTP/2

[RAIL_LAW]:
- Package: `Grpc.AspNetCore` + `Grpc.AspNetCore.HealthChecks` + `Grpc.AspNetCore.Web`
- Owns: server DI registration, service routing, interceptor hosting, health-check bridge, gRPC-Web middleware
- Accept: generated service implementations, DI-resolved interceptors, ASP.NET Core pipeline composition
- Reject: manual request framing, server-side Grpc.Core channel construction, raw HTTP/2 stream manipulation
