# [PY_RUNTIME_METRICS]

`Metrics` is the async-observable metric spine, registering the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider` — it constructs no provider, reader, or exporter, minting its meter once from the `reliability/faults#FAULT` `SCOPES[Scope.METER]` row — and one `INSTRUMENTS` table owns both instrument disciplines and every derived surface, so a new signal is one row. Install custody is two-tier: per-composition `MeterReceipt`s key by the receipts-owned `ScopeKey` — a same-scope re-bootstrap returns its cached receipt stamped `REENTRANT`, a second composition after enrollment receives `ADOPTED` — while the imported `latched` guards the one process enrollment, so the SDK never holds a doubled callback set.

Instruments read frozen `MetricState` snapshots keyed per composition scope and swapped under one atomic reference store, so the export thread reads each scope's latest complete fold and the serve leg publishes lock-free; a non-default scope's observations carry a `composition` attribute, the default scope staying attribute-free exactly as the tenant law spells absence. Every recording path threads `context=` because Python exemplars attach only from a supplied span context, and the telemetry install wires `exemplar_filter=TraceBasedExemplarFilter()` — without it the SDK default drops every exemplar. Names spell `rasm.<domain>.<measure>` with UCUM units and no pre-baked `_total` or unit suffix, scope stays the package id off the `reliability/faults#FAULT` `SCOPES[Scope.METER]` row, and the `schema_url` pin is the telemetry owner's. Tenant folds on as the `rasm.tenant` baggage entry the W3C composite carries; a single-tenant process carries no entry. Retry attempts ride the metrics-owned `retry_hook` the `reliability/resilience#RESILIENCE` owner registers, the drain taxonomy imports from `observability/receipts#RECEIPT`, and provider install, `View`/exemplar machinery, product export, and health stay telemetry-, resilience-, and AppHost-owned.

## [01]-[INDEX]

- [01]-[METRIC]: the one `INSTRUMENTS` table, the scope-keyed atomic-swap `MetricState` snapshots, the two-tier `latched` install custody, the polymorphic `record`, the composed `retry_hook`, and the `measured` serve aspect.
- [02]-[INSTRUMENTATION]: the composition-root instrumentor train over the contrib packages, the generic PEP-249 `DbapiSeam` wrap arm, and the system-metrics slice ruling.

## [02]-[METRIC]

- Owner: `INSTRUMENTS` is the one table every derived surface reads — the `slot` rows mint the `SyncInstruments` carrier by comprehension, the `slot=None` rows register through the typed `ObservableFactory` dispatch, the `domain` rows derive `_DOMAIN_SLOT`, and `MeterReceipt.instruments` names the same rows — no per-instrument `create_*` call, seed function, or name-to-field map beside it. `latched` imports from `reliability/faults#FAULT` and guards the process enrollment `_enrolled` beside the telemetry owner's pipeline latch — one definition, its `reentrant` closure stamping `ADOPTED` for a later composition — while the per-scope latch is the `_receipts` map fold, never a re-pinned local guard.
- Entry: under a `PACKAGE`/`TEST` profile no provider is set, so `get_meter` resolves the API no-op meter and the fold mints no-op instruments — the gate is the installed provider, never a profile argument here. `record` is one polymorphic entrypoint: a scalar records the request-duration histogram, a `Mapping` records each named measure onto its `_DOMAIN_SLOT`-resolved row — the artifacts emit-harvest seam records under `domain="artifact"`, the data query-receipt projection under `domain="query"`, the geometry evidence weave under `domain="geometry"`, the bench family under `domain="bench"` — both arms under the active context so every measurement exemplar-correlates to its span, and both arms fold the `rasm.tenant` baggage entry onto the attributes through `_attributed`. `measured` folds a resolved rail through `FAULT_OUTCOME` at one site — `deadline` lands `cancelled`, every other fault `rejected`, an `Ok` rail `completed` — never a lossy ok/error bool per handler.
- Auto: the frozen carriers turn over under atomic reference stores — a callback load reads either the prior or the next complete snapshot, immutability the correctness guarantee rather than a mutex — and every observable callback folds across the scope-keyed state map, stamping `composition` on a non-default scope's observations so two embedded compositions' drains stay distinguishable on one instrument set. Its `psutil.Process` handle mints at install and carries through every swap, and the reading samples once per `observe`, so gauge callbacks read a cached fold rather than firing syscalls on the export thread; `cpu_percent(interval=None)` is the non-blocking since-last-call delta, the first sample the `0.0` seed. `rasm.lane.in_flight` carries the lane-computed went-live-but-unresolved cardinality — this owner never reads the lane's `CapacityLimiter` directly. `ProcessReading.sample`'s `suppress` is the one admitted raw-except site: the OTel observable-callback contract returns `Iterable[Observation]` and forbids a railed `Result`, so a dead-process race drops the reading and the gauges yield empty for that cycle; its vanished-process fence rides the receipts-owned `PROCESS_FAULTS` tuple, never a second local mint.
- Growth: a new measured signal is one `InstrumentSpec` row — a synchronous row adds its `SyncInstruments` field, a domain row carries `domain`, an observable row registers through the one dispatch — and the receipt names itself from the table; a new instrument family is one `InstrumentKind` member and one dispatch entry; a new process probe one `ProbeField` literal with its `ProcessReading` field and `_gauge` row inside the batched `oneshot`; a new drain dimension one `DrainOutcome` member at the receipts owner, reaching this counter through the imported `DRAIN_COLUMNS` with no edit here; a new fault-to-outcome mapping one `FAULT_OUTCOME` row, unmapped tags defaulting `rejected`; a new metric dimension one attribute in the `_attributed` fold; a new composition one `ScopeKey` value threaded through `install`/`observe`'s `scope` keyword.
- Boundary: no second `MeterProvider`, no SDK provider/reader/exporter/`View`/exemplar construction, no `set_on_retry_hooks` registration, and no AppHost telemetry envelope, health status, or product export — histogram wire shape is the telemetry owner's base2-exponential `WIRE_AGGREGATION` default, the advisory rows here staying the explicit-shape fallback a deployment `View` re-arms, and `View` ownership stays telemetry's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping, Sequence
from contextlib import suppress
from enum import StrEnum
from functools import wraps
from time import perf_counter
from types import ModuleType
from typing import ClassVar, Final, Literal, Protocol, overload

