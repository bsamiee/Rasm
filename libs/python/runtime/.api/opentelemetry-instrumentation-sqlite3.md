# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_SQLITE3]

`opentelemetry-instrumentation-sqlite3` traces the stdlib `sqlite3` driver: a `BaseInstrumentor` patching `sqlite3.connect` so every cursor execution emits a db-semconv client span. Coverage stops at the stdlib driver; engine statistics ride the embedding process's own harvest.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-sqlite3`
- package: `opentelemetry-instrumentation-sqlite3` (Apache-2.0)
- module: `opentelemetry.instrumentation.sqlite3`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :-------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `SQLite3Instrumentor` | instrumentor  | patch `sqlite3.connect`, emit db-semconv spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `SQLite3Instrumentor().instrument(**kwargs)`              | instance | patch `sqlite3.connect`        |
|  [02]   | `SQLite3Instrumentor().uninstrument(**kwargs)`            | instance | unwrap `sqlite3.connect`       |
|  [03]   | `instrument_connection(connection, tracer_provider=None)` | static   | instrument one live connection |
|  [04]   | `uninstrument_connection(connection)`                     | static   | strip one live connection      |

- `instrument_connection`/`uninstrument_connection` return the same `SQLite3Connection` the caller keeps using; a discarded return loses the instrumented handle.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `instrument()` at the composition root patches `sqlite3.connect`; a connection built before that patch re-enters through `instrument_connection`.

[STACKING]:
- `opentelemetry-instrumentation-dbapi`(`.api/opentelemetry-instrumentation-dbapi.md`): `SQLite3Instrumentor` patches `sqlite3.connect` and delegates every cursor span to the dbapi `DatabaseApiIntegration`/`CursorTracer` substrate.
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): `tracer_provider` defaults to `trace.get_tracer_provider()`, a no-op until the SDK installs a real provider at the composition root.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-sqlite3`
- Owns: db-semconv client spans around every stdlib-`sqlite3` cursor execution
- Accept: one train-row `instrument()` at the composition root, `instrument_connection` for a pre-train connection
- Reject: activation inside a data or sibling library module, hand-rolled sqlite spans beside the patched connect
