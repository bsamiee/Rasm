# [PY_RUNTIME_TELEMETRY]

The one composition-root install owner for the OTLP signal egress every other observability surface assumes already wired. `Telemetry` is the single static surface that mints the detector-merged `RUNTIME_RESOURCE`, folds the `SIGNAL_SPECS` row table into the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio over the one `OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` family under `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`, registers each provider globally through the spec row's `set_*_provider` cell, and installs the composite `TextMapPropagator` through `set_global_textmap` so the inbound extract (`observability/receipts#RECEIPT`) and the outbound client interceptors (`transport/serve#CAPABILITY_INVOKE`) resolve one cross-language trace, the whole composition gated behind the `latched` one-shot aspect — a re-entrant call reads the cached `InstallReceipt` and returns it stamped `REENTRANT`, never re-running the mint. The trio is no per-signal arm scatter: a `Signal`-keyed `SignalSpec` row owns the exporter factory, the provider-wiring closure, and the global-register callable, so `install` is one `Block.fold` over the table and a new signal modality is one row, not an `install` edit. The provider trio threads four `opentelemetry`/`expression` capabilities as one rail rather than one feature each — the `MeterProvider` is built first and passed as the `meter_provider=` self-observability sink into the span/log exporters so the exporters' own success/failure/duration counters ride the same pipeline with no second meter; the `TracerProvider` carries `PARENT_SAMPLER` (`ParentBased(root=ALWAYS_ON)`) so the local leg honors the C#-minted parent's sampled decision rather than re-sampling the inbound trace independently; `RUNTIME_RESOURCE` is the `get_aggregated_resources` merge of `ProcessResourceDetector`/`OsResourceDetector` over the `SERVICE_NAME`/`SERVICE_VERSION`/`SERVICE_INSTANCE_ID` triple, not a bare `service.name` literal; and the host drain folds `force_flush`-then-`shutdown` over every installed provider through the `reliability/faults#FAULT` `boundary`+`traversed` rail into a typed `RuntimeRail[Block[Signal]]`, so a wedged exporter session surfaces as an accumulated `BoundaryFault` member rather than a swallowed `except: continue` — and because the SDK `force_flush` signals a drained-queue timeout by returning `False` rather than raising, the drain kernel raises `TimeoutError` on a falsy flush so the `boundary` conversion folds the timed-out queue onto the `deadline` fault class instead of treating an undrained queue as a clean shutdown. The `grpc.aio` serve leg's `SERVER` span emission is owned by `transport/serve#SERVE`, which wires `aio_server_interceptor` into its server at construction; `Telemetry` installs only the provider trio + composite propagator, and the serve interceptor's defaulted `tracer_provider` resolves the global `TracerProvider` this install registers — `Telemetry` never globally patches the serve leg. The install is profile-gated by the `execution/admission#CONTEXT` `emit_otel` policy row through the frozen `SIGNAL_PROFILE` table keyed by `RuntimeProfile` — a `SIDECAR`/`TOOL` profile installs the batched OTLP trio, a `PACKAGE`/`TEST` profile leaves the `opentelemetry-api` no-op providers in place and emits nothing. The siblings read the installed providers and own no construction: `observability/metrics#METRIC` reads the installed `MeterProvider` through `metrics.get_meter`, `observability/receipts#RECEIPT` reads the installed `LoggerProvider` and the trace context the propagator seeds, and the providers all carry the one `RUNTIME_RESOURCE` so the three signals join one `service.name`. The package installs the local signal pipeline only; the product telemetry envelope, sampler floor, and health stay AppHost-owned — the C# `diagnostics` one-`UseOtlpExporter`, one-`Resource`, one-propagator parity bar realized on the cp315 core.

## [01]-[INDEX]

- [01]-[TELEMETRY]: `Telemetry` composition-root install returning the typed `InstallReceipt`, the `latched` one-shot install aspect, the `SIGNAL_SPECS` exporter/wiring/register row table folded into the provider trio, the `SIGNAL_PROFILE` batch-geometry/compression table read under the `execution/admission#CONTEXT` `emit_otel` gate, the detector-merged `RUNTIME_RESOURCE`, the `PARENT_SAMPLER` parent-respecting head sampler, the `meter_provider=` self-observability loop, the `Drainable`-typed `InstalledProviders` carrier, and the host-drain `force_flush`-then-`shutdown` fold over the `boundary`+`traversed` rail that raises `TimeoutError` on a falsy flush so an undrained queue crosses the `deadline` fault class rather than passing clean.

