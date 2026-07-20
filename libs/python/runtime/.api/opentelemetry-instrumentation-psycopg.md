# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_PSYCOPG]

`opentelemetry-instrumentation-psycopg` supplies psycopg DBAPI tracing: one `BaseInstrumentor` that patches `psycopg.connect`, `Connection.connect`, and `AsyncConnection.connect` so every cursor execution emits a db-semconv client span, with an opt-in SQLCommenter that stamps trace context into the SQL text. It is the train row that gives the data query surfaces their PostgreSQL span coverage with zero data-altitude activation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-psycopg`
- package: `opentelemetry-instrumentation-psycopg`
- module: `opentelemetry.instrumentation.psycopg`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.psycopg`
- capability: global sync + async psycopg connect patching, per-connection instrumentation, SQLCommenter key opt-out via `commenter_options`, span-attribute SQL comment via `enable_attribute_commenter`, and query-parameter capture via `capture_parameters`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability
- span mechanics delegate to `opentelemetry-instrumentation-dbapi`: the instrumentor supplies the psycopg connection classes and `DatabaseApiIntegration`/`DatabaseApiAsyncIntegration` factories; span names and `db.*` attributes are the dbapi layer's.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `PsycopgInstrumentor` | instrumentor  | sync + async psycopg connect patching, db spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability
- `instrument` kwargs: `tracer_provider`, `enable_commenter`, `commenter_options`, `enable_attribute_commenter`, `capture_parameters`.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `instrument(**kwargs)`                              | enable         | patch `psycopg.connect` + `Connection`/`AsyncConnection` |
|  [02]   | `uninstrument(**kwargs)`                            | disable        | unwrap every patched connect                             |
|  [03]   | `instrument_connection(conn, tracer_provider=None)` | connection     | instrument one live connection (static)                  |
|  [04]   | `uninstrument_connection(conn)`                     | connection     | strip one live connection (static)                       |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- activation law: one `instrument()` at the composition root — the runtime metrics `Instrumentation.install` train row — so the data query surfaces gain spans without importing this package; `instrument_connection` covers a connection built before the train ran.
- commenter law: `instrument(enable_commenter=True, commenter_options={...})` appends the sqlcomment KV set to the outbound SQL; `commenter_options` opts individual keys OUT, and `enable_attribute_commenter=True` mirrors the comment onto the span attribute.
- provider law: `tracer_provider` defaults to the global provider; the SDK install stays the telemetry composition root's.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-psycopg`
- Owns: db-semconv client spans around every psycopg cursor execution
- Accept: one train-row `instrument()` at the composition root, `instrument_connection` for a pre-train connection, commenter opt-outs through `commenter_options`
- Reject: activation inside a data or sibling library module, hand-rolled cursor spans beside the patched connect, a second DBAPI instrumentation stacked on the same driver
