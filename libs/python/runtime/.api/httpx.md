# [PY_RUNTIME_API_HTTPX]

`httpx` supplies the async/sync HTTP client surface: pooled `AsyncClient`/`Client` with HTTP/releaseHTTP/2 negotiation, an `Auth` flow protocol, request/response streaming, per-phase `Timeout` and pool `Limits`, `event_hooks`, transport/proxy `mounts` injection, an `httpx.codes` status enum, and a full request/transport error taxonomy. It is the runtime owner for outbound HTTP transport and its credential auth lane — the custom bearer `auth_flow` (the realized `_BearerAuth`, since httpx ships no bearer `Auth`) plus the `BasicAuth` forward `CREDENTIAL`-shape row seamed at `SecretBoundary.resolve`, the unconsumed `NetRCAuth`/`DigestAuth` family outside the runtime slice — and the only admitted HTTP client (stdlib `http.client`/`urllib`/`requests`/`urllib3` are banned at the import boundary).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `httpx`
- package: `httpx`
- import: `httpx`
- owner: `runtime`
- rail: transport
- version: `0.28.1`
- license: `BSD-3-Clause`
- namespaces: `httpx`
- capability: pooled async/sync HTTP/releaseHTTP/2 client, `Auth` flow protocol, request/response streaming, per-phase timeouts and pool limits, event hooks, transport/proxy injection, `codes` status enum, full request/transport error taxonomy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and message family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `AsyncClient` | client | pooled async HTTP client (the canonical surface) |
| [02] | `Client` | client | pooled sync HTTP client (boundary scripts only) |
| [03] | `Request` | message | outbound request (`build_request` product) |
| [04] | `Response` | message | inbound response with decode/stream/status surface |
| [05] | `URL` | value | parsed/immutable URL with `.copy_with`/`.join` |
| [06] | `QueryParams` | value | immutable multidict query-string map |
| [07] | `Headers` | value | case-insensitive header multidict |
| [08] | `Cookies` | value | cookie jar (read/set across requests) |
| [09] | `Timeout` | config | per-phase timeout (`connect`/`read`/`write`/`pool`) |
| [10] | `Limits` | config | pool limits (`max_connections`/`max_keepalive_*`) |
| [11] | `Proxy` | config | proxy spec (url + auth + headers) |
| [12] | `codes` | enum | `httpx.codes` HTTP status-code vocabulary (`OK`, `...`) |
| [13] | `USE_CLIENT_DEFAULT` | sentinel | per-call sentinel to defer to client-level config |

[PUBLIC_TYPE_SCOPE]: auth and transport family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `Auth` | auth base | generator-based request-signing flow (`auth_flow`) |
| [02] | `BasicAuth` | auth | HTTP basic auth |
| [03] | `AsyncBaseTransport` | transport | async transport ABC (`handle_async_request`) |
| [04] | `BaseTransport` | transport | sync transport ABC (`handle_request`) |
| [05] | `AsyncHTTPTransport` | transport | default httpcore-backed async transport (retries, proxy, h2) |
| [06] | `HTTPTransport` | transport | default httpcore-backed sync transport |
| [07] | `ASGITransport` | transport | in-process ASGI app transport (server-side checks) |
| [08] | `WSGITransport` | transport | in-process WSGI app transport |
| [09] | `MockTransport` | transport | handler-driven test transport |
| [10] | `AsyncByteStream` / `SyncByteStream` / `ByteStream` | stream | request/response body stream protocol (custom-transport authoring) |

