# [PY_GEOMETRY_GRADUATION]

`graduation` is the folder's tier-0 evidence spine every producer composes and no page precedes. It mints the graduation vocabulary — `GeometrySubject`, the geometry-owned union of what the folder produces — and the typed `GeometryHandoff` carrier every producer's `graduates()` returns; the crossing to compute is content-keyed receipt DATA (`GeometryHandoff.wire()`), never an import. It also owns the folder's evidence weave, geometry's faults-analogue tier, so a producer writes only its domain fold.

Compute is the named demanding consumer: its `HandoffAxis` hub and `GraduationReceipt` fold decode `wire()` at the seam and re-shape nothing without a geometry ripple that a union change ships, never a silent admit; no geometry prelude names a compute symbol. Evidence weave composes the runtime `measured` weave — span lifecycle, modality probe, boundary fence, conditional receipt harvest, OK/ERROR close — under a tracer scope seeded from `EvidenceScope`, adding only geometry's close-out layer, so producers thread no page-local instrumentation. Four evidence surfaces ride the same spine: the `EvidenceCost` oneshot bracket prices every crossing, the `CHARTER` measure table is the one dashboard authority every instrument row and compile leg derives from, `PROFILE_SUBJECTS` names the worker-attach flame subjects per hostile entry seam, and the `EvidenceFrame` port projects every receipt family as a columnar frame beside `wire()`.

## [01]-[INDEX]

- [01]-[GRADUATION]: the `GeometrySubject` union, the `EvidenceScope` scope table, the `GeometryHandoff` carrier with its residual-over-ceiling admission and `wire()` crossing, the `CHARTER` measure authority, the `PROFILE_SUBJECTS` worker-attach map, the `EvidenceCost` bracket and `EvidenceFrame` port, the one `evidence_run` weave, the `bench_seam` entry-seam macro-bench fold, and its `bench_terminal` process-terminal envelope.

## [02]-[GRADUATION]

