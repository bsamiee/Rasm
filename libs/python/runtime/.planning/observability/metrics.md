# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured-execution instrument set against the `MeterProvider` `observability/telemetry#TELEMETRY` installs at the composition root — it constructs no provider, reader, or exporter of its own, reading the installed meter through `metrics.get_meter` so all signals carry the one shared `Resource`. The observable instruments read a frozen `MetricState` snapshot swapped under an atomic reference assignment, so the export thread reads the latest drain fold and `psutil.Process` handle with no lock on the read path and the serve leg never blocks the lane to publish — companion request duration is a synchronous `Histogram`, lane drain counts an `ObservableCounter` folded from one `DrainReceipt` over the `DRAIN_COLUMNS` outcome axis, process RSS and CPU utilization `ObservableGauge` instruments reading the snapshot's `psutil.Process`. Lane saturation, retry exhaustion, and companion latency surface as first-class metrics rather than log fields, and the registered instrument set is named once in a typed `MeterReceipt` the measured-signal consumers read. The package mints the local metric stream only; the provider install, product telemetry export, and health stay `observability/telemetry#TELEMETRY` / AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the installed-meter read, the atomic-swap `MetricState` snapshot, the observable instrument set, the drain-counter and process-gauge callbacks, the request-duration histogram, the `MeterReceipt` instrument evidence.

## [02]-[METRIC]

