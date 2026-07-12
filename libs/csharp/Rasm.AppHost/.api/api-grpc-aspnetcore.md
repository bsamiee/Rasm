# [RASM_APPHOST_API_GRPC_ASPNETCORE]

`Grpc.AspNetCore` is a meta-package aggregating `Grpc.AspNetCore.Server.ClientFactory`, `Google.Protobuf`, and `Grpc.Tools`; `Grpc.AspNetCore.Server` supplies DI registration, routing, service options, interceptor hosting, and the server-side model provider; `Grpc.AspNetCore.HealthChecks` adds a gRPC health-checking service backed by ASP.NET Core health checks; `Grpc.AspNetCore.Web` adds middleware and endpoint conventions for gRPC-Web browser transport. `Grpc.HealthCheck` (pulled transitively by `Grpc.AspNetCore.HealthChecks`) carries the reference `HealthServiceImpl` `grpc.health.v1` service implementation and the `HealthCheckResponse.Types.ServingStatus` enum that capability-health projection sets directly.

## [01]-[PACKAGE_SURFACE]

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

[PACKAGE_SURFACE]: `Grpc.HealthCheck` 'transitive via `Grpc.AspNetCore.HealthChecks`'

- package: `Grpc.HealthCheck`
- assembly: `Grpc.HealthCheck`
- namespace: `Grpc.HealthCheck` (`HealthServiceImpl`), `Grpc.Health.V1` (generated `Health`/`HealthCheckRequest`/`HealthCheckResponse`)
- asset: runtime library — pulled transitively (`Grpc.AspNetCore.HealthChecks` → `Grpc.HealthCheck`), never a direct project pin
- rail: grpc-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: server core — `Grpc.AspNetCore.Server`

- rail: grpc-server

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `GrpcServiceOptions`                   | options class | global gRPC service configuration               |
|  [02]   | `GrpcServiceOptions<TService>`         | options class | per-service gRPC configuration, extends base    |
|  [03]   | `IGrpcServerBuilder`                   | interface     | builder contract for `AddGrpc` return value     |
|  [04]   | `IGrpcServiceActivator<TService>`      | interface     | service-instance lifecycle contract             |
|  [05]   | `IGrpcInterceptorActivator`            | interface     | interceptor-instance lifecycle contract         |
|  [06]   | `IGrpcInterceptorActivator<T>`         | interface     | typed interceptor activator, extends base       |
|  [07]   | `IServiceMethodProvider<TService>`     | interface     | service-method discovery contract               |
|  [08]   | `ServiceMethodProviderContext<T>`      | class         | method-binding context passed to providers      |
|  [09]   | `InterceptorCollection`                | class         | ordered `InterceptorRegistration` list          |
|  [10]   | `InterceptorRegistration`              | class         | one interceptor binding with type and arguments |
|  [11]   | `GrpcServiceEndpointConventionBuilder` | sealed class  | endpoint convention builder for mapped services |
|  [12]   | `GrpcMethodMetadata`                   | sealed class  | per-method metadata for routing                 |
|  [13]   | `IServerCallContextFeature`            | interface     | exposes `ServerCallContext` via HTTP features   |

[PUBLIC_TYPE_SCOPE]: health checks — `Grpc.AspNetCore.HealthChecks`

- rail: grpc-server

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `GrpcHealthChecksOptions`  | sealed class  | maps health-check entries to gRPC service names |
|  [02]   | `ServiceMappingCollection` | class         | ordered `ServiceMapping` entries                |
|  [03]   | `ServiceMapping`           | sealed class  | one gRPC service → health-check name mapping    |
|  [04]   | `HealthCheckMapContext`    | sealed class  | evaluation context for mapping predicates       |
|  [05]   | `HealthResult`             | sealed class  | resolved `ServingStatus` + entry count pair     |

[PUBLIC_TYPE_SCOPE]: wire-health service — `Grpc.HealthCheck` (transitive)

- rail: grpc-server

`Grpc.HealthCheck.HealthServiceImpl` extends `Grpc.Health.V1.Health.HealthBase` and owns an internal per-service status map. `Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus` assigns `Unknown=0`, `Serving=1`, `NotServing=2`, and `ServiceUnknown=3`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `Grpc.HealthCheck.HealthServiceImpl`                     | class         | reference `grpc.health.v1` service |
|  [02]   | `Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus` | nested enum   | proto serving-status vocabulary    |

[PUBLIC_TYPE_SCOPE]: gRPC-Web — `Grpc.AspNetCore.Web`