- Owner: `GeometrySubject` — the closed union the folder graduates, each member a frozen wire literal; `EvidenceScope` — the closed tracer-scope table, one member per producing page; `GeometryHandoff` — the frozen crossing carrier owning the `admitted` verdict, the `wire()` projection that IS the compute crossing, and a `contribute` receipt stream; `evidence_run` — the one weave every producer threads, the runtime `measured` spine composed once here with the geometry close-out and never re-authored.
- Cases: the union is DIFFERENTIATED, one member per evidence class, each owner binding its own. Scan produces `REGISTRATION_TRANSFORM`, `SCAN_DEVIATION`, and (with mesh/repair) `RECONSTRUCTED_MESH`; graph produces `TOPOLOGY_GRAPH`, `NETWORK_GRAPH` (features and algebra co-produce it), `FORM_FINDING`, and `NUMERICAL_PRIMITIVE` (the retained compas-numerics member); mesh/brep, mesh/repair, and graph/algebra co-produce `MESH_ALGEBRA`; ifc produces `BIM_COMPLIANCE`, `BIM_LIFECYCLE`, `SECTION_PROPERTY`; energy produces `BUILDING_ENERGY`, `THERMAL_COMFORT`. Two or three producers of one subject stay one member, never parallel vocabularies; a page earns its `EvidenceScope` member the moment its rail returns through `evidence_run`, and an engine or gate page whose receipts an enclosing weave harvests holds none.
- Entry: `evidence_run(scope, operation, dispatch, upstream=None)` is the one polymorphic weave, modality discriminated on the dispatch shape (`iscoroutinefunction`), never an `evidence_run_async` sibling — each arm wraps the dispatch and delegates span, fence, conditional harvest, and OK/ERROR close to the runtime `measured` weave under the scope's tracer name. Each arm FLATTENS through `_flat`, so a `lane.offload` or rail-returning fold absorbs un-nested and a bare value lifts `Ok`, then closes through `_priced`, the one cost-record-rename terminal on the live span — the serve leg composes the weave under `EvidenceScope.MESH_SERVE`, so serve latency is one weave composition and pool depth stays the runtime lane spine's own gauges. `GeometryHandoff.of` mints, `admitted` folds residual-over-ceiling, `wire()` projects the dict the receipt stream and compute seam consume.
- Cost: `EvidenceCost` is the resource-cost ledger on every crossing — one `psutil` `oneshot` sample bracket around the dispatch reads `cpu_times` and `memory_info().rss`, and `_priced` folds the wall/cpu/rss deltas three ways at once: span facts (`cost.*`), the `UNIVERSAL_MEASURES` charter rows recorded through `Metrics.record` under `domain="geometry"` (tenant baggage rides the runtime `_attributed` fold), and one harvested `rasm.geometry.evidence` cost receipt. Parent-observed cost only: a `KernelTrait.HOSTILE` kernel's worker-floor burn is the profile lane's evidence, never re-priced here.
- Charter: `CHARTER` + `UNIVERSAL_MEASURES` are the one dashboard authority — each `MeasureRow` names its `rasm.<domain>.<measure>` spelling, UCUM unit, source receipt field, and aggregation, keyed by `GeometrySubject`, and `charter_of` folds the universal cost rows onto the subject's own. Every producing-fold record call, every runtime `INSTRUMENTS` row for a geometry measure, and the ts iac dashboard compile leg derive from these rows: `charter_record` is the one producing-fold projection — the subject selects the rows, the measure spelling and source receipt field derive from them — so a hand-picked measure list beside the charter is the deleted form.
- Profile: `PROFILE_SUBJECTS` maps each scope-owning `KernelTrait.HOSTILE` entry seam to the module-level kernel names its offloads run, so the runtime worker-attach counterpart tags flame windows with one vocabulary the evidence spans already speak; geometry names subjects and seams, runtime owns the worker bootstrap and the pyroscope agent.
- Frame: `EvidenceFrame` is the columnar egress port beside `wire()` — subject-discriminated, content-keyed, numpy-backed columns — and every receipt family projects one frame through its own `frame()` row (deviation bands, quality metrics, section properties, lifecycle rollups, analytic boards through `AnalyticValue.tabled()`), crossing the standing geometry-to-data seam beside the energy `ResultFrame` so the data branch's arrow and duckdb tiers aggregate geometry evidence without receipt re-parsing.
- Receipt: `GeometryHandoff.contribute` yields one `Receipt.of("rasm.geometry.graduation", (phase, subject, wire))` row — `phase="emitted"` for an admitted crossing, `"admitted"` for a breaching one whose caveat the receipt flags rather than asserts — so every graduation is visible before compute decodes it; `of` additionally lands the crossing as one `rasm.geometry.graduation` span event on the live producer span. Producer receipts stay on the producing pages; this owner receipts only the crossing, the weave's cost, and the frame projection.
- Trace: the crossing carries one optional W3C band — `of` mints `traceparent` off the live span through the `_TRACE_WIRE` codec (an invalid or absent span context injects nothing, so absence means no link), `wire()` ships the band only when present, and compute's decode folds it as a `Link` on the consumer span. The consumer arm is symmetric: `evidence_run`'s `upstream` band decodes through the same codec at `_linked` and folds one `Span.add_link` onto the opening span — a zero trace-id after extract proves no producer trace and folds nothing — so a geometry consumer of a content-keyed crossing (deviation's reference-GLB read) joins its producer chain under the identical `rasm.link.kind` attribute the compute mirror stamps. Producers edit nothing: `of` runs inside the producer's `evidence_run` span, so the band names the producing evidence span by construction; the band is co-shipped wire law — a re-shape is a geometry ripple landing on the compute mirror, never a silent widen. `_priced` renames the span `{operation}:{subject}` when the cleared value carries a `GeometrySubject`, so trace search keys on subjects.
- Bench: `bench_seam` is the one macro-bench fold every kernel page composes — both `BenchMode` rows run over one whole entry-seam crossing (`anyio.run` drives the awaitable seam per round on a fresh loop while the warm process lane amortizes across rounds), and each `BenchmarkReceipt` harvests through the runtime `Signals` emit, so the receipt row and the runtime `domain="bench"` instrument projection fire at the receipt's own `contribute` — zero geometry instrument rows. `bench_terminal` wraps the whole `bench_seam` fold in the runtime `JobRun.bounded` envelope so a process-terminal run's final `domain="bench"` projection flushes before exit; an in-daemon run composes `bench_seam` directly under the standing periodic reader; the pulse boundary bars any in-kernel probe, so the seam is always the whole crossing.
- Packages: `anyio` (the bench fold's per-round loop driver), `expression` (`Block`/`Map`), `msgspec` (`Struct` carrier), `numpy` (frame columns), `psutil` (the cost bracket's `oneshot` sampler), `opentelemetry-api` — the ONE page that names the OTel tracer/span API, every sibling composing `evidence_run` — and runtime (`RuntimeRail`, `measured`/`Signals`/`OPEN` as the span-fence-harvest spine, `ContentKey`, `Receipt`, `Metrics.record` as the one instrument projection port, `Bench`/`BenchMode`/`BenchmarkReceipt` as the measurement tier `bench_seam` composes, and `RuntimeProfile`/`JobRun` as the process-terminal envelope `bench_terminal` composes).
- Growth: a new evidence class is one `GeometrySubject` member and its producing binding, shipping the compute-side decode ripple; a new producing page is one `EvidenceScope` member; a new cross-cutting concern on the weave is one composition inside `evidence_run`, never a per-page re-weave; a producer whose result carries `span_facts` gains span-visible evidence with zero weave edit; a new dashboard measure is one `MeasureRow` (and its runtime `INSTRUMENTS` counterpart row); a new hostile kernel is one `PROFILE_SUBJECTS` tuple entry; a new frame family is one `frame()` row on its receipt owner; a new bench subject is one `bench_seam` composition on its producing page, zero runtime edits, and a process-terminal bench harness is one `bench_terminal` wrap composing the runtime `JobRun.bounded` envelope; the crossing is one-way data, no reverse direction.
- Boundary: no compute import anywhere in the folder, the crossing being `wire()` data; no page-local tracer mint, emit pair, or inline span manager anywhere in the folder — the runtime `measured` weave owns span lifecycle and harvest, this owner adds only the geometry close-out; every weave span opens INTERNAL and nests beneath the live seam parent — the runtime `ServerHost` interceptor owns the SERVER span at the gRPC seam; no second subject vocabulary, bare-`str` subject, or per-receipt `subject` field racing the union. Mid-operation facts stay worker-side receipt evidence — the offloaded `KernelTrait.HOSTILE` kernels cross a process seam no in-process hook registry or live span reaches; the `rasm.geometry.graduation` span event is the settled completion-grain tap, never a mid-kernel pulse claim. A `"numerical-primitive"` literal carrying compliance, lifecycle, or section evidence breaches the differentiated members; cost never rides `wire()` — the crossing widens only by the co-shipped trace band; the compute graduation interior is compute's to own — this owner only mints and projects.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from inspect import iscoroutinefunction
from time import perf_counter
from typing import Final, Protocol, runtime_checkable

import anyio
import numpy as np
import psutil
from expression import Ok, Result
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator

from rasm.runtime.admission import RuntimeProfile
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.profiles import Bench, BenchMode, BenchmarkReceipt, JobRun
from rasm.runtime.receipts import OPEN, Receipt, Signals, measured

# --- [TYPES] ----------------------------------------------------------------------------


class GeometrySubject(StrEnum):
    # each member a frozen wire literal compute inherits as decode-only data.
    REGISTRATION_TRANSFORM = "registration-transform"
    SCAN_DEVIATION = "scan-deviation"
    RECONSTRUCTED_MESH = "reconstructed-mesh"
    TOPOLOGY_GRAPH = "topology-graph"
    NETWORK_GRAPH = "network-graph"
    FORM_FINDING = "form-finding"
    NUMERICAL_PRIMITIVE = "numerical-primitive"
    MESH_ALGEBRA = "mesh-algebra"
    BIM_COMPLIANCE = "bim-compliance"
    BIM_LIFECYCLE = "bim-lifecycle"
    SECTION_PROPERTY = "section-property"
    BUILDING_ENERGY = "building-energy"
    THERMAL_COMFORT = "thermal-comfort"


class EvidenceScope(StrEnum):
    # one member per weave-returning rail; value the rasm.-prefixed instrumentation scope the weave's tracer mints from —
    # one spelling law with the runtime SCOPES vocabulary and this owner's own receipt scopes.
    MESH_SERVE = "rasm.geometry.mesh.serve"
    MESH_BREP = "rasm.geometry.mesh.brep"
    MESH_REPAIR = "rasm.geometry.mesh.repair"
    IFC_ANALYSIS = "rasm.geometry.ifc.analysis"
    IFC_LIFECYCLE = "rasm.geometry.ifc.costing"
    IFC_SECTION = "rasm.geometry.ifc.structural"
    SCAN_INGESTION = "rasm.geometry.scan.ingestion"
    SCAN_REGISTRATION = "rasm.geometry.scan.registration"
    SCAN_DEVIATION = "rasm.geometry.scan.deviation"
    SCAN_RECONSTRUCTION = "rasm.geometry.scan.reconstruction"
    GRAPH_ALGEBRA = "rasm.geometry.graph.algebra"
    GRAPH_FEATURES = "rasm.geometry.graph.features"
    GRAPH_TOPOLOGY = "rasm.geometry.graph.nonmanifold"
    ENERGY_CLIMATE = "rasm.geometry.energy.climate"
    ENERGY_MODEL = "rasm.geometry.energy.model"
    ENERGY_DISTRICT = "rasm.geometry.energy.district"
    ENERGY_SIMULATE = "rasm.geometry.energy.simulate"


class Aggregation(StrEnum):
    # dashboard fold per charter row — the compile leg reads it as data, never a panel-side guess.
    MAX = "max"
    MEAN = "mean"
    P95 = "p95"
    SUM = "sum"
    LAST = "last"


@runtime_checkable
class SpanFacts(Protocol):
    # structural close-out conformance the weave reads like the receipt harvest; flat primitives only — the span attribute API refuses a nested mapping.
    @property
    def span_facts(self) -> Mapping[str, object]: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

# frozen tuple compute inherits as decode-only wire data; a union change lands as a compute ripple, never a silent admit.
SUBJECTS: Final[tuple[str, ...]] = tuple(member.value for member in GeometrySubject)
_TRACE_WIRE: Final[TraceContextTextMapPropagator] = TraceContextTextMapPropagator()  # W3C codec: crossing mint AND upstream link decode
_PROCESS: Final[psutil.Process] = psutil.Process()  # one parent-process handle the cost bracket samples through

# worker-attach truth for the runtime profile counterpart: each row names the module-level HOSTILE kernels a scope's
# offloads run, so flame-window tags spell one vocabulary with the evidence spans. Scope-less kernels — quality's
# `_decimate_kernel`/`_topology_kernel`, spatial's `_dispatch` — window under the composing plane's member;
# a new hostile kernel is one tuple entry on its scope row.
PROFILE_SUBJECTS: Final[Map[EvidenceScope, tuple[str, ...]]] = Map.of_seq([
    (EvidenceScope.MESH_SERVE, ("_tessellate_ifc", "_tessellate_cad")),
    (EvidenceScope.MESH_REPAIR, ("_dispatch",)),
    (EvidenceScope.MESH_BREP, ("_dispatch",)),
    (EvidenceScope.SCAN_REGISTRATION, ("_register_kernel",)),
    (EvidenceScope.SCAN_RECONSTRUCTION, ("_reconstruct_kernel",)),
    (EvidenceScope.SCAN_DEVIATION, ("_deviation_kernel",)),
    (EvidenceScope.IFC_LIFECYCLE, ("_lifecycle_kernel",)),
    (EvidenceScope.ENERGY_SIMULATE, ("_translated",)),
])

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CHARTER]


