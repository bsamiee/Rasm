# [RASM_APPHOST_API_GRPC_HEALTHCHECK]

`Grpc.HealthCheck` owns the reference `grpc.health.v1` service surface: the `HealthServiceImpl` per-service serving-status map and the `ServingStatus` vocabulary every AppHost health contributor writes into. Its map is the one wire-health sink — a contributor projects its result through `SetStatus`, never a parallel serving map. `HealthServiceImpl` terminates health probes on the gRPC server rail and never dials them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.HealthCheck`
- package: `Grpc.HealthCheck` (Apache-2.0, The gRPC Authors)
- assembly: `Grpc.HealthCheck`
- namespace: `Grpc.HealthCheck`, `Grpc.Health.V1`
- rail: remote-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reference wire-health service — `Grpc.HealthCheck`

`HealthServiceImpl` extends `Grpc.Health.V1.Health.HealthBase`; `Check` and `Watch` carry the generated `HealthCheckRequest` and `HealthCheckResponse` proto messages.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `HealthServiceImpl`                       | class         | reference `grpc.health.v1` serving-status service |
|  [02]   | `HealthCheckResponse.Types.ServingStatus` | enum          | proto serving-status vocabulary                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `HealthServiceImpl` serving-status operations

Every surface belongs to `HealthServiceImpl`; `ServingStatus` denotes `HealthCheckResponse.Types.ServingStatus`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new HealthServiceImpl()`                                    | ctor     | empty per-service status map, bound as singleton |
|  [02]   | `SetStatus(string, ServingStatus)`                           | instance | set or update one service, notifying `Watch`     |
|  [03]   | `ClearStatus(string)`                                        | instance | drop one service to `ServiceUnknown`             |
|  [04]   | `ClearAll()`                                                 | instance | drop every service to `ServiceUnknown`           |
|  [05]   | `Check(HealthCheckRequest, ServerCallContext) -> Task<Resp>` | instance | unary health RPC                                 |
|  [06]   | `Watch(HealthCheckRequest, IServerStreamWriter<Resp>, ...)`  | instance | server-streaming health RPC                      |

- `Check`, `Watch`: override `Grpc.Health.V1.Health.HealthBase`, served by the substrate `MapGrpcHealthChecksService` endpoint; `Resp` is `HealthCheckResponse`, and `Watch` takes a trailing `ServerCallContext` returning `Task`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HealthServiceImpl` holds one `Dictionary<string, ServingStatus>`; `SetStatus` mutates it and pushes to live `Watch` streams, so capability-health projection writes serving status directly.
- `ServingStatus` proto values `Unknown=0` `Serving=1` `NotServing=2` `ServiceUnknown=3` are the wire contract; the canonical projection maps healthy and degraded to `Serving`, unhealthy to `NotServing`.

[STACKING]:
- `Grpc.AspNetCore.HealthChecks`(`.api/api-grpc-aspnetcore.md`): its `MapGrpcHealthChecksService` endpoint serves this `HealthServiceImpl`, bound as the `grpc.health.v1` singleton beside `AddGrpcHealthChecks`.
- AppHost composition: every `Microsoft.Extensions.Diagnostics.HealthChecks` contributor projects its result into `HealthServiceImpl.SetStatus` through the tag-predicate mapping, so broker, store, and system readiness reach one serving-status surface.

[LOCAL_ADMISSION]:
- AppHost binds `HealthServiceImpl` as the `grpc.health.v1` singleton; health contributors set status through `SetStatus`.

[RAIL_LAW]:
- Package: `Grpc.HealthCheck`
- Owns: the reference `grpc.health.v1` `HealthServiceImpl` serving-status map and the `ServingStatus` vocabulary
- Accept: contributor results projected through `HealthServiceImpl.SetStatus`
- Reject: a hand-rolled serving-status map beside `HealthServiceImpl`
