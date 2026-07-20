# [PY_BRANCH_API_STRUCTLOG]

`structlog` supplies structured logging as a left-to-right processor chain over a mutable event dict: per-logger context binding (`bind`/`unbind`/`try_unbind`/`new`), contextvars-backed ambient context, a `FilteringBoundLogger` that compiles away sub-threshold levels, a stdlib `ProcessorFormatter` bridge routing structlog and foreign `logging` records through one render chain, callsite/timestamp/level injectors, structured exception transformers, a `ConsoleRenderer` for dev and `JSONRenderer`/`KeyValueRenderer`/`LogfmtRenderer` for production, and `testing` capture helpers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `structlog`
- package: `structlog`
- module: `structlog`
- version: `26.1.0`
- license: `MIT OR Apache-2.0` (dual; `LICENSE-MIT` + `LICENSE-APACHE` + `NOTICE`)
- rail: observability
- namespaces: `structlog` (core), `structlog.processors`, `structlog.stdlib`, `structlog.dev`, `structlog.contextvars`, `structlog.testing`, `structlog.tracebacks`, `structlog.typing` (= `structlog.types`), `structlog.threadlocal`, `structlog.twisted`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger and factory types
- rail: observability

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                                          |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `BoundLogger`         | bound logger  | the default generic bound logger proxy                                          |
|  [02]   | `BoundLoggerBase`     | abstract base | minimal bound-logger contract; subclass to add methods                          |
|  [03]   | `PrintLogger`         | sync logger   | print-to-stream wrapped logger                                                  |
|  [04]   | `PrintLoggerFactory`  | factory       | factory for `PrintLogger`                                                       |
|  [05]   | `BytesLogger`         | sync logger   | write-bytes wrapped logger (pairs with `JSONRenderer(serializer=orjson.dumps)`) |
|  [06]   | `BytesLoggerFactory`  | factory       | factory for `BytesLogger`                                                       |
|  [07]   | `WriteLogger`         | sync logger   | write-to-file/stream wrapped logger                                             |
|  [08]   | `WriteLoggerFactory`  | factory       | factory for `WriteLogger`                                                       |
|  [09]   | `ReturnLogger`        | test logger   | returns the rendered event for testing                                          |
|  [10]   | `ReturnLoggerFactory` | test factory  | factory for `ReturnLogger`                                                      |
|  [11]   | `DropEvent`           | exception     | raised by a processor to discard the event silently                             |

[PUBLIC_TYPE_SCOPE]: typing contracts
- rail: observability

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                                                |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `typing.FilteringBoundLogger` | protocol      | level-method protocol returned by `make_filtering_bound_logger`       |
|  [02]   | `typing.BindableLogger`       | protocol      | `wrapper_class` contract (`bind`/`unbind`/`new`)                      |
|  [03]   | `typing.Processor`            | type alias    | `(logger, method_name, event_dict) -> event_dict str \ bytes \ tuple` |
|  [04]   | `typing.WrappedLogger`        | type alias    | the underlying sink logger wrapped by the bound proxy                 |
|  [05]   | `typing.EventDict`            | type alias    | `MutableMapping[str, Any]` flowing through the chain                  |
|  [06]   | `typing.Context`              | type alias    | the per-logger bound context mapping                                  |
|  [07]   | `typing.ExcInfo`              | type alias    | `(type, value, traceback)` triple                                     |
|  [08]   | `typing.ExceptionTransformer` | type alias    | `(ExcInfo) -> object` formatter signature                             |
|  [09]   | `typing.ProcessorReturnValue` | type alias    | union of admissible processor return shapes                           |

[PUBLIC_TYPE_SCOPE]: stdlib bridge
- rail: observability

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `stdlib.ProcessorFormatter`            | formatter     | renders structlog and foreign records through one chain |
|  [02]   | `stdlib.LoggerFactory`                 | factory       | `logging.getLogger`-backed logger factory               |
|  [03]   | `stdlib.BoundLogger`                   | stdlib logger | bound logger with stdlib level methods + `bind`         |
|  [04]   | `stdlib.AsyncBoundLogger`              | async logger  | `await log.ainfo(...)` async-mirror bound logger        |
|  [05]   | `stdlib.ExtraAdder`                    | processor     | merge a stdlib record's `extra` dict into the event     |
|  [06]   | `stdlib.PositionalArgumentsFormatter`  | processor     | apply `%`-style positional args to the event message    |
|  [07]   | `stdlib.add_log_level`                 | processor fn  | inject the level name into the event                    |
|  [08]   | `stdlib.add_log_level_number`          | processor fn  | inject the numeric level for ordering/filtering         |
|  [09]   | `stdlib.add_logger_name`               | processor fn  | inject the wrapped logger's name as `logger`            |
|  [10]   | `stdlib.filter_by_level`               | processor fn  | drop the event (`DropEvent`) below the stdlib level     |
|  [11]   | `stdlib.render_to_log_kwargs`          | processor fn  | hand the event to stdlib as `msg` + `extra` kwargs      |
|  [12]   | `stdlib.render_to_log_args_and_kwargs` | processor fn  | hand the event to stdlib as positional args + kwargs    |
|  [13]   | `stdlib.recreate_defaults`             | config fn     | rebuild stdlib-integrated default config (`log_level=`) |

