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
- Growth: a new measured signal is ONE `InstrumentSpec` row and nothing else — a mapping-arm row carries `mapped=True` under a rostered segment beside whatever `dimensions` its producer stamps through the `record` discriminant map, an observable row carries its `project`, and the carrier key, the census pair, the receipt name, and the view allow-list all name themselves from that row; a measure whose capability subject no `DOMAINS` row holds admits that row first, while a second producer under a standing subject adds none; a new by-name reader is one `Final` name constant its own row spells; a new metric dimension is one `Dimension` member on the row's `dimensions` tuple, reaching the write site and the view allow-list at once; a new process probe one `ProbeField` literal with its `ProcessReading` field and `_gauge` row inside the batched `oneshot`, a platform-gated one taking the optional slot so its gauge publishes nothing where the host binds no counter; a new bounded owner reporting occupancy one `occupied` scope around its lifetime under its own `band` value, its probe minting that band's series on entry and retiring it with the band's last owner, no row edit either way; a new terminal drain disposition one `DrainOutcome` member at the receipts owner, reaching this counter through the imported `DRAIN_DISPOSITIONS` with no edit here; a new fault-to-outcome mapping one `FAULT_OUTCOME` row, unmapped tags defaulting `rejected`; a new cardinality ceiling one `SignalProfile.cardinality_budget` value threaded through `install`; a new composition one `ScopeKey` value threaded through the `scope` keyword every entry carries, reaching each series through the one attribute fold. OTel closes the instrument space, so `InstrumentKind` grows only where that specification mints a family.
- Boundary: no second `MeterProvider`, no SDK provider, reader, exporter, `View`, or exemplar-reservoir construction, no `set_on_retry_hooks` registration, and no AppHost telemetry envelope, health status, or product export — the metric-stream shaping this owner holds is DATA, and the `observability/telemetry#TELEMETRY` install is the one surface that turns a `ViewRow` into an SDK `View`, which is what keeps every SDK type above the composition root. Histogram wire shape is that owner's base2-exponential `WIRE_AGGREGATION` default; the advisory rows here are the explicit-shape fallback a deployment re-arms by naming the instrument, and the tenant ceiling arrives as a policy value rather than a literal minted here. Occupancy arrives the same way: an owner hands in its own read, so no concurrency primitive, lane type, or `anyio` import crosses into this tier to be sampled.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping, Sequence
from contextlib import contextmanager, suppress
from enum import StrEnum
from functools import wraps
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

from rasm.runtime.faults import SCOPES, BoundaryFault, FaultTag, RuntimeRail, Scope, boundary, latched, scoped
from rasm.runtime.receipts import DEFAULT_SCOPE, DRAIN_DISPOSITIONS, PROCESS_FAULTS, DrainOutcome, DrainReceipt, ScopeKey

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
# a row's WHOLE projection: the callback hands over one collection's readable surface and the row folds whichever half
# it measures, so no caller re-wraps a row's observations to finish them and no row's attribute set is decided twice.
type Project = Callable[["MetricSnapshot"], Iterable[Observation]]
type ObservableCallback = Callable[[CallbackOptions], Iterable[Observation]]
# occupancy read a bounded owner registers for its own lifetime — a bare integer the export cycle calls, never a
# rail, because the observable-callback contract admits no failure channel. The callback FENCES every call rather
# than trusting that signature: a probe is caller code holding a live bound, and one raise abandons the collection
# for the whole instrument. Every registration names its own BAND, so this level partitions by bound rather than
# summing unrelated limiters into one number.
type Occupancy = Callable[[], int]
type AttributeValue = str | bool | int | float | Sequence[str] | Sequence[bool] | Sequence[int] | Sequence[float]
type Attributes = Mapping[str, AttributeValue]
# `SyncInstrument` spans the four synchronous families; `opentelemetry.metrics` exports its gauge ABC under an
# underscore spelling — `Gauge` resolves nowhere at module top level — so this union names `_Gauge` exactly as
# `create_gauge` returns it.
type SyncInstrument = Counter | UpDownCounter | Histogram | _Gauge
type SyncMint = Callable[[Meter, "InstrumentSpec"], SyncInstrument]
# observable mint binds the meter first: the three `create_observable_*` methods are instance members, so a
# module-level row holds the meter-to-factory step and the enrollment fold supplies name, callbacks, and unit.
type ObservableMint = Callable[[Meter], "ObservableFactory"]


# `InstrumentKind` spans the closed OTel instrument space whole — four synchronous families and three observable
# ones — so this table covers every shape a measure can take and a kind is selected, never invented. Naming
# mirrors the meter factory minting each row, which keeps `SYNC_MINT` and the observable enrollment total.
class InstrumentKind(StrEnum):
    COUNTER = "counter"
    UP_DOWN_COUNTER = "up_down_counter"
    HISTOGRAM = "histogram"
    GAUGE = "gauge"
    OBSERVABLE_COUNTER = "observable_counter"
    OBSERVABLE_UP_DOWN_COUNTER = "observable_up_down_counter"
    OBSERVABLE_GAUGE = "observable_gauge"


