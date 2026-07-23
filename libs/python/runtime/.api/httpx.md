# [PY_RUNTIME_API_HTTPX]

`httpx` owns outbound HTTP transport for the Python branch: one pooled `AsyncClient`/`Client` negotiating HTTP/1.1 and HTTP/2, a generator-based `Auth` signing protocol, request and response streaming, per-phase `Timeout` and pool `Limits`, `event_hooks`, transport and proxy injection, the `codes` status vocabulary, and the full request/transport error taxonomy. It is the branch's sole HTTP client; responses feed the wire-model decode rail and transient faults the resilience rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `httpx`
- package: `httpx` (BSD-3-Clause)
- module: `httpx`
- namespaces: `httpx`
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and message family

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `AsyncClient`        | client        | pooled async HTTP client (the canonical surface)        |
|  [02]   | `Client`             | client        | pooled sync HTTP client (boundary scripts only)         |
|  [03]   | `Request`            | message       | outbound request (`build_request` product)              |
|  [04]   | `Response`           | message       | inbound response with decode/stream/status surface      |
|  [05]   | `URL`                | value         | parsed/immutable URL with `.copy_with`/`.join`          |
|  [06]   | `QueryParams`        | value         | immutable multidict query-string map                    |
|  [07]   | `Headers`            | value         | case-insensitive header multidict                       |
|  [08]   | `Cookies`            | value         | cookie jar (read/set across requests)                   |
|  [09]   | `Timeout`            | config        | per-phase timeout (`connect`/`read`/`write`/`pool`)     |
|  [10]   | `Limits`             | config        | pool limits (`max_connections`/`max_keepalive_*`)       |
|  [11]   | `Proxy`              | config        | proxy spec (url + auth + headers)                       |
|  [12]   | `codes`              | enum          | `httpx.codes` HTTP status-code vocabulary (`OK`, `...`) |
|  [13]   | `USE_CLIENT_DEFAULT` | sentinel      | per-call sentinel to defer to client-level config       |

