# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_HTTPX]

`opentelemetry-instrumentation-httpx` traces the httpx client legs: one `HTTPXClientInstrumentor` patches the sync and async clients at the transport seam so every request emits an HTTP client span and injects W3C trace context onto the wire, with sync and async request/response hooks enriching the active span.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-httpx`
- package: `opentelemetry-instrumentation-httpx`
- module: `opentelemetry.instrumentation.httpx`
- namespaces: `opentelemetry.instrumentation.httpx`
- rail: observability
- abi: pure-Python runtime library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------ | :------------ | :------------------------------------------- |
|  [01]   | `HTTPXClientInstrumentor` | instrumentor  | sync + async httpx client spans, propagation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- `instrument` kwargs: `tracer_provider`, `request_hook`, `response_hook`, `async_request_hook`, `async_response_hook`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `HTTPXClientInstrumentor().instrument(**kwargs)`         | instance | patch both client classes                   |
|  [02]   | `HTTPXClientInstrumentor().uninstrument(**kwargs)`       | instance | unwrap both client classes                  |
|  [03]   | `HTTPXClientInstrumentor.instrument_client(client, ...)` | static   | instrument one built `Client`/`AsyncClient` |
|  [04]   | `HTTPXClientInstrumentor.uninstrument_client(client)`    | static   | strip one built client                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- activation: one `instrument()` at the composition root patches both client classes; pooled `transport/roots` clients built before activation re-enter through `instrument_client`.
- hooks: the sync pair enriches `Client`, the async pair `AsyncClient`, each running inside the active span scope and never blocking or raising into the request path.
- propagation: the patched send injects the W3C composite the telemetry root registered, so the httpx legs join the one distributed trace with no carrier code.

[STACKING]:
- `httpx`(`.api/httpx.md`): `HTTPXClientInstrumentor().instrument()` swaps each client's transport for `AsyncOpenTelemetryTransport`/`SyncOpenTelemetryTransport`, or `instrument_client` wraps one pre-built `Client`/`AsyncClient`, emitting a client span around every `AsyncClient.request`/`send`.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): the patched send calls `propagate.inject` with the `set_global_textmap` W3C composite (`TraceContextTextMapPropagator`/`W3CBaggagePropagator`), stamping `traceparent`/`tracestate` onto the outbound headers.

[LOCAL_ADMISSION]:
- one `instrument()` at the composition root; pre-built `transport/roots` clients re-enter via `instrument_client`, and no second HTTP-client instrumentation binds the same clients.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-httpx`
- Owns: HTTP client spans and W3C context injection on every httpx request
- Accept: one `instrument()` at the composition root, `instrument_client` for pre-built clients, sync/async request/response hooks for span enrichment
- Reject: activation inside a library module, hand-rolled `traceparent` writes on an httpx leg, a second HTTP-client instrumentation over the same clients
