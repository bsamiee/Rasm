# [PY_BRANCH_API_STRUCTLOG]

`structlog` owns the structured-logging pipeline: a left-to-right processor chain folds a mutable event dict from bind-time context through injectors and an exception transformer to a terminal renderer. `FilteringBoundLogger` compiles sub-threshold levels to no-ops at method resolution, `contextvars` carry ambient context across async boundaries, and `stdlib.ProcessorFormatter` routes structlog and foreign `logging` records through one chain. It owns the observability rail's log output — `ConsoleRenderer` for dev, `JSONRenderer`/`KeyValueRenderer`/`LogfmtRenderer` for production.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger and factory types

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                          |
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

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                            |
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

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :------------------------------------ | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `processors.TimeStamper`              | processor     | add timestamp (`fmt=`, `utc=`, `key=`) to the event                           |
|  [02]   | `processors.MaybeTimeStamper`         | processor     | add timestamp only when the key is absent                                     |
|  [03]   | `processors.add_log_level`            | processor fn  | inject the level name into the event                                          |
|  [04]   | `processors.CallsiteParameterAdder`   | processor     | inject selected callsite fields (`parameters=`, `additional_ignores=`)        |
|  [05]   | `processors.CallsiteParameter`        | enum          | field selector: `FILENAME`/`LINENO`/`FUNC_NAME`/`MODULE`/`PROCESS`/`THREAD`/… |
|  [06]   | `processors.EventRenamer`             | processor     | rename the `event` key (`to=`, `replace_by=`)                                 |
|  [07]   | `processors.UnicodeDecoder`           | processor     | decode `bytes` values to `str`                                                |
|  [08]   | `processors.UnicodeEncoder`           | processor     | encode `str` values to `bytes`                                                |
|  [09]   | `processors.StackInfoRenderer`        | processor     | render `stack_info=True` call sites into the event                            |
|  [10]   | `processors.ExceptionRenderer`        | processor     | render `exc_info` via an `ExceptionTransformer`                               |
|  [11]   | `processors.ExceptionDictTransformer` | processor     | exception -> JSON-safe nested dict; `show_locals`/`use_rich` default ON       |
|  [12]   | `processors.dict_tracebacks`          | processor fn  | `ExceptionRenderer(ExceptionDictTransformer())`, no knobs                     |
|  [13]   | `processors.format_exc_info`          | processor fn  | render `exc_info` to a `exception` traceback string                           |
|  [14]   | `processors.ExceptionPrettyPrinter`   | processor     | pretty-print exceptions to a stream (dev only)                                |
|  [15]   | `processors.get_processname`          | processor fn  | inject the OS process name                                                    |
|  [16]   | `processors.JSONRenderer`             | renderer      | serialize the event dict to JSON (`serializer=`, `**dumps_kw`)                |
|  [17]   | `processors.KeyValueRenderer`         | renderer      | serialize to `key=value` (`sort_keys=`, `key_order=`, `drop_missing=`)        |
|  [18]   | `processors.LogfmtRenderer`           | renderer      | serialize to logfmt                                                           |

[PUBLIC_TYPE_SCOPE]: dev renderer and traceback formatters

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
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
- `tracebacks.ExceptionDictTransformer(*, show_locals=True, locals_max_length=10, locals_max_string=80, locals_hide_dunder=True, locals_hide_sunder=False, suppress=(), max_frames=50, use_rich=True)` — `show_locals` emits every frame local into the rendered stack, past any field policy keyed on top-level names, and `use_rich` reaches for `rich`, so a pipeline carrying untrusted or classified state spells the transformer explicitly rather than taking `dict_tracebacks`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `tracebacks.ExceptionDictTransformer` | transformer   | `ExcInfo -> list[dict]` JSON-safe stack                 |
|  [02]   | `tracebacks.{Trace,Stack,Frame}`      | dataclass     | structured traceback node hierarchy                     |
|  [03]   | `tracebacks.extract`                  | extractor     | build a `Trace` from `(exc_type, exc_value, traceback)` |
|  [04]   | `testing.LogCapture`                  | capture class | processor that appends events to `.entries`             |
|  [05]   | `testing.CapturingLogger`             | test logger   | wrapped logger storing `CapturedCall` records           |
|  [06]   | `testing.CapturedCall`                | event record  | one captured `(method_name, args, kwargs)` record       |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration and logger creation
- `configure`/`configure_once` carry: `processors, wrapper_class, context_class, logger_factory, cache_logger_on_first_use`; `wrap_logger` carries `logger, processors, wrapper_class, context_class, cache_logger_on_first_use, **initial_values`

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `get_logger(*args, **initial_values)` (alias `getLogger`) | factory | get a lazily-configured bound logger                   |
|  [02]   | `configure(...)`                                          | static  | set the global pipeline (call once at startup)         |
|  [03]   | `configure_once(...)`                                     | static  | configure only if unset (library-safe)                 |
|  [04]   | `make_filtering_bound_logger(min_level)`                  | factory | `FilteringBoundLogger` with only enabled-level methods |
|  [05]   | `wrap_logger(...)`                                        | factory | wrap an existing logger with a local pipeline          |
|  [06]   | `reset_defaults()`                                        | static  | restore the default configuration                      |
|  [07]   | `get_config()` / `is_configured()`                        | static  | current global config dict / configured flag           |
|  [08]   | `get_context(bound_logger)`                               | static  | the bound logger's current context dict                |

