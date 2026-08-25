# [PY_GEOMETRY_GRAPH_ALGEBRA]

AEC computational and numerical geometry — one `@tagged_union` dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`), datastructure algebra over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, and a parameterized `NumericalOp` table folding best-fit/bbox/hull primitives AND the rigid/affine/similarity/projective transform rows into one keyed catalogue. No separate `graph/transform` owner exists: an affine map is a numerical op on a coordinate set, not a second concern. This owner excludes non-manifold topology (the `nonmanifold` sibling over `topologicpy`) and raw mesh-file exchange, which defers to data `MeshPayload`.

Each case keys its `CASE` row for the `GeometrySubject` it crosses — `NUMERICAL_PRIMITIVE` beside `NETWORK_GRAPH`/`FORM_FINDING`/`MESH_ALGEBRA` — and `graduates()` derives its own content key off the receipt's `spec` and rails the `GeometryHandoff` whose `wire()` projection is the compute crossing. `run` and `bridged` return through the graduation `evidence_run` weave seeded `EvidenceScope.GRAPH_ALGEBRA` — span, fence, and receipt harvest in one composition, so a solve that raises is an `Error(BoundaryFault)` on the recorded rail, never a synthetic zero-handle receipt — the same wiring the `nonmanifold` sibling carries. Proxy bring-up, teardown, and every RPC wait cross as `RELEASING`-trait runtime `Kernel`s through `lane.offload`, bring-up under `RetryClass.RPC`: the runtime-owned thread band, zero geometry-minted limiters.

## [01]-[INDEX]

- [02]-[ALGEBRA]: `ComputationalGeometry` union, its `CASE`/`NUMERICAL`/`DATASTRUCTURE`/`_FORM` tables, the census-row `graduates`/`frame` egress pair, and the sync/async `run`/`bridged` pair under one `ReceiptContributor`.

## [02]-[ALGEBRA]

- Owner: `ComputationalGeometry` discriminates by `AlgebraKind`, and the four per-case data axes — graduation subject, residual-ledger projector, ceiling, and whether the case graduates — are ONE `CASE` table, so a new kind is one row and one union case, never a `_subject` match racing a parallel ledger fold. `AlgebraResult` is the sole `ReceiptContributor`, its phase data-driven — `emitted` for a converged/clean result, `admitted` for a form-finding pass whose residual exceeds `FormParams.tol` — so an unconverged equilibrium is flagged rather than asserted. Every parameterized case's sub-op is a closed `StrEnum`, never a raw string in the payload.
- Entry: `run` discriminates a single op or a batch, each returning through its own weave rail; `bridged` is the async mirror routing the SAME `_dispatch` through the `compas.rpc.Proxy`. `Proxy` reaches ONLY the scipy-backed heavy band — the `_dr`/`_tna` solvers and the `rpc`-routed `_numpy` primitives whose scipy cores must not block the companion in-process; the pure-Python transform rows carry `rpc=None`, so the proxy route is a per-row capability, never a blanket re-entry marshaling a matrix multiply across the process wall.
- Law: `Proxy` lifecycle is in-page — `solver_proxy(lane)` is the one async-resource owner: bring-up crosses as a `RELEASING` kernel under `retry=Some(RetryClass.RPC)` (the resilience row exists for this cold-start), teardown crosses the same band inside the scope exit, and a `bridged` fan shares ONE reconnected worker through an enclosing `AsyncExitStack`. Localhost server is per-session material the first proxy spawns and the spawning proxy stops, so no `Supervisor` DAEMON charge is minted at the serve composition root; RPC waits block on the socket, exactly the `RELEASING` trait's syscall arm.
- Receipt: network/numerical/datastructure cases key an `empty_handle_fraction` against the zero ceiling, so a vacuous result does not graduate; form-finding keys its solver residual against `_RESIDUAL_CEILING`; residual census is the evidence the fold reads, never a re-measured value. `json_dumps` is the one COMPAS serializer for every result handle, never a per-type encoder. `spec` is the handle set beside the kind and sub-op discriminants, and both egress ports fold it through the graduation spine's `evidence_key` mint, so `graduates()` and `frame()` key one evidence identically and neither takes a key from its caller; `frame` projects the census one row wide with columns DERIVED off the struct, since this owner's product is scalar census plus JSON handles rather than a reducer board. The `_result` construction site records the charter distribution once, keyed on the case's own subject — the form-finding residual reaches `rasm.geometry.form.residual` and the three uncharted subjects write nothing, the charter table answering which rather than a per-arm branch.
- Packages: `compas`, `compas_dr`, and `compas_tna` per the fence imports, beside the runtime lane/fault/receipt rails and the graduation spine.
- Growth: a new algebra kind is one union case, one `match` arm, and one `CASE` row, its charter distribution and frame projection following from the row with no fold edit; a new numerical primitive or transform is one `NumericalOp` row and one `NUMERICAL` entry — its RPC route a row field, never a parallel map; a new datastructure verb is one `DATASTRUCTURE` entry; a new form-finding engine is one `FormEngine` row and one `_FORM` arm; a new `Census` column reaches the receipt, the charter source, and the frame at once; a new geometric constraint is one `NodeConstraint` row — `Constraint.get_constraint_cls` dispatches on the decoded COMPAS-JSON, never a new arm; a new composition is one `ScopeKey` threaded through the `composition` keyword both entries carry; `compas_cem` admits as a `FormEngine.CEM` row once it ships `compas>=2.0` support.
- Boundary: non-manifold topology is the `nonmanifold` sibling's; raw mesh-file exchange defers to data `MeshPayload`; retry/telemetry rides the graduation weave's fence and harvest, never a second hand-rolled rail.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import contextlib
import sys
from collections.abc import AsyncIterator, Callable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from types import MappingProxyType
from typing import Final, Literal, assert_never

import anyio
import compas.geometry
import numpy as np
from compas import json_dumps, json_loads
from compas.datastructures import Assembly, Mesh, Network, VolMesh
from compas.geometry import NurbsSurface, Projection, Scale, Transformation, Translation
from compas.rpc import Proxy
from compas_dr.constraints import Constraint
from compas_dr.loads import SelfweightCalculator
from compas_dr.numdata import InputData, ResultData
from compas_dr.solvers import dr_constrained_numpy, dr_numpy
from compas_tna.diagrams import FormDiagram, ForceDiagram
from compas_tna.equilibrium import horizontal_numpy, relax_boundary_openings, vertical_from_zmax
from compas_tna.loads import LoadUpdater
from expression import Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, structs

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type AlgebraKind = Literal["network", "form_finding", "numerical", "datastructure"]
type Coords = list[list[float]]
type Points = tuple[tuple[float, float, float], ...]
type Edges = tuple[tuple[int, int], ...]


class FormEngine(StrEnum):
    DR = "dynamic-relaxation"
    TNA = "thrust-network"


class NumericalOp(StrEnum):
    BESTFIT_FRAME = "bestfit-frame"
    OBB = "oriented-bbox"
    CONVEX_HULL = "convex-hull"
    RIGID = "rigid"
    AFFINE = "affine"
    SIMILARITY = "similarity"
    PROJECTIVE = "projective"


class DatastructureOp(StrEnum):
    DUAL = "dual"
    SUBDIVIDE = "subdivide"
    VOLMESH_DUAL = "volmesh-dual"
    ASSEMBLY_GRAPH = "assembly-graph"
    SURFACE_TESSELLATE = "surface-tessellate"


# --- [CONSTANTS] ------------------------------------------------------------------------

_RESIDUAL_CEILING: Final[float] = 1e-3
_HANDLE_CEILING: Final[float] = 0.0
# --- [MODELS] ---------------------------------------------------------------------------


class NodeConstraint(Struct, frozen=True):
    node: int
    geometry: str
    damping: float = 0.1


class FormParams(Struct, frozen=True):
    target: float = 0.0
    density: float = 1.0
    rho: float = 1.0
    alpha: float = 100.0
    rk_steps: Literal[1, 2, 4] = 2
    kmax: int = 10000
    tol: float = 1e-3
    constraints: tuple[NodeConstraint, ...] = ()


class FormResult(Struct, frozen=True):
    handles: tuple[str, ...]
    residual: float


class Census(Struct, frozen=True, gc=False):
    kind: AlgebraKind
    handles: int
    inputs: int = 0
    edges: int = 0
    op: str = ""
    residual: float = 0.0


class CaseSpec(Struct, frozen=True):
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: dict[str, float]


class NumericalSpec(Struct, frozen=True):
    local: Callable[[Coords], object]
    rpc: str | None = None


class AlgebraResult(Struct, frozen=True):
    kind: AlgebraKind
    handles: tuple[str, ...]
    census: Census
    graduation_subject: GeometrySubject
    converged: bool = True

    def contribute(self) -> tuple[Receipt, ...]:
        phase: Phase = "emitted" if self.converged else "admitted"
        facts: dict[str, object] = structs.asdict(self.census)
        return (Receipt.of("rasm.geometry.graph.algebra", (phase, self.graduation_subject, facts)),)

    @property
    def spec(self) -> bytes:
        return b"|".join((self.kind.encode(), self.census.op.encode(), *(handle.encode() for handle in self.handles)))

    def graduates(self) -> GeometryHandoff:
        case_spec = CASE[self.kind]
        return GeometryHandoff.of(
            self.graduation_subject, evidence_key(self.graduation_subject, self.spec), case_spec.ledger(self.census), case_spec.ceiling
        )

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        table: dict[str, list[object]] = {name: [value] for name, value in structs.asdict(self.census).items()} | {"converged": [self.converged]}
        return EvidenceFrame.of(self.graduation_subject, evidence_key(self.graduation_subject, self.spec), table)


# --- [OPERATIONS] -----------------------------------------------------------------------


@tagged_union(frozen=True)
class ComputationalGeometry:
    tag: AlgebraKind = tag()
    network: tuple[Points, Edges] = case()
    form_finding: tuple[str, tuple[int, ...], FormEngine, FormParams] = case()
    numerical: tuple[Points, NumericalOp] = case()
    datastructure: tuple[str, DatastructureOp] = case()

    @staticmethod
    def Network(vertices: Points, edges: Edges) -> "ComputationalGeometry":
        return ComputationalGeometry(network=(vertices, edges))

    @staticmethod
    def FormFinding(mesh: str, anchors: tuple[int, ...], engine: FormEngine, params: FormParams = FormParams()) -> "ComputationalGeometry":
        return ComputationalGeometry(form_finding=(mesh, anchors, engine, params))

    @staticmethod
    def Numerical(points: Points, op: NumericalOp) -> "ComputationalGeometry":
        return ComputationalGeometry(numerical=(points, op))

    @staticmethod
    def Datastructure(payload: str, op: DatastructureOp) -> "ComputationalGeometry":
        return ComputationalGeometry(datastructure=(payload, op))


def _dispatch(algebra: ComputationalGeometry, *, proxy: Proxy | None = None, composition: ScopeKey = DEFAULT_SCOPE) -> AlgebraResult:
    match algebra:
        case ComputationalGeometry(tag="network", network=(vertices, edges)):
            graph = Network.from_nodes_and_edges([list(v) for v in vertices], list(edges))
            return _result(
                "network", (json_dumps(graph),), Census(kind="network", handles=1, inputs=len(vertices), edges=len(edges)), composition=composition
            )
        case ComputationalGeometry(tag="numerical", numerical=(points, op)):
            pts = [list(p) for p in points]
            spec = NUMERICAL[op]
            value = proxy.function(spec.rpc)(pts) if proxy and spec.rpc else spec.local(pts)
            return _result(
                "numerical", (json_dumps(value),), Census(kind="numerical", handles=1, inputs=len(points), op=op), composition=composition
            )
        case ComputationalGeometry(tag="form_finding", form_finding=(mesh, anchors, engine, params)):
            form = _FORM[engine](Mesh.from_json(mesh), list(anchors), params, proxy)
            return _result(
                "form_finding",
                form.handles,
                Census(kind="form_finding", handles=len(form.handles), inputs=len(anchors), op=engine, residual=form.residual),
                converged=form.residual <= params.tol,
                composition=composition,
            )
        case ComputationalGeometry(tag="datastructure", datastructure=(payload, op)):
            return _result(
                "datastructure", (json_dumps(DATASTRUCTURE[op](payload)),), Census(kind="datastructure", handles=1, op=op), composition=composition
            )
        case _ as unreachable:
            assert_never(unreachable)


def _result(
    kind: AlgebraKind, handles: tuple[str, ...], census: Census, *, converged: bool = True, composition: ScopeKey = DEFAULT_SCOPE
) -> AlgebraResult:
    subject = CASE[kind].subject
    charter_record(subject, structs.asdict(census), composition=composition)
    return AlgebraResult(kind=kind, handles=handles, census=census, graduation_subject=subject, converged=converged)


def _dr(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    xyz = mesh.vertices_attributes("xyz")
    weight = np.asarray(SelfweightCalculator(mesh, density=params.rho)(xyz), dtype=float).reshape(-1, 1)
    loads = (weight * (0.0, 0.0, -1.0)).tolist()
    indata = InputData.from_mesh(mesh, fixed=anchors, loads=loads, qpre=[1.0] * mesh.number_of_edges())
    constraints = tuple(Constraint(json_loads(c.geometry)) for c in params.constraints)
    solve = (
        proxy.function("compas_dr.solvers.dr_constrained_numpy" if constraints else "compas_dr.solvers.dr_numpy")
        if proxy
        else (dr_constrained_numpy if constraints else dr_numpy)
    )
    result: ResultData = (
        solve(indata=indata, constraints=list(constraints), kmax=params.kmax, tol1=params.tol, rk_steps=params.rk_steps)
        if constraints
        else solve(indata, kmax=params.kmax, tol1=params.tol, rk_steps=params.rk_steps)
    )
    result.update_mesh(mesh)
    return FormResult((json_dumps(mesh), json_dumps(result)), float(np.abs(np.asarray(result.residuals, dtype=float)).max(initial=0.0)))


def _tna(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    form = relax_boundary_openings(FormDiagram.from_mesh(mesh), anchors)
    force = ForceDiagram.from_formdiagram(form)
    keys = list(form.vertices())
    xyz = np.asarray(form.vertices_attributes("xyz", keys=keys), dtype=float)
    loads = np.zeros_like(xyz)
    LoadUpdater(form, loads, density=params.rho)(loads, xyz)
    for key, row in zip(keys, loads.tolist()):
        form.vertex_attributes(key, ("px", "py", "pz"), row)
    horizontal = proxy.function("compas_tna.equilibrium.horizontal_numpy") if proxy else horizontal_numpy
    form, force = horizontal(form, force, alpha=params.alpha, kmax=params.kmax)
    form, scale = vertical_from_zmax(form, params.target, density=params.density)
    return FormResult((json_dumps(form), json_dumps(force)), scale)


# --- [TABLES] ---------------------------------------------------------------------------

_TRANSFORM: Final[Mapping[NumericalOp, Callable[[Coords], Transformation]]] = MappingProxyType({
    NumericalOp.RIGID: lambda pts: Transformation.from_frame_to_frame(
        compas.geometry.Frame.worldXY(), compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))
    ),
    NumericalOp.AFFINE: lambda pts: Translation.from_vector(compas.geometry.centroid_points(pts)),
    NumericalOp.SIMILARITY: lambda pts: Scale.from_factors([1.0, 1.0, 1.0], compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))),
    NumericalOp.PROJECTIVE: lambda pts: Projection.from_plane(compas.geometry.bestfit_plane(pts)),
})

NUMERICAL: Final[Mapping[NumericalOp, NumericalSpec]] = MappingProxyType({
    NumericalOp.BESTFIT_FRAME: NumericalSpec(compas.geometry.bestfit_frame_numpy, "compas.geometry.bestfit_frame_numpy"),
    NumericalOp.OBB: NumericalSpec(compas.geometry.oriented_bounding_box_numpy, "compas.geometry.oriented_bounding_box_numpy"),
    NumericalOp.CONVEX_HULL: NumericalSpec(compas.geometry.convex_hull_numpy, "compas.geometry.convex_hull_numpy"),
    **{op: NumericalSpec(fn) for op, fn in _TRANSFORM.items()},
})

DATASTRUCTURE: Final[Mapping[DatastructureOp, Callable[[str], object]]] = MappingProxyType({
    DatastructureOp.DUAL: lambda p: Mesh.from_json(p).dual(),
    DatastructureOp.SUBDIVIDE: lambda p: Mesh.from_json(p).subdivide(),
    DatastructureOp.VOLMESH_DUAL: lambda p: VolMesh.from_json(p).dual(),
    DatastructureOp.ASSEMBLY_GRAPH: lambda p: Assembly.from_json(p).graph,
    DatastructureOp.SURFACE_TESSELLATE: lambda p: NurbsSurface.from_json(p).to_mesh(),
})

_FORM: Final[Mapping[FormEngine, Callable[[Mesh, list[int], FormParams, Proxy | None], FormResult]]] = MappingProxyType({
    FormEngine.DR: _dr,
    FormEngine.TNA: _tna,
})

CASE: Final[Mapping[AlgebraKind, CaseSpec]] = MappingProxyType({
    "network": CaseSpec(GeometrySubject.NETWORK_GRAPH, lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}),
    "form_finding": CaseSpec(GeometrySubject.FORM_FINDING, lambda c: {"residual": c.residual}, {"residual": _RESIDUAL_CEILING}),
    "numerical": CaseSpec(
        GeometrySubject.NUMERICAL_PRIMITIVE, lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}
    ),
    "datastructure": CaseSpec(
        GeometrySubject.MESH_ALGEBRA, lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}
    ),
})

# --- [COMPOSITION] ----------------------------------------------------------------------


def _raised(phase: str, fault: BoundaryFault) -> RuntimeError:
    raised = RuntimeError(fault)
    raised.add_note(f"<at:solver_proxy.{phase}>")
    return raised


def _open_proxy() -> Proxy:
    proxy = Proxy(url="http://127.0.0.1", autoreload=False, capture_output=True)
    proxy.__enter__()
    return proxy


@contextlib.asynccontextmanager
async def solver_proxy(lane: LanePolicy) -> AsyncIterator[Proxy]:
    match await lane.offload(Kernel.of(_open_proxy, KernelTrait.RELEASING, retry=Some(RetryClass.RPC))):
        case Result(tag="error", error=fault):
            raise _raised("bring-up", fault)
        case Result(tag="ok", ok=proxy):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    try:
        yield proxy
    finally:
        with anyio.CancelScope(shield=True):
            closed = await lane.offload(Kernel.of(lambda: proxy.__exit__(None, None, None), KernelTrait.RELEASING))
        match closed, sys.exception():
            case (Result(tag="error", error=fault), None):
                raise _raised("teardown", fault)
            case (Result(tag="error", error=fault), active) if isinstance(active, anyio.get_cancelled_exc_class()):
                active.add_note(f"<solver-proxy-teardown:{fault}>")
            case (Result(tag="error", error=fault), BaseException() as active):
                raise BaseExceptionGroup("solver_proxy", [active, _raised("teardown", fault)]) from None
            case _:
                pass


def run(
    op: ComputationalGeometry | Sequence[ComputationalGeometry], *, composition: ScopeKey = DEFAULT_SCOPE
) -> RuntimeRail[AlgebraResult] | RuntimeRail[Block[AlgebraResult]]:
    match op:
        case Sequence() as batch:
            return traversed(
                Block.of_seq([
                    evidence_run(
                        EvidenceScope.GRAPH_ALGEBRA,
                        f"run.{item.tag}",
                        lambda i=item: _dispatch(i, composition=composition),
                        composition=composition,
                    )
                    for item in batch
                ]),
                by=Disposition.ACCUMULATE,
            )
        case ComputationalGeometry() as single:
            return evidence_run(
                EvidenceScope.GRAPH_ALGEBRA, f"run.{single.tag}", lambda: _dispatch(single, composition=composition), composition=composition
            )
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: ComputationalGeometry, proxy: Proxy, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[AlgebraResult]:
    return await evidence_run(
        EvidenceScope.GRAPH_ALGEBRA,
        f"bridged.{op.tag}",
        partial(lane.offload, Kernel.of(lambda: _dispatch(op, proxy=proxy, composition=composition), KernelTrait.RELEASING)),
        composition=composition,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
