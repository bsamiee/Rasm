# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_PSYCOPG]

`opentelemetry-instrumentation-psycopg` supplies psycopg DBAPI tracing: `PsycopgInstrumentor` patches `psycopg.connect`, `Connection.connect`, and `AsyncConnection.connect` so every cursor execution emits a db-semconv client span, with an opt-in SQLCommenter stamping trace context into the SQL text. One composition-root train row lands PostgreSQL span coverage on the data query surfaces, no data-altitude activation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-psycopg`
- package: `opentelemetry-instrumentation-psycopg`
- module: `opentelemetry.instrumentation.psycopg`
- rail: observability
- abi: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.psycopg`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `PsycopgInstrumentor` | instrumentor  | sync + async psycopg connect patching, db spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- `instrument` carry: `tracer_provider`, `enable_commenter`, `commenter_options`, `enable_attribute_commenter`, `capture_parameters`.

| [INDEX] | [SURFACE]                                           | [SHAPE]    | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------------------------- |
|  [01]   | `instrument(**kwargs)`                              | enable     | patch `psycopg.connect` + `Connection`/`AsyncConnection` |
|  [02]   | `uninstrument(**kwargs)`                            | disable    | unwrap every patched connect                             |
|  [03]   | `instrument_connection(conn, tracer_provider=None)` | connection | instrument one live connection (static)                  |
|  [04]   | `uninstrument_connection(conn)`                     | connection | strip one live connection (static)                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `instrument()` at the telemetry composition root patches all three connect callables; `instrument_connection` covers a connection built before the train ran.
- `tracer_provider` defaults to the global provider; SDK install stays the telemetry composition root's.

[STACKING]:
- `opentelemetry-instrumentation-dbapi`(`.api/opentelemetry-instrumentation-dbapi.md`): `PsycopgInstrumentor` feeds its `DatabaseApiIntegration`/`DatabaseApiAsyncIntegration` subclasses and the `psycopg.Connection`/`AsyncConnection` classes into `dbapi.wrap_connect`, where `CursorTracer` drives the cursor-execution spans, `db.*` attributes, SQLCommenter injection, and the db-client duration/returned-rows histograms; `commenter_options`, `enable_attribute_commenter`, and `capture_parameters` forward through unchanged.
- runtime observability: the `Instrumentation.install` train row owns the single `instrument()` call, so the data query surfaces gain psycopg spans without importing this package.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-psycopg`
- Owns: db-semconv client spans around every psycopg cursor execution, and the `DatabaseApiIntegration`/`DatabaseApiAsyncIntegration` span-shape subclasses the dbapi layer traces through
- Accept: one train-row `instrument()` at the composition root, `instrument_connection` for a pre-train connection, commenter opt-outs through `commenter_options`
- Reject: activation inside a data or sibling library module, hand-rolled cursor spans beside the patched connect, a second DBAPI instrumentation stacked on the same driver
