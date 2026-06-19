# [PY_RUNTIME_TELEMETRY]

The one composition-root install owner for the OTLP signal egress every other observability surface assumes already wired. `Telemetry` is the single static surface that mints the shared `Resource`, constructs the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio over the one `OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` family under `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`, registers all three providers globally through the `opentelemetry-api` `set_*_provider` surface, and installs the composite `TextMapPropagator` through `set_global_textmap` so the inbound extract (`observability/receipts#RECEIPT`) and the outbound client interceptors (`transport/serve#CAPABILITY_INVOKE`) resolve one cross-language trace. The install is profile-gated by the `execution/admission#CONTEXT` `emit_otel` policy row through a frozen `SignalProfile` table keyed by `RuntimeProfile` — a `SIDECAR`/`TOOL` profile installs the batched OTLP trio, a `PACKAGE`/`TEST` profile leaves the `opentelemetry-api` no-op providers in place and emits nothing. The siblings read the installed providers and own no construction: `observability/metrics#METRIC` reads the installed `MeterProvider` through `metrics.get_meter`, `observability/receipts#RECEIPT` reads the installed `LoggerProvider` and the trace context the propagator seeds, and the providers all carry the one `RUNTIME_RESOURCE` so the three signals join one `service.name`. The package installs the local signal pipeline only; the product telemetry envelope, sampler floor, and health stay AppHost-owned — the C# `diagnostics` one-`UseOtlpExporter`, one-`Resource`, one-propagator parity bar realized on the cp315 core.

## [1]-[INDEX]

- [1]-[TELEMETRY]: `Telemetry` composition-root install, `SignalProfile` emit-gated provider-trio table, `RUNTIME_RESOURCE`, the OTLP exporter trio, the `set_global_textmap` composite, the host-drain flush boundary.

## [2]-[TELEMETRY]

- Owner: `Telemetry` — the static composition-root surface owning one `Telemetry.install(profile, endpoint)` entrypoint that constructs and globally registers the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio over the one OTLP exporter family and installs the composite propagator, plus `Telemetry.shutdown` flushing the batch processors on the host drain; `SignalProfile` the frozen `Map[RuntimeProfile, SignalProfile]` table whose row carries the `emit_otel` gate and the per-profile batch geometry; `RUNTIME_RESOURCE` the one process/runtime-convention `Resource` all three signals join.
- Cases: the install dispatches by the `SignalProfile` row keyed off the admitted `RuntimeProfile` — an `emit` row (`SIDECAR`/`TOOL`, `emit_otel=True`) constructs the batched OTLP trio + composite propagator; a `silent` row (`PACKAGE`/`TEST`, `emit_otel=False`) installs nothing and the `opentelemetry-api` no-op providers stay, so a library/test import touches no exporter and opens no batch thread; the modality is the row value, never a per-signal toggle the caller re-derives.
- Entry: `Telemetry.install(profile, endpoint)` reads the `SignalProfile` row, returns early on `silent`, and on `emit` mints `RUNTIME_RESOURCE` once, constructs the three OTLP exporters against `(endpoint, headers, timeout, compression)`, wraps the span/log exporters in `BatchSpanProcessor`/`BatchLogRecordProcessor` and the metric exporter in a `PeriodicExportingMetricReader`, builds the three providers over the shared `Resource`, registers each through `trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider`, and installs the `TraceContextTextMapPropagator`+`W3CBaggagePropagator` composite through `propagate.set_global_textmap`; `Telemetry.shutdown` flushes the registered providers on host drain so the batch queues drain before exit.
- Auto: the providers are constructed once per process at the composition root and never imported by library code — the SDK import lives in this owner alone; `RUNTIME_RESOURCE` carries `service.name=rasm-companion` so `observability/metrics#METRIC` and `observability/receipts#RECEIPT` read one identity off the installed providers rather than minting a second; the install is idempotent through the `_installed` latch so a re-admission under the same process is a no-op; the `silent` profile installs zero exporters and opens zero batch threads, so the cost of OTel under a `PACKAGE`/`TEST` profile is exactly the API no-op providers `opentelemetry-api` already supplies; `propagate.set_global_textmap` populates the global composite `propagate.get_global_textmap` resolves, so the Wave-2 inbound `propagate.extract` and the Wave-3 `aio_client_interceptors` seed the C# parent rather than a fresh root.
- Packages: `opentelemetry-sdk` (`sdk.trace.TracerProvider`/`sdk.metrics.MeterProvider`/`sdk._logs.LoggerProvider`, `BatchSpanProcessor`/`BatchLogRecordProcessor`/`PeriodicExportingMetricReader`, `Resource.create`), `opentelemetry-exporter-otlp-proto-http` (`OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` over the shared `(endpoint, headers, timeout, compression)` constructor), `opentelemetry-api` (`trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider` + `propagate.set_global_textmap` installing the `TextMapPropagator` composite), `expression` (`Map`/`pipe` over the profile table), `msgspec` (the frozen `SignalProfile` row).
- Growth: a new signal modality is one provider construction inside `install`, never a second install surface; a new per-profile batch geometry is one column on `SignalProfile` plus one `SIGNAL_PROFILE.add` row, never a per-signal flag; a new propagator format is one entry in the `CompositePropagator` list passed to `set_global_textmap`, never a second propagator install; zero new entrypoint, one install gate.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, AppHost telemetry envelope, sampler-floor ownership, product signal export, or exporter ownership beyond the shared OTLP family; an SDK import outside `install`, a per-signal provider/exporter scatter, an eager install under a `silent` (`emit_otel=False`) profile, a re-minted `Resource` beside `RUNTIME_RESOURCE`, and a fresh-root propagator that drops the inbound composite are the deleted forms; the suite telemetry taxonomy and the cross-runtime sampler floor stay AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final

