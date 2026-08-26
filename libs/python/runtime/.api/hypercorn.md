# [PY_RUNTIME_API_HYPERCORN]

`hypercorn` owns the ASGI host beneath the Connect server: one `Config` declares listeners, TLS, protocol limits, and lifecycle timeouts, and `hypercorn.asyncio.serve` or `hypercorn.trio.serve` runs an ASGI or WSGI application on them inside the caller's event loop. One `bind` grammar spells TCP, UNIX-socket, and inherited-descriptor listeners; HTTP/2 arrives by ALPN over TLS and as h2c over plaintext.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration and host

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `Config`         | class         | listener, TLS, protocol-limit, logging, and lifecycle knobs as class attributes |
|  [02]   | `Sockets`        | dataclass     | `secure_sockets` `insecure_sockets` `quic_sockets` that `create_sockets` binds  |
|  [03]   | `Logger`         | class         | async `access` `atoms` `log` and level sinks over stdlib handlers               |
|  [04]   | `AccessLogAtoms` | dict          | `%(h)s`-keyed atoms `access_log_format` interpolates                            |
|  [05]   | `StatsdLogger`   | class         | `Logger` plus `increment` `decrement` `gauge` `histogram` onto `statsd_host`    |
|  [06]   | `ASGIWrapper`    | class         | admits an ASGI callable once; `wrap_app` mints it                               |
|  [07]   | `WSGIWrapper`    | class         | runs a WSGI callable on a worker thread under `max_body_size`                   |

[PUBLIC_TYPE_SCOPE]: middleware

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `DispatcherMiddleware`                         | ASGI          | mount-prefix router on asyncio; an unmatched path answers 404     |
|  [02]   | `TrioDispatcherMiddleware`                     | ASGI          | same router under trio, from `hypercorn.middleware.dispatcher`    |
|  [03]   | `ProxyFixMiddleware`                           | ASGI          | rewrites `client` `scheme` `host` from the trusted forwarded hop  |
|  [04]   | `HTTPToHTTPSRedirectMiddleware`                | ASGI          | 307 to `https://<host>`; a websocket redirects or closes          |
|  [05]   | `AsyncioWSGIMiddleware` / `TrioWSGIMiddleware` | ASGI          | hosts a WSGI app on the ASGI loop under a bounded `max_body_size` |

