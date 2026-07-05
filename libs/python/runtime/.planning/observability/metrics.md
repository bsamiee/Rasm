# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured instrument set against the `MeterProvider` `observability/telemetry#TELEMETRY` installs at the composition root — it constructs no provider, reader, or exporter, minting its meter once from the `reliability/faults#FAULT` `SCOPES[Scope.METER]` row through `metrics.get_meter` so all signals carry the one shared `Resource` and no scope literal is re-spelled here. One `INSTRUMENTS` `Block[InstrumentSpec]` table owns both instrument disciplines and every derived surface: a synchronous row carries the `slot` column naming its `SyncInstruments` field (the carrier is a comprehension over exactly those rows — no per-instrument `create_*` call, no seed function, no name-to-field map beside the table), an observable row carries a `Project` closure the one polymorphic `_callback` closes over the frozen `MetricState`, a `domain` column marks the domain-distribution rows the mapping arm of `record` reaches through the table-derived `_DOMAIN_SLOT`, and `MeterReceipt.instruments` names the same rows. `install` is wrapped by the imported `latched[R, **P]` one-shot aspect, so a re-bootstrap returns the cached `MeterReceipt` stamped `REENTRANT` before the mint body runs and the SDK never holds a doubled callback set.

The instruments read a frozen `MetricState` swapped under one atomic reference assignment — the single bytecode store CPython executes indivisibly — so the export thread reads the latest drain fold, in-flight occupancy, and batched `ProcessReading` with no lock and the serve leg never blocks the lane to publish. Request duration is a synchronous `Histogram` shaped by `explicit_bucket_boundaries_advisory` and recorded under the active OTel `Context`; the exemplar hand-off is real because the telemetry install wires `MeterProvider(exemplar_filter=TraceBasedExemplarFilter())` — without that seam the SDK default drops every exemplar, so the `context=` thread and the filter install are one cross-page contract. Lane drain counts an `ObservableCounter` folded from one `DrainReceipt` over the `DRAIN_COLUMNS` axis, lane saturation rides an `ObservableUpDownCounter` over the `in_flight` count the `execution/lanes#LANE` `drained` aspect threads in as `Metrics.observe(receipt, in_flight=receipt.cancelled)`, and the process probes are `ObservableGauge` rows over one `psutil.Process.oneshot()` collection. Retry attempts ride the metrics-owned `retry_hook` the `reliability/resilience#RESILIENCE` owner composes into its one `set_on_retry_hooks` registration, and the `measured` aspect projects a resolved rail's `BoundaryFault.tag` through the `FAULT_OUTCOME` `Map` onto the shared `DrainOutcome` axis — a deadline trip surfacing as `cancelled`, every other fault as `rejected`, an `Ok` rail as `completed` — at one site rather than a lossy ok/error bool per handler. The package mints the local metric stream only; provider install, exemplar-filter and `View` machinery, hook registration, product export, and health stay `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE` / AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the `SCOPES[Scope.METER]`-minted installed-meter read, the atomic-swap `MetricState` snapshot, the one `INSTRUMENTS` table whose `slot`/`project`/`domain` columns derive the `SyncInstruments` carrier, the observable registrations, and the `_DOMAIN_SLOT` domain-recorder map, the `latched` one-shot install returning the `MeterReceipt` stamped `INSTALLED`/`REENTRANT`, the in-flight saturation gauge and process probes over the `ProcessReading.sample` `oneshot` batch, the exemplar-recorded request-duration histogram, the polymorphic `record` (scalar duration arm, mapping domain-distribution arm), the `retry.attempts` counter and its composed `retry_hook`, and the `measured` serve aspect folding the rail through the `FAULT_OUTCOME` `Map`.

## [02]-[METRIC]