[PUBLIC_TYPE_SCOPE]: auth and transport family

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Auth`                                              | auth base     | generator-based request-signing flow (`auth_flow`)                 |
|  [02]   | `BasicAuth`                                         | auth          | HTTP basic auth                                                    |
|  [03]   | `DigestAuth`                                        | auth          | HTTP digest challenge/response auth                                |
|  [04]   | `NetRCAuth`                                         | auth          | netrc-file credential lookup                                       |
|  [05]   | `AsyncBaseTransport`                                | transport     | async transport ABC (`handle_async_request`)                       |
|  [06]   | `BaseTransport`                                     | transport     | sync transport ABC (`handle_request`)                              |
|  [07]   | `AsyncHTTPTransport`                                | transport     | default httpcore-backed async transport (retries, proxy, h2)       |
|  [08]   | `HTTPTransport`                                     | transport     | default httpcore-backed sync transport                             |
|  [09]   | `ASGITransport`                                     | transport     | in-process ASGI app transport (server-side checks)                 |
|  [10]   | `WSGITransport`                                     | transport     | in-process WSGI app transport                                      |
|  [11]   | `MockTransport`                                     | transport     | handler-driven test transport                                      |
|  [12]   | `AsyncByteStream` / `SyncByteStream` / `ByteStream` | stream        | request/response body stream protocol (custom-transport authoring) |

[PUBLIC_TYPE_SCOPE]: fault family (full taxonomy)

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `HTTPError`                                                              | fault base    | base for `RequestError` + `HTTPStatusError` |
|  [02]   | `RequestError`                                                           | fault base    | request-side failure base; has `.request`   |
|  [03]   | `TransportError`                                                         | fault base    | transport-layer failure base                |
|  [04]   | `TimeoutException`                                                       | fault base    | base for all phase-timeout faults           |
|  [05]   | `ConnectTimeout` / `ReadTimeout` / `WriteTimeout`                        | fault         | per-phase timeout                           |
|  [06]   | `PoolTimeout`                                                            | fault         | pool-acquire timeout (pool exhausted)       |
|  [07]   | `NetworkError`                                                           | fault base    | base for network faults                     |
|  [08]   | `ConnectError` / `ReadError` / `WriteError` / `CloseError`               | fault         | concrete network faults                     |
|  [09]   | `ProtocolError` / `LocalProtocolError` / `RemoteProtocolError`           | fault         | HTTP framing violations                     |
|  [10]   | `ProxyError`                                                             | fault         | proxy-tunnel failure                        |
|  [11]   | `UnsupportedProtocol`                                                    | fault         | scheme has no mounted transport             |
|  [12]   | `DecodingError`                                                          | fault         | content-decode (gzip/br/zstd) failure       |
|  [13]   | `TooManyRedirects`                                                       | fault         | redirect-limit exceeded                     |
|  [14]   | `HTTPStatusError`                                                        | fault         | from `raise_for_status`; has `.response`    |
|  [15]   | `InvalidURL`                                                             | fault         | malformed URL                               |
|  [16]   | `CookieConflict`                                                         | fault         | ambiguous cookie lookup                     |
|  [17]   | `StreamError`                                                            | fault base    | base for stream-state faults                |
|  [18]   | `StreamConsumed` / `StreamClosed` / `ResponseNotRead` / `RequestNotRead` | fault         | stream lifecycle misuse                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- `AsyncClient` carry: `auth`, `headers`, `params`, `cookies`, `verify`, `cert`, `http1`, `http2`, `proxy`, `mounts`, `timeout`, `follow_redirects`, `limits`, `max_redirects`, `event_hooks`, `base_url`, `transport`, `trust_env`
- `request` carry: `content`, `data`, `files`, `json`, `params`, `headers`, `cookies`, `auth`, `follow_redirects`, `timeout`, `extensions`; `Client` mirrors every method synchronously

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `AsyncClient(*, ...)`                                                  | build   | construct a pooled client with full policy     |
|  [02]   | `AsyncClient.request(method, url, *, ...)`                             | send    | any-method request (polymorphic entry)         |
|  [03]   | `AsyncClient.get / .options / .head / .post / .put / .patch / .delete` | send    | method-specialized requests (same kwargs)      |
|  [04]   | `AsyncClient.build_request(method, url, ...)`                          | build   | materialize a `Request` without sending        |
|  [05]   | `AsyncClient.send(request, *, stream=False, auth, follow_redirects)`   | send    | send a prebuilt `Request` (auth-flow re-entry) |
|  [06]   | `AsyncClient.stream(method, url, ...)`                                 | stream  | `async with` streaming context                 |
|  [07]   | `AsyncClient.aclose` / `Client.close`                                  | drain   | close the connection pool                      |

[ENTRYPOINT_SCOPE]: response decode and streaming
- Defined on `Response`; the `Response.` receiver is elided below.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]    | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------- | :--------- | :----------------------------------- |
|  [01]   | `.raise_for_status() -> Response`                                              | check      | raise `HTTPStatusError` on 4xx/5xx   |
|  [02]   | `.json(**kwargs)`                                                              | decode     | JSON body; msgspec/pydantic handoff  |
|  [03]   | `.aread` / `.read`                                                             | decode     | buffer the full body                 |
|  [04]   | `.aiter_bytes` / `.aiter_text` / `.aiter_lines` / `.aiter_raw`                 | stream     | chunked async iter (raw = undecoded) |
|  [05]   | `.is_success / .is_error / .is_redirect / .is_client_error / .is_server_error` | check      | status-class predicates              |
|  [06]   | `.elapsed / .http_version / .num_bytes_downloaded / .encoding / .links`        | introspect | timing, protocol, link header        |

[ENTRYPOINT_SCOPE]: module-level helpers (boundary/one-shot only)

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `httpx.request` / `.get` / `.post` / `.stream`      | one-shot | module-level single-call (constructs a throwaway client) |
|  [02]   | `httpx.create_ssl_context(verify, cert, trust_env)` | tls      | build the SSL context the transport consumes             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One long-lived `AsyncClient` per endpoint group carries explicit `Timeout(connect=, read=, write=, pool=)`, `Limits`, `Auth`, and `base_url`, then is reused; `Client` mirrors it synchronously and per-request or module-level (`httpx.get`) construction stays in boundary scripts.
- `http2=True` binds where the upstream negotiates HTTP/2, collapsing the pool through h2 multiplexing; `Response.http_version` confirms the negotiated wire.
- Credentials sign through an `Auth` subclass — `BasicAuth`/`DigestAuth`/`NetRCAuth` or a custom `auth_flow` generator — bound at construction; the flow owns every challenge leg, and `USE_CLIENT_DEFAULT` is the per-call deferral sentinel distinct from `None`.
- Large bodies stream through `AsyncClient.stream` with `aiter_bytes`/`aiter_lines`; `aiter_raw` yields undecoded bytes for content-encoding passthrough, and full-body `aread`/`json()` serves small payloads only.
- Cross-cutting observation binds `event_hooks={"request": [...], "response": [...]}` at construction, where `raise_for_status` and span enrichment hang.
- Transport is injected: `MockTransport(handler)` for tests, `ASGITransport(app=)`/`WSGITransport(app=)` for in-process app checks, `mounts={scheme: transport}` for per-scheme routing, `AsyncHTTPTransport(retries=, proxy=)` in production.
- `aclose`/`close` drains the pool under the host drain choreography.

[STACKING]:
- `msgspec`(`.api/msgspec.md`) / `pydantic`(`.api/pydantic.md`): `Response.json()` yields the `dict`/`list` that `msgspec.convert(obj, type)` or a pydantic discriminated union decodes to a typed model — one rail, no intermediate re-parse.
- `stamina`(`.api/stamina.md`): `stamina.retry_context`/`AsyncRetryingCaller` wraps `AsyncClient.send` for transient `TransportError`/`TimeoutException`/`PoolTimeout`, while `AsyncHTTPTransport(retries=)` covers only connection-establishment retries; `HTTPStatusError` is non-transient and surfaces as `Error(BoundaryFault(...))`.
- `hishel`(`.api/hishel.md`): `AsyncCacheTransport(next_transport=AsyncHTTPTransport(...))` mounts as the client `transport`, caching above the pool while `Timeout`/`Limits`/`Auth` stay owned here.
- `pydantic-settings`(`.api/pydantic-settings.md`): a `BaseSettings` model mints the `BasicAuth`/custom-`Auth` instance once at construction; credentials never appear inline.
- `opentelemetry-instrumentation-httpx`(`.api/opentelemetry-instrumentation-httpx.md`): `HTTPXClientInstrumentor().instrument()` patches both client classes at the composition root, and pooled clients built earlier re-enter through `instrument_client`, so spans ride the transport seam.
- within-lib: the `transport/roots` and `serve` owners build and reuse the pooled clients; the `anyio` deadline scope wraps each call so cancellation propagates into `httpcore`, and `aclose` joins the anyio drain lane.

[LOCAL_ADMISSION]:
- `AsyncClient` is the sole outbound surface for calls and credential probes; the runtime holds no second HTTP client and no client-per-auth-mode.
- Boundary decode and status checks lift faults to a `Result`; domain logic never receives a raw `Response` error.

[RAIL_LAW]:
- Package: `httpx`
- Owns: async/sync HTTP/1.1 and HTTP/2 transport, connection pooling, the `Auth` flow protocol, streaming bodies, per-phase timeouts and pool limits, event hooks, transport/proxy injection, the `codes` status enum, and the full error taxonomy
- Accept: one reused `AsyncClient` with explicit `Timeout`/`Limits`/`Auth`/`base_url`, `http2` where negotiated, `stream`+`aiter_*` for large bodies, `event_hooks`, injected `MockTransport`/`ASGITransport`/`mounts`, `stamina`-retried transient faults, `Response.json()` handed to the wire-model decoder, the settled OTel httpx spans
- Reject: per-request or module-level client construction in service code, bare-float or implicit timeouts, full-body reads of large payloads, transport monkeypatching, hand-rolled auth-challenge or retry ladders, re-parsing `json()` output, a second HTTP client, stdlib `http.client`/`urllib`/`requests`/`urllib3`