# `Dimension` closes the branch's whole metric-attribute vocabulary: every dimension a recording path stamps is
# a member here and `InstrumentSpec.dimensions` rows against it, so view allow-list and write site read one spelling.
# `KIND` is the one prefixed member and it rides the estate-identity carve beside `rasm.tenant`: every mapping-arm
# record stamps it whatever domain the measure names, so it owns no segment and a `DOMAINS` row for it would seat a
# capability subject nothing joins on. Semconv keys keep their spec spelling and the rest carry no prefix at all,
# which is what keeps them outside the dotted grammar rather than inside it unrostered.
class Dimension(StrEnum):
    METHOD = "rpc.method"
    OUTCOME = "outcome"
    TARGET = "target"
    CAUSE = "cause"
    TOOL = "tool"
    KIND = "rasm.kind"
    BAND = "band"
    COMPOSITION = "composition"


# structural create signature the three `create_observable_*` methods satisfy, so the
# kind-keyed dispatch is typed rather than an erased `Callable[..., object]`.
class ObservableFactory(Protocol):
    def __call__(self, name: str, *, callbacks: Sequence[ObservableCallback], unit: str) -> object: ...


# `Counter.add`, `UpDownCounter.add`, `Histogram.record`, and `_Gauge.set` all spell one signature, so `_write`
# returns a bound member the mapping arm calls without knowing which family it resolved.
class SyncRecord(Protocol):
    def __call__(self, amount: float, attributes: Attributes | None = ..., context: "otel_context.Context | None" = ...) -> None: ...


# structural port over the contrib instrumentor family; keeps the TRAIN rows typed with zero eager contrib import.
# Both members are load-bearing: `instrument` patches and answers `None` on every path it can take, leaving the LATCH
# as the one witness a row has that its patch landed.
class Instrumentor(Protocol):
    def instrument(self, **kwargs: object) -> None: ...
    @property
    def is_instrumented_by_opentelemetry(self) -> bool: ...


# INSTALLED enrolled the process instrument set; REENTRANT is a same-scope re-install; ADOPTED is a later
# composition riding the standing enrollment with its own state slot and receipt.
class MeterOutcome(StrEnum):
    INSTALLED = "installed"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


# --- [CONSTANTS] ------------------------------------------------------------------------

# request-duration bucket advisory (ms) — the explicit-shape fallback: inert under telemetry's base2-exponential wire
# default, it supplies the boundaries a `ViewRow` carries once a deployment names this instrument in its re-arm roster.
DURATION_BUCKETS_MS: Final[tuple[float, ...]] = (1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 5000.0)

# Name anchors for rows a call site reads directly rather than through the `(domain, measure)` mapping arm — serve
# duration, retry attempts, and lane drains. Each spells once: its `INSTRUMENTS` row and its reader both read this
# constant, so a rename cannot leave one end behind.
SERVE_DURATION: Final[str] = "rasm.serve.request.duration"
RETRY_ATTEMPTS: Final[str] = "rasm.retry.attempts"
LANE_DRAINED: Final[str] = "rasm.lane.drained"

# process-sample window. The five `rasm.process.*` callbacks of one collection fire back to back, so the first
# refreshes the reading and the rest read it: `cpu_percent(interval=None)` is a since-last-call delta, and sampling
# per callback would hand four of the five a near-empty window. The value sits below every `SIGNAL_PROFILE`
# export interval, so each cycle takes exactly one `oneshot` and no cycle exports a reading the prior one took.
READING_TTL_S: Final[float] = 0.5

# tenant dimension: the W3C Baggage entry the C#-parented context carries; absent entry = single-tenant, no attribute.
TENANT_BAGGAGE: Final[str] = "rasm.tenant"

# `OVERFLOW_KEY` spells the OTel specification's own overflow marker: a tenant value past the budget folds its
# series onto this key instead of minting a new one, so a backend reads the clipped stream as SDK-limited.
OVERFLOW_KEY: Final[str] = "otel.metric.overflow"

# tenant-value ceiling this branch enforces, and the default the telemetry owner's `SignalProfile` row carries.
# Allow-lists bound the KEY axis; only the tenant VALUE axis stays unbounded, and this SDK train ships no
# per-view numeric limit, so the budget closes it at the one attribute fold every recording path shares.
TENANT_BUDGET: Final[int] = 2048

# every attribute key a view admits beyond the row's own dimensions: the tenant fold's canonical key, its
# overflow marker, and the composition stamp a non-default scope carries. A key outside a row's admitted set
# drops before the stream is identified, which is what makes the per-row allow-list the primary cardinality bound.
CROSS_DIMENSIONS: Final[frozenset[str]] = frozenset({TENANT_BAGGAGE, OVERFLOW_KEY, Dimension.COMPOSITION})

# branch domain roster: the `<domain>` segment of every segmented rasm. name this branch mints — instrument names,
# metric dimensions, and the span and log attribute keys its producers stamp. A row names the capability SUBJECT a
# query joins on rather than the package emitting it, so a second emitter under a standing subject adds no row and a
# name whose subject no row holds admits its row first. `MEASURES` reads this map TOTALLY across the whole instrument
# table, so an unrostered segment refuses at import rather than at a producer's first record. Peer branches
# claiming a segment carry its subject spelling byte-identical, so the corpus mint compares one vocabulary.
DOMAINS: Final[Map[str, str]] = Map.of_seq([
    ("artifact", "produced-artifact byte volume and compression economics"),
    ("band", "live occupancy of every bounded band the branch names"),
    ("bench", "benchmark claims and the verdicts grading them"),
    ("catalog", "cloud-asset discovery volume per STAC query"),
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
    ("retry", "retry attempts per target"),
    ("runtime", "mid-operation fact delivery across the worker conduit"),
    ("serve", "served-request latency per method"),
    ("tensor", "gridded tensor backend and region selection"),
    ("texture", "produced plane-set pyramid depth and texel volume per producing tool"),
    ("virtual", "virtual-dataset source composition and branch selection"),
])

