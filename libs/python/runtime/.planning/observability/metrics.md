# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured-execution instrument set against the `MeterProvider` `observability/telemetry#TELEMETRY` installs at the composition root — it constructs no provider, reader, or exporter of its own, reading the installed meter through `metrics.get_meter` so all signals carry the one shared `Resource`. The observable instrument set is a data-driven `INSTRUMENTS` spec table — one `InstrumentSpec` row per signal carrying its name, `counter`/`up_down`/`gauge` kind, unit, and a `Project` closure off the shared `MetricState` — that `install` folds through one register-keyed `create_observable_*` dispatch rather than a per-instrument creation call plus a sibling callback method per signal; one polymorphic `_callback` closes each spec over the frozen snapshot, so the drain fold, the in-flight occupancy gauge, and every process probe are one callback shape over one state read, never three near-identical classmethods. The instruments read a frozen `MetricState` swapped under an atomic reference assignment, so the export thread reads the latest drain fold, the live lane occupancy, and the one batched `ProcessReading` with no lock on the read path and the serve leg never blocks the lane to publish — companion request duration is a synchronous `Histogram` shaped by an `explicit_bucket_boundaries_advisory` and recorded under the active OTel `Context` so the SDK exemplar filter ties each measurement to its span, lane drain counts an `ObservableCounter` folded from one `DrainReceipt` over the `DRAIN_COLUMNS` outcome axis, lane saturation rides an `ObservableUpDownCounter` reading the live `CapacityLimiter` occupancy the `execution/lanes#LANE` `drained` aspect threads into the swap (the additive in-flight gauge the OTel `LOCAL_ADMISSION` names for active-lane/queue-depth occupancy, never a monotonic counter), and the process probes (RSS, USS, CPU utilization, thread count, fd count) are `ObservableGauge` rows over one `psutil.Process.oneshot()` collection. Lane saturation, retry exhaustion, and companion latency surface as first-class metrics rather than log fields: retry attempts ride a `retry.attempts` `Counter` fed by the metrics-owned `retry_hook` the `reliability/resilience#RESILIENCE` owner composes into its one `set_on_retry_hooks` registration, and the `measured` aspect times a serve coroutine and projects its `BoundaryFault.tag` through the `FAULT_OUTCOME` table onto the shared `DrainOutcome` axis — a deadline-tripped rail surfacing as `cancelled`, every other fault as `rejected`, an `Ok` rail as `completed` — at one site rather than inline timing or a lossy ok/error bool per handler. The registered instrument set is named once in a typed `MeterReceipt` the measured-signal consumers read. The package mints the local metric stream only; the provider install, the on-retry hook registration, product telemetry export, and health stay `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE` / AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the installed-meter read, the atomic-swap `MetricState` snapshot, the data-driven `INSTRUMENTS` spec table folded through one register-keyed `create_observable_*` dispatch over the `counter`/`up_down`/`gauge` kinds, the one polymorphic `_callback` over the drain fold, the live-occupancy in-flight gauge, and the process probes, the `ProcessReading.oneshot` batch, the bucket-shaped exemplar-recorded request-duration histogram, the `retry.attempts` counter and its composed `retry_hook`, the `measured` serve aspect projecting the rail `BoundaryFault.tag` through the `FAULT_OUTCOME` table onto the `DrainOutcome` axis, the `MeterReceipt` instrument evidence.

## [02]-[METRIC]

