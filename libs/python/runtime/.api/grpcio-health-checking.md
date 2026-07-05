# [PY_RUNTIME_API_GRPCIO_HEALTH_CHECKING]

`grpcio-health-checking` supplies the canonical gRPC health protocol (`grpc.health.v1.Health`): an `aio.HealthServicer` holding a per-service `ServingStatus` map, `Check`/`Watch` RPC handlers a caller polls or streams, an async `set(service, status)` mutation, and an `enter_graceful_shutdown()` that permanently flips every registered service to `NOT_SERVING`, plus the `health_pb2`/`health_pb2_grpc` generated message and registration surface. It is the health servicer the `transport/serve#SERVE` `ServerHost` lifecycle registers into its one `grpc.aio.server`, so the `filters.negate(filters.health_check())` trace filter suppresses a liveness protocol that is actually served rather than a phantom the interceptor filters against nothing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-health-checking`
- package: `grpcio-health-checking`
- import: `grpc_health.v1`
- owner: `runtime`
- rail: transport, serve
- version: `1.81.1`
- license: Apache-2.0
- namespaces: `grpc_health.v1.health`, `grpc_health.v1.health.aio`, `grpc_health.v1.health_pb2`, `grpc_health.v1.health_pb2_grpc`
- installed: `1.81.1`
- capability: the standard `grpc.health.v1.Health` servicer (async `aio.HealthServicer` and the threading-backed sync `HealthServicer`), per-service serving-status set/read, `Watch` streaming status observation, single-call graceful-shutdown flip of all services, and the generated `add_HealthServicer_to_server` registration binding into a `grpc.aio` server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: servicer family
- rail: serve

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :-------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `health.aio.HealthServicer`             | servicer      | asyncio `grpc.aio` health servicer; the runtime default   |
|  [02]   | `health.HealthServicer`                 | servicer      | threading-backed sync servicer for blocking serve legs    |
|  [03]   | `health_pb2_grpc.HealthServicer`        | abstract base | generated servicer contract both impls satisfy            |
|  [04]   | `health_pb2_grpc.HealthStub`            | stub          | client `Check`/`Watch` caller over a channel              |

[PUBLIC_TYPE_SCOPE]: wire message + status vocabulary
- rail: serve
- `health_pb2.HealthCheckResponse.ServingStatus` is the closed status enum the servicer sets and the client reads; the serve leg treats `SERVING`/`NOT_SERVING` as the two live states and never invents a parallel liveness flag.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `health_pb2.HealthCheckRequest`            | message       | `service` string selector (`""` = overall)             |
|  [02]   | `health_pb2.HealthCheckResponse`           | message       | `status: ServingStatus` reply                          |
|  [03]   | `health_pb2.HealthCheckResponse.UNKNOWN`   | status = 0    | status not yet determined                              |
|  [04]   | `health_pb2.HealthCheckResponse.SERVING`   | status = 1    | service is live                                        |
|  [05]   | `health_pb2.HealthCheckResponse.NOT_SERVING` | status = 2  | service draining or down; the graceful-shutdown flip   |
|  [06]   | `health_pb2.HealthCheckResponse.SERVICE_UNKNOWN` | status = 3 | `Watch`-only reply for an unregistered service        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: health-key constants
- rail: serve

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :--------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `health.SERVICE_NAME`        | constant       | `"grpc.health.v1.Health"` — the health service's own name    |
|  [02]   | `health.OVERALL_HEALTH`      | constant       | `""` — the empty-string overall-server serving-status key     |

[ENTRYPOINT_SCOPE]: async servicer lifecycle
- rail: serve

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                                              |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `health.aio.HealthServicer()`                         | construct      | one servicer instance; no args                                     |
|  [02]   | `await servicer.set(service: str, status)`            | mutate         | set one service's serving status; ignored after graceful shutdown   |
|  [03]   | `await servicer.enter_graceful_shutdown()`            | drain          | permanently flip every registered service to `NOT_SERVING`; idempotent, later `set` no-ops |
|  [04]   | `servicer.Check(request, context)`                    | rpc handler    | unary serving-status reply the client polls                        |
|  [05]   | `servicer.Watch(request, context)`                    | rpc handler    | streaming status observation yielding on each change               |
|  [06]   | `health_pb2_grpc.add_HealthServicer_to_server(servicer, server)` | register | attach the servicer to the `grpc.aio.Server` at construction       |

## [04]-[IMPLEMENTATION_LAW]

[HEALTH_TOPOLOGY]:
- servicer law: the async `health.aio.HealthServicer` is the one runtime health owner; the sync `health.HealthServicer` covers blocking serve legs only. One servicer instance registers into the one `grpc.aio.server` at construction, never per request.
- key law: the empty-string `OVERALL_HEALTH` key is the whole-server serving status; each dispatched capability surface registers under its own `grpc.health.v1.<Service>` key so a probe can target a single service. The runtime sets `OVERALL_HEALTH` plus each served surface to `SERVING` once the bind completes.
- status law: `set(service, status)` is the sole mutation, awaited under the serve leg's anyio scope; the closed `ServingStatus` enum (`SERVING`/`NOT_SERVING`) is the two live states, `UNKNOWN`/`SERVICE_UNKNOWN` reserved for the not-yet-set and unregistered-`Watch` cases the servicer answers natively.
- drain law: `enter_graceful_shutdown()` is called FIRST in the host drain choreography — before `server.stop(grace)` — so in-flight clients observe `NOT_SERVING` and stop routing new work while the grace window lets outstanding calls complete. The flip is permanent and idempotent: after it, `set` is a no-op, so a late success cannot re-advertise `SERVING` mid-drain.

[INTEGRATION_STACK]:
- serve leg: the `transport/serve#SERVE` `ServerHost` constructs one `health.aio.HealthServicer`, registers it with `add_HealthServicer_to_server(servicer, server)` into the same `grpc.aio.server(interceptors=[...])` the dispatch servicers bind, sets each served descriptor surface plus `OVERALL_HEALTH` to `SERVING` at bind, and awaits `enter_graceful_shutdown()` as the first drainable stage of the ordered receipted shutdown fold ahead of `server.stop(grace)`.
- trace leg: the serve interceptor filters health RPCs from tracing via `filters.negate(filters.health_check())` (`.api/opentelemetry-instrumentation-grpc.md`) — registering this servicer is what makes that filter suppress real, served liveness noise rather than a phantom protocol; the two rows are one contract.
- client leg: an out-of-process probe (or the C# host's global health) dials `health_pb2_grpc.HealthStub(channel).Check(HealthCheckRequest(service=...))`; the runtime authors no client-side health poller — it is the server of the protocol, not a consumer.

[LOCAL_ADMISSION]:
- the serve leg admits `health.aio.HealthServicer`, `add_HealthServicer_to_server`, `set`, and `enter_graceful_shutdown`; the sync servicer and the `Watch` streaming handler ride the generated surface, never re-implemented.
- the `grpc.aio.Server` lifecycle (`add_insecure_port`/`start`/`stop`) arrives settled from `.api/grpcio.md`; this page owns only the health servicer and its serving-status vocabulary.

[RAIL_LAW]:
- Package: `grpcio-health-checking`
- Owns: the `grpc.health.v1.Health` servicer, its per-service serving-status map, the graceful-shutdown flip, and the generated registration binding into a `grpc.aio` server
- Accept: one `health.aio.HealthServicer` registered via `add_HealthServicer_to_server` into the serve leg's `grpc.aio.server`, per-surface + `OVERALL_HEALTH` `set(service, SERVING)` at bind, `enter_graceful_shutdown()` as the first ordered drain stage before `server.stop(grace)`, the `ServingStatus` vocabulary as the closed liveness state
- Reject: a hand-rolled health RPC or a bespoke liveness flag beside the standard servicer, a `filters.health_check()` trace suppression with no servicer actually serving the filtered protocol, a `NOT_SERVING` flip sequenced after `server.stop` where in-flight clients never observe the drain, a late `set(SERVING)` re-advertising a service mid-shutdown, a client-side health poller the runtime does not own