import psutil
from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import baggage
from opentelemetry import context as otel_context
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Counter, Histogram, Meter, Observation
from stamina.instrumentation import RetryDetails, RetryHook

from rasm.runtime.faults import SCOPES, BoundaryFault, FaultTag, RuntimeRail, Scope, latched
from rasm.runtime.receipts import DEFAULT_SCOPE, DRAIN_COLUMNS, PROCESS_FAULTS, DrainOutcome, DrainReceipt, ScopeKey

lazy from opentelemetry.instrumentation.asyncio import AsyncioInstrumentor  # train rows reify on first install, never at import
lazy from opentelemetry.instrumentation.dbapi import instrument_connection, wrap_connect  # generic PEP-249 seam, reified at the first dbapi wrap
lazy from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
lazy from opentelemetry.instrumentation.jinja2 import Jinja2Instrumentor
lazy from opentelemetry.instrumentation.psycopg import PsycopgInstrumentor
lazy from opentelemetry.instrumentation.sqlite3 import SQLite3Instrumentor
lazy from opentelemetry.instrumentation.system_metrics import SystemMetricsInstrumentor
lazy from opentelemetry.instrumentation.threading import ThreadingInstrumentor

# --- [TYPES] ----------------------------------------------------------------------------

type ProbeField = Literal["rss", "uss", "cpu", "threads", "fds"]
type Project = Callable[["MetricState"], Iterable[Observation]]
type ObservableCallback = Callable[[CallbackOptions], Iterable[Observation]]


# create-dispatch discriminant: two synchronous recorders + three observable kinds.
class InstrumentKind(StrEnum):
    HISTOGRAM = "histogram"
    COUNTER = "counter"
    OBSERVABLE = "observable"
    UP_DOWN = "up_down"
    GAUGE = "gauge"


# structural create signature the three `create_observable_*` methods satisfy, so the
# kind-keyed dispatch is typed rather than an erased `Callable[..., object]`.
class ObservableFactory(Protocol):
    def __call__(self, name: str, *, callbacks: Sequence[ObservableCallback], unit: str) -> object: ...


# structural port over the contrib instrumentor family; keeps the TRAIN rows typed with zero eager contrib import.
class Instrumentor(Protocol):
    def instrument(self, **kwargs: object) -> None: ...


