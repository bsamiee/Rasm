# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_DBAPI]

`opentelemetry-instrumentation-dbapi` supplies the generic PEP-249 tracing seam every driver-specific DBAPI instrumentor delegates to: module-level `wrap_connect`/`instrument_connection` patch a connect callable so cursor execution emits db-semconv client spans, `DatabaseApiIntegration` owns span and attribute configuration, and `CursorTracer` drives sync and async execution tracing and the two db-client metric histograms. It carries no `BaseInstrumentor`; it is the wrap layer a driver without a dedicated contrib instrumentor (duckdb, ADBC DBAPI) rides directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-dbapi`
- package: `opentelemetry-instrumentation-dbapi`
- module: `opentelemetry.instrumentation.dbapi`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.dbapi`
- capability: generic connect-callable patching, per-connection instrumentation, `wrapt` proxy factories over connection and cursor, SQLCommenter injection, query-parameter capture, and db-client operation-duration and returned-rows metric histograms

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: integration factory, cursor tracer, wrapt proxies
- rail: observability
- `DatabaseApiIntegration` subclasses feed the driver instrumentors through the `db_api_integration_factory` kwarg; span names and `db.*` attributes originate here, so a driver instrumentor supplies connection classes and this layer supplies span mechanics.
- `CursorTracer` owns statement extraction, span population, and metric recording; both `traced_execution` and `traced_execution_async` live on it, so async coverage rides the one tracer without a separate integration class.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [RAIL]                                                      |
| :-----: | :----------------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `DatabaseApiIntegration` | integration factory | span/attribute config, subclassed per driver                |
|  [02]   | `CursorTracer`           | tracer              | sync + async execution spans, statement extraction, metrics |
|  [03]   | `TracedConnectionProxy`  | wrapt proxy         | connection proxy yielding traced cursors                    |
|  [04]   | `TracedCursorProxy`      | wrapt proxy         | cursor proxy tracing `execute`/`executemany`/`callproc`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connect patching, per-connection instrumentation, proxy and metric construction
- rail: observability
- `wrap_connect`/`trace_integration` patch a connect callable globally; `instrument_connection` covers a connection built before the patch ran and returns the traced proxy the caller keeps using.
- span config kwargs shared across entrypoints: `tracer_provider`, `meter_provider`, `capture_parameters`, `enable_commenter`, `commenter_options`, `enable_attribute_commenter`, `db_api_integration_factory`, `connection_attributes`. `capture_parameters=False` stays fixed outside an explicit redacted diagnostic opt-in.
- metric factories build the two db-client histograms once against a meter; `CursorTracer` records into them per execution.

```python signature
wrap_connect(name, connect_module, connect_method_name, database_system, **kwargs)
unwrap_connect(connect_module, connect_method_name)
trace_integration(connect_module, connect_method_name, database_system, **kwargs)
instrument_connection(name, connection, database_system, **kwargs) -> TracedConnectionProxy
uninstrument_connection(connection) -> ConnectionT
get_traced_connection_proxy(connection, db_api_integration, *args, **kwargs)
get_traced_cursor_proxy(cursor, db_api_integration, *args, **kwargs)
create_db_client_operation_duration(meter) -> Histogram
create_db_client_response_returned_rows(meter) -> Histogram
```

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `wrap_connect`                            | enable         | patch a driver connect callable            |
|  [02]   | `unwrap_connect`                          | disable        | restore the original connect callable      |
|  [03]   | `trace_integration`                       | enable         | one-shot wrap convenience                  |
|  [04]   | `instrument_connection`                   | connection     | instrument one live connection             |
|  [05]   | `uninstrument_connection`                 | connection     | strip one live connection                  |
|  [06]   | `get_traced_connection_proxy`             | proxy          | build a connection proxy                   |
|  [07]   | `get_traced_cursor_proxy`                 | proxy          | build a cursor proxy                       |
|  [08]   | `create_db_client_operation_duration`     | metric         | db.client.operation.duration histogram     |
|  [09]   | `create_db_client_response_returned_rows` | metric         | db.client.response.returned_rows histogram |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- seam law: this layer sits under the driver instrumentors; a driver carrying its own contrib instrumentor (psycopg, sqlite3) rides that instrumentor and never touches this package directly, while a driver without one (duckdb, ADBC DBAPI) enters through `wrap_connect` or `instrument_connection` from the runtime train row that owns it.
- factory law: a driver-specific span shape subclasses `DatabaseApiIntegration` and passes the subclass as `db_api_integration_factory`; the subclass overrides attribute derivation only, and `CursorTracer` keeps ownership of execution spans and metric recording.
- async law: `CursorTracer.traced_execution_async` covers async cursors on the one tracer; no separate async integration class exists, so an async driver reuses the same `DatabaseApiIntegration` and proxy factories.
- commenter law: `enable_commenter=True` with `commenter_options` appends the sqlcomment KV set to the outbound SQL, and `enable_attribute_commenter=True` mirrors the comment onto the span attribute.
- parameter law: `capture_parameters=False` is the export posture; explicit diagnostic opt-in admits only allowlisted statement and parameter shapes, and the telemetry processor redacts `db.statement.parameters` before export. Credentials and PII are never allowlisted.
- metric law: `create_db_client_operation_duration` and `create_db_client_response_returned_rows` build the db-client histograms against the resolved meter; `meter_provider` defaults to the global provider, `tracer_provider` likewise, and SDK install stays the telemetry composition root's.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-dbapi`
- Owns: db-semconv client spans and db-client metric histograms around any PEP-249 cursor execution, and the `DatabaseApiIntegration`/`CursorTracer` substrate the driver instrumentors delegate to
- Accept: a runtime train row wrapping a dedicated-instrumentor-less driver (duckdb, ADBC DBAPI) through `wrap_connect`/`instrument_connection`, a `DatabaseApiIntegration` subclass for a driver-specific span shape
- Reject: direct use against a driver that already carries a contrib instrumentor, activation inside a data or sibling library module, hand-rolled cursor spans beside the patched connect, a second DBAPI wrap stacked on the same connect callable