class MeasureRow(Struct, frozen=True, gc=False):
    measure: str  # rasm.<domain>.<measure> wire-law spelling — no pre-baked unit or _total suffix
    unit: str  # UCUM
    field: str  # source receipt field the measure projects from
    aggregation: Aggregation


# every subject inherits the weave's cost rows; the subject map adds its own dashboard measures.
UNIVERSAL_MEASURES: Final[tuple[MeasureRow, ...]] = (
    MeasureRow("rasm.geometry.evidence.duration", "ms", "wall_ms", Aggregation.P95),
    MeasureRow("rasm.geometry.evidence.cpu", "s", "cpu_s", Aggregation.SUM),
    MeasureRow("rasm.geometry.evidence.rss_delta", "By", "rss_delta", Aggregation.MAX),
)

CHARTER: Final[Map[GeometrySubject, tuple[MeasureRow, ...]]] = Map.of_seq([
    (
        GeometrySubject.SCAN_DEVIATION,
        (
            MeasureRow("rasm.geometry.deviation.max", "m", "max_distance", Aggregation.MAX),
            MeasureRow("rasm.geometry.deviation.noncompliant", "1", "noncompliant_fraction", Aggregation.P95),
        ),
    ),
    (
        GeometrySubject.RECONSTRUCTED_MESH,
        (
            MeasureRow("rasm.geometry.mesh.genus", "1", "genus", Aggregation.MAX),
            MeasureRow("rasm.geometry.mesh.aspect", "1", "worst_aspect_ratio", Aggregation.P95),
            MeasureRow("rasm.bench.duration", "ms", "p50_ms", Aggregation.P95),
        ),
    ),
    (
        GeometrySubject.MESH_ALGEBRA,
        (
            MeasureRow("rasm.geometry.mesh.genus", "1", "genus", Aggregation.MAX),
            MeasureRow("rasm.geometry.mesh.aspect", "1", "worst_aspect_ratio", Aggregation.P95),
        ),
    ),
    (
        GeometrySubject.REGISTRATION_TRANSFORM,
        (
            MeasureRow("rasm.geometry.registration.fitness", "1", "fitness", Aggregation.MEAN),
            MeasureRow("rasm.bench.duration", "ms", "p50_ms", Aggregation.P95),
        ),
    ),
    (GeometrySubject.SECTION_PROPERTY, (MeasureRow("rasm.geometry.section.closure", "1", "ring-closure", Aggregation.MAX),)),
    (GeometrySubject.BUILDING_ENERGY, (MeasureRow("rasm.geometry.energy.eui", "kW.h/m2", "eui_total", Aggregation.LAST),)),
])