# INSTALLED enrolled the process instrument set; REENTRANT is a same-scope re-install; ADOPTED is a later
# composition riding the standing enrollment with its own state slot and receipt.
class MeterOutcome(StrEnum):
    INSTALLED = "installed"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


# --- [CONSTANTS] ------------------------------------------------------------------------

# request-duration bucket advisory (ms) — the explicit-shape fallback: inert under telemetry's base2-exponential wire
# default, it supplies the boundaries where a deployment View re-selects the explicit aggregation; View ownership stays telemetry.
DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

# tenant dimension: the W3C Baggage entry the C#-parented context carries; absent entry = single-tenant, no attribute.
TENANT_BAGGAGE: Final[str] = "rasm.tenant"

# serve-rail FaultTag -> DrainOutcome projection: `deadline` lands `cancelled`, every other
# tag defaults `rejected` through the `try_find` fold, an Ok rail folds `completed` at the call site.
FAULT_OUTCOME: Final[Map[FaultTag, DrainOutcome]] = Map.of_seq([("deadline", "cancelled")])

# all-zero seed published before the first `observe`; `hit=0` is spelled so the seed carries every DRAIN_COLUMNS column explicitly.
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


# `slot` names the SyncInstruments field a synchronous row lands in (None marks an observable row carrying `project`), `advisory` rides
# only histogram rows, `domain` marks a domain-distribution row `record`'s mapping arm reaches by name.
class InstrumentSpec(Struct, frozen=True):
    name: str
    kind: InstrumentKind
    unit: str
    project: Project | None = None
    advisory: tuple[float, ...] | None = None
    slot: str | None = None
    domain: str | None = None


# held synchronous recorders, swapped as one frozen carrier (the `MetricState`
# atomic-reference idiom). No `gc=False`: it holds instrument references, not a leaf.
class SyncInstruments(Struct, frozen=True):
    duration: Histogram
    retries: Counter
    artifact_bytes: Histogram
    artifact_ratio: Histogram
    query_duration: Histogram
    query_rows: Histogram
    egress_bytes: Histogram
    quality_breach: Histogram
    impact_score: Histogram
    graph_nodes: Histogram
    graph_edges: Histogram
    materialize_rows: Histogram
    geometry_duration: Histogram
    bench_duration: Histogram
    bench_throughput: Histogram


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


# one table every derived surface reads: sync rows land by `slot`, observable rows pull
# from `_state` through `project`, `domain` rows feed `record`'s mapping arm.
# names spell rasm.<domain>.<measure> with UCUM units and no pre-baked suffixes — the translation layer appends them.
INSTRUMENTS: Final[Block[InstrumentSpec]] = Block.of_seq([
    InstrumentSpec("rasm.serve.request.duration", InstrumentKind.HISTOGRAM, "ms", advisory=DURATION_BUCKETS_MS, slot="duration"),
    InstrumentSpec("rasm.retry.attempts", InstrumentKind.COUNTER, "1", slot="retries"),
    InstrumentSpec("rasm.artifact.byte_volume", InstrumentKind.HISTOGRAM, "By", slot="artifact_bytes", domain="artifact"),
    InstrumentSpec("rasm.artifact.compression_ratio", InstrumentKind.HISTOGRAM, "1", slot="artifact_ratio", domain="artifact"),
    InstrumentSpec("rasm.query.engine.duration", InstrumentKind.HISTOGRAM, "ms", slot="query_duration", domain="query"),
    InstrumentSpec("rasm.query.rows", InstrumentKind.HISTOGRAM, "{row}", slot="query_rows", domain="query"),
    InstrumentSpec("rasm.egress.byte_volume", InstrumentKind.HISTOGRAM, "By", slot="egress_bytes", domain="egress"),
    InstrumentSpec("rasm.quality.breach_fraction", InstrumentKind.HISTOGRAM, "1", slot="quality_breach", domain="quality"),
    InstrumentSpec("rasm.impact.score", InstrumentKind.HISTOGRAM, "kg", slot="impact_score", domain="impact"),
    InstrumentSpec("rasm.graph.nodes", InstrumentKind.HISTOGRAM, "{node}", slot="graph_nodes", domain="graph"),
    InstrumentSpec("rasm.graph.edges", InstrumentKind.HISTOGRAM, "{edge}", slot="graph_edges", domain="graph"),
    InstrumentSpec("rasm.materialize.rows", InstrumentKind.HISTOGRAM, "{row}", slot="materialize_rows", domain="materialize"),
    InstrumentSpec("rasm.geometry.evidence.duration", InstrumentKind.HISTOGRAM, "ms", slot="geometry_duration", domain="geometry"),
    InstrumentSpec("rasm.bench.duration", InstrumentKind.HISTOGRAM, "ms", slot="bench_duration", domain="bench"),
    InstrumentSpec("rasm.bench.throughput", InstrumentKind.HISTOGRAM, "1/s", slot="bench_throughput", domain="bench"),
    InstrumentSpec("rasm.lane.drained", InstrumentKind.OBSERVABLE, "1", _drain_fold),
    InstrumentSpec("rasm.lane.in_flight", InstrumentKind.UP_DOWN, "1", _inflight),
    InstrumentSpec("rasm.process.memory.rss", InstrumentKind.GAUGE, "By", _gauge("rss")),
    InstrumentSpec("rasm.process.memory.uss", InstrumentKind.GAUGE, "By", _gauge("uss")),
    InstrumentSpec("rasm.process.cpu.utilization", InstrumentKind.GAUGE, "1", _gauge("cpu")),
    InstrumentSpec("rasm.process.thread.count", InstrumentKind.GAUGE, "1", _gauge("threads")),
    InstrumentSpec("rasm.process.fd.count", InstrumentKind.GAUGE, "1", _gauge("fds")),
])

