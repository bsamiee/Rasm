# [PY_RUNTIME_API_HTTPX]

`httpx` supplies the async/sync HTTP client: `AsyncClient`/`Client` with connection pooling, an auth family, streaming request/response bodies, timeout and limit configuration, proxy/transport injection, and a full error taxonomy. It is the runtime owner for outbound HTTP transport and inbound-server credential checks over the companion seam.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `httpx`
- package: `httpx`
- import: `httpx`
- version: `0.28.1`
- owner: `runtime`
- rail: transport
- namespaces: `httpx`
- capability: async/sync HTTP client, connection pooling, auth, streaming bodies, timeouts/limits, proxy/transport injection, error taxonomy

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and message family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `AsyncClient` | client | pooled async HTTP client |
| [2] | `Client` | client | pooled sync HTTP client |
| [3] | `Request` | message | outbound request |
| [4] | `Response` | message | inbound response |
| [5] | `URL` | value | parsed URL |
| [6] | `Headers` | value | case-insensitive header map |
| [7] | `QueryParams` | value | query-string map |
| [8] | `Cookies` | value | cookie jar |
| [9] | `Timeout` | config | per-phase timeout policy |
| [10] | `Limits` | config | connection-pool limits |
| [11] | `Proxy` | config | proxy configuration |

[PUBLIC_TYPE_SCOPE]: auth and transport family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Auth` | auth base | request-signing contract |
| [2] | `BasicAuth` | auth | HTTP basic auth |
| [3] | `DigestAuth` | auth | HTTP digest auth |
| [4] | `NetRCAuth` | auth | netrc-file auth |
| [5] | `AsyncBaseTransport` | transport | async transport contract |
| [6] | `AsyncHTTPTransport` | transport | default async transport |
| [7] | `ASGITransport` | transport | in-process ASGI transport |
| [8] | `MockTransport` | transport | test transport |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `HTTPError` | fault base | request/transport error base |
| [2] | `RequestError` | fault | request-side failure base |
| [3] | `ConnectError` | fault | connection failure |
| [4] | `ConnectTimeout` | fault | connect-phase timeout |
| [5] | `ReadTimeout` | fault | read-phase timeout |
| [6] | `PoolTimeout` | fault | pool-acquire timeout |
| [7] | `HTTPStatusError` | fault | raised non-2xx status |
| [8] | `TooManyRedirects` | fault | redirect-limit exceeded |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client operations
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `AsyncClient(...)` | build | pooled client with timeout/limits/auth |
| [2] | `AsyncClient.request` | send | issue any-method request |
| [3] | `AsyncClient.get` / `.post` / `.put` / `.delete` / `.patch` | send | method-specific request |
| [4] | `AsyncClient.send` | send | send a prebuilt `Request` |
| [5] | `AsyncClient.stream` | stream | streaming request/response context |
| [6] | `AsyncClient.aclose` | drain | close the connection pool |
| [7] | `Response.raise_for_status` | check | raise on non-2xx |
| [8] | `Response.json` | decode | response body to JSON |
| [9] | `Response.aiter_bytes` / `.aiter_lines` | stream | chunked body iteration |

## [4]-[IMPLEMENTATION_LAW]

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
