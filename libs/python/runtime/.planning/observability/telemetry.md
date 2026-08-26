# [PY_RUNTIME_TELEMETRY]

`Telemetry` is the one composition-root install owner for OTLP signal egress — every other observability surface assumes the providers it registers and constructs none. Install custody is two-tier: per-composition `TelemetryInstall`s key by the observe-owned `ScopeKey`, while one generation reservation serializes the process-global provider mint and drain. Admission's `emit_otel` cell gates every mint, so a silent profile keeps the `opentelemetry-api` no-ops and opens no batch thread.

One `SIGNAL_SPECS` fold owns the batched span/log pair; the meter stands beside it — `metric_readers` and `views` are construction-only — built FIRST and threaded as `meter_provider=` into both exporters. `PROPAGATORS` registers one W3C composite as the process global, the sole inbound-context reader the `observability/observe#OBSERVE` bracket, the `transport/serve#CAPABILITY_INVOKE` interceptors, and the `execution/admission#CONTEXT` correlation adoption each resolve. `_sampler` honors the parent it decodes, so the Python leg continues `ONE_DISTRIBUTED_TRACE` rather than rooting a second.

## [01]-[INDEX]

- [02]-[TELEMETRY]: scope-keyed install custody over the generation-reserved pipeline mint, the `diagnostic_read` backend-free read plane, the `SIGNAL_SPECS` fold, the `EGRESS` transport table and `SIGNAL_PROFILE` gate, the `ViewRow` projection and ratio sampler, the detector-merged `RUNTIME_RESOURCE`, and the per-signal result-typed host drain.
- [03]-[CONFORMANCE]: the `CONFORMANCE` cell table proving one row per legislated key at import, the three-disposition `ConformanceRow`, and the local `Conformance` diagnostic projection.

## [02]-[TELEMETRY]

