# [PY_RUNTIME_METRICS]

The async-observable metric spine. `Metrics` is the one static surface registering the measured-execution instrument set against the `MeterProvider` `observability/telemetry#TELEMETRY` installs at the composition root — it constructs no provider, reader, or exporter, reading the installed meter through `metrics.get_meter` so all signals carry the one shared `Resource`. One data-driven `INSTRUMENTS` `Block[InstrumentSpec]` table owns both instrument disciplines: each row carries a name, an `InstrumentKind` discriminant, a unit, and (for observables) a `Project` closure off the shared `MetricState`, and `install` runs one `Block.fold` over it — the two synchronous `histogram`/`counter` rows minting held recorders into the swapped `SyncInstruments` carrier, the three observable `observable`/`up_down`/`gauge` rows registering through one kind-keyed `ObservableFactory` dispatch — so there is no per-instrument `create_*` call, no twice-constructed class slot, and no sibling callback method per signal; one polymorphic `_callback` closes each row's `Project` over the frozen snapshot, so the drain fold, the in-flight gauge, and every process probe are one callback shape over one state read. The install is the slot-parametric `latched` one-shot aspect — a re-bootstrap reads the cached `MeterReceipt` and returns it stamped `REENTRANT` before the mint body runs, so the SDK never double-registers a callback set that would fire twice per export.

The instruments read a frozen `MetricState` swapped under an atomic reference assignment, so the export thread reads the latest drain fold, the in-flight occupancy, and the one batched `ProcessReading` with no lock on the read path and the serve leg never blocks the lane to publish. Companion request duration is a synchronous `Histogram` shaped by an `explicit_bucket_boundaries_advisory` and recorded under the active OTel `Context` so the SDK exemplar filter ties each measurement to its span; lane drain counts an `ObservableCounter` folded from one `DrainReceipt` over the `DRAIN_COLUMNS` outcome axis; lane saturation rides an `ObservableUpDownCounter` reading the `MetricState.in_flight` field the `execution/lanes#LANE` `drained` aspect threads in as `Metrics.observe(receipt, in_flight=receipt.cancelled)` — the went-live-but-unresolved cardinality the bidirectional in-flight gauge the OTel `LOCAL_ADMISSION` names for active-lane/queue-depth saturation, never a monotonic counter; and the process probes (RSS, USS, CPU utilization, thread count, fd count) are `ObservableGauge` rows over one `psutil.Process.oneshot()` collection. Lane saturation, retry exhaustion, and companion latency are first-class metrics, not log fields: retry attempts ride a `retry.attempts` `Counter` fed by the metrics-owned `retry_hook` the `reliability/resilience#RESILIENCE` owner composes into its one `set_on_retry_hooks` registration, and the `measured` aspect times a serve coroutine and projects its `BoundaryFault.tag` through the `FAULT_OUTCOME` table onto the shared `DrainOutcome` axis — a deadline-tripped rail surfacing as `cancelled`, every other fault as `rejected`, an `Ok` rail as `completed` — at one site rather than inline timing or a lossy ok/error bool per handler. The registered instrument set is named once in the typed `MeterReceipt` the measured-signal consumers read. The package mints the local metric stream only; the provider install, the on-retry hook registration, product telemetry export, and health stay `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE` / AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the installed-meter read, the atomic-swap `MetricState` snapshot, the one `INSTRUMENTS` `Block[InstrumentSpec]` table `install` folds across both disciplines through one `InstrumentKind`-keyed dispatch (synchronous `histogram`/`counter` rows minting the `SyncInstruments` carrier, observable `observable`/`up_down`/`gauge` rows registering one polymorphic `_callback`), the `latched` one-shot install aspect returning the `MeterReceipt` stamped `INSTALLED`/`REENTRANT`, the in-flight saturation gauge and the process probes, the `ProcessReading.oneshot` batch, the bucket-shaped exemplar-recorded request-duration histogram, the `retry.attempts` counter and its composed `retry_hook`, the `measured` serve aspect projecting the rail `BoundaryFault.tag` through the `FAULT_OUTCOME` table onto the `DrainOutcome` axis.

## [02]-[METRIC]

