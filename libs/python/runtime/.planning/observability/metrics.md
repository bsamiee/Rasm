# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured-execution instrument set against the `MeterProvider` `observability/telemetry#TELEMETRY` installs at the composition root — it constructs no provider, reader, or exporter of its own, reading the installed meter through `metrics.get_meter` so all three signals carry the one shared `Resource`. The instruments read live `DrainReceipt` and `psutil.Process` state through observable callbacks — companion request duration as a histogram, lane drain counts folded from `DrainReceipt`, process RSS and CPU as gauges — so lane saturation, retry exhaustion, and companion latency surface as first-class metrics rather than log fields. The package mints the local metric stream only; the provider install, product telemetry export, and health stay `observability/telemetry#TELEMETRY` / AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the installed-meter read, the observable instrument set, the drain-counter and process-gauge callbacks, the request-duration histogram.

## [02]-[METRIC]

- Owner: `Metrics` — the static surface owning the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider`; `MetricState` the frozen carrier the observable callbacks read, holding the latest `DrainReceipt` fold and the `psutil.Process` handle so a callback never reconstructs live state under a lane lock. The provider, the `PeriodicExportingMetricReader`/`OTLPMetricExporter`, and the `RUNTIME_RESOURCE` are NOT owned here — they install once at `observability/telemetry#TELEMETRY` and `Metrics` reads the registered meter.
- Entry: `Metrics.install` obtains the installed meter through `metrics.get_meter` (the `observability/telemetry#TELEMETRY` `set_meter_provider` install having already run, gated on the `execution/admission#CONTEXT` `emit_otel` row so a non-emitting profile reads the API no-op meter) and creates the four instruments once — `companion.request.duration` a synchronous `Histogram` the serve leg records per call, `lane.drained` an `ObservableCounter` whose callback yields one `Observation` per `DrainReceipt` count column, `process.memory.rss` and `process.cpu.utilization` `ObservableGauge` instruments reading `psutil`; `Metrics.observe` folds a fresh `DrainReceipt` into the shared `MetricState` the observable counter callback reads, never holding the lane's task-group lock.
- Auto: the meter is obtained once per process through `metrics.get_meter` and the instruments are created once and reused — never per request, per `LOCAL_ADMISSION`; the `ObservableCounter`/`ObservableGauge` callbacks receive a `CallbackOptions` and return an iterable of `Observation`, reading the latest `MetricState` snapshot rather than blocking on the live lane; `process.cpu.utilization` reads `psutil.Process.cpu_percent(interval=None)` so the callback never blocks the export thread; the drain counter folds the four `DrainReceipt` columns into one instrument keyed by an `outcome` attribute (`accepted`/`completed`/`cancelled`/`rejected`) rather than four parallel counters; the installed `MeterProvider` carries the one shared `Resource` the trace/log providers join so all three signals carry one `service.name`.
- Packages: `opentelemetry-api` (`metrics.get_meter`/`Observation`/`CallbackOptions`/`Histogram.record`), `psutil` (`Process.memory_info`/`Process.cpu_percent`), `msgspec`. The SDK `MeterProvider`/`PeriodicExportingMetricReader`/`Resource.create` and the `OTLPMetricExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here.
- Growth: a new measured signal is one instrument created in `install`; a new drain dimension is one attribute value on the `lane.drained` counter, never a new counter; a new gauge is one `ObservableGauge` callback reading the shared `MetricState`; zero new surface, one installed `MeterProvider`.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter construction (the `observability/telemetry#TELEMETRY` install owns it), no AppHost telemetry envelope, health status, product metric export, or exporter ownership; an SDK import in this owner, a per-request instrument, a callback that blocks on the live lane lock, and four parallel drain counters where one attribute-keyed counter folds them are the deleted forms; the suite metric taxonomy stays AppHost-owned.

```python signature
from collections.abc import Iterable
from typing import Final

import psutil
from msgspec import Struct
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Histogram, Observation

from rasm.runtime.lanes import DrainReceipt

DRAIN_COLUMNS: Final[tuple[str, ...]] = ("accepted", "completed", "cancelled", "rejected")


class MetricState(Struct):
    drain: DrainReceipt
    process: psutil.Process


class Metrics:
    _state: MetricState | None = None
    _duration: Histogram | None = None

    @classmethod
    def install(cls) -> None:
        meter = metrics.get_meter("rasm.runtime")
        cls._state = MetricState(drain=DrainReceipt(0, 0, 0, 0), process=psutil.Process())
        cls._duration = meter.create_histogram("companion.request.duration", unit="ms")
        meter.create_observable_counter("lane.drained", callbacks=[cls._drained])
        meter.create_observable_gauge("process.memory.rss", callbacks=[cls._rss], unit="By")
        meter.create_observable_gauge("process.cpu.utilization", callbacks=[cls._cpu], unit="1")

    @classmethod
    def observe(cls, drain: DrainReceipt) -> None:
        if cls._state is not None:
            cls._state.drain = drain

    @classmethod
    def record(cls, duration_ms: float, method: str) -> None:
        if cls._duration is not None:
            cls._duration.record(duration_ms, {"rpc.method": method})

    @classmethod
    def _drained(cls, _: CallbackOptions) -> Iterable[Observation]:
        drain = cls._state.drain if cls._state is not None else DrainReceipt(0, 0, 0, 0)
        return [Observation(getattr(drain, column), {"outcome": column}) for column in DRAIN_COLUMNS]

    @classmethod
    def _rss(cls, _: CallbackOptions) -> Iterable[Observation]:
        return [Observation(cls._state.process.memory_info().rss)] if cls._state is not None else []

    @classmethod
    def _cpu(cls, _: CallbackOptions) -> Iterable[Observation]:
        return [Observation(cls._state.process.cpu_percent(interval=None))] if cls._state is not None else []
```