- rail: grpc-server

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `GrpcWebOptions`          | class            | gRPC-Web middleware options; owns `DefaultEnabled` |
|  [02]   | `EnableGrpcWebAttribute`  | sealed attribute | enables gRPC-Web on a specific endpoint            |
|  [03]   | `DisableGrpcWebAttribute` | sealed attribute | disables gRPC-Web on a specific endpoint           |
|  [04]   | `IGrpcWebEnabledMetadata` | interface        | endpoint metadata contract for gRPC-Web activation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DI registration — `Microsoft.Extensions.DependencyInjection`

- rail: grpc-server

| [INDEX] | [TARGET]             | [SURFACE]                                | [ENTRY_FAMILY]  | [CAPABILITY]                 |
| :-----: | :------------------- | :--------------------------------------- | :-------------- | :--------------------------- |
|  [01]   | `IServiceCollection` | `AddGrpc()`                              | DI registration | returns `IGrpcServerBuilder` |
|  [02]   | `IServiceCollection` | `AddGrpc(configure)`                     | DI registration | global gRPC options          |
|  [03]   | `IGrpcServerBuilder` | `AddServiceOptions<TService>(configure)` | builder fluent  | per-service options          |
|  [04]   | `IServiceCollection` | `AddGrpcHealthChecks()`                  | DI registration | gRPC health service          |
|  [05]   | `IServiceCollection` | `AddGrpcHealthChecks(configure)`         | DI registration | health-check options         |

[ENTRYPOINT_SCOPE]: routing — `Microsoft.AspNetCore.Builder`

- rail: grpc-server

| [INDEX] | [TARGET]                | [SURFACE]                      | [ENTRY_FAMILY] | [CAPABILITY]                   |
| :-----: | :---------------------- | :----------------------------- | :------------- | :----------------------------- |
|  [01]   | `IEndpointRouteBuilder` | `MapGrpcService<TService>()`   | routing        | maps generated service         |
|  [02]   | `IEndpointRouteBuilder` | `MapGrpcService(factory)`      | routing        | dynamic mapping                |
|  [03]   | `IEndpointRouteBuilder` | `MapGrpcService(definition)`   | routing        | static definition              |
|  [04]   | `IEndpointRouteBuilder` | `MapGrpcHealthChecksService()` | routing        | maps health-checks endpoint    |
|  [05]   | `IApplicationBuilder`   | `UseGrpcWeb()`                 | middleware     | enables gRPC-Web middleware    |
|  [06]   | `IApplicationBuilder`   | `UseGrpcWeb(options)`          | middleware     | configures gRPC-Web middleware |
|  [07]   | endpoint convention     | `EnableGrpcWeb<TBuilder>()`    | convention     | enables gRPC-Web per endpoint  |
|  [08]   | endpoint convention     | `DisableGrpcWeb<TBuilder>()`   | convention     | disables gRPC-Web per endpoint |

[ENTRYPOINT_SCOPE]: service options surface

- rail: grpc-server

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `GrpcServiceOptions.MaxSendMessageSize`           | option property | nullable send limit in bytes                 |
|  [02]   | `GrpcServiceOptions.MaxReceiveMessageSize`        | option property | nullable receive limit in bytes              |
|  [03]   | `GrpcServiceOptions.EnableDetailedErrors`         | option property | includes exception detail in responses       |
|  [04]   | `GrpcServiceOptions.ResponseCompressionAlgorithm` | option property | compression algorithm name                   |
|  [05]   | `GrpcServiceOptions.ResponseCompressionLevel`     | option property | `CompressionLevel` for response compression  |
|  [06]   | `GrpcServiceOptions.CompressionProviders`         | option property | registered `ICompressionProvider` list       |
|  [07]   | `GrpcServiceOptions.Interceptors`                 | option property | ordered `InterceptorCollection`              |
|  [08]   | `GrpcServiceOptions.IgnoreUnknownServices`        | option property | ignores requests for unmapped services       |
|  [09]   | `GrpcHealthChecksOptions.Services`                | option property | `ServiceMappingCollection` for name mapping  |
|  [10]   | `GrpcHealthChecksOptions.UseHealthChecksCache`    | option property | enables periodic health report caching       |
|  [11]   | `GrpcWebOptions.DefaultEnabled`                   | option property | enables gRPC-Web on all endpoints by default |

[ENTRYPOINT_SCOPE]: server call context access

- rail: grpc-server
- surface: `ServerCallContextExtensions.GetHttpContext(this ServerCallContext)` — extension that retrieves `HttpContext` from `ServerCallContext`

[ENTRYPOINT_SCOPE]: `HealthServiceImpl` wire-health operations — `Grpc.HealthCheck`

- rail: grpc-server

