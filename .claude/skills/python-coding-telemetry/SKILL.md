---
name: python-coding-telemetry
description: "Use when writing or reviewing Python that logs, traces, or measures, covering structlog and OpenTelemetry from setup to correlation"
---

# [PYTHON_CODING_TELEMETRY]

Covers logs, traces, and measurements through structlog and the OpenTelemetry API and SDK. `Fault`, `Outcome`, and `Reason` are python-coding's package faults, and `Reason.UNWRITABLE` joins its enum.

## [01]-[COMPOSITION_ROOT]

Applications install SDK providers, library instrumentors, and structlog in one entry scope that releases each on exit. Libraries depend on `opentelemetry-api` and structlog alone. Roots pass what the SDK reads from nowhere else: exporters resolve endpoint, headers, and timeout from `OTEL_EXPORTER_OTLP_*` variables, `TracerProvider` reads `OTEL_TRACES_SAMPLER` without a `sampler=` argument, and `Resource.create` merges `OTEL_RESOURCE_ATTRIBUTES` under its arguments:

```python
class Instrumentor(Protocol):
    def instrument(self) -> object: ...

    def uninstrument(self) -> object: ...


ENCODER = msgspec.json.Encoder(enc_hook=repr)


def correlated(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    span = trace.get_current_span().get_span_context()
    return {**event_dict, "trace_id": trace.format_trace_id(span.trace_id), "span_id": trace.format_span_id(span.span_id)} if span.is_valid else event_dict


def rendered(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> bytes:
    return ENCODER.encode(event_dict)


@contextmanager
def observed(distribution: str, level: str, views: Sequence[View], instrumentors: Iterable[Instrumentor], /) -> Generator[None]:
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
        for instrumentor in instrumentors:
            instrumentor.instrument()
            stack.callback(instrumentor.uninstrument)
        yield
```

Entries run every handler inside `with observed(<distribution>, <level>, <views>, <instrumentors>):`, and providers, patches, and processor chains hold for the process:
- Second `set_tracer_provider` or `set_meter_provider` calls log a warning and keep the first provider
- Tracers and instruments obtained before `observed` record once it sets providers, and measurements before it drop
- `shutdown_on_exit=False` leaves release to the entry scope, and a default provider registers a second release through `atexit`
- Exporter `timeout=` bounds each export and `shutdown`, and `BatchSpanProcessor` ignores `export_timeout_millis` and a `force_flush` timeout
- Instrumentors hold one instance per class and patch a library for every caller through global providers, and `uninstrument` restores it
- `instrument()` over a patched library warns and changes nothing, and a library release outside an instrumentor's range logs an error and patches nothing
- `Instrumentor` types members a root calls, and mypy reads every instrumentor as `Any` under a file-level `# type: ignore` in their base module
- Detector packages run by entry-point name from `OTEL_EXPERIMENTAL_RESOURCE_DETECTORS`, and each process gets a `service.instance.id`
- `BytesLoggerFactory` writes the bytes `rendered` returns, and a `str` renderer before it raises `TypeError` at write
- `JSONRenderer(serializer=msgspec.json.encode)` raises `TypeError` from the `default=` keyword it passes, an `Encoder.encode` step replaces it
- `enc_hook=repr` encodes values msgspec lacks (a `Path`, an exception)
- `dict_tracebacks` before the renderer turns `exc_info` into an `exception` list, and `ExceptionGroup` members nest under `exceptions`
- Chains without an exception processor write `"exc_info":true` and lose the traceback
- `cache_logger_on_first_use=True` fixes each logger at its first log call, and a later `configure` reaches only loggers that never logged

Stdlib records (an SDK export failure, an instrumentor conflict, a dependency's warning) reach stderr as plain text through `logging.lastResort`. A root `logging.StreamHandler` with `ProcessorFormatter(foreign_pre_chain=[*<chain before rendered>, add_logger_name], processors=[ProcessorFormatter.remove_processors_meta, <str renderer>])` writes them as entries holding bound fields and trace ids.

## [02]-[INSTRUMENTED_STEPS]

Steps returning an `Outcome` take their span, status, duration histogram, and completion entry from one decorator. Each name derives from the decorated function: tracer and meter from its module, span and event from its qualified name, and histogram from both. `Step` declares the `__qualname__` ty reads on no `Callable`. `marked` turns an expected failure into `ERROR` status and returns the `error.type` attributes histogram and entry share. Defects raise through the span:

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
    tracer = trace.get_tracer(step.__module__)
    durations = metrics.get_meter(step.__module__).create_histogram(f"{step.__module__}.{step.__qualname__}", unit="s", explicit_bucket_boundaries_advisory=SECONDS)

    @wraps(step)
    def run(*args: P.args, **kwargs: P.kwargs) -> Outcome[T]:
        with tracer.start_as_current_span(step.__qualname__) as span:
            started = perf_counter()
            outcome = step(*args, **kwargs)
            elapsed, attributes = perf_counter() - started, marked(span, outcome)
            durations.record(elapsed, attributes)
            log.info(step.__qualname__, seconds=elapsed, **attributes)
            return outcome

    return run


@traced
def stored(target: Path, content: bytes, /) -> Outcome[Path]:
    return catch(target.write_bytes, exception=OSError)(content).map(lambda _: target).map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))