[PUBLIC_TYPE_SCOPE]: typing (`hypercorn.typing`)

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Framework`                                            | union alias    | `ASGIFramework \| WSGIFramework`, the `serve` app parameter  |
|  [02]   | `ASGIFramework` / `WSGIFramework`                      | callable alias | `(scope, receive, send)` and `(environ, start_response)`     |
|  [03]   | `HTTPScope` / `WebsocketScope` / `LifespanScope`       | TypedDict      | `Scope` unions all three, `WWWScope` the two request shapes  |
|  [04]   | `HTTPRequestEvent` … `HTTPEarlyHintEvent`              | TypedDict      | http body, trailers, push, early-hint, and disconnect events |
|  [05]   | `WebsocketConnectEvent` … `WebsocketCloseEvent`        | TypedDict      | websocket handshake, message, response, and close events     |
|  [06]   | `LifespanStartupEvent` … `LifespanShutdownFailedEvent` | TypedDict      | lifespan events the startup and shutdown timeouts bound      |
|  [07]   | `ASGIReceiveEvent` / `ASGISendEvent`                   | union          | closed event unions the two channels carry                   |
|  [08]   | `ASGIReceiveCallable` / `ASGISendCallable`             | callable alias | the two channels an application is handed                    |
|  [09]   | `ResponseSummary`                                      | TypedDict      | `status` `headers` that `Logger.access` reads                |
|  [10]   | `ASGIVersions`                                         | TypedDict      | `spec_version` `version` under `scope['asgi']`               |
|  [11]   | `ConnectionState` / `LifespanState`                    | dict alias     | per-connection and per-process `state` carriers              |
|  [12]   | `AppWrapper` / `WorkerContext` / `TaskGroup` / `Event` | protocol       | backend boundaries asyncio and trio each implement           |
|  [13]   | `SingleTask` / `H2SyncStream` / `H2AsyncStream`        | protocol       | restartable task and HTTP/2 stream boundaries                |
|  [14]   | `WorkerFunc`                                           | callable alias | `(config, sockets, shutdown_event)` worker `run` forks       |
|  [15]   | `H11SendableEvent`                                     | union          | h11 events the HTTP/1.1 protocol writes                      |

[PUBLIC_TYPE_SCOPE]: connection events (`hypercorn.events`)

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------- | :------------ | :------------------------------------------------ |
|  [01]   | `Event`   | ABC           | base of the server-to-protocol IO vocabulary      |
|  [02]   | `RawData` | dataclass     | `data` and optional `address` read off a listener |
|  [03]   | `Closed`  | dataclass     | peer closed the connection                        |
|  [04]   | `Updated` | dataclass     | `idle` flip the keep-alive reaper reads           |

[PUBLIC_TYPE_SCOPE]: faults

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `LifespanTimeoutError(stage)`          | exception     | `startup_timeout` or `shutdown_timeout` elapsed; escapes `serve` |
|  [02]   | `LifespanFailureError(stage, message)` | exception     | app answered `lifespan.*.failed`; escapes `serve`                |
|  [03]   | `UnexpectedMessageError(state, type)`  | exception     | app sent an ASGI message its stream state forbids                |
|  [04]   | `NoAppError`                           | exception     | `load_application` resolved no module or no attribute            |
|  [05]   | `SocketTypeError(expected, actual)`    | exception     | `fd://<n>` named a descriptor of the wrong socket kind           |
|  [06]   | `ShutdownError`                        | exception     | `raise_shutdown` fires it; `worker_serve` swallows it            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serving
- `hypercorn.asyncio` and `hypercorn.trio` each export `serve` and `worker_serve`, and the trio flavour of both adds `task_status`.

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `asyncio.serve(app, config, *, shutdown_trigger, mode)`           | static  | host on the running asyncio loop until the trigger fires  |
|  [02]   | `trio.serve(app, config, *, shutdown_trigger, task_status, mode)` | static  | host under trio; `task_status.started(binds)` lists binds |
|  [03]   | `asyncio.worker_serve(app, config, *, sockets, shutdown_trigger)` | static  | serve an already-wrapped app on caller-bound `Sockets`    |
|  [04]   | `trio.worker_serve(app, config, *, sockets, shutdown_trigger, …)` | static  | same under trio, adding `task_status`                     |
|  [05]   | `run.run(config) -> int`                                          | static  | fork-and-supervise behind the CLI; returns the exit code  |

- `hypercorn.asyncio.serve`: with no `shutdown_trigger` it seizes `SIGINT` `SIGTERM` `SIGBREAK` on the caller's loop.
- `hypercorn.asyncio.serve`: `mode=None` picks ASGI or WSGI by `is_asgi`; `debug` and `workers` warn and decide nothing here.

[ENTRYPOINT_SCOPE]: configuration loading

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `Config()`                               | ctor    | takes no arguments; every knob lands by attribute assignment   |
|  [02]   | `Config.from_mapping(mapping, **kwargs)` | factory | mapping then kwargs; a key no attribute accepts drops silently |
|  [03]   | `Config.from_toml(filename)`             | factory | TOML file folded through `from_mapping`                        |
|  [04]   | `Config.from_pyfile(filename)`           | factory | executes the file, then reads it through `from_object`         |
|  [05]   | `Config.from_object(instance)`           | factory | object, module, or dotted string; dunder and module names skip |

