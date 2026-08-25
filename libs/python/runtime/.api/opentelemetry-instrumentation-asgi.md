# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_ASGI]

`opentelemetry-instrumentation-asgi` owns the served leg of the trace: one `OpenTelemetryMiddleware` wraps an ASGI application so every HTTP and websocket connection opens a span continued from the inbound W3C headers, records the HTTP server duration, body-size, and active-request instruments, and closes on the terminal send event. Shipping no `BaseInstrumentor`, this surface activates by explicit wrap alone.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: middleware, propagation carriers, and hook aliases

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `OpenTelemetryMiddleware`    | class         | server span, receive and send child spans, HTTP metrics around one ASGI app |
|  [02]   | `ASGIGetter` / `asgi_getter` | getter        | decodes `scope["headers"]` as the W3C extract carrier                       |
|  [03]   | `ASGISetter` / `asgi_setter` | setter        | appends a lowercased header onto a send message as the inject carrier       |
|  [04]   | `ServerRequestHook`          | alias         | `(span, scope) -> None` on the server span                                  |
|  [05]   | `ClientRequestHook`          | alias         | `(span, scope, message) -> None` on each receive child span                 |
|  [06]   | `ClientResponseHook`         | alias         | `(span, scope, message) -> None` on each send child span                    |

[PUBLIC_TYPE_SCOPE]: instruments the constructor mints, selected by stability mode

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                                                     |
| :-----: | :------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `http.server.request.duration`   | histogram       | seconds on the semconv bucket advisory; `http` and `http/dup`    |
|  [02]   | `http.server.duration`           | histogram       | milliseconds rounded and floored at zero; default and `http/dup` |
|  [03]   | `http.server.request.body.size`  | histogram       | bytes off the request `content-length`; `http` and `http/dup`    |
|  [04]   | `http.server.response.body.size` | histogram       | bytes off the response `content-length`; `http` and `http/dup`   |
|  [05]   | `http.server.request.size`       | histogram       | bytes off the request `content-length`; default and `http/dup`   |
|  [06]   | `http.server.response.size`      | histogram       | bytes off the response `content-length`; default and `http/dup`  |
|  [07]   | `http.server.active_requests`    | up-down counter | in-flight HTTP requests under `{request}`; every mode            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: middleware construction and the module helpers it folds
- `OpenTelemetryMiddleware` carries: `app`, `excluded_urls`, `default_span_details`, `server_request_hook`, `client_request_hook`, `client_response_hook`, `tracer_provider`, `meter_provider`, `tracer`, `meter`, `http_capture_headers_server_request`, `http_capture_headers_server_response`, `http_capture_headers_sanitize_fields`, `exclude_spans` — every slot past `app` positional-or-keyword, defaulting to `None`.
- `collect_custom_headers_attributes` carries: `scope_or_response_message`, `sanitize`, `header_regexes`, `normalize_names`; `set_status_code` carries: `span`, `status_code`, `metric_attributes`, `sem_conv_opt_in_mode`.
- Header-capture slots left `None` fall back to `OTEL_INSTRUMENTATION_HTTP_CAPTURE_HEADERS_SERVER_REQUEST`, `_SERVER_RESPONSE`, and `_SANITIZE_FIELDS`, each a comma-separated regex list matched case-insensitively.
- `SanitizeValue` and `normalise_request_header_name` / `normalise_response_header_name` supply the `sanitize` and `normalize_names` arguments, re-exported here from `opentelemetry.util.http`.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `OpenTelemetryMiddleware(app, ...)`                                 | ctor     | wrap one ASGI app, reading the opt-in once            |
|  [02]   | `middleware(scope, receive, send)`                                  | instance | the awaitable ASGI callable a host serves             |
|  [03]   | `get_default_span_details(scope)`                                   | static   | `(name, attributes)`; `default_span_details` wins     |
|  [04]   | `collect_request_attributes(scope, sem_conv_opt_in_mode)`           | static   | the semconv request attributes one scope yields       |
|  [05]   | `collect_custom_headers_attributes(scope_or_response_message, ...)` | static   | sanitized captured-header attributes                  |
|  [06]   | `get_host_port_url_tuple(scope)`                                    | static   | `(server_host, port, url)` the attributes derive from |
|  [07]   | `set_status_code(span, status_code, ...)`                           | static   | span status plus the response-status attribute        |
|  [08]   | `asgi_getter.get(carrier, key)`                                     | instance | header values as a list, `None` where absent          |
|  [09]   | `asgi_getter.keys(carrier)`                                         | instance | every decoded header name on the carrier              |
|  [10]   | `asgi_setter.set(carrier, key, value)`                              | instance | append one lowercased header onto a send message      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `is_http_instrumentation_enabled()` reading false, or a `scope["type"]` outside `http` and `websocket`, hands the call straight to the wrapped app, so a lifespan scope and a suppressed leg each cost one dict read.
- `excluded_urls` regex-searches the FULL url `get_host_port_url_tuple` builds — scheme, host, port, and path — never the path alone; a string argument parses through `parse_excluded_urls` into an `ExcludeList` at construction.
- `_start_internal_or_server_span` opens `SpanKind.SERVER` under `asgi_getter` extraction where no local span is active and `SpanKind.INTERNAL` where one is, and both request- and response-header capture gate on that `SERVER` kind.
- Hooks run wrapped in `failsafe`: an exception raised inside one lands on its span through `record_exception` and never reaches the request, so a faulty hook reports itself and the served call survives.
- `otel_send` closes the server span on the last `http.response.body`, or on the last `http.response.trailers` once an `http.response.start` announced `trailers`, so a gRPC-over-HTTP/2 call closes at its trailers carrying the real status.
- `__call__` ends the span again in its `finally` arm behind an `is_recording()` guard, so an application dying mid-response still closes a span its send path never terminated.
- Metric recording gates on the `http` scope type alone: a websocket connection opens a span and runs its hooks while emitting no duration, size, or active-request measurement.
- `content_length_header` lives on the MIDDLEWARE rather than the request, so concurrent responses overwrite one another's byte count and the recorded response size belongs to whichever send landed last.
- `_collect_target_attribute` reads `scope["route"].path_format` behind `root_path`, so a host seating no `route` records the duration histograms with no low-cardinality target and leaves the legacy `http.target` attribute unset.
- `tracer` and `meter` bypass `tracer_provider` and `meter_provider` whole, so an instrument passed beside its provider drops the provider silently.
- `OTEL_SEMCONV_STABILITY_OPT_IN` resolves once, at the first construction in the process, so a later mutation moves no instrument the middleware already minted.
- `get_global_response_propagator()` injects onto every send message through `asgi_setter`, so installing `TraceResponsePropagator` publishes `traceresponse` to the client.
- `exclude_spans` drops per-event children by name: `receive` returns the raw callable and `send` skips its child span, while status, header capture, and response propagation stay on the server span.
- Header capture stamps `http.request.header.<name>` and `http.response.header.<name>` as string lists off a `fullmatch` against the header name, and a `http_capture_headers_sanitize_fields` hit replaces the value with `[REDACTED]`.