```

`log` is the module logger. Histogram series count their calls, one per `error.type` value and one without it for successes, and no counter restates the count. Spans record what their block returns, raises, or sets:
- Raises inside `start_as_current_span` set `ERROR` as `<Type>: <message>` with an `exception` event and replace a status set before them
- Successes stay `UNSET`, `OK` set before a raise stays final, and cancellation under anyio ends a span `UNSET` with no event
- Samplers read attributes passed at start through `attributes=`, and `span.is_recording()` gates a later attribute that costs work
- `@tracer.start_as_current_span(...)` opens one span per call and holds it across an `async def`'s whole await
- Decorated generators stay current in their caller between `next` calls and parent spans opened there, and their consumer opens the span around its loop
- Spans around `async with anyio.create_task_group()` record a child's `ExceptionGroup` as `ERROR`, and a span in the host task ends `UNSET`
- Misuse (`end()` twice, `set_attribute` after `end()`, `Counter.add(-1)`) logs a warning and changes nothing
- Attribute keys come from `opentelemetry.semconv.attributes.<area>_attributes` constants, and custom keys take a dotted namespace

## [03]-[MEASUREMENTS]

Instruments take their kind from what a reading means, and observable forms read a value at collection in place of a call at each change:

| [INDEX] | [READING]           | [CASES]                   | [INSTRUMENT]                                                  |
| :-----: | :------------------ | :------------------------ | :------------------------------------------------------------ |
|  [01]   | Monotonic total     | Calls, bytes sent         | `create_counter`, `create_observable_counter`                 |
|  [02]   | Additive level      | Tokens held, queued items | `create_up_down_counter`, `create_observable_up_down_counter` |
|  [03]   | Distribution        | Durations, request sizes  | `create_histogram`                                            |
|  [04]   | Non-additive sample | Temperature, hit ratio    | `create_gauge`, `create_observable_gauge`                     |

Observable instruments read host objects that count their own state (limiter tokens, stream buffers, cache entries) through their statistics, and no call site restates that state with adds and subtracts. `occupancy` reads an anyio limiter, and `limited` builds the limiter for one shared resource with the instrument that reads it:

```python
def occupancy(limiter: anyio.CapacityLimiter, _options: CallbackOptions, /) -> tuple[Observation, ...]:
    statistics = limiter.statistics()
    return (Observation(statistics.borrowed_tokens, {"state": "borrowed"}), Observation(statistics.tasks_waiting, {"state": "waiting"}))


def limited(resource: str, limit: int, /) -> anyio.CapacityLimiter:
    limiter = anyio.CapacityLimiter(limit)
    metrics.get_meter(__name__).create_observable_up_down_counter(f"{resource}.limiter", callbacks=[partial(occupancy, limiter)], unit="{task}")
    return limiter