- Owner: `Metrics` — the static surface owning the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider`; `InstrumentSpec` the one observable-instrument row (name, `counter`/`up_down`/`gauge` kind, unit, `Project` closure) and `INSTRUMENTS` the spec table `install` folds; `MetricState` the frozen carrier the observable callbacks read, holding the latest `DrainReceipt` fold, the live `in_flight` lane occupancy, the `psutil.Process` handle, and the one batched `ProcessReading` so a callback never reconstructs live state or fires a syscall under a lane lock; `FAULT_OUTCOME` the `FaultTag`-keyed projection table the `measured` aspect folds a rail's `BoundaryFault.tag` through onto the shared `DrainOutcome` axis so the duration histogram outcome dimension is the one `execution/lanes#LANE` taxonomy and never a lossy ok/error bool; `ProcessReading` the value object folding one `Process.oneshot()` collection into a typed RSS/USS/CPU/thread/fd reading; `MeterReceipt` the typed instrument-registration evidence the measured-signal consumers read. The provider, the `PeriodicExportingMetricReader`/`OTLPMetricExporter`, the `View`/exemplar machinery, the on-retry hook registration, and the `RUNTIME_RESOURCE` are NOT owned here — they install once at `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE`, and `Metrics` reads the registered meter and contributes a hook.
- State: `MetricState` is a frozen `gc=False` snapshot, never mutated in place and never partially read — the class body seeds a zero-drain snapshot over `psutil.Process()` with a `None` reading and a bucket-shaped no-op `companion.request.duration` `Histogram` plus a no-op `retry.attempts` `Counter` off the API no-op meter at declaration, so every callback and the `record`/`observe` path reads a fully-formed struct with no live reconstruction; `install` re-seeds the meter, instruments, and an initial `ProcessReading.sample` against the installed meter. `Metrics.observe` builds a fresh snapshot folding the new `DrainReceipt`, the live lane `in_flight` occupancy, and a fresh `ProcessReading.sample` against the carried `psutil.Process` handle and publishes it through one atomic `cls._state` reference assignment, the single bytecode store CPython guarantees atomic so the observable callbacks read a fully-formed snapshot with no lock and the serve leg's `observe` never contends the lane's task-group lock; the read path is `cls._state` then field reads off the immutable struct, so a swap that lands mid-export only changes which complete snapshot the next column reads, never tears a partial one. The `psutil.Process` handle is minted at `install` and carried through every swap so the process identity is stable across the run; the drain fold, the in-flight occupancy, and the batched reading are the fields that turn over, the reading sampled once per `observe` so the gauge callbacks read a cached fold rather than each firing its own syscall on the export thread.
- Entry: `Metrics.install` obtains the installed meter through `metrics.get_meter` (the `observability/telemetry#TELEMETRY` `set_meter_provider` install having already run, gated on the `execution/admission#CONTEXT` `emit_otel` row so a non-emitting profile reads the API no-op meter), creates `companion.request.duration` a synchronous `Histogram` shaped by `explicit_bucket_boundaries_advisory=DURATION_BUCKETS_MS` and `retry.attempts` a `Counter`, then folds `INSTRUMENTS` through one `register: dict[ObservableKind, Register]` dispatch so each row's `create_observable_counter`/`create_observable_up_down_counter`/`create_observable_gauge` is keyed by `spec.kind` rather than a per-instrument call — seeding `cls._state` with the zero-drain snapshot and an initial `ProcessReading.sample` and returning the `MeterReceipt` naming the histogram, drain counter, in-flight up-down counter, retry counter, gauge set, and `DRAIN_COLUMNS` — a bare return the composition root threads to the measured-signal consumers, not a receipt fact minted into `observability/receipts#RECEIPT`; `Metrics.observe` swaps a fresh `DrainReceipt`+`in_flight`+`ProcessReading`-folded snapshot in one atomic reference assignment, never holding the lane's task-group lock; `Metrics.record` records one duration measurement keyed by the open `rpc.method` string and the bounded `DrainOutcome` `outcome` attribute under `context=otel_context.get_current()` so the SDK exemplar filter ties the measurement to the active serve span, the histogram outcome axis sharing the one `execution/lanes#LANE`-owned `DrainOutcome` literal `DRAIN_COLUMNS` derives from so the histogram attribute and the counter attribute cannot drift; `Metrics.measured(method)` is the AOP aspect wrapping a `Served[T]` serve coroutine — it times the call with `perf_counter`, projects the resolved rail's `BoundaryFault.tag` through the `FAULT_OUTCOME` table off `rail.swap().to_option().map(cls._outcome)` (a `deadline` fault landing as `cancelled`, every other fault defaulting to `rejected`, an `Ok` rail to `completed`), records the duration, and returns the rail untouched, so the `transport/serve#SERVE` leg decorates a handler once rather than threading inline timing through every method and the histogram outcome stays total over the closed `FaultTag` rather than a two-valued ok/error collapse that hides a deadline trip; `Metrics.retry_hook` returns a `RetryHook` incrementing `retry.attempts` keyed by `RetryDetails.name` and `caused_by` type, the metrics-owned instrument the `reliability/resilience#RESILIENCE` owner composes into its one `set_on_retry_hooks` call.
- Auto: the meter is obtained once per process through `metrics.get_meter` and the instruments are created once and reused — never per request, per `LOCAL_ADMISSION`; the observable callbacks receive a `CallbackOptions` and return an iterable of `Observation`, reading the latest frozen `MetricState` off one reference load through the one polymorphic `_callback` closing the spec's `Project` over `cls._state` rather than blocking on the live lane; the `lane.in_flight` `ObservableUpDownCounter` reads the live `in_flight` occupancy field off the same swapped snapshot — the additive, bidirectional in-flight gauge OTel `LOCAL_ADMISSION` names for active-lane/queue-depth saturation, distinct from the monotonic `lane.drained` `ObservableCounter` whose value only ever rises — so a lane that admits past its `CapacityLimiter` and drains back shows occupancy rise and fall on one instrument rather than two divergent counters, the occupancy threaded by the `execution/lanes#LANE` `drained` aspect into `observe` rather than sampled here; the process probes read one `ProcessReading` per `observe`, sampled inside a single `Process.oneshot()` block that batches the `memory_full_info`/`cpu_percent`/`num_threads`/`num_fds` syscalls into one collection per the psutil `ONESHOT_TOPOLOGY` rather than one syscall per gauge per export, `cpu_percent(interval=None)` reading the non-blocking since-last-call delta (the first sample the `0.0` seed the psutil contract returns, subsequent samples the true delta over the `observe` cadence) and `num_fds` `hasattr`-guarded for the POSIX-gated method; the drain counter folds the five `DrainReceipt` columns into one instrument keyed by a bounded `DrainOutcome` `outcome` attribute (`accepted`/`completed`/`cancelled`/`rejected`/`hit`, cache-`hit` a first-class counter dimension) rather than five parallel counters, the `DrainOutcome`/`DRAIN_COLUMNS` taxonomy imported from the `execution/lanes#LANE` `DrainReceipt` owner rather than redeclared here so the counter outcome vocabulary is the one typed fact the receipt also reads, the fold projecting the typed `DrainReceipt` through `msgspec.structs.asdict` and indexing by the imported `DRAIN_COLUMNS` order so the five columns are a typed struct fold rather than a string-keyed `getattr`; `ProcessReading.sample` runs the one boundary read this owner admits inside `contextlib.suppress(*PROCESS_FAULTS)`, returning `None` when the handle resolves no live process (`NoSuchProcess`/`AccessDenied`/`ZombieProcess`) so a dead-process race drops the whole reading and every process gauge yields `[]` for that cycle rather than raising on the export thread — `suppress` the admitted boundary-kernel form over the `reliability/faults#FAULT` `boundary` rail since the OTel observable-callback contract returning `Iterable[Observation]` forbids a railed `Result`, so the kernel drops to `None`/`[]` rather than crossing a fault as the `boundary` case; the installed `MeterProvider` carries the one shared `Resource` the trace/log providers join so all signals carry one `service.name`; `install` under a no-op meter (a `silent` `observability/telemetry#TELEMETRY` profile) creates API no-op instruments and the callbacks register but never fire, so the cost under a `PACKAGE`/`TEST` profile is exactly the no-op meter.
- Packages: `opentelemetry-api` (`metrics.get_meter` ENTRYPOINTS [01] / `Meter.create_histogram(..., explicit_bucket_boundaries_advisory=)` [03] / `create_counter` [01] / `create_observable_counter` [05] / `create_observable_up_down_counter` [06] / `create_observable_gauge` [07] / `Counter.add(amount, attributes, context)` [08] / `Histogram.record(amount, attributes, context)` [10] / `context.get_current` context-and-propagation [03], `Observation` PUBLIC_TYPES [10] / `CallbackOptions` [11] / `Counter` [03] / `Histogram` [05] / `Meter` [02]), `psutil` (`Process` PUBLIC_TYPES [01] / `Process.oneshot` batched-read ENTRYPOINTS [01] / `Process.memory_full_info` resource-metrics ENTRYPOINTS [02] / `Process.cpu_percent` [04] / `Process.num_threads` [06] / `Process.num_fds` [08], guarding `NoSuchProcess`/`AccessDenied`/`ZombieProcess` exception types [02]/[04]/[03]), `stamina` (`instrumentation.RetryHook` / `instrumentation.RetryDetails` mapped field-for-field onto the `retry.attempts` counter), `expression` (`Result.swap`/`Result.to_option` result-ops ENTRYPOINTS [08]/[06] and `Option.map`/`Option.default_value` option-ops [01]/[07] folding the resolved `RuntimeRail` into the `measured` outcome without a raised branch), `msgspec` (`Struct` with `gc=False` on the hot-path `MetricState`/`ProcessReading` leaves, `structs.asdict` for the drain fold); the `reliability/faults#FAULT` `BoundaryFault`/`FaultTag` carrier supplies the `measured` aspect's typed outcome discriminant and the `execution/lanes#LANE` `DrainReceipt`/`DrainOutcome`/`DRAIN_COLUMNS` the shared outcome taxonomy. The SDK `MeterProvider`/`PeriodicExportingMetricReader`/`View`/`TraceBasedExemplarFilter`/`Resource.create` and the `OTLPMetricExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here; `set_on_retry_hooks` is `reliability/resilience#RESILIENCE`-owned.
- Growth: a new measured signal is one `InstrumentSpec` row on `INSTRUMENTS` plus one name on `MeterReceipt`, reached by the existing register-keyed fold with no new `create_*` call — a `counter`, `up_down`, or `gauge` row keyed by `spec.kind` through the one `register` dispatch already wiring all three observable kinds; a new process probe is one `ProbeField` literal plus one field on `ProcessReading` plus one `_gauge`-projected spec row, the `oneshot` block already batching its read; a new state-polled saturation gauge is one `up_down` `InstrumentSpec` row over one `MetricState` field threaded through `observe`; a new drain dimension is one member added on the `execution/lanes#LANE` `DrainOutcome` taxonomy, reaching this counter through the imported `DRAIN_COLUMNS` fold off the existing `DrainReceipt` with no edit here, never a new counter; a new fault-to-outcome mapping is one `FAULT_OUTCOME` row keyed by `FaultTag` (the unmapped tags already defaulting to `rejected`), never a branch added to the `measured` aspect; a new histogram attribute is one key on the `record` attribute map; a new retry attribute is one key on the `retry_hook` map off `RetryDetails`; zero new surface, one installed `MeterProvider`, one atomic state slot.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter/`View`/exemplar construction (the `observability/telemetry#TELEMETRY` install owns it, the histogram shaping the API-level `explicit_bucket_boundaries_advisory` the SDK `View` honors rather than a View minted here), no `set_on_retry_hooks` registration (the `reliability/resilience#RESILIENCE` owner runs the one registration and composes the metrics-owned `retry_hook`), no AppHost telemetry envelope, health status, product metric export, or exporter ownership; `ProcessReading.sample`'s `suppress(*PROCESS_FAULTS)` over the `oneshot` block is the one admitted boundary-kernel exception to the `reliability/faults#FAULT` `boundary` rail — the OTel observable-callback contract forbids a railed return, so the kernel drops to `None`/`[]` rather than minting the `boundary` case, the only raw-except site this owner declares; an SDK import in this owner, a per-request instrument, a per-gauge syscall outside the shared `oneshot` collection, a parallel callback method per process probe where one `Project`-closed `_callback` folds them, a mutated-in-place `MetricState` a callback reads under a torn write, a callback that blocks on the live lane lock, a lock on the observe/read path where the atomic reference swap suffices, an unguarded `psutil` read raising on the export thread, a second on-retry hook registration trampling the resilience owner, a monotonic `ObservableCounter` for live lane occupancy where the additive `ObservableUpDownCounter` rises and falls with the `CapacityLimiter`, a two-valued ok/error bool collapsing the `measured` outcome where the `FAULT_OUTCOME` projection keeps `cancelled` distinct from `rejected` over the closed `FaultTag`, and five parallel drain counters where one attribute-keyed counter folds them are the deleted forms; the live `CapacityLimiter` occupancy read stays the `execution/lanes#LANE` `drained` aspect's to thread into `observe` (this owner reads the threaded snapshot, never the lane's limiter directly), and the suite metric taxonomy stays AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable
from contextlib import suppress
from functools import wraps
from time import perf_counter
from typing import Final, Literal

import psutil
from msgspec import Struct
from msgspec.structs import asdict
from opentelemetry import context as otel_context
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Counter, Histogram, Meter, Observation
from stamina.instrumentation import RetryDetails, RetryHook

from rasm.runtime.faults import BoundaryFault, FaultTag, RuntimeRail
from rasm.runtime.lanes import DRAIN_COLUMNS, DrainOutcome, DrainReceipt

# --- [TYPES] ----------------------------------------------------------------------------

type ProbeField = Literal["rss", "uss", "cpu", "threads", "fds"]
type ObservableKind = Literal["counter", "up_down", "gauge"]
type Project = Callable[["MetricState"], Iterable[Observation]]
type ObservableCallback = Callable[[CallbackOptions], Iterable[Observation]]
type Register = Callable[..., object]
type Served[T] = Callable[..., Awaitable[RuntimeRail[T]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

METER_NAME: Final = "rasm.runtime"

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (
    psutil.NoSuchProcess,
    psutil.ZombieProcess,
    psutil.AccessDenied,
)

# request-duration bucket advisory in ms; the API advisory the SDK View honors without
# minting a second View here (View ownership stays observability/telemetry#TELEMETRY).
DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

# the serve-rail outcome projection: a fault's FaultTag maps onto the shared DrainOutcome
# axis so a deadline-tripped serve coroutine surfaces as `cancelled` (not `rejected`) and
# the histogram outcome dimension stays the one execution/lanes#LANE taxonomy the drain
# counter keys by. Off-table tags (every non-deadline fault) fall to `rejected`; an Ok rail
# is `completed`. Total over the closed FaultTag without a per-tag branch in the aspect.
FAULT_OUTCOME: Final[dict[FaultTag, DrainOutcome]] = {"deadline": "cancelled"}

# --- [MODELS] ---------------------------------------------------------------------------


class ProcessReading(Struct, frozen=True, gc=False):
    rss: int
    uss: int
    cpu: float
    threads: int
    fds: int

    @classmethod
    def sample(cls, process: psutil.Process) -> "ProcessReading | None":
        with suppress(*PROCESS_FAULTS), process.oneshot():
            full = process.memory_full_info()
            return cls(
                rss=full.rss,
                uss=getattr(full, "uss", full.rss),
                cpu=process.cpu_percent(interval=None),
                threads=process.num_threads(),
                fds=process.num_fds() if hasattr(process, "num_fds") else 0,
            )
        return None


class MetricState(Struct, frozen=True, gc=False):
    drain: DrainReceipt
    process: psutil.Process
    reading: ProcessReading | None
    in_flight: int = 0


class InstrumentSpec(Struct, frozen=True):
    name: str
    kind: ObservableKind
    unit: str
    project: Project


class MeterReceipt(Struct, frozen=True):
    histogram: str
    counter: str
    in_flight: str
    retry_counter: str
    gauges: tuple[str, ...]
    outcomes: tuple[DrainOutcome, ...]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _drain_fold(state: MetricState) -> Iterable[Observation]:
    columns = asdict(state.drain)
    return [Observation(columns[column], {"outcome": column}) for column in DRAIN_COLUMNS]


def _inflight(state: MetricState) -> Iterable[Observation]:
    return [Observation(state.in_flight)]


def _gauge(field: ProbeField) -> Project:
    return lambda state: [] if state.reading is None else [Observation(getattr(state.reading, field))]

# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    _state: MetricState = MetricState(DrainReceipt(0, 0, 0, 0), psutil.Process(), None)
    _meter: Meter = metrics.get_meter(METER_NAME)
    _duration: Histogram = _meter.create_histogram(
        "companion.request.duration", unit="ms", explicit_bucket_boundaries_advisory=DURATION_BUCKETS_MS
    )
    _retries: Counter = _meter.create_counter("retry.attempts", unit="1")

    # one row per observable instrument; install folds the table, no parallel create_* calls.
    # the drain fold is the one observable counter (monotonic per outcome); lane.in_flight is
    # the additive up_down gauge over live CapacityLimiter occupancy; the process probes are
    # observable gauges, all reading the one atomically-swapped MetricState snapshot.
    _observable: Final[tuple[InstrumentSpec, ...]] = (
        InstrumentSpec("lane.drained", "counter", "1", _drain_fold),
        InstrumentSpec("lane.in_flight", "up_down", "1", _inflight),
        InstrumentSpec("process.memory.rss", "gauge", "By", _gauge("rss")),
        InstrumentSpec("process.memory.uss", "gauge", "By", _gauge("uss")),
        InstrumentSpec("process.cpu.utilization", "gauge", "1", _gauge("cpu")),
        InstrumentSpec("process.thread.count", "gauge", "1", _gauge("threads")),
        InstrumentSpec("process.fd.count", "gauge", "1", _gauge("fds")),
    )

    @classmethod
    def install(cls) -> MeterReceipt:
        meter = metrics.get_meter(METER_NAME)
        process = psutil.Process()
        register: dict[ObservableKind, Register] = {
            "counter": meter.create_observable_counter,
            "up_down": meter.create_observable_up_down_counter,
            "gauge": meter.create_observable_gauge,
        }
        cls._meter = meter
        cls._state = MetricState(DrainReceipt(0, 0, 0, 0), process, ProcessReading.sample(process))
        cls._duration = meter.create_histogram(
            "companion.request.duration", unit="ms", explicit_bucket_boundaries_advisory=DURATION_BUCKETS_MS
        )
        cls._retries = meter.create_counter("retry.attempts", unit="1")
        for spec in cls._observable:
            register[spec.kind](spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)
        return MeterReceipt(
            histogram="companion.request.duration",
            counter="lane.drained",
            in_flight="lane.in_flight",
            retry_counter="retry.attempts",
            gauges=tuple(spec.name for spec in cls._observable if spec.kind == "gauge"),
            outcomes=DRAIN_COLUMNS,
        )

    @classmethod
    def observe(cls, drain: DrainReceipt, in_flight: int = 0) -> None:
        process = cls._state.process
        cls._state = MetricState(drain, process, ProcessReading.sample(process), in_flight)

    @classmethod
    def record(cls, duration_ms: float, method: str, outcome: DrainOutcome) -> None:
        cls._duration.record(
            duration_ms, {"rpc.method": method, "outcome": outcome}, context=otel_context.get_current()
        )

    @classmethod
    def retry_hook(cls) -> RetryHook:
        def hook(details: RetryDetails) -> None:
            cls._retries.add(1, {"target": details.name, "cause": type(details.caused_by).__qualname__})

        return hook

    @classmethod
    def measured[T](cls, method: str) -> Callable[[Served[T]], Served[T]]:
        def aspect(serve: Served[T]) -> Served[T]:
            @wraps(serve)
            async def measured_serve(*args: object, **kwargs: object) -> RuntimeRail[T]:
                start = perf_counter()
                rail = await serve(*args, **kwargs)
                outcome = rail.swap().to_option().map(cls._outcome).default_value("completed")
                cls.record((perf_counter() - start) * 1000.0, method, outcome)
                return rail

            return measured_serve

        return aspect

    @staticmethod
    def _outcome(fault: BoundaryFault) -> DrainOutcome:
        return FAULT_OUTCOME.get(fault.tag, "rejected")

    @classmethod
    def _callback(cls, spec: InstrumentSpec) -> ObservableCallback:
        return lambda _: spec.project(cls._state)
```

## [03]-[RESEARCH]

- [METRIC_INSTRUMENT_TABLE]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `metrics.get_meter(name, ...)` (ENTRYPOINTS [01]) reads the `observability/telemetry#TELEMETRY`-installed `MeterProvider` (no-op until `set_meter_provider`, so a `silent` profile yields no-op instruments), `Meter.create_histogram(name, unit, explicit_bucket_boundaries_advisory=)` (ENTRYPOINTS [03]) mints the synchronous `companion.request.duration` recorder with an API-level bucket advisory the serve leg drives through `Histogram.record(amount, attributes, context)` (ENTRYPOINTS [10]), `Meter.create_counter` (ENTRYPOINTS [01]) mints the `retry.attempts` recorder driven through `Counter.add(amount, attributes, context)` (ENTRYPOINTS [08]), and `create_observable_counter`/`create_observable_up_down_counter`/`create_observable_gauge(name, callbacks, unit)` (ENTRYPOINTS [05]/[06]/[07]) register callbacks the `PeriodicExportingMetricReader` invokes per export cycle with a `CallbackOptions` (PUBLIC_TYPES [11]) returning `Iterable[Observation]` (PUBLIC_TYPES [10]). The dense form is the `INSTRUMENTS` `InstrumentSpec` table — one row per observable signal carrying name/kind/unit/`Project` — folded in `install` through a `register: dict[ObservableKind, Register]` over the `counter`/`up_down`/`gauge` kinds so the create call is keyed by `spec.kind` and one polymorphic `_callback` closes each spec's `Project` over `cls._state`, collapsing the prior three sibling callback classmethods (`_drained`/`_rss`/`_cpu`) into one table-driven shape; a new signal is one row, not a new method plus a new `create_*` call. The `lane.in_flight` `ObservableUpDownCounter` is the additive in-flight gauge the `.api/opentelemetry-api.md` `LOCAL_ADMISSION` (`UpDownCounter`/`Observable*` selection guidance) names for queue-depth/active-lane occupancy — it reads the live `MetricState.in_flight` occupancy the lane's `drained` aspect threads into `observe`, distinct from the monotonic `lane.drained` `ObservableCounter`, so saturation rises and falls on one bidirectional instrument rather than a counter that can only climb. The `explicit_bucket_boundaries_advisory` is the API-surfaced bucket hint the SDK `ExplicitBucketHistogramAggregation` View honors per `libs/python/.api/opentelemetry-sdk.md` (ENTRYPOINTS [07]) without minting a View in this owner, and the `context=otel_context.get_current()` on `record` is the active-context handle the SDK `TraceBasedExemplarFilter` (PUBLIC_TYPES [06]) samples to tie each duration measurement to its serve span. Instruments are created once per meter and reused per the `LOCAL_ADMISSION`. The instrument-table spellings are settled.
- [ATOMIC_STATE_SWAP]: the observable callbacks run on the SDK export thread the `PeriodicExportingMetricReader` drives while the serve leg's `Metrics.observe` runs on the lane's event loop — the cross-thread read demands the snapshot be published atomically, not mutated field-by-field. The frozen `MetricState` (`msgspec.Struct(frozen=True, gc=False)`, the `gc=False` opting the hot-path leaf out of the tracked GC set per `libs/python/.api/msgspec.md` `MSGSPEC_TOPOLOGY`) is built complete then published through a single `cls._state = ...` class-attribute store, the one bytecode `STORE_NAME`/`STORE_ATTR` CPython executes indivisibly under the GIL, so a callback's `state = cls._state` load reads either the prior or the next complete snapshot and never a half-written one; no lock guards either the swap or the read, the immutability the correctness guarantee rather than a mutex. The `psutil.Process` handle is invariant across swaps; the `DrainReceipt` fold and the batched `ProcessReading` are the fields that turn over, so the gauge callbacks read a stable process identity and a cached reading while the counter reads the freshest fold. The atomic-swap shape is settled.
- [PROCESS_ONESHOT_BATCH]: confirmed against `libs/python/.api/psutil.md` `ONESHOT_TOPOLOGY`/`LOCAL_ADMISSION` — `ProcessReading.sample` folds `Process.memory_full_info()` (resource-metrics ENTRYPOINTS [02], carrying `rss` and the macOS/Linux `uss`), `cpu_percent(interval=None)` ([04]), `num_threads()` ([06]), and the POSIX-gated `num_fds()` ([08], `hasattr`-guarded per `PLATFORM_GATING`) inside one `Process.oneshot()` (batched-read ENTRYPOINTS [01]) so the cluster of reads costs one collection per `observe` rather than one syscall per gauge per export cycle — the canonical batch path the psutil `INTEGRATION_LAW` names for the OTel observable-gauge hand-off. `cpu_percent(interval=None)` returns `0.0` on the first sample with the true delta on subsequent calls over the `observe` cadence, so the timing window rides the sampling cadence without the callback ever blocking. The whole `oneshot` block runs inside `contextlib.suppress(*PROCESS_FAULTS)` so a dead-process race between handle mint and sample (`NoSuchProcess`/`ZombieProcess`/`AccessDenied`, PUBLIC_TYPES [02]/[03]/[04]) drops the reading to `None` and every process gauge yields `[]` for that cycle rather than raising on the export thread — the one boundary read this owner admits, the guard the psutil `LOCAL_ADMISSION` requires. The batched-reading spellings are settled.
- [RETRY_HOOK_COMPOSITION]: reflection-confirmed against `libs/python/runtime/.api/stamina.md` — the `retry.attempts` `Counter` is metrics-owned, but the on-retry registration is `reliability/resilience#RESILIENCE`-owned: that owner runs the one `instrumentation.set_on_retry_hooks` call (the hooks are process-global, so a second registration here would overwrite or duplicate it). `Metrics.retry_hook` returns a `RetryHook` (a `(RetryDetails) -> None` callable per the instrumentation PUBLIC_TYPES) that the resilience owner composes into its hook tuple alongside `StructlogOnRetryHook`, folding `RetryDetails.name`/`caused_by` field-for-field onto the counter attributes per the `RetryDetails` fact-carrier contract, so retry exhaustion is a first-class metric without a second hook owner. The composed-hook seam is settled.
- [MEASURED_OUTCOME_PROJECTION]: reflection-confirmed against `libs/python/.api/expression.md` (result-ops `Result.swap`/`Result.to_option` [08]/[06], option-ops `Option.map`/`Option.default_value` [01]/[07]) and the `reliability/faults#FAULT` `BoundaryFault`/`FaultTag` owner — the `measured` aspect derives the duration-histogram `outcome` attribute from the resolved `RuntimeRail` rather than a two-valued ok/error bool: `rail.swap().to_option().map(cls._outcome).default_value("completed")` inverts the rail so an `Error` surfaces its `BoundaryFault`, `_outcome` reads `fault.tag` and indexes the `FAULT_OUTCOME` `dict[FaultTag, DrainOutcome]` table (`deadline -> cancelled`, every other tag defaulting to `rejected` through `dict.get`), and an `Ok` rail folds to `completed` via the `Option.default_value` fallback. This makes the histogram outcome total over the closed `FaultTag` and shares the one `execution/lanes#LANE` `DrainOutcome` axis the `lane.drained` counter keys by — a deadline-tripped serve coroutine surfaces as `cancelled` (matching the lane drain's `cancelled` deadline-containment column) rather than collapsing into `rejected`, so the duration histogram and the drain counter agree on the outcome vocabulary and a deadline trip is distinguishable from a domain rejection in the latency distribution. The expression result-fold avoids a raised branch: the rail is never unwrapped through an exception, only through the `swap`/`to_option`/`map`/`default_value` monadic chain. The outcome-projection spelling is settled. No open RESEARCH seam remains on this page.
```

