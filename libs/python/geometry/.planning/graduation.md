# [PY_GEOMETRY_GRADUATION]

`graduation` is the folder's tier-0 shared surface. It owns the geometry raise-leg roster, observation scopes, metric charter, progress points, columnar result frames, and benchmark corpus. Domain producers retain their own results and facts; this page adds no parallel outcome carrier.

## [01]-[INDEX]

- [02]-[GRADUATION]: raise legs, observation scopes, metric charter, progress points, columnar result frames, and benchmark corpus.

## [02]-[GRADUATION]

- Owner: `GeometryLeg` is the shared raise-leg roster. `GeometrySubject` keys the charter and result-frame families. `EvidenceScope` supplies the runtime instrumentation scope for each producer.
- Law: `evidence_run` delegates directly to runtime `measured`; geometry owns no second span lifecycle, rail fold, cost bracket, signal sink, trace carrier, or logging path.
- Law: domain facts remain on the domain result that produced them. `EvidenceFrame` is the columnar result projection and `crossing()` is its Arrow IPC egress.
- Law: `CHARTER` names geometry measures and `registered` proves the declared descriptors before registering the progress-point roster.
- Law: `bench_seam` delegates measurement to runtime `Bench.run`, and `bench_terminal` adds only the runtime job envelope while returning `Benchmark` unchanged.
- Boundary: no compute import, generic outcome carrier, caller-authored evidence map, page-local span manager, signal harvesting, or duplicate benchmark model exists in this folder.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from typing import Final

import anyio
import numpy as np
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

lazy import pyarrow as pa

from rasm.data.tabular.columnar import arrow_columns
from rasm.data.tabular.interop import arrow_bytes
from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.hooks import HookPoint, Hooks, Modality, StageMark
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import MEASURES as CENSUS
from rasm.runtime.metrics import InstrumentKind, Metrics
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey, measured
from rasm.runtime.profiles import Bench, Benchmark, BenchKernel, BenchMode, BenchSubject, BenchThreshold, BenchVerdict, JobRun

# --- [TYPES] ----------------------------------------------------------------------------


class GeometryLeg(StrEnum):
    GRADUATION = "geometry.graduation"
    INGESTION = "geometry.scan.ingestion"
    REGISTRATION = "geometry.scan.registration"
    DEVIATION = "geometry.scan.deviation"
    RECONSTRUCTION = "geometry.scan.reconstruction"
    BREP = "geometry.mesh.brep"
    CAD = "geometry.mesh.cad"
    DAEMON = "geometry.mesh.daemon"
    QUALITY = "geometry.mesh.quality"
    REPAIR = "geometry.mesh.repair"
    SERVE = "geometry.mesh.serve"
    SPATIAL = "geometry.mesh.spatial"
    ALGEBRA = "geometry.graph.algebra"
    ANALYTIC = "geometry.graph.analytic"
    FEATURES = "geometry.graph.features"
    NONMANIFOLD = "geometry.graph.nonmanifold"
    CLIMATE = "geometry.energy.climate"
    DISTRICT = "geometry.energy.district"
    MODEL = "geometry.energy.model"
    SIMULATE = "geometry.energy.simulate"
    ANALYSIS = "geometry.ifc.analysis"
    AUTHORING = "geometry.ifc.authoring"
    COSTING = "geometry.ifc.costing"
    SELECTOR = "geometry.ifc.selector"
    STRUCTURAL = "geometry.ifc.structural"


class GeometrySubject(StrEnum):
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
    MESH_SERVE = "rasm.geometry.mesh.serve"
    MESH_BREP = "rasm.geometry.mesh.brep"
    MESH_REPAIR = "rasm.geometry.mesh.repair"
    MESH_SPATIAL = "rasm.geometry.mesh.spatial"
    IFC_ANALYSIS = "rasm.geometry.ifc.analysis"
    IFC_AUTHORING = "rasm.geometry.ifc.authoring"
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

    @property
    def page(self) -> str:
        return self.value.split(".", 2)[2]