# serve-rail FaultTag -> DrainOutcome projection: `deadline` lands `cancelled`, every other
# tag defaults `rejected` through the `try_find` fold, an Ok rail folds `completed` at the call site.
FAULT_OUTCOME: Final[Map[FaultTag, DrainOutcome]] = Map.of_seq([("deadline", "cancelled")])

# --- [MODELS] ---------------------------------------------------------------------------


class ProcessReading(Struct, frozen=True, gc=False):
    # Platform-gated columns take the OPTIONAL slot: a platform binding no `uss` field and no `num_fds` member measured
    # nothing on those axes, and their gauges publish no point for a cycle that read none. Filling `rss` for `uss`
    # reports a working set the host never resolved and filling `0` for an unbound descriptor count reports a process
    # holding no handles, each indistinguishable on a board from the real reading it stands in for.
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


# one collection's whole readable surface, taken once per callback and handed to the row that projects it. The two
# halves carry DIFFERENT lifetimes, which is why neither is seated inside the other: a reading is a PROCESS fact — one
# host process stands behind every composition — so it samples once and publishes one point, where a per-composition
# reading republished one RSS under each stamp and doubled every sum a board takes across that dimension. Occupancy is
# genuinely composition-partitioned, so it stays keyed and each band carries its own owner's stamp. No per-event count
# lands here either — a delta ADDS at its call, where a snapshot held for the export thread reports the last event
# forever. Splitting the two also retires the read-modify-merge the conflated row needed: the sample runs off the gate,
# so a whole-row swap could drop a registration that landed meanwhile, and nothing now swaps a row at all.
class MetricSnapshot(Struct, frozen=True, gc=False):
    reading: ProcessReading | None
    occupancy: Map[ScopeKey, Map[str, Block[Occupancy]]]


# `kind` alone partitions the table — the synchronous rows against `SYNC_MINT`, the observable rows against
# `OBSERVABLE_MINT` — so no row carries a second discriminant that could disagree with its own family. `advisory`
# rides only histogram rows, `project` only observable ones, `mapped` marks a row `record`'s mapping arm reaches by
# `(domain, measure)`, and `dimensions` declares the attribute keys the row's own write stamps. The domain segment is
# NOT a field — it derives from `name`, so a row cannot declare one segment and spell another.
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
        # `keys` projects this row's admitted attribute set — its declared dimensions, the kind discriminant every
        # mapped row's write stamps, and the cross-cutting keys — so a mapped row never respells `Dimension.KIND`
        # and each view allow-list derives from the same table the write reads.
        return frozenset(self.dimensions) | (frozenset({Dimension.KIND}) if self.mapped else frozenset()) | CROSS_DIMENSIONS


# metric-stream shaping as DATA, never an SDK object: this owner holds the rows and the
# `observability/telemetry#TELEMETRY` install projects each into a `View` at `MeterProvider` construction, which
# keeps every SDK type above the composition root exactly as the branch's library-tier law requires. `boundaries`
# present marks the deployment-re-armed explicit shape; absent keeps the exporter's base2-exponential wire default.
class ViewRow(Struct, frozen=True):
    instrument: str
    keys: frozenset[str]
    boundaries: tuple[float, ...] | None = None


class MeterReceipt(Struct, frozen=True):
    # `outcomes` names the key space the drain counter's `outcome` dimension actually carries — the terminal
    # partition, never the admitted total riding beside it — so a board reading this receipt sums the same
    # series set the counter writes.
    outcome: MeterOutcome
    instruments: tuple[str, ...]
    outcomes: tuple[DrainOutcome, ...]
    budget: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def _composed(scope: ScopeKey) -> Attributes:
    # non-default compositions self-identify on every series they touch; the default scope stays attribute-free
    # exactly as the tenant law spells absence, so a single-composition process keeps today's series identity.
    return {} if scope == DEFAULT_SCOPE else {Dimension.COMPOSITION: scope}


def _held(probe: Occupancy) -> tuple[int, ...]:
    # Each probe is caller code the EXPORT thread calls, and the observable-callback contract carries no failure
    # channel, so a raise leaves the SDK to abandon the collection and every sibling bound goes dark with it. A
    # raising probe therefore degrades to its own absence — the same shape `ProcessReading.sample` takes for a
    # vanished process — and its band still reports whatever the surviving owners hold.
    with suppress(Exception):
        return (probe(),)
    return ()


def _banded(band: str, probes: Block[Occupancy], scope: ScopeKey) -> tuple[Observation, ...]:
    # Bands with no surviving reading publish NOTHING. A zero-length fold seeds `0` instead, which reports an idle
    # bound nobody holds: a retired owner and a live limiter sitting empty then read identically, and the operator
    # loses exactly the distinction this series exists to draw. Absence is spellable here because the callback
    # answers a measurement SEQUENCE, so an unmeasured band is a missing point rather than a fabricated one.
    # `band` is never absent on a published point — it IS the cell this map keys, one reading minted per band — so
    # this family never constructs the absent-key state the tenant and composition folds spell as the untagged
    # whole; the drop above governs absent MEASUREMENT, never an absent key. The composition stamp folds in HERE
    # because this is the one family whose state genuinely partitions by scope, and it obeys the same absence law the
    # `_attributed` fold holds: a default-scope band stays attribute-free rather than carrying a placeholder value.
    live = tuple(reading for probe in probes for reading in _held(probe))
    return (Observation(sum(live), {Dimension.BAND: band, **_composed(scope)}),) if live else ()