[PUBLIC_TYPE_SCOPE]: processors and renderers
- rail: observability

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                                                                           |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `processors.TimeStamper`              | processor     | add timestamp (`fmt=`, `utc=`, `key=`) to the event                              |
|  [02]   | `processors.MaybeTimeStamper`         | processor     | add timestamp only when the key is absent                                        |
|  [03]   | `processors.add_log_level`            | processor fn  | inject the level name into the event                                             |
|  [04]   | `processors.CallsiteParameterAdder`   | processor     | inject selected callsite fields (`parameters=`, `additional_ignores=`)           |
|  [05]   | `processors.CallsiteParameter`        | enum          | field selector: `FILENAME`/`LINENO`/`FUNC_NAME`/`MODULE`/`PROCESS`/`THREAD`/…    |
|  [06]   | `processors.EventRenamer`             | processor     | rename the `event` key (`to=`, `replace_by=`)                                    |
|  [07]   | `processors.UnicodeDecoder`           | processor     | decode `bytes` values to `str`                                                   |
|  [08]   | `processors.UnicodeEncoder`           | processor     | encode `str` values to `bytes`                                                   |
|  [09]   | `processors.StackInfoRenderer`        | processor     | render `stack_info=True` call sites into the event                               |
|  [10]   | `processors.ExceptionRenderer`        | processor     | render `exc_info` via an `ExceptionTransformer`                                  |
|  [11]   | `processors.ExceptionDictTransformer` | processor     | convert an exception into a JSON-safe nested dict                                |
|  [12]   | `processors.dict_tracebacks`          | processor fn  | preconfigured `ExceptionRenderer(ExceptionDictTransformer())` for JSON pipelines |
|  [13]   | `processors.format_exc_info`          | processor fn  | render `exc_info` to a `exception` traceback string                              |
|  [14]   | `processors.ExceptionPrettyPrinter`   | processor     | pretty-print exceptions to a stream (dev only)                                   |
|  [15]   | `processors.get_processname`          | processor fn  | inject the OS process name                                                       |
|  [16]   | `processors.JSONRenderer`             | renderer      | serialize the event dict to JSON (`serializer=`, `**dumps_kw`)                   |
|  [17]   | `processors.KeyValueRenderer`         | renderer      | serialize to `key=value` (`sort_keys=`, `key_order=`, `drop_missing=`)           |
|  [18]   | `processors.LogfmtRenderer`           | renderer      | serialize to logfmt                                                              |

[PUBLIC_TYPE_SCOPE]: dev renderer and traceback formatters
- rail: observability

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                                                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `dev.ConsoleRenderer`         | renderer      | colored column console output (`columns=`, `colors=`, `exception_formatter=`) |
|  [02]   | `dev.Column`                  | column spec   | one `ConsoleRenderer` column (key + `ColumnFormatter`)                        |
|  [03]   | `dev.ColumnFormatter`         | protocol      | column value-formatter protocol                                               |
|  [04]   | `dev.KeyValueColumnFormatter` | formatter     | `key=value` column formatter                                                  |
|  [05]   | `dev.LogLevelColumnFormatter` | formatter     | level-colored column formatter                                                |
|  [06]   | `dev.ColumnStyles`            | dataclass     | per-column color/style configuration                                          |
|  [07]   | `dev.RichTracebackFormatter`  | formatter     | Rich-based traceback formatter (default exc formatter)                        |
|  [08]   | `dev.plain_traceback`         | formatter fn  | dependency-free plain traceback                                               |
|  [09]   | `dev.rich_traceback`          | formatter fn  | Rich traceback (requires `rich`)                                              |
|  [10]   | `dev.better_traceback`        | formatter fn  | `better-exceptions` traceback                                                 |
|  [11]   | `dev.set_exc_info`            | processor fn  | set `exc_info=True` when an exception is being logged                         |