- Owner: `Metrics` — the static surface owning the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider`; `MetricState` the frozen carrier the observable callbacks read, holding the latest `DrainReceipt` fold and the `psutil.Process` handle so a callback never reconstructs live state under a lane lock; `MeterReceipt` the typed instrument-registration evidence the measured-signal consumers read. The provider, the `PeriodicExportingMetricReader`/`OTLPMetricExporter`, and the `RUNTIME_RESOURCE` are NOT owned here — they install once at `observability/telemetry#TELEMETRY` and `Metrics` reads the registered meter.
- State: `MetricState` is a frozen snapshot, never mutated in place and never `None` — the class body seeds a zero-drain snapshot over `psutil.Process()` and a no-op `companion.request.duration` `Histogram` off the API no-op meter at declaration, so every callback and the `record`/`observe` path reads a fully-formed struct with no `is not None` guard; `install` re-seeds both against the installed meter. `Metrics.observe` builds a fresh snapshot folding the new `DrainReceipt` against the current `psutil.Process` handle and publishes it through one atomic `cls._state` reference assignment, the single bytecode store CPython guarantees atomic so the observable callbacks read a fully-formed snapshot with no lock and the serve leg's `observe` never contends the lane's task-group lock; the read path is `cls._state` then field reads off the immutable struct, so a swap that lands mid-export only changes which complete snapshot the next column reads, never tears a partial one. The `psutil.Process` handle is minted at `install` and carried through every swap so the process identity is stable across the run, the drain fold the only field that turns over.
- Entry: `Metrics.install` obtains the installed meter through `metrics.get_meter` (the `observability/telemetry#TELEMETRY` `set_meter_provider` install having already run, gated on the `execution/admission#CONTEXT` `emit_otel` row so a non-emitting profile reads the API no-op meter) and creates the instrument set once — `companion.request.duration` a synchronous `Histogram` the `transport/serve#SERVE` leg records per call, threading the per-call `DrainOutcome` `outcome` so the histogram attribute is populated rather than defaulted, `lane.drained` an `ObservableCounter` whose callback yields one `Observation` per `DRAIN_COLUMNS` outcome column off the snapshot's `DrainReceipt`, `process.memory.rss` and `process.cpu.utilization` `ObservableGauge` instruments reading the snapshot's `psutil.Process` — seeding `cls._state` with the zero-drain snapshot and returning the `MeterReceipt` naming the registered instruments — a bare return the composition root threads to the measured-signal consumers, not a receipt fact minted into `observability/receipts#RECEIPT`, the install evidence staying a typed value the root carries rather than an emitted log line; `Metrics.observe` swaps a fresh `DrainReceipt`-folded snapshot in one atomic reference assignment, never holding the lane's task-group lock; `Metrics.record` records one duration measurement keyed by the open `rpc.method` string and the bounded `DrainOutcome` `outcome` attribute, the histogram outcome axis sharing the one `execution/lanes#LANE`-owned `DrainOutcome` literal `DRAIN_COLUMNS` derives from so the histogram attribute and the counter attribute cannot drift.
- Auto: the meter is obtained once per process through `metrics.get_meter` and the instruments are created once and reused — never per request, per `LOCAL_ADMISSION`; the `ObservableCounter`/`ObservableGauge` callbacks receive a `CallbackOptions` and return an iterable of `Observation`, reading the latest frozen `MetricState` off one reference load rather than blocking on the live lane; `process.cpu.utilization` reads `psutil.Process.cpu_percent(interval=None)` so the callback never blocks the export thread (the first export after install reads the `0.0` seed the psutil contract returns on first sample, subsequent exports the true delta over the export interval); the drain counter folds the five `DrainReceipt` columns into one instrument keyed by a bounded `DrainOutcome` `outcome` attribute (`accepted`/`completed`/`cancelled`/`rejected`/`hit`, cache-`hit` a first-class counter dimension) rather than five parallel counters, the `DrainOutcome`/`DRAIN_COLUMNS` taxonomy imported from the `execution/lanes#LANE` `DrainReceipt` owner rather than redeclared here so the counter outcome vocabulary is the one typed fact the receipt also reads, never a hand-maintained parallel tuple, the fold projecting the typed `DrainReceipt` through `msgspec.structs.asdict` and indexing by the imported `DRAIN_COLUMNS` order so the five columns are a typed struct fold rather than a string-keyed `getattr`; the gauge callbacks read the `psutil.Process` through a guarded snapshot read returning `[]` when the handle resolves no live process (`NoSuchProcess`/`AccessDenied`/`ZombieProcess`) so a dead-process race drops the sample rather than raising on the export thread, the one boundary read this owner admits per the psutil `LOCAL_ADMISSION` — the raw `try/except PROCESS_FAULTS` is the admitted boundary-kernel exception to the `reliability/faults#FAULT` `boundary` rail, the OTel observable-callback contract returning `Iterable[Observation]` forbidding a railed `Result`, so the kernel drops to `[]` rather than crossing a fault as the `boundary` case; the installed `MeterProvider` carries the one shared `Resource` the trace/log providers join so all signals carry one `service.name`; `install` under a no-op meter (a `silent` `observability/telemetry#TELEMETRY` profile) creates API no-op instruments and the callbacks register but never fire, so the cost under a `PACKAGE`/`TEST` profile is exactly the no-op meter.
- Packages: `opentelemetry-api` (`metrics.get_meter` ENTRYPOINTS [3] / `Meter.create_histogram` [6] / `create_observable_counter` [7] / `create_observable_gauge` [8] / `Histogram.record` [10], `Observation` PUBLIC_TYPES [9] / `CallbackOptions` [10] / `Histogram` [05] / `ObservableCounter` [06] / `ObservableGauge` [07]), `psutil` (`Process` PUBLIC_TYPES [01] / `Process.memory_info` process-metrics ENTRYPOINTS [01] / `Process.cpu_percent` process-metrics ENTRYPOINTS [02], guarding `NoSuchProcess`/`AccessDenied`/`ZombieProcess` exception types [02]/[04]/[03]), `msgspec` (the frozen `MetricState` snapshot and `MeterReceipt`). The SDK `MeterProvider`/`PeriodicExportingMetricReader`/`Resource.create` and the `OTLPMetricExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here.
- Growth: a new measured signal is one instrument created in `install` plus one name on `MeterReceipt`; a new drain dimension is one member added on the `execution/lanes#LANE` `DrainOutcome` taxonomy, reaching this counter through the imported `DRAIN_COLUMNS` fold off the existing `DrainReceipt` with no edit here, never a new counter; a new gauge is one `ObservableGauge` callback reading the shared snapshot; a new histogram attribute is one key on the `record` attribute map; zero new surface, one installed `MeterProvider`, one atomic state slot.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter construction (the `observability/telemetry#TELEMETRY` install owns it), no AppHost telemetry envelope, health status, product metric export, or exporter ownership; the gauge callbacks' raw `try/except PROCESS_FAULTS` is the one admitted boundary-kernel exception to the `reliability/faults#FAULT` `boundary` rail — the OTel observable-callback contract forbids a railed return, so the kernel drops to `[]` rather than minting the `boundary` case, the only raw-except site this owner declares; an SDK import in this owner, a per-request instrument, a mutated-in-place `MetricState` a callback reads under a torn write, a callback that blocks on the live lane lock, a lock on the observe/read path where the atomic reference swap suffices, an unguarded `psutil` read raising on the export thread, and five parallel drain counters where one attribute-keyed counter folds them are the deleted forms; the suite metric taxonomy stays AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from typing import Final

import msgspec
import psutil
from msgspec import Struct
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Histogram, Observation

from rasm.runtime.lanes import DRAIN_COLUMNS, DrainOutcome, DrainReceipt

# --- [CONSTANTS] ------------------------------------------------------------------------

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (
    psutil.NoSuchProcess,
    psutil.ZombieProcess,
    psutil.AccessDenied,
)

# --- [MODELS] ---------------------------------------------------------------------------


class MetricState(Struct, frozen=True):
    drain: DrainReceipt
    process: psutil.Process


class MeterReceipt(Struct, frozen=True):
    histogram: str
    counter: str
    gauges: tuple[str, ...]
    outcomes: tuple[DrainOutcome, ...]

# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    _state: MetricState = MetricState(drain=DrainReceipt(0, 0, 0, 0), process=psutil.Process())
    _duration: Histogram = metrics.get_meter("rasm.runtime").create_histogram("companion.request.duration", unit="ms")

    @classmethod
    def install(cls) -> MeterReceipt:
        meter = metrics.get_meter("rasm.runtime")
        cls._state = MetricState(drain=DrainReceipt(0, 0, 0, 0), process=psutil.Process())
        cls._duration = meter.create_histogram("companion.request.duration", unit="ms")
        meter.create_observable_counter("lane.drained", callbacks=[cls._drained])
        meter.create_observable_gauge("process.memory.rss", callbacks=[cls._rss], unit="By")
        meter.create_observable_gauge("process.cpu.utilization", callbacks=[cls._cpu], unit="1")
        return MeterReceipt(
            histogram="companion.request.duration",
            counter="lane.drained",
            gauges=("process.memory.rss", "process.cpu.utilization"),
            outcomes=DRAIN_COLUMNS,
        )

    @classmethod
    def observe(cls, drain: DrainReceipt) -> None:
        cls._state = MetricState(drain=drain, process=cls._state.process)

    @classmethod
    def record(cls, duration_ms: float, method: str, outcome: DrainOutcome) -> None:
        cls._duration.record(duration_ms, {"rpc.method": method, "outcome": outcome})

    @classmethod
    def _drained(cls, _: CallbackOptions) -> Iterable[Observation]:
        columns = msgspec.structs.asdict(cls._state.drain)
        return [Observation(columns[column], {"outcome": column}) for column in DRAIN_COLUMNS]

    @classmethod
    def _rss(cls, _: CallbackOptions) -> Iterable[Observation]:
        try:
            return [Observation(cls._state.process.memory_info().rss)]
        except PROCESS_FAULTS:
            return []

    @classmethod
    def _cpu(cls, _: CallbackOptions) -> Iterable[Observation]:
        try:
            return [Observation(cls._state.process.cpu_percent(interval=None))]
        except PROCESS_FAULTS:
            return []
```

## [03]-[RESEARCH]

- [METRIC_INSTRUMENT_SET]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `metrics.get_meter(name, ...)` (ENTRYPOINTS [3]) reads the `observability/telemetry#TELEMETRY`-installed `MeterProvider` (no-op until `set_meter_provider`, so a `silent` profile yields no-op instruments), `Meter.create_histogram(name, unit, ...)` (ENTRYPOINTS [6]) mints the synchronous `companion.request.duration` recorder the serve leg drives through `Histogram.record(amount, attributes)` (ENTRYPOINTS [10]), and `create_observable_counter`/`create_observable_gauge(name, callbacks)` (ENTRYPOINTS [7]/[8]) register callbacks that the `PeriodicExportingMetricReader` invokes per export cycle with a `CallbackOptions` (PUBLIC_TYPES [10]) and that return `Iterable[Observation]` (PUBLIC_TYPES [9]) — `Observation(value, attributes)` carrying the `outcome` attribute on the drain counter so one instrument folds the five `DRAIN_COLUMNS` rather than five counters. Instruments are created once per meter and reused per the `LOCAL_ADMISSION`; the install reads the `MeterProvider` the sibling owner installs over the one `RUNTIME_RESOURCE`. The instrument-set spellings are settled.
- [ATOMIC_STATE_SWAP]: the observable callbacks run on the SDK export thread the `PeriodicExportingMetricReader` drives while the serve leg's `Metrics.observe` runs on the lane's event loop — the cross-thread read demands the snapshot be published atomically, not mutated field-by-field. The frozen `MetricState` (`msgspec.Struct(frozen=True)`) is built complete then published through a single `cls._state = ...` class-attribute store, the one bytecode `STORE_NAME`/`STORE_ATTR` CPython executes indivisibly under the GIL, so a callback's `state = cls._state` load reads either the prior or the next complete snapshot and never a half-written one; no lock guards either the swap or the read, the immutability the correctness guarantee rather than a mutex. The `psutil.Process` handle is invariant across swaps, only the `DrainReceipt` field turning over, so the gauge callbacks read a stable process identity while the counter reads the freshest fold. The atomic-swap shape is settled.
- [PROCESS_GAUGE_RACE]: confirmed against `libs/python/.api/psutil.md` `LOCAL_ADMISSION` — `Process.memory_info().rss` (process-metrics ENTRYPOINTS [01]) and `Process.cpu_percent(interval=None)` (process-metrics ENTRYPOINTS [02]) read the handle minted at `install`, and the psutil contract returns `0.0` on the first `cpu_percent(interval=None)` sample with the true utilization delta on subsequent calls over the export interval, so the periodic reader's cadence supplies the timing window without the callback ever blocking. The gauge reads are guarded against `NoSuchProcess`/`ZombieProcess`/`AccessDenied` (PUBLIC_TYPES [02]/[03]/[04]) so a dead-process race between handle mint and export drops the sample (returns `[]`) rather than raising on the export thread — the one boundary read this owner admits, the guard the psutil `LOCAL_ADMISSION` requires. The process-gauge spellings are settled. No open RESEARCH seam remains on this page.
```