- Owner: `Metrics` — the static surface owning the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider`. `InstrumentKind` is the closed `StrEnum` keying the create dispatch across the two synchronous (`histogram`/`counter`) and three observable (`observable`/`up_down`/`gauge`) families; `InstrumentSpec` is the one row carrying `name`/`kind`/`unit` plus three optional columns — `project` (observable pull closure), `advisory` (histogram bucket hint), `slot` (the `SyncInstruments` field a synchronous row lands in), and `domain` (the attribute key marking a domain-distribution row); `INSTRUMENTS` is the one `Block[InstrumentSpec]` table every derived surface reads — the `_sync_carrier` comprehension mints the `SyncInstruments` frozen carrier from the `slot` rows, the install fold registers the `slot=None` rows through the `ObservableFactory` dispatch, `_DOMAIN_SLOT` derives name-to-field for the `domain` rows, and `MeterReceipt.instruments` names every row. `ObservableFactory` is the structural `Protocol` (`(name, *, callbacks, unit) -> object`) the three `create_observable_*` methods satisfy so the dispatch is typed, never an erased `Callable[..., object]`. `MetricState` is the frozen carrier the observable callbacks read — the latest `DrainReceipt[object]` fold, the lane-threaded `in_flight` saturation count, the stable `psutil.Process` handle, and the one batched `ProcessReading`. `FAULT_OUTCOME` is the `Map[FaultTag, DrainOutcome]` projection table `measured` folds a rail's fault tag through onto the one `observability/receipts#RECEIPT` taxonomy. `latched` is the receipt-agnostic one-shot aspect imported from `reliability/faults#FAULT`, closed here over the `_receipt` accessor pair and a `replace`-built `REENTRANT` restamp — one definition, two latches with the telemetry owner, never a re-pinned local guard. `MeterReceipt` is the typed registration evidence (the `MeterOutcome` stamp, the table-derived `instruments` tuple, the `DRAIN_COLUMNS` outcome axis). The provider, `PeriodicExportingMetricReader`/`OTLPMetricExporter`, the `View`/`TraceBasedExemplarFilter` machinery, the on-retry hook registration, and `RUNTIME_RESOURCE` are NOT owned here — they install once at `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE`.
- State: two frozen carriers turn over under atomic reference stores, never mutated in place — a callback's `cls._state` load reads either the prior or the next complete snapshot, immutability the correctness guarantee rather than a mutex. Only the `ProcessReading` leaf (all `int`/`float` fields) carries `gc=False` per the `msgspec` `MSGSPEC_TOPOLOGY` non-container-leaf contract; `MetricState` and `SyncInstruments` reference structs and instrument objects, so both stay tracked frozen structs. The class body seeds `_state` with the shared `ZERO_DRAIN` (spelling `hit=0` explicitly so the seed and `DRAIN_COLUMNS` agree column-for-column) and `_sync` with `_sync_carrier` over the pre-install no-op meter; `install` re-seeds both against the installed meter. The `psutil.Process` handle mints at `install` and carries through every swap, so process identity is stable across the run; the drain fold, `in_flight`, and the reading are the fields that turn over, the reading sampled once per `observe` so gauge callbacks read a cached fold rather than firing syscalls on the export thread.
- Entry: `Metrics.install` is wrapped by `latched(read, write, reentrant)` onto the `_receipt` latch — a re-entrant call returns the cached receipt `REENTRANT`-stamped before the body runs, so a re-admission or test re-bootstrap re-registers no callback and re-creates no recorder. The body obtains the installed meter through `metrics.get_meter(SCOPES[Scope.METER])` (under a `PACKAGE`/`TEST` profile no provider is set, so the read resolves the API no-op meter and the fold mints no-op instruments — the gate is the installed provider, never a profile argument here), registers every `slot=None` row through the `observable` kind-keyed dispatch with one polymorphic `_callback` per row, swaps `_sync` to `_sync_carrier(meter)`, re-seeds `_state`, and returns the `MeterReceipt`. `Metrics.observe(drain, in_flight)` swaps a fresh snapshot in one atomic assignment, never holding the lane's task-group lock. `Metrics.record` is one polymorphic entrypoint discriminating on the measure's shape: a scalar records the request-duration histogram keyed by `rpc.method` and the bounded `DrainOutcome` `outcome` attribute; a `Mapping[str, float]` records each named measure onto its domain-distribution row's held recorder through `_DOMAIN_SLOT` under one `{domain: kind}` attribute map (the artifacts emit-harvest seam records `{"artifact.byte_volume": ..., "artifact.compression_ratio": ...}` with `domain="artifact"` off each emitted `ArtifactReceipt._facts`) — both arms under `context=otel_context.get_current()` so every synchronous measurement exemplar-correlates to its active span. `Metrics.retry_hook` returns a `RetryHook` incrementing `retries` keyed by `RetryDetails.name` and `caused_by` type under the same active context — the metrics-owned instrument the resilience owner composes into its one `set_on_retry_hooks` call, never a second registration here. `Metrics.measured(method)` is the `[**P, T]`-parametric aspect over a `Callable[P, Awaitable[RuntimeRail[T]]]` serve coroutine: it times the call with `perf_counter`, folds the resolved rail through `rail.swap().to_option().map(cls._outcome).default_value("completed")`, records the duration, and returns the rail untouched — the histogram outcome total over the closed `FaultTag`, `cancelled` distinct from `rejected`.
- Auto: the meter is obtained once per process and the instruments created once and reused, the `latched` aspect enforcing the at-most-once fold. Observable callbacks receive a `CallbackOptions` and return `Iterable[Observation]` off one `cls._state` reference load; the `lane.in_flight` `ObservableUpDownCounter` is the additive bidirectional saturation instrument (the OTel `LOCAL_ADMISSION` queue-depth selection), its value the per-drain went-live-but-unresolved cardinality the lane owner computes — this owner never reads the lane's `CapacityLimiter` directly. The drain counter folds the five `DrainReceipt` columns into one instrument keyed by the imported `DRAIN_COLUMNS` literal read as typed field access — never an `asdict` materializing the receipt's `values`/`cache`/`faults` containers per export cycle. `ProcessReading.sample` batches `memory_full_info`/`cpu_percent`/`num_threads`/`num_fds` inside one `Process.oneshot()` block per the psutil `ONESHOT_TOPOLOGY`, `cpu_percent(interval=None)` the non-blocking since-last-call delta (first sample the `0.0` seed) and `num_fds` `hasattr`-guarded for the POSIX gate; the whole block runs inside `suppress(*PROCESS_FAULTS)` so a dead-process race drops the reading to `None` and every process gauge yields `[]` for that cycle — `suppress` is the admitted boundary-kernel form here because the OTel observable-callback contract returning `Iterable[Observation]` forbids a railed `Result`.
- Packages: `opentelemetry-api` (`metrics.get_meter` / `Meter.create_histogram(..., explicit_bucket_boundaries_advisory=)` / `create_counter` / `create_observable_counter` / `create_observable_up_down_counter` / `create_observable_gauge` / `Counter.add(amount, attributes, context)` / `Histogram.record(amount, attributes, context)` / `context.get_current` / `Observation` / `CallbackOptions`), `psutil` (`Process.oneshot` / `memory_full_info` / `cpu_percent` / `num_threads` / `num_fds`, guarding `NoSuchProcess`/`ZombieProcess`/`AccessDenied`), `stamina` (`instrumentation.RetryHook`/`RetryDetails` mapped field-for-field onto the counter attributes), `expression` (`Block.of_seq`/`Block.filter`/`Block.fold` over the one table, `Map.of_seq`/`Map.try_find` the `FAULT_OUTCOME` and `_DOMAIN_SLOT` rails, `Result.swap`/`Result.to_option`/`Option.map`/`Option.default_value` the `measured` fold), `msgspec` (`Struct` with `gc=False` only on the `ProcessReading` leaf; `structs.replace` the `REENTRANT` restamp); `reliability/faults#FAULT` supplies `BoundaryFault`/`FaultTag`, the `latched` aspect, and the `SCOPES`/`Scope` vocabulary; `observability/receipts#RECEIPT` supplies `DrainReceipt`/`DrainOutcome`/`DRAIN_COLUMNS`. The SDK `MeterProvider`/`PeriodicExportingMetricReader`/`View`/`TraceBasedExemplarFilter` and `OTLPMetricExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here; `set_on_retry_hooks` is `reliability/resilience#RESILIENCE`-owned.
- Growth: a new measured signal is one `InstrumentSpec` row — a synchronous row adds its `slot` field on `SyncInstruments` and reaches the carrier comprehension, a domain-distribution row additionally carries `domain` and reaches `record`'s mapping arm through the derived `_DOMAIN_SLOT` with no new classmethod, an observable row registers through the one `ObservableFactory` dispatch — and `MeterReceipt.instruments` derives from the same table so the row names itself; a new instrument family is one `InstrumentKind` member plus one dispatch entry; a new process probe is one `ProbeField` literal plus one `ProcessReading` field plus one `_gauge` row, the `oneshot` block already batching its read; a new drain dimension is one `DrainOutcome` member on the `observability/receipts#RECEIPT` owner, reaching this counter through the imported `DRAIN_COLUMNS` with no edit here; a new fault-to-outcome mapping is one `FAULT_OUTCOME` row (unmapped tags default `rejected`); zero new surface, one installed `MeterProvider`, one atomic state slot, one install latch.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter/`View`/exemplar construction (the telemetry install owns it — the histogram ships the API-level advisory the SDK honors, and the exemplar filter is telemetry's `exemplar_filter=TraceBasedExemplarFilter()` install), no `set_on_retry_hooks` registration, no AppHost telemetry envelope, health status, or product export; `ProcessReading.sample`'s `suppress` block is the one admitted raw-except site. The deleted forms: an SDK import in this owner; a per-request instrument; a per-gauge syscall outside the shared `oneshot`; a scope literal beside the `SCOPES` row; a seed function, name-to-field map, or per-instrument `create_*` call beside the one table whose `slot` column derives the carrier; a domain-specific `record_<domain>` classmethod beside the mapping arm of the one polymorphic `record`; a plain-`dict` projection table beside the corpus `Map` rail; a non-idempotent `install` that re-registers callbacks; a mutated-in-place `MetricState`; a lock on the observe/read path; a monotonic counter for the in-flight saturation signal; a two-valued ok/error bool collapsing the `measured` outcome; five parallel drain counters; and a `ZERO_DRAIN` seed omitting a `DRAIN_COLUMNS` column.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping, Sequence
from contextlib import suppress
from enum import StrEnum
from functools import wraps
from time import perf_counter
from typing import ClassVar, Final, Literal, Protocol, overload