- Owner: a `SignalSpec` row owns the `(provider, attach)` wiring pair and the global-register cell, so the span/log install is one fold and a new batch-processor signal is one row; the exporter lives apart on the `EGRESS` table — one `EgressTransport`-keyed row-map per transport holding the signal-keyed factory triple, so transport is a policy value the profile row carries and never a sibling install path — and the `_batched` kernel lifts the bound `add_*` method and processor class over one shared queue-and-deadline geometry, never two sibling closures. `InstallReservation` carries owner thread, scope, and generation across unlocked construction or drain; `PendingInstall` carries the completed provider graph and the environment pins that install introduced into the guarded commit, so every fault path gives back the same pair; `InstalledProviders` holds the trio as one atomically-assigned `Drainable`-typed carrier, never parallel `| None` provider slots.
- Law: W3C trace-context is the identity wire — a 16-byte trace id crossing every hop of one trace beside the 8-byte span id naming one hop's parent — and `_commit` publishes the `PROPAGATORS` composite as the one global textmap, so every ingress adopts the decoded parent and mints a root only where the carrier holds none. `evidence/clock#CLOCK` stamps its two-half cell on the same carrier as an independent read projecting onto spans as the `rasm.hlc` attribute, so no causal cell occupies a trace or span id slot.
- Entry: `Telemetry.install(ctx, endpoint, ...)` takes the admitted `RuntimeContext`, so the fork-safety gate reads `ctx.shape.topology` and the emission gate reads `ctx.policy.emit_otel` — the preset name rides through as the geometry key and the install column alone. Silent scopes cache a `SILENT` install, so a later re-admission under the same composition still no-ops, and OTel under a silent profile costs exactly the API no-op providers. `resource=` and `signal_profile=` are injection boundaries — the profiles-owned job envelope hands in its hand-built job identity and high-interval geometry, the workers boot hands in its worker identity and small-queue worker geometry, every daemon path resolves `RUNTIME_RESOURCE` and the profile-keyed row — the injected row's `transport` clamps to HTTP outside `GRPC_ELIGIBLE` with the install carrying the effective geometry so the fork-hazard fence is visible evidence, never a silent divergence, and `ship=` is the same `LogShip` value the `LogPipeline.configure` call carries, read through that owner's `SHIP_OTLP` row at both ends so the provider half and the chain half of the log egress cannot diverge. `Telemetry.snapshot()` is the one backend-free measurement read, answering `Nothing` under a profile that declined `diagnostic_read` and the collected tree under one that armed it. `install` reserves custody, builds outside the condition, and commits only the matching owner-generation; competing scopes wait, and same-thread recursion refuses. `shutdown` detaches custody before its unlocked `force_flush`-then-`shutdown` fold — each provider runs through `boundary(TELEMETRY_STOP, ..., catch=_DRAIN_RAISES)` under `traversed(ACCUMULATE)`, so a wedged exporter never short-circuits the remaining flushes — then releases the matching generation; a `SILENT`/`ADOPTED` scope's shutdown retires only its own install. One process mints the pipeline at most once — the OTel `set_*_provider` globals are set-once with no unset — so a post-drain emitting install refuses loudly, commit publishes only after every fallible construction lands, and custody releases on every commit exit so a fault never wedges a waiting install. Construction owns what it allocates: a raise below the meter unwinds that provider's reader and gives back exactly the `SDK_ENV` keys this install introduced, and a raise inside `_commit` unwinds the same pair off the carrier that holds them, so no refused attempt leaves an exporting reader for an unpublished pipeline or a dead profile's geometry standing as the authority the retry reads. `SignalProfile` admits its own sampling ratio at construction, so `_sampler` reads a settled unit fraction rather than collapsing an out-of-range value onto full sampling.
- Auto: `RUNTIME_RESOURCE` carries the `service.namespace`/`service.name`/`service.instance.id` uniqueness triple and orders the env detector last, so a deployment-time `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge without a code change and the three signals join one identity — a bare `{"service.name": ...}` literal degrades to `unknown_service`. Exporters' own `create_exporter_metrics` counters land on the one threaded meter and the batch processors' `create_processor_metrics` counters on the proxy meter that upgrades at commit, never a parallel pipeline — `SDK_ENV` arms both, since an unarmed flag hands every one of them a no-op recorder. Serve-leg `SERVER` span emission stays `transport/serve#SERVE`-owned: its defaulted `tracer_provider` resolves the global this install registers, and a global `GrpcAioInstrumentorServer` double-patches the leg — two overlapping `SERVER` spans per RPC, the serve health-check filter bypassed — so no instrumentor activates here.
- Growth: a new batch-processor signal is one `Signal` member, one `SignalSpec` row, and one factory cell per `EGRESS` transport row, reaching the install and drain folds with no entrypoint edit; a construction-only-reader signal seeds the carrier beside the fold and registers through the body's matching `set_*_provider`, since it owns no `attach` step; a new egress transport is one `EgressTransport` member with one `EGRESS` row-map and its `GRPC_ELIGIBLE`-style eligibility fact; a new per-profile geometry, policy, or transport knob is one `SignalProfile` column with its `SIGNAL_PROFILE` values — the sampling ratio, the cardinality ceiling, the explicit-bucket roster, and the diagnostic-read arm each landed exactly that way; a new metric-stream shaping axis is one `ViewRow` field at the metrics owner and one argument in `_viewed`, with no call site edited; a new composition is one `ScopeKey` value threaded through `install`/`shutdown`'s `scope` keyword; a new propagator format one `PROPAGATORS` row folded by the one `set_global_textmap`; a new resource detector one entry in the `get_aggregated_resources` list; a new per-span cardinality cap one `SPAN_LIMITS` argument; a new log-record cap one `RecordCaps` field beside its `SDK_ENV` row; a new environment-only SDK pin one `SDK_ENV` row; a new re-drive code one `WIRE_RETRYABLE` name reaching all three gRPC exporter rows and the conformance document through the one roster; a new promoted signal dimension one `PROMOTED_BAGGAGE` member reaching span and log promotion through the one predicate.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, no AppHost telemetry envelope, sampler-floor ownership, or product export, and no SDK import outside this owner; every provider disables its private atexit hook because `Telemetry.shutdown` owns the sole flush-and-shutdown path — `_commit` registers the ONE `atexit` final-drain leg instead, because `atexit` runs after `sys.excepthook` and a drain riding only the composition root's unwind would retire the `LoggerProvider` before the logging owner's uncaught-raise door emits its wire line; the diagnostic reader is a READ plane and never an egress — it mounts beside the exporting reader with its own aggregation storage so it drains nothing that reader owes, it stays construction-only like every other reader, and its custody releases with the providers at shutdown so a drained process answers `Nothing` rather than a tree the retired provider no longer feeds. `_logs` and gRPC exporter tiers ride module-scope `lazy from`, reified only when an emitting install selects them — the baggage-promotion package's own `__init__` imports `_logs`, so its pair defers with it — never at module import. Histogram wire shape is ruled here: `WIRE_AGGREGATION` sets the base2-exponential default at the metric exporter's `preferred_aggregation`, matching the repo bucket algebra across languages, and `WIRE_TEMPORALITY` pins every sum and histogram family beside it. Both maps key the SDK instrument families rather than their API base classes — the reader matches a preference key by identity and refuses anything else, so an API-spelled key is an install-killing raise and an omitted family falls to the exporter's environment-seeded table. `views=` is that gate's landed half rather than a promise: `_viewed` projects each metrics-owned `ViewRow` into an SDK `View` at construction — the allow-list bounding the stream's attribute keys, the reservoir selected per row — and a row naming an instrument in the profile's `explicit_buckets` re-selects `ExplicitBucketHistogramAggregation` over that instrument's own API-level advisory. Every other row leaves `aggregation` unset so the view resolves `DefaultAggregation` and defers to the reader's preference, which is what keeps the base2-exponential wire default intact under a per-instrument view set. Views are construction-only, so this is the one place the rows can land; SDK view, aggregation, and reservoir types stay this owner's alone, keeping the metrics tier free of every SDK import. `SPAN_LIMITS` rides the trace row's provider construction, capping span, event, and link attributes and value lengths so a hostile caller-shaped payload never balloons the batch queue's memory envelope; `RecordCaps` is that ceiling's log half, stamped through `SDK_ENV` at the head of the pipeline because the SDK exposes log-record limits on no constructor and a key landing after the first record caps nothing already queued. `SignalProfile.export_timeout_ms` reaches both batch processors and the metric reader, and `max_export_batch_size` reaches BOTH metric exporter rows — the reader hands its whole collection to one export call, so a row leaving that split open ships one unbounded request where its sibling transport ships a bounded stream; exporter transport timeout remains the terminal wire deadline. Export-loss accounting is armed rather than assumed: `SDK_ENV` pins the SDK's internal-metrics flag, without which every processor and exporter builds a no-op recorder and a queue-full drop reaches no series at all. Export RE-DRIVE is pinned on the same principle: the gRPC exporter runs a schedule of its own — a jittered exponential curve inside one `timeout`-derived deadline, a peer's `google.rpc.retryinfo-bin` window overriding each computed wait, and a shutdown event preempting a pending backoff — and its `retryable_error_codes` slot left empty reads a deployment environment variable ahead of the package default, so an operator moves a running pipeline's re-drive policy with nothing recording it; `WIRE_RETRYABLE` fills that slot on all three gRPC rows and rides the `EGRESS_RETRY` conformance seat, holding the OTLP specification set rather than narrowing it, since the pin's job is denying the environment a vote. `_retryable` resolves the roster's names inside those rows alone, so an HTTP install leaves the cold gRPC tier cold and answers that seat `absent` rather than a roster no slot on its transport accepts. Span- and log-level dimension promotion is this install's boundary alone — the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction — so no producer page folds a promoted dimension onto spans and the metric-side fold stays the metrics owner's. Both allow-lists this owner projects roster by KEY rather than by presence — that promotion set and every `ViewRow.keys` set `_viewed` lands as `attribute_keys` — so a sometimes-absent dimension stays rostered whatever a given context carries, an unrostered key strips from exactly the entries that do carry it, and an entry missing a rostered key reads as the untagged whole rather than as a stamped placeholder. That posture governs every sometimes-absent dimension crossing this wire, `rasm.tenant` among them and never the tenant alone.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import atexit
from collections.abc import Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from os import environ
from threading import Condition, RLock, get_ident
from typing import ClassVar, Final, Literal, Protocol
from uuid import uuid4