def charter_of(subject: GeometrySubject) -> tuple[MeasureRow, ...]:
    # dashboard authority: universal cost rows + the subject's own; instrument rows and the iac compile leg derive here.
    return (*UNIVERSAL_MEASURES, *CHARTER.try_find(subject).default_value(()))


def charter_record(subject: GeometrySubject, source: Mapping[str, object], kind: str) -> None:
    # producing-fold projection derived from the subject's charter rows — measure spelling and source field stay
    # single-edit-site; a row whose field is absent or non-numeric in the source records nothing (the universal
    # cost rows land in the weave's own close-out, never here).
    measures = {
        row.measure: float(value)
        for row in CHARTER.try_find(subject).default_value(())
        if isinstance(value := source.get(row.field), (int, float))
    }
    if measures:
        Metrics.record(measures, domain="geometry", kind=kind)


# --- [EVIDENCE]


class EvidenceCost(Struct, frozen=True, gc=False):
    # parent-observed crossing cost; a HOSTILE kernel's worker-floor burn is the profile lane's evidence.
    scope: EvidenceScope
    operation: str
    wall_ms: float
    cpu_s: float
    rss_delta: int

    @classmethod
    def of(cls, scope: EvidenceScope, operation: str, started: float, before: tuple[float, int]) -> "EvidenceCost":
        cpu, rss = _sampled()
        return cls(scope=scope, operation=operation, wall_ms=(perf_counter() - started) * 1000.0, cpu_s=cpu - before[0], rss_delta=rss - before[1])

    @property
    def span_facts(self) -> Mapping[str, object]:
        return {"cost.wall_ms": self.wall_ms, "cost.cpu_s": self.cpu_s, "cost.rss_delta": self.rss_delta}

    @property
    def measures(self) -> dict[str, float]:
        # the UNIVERSAL_MEASURES rows projected by field name, so the charter stays the single edit site.
        return {row.measure: float(getattr(self, row.field)) for row in UNIVERSAL_MEASURES}

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.evidence", ("emitted", f"{self.scope.value}:{self.operation}", dict(self.span_facts)))


