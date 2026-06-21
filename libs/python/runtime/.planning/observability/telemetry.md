# [PY_RUNTIME_TELEMETRY]

The one composition-root install owner for the OTLP signal egress every other observability surface assumes already wired. `Telemetry` is the single static surface that mints the shared `Resource`, constructs the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio over the one `OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` family under `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`, registers all three providers globally through the `opentelemetry-api` `set_*_provider` surface, and installs the composite `TextMapPropagator` through `set_global_textmap` so the inbound extract (`observability/receipts#RECEIPT`) and the outbound client interceptors (`transport/serve#CAPABILITY_INVOKE`) resolve one cross-language trace, the whole composition gated behind a one-shot latch — the cached `InstallReceipt` slot — so a second install is a strict no-op returning the prior receipt. The `grpc.aio` serve leg's `SERVER` span emission is owned by `transport/serve#SERVE`, which wires `aio_server_interceptor` into its server at construction; `Telemetry` installs only the provider trio + composite propagator, and the serve interceptor's defaulted `tracer_provider` resolves the global `TracerProvider` this install registers — `Telemetry` never globally patches the serve leg. The install is profile-gated by the `execution/admission#CONTEXT` `emit_otel` policy row through a frozen `SignalProfile` table keyed by `RuntimeProfile` — a `SIDECAR`/`TOOL` profile installs the batched OTLP trio, a `PACKAGE`/`TEST` profile leaves the `opentelemetry-api` no-op providers in place and emits nothing. The siblings read the installed providers and own no construction: `observability/metrics#METRIC` reads the installed `MeterProvider` through `metrics.get_meter`, `observability/receipts#RECEIPT` reads the installed `LoggerProvider` and the trace context the propagator seeds, and the providers all carry the one `RUNTIME_RESOURCE` so the three signals join one `service.name`. The package installs the local signal pipeline only; the product telemetry envelope, sampler floor, and health stay AppHost-owned — the C# `diagnostics` one-`UseOtlpExporter`, one-`Resource`, one-propagator parity bar realized on the cp315 core.

## [01]-[INDEX]

- [01]-[TELEMETRY]: `Telemetry` composition-root install returning the typed `InstallReceipt`, the one-shot cached-receipt latch, the `SignalProfile` batch-geometry/compression table read under the `execution/admission#CONTEXT` `emit_otel` gate, `RUNTIME_RESOURCE`, the OTLP exporter trio over `(endpoint, headers, timeout, compression)`, the `set_global_textmap` composite, the host-drain total-flush boundary.

## [02]-[TELEMETRY]