[ENTRYPOINT_SCOPE]: contextvars ambient context
- members live under `structlog.contextvars`

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------- | :------ | :--------------------------------------------------------------- |
|  [01]   | `merge_contextvars(logger, method, event)` | static  | merge the ambient context into the event (first in chain)        |
|  [02]   | `bind_contextvars(**kw)`                   | static  | add keys to the async/thread-local context; returns reset tokens |
|  [03]   | `unbind_contextvars(*keys)`                | static  | remove keys from the context                                     |
|  [04]   | `reset_contextvars(**tokens)`              | static  | restore prior context state from `bind` tokens                   |
|  [05]   | `clear_contextvars()`                      | static  | remove all keys from the context                                 |
|  [06]   | `get_contextvars()`                        | static  | copy of the current context dict                                 |
|  [07]   | `bound_contextvars(**kw)`                  | static  | scoped bind/auto-reset context manager over a `with` block       |

[ENTRYPOINT_SCOPE]: bound-logger methods and testing helpers
- every emit name carries an `a`-prefixed async mirror, and the mirrors exist only on `FilteringBoundLogger`

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `log.{bind,new}(**kw)` / `log.{unbind,try_unbind}(*keys)`                         | instance | derive a child logger with new context |
|  [02]   | `log.debug/info/warn/warning/error/critical/fatal/exception/msg/log(event, **kw)` | instance | emit through the processor chain       |
|  [03]   | `await log.a{name}(...)`                                                          | instance | async mirror, chained on a thread      |
|  [04]   | `testing.capture_logs()`                                                          | static   | capture block events as dicts          |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `get_logger()` returns a lazy proxy resolving configuration on the first log call, so module-level `get_logger(__name__)` is safe before `configure()` runs. Each call seeds an event dict, folds it left-to-right through the processor list, and the last processor must be a renderer returning `str`/`bytes`/a `(args, kwargs)` tuple.
- a processor is `(logger, method_name, event_dict) -> event_dict | str | bytes | tuple`; `DropEvent` silently discards the event. `contextvars.merge_contextvars` runs FIRST so the ambient context reaches every downstream processor; the renderer runs LAST.
- `make_filtering_bound_logger(min_level)` generates a class whose sub-threshold level methods and async mirrors are literal no-ops — filtering at method-resolution time, cheaper than a `filter_by_level` processor that still constructs the event dict.
- `stdlib.ProcessorFormatter` bridges the two systems: structlog processors run up to `wrap_for_formatter` (the last processor), then a stdlib `Handler` runs `ProcessorFormatter(processors=, foreign_pre_chain=)` so structlog-origin and foreign records share one render chain; `remove_processors_meta` strips the internal `_record`/`_from_structlog` keys. `format` seeds BOTH arms with `_record` and `_from_structlog` before any processor runs and reaches `foreign_pre_chain` on the foreign arm alone, so a chain row discriminates origin on `_record` presence while `wrap_for_formatter` itself carries only `_logger`/`_name`, and those two ride the stdlib record rather than the event dict.
- `stdlib.ExtraAdder()` with no `allow` collection copies EVERY attribute a foreign record carries beyond the standard `logging.LogRecord` set, so a library's own `extra` reaches the event dict wholesale and overwrites any chain-owned key an injector has not yet written; a chain that means one thing by a key writes it unconditionally downstream of the adder, and `allow=` narrows the merge where the producer set is known.
- exception rendering carries three tiers: `format_exc_info` (string traceback), `dict_tracebacks`/`ExceptionDictTransformer` (JSON-safe nested dict for `JSONRenderer`), and `ConsoleRenderer`'s `RichTracebackFormatter` (colored locals-bearing dev traceback); `ExceptionDictTransformer(use_rich=False)` drops the `rich` dependency.
- `CallsiteParameterAdder` resolves the call frame once per event and is comparatively expensive, so it runs after the filtering wrapper admits the event. Its `parameters` default is the WHOLE eleven-member enum — four keys naming one source location (`pathname`, `filename`, `module`, `qual_module`), two naming the process, two the thread, two the function — so a pipeline taking the default charges every record its full width against whatever attribute cap the sink applies, and a narrowed `parameters=` collection is the shape a wire-bound chain passes.
- `ExceptionRenderer` reads three `exc_info` spellings through `_figure_out_exc_info` — an `(type, value, traceback)` triple, a bare `BaseException`, and any other truthy value falling back to `sys.exc_info()` — and POPS the slot before writing `exception`, so a row needing the live exception downstream resolves and re-seats it ahead of the renderer rather than reading the rendered frames back. `ProcessorFormatter.format` seeds a foreign record's `exc_info` and `stack_info` onto the event dict, so both spellings reach one shared chain.
- `JSONRenderer(serializer=orjson.dumps)` swaps the JSON backend; pair it with `BytesLoggerFactory` so the `bytes` output reaches the sink without a re-encode.