- Owner: `Metrics` — the static surface owning the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider`; `InstrumentKind` the closed `StrEnum` keying the create dispatch across the two synchronous (`histogram`/`counter`) and three observable (`observable`/`up_down`/`gauge`) families; `InstrumentSpec` the one row carrying name/kind/unit, an optional `Project` closure (observable rows) and an optional bucket `advisory` (the histogram row); `INSTRUMENTS` the one `Block[InstrumentSpec]` table `install` folds across both disciplines; `SyncInstruments` the frozen carrier holding the two imperative recorders (`duration` `Histogram`, `retries` `Counter`) swapped as one reference rather than two class slots each constructed twice, the install fold landing them by name; `ObservableFactory` the structural `Protocol` (`(name, *, callbacks, unit) -> object`) the three `create_observable_*` methods satisfy so the dispatch is typed, never an erased `Callable[..., object]`; `MetricState` the frozen carrier the observable callbacks read, holding the latest `DrainReceipt[object]` fold, the `in_flight` saturation count the lane owner threads (`receipt.cancelled`, the went-live-but-unresolved cardinality), the `psutil.Process` handle, and the one batched `ProcessReading` so a callback never reconstructs live state or fires a syscall under a lane lock; `FAULT_OUTCOME` the `FaultTag`-keyed projection table the `measured` aspect folds a rail's `BoundaryFault.tag` through onto the shared `DrainOutcome` axis so the duration histogram outcome dimension is the one `execution/lanes#LANE` taxonomy and never a lossy ok/error bool; `ProcessReading` the value object folding one `Process.oneshot()` collection into a typed RSS/USS/CPU/thread/fd reading; `latched` the receipt-agnostic `latched[R, **P](read, write, reentrant)` one-shot install aspect imported from `observability/telemetry#TELEMETRY` — the same decorator the `InstallReceipt` latch binds, carrying the latch and the `REENTRANT` restamp through injected accessors so this owner re-stamps the cached `MeterReceipt` through its own `reentrant` closure rather than redefining a `MeterReceipt`-pinned guard — reading the cached `_receipt` and returning it `REENTRANT`-stamped before the mint body so a re-bootstrap never re-registers callbacks; `MeterReceipt` the typed registration evidence (the `MeterOutcome` `INSTALLED`/`REENTRANT` stamp, the full `instruments` name tuple, the `outcomes` `DRAIN_COLUMNS` axis) the measured-signal consumers read. The provider, the `PeriodicExportingMetricReader`/`OTLPMetricExporter`, the `View`/exemplar machinery, the on-retry hook registration, and the `RUNTIME_RESOURCE` are NOT owned here — they install once at `observability/telemetry#TELEMETRY` / `reliability/resilience#RESILIENCE`, and `Metrics` reads the registered meter and contributes a hook.
- State: two frozen carriers turn over under atomic reference stores, never mutated in place and never partially read. Only the `ProcessReading` leaf — all `int`/`float` fields — carries `gc=False` per the `msgspec` `MSGSPEC_TOPOLOGY` non-container-leaf contract; `MetricState` (holding the `DrainReceipt[object]` struct and a `ProcessReading` reference) and `SyncInstruments` (holding `Histogram`/`Counter` references) stay tracked frozen structs, since `gc=False` is admitted only for a leaf holding no container or struct field. `MetricState` carries the per-cycle facts; the class body seeds the shared `ZERO_DRAIN` snapshot over `psutil.Process()` with a `None` reading, and a parallel `SyncInstruments` slot is `_seed_sync(metrics.get_meter(METER_NAME))`, minting the bucket-shaped `companion.request.duration` `Histogram` and the `retry.attempts` `Counter` off one no-op-meter resolution (not two `get_meter` calls and not two standalone class attributes), so every callback and the `record`/`observe`/`retry_hook` path reads a fully-formed struct with no live reconstruction. `install` folds `INSTRUMENTS` once against the installed meter — re-seeding `_state` with `ZERO_DRAIN` plus an initial `ProcessReading.sample` and swapping `_sync` with the freshly-created recorders — and the `latched` aspect makes that fold run at most once per process: a second `install` returns the cached `MeterReceipt` `REENTRANT`-stamped without re-entering the body, so the SDK never holds a doubled callback set. `Metrics.observe` builds a fresh `MetricState` folding the new `DrainReceipt[object]`, the lane-threaded `in_flight` saturation count, and a fresh `ProcessReading.sample` against the carried `psutil.Process` handle and publishes it through one atomic `cls._state` reference assignment, the single bytecode store CPython guarantees atomic so the observable callbacks read a fully-formed snapshot with no lock and the serve leg's `observe` never contends the lane's task-group lock; the read path is `cls._state` then field reads off the immutable struct, so a swap that lands mid-export only changes which complete snapshot the next column reads, never tears a partial one. The `psutil.Process` handle is minted at `install` and carried through every swap so the process identity is stable across the run; the drain fold, the `in_flight` saturation count, and the batched reading are the fields that turn over, the reading sampled once per `observe` so the gauge callbacks read a cached fold rather than each firing its own syscall on the export thread.
- Entry: `Metrics.install` is wrapped by the imported `latched(read, write, reentrant)` aspect onto the `_receipt` latch — the `reentrant` closure `replace`-stamping the cached `MeterReceipt` `REENTRANT` so a re-entrant call returns it before the body runs, and a re-admission or test re-bootstrap re-registers no observable callback and re-creates no recorder. The body obtains the installed meter through `metrics.get_meter` (the `observability/telemetry#TELEMETRY` `set_meter_provider` install having already run under an emitting profile; under a `PACKAGE`/`TEST` profile no provider is set, so the read resolves the API no-op meter and the fold mints no-op instruments — the gate is the installed provider, never a profile argument here), then folds `INSTRUMENTS` once through one `Block.fold`: a `histogram` row mints the bucket-shaped `companion.request.duration` recorder and a `counter` row the `retry.attempts` recorder into the swapped `SyncInstruments` carrier, every observable row registers through the `observable: dict[InstrumentKind, ObservableFactory]` dispatch keyed by `spec.kind` so each `create_observable_*` call is one table row rather than a per-instrument call, and seeds `cls._state` with the shared `ZERO_DRAIN` plus an initial `ProcessReading.sample` — returning the `MeterReceipt` carrying the `INSTALLED` outcome, the full `instruments` name tuple, and `DRAIN_COLUMNS`, a bare return the composition root threads to the measured-signal consumers, not a receipt fact minted into `observability/receipts#RECEIPT`; `Metrics.observe` swaps a fresh `DrainReceipt[object]`+`in_flight`+`ProcessReading`-folded snapshot in one atomic reference assignment, never holding the lane's task-group lock; `Metrics.record` records one duration measurement off `cls._sync.duration` keyed by the open `rpc.method` string and the bounded `DrainOutcome` `outcome` attribute under `context=otel_context.get_current()` so the SDK exemplar filter ties the measurement to the active serve span, the histogram outcome axis sharing the one `execution/lanes#LANE`-owned `DrainOutcome` literal `DRAIN_COLUMNS` derives from so the histogram attribute and the counter attribute cannot drift; `Metrics.measured(method)` is the AOP aspect — a `[**P, T]`-parametric decorator over a `Callable[P, Awaitable[RuntimeRail[T]]]` serve coroutine that preserves the wrapped handler's exact call signature (the sibling `latched[R, **P]`/`faults#trapped[**P, T]` convention) rather than an erased `Callable[..., ...]` — it times the call with `perf_counter`, projects the resolved rail's `BoundaryFault.tag` through the `FAULT_OUTCOME` table off `rail.swap().to_option().map(cls._outcome)` (a `deadline` fault landing as `cancelled`, every other fault defaulting to `rejected`, an `Ok` rail to `completed`), records the duration, and returns the rail untouched, so the `transport/serve#SERVE` leg decorates a handler once rather than threading inline timing through every method and the histogram outcome stays total over the closed `FaultTag` rather than a two-valued ok/error collapse that hides a deadline trip; `Metrics.retry_hook` returns a `RetryHook` incrementing `cls._sync.retries` keyed by `RetryDetails.name` and `caused_by` type under `context=otel_context.get_current()` so a retry attempt fired inside a serve span exemplar-correlates to that span exactly as `record` correlates the duration measurement, the metrics-owned instrument the `reliability/resilience#RESILIENCE` owner composes into its one `set_on_retry_hooks` call.
- Auto: the meter is obtained once per process through `metrics.get_meter` and the instruments are created once and reused — never per request, per `LOCAL_ADMISSION` — the `latched` install aspect enforcing the at-most-once mint over the `_receipt` latch so no re-admission folds the table twice; the observable callbacks receive a `CallbackOptions` and return an iterable of `Observation`, reading the latest frozen `MetricState` off one reference load through the one polymorphic `_callback` closing the spec's `Project` over `cls._state` (a `None`-`project` synchronous row yields `()` and never registers a callback) rather than blocking on the live lane; the `lane.in_flight` `ObservableUpDownCounter` reads the `in_flight` field off the same swapped snapshot — the additive, bidirectional in-flight gauge OTel `LOCAL_ADMISSION` names for active-lane/queue-depth saturation, distinct from the monotonic `lane.drained` `ObservableCounter` whose value only ever rises — so a saturation signal rises and falls on one instrument rather than two divergent counters, the `execution/lanes#LANE` `drained` aspect threading `in_flight=receipt.cancelled` into `observe` rather than this owner sampling the lane limiter (the value is the per-drain went-live-but-unresolved cardinality the lane owner computes, never a direct `CapacityLimiter.borrowed_tokens` read this owner is barred from); the process probes read one `ProcessReading` per `observe`, sampled inside a single `Process.oneshot()` block that batches the `memory_full_info`/`cpu_percent`/`num_threads`/`num_fds` syscalls into one collection per the psutil `ONESHOT_TOPOLOGY` rather than one syscall per gauge per export, `cpu_percent(interval=None)` reading the non-blocking since-last-call delta (the first sample the `0.0` seed the psutil contract returns, subsequent samples the true delta over the `observe` cadence) and `num_fds` `hasattr`-guarded for the POSIX-gated method; the drain counter folds the five `DrainReceipt` columns into one instrument keyed by a bounded `DrainOutcome` `outcome` attribute (`accepted`/`completed`/`cancelled`/`rejected`/`hit`, cache-`hit` a first-class counter dimension) rather than five parallel counters, the `DrainOutcome`/`DRAIN_COLUMNS` taxonomy imported from the `execution/lanes#LANE` `DrainReceipt` owner rather than redeclared here so the counter outcome vocabulary is the one typed fact the receipt also reads, the fold reading each count field by the imported `DRAIN_COLUMNS` literal directly off the typed `DrainReceipt` (each column name being a real `DrainReceipt` field, total over the closed `DrainOutcome`) rather than materializing the whole struct — `asdict` over `DrainReceipt` would allocate its `values`/`cache`/`faults` container fields on every export-cycle callback only to drop them, so the five-scalar read stays a typed field access, never a full-struct dict projection; `ProcessReading.sample` runs the one boundary read this owner admits inside `contextlib.suppress(*PROCESS_FAULTS)`, returning `None` when the handle resolves no live process (`NoSuchProcess`/`AccessDenied`/`ZombieProcess`) so a dead-process race drops the whole reading and every process gauge yields `[]` for that cycle rather than raising on the export thread — `suppress` the admitted boundary-kernel form over the `reliability/faults#FAULT` `boundary` rail since the OTel observable-callback contract returning `Iterable[Observation]` forbids a railed `Result`, so the kernel drops to `None`/`[]` rather than crossing a fault as the `boundary` case; the installed `MeterProvider` carries the one shared `Resource` the trace/log providers join so all signals carry one `service.name`; `install` under a no-op meter (a `silent` `observability/telemetry#TELEMETRY` profile) creates API no-op instruments and the callbacks register but never fire, so the cost under a `PACKAGE`/`TEST` profile is exactly the no-op meter.
- Packages: `opentelemetry-api` (`metrics.get_meter` ENTRYPOINTS [01] / `Meter.create_histogram(..., explicit_bucket_boundaries_advisory=)` [03] / `create_counter` [01] / `create_observable_counter` [05] / `create_observable_up_down_counter` [06] / `create_observable_gauge` [07] / `Counter.add(amount, attributes, context)` [08] driving the `retry.attempts` add under the active `context` so the exemplar filter ties it to the serve span / `Histogram.record(amount, attributes, context)` [10] / `context.get_current` context-and-propagation [03], `Observation` PUBLIC_TYPES [10] / `CallbackOptions` [11] / `Counter` [03] / `Histogram` [05] / `Meter` [02]), `psutil` (`Process` PUBLIC_TYPES [01] / `Process.oneshot` batched-read ENTRYPOINTS [01] / `Process.memory_full_info` resource-metrics ENTRYPOINTS [02] / `Process.cpu_percent` [04] / `Process.num_threads` [06] / `Process.num_fds` [08], guarding `NoSuchProcess`/`AccessDenied`/`ZombieProcess` exception types [02]/[04]/[03]), `stamina` (`instrumentation.RetryHook` / `instrumentation.RetryDetails` mapped field-for-field onto the `retry.attempts` counter), `expression` (`Block.of_seq`/`Block.fold` collection-ops ENTRYPOINTS [01]/[04] building and folding the one `INSTRUMENTS` table across both disciplines — the sibling `observability/telemetry#TELEMETRY` `SIGNAL_SPECS.fold` install idiom — plus `Result.swap`/`Result.to_option` result-ops [08]/[06] and `Option.map`/`Option.default_value` option-ops [01]/[07] folding the resolved `RuntimeRail` into the `measured` outcome without a raised branch), `msgspec` (`Struct` with `gc=False` only on the `ProcessReading` non-container leaf per `MSGSPEC_TOPOLOGY`, the `MetricState`/`SyncInstruments` carriers staying tracked frozen structs since they reference structs and instrument objects; `structs.replace` slotting each created recorder into the `SyncInstruments` carrier in the install fold and `REENTRANT`-stamping the cached `MeterReceipt`); the `reliability/faults#FAULT` `BoundaryFault`/`FaultTag` carrier supplies the `measured` aspect's typed outcome discriminant and the `execution/lanes#LANE` `DrainReceipt`/`DrainOutcome`/`DRAIN_COLUMNS` the shared outcome taxonomy. The SDK `MeterProvider`/`PeriodicExportingMetricReader`/`View`/`TraceBasedExemplarFilter`/`Resource.create` and the `OTLPMetricExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here; `set_on_retry_hooks` is `reliability/resilience#RESILIENCE`-owned.
- Growth: a new measured signal is one `InstrumentSpec` row on `INSTRUMENTS`, reached by the existing kind-keyed fold with no new `create_*` call and no second edit — the `MeterReceipt.instruments` tuple derives from the same table, so the row names itself; an observable `observable`/`up_down`/`gauge` row registers through the one `ObservableFactory` dispatch, a synchronous `histogram`/`counter` row lands in `SyncInstruments` through the fold's `replace` arm, and a new instrument family is one `InstrumentKind` member plus one fold arm; a new process probe is one `ProbeField` literal plus one field on `ProcessReading` plus one `_gauge`-projected row, the `oneshot` block already batching its read; a new state-polled saturation gauge is one `up_down` row over one `MetricState` field threaded through `observe`; a new drain dimension is one member added on the `execution/lanes#LANE` `DrainOutcome` taxonomy, reaching this counter through the imported `DRAIN_COLUMNS` fold off the existing `DrainReceipt` with no edit here, never a new counter; a new fault-to-outcome mapping is one `FAULT_OUTCOME` row keyed by `FaultTag` (the unmapped tags already defaulting to `rejected`), never a branch added to the `measured` aspect; a new histogram attribute is one key on the `record` attribute map; a new retry attribute is one key on the `retry_hook` map off `RetryDetails`; zero new surface, one installed `MeterProvider`, one atomic state slot, one install latch.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter/`View`/exemplar construction (the `observability/telemetry#TELEMETRY` install owns it, the histogram shaping the API-level `explicit_bucket_boundaries_advisory` the SDK `View` honors rather than a View minted here), no `set_on_retry_hooks` registration (the `reliability/resilience#RESILIENCE` owner runs the one registration and composes the metrics-owned `retry_hook`), no AppHost telemetry envelope, health status, product metric export, or exporter ownership; `ProcessReading.sample`'s `suppress(*PROCESS_FAULTS)` over the `oneshot` block is the one admitted boundary-kernel exception to the `reliability/faults#FAULT` `boundary` rail — the OTel observable-callback contract forbids a railed return, so the kernel drops to `None`/`[]` rather than minting the `boundary` case, the only raw-except site this owner declares; an SDK import in this owner, a per-request instrument, a per-gauge syscall outside the shared `oneshot` collection, a parallel callback method per process probe where one `Project`-closed `_callback` folds them, a separate `create_*` call per instrument where the one `INSTRUMENTS` fold mints all five families, two standalone synchronous-instrument class slots each constructed twice where the one swapped `SyncInstruments` carrier holds them, an erased `Callable[..., object]` dispatch carrier where the `ObservableFactory` `Protocol` types the create surface, a non-idempotent `install` that re-registers a callback set on a re-bootstrap (firing every observation twice per export) where the `latched` aspect returns the cached `REENTRANT` receipt, a mutated-in-place `MetricState` a callback reads under a torn write, a callback that blocks on the live lane lock, a lock on the observe/read path where the atomic reference swap suffices, an unguarded `psutil` read raising on the export thread, a second on-retry hook registration trampling the resilience owner, a monotonic `ObservableCounter` for the in-flight saturation signal where the additive `ObservableUpDownCounter` rises and falls, a two-valued ok/error bool collapsing the `measured` outcome where the `FAULT_OUTCOME` projection keeps `cancelled` distinct from `rejected` over the closed `FaultTag`, and five parallel drain counters where one attribute-keyed counter folds them are the deleted forms; the `in_flight` saturation count stays the `execution/lanes#LANE` `drained` aspect's to compute and thread into `observe` (this owner reads the threaded snapshot, never the lane's `CapacityLimiter` directly), and the suite metric taxonomy stays AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Sequence
from contextlib import suppress
from enum import StrEnum
from functools import wraps
from time import perf_counter
from typing import ClassVar, Final, Literal, Protocol