class BenchBand(StrEnum):
    WIRE = "wire"
    SOLVE = "solve"
    BATCH = "batch"


class Aggregation(StrEnum):
    MAX = "max"
    MEAN = "mean"
    P95 = "p95"
    SUM = "sum"
    LAST = "last"


class GeometryPulse(StrEnum):
    TESSELLATION = "rasm.geometry.mesh.tessellation"
    REGISTRATION = "rasm.geometry.scan.registration"
    RECONSTRUCTION = "rasm.geometry.scan.reconstruction"
    LIFECYCLE = "rasm.geometry.ifc.lifecycle"
    BEM = "rasm.geometry.energy.bem"


# --- [CONSTANTS] ------------------------------------------------------------------------

_CEILING: Final[Map[BenchBand, float]] = Map.of_seq([
    (BenchBand.WIRE, 250.0),
    (BenchBand.SOLVE, 2_000.0),
    (BenchBand.BATCH, 30_000.0),
])

CORPUS_POINTS: Final[int] = 250_000
CORPUS_SPACES: Final[int] = 64

# --- [TABLES] ---------------------------------------------------------------------------

FRAME_RAGGED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.GRADUATION,
    point="frame",
    arm="config",
    defect="ragged-frame",
    retriability=TERMINAL,
    slots=("subject", "extents"),
)
FRAME_CROSSING: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.GRADUATION,
    point="frame.crossing",
    arm="boundary",
    defect="arrow-crossing",
    retriability=TERMINAL,
)
CHARTER_DIVERGED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.GRADUATION,
    point="charter",
    arm="config",
    defect="descriptor-diverged",
    retriability=TERMINAL,
    slots=("diverged",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([
    FRAME_RAGGED,
    FRAME_CROSSING,
    CHARTER_DIVERGED,
]))

# --- [MODELS] ---------------------------------------------------------------------------


class MeasureRow(Struct, frozen=True, gc=False):
    measure: str
    unit: str
    field: str
    aggregation: Aggregation

    @property
    def domain(self) -> str:
        return self.measure.split(".", 2)[1]


_MESH_MEASURES: Final[tuple[MeasureRow, ...]] = (
    MeasureRow("rasm.geometry.mesh.genus", "1", "genus", Aggregation.MAX),
    MeasureRow("rasm.geometry.mesh.aspect", "1", "worst_aspect_ratio", Aggregation.P95),
)

CHARTER: Final[Map[GeometrySubject, tuple[MeasureRow, ...]]] = Map.of_seq([
    (
        GeometrySubject.SCAN_DEVIATION,
        (
            MeasureRow("rasm.geometry.deviation.max", "m", "max_distance", Aggregation.MAX),
            MeasureRow("rasm.geometry.deviation.noncompliant", "1", "noncompliant_fraction", Aggregation.P95),
        ),
    ),
    (GeometrySubject.RECONSTRUCTED_MESH, _MESH_MEASURES),
    (GeometrySubject.MESH_ALGEBRA, _MESH_MEASURES),
    (GeometrySubject.REGISTRATION_TRANSFORM, (MeasureRow("rasm.geometry.registration.fitness", "1", "fitness", Aggregation.MEAN),)),
    (GeometrySubject.FORM_FINDING, (MeasureRow("rasm.geometry.form.residual", "1", "residual", Aggregation.MAX),)),
    (GeometrySubject.BIM_COMPLIANCE, (MeasureRow("rasm.geometry.compliance.noncompliant", "1", "non-compliant", Aggregation.P95),)),
    (GeometrySubject.SECTION_PROPERTY, (MeasureRow("rasm.geometry.section.closure", "1", "ring-closure", Aggregation.MAX),)),
    (GeometrySubject.BUILDING_ENERGY, (MeasureRow("rasm.geometry.energy.eui", "kW.h/m2", "eui_total", Aggregation.LAST),)),
    (GeometrySubject.THERMAL_COMFORT, (MeasureRow("rasm.geometry.comfort.discomfort", "1", "discomfort", Aggregation.P95),)),
])

