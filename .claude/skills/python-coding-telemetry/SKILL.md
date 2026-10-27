---
name: python-coding-telemetry
description: "Use when writing or reviewing Python that logs, traces, or measures, covering structlog and OpenTelemetry from setup to correlation"
---

# [PYTHON_CODING_TELEMETRY]

Covers structlog and the OpenTelemetry API and SDK, from one composition root through instrumented steps and context across tasks to log entries joined to their traces. `Fault`, `Outcome`, and `Reason` are python-coding's package faults with `UNREADABLE` and `UNWRITABLE` reasons.

## [01]-[COMPOSITION_ROOT]

Applications install SDK providers and configure structlog in one entry scope that releases each provider on exit, and libraries depend on `opentelemetry-api` and structlog alone. Roots pass what the SDK reads from nowhere else: exporters resolve endpoint, headers, and timeout from `OTEL_EXPORTER_OTLP_*` variables, and `Resource.create` merges `OTEL_RESOURCE_ATTRIBUTES` under its arguments:

```python
ENCODER = msgspec.json.Encoder(enc_hook=repr)


def correlated(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    span = trace.get_current_span().get_span_context()
    return {**event_dict, "trace_id": trace.format_trace_id(span.trace_id), "span_id": trace.format_span_id(span.span_id)} if span.is_valid else event_dict


def rendered(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> bytes:
    return ENCODER.encode(event_dict)


@contextmanager
def observed(distribution: str, level: str, views: Sequence[View], /) -> Generator[None]:
    resource = Resource.create({SERVICE_NAME: distribution, SERVICE_VERSION: version(distribution)})
    tracing = TracerProvider(resource=resource, shutdown_on_exit=False)
    tracing.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))
    metering = MeterProvider(metric_readers=[PeriodicExportingMetricReader(OTLPMetricExporter())], resource=resource, views=views, shutdown_on_exit=False)
    trace.set_tracer_provider(tracing)
    metrics.set_meter_provider(metering)
    structlog.configure(
        processors=[merge_contextvars, add_log_level, correlated, TimeStamper(fmt="iso", utc=True), dict_tracebacks, rendered],
        wrapper_class=structlog.make_filtering_bound_logger(level),
        logger_factory=structlog.BytesLoggerFactory(),
        cache_logger_on_first_use=True,
    )
    with ExitStack() as stack:
        stack.callback(metering.shutdown)
        stack.callback(tracing.shutdown)
        yield
```

Entries run every handler inside `with observed(<distribution>, settings.level, <views>):`, and providers and processor chains hold for a process:
- Providers take effect once per process, and a second `set_tracer_provider` or `set_meter_provider` logs a warning and keeps the first
- Tracers and instruments taken before a provider is set record once it is set, and measurements made before it drop
- `shutdown_on_exit=False` leaves release to the entry scope, a default provider registers a second release through `atexit`
- Exporter `timeout=` bounds each export and `shutdown`, `BatchSpanProcessor` ignores `export_timeout_millis` and a `force_flush` timeout
- Last processors return what the logger factory writes, `BytesLoggerFactory` takes bytes and a `str` renderer before it raises `TypeError` at write
- `JSONRenderer(serializer=msgspec.json.encode)` raises `TypeError` from the `default=` keyword it passes, an `Encoder.encode` step replaces it
- `enc_hook=repr` encodes values msgspec lacks (a `Path`, an exception), and `Result` and `Option` encode as tagged objects
- `dict_tracebacks` before the renderer turns `exc_info` into an `exception` list, and `ExceptionGroup` members nest under `exceptions`
- Chains without an exception processor write `"exc_info":true` and lose the traceback
- Processors pass as a `list`, `structlog.testing.capture_logs` swaps list contents in place and raises `AttributeError` over a tuple
- `cache_logger_on_first_use=True` fixes each logger at its first log call, a later `configure` reaches only loggers that have not logged

## [02]-[INSTRUMENTED_STEPS]

Steps returning an `Outcome` take their span, status, counter, and log entry from one decorator, and each name derives from the decorated function: tracer and meter from its module, span, instrument, and event from its qualified name. `Step` declares the `__qualname__` ty reads on no `Callable`. Expected failures return an `Error` that `marked` turns into `ERROR` status with `error.type`, and defects raise through the span:

```python
class Step[**P, T](Protocol):
    __qualname__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Outcome[T]: ...


def marked[T](span: Span, outcome: Outcome[T], /) -> Outcome[T]:
    match outcome:
        case Result(tag="error", error=(head, *_) as faults):
            span.set_status(Status(StatusCode.ERROR, "; ".join(fault.message for fault in faults)))
            span.set_attribute(ERROR_TYPE, head.reason.name)
    return outcome


def traced[**P, T](step: Step[P, T], /) -> Callable[P, Outcome[T]]:
    tracer = trace.get_tracer(step.__module__)
    counted = metrics.get_meter(step.__module__).create_counter(f"{step.__module__}.{step.__qualname__}", unit="{call}")

    @wraps(step)
    def run(*args: P.args, **kwargs: P.kwargs) -> Outcome[T]:
        with tracer.start_as_current_span(step.__qualname__) as span:
            outcome = marked(span, step(*args, **kwargs))
            counted.add(1, {"outcome": outcome.tag})
            log.info(step.__qualname__, outcome=outcome.tag)
            return outcome

    return run


@traced
def stored(target: Path, content: bytes, /) -> Outcome[Path]:
    return (
        catch(target.write_bytes, exception=OSError)(content)
        .map(lambda _: target)
        .map_error(lambda error: (Fault(reason=Reason.UNWRITABLE, subject=str(target), cause=error),))
    )
```