import psutil
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import context as otel_context
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Counter, Histogram, Meter, Observation
from stamina.instrumentation import RetryDetails, RetryHook

from rasm.runtime.faults import SCOPES, BoundaryFault, FaultTag, RuntimeRail, Scope, latched
from rasm.runtime.receipts import DRAIN_COLUMNS, DrainOutcome, DrainReceipt

# --- [TYPES] ----------------------------------------------------------------------------

type ProbeField = Literal["rss", "uss", "cpu", "threads", "fds"]
type Project = Callable[["MetricState"], Iterable[Observation]]
type ObservableCallback = Callable[[CallbackOptions], Iterable[Observation]]


# the create-dispatch discriminant: two synchronous recorders + three observable kinds.
class InstrumentKind(StrEnum):
    HISTOGRAM = "histogram"
    COUNTER = "counter"
    OBSERVABLE = "observable"
    UP_DOWN = "up_down"
    GAUGE = "gauge"


# the structural create signature the three `create_observable_*` methods satisfy, so the
# kind-keyed dispatch is typed rather than an erased `Callable[..., object]`.
class ObservableFactory(Protocol):
    def __call__(self, name: str, *, callbacks: Sequence[ObservableCallback], unit: str) -> object: ...


class MeterOutcome(StrEnum):
    INSTALLED = "installed"
    REENTRANT = "reentrant"


