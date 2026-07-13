# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_GRPC]

`opentelemetry-instrumentation-grpc` supplies the gRPC tracing instrumentation: four `BaseInstrumentor` subclasses for sync/async client and server legs, standalone interceptor factories for explicit channel and server wiring, and a `filters` predicate algebra that selects which RPCs are traced by method, service, or health-check shape. It is the runtime observability row that emits `CLIENT`/`SERVER` spans around the `grpc.aio` serve leg without hand-rolled interceptors.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-grpc`
- package: `opentelemetry-instrumentation-grpc`
- module: `opentelemetry.instrumentation.grpc`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.grpc`, `opentelemetry.instrumentation.grpc.filters`, `opentelemetry.instrumentation.grpc.grpcext`
- installed: `0.64b0`
- capability: sync/async client + server gRPC span instrumentation, standalone interceptor factories, a parameterized `filters.Condition` predicate algebra for trace selection, the `grpcext` legacy-channel interceptor contracts (`UnaryClientInterceptor`/`StreamClientInterceptor`/`UnaryClientInfo`/`StreamClientInfo`), `grpcext.intercept_channel` for wrapping an existing channel, and `OTEL_PYTHON_GRPC_EXCLUDED_SERVICES` deployment-time service exclusion

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor family
- rail: observability

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `GrpcInstrumentorClient`    | instrumentor  | sync `grpc` client channel patching     |
|  [02]   | `GrpcInstrumentorServer`    | instrumentor  | sync `grpc` server interceptor patching |
|  [03]   | `GrpcAioInstrumentorClient` | instrumentor  | async `grpc.aio` client patching        |
|  [04]   | `GrpcAioInstrumentorServer` | instrumentor  | async `grpc.aio` server patching        |

[PUBLIC_TYPE_SCOPE]: filter predicate algebra
- rail: observability
- `filters.Condition[CallDetailsT] = Callable[[CallDetailsT], bool]` — the parameterized predicate type every `method_*`/`service_*`/`health_check`/combinator returns and the `filter_=` kwarg accepts; the interceptor evaluates it against the per-call details object.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :------------------ | :------------ | :--------------------------------------------------- |
|  [01]   | `filters.Condition` | type alias    | `Callable[[CallDetailsT], bool]` predicate over call |

[PUBLIC_TYPE_SCOPE]: grpcext interceptor contracts
- rail: observability
- the legacy-channel interceptor abstractions `grpcext.intercept_channel` wraps; the OTel client interceptor implements `UnaryClientInterceptor`/`StreamClientInterceptor`, and a custom span-enriching channel interceptor subclasses these rather than re-deriving gRPC's own interceptor protocol.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                                          |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `grpcext.UnaryClientInterceptor`  | abstract base | `intercept_unary(request, metadata, client_info, invoker)`                      |
|  [02]   | `grpcext.StreamClientInterceptor` | abstract base | `intercept_stream(request_or_iterator, metadata, client_info, invoker)`         |
|  [03]   | `grpcext.UnaryClientInfo`         | call details  | unary `full_method`/`timeout` carrier                                           |
|  [04]   | `grpcext.StreamClientInfo`        | call details  | streaming `full_method`/`is_client_stream`/`is_server_stream`/`timeout` carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :-------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `GrpcInstrumentorClient(filter_=None)`              | construct      | sync client instrumentor                         |
|  [02]   | `GrpcInstrumentorServer(filter_=None)`              | construct      | sync server instrumentor                         |
|  [03]   | `GrpcAioInstrumentorClient(filter_=None)`           | construct      | async client instrumentor                        |
|  [04]   | `GrpcAioInstrumentorServer(filter_=None)`           | construct      | async server instrumentor                        |
|  [05]   | `instrument(**kwargs)`                              | enable         | patch gRPC; forwards `tracer_provider` and hooks |
|  [06]   | `uninstrument(**kwargs)`                            | disable        | unpatch gRPC                                     |
|  [07]   | `instrumentation_dependencies() -> Collection[str]` | dependency     | declares `grpcio >= 1.42.0`                      |

[ENTRYPOINT_SCOPE]: interceptor factories
- rail: observability
- every factory takes `(tracer_provider=None, filter_=None)`; the client factories add `request_hook=None, response_hook=None`. Wiring targets are the `INTEGRATION_STACK` grpcio leg.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `client_interceptor(...)`                           | sync client    | `grpcext` unary+stream interceptor for `intercept_channel` |
|  [02]   | `server_interceptor(...)`                           | sync server    | `grpc.ServerInterceptor` for `grpc.server`                 |
|  [03]   | `aio_client_interceptors(...)`                      | async client   | four `grpc.aio` interceptors: unary/stream × unary/stream  |
|  [04]   | `aio_server_interceptor(...)`                       | async server   | `grpc.aio.ServerInterceptor` for `grpc.aio.server`         |
|  [05]   | `grpcext.intercept_channel(channel, *interceptors)` | channel wrap   | wrap an existing sync channel, returning it wrapped        |

