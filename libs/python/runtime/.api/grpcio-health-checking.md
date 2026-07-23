# [PY_RUNTIME_API_GRPCIO_HEALTH_CHECKING]

`grpcio-health-checking` owns the canonical gRPC health protocol (`grpc.health.v1.Health`): a per-service `ServingStatus` map behind `Check`/`Watch` handlers, an async `set` mutation, and an `enter_graceful_shutdown` that permanently flips every registered service to `NOT_SERVING`. Runtime's serve leg registers one `aio.HealthServicer` into one `grpc.aio.server` as the sole owner of served liveness.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-health-checking`
- package: `grpcio-health-checking` (Apache-2.0)
- module: `grpc_health.v1`
- namespaces: `grpc_health.v1.health`, `grpc_health.v1.health.aio`, `grpc_health.v1.health_pb2`, `grpc_health.v1.health_pb2_grpc`
- rail: transport, serve

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: servicer family

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `health.aio.HealthServicer`      | servicer      | asyncio `grpc.aio` health servicer; the runtime default |
|  [02]   | `health.HealthServicer`          | servicer      | threading-backed sync servicer for blocking serve legs  |
|  [03]   | `health_pb2_grpc.HealthServicer` | abstract base | generated servicer contract both impls satisfy          |
|  [04]   | `health_pb2_grpc.HealthStub`     | stub          | client `Check`/`Watch` caller over a channel            |

[PUBLIC_TYPE_SCOPE]: wire message + status vocabulary
- `health_pb2.HealthCheckResponse.ServingStatus` is the closed status enum the servicer sets and the client reads.

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `health_pb2.HealthCheckRequest`                  | message       | `service` string selector (`""` = overall)           |
|  [02]   | `health_pb2.HealthCheckResponse`                 | message       | `status: ServingStatus` reply                        |
|  [03]   | `health_pb2.HealthCheckResponse.UNKNOWN`         | status = 0    | status not yet determined                            |
|  [04]   | `health_pb2.HealthCheckResponse.SERVING`         | status = 1    | service is live                                      |
|  [05]   | `health_pb2.HealthCheckResponse.NOT_SERVING`     | status = 2    | service draining or down; the graceful-shutdown flip |
|  [06]   | `health_pb2.HealthCheckResponse.SERVICE_UNKNOWN` | status = 3    | `Watch`-only reply for an unregistered service       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: health-key constants

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :---------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `health.SERVICE_NAME`   | static  | `"grpc.health.v1.Health"` — the health service's own name |
|  [02]   | `health.OVERALL_HEALTH` | static  | `""` — the empty-string overall-server serving-status key |

[ENTRYPOINT_SCOPE]: async servicer lifecycle

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `health.aio.HealthServicer()`                    | ctor     | one servicer instance, no args                         |
|  [02]   | `servicer.set(service, status) -> None`          | instance | `await` set one service's serving status               |
|  [03]   | `servicer.enter_graceful_shutdown() -> None`     | instance | `await` flip every registered service to `NOT_SERVING` |
|  [04]   | `servicer.Check(request, context)`               | instance | unary serving-status reply the client polls            |
|  [05]   | `servicer.Watch(request, context)`               | instance | streaming status, one send per change                  |
|  [06]   | `add_HealthServicer_to_server(servicer, server)` | static   | bind the servicer into the `grpc.aio` server           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `health.aio.HealthServicer` is the one runtime health owner, the sync `health.HealthServicer` for blocking legs only; one instance registers into the one `grpc.aio.server` at construction, never per request.
- `OVERALL_HEALTH` (empty string) is the whole-server serving status, each capability surface registering under its own `grpc.health.v1.<Service>` key; the runtime sets `OVERALL_HEALTH` and every served surface to `SERVING` once the bind completes.
- `set(service, status)` is the sole mutation, awaited under the serve leg's anyio scope; `UNKNOWN` and `SERVICE_UNKNOWN` answer the not-yet-set and unregistered-`Watch` cases natively.
- `enter_graceful_shutdown()` runs first, ahead of `server.stop(grace)`, so in-flight clients observe `NOT_SERVING` while the grace window drains outstanding calls; the flip is permanent and idempotent — after it `set` is a no-op, so a late success cannot re-advertise `SERVING` mid-drain.

[STACKING]:
- `grpcio`(`.api/grpcio.md`): the one `grpc.aio.server` this servicer registers into via `add_HealthServicer_to_server(servicer, server)`; the server lifecycle (`add_insecure_port`/`start`/`stop`) arrives settled there.
- `opentelemetry-instrumentation-grpc`(`.api/opentelemetry-instrumentation-grpc.md`): `filters.negate(filters.health_check())` drops this servicer's liveness RPCs from tracing, so registering the servicer is what makes that filter suppress real served noise rather than a phantom.
- serve: the `transport/serve` `ServerHost` constructs one `aio.HealthServicer`, sets each served descriptor and `OVERALL_HEALTH` to `SERVING` at bind, and awaits `enter_graceful_shutdown()` as the first stage of the receipted shutdown fold ahead of `server.stop(grace)`.

[LOCAL_ADMISSION]:
- Serve-leg admission binds `aio.HealthServicer`, `add_HealthServicer_to_server`, `set`, and `enter_graceful_shutdown`; the sync servicer and the `Watch` streaming handler ride the generated surface, never re-implemented.

[RAIL_LAW]:
- Package: `grpcio-health-checking`
- Owns: the `grpc.health.v1.Health` servicer, its per-service serving-status map, the graceful-shutdown flip, and the generated `grpc.aio` registration binding
- Accept: one `aio.HealthServicer` registered via `add_HealthServicer_to_server`, per-surface and `OVERALL_HEALTH` `set(service, SERVING)` at bind, `enter_graceful_shutdown()` as the first drain stage before `server.stop(grace)`, `ServingStatus` as the closed liveness vocabulary
- Reject: a hand-rolled health RPC or bespoke liveness flag beside the servicer, a `filters.health_check()` suppression with no servicer serving the filtered protocol, a `NOT_SERVING` flip sequenced after `server.stop`, a late `set(SERVING)` re-advertising mid-shutdown, a client-side health poller