# --- [CONSTANTS] ------------------------------------------------------------------------

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# request-duration bucket advisory (ms); the SDK honors it without a View, View ownership stays telemetry.
DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

# serve-rail FaultTag -> DrainOutcome projection: `deadline` lands `cancelled`, every other
# tag defaults `rejected` through the `try_find` fold, an Ok rail folds `completed` at the call site.
FAULT_OUTCOME: Final[Map[FaultTag, DrainOutcome]] = Map.of_seq([("deadline", "cancelled")])

# the all-zero seed the class body and the install re-seed publish before the first `observe`;
# `hit=0` is spelled so the seed carries every DRAIN_COLUMNS column explicitly.
ZERO_DRAIN: Final[DrainReceipt[object]] = DrainReceipt(accepted=0, completed=0, cancelled=0, rejected=0, hit=0)

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


class MetricState(Struct, frozen=True):
    drain: DrainReceipt[object]
    process: psutil.Process
    reading: ProcessReading | None
    in_flight: int = 0


# one row per instrument: `slot` names the SyncInstruments field a synchronous row lands in
# (None marks an observable row carrying `project`), `advisory` rides only histogram rows,
# `domain` marks a domain-distribution row `record`'s mapping arm reaches by name.
class InstrumentSpec(Struct, frozen=True):
    name: str
    kind: InstrumentKind
    unit: str
    project: Project | None = None
    advisory: tuple[float, ...] | None = None
    slot: str | None = None
    domain: str | None = None


