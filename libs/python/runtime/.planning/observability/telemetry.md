# [PY_RUNTIME_TELEMETRY]

`Telemetry` is the one composition-root install owner for OTLP signal egress — every other observability surface assumes the providers it registers and owns no construction. Install custody is two-tier: the per-composition receipt map keys by the receipts-owned `ScopeKey` — a same-scope re-install returns its cached `InstallReceipt` stamped `REENTRANT`, a second composition arriving after the pipeline exists receives its own receipt stamped `ADOPTED` — while `latched` guards the process pipeline mint itself, because provider install is process-global by SDK law and the first emitting install owns it. The `execution/admission#CONTEXT` `emit_otel` cell gates every mint: an emitting profile installs the batched providers, a silent one keeps the `opentelemetry-api` no-ops and opens no batch thread. Its LOG row rides the `observability/logging#PIPELINE` `LogShip` gate: only the `INPROCESS_OTLP` escape hatch registers the in-process `LoggerProvider` this composition root alone names. Egress rides two wire-law rows fixed here: the `EgressTransport` axis keys the per-transport exporter-factory triple — proto-http the estate default, the gRPC row SIDECAR-eligible alone because a persistent channel never survives `fork()` — and the metric exporter carries the base2-exponential histogram default that keeps Python bucket algebra identical to the C# and TS legs.

One `SIGNAL_SPECS` fold owns the batched span/log pair; the meter stands beside it — `metric_readers` are construction-only — built FIRST, threaded as `meter_provider=` into both exporters. Its composite propagator seeds one cross-language trace for the inbound `observability/receipts#RECEIPT` extract and outbound `transport/serve#CAPABILITY_INVOKE` interceptors; `PARENT_SAMPLER` honors the C#-minted parent's sampled bit, so the Python leg never fractures `ONE_DISTRIBUTED_TRACE`. `MeterProvider` carries `exemplar_filter=TraceBasedExemplarFilter()` — the `observability/metrics#METRIC` exemplar contract's install half.

## [01]-[INDEX]

- [01]-[TELEMETRY]: the scope-keyed install custody over the `latched` pipeline mint, the `SIGNAL_SPECS` fold, the `EGRESS` transport table and `SIGNAL_PROFILE` gate, the detector-merged `RUNTIME_RESOURCE`, and the per-signal railed host drain.

## [02]-[TELEMETRY]