[ENTRYPOINT_SCOPE]: `Config` operations

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Config.create_sockets() -> Sockets`                | instance | bind every listener ahead of serving                      |
|  [02]   | `Config.create_ssl_context() -> SSLContext \| None` | instance | TLS 1.2 floor, compression off, ALPN and ciphers seated   |
|  [03]   | `Config.response_headers(protocol)`                 | instance | `date`, `server`, and `alt-svc` rows for every response   |
|  [04]   | `Config.set_statsd_logger_class(statsd_logger)`     | instance | seat the backend statsd logger while `statsd_host` is set |
|  [05]   | `Config.ssl_enabled -> bool`                        | property | true once `certfile` and `keyfile` both land              |
|  [06]   | `Config.log -> Logger`                              | property | memoizes one `logger_class(config)` per config            |

[ENTRYPOINT_SCOPE]: `hypercorn.utils` helpers

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `wrap_app(app, wsgi_max_body_size, mode)`                  | static  | mint the wrapper; `mode=None` decides through `is_asgi`   |
|  [02]   | `load_application(path, wsgi_max_body_size)`               | static  | import `[mode:]module:app`; raises `NoAppError` on a miss |
|  [03]   | `is_asgi(app)`                                             | static  | coroutine-function test over the app or its `__call__`    |
|  [04]   | `build_and_validate_headers(headers)`                      | static  | strip each pair, refusing any `:`-prefixed name           |
|  [05]   | `filter_pseudo_headers(headers)`                           | static  | drop pseudo headers, folding `:authority` into `host`     |
|  [06]   | `suppress_body(method, status_code)`                       | static  | `HEAD`, 1xx, 204, and 304 carry no body                   |
|  [07]   | `valid_server_name(config, request)`                       | static  | empty `server_names` admits every `Host`                  |
|  [08]   | `parse_socket_addr(family, address)`                       | static  | `(host, port)` for INET and INET6, `None` elsewhere       |
|  [09]   | `repr_socket_addr(family, address)`                        | static  | log spelling, `unix:<path>` included                      |
|  [10]   | `raise_shutdown(shutdown_event)`                           | static  | await the trigger, then raise `ShutdownError`             |
|  [11]   | `check_multiprocess_shutdown_event(shutdown_event, sleep)` | static  | poll a multiprocessing event as a trigger                 |
|  [12]   | `files_to_watch()` / `check_for_updates(files)`            | static  | the reloader's module-mtime census and its verdict        |
|  [13]   | `write_pid_file(pid_path)`                                 | static  | write the current pid to `pid_path`                       |

[ENTRYPOINT_SCOPE]: middleware construction

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------- | :------ | :------------------------------------------------------------ |
|  [01]   | `DispatcherMiddleware(mounts)`                   | ctor    | `{prefix: app}`; the match extends `root_path` by the prefix  |
|  [02]   | `ProxyFixMiddleware(app, mode, trusted_hops)`    | ctor    | `mode='legacy'` reads `X-Forwarded-*`, `'modern'` `Forwarded` |
|  [03]   | `HTTPToHTTPSRedirectMiddleware(app, host)`       | ctor    | `host=None` reads the request `Host`, else raises on absence  |
|  [04]   | `AsyncioWSGIMiddleware(wsgi_app, max_body_size)` | ctor    | `max_body_size=65536`, independent of `wsgi_max_body_size`    |
|  [05]   | `TrioWSGIMiddleware(wsgi_app, max_body_size)`    | ctor    | the trio flavour of the same wrapper                          |

[ENTRYPOINT_SCOPE]: `Config` listener and TLS knobs
- every row below assigns onto a constructed `Config`, and the value shown is the live default.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                                             |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------------------------------------- |
|  [01]   | `bind = ['127.0.0.1:8000']`                  | property | `host:port`, `unix:<path>`, `fd://<n>`; a bare str wraps to a list       |
|  [02]   | `insecure_bind = []`                         | property | plaintext listeners beside a TLS `bind`; ignored while not `ssl_enabled` |
|  [03]   | `quic_bind = []`                             | property | HTTP/3 UDP listeners, bound only while `ssl_enabled`                     |
|  [04]   | `certfile = None` / `keyfile = None`         | instance | both set flips `ssl_enabled` and moves `bind` onto TLS                   |
|  [05]   | `keyfile_password = None`                    | instance | passphrase for an encrypted private key                                  |
|  [06]   | `ca_certs = None`                            | instance | verify locations for client certificates                                 |
|  [07]   | `ciphers = 'ECDHE+AESGCM'`                   | instance | cipher string the context loads                                          |
|  [08]   | `verify_mode = None` / `verify_flags = None` | instance | `ssl.VerifyMode` and `ssl.VerifyFlags` on the context                    |
|  [09]   | `alpn_protocols = ['h2', 'http/1.1']`        | instance | TLS ALPN offer, ordered to prefer HTTP/2                                 |
|  [10]   | `ssl_handshake_timeout = 60.0`               | instance | handshake deadline on every TLS accept                                   |
|  [11]   | `backlog = 100`                              | instance | listen backlog per socket                                                |
|  [12]   | `server_names = []`                          | instance | `Host` allow-list; a mismatch answers 404                                |
|  [13]   | `root_path = ''`                             | property | ASGI `root_path`; the setter strips trailing `/`                         |