[STACKING]:
- `connectrpc`(`libs/python/.api/connectrpc.md`): `OpenTelemetryMiddleware(<Svc>ASGIApplication(service))` is the served application, `server_request_hook` stamps rpc attributes off the `scope["path"]` a `ConnectASGIApplication.path` prefix mounts, and the trailers close carries the status `RequestContext.response_trailers` writes.
- `hypercorn`(`.api/hypercorn.md`): the wrapped callable is what `hypercorn.asyncio.serve(app, config)` hosts, `DispatcherMiddleware({app.path: app})` mounts several wrapped services on one listener, and the trailers close reads the `HTTPResponseTrailersEvent` this host emits.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): `asgi_getter` is the `Getter[dict]` `propagate.extract` consumes off the `set_global_textmap` composite, `trace.use_span(span, end_on_exit=False)` activates the server span, and the instruments land through `Histogram.record` and `UpDownCounter.add` on a `metrics.get_meter` meter.
- `opentelemetry-instrumentation`(`.api/opentelemetry-instrumentation.md`): this package ships no `BaseInstrumentor`, reaching that surface through `is_http_instrumentation_enabled()`, the `get_global_response_propagator()` seat `TraceResponsePropagator.inject` fills, and the `_StabilityMode` the opt-in resolves.
- `opentelemetry-semantic-conventions`(`.api/opentelemetry-semantic-conventions.md`): `metrics.http_metrics.HTTP_SERVER_REQUEST_DURATION` names the stable histogram, `_incubating.metrics.http_metrics.create_http_server_active_requests` mints the up-down counter under every mode, and `_incubating.attributes.http_attributes.HTTP_TARGET` keys the legacy duration attribute.
- runtime composition root: one wrap sits between the generated Connect application and `serve`, holding the branch's hook pair, exclusion list, and captured-header roster as declared values beside the provider install.

[LOCAL_ADMISSION]:
- One wrap at the composition root around the application handed to `serve`; no second ASGI tracing layer binds the same app.
- `server_request_hook` owns request-derived enrichment; attributes a handler computes ride the handler's own child span.
- `exclude_spans=["receive", "send"]` is the standing posture on a streaming rail, where per-event children multiply span volume by message count.
- Header capture arrives through the constructor slots so the captured and sanitized sets stay declared and reviewable at the composition root.
- `excluded_urls` patterns anchor on path segments, since the match runs against a url carrying scheme, host, and port.