[ENTRYPOINT_SCOPE]: filter predicates
- rail: observability

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `filters.method_name(name: str) -> Condition`      | predicate      | match exact RPC method name          |
|  [02]   | `filters.method_prefix(prefix: str) -> Condition`  | predicate      | match RPC method by prefix           |
|  [03]   | `filters.full_method_name(name: str) -> Condition` | predicate      | match `/package.Service/Method` path |
|  [04]   | `filters.service_name(name: str) -> Condition`     | predicate      | match exact service name             |
|  [05]   | `filters.service_prefix(prefix: str) -> Condition` | predicate      | match service by prefix              |
|  [06]   | `filters.health_check() -> Condition`              | predicate      | match gRPC health-check RPCs         |
|  [07]   | `filters.all_of(*conditions) -> Condition`         | combinator     | conjunction of predicates            |
|  [08]   | `filters.any_of(*conditions) -> Condition`         | combinator     | disjunction of predicates            |
|  [09]   | `filters.negate(condition) -> Condition`           | combinator     | logical negation of a predicate      |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- instrumentor model: the four `BaseInstrumentor` subclasses monkeypatch gRPC at `instrument()` and revert at `uninstrument()`; one instrumentor instance per leg, activated once at the composition root, never per request.
- explicit wiring: when the serve leg builds its own server, pass `aio_server_interceptor(tracer_provider, filter_)` into `grpc.aio.server(interceptors=[...])` instead of global patching; the async server leg is the runtime default, the sync legs cover blocking call sites.
- provider law: `tracer_provider` defaults to the global OTel provider resolved through `opentelemetry-api`; the SDK provider is installed at startup, never inside library code.
- filter law: trace selection is a composed `filters` predicate (`all_of`/`any_of`/`negate` over `method_*`/`service_*`/`health_check`) typed as `Condition[CallDetailsT]`; `negate(health_check())` is the canonical way to suppress noisy liveness RPCs. The four instrumentors auto-fold `OTEL_PYTHON_GRPC_EXCLUDED_SERVICES` (comma-separated service names) into the `filter_` at construction via `any_of(filter_, excluded_service_filter)`, so deployment-time exclusions compose with the code-declared predicate rather than overriding it.
- hook law: `request_hook`/`response_hook` enrich client spans only and run inside the active span scope, never blocking; `request_hook(span, request)` receives the outbound message, while `response_hook(span, payload)` receives the awaited unary status-details string (`call.details()`, empty on OK) on the unary legs and the streamed response message per yield on the streaming legs — the gRPC status code is set by the interceptor itself as the `rpc.grpc.status_code` attribute, not passed to the hook.

[INTEGRATION_STACK]:
- grpcio leg: `aio_server_interceptor(tracer_provider, filter_)` threads into the `grpc.aio.server(interceptors=[...])` built from `.api/grpcio.md`; `aio_client_interceptors(...)` is the four-element list passed to `grpc.aio.insecure_channel(target, interceptors=[...])`. The sync legs use `grpc.server(interceptors=[server_interceptor(...)])` and `grpcext.intercept_channel(grpc.insecure_channel(target), client_interceptor(...))`. One interceptor object per channel/server, never per-call.
- provider leg: `tracer_provider` and the `request_hook`/`response_hook` span objects are the `Tracer`/`Span` from the branch-tier `libs/python/.api/opentelemetry-api.md`; the interceptor sets `rpc.system`/`rpc.service`/`rpc.method`/`rpc.grpc.status_code` semantic-convention attributes and propagates context across the wire automatically. The SDK provider install (`TracerProvider`, span processors, OTLP exporter) is the composition-root concern, never inside this row.
- single rail: the dense form is one filter predicate (`negate(any_of(health_check(), service_prefix("grpc.reflection")))`) shared by the matched client and server interceptors wired into the `grpcio` serve and dial legs under one OTel `tracer_provider`, with a `response_hook` recording the unary status detail as a span attribute — never a hand-rolled `grpc.aio.ServerInterceptor` subclass duplicating span emission.

[LOCAL_ADMISSION]:
- the serve leg admits `aio_server_interceptor` on its `grpc.aio` server; `GrpcAioInstrumentorServer` is the global fallback when the server is constructed outside runtime control.
- spans, attributes, and propagation arrive settled from the branch-tier `libs/python/.api/opentelemetry-api.md`; this page owns only the gRPC interceptor and filter surface.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-grpc`
- Owns: OTel span emission around the gRPC client/server legs via interceptors and a filter predicate algebra
- Accept: `aio_server_interceptor`/`aio_client_interceptors` wired into `grpc.aio.server`/`grpc.aio.insecure_channel`, `server_interceptor`/`client_interceptor` + `grpcext.intercept_channel` on the sync legs, instrumentor `instrument()` at the composition root, composed `filters.Condition` predicates for trace selection, `OTEL_PYTHON_GRPC_EXCLUDED_SERVICES` deployment exclusions, request/response hooks for span enrichment
- Reject: hand-rolled `grpc.aio.ServerInterceptor` subclasses for tracing, per-request instrumentor activation, SDK provider construction in library code, untraced health-check RPC noise left unfiltered, a parallel filter predicate type beside `filters.Condition`
