# [PY_RUNTIME_METRICS]

`Metrics` is the metric spine, registering the measured instrument set against the `observability/telemetry#TELEMETRY`-installed `MeterProvider` — it constructs no provider, reader, or exporter, minting its meter once from the `reliability/faults#FAULT` `SCOPES[Scope.METER]` row — and one `INSTRUMENTS` table owns every instrument family and every derived surface, so a new signal is EXACTLY one row: its mint, its enrollment, its carrier key, its `MEASURES` census pair, its `ViewRow` allow-list, and its `MeterReceipt` name all read that row and no second declaration exists to lag it. Install custody is two-tier: per-composition `MeterReceipt`s key by the receipts-owned `ScopeKey` — a same-scope re-bootstrap returns its cached receipt stamped `REENTRANT`, a second composition after enrollment receives `ADOPTED` — while the imported `latched` guards the one process enrollment, so the SDK never holds a doubled callback set.

Observable instruments read one frozen `MetricSnapshot` per collection and each row folds its own half of it: the process reading is a PROCESS fact sampled once and published once, the occupancy roster is composition-partitioned and stamps its own scope. One `RLock` serializes map read-modify-write and lets the export thread read the window and copy the roster before the syscall runs unheld, so free-threaded publishers cannot lose a registration, while the instrument carrier and the admitted-tenant set read lock-free off their immutable snapshots — the gate bounds distinct values and map mutation, never every measurement. Non-default scopes stamp a `composition` attribute at the one `_attributed` fold every recording path shares — the default scope staying attribute-free exactly as the tenant law spells absence — so `record`, `observe`, `retry_hook`, `occupied`, and `timed` all partition on a `scope` keyword and none can omit what a sibling stamps. Every synchronous recording path threads `context=` because Python exemplars attach only from a supplied span context, and the telemetry install pins `exemplar_filter=TraceBasedExemplarFilter()` on the constructor, so no deployment `OTEL_METRICS_EXEMPLAR_FILTER` value can silence the hand-off; an observable callback samples on the export thread where no span is active, so its rows mint an `Observation` carrying no exemplar slot at all. Names spell `rasm.<domain>.<measure>` with UCUM units and no pre-baked `_total` or unit suffix, and the instrumentation scope mints through the faults-owned `scoped` stamp over that same `SCOPES[Scope.METER]` row, so meter, tracer, and logger carry one version and one semconv coordinate. Tenant folds on as the `rasm.tenant` baggage entry the W3C composite carries; a single-tenant process carries no entry. This owner also holds the cardinality plane whole — the per-instrument `ViewRow` allow-lists and the tenant-value budget `_attributed` enforces — as DATA the telemetry install projects into SDK `View`s, so no SDK type enters below the composition root. Retry attempts ride the metrics-owned `retry_hook` the `reliability/resilience#RESILIENCE` owner registers, the drain taxonomy imports from `observability/receipts#RECEIPT`, and provider construction, exporter wiring, product export, and health stay telemetry-, resilience-, and AppHost-owned.

## [01]-[INDEX]

- [02]-[METRIC]: one `INSTRUMENTS` table over the `DOMAINS` roster and the closed `InstrumentKind` space, the derived `MEASURES` admission map, `ViewRow` allow-lists, and tenant budget, the per-collection `MetricSnapshot` over a process-tier reading and the scope-keyed `occupied` probe registry, two-tier `latched` install custody, polymorphic `record`, composed `retry_hook`, and the `timed` serve aspect.
- [03]-[INSTRUMENTATION]: composition-root instrumentor train over the contrib packages, generic PEP-249 `DbapiSeam` wrap arm, and the system-metrics slice ruling.

## [02]-[METRIC]