from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.metrics import MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.propagators.textmap import TextMapPropagator
from opentelemetry.resource.detector.containerid import ContainerResourceDetector
from opentelemetry.sdk.metrics import (
    AlignedHistogramBucketExemplarReservoir,
    Counter,
    ExemplarReservoir,
    Histogram,
    MeterProvider,
    ObservableCounter,
    ObservableUpDownCounter,
    SimpleFixedSizeExemplarReservoir,
    TraceBasedExemplarFilter,
    UpDownCounter,
)
from opentelemetry.sdk.environment_variables import (
    OTEL_ATTRIBUTE_COUNT_LIMIT,
    OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT,
    OTEL_LOGRECORD_ATTRIBUTE_COUNT_LIMIT,
    OTEL_LOGRECORD_ATTRIBUTE_VALUE_LENGTH_LIMIT,
    OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED,
)
from opentelemetry.sdk.metrics.export import AggregationTemporality, InMemoryMetricReader, MetricExporter, MetricsData, PeriodicExportingMetricReader
from opentelemetry.sdk.metrics.view import Aggregation, ExplicitBucketHistogramAggregation, ExponentialBucketHistogramAggregation, View
from opentelemetry.sdk.resources import (
    SERVICE_INSTANCE_ID,
    SERVICE_NAME,
    SERVICE_NAMESPACE,
    SERVICE_VERSION,
    OsResourceDetector,
    OTELResourceDetector,
    ProcessResourceDetector,
    Resource,
    ResourceDetector,
    get_aggregated_resources,
)
from opentelemetry.sdk.trace import SpanLimits, TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, SpanExporter
from opentelemetry.sdk.trace.sampling import ALWAYS_ON, ParentBased, Sampler, TraceIdRatioBased
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator
from rasm.runtime.admission import RuntimeContext, RuntimeProfile, Topology
from rasm.runtime.faults import SCHEMA_URL, SCOPES, SCOPE_VERSION, TELEMETRY_STOP, Catch, Disposition, RuntimeResult, Scope, boundary, traversed
from rasm.runtime.logging import SHIP_OTLP, LogShip
from rasm.runtime.metrics import DOMAINS, INSTRUMENTS, OVERFLOW_KEY, TENANT_BAGGAGE, TENANT_BUDGET, Dimension, ViewRow, views
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

lazy import grpc
lazy from opentelemetry import _logs
lazy from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter as GrpcLogExporter
lazy from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter as GrpcMetricExporter
lazy from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter as GrpcSpanExporter
lazy from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
lazy from opentelemetry.processor.baggage import BaggageLogProcessor, BaggageSpanProcessor
lazy from opentelemetry.sdk._logs import LoggerProvider
lazy from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogRecordExporter

# --- [TYPES] ----------------------------------------------------------------------------


class Signal(StrEnum):
    TRACE = "traces"
    METRIC = "metrics"
    LOG = "logs"


class EgressTransport(StrEnum):
    HTTP = "http"
    GRPC = "grpc"


class InstallOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


class Drainable(Protocol):
    def force_flush(self, timeout_millis: int = ...) -> bool: ...
    def shutdown(self) -> None: ...


type SignalExporter = SpanExporter | LogRecordExporter
type WireExporter = SignalExporter | MetricExporter
type ExporterFactory = Callable[[str, dict[str, str] | None, "SignalProfile", ApiMeterProvider | None], WireExporter]
type ProviderFactory = Callable[[Resource, Sampler], Drainable]
type ProcessorAttach = Callable[[Drainable, SignalExporter, "SignalProfile"], None]
type ReservoirBuilder = Callable[..., ExemplarReservoir]
type ReservoirFactory = Callable[[type], ReservoirBuilder]

# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORT_TIMEOUT_MS: Final[int] = 10_000
EXPORT_TIMEOUT_S: Final[float] = EXPORT_TIMEOUT_MS / 1000.0

_DRAIN_RAISES: Final[Catch] = (TimeoutError, ExceptionGroup)

NAMESPACE: Final[str] = "rasm"

RESOURCE_DETECTORS: Final[tuple[ResourceDetector, ...]] = (
    ProcessResourceDetector(),
    OsResourceDetector(),
    ContainerResourceDetector(),
    OTELResourceDetector(),
)

RUNTIME_IDENTITY: Final[Mapping[str, str]] = {
    SERVICE_NAMESPACE: NAMESPACE,
    SERVICE_NAME: SCOPES[Scope.SERVICE],
    SERVICE_VERSION: SCOPE_VERSION,
    SERVICE_INSTANCE_ID: uuid4().hex,
}

RUNTIME_RESOURCE: Final[Resource] = get_aggregated_resources(
    list(RESOURCE_DETECTORS),
    initial_resource=Resource.create(dict(RUNTIME_IDENTITY), schema_url=SCHEMA_URL),
)

EXEMPLAR_RESERVOIR_SIZE: Final[int] = 4

WIRE_RETRYABLE: Final[tuple[str, ...]] = (
    "CANCELLED", "DEADLINE_EXCEEDED", "RESOURCE_EXHAUSTED", "ABORTED", "OUT_OF_RANGE", "UNAVAILABLE", "DATA_LOSS",
)

GRPC_ELIGIBLE: Final[frozenset[Topology]] = frozenset({Topology.SIDECAR})

WIRE_TEMPORALITY: Final[Mapping[type, AggregationTemporality]] = {
    Counter: AggregationTemporality.DELTA,
    UpDownCounter: AggregationTemporality.CUMULATIVE,
    Histogram: AggregationTemporality.DELTA,
    ObservableCounter: AggregationTemporality.DELTA,
    ObservableUpDownCounter: AggregationTemporality.CUMULATIVE,
}

WIRE_AGGREGATION: Final[Mapping[type, Aggregation]] = {Histogram: ExponentialBucketHistogramAggregation()}

SPAN_LIMITS: Final[SpanLimits] = SpanLimits(
    max_attributes=64,
    max_events=128,
    max_links=32,
    max_event_attributes=64,
    max_link_attributes=64,
    max_attribute_length=4096,
    max_span_attribute_length=4096,
)

SDK_ENV: Final[Map[str, Callable[["SignalProfile"], str]]] = Map.of_seq([
    (OTEL_ATTRIBUTE_COUNT_LIMIT, lambda geometry: str(geometry.record_caps.attribute_count)),
    (OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT, lambda geometry: str(geometry.record_caps.attribute_length)),
    (OTEL_LOGRECORD_ATTRIBUTE_COUNT_LIMIT, lambda geometry: str(geometry.record_caps.record_attribute_count)),
    (OTEL_LOGRECORD_ATTRIBUTE_VALUE_LENGTH_LIMIT, lambda geometry: str(geometry.record_caps.record_attribute_length)),
    (OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED, lambda _geometry: "true"),
])

PROPAGATORS: Final[Sequence[TextMapPropagator]] = (TraceContextTextMapPropagator(), W3CBaggagePropagator())

PROMOTED_BAGGAGE: Final[frozenset[str]] = frozenset({TENANT_BAGGAGE})

# --- [MODELS] ---------------------------------------------------------------------------


class RecordCaps(Struct, frozen=True):
    attribute_count: int = 64
    attribute_length: int = 4096
    record_attribute_count: int = 64
    record_attribute_length: int = 4096