Every surface belongs to `HealthServiceImpl`; `ServingStatus` denotes `HealthCheckResponse.Types.ServingStatus`.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------ | :------------- | :---------------------------------- |
|  [01]   | `new HealthServiceImpl()`                         | ctor           | status-map initialization           |
|  [02]   | `SetStatus(string service, ServingStatus status)` | status set     | registers or updates one service    |
|  [03]   | `ClearStatus(string service)`                     | status clear   | marks one service `ServiceUnknown`  |
|  [04]   | `ClearAll()`                                      | status clear   | marks all services `ServiceUnknown` |
|  [05]   | `Check`                                           | RPC handler    | unary health RPC                    |
|  [06]   | `Watch`                                           | RPC handler    | server-streaming health RPC         |

[OPERATION_DETAILS]:

- `new HealthServiceImpl()`: parameterless; creates an empty per-service status map and binds as a singleton.
- `SetStatus`: returns `void` and notifies active `Watch` streams.
- `ClearStatus` and `ClearAll`: return `void`.
- `Check` and `Watch`: override `Grpc.Health.V1.Health.HealthBase` and are served by `MapGrpcHealthChecksService`.

## [04]-[IMPLEMENTATION_LAW]

[SERVER_TOPOLOGY]:

- meta-package `Grpc.AspNetCore` has no lib DLL — it selects `Grpc.AspNetCore.Server.ClientFactory` + `Google.Protobuf` + `Grpc.Tools` as runtime dependencies
- primary server assembly: `Grpc.AspNetCore.Server` version `2.80.0`; public surfaces live in `Grpc.AspNetCore.Server` and `Microsoft.Extensions.DependencyInjection` / `Microsoft.AspNetCore.Builder` extension namespaces
- service activation: `IGrpcServiceActivator<TService>` controls per-request service instantiation; default uses DI scoping
- interceptor order: `InterceptorCollection` is ordered; interceptors execute in list order on every call

[GRPC_WEB_TOPOLOGY]:

- middleware: `UseGrpcWeb()` must be placed between `UseRouting` and `MapGrpcService` in the pipeline
- per-endpoint control: `EnableGrpcWebAttribute` / `DisableGrpcWebAttribute` override `GrpcWebOptions.DefaultEnabled`
- content types handled: `application/grpc-web` and `application/grpc-web-text` (base64-framed)

[HEALTH_SERVICE_TOPOLOGY]:

- `Grpc.AspNetCore.HealthChecks` pulls `Grpc.HealthCheck` transitively (lockfile `Grpc.AspNetCore.HealthChecks` → `Grpc.HealthCheck`); `Grpc.HealthCheck.HealthServiceImpl` is the reference `grpc.health.v1` service extending the generated `Grpc.Health.V1.Health.HealthBase` and is registered as a singleton (`AddSingleton(new HealthServiceImpl)`) alongside `AddGrpcHealthChecks` / `MapGrpcHealthChecksService`
- `HealthServiceImpl.SetStatus(service, status)` mutates an internal `Dictionary<string, ServingStatus>` and pushes to live `Watch` streams; capability-health projection calls it directly rather than re-deriving a parallel serving map
- `Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus` is the proto-grounded enum (`Unknown=0` / `Serving=1` / `NotServing=2` / `ServiceUnknown=3`); the canonical projection maps healthy and degraded → `Serving`, unhealthy → `NotServing`

[LOCAL_ADMISSION]:

- Registration entry: `services.AddGrpc()` → `IGrpcServerBuilder`; `AddServiceOptions<TService>()` on the builder sets per-service overrides
- Mapping entry: `endpoints.MapGrpcService<TService>()` — `TService` is the protoc-generated service class
- Health checks: `services.AddGrpcHealthChecks()` → configure `GrpcHealthChecksOptions.Services` mappings; `endpoints.MapGrpcHealthChecksService()` exposes the standard health protocol
- gRPC-Web: admitted only when non-browser clients require it; production gRPC-first traffic uses native HTTP/2

[RAIL_LAW]:

- Package: `Grpc.AspNetCore` + `Grpc.AspNetCore.HealthChecks` + `Grpc.AspNetCore.Web` + `Grpc.HealthCheck` (transitive)
- Owns: server DI registration, service routing, interceptor hosting, ASP.NET-Core health-check bridge, gRPC-Web middleware, and the reference `grpc.health.v1` `HealthServiceImpl` serving-status surface
- Accept: generated service implementations, DI-resolved interceptors, ASP.NET Core pipeline composition, `HealthServiceImpl.SetStatus` serving-status projection
- Reject: manual request framing, server-side Grpc.Core channel construction, raw HTTP/2 stream manipulation, a hand-rolled serving-status map beside `HealthServiceImpl`