from expression.collections import Map
from msgspec import Struct
from opentelemetry import _logs, metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator
from opentelemetry.sdk._logs import LoggerProvider
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter

from rasm.runtime.admission import PROFILE_POLICY, RuntimeProfile

# --- [CONSTANTS] ------------------------------------------------------------------------

RUNTIME_RESOURCE: Final[Resource] = Resource.create({"service.name": "rasm-companion"})

EXPORT_TIMEOUT_S: Final[float] = 10.0

# --- [MODELS] ---------------------------------------------------------------------------


class SignalProfile(Struct, frozen=True):
    profile: RuntimeProfile
    emit_otel: bool
    export_interval_ms: int
    max_queue_size: int


SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq([
    (
        profile,
        SignalProfile(
            profile,
            emit_otel=PROFILE_POLICY[profile].emit_otel,
            export_interval_ms=2000 if profile is RuntimeProfile.SIDECAR else 5000,
            max_queue_size=2048 if profile is RuntimeProfile.SIDECAR else 512,
        ),
    )
    for profile in RuntimeProfile
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    _installed: bool = False
    _tracer: TracerProvider | None = None
    _meter: MeterProvider | None = None
    _logger: LoggerProvider | None = None

    @classmethod
    def install(cls, profile: RuntimeProfile, endpoint: str) -> None:
        signal = SIGNAL_PROFILE[profile]
        if cls._installed or not signal.emit_otel:
            return
        span_exporter = OTLPSpanExporter(endpoint=endpoint, timeout=EXPORT_TIMEOUT_S)
        metric_exporter = OTLPMetricExporter(endpoint=endpoint, timeout=EXPORT_TIMEOUT_S)
        log_exporter = OTLPLogExporter(endpoint=endpoint, timeout=EXPORT_TIMEOUT_S)
        tracer = TracerProvider(resource=RUNTIME_RESOURCE)
        tracer.add_span_processor(BatchSpanProcessor(span_exporter, max_queue_size=signal.max_queue_size))
        reader = PeriodicExportingMetricReader(metric_exporter, export_interval_millis=signal.export_interval_ms)
        meter = MeterProvider(metric_readers=[reader], resource=RUNTIME_RESOURCE)
        logger = LoggerProvider(resource=RUNTIME_RESOURCE)
        logger.add_log_record_processor(BatchLogRecordProcessor(log_exporter, max_queue_size=signal.max_queue_size))
        trace.set_tracer_provider(tracer)
        metrics.set_meter_provider(meter)
        _logs.set_logger_provider(logger)
        propagate.set_global_textmap(CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()]))
        cls._tracer, cls._meter, cls._logger, cls._installed = tracer, meter, logger, True

    @classmethod
    def shutdown(cls) -> None:
        for provider in (cls._tracer, cls._meter, cls._logger):
            if provider is not None:
                provider.shutdown()
        cls._tracer = cls._meter = cls._logger = None
        cls._installed = False
```

## [3]-[RESEARCH]

- [OTLP_PROVIDER_INSTALL]: reflection-confirmed — the `opentelemetry-sdk` provider trio (`sdk.trace.TracerProvider(resource, ...)` + `add_span_processor(BatchSpanProcessor(exporter, max_queue_size, ...))`, `sdk.metrics.MeterProvider(metric_readers=[PeriodicExportingMetricReader(exporter, export_interval_millis)], resource)`, `sdk._logs.LoggerProvider(resource)` + `add_log_record_processor(BatchLogRecordProcessor(exporter, max_queue_size))`) over the `opentelemetry-exporter-otlp-proto-http` exporter family (`OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` sharing the `(endpoint, headers, timeout, compression, session)` constructor) is the production install path; the providers register through the `opentelemetry-api` `trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider` global resolution points, replacing the no-op API providers only at the composition root. The composite `TextMapPropagator` is the `opentelemetry.propagators.composite.CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()])` (the W3C trace-context + baggage construction the API catalogues as the `TextMapPropagator` surface without separately indexing the concrete classes) installed through `propagate.set_global_textmap`, which `propagate.get_global_textmap` resolves for the inbound `observability/receipts#RECEIPT` extract and the outbound `transport/serve#CAPABILITY_INVOKE` client interceptors — the C# `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` `Propagators.DefaultTextMapPropagator` parity at the wire. The install gate reads the `execution/admission#CONTEXT` `PROFILE_POLICY` `emit_otel` row (`SIDECAR`/`TOOL` true → batched OTLP trio, `PACKAGE`/`TEST` false → API no-op providers, zero egress); `BatchSpanProcessor`/`BatchLogRecordProcessor` require an explicit `shutdown` flush on host drain, owned by `Telemetry.shutdown`. The provider-install spellings are settled. No open RESEARCH seam remains on this page.