class SignalProfile(Struct, frozen=True):
    export_interval_ms: int
    schedule_delay_ms: int
    max_queue_size: int
    max_export_batch_size: int
    compression: Compression
    export_timeout_ms: int = EXPORT_TIMEOUT_MS
    transport: EgressTransport = EgressTransport.HTTP
    sample_ratio: float = 1.0
    cardinality_budget: int = TENANT_BUDGET
    explicit_buckets: frozenset[str] = frozenset()
    record_caps: RecordCaps = RecordCaps()
    diagnostic_read: bool = False

    def __post_init__(self) -> None:
        if not 0.0 <= self.sample_ratio <= 1.0:
            raise ValueError(f"sample_ratio {self.sample_ratio} outside the unit interval")


class SignalSpec(Struct, frozen=True):
    signal: Signal
    provider: ProviderFactory
    attach: ProcessorAttach
    register: Callable[[Drainable], None]


class TelemetryInstall(Struct, frozen=True):
    profile: RuntimeProfile
    outcome: InstallOutcome
    endpoint: str
    signal_profile: SignalProfile
    ship: LogShip


class InstallReservation(Struct, frozen=True):
    generation: int
    scope: ScopeKey
    thread: int


class InstalledProviders(Struct, frozen=True):
    by_signal: Map[Signal, Drainable]

    def flush(self) -> RuntimeResult[Block[Signal]]:
        ordered = sorted(self.by_signal.items(), key=lambda kv: kv[0] == Signal.METRIC)
        results = Block.of_seq(ordered).map(lambda kv: boundary(TELEMETRY_STOP, lambda kv=kv: _drained(kv[1], kv[0]), catch=_DRAIN_RAISES))
        return traversed(results, by=Disposition.ACCUMULATE)


class PendingInstall(Struct, frozen=True):
    install: TelemetryInstall
    providers: InstalledProviders
    meter: MeterProvider
    diagnostic: InMemoryMetricReader | None
    pinned: Block[str] = Block.empty()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _signal_endpoint(base: str, signal: Signal, transport: EgressTransport) -> str:
    return base if transport is EgressTransport.GRPC else f"{base.rstrip('/')}/v1/{signal.value}"


def _retryable() -> tuple[grpc.StatusCode, ...]:
    return tuple(grpc.StatusCode[name] for name in WIRE_RETRYABLE)


def _selected(ship: LogShip) -> Block[SignalSpec]:
    return SIGNAL_SPECS.filter(lambda spec: spec.signal is not Signal.LOG or SHIP_OTLP[ship])


def _batched(add: Callable[[Drainable, object], None], processor: Callable[..., object]) -> ProcessorAttach:
    return lambda prov, exp, prof: add(
        prov,
        processor(
            exp,
            max_queue_size=prof.max_queue_size,
            schedule_delay_millis=prof.schedule_delay_ms,
            max_export_batch_size=prof.max_export_batch_size,
            export_timeout_millis=prof.export_timeout_ms,
        ),
    )


def _tracer_provider(resource: Resource, sampler: Sampler) -> TracerProvider:
    provider = TracerProvider(resource=resource, sampler=sampler, span_limits=SPAN_LIMITS, shutdown_on_exit=False)
    provider.add_span_processor(BaggageSpanProcessor(PROMOTED_BAGGAGE.__contains__))
    return provider


def _pinned(key: str, value: str) -> str:
    environ[key] = value
    return key


def _unpinned(keys: Block[str]) -> None:
    keys.fold(lambda _held, key: environ.pop(key, ""), None)


def _environed(geometry: SignalProfile) -> Block[str]:
    return Block.of_seq(SDK_ENV.items()).choose(
        lambda row: Nothing if row[0] in environ else Some(_pinned(row[0], row[1](geometry)))
    )


def _log_attach(provider: Drainable, exporter: SignalExporter, profile: SignalProfile) -> None:
    LoggerProvider.add_log_record_processor(provider, BaggageLogProcessor(PROMOTED_BAGGAGE.__contains__))
    _batched(LoggerProvider.add_log_record_processor, BatchLogRecordProcessor)(provider, exporter, profile)


def _sampler(ratio: float) -> Sampler:
    return ParentBased(root=ALWAYS_ON if ratio >= 1.0 else TraceIdRatioBased(ratio))


def _reservoir(row: ViewRow) -> ReservoirFactory:
    builder: ReservoirBuilder = (
        AlignedHistogramBucketExemplarReservoir
        if row.boundaries is not None
        else partial(SimpleFixedSizeExemplarReservoir, size=EXEMPLAR_RESERVOIR_SIZE)
    )
    return lambda _aggregation: builder


def _viewed(row: ViewRow) -> View:
    return View(
        instrument_name=row.instrument,
        attribute_keys=set(row.keys),
        aggregation=ExplicitBucketHistogramAggregation(row.boundaries) if row.boundaries is not None else None,
        exemplar_reservoir_factory=_reservoir(row),
    )


def _meter_provider(
    endpoint: str, headers: dict[str, str] | None, profile: SignalProfile, resource: Resource
) -> tuple[MeterProvider, InMemoryMetricReader | None]:
    exporter = EGRESS[profile.transport][Signal.METRIC](_signal_endpoint(endpoint, Signal.METRIC, profile.transport), headers, profile, None)
    reader = PeriodicExportingMetricReader(
        exporter,
        export_interval_millis=profile.export_interval_ms,
        export_timeout_millis=profile.export_timeout_ms,
    )
    diagnostic = InMemoryMetricReader() if profile.diagnostic_read else None
    return (
        MeterProvider(
            metric_readers=[reader] if diagnostic is None else [reader, diagnostic],
            resource=resource,
            exemplar_filter=TraceBasedExemplarFilter(),
            views=tuple(_viewed(row) for row in views(profile.explicit_buckets)),
            shutdown_on_exit=False,
        ),
        diagnostic,
    )


def _drained(provider: Drainable, signal: Signal) -> Signal:
    faults: list[BaseException] = []
    try:
        if not provider.force_flush(timeout_millis=EXPORT_TIMEOUT_MS):
            faults.append(TimeoutError(signal.value))
    except Exception as flush_fault:
        faults.append(flush_fault)
    try:
        provider.shutdown()
    except Exception as shutdown_fault:
        faults.append(shutdown_fault)
    match faults:
        case []:
            return signal
        case [TimeoutError() as unflushed]:
            raise unflushed
        case held:
            raise ExceptionGroup(f"drain:{signal.value}", held)


# --- [TABLES] ---------------------------------------------------------------------------

SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq(
    (
        p,
        SignalProfile(
            export_interval_ms=i,
            schedule_delay_ms=d,
            max_queue_size=q,
            max_export_batch_size=b,
            compression=Compression.Gzip,
            sample_ratio=r,
            diagnostic_read=x,
        ),
    )
    for p, i, d, q, b, r, x in (
        (RuntimeProfile.SIDECAR, 2000, 1000, 2048, 512, 0.1, True),
        (RuntimeProfile.TOOL, 5000, 5000, 512, 128, 1.0, False),
        (RuntimeProfile.PACKAGE, 5000, 5000, 512, 128, 1.0, False),
        (RuntimeProfile.TEST, 5000, 5000, 512, 128, 1.0, True),
    )
)

SIGNAL_SPECS: Final[Block[SignalSpec]] = Block.of_seq([
    SignalSpec(
        Signal.TRACE,
        _tracer_provider,
        _batched(TracerProvider.add_span_processor, BatchSpanProcessor),
        trace.set_tracer_provider,
    ),
    SignalSpec(
        Signal.LOG,
        lambda resource, _: LoggerProvider(resource=resource, shutdown_on_exit=False),
        _log_attach,
        lambda provider: _logs.set_logger_provider(provider),
    ),
])

EGRESS: Final[Map[EgressTransport, Map[Signal, ExporterFactory]]] = Map.of_seq([
    (
        EgressTransport.HTTP,
        Map.of_seq([
            (
                Signal.TRACE,
                lambda ep, hd, prof, mp: OTLPSpanExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=prof.compression, meter_provider=mp
                ),
            ),
            (
                Signal.METRIC,
                lambda ep, hd, prof, _mp: OTLPMetricExporter(
                    endpoint=ep,
                    headers=hd,
                    timeout=EXPORT_TIMEOUT_S,
                    compression=prof.compression,
                    preferred_temporality=dict(WIRE_TEMPORALITY),
                    preferred_aggregation=dict(WIRE_AGGREGATION),
                    max_export_batch_size=prof.max_export_batch_size,
                ),
            ),
            (
                Signal.LOG,
                lambda ep, hd, prof, mp: OTLPLogExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=prof.compression, meter_provider=mp
                ),
            ),
        ]),
    ),
    (
        EgressTransport.GRPC,
        Map.of_seq([
            (
                Signal.TRACE,
                lambda ep, hd, prof, mp: GrpcSpanExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name],
                    retryable_error_codes=_retryable(), meter_provider=mp,
                ),
            ),
            (
                Signal.METRIC,
                lambda ep, hd, prof, _mp: GrpcMetricExporter(
                    endpoint=ep,
                    headers=hd,
                    timeout=EXPORT_TIMEOUT_S,
                    compression=grpc.Compression[prof.compression.name],
                    retryable_error_codes=_retryable(),
                    preferred_temporality=dict(WIRE_TEMPORALITY),
                    preferred_aggregation=dict(WIRE_AGGREGATION),
                    max_export_batch_size=prof.max_export_batch_size,
                ),
            ),
            (
                Signal.LOG,
                lambda ep, hd, prof, mp: GrpcLogExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name],
                    retryable_error_codes=_retryable(), meter_provider=mp,
                ),
            ),
        ]),
    ),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    _installs: ClassVar[Map[ScopeKey, TelemetryInstall]] = Map.empty()
    _process: ClassVar[TelemetryInstall | None] = None
    _installed: ClassVar[InstalledProviders | None] = None
    _diagnostic: ClassVar[InMemoryMetricReader | None] = None
    _generation: ClassVar[int] = 0
    _minted: ClassVar[bool] = False
    _reservation: ClassVar[InstallReservation | None] = None
    _gate = Condition(RLock())

    @classmethod
    def _reserve(cls, scope: ScopeKey) -> InstallReservation:
        cls._generation += 1
        cls._reservation = InstallReservation(generation=cls._generation, scope=scope, thread=get_ident())
        return cls._reservation

    @classmethod
    def _release(cls, reservation: InstallReservation) -> None:
        if cls._reservation == reservation:
            cls._reservation = None
            cls._gate.notify_all()

    @classmethod
    def _commit(cls, pending: PendingInstall, scope: ScopeKey) -> TelemetryInstall:
        composite = CompositePropagator(list(PROPAGATORS))
        installs = cls._installs.add(scope, pending.install)
        metrics.set_meter_provider(pending.meter)
        for spec in SIGNAL_SPECS:
            pending.providers.by_signal.try_find(spec.signal).map(spec.register)
        propagate.set_global_textmap(composite)
        cls._minted = True
        cls._installed = pending.providers
        cls._diagnostic = pending.diagnostic
        cls._process = pending.install
        cls._installs = installs
        atexit.register(partial(cls.shutdown, scope))
        return pending.install

    @classmethod
    def snapshot(cls) -> Option[MetricsData]:
        match cls._diagnostic:
            case None:
                return Nothing
            case reader:
                return Option.of_optional(reader.get_metrics_data())

    @classmethod
    def _pipeline(
        cls, profile: RuntimeProfile, endpoint: str, headers: dict[str, str] | None, identity: Resource, geometry: SignalProfile, ship: LogShip
    ) -> PendingInstall:
        pinned = _environed(geometry)
        meter, diagnostic = _meter_provider(endpoint, headers, geometry, identity)
        sampler = _sampler(geometry.sample_ratio)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = EGRESS[geometry.transport][spec.signal](_signal_endpoint(endpoint, spec.signal, geometry.transport), headers, geometry, meter)
            provider = spec.provider(identity, sampler)
            spec.attach(provider, exporter, geometry)
            return installed.add(spec.signal, provider)

        seed: Map[Signal, Drainable] = Map.empty().add(Signal.METRIC, meter)
        try:
            by_signal = _selected(ship).fold(emit, seed)
        except BaseException:
            boundary(TELEMETRY_STOP, lambda: _drained(meter, Signal.METRIC), catch=_DRAIN_RAISES)
            _unpinned(pinned)
            raise
        return PendingInstall(
            install=TelemetryInstall(profile, InstallOutcome.INSTALLED, endpoint, geometry, ship),
            providers=InstalledProviders(by_signal=by_signal),
            meter=meter,
            diagnostic=diagnostic,
            pinned=pinned,
        )

    @classmethod
    def install(
        cls,
        ctx: RuntimeContext,
        endpoint: str,
        headers: dict[str, str] | None = None,
        *,
        resource: Resource | None = None,
        signal_profile: SignalProfile | None = None,
        ship: LogShip = LogShip.OTLP_CONSOLE,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> TelemetryInstall:
        profile = ctx.profile
        while True:
            with cls._gate:
                match cls._installs.try_find(scope):
                    case Option(tag="some", some=prior):
                        return replace(prior, outcome=InstallOutcome.REENTRANT)
                    case _:
                        pass
                match cls._reservation:
                    case InstallReservation(thread=owner) if owner == get_ident():
                        raise RuntimeError(f"telemetry install re-entered by reservation owner {scope!r}")
                    case InstallReservation():
                        cls._gate.wait()
                        continue
                    case None:
                        pass

                identity = resource if resource is not None else RUNTIME_RESOURCE
                requested = signal_profile if signal_profile is not None else SIGNAL_PROFILE[profile]
                geometry = (
                    replace(requested, transport=EgressTransport.HTTP)
                    if requested.transport is EgressTransport.GRPC and ctx.shape.topology not in GRPC_ELIGIBLE
                    else requested
                )
                if not ctx.policy.emit_otel:
                    silent = TelemetryInstall(profile, InstallOutcome.SILENT, endpoint, geometry, ship)
                    cls._installs = cls._installs.add(scope, silent)
                    return silent
                if cls._process is not None:
                    adopted = replace(cls._process, outcome=InstallOutcome.ADOPTED)
                    cls._installs = cls._installs.add(scope, adopted)
                    return adopted
                if cls._minted:
                    raise RuntimeError(f"telemetry pipeline already drained; set-once OTel globals cannot re-register for {scope!r}")
                reservation = cls._reserve(scope)
                break

        try:
            pending = cls._pipeline(profile, endpoint, headers, identity, geometry, ship)
        except BaseException:
            with cls._gate:
                cls._release(reservation)
            raise

        commit_fault: BaseException | None = None
        with cls._gate:
            if cls._reservation == reservation:
                try:
                    return cls._commit(pending, scope)
                except BaseException as fault:
                    commit_fault = fault
                finally:
                    cls._release(reservation)
        pending.providers.flush()
        _unpinned(pending.pinned)
        if commit_fault is not None:
            raise commit_fault
        raise RuntimeError(f"telemetry install reservation {reservation.generation} lost custody")

    @classmethod
    def installed(cls) -> Option[TelemetryInstall]:
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def shutdown(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[Block[Signal]]:
        while True:
            with cls._gate:
                match cls._reservation:
                    case InstallReservation(thread=owner) if owner == get_ident():
                        raise RuntimeError(f"telemetry shutdown re-entered by reservation owner {scope!r}")
                    case InstallReservation():
                        cls._gate.wait()
                        continue
                    case None:
                        pass
                owner = cls._installs.try_find(scope).map(lambda r: r.outcome is InstallOutcome.INSTALLED).default_value(False)
                match (owner, cls._installed):
                    case (True, InstalledProviders() as installed):
                        reservation = cls._reserve(scope)
                        cls._installed = cls._process = cls._diagnostic = None
                        cls._installs = Map.empty()
                        break
                    case _:
                        cls._installs = cls._installs.remove(scope) if cls._installs.contains_key(scope) else cls._installs
                        return Ok(Block.empty())

        try:
            return installed.flush()
        finally:
            with cls._gate:
                cls._release(reservation)
```

## [03]-[CONFORMANCE]

- Owner: `ConformanceKey` closes the local diagnostic row space, `ConformanceRow` carries the three projection cases, `Conformance` retains the role, schema coordinate, and ordered rows, and `CONFORMANCE` maps each key to the member deciding its value. `ROSTER` is the import-time totality gate and row spine.
- Cases: three dispositions carry three DIFFERENT facts and nothing collapses them. `carried` is the value the plane holds; `absent` is a row this minter's ROLE reaches no seat for at all; `withheld` is a seat the pinned distribution exposes on no member, so the value the branch honours still projects beside the pin that withholds it. Two rows take `withheld` here — the log-record ceiling, which the SDK seats on no constructor and reaches through the environment alone, and the tenant-value ceiling, which this SDK train exposes as no per-view numeric limit and the attribute fold closes instead. The exemplar filter and sender encoding knob are both CARRIED because this branch holds those values directly.
- Entry: `conformance(install)` projects the effective `TelemetryInstall` into the native immutable diagnostic table.
- Law: this page keeps NO hand-written governance table. Every value PROJECTS off the member that decides it — the detector chain, the minted identity keys, the domain roster beside its subject spellings, the instrument units, the wire temporality and aggregation maps, the propagator composite, the promoted allowlist, the profile's resolved batch square, ratio, cardinality budget, and caps — so a value and the surface it reports move together and a row asserted beside its owner cannot drift from it. The key is not a row field: `CONFORMANCE` holds it, so a row can never spell a key its own cell is not filed under.
- Auto: `ROSTER` supplies row order, the tagged union makes every unselected disposition unrepresentable, and `service.instance.id` reaches the identity row as a key alone because its value is process-specific.
- Growth: a new conformance obligation is one `ConformanceKey` member and one `CONFORMANCE` cell, with the import gate breaking loudly until the cell lands; a plane that opens a seat this branch reads as withheld re-values that same cell rather than adding one; a new provenance column is one owner argument; a role that reaches a row this one cannot flips exactly one cell onto `absent`.
- Boundary: this is a terminal local diagnostic projection, not a peer contract, manifest actor, or generated wire. It constructs no provider, opens no reader, and names no SDK type the install owner does not already hold. Cells name no lazily imported member, so reading the table never reifies exporter tiers a silent process declined.

```python
# --- [TYPES] ----------------------------------------------------------------------------

class ConformanceKey(StrEnum):
    RESOURCE_IDENTITY = "resource.identity"
    RESOURCE_DETECTORS = "resource.detectors"
    RESOURCE_PRECEDENCE = "resource.precedence"
    SCHEMA_COORDINATE = "schema.coordinate"
    SCOPE_COORDINATE = "scope.coordinate"
    SCOPE_ROSTER = "scope.roster"
    METRIC_GRAMMAR = "metric.grammar"
    METRIC_SUBJECTS = "metric.subjects"
    METRIC_UNITS = "metric.units"
    METRIC_TEMPORALITY = "metric.temporality"
    METRIC_AGGREGATION = "metric.aggregation"
    METRIC_EXEMPLAR = "metric.exemplar"
    METRIC_VIEWCAP = "metric.viewcap"
    METRIC_VIEWS = "metric.views"
    METRIC_DIMENSIONS = "metric.dimensions"
    METRIC_ABSENCE = "metric.dimension.absence"
    METRIC_PROCESS = "metric.process.series"
    PROPAGATION_DIALECT = "propagation.dialect"
    PROPAGATION_REGISTRATION = "propagation.registration"
    PROPAGATION_ADOPTION = "propagation.adoption"
    SAMPLE_TRACE_RATIO = "sample.trace.ratio"
    SPAN_LIMIT_CAPS = "span.limits"
    TENANT_KEY = "tenant.key"
    TENANT_ALLOWLIST = "tenant.allowlist"
    TENANT_OVERFLOW = "tenant.overflow"
    SIGNAL_ROSTER = "signal.roster"
    SIGNAL_DIAGNOSTIC = "signal.diagnostic"
    SIGNAL_VITALS = "signal.vitals"
    EGRESS_PROTOCOL = "egress.protocol"
    EGRESS_COMPRESSION = "egress.compression"
    EGRESS_ELIGIBILITY = "egress.eligibility"
    EGRESS_ENDPOINT = "egress.endpoint"
    EGRESS_SIGNALS = "egress.signals"
    EGRESS_BATCH_SPAN = "egress.batch.span"
    EGRESS_BATCH_LOG = "egress.batch.log"
    EGRESS_READER = "egress.reader.cadence"
    EGRESS_PINS = "egress.pins"
    EGRESS_RETRY = "egress.retry"
    EGRESS_LOSS = "egress.loss"
    EGRESS_QUEUE = "egress.queue"
    LOG_IDENTITY = "log.identity"
    LOG_PIPELINE = "log.pipeline"
    LOG_PROMOTION = "log.promotion"
    LOG_RECORD_CAPS = "log.record.caps"
    DRAIN_ORDER = "drain.order"
    DRAIN_BOUND = "drain.bound"
    DRAIN_EXIT = "drain.exit"


type RowCell = Callable[[TelemetryInstall], "ConformanceRow"]

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ConformanceRow:
    tag: Literal["carried", "absent", "withheld"] = tag()
    carried: tuple[str, str] = case()
    absent: str = case()
    withheld: tuple[str, str, str] = case()


class Conformance(Struct, frozen=True, gc=False):
    role: Literal["process"]
    schema_url: str
    rows: tuple[tuple[ConformanceKey, ConformanceRow], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def conformance(install: TelemetryInstall, /) -> Conformance:
    return Conformance(
        role="process",
        schema_url=SCHEMA_URL,
        rows=tuple((key, CONFORMANCE[key](install)) for key in ROSTER),
    )


def _joined(values: Iterable[str]) -> str:
    return ",".join(sorted(set(values)))


def _square(*bounds: int) -> str:
    return "/".join(str(bound) for bound in bounds)


def _batch_square(profile: SignalProfile) -> str:
    return _square(profile.max_queue_size, profile.max_export_batch_size, profile.schedule_delay_ms, profile.export_timeout_ms)


# --- [TABLES] ---------------------------------------------------------------------------

CONFORMANCE: Final[Map[ConformanceKey, RowCell]] = Map.of_seq([
    (ConformanceKey.RESOURCE_IDENTITY, lambda _r: ConformanceRow(carried=(_joined(RUNTIME_IDENTITY.keys()), "RUNTIME_IDENTITY"))),
    (
        ConformanceKey.RESOURCE_DETECTORS,
        lambda _r: ConformanceRow(carried=(_joined(type(detector).__name__ for detector in RESOURCE_DETECTORS), "RESOURCE_DETECTORS")),
    ),
    (
        ConformanceKey.RESOURCE_PRECEDENCE,
        lambda _r: ConformanceRow(
            carried=("detector-chain order, env detector last, deployment override wins the merge", "get_aggregated_resources")
        ),
    ),
    (ConformanceKey.SCHEMA_COORDINATE, lambda _r: ConformanceRow(carried=(SCHEMA_URL, "SCHEMA_URL"))),
    (
        ConformanceKey.SCOPE_COORDINATE,
        lambda _r: ConformanceRow(carried=("scope name, distribution version, semconv schema url — one triple per emitting library", "scoped")),
    ),
    (ConformanceKey.SCOPE_ROSTER, lambda _r: ConformanceRow(carried=(_joined(SCOPES.values()), "SCOPES"))),
    (ConformanceKey.METRIC_GRAMMAR, lambda _r: ConformanceRow(carried=(f"{NAMESPACE}.<domain>.<measure>", "InstrumentSpec.domain"))),
    (
        ConformanceKey.METRIC_SUBJECTS,
        lambda _r: ConformanceRow(carried=(_joined(f"{segment}={subject}" for segment, subject in DOMAINS.items()), "DOMAINS")),
    ),
    (ConformanceKey.METRIC_UNITS, lambda _r: ConformanceRow(carried=(_joined(spec.unit for spec in INSTRUMENTS), "InstrumentSpec.unit"))),
    (
        ConformanceKey.METRIC_TEMPORALITY,
        lambda _r: ConformanceRow(
            carried=(_joined(f"{family.__name__}={temporality.name.lower()}" for family, temporality in WIRE_TEMPORALITY.items()), "WIRE_TEMPORALITY")
        ),
    ),
    (
        ConformanceKey.METRIC_AGGREGATION,
        lambda _r: ConformanceRow(
            carried=(
                _joined(f"{family.__name__}={type(aggregation).__name__}" for family, aggregation in WIRE_AGGREGATION.items()),
                "WIRE_AGGREGATION",
            )
        ),
    ),
    (
        ConformanceKey.METRIC_EXEMPLAR,
        lambda _r: ConformanceRow(
            carried=(
                f"{TraceBasedExemplarFilter.__name__} at the constructor, {EXEMPLAR_RESERVOIR_SIZE}-slot fixed reservoir",
                "_meter_provider",
            )
        ),
    ),
    (
        ConformanceKey.METRIC_VIEWCAP,
        lambda r: ConformanceRow(
            withheld=(str(r.signal_profile.cardinality_budget), "no per-view cardinality seat in this sdk train", "SignalProfile.cardinality_budget")
        ),
    ),
    (
        ConformanceKey.METRIC_VIEWS,
        lambda _r: ConformanceRow(
            carried=("one name-exact view per instrument row, allowlist bounded, aggregation deferred to the reader preference", "views")
        ),
    ),
    (ConformanceKey.METRIC_DIMENSIONS, lambda _r: ConformanceRow(carried=(_joined(Dimension), "Dimension"))),
    (
        ConformanceKey.METRIC_ABSENCE,
        lambda _r: ConformanceRow(
            carried=("per-row key allowlist; a sometimes-absent dimension stays rostered and an absent entry omits its key", "InstrumentSpec.keys")
        ),
    ),
    (
        ConformanceKey.METRIC_PROCESS,
        lambda _r: ConformanceRow(carried=(_joined(spec.name for spec in INSTRUMENTS if spec.domain == "process"), "InstrumentSpec.domain")),
    ),
    (
        ConformanceKey.PROPAGATION_DIALECT,
        lambda _r: ConformanceRow(carried=(_joined(type(propagator).__name__ for propagator in PROPAGATORS), "PROPAGATORS")),
    ),
    (
        ConformanceKey.PROPAGATION_REGISTRATION,
        lambda _r: ConformanceRow(carried=("one composite published as the process global textmap", "Telemetry._commit")),
    ),
    (
        ConformanceKey.PROPAGATION_ADOPTION,
        lambda _r: ConformanceRow(carried=("inbound parent honoured whole; the ratio governs the root arm alone", "_sampler")),
    ),
    (ConformanceKey.SAMPLE_TRACE_RATIO, lambda r: ConformanceRow(carried=(str(r.signal_profile.sample_ratio), "SignalProfile.sample_ratio"))),
    (
        ConformanceKey.SPAN_LIMIT_CAPS,
        lambda _r: ConformanceRow(
            carried=(
                _square(SPAN_LIMITS.max_attributes, SPAN_LIMITS.max_events, SPAN_LIMITS.max_links, SPAN_LIMITS.max_span_attribute_length),
                "SPAN_LIMITS",
            )
        ),
    ),
    (ConformanceKey.TENANT_KEY, lambda _r: ConformanceRow(carried=(TENANT_BAGGAGE, "TENANT_BAGGAGE"))),
    (ConformanceKey.TENANT_ALLOWLIST, lambda _r: ConformanceRow(carried=(_joined(PROMOTED_BAGGAGE), "PROMOTED_BAGGAGE"))),
    (ConformanceKey.TENANT_OVERFLOW, lambda _r: ConformanceRow(carried=(OVERFLOW_KEY, "OVERFLOW_KEY"))),
    (ConformanceKey.SIGNAL_ROSTER, lambda _r: ConformanceRow(carried=(_joined(signal.value for signal in Signal), "Signal"))),
    (
        ConformanceKey.SIGNAL_DIAGNOSTIC,
        lambda r: ConformanceRow(carried=(str(r.signal_profile.diagnostic_read).lower(), "SignalProfile.diagnostic_read")),
    ),
    (ConformanceKey.SIGNAL_VITALS, lambda _r: ConformanceRow(absent="Signal")),
    (ConformanceKey.EGRESS_PROTOCOL, lambda r: ConformanceRow(carried=(f"{r.signal_profile.transport.value}/protobuf", "SignalProfile.transport"))),
    (ConformanceKey.EGRESS_COMPRESSION, lambda r: ConformanceRow(carried=(r.signal_profile.compression.value, "SignalProfile.compression"))),
    (ConformanceKey.EGRESS_ELIGIBILITY, lambda _r: ConformanceRow(carried=(_joined(topology.value for topology in GRPC_ELIGIBLE), "GRPC_ELIGIBLE"))),
    (
        ConformanceKey.EGRESS_ENDPOINT,
        lambda r: ConformanceRow(carried=(_signal_endpoint("<base>", Signal.TRACE, r.signal_profile.transport), "_signal_endpoint")),
    ),
    (
        ConformanceKey.EGRESS_SIGNALS,
        lambda r: ConformanceRow(carried=(_joined([Signal.METRIC.value, *(spec.signal.value for spec in _selected(r.ship))]), "_selected")),
    ),
    (ConformanceKey.EGRESS_BATCH_SPAN, lambda r: ConformanceRow(carried=(_batch_square(r.signal_profile), "_batch_square"))),
    (ConformanceKey.EGRESS_BATCH_LOG, lambda r: ConformanceRow(carried=(_batch_square(r.signal_profile), "_batch_square"))),
    (
        ConformanceKey.EGRESS_READER,
        lambda r: ConformanceRow(carried=(_square(r.signal_profile.export_interval_ms, r.signal_profile.export_timeout_ms), "_meter_provider")),
    ),
    (ConformanceKey.EGRESS_PINS, lambda _r: ConformanceRow(carried=(_joined(key for key, _cell in SDK_ENV.items()), "SDK_ENV"))),
    (
        ConformanceKey.EGRESS_RETRY,
        lambda r: ConformanceRow(carried=(_joined(WIRE_RETRYABLE), "WIRE_RETRYABLE"))
        if r.signal_profile.transport is EgressTransport.GRPC
        else ConformanceRow(absent="WIRE_RETRYABLE"),
    ),
    (
        ConformanceKey.EGRESS_LOSS,
        lambda _r: ConformanceRow(
            carried=(f"{OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED} armed; processor and exporter recorders mount on the install meter", "SDK_ENV")
        ),
    ),
    (ConformanceKey.EGRESS_QUEUE, lambda _r: ConformanceRow(carried=("gateway persistent queue; no branch-side durable otlp queue armed", "EGRESS"))),
    (ConformanceKey.LOG_IDENTITY, lambda _r: ConformanceRow(carried=("the runtime resource, verbatim", "RUNTIME_RESOURCE"))),
    (ConformanceKey.LOG_PIPELINE, lambda r: ConformanceRow(carried=(f"{r.ship.value}/{'wire' if SHIP_OTLP[r.ship] else 'silent'}", "SHIP_OTLP"))),
    (
        ConformanceKey.LOG_PROMOTION,
        lambda _r: ConformanceRow(
            carried=("allowlisted baggage processors seated on the span and log providers at construction", "PROMOTED_BAGGAGE")
        ),
    ),
    (
        ConformanceKey.LOG_RECORD_CAPS,
        lambda r: ConformanceRow(
            withheld=(
                _square(
                    r.signal_profile.record_caps.attribute_count, r.signal_profile.record_caps.attribute_length,
                    r.signal_profile.record_caps.record_attribute_count, r.signal_profile.record_caps.record_attribute_length,
                ),
                "sdk seats log-record limits on no constructor; environment-only and deployment-authoritative",
                "SDK_ENV",
            )
        ),
    ),
    (
        ConformanceKey.DRAIN_ORDER,
        lambda _r: ConformanceRow(
            carried=("span and log providers first, meter last, faults accumulating across signals", "InstalledProviders.flush")
        ),
    ),
    (ConformanceKey.DRAIN_BOUND, lambda _r: ConformanceRow(carried=(f"{EXPORT_TIMEOUT_MS}ms per provider flush", "EXPORT_TIMEOUT_MS"))),
    (
        ConformanceKey.DRAIN_EXIT,
        lambda _r: ConformanceRow(carried=("one atexit final-drain leg; every sdk per-provider exit hook disabled", "Telemetry._commit")),
    ),
])

ROSTER: Final[Block[ConformanceKey]] = Block.of_seq([key for key in sorted(ConformanceKey) if CONFORMANCE[key]])
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