## [02]-[TELEMETRY]

- Owner: `Telemetry` — the static composition-root surface owning one `Telemetry.install(profile, endpoint, headers)` entrypoint that folds the `SIGNAL_SPECS` table into the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio, registers each through its spec row's `set_*_provider` cell, installs the composite propagator, and returns the typed `InstallReceipt`, all behind the `latched` aspect — the cached `_receipt` slot — plus `Telemetry.shutdown` folding the host-drain flush over the `boundary` rail; the `grpc.aio` serve-leg span emission is the `transport/serve#SERVE` construction-time `aio_server_interceptor`'s, never a global instrumentor here; `Signal` the `StrEnum` keying the row table to the OTLP signal-path segment (`traces`/`metrics`/`logs`); `SignalSpec` the row carrying the per-signal exporter factory, provider-wiring closure, and global-register callable so the trio is one fold over a `Block[SignalSpec]` rather than three parallel arms; `SignalProfile` the frozen `Map[RuntimeProfile, SignalProfile]` table whose row carries only the telemetry-local per-profile batch geometry — the `export_interval_ms` metric-reader cadence plus the full `BatchSpanProcessor`/`BatchLogRecordProcessor` queue triple (`schedule_delay_ms`/`max_queue_size`/`max_export_batch_size`) and the per-profile `Compression` (`Gzip` on the emitting `SIDECAR`/`TOOL` rows, `NoCompression` on the silent `PACKAGE`/`TEST` rows) — the emit gate staying the authoritative `execution/admission#CONTEXT` `PROFILE_POLICY[profile].emit_otel` cell rather than a mirrored column; `InstalledProviders` the one frozen carrier holding the `Signal`-keyed registered providers typed to the structural `Drainable` protocol (`force_flush(timeout_millis) -> bool` + `shutdown()`, the one surface the trio shares and the drain folds), collapsing the prior four `| None` provider slots into a single atomically-assigned reference (the sibling `observability/metrics#METRIC` `MetricState` swap idiom) that the drain folds and clears; `InstallReceipt` the frozen install evidence the host drain and tests read for the resolved profile, the `InstallOutcome` (`INSTALLED`/`SILENT`/`REENTRANT`), the endpoint, and the resolved `SignalProfile` carried by reference rather than re-spelled column-for-column, the per-signal drain evidence staying the separate `shutdown` `RuntimeRail[Block[Signal]]` return rather than a vestigial receipt column; `RUNTIME_RESOURCE` the one detector-merged `Resource` all three signals join.
- Cases: the install dispatches a three-way decision keyed off the `latched` aspect then the `PROFILE_POLICY[profile].emit_otel` gate over the admitted `RuntimeProfile` — a re-entrant `install` (a cached `InstallReceipt`) returns the prior receipt `msgspec.structs.replace`-stamped `REENTRANT` before the mint body runs, so the aspect owns idempotency and the body never re-reads the latch; a `silent` gate (`PACKAGE`/`TEST`, `emit_otel=False`) caches the `SILENT` receipt carrying the resolved `SignalProfile` and the `opentelemetry-api` no-op providers stay, so a library/test import touches no exporter and opens no batch thread; an `emit` gate (`SIDECAR`/`TOOL`, `emit_otel=True`) builds the meter first, folds the `SIGNAL_SPECS` table into the span/log providers threading the meter as their `meter_provider=` self-observability sink, installs the composite propagator, swaps the `InstalledProviders` carrier, and returns the `INSTALLED` receipt; the modality is the admission policy cell and the spec-row table, never a per-signal toggle the caller re-derives or a column mirrored onto `SignalProfile`. The host drain is itself a railed fold: `shutdown` matches the `InstalledProviders` carrier and folds `force_flush`-then-`shutdown` over each provider through `boundary("resource", ...)` accumulated by `traversed`, the drain kernel raising `TimeoutError` when `force_flush` returns `False` (the SDK's non-raising undrained-queue signal) so a timed-out queue crosses the `CLASSIFY` `deadline` row rather than passing as a clean drain, so the `RuntimeRail[Block[Signal]]` carries every drained signal on success or the accumulated `BoundaryFault` members on a wedged session — never a swallowed `except`, and never a falsy-flush silently dropped.
- Entry: `Telemetry.install(profile, endpoint, headers)` is wrapped by the `latched(read, write)` aspect — a slot-parametric one-shot guard closed over a `(read, write)` accessor pair onto the `_receipt` latch rather than a name-bound reference to the class, so the idempotency aspect carries no hardcoded owner and reads/writes only through the injected slot — which matches the cached `_receipt` and returns it `replace`-stamped `REENTRANT` before any side effect, so a second `install` under the same process — a re-admission, a re-entrant composition root, a test re-bootstrap — never re-mints a provider or re-opens a batch thread; the mint body returns early on the `silent` gate (caching the `SILENT` receipt so a later `emit` re-admission under the same process still no-ops), and on `emit` builds the `MeterProvider` once through `_meter_provider` (the `OTLPMetricExporter` carrying `preferred_temporality={Counter: DELTA}` for the OTLP-delta backend, wrapped in a `PeriodicExportingMetricReader` over the `SignalProfile.export_interval_ms`), registers it, then folds `SIGNAL_SPECS` — each row's `exporter(endpoint, headers, compression, meter)` factory minting the `OTLPSpanExporter`/`OTLPLogExporter` against the resolved `(endpoint, headers, timeout, compression)` with the meter threaded as the `meter_provider=` self-observability sink (`headers` carrying the secret-resolved bearer `OTEL_EXPORTER_OTLP_HEADERS`, `compression` the `SignalProfile.compression` `Compression.Gzip` column), the row's `wire` closure wrapping it in the `BatchSpanProcessor`/`BatchLogRecordProcessor` provider over `RUNTIME_RESOURCE` and `PARENT_SAMPLER`, and the row's `register` cell installing it through `trace.set_tracer_provider`/`_logs.set_logger_provider` — installs the `TraceContextTextMapPropagator`+`W3CBaggagePropagator` composite through `propagate.set_global_textmap`, swaps the `InstalledProviders` carrier as the single terminal state write, and the `latched` aspect caches the `INSTALLED` receipt; `Telemetry.shutdown` folds the host-drain flush over the carrier through the `boundary`+`traversed` rail and clears the carrier and the receipt latch, re-arming a subsequent `install` cleanly, returning the `RuntimeRail[Block[Signal]]` drain evidence (`Ok(Block.empty())` when nothing was installed). The serve leg's `SERVER` span emission is wired once at `transport/serve#SERVE` server construction and resolves the `TracerProvider` this install registers, so `Telemetry` activates no gRPC instrumentor and reverts none on drain.
- Auto: the providers are activated once per process at the composition root and never imported by library code — the SDK imports live in this owner alone; `endpoint` is the OTLP base root (the `execution/admission#SETTINGS` `otel_endpoint`, the `OTEL_EXPORTER_OTLP_ENDPOINT` base-URL contract), never a path-suffixed URL, so each signal derives its non-colliding `/v1/{Signal}` path (`/v1/traces`, `/v1/metrics`, `/v1/logs`) off the one base, the `Signal` `StrEnum` value being that path segment; `RUNTIME_RESOURCE` is the `get_aggregated_resources` merge of `ProcessResourceDetector` (pid/runtime/command) and `OsResourceDetector` (os type/version) over the `SERVICE_NAME=rasm-companion`/`SERVICE_VERSION`/`SERVICE_INSTANCE_ID` triple, so the three signals join one fully-attributed identity and `observability/metrics#METRIC`/`observability/receipts#RECEIPT` read it off the installed providers rather than minting a second — the bare `{"service.name": ...}` literal the SDK `LOCAL_ADMISSION` warns degrades to `unknown_service` is the deleted form; the meter is constructed first and threaded into the span/log exporters as `meter_provider=` so the exporters' own `create_exporter_metrics` success/failure/duration counters land on the same `MeterProvider` (the exporter `INTEGRATION_LAW` self-observability loop) rather than a parallel pipeline; `PARENT_SAMPLER` (`ParentBased(root=ALWAYS_ON)`) routes the head-sampling decision by the inbound parent's sampled flag — a remote-sampled parent keeps the local SERVER span sampled and a remote-dropped parent drops it — so the Python leg cannot independently re-sample the C#-minted trace and fracture `ONE_DISTRIBUTED_TRACE`, the cross-runtime probabilistic floor still staying AppHost-owned; the install is idempotent through the `latched` aspect, which reads the cached `_receipt` before the mint body and returns it `REENTRANT`-stamped, so a re-admission touches no provider; the `silent` profile installs zero exporters and opens zero batch threads, so the cost of OTel under a `PACKAGE`/`TEST` profile is exactly the API no-op providers `opentelemetry-api` already supplies; `propagate.set_global_textmap` populates the global composite `propagate.get_global_textmap` resolves, so the Wave-2 inbound `propagate.extract` and the Wave-3 `aio_client_interceptors` seed the C# parent rather than a fresh root; the host drain reads the `InstalledProviders` carrier and folds `force_flush(timeout_millis)`-then-`shutdown()` over each provider — `force_flush` draining the batch queue before `shutdown` tears the background thread — through `boundary("resource", ...)` accumulated by `traversed(..., accumulate=True)`, so a wedged exporter never short-circuits the remaining flushes and the failure is a structurally-addressable `BoundaryFault` aggregate member rather than a discarded exception.
- Packages: `opentelemetry-sdk` (`sdk.trace.TracerProvider` construction + `sampler=` ENTRYPOINTS [01] + `add_span_processor` [02] + `force_flush` [04] + `shutdown` [05] / `sdk.trace.sampling.ParentBased` [09] + `ALWAYS_ON` [13] / `sdk.metrics.MeterProvider` ENTRYPOINTS [01] + `force_flush` [02] + `shutdown` [03] + `PeriodicExportingMetricReader` [04] + `AggregationTemporality` PUBLIC_TYPES [07] / `sdk._logs.LoggerProvider` ENTRYPOINTS [01] + `add_log_record_processor` [02] + `force_flush` [03] + `shutdown` [04] + `BatchLogRecordProcessor(exporter, max_queue_size, schedule_delay_millis, max_export_batch_size)` [05] + `LogExporter` PUBLIC_TYPES [10] / `BatchSpanProcessor(span_exporter, max_queue_size, schedule_delay_millis, max_export_batch_size)` ENTRYPOINTS [06] + `SpanExporter` PUBLIC_TYPES [03] / `Resource.create` resource [01] + `get_aggregated_resources` [06] + `SERVICE_NAME`/`SERVICE_VERSION`/`SERVICE_INSTANCE_ID` [07] + `ProcessResourceDetector`/`OsResourceDetector` PUBLIC_TYPES [17]/[18]), `opentelemetry-exporter-otlp-proto-http` (`OTLPSpanExporter`/`OTLPLogExporter` over the shared `(endpoint, headers, timeout, compression)` constructor + keyword-only `meter_provider=` ENTRYPOINTS [01] / `OTLPMetricExporter` + `preferred_temporality` [01] / `Compression.Gzip`/`Compression.NoCompression` PUBLIC_TYPES [04]), `opentelemetry-api` (`trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider` + `propagate.set_global_textmap` installing the `TextMapPropagator` composite + `metrics.Counter` PUBLIC_TYPES [03] for the temporality key), `expression` (`Block.of_seq`/`Block.fold`/`Block.map` over the `SIGNAL_SPECS` table, `Map.of_seq`/`Map.add`/`Map.items` over the profile and installed-provider tables, `Ok` and the `reliability/faults#FAULT` `RuntimeRail`/`boundary`/`traversed` drain rail), `msgspec` (the frozen `SignalProfile`/`SignalSpec`/`InstalledProviders`/`InstallReceipt` carriers and `structs.replace` for the `REENTRANT` re-stamp). `typing.Protocol` types the `Drainable` structural surface (`force_flush(timeout_millis) -> bool` + `shutdown()`) the heterogeneous trio shares so the carrier and drain fold are typed against a real protocol rather than `object`; `importlib.metadata.version` reads the installed `rasm-runtime` distribution for `SERVICE_VERSION`.
- Growth: a new signal modality is one `Signal` member plus one `SignalSpec` row (exporter factory, wiring closure, register cell) in `SIGNAL_SPECS`, reaching the install fold and the drain fold with no `install`/`shutdown` edit; a new per-profile batch geometry or transport knob is one column on `SignalProfile` plus its value in each `SIGNAL_PROFILE` tuple, never a per-signal flag; a new propagator format is one entry in the `CompositePropagator` list passed to `set_global_textmap`, never a second propagator install; a new install outcome is one `InstallOutcome` member; a new resource detector is one entry in the `get_aggregated_resources` detector list; zero new entrypoint, one install gate, one typed receipt.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, AppHost telemetry envelope, cross-runtime sampler-floor ownership (the local `PARENT_SAMPLER` honors the parent decision but mints no probabilistic floor), product signal export, exporter ownership beyond the shared OTLP family, or second install latch; an SDK import outside `install`, a per-signal provider/exporter arm scatter where the `SIGNAL_SPECS` fold belongs, four parallel `| None` provider slots where the one `InstalledProviders` carrier holds them, a stringly `Literal` outcome where the `InstallOutcome` `StrEnum` belongs, an `object`-typed provider slot or factory return where the `Drainable` protocol and the `SpanExporter`/`LogExporter` union type the carrier, a name-bound `latched` guard hardcoding `Telemetry._receipt` where the slot-parametric `latched(read, write)` aspect carries the latch through injected accessors, a `try/except: continue` swallow on drain where the `boundary`+`traversed` rail accumulates the fault, a discarded `force_flush() -> bool` whose `False` (undrained-queue timeout) passes as a clean drain where the kernel raises `TimeoutError` onto the `deadline` fault class, a second `MeterProvider` for the exporter self-observability counters where the `meter_provider=` loop reuses the one meter, a global `GrpcAioInstrumentorServer` activation that double-patches the `transport/serve#SERVE` leg already wired with `aio_server_interceptor` at construction (the global instrumentor is the fallback only for a server built outside runtime control, which this runtime never does — it would emit two overlapping SERVER spans per RPC and bypass the serve leg's `filters.negate(filters.health_check())`), an eager install under a `silent` (`emit_otel=False`) profile, a bare `{"service.name": ...}` `Resource` literal beside the detector-merged `RUNTIME_RESOURCE`, and a fresh-root propagator that drops the inbound composite are the deleted forms; the serve-leg server-span emission stays `transport/serve#SERVE`-owned, and the suite telemetry taxonomy and the cross-runtime sampler floor stay AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from enum import StrEnum
from functools import wraps
from importlib.metadata import version
from typing import ClassVar, Final, Protocol
from uuid import uuid4

from expression import Block, Ok
from expression.collections import Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import _logs, metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.metrics import Counter, MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator
from opentelemetry.sdk._logs import LoggerProvider
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogExporter
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import AggregationTemporality, PeriodicExportingMetricReader
from opentelemetry.sdk.resources import (
    SERVICE_INSTANCE_ID,
    SERVICE_NAME,
    SERVICE_VERSION,
    OsResourceDetector,
    ProcessResourceDetector,
    Resource,
    get_aggregated_resources,
)
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, SpanExporter
from opentelemetry.sdk.trace.sampling import ALWAYS_ON, ParentBased, Sampler
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter

from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile
from rasm.runtime.faults import RuntimeRail, boundary, traversed

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
type ProviderWiring = Callable[[SignalExporter, Resource, "SignalProfile", Sampler], Drainable]

# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORT_TIMEOUT_S: Final[float] = 10.0

RUNTIME_RESOURCE: Final[Resource] = get_aggregated_resources(
    [ProcessResourceDetector(), OsResourceDetector()],
    initial_resource=Resource.create({
        SERVICE_NAME: "rasm-companion",
        SERVICE_VERSION: version("rasm-runtime"),
        SERVICE_INSTANCE_ID: uuid4().hex,
    }),
)

PARENT_SAMPLER: Final[Sampler] = ParentBased(root=ALWAYS_ON)

# --- [MODELS] ---------------------------------------------------------------------------


class SignalProfile(Struct, frozen=True):
    profile: RuntimeProfile
    export_interval_ms: int
    schedule_delay_ms: int
    max_queue_size: int
    max_export_batch_size: int
    compression: Compression


SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq(
    (p, SignalProfile(p, export_interval_ms=i, schedule_delay_ms=d, max_queue_size=q, max_export_batch_size=b, compression=c))
    for p, i, d, q, b, c in (
        (RuntimeProfile.SIDECAR, 2000, 1000, 2048, 512, Compression.Gzip),
        (RuntimeProfile.TOOL, 5000, 5000, 512, 128, Compression.Gzip),
        (RuntimeProfile.PACKAGE, 5000, 5000, 512, 128, Compression.NoCompression),
        (RuntimeProfile.TEST, 5000, 5000, 512, 128, Compression.NoCompression),
    )
)


class SignalSpec(Struct, frozen=True):
    signal: Signal
    exporter: ExporterFactory
    wire: ProviderWiring
    register: Callable[[Drainable], None]


def _trace_provider(exporter: SignalExporter, resource: Resource, profile: SignalProfile, sampler: Sampler) -> TracerProvider:
    provider = TracerProvider(resource=resource, sampler=sampler)
    provider.add_span_processor(BatchSpanProcessor(
        exporter, max_queue_size=profile.max_queue_size,
        schedule_delay_millis=profile.schedule_delay_ms, max_export_batch_size=profile.max_export_batch_size,
    ))
    return provider


def _logger_provider(exporter: SignalExporter, resource: Resource, profile: SignalProfile, _: Sampler) -> LoggerProvider:
    provider = LoggerProvider(resource=resource)
    provider.add_log_record_processor(BatchLogRecordProcessor(
        exporter, max_queue_size=profile.max_queue_size,
        schedule_delay_millis=profile.schedule_delay_ms, max_export_batch_size=profile.max_export_batch_size,
    ))
    return provider


SIGNAL_SPECS: Final[Block[SignalSpec]] = Block.of_seq([
    SignalSpec(
        Signal.TRACE,
        lambda ep, hd, comp, mp: OTLPSpanExporter(
            endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=comp, meter_provider=mp
        ),
        _trace_provider,
        trace.set_tracer_provider,
    ),
    SignalSpec(
        Signal.LOG,
        lambda ep, hd, comp, mp: OTLPLogExporter(
            endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=comp, meter_provider=mp
        ),
        _logger_provider,
        _logs.set_logger_provider,
    ),
])


def _meter_provider(endpoint: str, headers: Mapping[str, str] | None, profile: SignalProfile) -> MeterProvider:
    exporter = OTLPMetricExporter(
        endpoint=endpoint, headers=headers, timeout=EXPORT_TIMEOUT_S, compression=profile.compression,
        preferred_temporality={Counter: AggregationTemporality.DELTA},
    )
    reader = PeriodicExportingMetricReader(exporter, export_interval_millis=profile.export_interval_ms)
    return MeterProvider(metric_readers=[reader], resource=RUNTIME_RESOURCE)


def _drained(provider: Drainable, signal: Signal) -> Signal:
    if not provider.force_flush(timeout_millis=int(EXPORT_TIMEOUT_S * 1000)):
        raise TimeoutError(signal.value)
    provider.shutdown()
    return signal


class InstalledProviders(Struct, frozen=True):
    by_signal: Map[Signal, Drainable]

    def flush(self) -> RuntimeRail[Block[Signal]]:
        rails = Block.of_seq(self.by_signal.items()).map(
            lambda kv: boundary("resource", lambda kv=kv: _drained(kv[1], kv[0]))
        )
        return traversed(rails, accumulate=True)


class InstallReceipt(Struct, frozen=True):
    profile: RuntimeProfile
    outcome: InstallOutcome
    endpoint: str
    signal_profile: SignalProfile

# --- [SERVICES] -------------------------------------------------------------------------


def latched[**P](
    read: Callable[[], InstallReceipt | None], write: Callable[[InstallReceipt], None]
) -> Callable[[Callable[P, InstallReceipt]], Callable[P, InstallReceipt]]:
    def aspect(mint: Callable[P, InstallReceipt]) -> Callable[P, InstallReceipt]:
        @wraps(mint)
        def guarded(*args: P.args, **kwargs: P.kwargs) -> InstallReceipt:
            match read():
                case InstallReceipt() as prior:
                    return replace(prior, outcome=InstallOutcome.REENTRANT)
                case _:
                    write(receipt := mint(*args, **kwargs))
                    return receipt

        return guarded

    return aspect


class Telemetry:
    _receipt: ClassVar[InstallReceipt | None] = None
    _installed: ClassVar[InstalledProviders | None] = None

    @classmethod
    @latched(lambda: Telemetry._receipt, lambda r: setattr(Telemetry, "_receipt", r))
    def install(
        cls, profile: RuntimeProfile, endpoint: str, headers: Mapping[str, str] | None = None
    ) -> InstallReceipt:
        signal_profile = SIGNAL_PROFILE[profile]
        if not PROFILE_POLICY[profile].emit_otel:
            return InstallReceipt(profile, InstallOutcome.SILENT, endpoint, signal_profile)
        meter = _meter_provider(endpoint, headers, signal_profile)
        metrics.set_meter_provider(meter)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = spec.exporter(endpoint, headers, signal_profile.compression, meter)
            provider = spec.wire(exporter, RUNTIME_RESOURCE, signal_profile, PARENT_SAMPLER)
            spec.register(provider)
            return installed.add(spec.signal, provider)

        seed: Map[Signal, Drainable] = Map.empty().add(Signal.METRIC, meter)
        by_signal = SIGNAL_SPECS.fold(emit, seed)
        propagate.set_global_textmap(CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()]))
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