- Owner: a `SignalSpec` row owns the `(provider, attach)` wiring pair and the global-register cell, so the span/log install is one fold and a new batch-processor signal is one row; the exporter lives apart on the `EGRESS` table — one `EgressTransport`-keyed row-map per transport holding the signal-keyed factory triple, so transport is a policy value the profile row carries and never a sibling install path — and the `_batched` kernel lifts the bound `add_*` method and processor class over one shared queue-triple wiring, never two sibling closures. `InstalledProviders` holds the trio as one atomically-assigned `Drainable`-typed carrier, never parallel `| None` provider slots.
- Entry: the silent gate caches a `SILENT` receipt per scope, so a later re-admission under the same composition still no-ops, and the cost of OTel under a silent profile is exactly the API no-op providers. `resource=` and `signal_profile=` are injection seams — the profiles-owned job envelope hands in its hand-built job identity and high-interval geometry, every daemon path resolves `RUNTIME_RESOURCE` and the profile-keyed row — the injected row's `transport` clamps to HTTP outside `GRPC_ELIGIBLE` with the receipt carrying the effective geometry so the fork-hazard fence is visible evidence, never a silent divergence, and `ship=` is the same `LogShip` value the `LogPipeline.configure` call carries, so the provider half and the handler half of the log escape hatch cannot diverge. `shutdown` is scope-keyed custody: only the scope whose receipt reads `INSTALLED` drains the pipeline — `force_flush`-then-`shutdown` per provider through `boundary(signal.value, ...)` under `traversed(ACCUMULATE)`, a wedged exporter never short-circuiting the remaining flushes and classifying with its per-signal identity intact — then clears the carrier, the process latch, and every scope receipt (an `ADOPTED` receipt references a drained pipeline and is stale), re-arming a clean re-install; a `SILENT`/`ADOPTED` scope's shutdown retires only its own receipt.
- Auto: `RUNTIME_RESOURCE` carries the `service.namespace`/`service.name`/`service.instance.id` uniqueness triple and orders the env detector last, so a deployment-time `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge without a code change and the three signals join one identity — a bare `{"service.name": ...}` literal degrades to `unknown_service`. Exporters' own `create_exporter_metrics` counters land on the one threaded meter when `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED` admits them, never a parallel pipeline. Serve-leg `SERVER` span emission stays `transport/serve#SERVE`-owned: its defaulted `tracer_provider` resolves the global this install registers, and a global `GrpcAioInstrumentorServer` double-patches the leg — two overlapping `SERVER` spans per RPC, the serve health-check filter bypassed — so no instrumentor activates here.
- Growth: a new batch-processor signal is one `Signal` member, one `SignalSpec` row, and one factory cell per `EGRESS` transport row, reaching the install and drain folds with no entrypoint edit; a construction-only-reader signal seeds the carrier beside the fold and registers through the body's matching `set_*_provider`, since it owns no `attach` step; a new egress transport is one `EgressTransport` member with one `EGRESS` row-map and its `GRPC_ELIGIBLE`-style eligibility fact; a new per-profile geometry or transport knob is one `SignalProfile` column with its `SIGNAL_PROFILE` values; a new composition is one `ScopeKey` value threaded through `install`/`shutdown`'s `scope` keyword; a new propagator format one `PROPAGATORS` row folded by the one `set_global_textmap`; a new resource detector one entry in the `get_aggregated_resources` list; a new per-span cardinality cap one `SPAN_LIMITS` argument; a new promoted signal dimension one `PROMOTED_BAGGAGE` member reaching span and log promotion through the one predicate.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, no AppHost telemetry envelope, sampler-floor ownership, or product export, and no SDK import outside this owner; the `_logs` tier and the gRPC exporter tier ride module-scope `lazy from`, reified only when an emitting install selects them — the baggage-promotion package's own `__init__` imports `_logs`, so its pair defers with it — never at module import. Histogram wire shape is ruled here: `WIRE_AGGREGATION` sets the base2-exponential default at the metric exporter's `preferred_aggregation`, matching the estate bucket algebra across languages, while `views=` stays the deployment override valve — where a deployment rules an instrument's explicit shape, one `View` re-selects `ExplicitBucketHistogramAggregation` and the instrument's API-level bucket advisory supplies the boundaries, the metrics owner's advisory rows being exactly that fallback contract. `SPAN_LIMITS` rides the trace row's provider construction, capping per-span attribute, event, and link cardinality so a hostile caller-shaped payload never balloons the batch queue's memory envelope. Span- and log-level `rasm.tenant` promotion is this install's seam alone — the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction — so no producer page folds tenant onto spans and the metric-side fold stays the metrics owner's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from importlib.metadata import version
from typing import ClassVar, Final, Protocol
from uuid import uuid4

from expression import Ok, Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.metrics import Counter, Histogram, MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.propagators.textmap import TextMapPropagator
from opentelemetry.resource.detector.containerid import ContainerResourceDetector
from opentelemetry.sdk.metrics import MeterProvider, TraceBasedExemplarFilter
from opentelemetry.sdk.metrics.export import AggregationTemporality, MetricExporter, PeriodicExportingMetricReader
from opentelemetry.sdk.metrics.view import Aggregation, ExponentialBucketHistogramAggregation
from opentelemetry.sdk.resources import (
    SERVICE_INSTANCE_ID,
    SERVICE_NAME,
    SERVICE_NAMESPACE,
    SERVICE_VERSION,
    OsResourceDetector,
    OTELResourceDetector,
    ProcessResourceDetector,
    Resource,
    get_aggregated_resources,
)
from opentelemetry.sdk.trace import SpanLimits, TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, SpanExporter
from opentelemetry.sdk.trace.sampling import ALWAYS_ON, ParentBased, Sampler
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator

from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile
from rasm.runtime.faults import SCOPES, Disposition, RuntimeRail, Scope, boundary, latched, traversed
from rasm.runtime.logging import LogShip
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey

