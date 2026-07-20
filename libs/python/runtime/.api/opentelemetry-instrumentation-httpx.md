# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_HTTPX]

`opentelemetry-instrumentation-httpx` supplies httpx client tracing: one `BaseInstrumentor` patching the sync and async clients so every request emits an HTTP client span carrying W3C trace context onto the wire, with sync and async request/response hooks for span enrichment. It is the train row that puts the transport-roots HTTP legs on the one distributed trace.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-httpx`
- package: `opentelemetry-instrumentation-httpx`
- module: `opentelemetry.instrumentation.httpx`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.httpx`
- installed: `0.64b0`
- capability: global `httpx.Client`/`httpx.AsyncClient` patching, per-client instrument/uninstrument, and the four-hook enrichment set — `request_hook(span, request)`, `response_hook(span, request, response)`, `async_request_hook`, `async_response_hook`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                     |
| :-----: | :----------------------- | :------------ | :------------------------------------------ |
|  [01]   | `HTTPXClientInstrumentor` | instrumentor  | sync + async httpx client spans, propagation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability
- `instrument` kwargs: `tracer_provider`, `request_hook`, `response_hook`, `async_request_hook`, `async_response_hook`.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `HTTPXClientInstrumentor().instrument(**kwargs)`     | enable         | patch both client classes                     |
|  [02]   | `HTTPXClientInstrumentor().uninstrument(**kwargs)`   | disable        | unwrap both client classes                    |
|  [03]   | `HTTPXClientInstrumentor.instrument_client(client, ...)` | client     | instrument one built `Client`/`AsyncClient`   |
|  [04]   | `HTTPXClientInstrumentor.uninstrument_client(client)` | client        | strip one built client                        |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- activation law: one `instrument()` at the composition root — the runtime metrics `Instrumentation.install` train row; the pooled `transport/roots` clients built before the train re-enter through `instrument_client`.
- hook law: hooks run inside the active span scope and enrich only — the sync pair serves `Client`, the async pair `AsyncClient`, and a hook never blocks or raises into the request path.
- propagation law: the patched send injects the W3C composite the telemetry root registered, so the httpx legs join the one distributed trace with no carrier code.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-httpx`
- Owns: HTTP client spans and context injection on every httpx request
- Accept: one train-row `instrument()` at the composition root, `instrument_client` for pre-train clients, the four hooks for span enrichment
- Reject: activation inside a library module, hand-rolled `traceparent` header writes on an httpx leg, a second HTTP-client instrumentation over the same clients