_KIND: Final[Map[Aggregation, InstrumentKind]] = Map.of_seq([
    (Aggregation.LAST, InstrumentKind.GAUGE),
    (Aggregation.MAX, InstrumentKind.HISTOGRAM),
    (Aggregation.MEAN, InstrumentKind.HISTOGRAM),
    (Aggregation.P95, InstrumentKind.HISTOGRAM),
    (Aggregation.SUM, InstrumentKind.COUNTER),
])

MEASURES: Final[frozenset[MeasureRow]] = frozenset(row for _, rows in CHARTER.items() for row in rows)


def _diverged(row: MeasureRow) -> Option[str]:
    match CENSUS.try_find((row.domain, row.measure)):
        case Option(tag="none"):
            return Some(f"uncensused:{row.measure}")
        case Option(tag="some", some=spec) if spec.unit != row.unit:
            return Some(f"unit:{row.measure}:{row.unit}!={spec.unit}")
        case Option(tag="some", some=spec) if spec.kind is not _KIND[row.aggregation]:
            return Some(f"fold:{row.measure}:{row.aggregation.value}!={spec.kind.value}")
        case _:
            return Nothing


def charter_record(subject: GeometrySubject, source: Mapping[str, object], *, composition: ScopeKey = DEFAULT_SCOPE) -> None:
    rows = CHARTER.try_find(subject).default_value(())
    facts = {
        row.measure: float(value)
        for row in rows
        if isinstance(value := source.get(row.field), (int, float))
    }
    if facts:
        Metrics.record(facts, domain=rows[0].domain, kind=subject.value, scope=composition)


def _sealed(values: "Sequence[object] | np.ndarray") -> np.ndarray:
    column = np.asarray(values).view()
    column.flags.writeable = False
    return column