[PUBLIC_TYPE_SCOPE]: tracebacks, contextvars, testing
- rail: observability
- `tracebacks.ExceptionDictTransformer` carries `show_locals, max_frames, suppress, use_rich`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `tracebacks.ExceptionDictTransformer` | transformer   | `ExcInfo -> list[dict]` JSON-safe stack                 |
|  [02]   | `tracebacks.{Trace,Stack,Frame}`      | dataclass     | structured traceback node hierarchy                     |
|  [03]   | `tracebacks.extract`                  | extractor     | build a `Trace` from `(exc_type, exc_value, traceback)` |
|  [04]   | `testing.LogCapture`                  | capture class | processor that appends events to `.entries`             |
|  [05]   | `testing.CapturingLogger`             | test logger   | wrapped logger storing `CapturedCall` records           |
|  [06]   | `testing.CapturedCall`                | event record  | one captured `(method_name, args, kwargs)` record       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration and logger creation
- rail: observability
- `configure`/`configure_once` take `processors, wrapper_class, context_class, logger_factory, cache_logger_on_first_use`; `wrap_logger` takes `logger, processors, wrapper_class, context_class, cache_logger_on_first_use, **initial_values`

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :-------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `get_logger(*args, **initial_values)` (alias `getLogger`) | logger factory | get a lazily-configured bound logger                   |
|  [02]   | `configure(...)`                                          | config         | set the global pipeline (call once at startup)         |
|  [03]   | `configure_once(...)`                                     | config         | configure only if unset (library-safe)                 |
|  [04]   | `make_filtering_bound_logger(min_level)`                  | class factory  | `FilteringBoundLogger` with only enabled-level methods |
|  [05]   | `wrap_logger(...)`                                        | wrapper        | wrap an existing logger with a local pipeline          |
|  [06]   | `reset_defaults()`                                        | reset          | restore the default configuration                      |
|  [07]   | `get_config()` / `is_configured()`                        | introspection  | current global config dict / configured flag           |
|  [08]   | `get_context(bound_logger)`                               | context query  | the bound logger's current context dict                |

[ENTRYPOINT_SCOPE]: contextvars ambient context
- rail: observability
- members live under `structlog.contextvars`

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RAIL]                                                           |
| :-----: | :----------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `merge_contextvars(logger, method, event)` | processor fn    | merge the ambient context into the event (first in chain)        |
|  [02]   | `bind_contextvars(**kw)`                   | bind            | add keys to the async/thread-local context; returns reset tokens |
|  [03]   | `unbind_contextvars(*keys)`                | unbind          | remove keys from the context                                     |
|  [04]   | `reset_contextvars(**tokens)`              | reset           | restore prior context state from `bind` tokens                   |
|  [05]   | `clear_contextvars()`                      | clear           | remove all keys from the context                                 |
|  [06]   | `get_contextvars()`                        | query           | copy of the current context dict                                 |
|  [07]   | `bound_contextvars(**kw)`                  | context manager | scoped bind/auto-reset over a `with` block                       |

[ENTRYPOINT_SCOPE]: bound-logger methods and testing helpers
- rail: observability
- async mirrors exist only on `FilteringBoundLogger`

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]  | [RAIL]                                               |
| :-----: | :------------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `log.{bind,new}(**kw)` / `log.{unbind,try_unbind}(*keys)`      | bound op        | derive a child logger with mutated context           |
|  [02]   | `log.debug/info/warning/error/critical/exception(event, **kw)` | log call        | emit through the processor chain                     |
|  [03]   | `await log.adebug/ainfo/awarning/aerror(...)`                  | async log       | async-mirror levels offloading the chain to a thread |
|  [04]   | `testing.capture_logs()`                                       | context manager | capture every event in the block as a list of dicts  |

## [04]-[IMPLEMENTATION_LAW]

[STRUCTLOG_TOPOLOGY]:
- `get_logger()` returns a lazy proxy; configuration is resolved on first log call, so module-level `get_logger(__name__)` is safe before `configure()` runs. Each log call seeds an event dict, runs it through the configured processor list left-to-right, and the LAST processor must be a renderer that returns `str`/`bytes`/a `(args, kwargs)` tuple for the wrapped logger
- a processor is `(logger, method_name, event_dict) -> event_dict | str | bytes | tuple`; raising `DropEvent` silently discards the event. `contextvars.merge_contextvars` must be FIRST so the ambient context is present for every downstream processor; renderers (`JSONRenderer`/`ConsoleRenderer`/`KeyValueRenderer`/`LogfmtRenderer`) must be LAST
- `make_filtering_bound_logger(min_level)` generates a class whose sub-threshold level methods are literal no-ops (and whose async mirrors short-circuit) — this is filtering at method-resolution time, strictly cheaper than a `filter_by_level` processor that still constructs the event dict; use it as `wrapper_class`
- `stdlib.ProcessorFormatter` is the bridge: structlog processors run up to `ProcessorFormatter.wrap_for_formatter` (the last processor), then a stdlib `logging.Handler` runs `ProcessorFormatter(processors=[...], foreign_pre_chain=[...])` so BOTH structlog-origin and foreign stdlib records render through one chain; `ProcessorFormatter.remove_processors_meta` strips the internal `_record`/`_from_structlog` keys before the user chain
- exception handling has three tiers: `format_exc_info` (string traceback for plain text), `dict_tracebacks` / `ExceptionDictTransformer` (JSON-safe nested dict for `JSONRenderer`), and `ConsoleRenderer`'s `RichTracebackFormatter` (colored locals-bearing traceback for dev); `ExceptionDictTransformer(use_rich=False)` drops the `rich` dependency
- `CallsiteParameterAdder` resolves the call frame once per event; it is comparatively expensive, so place it after `filter_by_level`/the filtering wrapper has already admitted the event
- `JSONRenderer(serializer=orjson.dumps)` swaps the JSON backend; pair with `BytesLoggerFactory` so the `bytes` output of `orjson.dumps` flows to the sink without a re-encode