- Owner: `INSTRUMENTS` is the one table every derived surface reads — a row's `kind` selects its mint from `SYNC_MINT` or `OBSERVABLE_MINT`, its `name` IS the key the synchronous carrier holds it under, the `mapped` rows derive `MEASURES` against the `DOMAINS` roster, `views` derives one name-exact `ViewRow` per row from its own `keys` projection, and `MeterReceipt.instruments` names the same rows — no per-instrument `create_*` call, seed function, carrier field, name-to-field map, or hand-listed allow-list beside it. Both mint tables key one enum and their key sets partition it exactly, so `SYNC_MINT` membership is the single discriminant both enrollment folds read and a row can neither declare one family while landing in the other's fold nor fall through both. `InstrumentKind` spans the closed OTel instrument space whole rather than the subset in hand, and a producer's declared aggregation intent selects a kind instead of forcing every measure through a histogram. `latched` imports from `reliability/faults#FAULT` and guards the process enrollment `_enrolled` beside the telemetry owner's pipeline latch — one definition, its `reentrant` closure stamping `ADOPTED` for a later composition — while the per-scope latch is the `_receipts` map fold, never a re-pinned local guard.
- Entry: under a `PACKAGE`/`TEST` profile no provider is set, so `get_meter` resolves the API proxy meter and the fold mints proxy instruments that upgrade in place at the install — the gate is the installed provider, never a profile argument here, and every proxy family answers the same `isinstance` narrowing its real counterpart does. `record` is one polymorphic entrypoint: a scalar records the `SERVE_DURATION` row, a `Mapping` records each named measure onto the row `MEASURES` resolves — the artifacts emit-harvest seam records under `domain="artifact"` and its graduated texture slots under `domain="texture"` fanned by the producing `tool` its map band names, the data query-receipt projection under `domain="query"`, the geometry charter fold under `domain="geometry"`, the compute graduation taps under `domain="compute"`, the bench family under `domain="bench"` — timings keyed by subject beside the graded verdict under its `outcome` discriminant — the worker-crossing `Cost` bracket under `domain="cost"` keyed by kernel name, the evidence drain's landed facts and metered quantities under `domain="journal"`, and this branch's own pulse-conduit drops under `domain="runtime"` — both arms under the active context so every measurement exemplar-correlates to its span, and both arms fold the tenant entry and the caller's composition stamp onto the attributes through `_attributed` under its budget. EVERY recording path resolves its write member from the instrument's own family through `_write` — the three by-name rows included — so a row re-kinded from histogram to counter moves every call site with it and no site names a write verb. `timed` folds a resolved rail through `FAULT_OUTCOME` at one site — `deadline` lands `cancelled`, every other fault `rejected`, an `Ok` rail `completed` — never a lossy ok/error bool per handler.
- Auto: the free-threading gate protects each registry and receipt map mutation and nothing else — the carrier and the tenant set are immutable values swapped whole, so a measurement reads them without queueing behind a process-wide lock, and only a first-seen tenant value takes the gate. Each observable callback reads the sample window and the occupancy roster under one gate pass, samples the process off the gate, publishes the reading back under a second pass, then hands ONE `MetricSnapshot` to the row's own projection — the syscall window never held across a lock a measurement takes, and a registration landing mid-sample never overwritten because nothing swaps an occupancy row at all. That reading is a process fact seated at the process tier: it samples once and publishes one point whatever the composition count, where seating it per scope republished one host RSS under every `composition` value and doubled every sum a board takes across that dimension. It refreshes on the EXPORT cycle under `READING_TTL_S` rather than at a producer call, so a composition that records nothing still exports live gauges and one cycle's five process rows share one `oneshot`; `cpu_percent(interval=None)` is the non-blocking since-last-call delta, the first sample the `0.0` seed. Platform-gated columns leave their slot absent and publish no point where the host binds no `uss` field or `num_fds` member, since a substituted RSS and a zeroed descriptor count each read on a board exactly like the measurement they stand in for. `rasm.band.in_flight` is a level the export cycle samples through the `occupied` probes bounded owners register, one series per named band — this owner names no `CapacityLimiter` and imports no `anyio`, the probe being a bare integer read — so a lane limiter, a worker pool, and a durable intake each report their own saturation instead of summing into one number, where a per-drain remainder instead republishes the last drain's abandoned count under a level's name and doubles the drain counter's own `cancelled` column. Probe custody is lifetime-bound at both ends: each call fences, so one raising owner subtracts itself from its band rather than darkening every other bound in the cycle, and a band whose last owner retires leaves the map entirely — a level nobody holds publishes NO point, because a zero-seeded fold reads identically to a live limiter sitting empty and buries the one distinction the series answers. `rasm.lane.drained` is the opposite shape and therefore synchronous: a drain receipt is a per-drain delta whose counts add at the call. Its `outcome` dimension carries the receipts-owned TERMINAL partition alone, `accepted` naming the admitted total those four columns exactly sum to. `ProcessReading.sample`'s `suppress` is the one admitted raw-except site: the OTel observable-callback contract returns `Iterable[Observation]` and forbids a railed `Result`, so a dead-process race drops the reading and the gauges yield empty for that cycle; its vanished-process fence rides the receipts-owned `PROCESS_FAULTS` tuple, never a second local mint.
- Law: the fault attribute roster seats at `reliability/faults#FAULT`, never here — this owner IMPORTS the fault family, and receipts, logging, and the fault owner itself all stamp those keys while receipts sits BELOW this page on the import rail. Metric dimensions and fault attributes are two vocabularies: `CROSS_DIMENSIONS` admits what a view identifies a STREAM by, and a fault subject is unbounded cardinality there.
- Growth: a new measured signal is ONE `InstrumentSpec` row and nothing else — a mapping-arm row carries `mapped=True` under a rostered segment beside whatever `dimensions` its producer stamps through the `record` discriminant map, an observable row carries its `project`, and the carrier key, the census pair, the receipt name, and the view allow-list all name themselves from that row; a measure whose capability subject no `DOMAINS` row holds admits that row first, while a second producer under a standing subject adds none; a new by-name reader is one `Final` name constant its own row spells; a new metric dimension is one `Dimension` member on the row's `dimensions` tuple, reaching the write site and the view allow-list at once; a new process probe one `ProbeField` literal with its `ProcessReading` field and `_gauge` row inside the batched `oneshot`, a platform-gated one taking the optional slot so its gauge publishes nothing where the host binds no counter; a new bounded owner reporting occupancy one `occupied` scope around its lifetime under its own `band` value, its probe minting that band's series on entry and retiring it with the band's last owner, no row edit either way; a new terminal drain disposition one `DrainOutcome` member at the receipts owner, reaching this counter through the imported `DRAIN_DISPOSITIONS` with no edit here; a new fault-to-outcome mapping one `FAULT_OUTCOME` row, unmapped tags defaulting `rejected`; a new cardinality ceiling one `SignalProfile.cardinality_budget` value threaded through `install`; a new composition one `ScopeKey` value threaded through the `scope` keyword every entry carries, reaching each series through the one attribute fold. OTel closes the instrument space, so `InstrumentKind` grows only where that specification mints a family.
- Boundary: no second `MeterProvider`, no SDK provider, reader, exporter, `View`, or exemplar-reservoir construction, no `set_on_retry_hooks` registration, and no AppHost telemetry envelope, health status, or product export — the metric-stream shaping this owner holds is DATA, and the `observability/telemetry#TELEMETRY` install is the one surface that turns a `ViewRow` into an SDK `View`, which is what keeps every SDK type above the composition root. Histogram wire shape is that owner's base2-exponential `WIRE_AGGREGATION` default; the advisory rows here are the explicit-shape fallback a deployment re-arms by naming the instrument, and the tenant ceiling arrives as a policy value rather than a literal minted here. Occupancy arrives the same way: an owner hands in its own read, so no concurrency primitive, lane type, or `anyio` import crosses into this tier to be sampled.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator, Mapping, Sequence
from contextlib import contextmanager, suppress
from enum import StrEnum
from importlib.util import find_spec
from threading import RLock
from time import perf_counter
from types import MappingProxyType, ModuleType
from typing import ClassVar, Final, Literal, Protocol, assert_never, get_args, overload