[STACKING]:
- `opentelemetry`(`.api/opentelemetry-api.md`): a `merge_contextvars`-first chain reads `bind_contextvars(trace_id=, span_id=)` — sourced from `trace.get_current_span().get_span_context()` — into every event, so one ambient context populates both the log line and the active span; a logged error maps to `span.record_exception`/`span.set_status` at the same site.
- `pydantic`(`.api/pydantic.md`): catch `ValidationError` at intake and bind `exc.errors(include_url=False)` as a structured `errors` key (`{type, loc, msg}` list) before logging; under `JSONRenderer` each field-failure path stays machine-queryable against the bound context.
- `msgspec`(`.api/msgspec.md`): configure `JSONRenderer(serializer=)` with a `msgspec.json.Encoder().encode` backend and a `BytesLoggerFactory` sink so domain `Struct` payloads serialize through the wire encoder with no stdlib `json` re-encode on the hot path.
- `anyio`(`.api/anyio.md`): `bind_contextvars` writes real `contextvars`, so a value bound before a task-group spawn is visible inside child tasks; `await log.ainfo(...)` on a `FilteringBoundLogger` offloads render+sink to a worker thread, keeping the event loop unblocked under high log volume.
- `beartype`(`.api/beartype.md`): a caught `BeartypeCallHintViolation` renders via `format_exc_info`/`dict_tracebacks` like any exception, so type-contract failures and validation failures share one structured exception shape in the stream.
- runtime observability owner: one `configure(...)` owner threads `merge_contextvars` first and an environment-selected renderer last, so every folder logs through the shared chain without standing up a local sink.

[LOCAL_ADMISSION]:
- `structlog.configure(...)` runs once at the application entry point with the processor list in a single owner; a library that may run before the app configures uses `configure_once`.
- order the chain `merge_contextvars` -> context/level/timestamp/callsite injectors -> exception transformer -> renderer-last; a renderer never precedes a mutating processor.
- `make_filtering_bound_logger(level)` is the production `wrapper_class`; `filter_by_level` serves only the stdlib-bridge path whose wrapper is `stdlib.BoundLogger`.
- request-scoped fields (request id, trace id, tenant) bind through `bind_contextvars`/`bound_contextvars` and reset via the returned tokens, never repeated per-logger `.bind()`.
- production renders through `JSONRenderer` (fast serializer + `BytesLoggerFactory`), dev through `dev.ConsoleRenderer`, selected by environment rather than a branch inside a processor.
- tests capture through `testing.capture_logs()`/`LogCapture`, never a real sink.