[PUBLIC_TYPE_SCOPE]: fault family (full taxonomy)
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `HTTPError` | fault base | base for `RequestError` + `HTTPStatusError` |
| [02] | `RequestError` | fault base | request-side failure base (carries `.request`) |
| [03] | `TransportError` | fault base | transport-layer failure base |
| [04] | `TimeoutException` | fault base | base for all phase-timeout faults |
| [05] | `ConnectTimeout` / `ReadTimeout` / `WriteTimeout` | fault | per-phase timeout |
| [06] | `PoolTimeout` | fault | pool-acquire timeout (pool exhausted) |
| [07] | `NetworkError` | fault base | base for connect/read/write/close network faults |
| [08] | `ConnectError` / `ReadError` / `WriteError` / `CloseError` | fault | concrete network faults |
| [09] | `ProtocolError` / `LocalProtocolError` / `RemoteProtocolError` | fault | HTTP framing violations |
| [10] | `ProxyError` | fault | proxy-tunnel failure |
| [11] | `UnsupportedProtocol` | fault | scheme has no mounted transport |
| [12] | `DecodingError` | fault | content-decode (gzip/br/zstd) failure |
| [13] | `TooManyRedirects` | fault | redirect-limit exceeded |
| [14] | `HTTPStatusError` | fault | raised by `raise_for_status` (carries `.response`) |
| [15] | `InvalidURL` | fault | malformed URL |
| [16] | `CookieConflict` | fault | ambiguous cookie lookup |
| [17] | `StreamError` | fault base | base for stream-state faults |
| [18] | `StreamConsumed` / `StreamClosed` / `ResponseNotRead` / `RequestNotRead` | fault | stream lifecycle misuse |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- rail: transport
- defined on `AsyncClient` (PUBLIC_TYPES [01]); `Client` mirrors every method synchronously.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `AsyncClient(*, auth, headers, params, cookies, verify, cert, http1=True, http2=False, proxy, mounts, timeout, follow_redirects=False, limits, max_redirects, event_hooks, base_url, transport, trust_env)` | build | construct a pooled client with full policy |
| [02] | `AsyncClient.request(method, url, *, content, data, files, json, params, headers, cookies, auth, follow_redirects, timeout, extensions)` | send | issue any-method request (the polymorphic entry) |
| [03] | `AsyncClient.get` / `.options` / `.head` / `.post` / `.put` / `.patch` / `.delete` | send | method-specialized requests (same kwargs) |
| [04] | `AsyncClient.build_request(method, url, ...)` | build | materialize a `Request` without sending |
| [05] | `AsyncClient.send(request, *, stream=False, auth, follow_redirects)` | send | send a prebuilt `Request` (auth-flow re-entry) |
| [06] | `AsyncClient.stream(method, url, ...)` | stream | `async with` streaming request/response context |
| [07] | `AsyncClient.aclose` / `Client.close` | drain | close the connection pool |

[ENTRYPOINT_SCOPE]: response decode and streaming
- rail: transport
- defined on `Response` (PUBLIC_TYPES [04]).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `Response.raise_for_status() -> Response` | check | raise `HTTPStatusError` on 4xx/5xx, else self |
| [02] | `Response.json(**kwargs)` | decode | body to JSON (handoff point to msgspec/pydantic) |
| [03] | `Response.aread` / `.read` | decode | buffer the full body |
| [04] | `Response.aiter_bytes` / `.aiter_text` / `.aiter_lines` / `.aiter_raw` | stream | chunked async iteration (raw = undecoded) |
| [05] | `Response.is_success` / `.is_error` / `.is_redirect` / `.is_client_error` / `.is_server_error` | check | status-class predicates |
| [06] | `Response.elapsed` / `.http_version` / `.num_bytes_downloaded` / `.encoding` / `.links` | introspect | timing, negotiated protocol, link header |

