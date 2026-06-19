# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_GRPC]

`opentelemetry-instrumentation-grpc` supplies the gRPC tracing instrumentation: four `BaseInstrumentor` subclasses for sync/async client and server legs, standalone interceptor factories for explicit channel and server wiring, and a `filters` predicate algebra that selects which RPCs are traced by method, service, or health-check shape. It is the runtime observability row that emits `CLIENT`/`SERVER` spans around the `grpc.aio` serve leg without hand-rolled interceptors.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-grpc`
- package: `opentelemetry-instrumentation-grpc`
- module: `opentelemetry.instrumentation.grpc`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.instrumentation.grpc`, `opentelemetry.instrumentation.grpc.filters`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor family
- rail: observability

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|   [1]   | `GrpcInstrumentorClient`    | instrumentor  | sync `grpc` client channel patching     |
|   [2]   | `GrpcInstrumentorServer`    | instrumentor  | sync `grpc` server interceptor patching |
|   [3]   | `GrpcAioInstrumentorClient` | instrumentor  | async `grpc.aio` client patching        |
|   [4]   | `GrpcAioInstrumentorServer` | instrumentor  | async `grpc.aio` server patching        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :-------------------------------------------------- | :------------- | :----------------------------------------------- |
|   [1]   | `GrpcInstrumentorClient(filter_=None)`              | construct      | sync client instrumentor                         |
|   [2]   | `GrpcInstrumentorServer(filter_=None)`              | construct      | sync server instrumentor                         |
|   [3]   | `GrpcAioInstrumentorClient(filter_=None)`           | construct      | async client instrumentor                        |
|   [4]   | `GrpcAioInstrumentorServer(filter_=None)`           | construct      | async server instrumentor                        |
|   [5]   | `instrument(**kwargs)`                              | enable         | patch gRPC; forwards `tracer_provider` and hooks |
|   [6]   | `uninstrument(**kwargs)`                            | disable        | unpatch gRPC                                     |
|   [7]   | `instrumentation_dependencies() -> Collection[str]` | dependency     | declares `grpcio >= 1.42.0`                      |

[ENTRYPOINT_SCOPE]: interceptor factories
- rail: observability

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|   [1]   | `client_interceptor(tracer_provider=None, filter_=None, request_hook=None, response_hook=None)`      | sync client    | sync `grpc` unary/stream interceptor               |
|   [2]   | `server_interceptor(tracer_provider=None, filter_=None)`                                             | sync server    | `grpc.ServerInterceptor` for `grpc.server`         |
|   [3]   | `aio_client_interceptors(tracer_provider=None, filter_=None, request_hook=None, response_hook=None)` | async client   | list of `grpc.aio` client interceptors             |
|   [4]   | `aio_server_interceptor(tracer_provider=None, filter_=None)`                                         | async server   | `grpc.aio.ServerInterceptor` for `grpc.aio.server` |
|   [5]   | `intercept_channel(channel, *interceptors)`                                                          | channel wrap   | apply interceptors to an existing channel          |

[ENTRYPOINT_SCOPE]: filter predicates
- rail: observability

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `filters.method_name(name: str) -> Condition`      | predicate      | match exact RPC method name          |
|   [2]   | `filters.method_prefix(prefix: str) -> Condition`  | predicate      | match RPC method by prefix           |
|   [3]   | `filters.full_method_name(name: str) -> Condition` | predicate      | match `/package.Service/Method` path |
|   [4]   | `filters.service_name(name: str) -> Condition`     | predicate      | match exact service name             |
|   [5]   | `filters.service_prefix(prefix: str) -> Condition` | predicate      | match service by prefix              |
|   [6]   | `filters.health_check() -> Condition`              | predicate      | match gRPC health-check RPCs         |
|   [7]   | `filters.all_of(*conditions) -> Condition`         | combinator     | conjunction of predicates            |
|   [8]   | `filters.any_of(*conditions) -> Condition`         | combinator     | disjunction of predicates            |
|   [9]   | `filters.negate(condition) -> Condition`           | combinator     | logical negation of a predicate      |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- instrumentor model: the four `BaseInstrumentor` subclasses monkeypatch gRPC at `instrument()` and revert at `uninstrument()`; one instrumentor instance per leg, activated once at the composition root, never per request.
- explicit wiring: when the serve leg builds its own server, pass `aio_server_interceptor(tracer_provider, filter_)` into `grpc.aio.server(interceptors=[...])` instead of global patching; the async server leg is the runtime default, the sync legs cover blocking call sites.
- provider law: `tracer_provider` defaults to the global OTel provider resolved through `opentelemetry-api`; the SDK provider is installed at startup, never inside library code.
- filter law: trace selection is a composed `filters` predicate (`all_of`/`any_of`/`negate` over `method_*`/`service_*`/`health_check`); `health_check()` negated is the canonical way to suppress noisy liveness RPCs.
- hook law: `request_hook`/`response_hook` enrich client spans only and run inside the active span scope, never blocking; `request_hook(span, request)` receives the outbound message, while `response_hook(span, payload)` receives the awaited unary status-details string (`call.details()`, empty on OK) on the unary legs and the streamed response message per yield on the streaming legs — the gRPC status code is set by the interceptor itself as the `rpc.grpc.status_code` attribute, not passed to the hook.

[LOCAL_ADMISSION]:
- the serve leg admits `aio_server_interceptor` on its `grpc.aio` server; `GrpcAioInstrumentorServer` is the global fallback when the server is constructed outside runtime control.
- `instrumentation_dependencies()` declares `grpcio >= 1.42.0`; the runtime grpc stack already satisfies this on the cp315 core, so no companion lane is required.
- spans, attributes, and propagation arrive settled from `.api/opentelemetry-api.md`; this page owns only the gRPC interceptor and filter surface.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-grpc`
- Owns: OTel span emission around the gRPC client/server legs via interceptors and a filter predicate algebra
- Accept: `aio_server_interceptor` wired into `grpc.aio.server`, instrumentor `instrument()` at the composition root, `filters` predicates for trace selection, request/response hooks for span enrichment
- Reject: hand-rolled gRPC interceptors for tracing, per-request instrumentor activation, SDK provider construction in library code, untraced health-check RPC noise left unfiltered