lazy import grpc  # channel substrate + grpc.Compression; only a selected GRPC row reifies it
lazy from opentelemetry import _logs  # the logs tier stays cold: only a selected INPROCESS_OTLP install reifies it
lazy from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter as GrpcLogExporter  # gRPC tier defers with grpc
lazy from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter as GrpcMetricExporter
lazy from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter as GrpcSpanExporter
lazy from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
lazy from opentelemetry.processor.baggage import BaggageLogProcessor, BaggageSpanProcessor  # package __init__ imports the _logs tier, so the pair defers to install time
lazy from opentelemetry.sdk._logs import LoggerProvider
lazy from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogExporter

# --- [TYPES] ----------------------------------------------------------------------------


class Signal(StrEnum):
    TRACE = "traces"
    METRIC = "metrics"
    LOG = "logs"


class EgressTransport(StrEnum):
    HTTP = "http"
    GRPC = "grpc"


# INSTALLED minted the process pipeline; SILENT kept the no-ops; REENTRANT is a same-scope re-install returning its cached
# receipt; ADOPTED is a second composition arriving after the pipeline exists — its receipt records the standing custody.
class InstallOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


class Drainable(Protocol):
    def force_flush(self, timeout_millis: int = ...) -> bool: ...
    def shutdown(self) -> None: ...


type SignalExporter = SpanExporter | LogExporter
type WireExporter = SignalExporter | MetricExporter
type ExporterFactory = Callable[[str, Mapping[str, str] | None, "SignalProfile", ApiMeterProvider | None], WireExporter]
type ProviderFactory = Callable[[Resource, Sampler], Drainable]
type ProcessorAttach = Callable[[Drainable, SignalExporter, "SignalProfile"], None]

# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORT_TIMEOUT_S: Final[float] = 10.0

# semconv schema pin every runtime Resource carries identically; detector merges keep the pinned url because detectors carry none.
SCHEMA_URL: Final[str] = "https://opentelemetry.io/schemas/1.43.0"

# resource uniqueness triple: namespace + name + instance id, the service name off the faults-owned SCOPES row — never a second literal.
NAMESPACE: Final[str] = "rasm"

RUNTIME_RESOURCE: Final[Resource] = get_aggregated_resources(
    [ProcessResourceDetector(), OsResourceDetector(), ContainerResourceDetector(), OTELResourceDetector()],
    initial_resource=Resource.create(
        {
            SERVICE_NAMESPACE: NAMESPACE,
            SERVICE_NAME: SCOPES[Scope.SERVICE],
            SERVICE_VERSION: version("rasm-runtime"),
            SERVICE_INSTANCE_ID: uuid4().hex,
        },
        schema_url=SCHEMA_URL,
    ),
)

PARENT_SAMPLER: Final[Sampler] = ParentBased(root=ALWAYS_ON)

# fork-hazard fence as data: a persistent gRPC channel never survives fork(), so only the non-forking sidecar daemon may
# carry the GRPC transport value; install clamps every other profile's injected row to HTTP with the receipt as witness.
GRPC_ELIGIBLE: Final[frozenset[RuntimeProfile]] = frozenset({RuntimeProfile.SIDECAR})

# wire law pair every metric exporter row carries: DELTA counters, and the base2-exponential histogram default that
# matches the C# and TS legs so cross-runtime latency panels merge one bucket algebra with no translation re-bucketing.
WIRE_TEMPORALITY: Final[Mapping[type, AggregationTemporality]] = {Counter: AggregationTemporality.DELTA}
WIRE_AGGREGATION: Final[Mapping[type, Aggregation]] = {Histogram: ExponentialBucketHistogramAggregation()}

# per-span cardinality caps: the runtime ingests caller-shaped payloads, so a hostile emitter's attribute, event, or
# link fan is bounded at provider construction and the value-length cap bounds the string blowup a batch queue buffers.
SPAN_LIMITS: Final[SpanLimits] = SpanLimits(max_attributes=64, max_events=128, max_links=32, max_attribute_length=4096)

# W3C composite the install folds into one `set_global_textmap`; a new wire format is one
# row here, never a second propagator install — the C# CORRELATION_SPINE composite at the wire.
PROPAGATORS: Final[Sequence[TextMapPropagator]] = (TraceContextTextMapPropagator(), W3CBaggagePropagator())