def _inflight(snapshot: "MetricSnapshot") -> Iterable[Observation]:
    # Occupancy is a LEVEL the export cycle samples, PARTITIONED by band: each bounded owner registers its own
    # borrowed-slot read through `occupied` under the band it bounds, and this fold sums the live probes per band AT
    # COLLECTION, so the series reports concurrency now and an operator answers WHICH bound is full. One unlabeled
    # block instead adds a lane limiter, a worker pool, and a durable intake into one number nothing decomposes.
    # Feeding it a per-drain remainder republishes the last drain's abandoned count until the next drain, doubling
    # under a level's name exactly what the drain counter's own `cancelled` column already carries.
    return [
        reading
        for scope, bands in snapshot.occupancy.items()
        for band, probes in bands.items()
        for reading in _banded(band, probes, scope)
    ]


def _gauge(field: ProbeField) -> Project:
    # one point per cycle for one host process, and absence covers both ways a column goes unread: a failed sample
    # leaves no reading at all, a platform binding no counter leaves that column `None`, and neither publishes —
    # exactly the drop `_banded` takes, since a fabricated level reads identically to a measured one.
    return lambda snapshot: [
        Observation(value) for value in (getattr(snapshot.reading, field, None),) if value is not None
    ]


def _write(instrument: SyncInstrument) -> SyncRecord:
    # `_write` reads the write member off the instrument FAMILY, never a table column: both counters spell `add`,
    # histograms `record`, synchronous gauges `set`. Resolving it from the value keeps the mapping arm total over
    # every synchronous kind, so a counter or gauge row reaches it unchanged where a hardcoded `.record` would
    # raise `AttributeError` on the first non-histogram measure a producer sends.
    match instrument:
        case Counter() | UpDownCounter():
            return instrument.add
        case Histogram():
            return instrument.record
        case _Gauge():
            return instrument.set
        case _ as unreachable:
            assert_never(unreachable)


# Lifecycle splits the closed instrument space across two kind-keyed tables whose key sets partition
# `InstrumentKind` exactly: a synchronous row is HELD and written to, an observable row is REGISTERED once and never
# touched again. Membership in `SYNC_MINT` is therefore the one discriminant both enrollment folds read, so a row
# cannot declare one family and land in the other's fold, and a kind whose row is absent refuses at the lookup
# rather than silently minting the wrong family. Advisory boundaries ride the histogram row alone.
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