import psutil
from expression import Block
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import context as otel_context
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Counter, Histogram, Meter, Observation
from stamina.instrumentation import RetryDetails, RetryHook

from rasm.runtime.faults import BoundaryFault, FaultTag, RuntimeRail
from rasm.runtime.lanes import DRAIN_COLUMNS, DrainOutcome, DrainReceipt
from rasm.runtime.telemetry import latched

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

METER_NAME: Final = "rasm.runtime"

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (
    psutil.NoSuchProcess,
    psutil.ZombieProcess,
    psutil.AccessDenied,
)

# request-duration bucket advisory (ms); the SDK View honors it, View ownership stays telemetry.
DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

# serve-rail FaultTag -> DrainOutcome projection: `deadline` lands `cancelled`, every other
# tag defaults `rejected` through `dict.get`, an Ok rail folds `completed` at the call site.
FAULT_OUTCOME: Final[dict[FaultTag, DrainOutcome]] = {"deadline": "cancelled"}

# the all-zero seed the class body and the install re-seed publish before the first `observe`,
# so the `lane.drained` callback reads a complete fold from process start.
ZERO_DRAIN: Final[DrainReceipt[object]] = DrainReceipt(accepted=0, completed=0, cancelled=0, rejected=0)

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


# one row per instrument: an observable row carries its `project` callback-pull closure, a
# synchronous row carries `project=None` and slots into `SyncInstruments` by name; `advisory`
# rides only the histogram row.
class InstrumentSpec(Struct, frozen=True):
    name: str
    kind: InstrumentKind
    unit: str
    project: Project | None = None
    advisory: tuple[float, ...] | None = None


