# [PY_RUNTIME_API_PYQWEST]

`pyqwest` owns the HTTP transport beneath every `connectrpc` dial: one native `HTTPTransport` carries TLS roots, the mTLS pair, proxy, pool, and HTTP-version policy over reqwest, `Client` issues requests through it, and `aclose` on the transport is the one socket release — the `ConnectClient` riding it closes nothing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyqwest`
- package: `pyqwest`
- module: `pyqwest`
- namespaces: `pyqwest`, `pyqwest.httpx`, `pyqwest.middleware`, `pyqwest.testing`
- abi: reqwest-backed native extension (`_pyqwest.abi3.so`) carrying `HTTPTransport`, `Client`, and their sync twins; `py.typed` ships
- rail: transport

## [02]-[PUBLIC_TYPES]

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

## [03]-[ENTRYPOINTS]

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
- `Client` exposes no per-request timeout on the async methods; `timeout` lives on the transport or on an `anyio` scope around the call.
- `enable_otel` defaults on, so the transport emits its own client spans under the ambient provider unless the composition passes `tracer_provider`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HTTPTransport` owns sockets and pools; `Client` and `ConnectClient` hold no resource of their own, so whoever constructs the transport owns its `aclose`.
- `Transport` is a protocol, so a middleware or test double wraps `execute` and the client never learns it; `pyqwest.testing` and `pyqwest.middleware` ride that seam.
- transport-leg raises (`ReadError`, `WriteError`, `TooManyRedirects`, a connect timeout) surface through the `connectrpc` client as `ConnectError`, never bare past the dial.

[STACKING]:
- `connectrpc`(`libs/python/.api/connectrpc.md`): `ConnectClient(address, http_client=Client(transport=HTTPTransport(...)))` binds the dial; `ConnectClient.close` flips a flag, so the composition `aclose`s the transport it built.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): `enable_otel` emits the transport's own client spans; the branch's client `MetadataInterceptor` span on `transport/serve` is the rpc-grain span above it.
- `runtime/transport/serve`: `CredentialPolicy.client_transport` projects each outbound posture to one `HTTPTransport` (`tls_ca_cert`, `tls_cert`/`tls_key`, `tls_include_system_certs`) and `CapabilityInvoke.aclose` releases it.

[LOCAL_ADMISSION]:
- every fence constructs the transport from a `CredentialPolicy` row and never from ambient defaults; a bare `HTTPTransport()` dials nothing TLS.
- `pyqwest.httpx` stays out — the branch dials Connect through `ConnectClient`, never an httpx-shaped client.

[RAIL_LAW]:
- Package: `pyqwest`
- Owns: the HTTP/1 and HTTP/2 transport, TLS and mTLS material, proxy and pool policy, and the socket release beneath every Connect dial
- Accept: `HTTPTransport(tls_ca_cert=..., tls_cert=..., tls_key=...)` per credential posture, `Client(transport=...)` injected into `ConnectClient`, `await transport.aclose()` at the drain
- Reject: an `HTTPTransport()` carrying no TLS material on a TLS dial, a transport no composition closes, a second HTTP client beside `connectrpc` on the Connect seam