[ENTRYPOINT_SCOPE]: `Config` protocol limits

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `h11_max_incomplete_size = 16384`       | instance | HTTP/1.1 incomplete-request ceiling             |
|  [02]   | `h11_pass_raw_headers = False`          | instance | hand unnormalized header names to the scope     |
|  [03]   | `h2_max_concurrent_streams = 100`       | instance | per-connection stream ceiling                   |
|  [04]   | `h2_max_header_list_size = 65536`       | instance | HTTP/2 header-list ceiling                      |
|  [05]   | `h2_max_inbound_frame_size = 16384`     | instance | HTTP/2 inbound frame ceiling                    |
|  [06]   | `websocket_max_message_size = 16777216` | instance | an oversize message closes the socket with 1009 |
|  [07]   | `websocket_ping_interval = None`        | instance | server ping cadence in seconds                  |
|  [08]   | `wsgi_max_body_size = 16777216`         | instance | body ceiling `wrap_app` hands the WSGI wrapper  |
|  [09]   | `max_app_queue_size = 10`               | instance | per-connection ASGI receive-queue depth         |

[ENTRYPOINT_SCOPE]: `Config` timeouts and recycling

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `read_timeout = None`                                | instance | deadline on each socket read                            |
|  [02]   | `keep_alive_timeout = 5.0`                           | instance | idle window before the connection closes                |
|  [03]   | `keep_alive_max_requests = 1000`                     | instance | request ceiling per HTTP/2 connection                   |
|  [04]   | `graceful_timeout = 3.0`                             | instance | in-flight drain window after the listeners close        |
|  [05]   | `startup_timeout = 60.0` / `shutdown_timeout = 60.0` | instance | lifespan deadlines, each raising `LifespanTimeoutError` |
|  [06]   | `max_requests = None` / `max_requests_jitter = 0`    | instance | worker retires at `max_requests + randint(0, jitter)`   |

[ENTRYPOINT_SCOPE]: `Config` process knobs the `run` supervisor reads

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `application_path`                              | instance | `[mode:]module:app` string `run` loads; carries no default |
|  [02]   | `workers = 1`                                   | instance | forked worker count; above one sets `SO_REUSEPORT`         |
|  [03]   | `worker_class = 'asyncio'`                      | instance | `asyncio`, `uvloop`, or `trio`; any other value raises     |
|  [04]   | `use_reloader = False`                          | instance | restart workers when a watched module file changes         |
|  [05]   | `debug = False`                                 | instance | asyncio debug mode inside the forked worker                |
|  [06]   | `daemon = True`                                 | instance | mark each forked worker a daemon process                   |
|  [07]   | `pid_path = None`                               | instance | write the supervisor pid before forking                    |
|  [08]   | `user = None` / `group = None` / `umask = None` | instance | ownership and mode applied to a `unix:` socket             |