import psutil
from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import baggage
from opentelemetry import context as otel_context
from opentelemetry import metrics
from opentelemetry.metrics import CallbackOptions, Counter, Histogram, Meter, Observation, UpDownCounter, _Gauge
from stamina.instrumentation import RetryDetails, RetryHook

from rasm.runtime.faults import METRICS_INSTRUMENT, SCOPES, FaultTag, Scope, boundary, latched, scoped
from rasm.runtime.receipts import DEFAULT_SCOPE, DRAIN_DISPOSITIONS, PROCESS_FAULTS, DrainOutcome, DrainReceipt, ScopeKey

lazy from opentelemetry.instrumentation.asyncio import AsyncioInstrumentor
lazy from opentelemetry.instrumentation.dbapi import instrument_connection, wrap_connect
lazy from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
lazy from opentelemetry.instrumentation.jinja2 import Jinja2Instrumentor
lazy from opentelemetry.instrumentation.psycopg import PsycopgInstrumentor
lazy from opentelemetry.instrumentation.sqlite3 import SQLite3Instrumentor
lazy from opentelemetry.instrumentation.system_metrics import SystemMetricsInstrumentor
lazy from opentelemetry.instrumentation.threading import ThreadingInstrumentor

# --- [TYPES] ----------------------------------------------------------------------------

type ProbeField = Literal["rss", "uss", "cpu", "threads", "fds"]
type Project = Callable[["MetricSnapshot"], Iterable[Observation]]
type ObservableCallback = Callable[[CallbackOptions], Iterable[Observation]]
type Occupancy = Callable[[], int]
type AttributeValue = str | bool | int | float | Sequence[str] | Sequence[bool] | Sequence[int] | Sequence[float]
type Attributes = Mapping[str, AttributeValue]
type SyncInstrument = Counter | UpDownCounter | Histogram | _Gauge
type SyncMint = Callable[[Meter, "InstrumentSpec"], SyncInstrument]
type ObservableMint = Callable[[Meter], "ObservableFactory"]


class InstrumentKind(StrEnum):
    COUNTER = "counter"
    UP_DOWN_COUNTER = "up_down_counter"
    HISTOGRAM = "histogram"
    GAUGE = "gauge"
    OBSERVABLE_COUNTER = "observable_counter"
    OBSERVABLE_UP_DOWN_COUNTER = "observable_up_down_counter"
    OBSERVABLE_GAUGE = "observable_gauge"


class Dimension(StrEnum):
    METHOD = "rpc.method"
    OUTCOME = "outcome"
    TARGET = "target"
    CAUSE = "cause"
    TOOL = "tool"
    KIND = "rasm.kind"
    BAND = "band"
    COMPOSITION = "composition"


class ObservableFactory(Protocol):
    def __call__(self, name: str, *, callbacks: Sequence[ObservableCallback], unit: str) -> object: ...


class SyncRecord(Protocol):
    def __call__(self, amount: float, attributes: Attributes | None = ..., context: "otel_context.Context | None" = ...) -> None: ...


class Instrumentor(Protocol):
    def instrument(self, **kwargs: object) -> None: ...
    @property
    def is_instrumented_by_opentelemetry(self) -> bool: ...


class MeterOutcome(StrEnum):
    INSTALLED = "installed"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


# --- [CONSTANTS] ------------------------------------------------------------------------

DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

SERVE_DURATION: Final[str] = "rasm.serve.request.duration"
RETRY_ATTEMPTS: Final[str] = "rasm.retry.attempts"
LANE_DRAINED: Final[str] = "rasm.lane.drained"

READING_TTL_S: Final[float] = 0.5

TENANT_BAGGAGE: Final[str] = "rasm.tenant"


OVERFLOW_KEY: Final[str] = "otel.metric.overflow"

TENANT_BUDGET: Final[int] = 2048

CROSS_DIMENSIONS: Final[frozenset[str]] = frozenset({TENANT_BAGGAGE, OVERFLOW_KEY, Dimension.COMPOSITION})

DOMAINS: Final[Map[str, str]] = Map.of_seq([
    ("artifact", "produced-artifact byte volume and compression economics"),
    ("band", "live occupancy of every bounded band the branch names"),
    ("bench", "benchmark claims and the verdicts grading them"),
    ("broker", "message-envelope crossings per binding — ingest lag, settled halves, and every shed fact"),
    ("catalog", "cloud-asset discovery volume per STAC query"),
    ("circuit", "failure-window transitions per guarded dependency"),
    ("compute", "solver execution, monitoring, and the numerical residual per graduation"),
    ("contract", "data-contract claim breaches per checked frame"),
    ("cost", "worker-crossing resource price per kernel"),
    ("deploy", "the consumption-profile axes a signal groups on"),
    ("egress", "object-plane byte movement per operation"),
    ("fault", "the fault-triple discriminants a signal groups on"),
    ("field", "gridded field engine selection and its interpolation legs"),
    ("geo", "spatial operation, tiling scheme, and reference-system discriminants"),
    ("geometry", "kernel graduation evidence and the resource price of each crossing"),
    ("graph", "analysed graph magnitude per algorithm"),
    ("hlc", "hybrid-logical clock halves stamped on a causal frame"),
    ("host", "the host-boundary discriminant a signal groups on"),
    ("ifc", "IFC analysis legs over an exchanged model"),
    ("impact", "environmental impact scores per declaration source"),
    ("journal", "durable evidence facts and the metered quantities they price"),
    ("lake", "lakehouse table format and commit-operation discriminants"),
    ("lane", "serialized work-lane drain progress and occupancy"),
    ("link", "the graduation-handoff subject crossing the geometry seam"),
    ("materialize", "recomputed row volume per materialization"),
    ("mesh", "mesh backend selection and its remote compression legs"),
    ("pointcloud", "point-cloud transport and compression legs"),
    ("process", "host-process resource occupancy"),
    ("quality", "data-quality breach fractions per grade"),
    ("query", "query-engine latency and row volume per engine"),
    ("ragged", "ragged-array source and row volume"),
    ("rate", "admission pacing per destination — the queue a published rate imposes"),
    ("retry", "retry attempts per target"),
    ("runtime", "mid-operation fact delivery across the worker conduit"),
    ("serve", "served-request latency per method"),
    ("tensor", "gridded tensor backend and region selection"),
    ("texture", "produced plane-set pyramid depth and texel volume per producing tool"),
    ("virtual", "virtual-dataset source composition and branch selection"),
])