class GeometryHandoff(Struct, frozen=True):
    subject: GeometrySubject
    key: ContentKey
    measured: dict[str, float]
    ceilings: dict[str, float]
    traceparent: str | None = None  # optional W3C band: the producing evidence span's serialized context, absent meaning no link

    @classmethod
    def of(cls, subject: GeometrySubject, key: ContentKey, measured: Mapping[str, float], ceilings: Mapping[str, float]) -> "GeometryHandoff":
        # of runs inside the producer's evidence_run span, so the minted band names the producing span and the
        # crossing lands as one `rasm.geometry.graduation` span event — zero producer edits either way.
        handoff = cls(subject=subject, key=key, measured=dict(measured), ceilings=dict(ceilings), traceparent=_traceparent())
        span = trace.get_current_span()
        if span.is_recording():
            span.add_event("rasm.geometry.graduation", dict(handoff.span_facts))
        return handoff

    @property
    def admitted(self) -> bool:
        # residual-over-ceiling: every ceiling row demands its measured fact clearing the limit; an unmeasured row breaches.
        return all(name in self.measured and self.measured[name] <= limit for name, limit in self.ceilings.items())

    @property
    def span_facts(self) -> Mapping[str, object]:
        # dotted-key flattening: measured/ceilings are nested dicts the span attribute API refuses whole.
        return {
            "subject": self.subject.value,
            "key": self.key.hex,
            "admitted": self.admitted,
            **{f"measured.{name}": value for name, value in self.measured.items()},
            **{f"ceilings.{name}": value for name, value in self.ceilings.items()},
        }

    def wire(self) -> dict[str, object]:
        # the trace band ships only when minted, so a band-free crossing stays byte-identical to the pre-band wire.
        return {
            "subject": self.subject.value,
            "key": self.key.hex,
            "measured": self.measured,
            "ceilings": self.ceilings,
            "admitted": self.admitted,
            **({"traceparent": self.traceparent} if self.traceparent is not None else {}),
        }

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.graduation", ("emitted" if self.admitted else "admitted", self.subject.value, self.wire()))


