# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_DBAPI]

`opentelemetry-instrumentation-dbapi` owns the generic PEP-249 tracing seam every driver-specific DBAPI instrumentor delegates to: patching a connect callable so each cursor execution emits a db-semconv client span and records the two db-client metric histograms — operation-duration and returned-rows. It carries no `BaseInstrumentor` — a driver without a dedicated contrib instrumentor rides `wrap_connect`/`instrument_connection` directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-dbapi`
- package: `opentelemetry-instrumentation-dbapi`
- module: `opentelemetry.instrumentation.dbapi`
- namespaces: `opentelemetry.instrumentation.dbapi`
- rail: observability
- abi: pure-Python runtime library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: integration factory, cursor tracer, wrapt proxies

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [CAPABILITY]                                                |
| :-----: | :----------------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `DatabaseApiIntegration` | integration factory | span/attribute config, subclassed per driver                |
|  [02]   | `CursorTracer`           | tracer              | sync + async execution spans, statement extraction, metrics |
|  [03]   | `TracedConnectionProxy`  | wrapt proxy         | connection proxy yielding traced cursors                    |
|  [04]   | `TracedCursorProxy`      | wrapt proxy         | cursor proxy tracing `execute`/`executemany`/`callproc`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connect patching, per-connection instrumentation, wrapt proxy construction
- config carry: `connection_attributes`, `tracer_provider`, `meter_provider`, `capture_parameters`, `enable_commenter`, `commenter_options`, `enable_attribute_commenter`, `db_api_integration_factory`

| [INDEX] | [SURFACE]                                                                                  | [SHAPE]    | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------------------------------- | :--------- | :--------------------------- |
|  [01]   | `wrap_connect(name, connect_module, connect_method_name, database_system, **cfg)`          | enable     | patch the connect callable   |
|  [02]   | `unwrap_connect(connect_module, connect_method_name)`                                      | disable    | restore the connect callable |
|  [03]   | `trace_integration(connect_module, connect_method_name, database_system, **cfg)`           | enable     | one-shot wrap convenience    |
|  [04]   | `instrument_connection(name, connection, database_system, **cfg) -> TracedConnectionProxy` | connection | instrument a live connection |
|  [05]   | `uninstrument_connection(connection) -> ConnectionT`                                       | connection | strip a live connection      |
|  [06]   | `get_traced_connection_proxy(connection, db_api_integration, *args, **kwargs)`             | proxy      | build a connection proxy     |
|  [07]   | `get_traced_cursor_proxy(cursor, db_api_integration, *args, **kwargs)`                     | proxy      | build a cursor proxy         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- factory law: a driver-specific span shape subclasses `DatabaseApiIntegration` passed as `db_api_integration_factory`; the subclass overrides attribute derivation, and `CursorTracer` retains execution spans and metric recording.
- async law: `CursorTracer.traced_execution_async` covers async cursors on the one tracer, so an async driver reuses the same `DatabaseApiIntegration` and proxy factories.
- commenter law: `enable_commenter=True` with `commenter_options` appends the sqlcomment KV set to outbound SQL; `enable_attribute_commenter=True` mirrors the comment onto the span attribute.
- parameter law: `capture_parameters=False` is the export posture; an explicit diagnostic opt-in admits only allowlisted statement and parameter shapes, redacted before export, and credentials and PII never allowlist.
- metric law: `DatabaseApiIntegration` builds the two db-client histograms against the resolved meter when a `meter_provider` binds, and `CursorTracer` records into them per execution; `tracer_provider` and `meter_provider` default to the global providers.

[STACKING]:
- `opentelemetry-instrumentation-psycopg`(`.api/opentelemetry-instrumentation-psycopg.md`): `PsycopgInstrumentor` supplies the psycopg connection classes and a `DatabaseApiIntegration` factory through `db_api_integration_factory`; this layer's `CursorTracer` owns the emitted spans and `db.*` attributes.
- `opentelemetry-instrumentation-sqlite3`(`.api/opentelemetry-instrumentation-sqlite3.md`): `SQLite3Instrumentor` patches `sqlite3.connect` through this layer's `wrap_connect`, every stdlib cursor execution riding `CursorTracer`.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): `DatabaseApiIntegration` resolves a `Tracer` via `trace.get_tracer` and builds the two db-client `Histogram` instruments off a `Meter`; `CursorTracer` opens each `SpanKind.CLIENT` span through `start_as_current_span`.
- runtime observability train: a driver with no dedicated contrib instrumentor (duckdb, ADBC DBAPI) enters `wrap_connect`/`instrument_connection` from the `Instrumentation.install` train row, gaining spans with zero data-altitude activation.

[LOCAL_ADMISSION]:
- one `Instrumentation.install` train row wraps a dedicated-instrumentor-less driver; `instrument_connection` covers a connection built before the patch ran, and `capture_parameters` stays `False` outside an explicit redacted diagnostic opt-in.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-dbapi`
- Owns: db-semconv client spans and db-client metric histograms around any PEP-249 cursor execution, and the `DatabaseApiIntegration`/`CursorTracer` substrate the driver instrumentors delegate to
- Accept: a train row wrapping a dedicated-instrumentor-less driver (duckdb, ADBC DBAPI) through `wrap_connect`/`instrument_connection`, a `DatabaseApiIntegration` subclass for a driver-specific span shape
- Reject: direct use against a driver that already carries a contrib instrumentor, activation inside a data or sibling library module, hand-rolled cursor spans beside the patched connect, a second DBAPI wrap stacked on the same connect callable
