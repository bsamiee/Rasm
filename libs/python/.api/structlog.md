# [PY_BRANCH_API_STRUCTLOG]

`structlog` supplies structured logging through a processor chain: context binding (`bind`/`unbind`), contextvars-backed ambient context, a `ProcessorFormatter` bridge to stdlib `logging`, a `ConsoleRenderer` for human-readable dev output, `JSONRenderer` for production, callsite injection, exception rendering, and a `FilteringBoundLogger` for level-filtered loggers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `structlog`
- package: `structlog`
- module: `structlog`
- asset: runtime library
- rail: observability

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger and factory types
- rail: observability

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :-------------------- | :------------ | :--------------------------------- |
|   [1]   | `BoundLogger`         | bound logger  | stdlib-compatible structlog logger |
|   [2]   | `BoundLoggerBase`     | abstract base | minimal bound logger contract      |
|   [3]   | `PrintLogger`         | sync logger   | print-to-stdout wrapped logger     |
|   [4]   | `PrintLoggerFactory`  | factory       | factory for PrintLogger            |
|   [5]   | `BytesLogger`         | sync logger   | write-bytes wrapped logger         |
|   [6]   | `BytesLoggerFactory`  | factory       | factory for BytesLogger            |
|   [7]   | `WriteLogger`         | sync logger   | write-to-file wrapped logger       |
|   [8]   | `WriteLoggerFactory`  | factory       | factory for WriteLogger            |
|   [9]   | `ReturnLogger`        | test logger   | returns log event for testing      |
|  [10]   | `ReturnLoggerFactory` | test factory  | factory for ReturnLogger           |
|  [11]   | `DropEvent`           | exception     | raised by processor to drop event  |

[PUBLIC_TYPE_SCOPE]: stdlib bridge
- rail: observability

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------ | :------------ | :------------------------------- |
|   [1]   | `stdlib.ProcessorFormatter`           | formatter     | stdlib Handler formatter bridge  |
|   [2]   | `stdlib.LoggerFactory`                | factory       | stdlib-backed logger factory     |
|   [3]   | `stdlib.BoundLogger`                  | stdlib logger | stdlib-compatible bound logger   |
|   [4]   | `stdlib.AsyncBoundLogger`             | async logger  | async-compatible bound logger    |
|   [5]   | `stdlib.ExtraAdder`                   | processor     | add stdlib `extra` dict to event |
|   [6]   | `stdlib.PositionalArgumentsFormatter` | processor     | format positional log args       |

[PUBLIC_TYPE_SCOPE]: processors
- rail: observability

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------ | :------------ | :---------------------------------- |
|   [1]   | `processors.TimeStamper`              | processor     | add timestamp key to event dict     |
|   [2]   | `processors.CallsiteParameterAdder`   | processor     | inject file/line/func into event    |
|   [3]   | `processors.CallsiteParameter`        | enum          | callsite field selector             |
|   [4]   | `processors.EventRenamer`             | processor     | rename event key in event dict      |
|   [5]   | `processors.JSONRenderer`             | renderer      | serialize event dict to JSON string |
|   [6]   | `processors.KeyValueRenderer`         | renderer      | serialize event dict to key=value   |
|   [7]   | `processors.LogfmtRenderer`           | renderer      | serialize event dict to logfmt      |
|   [8]   | `processors.ExceptionRenderer`        | processor     | render exc_info into event          |
|   [9]   | `processors.ExceptionDictTransformer` | processor     | convert exception to dict           |
|  [10]   | `processors.StackInfoRenderer`        | processor     | render stack_info into event        |
|  [11]   | `processors.UnicodeDecoder`           | processor     | decode bytes values to str          |
|  [12]   | `processors.MaybeTimeStamper`         | processor     | add timestamp only when absent      |
|  [13]   | `processors.add_log_level`            | processor fn  | inject log level name into event    |

[PUBLIC_TYPE_SCOPE]: dev renderer
- rail: observability

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------- | :------------ | :------------------------------ |
|   [1]   | `dev.ConsoleRenderer`         | renderer      | rich or plain colored console   |
|   [2]   | `dev.Column`                  | column spec   | ConsoleRenderer column config   |
|   [3]   | `dev.ColumnFormatter`         | protocol      | column value formatter protocol |
|   [4]   | `dev.KeyValueColumnFormatter` | formatter     | key=value column formatter      |
|   [5]   | `dev.LogLevelColumnFormatter` | formatter     | log-level colored formatter     |
|   [6]   | `dev.RichTracebackFormatter`  | formatter     | Rich-based traceback formatter  |