[STACKS_WITH]:
- otel trace correlation: a `merge_contextvars`-first chain reads request-scoped `bind_contextvars(trace_id=, span_id=)` into every event; the structlog processor that injects those ids is fed by `opentelemetry`'s current span (`trace.get_current_span().get_span_context()`), so one ambient context populates both the log line and the active span. Map a logged error to `span.record_exception` / `span.set_status` at the same site, and the JSON log carries the `trace_id` that ties it to the OTLP span.
- pydantic structured errors: catch `pydantic.ValidationError` at intake and bind `exc.errors(include_url=False)` as a structured `errors` key (list of `{type, loc, msg}`) before logging; under `JSONRenderer` each field-failure path stays machine-queryable and correlates with the request's bound context, instead of a flattened `str(exc)`.
- msgspec fast JSON: configure `JSONRenderer(serializer=...)` with a `msgspec.json.Encoder().encode`-backed serializer and a `BytesLoggerFactory` sink so domain `Struct` payloads bound onto the event serialize through the same fast encoder that owns the wire, with no stdlib `json` re-encode on the hot logging path.
- anyio structured concurrency: `bind_contextvars` writes into real `contextvars`, so a value bound before an `anyio` task group spawn is visible inside child tasks without explicit threading; the async-mirror `await log.ainfo(...)` on a `FilteringBoundLogger` offloads the (potentially blocking) render+sink to a worker thread, keeping the event loop unblocked under high log volume.
- beartype boundary diagnostics: a `beartype.roar.BeartypeCallHintViolation` caught at a boundary logs via `processors.format_exc_info` / `dict_tracebacks` exactly like any exception — the violation's culprit/expected-type message becomes a structured traceback record rather than a bare string, so type-contract failures and validation failures share one structured exception shape in the log stream.

[LOCAL_ADMISSION]:
- Call `structlog.configure(...)` exactly once at the application entry point and keep the processor list in a single owner; libraries that may run before the app configures use `configure_once`.
- Order the chain `merge_contextvars` -> context/level/timestamp/callsite injectors -> exception transformer -> renderer-last; never place a renderer before a mutating processor.
- Use `make_filtering_bound_logger(level)` as `wrapper_class` in production to compile away sub-threshold calls; reserve `filter_by_level` for the stdlib-bridge path where the wrapper is `stdlib.BoundLogger`.
- Bind request-scoped ambient fields (request id, trace id, tenant) with `bind_contextvars` / `bound_contextvars`, not repeated per-logger `.bind()`; reset with the tokens from `bind_contextvars` or the `bound_contextvars` context manager.
- Render production output with `JSONRenderer` (swap in a fast serializer + `BytesLoggerFactory`) and dev output with `dev.ConsoleRenderer`; select by environment, not by branching inside a processor.
- Capture logs in tests with `testing.capture_logs()` / `LogCapture`; never stand up a real sink in unit tests.

[RAIL_LAW]:
- Package: `structlog`
- Owns: the structured log pipeline, processor chain ordering, contextvars ambient context, the stdlib `logging` bridge, level filtering, dev/JSON/keyvalue/logfmt renderers, structured exception/traceback transformation
- Accept: `get_logger`, `configure`/`configure_once`, `make_filtering_bound_logger`, `contextvars.merge_contextvars`/`bind_contextvars`, `processors.TimeStamper`/`JSONRenderer`/`dict_tracebacks`, `stdlib.ProcessorFormatter`
- Reject: bare `logging.getLogger` in a structlog-configured codebase without the `ProcessorFormatter` bridge, `str(exc)` where `format_exc_info`/`dict_tracebacks` carries the structured traceback, a renderer placed before mutating processors, per-call `.bind()` where ambient `bind_contextvars` is correct