[ENTRYPOINT_SCOPE]: `Config` logging, metrics, and response headers
- `access_log_format` defaults to `'%(h)s %(l)s %(l)s %(t)s "%(r)s" %(s)s %(b)s "%(f)s" "%(a)s"'`.
- `AccessLogAtoms` also keys `{header}i` on request headers, `{header}o` on response headers, and `{var}e` on the environment.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `accesslog = None`                                            | instance | access sink; unset writes no access line at all       |
|  [02]   | `errorlog = '-'`                                              | instance | `'-'` is stderr, a `logging.Logger` passes through    |
|  [03]   | `loglevel = 'INFO'`                                           | instance | level on both derived loggers                         |
|  [04]   | `access_log_format`                                           | instance | atom template `AccessLogAtoms` fills                  |
|  [05]   | `logconfig = None`                                            | instance | `json:` or `toml:` for `dictConfig`, else an ini path |
|  [06]   | `logconfig_dict = None`                                       | instance | `dictConfig` mapping read when `logconfig` is unset   |
|  [07]   | `logger_class = Logger`                                       | instance | class `Config.log` instantiates once                  |
|  [08]   | `statsd_host = None` / `statsd_prefix = ''`                   | instance | `host:port` UDP sink and metric-name prefix           |
|  [09]   | `dogstatsd_tags = ''`                                         | instance | tag string appended to every statsd line              |
|  [10]   | `include_date_header = True` / `include_server_header = True` | instance | `date` and `server: hypercorn-<protocol>` rows        |
|  [11]   | `alt_svc_headers = []`                                        | instance | empty derives H3 rows from the bound QUIC ports       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `serve` runs inside the caller's event loop and returns when `shutdown_trigger` resolves, so the composition root owns the loop and `hypercorn.run.run` stays outside every fence.
- `create_sockets` reads `ssl_enabled`: without both `certfile` and `keyfile` the whole `bind` roster becomes plaintext, `insecure_bind` binds nothing, and every `quic_bind` socket drops — a TLS-less h2c host spells its listener on `bind`.
- HTTP/2 reaches a plaintext listener two ways — the `PRI * HTTP/2.0` preface, and `Upgrade: h2c` on a bodyless request — while an upgrade request carrying a body stays on HTTP/1.1.
- `http.response.trailers`, `http.response.push`, and `http.response.early_hint` seat in `scope['extensions']` on HTTP/2 and HTTP/3 alone, so a gRPC status trailer needs one of the two.
- `bind` spells every listener kind: `unix:<path>` removes a stale socket and applies `umask` `user` `group`, `fd://<n>` inherits a descriptor and refuses a wrong socket kind, and a bare host defaults to port 8000.
- Shutdown drains in order — listeners close, `graceful_timeout` bounds the in-flight gather, then `shutdown_timeout` bounds the lifespan leg — while `startup_timeout` bounds admission.
- `server_names` decides before the application spawns: a mismatched `Host` answers 404, and an empty roster admits every host.

[STACKING]:
- `connectrpc`(`libs/python/.api/connectrpc.md`): `serve(<Svc>ASGIApplication(service), config)` is the server, `DispatcherMiddleware({app.path: app, ...})` mounts several generated applications on one listener, and gRPC and bidi streams need `alpn_protocols` HTTP/2 or an h2c `insecure_bind` for the trailers extension their status rides.
- `opentelemetry-instrumentation-asgi`(`.api/opentelemetry-instrumentation-asgi.md`): `OpenTelemetryMiddleware(app)` is the value handed to `serve`, and its server span closes on the final send event the ASGI stream emits.
- `anyio`(`libs/python/.api/anyio.md`): `anyio.run(partial(serve, app, config, shutdown_trigger=stop.wait), backend='asyncio')` hosts the result, `shutdown_trigger` binding one `anyio.Event` the composition root sets; `create_sockets()` then `worker_serve(wrap_app(app, config.wsgi_max_body_size, 'asgi'), config, sockets=...)` splits bind from serve where readiness must follow the bind.
- `cyclopts`(`.api/cyclopts.md`): `App.command` registers the async serve function that assigns `bind`, TLS paths, and timeouts onto one `Config`, then awaits `serve` under `App.run_async(backend='asyncio')`.
- `runtime/transport/serve`: seats the one `Config` and the `serve` await at the daemon entry, binding `shutdown_trigger` onto its termination latch and `graceful_timeout` onto the drain, so no other fence opens a listener.

[LOCAL_ADMISSION]:
- one `Config` built at the composition root from typed settings; the `Config.from_*` loaders and the `hypercorn` CLI stay outside every fence.
- `asyncio` is the loop flavour the server runs; `hypercorn.trio.serve` hosts no Connect application, whose server runs asyncio primitives, so it stays outside the branch fences.
- TLS client verification rides `verify_mode` and `verify_flags`; the UNIX-socket h2c boundary is a plaintext `bind`, and `insecure_bind` exists beside a TLS `bind` alone.