# the held synchronous recorders, swapped as one frozen carrier (the `MetricState`
# atomic-reference idiom). No `gc=False`: it holds instrument references, not a leaf.
class SyncInstruments(Struct, frozen=True):
    duration: Histogram
    retries: Counter
    artifact_bytes: Histogram
    artifact_ratio: Histogram


class MeterReceipt(Struct, frozen=True):
    outcome: MeterOutcome
    instruments: tuple[str, ...]
    outcomes: tuple[DrainOutcome, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _drain_fold(state: MetricState) -> Iterable[Observation]:
    return [Observation(getattr(state.drain, column), {"outcome": column}) for column in DRAIN_COLUMNS]


def _inflight(state: MetricState) -> Iterable[Observation]:
    return [Observation(state.in_flight)]


def _gauge(field: ProbeField) -> Project:
    return lambda state: [] if state.reading is None else [Observation(getattr(state.reading, field))]


def _recorder(meter: Meter, spec: InstrumentSpec) -> Histogram | Counter:
    match spec.kind:
        case InstrumentKind.COUNTER:
            return meter.create_counter(spec.name, unit=spec.unit)
        case _:
            return meter.create_histogram(spec.name, unit=spec.unit, explicit_bucket_boundaries_advisory=spec.advisory)


# the one table every derived surface reads: sync rows land by `slot`, observable rows pull
# from `_state` through `project`, `domain` rows feed `record`'s mapping arm.
INSTRUMENTS: Final[Block[InstrumentSpec]] = Block.of_seq([
    InstrumentSpec("companion.request.duration", InstrumentKind.HISTOGRAM, "ms", advisory=DURATION_BUCKETS_MS, slot="duration"),
    InstrumentSpec("retry.attempts", InstrumentKind.COUNTER, "1", slot="retries"),
    InstrumentSpec("artifact.byte_volume", InstrumentKind.HISTOGRAM, "By", slot="artifact_bytes", domain="artifact"),
    InstrumentSpec("artifact.compression_ratio", InstrumentKind.HISTOGRAM, "1", slot="artifact_ratio", domain="artifact"),
    InstrumentSpec("lane.drained", InstrumentKind.OBSERVABLE, "1", _drain_fold),
    InstrumentSpec("lane.in_flight", InstrumentKind.UP_DOWN, "1", _inflight),
    InstrumentSpec("process.memory.rss", InstrumentKind.GAUGE, "By", _gauge("rss")),
    InstrumentSpec("process.memory.uss", InstrumentKind.GAUGE, "By", _gauge("uss")),
    InstrumentSpec("process.cpu.utilization", InstrumentKind.GAUGE, "1", _gauge("cpu")),
    InstrumentSpec("process.thread.count", InstrumentKind.GAUGE, "1", _gauge("threads")),
    InstrumentSpec("process.fd.count", InstrumentKind.GAUGE, "1", _gauge("fds")),
])

# instrument-name -> SyncInstruments field for the domain rows, derived from the one table so a
# new domain distribution is one row + one carrier field, never an edit to `record`.
_DOMAIN_SLOT: Final[Map[str, str]] = Map.of_seq([
    (spec.name, slot) for spec in INSTRUMENTS if spec.domain is not None and (slot := spec.slot) is not None
])


# the synchronous carrier DERIVES from the table's `slot` rows against one meter resolution.
def _sync_carrier(meter: Meter) -> SyncInstruments:
    return SyncInstruments(**{slot: _recorder(meter, spec) for spec in INSTRUMENTS if (slot := spec.slot) is not None})


# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    _state: ClassVar[MetricState] = MetricState(ZERO_DRAIN, psutil.Process(), None)
    _sync: ClassVar[SyncInstruments] = _sync_carrier(metrics.get_meter(SCOPES[Scope.METER]))
    _receipt: ClassVar[MeterReceipt | None] = None

    @classmethod
    @latched(lambda: Metrics._receipt, lambda r: setattr(Metrics, "_receipt", r), lambda prior: replace(prior, outcome=MeterOutcome.REENTRANT))
    def install(cls) -> MeterReceipt:
        meter = metrics.get_meter(SCOPES[Scope.METER])
        process = psutil.Process()
        observable: dict[InstrumentKind, ObservableFactory] = {
            InstrumentKind.OBSERVABLE: meter.create_observable_counter,
            InstrumentKind.UP_DOWN: meter.create_observable_up_down_counter,
            InstrumentKind.GAUGE: meter.create_observable_gauge,
        }

        def enroll(_: None, spec: InstrumentSpec) -> None:
            observable[spec.kind](spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)

        INSTRUMENTS.filter(lambda spec: spec.slot is None).fold(enroll, None)
        cls._state = MetricState(ZERO_DRAIN, process, ProcessReading.sample(process))
        cls._sync = _sync_carrier(meter)
        return MeterReceipt(MeterOutcome.INSTALLED, tuple(spec.name for spec in INSTRUMENTS), DRAIN_COLUMNS)

    @classmethod
    def observe(cls, drain: DrainReceipt[object], in_flight: int = 0) -> None:
        process = cls._state.process
        cls._state = MetricState(drain, process, ProcessReading.sample(process), in_flight)

    @overload
    @classmethod
    def record(cls, measure: float, *, method: str, outcome: DrainOutcome) -> None: ...
    @overload
    @classmethod
    def record(cls, measure: Mapping[str, float], *, domain: str, kind: str) -> None: ...
    @classmethod
    def record(
        cls, measure: float | Mapping[str, float], *, method: str = "", outcome: DrainOutcome = "completed", domain: str = "", kind: str = ""
    ) -> None:
        # one polymorphic recorder: a scalar is the request-duration row, a mapping records each
        # named measure onto its `_DOMAIN_SLOT`-resolved domain-distribution row.
        match measure:
            case Mapping() as measures:
                attributes, context = {domain: kind}, otel_context.get_current()
                Block.of_seq(measures.items()).fold(
                    lambda _, kv: getattr(cls._sync, _DOMAIN_SLOT[kv[0]]).record(kv[1], attributes, context=context), None
                )
            case amount:
                cls._sync.duration.record(amount, {"rpc.method": method, "outcome": outcome}, context=otel_context.get_current())

    @classmethod
    def retry_hook(cls) -> RetryHook:
        def hook(details: RetryDetails) -> None:
            cls._sync.retries.add(1, {"target": details.name, "cause": type(details.caused_by).__qualname__}, context=otel_context.get_current())

        return hook

    @classmethod
    def measured[**P, T](cls, method: str) -> Callable[[Callable[P, Awaitable[RuntimeRail[T]]]], Callable[P, Awaitable[RuntimeRail[T]]]]:
        def aspect(serve: Callable[P, Awaitable[RuntimeRail[T]]]) -> Callable[P, Awaitable[RuntimeRail[T]]]:
            @wraps(serve)
            async def measured_serve(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
                start = perf_counter()
                rail = await serve(*args, **kwargs)
                outcome = rail.swap().to_option().map(cls._outcome).default_value("completed")
                cls.record((perf_counter() - start) * 1000.0, method=method, outcome=outcome)
                return rail

            return measured_serve

        return aspect

    @staticmethod
    def _outcome(fault: BoundaryFault) -> DrainOutcome:
        return FAULT_OUTCOME.try_find(fault.tag).default_value("rejected")

    @classmethod
    def _callback(cls, spec: InstrumentSpec) -> ObservableCallback:
        return lambda _: () if spec.project is None else spec.project(cls._state)
```