class EvidenceFrame(Struct, frozen=True):
    subject: GeometrySubject
    key: ContentKey
    columns: tuple[str, ...]
    table: frozendict[str, np.ndarray]

    @classmethod
    def of(cls, subject: GeometrySubject, key: ContentKey, table: "Mapping[str, Sequence[object] | np.ndarray]") -> "RuntimeRail[EvidenceFrame]":
        arrays = frozendict({name: _sealed(values) for name, values in table.items()})
        extents = Block.of_seq([(name, array.shape[0] if array.ndim else -1) for name, array in arrays.items()])
        widths = frozenset(extent for _, extent in extents)
        return (
            Ok(cls(subject=subject, key=key, columns=tuple(arrays), table=arrays))
            if len(widths) <= 1 and -1 not in widths
            else Error(FRAME_RAGGED.raised(subject.value, ",".join(f"{name}={width}" for name, width in extents.sort())))
        )

    @property
    def rows(self) -> int:
        return next(iter(self.table.values())).shape[0] if self.table else 0

    def crossing(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        return boundary(
            FRAME_CROSSING,
            lambda: bytes(arrow_bytes(arrow_columns(self.columns, dict(self.table)))),
            catch=pa.ArrowException,
        ).bind(lambda payload: ContentIdentity.of("evidence.frame", payload).map(lambda key: (payload, key)))


class GraduationInstall(Struct, frozen=True, gc=False):
    points: tuple[str, ...]
    measures: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def evidence_key(subject: GeometrySubject, spec: bytes | str) -> ContentKey:
    return ContentIdentity.key(f"geometry.{subject.value}", spec if isinstance(spec, bytes) else spec.encode())


def evidence_run[T](
    scope: EvidenceScope,
    operation: str,
    dispatch: Callable[[], T] | Callable[[], Awaitable[T]],
    *,
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    return measured(scope.value, operation, dispatch, {"composition": composition})


def bench_subject(scope: EvidenceScope, *axes: object) -> str:
    return ".".join((scope.value, *(str(axis) for axis in axes)))


def _bar(band: BenchBand, mode: BenchMode) -> BenchThreshold:
    ceiling = _CEILING[band]
    return BenchThreshold(p95_ceiling_ms=ceiling, floor_hz=1_000.0 / ceiling if mode is BenchMode.THROUGHPUT else 0.0)


def _rostered(scope: EvidenceScope, axes: tuple[object, ...], band: BenchBand, mode: BenchMode) -> BenchSubject:
    return BenchSubject(subject=bench_subject(scope, *axes), kind=scope.page, mode=mode, threshold=_bar(band, mode))


CORPUS: Final[Block[BenchSubject]] = Block.of_seq((
    _rostered(EvidenceScope.MESH_SERVE, ("tessellate",), BenchBand.WIRE, BenchMode.THROUGHPUT),
    _rostered(EvidenceScope.MESH_SPATIAL, ("proximity",), BenchBand.WIRE, BenchMode.THROUGHPUT),
    _rostered(EvidenceScope.MESH_REPAIR, ("boolean",), BenchBand.SOLVE, BenchMode.LATENCY),
    _rostered(EvidenceScope.MESH_BREP, ("boolean",), BenchBand.SOLVE, BenchMode.LATENCY),
    _rostered(EvidenceScope.IFC_LIFECYCLE, ("quantity",), BenchBand.SOLVE, BenchMode.LATENCY),
    _rostered(EvidenceScope.ENERGY_MODEL, ("hbjson", f"s{CORPUS_SPACES}"), BenchBand.BATCH, BenchMode.LATENCY),
    _rostered(EvidenceScope.SCAN_REGISTRATION, ("multiscale", f"p{CORPUS_POINTS}"), BenchBand.BATCH, BenchMode.LATENCY),
    _rostered(EvidenceScope.SCAN_RECONSTRUCTION, ("poisson", f"p{CORPUS_POINTS}"), BenchBand.BATCH, BenchMode.LATENCY),
))


def graded(kernels: Map[str, BenchKernel], corpus: Block[BenchSubject] = CORPUS) -> RuntimeRail[Block[BenchVerdict]]:
    return Bench.graded(corpus, kernels)


def bench_seam(
    subject: str,
    seam: Callable[[], Awaitable[object]],
    *,
    rounds: int = 32,
    warmup: int = 4,
) -> RuntimeRail[Benchmark]:
    return Bench.run(subject, lambda: anyio.run(seam), rounds=rounds, warmup=warmup)


def bench_terminal(
    ctx: RuntimeContext,
    endpoint: str,
    run_id: str,
    subject: str,
    seam: Callable[[], Awaitable[object]],
    *,
    rounds: int = 32,
    warmup: int = 4,
) -> RuntimeRail[Benchmark]:
    return JobRun.bounded(
        ctx,
        endpoint,
        f"bench.{subject}",
        run_id,
        partial(bench_seam, subject, seam, rounds=rounds, warmup=warmup),
    ).bind(lambda inner: inner)


def registered(composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[GraduationInstall]:
    diverged = Block.of_seq(sorted(MEASURES, key=lambda row: row.measure)).choose(_diverged)
    if not diverged.is_empty():
        return Error(CHARTER_DIVERGED.raised(";".join(diverged)))
    roster = Block.of_seq(GeometryPulse).map(lambda point: HookPoint(id=point, payload=StageMark, modality=Modality(observe=None)))
    return Hooks.register(roster, scope=composition).map(
        lambda points: Hooks.installed(
            "geometry.graduation",
            GraduationInstall(points=tuple(row.id for row in points), measures=tuple(sorted(row.measure for row in MEASURES))),
            scope=composition,
        )
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