class EvidenceFrame(Struct, frozen=True):
    # subject-keyed columnar egress beside wire(): numpy-backed columns the data plane's arrow fold consumes whole,
    # so evidence analytics never re-parse receipt payloads; dict order IS the column order.
    subject: GeometrySubject
    key: ContentKey
    columns: tuple[str, ...]
    table: dict[str, np.ndarray]

    @classmethod
    def of(cls, subject: GeometrySubject, key: ContentKey, table: "Mapping[str, Sequence[object] | np.ndarray]") -> "EvidenceFrame":
        arrays = {name: np.asarray(values) for name, values in table.items()}
        if len({array.shape[0] for array in arrays.values()}) > 1:
            raise ValueError(f"ragged-frame:{subject.value}")  # converted once by the composing weave's fence
        return cls(subject=subject, key=key, columns=tuple(arrays), table=arrays)

    @property
    def rows(self) -> int:
        return next(iter(self.table.values())).shape[0] if self.table else 0

    def pydict(self) -> dict[str, list[object]]:
        # the arrow-crossing shape: the data consumer feeds this straight into its Table fold beside ResultFrame.
        return {name: self.table[name].tolist() for name in self.columns}

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.frame", ("emitted", self.subject.value, {"key": self.key.hex, "rows": self.rows, "columns": len(self.columns)}))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _traceparent() -> str | None:
    # W3C mint off the live span: the codec injects nothing for an invalid or absent span context, so the band is
    # absent exactly when there is no producer trace to link — never an empty or garbage render on the wire.
    carrier: dict[str, str] = {}
    _TRACE_WIRE.inject(carrier)
    return carrier.get("traceparent")


def _sampled() -> tuple[float, int]:
    # one oneshot collection per bracket edge: cpu-seconds (user+system) and rss off the parent handle.
    with _PROCESS.oneshot():
        times = _PROCESS.cpu_times()
        return times.user + times.system, _PROCESS.memory_info().rss


def _linked(upstream: str | None) -> None:
    # producer-chain join on the live measured span: an upstream W3C band decodes through the codec and folds one
    # Link; a missing or malformed band folds nothing — the zero trace-id the codec leaves proves there is no
    # producer trace to join. Name-mirrors the compute decode arm, so both crossing ends spell one fold.
    if upstream is None:
        return
    linked = trace.get_current_span(_TRACE_WIRE.extract({"traceparent": upstream})).get_span_context()
    if linked.trace_id:
        trace.get_current_span().add_link(linked, {"rasm.link.kind": "geometry-graduation"})


