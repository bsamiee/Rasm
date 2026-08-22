# [PY_BRANCH_API_CONNECTRPC]

`connectrpc` owns the branch RPC seam: generated handler protocols and their ASGI / WSGI applications answer Connect, gRPC, and gRPC-Web on one route set, typed async and sync clients dial that seam over `pyqwest`, and interceptors, codecs, compression, and the `ConnectError` status rail bind both ends. `protobuf-py` keeps message shape; every request and response crosses here as its own generated class.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `connectrpc`
- package: `connectrpc` (Apache-2.0)
- module: `connectrpc`
- namespaces: `connectrpc.server`, `connectrpc.client`, `connectrpc.request`, `connectrpc.method`, `connectrpc.interceptor`, `connectrpc.codec`, `connectrpc.compression` with `.gzip` / `.brotli` / `.zstd`, `connectrpc.errors`, `connectrpc.code`, `connectrpc.protocol`, `connectrpc.compat`
- abi: pure Python; clients dial through the `pyqwest` reqwest-backed native extension, servers mount on any ASGI or WSGI host
- depends: `pyqwest` supplies `Client` and `SyncClient` over an `HTTPTransport` carrying TLS, mTLS, proxy, pool, and `HTTPVersion` selection
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: request, method, and status family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `Headers`          | multimap      | case-insensitive `MutableMapping[str, str]`; `add` / `getall` / `allitems` keep repeats |
|  [02]   | `RequestContext`   | generic class | per-call context both ends read; response headers and trailers mint on first read       |
|  [03]   | `MethodInfo`       | frozen record | `name` `service_name` `input` `output` `idempotency_level`, one per rpc                 |
|  [04]   | `IdempotencyLevel` | enum          | `UNKNOWN` `NO_SIDE_EFFECTS` `IDEMPOTENT`; gates GET admission and `use_get`             |
|  [05]   | `ProtocolType`     | enum          | `CONNECT` `GRPC` `GRPC_WEB`; fixes one client's wire, never a server's                  |
|  [06]   | `Code`             | enum          | 16 Connect status codes whose `.value` is the wire spelling                             |

[Code]: `CANCELED` `UNKNOWN` `INVALID_ARGUMENT` `DEADLINE_EXCEEDED` `NOT_FOUND` `ALREADY_EXISTS` `PERMISSION_DENIED` `RESOURCE_EXHAUSTED` `FAILED_PRECONDITION` `ABORTED` `OUT_OF_RANGE` `UNIMPLEMENTED` `INTERNAL` `UNAVAILABLE` `DATA_LOSS` `UNAUTHENTICATED`

[PUBLIC_TYPE_SCOPE]: server family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `ConnectASGIApplication` | generic ABC   | ASGI callable over one service; `path` names its mount prefix            |
|  [02]   | `ConnectWSGIApplication` | ABC           | WSGI callable over sync endpoints; `path` names its mount prefix         |
|  [03]   | `Endpoint`               | frozen record | async rpc binding; four static factories mint the shape-specific subtype |
|  [04]   | `EndpointSync`           | frozen record | sync rpc binding carrying the same four factories                        |

