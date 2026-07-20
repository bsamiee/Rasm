# [PY_RUNTIME_TELEMETRY]

`Telemetry` is the one composition-root install owner for OTLP signal egress — every other observability surface assumes the providers it registers and owns no construction. `latched` guards the whole mint — a re-entrant `install` returns the cached `InstallReceipt` stamped `REENTRANT` — and the `execution/admission#CONTEXT` `emit_otel` cell gates it: an emitting profile installs the batched providers, a silent one keeps the `opentelemetry-api` no-ops and opens no batch thread. Its LOG row rides the `observability/logging#PIPELINE` `LogShip` gate: only the `INPROCESS_OTLP` escape hatch registers the in-process `LoggerProvider` this composition root alone names.

One `SIGNAL_SPECS` fold owns the batched span/log pair; the meter stands beside it — `metric_readers` are construction-only — built FIRST, threaded as `meter_provider=` into both exporters. Its composite propagator seeds one cross-language trace for the inbound `observability/receipts#RECEIPT` extract and outbound `transport/serve#CAPABILITY_INVOKE` interceptors; `PARENT_SAMPLER` honors the C#-minted parent's sampled bit, so the Python leg never fractures `ONE_DISTRIBUTED_TRACE`. `MeterProvider` carries `exemplar_filter=TraceBasedExemplarFilter()` — the `observability/metrics#METRIC` exemplar contract's install half.

## [01]-[INDEX]

- [01]-[TELEMETRY]: the `latched` composition-root install over the `SIGNAL_SPECS` fold and `SIGNAL_PROFILE` gate, the detector-merged `RUNTIME_RESOURCE`, and the per-signal railed host drain.

## [02]-[TELEMETRY]

- Owner: a `SignalSpec` row owns the exporter factory, the `(provider, attach)` wiring pair, and the global-register cell, so the span/log install is one fold and a new batch-processor signal is one row; the `_batched` kernel lifts the bound `add_*` method and processor class over one shared queue-triple wiring, never two sibling closures. `InstalledProviders` holds the trio as one atomically-assigned `Drainable`-typed carrier, never parallel `| None` provider slots.
- Entry: the silent gate caches a `SILENT` receipt, so a later re-admission under the same process still no-ops, and the cost of OTel under a silent profile is exactly the API no-op providers. `resource=` and `signal_profile=` are injection seams — the profiles-owned job envelope hands in its hand-built job identity and high-interval geometry, every daemon path resolves `RUNTIME_RESOURCE` and the profile-keyed row — and `ship=` is the same `LogShip` value the `LogPipeline.configure` call carries, so the provider half and the handler half of the log escape hatch cannot diverge. `shutdown` folds `force_flush`-then-`shutdown` per provider through `boundary(signal.value, ...)` under `traversed(ACCUMULATE)` — a wedged exporter never short-circuits the remaining flushes and classifies with its per-signal identity intact — then clears the carrier and the latch, re-arming a clean re-install.
- Auto: `RUNTIME_RESOURCE` carries the `service.namespace`/`service.name`/`service.instance.id` uniqueness triple and orders the env detector last, so a deployment-time `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge without a code change and the three signals join one identity — a bare `{"service.name": ...}` literal degrades to `unknown_service`. Exporters' own `create_exporter_metrics` counters land on the one threaded meter when `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED` admits them, never a parallel pipeline. Serve-leg `SERVER` span emission stays `transport/serve#SERVE`-owned: its defaulted `tracer_provider` resolves the global this install registers, and a global `GrpcAioInstrumentorServer` double-patches the leg — two overlapping `SERVER` spans per RPC, the serve health-check filter bypassed — so no instrumentor activates here.
- Growth: a new batch-processor signal is one `Signal` member and one `SignalSpec` row reaching the install and drain folds with no entrypoint edit; a construction-only-reader signal seeds the carrier beside the fold and registers through the body's matching `set_*_provider`, since it owns no `attach` step; a new per-profile geometry or transport knob is one `SignalProfile` column with its `SIGNAL_PROFILE` values; a new propagator format one `PROPAGATORS` row folded by the one `set_global_textmap`; a new resource detector one entry in the `get_aggregated_resources` list; a new per-span cardinality cap one `SPAN_LIMITS` argument; a new promoted signal dimension one `PROMOTED_BAGGAGE` member reaching span and log promotion through the one predicate.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, no AppHost telemetry envelope, sampler-floor ownership, or product export, and no SDK import outside this owner; the `_logs` tier rides module-scope `lazy from`, reified only at an emitting install — the baggage-promotion package's own `__init__` imports it, so its pair defers with it — never at module import. `views=` stays a deliberate non-wire constructor slot — the duration histogram ships the API-level bucket advisory the SDK default aggregation honors — until a deployment overrides an instrument's shape against its advisory. `SPAN_LIMITS` rides the trace row's provider construction, capping per-span attribute, event, and link cardinality so a hostile caller-shaped payload never balloons the batch queue's memory envelope. Span- and log-level `rasm.tenant` promotion is this install's seam alone — the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction — so no producer page folds tenant onto spans and the metric-side fold stays the metrics owner's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from importlib.metadata import version
from typing import ClassVar, Final, Protocol
from uuid import uuid4

