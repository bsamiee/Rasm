---
name: python-coding-telemetry
description: "Use when writing or reviewing Python that logs, traces, or measures, covering structlog and OpenTelemetry from setup to correlation"
---

# [PYTHON_CODING_TELEMETRY]

Covers logs, traces, and measurements through structlog and the OpenTelemetry API and SDK.

## [01]-[COMPOSITION_ROOT]

Applications install SDK providers, library instrumentors, and structlog in one root scope that releases each on exit, and libraries depend on `opentelemetry-api` and structlog alone. Roots pass what the SDK reads from nowhere else: exporters resolve endpoint, headers, and timeout from `OTEL_EXPORTER_OTLP_*` variables, `TracerProvider` reads `OTEL_TRACES_SAMPLER` without a `sampler=` argument, and `Resource.create` merges `OTEL_RESOURCE_ATTRIBUTES` under its arguments:

```python
class Instrumentor(Protocol):
    def instrument(self, *, raise_exception_on_conflict: bool) -> object: ...

    def uninstrument(self) -> object: ...


ENCODER = msgspec.json.Encoder(enc_hook=repr)


def correlated(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    span = trace.get_current_span().get_span_context()
    return {**event_dict, "trace_id": trace.format_trace_id(span.trace_id), "span_id": trace.format_span_id(span.span_id)} if span.is_valid else event_dict


def rendered(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> bytes:
    return ENCODER.encode(event_dict)


@contextmanager
def observed(distribution: str, /, *, level: int = logging.INFO, views: Sequence[View] = (), instrumentors: Iterable[Instrumentor] = ()) -> Generator[None]:
    resource = Resource.create({SERVICE_NAME: distribution, SERVICE_VERSION: version(distribution)})
    tracing = TracerProvider(resource=resource, shutdown_on_exit=False)
    tracing.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))
    metering = MeterProvider(metric_readers=[PeriodicExportingMetricReader(OTLPMetricExporter())], resource=resource, views=views, shutdown_on_exit=False)
    trace.set_tracer_provider(tracing)
    metrics.set_meter_provider(metering)
    structlog.configure(
        processors=[merge_contextvars, add_logger_name, add_log_level, correlated, TimeStamper(fmt="iso"), ExceptionRenderer(ExceptionDictTransformer(show_locals=False)), rendered],
        wrapper_class=structlog.make_filtering_bound_logger(level),
        logger_factory=structlog.BytesLoggerFactory(),
        cache_logger_on_first_use=True,
    )
    with ExitStack() as stack:
        stack.callback(metering.shutdown)
        stack.callback(tracing.shutdown)
        for instrumentor in instrumentors:
            instrumentor.instrument(raise_exception_on_conflict=True)
            stack.callback(instrumentor.uninstrument)
        yield
```

Roots run every handler inside `with observed(<distribution>):` with the level, views, and instrumentors they change. Providers, patches, and processor chains hold for the process:
- Second `set_tracer_provider` or `set_meter_provider` calls log a warning and keep the first provider
- Tracers and instruments obtained before `observed` record once `observed` sets providers, and earlier measurements drop
- `shutdown_on_exit=False` leaves release to `observed`, and a default provider registers a second release through `atexit`
- Exporter `timeout=` bounds each export and `shutdown`, and `BatchSpanProcessor` ignores `export_timeout_millis` and a `force_flush` timeout
- `BatchSpanProcessor` queues 2048 spans and exports every 5 s, and a full queue drops its oldest span with a warning per drop
- `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED=true` reports processed spans by `error.type` (`queue_full`, `already_shutdown`) and queue size under meter `opentelemetry-sdk`
- Instrumentors hold one instance per class and patch a library for every caller through global providers, and `uninstrument` restores the library
- `instrument()` over a patched library warns and changes nothing
- `raise_exception_on_conflict=True` raises `DependencyConflictError` for a library release outside an instrumentor's range, where the default logs an error and patches nothing
- `Instrumentor` types members a root calls, and mypy reads every instrumentor as `Any` under a file-level `# type: ignore` in their base module
- Detector packages run by entry-point name from `OTEL_EXPERIMENTAL_RESOURCE_DETECTORS`, `*` runs every installed one, and a forked child draws a new `service.instance.id` over any value a root set
- `rendered` encodes through one held `Encoder` to the bytes `BytesLogger` writes, `enc_hook=repr` covering values msgspec lacks (a `Path`, an exception), where `JSONRenderer` returns `str` and passes `default=` to its serializer
- `add_logger_name` writes the name each `get_logger(<name>)` call hands its `BytesLogger`
- `dict_tracebacks` writes every frame's locals as reprs, a token argument included, and `ExceptionDictTransformer(show_locals=False)` keeps frames alone
- `ExceptionRenderer` turns `exc_info` into an `exception` list with `ExceptionGroup` members under `exceptions`, and a chain without it writes `"exc_info":true` with no traceback
- `cache_logger_on_first_use=True` fixes each logger at its first log call, and a later `configure` reaches only loggers that never logged