# the two held synchronous recorders, swapped as one frozen carrier (the `MetricState`
# atomic-reference idiom). No `gc=False`: it holds instrument references, not a leaf.
class SyncInstruments(Struct, frozen=True):
    duration: Histogram
    retries: Counter


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


# both synchronous recorders mint off one `get_meter` resolution into the swapped carrier.
def _seed_sync(meter: Meter) -> SyncInstruments:
    return SyncInstruments(
        meter.create_histogram(
            "companion.request.duration", unit="ms", explicit_bucket_boundaries_advisory=DURATION_BUCKETS_MS
        ),
        meter.create_counter("retry.attempts", unit="1"),
    )


# the one table `install` creates from and `MeterReceipt.instruments` names; the two leading
# synchronous rows are imperative recorders, the trailing observable rows pull from `_state`.
INSTRUMENTS: Final[Block[InstrumentSpec]] = Block.of_seq([
    InstrumentSpec("companion.request.duration", InstrumentKind.HISTOGRAM, "ms", advisory=DURATION_BUCKETS_MS),
    InstrumentSpec("retry.attempts", InstrumentKind.COUNTER, "1"),
    InstrumentSpec("lane.drained", InstrumentKind.OBSERVABLE, "1", _drain_fold),
    InstrumentSpec("lane.in_flight", InstrumentKind.UP_DOWN, "1", _inflight),
    InstrumentSpec("process.memory.rss", InstrumentKind.GAUGE, "By", _gauge("rss")),
    InstrumentSpec("process.memory.uss", InstrumentKind.GAUGE, "By", _gauge("uss")),
    InstrumentSpec("process.cpu.utilization", InstrumentKind.GAUGE, "1", _gauge("cpu")),
    InstrumentSpec("process.thread.count", InstrumentKind.GAUGE, "1", _gauge("threads")),
    InstrumentSpec("process.fd.count", InstrumentKind.GAUGE, "1", _gauge("fds")),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    _state: ClassVar[MetricState] = MetricState(ZERO_DRAIN, psutil.Process(), None)
    _sync: ClassVar[SyncInstruments] = _seed_sync(metrics.get_meter(METER_NAME))
    _receipt: ClassVar[MeterReceipt | None] = None

    @classmethod
    @latched(
        lambda: Metrics._receipt,
        lambda r: setattr(Metrics, "_receipt", r),
        lambda prior: replace(prior, outcome=MeterOutcome.REENTRANT),
    )
    def install(cls) -> MeterReceipt:
        meter = metrics.get_meter(METER_NAME)
        process = psutil.Process()
        observable: dict[InstrumentKind, ObservableFactory] = {
            InstrumentKind.OBSERVABLE: meter.create_observable_counter,
            InstrumentKind.UP_DOWN: meter.create_observable_up_down_counter,
            InstrumentKind.GAUGE: meter.create_observable_gauge,
        }

        def mint(sync: SyncInstruments, spec: InstrumentSpec) -> SyncInstruments:
            match spec.kind:
                case InstrumentKind.HISTOGRAM:
                    return replace(sync, duration=meter.create_histogram(
                        spec.name, unit=spec.unit, explicit_bucket_boundaries_advisory=spec.advisory
                    ))
                case InstrumentKind.COUNTER:
                    return replace(sync, retries=meter.create_counter(spec.name, unit=spec.unit))
                case kind:
                    observable[kind](spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)
                    return sync

        cls._state = MetricState(ZERO_DRAIN, process, ProcessReading.sample(process))
        cls._sync = INSTRUMENTS.fold(mint, cls._sync)
        return MeterReceipt(
            MeterOutcome.INSTALLED, tuple(spec.name for spec in INSTRUMENTS), DRAIN_COLUMNS
        )

    @classmethod
    def observe(cls, drain: DrainReceipt[object], in_flight: int = 0) -> None:
        process = cls._state.process
        cls._state = MetricState(drain, process, ProcessReading.sample(process), in_flight)

    @classmethod
    def record(cls, duration_ms: float, method: str, outcome: DrainOutcome) -> None:
        cls._sync.duration.record(
            duration_ms, {"rpc.method": method, "outcome": outcome}, context=otel_context.get_current()
        )

    @classmethod
    def retry_hook(cls) -> RetryHook:
        def hook(details: RetryDetails) -> None:
            cls._sync.retries.add(
                1,
                {"target": details.name, "cause": type(details.caused_by).__qualname__},
                context=otel_context.get_current(),
            )

        return hook

    @classmethod
    def measured[**P, T](
        cls, method: str
    ) -> Callable[[Callable[P, Awaitable[RuntimeRail[T]]]], Callable[P, Awaitable[RuntimeRail[T]]]]:
        def aspect(serve: Callable[P, Awaitable[RuntimeRail[T]]]) -> Callable[P, Awaitable[RuntimeRail[T]]]:
            @wraps(serve)
            async def measured_serve(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
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
        return lambda _: () if spec.project is None else spec.project(cls._state)
```

## [03]-[RESEARCH]

- [METRIC_INSTRUMENT_TABLE]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `metrics.get_meter(name, ...)` (ENTRYPOINTS [01]) reads the `observability/telemetry#TELEMETRY`-installed `MeterProvider` (no-op until `set_meter_provider`, so a `silent` profile yields no-op instruments), `Meter.create_histogram(name, unit, explicit_bucket_boundaries_advisory=)` (ENTRYPOINTS [03]) mints the synchronous `companion.request.duration` recorder with an API-level bucket advisory the serve leg drives through `Histogram.record(amount, attributes, context)` (ENTRYPOINTS [10]), `Meter.create_counter` (ENTRYPOINTS [01]) mints the `retry.attempts` recorder driven through `Counter.add(amount, attributes, context)` (ENTRYPOINTS [08]), and `create_observable_counter`/`create_observable_up_down_counter`/`create_observable_gauge(name, callbacks, unit)` (ENTRYPOINTS [05]/[06]/[07]) register callbacks the `PeriodicExportingMetricReader` invokes per export cycle with a `CallbackOptions` (PUBLIC_TYPES [11]) returning `Iterable[Observation]` (PUBLIC_TYPES [10]). The dense form is one `INSTRUMENTS` `Block[InstrumentSpec]` table spanning both disciplines — each row carrying name/kind/unit, an optional `Project` closure (observable rows) and an optional bucket `advisory` (the histogram row) — folded in `install` through one `Block.fold` whose arm matches `spec.kind`: a `histogram`/`counter` row mints a held recorder into the swapped `SyncInstruments` carrier through `msgspec.structs.replace`, and an `observable`/`up_down`/`gauge` row registers through one `observable: dict[InstrumentKind, ObservableFactory]` dispatch keyed by `spec.kind`, with one polymorphic `_callback` closing each spec's `Project` over `cls._state`. This collapses the prior shape on two axes — the three sibling callback classmethods (`_drained`/`_rss`/`_cpu`) into one table-driven `_callback`, and the five sibling `create_*` factory calls (two synchronous + three observable) into one kind-keyed fold — so the five OTel instrument families mint from one table, the create surface is typed against the structural `ObservableFactory` `Protocol` (`(name, *, callbacks, unit) -> object`) rather than an erased `Callable[..., object]`, and a new signal is one row reaching both the create fold and the `MeterReceipt.instruments` projection. The `lane.in_flight` `ObservableUpDownCounter` is the additive in-flight gauge the `.api/opentelemetry-api.md` `LOCAL_ADMISSION` (`UpDownCounter`/`Observable*` selection guidance) names for queue-depth/active-lane saturation — it reads the `MetricState.in_flight` field the lane's `drained` aspect threads in as `in_flight=receipt.cancelled` (the per-drain went-live-but-unresolved cardinality, never a direct `CapacityLimiter` read), distinct from the monotonic `lane.drained` `ObservableCounter`, so the saturation signal rises and falls on one bidirectional instrument rather than a counter that can only climb. The `explicit_bucket_boundaries_advisory` is the API-surfaced bucket hint the SDK `ExplicitBucketHistogramAggregation` View honors per `libs/python/.api/opentelemetry-sdk.md` (ENTRYPOINTS [07]) without minting a View in this owner, and the `context=otel_context.get_current()` on `record` is the active-context handle the SDK `TraceBasedExemplarFilter` (PUBLIC_TYPES [06]) samples to tie each duration measurement to its serve span. Instruments are created once per meter and reused per the `LOCAL_ADMISSION`. The instrument-table spellings are settled.
- [ATOMIC_STATE_SWAP]: the observable callbacks run on the SDK export thread the `PeriodicExportingMetricReader` drives while the serve leg's `Metrics.observe` runs on the lane's event loop — the cross-thread read demands the snapshot be published atomically, not mutated field-by-field. The frozen `MetricState` is built complete then published through a single `cls._state = ...` class-attribute store, the one bytecode `STORE_NAME`/`STORE_ATTR` CPython executes indivisibly under the GIL, so a callback's `state = cls._state` load reads either the prior or the next complete snapshot and never a half-written one; no lock guards either the swap or the read, the immutability the correctness guarantee rather than a mutex. `gc=False` rides only the `ProcessReading` leaf — the one struct holding exclusively `int`/`float` fields the `libs/python/.api/msgspec.md` `MSGSPEC_TOPOLOGY` non-container-leaf rule admits out of the tracked GC set; `MetricState` references the `DrainReceipt[object]` struct and a `ProcessReading`, so it stays a tracked frozen struct. The `psutil.Process` handle is invariant across swaps; the `DrainReceipt[object]` fold and the batched `ProcessReading` are the fields that turn over, so the gauge callbacks read a stable process identity and a cached reading while the counter reads the freshest fold. The atomic-swap shape is settled.
- [PROCESS_ONESHOT_BATCH]: confirmed against `libs/python/.api/psutil.md` `ONESHOT_TOPOLOGY`/`LOCAL_ADMISSION` — `ProcessReading.sample` folds `Process.memory_full_info()` (resource-metrics ENTRYPOINTS [02], carrying `rss` and the macOS/Linux `uss`), `cpu_percent(interval=None)` ([04]), `num_threads()` ([06]), and the POSIX-gated `num_fds()` ([08], `hasattr`-guarded per `PLATFORM_GATING`) inside one `Process.oneshot()` (batched-read ENTRYPOINTS [01]) so the cluster of reads costs one collection per `observe` rather than one syscall per gauge per export cycle — the canonical batch path the psutil `INTEGRATION_LAW` names for the OTel observable-gauge hand-off. `cpu_percent(interval=None)` returns `0.0` on the first sample with the true delta on subsequent calls over the `observe` cadence, so the timing window rides the sampling cadence without the callback ever blocking. The whole `oneshot` block runs inside `contextlib.suppress(*PROCESS_FAULTS)` so a dead-process race between handle mint and sample (`NoSuchProcess`/`ZombieProcess`/`AccessDenied`, PUBLIC_TYPES [02]/[03]/[04]) drops the reading to `None` and every process gauge yields `[]` for that cycle rather than raising on the export thread — the one boundary read this owner admits, the guard the psutil `LOCAL_ADMISSION` requires. The batched-reading spellings are settled.
- [RETRY_HOOK_COMPOSITION]: reflection-confirmed against `libs/python/runtime/.api/stamina.md` — the `retry.attempts` `Counter` is metrics-owned, but the on-retry registration is `reliability/resilience#RESILIENCE`-owned: that owner runs the one `instrumentation.set_on_retry_hooks` call (the hooks are process-global, so a second registration here would overwrite or duplicate it). `Metrics.retry_hook` returns a `RetryHook` (a `(RetryDetails) -> None` callable per the instrumentation PUBLIC_TYPES) that the resilience owner composes into its hook tuple alongside `StructlogOnRetryHook`, folding `RetryDetails.name`/`caused_by` field-for-field onto the counter attributes per the `RetryDetails` fact-carrier contract and adding under `context=otel_context.get_current()` (`Counter.add` ENTRYPOINTS [08]) so a retry attempt fired inside a serve span exemplar-correlates to that span exactly as `record` ties the duration measurement — the same `TraceBasedExemplarFilter` hand-off, now total across both synchronous recorders rather than the histogram alone — so retry exhaustion is a first-class metric without a second hook owner. The composed-hook seam is settled.
- [INSTALL_LATCH]: the observable `create_*` calls register process-global callbacks the SDK `PeriodicExportingMetricReader` pulls every cycle, so a second `install` (a re-admission, a re-entrant composition root, a test re-bootstrap) without an idempotency guard would register the callback set twice and the reader would fire every `Observation` twice per export — the exact hazard the sibling `observability/telemetry#TELEMETRY` `latched` aspect defends for the provider trio. `install` is therefore wrapped by that owner's receipt-agnostic `latched[R, **P](read, write, reentrant)` decorator imported directly (`from rasm.runtime.telemetry import latched`, no local redefinition), closed over the `(read, write)` accessor pair onto the `_receipt` latch (not a hardcoded `Metrics._receipt` reference) and the `reentrant = lambda prior: replace(prior, outcome=MeterOutcome.REENTRANT)` restamp closure, so the `[R, **P]` parametricity is real and the aspect carries no `MeterReceipt`-pinned `case MeterReceipt()` class pattern — the exact form telemetry names a deleted re-pinning. It matches `read()` `None` versus a `prior` receipt and on re-entry returns `reentrant(prior)` before the fold body runs, writing the latch as the single terminal store on first install — one cross-cutting decorator genuinely shared with the telemetry owner (one definition, two latches), not a forked guard, and the at-most-once mint CPython's atomic class-attribute store already guarantees on the read side. The held synchronous recorders collapse into one `SyncInstruments` frozen carrier (`duration` `Histogram`, `retries` `Counter`) seeded by `_seed_sync(meter)` — one `get_meter` resolution minting both recorders off the no-op API meter at class body, the installed meter in the fold — and swapped as one reference in the install fold — the sibling `MetricState`/`InstalledProviders` atomic-swap idiom — rather than two standalone class attributes each resolving its own meter and constructed twice, so the synchronous and observable instruments share one create fold and the imperative `record`/`retry_hook` path reads one carrier. The latch and carrier spellings are settled.
- [MEASURED_OUTCOME_PROJECTION]: reflection-confirmed against `libs/python/.api/expression.md` (result-ops `Result.swap`/`Result.to_option` [08]/[06], option-ops `Option.map`/`Option.default_value` [01]/[07]) and the `reliability/faults#FAULT` `BoundaryFault`/`FaultTag` owner — the `measured` aspect derives the duration-histogram `outcome` attribute from the resolved `RuntimeRail` rather than a two-valued ok/error bool: `rail.swap().to_option().map(cls._outcome).default_value("completed")` inverts the rail so an `Error` surfaces its `BoundaryFault`, `_outcome` reads `fault.tag` and indexes the `FAULT_OUTCOME` `dict[FaultTag, DrainOutcome]` table (`deadline -> cancelled`, every other tag defaulting to `rejected` through `dict.get`), and an `Ok` rail folds to `completed` via the `Option.default_value` fallback. This makes the histogram outcome total over the closed `FaultTag` and shares the one `execution/lanes#LANE` `DrainOutcome` axis the `lane.drained` counter keys by — a deadline-tripped serve coroutine surfaces as `cancelled` (matching the lane drain's `cancelled` deadline-containment column) rather than collapsing into `rejected`, so the duration histogram and the drain counter agree on the outcome vocabulary and a deadline trip is distinguishable from a domain rejection in the latency distribution. The expression result-fold avoids a raised branch: the rail is never unwrapped through an exception, only through the `swap`/`to_option`/`map`/`default_value` monadic chain. The outcome-projection spelling is settled. No open RESEARCH seam remains on this page.
```