`log` is the module logger, and `View(instrument_name="<package>.*", attribute_keys={"outcome"})` keeps one series per tag for every step of a package. Status reads what a block returns or raises:
- Raises inside `start_as_current_span` set `ERROR` as `<Type>: <message>` with an `exception` event and replace a status set before them
- Successes stay `UNSET`, `OK` set before a raise stays final, and cancellation under anyio ends a span `UNSET` with no event
- Spans receive attributes at start through `attributes=`, where samplers read them
- `@tracer.start_as_current_span(...)` over an `async def` holds its span for the whole await, over a `def` it opens one span per call
- Decorated generators stay current in their caller between `next` calls and parent spans opened there, their consumer opens the span around its loop
- Spans around `async with anyio.create_task_group()` record a child's `ExceptionGroup` as `ERROR`, and a span in the host task ends `UNSET`
- Misuse (`end()` twice, `set_attribute` after `end()`, `Counter.add(-1)`) logs a warning and changes nothing
- Attribute keys come from `opentelemetry.semconv.attributes.<area>_attributes` constants, and custom keys take a dotted namespace

SDK metrics hold no cardinality limit, and attribute values decide series count:
- Metric attributes hold closed sets (a result tag, a reason name), and ids (request, job, path) go on spans and log fields
- Views with `attribute_keys` merge series over the keys they keep, and each view matching an instrument adds its own stream
- `explicit_bucket_boundaries_advisory` sets histogram buckets where no view sets an aggregation

## [03]-[LOGGERS]

Events are constant names, and each varying value goes in a keyword field. Module loggers are annotated `log: FilteringBoundLogger = structlog.get_logger(__name__)`, a lazy proxy typed `Any` that assembles at its first log call:
- `bind` returns a new logger with added keys, and `get_logger(__name__, <key>=<value>)` binds initial keys
- Components holding fixed context (an extension id, a connection) receive a bound logger as an argument
- Call keywords win over bound keys, and bound keys win over context-local keys
- `bind` at module scope before `configure` builds a logger from builtin defaults that keeps printing console lines to stdout
- Methods below the wrapper's level return `None` without running the chain, `log.is_enabled_for(logging.DEBUG)` gates a field that costs work
- Async code calls sync methods, `ainfo` and its siblings run the chain in an asyncio executor and raise `RuntimeError` under trio
- `log.err` passes mypy and ty through the `FilteringBoundLogger` protocol and raises `AttributeError` at runtime

## [04]-[CONTEXT]

Units of work bind log fields through `bound_contextvars` and open their span in one `with`, and every task and worker thread they start inherits both. Remote parents enter as a `Context` through `context=`:

```python
TRACER = trace.get_tracer(__name__)


async def read(path: anyio.Path, /) -> Outcome[bytes]:
    with TRACER.start_as_current_span("item.read") as span, bound_contextvars(path=str(path)):
        try:
            content = await path.read_bytes()
        except OSError as error:
            return marked(span, Error((Fault(reason=Reason.UNREADABLE, subject=str(path), cause=error),)))
        log.info("item.read", size=len(content))
        return Ok(content)


async def handled(headers: Mapping[str, str], paths: Sequence[anyio.Path], /) -> list[Outcome[bytes]]:
    with TRACER.start_as_current_span("batch.read", context=propagate.extract(headers), kind=SpanKind.CONSUMER), bound_contextvars(batch=len(paths)):
        return await anyio.amap(read, paths)


def outgoing() -> dict[str, str]:
    carrier: dict[str, str] = {}
    propagate.inject(carrier)
    return carrier
```

Entries from `read` hold `batch`, `path`, and their `item.read` span's ids on asyncio and trio, and `item.read` nests under `batch.read` under the sender's span. Context reaches tasks, threads, and carriers:
- `merge_contextvars` runs first in the chain, and `bound_contextvars` restores overwritten keys and removes new ones on exit
- `bound_contextvars` decorates a `def` or an `async def` with the same restore
- Child tasks under anyio inherit bound fields and the current span on asyncio and trio, and a bind inside a child reaches no parent or sibling
- `to_thread.run_sync` workers see their caller's fields and span with no threading instrumentation
- `propagate.extract(carrier)` starts from an empty `Context`, and `context=context.get_current()` keeps baggage and a span it holds
- `extract({})` returns a context with no parent, and `inject` writes `traceparent` inside a valid span and `baggage` once baggage is set
- Default getters match keys exactly, `Traceparent` extracts nothing, and HTTP header maps pass a case-insensitive `Getter`
- `context.attach` and `context.detach` pair in one `try` with `finally` at a host entry taking no `context=` (a callback API)

## [05]-[CORRELATION]

`correlated` runs after `merge_contextvars` and before the renderer, and each log entry inside a span holds its trace and span ids. Ids and exemplars join each signal to its trace:
- `correlated` reads the current span in its calling thread's context, and entries outside any span hold no ids
- `format_trace_id` and `format_span_id` render 32 and 16 lowercase hex digits, the form `traceparent` holds
- Processes without an SDK write a remote parent's ids, their non-recording spans keep the parent's span context
- Measurements inside a sampled span carry an exemplar with its span id, and measurements outside one carry none
- Log entries reach a backend as structlog output joined to traces by ids, and the OpenTelemetry logs API is private `opentelemetry._logs`