# signal-side dimension promotion: the metrics-owned rasm.tenant baggage entry lands as a span attribute at
# start and a log attribute at emit, so tenant rides every signal exactly as it rides the metric dimension;
# membership stays closed — an ALLOW_ALL predicate would stamp arbitrary caller baggage past SPAN_LIMITS intent.
PROMOTED_BAGGAGE: Final[frozenset[str]] = frozenset({TENANT_BAGGAGE})

# --- [MODELS] ---------------------------------------------------------------------------


# Map key IS the profile: no `profile` field rides the row (the admission `ProfilePolicy`
# no-drift law), `InstallReceipt.profile` carrying the resolved key separately. `transport`
# defaults HTTP on every standing row; the GRPC value enters only via the injected seam.
class SignalProfile(Struct, frozen=True):
    export_interval_ms: int
    schedule_delay_ms: int
    max_queue_size: int
    max_export_batch_size: int
    compression: Compression
    transport: EgressTransport = EgressTransport.HTTP


# exporter construction lives on the EGRESS table, keyed by the profile row's transport — a spec row wires and registers.
class SignalSpec(Struct, frozen=True):
    signal: Signal
    provider: ProviderFactory
    attach: ProcessorAttach
    register: Callable[[Drainable], None]


class InstallReceipt(Struct, frozen=True):
    profile: RuntimeProfile
    outcome: InstallOutcome
    endpoint: str
    signal_profile: SignalProfile


class InstalledProviders(Struct, frozen=True):
    by_signal: Map[Signal, Drainable]

    def flush(self) -> RuntimeRail[Block[Signal]]:
        # drain the meter LAST — the span/log force_flush drives their exporters' `create_exporter_metrics` counters onto the meter's
        # reader, so the meter must still be live to export that final self-observability batch before its own shutdown.
        ordered = sorted(self.by_signal.items(), key=lambda kv: kv[0] == Signal.METRIC)
        rails = Block.of_seq(ordered).map(lambda kv: boundary(kv[0].value, lambda kv=kv: _drained(kv[1], kv[0])))
        return traversed(rails, by=Disposition.ACCUMULATE)


# --- [OPERATIONS] -----------------------------------------------------------------------


# an explicit exporter `endpoint=` is used VERBATIM — the SDK appends `/v1/<signal>` only when resolving the env base URL — so the
# per-signal path derives here and one base HTTP endpoint fans to three non-colliding sinks; a gRPC target is a bare
# `host:port` netloc the channel multiplexes, so the GRPC arm returns the base untouched.
def _signal_endpoint(base: str, signal: Signal, transport: EgressTransport) -> str:
    return base if transport is EgressTransport.GRPC else f"{base.rstrip('/')}/v1/{signal.value}"


# one processor-attach kernel both batched rows parameterize: the bound `add_*` method and the processor class are the only per-signal
# variation, so the identical queue-triple wiring lives once.
def _batched(add: Callable[[Drainable, object], None], processor: Callable[..., object]) -> ProcessorAttach:
    return lambda prov, exp, prof: add(
        prov,
        processor(
            exp, max_queue_size=prof.max_queue_size, schedule_delay_millis=prof.schedule_delay_ms, max_export_batch_size=prof.max_export_batch_size
        ),
    )


def _tracer_provider(resource: Resource, sampler: Sampler) -> TracerProvider:
    # TracerProvider carries no span_processors= constructor slot, so the promotion registers through
    # add_span_processor BEFORE the batch attach and the global set: every span starts already stamped.
    provider = TracerProvider(resource=resource, sampler=sampler, span_limits=SPAN_LIMITS)
    provider.add_span_processor(BaggageSpanProcessor(PROMOTED_BAGGAGE.__contains__))
    return provider


def _log_attach(provider: Drainable, exporter: SignalExporter, profile: SignalProfile) -> None:
    # log-side promotion registers first so the promoted attributes ride the record before the batch processor enqueues;
    # on_emit never overwrites an attribute a stdlib or structlog field already set.
    LoggerProvider.add_log_record_processor(provider, BaggageLogProcessor(PROMOTED_BAGGAGE.__contains__))
    _batched(LoggerProvider.add_log_record_processor, BatchLogRecordProcessor)(provider, exporter, profile)


