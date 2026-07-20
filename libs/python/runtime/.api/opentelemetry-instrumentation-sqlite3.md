# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_SQLITE3]

`opentelemetry-instrumentation-sqlite3` supplies stdlib-sqlite3 DBAPI tracing: one `BaseInstrumentor` patching `sqlite3.connect` so every cursor execution emits a db-semconv client span, with a per-connection form returning an instrumented connection. It covers the stdlib driver alone; the engine's own statistics stay the embedding process's harvest concern.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-sqlite3`
- package: `opentelemetry-instrumentation-sqlite3`
- module: `opentelemetry.instrumentation.sqlite3`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.sqlite3`
- capability: global `sqlite3.connect` patching and per-connection instrument/uninstrument over the dbapi span layer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :-------------------- | :------------ | :--------------------------------------- |
|  [01]   | `SQLite3Instrumentor` | instrumentor  | stdlib `sqlite3` connect patching, spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability
- Connection form is static: `instrument_connection(connection, tracer_provider=None) -> SQLite3Connection` returns the instrumented connection the caller keeps using; `uninstrument_connection(connection)` returns it stripped.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `SQLite3Instrumentor().instrument(**kwargs)`                            | enable         | patch `sqlite3.connect`        |
|  [02]   | `SQLite3Instrumentor().uninstrument(**kwargs)`                          | disable        | unwrap `sqlite3.connect`       |
|  [03]   | `SQLite3Instrumentor.instrument_connection(conn, tracer_provider=None)` | connection     | instrument one live connection |
|  [04]   | `SQLite3Instrumentor.uninstrument_connection(conn)`                     | connection     | strip one live connection      |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- activation law: one `instrument()` at the composition root — the runtime metrics `Instrumentation.install` train row; a connection built before the train ran re-enters through `instrument_connection`.
- coverage law: the patch covers the stdlib `sqlite3` module alone; engine statistics and page-layout evidence ride the embedding process's own harvest, never this span layer.
- provider law: `tracer_provider` defaults to the global provider; SDK install stays the telemetry composition root's.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-sqlite3`
- Owns: db-semconv client spans around every stdlib-sqlite3 cursor execution
- Accept: one train-row `instrument()` at the composition root, `instrument_connection` for a pre-train connection
- Reject: activation inside a data or sibling library module, hand-rolled sqlite spans beside the patched connect