- [OTLP_PROVIDER_INSTALL]: reflection-confirmed against `.api/opentelemetry-sdk.md` — the provider trio `sdk.trace.TracerProvider(resource, sampler)` (TracerProvider ENTRYPOINTS [01]) + `add_span_processor(BatchSpanProcessor(exporter, max_queue_size, schedule_delay_millis, max_export_batch_size))` ([02] + ENTRYPOINTS [06]), `sdk.metrics.MeterProvider(metric_readers=[PeriodicExportingMetricReader(exporter, export_interval_millis)], resource)` (MeterProvider ENTRYPOINTS [01] + [04]), and `sdk._logs.LoggerProvider(resource)` + `add_log_record_processor(BatchLogRecordProcessor(exporter, max_queue_size, schedule_delay_millis, max_export_batch_size))` (LoggerProvider ENTRYPOINTS [01] + [02] + [05]) over the `opentelemetry-exporter-otlp-proto-http` exporter family (`OTLPSpanExporter`/`OTLPLogExporter` sharing the `(endpoint, headers, timeout, compression, session, *, meter_provider)` constructor, `OTLPMetricExporter` adding `preferred_temporality`/`preferred_aggregation`) is the production install path. The full `BatchSpanProcessor`/`BatchLogRecordProcessor` queue triple (`max_queue_size`/`schedule_delay_millis`/`max_export_batch_size`, ENTRYPOINTS [06]/[05]) rides the `SignalProfile` row so the batch geometry is per-profile real (the `SIDECAR` row a tight 2048/1s/512 high-throughput queue, the `TOOL`/`PACKAGE`/`TEST` rows a relaxed 512/5s/128 low-cardinality queue) rather than the single `max_queue_size` knob the prior cell wired while the other two batch dimensions defaulted. The trio is authored as a `Block[SignalSpec]` data table rather than three parallel arms: each row owns the exporter factory (typed `ExporterFactory -> SignalExporter`), the provider-wiring closure (`_trace_provider`/`_logger_provider`, typed `ProviderWiring -> Drainable`), and the `set_*_provider` register cell, so `install` is one `Block.fold` building each provider, registering it through the row's cell, and accumulating the `Map[Signal, Drainable]` carrier; the `MeterProvider` is built once outside the fold (the `metric_readers` arg is construction-only per the SDK `[SDK_TOPOLOGY]`, readers cannot be added later) and seeds the carrier so a new signal modality is a row, not an `install` edit. The composite `TextMapPropagator` is `CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()])` installed through `propagate.set_global_textmap`, which `propagate.get_global_textmap` resolves for the inbound `observability/receipts#RECEIPT` extract and the outbound `transport/serve#CAPABILITY_INVOKE` client interceptors — the C# `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` `Propagators.DefaultTextMapPropagator` parity at the wire. The install gate reads the `execution/admission#CONTEXT` `PROFILE_POLICY[profile].emit_otel` cell directly (`SIDECAR`/`TOOL` true → batched OTLP trio, `PACKAGE`/`TEST` false → API no-op providers, zero egress); `SignalProfile` carries only the telemetry-local batch-geometry and `Compression` columns and mirrors no admission boolean, `headers` threading the secret-resolved bearer (`OTEL_EXPORTER_OTLP_HEADERS`) and `compression` the per-profile `Compression` column (`Gzip` on the emitting rows, `NoCompression` on the never-egressing silent rows) into every exporter factory. The `grpc.aio` serve-leg `SERVER` span emission is NOT installed here: per `.api/opentelemetry-instrumentation-grpc.md` IMPLEMENTATION_LAW, explicit `aio_server_interceptor` wiring at `grpc.aio.server(interceptors=[...])` construction is used INSTEAD OF the global `GrpcAioInstrumentorServer` monkeypatch (the global instrumentor is the fallback only when the server is built outside runtime control); the runtime controls its server in `transport/serve#SERVE`, so a global `instrument()` here would double-patch the leg — two overlapping SERVER spans per RPC and an unfiltered global path that re-admits the health-check noise the serve interceptor's `filters.negate(filters.health_check())` suppresses — and is the deleted form. The serve interceptor's defaulted `tracer_provider` resolves the global `TracerProvider` this install registers, so one composition root mints all spans. The provider-install spellings are settled.
- [PARENT_SAMPLER_AND_RESOURCE]: reflection-confirmed against `.api/opentelemetry-sdk.md` — `TracerProvider(sampler=ParentBased(root=ALWAYS_ON))` (sampling ENTRYPOINTS [09] + `ALWAYS_ON` PUBLIC_TYPES [13]) makes the local head-sampling decision follow the inbound parent's `TraceFlags.sampled` bit (`ParentBased` routes a remote-sampled parent to `remote_parent_sampled=ALWAYS_ON` and a remote-dropped parent to `remote_parent_not_sampled=ALWAYS_OFF`), so the Python leg honors the C#-minted decision instead of independently re-sampling and fracturing `ONE_DISTRIBUTED_TRACE`; the cross-runtime probabilistic floor (`TraceIdRatioBased` rate) stays AppHost-owned, the local sampler being parent-deferring only. `RUNTIME_RESOURCE` is `get_aggregated_resources([ProcessResourceDetector(), OsResourceDetector()], initial_resource=Resource.create({SERVICE_NAME: ..., SERVICE_VERSION: version("rasm-runtime"), SERVICE_INSTANCE_ID: uuid4().hex}))` (resource ENTRYPOINTS [01]/[06]/[07], detectors PUBLIC_TYPES [17]/[18]) — the SDK `LOCAL_ADMISSION` requires an explicit `SERVICE_NAME` (a bare unset name degrades to `unknown_service`) and the detectors attach pid/runtime/command and os type/version so the joined identity is fully attributed, not a one-key literal. The sampler and resource spellings are settled.
- [SELF_OBSERVABILITY_LOOP]: reflection-confirmed against `.api/opentelemetry-exporter-otlp-proto-http.md` — the `OTLPSpanExporter`/`OTLPLogExporter` keyword-only `meter_provider=` (ENTRYPOINTS [01], INTEGRATION_LAW "closing the self-observability loop without a second pipeline") routes the exporter's own `create_exporter_metrics` success/failure/duration counters into the same `MeterProvider`, so the meter is constructed first and threaded into the span/log exporter factories rather than standing up a parallel internal-metrics pipeline; the `OTLPMetricExporter` carries `preferred_temporality={Counter: AggregationTemporality.DELTA}` (the temporality decision point set once at construction per the exporter `INTEGRATION_LAW`, matching an OTLP-delta backend) and the metric exporter takes no `meter_provider` (it would observe itself). The self-observability and temporality spellings are settled.
- [LATCH_AND_RAILED_DRAIN]: the one-shot install is the `latched(read, write)` aspect — a slot-parametric decorator factory closed over a `(read, write)` accessor pair onto the `_receipt` latch (not a hardcoded `Telemetry._receipt` reference, so the `[**P]` parametricity is real and the aspect reads/writes only the injected slot) that matches the cached receipt and returns it `msgspec.structs.replace`-stamped `InstallOutcome.REENTRANT` before the mint body runs, writing the slot as the single terminal assignment on first install — so idempotency is one cross-cutting decorator, not a latch read scattered as the first and last statement of the body; the `silent` profile caches its `SILENT` receipt so a later `emit` re-admission under the same process stays a strict no-op, and the returned `InstallReceipt` gives the host drain and tests typed evidence of the resolved profile, the `InstallOutcome`, the endpoint, and the resolved `SignalProfile` carried by reference (no column re-spell). The four prior `| None` provider slots collapse into one frozen `InstalledProviders` carrier keyed `Map[Signal, Drainable]` (the structural `force_flush(timeout_millis) -> bool` + `shutdown()` protocol the trio shares, so the carrier and drain fold are typed against a real protocol rather than `object`) swapped atomically (the sibling `observability/metrics#METRIC` `MetricState` reference-store idiom), and `Telemetry.shutdown` matches the carrier and folds `force_flush(timeout_millis)`-then-`shutdown()` (TracerProvider [04]/[05], MeterProvider [02]/[03], LoggerProvider [03]/[04]) over each provider through the `reliability/faults#FAULT` `boundary("resource", ...)` conversion accumulated by `traversed(..., accumulate=True)`, the kernel raising `TimeoutError` on a `False` `force_flush` (the SDK's `-> bool` undrained-queue signal, which never raises) so a timed-out flush crosses the `CLASSIFY` `deadline` row rather than passing as a clean drain, returning `RuntimeRail[Block[Signal]]` — every drained signal on success or the accumulated `BoundaryFault` members on a wedged exporter session, never a `try/except: continue` swallow and never a falsy-flush silent drop — and clears the carrier and latch so a subsequent `install` re-arms cleanly. The aspect, carrier, and railed-drain spellings are settled. No open RESEARCH seam remains on this page.
