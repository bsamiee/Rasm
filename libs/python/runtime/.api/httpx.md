# [PY_RUNTIME_API_HTTPX]

`httpx` supplies the async/sync HTTP client: `AsyncClient`/`Client` with connection pooling, an auth family, streaming request/response bodies, timeout and limit configuration, proxy/transport injection, and a full error taxonomy. It is the runtime owner for outbound HTTP transport and inbound-server credential checks over the companion seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `httpx`
- package: `httpx`
- import: `httpx`
- owner: `runtime`
- rail: transport
- namespaces: `httpx`
- capability: async/sync HTTP client, connection pooling, auth, streaming bodies, timeouts/limits, proxy/transport injection, error taxonomy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and message family
- rail: transport

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------ | :------------ | :-------------------------- |
|  [01]   | `AsyncClient` | client        | pooled async HTTP client    |
|  [02]   | `Client`      | client        | pooled sync HTTP client     |
|  [03]   | `Request`     | message       | outbound request            |
|  [04]   | `Response`    | message       | inbound response            |
|  [05]   | `URL`         | value         | parsed URL                  |
|  [06]   | `Headers`     | value         | case-insensitive header map |
|  [07]   | `QueryParams` | value         | query-string map            |
|  [08]   | `Cookies`     | value         | cookie jar                  |
|  [09]   | `Timeout`     | config        | per-phase timeout policy    |
|  [10]   | `Limits`      | config        | connection-pool limits      |
|  [11]   | `Proxy`       | config        | proxy configuration         |

[PUBLIC_TYPE_SCOPE]: auth and transport family
- rail: transport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :------------------- | :------------ | :------------------------ |
|  [01]   | `Auth`               | auth base     | request-signing contract  |
|  [02]   | `BasicAuth`          | auth          | HTTP basic auth           |
|  [03]   | `DigestAuth`         | auth          | HTTP digest auth          |
|  [04]   | `NetRCAuth`          | auth          | netrc-file auth           |
|  [05]   | `AsyncBaseTransport` | transport     | async transport contract  |
|  [06]   | `AsyncHTTPTransport` | transport     | default async transport   |
|  [07]   | `ASGITransport`      | transport     | in-process ASGI transport |
|  [08]   | `MockTransport`      | transport     | test transport            |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :----------------- | :------------ | :--------------------------- |
|  [01]   | `HTTPError`        | fault base    | request/transport error base |
|  [02]   | `RequestError`     | fault         | request-side failure base    |
|  [03]   | `ConnectError`     | fault         | connection failure           |
|  [04]   | `ConnectTimeout`   | fault         | connect-phase timeout        |
|  [05]   | `ReadTimeout`      | fault         | read-phase timeout           |
|  [06]   | `PoolTimeout`      | fault         | pool-acquire timeout         |
|  [07]   | `HTTPStatusError`  | fault         | raised non-2xx status        |
|  [08]   | `TooManyRedirects` | fault         | redirect-limit exceeded      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client operations
- rail: transport

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `AsyncClient(...)`                                          | build          | pooled client with timeout/limits/auth |
|  [02]   | `AsyncClient.request`                                       | send           | issue any-method request               |
|  [03]   | `AsyncClient.get` / `.post` / `.put` / `.delete` / `.patch` | send           | method-specific request                |
|  [04]   | `AsyncClient.send`                                          | send           | send a prebuilt `Request`              |
|  [05]   | `AsyncClient.stream`                                        | stream         | streaming request/response context     |
|  [06]   | `AsyncClient.aclose`                                        | drain          | close the connection pool              |
|  [07]   | `Response.raise_for_status`                                 | check          | raise on non-2xx                       |
|  [08]   | `Response.json`                                             | decode         | response body to JSON                  |
|  [09]   | `Response.aiter_bytes` / `.aiter_lines`                     | stream         | chunked body iteration                 |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- client law: one long-lived `AsyncClient` per outbound endpoint group is constructed with explicit `Timeout`, `Limits`, and `Auth` and reused; per-request client construction is deleted.
- timeout law: every request carries an explicit `Timeout` (connect/read/write/pool); the anyio deadline scope wraps the call so cancellation propagates.
- streaming law: large bodies use `stream` + `aiter_bytes`/`aiter_lines`; full-body reads are reserved for small payloads.
- resilience law: transient transport faults (`ConnectError`, `ReadTimeout`, `PoolTimeout`) are retried through the `stamina` owner; non-transient faults surface as `Error(BoundaryFault(...))`.
- transport injection law: tests inject `MockTransport`; in-process server checks use `ASGITransport`; production uses the default `AsyncHTTPTransport` — no monkeypatching.
- drain law: `aclose` participates in the host drain choreography.

[LOCAL_ADMISSION]:
- The transport surface composes `AsyncClient` for outbound calls and credential probes; the runtime owns no second HTTP client.
- Response decode and status checks lift faults at the boundary; domain logic receives a `Result`, never a raw `Response` error.

[RAIL_LAW]:
- Package: `httpx`
- Owns: async/sync HTTP transport, connection pooling, auth, streaming bodies, timeouts/limits, and transport injection
- Accept: reused `AsyncClient`, explicit `Timeout`/`Limits`/`Auth`, streaming bodies, injected transports, retried transient faults
- Reject: per-request client construction, implicit timeouts, full-body reads of large payloads, transport monkeypatching, a second HTTP client
