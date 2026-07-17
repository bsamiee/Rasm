# [PY_RUNTIME_TELEMETRY]

`Telemetry` is the one composition-root install owner for OTLP signal egress — every other observability surface assumes the providers it registers are already wired and owns no construction. Its whole mint sits behind the imported `latched` one-shot aspect, so a re-entrant `install` returns the cached `InstallReceipt` stamped `REENTRANT`, and the install is profile-gated by the `execution/admission#CONTEXT` `emit_otel` policy cell: an emitting profile installs the batched OTLP trio, a silent profile leaves the `opentelemetry-api` no-op providers in place and opens no batch thread.

One `SIGNAL_SPECS` fold owns the batched span/log pair; the meter stands beside the fold because its `metric_readers` are construction-only — the SDK forbids a later `add` — built FIRST and threaded as the `meter_provider=` self-observability sink into the span/log exporters. Its installed composite propagator seeds one cross-language trace for the inbound `observability/receipts#RECEIPT` extract and the outbound `transport/serve#CAPABILITY_INVOKE` client interceptors, and `PARENT_SAMPLER` honors the C#-minted parent's sampled bit, so the Python leg cannot re-sample and fracture `ONE_DISTRIBUTED_TRACE` — the cross-runtime probabilistic floor stays AppHost-owned. `MeterProvider` carries `exemplar_filter=TraceBasedExemplarFilter()`, the install half of the `observability/metrics#METRIC` exemplar contract.

## [01]-[INDEX]

- [01]-[TELEMETRY]: the `latched` composition-root install over the `SIGNAL_SPECS` fold and `SIGNAL_PROFILE` gate, the detector-merged `RUNTIME_RESOURCE`, and the per-signal railed host drain.

## [02]-[TELEMETRY]

- Owner: a `SignalSpec` row owns the exporter factory, the `(provider, attach)` wiring pair, and the global-register cell, so the span/log install is one fold and a new batch-processor signal is one row; the `_batched` kernel lifts the bound `add_*` method and processor class over one shared queue-triple wiring, never two sibling closures. `InstalledProviders` holds the trio as one atomically-assigned `Drainable`-typed carrier, never parallel `| None` provider slots.
- Entry: the silent gate caches a `SILENT` receipt, so a later re-admission under the same process still no-ops, and the cost of OTel under a silent profile is exactly the API no-op providers. `shutdown` folds `force_flush`-then-`shutdown` per provider through `boundary(signal.value, ...)` under `traversed(ACCUMULATE)` — a wedged exporter never short-circuits the remaining flushes and classifies with its per-signal identity intact — then clears the carrier and the latch, re-arming a clean re-install.
- Auto: `RUNTIME_RESOURCE` orders the env detector last, so a deployment-time `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge without a code change and the three signals join one identity — a bare `{"service.name": ...}` literal degrades to `unknown_service`. Exporters' own `create_exporter_metrics` counters land on the one threaded meter when `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED` admits them, never a parallel pipeline. Serve-leg `SERVER` span emission stays `transport/serve#SERVE`-owned: its defaulted `tracer_provider` resolves the global this install registers, and a global `GrpcAioInstrumentorServer` double-patches the leg — two overlapping `SERVER` spans per RPC, the serve health-check filter bypassed — so no instrumentor activates here.
- Growth: a new batch-processor signal is one `Signal` member plus one `SignalSpec` row reaching the install and drain folds with no entrypoint edit; a construction-only-reader signal seeds the carrier beside the fold and registers through the body's matching `set_*_provider`, since it owns no `attach` step; a new per-profile geometry or transport knob is one `SignalProfile` column plus its `SIGNAL_PROFILE` values; a new propagator format one `PROPAGATORS` row folded by the one `set_global_textmap`; a new resource detector one entry in the `get_aggregated_resources` list.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, no AppHost telemetry envelope, sampler-floor ownership, or product export, and no SDK import outside this owner. `views=` stays a deliberate non-wire constructor slot — the duration histogram ships the API-level bucket advisory the SDK default aggregation honors — until a deployment overrides an instrument's shape against its advisory.

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
from opentelemetry import _logs, metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.metrics import Counter, MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.propagators.textmap import TextMapPropagator
from opentelemetry.sdk._logs import LoggerProvider
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogExporter
from opentelemetry.sdk.metrics import MeterProvider, TraceBasedExemplarFilter
from opentelemetry.sdk.metrics.export import AggregationTemporality, PeriodicExportingMetricReader
from opentelemetry.sdk.resources import (
    SERVICE_INSTANCE_ID,
    SERVICE_NAME,
    SERVICE_VERSION,
    OsResourceDetector,
    OTELResourceDetector,
    ProcessResourceDetector,
    Resource,
    get_aggregated_resources,
)
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, SpanExporter
from opentelemetry.sdk.trace.sampling import ALWAYS_ON, ParentBased, Sampler
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator

from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile
from rasm.runtime.faults import SCOPES, Disposition, RuntimeRail, Scope, boundary, latched, traversed

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

# service identity reads the faults-owned SCOPES row — never a second "rasm-companion" literal.
RUNTIME_RESOURCE: Final[Resource] = get_aggregated_resources(
    [ProcessResourceDetector(), OsResourceDetector(), OTELResourceDetector()],
    initial_resource=Resource.create(
        {SERVICE_NAME: SCOPES[Scope.SERVICE], SERVICE_VERSION: version("rasm-runtime"), SERVICE_INSTANCE_ID: uuid4().hex}
    ),
)

PARENT_SAMPLER: Final[Sampler] = ParentBased(root=ALWAYS_ON)

# W3C composite the install folds into one `set_global_textmap`; a new wire format is one
# row here, never a second propagator install — the C# CORRELATION_SPINE composite at the wire.
PROPAGATORS: Final[Sequence[TextMapPropagator]] = (TraceContextTextMapPropagator(), W3CBaggagePropagator())

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
        lambda resource, sampler: TracerProvider(resource=resource, sampler=sampler),
        _batched(TracerProvider.add_span_processor, BatchSpanProcessor),
        trace.set_tracer_provider,
    ),
    SignalSpec(
        Signal.LOG,
        lambda ep, hd, comp, mp: OTLPLogExporter(endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=comp, meter_provider=mp),
        lambda resource, _: LoggerProvider(resource=resource),
        _batched(LoggerProvider.add_log_record_processor, BatchLogRecordProcessor),
        _logs.set_logger_provider,
    ),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    _receipt: ClassVar[InstallReceipt | None] = None
    _installed: ClassVar[InstalledProviders | None] = None

    @classmethod
    @latched(lambda: Telemetry._receipt, lambda r: setattr(Telemetry, "_receipt", r), lambda prior: replace(prior, outcome=InstallOutcome.REENTRANT))
    def install(cls, profile: RuntimeProfile, endpoint: str, headers: Mapping[str, str] | None = None) -> InstallReceipt:
        signal_profile = SIGNAL_PROFILE[profile]
        if not PROFILE_POLICY[profile].emit_otel:
            return InstallReceipt(profile, InstallOutcome.SILENT, endpoint, signal_profile)
        meter = _meter_provider(endpoint, headers, signal_profile)
        metrics.set_meter_provider(meter)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = spec.exporter(_signal_endpoint(endpoint, spec.signal), headers, signal_profile.compression, meter)
            provider = spec.provider(RUNTIME_RESOURCE, PARENT_SAMPLER)
            spec.attach(provider, exporter, signal_profile)
            spec.register(provider)
            return installed.add(spec.signal, provider)

        seed: Map[Signal, Drainable] = Map.empty().add(Signal.METRIC, meter)
        by_signal = SIGNAL_SPECS.fold(emit, seed)
        propagate.set_global_textmap(CompositePropagator(list(PROPAGATORS)))
        cls._installed = InstalledProviders(by_signal=by_signal)
        return InstallReceipt(profile, InstallOutcome.INSTALLED, endpoint, signal_profile)

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