def _meter_provider(endpoint: str, headers: Mapping[str, str] | None, profile: SignalProfile, resource: Resource) -> MeterProvider:
    exporter = EGRESS[profile.transport][Signal.METRIC](_signal_endpoint(endpoint, Signal.METRIC, profile.transport), headers, profile, None)
    reader = PeriodicExportingMetricReader(exporter, export_interval_millis=profile.export_interval_ms)
    # `exemplar_filter` arms the metrics `context=` hand-off: a sampled active span admits the
    # measurement's exemplar; the SDK default filter would drop every one.
    return MeterProvider(metric_readers=[reader], resource=resource, exemplar_filter=TraceBasedExemplarFilter())


def _drained(provider: Drainable, signal: Signal) -> Signal:
    # shutdown runs on EVERY exit — a raising or false flush included — so a wedged exporter still releases its
    # provider before the carrier clears; BOTH legs' failures survive together: a shutdown raise joins the held
    # flush failure as one ExceptionGroup instead of masking it, so the per-signal deadline classification the
    # false-flush TimeoutError carries reaches the caller even when the shutdown leg also raises.
    faults: list[BaseException] = []
    try:  # Exemption: the flush leg's raise is held, never propagated mid-drain, so the shutdown leg always runs
        if not provider.force_flush(timeout_millis=int(EXPORT_TIMEOUT_S * 1000)):
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
        case [lone]:
            raise lone
        case both:
            raise ExceptionGroup(f"telemetry.drain.{signal.value}", both)


# --- [TABLES] ---------------------------------------------------------------------------

# every standing row keeps the HTTP transport default; the SIDECAR daemon alone may inject a GRPC-valued row through
# the `signal_profile=` seam, and GRPC_ELIGIBLE clamps every other profile's injection back to HTTP at install.
SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq(
    (p, SignalProfile(export_interval_ms=i, schedule_delay_ms=d, max_queue_size=q, max_export_batch_size=b, compression=c))
    for p, i, d, q, b, c in (
        (RuntimeProfile.SIDECAR, 2000, 1000, 2048, 512, Compression.Gzip),
        (RuntimeProfile.TOOL, 5000, 5000, 512, 128, Compression.Gzip),
        (RuntimeProfile.PACKAGE, 5000, 5000, 512, 128, Compression.NoCompression),
        (RuntimeProfile.TEST, 5000, 5000, 512, 128, Compression.NoCompression),
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
        # every LOG cell defers its lazy dereference to call time — a module-scope bound-method or module-attribute read
        # reifies the cold _logs tier at import — so the row's install cost lands only when the INPROCESS_OTLP hatch selects it.
        lambda resource, _: LoggerProvider(resource=resource),
        _log_attach,
        lambda provider: _logs.set_logger_provider(provider),
    ),
])