## [02]-[INSTRUMENTED_STEPS]

Steps returning an `Outcome` take span, status, duration, and completion entry from one decorator, each name derived from the decorated function: tracer, meter, and logger from its module, span and event from its qualified name, and the `code.function.name` on every `step.duration` reading from both. ty reads no `__qualname__` on a `Callable`, and `Step` declares it. `marked` sets `ERROR` status for an expected failure and returns `error.type` attributes that histogram and entry share, and defects raise through the span:

```python
SECONDS = tuple(mantissa * 10.0**exponent for exponent in range(-3, 2) for mantissa in (1.0, 2.5, 5.0))


class Step[**P, T](Protocol):
    __qualname__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Outcome[T]: ...


def marked[T](span: Span, outcome: Outcome[T], /) -> dict[str, str]:
    match outcome:
        case Result(tag="error", error=(head, *_) as faults):
            span.set_status(Status(StatusCode.ERROR, "; ".join(fault.message for fault in faults)))
            span.set_attributes(attributes := {ERROR_TYPE: head.reason.name})
            return attributes
        case _:
            return {}


def traced[**P, T](step: Step[P, T], /) -> Callable[P, Outcome[T]]:
    tracer, log, function = trace.get_tracer(step.__module__), structlog.get_logger(step.__module__), {CODE_FUNCTION_NAME: f"{step.__module__}.{step.__qualname__}"}
    durations = metrics.get_meter(step.__module__).create_histogram("step.duration", unit="s", explicit_bucket_boundaries_advisory=SECONDS)

    @wraps(step)
    def run(*args: P.args, **kwargs: P.kwargs) -> Outcome[T]:
        with tracer.start_as_current_span(step.__qualname__) as span:
            started = perf_counter()
            outcome = step(*args, **kwargs)
            elapsed, attributes = perf_counter() - started, marked(span, outcome)
            durations.record(elapsed, function | attributes)
            log.info(step.__qualname__, seconds=elapsed, **attributes)
            return outcome

    return run


@traced
def stored(target: Path, content: bytes, /) -> Outcome[Path]:
    return catch(target.write_bytes, exception=OSError)(content).map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))
```

Histogram series count each function's calls, one per `error.type` value and one without it for successes, and no counter restates the count. Spans record what their block returns, raises, or sets:
- Raises inside `start_as_current_span` set `ERROR` as `<Type>: <message>` with an `exception` event and replace a status set before them
- Successes stay `UNSET`, `OK` set before a raise stays final, and cancellation under anyio ends a span `UNSET` with no event
- Samplers read attributes passed at start through `attributes=`, and `span.is_recording()` gates a later attribute that costs work
- `@tracer.start_as_current_span(...)` opens one span per call and holds it across an `async def`'s whole await
- Decorated generators parent every span their caller opens between `next` calls, and consumers open a span around their loop
- Spans around `async with anyio.create_task_group()` record a child's `ExceptionGroup` as `ERROR`, and a span in the host task ends `UNSET`
- Misuse (`end()` twice, `set_attribute` after `end()`, `Counter.add(-1)`) logs a warning and changes nothing

## [03]-[MEASUREMENTS]

Observable instruments read state host objects count themselves (limiter tokens, stream buffers, cache entries) through their statistics at collection, in place of adds and subtracts at each call site. `occupancy` reads an anyio limiter, and `limited` builds one shared resource's limiter with the instrument reading it:

```python
def occupancy(limiter: anyio.CapacityLimiter, _options: CallbackOptions, /) -> tuple[Observation, ...]:
    statistics = limiter.statistics()
    return (Observation(statistics.borrowed_tokens, {"limiter.state": "borrowed"}), Observation(statistics.tasks_waiting, {"limiter.state": "waiting"}))


def limited(resource: str, limit: int, /) -> anyio.CapacityLimiter:
    limiter = anyio.CapacityLimiter(limit)
    metrics.get_meter(__name__).create_observable_up_down_counter(f"{resource}.limiter", callbacks=[partial(occupancy, limiter)], unit="{task}")
    return limiter
```