[ENTRYPOINT_SCOPE]: module-level helpers (boundary/one-shot only)
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `httpx.request` / `.get` / `.post` / `.stream` | one-shot | module-level single-call (constructs a throwaway client) |
| [02] | `httpx.create_ssl_context(verify, cert, trust_env)` | tls | build the SSL context the transport consumes |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- client law: one long-lived `AsyncClient` per outbound endpoint group is constructed with explicit `Timeout`, `Limits`, `Auth`, and `base_url`, then reused; module-level `httpx.get`/`request` and per-request client construction are deleted from service code.
- protocol law: `http2=True` is set when the upstream negotiates HTTP/2 (h2 multiplexing collapses the pool); `Response.http_version` confirms the negotiated protocol.
- timeout law: every client carries an explicit `Timeout(connect=, read=, write=, pool=)`; a bare float is rejected in favor of the per-phase shape, and the anyio deadline scope wraps the call so cancellation propagates into `httpcore`.
- auth law: credentials flow through an `Auth` subclass (`BasicAuth` or a custom `auth_flow` generator — the realized `_BearerAuth`) bound at client construction from the `pydantic-settings` model; challenge legs are owned by the flow, never hand-rolled. `USE_CLIENT_DEFAULT` is the per-call deferral sentinel — never `None` to mean "client default".
- streaming law: large bodies use `AsyncClient.stream` + `aiter_bytes`/`aiter_lines`; `aiter_raw` reads undecoded for content-encoding passthrough. Full-body `aread`/`json` is reserved for small payloads.
- decode law: `Response.json()` is the handoff seam — its `dict`/`list` feeds the `msgspec.convert`/pydantic discriminated-union decoder, never re-parsed; the wire model owner converts, this owner transports.
- event-hook law: cross-cutting request/response observation wires `event_hooks={"request": [...], "response": [...]}` at construction; this is the seam where `raise_for_status` and span enrichment hang, not a per-call wrapper.
- transport injection law: tests inject `MockTransport(handler)`; in-process server checks use `ASGITransport(app=...)`; per-scheme routing uses `mounts={...: transport}`; production uses the default `AsyncHTTPTransport(retries=, proxy=)` — no monkeypatching.
- resilience law: transient transport faults (`ConnectError`, the `TimeoutException` family, `PoolTimeout`, `RemoteProtocolError`) are retried through the `stamina` owner's `retry_context`; `AsyncHTTPTransport(retries=)` covers connection-establishment retries only, while `stamina` owns request-level backoff. `HTTPStatusError` is non-transient and surfaces as `Error(BoundaryFault(...))`.
- drain law: `aclose` participates in the host drain choreography under the anyio lane; the pool is never left to GC.

[LOCAL_ADMISSION]:
- The transport surface composes `AsyncClient` for outbound calls and credential probes; the runtime owns no second HTTP client and no parallel client-per-auth-mode.
- Response decode and status checks lift faults at the boundary; domain logic receives a `Result`, never a raw `Response` error.
- OTel `opentelemetry-instrumentation-httpx` attaches client spans through the same transport seam; tracing is not a hand-rolled event hook.

[STACK_LAW]:
- `httpx.AsyncClient` -> `Response.json()` -> `msgspec.convert`/pydantic discriminated union -> typed domain model: one rail, no intermediate dict re-parse.
- `stamina.retry_context` wraps `AsyncClient.send` for transient `TransportError`/`TimeoutException`; the OTel span and the retry attempt count share the same context, never separate try/except ladders.
- `Auth` flow + `pydantic-settings` credential source: the settings model mints the `BasicAuth`/`_BearerAuth` instance once at client construction; secrets never appear inline.

[RAIL_LAW]:
- Package: `httpx`
- Owns: async/sync HTTP/releaseHTTP/2 transport, connection pooling, the `Auth` flow protocol, streaming bodies, per-phase timeouts and pool limits, event hooks, transport/proxy injection, the `codes` status enum, and the full error taxonomy
- Accept: one reused `AsyncClient`, explicit `Timeout`/`Limits`/`Auth`/`base_url`, `http2` where negotiated, `stream`+`aiter_*` for large bodies, `event_hooks`, injected `MockTransport`/`ASGITransport`/`mounts`, `stamina`-retried transient faults, `Response.json()` handed to the wire-model decoder, settled OTel httpx spans
- Reject: per-request or module-level (`httpx.get`) client construction in service code, bare-float/implicit timeouts, full-body reads of large payloads, transport monkeypatching, hand-rolled auth-challenge flows or retry ladders, re-parsing `json()` output, a second HTTP client, stdlib `http.client`/`urllib`/`requests`/`urllib3`
