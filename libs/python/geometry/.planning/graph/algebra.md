# [PY_GEOMETRY_GRAPH_ALGEBRA]

AEC computational and numerical geometry — one `@tagged_union` dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`), datastructure algebra over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, and a parameterized `NumericalOp` table folding best-fit/bbox/hull primitives AND the rigid/affine/similarity/projective transform rows into one keyed catalogue. There is no separate `graph/transform` owner: an affine map is a numerical op on a coordinate set, not a second concern. This owner is distinct from non-manifold topology (the `nonmanifold` sibling over `topologicpy`) and from raw mesh-file exchange, which defers to data `MeshPayload`.

Each case keys its `CASE` row for the `GeometrySubject` it crosses — `NUMERICAL_PRIMITIVE` retained beside `NETWORK_GRAPH`/`FORM_FINDING`/`MESH_ALGEBRA` — and `graduates()` returns the local `GeometryHandoff` whose `wire()` projection is the compute crossing. The `@receipted` aspect sits on the pure `_extract` with the `boundary` fence OUTSIDE it — a solve that raises is an `Error(BoundaryFault)`, never a synthetic zero-handle receipt — the same wiring the `nonmanifold` sibling carries. Proxy bring-up, teardown, and every RPC wait ride `lane.offload(Modality.THREAD)` with `RetryClass.RPC` on the cold start: the runtime-owned band, zero geometry-minted limiters.

## [01]-[INDEX]

- [01]-[ALGEBRA]: the `ComputationalGeometry` union, its `CASE`/`NUMERICAL`/`DATASTRUCTURE`/`_FORM` tables, and the sync/async `run`/`bridged` pair under one `ReceiptContributor`.

## [02]-[ALGEBRA]

- Owner: `ComputationalGeometry` discriminates by `AlgebraKind`, and the four per-case data axes — graduation subject, residual-ledger projector, ceiling, and whether the case graduates — are ONE `CASE` table, so a new kind is one row plus one union case, never a `_subject` match racing a parallel ledger fold. `AlgebraResult` is the sole `ReceiptContributor`, and its phase is data-driven — `emitted` for a converged/clean result, `admitted` for a form-finding pass whose residual exceeds `FormParams.tol` — so an unconverged equilibrium is flagged rather than asserted. The sub-op of every parameterized case is a closed `StrEnum`, never a raw string in the payload.
- Entry: `run` discriminates a single op or a batch over one fenced rail; `bridged` is the async mirror routing the SAME `_extract` through the `compas.rpc.Proxy`. The proxy reaches ONLY the scipy-backed heavy band — the `_dr`/`_tna` solvers and the `rpc`-routed `_numpy` primitives whose scipy cores must not block the companion in-process; the pure-Python transform rows carry `rpc=None`, so the proxy route is a per-row capability, never a blanket re-entry marshaling a matrix multiply across the process wall.
- Receipt: the network/numerical/datastructure cases key an `empty_handle_fraction` against the zero ceiling, so a vacuous result does not graduate; form-finding keys its solver residual against `_RESIDUAL_CEILING`; the residual census is the evidence the fold reads, never a re-measured value. `json_dumps` is the one COMPAS serializer for every result handle, never a per-type encoder.
- Packages: `compas`, `compas_dr`, and `compas_tna` per the fence imports, beside the runtime lane/fault/receipt rails and the graduation spine.
- Growth: a new algebra kind is one union case, one `match` arm, and one `CASE` row; a new numerical primitive or transform is one `NumericalOp` row plus one `NUMERICAL` entry — its RPC route a row field, never a parallel map; a new datastructure verb is one `DATASTRUCTURE` entry; a new form-finding engine is one `FormEngine` row plus one `_FORM` arm; a new geometric constraint is one `NodeConstraint` row — `Constraint.get_constraint_cls` dispatches on the decoded COMPAS-JSON, never a new arm; `compas_cem` admits as a `FormEngine.CEM` row once it ships `compas>=2.0` support.
- Boundary: non-manifold topology is the `nonmanifold` sibling's; raw mesh-file exchange defers to data `MeshPayload`; retry/telemetry rides the `boundary` fence subject and the `@receipted` aspect, never a second hand-rolled rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import contextlib
from collections.abc import AsyncIterator, Callable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Literal, assert_never

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
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.geometry.graduation import GeometryHandoff, GeometrySubject
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.resilience import RetryClass

# --- [TYPES] ----------------------------------------------------------------------------

type AlgebraKind = Literal["network", "form_finding", "numerical", "datastructure"]
type Phase = Literal["admitted", "emitted"]
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

REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # algebra facts carry no secret field
# mirrors the dr_numpy tol1 default and the FormParams.tol verdict, so the receipt and the graduation gate read one bar.
_RESIDUAL_CEILING: Final[float] = 1e-3
_HANDLE_CEILING: Final[float] = 0.0
# --- [MODELS] ---------------------------------------------------------------------------


class NodeConstraint(Struct, frozen=True):
    node: int  # constrained vertex index
    geometry: str  # COMPAS-JSON of the Plane/Line/Circle/Curve/Surface the node snaps to; Constraint dispatches on its decoded type
    damping: float = 0.1


class FormParams(Struct, frozen=True):
    target: float = 0.0  # vertical_from_zmax crown height (TNA)
    density: float = 1.0  # vertical_from_zmax force-density scale (TNA only; DR selfweight rides rho)
    rho: float = 1.0  # material density: SelfweightCalculator(density=rho) drives the DR and TNA selfweight load
    alpha: float = 100.0  # horizontal_numpy form/force weighting; 100 fixes the form diagram
    rk_steps: Literal[1, 2, 4] = 2
    kmax: int = 10000
    tol: float = 1e-3  # residual ceiling the convergence verdict folds against; mirrors the dr_numpy tol1 default
    constraints: tuple[NodeConstraint, ...] = ()  # each node snaps to its decoded constraint geometry each step; () = unconstrained


class FormResult(Struct, frozen=True):
    handles: tuple[str, ...]  # the equilibrium mesh/diagram COMPAS-JSON handles
    residual: float  # numpy-reduced max-abs residual (DR) or the vertical_from_zmax crown-scale (TNA)


class Census(Struct, frozen=True, gc=False):
    # `op` carries the case's sub-op StrEnum value; the network case has no sub-op and carries its edge count in the typed `edges` slot.
    kind: AlgebraKind
    handles: int
    inputs: int = 0
    edges: int = 0
    op: str = ""
    residual: float = 0.0


class CaseSpec(Struct, frozen=True):
    # one row per AlgebraKind: subject, Census-read ledger projector, per-key ceiling.
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: dict[str, float]


class NumericalSpec(Struct, frozen=True):
    # one row per NumericalOp pairing the local callable to its optional out-of-process RPC dotted-name.
    local: Callable[[Coords], object]
    rpc: str | None = None


class AlgebraResult(Struct, frozen=True):
    kind: AlgebraKind
    handles: tuple[str, ...]
    census: Census
    graduation_subject: GeometrySubject
    converged: bool = True

    def contribute(self) -> tuple[Receipt, ...]:
        # the Census int/float scalars ride the dict[str, object] slots natively — no str() coerce.
        phase: Phase = "emitted" if self.converged else "admitted"
        facts: dict[str, object] = structs.asdict(self.census)
        return (Receipt.of("geometry.graph.algebra", (phase, self.graduation_subject, facts)),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        spec = CASE[self.kind]
        return GeometryHandoff.of(self.graduation_subject, evidence_key, spec.ledger(self.census), spec.ceiling)


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


def _dispatch(algebra: ComputationalGeometry, *, proxy: Proxy | None = None) -> AlgebraResult:
    match algebra:
        case ComputationalGeometry(tag="network", network=(vertices, edges)):
            graph = Network.from_nodes_and_edges([list(v) for v in vertices], list(edges))
            return _result("network", (json_dumps(graph),), Census(kind="network", handles=1, inputs=len(vertices), edges=len(edges)))
        case ComputationalGeometry(tag="numerical", numerical=(points, op)):
            pts = [list(p) for p in points]
            spec = NUMERICAL[op]
            # scipy-backed _numpy rows offload out of process when bridged; pure-Python transform rows (rpc=None) stay in-thread.
            value = proxy.function(spec.rpc)(pts) if proxy and spec.rpc else spec.local(pts)
            return _result("numerical", (json_dumps(value),), Census(kind="numerical", handles=1, inputs=len(points), op=op))
        case ComputationalGeometry(tag="form_finding", form_finding=(mesh, anchors, engine, params)):
            form = _FORM[engine](Mesh.from_json(mesh), list(anchors), params, proxy)
            return _result(
                "form_finding",
                form.handles,
                Census(kind="form_finding", handles=len(form.handles), inputs=len(anchors), op=engine, residual=form.residual),
                converged=form.residual <= params.tol,
            )
        case ComputationalGeometry(tag="datastructure", datastructure=(payload, op)):
            return _result("datastructure", (json_dumps(DATASTRUCTURE[op](payload)),), Census(kind="datastructure", handles=1, op=op))
        case _ as unreachable:
            assert_never(unreachable)


def _result(kind: AlgebraKind, handles: tuple[str, ...], census: Census, *, converged: bool = True) -> AlgebraResult:
    return AlgebraResult(kind=kind, handles=handles, census=census, graduation_subject=CASE[kind].subject, converged=converged)


def _dr(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    xyz = mesh.vertices_attributes("xyz")
    # `SelfweightCalculator.__call__(xyz)` is `tributary_area * rho`, so `rho` drives the load as material
    # density; the `(N, 1)` magnitude broadcasts onto the down `z` of the `(N, 3)` load, counted once.
    weight = np.asarray(SelfweightCalculator(mesh, density=params.rho)(xyz), dtype=float).reshape(-1, 1)
    loads = (weight * (0.0, 0.0, -1.0)).tolist()
    indata = InputData.from_mesh(mesh, fixed=anchors, loads=loads, qpre=[1.0] * mesh.number_of_edges())
    constraints = tuple(Constraint(json_loads(c.geometry)) for c in params.constraints)
    solve = (
        proxy.function("compas_dr.solvers.dr_constrained_numpy" if constraints else "compas_dr.solvers.dr_numpy")
        if proxy
        else (dr_constrained_numpy if constraints else dr_numpy)
    )
    # `tol1=params.tol` threads the convergence bar into the solver so the residual gate AND the `converged`
    # verdict read one value; a default-`tol1` solve under a tighter `params.tol` verdict spuriously admits.
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
    # `LoadUpdater` mutates the Nx3 load buffer in place at column 2; the write-back MUST be a per-vertex
    # `vertex_attributes(key, names, row)` loop over the SAME `keys` ordering, because `vertices_attributes(names, values=X)`
    # broadcasts the WHOLE `X` onto every vertex — the plural row-write call is the broadcast defect, not a row distributor.
    loads = np.zeros_like(xyz)
    LoadUpdater(form, loads, density=params.rho)(loads, xyz)
    for key, row in zip(keys, loads.tolist()):
        form.vertex_attributes(key, ("px", "py", "pz"), row)
    horizontal = proxy.function("compas_tna.equilibrium.horizontal_numpy") if proxy else horizontal_numpy
    form, force = horizontal(form, force, alpha=params.alpha, kmax=params.kmax)
    form, scale = vertical_from_zmax(form, params.target, density=params.density)
    return FormResult((json_dumps(form), json_dumps(force)), scale)


# --- [TABLES] ---------------------------------------------------------------------------

# `bestfit_frame_numpy(pts)` returns the `(origin, xaxis, yaxis)` ndarray triple, NOT a `Frame`, so the RIGID/SIMILARITY rows
# wrap it as `Frame(*triple)` before the matrix constructors — `Transformation.from_frame_to_frame` requires a real `Frame`;
# the AFFINE/PROJECTIVE rows take the plain `centroid_points` list and the `bestfit_plane` (point, normal) tuple directly.
_TRANSFORM: Final[Mapping[NumericalOp, Callable[[Coords], Transformation]]] = MappingProxyType({
    NumericalOp.RIGID: lambda pts: Transformation.from_frame_to_frame(
        compas.geometry.Frame.worldXY(), compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))
    ),
    NumericalOp.AFFINE: lambda pts: Translation.from_vector(compas.geometry.centroid_points(pts)),
    NumericalOp.SIMILARITY: lambda pts: Scale.from_factors([1.0, 1.0, 1.0], compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))),
    NumericalOp.PROJECTIVE: lambda pts: Projection.from_plane(compas.geometry.bestfit_plane(pts)),
})

# the three _numpy primitives return raw ndarray shapes (frame triple / 8-corner list / (indices, faces) pair), NOT `Data`
# subclasses, yet one `json_dumps(value)` serializes them with no `.tolist()` coerce: COMPAS `DataEncoder` maps `ndarray` ->
# `tolist()` and numpy scalars -> Python `int`/`float` natively, so every row rides the same single serializer.
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


def _open_proxy() -> Proxy:
    # `Proxy(...)` eagerly reconnects to the running localhost server (port 1753) or spawns one through the blocking `start_server()`;
    # `autoreload=False` keeps the worker from reloading mid-fan. A cold-start `RPCServerError` retries under `RetryClass.RPC`.
    proxy = Proxy(url="http://127.0.0.1", autoreload=False, capture_output=True)
    proxy.__enter__()
    return proxy


@contextlib.asynccontextmanager
async def solver_proxy(lane: LanePolicy) -> AsyncIterator[Proxy]:
    # the one async-resource owner of the Proxy lifecycle: the ownership-aware `__exit__` runs `stop_server()` only when this
    # proxy spawned the server, and rides the same band so teardown is bounded even on a solve fault inside the scope. A
    # `bridged` fan enters this through one `AsyncExitStack`, so a fan of heavy solves shares ONE reconnected worker.
    proxy = (await lane.offload(_open_proxy, modality=Modality.THREAD, retry=RetryClass.RPC)).default_value(None)
    if proxy is None:
        msg = "compas.rpc.Proxy bring-up exhausted RetryClass.RPC"
        raise RuntimeError(msg)
    try:
        yield proxy
    finally:
        await lane.offload(lambda: proxy.__exit__(None, None, None), modality=Modality.THREAD)


@receipted(REDACTION)
def _extract(op: ComputationalGeometry, *, proxy: Proxy | None = None) -> AlgebraResult:
    return _dispatch(op, proxy=proxy)


def run(op: ComputationalGeometry | Sequence[ComputationalGeometry]) -> RuntimeRail[AlgebraResult] | RuntimeRail[Block[AlgebraResult]]:
    # a batch folds one Block of the SAME fenced rail through traversed(ACCUMULATE); the default `i=item` binds the loop
    # variable per closure so the comprehension never captures the last item.
    match op:
        case Sequence() as batch:
            return traversed(Block.of_seq([boundary(f"algebra.{item.tag}", lambda i=item: _extract(i)) for item in batch]), by=Disposition.ACCUMULATE)
        case ComputationalGeometry() as single:
            return boundary(f"algebra.{single.tag}", lambda: _extract(single))
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: ComputationalGeometry, proxy: Proxy, lane: LanePolicy) -> RuntimeRail[AlgebraResult]:
    # `bridged` is not itself @receipted — the aspect lives on `_extract` for both paths; the proxy is supplied by an
    # enclosing solver_proxy(lane) scope, never constructed per call.
    return await lane.offload(lambda: _extract(op, proxy=proxy), modality=Modality.THREAD)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