[PUBLIC_TYPE_SCOPE]: contextvars and testing
- rail: observability

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :------------------------ | :------------ | :--------------------------- |
|   [1]   | `testing.LogCapture`      | capture class | captures log events in tests |
|   [2]   | `testing.CapturingLogger` | test logger   | logger that stores events    |
|   [3]   | `testing.CapturedCall`    | event record  | one captured log call record |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration and logger creation
- rail: observability

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `get_logger(*args, **initial_values)`                                                            | logger factory | get or create a bound logger       |
|   [2]   | `configure(processors, wrapper_class, context_class, logger_factory, cache_logger_on_first_use)` | config         | set global pipeline                |
|   [3]   | `make_filtering_bound_logger(min_level)`                                                         | class factory  | create level-filtered logger class |
|   [4]   | `wrap_logger(logger, processors, **initial_values)`                                              | wrapper        | wrap existing logger in structlog  |
|   [5]   | `reset_defaults()`                                                                               | reset          | restore default configuration      |
|   [6]   | `get_config()`                                                                                   | introspection  | current global configuration dict  |
|   [7]   | `is_configured()`                                                                                | introspection  | true if configure() was called     |
|   [8]   | `get_context(bound_logger)`                                                                      | context query  | current bound context dict         |

[ENTRYPOINT_SCOPE]: contextvars ambient context
- rail: observability

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]  | [RAIL]                               |
| :-----: | :----------------------------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `contextvars.bind_contextvars(**kw)`                   | bind            | add keys to async-context-local dict |
|   [2]   | `contextvars.unbind_contextvars(*keys)`                | unbind          | remove keys from context dict        |
|   [3]   | `contextvars.clear_contextvars()`                      | clear           | remove all keys from context dict    |
|   [4]   | `contextvars.get_contextvars()`                        | query           | copy of current context dict         |
|   [5]   | `contextvars.merge_contextvars(logger, method, event)` | processor fn    | merge context into event dict        |
|   [6]   | `contextvars.bound_contextvars(**kw)`                  | context manager | scoped bind/unbind                   |
|   [7]   | `contextvars.reset_contextvars(**tokens)`              | reset           | restore prior context state          |

[ENTRYPOINT_SCOPE]: testing helpers
- rail: observability

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :----------------------- | :-------------- | :------------------------------ |
|   [1]   | `testing.capture_logs()` | context manager | capture all log events in block |

## [4]-[IMPLEMENTATION_LAW]

[STRUCTLOG_TOPOLOGY]:
- pipeline: `get_logger()` returns a `BoundLoggerBase` proxy; on each log call the event dict flows through the configured processor chain left-to-right; the final processor renders the event and passes it to the wrapped logger
- `configure(processors=[...])` sets the global pipeline; call once at application startup
- `contextvars.merge_contextvars` must appear in the processor chain before renderers to inject the ambient context dict
- `make_filtering_bound_logger(min_level)` generates a class with only the enabled log-level methods, avoiding no-op calls below the threshold
- `stdlib.ProcessorFormatter` integrates structlog's processor chain into a stdlib `logging.Handler` so both emit through the same pipeline

[LOCAL_ADMISSION]:
- Call `structlog.configure(...)` once at application entry; keep the processor list in a single owner.
- Use `contextvars.bind_contextvars` for request-scoped ambient fields (request ID, trace ID) rather than repeated `.bind()` calls.
- Use `testing.capture_logs()` / `LogCapture` in tests; avoid configuring a real logger in unit tests.
- Use `make_filtering_bound_logger(logging.DEBUG)` as the `wrapper_class` in production to eliminate no-op method call overhead.

[RAIL_LAW]:
- Package: `structlog`
- Owns: structured log pipeline, processor chain, contextvars ambient context, stdlib bridge, dev/JSON renderers
- Accept: `get_logger`, `configure`, `make_filtering_bound_logger`, `contextvars.bind_contextvars`, `processors.TimeStamper`, `stdlib.ProcessorFormatter`
- Reject: direct use of `logging.getLogger` in structlog-configured codebases without `ProcessorFormatter` bridge