FAULT_OUTCOME: Final[Map[FaultTag, DrainOutcome]] = Map.of_seq([("deadline", "cancelled")])

# --- [MODELS] ---------------------------------------------------------------------------


class ProcessReading(Struct, frozen=True, gc=False):
    rss: int
    cpu: float
    threads: int
    uss: int | None = None
    fds: int | None = None

    @classmethod
    def sample(cls, process: psutil.Process) -> "ProcessReading | None":
        with suppress(*PROCESS_FAULTS), process.oneshot():
            full = process.memory_full_info()
            return cls(
                rss=full.rss,
                cpu=process.cpu_percent(interval=None),
                threads=process.num_threads(),
                uss=getattr(full, "uss", None),
                fds=process.num_fds() if hasattr(process, "num_fds") else None,
            )
        return None


class MetricSnapshot(Struct, frozen=True, gc=False):
    reading: ProcessReading | None
    occupancy: Map[ScopeKey, Map[str, Block[Occupancy]]]


class InstrumentSpec(Struct, frozen=True):
    name: str
    kind: InstrumentKind
    unit: str
    project: Project | None = None
    advisory: tuple[float, ...] | None = None
    mapped: bool = False
    dimensions: tuple[Dimension, ...] = ()

    @property
    def domain(self) -> str:
        return self.name.split(".", 2)[1]

    @property
    def keys(self) -> frozenset[str]:
        return frozenset(self.dimensions) | (frozenset({Dimension.KIND}) if self.mapped else frozenset()) | CROSS_DIMENSIONS


class ViewRow(Struct, frozen=True):
    instrument: str
    keys: frozenset[str]
    boundaries: tuple[float, ...] | None = None


class MeterReceipt(Struct, frozen=True):
    outcome: MeterOutcome
    instruments: tuple[str, ...]
    outcomes: tuple[DrainOutcome, ...]
    budget: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def _composed(scope: ScopeKey) -> Attributes:
    return {} if scope == DEFAULT_SCOPE else {Dimension.COMPOSITION: scope}


def _held(probe: Occupancy) -> tuple[int, ...]:
    with suppress(Exception):
        return (probe(),)
    return ()


def _banded(band: str, probes: Block[Occupancy], scope: ScopeKey) -> tuple[Observation, ...]:
    live = tuple(reading for probe in probes for reading in _held(probe))
    return (Observation(sum(live), {Dimension.BAND: band, **_composed(scope)}),) if live else ()


def _inflight(snapshot: "MetricSnapshot") -> Iterable[Observation]:
    return [
        reading
        for scope, bands in snapshot.occupancy.items()
        for band, probes in bands.items()
        for reading in _banded(band, probes, scope)
    ]


def _gauge(field: ProbeField) -> Project:
    return lambda snapshot: [
        Observation(value) for value in (getattr(snapshot.reading, field, None),) if value is not None
    ]


def _write(instrument: SyncInstrument) -> SyncRecord:
    match instrument:
        case Counter() | UpDownCounter():
            return instrument.add
        case Histogram():
            return instrument.record
        case _Gauge():
            return instrument.set
        case _ as unreachable:
            assert_never(unreachable)


SYNC_MINT: Final[Map[InstrumentKind, SyncMint]] = Map.of_seq([
    (InstrumentKind.COUNTER, lambda meter, spec: meter.create_counter(spec.name, unit=spec.unit)),
    (InstrumentKind.UP_DOWN_COUNTER, lambda meter, spec: meter.create_up_down_counter(spec.name, unit=spec.unit)),
    (InstrumentKind.GAUGE, lambda meter, spec: meter.create_gauge(spec.name, unit=spec.unit)),
    (
        InstrumentKind.HISTOGRAM,
        lambda meter, spec: meter.create_histogram(spec.name, unit=spec.unit, explicit_bucket_boundaries_advisory=spec.advisory),
    ),
])

OBSERVABLE_MINT: Final[Map[InstrumentKind, ObservableMint]] = Map.of_seq([
    (InstrumentKind.OBSERVABLE_COUNTER, lambda meter: meter.create_observable_counter),
    (InstrumentKind.OBSERVABLE_UP_DOWN_COUNTER, lambda meter: meter.create_observable_up_down_counter),
    (InstrumentKind.OBSERVABLE_GAUGE, lambda meter: meter.create_observable_gauge),
])


