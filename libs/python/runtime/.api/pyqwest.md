# [PY_RUNTIME_API_PYQWEST]

`pyqwest` owns the HTTP transport beneath every `connectrpc` dial: one native `HTTPTransport` carries TLS roots, the mTLS pair, proxy, pool, and HTTP-version policy over reqwest, `Client` issues requests through it, and `aclose` on the transport is the one socket release — the `ConnectClient` riding it closes nothing.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport, client, and message family

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :---------------------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `HTTPTransport`                                 | final class   | async reqwest transport; TLS, mTLS, proxy, pool, version ctor knobs    |
|  [02]   | `Transport`                                     | protocol      | `execute(request) -> Awaitable[Response]`, the seam a middleware wraps |
|  [03]   | `Client`                                        | class         | async request issuer over one `Transport`; `connectrpc` injects it     |
|  [04]   | `Request` / `Response` / `FullResponse`         | class         | method, url, `Headers`, streamed or buffered content, `trailers`       |
|  [05]   | `Headers`                                       | multimap      | case-insensitive mutable headers; `add` / `getall` keep repeats        |
|  [06]   | `HTTPVersion`                                   | value         | ordered HTTP version the transport pins or ALPN negotiates             |
|  [07]   | `Proxy`                                         | class         | proxy `url` with `auth`, `headers`, and routing rules                  |
|  [08]   | `SyncHTTPTransport` / `SyncClient`              | class         | the sync twins behind `ConnectClientSync`; `close` releases            |
|  [09]   | `ReadError` / `WriteError` / `TooManyRedirects` | exception     | transport-leg refusals the client lifts into `ConnectError` codes      |
|  [10]   | `middleware.retry.RetryTransport`               | class         | SECOND retry schedule wrapping a `Transport`; refused, see the law     |
|  [11]   | `middleware.retry.SyncRetryTransport`           | class         | its `SyncTransport` twin over the same schedule                        |
|  [12]   | `middleware.retry.RetryMode`                    | `Enum`        | `BUFFERED` / `UNBUFFERED` — whether a streamed body survives a re-send |

`pyqwest.middleware` exports `retry` alone, whose `RetryTransport(transport, initial_interval=0.5, randomization_factor=0.5, multiplier=1.5, max_interval=60.0, max_retries=4)` carries a complete jittered-exponential curve, a `Retry-After` reader over both the seconds and HTTP-date forms, and a transient predicate re-driving `ConnectionError`, `429`, and `5xx` other than `501` on `GET`/`HEAD`/`PUT`/`DELETE` alone.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport construction, dialing, and release
- `HTTPTransport` ctor carries keyword-only: `tls_ca_cert`, `tls_include_system_certs`, `tls_key`, `tls_cert`, `http_version`, `proxy`, `timeout`, `connect_timeout`, `read_timeout`, `pool_idle_timeout`, `pool_max_idle_per_host`, `tcp_keepalive_interval`, `enable_gzip`, `enable_brotli`, `enable_zstd`, `use_system_dns`, `enable_cookie_store`, `follow_redirects`, `max_redirects`, `enable_otel`, `meter_provider`, `tracer_provider`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `HTTPTransport(*, ...)`                                    | ctor     | one transport per credential posture; `async with` brackets it  |
|  [02]   | `transport.execute(request)`                               | instance | issue one `Request`, answering a streaming `Response`           |
|  [03]   | `transport.aclose()`                                       | instance | release the pool and every socket; the one teardown             |
|  [04]   | `Client(transport)`                                        | ctor     | bind a client to a transport; `None` takes the shared default   |
|  [05]   | `client.execute(method, url, headers, content, *, params)` | instance | issue one request, buffering the body into a `FullResponse`     |
|  [06]   | `client.stream(method, url, headers, content, *, params)`  | instance | issue one request, answering the streaming `Response`           |
|  [07]   | `client.get/post/put/patch/delete/head/options(url, ...)`  | instance | the verb sugar over `execute`                                   |
|  [08]   | `response.status` / `headers` / `content` / `trailers`     | property | status, headers, async byte iterator, and the trailers after it |
|  [09]   | `Headers.add(key, value)` / `getall(key)`                  | instance | append and read repeated values                                 |