- Owner: `Telemetry` — the static composition-root surface owning one `Telemetry.install(profile, endpoint, headers)` entrypoint that constructs and globally registers the `TracerProvider`/`MeterProvider`/`LoggerProvider` trio over the one OTLP exporter family, installs the composite propagator, and returns the typed `InstallReceipt`, all behind a one-shot latch — the cached `_receipt` slot — plus `Telemetry.shutdown` totally flushing the batch processors on the host drain; the `grpc.aio` serve-leg span emission is the `transport/serve#SERVE` construction-time `aio_server_interceptor`'s, never a global instrumentor here; `SignalProfile` the frozen `Map[RuntimeProfile, SignalProfile]` table whose row carries only the telemetry-local per-profile batch geometry and `Compression`, the emit gate staying the authoritative `execution/admission#CONTEXT` `PROFILE_POLICY[profile].emit_otel` cell rather than a mirrored column; `InstallReceipt` the frozen evidence the host drain and tests read for the resolved profile, the install outcome (`installed`/`silent`/`reentrant`), the endpoint, and the per-signal batch/compression geometry; `RUNTIME_RESOURCE` the one process/runtime-convention `Resource` all three signals join.
- Cases: the install dispatches by the `PROFILE_POLICY[profile].emit_otel` gate keyed off the admitted `RuntimeProfile` under the one-shot latch — a re-entrant `install` (`_receipt is not None`) returns the cached `reentrant`-equivalent receipt before reading the row; an `emit` gate (`SIDECAR`/`TOOL`, `emit_otel=True`) constructs the batched OTLP trio over `(endpoint, headers, timeout, compression)` + composite propagator and returns the `installed` receipt; a `silent` gate (`PACKAGE`/`TEST`, `emit_otel=False`) installs nothing, caches the `silent` receipt, and the `opentelemetry-api` no-op providers stay, so a library/test import touches no exporter and opens no batch thread; the modality is the admission policy cell, never a per-signal toggle the caller re-derives or a column mirrored onto `SignalProfile`.
- Entry: `Telemetry.install(profile, endpoint, headers)` short-circuits on the one-shot cached-receipt latch (`_receipt is not None`) before any side effect, so a second `install` under the same process — a re-admission, a re-entrant composition root, a test re-bootstrap — returns the prior `InstallReceipt` untouched and never re-mints a provider or re-opens a batch thread; it returns early on `silent` (caching the `silent` receipt so a later `emit` re-admission under the same process still no-ops), and on `emit` mints `RUNTIME_RESOURCE` once, constructs the three OTLP exporters against `(endpoint, headers, timeout, compression)` — `headers` carrying the secret-resolved bearer (`OTEL_EXPORTER_OTLP_HEADERS`), `compression` the `SignalProfile.compression` `Compression.GZIP` column — wraps the span/log exporters in `BatchSpanProcessor`/`BatchLogRecordProcessor` and the metric exporter in a `PeriodicExportingMetricReader`, builds the three providers over the shared `Resource`, registers each through `trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider`, installs the `TraceContextTextMapPropagator`+`W3CBaggagePropagator` composite through `propagate.set_global_textmap`, and caches the `installed` `InstallReceipt` as the single terminal write the host reads for install evidence; `Telemetry.shutdown` totally flushes the registered providers on host drain — each `shutdown()` outcome collected so a wedged exporter session never short-circuits the remaining flushes — so the batch queues drain before exit and the receipt latch always clears, re-arming a subsequent `install` cleanly. The serve leg's `SERVER` span emission is wired once at `transport/serve#SERVE` server construction and resolves the `TracerProvider` this install registers, so `Telemetry` activates no gRPC instrumentor and reverts none on drain.
- Auto: the providers are activated once per process at the composition root and never imported by library code — the SDK imports live in this owner alone; `endpoint` is the OTLP base root (the `execution/admission#SETTINGS` `otel_endpoint`, the `OTEL_EXPORTER_OTLP_ENDPOINT` base-URL contract), never a path-suffixed URL, so the three signals derive non-colliding `/v1/traces`, `/v1/metrics`, `/v1/logs` paths off the one base; `RUNTIME_RESOURCE` carries `service.name=rasm-companion` so `observability/metrics#METRIC` and `observability/receipts#RECEIPT` read one identity off the installed providers rather than minting a second; the install is idempotent through the one-shot cached-receipt latch read as the first statement and the `InstallReceipt` cached as the last, so a re-admission under the same process is a strict no-op that touches no provider and returns the prior receipt; the `silent` profile installs zero exporters and opens zero batch threads, so the cost of OTel under a `PACKAGE`/`TEST` profile is exactly the API no-op providers `opentelemetry-api` already supplies; `propagate.set_global_textmap` populates the global composite `propagate.get_global_textmap` resolves, so the Wave-2 inbound `propagate.extract` and the Wave-3 `aio_client_interceptors` seed the C# parent rather than a fresh root.
- Packages: `opentelemetry-sdk` (`sdk.trace.TracerProvider` construction ENTRYPOINTS [01] + `add_span_processor` [02] + `shutdown` [03] / `sdk.metrics.MeterProvider` ENTRYPOINTS [01] + `PeriodicExportingMetricReader` [03] / `sdk._logs.LoggerProvider` construction ENTRYPOINTS [01] + `add_log_record_processor` [02] + `shutdown` [03] + `BatchLogRecordProcessor` [04] / `BatchSpanProcessor` [04] / `Resource.create` [02]), `opentelemetry-exporter-otlp-proto-http` (`OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` over the shared `(endpoint, headers, timeout, compression)` constructor + `Compression` PUBLIC_TYPES [04] `NONE`/`GZIP`), `opentelemetry-api` (`trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider` + `propagate.set_global_textmap` installing the `TextMapPropagator` composite), `expression` (`Map` over the profile table), `msgspec` (the frozen `SignalProfile` row and the `InstallReceipt` install evidence).
- Growth: a new signal modality is one provider construction inside `install` plus one column on `InstallReceipt`, never a second install surface; a new per-profile batch geometry or transport knob is one column on `SignalProfile` (e.g. the `Compression` column) plus one `SIGNAL_PROFILE.add` value, never a per-signal flag; a new propagator format is one entry in the `CompositePropagator` list passed to `set_global_textmap`, never a second propagator install; zero new entrypoint, one install gate, one typed receipt.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, AppHost telemetry envelope, sampler-floor ownership, product signal export, exporter ownership beyond the shared OTLP family, or second install latch; an SDK import outside `install`, a per-signal provider/exporter scatter, a global `GrpcAioInstrumentorServer` activation that double-patches the `transport/serve#SERVE` leg already wired with `aio_server_interceptor` at construction (the global instrumentor is the fallback only for a server built outside runtime control, which this runtime never does — it would emit two overlapping SERVER spans per RPC and bypass the serve leg's `filters.negate(filters.health_check())`), an eager install under a `silent` (`emit_otel=False`) profile, a re-minted `Resource` beside `RUNTIME_RESOURCE`, and a fresh-root propagator that drops the inbound composite are the deleted forms; the serve-leg server-span emission stays `transport/serve#SERVE`-owned, and the suite telemetry taxonomy and the cross-runtime sampler floor stay AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping
from typing import Final, Literal

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
from opentelemetry.exporter.otlp.proto.http import Compression
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
    export_interval_ms: int
    max_queue_size: int
    compression: Compression


SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq([
    (
        profile,
        SignalProfile(
            profile,
            export_interval_ms=2000 if profile is RuntimeProfile.SIDECAR else 5000,
            max_queue_size=2048 if profile is RuntimeProfile.SIDECAR else 512,
            compression=Compression.GZIP,
        ),
    )
    for profile in RuntimeProfile
])


class InstallReceipt(Struct, frozen=True):
    profile: RuntimeProfile
    outcome: Literal["installed", "silent", "reentrant"]
    endpoint: str
    export_interval_ms: int
    max_queue_size: int
    compression: Compression

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    _receipt: InstallReceipt | None = None
    _tracer: TracerProvider | None = None
    _meter: MeterProvider | None = None
    _logger: LoggerProvider | None = None

    @classmethod
    def install(
        cls, profile: RuntimeProfile, endpoint: str, headers: Mapping[str, str] | None = None
    ) -> InstallReceipt:
        if cls._receipt is not None:
            return cls._receipt
        signal = SIGNAL_PROFILE[profile]
        if not PROFILE_POLICY[profile].emit_otel:
            cls._receipt = InstallReceipt(
                profile, "silent", endpoint, signal.export_interval_ms, signal.max_queue_size, signal.compression
            )
            return cls._receipt
        span_exporter = OTLPSpanExporter(
            endpoint=endpoint, headers=headers, timeout=EXPORT_TIMEOUT_S, compression=signal.compression
        )
        metric_exporter = OTLPMetricExporter(
            endpoint=endpoint, headers=headers, timeout=EXPORT_TIMEOUT_S, compression=signal.compression
        )
        log_exporter = OTLPLogExporter(
            endpoint=endpoint, headers=headers, timeout=EXPORT_TIMEOUT_S, compression=signal.compression
        )
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
        cls._tracer, cls._meter, cls._logger = tracer, meter, logger
        cls._receipt = InstallReceipt(
            profile, "installed", endpoint, signal.export_interval_ms, signal.max_queue_size, signal.compression
        )
        return cls._receipt

    @classmethod
    def shutdown(cls) -> None:
        for provider in (cls._tracer, cls._meter, cls._logger):
            match provider:
                case None:
                    continue
                case _:
                    try:
                        provider.shutdown()
                    except Exception:
                        continue
        cls._tracer = cls._meter = cls._logger = cls._receipt = None
```

## [03]-[RESEARCH]

- [OTLP_PROVIDER_INSTALL]: reflection-confirmed against `.api/opentelemetry-sdk.md` — the provider trio `sdk.trace.TracerProvider(resource, ...)` (TracerProvider ENTRYPOINTS [01]) + `add_span_processor(BatchSpanProcessor(exporter, max_queue_size, ...))` ([02] + [04]), `sdk.metrics.MeterProvider(metric_readers=[PeriodicExportingMetricReader(exporter, export_interval_millis)], resource)` (MeterProvider ENTRYPOINTS [01] + [03]), and `sdk._logs.LoggerProvider(resource)` + `add_log_record_processor(BatchLogRecordProcessor(exporter, max_queue_size))` (LoggerProvider ENTRYPOINTS [01] + [02] + [04], `LoggerProvider.shutdown` [03]) over the `opentelemetry-exporter-otlp-proto-http` exporter family (`OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` sharing the `(endpoint, headers, timeout, compression, session)` constructor) is the production install path; the providers register through the `opentelemetry-api` `trace.set_tracer_provider`/`metrics.set_meter_provider`/`_logs.set_logger_provider` global resolution points, replacing the no-op API providers only at the composition root. The composite `TextMapPropagator` is the `opentelemetry.propagators.composite.CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()])` (the W3C trace-context + baggage construction the API catalogues as the `TextMapPropagator` surface without separately indexing the concrete classes) installed through `propagate.set_global_textmap`, which `propagate.get_global_textmap` resolves for the inbound `observability/receipts#RECEIPT` extract and the outbound `transport/serve#CAPABILITY_INVOKE` client interceptors — the C# `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` `Propagators.DefaultTextMapPropagator` parity at the wire. The install gate reads the `execution/admission#CONTEXT` `PROFILE_POLICY` `emit_otel` row (`SIDECAR`/`TOOL` true → batched OTLP trio, `PACKAGE`/`TEST` false → API no-op providers, zero egress); `BatchSpanProcessor`/`BatchLogRecordProcessor` require an explicit `shutdown` flush on host drain, owned by `Telemetry.shutdown`. The `grpc.aio` serve-leg `SERVER` span emission is NOT installed here: per `.api/opentelemetry-instrumentation-grpc.md` IMPLEMENTATION_LAW, explicit `aio_server_interceptor` wiring at `grpc.aio.server(interceptors=[...])` construction is used INSTEAD OF the global `GrpcAioInstrumentorServer` monkeypatch (the global instrumentor is the fallback only when the server is built outside runtime control); the runtime controls its server in `transport/serve#SERVE`, so a global `instrument()` here would double-patch the leg — two overlapping SERVER spans per RPC and an unfiltered global path that re-admits the health-check noise the serve interceptor's `filters.negate(filters.health_check())` suppresses — and is the deleted form. The serve interceptor's defaulted `tracer_provider` resolves the global `TracerProvider` this install registers, so one composition root mints all spans. The one-shot install is the cached `InstallReceipt` slot read as the first statement of `install` (`_receipt is not None`) and written as the single terminal assignment after every side effect, with the `silent` profile caching its own `silent` receipt so a later `emit` re-admission under the same process stays a strict no-op; this guards against re-minting providers or re-opening batch threads on a re-entrant composition root or test re-bootstrap, and the returned `InstallReceipt` gives the host drain and tests typed evidence of the resolved profile, install outcome, endpoint, and per-signal batch/compression geometry rather than inferring it from a bare latch. The emit gate is the authoritative `execution/admission#CONTEXT` `PROFILE_POLICY[profile].emit_otel` cell read directly at install — `SignalProfile` carries only the telemetry-local batch/`Compression` columns and mirrors no admission boolean. `headers` threads the secret-resolved bearer (`OTEL_EXPORTER_OTLP_HEADERS`) and `compression` the `Compression.GZIP` column into all three exporter constructors, so the authenticated/compressed egress path the shared constructor admits is bound, not dropped. The provider-install and idempotency spellings are settled. No open RESEARCH seam remains on this page.