Roots call `limited` once per resource, and each collection reads borrowed tokens and waiting tasks on asyncio and trio:
- Callbacks run on the collecting thread (a periodic reader's worker), read state safe across threads, and await nothing
- Repeated `create_*` calls of one name on one meter return the first instrument, every `@traced` step in a module feeding one `step.duration`, and an observable repeat drops its callbacks unreported
- Default histogram boundaries run from 0 to 10000 for milliseconds, and `SECONDS` spans 1 ms to 50 s for a histogram in `s`
- `View(instrument_type=Histogram, aggregation=ExponentialBucketHistogramAggregation())` fits buckets to any scale and replaces an advisory
- SDK metrics hold no cardinality limit, metric attributes hold closed sets (an `error.type`, a state), and ids go on spans and log fields
- Views with `attribute_keys` merge series over the keys they keep, and each view matching an instrument adds its own stream
- `View(meter_name="<dependency>", aggregation=DropAggregation())` silences every instrument a dependency's meter creates
- SDK meters raise `Exception` at creation for a name outside `[a-zA-Z][-_./a-zA-Z0-9]{0,254}`, a `__qualname__` holding `<locals>` among them

## [04]-[LOGGERS]

Events are constant names, and each varying value goes in a keyword field. Module loggers declare `log: FilteringBoundLogger = structlog.get_logger(__name__)` over a lazy proxy typed `Any` that assembles at its first log call:
- Components holding fixed context (an extension id, a connection) receive a bound logger as an argument
- Call keywords win over bound keys, and bound keys win over context-local keys
- `bind` at module scope before `configure` builds a logger from builtin defaults that keeps printing console lines to stdout
- Methods below the wrapper's level return `None` without running the chain, `log.is_enabled_for(logging.DEBUG)` gates a field that costs work
- Async code calls sync methods, `ainfo` and its siblings run the chain in an asyncio executor and raise `RuntimeError` under trio

## [05]-[CONTEXT]

Units of work bind log fields through `bound_contextvars` and open their span in one `with`. Remote contexts attach for a unit's scope through one `ExitStack.callback`, and `to_thread.run_sync` workers run `@traced` steps under their caller's span and fields with no threading instrumentation:

```python
TRACER = trace.get_tracer(__name__)


async def handled(headers: Mapping[str, str], writes: Mapping[Path, bytes], /) -> list[Outcome[Path]]:
    with ExitStack() as stack:
        stack.callback(context.detach, context.attach(propagate.extract(headers)))
        with TRACER.start_as_current_span(handled.__qualname__, kind=SpanKind.CONSUMER), bound_contextvars(batch=len(writes)):
            return await anyio.amap(lambda write: to_thread.run_sync(stored, *write), writes.items())
```

Entries from each `stored` call hold `batch` and their own span's ids on asyncio and trio, and each `stored` span nests under `handled`, which nests under the sender's span with sender baggage current. Context reaches tasks, threads, and carriers:
- `bound_contextvars` restores overwritten keys and removes new ones on exit, as a `with` or a decorator on a `def` or an `async def`
- Child tasks under anyio inherit bound fields and the current span on asyncio and trio, and a bind inside a child reaches no parent or sibling
- `propagate.extract(carrier)` starts from an empty `Context`, and `context=context.get_current()` keeps baggage and a span it holds
- `to_process` workers hold no SDK provider, start from an empty context, and point stdout at `os.devnull`, so their spans and entries vanish and a `@traced` step in the caller records the call
- `inject` writes `traceparent` inside a valid span and `baggage` once baggage is set
- Default getters match keys exactly, `Traceparent` extracts nothing, and HTTP header maps pass a case-insensitive `Getter`
- Batches from many senders pass one `Link(trace.get_current_span(propagate.extract(<carrier>)).get_span_context())` per carrier to `links=`
- SDK spans drop a link to a carrier holding no `traceparent`, and samplers read links at start beside `attributes=`
- `baggage.set_baggage(<key>, <value>)` returns a new `Context`, and an attached one reaches every later `inject` and span inside its scope
- `context=` on `start_as_current_span` parents that span alone, and its baggage reaches no child span and no `inject` inside it
- `BaggageSpanProcessor(ALLOW_ALL_BAGGAGE_KEYS)` or a key predicate copies the starting context's baggage onto each span, a remote sender's entries included

## [06]-[CORRELATION]

Ids and exemplars join each signal to its trace:
- `correlated` reads the current span in its calling thread's context, and entries outside any span hold no ids
- Unsampled spans keep a valid span context, and entries inside one hold ids that join a request's entries and match no exported trace
- Processes without an SDK write a remote parent's ids from non-recording spans holding the parent's span context
- Measurements inside a sampled span hold an exemplar with its span id, `record(..., context=<context>)` joins one taken outside it, and each collection resets exemplars
- Log entries reach a backend as structlog output joined to traces by ids, and the OpenTelemetry logs API is private `opentelemetry._logs`