# per-transport exporter-factory triples: the profile row's transport selects the row-map, the signal the cell. Every cell
# defers its lazy dereference to call time, so the gRPC tier and the _logs tier reify only when a selected install fires.
# Both enums spell NoCompression/Deflate/Gzip, so the gRPC cell derives grpc.Compression by member name — one compression
# column on the profile row, projected per transport at the seam, never a second knob. The METRIC cell carries the wire
# law pair and reads no meter (the metric exporter has no self-observability meter_provider slot).
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
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name], meter_provider=mp
                ),
            ),
            (
                Signal.METRIC,
                lambda ep, hd, prof, _mp: GrpcMetricExporter(
                    endpoint=ep,
                    headers=hd,
                    timeout=EXPORT_TIMEOUT_S,
                    compression=grpc.Compression[prof.compression.name],
                    preferred_temporality=dict(WIRE_TEMPORALITY),
                    preferred_aggregation=dict(WIRE_AGGREGATION),
                ),
            ),
            (
                Signal.LOG,
                lambda ep, hd, prof, mp: GrpcLogExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name], meter_provider=mp
                ),
            ),
        ]),
    ),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    # two-tier custody: `_receipts` keys per-composition install evidence by ScopeKey, while `_process` is the one
    # `latched` pipeline latch — provider install is process-global by SDK law, so the first emitting install owns it.
    _receipts: ClassVar[Map[ScopeKey, InstallReceipt]] = Map.empty()
    _process: ClassVar[InstallReceipt | None] = None
    _installed: ClassVar[InstalledProviders | None] = None

    @classmethod
    @latched(lambda: Telemetry._process, lambda r: setattr(Telemetry, "_process", r), lambda prior: replace(prior, outcome=InstallOutcome.ADOPTED))
    def _pipeline(
        cls, profile: RuntimeProfile, endpoint: str, headers: Mapping[str, str] | None, identity: Resource, geometry: SignalProfile, ship: LogShip
    ) -> InstallReceipt:
        meter = _meter_provider(endpoint, headers, geometry, identity)
        metrics.set_meter_provider(meter)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = EGRESS[geometry.transport][spec.signal](_signal_endpoint(endpoint, spec.signal, geometry.transport), headers, geometry, meter)
            provider = spec.provider(identity, PARENT_SAMPLER)
            spec.attach(provider, exporter, geometry)
            spec.register(provider)
            return installed.add(spec.signal, provider)

        # LOG row = the in-process escape hatch: it installs only under INPROCESS_OTLP, and the same ship value
        # drives the LogPipeline handler attach — the default ship leaves log promotion to the collector tail.
        selected = SIGNAL_SPECS.filter(lambda spec: spec.signal is not Signal.LOG or ship is LogShip.INPROCESS_OTLP)
        seed: Map[Signal, Drainable] = Map.empty().add(Signal.METRIC, meter)
        by_signal = selected.fold(emit, seed)
        propagate.set_global_textmap(CompositePropagator(list(PROPAGATORS)))
        cls._installed = InstalledProviders(by_signal=by_signal)
        return InstallReceipt(profile, InstallOutcome.INSTALLED, endpoint, geometry)

    @classmethod
    def install(
        cls,
        profile: RuntimeProfile,
        endpoint: str,
        headers: Mapping[str, str] | None = None,
        *,
        resource: Resource | None = None,
        signal_profile: SignalProfile | None = None,
        ship: LogShip = LogShip.STDOUT_COLLECTOR,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> InstallReceipt:
        # resource/signal_profile are injection seams: the profiles-owned job envelope hands in its hand-built identity
        # and high-interval geometry; every daemon path resolves the standing rows. The transport clamp is the
        # fork-hazard fence made visible: the receipt carries the effective geometry, never a silently divergent one.
        match cls._receipts.try_find(scope):
            case Option(tag="some", some=prior):
                return replace(prior, outcome=InstallOutcome.REENTRANT)
            case _:
                identity = resource if resource is not None else RUNTIME_RESOURCE
                requested = signal_profile if signal_profile is not None else SIGNAL_PROFILE[profile]
                geometry = (
                    replace(requested, transport=EgressTransport.HTTP)
                    if requested.transport is EgressTransport.GRPC and profile not in GRPC_ELIGIBLE
                    else requested
                )
                receipt = (
                    InstallReceipt(profile, InstallOutcome.SILENT, endpoint, geometry)
                    if not PROFILE_POLICY[profile].emit_otel
                    else cls._pipeline(profile, endpoint, headers, identity, geometry, ship)
                )
                cls._receipts = cls._receipts.add(scope, receipt)
                return receipt

    @classmethod
    def shutdown(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Block[Signal]]:
        # custody law: only the scope whose receipt reads INSTALLED drains the process pipeline; a SILENT/ADOPTED scope
        # retires its own receipt alone. A pipeline drain clears every scope receipt — an ADOPTED receipt over a drained
        # pipeline is stale evidence — re-arming a clean re-install.
        owner = cls._receipts.try_find(scope).map(lambda r: r.outcome is InstallOutcome.INSTALLED).default_value(False)
        match (owner, cls._installed):
            case (True, InstalledProviders() as installed):
                drained = installed.flush()
                cls._installed = cls._process = None
                cls._receipts = Map.empty()
                return drained
            case _:
                cls._receipts = cls._receipts.remove(scope) if cls._receipts.contains_key(scope) else cls._receipts
                return Ok(Block.empty())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
