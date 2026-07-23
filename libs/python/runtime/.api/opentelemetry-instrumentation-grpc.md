# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_GRPC]

`opentelemetry-instrumentation-grpc` emits `CLIENT`/`SERVER` OTel spans around the gRPC serve and dial legs: four `BaseInstrumentor` subclasses monkeypatch the sync and async client/server legs, standalone interceptor factories wire an explicit channel or server, and a `filters` predicate algebra selects which RPCs trace by method, service, or health-check shape. It is the observability row that traces the `grpc.aio` legs without a hand-rolled interceptor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-grpc`
- package: `opentelemetry-instrumentation-grpc`
- module: `opentelemetry.instrumentation.grpc`
- namespaces: `opentelemetry.instrumentation.grpc`, `opentelemetry.instrumentation.grpc.filters`, `opentelemetry.instrumentation.grpc.grpcext`
- rail: observability
- abi: pure-Python runtime library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor family

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `GrpcInstrumentorClient`    | instrumentor  | sync `grpc` client channel patching     |
|  [02]   | `GrpcInstrumentorServer`    | instrumentor  | sync `grpc` server interceptor patching |
|  [03]   | `GrpcAioInstrumentorClient` | instrumentor  | async `grpc.aio` client patching        |
|  [04]   | `GrpcAioInstrumentorServer` | instrumentor  | async `grpc.aio` server patching        |

[PUBLIC_TYPE_SCOPE]: filter predicate algebra
- `filters.Condition[CallDetailsT] = Callable[[CallDetailsT], bool]` — names the predicate type every `method_*`/`service_*`/`health_check`/combinator returns and the `filter_=` kwarg accepts, evaluated against the per-call details object.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------ | :------------ | :--------------------------------------------------- |
|  [01]   | `filters.Condition` | type-alias    | `Callable[[CallDetailsT], bool]` predicate over call |

[PUBLIC_TYPE_SCOPE]: grpcext interceptor contracts
- `grpcext.intercept_channel` wraps these sync-channel interceptor contracts; a custom span-enriching channel interceptor subclasses `UnaryClientInterceptor`/`StreamClientInterceptor` rather than re-deriving gRPC's own interceptor protocol.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `grpcext.UnaryClientInterceptor`  | abstract      | `intercept_unary(request, metadata, client_info, invoker)`                      |
|  [02]   | `grpcext.StreamClientInterceptor` | abstract      | `intercept_stream(request_or_iterator, metadata, client_info, invoker)`         |
|  [03]   | `grpcext.UnaryClientInfo`         | value         | unary `full_method`/`timeout` carrier                                           |
|  [04]   | `grpcext.StreamClientInfo`        | value         | streaming `full_method`/`is_client_stream`/`is_server_stream`/`timeout` carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `GrpcInstrumentorClient(filter_=None)`              | ctor     | sync client instrumentor                         |
|  [02]   | `GrpcInstrumentorServer(filter_=None)`              | ctor     | sync server instrumentor                         |
|  [03]   | `GrpcAioInstrumentorClient(filter_=None)`           | ctor     | async client instrumentor                        |
|  [04]   | `GrpcAioInstrumentorServer(filter_=None)`           | ctor     | async server instrumentor                        |
|  [05]   | `instrument(**kwargs)`                              | instance | patch gRPC; forwards `tracer_provider` and hooks |
|  [06]   | `uninstrument(**kwargs)`                            | instance | unpatch gRPC                                     |
|  [07]   | `instrumentation_dependencies() -> Collection[str]` | instance | declares the `grpcio` runtime dependency         |

[ENTRYPOINT_SCOPE]: interceptor factories
- every factory carries `(tracer_provider=None, filter_=None)`; the client factories add `request_hook=None, response_hook=None`.

| [INDEX] | [SURFACE]                                           | [SHAPE]      | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------- | :----------- | :--------------------------------------------------------- |
|  [01]   | `client_interceptor(...)`                           | factory      | `grpcext` unary+stream interceptor for `intercept_channel` |
|  [02]   | `server_interceptor(...)`                           | factory      | `grpc.ServerInterceptor` for `grpc.server`                 |
|  [03]   | `aio_client_interceptors(...)`                      | factory      | four `grpc.aio` interceptors: unary/stream × unary/stream  |
|  [04]   | `aio_server_interceptor(...)`                       | factory      | `grpc.aio.ServerInterceptor` for `grpc.aio.server`         |
|  [05]   | `grpcext.intercept_channel(channel, *interceptors)` | channel wrap | wrap an existing sync channel, returning it wrapped        |

[ENTRYPOINT_SCOPE]: filter predicates

| [INDEX] | [SURFACE]                                          | [SHAPE]    | [CAPABILITY]                         |
| :-----: | :------------------------------------------------- | :--------- | :----------------------------------- |
|  [01]   | `filters.method_name(name: str) -> Condition`      | predicate  | match exact RPC method name          |
|  [02]   | `filters.method_prefix(prefix: str) -> Condition`  | predicate  | match RPC method by prefix           |
|  [03]   | `filters.full_method_name(name: str) -> Condition` | predicate  | match `/package.Service/Method` path |
|  [04]   | `filters.service_name(name: str) -> Condition`     | predicate  | match exact service name             |
|  [05]   | `filters.service_prefix(prefix: str) -> Condition` | predicate  | match service by prefix              |
|  [06]   | `filters.health_check() -> Condition`              | predicate  | match gRPC health-check RPCs         |
|  [07]   | `filters.all_of(*conditions) -> Condition`         | combinator | conjunction of predicates            |
|  [08]   | `filters.any_of(*conditions) -> Condition`         | combinator | disjunction of predicates            |
|  [09]   | `filters.negate(condition) -> Condition`           | combinator | logical negation of a predicate      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- four `BaseInstrumentor` subclasses monkeypatch gRPC at `instrument()` and revert at `uninstrument()`; one instrumentor per leg, activated once at the composition root, never per request.
- an explicit serve leg passes `aio_server_interceptor(tracer_provider, filter_)` into `grpc.aio.server(interceptors=[...])` over global patching; the async server is the runtime default, the sync legs cover blocking call sites.
- trace selection is a composed `filters` predicate typed `Condition[CallDetailsT]`; `negate(health_check())` suppresses noisy liveness RPCs, and the four instrumentors fold `OTEL_PYTHON_GRPC_EXCLUDED_SERVICES` into `filter_` at construction via `any_of(filter_, excluded_service_filter)`, composing deployment exclusions with the code-declared predicate.
- `request_hook`/`response_hook` enrich client spans only and never block; `response_hook(span, payload)` receives the awaited unary status detail (`call.details()`, empty on OK) on the unary legs and each streamed message on the streaming legs, while the interceptor itself sets `rpc.grpc.status_code`, never passed to the hook.

[STACKING]:
- `grpcio`(`.api/grpcio.md`): `aio_server_interceptor(tracer_provider, filter_)` threads into `grpc.aio.server(interceptors=[...])`, `aio_client_interceptors(...)` is the four-element list into `grpc.aio.insecure_channel(target, interceptors=[...])`; the sync legs use `grpc.server(interceptors=[server_interceptor(...)])` and `grpcext.intercept_channel(grpc.insecure_channel(target), client_interceptor(...))` — one interceptor object per channel/server, never per-call.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `tracer_provider` and the `request_hook`/`response_hook` span objects are the `Tracer`/`Span`; the interceptor sets `rpc.system`/`rpc.service`/`rpc.method`/`rpc.grpc.status_code` semantic-convention attributes and propagates context across the wire. SDK provider install is the composition-root concern, never inside this row.
- within-lib: one filter predicate (`negate(any_of(health_check(), service_prefix("grpc.reflection")))`) shared by the matched client and server interceptors under one `tracer_provider`, with a `response_hook` recording the unary status detail as a span attribute.

[LOCAL_ADMISSION]:
- serve legs admit `aio_server_interceptor` on their `grpc.aio` server; `GrpcAioInstrumentorServer` is the global fallback when the server is constructed outside runtime control.
- spans, attributes, and propagation arrive settled from `libs/python/.api/opentelemetry-api.md`; this page owns only the gRPC interceptor and filter surface.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-grpc`
- Owns: OTel span emission around the gRPC client/server legs via interceptors and a filter predicate algebra
- Accept: `aio_server_interceptor`/`aio_client_interceptors` on `grpc.aio.server`/`grpc.aio.insecure_channel`, `server_interceptor`/`client_interceptor` + `grpcext.intercept_channel` on the sync legs, instrumentor `instrument()` at the composition root, composed `filters.Condition` predicates, `OTEL_PYTHON_GRPC_EXCLUDED_SERVICES` exclusions, request/response hooks for span enrichment
- Reject: hand-rolled `grpc.aio.ServerInterceptor` subclasses for tracing, per-request instrumentor activation, SDK provider construction in library code, unfiltered health-check RPC noise, a parallel filter predicate type beside `filters.Condition`