INSTRUMENTS: Final[Block[InstrumentSpec]] = Block.of_seq([
    InstrumentSpec(
        SERVE_DURATION, InstrumentKind.HISTOGRAM, "ms", advisory=DURATION_BUCKETS_MS,
        dimensions=(Dimension.METHOD, Dimension.OUTCOME),
    ),
    InstrumentSpec(RETRY_ATTEMPTS, InstrumentKind.COUNTER, "{attempt}", dimensions=(Dimension.TARGET, Dimension.CAUSE)),
    InstrumentSpec("rasm.circuit.transitions", InstrumentKind.COUNTER, "{transition}", mapped=True, dimensions=(Dimension.TARGET, Dimension.OUTCOME)),
    InstrumentSpec("rasm.rate.wait", InstrumentKind.HISTOGRAM, "ms", mapped=True, advisory=DURATION_BUCKETS_MS, dimensions=(Dimension.TARGET,)),
    InstrumentSpec("rasm.broker.ingest_lag", InstrumentKind.HISTOGRAM, "ms", mapped=True, advisory=DURATION_BUCKETS_MS, dimensions=(Dimension.TARGET,)),
    InstrumentSpec("rasm.broker.settled", InstrumentKind.COUNTER, "{fact}", mapped=True, dimensions=(Dimension.TARGET, Dimension.OUTCOME)),
    InstrumentSpec("rasm.broker.shed", InstrumentKind.COUNTER, "{fact}", mapped=True, dimensions=(Dimension.TARGET, Dimension.CAUSE)),
    InstrumentSpec("rasm.artifact.byte_volume", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.artifact.compression_ratio", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.texture.mip_depth", InstrumentKind.HISTOGRAM, "{level}", mapped=True, dimensions=(Dimension.TOOL,)),
    InstrumentSpec("rasm.texture.texels", InstrumentKind.HISTOGRAM, "{texel}", mapped=True, dimensions=(Dimension.TOOL,)),
    InstrumentSpec("rasm.query.engine.duration", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.query.rows", InstrumentKind.HISTOGRAM, "{row}", mapped=True),
    InstrumentSpec("rasm.egress.byte_volume", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.quality.breach_fraction", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.contract.breaches", InstrumentKind.COUNTER, "{breach}", mapped=True),
    InstrumentSpec("rasm.impact.score", InstrumentKind.HISTOGRAM, "kg", mapped=True),
    InstrumentSpec("rasm.graph.nodes", InstrumentKind.HISTOGRAM, "{node}", mapped=True),
    InstrumentSpec("rasm.graph.edges", InstrumentKind.HISTOGRAM, "{edge}", mapped=True),
    InstrumentSpec("rasm.lake.commit.files_added", InstrumentKind.COUNTER, "{file}", mapped=True),
    InstrumentSpec("rasm.lake.commit.files_removed", InstrumentKind.COUNTER, "{file}", mapped=True),
    InstrumentSpec("rasm.materialize.rows", InstrumentKind.HISTOGRAM, "{row}", mapped=True),
    InstrumentSpec("rasm.tensor.byte_volume", InstrumentKind.COUNTER, "By", mapped=True),
    InstrumentSpec("rasm.catalog.items", InstrumentKind.COUNTER, "{item}", mapped=True),
    InstrumentSpec("rasm.virtual.references", InstrumentKind.COUNTER, "{reference}", mapped=True),
    InstrumentSpec("rasm.field.byte_volume", InstrumentKind.COUNTER, "By", mapped=True),
    InstrumentSpec("rasm.ragged.rows", InstrumentKind.COUNTER, "{row}", mapped=True),
    InstrumentSpec("rasm.mesh.points", InstrumentKind.COUNTER, "{point}", mapped=True),
    InstrumentSpec("rasm.pointcloud.points", InstrumentKind.COUNTER, "{point}", mapped=True),
    InstrumentSpec("rasm.geometry.evidence.duration", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.geometry.evidence.cpu_time", InstrumentKind.COUNTER, "s", mapped=True),
    InstrumentSpec("rasm.geometry.evidence.rss_delta", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.geometry.mesh.genus", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.mesh.aspect", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.deviation.max", InstrumentKind.HISTOGRAM, "m", mapped=True),
    InstrumentSpec("rasm.geometry.deviation.noncompliant", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.registration.fitness", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.section.closure", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.compliance.noncompliant", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.form.residual", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.geometry.energy.eui", InstrumentKind.GAUGE, "kW.h/m2", mapped=True),
    InstrumentSpec("rasm.geometry.comfort.discomfort", InstrumentKind.HISTOGRAM, "1", mapped=True),
    InstrumentSpec("rasm.compute.evidence.duration", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.compute.evidence.cpu_time", InstrumentKind.COUNTER, "s", mapped=True),
    InstrumentSpec("rasm.compute.evidence.rss_delta", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.compute.graduation.residual_count", InstrumentKind.HISTOGRAM, "{residual}", mapped=True),
    InstrumentSpec("rasm.bench.duration", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.bench.throughput", InstrumentKind.HISTOGRAM, "1/s", mapped=True),
    InstrumentSpec("rasm.bench.verdicts", InstrumentKind.COUNTER, "{verdict}", mapped=True, dimensions=(Dimension.OUTCOME,)),
    InstrumentSpec("rasm.cost.cpu_time", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.cost.memory_delta", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.cost.byte_volume", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.cost.ctx_switches", InstrumentKind.HISTOGRAM, "{switch}", mapped=True),
    InstrumentSpec("rasm.journal.appended", InstrumentKind.COUNTER, "{fact}", mapped=True),
    InstrumentSpec("rasm.journal.deduped", InstrumentKind.COUNTER, "{fact}", mapped=True),
    InstrumentSpec("rasm.journal.deferred", InstrumentKind.COUNTER, "{attempt}", mapped=True),
    InstrumentSpec("rasm.journal.erased", InstrumentKind.COUNTER, "{subject}", mapped=True),
    InstrumentSpec("rasm.journal.groomed", InstrumentKind.COUNTER, "{fact}", mapped=True),
    InstrumentSpec("rasm.journal.metered.duration", InstrumentKind.COUNTER, "ms", mapped=True),
    InstrumentSpec("rasm.journal.metered.tally", InstrumentKind.COUNTER, "{item}", mapped=True),
    InstrumentSpec("rasm.journal.metered.volume", InstrumentKind.COUNTER, "By", mapped=True),
    InstrumentSpec("rasm.runtime.pulse.dropped", InstrumentKind.COUNTER, "{pulse}", mapped=True),
    InstrumentSpec("rasm.runtime.pulse.rejected", InstrumentKind.COUNTER, "{pulse}", mapped=True),
    InstrumentSpec("rasm.runtime.hook.shed", InstrumentKind.COUNTER, "{fact}", mapped=True),
    InstrumentSpec("rasm.runtime.hook.lost", InstrumentKind.COUNTER, "{fact}", mapped=True),
    InstrumentSpec(LANE_DRAINED, InstrumentKind.COUNTER, "{unit}", dimensions=(Dimension.OUTCOME,)),
    InstrumentSpec("rasm.band.in_flight", InstrumentKind.OBSERVABLE_UP_DOWN_COUNTER, "{unit}", _inflight, dimensions=(Dimension.BAND,),),
    InstrumentSpec("rasm.process.memory.rss", InstrumentKind.OBSERVABLE_GAUGE, "By", _gauge("rss")),
    InstrumentSpec("rasm.process.memory.uss", InstrumentKind.OBSERVABLE_GAUGE, "By", _gauge("uss")),
    InstrumentSpec("rasm.process.cpu.utilization", InstrumentKind.OBSERVABLE_GAUGE, "1", _gauge("cpu")),
    InstrumentSpec("rasm.process.thread.count", InstrumentKind.OBSERVABLE_GAUGE, "{thread}", _gauge("threads")),
    InstrumentSpec("rasm.process.fd.count", InstrumentKind.OBSERVABLE_GAUGE, "{fd}", _gauge("fds")),
])

MEASURES: Final[Map[tuple[str, str], InstrumentSpec]] = Map.of_seq([
    ((spec.domain, spec.name), spec) for spec in INSTRUMENTS if DOMAINS[spec.domain] and spec.mapped
])


def _carrier(meter: Meter) -> Map[str, SyncInstrument]:
    return Map.of_seq((spec.name, SYNC_MINT[spec.kind](meter, spec)) for spec in INSTRUMENTS if spec.kind in SYNC_MINT)


def _recorded(
    instruments: Map[str, SyncInstrument], domain: str, measure: str, amount: float, stamped: Attributes,
    context: "otel_context.Context",
) -> None:
    row = MEASURES[(domain, measure)]
    _write(instruments[row.name])(amount, {key: value for key, value in stamped.items() if key in row.keys}, context=context)


def views(explicit: frozenset[str] = frozenset()) -> tuple[ViewRow, ...]:
    return tuple(
        ViewRow(instrument=spec.name, keys=spec.keys, boundaries=spec.advisory if spec.name in explicit else None) for spec in INSTRUMENTS
    )


# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    _occupancy: ClassVar[Map[ScopeKey, Map[str, Block[Occupancy]]]] = Map.empty()
    _probe: ClassVar[psutil.Process] = psutil.Process()
    _reading: ClassVar[ProcessReading | None] = None
    _sampled_at: ClassVar[float] = 0.0
    _instruments: ClassVar[Map[str, SyncInstrument]] = _carrier(scoped(metrics.get_meter, SCOPES[Scope.METER]))
    _receipts: ClassVar[Map[ScopeKey, MeterReceipt]] = Map.empty()
    _process: ClassVar[MeterReceipt | None] = None
    _tenants: ClassVar[frozenset[str]] = frozenset()
    _budget: ClassVar[int] = TENANT_BUDGET
    _observed: ClassVar[frozenset[str]] = frozenset()
    _gate = RLock()

    @classmethod
    @latched(lambda: Metrics._process, lambda r: setattr(Metrics, "_process", r), lambda prior: replace(prior, outcome=MeterOutcome.ADOPTED))
    def _enrolled(cls) -> MeterReceipt:
        meter = scoped(metrics.get_meter, SCOPES[Scope.METER])

        def enroll(_: None, spec: InstrumentSpec) -> None:
            OBSERVABLE_MINT[spec.kind](meter)(spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)
            cls._stamped(spec.name)

        INSTRUMENTS.filter(lambda spec: spec.kind not in SYNC_MINT and spec.name not in cls._observed).fold(enroll, None)
        cls._instruments = _carrier(meter)
        return MeterReceipt(MeterOutcome.INSTALLED, tuple(spec.name for spec in INSTRUMENTS), DRAIN_DISPOSITIONS, cls._budget)

    @classmethod
    def _stamped(cls, name: str) -> None:
        with cls._gate:
            cls._observed = cls._observed | {name}

    @classmethod
    def install(cls, scope: ScopeKey = DEFAULT_SCOPE, budget: int = TENANT_BUDGET) -> MeterReceipt:
        with cls._gate:
            standing = cls._receipts.try_find(scope)
            cls._budget = cls._budget if cls._process is not None else budget
        match standing:
            case Option(tag="some", some=prior):
                return replace(prior, outcome=MeterOutcome.REENTRANT)
            case _:
                receipt = cls._enrolled()
                with cls._gate:
                    cls._occupancy = cls._seeded(scope)
                    cls._receipts = cls._receipts.add(scope, receipt)
                return receipt

    @classmethod
    def _seeded(cls, scope: ScopeKey) -> Map[ScopeKey, Map[str, Block[Occupancy]]]:
        return cls._occupancy if cls._occupancy.contains_key(scope) else cls._occupancy.add(scope, Map.empty())

    @classmethod
    @contextmanager
    def occupied(cls, probe: Occupancy, *, band: str, scope: ScopeKey = DEFAULT_SCOPE) -> Iterator[None]:
        def rebound(fold: Callable[[Block[Occupancy]], Block[Occupancy]]) -> None:
            with cls._gate:
                held = cls._seeded(scope)
                live = held[scope]
                bound = fold(live.try_find(band).default_value(Block.empty()))
                cls._occupancy = held.add(
                    scope,
                    live.add(band, bound)
                    if len(bound) > 0
                    else Map.of_seq((key, probes) for key, probes in live.items() if key != band),
                )

        rebound(lambda held: held.cons(probe))
        try:
            yield
        finally:
            rebound(lambda held: held.filter(lambda live: live is not probe))

    @classmethod
    def receipt(cls) -> Option[MeterReceipt]:
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def observe(cls, drain: DrainReceipt[object], *, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        context = otel_context.get_current()
        base = cls._attributed({}, context, scope)
        drained = _write(cls._instruments[LANE_DRAINED])
        Block.of_seq(DRAIN_DISPOSITIONS).fold(
            lambda _, column: drained(getattr(drain, column), {**base, Dimension.OUTCOME: column}, context=context), None
        )

    @overload
    @classmethod
    def record(cls, measure: float, *, method: str, outcome: DrainOutcome, scope: ScopeKey = ...) -> None: ...
    @overload
    @classmethod
    def record(
        cls, measure: Mapping[str, float], *, domain: str, kind: str, dimensions: Mapping[Dimension, str] = ...,
        scope: ScopeKey = ...,
    ) -> None: ...
    @classmethod
    def record(
        cls, measure: float | Mapping[str, float], *, method: str = "", outcome: DrainOutcome = "completed", domain: str = "",
        kind: str = "", dimensions: Mapping[Dimension, str] = MappingProxyType({}), scope: ScopeKey = DEFAULT_SCOPE,
    ) -> None:
        instruments = cls._instruments
        context = otel_context.get_current()
        match measure:
            case Mapping() as measures:
                attributes = cls._attributed({**dimensions, **({Dimension.KIND: kind} if kind else {})}, context, scope)
                Block.of_seq(measures.items()).fold(
                    lambda _, kv: _recorded(instruments, domain, kv[0], kv[1], attributes, context), None
                )
            case float() | int() as amount:
                attributes = cls._attributed(
                    {**({Dimension.METHOD: method} if method else {}), Dimension.OUTCOME: outcome}, context, scope
                )
                _write(instruments[SERVE_DURATION])(amount, attributes, context=context)
            case _ as unreachable:
                assert_never(unreachable)

    @classmethod
    def retry_hook(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RetryHook:
        def hook(details: RetryDetails) -> None:
            context = otel_context.get_current()
            attributes = cls._attributed({Dimension.TARGET: details.name, Dimension.CAUSE: type(details.caused_by).__qualname__}, context, scope)
            _write(cls._instruments[RETRY_ATTEMPTS])(1, attributes, context=context)

        return hook

    @classmethod
    def _attributed(cls, base: Attributes, context: "otel_context.Context", scope: ScopeKey) -> dict[str, AttributeValue]:
        composed = {**base, **_composed(scope)}
        match baggage.get_baggage(TENANT_BAGGAGE, context):
            case str() as tenant if tenant in cls._tenants:
                return {**composed, TENANT_BAGGAGE: tenant}
            case str() as tenant if tenant:
                return {**composed, TENANT_BAGGAGE: tenant} if cls._admitted(tenant) else {**composed, OVERFLOW_KEY: True}
            case _:
                return composed

    @classmethod
    def _admitted(cls, tenant: str) -> bool:
        with cls._gate:
            admitted = tenant in cls._tenants or len(cls._tenants) < cls._budget
            cls._tenants = cls._tenants | {tenant} if admitted else cls._tenants
            return admitted

    @classmethod
    def _callback(cls, spec: InstrumentSpec) -> ObservableCallback:
        def observed(_: CallbackOptions) -> Iterable[Observation]:
            now = perf_counter()
            with cls._gate:
                stale, occupancy = now - cls._sampled_at >= READING_TTL_S, cls._occupancy
            sampled = ProcessReading.sample(cls._probe) if stale else None
            with cls._gate:
                cls._reading, cls._sampled_at = (sampled, now) if stale else (cls._reading, cls._sampled_at)
                reading = cls._reading
            return () if spec.project is None else spec.project(MetricSnapshot(reading, occupancy))

        return observed
```

## [03]-[INSTRUMENTATION]

- Owner: `Instrumentation.install` activates the contrib instrumentor train once — one `TRAIN` table of thunk rows over the module-scope `lazy from` imports, so the cold contrib modules reify on first install, never at import, and a table row never dereferences a lazy proxy at module scope. `Instrumentation.dbapi` is the generic PEP-249 arm beside it: one `DbapiSeam` row names the driver module, connect callable, and `db.system` token, and one polymorphic entry either patches the connect callable forward through `wrap_connect` or retrofits a pre-patch live connection through `instrument_connection`, discriminating on whether a connection is handed in.
- Cases: the DBAPI rows (`PsycopgInstrumentor`, `SQLite3Instrumentor`) patch the drivers the data query surfaces ride; `HTTPXClientInstrumentor` spans the transport client legs; `Jinja2Instrumentor` spans the artifacts template render/compile/load legs — renders happen at artifacts altitude, activation stays here; `ThreadingInstrumentor` and `AsyncioInstrumentor` propagate context across the thread and coroutine hops the worker crossing drives; `SystemMetricsInstrumentor` runs the `_SYSTEM_SLICE` — the `system.*` and `cpython.gc.*` families alone, because the `rasm.process.*` gauges own the process family off the snapshot's own cached reading and one fact keeps one owner.
- Entry: `install(scope=)` latches per composition over the one `latched`-guarded train activation and takes no profile argument — the gate is the installed provider, the same law the instrument fold holds — so a PACKAGE/TEST process patches against the no-op providers at zero export cost, a later composition's receipt truthfully records zero newly activated rows, and activation happens once at the composition root, never at library altitude. `_verdict` partitions the whole roster in one pass: the `wraps` presence probe runs before a row's thunk reifies, so one absent driver skips instead of raising out of the composition root and taking every later row with it, and the row's own dependency gate arms its raising arm behind the fence, so a driver whose version falls outside the instrumentor's requirement rows is REFUSED by name rather than silently unpatched under a receipt claiming otherwise. Its columns sum to the roster, so a support bundle reads what this host ships, what it declined, and what it patched off one receipt. DBAPI wrapping likewise activates at the composition root alone: the data-side consumer hands its own admitted driver module in (duckdb, ADBC DBAPI), so this folder imports and patches nothing it does not admit.
- Growth: a new instrumentor is one `lazy from` line and one `TRAIN` row naming the driver it wraps; a new system-metrics family is one `_SYSTEM_SLICE` key; a new dedicated-instrumentor-less driver is one `DbapiSeam` value the composition root threads through `Instrumentation.dbapi`.
- Boundary: the Connect server legs stay the serve page's `OpenTelemetryMiddleware` and `Admission` interceptor — its context authority forbids a second server-leg patch — and no sibling package activates an instrumentor. DBAPI spans complement the receipts data plane, never replace it: `QueryReceipt.profile` stays the data owner's truth, `capture_parameters` stays `False` as the export posture, and a driver carrying its own contrib instrumentor never routes through the generic seam.

```python signature
type TrainVerdict = Literal["activated", "refused", "absent"]

TRAIN_VERDICTS: Final[tuple[TrainVerdict, ...]] = get_args(TrainVerdict.__value__)


class TrainReceipt(Struct, frozen=True):
    activated: tuple[str, ...] = ()
    refused: tuple[str, ...] = ()
    absent: tuple[str, ...] = ()


class DbapiSeam(Struct, frozen=True):
    name: str
    connect_module: ModuleType
    connect_method_name: str
    database_system: str


_DBAPI_POSTURE: Final[dict[str, bool]] = {"capture_parameters": False}


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

class TrainRow(Struct, frozen=True, gc=False):
    name: str
    wraps: str
    mount: Callable[[], Instrumentor]


TRAIN: Final[Block[TrainRow]] = Block.of_seq([
    TrainRow("psycopg", "psycopg", lambda: PsycopgInstrumentor()),
    TrainRow("sqlite3", "sqlite3", lambda: SQLite3Instrumentor()),
    TrainRow("httpx", "httpx", lambda: HTTPXClientInstrumentor()),
    TrainRow("jinja2", "jinja2", lambda: Jinja2Instrumentor()),
    TrainRow("system-metrics", "psutil", lambda: SystemMetricsInstrumentor(config=_SYSTEM_SLICE)),
    TrainRow("threading", "threading", lambda: ThreadingInstrumentor()),
    TrainRow("asyncio", "asyncio", lambda: AsyncioInstrumentor()),
])


def _verdict(row: TrainRow) -> TrainVerdict:
    if find_spec(row.wraps) is None:
        return "absent"
    instrumentor = row.mount()
    landed = boundary(METRICS_INSTRUMENT, lambda: instrumentor.instrument(raise_exception_on_conflict=True), catch=Exception)
    return "activated" if landed.is_ok() and instrumentor.is_instrumented_by_opentelemetry else "refused"


class Instrumentation:
    _receipts: ClassVar[Map[ScopeKey, TrainReceipt]] = Map.empty()
    _process: ClassVar[TrainReceipt | None] = None
    _gate = RLock()

    @classmethod
    @latched(lambda: Instrumentation._process, lambda r: setattr(Instrumentation, "_process", r), lambda _prior: TrainReceipt())
    def _activated(cls) -> TrainReceipt:
        def enroll(held: Map[TrainVerdict, tuple[str, ...]], row: TrainRow) -> Map[TrainVerdict, tuple[str, ...]]:
            verdict = _verdict(row)
            return held.add(verdict, (*held[verdict], row.name))

        folded = TRAIN.fold(enroll, Map.of_seq((verdict, ()) for verdict in TRAIN_VERDICTS))
        return TrainReceipt(activated=folded["activated"], refused=folded["refused"], absent=folded["absent"])

    @classmethod
    def install(cls, scope: ScopeKey = DEFAULT_SCOPE) -> TrainReceipt:
        with cls._gate:
            match cls._receipts.try_find(scope):
                case Option(tag="some", some=prior):
                    return prior
                case _:
                    receipt = cls._activated()
                    cls._receipts = cls._receipts.add(scope, receipt)
                    return receipt

    @classmethod
    def receipt(cls) -> Option[TrainReceipt]:
        with cls._gate:
            return Option.of_optional(cls._process)

    @overload
    @classmethod
    def dbapi(cls, seam: DbapiSeam) -> None: ...
    @overload
    @classmethod
    def dbapi[C](cls, seam: DbapiSeam, connection: C) -> C: ...
    @classmethod
    def dbapi[C](cls, seam: DbapiSeam, connection: C | None = None) -> C | None:
        with cls._gate:
            match connection:
                case None:
                    wrap_connect(seam.name, seam.connect_module, seam.connect_method_name, seam.database_system, **_DBAPI_POSTURE)
                    return None
                case live:
                    return instrument_connection(seam.name, live, seam.database_system, **_DBAPI_POSTURE)
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