from expression import Ok
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.metrics import Counter, MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.propagators.textmap import TextMapPropagator
from opentelemetry.resource.detector.containerid import ContainerResourceDetector
from opentelemetry.sdk.metrics import MeterProvider, TraceBasedExemplarFilter
from opentelemetry.sdk.metrics.export import AggregationTemporality, PeriodicExportingMetricReader
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

lazy from opentelemetry import _logs  # the logs tier stays cold: only a selected INPROCESS_OTLP install reifies it
lazy from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
lazy from opentelemetry.processor.baggage import BaggageLogProcessor, BaggageSpanProcessor  # package __init__ imports the _logs tier, so the pair defers to install time
lazy from opentelemetry.sdk._logs import LoggerProvider
lazy from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogExporter

# --- [TYPES] ----------------------------------------------------------------------------


class Signal(StrEnum):
    TRACE = "traces"
    METRIC = "metrics"
    LOG = "logs"


class InstallOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"


class Drainable(Protocol):
    def force_flush(self, timeout_millis: int = ...) -> bool: ...
    def shutdown(self) -> None: ...


type SignalExporter = SpanExporter | LogExporter
type ExporterFactory = Callable[[str, Mapping[str, str] | None, Compression, ApiMeterProvider], SignalExporter]
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
# no-drift law), `InstallReceipt.profile` carrying the resolved key separately.
class SignalProfile(Struct, frozen=True):
    export_interval_ms: int
    schedule_delay_ms: int
    max_queue_size: int
    max_export_batch_size: int
    compression: Compression


class SignalSpec(Struct, frozen=True):
    signal: Signal
    exporter: ExporterFactory
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
# per-signal path derives here and one base endpoint fans to three non-colliding sinks.
def _signal_endpoint(base: str, signal: Signal) -> str:
    return f"{base.rstrip('/')}/v1/{signal.value}"


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


def _meter_provider(endpoint: str, headers: Mapping[str, str] | None, profile: SignalProfile) -> MeterProvider:
    exporter = OTLPMetricExporter(
        endpoint=_signal_endpoint(endpoint, Signal.METRIC),
        headers=headers,
        timeout=EXPORT_TIMEOUT_S,
        compression=profile.compression,
        preferred_temporality={Counter: AggregationTemporality.DELTA},
    )
    reader = PeriodicExportingMetricReader(exporter, export_interval_millis=profile.export_interval_ms)
    # `exemplar_filter` arms the metrics `context=` hand-off: a sampled active span admits the
    # measurement's exemplar; the SDK default filter would drop every one.
    return MeterProvider(metric_readers=[reader], resource=RUNTIME_RESOURCE, exemplar_filter=TraceBasedExemplarFilter())


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
        lambda ep, hd, comp, mp: OTLPSpanExporter(endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=comp, meter_provider=mp),
        _tracer_provider,
        _batched(TracerProvider.add_span_processor, BatchSpanProcessor),
        trace.set_tracer_provider,
    ),
    SignalSpec(
        Signal.LOG,
        # every LOG cell defers its lazy dereference to call time — a module-scope bound-method or module-attribute read
        # reifies the cold _logs tier at import — so the row's install cost lands only when the INPROCESS_OTLP hatch selects it.
        lambda ep, hd, comp, mp: OTLPLogExporter(endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=comp, meter_provider=mp),
        lambda resource, _: LoggerProvider(resource=resource),
        _log_attach,
        lambda provider: _logs.set_logger_provider(provider),
    ),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    _receipt: ClassVar[InstallReceipt | None] = None
    _installed: ClassVar[InstalledProviders | None] = None

    @classmethod
    @latched(lambda: Telemetry._receipt, lambda r: setattr(Telemetry, "_receipt", r), lambda prior: replace(prior, outcome=InstallOutcome.REENTRANT))
    def install(
        cls,
        profile: RuntimeProfile,
        endpoint: str,
        headers: Mapping[str, str] | None = None,
        *,
        resource: Resource | None = None,
        signal_profile: SignalProfile | None = None,
        ship: LogShip = LogShip.STDOUT_COLLECTOR,
    ) -> InstallReceipt:
        # resource/signal_profile are injection seams: the profiles-owned job envelope hands in its hand-built identity
        # and high-interval geometry; every daemon path resolves the standing rows.
        identity = resource if resource is not None else RUNTIME_RESOURCE
        geometry = signal_profile if signal_profile is not None else SIGNAL_PROFILE[profile]
        if not PROFILE_POLICY[profile].emit_otel:
            return InstallReceipt(profile, InstallOutcome.SILENT, endpoint, geometry)
        meter = _meter_provider(endpoint, headers, geometry)
        metrics.set_meter_provider(meter)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = spec.exporter(_signal_endpoint(endpoint, spec.signal), headers, geometry.compression, meter)
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
    def shutdown(cls) -> RuntimeRail[Block[Signal]]:
        match cls._installed:
            case InstalledProviders() as installed:
                drained = installed.flush()
                cls._installed = cls._receipt = None
                return drained
            case _:
                cls._receipt = None
                return Ok(Block.empty())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
