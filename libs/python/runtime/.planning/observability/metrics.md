# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured-execution instrument set against a single `MeterProvider` carrying the process/runtime semantic-conventions `Resource`, exported over the same OTLP/HTTP exporter the `observability/receipts#RECEIPT` log and trace egress use. The instruments read live `DrainReceipt` and `psutil.Process` state through observable callbacks — companion request duration as a histogram, lane drain counts folded from `DrainReceipt`, process RSS and CPU as gauges — so lane saturation, retry exhaustion, and companion latency surface as first-class metrics rather than log fields. The package mints the local metric stream only; product telemetry export and health stay AppHost-owned.

## [1]-[INDEX]

One cluster: `[2]-[METRIC]` — the meter provider, the observable instrument set, the drain-counter and process-gauge callbacks, the request-duration histogram.

## [2]-[METRIC]

- Owner: `Metrics` — the static surface owning one `MeterProvider` over a `PeriodicExportingMetricReader`/`OTLPMetricExporter`, the `RUNTIME_RESOURCE` process/runtime-convention `Resource`, and the instrument set; `MetricState` the frozen carrier the observable callbacks read, holding the latest `DrainReceipt` fold and the `psutil.Process` handle so a callback never reconstructs live state under a lane lock.
- Entry: `Metrics.install` constructs the `MeterProvider` against the shared `OTLPMetricExporter`, registers it globally through `metrics.set_meter_provider`, and creates the four instruments once — `companion.request.duration` a synchronous `Histogram` the serve leg records per call, `lane.drained` an `ObservableCounter` whose callback yields one `Observation` per `DrainReceipt` count column, `process.memory.rss` and `process.cpu.utilization` `ObservableGauge` instruments reading `psutil`; `Metrics.observe` folds a fresh `DrainReceipt` into the shared `MetricState` the observable counter callback reads, never holding the lane's task-group lock.
- Auto: the meter is obtained once per process through `metrics.get_meter` and the instruments are created once and reused — never per request, per `LOCAL_ADMISSION`; the `ObservableCounter`/`ObservableGauge` callbacks receive a `CallbackOptions` and return an iterable of `Observation`, reading the latest `MetricState` snapshot rather than blocking on the live lane; `process.cpu.utilization` reads `psutil.Process.cpu_percent(interval=None)` so the callback never blocks the export thread; the drain counter folds the four `DrainReceipt` columns into one instrument keyed by an `outcome` attribute (`accepted`/`completed`/`cancelled`/`rejected`) rather than four parallel counters; the `MeterProvider` shares the `RUNTIME_RESOURCE` with the trace/log providers so all three signals carry one `service.name`.
- Packages: `opentelemetry-api` (`metrics.get_meter`/`metrics.set_meter_provider`/`Observation`/`CallbackOptions`/`Histogram.record`), `opentelemetry-sdk` (`MeterProvider`/`PeriodicExportingMetricReader`/`Resource.create`), `opentelemetry-exporter-otlp-proto-http` (`OTLPMetricExporter`), `psutil` (`Process.memory_info`/`Process.cpu_percent`), `msgspec`.
- Growth: a new measured signal is one instrument created in `install`; a new drain dimension is one attribute value on the `lane.drained` counter, never a new counter; a new gauge is one `ObservableGauge` callback reading the shared `MetricState`; zero new surface, one `MeterProvider`.
- Boundary: no second `MeterProvider`, AppHost telemetry envelope, health status, product metric export, or exporter ownership beyond the shared OTLP exporter; an SDK import outside `install` (the composition-root admission), a per-request instrument, a callback that blocks on the live lane lock, and four parallel drain counters where one attribute-keyed counter folds them are the deleted forms; the suite metric taxonomy stays AppHost-owned.

```python signature
from collections.abc import Iterable
from typing import Final

import psutil
from msgspec import Struct
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Histogram, Observation
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.resources import Resource
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter

from rasm.runtime.lanes import DrainReceipt

RUNTIME_RESOURCE: Final[Resource] = Resource.create({"service.name": "rasm-companion"})

DRAIN_COLUMNS: Final[tuple[str, ...]] = ("accepted", "completed", "cancelled", "rejected")


class MetricState(Struct):
    drain: DrainReceipt
    process: psutil.Process


class Metrics:
    _state: MetricState | None = None
    _duration: Histogram | None = None

    @classmethod
    def install(cls, endpoint: str) -> None:
        reader = PeriodicExportingMetricReader(OTLPMetricExporter(endpoint=endpoint))
        provider = MeterProvider(metric_readers=[reader], resource=RUNTIME_RESOURCE)
        metrics.set_meter_provider(provider)
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