[PUBLIC_TYPE_SCOPE]: client family

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ConnectClient`     | class         | async dialer over a `pyqwest` `Client`; `<Svc>Client` derives from it         |
|  [02]   | `ConnectClientSync` | class         | sync dialer over a `pyqwest` `SyncClient`; `<Svc>ClientSync` derives from it  |
|  [03]   | `ResponseMetadata`  | context class | contextvar-scoped capture of `headers` / `trailers` for the calls it encloses |

[PUBLIC_TYPE_SCOPE]: interceptor family
- async and sync protocols pair one to one, the sync member name carrying a `_sync` suffix; every protocol is `runtime_checkable`, so an application admits any object holding the hook.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `Interceptor` / `InterceptorSync`                         | union         | closed protocol set a client or application admits |
|  [02]   | `MetadataInterceptor` / `MetadataInterceptorSync`         | protocol      | header-only pair spanning every rpc shape          |
|  [03]   | `UnaryInterceptor` / `UnaryInterceptorSync`               | protocol      | wraps one unary call                               |
|  [04]   | `ServerStreamInterceptor` / `ServerStreamInterceptorSync` | protocol      | wraps one server stream                            |
|  [05]   | `ClientStreamInterceptor` / `ClientStreamInterceptorSync` | protocol      | wraps one client stream                            |
|  [06]   | `BidiStreamInterceptor` / `BidiStreamInterceptorSync`     | protocol      | wraps one bidi stream                              |

[PUBLIC_TYPE_SCOPE]: codec, compression, and error family
- `connectrpc.compression` declares `Compression` alone; each implementation imports from its own module, and `brotli` / `zstandard` are the distributions those two modules import at top level.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `Codec`             | protocol      | `name()` `encode(message)` `decode(data, message_class)`                      |
|  [02]   | `Compression`       | protocol      | `name()` `compress(data)` `decompress(data)`                                  |
|  [03]   | `GzipCompression`   | compression   | `gzip`; default send and accept compression on both ends                      |
|  [04]   | `BrotliCompression` | compression   | `br` over the `brotli` distribution                                           |
|  [05]   | `ZstdCompression`   | compression   | `zstd` over the `zstandard` distribution                                      |
|  [06]   | `ConnectError`      | exception     | `code` `message` `details`; raised server-side, re-raised client-side intact  |
|  [07]   | `ErrorDetail`       | value class   | `Any`-packed detail message; `type_name` and `message_bytes` read the packing |

[PUBLIC_TYPE_SCOPE]: generated stub family — `protoc-gen-connectrpc` mints these per service in `<path>_connect.py`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `<Svc>`                | protocol      | async handler surface, one snake_case method per rpc                  |
|  [02]   | `<Svc>Sync`            | protocol      | sync handler surface carrying the same method names                   |
|  [03]   | `<Svc>ASGIApplication` | class         | `ConnectASGIApplication[<Svc>]` seating every endpoint and its `path` |
|  [04]   | `<Svc>WSGIApplication` | class         | `ConnectWSGIApplication` seating every endpoint and its `path`        |
|  [05]   | `<Svc>Client`          | class         | `ConnectClient` carrying one typed method per rpc                     |
|  [06]   | `<Svc>ClientSync`      | class         | `ConnectClientSync` carrying one typed method per rpc                 |

- `<Svc>` / `<Svc>Sync`: each method body raises `ConnectError(Code.UNIMPLEMENTED, 'Not implemented')`, so a handler satisfies the protocol structurally and overrides what it serves.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serving
- application ctors carry: `interceptors`, `read_max_bytes`, `compressions`, `codecs`.

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `ConnectASGIApplication(*, service, endpoints, ...)` | ctor     | async application over one service                |
|  [02]   | `ConnectWSGIApplication(*, endpoints, ...)`          | ctor     | sync application over one endpoint mapping        |
|  [03]   | `app(scope, receive, send)`                          | instance | ASGI callable a host serves                       |
|  [04]   | `app(environ, start_response)`                       | instance | WSGI callable a host serves                       |
|  [05]   | `app.path`                                           | property | `/<package>.<Service>` prefix a dispatcher mounts |
|  [06]   | `Endpoint.unary(method, function)`                   | static   | bind `async (request, ctx) -> RES`                |
|  [07]   | `Endpoint.client_stream(method, function)`           | static   | bind `async (AsyncIterator, ctx) -> RES`          |
|  [08]   | `Endpoint.server_stream(method, function)`           | static   | bind `(request, ctx) -> AsyncIterator`            |
|  [09]   | `Endpoint.bidi_stream(method, function)`             | static   | bind `(AsyncIterator, ctx) -> AsyncIterator`      |
|  [10]   | `EndpointSync.unary(*, method, function)`            | static   | bind `(request, ctx) -> RES`                      |
|  [11]   | `EndpointSync.client_stream(*, method, function)`    | static   | bind `(Iterator, ctx) -> RES`                     |
|  [12]   | `EndpointSync.server_stream(*, method, function)`    | static   | bind `(request, ctx) -> Iterator`                 |
|  [13]   | `EndpointSync.bidi_stream(method, function)`         | static   | bind `(Iterator, ctx) -> Iterator`                |

- `EndpointSync.bidi_stream`: takes `method` and `function` positionally where its three siblings take them keyword-only.
- `ConnectASGIApplication`: `endpoints` is `Callable[[SVC], Mapping[str, Endpoint]]`, and `service` admits an instance or an async generator resolved at lifespan startup and `aclose()`d at shutdown — a host without lifespan support refuses the generator with `RuntimeError`.
- `ConnectWSGIApplication`: `endpoints` is the `Mapping[str, EndpointSync]` itself, keyed by `/<package>.<Service>/<Method>`.

[ENTRYPOINT_SCOPE]: dialing
- client ctors carry: `codec`, `protocol`, `accept_compression`, `send_compression`, `timeout_ms`, `read_max_bytes`, `interceptors`, `http_client`.
- `execute_*` members are keyword-only; stream-in shapes take `request: AsyncIterator[REQ]`, and `Iterator[REQ]` on the sync twin.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `ConnectClient(address, *, ...)`                                         | ctor     | async dialer; `async with` brackets  |
|  [02]   | `ConnectClientSync(address, *, ...)`                                     | ctor     | sync dialer; `with` brackets         |
|  [03]   | `client.execute_unary(*, request, method, headers, timeout_ms, use_get)` | instance | unary; `use_get` sends Connect GET   |
|  [04]   | `client.execute_client_stream(*, request, method, headers, timeout_ms)`  | instance | stream in, one message back          |
|  [05]   | `client.execute_server_stream(*, request, method, headers, timeout_ms)`  | instance | one message in, stream back          |
|  [06]   | `client.execute_bidi_stream(*, request, method, headers, timeout_ms)`    | instance | stream both ways over one body       |
|  [07]   | `client.close()`                                                         | instance | flips a closed flag; brackets run it |
|  [08]   | `ResponseMetadata()`                                                     | ctor     | capture responses of enclosed calls  |
|  [09]   | `md.headers` / `md.trailers`                                             | property | `Headers` off the captured response  |

- `ConnectClient.close`: no request path reads the flag and no HTTP client closes, so whoever built the `http_client` owns closing it.
- `ConnectClientSync`: mirrors every `execute_*` name and keyword set, swapping `AsyncIterator` for `Iterator`.
- `client.execute_unary`: `ProtocolType.GRPC` routes unary through the bidi stream path, leaving `use_get` inert.

[ENTRYPOINT_SCOPE]: context, headers, and rpc identity

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `MethodInfo(*, name, service_name, input, output, idempotency_level)` | ctor     | one rpc's wire identity                             |
|  [02]   | `RequestContext(*, method, http_method, request_headers, ...)`        | ctor     | context the protocol layer mints per call           |
|  [03]   | `ctx.method` / `ctx.http_method` / `ctx.request_headers`              | property | inbound identity and headers                        |
|  [04]   | `ctx.response_headers` / `ctx.response_trailers`                      | property | mutable `Headers` a handler fills before first send |
|  [05]   | `ctx.timeout_ms`                                                      | property | milliseconds REMAINING, recomputed on each read     |
|  [06]   | `ctx.server_address` / `ctx.client_address`                           | property | `address:port` strings, `None` where unavailable    |
|  [07]   | `Headers(items)`                                                      | ctor     | admit a mapping or `(key, value)` sequence          |
|  [08]   | `headers.add(key, value)`                                             | instance | append without overwriting an existing value        |
|  [09]   | `headers.getall(key)`                                                 | instance | every value for one key, `()` on a miss             |
|  [10]   | `headers.allitems()`                                                  | instance | item view carrying duplicates                       |
|  [11]   | `headers.items()` / `keys()` / `values()`                             | instance | duplicate-free views                                |
|  [12]   | `headers.clear()`                                                     | instance | drop every header and its duplicates                |

- `RequestContext`: `timeout_ms`, `server_address`, and `client_address` fill the remaining ctor slots, each defaulting to `None`.
- `ctx.timeout_ms`: reads `None` where no deadline arrived, and goes negative once an accepted deadline passes.

[ENTRYPOINT_SCOPE]: codecs, compression, and errors

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `codec.proto_binary_codec()`                                       | static   | `proto` codec over `to_binary` / `from_binary`         |
|  [02]   | `codec.proto_json_codec(registry)`                                 | static   | `json` codec; `registry` resolves `Any` and extensions |
|  [03]   | `compat.google_protobuf_binary_codec()`                            | static   | `proto` codec for `google.protobuf` stubs              |
|  [04]   | `compat.google_protobuf_json_codec()`                              | static   | `json` codec for the same stubs                        |
|  [05]   | `compat.google_protobuf_codecs()`                                  | static   | both compat codecs as one list                         |
|  [06]   | `codec.name()` / `encode(message)` / `decode(data, message_class)` | instance | the three members a codec answers                      |
|  [07]   | `GzipCompression(level)`                                           | ctor     | `gzip` at level 6 by default                           |
|  [08]   | `BrotliCompression(quality)`                                       | ctor     | `br` at quality 3 by default                           |
|  [09]   | `ZstdCompression(level)`                                           | ctor     | `zstd` at level 3 by default                           |
|  [10]   | `compression.name()` / `compress(data)` / `decompress(data)`       | instance | the three members a compression answers                |
|  [11]   | `ConnectError(code, message, details)`                             | ctor     | raise inside a handler; `details` is `()` by default   |
|  [12]   | `err.code` / `err.message` / `err.details`                         | property | the wire triple a client re-raises                     |
|  [13]   | `ErrorDetail(message)`                                             | ctor     | pack one message, or adopt an `Any` unchanged          |
|  [14]   | `detail.value(desc_or_registry)`                                   | instance | unpack through a `Registry`, descriptor, or class      |
|  [15]   | `detail.type_name` / `detail.message_bytes`                        | property | packed type name and serialized payload                |

- `codec.proto_json_codec`: absent `registry`, one shared default holding the well-known types alone resolves, so a fence packing its own `Any` passes its `Registry`.
- `ConnectError.details`: admits generated `Message` values and `ErrorDetail` values alike, wrapping every bare message on construction.
- `detail.value`: returns `None` where the argument is absent and the detail arrived packed, so an unresolvable detail never raises.

[ENTRYPOINT_SCOPE]: interceptor hooks
- every shape hook takes `(call_next, request, ctx)` and returns what its rpc shape returns; the sync twin suffixes each name with `_sync`.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `intercept_unary(call_next, request, ctx)`         | instance | wrap, replace, or short-circuit one unary call           |
|  [02]   | `intercept_client_stream(call_next, request, ctx)` | instance | wrap one client stream                                   |
|  [03]   | `intercept_server_stream(call_next, request, ctx)` | instance | wrap one server stream                                   |
|  [04]   | `intercept_bidi_stream(call_next, request, ctx)`   | instance | wrap one bidi stream                                     |
|  [05]   | `on_start(ctx)`                                    | instance | open cross-cutting work, returning a token               |
|  [06]   | `on_end(token, ctx, error)`                        | instance | close it in a `finally`, `error` naming the terminal one |

[ENTRYPOINT_SCOPE]: generated stubs

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `<Svc>ASGIApplication(service, *, interceptors, read_max_bytes, compressions, codecs)` | ctor     | mount one async service |
|  [02]   | `<Svc>WSGIApplication(service, interceptors, read_max_bytes, compressions, codecs)`    | ctor     | mount one sync service  |
|  [03]   | `<Svc>Client.<rpc>(request, *, headers, timeout_ms)`                                   | instance | dial one rpc            |
|  [04]   | `<Svc>Client.<rpc>(request, *, headers, timeout_ms, use_get)`                          | instance | dial one GET-able rpc   |

- `<Svc>WSGIApplication`: every ctor argument is positional-or-keyword where its ASGI twin keeps all but `service` keyword-only.
- `<rpc>`: each proto method name folds to snake_case, so `GetThing` is reached as `get_thing`.
- `use_get`: generated onto `NO_SIDE_EFFECTS` unary methods alone, and absent from every other rpc's signature.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `protoc-gen-connectrpc` emits `<path>_connect.py` beside `<path>_pb.py` for service-bearing sources alone, under two options: `protobuf` (`py` default, `google`) selects the message runtime, and `io` (`async`, `sync`, both when unset) selects the handler flavour.
- one application answers every protocol on one route set: the server reads the request `content-type`, routing `application/grpc` to gRPC, `application/grpc-web` to gRPC-Web, and everything else to Connect, while each client fixes its own wire through `protocol=`.
- gRPC answers over host trailers: an ASGI host publishes the `http.response.trailers` extension and a WSGI host `wsgi.ext.http.send_trailers`, else the application raises `RuntimeError` on the first gRPC request; Connect streaming and gRPC-Web carry their status in the final envelope instead.
- GET admission is idempotency-gated at the server: a `NO_SIDE_EFFECTS` unary endpoint answers GET or POST and reads its codec from the `encoding` query parameter, while every other endpoint answers POST alone and refuses the rest with 405.
- `RequestContext.timeout_ms` reports time REMAINING against the inbound `connect-timeout-ms` or `grpc-timeout` header, so a handler enforces its own deadline; clients enforce theirs locally and raise `ConnectError(Code.DEADLINE_EXCEEDED)`.
- `ConnectError` raised in a handler crosses intact with its `code`, `message`, and `details`, while any other exception crosses as `Code.UNKNOWN` carrying `str(exc)` and re-raises into the host after the response lands.
- clients map transport failure themselves: a timeout to `DEADLINE_EXCEEDED`, a cancellation to `CANCELED`, an HTTP/2 stream reset through its own reset-code table, and every remaining exception to `UNAVAILABLE` — the request's own encode-side `TypeError`/`OverflowError`/`ValueError` from `to_binary` included, so a caller pre-encodes under its own fence ahead of any retried call or a caller-repairable refusal reads as a transient.
- `Code` is a string-valued `Enum` whose `.value` is the wire spelling — `int(code)` raises `TypeError`, and no ordinal of it is a wire fact a detail may carry.
- interceptors wrap in declaration order, first declared outermost; `MetadataInterceptor.on_start` is an async hook returning a token the async `on_end` receives with the terminal error, the one pair spanning every rpc shape.
- both halves run asyncio loop primitives — the server `create_task`/`Event`/`sleep`, the client `asyncio.timeout`/`wait_for` — so an application serves and a client dials on the asyncio backend alone; under trio the gRPC and lifespan legs raise `trio.run received unrecognized yield message`.
- compression negotiates per call: unset `compressions` or `accept_compression` seats gzip beside identity, an empty iterable leaves identity alone, identity always survives resolution, and an unrecognized request encoding draws `Code.UNIMPLEMENTED`.
- unset `codecs` seats the proto binary and proto JSON pair, and a request naming an unseated codec draws 415.
- `read_max_bytes` bounds the decompressed unary body at the server and the response payload at the client, both refusing with `Code.RESOURCE_EXHAUSTED`; streams enforce it per envelope.

[STACKING]:
- `protobuf-py`(`.api/protobuf-py.md`): `MethodInfo.input` / `output` name generated `Message` classes, `proto_binary_codec()` / `proto_json_codec(registry)` drive `Message.to_binary` / `from_binary` / `to_json` / `from_json`, and `ErrorDetail.value(registry)` unpacks a detail through a `Registry` seated from `<module>_pb.desc()`.
- `hypercorn`(`libs/python/runtime/.api/hypercorn.md`): `hypercorn.asyncio.serve(app, config)` hosts `<Svc>ASGIApplication` off a `Config` whose `bind` roster the composition root assigns (plaintext h2c on a UNIX socket included), and `DispatcherMiddleware({app.path: app})` mounts several services on one listener; `hypercorn.trio` hosts no Connect application.
- `opentelemetry-instrumentation-asgi`(`libs/python/runtime/.api/opentelemetry-instrumentation-asgi.md`): `OpenTelemetryMiddleware(<Svc>ASGIApplication(service))` opens the server span, and `server_request_hook` stamps rpc attributes off the `/<package>.<Service>/<Method>` path.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): the server span is `opentelemetry-instrumentation-asgi`'s off `scope["headers"]`, the client `MetadataInterceptor.on_start` stamps the active context through `propagate.inject(ctx.request_headers)`, and its `on_end` closes the span off `ConnectError.code`.
- `anyio`(`.api/anyio.md`): a blocking handler body bounds through `to_thread.run_sync(fn, limiter=CapacityLimiter(n))`, and `ctx.timeout_ms` feeds `fail_after(ctx.timeout_ms / 1000)` so the handler refuses on its own remaining deadline.
- `stamina`(`libs/python/runtime/.api/stamina.md`): `AsyncRetryingCaller.on(hook)` returns a `BoundAsyncRetryingCaller` whose backoff hook reads `ConnectError.code` — `UNAVAILABLE` and `DEADLINE_EXCEEDED` retry, `INVALID_ARGUMENT` and `FAILED_PRECONDITION` never — wrapping every `<Svc>Client.<rpc>` call.
- `zstandard`(`libs/python/artifacts/.api/zstandard.md`) and `brotli`(`libs/python/artifacts/.api/brotli.md`): `ZstdCompression(level)` folds onto `ZstdCompressor(level=...).compress` and `ZstdDecompressor().stream_reader`, `BrotliCompression(quality)` onto `brotli.compress(string, quality=...)`, and seating either in `compressions=` or `accept_compression=` admits its distribution.
- within the branch, one composition root seats every `<Svc>ASGIApplication` under a single dispatcher, sharing one interceptor tuple across the applications and one `pyqwest` `Client` across every `<Svc>Client`.

[LOCAL_ADMISSION]:
- generated `<Svc>ASGIApplication` / `<Svc>Client` pairs are the sole handler and dialer shape; a hand-built `ConnectASGIApplication(endpoints=...)` or a raw `execute_unary(method=MethodInfo(...))` lives only where no generated stub exists.
- `protobuf=py` is the one generator option the estate emits, so `connectrpc.compat` codecs stay out of every fence.
- `connectrpc` and `protoc-gen-connectrpc` pin as one set with `protobuf-py`, and the `_connect.py` tree regenerates on every bump.
- every client takes an injected `pyqwest` `Client` over an `HTTPTransport` the composition root `aclose`s, and no fence leans on `ConnectClient.close()` to release a socket.

[RAIL_LAW]:
- Package: `connectrpc`
- Owns: Connect / gRPC / gRPC-Web protocol handling, generated handler protocols and their ASGI / WSGI applications, typed async and sync clients, interceptors, codecs, compression, and the `ConnectError` status rail
- Accept: `<Svc>ASGIApplication(service)` under an ASGI host, `<Svc>Client(address, protocol=..., http_client=...)` inside `async with`, `ConnectError(Code.X, msg, details)` from handlers, `MetadataInterceptor` for telemetry and auth
- Reject: hand-rolled HTTP framing of Connect or gRPC bodies, a second RPC transport beside this one, status codes spelled as strings or integers, blocking handler bodies on the event loop