- `HTTPTransport()` with no TLS argument verifies nothing — `tls_ca_cert` or `tls_include_system_certs=True` is what makes a TLS dial succeed.
- `tls_key` and `tls_cert` bind as a PAIR; one without the other refuses at construction.
- `http_version` unset rides HTTP/1 on plaintext and ALPN on TLS, so a plaintext h2c dial pins `HTTPVersion` explicitly.
- `timeout` and `read_timeout` default to NONE — the whole-request and per-read deadlines are UNBOUNDED where a caller states neither, while `connect_timeout=30.0`, `pool_idle_timeout=90.0`, and `tcp_keepalive_interval=30.0` each carry one.
- `Client` exposes no per-request timeout on the async methods, so the wire deadline rides the transport row or an `anyio` scope; a `stamina` `timeout` bounds no in-flight read, per the law below.
- `pool_max_idle_per_host` defaults to two idle connections per host, so a fan-out dial re-establishes rather than reusing past that width.
- `follow_redirects` carries the shipped stub's `True` default while that same docstring states the default is disabled, so a Connect dial states the value rather than inheriting a surface contradicting itself; `max_redirects=10` bounds the follow with `TooManyRedirects`.
- `enable_otel` defaults on and emits the transport's own client spans; `tracer_provider` selects WHICH provider receives them and `enable_otel=False` is the one off switch.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HTTPTransport` owns sockets and pools; `Client` and `ConnectClient` hold no resource of their own, so whoever constructs the transport owns its `aclose`.
- `Transport` is a protocol, so a middleware or test double wraps `execute` and the client never learns it; `pyqwest.testing` and `pyqwest.middleware` ride that seam.
- transport-leg raises (`ReadError`, `WriteError`, `TooManyRedirects`, a connect timeout) surface through the `connectrpc` client as `ConnectError`, never bare past the dial.
- `middleware.retry` is a SECOND retry owner on exactly the seam `connectrpc` dials through: `RetryTransport` wraps a `Transport` and re-drives beneath the client, so a schedule stacked there multiplies the enclosing `RetryClass` attempt count and widens the idempotency window every re-drive already opened. Its `RetryMode` names what a stream costs — `BUFFERED` holds the whole request body in memory to make a re-send possible, `UNBUFFERED` re-drives connection errors alone — and its `Retry-After` reader duplicates the throttle window the resilience owner's own probe already lifts onto a `throttled` verdict.
- Retry deadlines do NOT bound an in-flight read: `stamina`'s `timeout` lowers to a `stop_after_delay` STOP condition evaluated BETWEEN attempts, so a hung read under an unset `read_timeout` outlives every `RetryClass` budget — the transport row carries the one wire deadline, and a retry schedule substitutes for it nowhere.

[STACKING]:
- `connectrpc`(`libs/python/.api/connectrpc.md`): `ConnectClient(address, http_client=Client(transport=HTTPTransport(...)))` binds the dial; `ConnectClient.close` flips a flag, so the composition `aclose`s the transport it built.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): `enable_otel` emits the transport's own client spans; the branch's client `MetadataInterceptor` span on `transport/serve` is the rpc-grain span above it.
- `runtime/transport/serve`: `CredentialPolicy.client_transport` projects each outbound posture to one `HTTPTransport` (`tls_ca_cert`, `tls_cert`/`tls_key`, `tls_include_system_certs`) and `CapabilityInvoke.aclose` releases it.

[LOCAL_ADMISSION]:
- every fence constructs the transport from a `CredentialPolicy` row and never from ambient defaults; a bare `HTTPTransport()` dials nothing TLS.
- `pyqwest.httpx` stays out — the branch dials Connect through `ConnectClient`, never an httpx-shaped client.
- `middleware.retry` is REFUSED whole. `reliability/resilience#RESILIENCE` holds every schedule the branch runs, `RetryClass.WIRE` names the Connect row, and a transport-level curve beneath it makes effective attempts the product of two schedules the branch RULINGS foreclose for exactly that reason.
- Every transport row states `timeout` and `read_timeout`, since the package leaves both unset and the enclosing retry budget bounds no in-flight read.
- Every transport row states `follow_redirects`, because a silently followed 3xx re-issues one RPC against a second authority the credential posture never named.