# one table every derived surface reads: a row's `kind` decides whether it lands in the name-keyed synchronous carrier
# or registers its `project` callback, `mapped` rows feed `record`'s mapping arm, and the name IS the carrier key —
# a new measured signal is exactly ONE row here with no second declaration anywhere.
# names spell rasm.<domain>.<measure> under a `DOMAINS` segment, UCUM units, and no pre-baked suffixes — the translation layer appends them.
# Names never carry their own unit: `unit` already rides the row and the wire, so a `_bytes`, `_ms`, or `_total` tail
# doubles that fact and strands it the moment the row's unit changes; `byte_volume` names the CONCEPT and `By` measures it.
# stable-tier semconv names are the bind-safe default; an incubating name enters only as an alias row — the one seam absorbing an upstream rename.
# Each producer's declared aggregation intent projects onto its row kind: SUM lands a monotonic `COUNTER`, LAST
# lands the synchronous `GAUGE`, P95/MEAN/MAX all ride `HISTOGRAM` — whose data point already carries min, max,
# sum, and count, so a max-intent measure needs no second family. Charter units transcribe verbatim; inventing
# one here would put two spellings on one measure. The `<measure>` tail names the CONCEPT and stays byte-identical
# across every domain measuring it, so a cross-domain board joins on the tail and a per-producer synonym splitting
# one concept into two series has no owner.
INSTRUMENTS: Final[Block[InstrumentSpec]] = Block.of_seq([
    InstrumentSpec(
        SERVE_DURATION, InstrumentKind.HISTOGRAM, "ms", advisory=DURATION_BUCKETS_MS,
        dimensions=(Dimension.METHOD, Dimension.OUTCOME),
    ),
    InstrumentSpec(RETRY_ATTEMPTS, InstrumentKind.COUNTER, "{attempt}", dimensions=(Dimension.TARGET, Dimension.CAUSE)),
    InstrumentSpec("rasm.artifact.byte_volume", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.artifact.compression_ratio", InstrumentKind.HISTOGRAM, "1", mapped=True),
    # Pyramid depth and texel volume are the produced-set facts a texture regression moves that byte volume alone
    # cannot separate — a truncated ladder and a downscaled plane both read as fewer bytes on
    # `rasm.artifact.byte_volume` — so each graduates to its own distribution. The producing leg stays a FAN rather
    # than a second series: one set legitimately mixes the spawned encode floor with an in-process leg, and `tool`
    # is the bounded key the manifest's own per-map column already carries, so a board attributes a shift to the
    # leg that pressed it without doubling the instrument roster.
    InstrumentSpec("rasm.texture.mip_depth", InstrumentKind.HISTOGRAM, "{level}", mapped=True, dimensions=(Dimension.TOOL,)),
    InstrumentSpec("rasm.texture.texels", InstrumentKind.HISTOGRAM, "{texel}", mapped=True, dimensions=(Dimension.TOOL,)),
    InstrumentSpec("rasm.query.engine.duration", InstrumentKind.HISTOGRAM, "ms", mapped=True),
    InstrumentSpec("rasm.query.rows", InstrumentKind.HISTOGRAM, "{row}", mapped=True),
    InstrumentSpec("rasm.egress.byte_volume", InstrumentKind.HISTOGRAM, "By", mapped=True),
    InstrumentSpec("rasm.quality.breach_fraction", InstrumentKind.HISTOGRAM, "1", mapped=True),
    # A profile's breach FRACTION and a contract's breach COUNT answer different questions off different gates —
    # the fraction grades a sampled frame's shape, this counter tallies the settled claims a covenant refused — so
    # the contract claim trends its own monotonic series rather than folding onto a distribution it would skew.
    InstrumentSpec("rasm.contract.breaches", InstrumentKind.COUNTER, "{breach}", mapped=True),
    InstrumentSpec("rasm.impact.score", InstrumentKind.HISTOGRAM, "kg", mapped=True),
    InstrumentSpec("rasm.graph.nodes", InstrumentKind.HISTOGRAM, "{node}", mapped=True),
    InstrumentSpec("rasm.graph.edges", InstrumentKind.HISTOGRAM, "{edge}", mapped=True),
    InstrumentSpec("rasm.lake.commit.files_added", InstrumentKind.COUNTER, "{file}", mapped=True),
    InstrumentSpec("rasm.lake.commit.files_removed", InstrumentKind.COUNTER, "{file}", mapped=True),
    InstrumentSpec("rasm.materialize.rows", InstrumentKind.HISTOGRAM, "{row}", mapped=True),
    # Data-plane throughput rows: every one is a per-operation DELTA a producer adds at its own call, so each takes
    # the monotonic counter a cumulative reader integrates rather than a distribution over magnitudes nobody
    # compares across sources. The `<measure>` tail is byte-identical to its cross-domain twin — `byte_volume`
    # beside artifact and egress, `rows` beside query and materialize, `points` shared by both point-bearing
    # producers — so one board expression joins the tail across every domain producing it.
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
    # The three graduating subjects whose charter counterparts had no mounted row: an IDS/clash verdict's failing
    # share, a form-finding solve's max-abs residual, and a comfort run's discomfort fraction. Each reads `1`
    # because each is dimensionless by construction — a share, a fraction, and a residual whose two engines carry
    # different physical dimensions (a DR force residual against a TNA crown scale), so naming either engine's unit
    # would export the other's magnitude against a descriptor it does not satisfy. `noncompliant` spells identically
    # to its deviation twin, one tail a board joins across both compliance planes.
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
    # Verdicts close the bench subject's graded half: a timing ladder shows a regression's SHAPE where this counter
    # shows whether the bar the corpus set was crossed, and a subject whose grade lives only in a returned value
    # leaves the board with nothing to trend and the alert plane with nothing to fire on. `outcome` carries the
    # grade, so pass and fail stay one series a share expression divides rather than two counters to keep aligned.
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
    InstrumentSpec(LANE_DRAINED, InstrumentKind.COUNTER, "{unit}", dimensions=(Dimension.OUTCOME,)),
    InstrumentSpec(
        "rasm.band.in_flight", InstrumentKind.OBSERVABLE_UP_DOWN_COUNTER, "{unit}", _inflight, dimensions=(Dimension.BAND,),
    ),
    InstrumentSpec("rasm.process.memory.rss", InstrumentKind.OBSERVABLE_GAUGE, "By", _gauge("rss")),
    InstrumentSpec("rasm.process.memory.uss", InstrumentKind.OBSERVABLE_GAUGE, "By", _gauge("uss")),
    InstrumentSpec("rasm.process.cpu.utilization", InstrumentKind.OBSERVABLE_GAUGE, "1", _gauge("cpu")),
    InstrumentSpec("rasm.process.thread.count", InstrumentKind.OBSERVABLE_GAUGE, "{thread}", _gauge("threads")),
    InstrumentSpec("rasm.process.fd.count", InstrumentKind.OBSERVABLE_GAUGE, "{fd}", _gauge("fds")),
])

# (domain, measure) -> its census ROW, derived from the one table: the mapping arm's admission gate AND the whole
# descriptor a producer's own composition gate proves its charter against — name beside unit beside instrument
# family — so a producer resolves this map instead of re-deriving the same filter beside its charter. Keys pair the
# derived segment with the name, so a producer naming a measure under a domain that does not own it misses the
# total lookup exactly as an unknown measure does.
# `DOMAINS[spec.domain]` is the segment proof and evaluates for EVERY row ahead of the `mapped` select, so the whole
# table refuses at IMPORT under the ruling homing that guard; charter strings are non-empty by row constraint, which
# is what lets the total read stand as the filter's own left operand.
MEASURES: Final[Map[tuple[str, str], InstrumentSpec]] = Map.of_seq([
    ((spec.domain, spec.name), spec) for spec in INSTRUMENTS if DOMAINS[spec.domain] and spec.mapped
])


def _carrier(meter: Meter) -> Map[str, SyncInstrument]:
    # Synchronous census rows key by their own NAME, minted against one meter resolution and swapped as one immutable
    # value (the atomic-reference idiom). Membership in `SYNC_MINT` partitions the table, so no row carries a second
    # discriminant that could disagree with its kind, and no hand-listed carrier shape can lag the census.
    return Map.of_seq((spec.name, SYNC_MINT[spec.kind](meter, spec)) for spec in INSTRUMENTS if spec.kind in SYNC_MINT)


def _recorded(
    instruments: Map[str, SyncInstrument], domain: str, measure: str, amount: float, stamped: Attributes,
    context: "otel_context.Context",
) -> None:
    # two total lookups, each refusing at the producer: `MEASURES` admits the `(domain, measure)` pair or raises — a
    # measure recorded under a domain that does not own it never reaches an instrument — and the carrier resolves that
    # row's own instrument, whose FAMILY supplies the write member. A row re-kinded from histogram to counter therefore
    # moves every recording site with it, and no call site names a write verb.
    # Each row's own `keys` projection bounds the write exactly as it bounds that row's `ViewRow`, so one declaration
    # decides what a stream carries at BOTH ends: a mapping call stamping a discriminant one row declares and another
    # does not lands it on the declaring row alone, where one attribute set fanned across every measure of the call
    # exports keys whose views drop them a whole collection later.
    row = MEASURES[(domain, measure)]
    _write(instruments[row.name])(amount, {key: value for key, value in stamped.items() if key in row.keys}, context=context)


def views(explicit: frozenset[str] = frozenset()) -> tuple[ViewRow, ...]:
    # one row per instrument, name-exact, generated from the one table: the SDK mints a stream per MATCHING view,
    # so a wildcard row beside a per-instrument row would double every series both match, and a per-row allow-list
    # bounds each stream to the keys ITS write stamps rather than to the union every instrument could carry.
    # `explicit` names the instruments a deployment re-arms onto their own bucket advisory — the override gate the
    # wire's base2-exponential default otherwise holds; a name whose row carries no advisory keeps that default.
    return tuple(
        ViewRow(instrument=spec.name, keys=spec.keys, boundaries=spec.advisory if spec.name in explicit else None) for spec in INSTRUMENTS
    )


# --- [SERVICES] -------------------------------------------------------------------------


class Metrics:
    # two-tier custody: `_occupancy`/`_receipts` key per-composition registrations and evidence by ScopeKey, while
    # `_instruments` and the observable enrollment are the process pipeline the `latched` `_enrolled` guards —
    # instruments are SDK process singletons, so a doubled callback set stays structurally impossible while every
    # composition owns its own occupancy entry. `_probe`/`_reading`/`_sampled_at` are the process tier beside them:
    # ONE handle on the one process every composition runs inside, so the sample and the point it publishes are as
    # singular as the instruments are. `_tenants`/`_budget` are the process-wide cardinality guard: admitted tenant
    # values and their ceiling. Carrier and tenant set read WITHOUT the gate — atomic-reference reads are lock-free by
    # definition, and gating them puts a process-wide acquisition on every measurement a free-threaded publisher
    # takes. Gate custody covers map read-modify-write alone.
    _occupancy: ClassVar[Map[ScopeKey, Map[str, Block[Occupancy]]]] = Map.empty()
    _probe: ClassVar[psutil.Process] = psutil.Process()
    _reading: ClassVar[ProcessReading | None] = None
    _sampled_at: ClassVar[float] = 0.0
    _instruments: ClassVar[Map[str, SyncInstrument]] = _carrier(scoped(metrics.get_meter, SCOPES[Scope.METER]))
    _receipts: ClassVar[Map[ScopeKey, MeterReceipt]] = Map.empty()
    _process: ClassVar[MeterReceipt | None] = None
    _tenants: ClassVar[frozenset[str]] = frozenset()
    _budget: ClassVar[int] = TENANT_BUDGET
    # observable rows this process already registered. The SDK retires no callback, so enrollment must be resumable
    # rather than compensable: a row landing before a later row raises is recorded here and skipped on the retry,
    # where a re-run would mount a second callback on the same instrument and double every point it publishes.
    _observed: ClassVar[frozenset[str]] = frozenset()
    _gate = RLock()

    @classmethod
    @latched(lambda: Metrics._process, lambda r: setattr(Metrics, "_process", r), lambda prior: replace(prior, outcome=MeterOutcome.ADOPTED))
    def _enrolled(cls) -> MeterReceipt:
        # both folds read ONE discriminant — `SYNC_MINT` membership — so the enum partitions the two mint tables with
        # nothing left over and a row can neither enroll twice nor fall through both. Nothing here takes the custody
        # gate: `create_observable_*` reaches SDK machinery whose collection thread re-enters this owner's callback
        # under that same gate, so registering beneath it inverts a lock order the export cycle takes the other way,
        # and the carrier lands as one immutable swap a measurement reads lock-free anyway. `latched` publishes the
        # receipt only after the whole fold returns, so a partial enrollment leaves no process custody behind it.
        meter = scoped(metrics.get_meter, SCOPES[Scope.METER])

        def enroll(_: None, spec: InstrumentSpec) -> None:
            OBSERVABLE_MINT[spec.kind](meter)(spec.name, callbacks=[cls._callback(spec)], unit=spec.unit)
            cls._stamped(spec.name)

        INSTRUMENTS.filter(lambda spec: spec.kind not in SYNC_MINT and spec.name not in cls._observed).fold(enroll, None)
        cls._instruments = _carrier(meter)
        return MeterReceipt(MeterOutcome.INSTALLED, tuple(spec.name for spec in INSTRUMENTS), DRAIN_DISPOSITIONS, cls._budget)

    @classmethod
    def _stamped(cls, name: str) -> None:
        # one row's registration recorded the instant it lands, so the resume boundary is per row rather than per fold.
        with cls._gate:
            cls._observed = cls._observed | {name}

    @classmethod
    def install(cls, scope: ScopeKey = DEFAULT_SCOPE, budget: int = TENANT_BUDGET) -> MeterReceipt:
        # `budget` is the ceiling the telemetry owner's `SignalProfile.cardinality_budget` carries. Instruments are
        # process singletons, so the first ENROLLING composition fixes the guard and a later one adopts the standing
        # ceiling — its receipt records the effective value, never its request, exactly as `ADOPTED` reads elsewhere.
        # `cls._gate` spans CUSTODY alone — claiming the ceiling and reading the scope's receipt, then publishing state
        # and receipt — with meter construction and instrument enrollment outside it, so a measurement never queues
        # behind SDK machinery and no lock order runs opposite the export cycle's.
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
        # gate-held registry seed: a scope reaching any custody surface first owns its entry, so `install` and
        # `occupied` arrive in either order and neither overwrites the other's registrations.
        return cls._occupancy if cls._occupancy.contains_key(scope) else cls._occupancy.add(scope, Map.empty())

    @classmethod
    @contextmanager
    def occupied(cls, probe: Occupancy, *, band: str, scope: ScopeKey = DEFAULT_SCOPE) -> Iterator[None]:
        # Bounded owners register their own occupancy read for their lifetime under the band each bounds, and the
        # `rasm.band.in_flight` callback sums every live probe of that band at collection — so a composition running
        # many lanes reports one lane level, a worker pool reports its own, and a retired owner leaves no phantom
        # contribution behind. `band` is REQUIRED because the series is meaningless without it: unlabeled probes make
        # one number out of bounds that saturate independently, which is the reading an operator opens this series to
        # get. Registration keys on the probe OBJECT, so two owners sharing one limiter still register twice and each
        # retire drops exactly the entry it added, and the band leaves the MAP with its last owner rather than
        # lingering as an empty block the collection fold would publish a zero for.
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
        # process-custody read for the bundle capsule: Some only while an enrollment owns the instrument set.
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def observe(cls, drain: DrainReceipt[object], *, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        # a drain receipt is a per-drain DELTA, so its counts add onto the synchronous counter at the call and the
        # wire's DELTA temporality reports each window's own movement. This fold walks the TERMINAL partition alone,
        # `accepted` naming the admitted total those columns exactly partition. Occupancy is NOT this call's to report:
        # a drain reports what finished, so the level a lane still holds rides the `occupied` probe the export cycle
        # samples, and the units this receipt abandoned ride its own `cancelled` column rather than a second name.
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
        # one polymorphic recorder: a scalar is the request-duration row, a mapping records each named measure onto the
        # row `(domain, name)` resolves. `domain` is the roster segment, never an attribute key — the segment already
        # rides the metric name, so the discriminant tags under the one `Dimension.KIND` key at any cardinality. The
        # write member comes from the resolved instrument's own family, so a counter, histogram, and gauge row all
        # ride this one arm. `dimensions` carries whatever FURTHER discriminants the rows in this call declare, so a
        # mapped row's own `dimensions` tuple is reachable from a producer and each row keeps exactly the keys it
        # declares; stamping `kind` alone instead leaves every mapped-row dimension unwritable while its view
        # allow-list still admits the key, which reads as a fan a board can group on and a producer cannot fill.
        # `scope` is the composition the caller records under and reaches the series through the one attribute fold,
        # so a producer holding an embedded scope partitions here exactly as a drain does. An omitted `kind` spells
        # ABSENCE exactly as the tenant and composition laws do — no key at all rather than an empty-string value,
        # which would identify a distinct series a board groups on and no producer can ever fill.
        instruments = cls._instruments
        context = otel_context.get_current()
        match measure:
            case Mapping() as measures:
                attributes = cls._attributed({**dimensions, **({Dimension.KIND: kind} if kind else {})}, context, scope)
                Block.of_seq(measures.items()).fold(
                    lambda _, kv: _recorded(instruments, domain, kv[0], kv[1], attributes, context), None
                )
            # Scalar admission names its own shapes and the tail closes the match, so a sixth measure shape earns
            # its arm at the type checker rather than landing on the request-duration row — the write a bare
            # catch-all performs, reporting a non-numeric payload as a served latency nothing raises on.
            case float() | int() as amount:
                # `method` spells ABSENCE exactly as `kind` does above — no key at all rather than an empty-string
                # value identifying a series a board groups on and no producer can ever fill. The scalar overload
                # declares it required, so this fold closes the one route the implementation default leaves open
                # and no arm of this entry can stamp a placeholder its sibling omits.
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
        # one attribute fold every recording path shares, and the branch's whole cardinality enforcement. Composition
        # identity folds HERE rather than at each caller, so every series any entry writes carries the stamp its
        # scope earns and no recording path can omit what a sibling stamps. The tenant entry joins under its canonical
        # key so the TS Convention tenant views and the tenant board queries match Python series without relabeling.
        # Per-row allow-lists bound the KEY axis; tenant VALUE is the one axis no view can close and this SDK train
        # ships no numeric per-view limit, so the budget closes it here — a value past the ceiling folds onto
        # `OVERFLOW_KEY`, the specification's own marker, instead of minting an unbounded series, and that marker
        # rides `CROSS_DIMENSIONS` so the clipped stream survives the allow-list. The admitted-set read is lock-free
        # off the immutable snapshot, so a steady-state tenant never takes the gate — the budget is a bound on
        # DISTINCT values, not a serialization point every measurement queues behind.
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
        # Gated slow path a value takes exactly once: membership re-tests inside the gate, so two threads racing one
        # new value spend a single budget slot and a refused value never enters the set it was measured against.
        with cls._gate:
            admitted = tenant in cls._tenants or len(cls._tenants) < cls._budget
            cls._tenants = cls._tenants | {tenant} if admitted else cls._tenants
            return admitted

    @classmethod
    def timed[**P, T](
        cls, method: str, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> Callable[[Callable[P, Awaitable[RuntimeRail[T]]]], Callable[P, Awaitable[RuntimeRail[T]]]]:
        # DURATION aspect, named apart from the receipts-owned `measured` evidence weave: one spelling binds one
        # bounded concept, so a seam census over `measured` returns the span/fault/receipt weave alone and never this
        # handler timer, which opens no span and harvests no contributor.
        def aspect(serve: Callable[P, Awaitable[RuntimeRail[T]]]) -> Callable[P, Awaitable[RuntimeRail[T]]]:
            @wraps(serve)
            async def timed_serve(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
                start = perf_counter()
                rail = await serve(*args, **kwargs)
                outcome = rail.swap().to_option().map(cls._outcome).default_value("completed")
                cls.record((perf_counter() - start) * 1000.0, method=method, outcome=outcome, scope=scope)
                return rail

            return timed_serve

        return aspect

    @staticmethod
    def _outcome(fault: BoundaryFault) -> DrainOutcome:
        return FAULT_OUTCOME.try_find(fault.tag).default_value("rejected")

    @classmethod
    def _callback(cls, spec: InstrumentSpec) -> ObservableCallback:
        # One snapshot, one call: each row owns its whole projection, so nothing here re-wraps a row's observations to
        # finish them and no row's attribute set is decided at two sites. Process readings refresh HERE, under the
        # collection window rather than at a producer call, so a composition that records nothing still exports live
        # gauges and `READING_TTL_S` keeps one cycle's five process rows on one `oneshot` whatever the composition
        # count. `ProcessReading.sample` is a syscall window — `memory_full_info` walks the kernel's own maps — so it
        # runs OUTSIDE the gate every `occupied` registration and first-seen tenant queues behind: the gate reads the
        # window and the occupancy roster, the sample runs unheld, and the reading publishes back. A failed sample
        # still stamps the window and clears the reading, so a vanished process re-probes once per cycle rather than
        # once per gauge and no gauge republishes a level the host has stopped answering for.
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
- Boundary: the gRPC legs stay the serve interceptor's — the serve page's context authority forbids a second server-leg patch — and no sibling package activates an instrumentor. DBAPI spans complement the receipts data plane, never replace it: `QueryReceipt.profile` stays the data owner's truth, `capture_parameters` stays `False` as the export posture, and a driver carrying its own contrib instrumentor never routes through the generic seam.

```python signature
# each row lands in exactly one column, so the three sum to `TRAIN` and the receipt is total over the roster. The
# split is what an operator acts on: an ABSENT driver means this deployment ships none, a REFUSED one means the driver
# resolved and its version fell outside the instrumentor's own requirement rows — install nothing against move a pin,
# two moves one silence cannot tell apart.
type TrainVerdict = Literal["activated", "refused", "absent"]

TRAIN_VERDICTS: Final[tuple[TrainVerdict, ...]] = get_args(TrainVerdict.__value__)


class TrainReceipt(Struct, frozen=True):
    activated: tuple[str, ...] = ()
    refused: tuple[str, ...] = ()
    absent: tuple[str, ...] = ()


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

# each row names the driver it wraps beside the thunk minting its instrumentor, and `wraps` is the presence gate
# `_verdict` reads FIRST. A contrib instrumentor imports its driver at ITS OWN module scope, so reifying a thunk whose
# driver the environment never installed raises `ModuleNotFoundError` out of the composition root and takes the whole
# train — every later row included — with it. `find_spec` answers presence without importing, so an absent driver
# lands in its own receipt column and activates unchanged the moment that driver resolves.
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
    # gate, then mount, then PROVE — in that order and once per row. `find_spec` answers presence without importing,
    # so an absent driver skips before its lazy instrumentor proxy reifies. `instrument()` then answers `None` on
    # every path it takes — patched, already patched, and dependency-refused alike — so its return value witnesses
    # nothing and the instrumentor's own latch is the proof. `raise_exception_on_conflict=True` converts the silent
    # refusal into the typed raise this fence catches, so a drifted requirement row leaves that ONE row out of the
    # receipt rather than publishing a patch the process never took, and the fence keeps the raise off every later row.
    if find_spec(row.wraps) is None:
        return "absent"
    instrumentor = row.mount()
    landed = boundary(f"instrumentation.{row.name}", lambda: instrumentor.instrument(raise_exception_on_conflict=True))
    return "activated" if landed.is_ok() and instrumentor.is_instrumented_by_opentelemetry else "refused"


class Instrumentation:
    _receipts: ClassVar[Map[ScopeKey, TrainReceipt]] = Map.empty()
    _process: ClassVar[TrainReceipt | None] = None
    _gate = RLock()

    # reentrant closure returns every column empty: a later composition activated, refused, and skipped nothing,
    # because the process train already ran; its receipt reports its own work, never the standing roster's.
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
        # process-custody read for the bundle capsule: Some only after the train activated once.
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
        # one polymorphic wrap seam: absent connection patches the seam's connect callable forward, so every later
        # connect returns a traced connection; a connection built before the patch retrofits through the returned proxy.
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