def _priced[T](scope: EvidenceScope, operation: str, started: float, before: tuple[float, int], result: T) -> T:
    # geometry close-out on the live measured span — OK status, ERROR marking, and span end stay the runtime
    # weave's. The cost bracket closes here: deltas land as span facts, charter measures, and one Signals-emitted
    # cost receipt; a conforming value annotates its facts, and a cleared value carrying a GeometrySubject renames
    # the span `{operation}:{subject}` so trace search keys on subjects. Runs inside the runtime fence, so a
    # record-time raise folds onto the rail instead of escaping it.
    ledger = EvidenceCost.of(scope, operation, started, before)
    span = trace.get_current_span()
    if span.is_recording():
        if isinstance(result, SpanFacts):
            span.set_attributes(dict(result.span_facts))
        span.set_attributes(dict(ledger.span_facts))
        if isinstance(subject := getattr(result, "subject", None), GeometrySubject):
            span.update_name(f"{operation}:{subject.value}")
    Metrics.record(ledger.measures, domain="geometry", kind=scope.value)
    Signals.emit(ledger, OPEN)
    return result


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a lane-offloaded or rail-returning dispatch already carries a fenced rail; flatten so an offload composes un-nested.
    return value if isinstance(value, Result) else Ok(value)


def evidence_run[T](
    scope: EvidenceScope, operation: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], upstream: str | None = None
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # composes the runtime measured weave — span lifecycle, modality probe, boundary fence, conditional receipt
    # harvest, OK/ERROR close — and adds only geometry's layer inside the live span: the upstream link fold, the
    # cost bracket, the charter cost record, and the subject rename. Each arm flattens BEFORE _priced so the
    # close-out reads the cleared value; the runtime _flat then passes the finished rail through unchanged.
    started, before = perf_counter(), _sampled()
    if iscoroutinefunction(dispatch):

        async def woven() -> RuntimeRail[T]:
            _linked(upstream)
            return _flat(await dispatch()).map(lambda result: _priced(scope, operation, started, before, result))

        return measured(scope.value, operation, OPEN, woven)

    def fold() -> RuntimeRail[T]:
        _linked(upstream)
        return _flat(dispatch()).map(lambda result: _priced(scope, operation, started, before, result))

    return measured(scope.value, operation, OPEN, fold)


def bench_seam(subject: str, seam: Callable[[], Awaitable[object]], *, rounds: int = 32, warmup: int = 4) -> Block[BenchmarkReceipt]:
    # entry-seam macro-bench fold every kernel page composes: both BenchMode rows run over ONE whole crossing —
    # offload, kernel, and weave together, never an in-kernel probe (a HOSTILE worker reaches no live span) — and
    # each BenchmarkReceipt harvests through the runtime Signals emit, so the receipt row and the runtime
    # domain="bench" instrument projection fire at the receipt's own contribute, zero geometry instrument rows.
    # anyio.run drives the awaitable seam per round on a fresh loop off any serving loop, and the warm process lane
    # amortizes across rounds; a process-terminal run wraps this fold through bench_terminal below.
    def rowed(mode: BenchMode) -> BenchmarkReceipt:
        receipt = Bench.run(subject, lambda: anyio.run(seam), mode=mode, rounds=rounds, warmup=warmup)
        Signals.emit(receipt, OPEN)
        return receipt

    return Block.of_seq(BenchMode).map(rowed)


def bench_terminal(
    profile: RuntimeProfile, endpoint: str, run_id: str, subject: str, seam: Callable[[], Awaitable[object]], *, rounds: int = 32, warmup: int = 4
) -> RuntimeRail[Block[BenchmarkReceipt]]:
    # process-terminal wrap of bench_seam: JobRun.bounded installs the job telemetry resource, runs the whole fold
    # under the boundary fence, then drives the flush-then-shutdown drain so the domain="bench" projection every
    # BenchmarkReceipt emits exports before the short-lived process exits — the seam's final leg for a one-shot
    # bench binary, where an in-daemon run composes bench_seam directly under the standing periodic reader.
    return JobRun.bounded(profile, endpoint, f"bench.{subject}", run_id, partial(bench_seam, subject, seam, rounds=rounds, warmup=warmup))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