```

Entries call `limited` once per resource, and each collection reads borrowed tokens and waiting tasks on asyncio and trio:
- Callbacks run on the collecting thread (a periodic reader's worker), read state safe across threads, and await nothing
- Second `create_*` calls of one observable name drop their callbacks unreported and return the first instrument
- Default histogram boundaries run from 0 to 10000 for milliseconds, and `SECONDS` spans 1 ms to 50 s for a histogram in `s`
- `View(instrument_type=Histogram, aggregation=ExponentialBucketHistogramAggregation())` fits buckets to any scale and replaces an advisory
- SDK metrics hold no cardinality limit, metric attributes hold closed sets (an `error.type`, a state), and ids go on spans and log fields
- Views with `attribute_keys` merge series over the keys they keep, and each view matching an instrument adds its own stream
- `View(meter_name="<dependency>", aggregation=DropAggregation())` silences every instrument a dependency's meter creates
- Units follow UCUM (`s`, `By`), and braces annotate a count (`{task}`, `{call}`)

## [04]-[LOGGERS]

Events are constant names, and each varying value goes in a keyword field. Module loggers declare `log: FilteringBoundLogger = structlog.get_logger(__name__)` over a lazy proxy typed `Any` that assembles at its first log call:
- `bind` returns a new logger with added keys, and `get_logger(__name__, <key>=<value>)` binds initial keys
- Components holding fixed context (an extension id, a connection) receive a bound logger as an argument
- Call keywords win over bound keys, and bound keys win over context-local keys
- `bind` at module scope before `configure` builds a logger from builtin defaults that keeps printing console lines to stdout
- Methods below the wrapper's level return `None` without running the chain, `log.is_enabled_for(logging.DEBUG)` gates a field that costs work
- Async code calls sync methods, `ainfo` and its siblings run the chain in an asyncio executor and raise `RuntimeError` under trio

## [05]-[CONTEXT]

Units of work bind log fields through `bound_contextvars` and open their span in one `with`. Remote contexts attach for a unit's scope through one `ExitStack.callback`:

```python
TRACER = trace.get_tracer(__name__)


async def read(path: anyio.Path, /) -> Outcome[bytes]:
    with TRACER.start_as_current_span("item.read") as span, bound_contextvars(path=str(path)):
        try:
            content = await path.read_bytes()
        except OSError as error:
            failed: Outcome[bytes] = Error(Fault.of(Reason.UNREADABLE, str(path), error))
            log.info("item.read", **marked(span, failed))
            return failed
        log.info("item.read", size=len(content))
        return Ok(content)


async def handled(headers: Mapping[str, str], paths: Sequence[anyio.Path], /) -> list[Outcome[bytes]]:
    with ExitStack() as stack:
        stack.callback(context.detach, context.attach(propagate.extract(headers)))
        with TRACER.start_as_current_span("batch.read", kind=SpanKind.CONSUMER), bound_contextvars(batch=len(paths)):
            return await anyio.amap(read, paths)


def outgoing() -> dict[str, str]:
    carrier: dict[str, str] = {}
    propagate.inject(carrier)
    return carrier
```

Entries from `read` hold `batch`, `path`, and their `item.read` span's ids on asyncio and trio, and `item.read` nests under `batch.read` under the sender's span with the sender's baggage current. Context reaches tasks, threads, and carriers:
- `bound_contextvars` restores overwritten keys and removes new ones on exit, as a `with` or a decorator on a `def` or an `async def`
- Child tasks under anyio inherit bound fields and the current span on asyncio and trio, and a bind inside a child reaches no parent or sibling
- `to_thread.run_sync` workers see their caller's fields and span with no threading instrumentation
- `propagate.extract(carrier)` starts from an empty `Context`, and `context=context.get_current()` keeps baggage and a span it holds
- `extract({})` returns a context with no parent, and `inject` writes `traceparent` inside a valid span and `baggage` once baggage is set
- Default getters match keys exactly, `Traceparent` extracts nothing, and HTTP header maps pass a case-insensitive `Getter`
- Batches from many senders pass one `Link(trace.get_current_span(propagate.extract(<carrier>)).get_span_context())` per carrier to `links=`
- SDK spans drop a link to a carrier holding no `traceparent`, and samplers read links at start beside `attributes=`
- `baggage.set_baggage(<key>, <value>)` returns a new `Context`, and an attached one reaches every later `inject` and span inside its scope
- `context=` on `start_as_current_span` parents that span alone, and its baggage reaches no child span and no `inject` inside it
- `BaggageSpanProcessor(ALLOW_ALL_BAGGAGE_KEYS)` or a key predicate copies the starting context's baggage onto each span, a remote sender's entries included

## [06]-[CORRELATION]

Ids and exemplars join each signal to its trace:
- `correlated` reads the current span in its calling thread's context, and entries outside any span hold no ids
- `format_trace_id` and `format_span_id` render 32 and 16 lowercase hex digits, the form `traceparent` holds
- Unsampled spans keep a valid span context, and entries inside one hold ids that join a request's entries and match no exported trace
- Processes without an SDK write a remote parent's ids, their non-recording spans keep the parent's span context
- Measurements inside a sampled span hold an exemplar with its span id, measurements outside one hold none, and each collection resets exemplars
- Log entries reach a backend as structlog output joined to traces by ids, and the OpenTelemetry logs API is private `opentelemetry._logs`