# instrument-name -> SyncInstruments field for the domain rows, derived from the one table so a
# new domain distribution is one row + one carrier field, never an edit to `record`.
_DOMAIN_SLOT: Final[Map[str, str]] = Map.of_seq([
    (spec.name, slot) for spec in INSTRUMENTS if spec.domain is not None and (slot := spec.slot) is not None
])


# synchronous carrier DERIVES from the table's `slot` rows against one meter resolution.
def _sync_carrier(meter: Meter) -> SyncInstruments:
    return SyncInstruments(**{slot: _recorder(meter, spec) for spec in INSTRUMENTS if (slot := spec.slot) is not None})


# one attribute fold every recording path shares: the rasm.tenant baggage entry joins when the parented context carries it.
def _attributed(base: Mapping[str, object], context: "otel_context.Context") -> dict[str, object]:
    tenant = baggage.get_baggage(TENANT_BAGGAGE, context)
    return {**base, "tenant": tenant} if isinstance(tenant, str) and tenant else dict(base)


# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    # two-tier custody: `_state`/`_receipts` key per-composition snapshots and evidence by ScopeKey; `_sync` and the
    # observable enrollment are the process pipeline the `latched` `_enrolled` guards — instruments are SDK process
    # singletons, so a doubled callback set stays structurally impossible while every composition owns its state slot.
    _state: ClassVar[Map[ScopeKey, MetricState]] = Map.of_seq([(DEFAULT_SCOPE, MetricState(ZERO_DRAIN, psutil.Process(), None))])
    _sync: ClassVar[SyncInstruments] = _sync_carrier(metrics.get_meter(SCOPES[Scope.METER]))
    _receipts: ClassVar[Map[ScopeKey, MeterReceipt]] = Map.empty()
    _process: ClassVar[MeterReceipt | None] = None

    @classmethod
    @latched(lambda: Metrics._process, lambda r: setattr(Metrics, "_process", r), lambda prior: replace(prior, outcome=MeterOutcome.ADOPTED))
    def _enrolled(cls) -> MeterReceipt:
        meter = metrics.get_meter(SCOPES[Scope.METER])
        observable: dict[InstrumentKind, ObservableFactory] = {
            InstrumentKind.OBSERVABLE: meter.create_observable_counter,
            InstrumentKind.UP_DOWN: meter.create_observable_up_down_counter,
            InstrumentKind.GAUGE: meter.create_observable_gauge,
        }

        def enroll(_: None, spec: InstrumentSpec) -> None:
            observable[spec.kind](spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)

        INSTRUMENTS.filter(lambda spec: spec.slot is None).fold(enroll, None)
        cls._sync = _sync_carrier(meter)
        return MeterReceipt(MeterOutcome.INSTALLED, tuple(spec.name for spec in INSTRUMENTS), DRAIN_COLUMNS)

    @classmethod
    def install(cls, scope: ScopeKey = DEFAULT_SCOPE) -> MeterReceipt:
        match cls._receipts.try_find(scope):
            case Option(tag="some", some=prior):
                return replace(prior, outcome=MeterOutcome.REENTRANT)
            case _:
                receipt = cls._enrolled()
                process = psutil.Process()
                cls._state = cls._state.add(scope, MetricState(ZERO_DRAIN, process, ProcessReading.sample(process)))
                cls._receipts = cls._receipts.add(scope, receipt)
                return receipt

    @classmethod
    def observe(cls, drain: DrainReceipt[object], in_flight: int = 0, *, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        held = cls._state.try_find(scope).default_with(lambda: MetricState(ZERO_DRAIN, psutil.Process(), None))
        cls._state = cls._state.add(scope, MetricState(drain, held.process, ProcessReading.sample(held.process), in_flight))

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
                context = otel_context.get_current()
                attributes = _attributed({domain: kind}, context)
                Block.of_seq(measures.items()).fold(
                    lambda _, kv: getattr(cls._sync, _DOMAIN_SLOT[kv[0]]).record(kv[1], attributes, context=context), None
                )
            case amount:
                context = otel_context.get_current()
                cls._sync.duration.record(amount, _attributed({"rpc.method": method, "outcome": outcome}, context), context=context)

    @classmethod
    def retry_hook(cls) -> RetryHook:
        def hook(details: RetryDetails) -> None:
            context = otel_context.get_current()
            attributes = _attributed({"target": details.name, "cause": type(details.caused_by).__qualname__}, context)
            cls._sync.retries.add(1, attributes, context=context)

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
        # one callback folds every composition's snapshot: the default scope's observations pass untouched (today's
        # shape), a non-default scope's re-wrap with the `composition` attribute so embedded compositions never merge.
        def observed(_: CallbackOptions) -> Iterable[Observation]:
            match spec.project:
                case None:
                    return ()
                case project:
                    return [
                        obs if scope == DEFAULT_SCOPE else Observation(obs.value, {**dict(obs.attributes or {}), "composition": scope})
                        for scope, state in cls._state.items()
                        for obs in project(state)
                    ]

        return observed
```

## [03]-[INSTRUMENTATION]

- Owner: `Instrumentation.install` activates the contrib instrumentor train once — one `TRAIN` table of thunk rows over the module-scope `lazy from` imports, so the cold contrib modules reify on first install, never at import, and a table row never dereferences a lazy proxy at module scope. `Instrumentation.dbapi` is the generic PEP-249 arm beside it: one `DbapiSeam` row names the driver module, connect callable, and `db.system` token, and one polymorphic entry either patches the connect callable forward through `wrap_connect` or retrofits a pre-patch live connection through `instrument_connection`, discriminating on whether a connection is handed in.
- Cases: the DBAPI rows (`PsycopgInstrumentor`, `SQLite3Instrumentor`) patch the drivers the data query surfaces ride; `HTTPXClientInstrumentor` spans the transport client legs; `Jinja2Instrumentor` spans the artifacts template render/compile/load legs — renders happen at artifacts altitude, activation stays here; `ThreadingInstrumentor` and `AsyncioInstrumentor` propagate context across the thread and coroutine crossings the worker fabric drives; `SystemMetricsInstrumentor` runs the `_SYSTEM_SLICE` — the `system.*` and `cpython.gc.*` families alone, because the `rasm.process.*` gauges own the process family off the cached `MetricState` fold and one fact keeps one owner.
- Entry: `install(scope=)` latches per composition over the one `latched`-guarded train activation and takes no profile argument — the gate is the installed provider, the same law the instrument fold holds — so a PACKAGE/TEST process patches against the no-op providers at zero export cost, a later composition's receipt truthfully records zero newly activated rows, and activation happens once at the composition root, never at library altitude. The dbapi wrap likewise activates at the composition root alone: the data-side consumer hands its own admitted driver module in (duckdb, ADBC DBAPI), so this folder imports and patches nothing it does not admit.
- Growth: a new instrumentor is one `lazy from` line and one `TRAIN` thunk row; a new system-metrics family is one `_SYSTEM_SLICE` key; a new dedicated-instrumentor-less driver is one `DbapiSeam` value the composition root threads through `Instrumentation.dbapi`.
- Boundary: the gRPC legs stay the serve interceptor's — the serve page's context authority forbids a second server-leg patch — and no sibling package activates an instrumentor. DBAPI spans complement the receipts data plane, never replace it: `QueryReceipt.profile` stays the data owner's truth, `capture_parameters` stays `False` as the export posture, and a driver carrying its own contrib instrumentor never routes through the generic seam.

```python signature
class TrainReceipt(Struct, frozen=True):
    activated: tuple[str, ...]


# a driver with no dedicated contrib instrumentor rides the generic PEP-249 seam: the data-side consumer hands its own
# admitted driver module in, so this folder patches nothing it does not admit and the wrap activates at composition only.
class DbapiSeam(Struct, frozen=True):
    name: str  # instrumenting scope the emitted spans carry
    connect_module: ModuleType  # consumer-admitted driver module (duckdb, adbc_driver_manager.dbapi)
    connect_method_name: str  # the module's connect callable name, "connect" on every PEP-249 driver
    database_system: str  # db.system semconv token the spans carry


# export posture fixed as data: statement parameters never captured outside an explicit redacted diagnostic opt-in.
_DBAPI_POSTURE: Final[dict[str, bool]] = {"capture_parameters": False}


# system.* + cpython.gc.* alone: the rasm.process.* gauges own the process family, so one fact keeps one owner.
_SYSTEM_SLICE: Final[dict[str, list[str] | None]] = {
    key: None
    for key in (
        "system.cpu.time",
        "system.cpu.utilization",
        "system.memory.usage",
        "system.memory.utilization",
        "system.swap.usage",
        "system.swap.utilization",
        "system.disk.io",
        "system.disk.operations",
        "system.disk.time",
        "system.network.dropped.packets",
        "system.network.errors",
        "system.network.io",
        "system.network.packets",
        "system.thread_count",
        "cpython.gc.collections",
        "cpython.gc.collected_objects",
        "cpython.gc.uncollectable_objects",
    )
}

# thunk rows keep the lazy imports cold until install; each thunk constructs and the fold calls .instrument().
TRAIN: Final[Block[tuple[str, Callable[[], Instrumentor]]]] = Block.of_seq([
    ("psycopg", lambda: PsycopgInstrumentor()),
    ("sqlite3", lambda: SQLite3Instrumentor()),
    ("httpx", lambda: HTTPXClientInstrumentor()),
    ("jinja2", lambda: Jinja2Instrumentor()),
    ("system-metrics", lambda: SystemMetricsInstrumentor(config=_SYSTEM_SLICE)),
    ("threading", lambda: ThreadingInstrumentor()),
    ("asyncio", lambda: AsyncioInstrumentor()),
])


class Instrumentation:
    _receipts: ClassVar[Map[ScopeKey, TrainReceipt]] = Map.empty()
    _process: ClassVar[TrainReceipt | None] = None

    # reentrant closure returns the empty roster: a later composition activated nothing, and its receipt says so.
    @classmethod
    @latched(lambda: Instrumentation._process, lambda r: setattr(Instrumentation, "_process", r), lambda _prior: TrainReceipt(activated=()))
    def _activated(cls) -> TrainReceipt:
        def enroll(names: tuple[str, ...], row: tuple[str, Callable[[], Instrumentor]]) -> tuple[str, ...]:
            name, thunk = row
            thunk().instrument()
            return (*names, name)

        return TrainReceipt(activated=TRAIN.fold(enroll, ()))

    @classmethod
    def install(cls, scope: ScopeKey = DEFAULT_SCOPE) -> TrainReceipt:
        match cls._receipts.try_find(scope):
            case Option(tag="some", some=prior):
                return prior
            case _:
                receipt = cls._activated()
                cls._receipts = cls._receipts.add(scope, receipt)
                return receipt

    @overload
    @classmethod
    def dbapi(cls, seam: DbapiSeam) -> None: ...
    @overload
    @classmethod
    def dbapi[C](cls, seam: DbapiSeam, connection: C) -> C: ...
    @classmethod
    def dbapi[C](cls, seam: DbapiSeam, connection: C | None = None) -> C | None:
        # one polymorphic wrap seam: absent connection patches the seam's connect callable forward, so every later
        # connect returns a traced connection; a connection built before the patch retrofits through the returned proxy.
        match connection:
            case None:
                wrap_connect(seam.name, seam.connect_module, seam.connect_method_name, seam.database_system, **_DBAPI_POSTURE)
                return None
            case live:
                return instrument_connection(seam.name, live, seam.database_system, **_DBAPI_POSTURE)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
