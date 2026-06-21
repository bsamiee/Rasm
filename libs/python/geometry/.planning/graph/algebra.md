# [PY_GEOMETRY_GRAPH_ALGEBRA]

AEC computational and numerical geometry. `ComputationalGeometry` is one `@tagged_union` dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`, selected by a `FormEngine` sub-enum on the one form-finding case), datastructure algebra over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, and a parameterized `NumericalOp` table that folds best-fit/bbox/hull primitives AND the rigid/affine/similarity/projective transform rows into one keyed catalogue. There is no separate `graph/transform` owner: an affine map is a numerical op on a coordinate set, not a second concern. Every arm carries the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`) keyed to its `AlgebraKind`, never a bare `str`, and the `geometry` `HandoffAxis` case is the single graduation rail. This owner is distinct from non-manifold topology (the `nonmanifold` sibling over `topologicpy`) and from raw mesh-file exchange at the data seam, which defers to data `MeshPayload`.

`compas` is the gated companion band — `compas>=2.15.1; python_version<'3.15'`, the cp312 floor forced by the scipy-family CPython 3.15 lag — alongside `compas_dr` and `compas_tna`; the owner and fences stay authored, dark on the intended cp315 core until the wheels ship.

## [01]-[INDEX]

- [01]-[ALGEBRA]: the `ComputationalGeometry` `@tagged_union` of algebra kinds, the `CASE` subject/ledger/ceiling spec table, the `NUMERICAL` primitive-plus-transform table with its per-row RPC route, the `DATASTRUCTURE` and `_FORM` op tables, the woven `compas` rail with its `anyio.to_thread.run_sync` proxy offload, the typed `Census` and `FormResult` value objects, the `@receipted` aspect, the polymorphic single-or-batch capsule, and the per-case `graduates` rail under one `ReceiptContributor`.

## [02]-[ALGEBRA]

- Owner: `ComputationalGeometry` — the `@tagged_union(frozen=True)` discriminating by `AlgebraKind`; `AlgebraResult` the typed receipt carrying the case kind as the closed `Literal` tag (never `str`), the COMPAS-JSON result handles, a typed `Census` value object, and the case-keyed `GeometrySubject`. The four per-case data axes — graduation subject, residual-ledger projector, ceiling, and whether the case graduates — are ONE `AlgebraKind`-keyed `CASE` `CaseSpec` table, never a `_subject` `match` racing a parallel ledger fold: the case already names its kind, so a new algebra kind is one `CaseSpec` row and one `@tagged_union` case, never a subject map drifting against the discriminant (the parallel-subject defect the compute `graduation/handoff#HandoffAxis` owner forbids). `AlgebraResult` is the sole `ReceiptContributor`: `contribute` is the one emission, projecting `Census` through `msgspec.structs.asdict` into one phase-keyed `Receipt.of("geometry.graph.algebra", (phase, subject, facts))` row under the verified runtime two-argument factory — `phase="emitted"` for a converged/clean result and `phase="admitted"` for the entry caveat a non-converged form-finding pass keys off the residual census — so the admitted and emitted rows ride the one contributor path, never a discarded `Receipt.of` minted inside a decorator that never reaches the sink. The facts ride as native `dict[str, object]` carrying the `Census` `int`/`float` scalars; the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them without a `str()` coerce, so a pre-`str()` `dict[str, str]` map is the deleted form that owner rejects. Retry/telemetry rides the `boundary(f"algebra.{tag}")` fence subject and the `@receipted` aspect, not a second hand-rolled rail.
- Cases: `ComputationalGeometry` cases `Network(vertices, edges)` (compas `Network.from_nodes_and_edges` adjacency, `network-graph` subject) · `FormFinding(mesh, anchors, engine, params)` (the one structural case keyed by the `FormEngine` sub-enum carrying a typed `FormParams` value object — never two semantics crammed into one bare `float` — `DR` weaving `SelfweightCalculator.compute_tributary_areas` selfweight loads, the `Constraint`-family node projection, `dr_numpy`/`dr_constrained_numpy` with `FormParams.rk_steps`, and `ResultData.update_mesh` write-back into one rail; `TNA` weaving `FormDiagram.from_mesh`, `relax_boundary_openings`, the `LoadUpdater` applied to write tributary selfweight (never constructed and discarded), the `ForceDiagram.from_formdiagram` reciprocal dual, `horizontal_numpy`, and `vertical_from_zmax` capturing the returned crown-scale — `form-finding` subject) · `Numerical(points, op)` (the closed `NumericalOp`-keyed `NUMERICAL` table folding `compas.geometry` best-fit/bbox/hull free functions AND the transform-matrix constructors, `numerical-primitive` subject) · `Datastructure(payload, op)` (the `DatastructureOp`-keyed `DATASTRUCTURE` table over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, `mesh-algebra` subject) — matched by total `match`/`case` closed with `assert_never`. The sub-op of every parameterized case is a closed `StrEnum`, never a raw string literal inside the payload.
- Entry: `Algebra.run` is the one polymorphic entrypoint over the `ComputationalGeometry` op union, discriminating a single op or a batch `Sequence[ComputationalGeometry]` — a single op lifts through one `_run` wrapped in `boundary(f"algebra.{op.tag}", ...)` so the exception-to-fault lift is the single runtime seam, a batch builds a `Block` of per-op rails in one comprehension and folds them through `runtime.faults.traversed` (`Disposition.ACCUMULATE`) so one fault stays addressable in the aggregate while every successful `AlgebraResult` already landed on the contributed stream. The `@receipted(REDACTION)` aspect harvests `contribute` on exit, so the public surface returns the `ReceiptContributor` capsule the caller reads, never a per-op rail the aspect would discard. `bridged` is the async mirror routing the same `_dispatch` through the `compas.rpc.Proxy` out-of-process CPython solver when the case is heavy, folding the blocking proxy call through `anyio.to_thread.run_sync` so the sync solver becomes the awaitable thunk `async_boundary` requires and the in-process and out-of-process paths share one fault rail rather than a parallel async surface. The proxy reaches exactly the scipy-backed heavy band — the `_dr`/`_tna` numpy solvers and the `NumericalSpec.rpc`-routed `bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy` primitives whose scipy spatial/linalg cores must not block the gated companion in-process; the pure-Python transform-matrix rows carry `rpc=None`, the `Network` adjacency build and the datastructure `from_json` algebra carry no heavy solver, so the proxy route is a per-row capability (`spec.rpc is not None`) rather than a blanket re-entry that pointlessly marshals a matrix multiply across the process wall.
- Auto: the numerical case folds the `NUMERICAL` `NumericalSpec` table by `NumericalOp` — each row pairs the local callable to its optional RPC dotted-name, so best-fit/bbox/hull primitives reach the `_numpy`-accelerated variants routed out-of-process through `spec.rpc` when a proxy is supplied while the transform rows compose a `Transformation` from `Translation`/`Scale`/`Projection` constructors with `rpc=None`, and a new map is one table row, never a dispatch branch and never a parallel `_NUMERICAL_RPC` map drifting against the op table. The form-finding case threads the mesh, anchor set, and `FormParams` through the `FormEngine`-selected solver into a typed `FormResult`: `DR` weaving `numpy`-vectorized selfweight, optional geometric constraints, the RK-order solver, and `ResultData.update_mesh` write-back so the equilibrium mesh (not the raw `ResultData`) is the handle and the `numpy`-reduced max-abs residual the receipt fact, and `TNA` weaving the `LoadUpdater` applied to the form, the `ForceDiagram.from_formdiagram` reciprocal pair, and the `vertical_from_zmax` crown-scale. The datastructure case folds the `Mesh.dual`/`Mesh.subdivide`/`VolMesh`/`Assembly`/`NurbsSurface` algebra over `from_json` payloads; `json_dumps` serializes every result through the one COMPAS serializer for graduation, never a per-type encoder. The form-finding residual sets `AlgebraResult.converged` against `FormParams.tol`, so the contributor phase is data-driven, not a constant.
- Receipt: each evaluation produces an `AlgebraResult` that is the sole `ReceiptContributor`; `contribute` returns the one-element `tuple[Receipt, ...]` the port streams, minting one phase-keyed `Receipt.of("geometry.graph.algebra", (phase, subject, facts))` row — `phase="emitted"` for a converged/clean result and `phase="admitted"` for a form-finding pass whose `numpy`-reduced residual exceeds `FormParams.tol` (the caveat row, so the unconverged equilibrium is flagged rather than asserted), the facts being the `Census` value object (`structs.asdict`-projected) carrying the case kind, the input vertex/point/edge census, the result handle count, and the solver residual/crown-scale where the case produces one. `graduates` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=self.graduation_subject)`, gating the case's `CaseSpec.ledger`-projected residual against its `CaseSpec.ceiling` — the form-finding `residual` against `_RESIDUAL_CEILING`, the network/numerical/datastructure cases their `empty_handle_fraction` against the zero ceiling so a vacuous result (no handle produced) does not graduate — so a result breaching its ceiling is an `Error(BoundaryFault)` on the rail rather than a graduated handoff, all keys riding the compute owner's residual-over-ceiling `_admit` direction unchanged. The residual census is the evidence that fold reads, never a re-measured value.
- Packages: `compas` (`datastructures.Network`/`Mesh`/`VolMesh`/`Assembly`/`NurbsSurface`, `geometry.bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy`/`bestfit_plane`/`centroid_points`/`Frame.worldXY`/`Transformation`/`Translation`/`Scale`/`Projection`, `json_dumps`/`json_loads`, `rpc.Proxy`), `compas_dr` (`numdata.InputData.from_mesh`/`numdata.ResultData.update_mesh`/`numdata.ResultData.residuals`/`solvers.dr_numpy`/`solvers.dr_constrained_numpy`/`constraints.Constraint`/`loads.SelfweightCalculator`), `compas_tna` (`diagrams.FormDiagram.from_mesh`/`diagrams.ForceDiagram.from_formdiagram`/`loads.LoadUpdater`/`equilibrium.relax_boundary_openings`/`equilibrium.horizontal_numpy`/`equilibrium.vertical_from_zmax`), `anyio` (`to_thread.run_sync`), `numpy` (`asarray`/`abs`/`max` over the load and residual reductions), `expression` (`tagged_union`/`case`/`tag`, `Block`), `msgspec` (`Struct`/`structs.asdict`), runtime (`RuntimeRail`/`boundary`/`async_boundary`/`traversed`/`Disposition`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `ContentKey`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`).
- Growth: a new algebra kind is one `ComputationalGeometry` case, one `match` arm, and one `CASE` `CaseSpec` row carrying its subject/ledger/ceiling; a new numerical primitive or transform is one `NumericalOp` row plus one `NUMERICAL` `NumericalSpec` entry (its RPC route a row field, never a parallel map); a new datastructure verb is one `DatastructureOp` row plus one `DATASTRUCTURE` entry; a new form-finding engine is one `FormEngine` row plus one `_FORM` arm; a new geometric constraint is one `NodeConstraint` row whose decoded `compas.geometry` the `Constraint.get_constraint_cls` dispatch already resolves, never a new arm — `compas_cem` constrained-equilibrium admits as a `FormEngine.CEM` row once it ships `compas>=2.0` support, a RESEARCH-gated growth axis on the existing case, never a new case or page; zero new surface.
- Boundary: non-manifold cell/aperture topology is the `nonmanifold` sibling over `topologicpy`, never folded here; robust mesh repair/boolean is the `mesh/repair` sibling over `trimesh`/`manifold3d`, not the compas datastructure algebra; raw mesh-file decode/encode and GLB preview stay at the data `MeshPayload` seam — `run` returns COMPAS-JSON handles across the wire and never writes a mesh file; visualization-scene/USD/GLTF/OBJ export is the artifacts figures/scene owner; the rigid/affine/similarity/projective transform rows live HERE as `NumericalOp` table rows, never as a parallel `graph/transform` page; `compas.rpc.Proxy` is the out-of-process solver bridge for the gated companion, never an in-process re-entry, and the blocking proxy call crosses to the event loop only through `anyio.to_thread.run_sync` so the `async_boundary` thunk is a real awaitable rather than a sync lambda the awaitable rail would reject. The `network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` subjects cross from this owner on the one geometry `HandoffAxis` case keyed per `compas` dispatch case, distinct from the `network-graph` arm the `features` sibling also produces (mesh-feature projection there, compas adjacency here), never folded into one file. The deleted forms: a four-positional `Receipt.of(phase, owner, subject, facts)` against the runtime two-argument `Receipt.of(owner, (phase, subject, facts))` contract; a `str()`-coerced `dict[str, str]` facts map where the `EventDict` carries native `dict[str, object]`; a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port; a `_subject` `match` racing a parallel ledger fold where the `CASE` `CaseSpec` table owns subject and ledger together; a parallel `_NUMERICAL_RPC` map drifting against `NUMERICAL` where the RPC route is a `NumericalSpec` row field; a `form.dual_diagram(FormDiagram)` call passing the uncited `cls` argument where `ForceDiagram.from_formdiagram(form)` is the catalogue-confirmed reciprocal factory; an `AlgebraResult` minting a `graduation_subject` it never crosses where `graduates` folds the per-case ledger through the one admission rail; a second batch method where the one `traversed` fold drains a `Sequence`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Literal, Mapping, assert_never

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
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import Disposition, RuntimeRail, async_boundary, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

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
# the form-finding equilibrium residual ceiling the graduation fold gates; mirrors the dr_numpy tol1 default
# and the FormParams.tol convergence verdict, so the receipt and the graduation gate read one bar.
_RESIDUAL_CEILING: Final[float] = 1e-3
# the non-solver cases gate an empty-handle fraction: a vacuous result that produced no handle breaches 0.0.
_HANDLE_CEILING: Final[float] = 0.0

# --- [MODELS] ---------------------------------------------------------------------------


class NodeConstraint(Struct, frozen=True):
    node: int  # constrained vertex index
    geometry: str  # COMPAS-JSON of the Plane/Line/Circle/Curve/Surface the node snaps to; Constraint dispatches on its decoded type
    damping: float = 0.1


class FormParams(Struct, frozen=True):
    target: float = 0.0  # vertical_from_zmax crown height
    density: float = 1.0
    rho: float = 1.0
    alpha: float = 100.0  # horizontal_numpy form/force weighting; 100 fixes the form diagram
    rk_steps: Literal[1, 2, 4] = 2
    kmax: int = 10000
    tol: float = 1e-3  # residual ceiling the convergence verdict folds against; mirrors the dr_numpy tol1 default
    constraints: tuple[NodeConstraint, ...] = ()  # each node snaps to its decoded constraint geometry each step; () = unconstrained


class FormResult(Struct, frozen=True):
    handles: tuple[str, ...]  # the equilibrium mesh/diagram COMPAS-JSON handles
    residual: float  # numpy-reduced max-abs residual (DR) or the vertical_from_zmax crown-scale (TNA)


class Census(Struct, frozen=True, gc=False):
    kind: AlgebraKind
    handles: int
    inputs: int = 0
    op: str = ""
    residual: float = 0.0


class CaseSpec(Struct, frozen=True):
    # one row per AlgebraKind: the graduation subject the case crosses, the residual-ledger projector reading
    # the Census, and the per-key ceiling the graduation fold gates — so a new kind is one row, never a
    # _subject match racing a parallel ledger fold drifting against the discriminant.
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: dict[str, float]


class NumericalSpec(Struct, frozen=True):
    # one row per NumericalOp pairing the local callable to its optional out-of-process RPC dotted-name; a
    # scipy-backed _numpy primitive carries its rpc path, a pure-Python transform row carries rpc=None, so
    # the proxy route is a row field rather than a parallel _NUMERICAL_RPC map drifting against the table.
    local: Callable[[Coords], object]
    rpc: str | None = None


class AlgebraResult(Struct, ReceiptContributor, frozen=True):
    kind: AlgebraKind
    handles: tuple[str, ...]
    census: Census
    graduation_subject: GeometrySubject
    converged: bool = True

    def contribute(self) -> tuple[Receipt, ...]:
        # runtime two-argument Receipt.of(owner, (phase, subject, facts)) contract; the Census int/float
        # scalars ride the EventDict dict[str, object] slots the enc_hook=repr renderer serializes natively.
        phase: Phase = "emitted" if self.converged else "admitted"
        facts: dict[str, object] = structs.asdict(self.census)
        return (Receipt.of("geometry.graph.algebra", (phase, self.graduation_subject, facts)),)

    def graduates(self, evidence_key: ContentKey) -> "RuntimeRail[GraduationReceipt]":
        # the per-case CaseSpec supplies subject, the Census-read residual ledger, and the ceiling, folded
        # through the one compute residual-over-ceiling admission; never a re-measured value or a second gate.
        spec = CASE[self.kind]
        return GraduationReceipt.graduates(
            "geometry.graph.algebra", HandoffAxis(geometry=self.graduation_subject), evidence_key,
            spec.ledger(self.census), spec.ceiling,
        )


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
            return _result("network", (json_dumps(graph),), Census(kind="network", handles=1, inputs=len(vertices), op=str(len(edges))))
        case ComputationalGeometry(tag="numerical", numerical=(points, op)):
            pts = [list(p) for p in points]
            spec = NUMERICAL[op]
            # scipy-backed _numpy rows offload out of process when bridged; pure-Python transform rows (rpc=None) stay in-thread.
            value = proxy.function(spec.rpc)(pts) if proxy and spec.rpc else spec.local(pts)
            return _result("numerical", (json_dumps(value),), Census(kind="numerical", handles=1, inputs=len(points), op=op))
        case ComputationalGeometry(tag="form_finding", form_finding=(mesh, anchors, engine, params)):
            form = _FORM[engine](Mesh.from_json(mesh), list(anchors), params, proxy)
            return _result("form_finding", form.handles, Census(kind="form_finding", handles=len(form.handles), inputs=len(anchors), op=engine, residual=form.residual), converged=form.residual <= params.tol)
        case ComputationalGeometry(tag="datastructure", datastructure=(payload, op)):
            return _result("datastructure", (json_dumps(DATASTRUCTURE[op](payload)),), Census(kind="datastructure", handles=1, op=op))
        case _ as unreachable:
            assert_never(unreachable)


def _result(kind: AlgebraKind, handles: tuple[str, ...], census: Census, *, converged: bool = True) -> AlgebraResult:
    return AlgebraResult(kind=kind, handles=handles, census=census, graduation_subject=CASE[kind].subject, converged=converged)


def _dr(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    xyz = mesh.vertices_attributes("xyz")
    loads = (np.asarray(SelfweightCalculator(mesh, density=params.rho).compute_tributary_areas(xyz)) * (0.0, 0.0, -params.density)).tolist()
    indata = InputData.from_mesh(mesh, fixed=anchors, loads=loads, qpre=[1.0] * mesh.number_of_edges())
    constraints = tuple(Constraint(json_loads(c.geometry)) for c in params.constraints)
    solve = proxy.function("compas_dr.solvers.dr_constrained_numpy" if constraints else "compas_dr.solvers.dr_numpy") if proxy else (dr_constrained_numpy if constraints else dr_numpy)
    result: ResultData = solve(indata=indata, constraints=list(constraints), kmax=params.kmax, rk_steps=params.rk_steps) if constraints else solve(indata, kmax=params.kmax, rk_steps=params.rk_steps)
    result.update_mesh(mesh)
    return FormResult((json_dumps(mesh), json_dumps(result)), float(np.abs(np.asarray(result.residuals, dtype=float)).max(initial=0.0)))


def _tna(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    form = relax_boundary_openings(FormDiagram.from_mesh(mesh), anchors)
    force = ForceDiagram.from_formdiagram(form)
    xyz = np.asarray(form.vertices_attributes("xyz"), dtype=float)
    # LoadUpdater(mesh, p0, density)(p, xyz) writes tributary selfweight into p[:, 2] in place against xyz.
    LoadUpdater(form, xyz, density=params.rho)(xyz, xyz)
    horizontal = proxy.function("compas_tna.equilibrium.horizontal_numpy") if proxy else horizontal_numpy
    form, force = horizontal(form, force, alpha=params.alpha, kmax=params.kmax)
    form, scale = vertical_from_zmax(form, params.target, density=params.density)
    return FormResult((json_dumps(form), json_dumps(force)), scale)


# --- [TABLES] ---------------------------------------------------------------------------

_TRANSFORM: Final[Mapping[NumericalOp, Callable[[Coords], Transformation]]] = MappingProxyType({
    NumericalOp.RIGID: lambda pts: Transformation.from_frame_to_frame(
        compas.geometry.Frame.worldXY(), compas.geometry.bestfit_frame_numpy(pts)
    ),
    NumericalOp.AFFINE: lambda pts: Translation.from_vector(compas.geometry.centroid_points(pts)),
    NumericalOp.SIMILARITY: lambda pts: Scale.from_factors([1.0, 1.0, 1.0], compas.geometry.bestfit_frame_numpy(pts)),
    NumericalOp.PROJECTIVE: lambda pts: Projection.from_plane(compas.geometry.bestfit_plane(pts)),
})

# each NumericalSpec pairs the local callable to its optional RPC dotted-name; the three scipy-backed _numpy
# primitives carry their rpc path, the four pure-Python transform rows carry rpc=None — the proxy route is a
# row field, never a parallel map. A new transform admits its constructor the same gated way.
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

# one row per AlgebraKind owning the subject the case crosses, the Census-read residual ledger, and the per-key
# ceiling the graduation fold gates: the form-finding case keys its solver residual, every other case keys an
# empty-handle fraction so a vacuous result does not graduate. A new kind is one row.
CASE: Final[Mapping[AlgebraKind, CaseSpec]] = MappingProxyType({
    "network": CaseSpec("network-graph", lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}),
    "form_finding": CaseSpec("form-finding", lambda c: {"residual": c.residual}, {"residual": _RESIDUAL_CEILING}),
    "numerical": CaseSpec("numerical-primitive", lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}),
    "datastructure": CaseSpec("mesh-algebra", lambda c: {"empty_handle_fraction": 0.0 if c.handles else 1.0}, {"empty_handle_fraction": _HANDLE_CEILING}),
})

# --- [COMPOSITION] ----------------------------------------------------------------------


@receipted(REDACTION)
def _extract(op: ComputationalGeometry, *, proxy: Proxy | None = None) -> AlgebraResult:
    # the `@receipted` aspect harvests `AlgebraResult.contribute()` on exit; the body writes only the
    # dispatch and the `boundary` fence is the single exception-to-fault seam — no inline `Signals.emit`,
    # no mutated receipt accumulator. The result IS the contributor the aspect routes to `emit`.
    return boundary(f"algebra.{op.tag}", lambda: _dispatch(op, proxy=proxy)).default_value(
        _result(op.tag, (), Census(kind=op.tag, handles=0), converged=False)
    )


def run(op: ComputationalGeometry | Sequence[ComputationalGeometry]) -> AlgebraResult | RuntimeRail[Block[AlgebraResult]]:
    # the one polymorphic entrypoint over the input axis: a single op routes through the `@receipted`
    # `_extract` so the aspect emits and the result-is-contributor is the public surface; a batch folds
    # one `Block` of per-op rails through `traversed` (`ACCUMULATE`) so one fault stays addressable in
    # the aggregate while every successful `AlgebraResult` already emitted through the aspect on its arm.
    match op:
        case Sequence() as batch:
            return traversed(Block.of_seq([boundary(f"algebra.{item.tag}", lambda i=item: _dispatch(i)) for item in batch]), by=Disposition.ACCUMULATE)
        case ComputationalGeometry() as single:
            return _extract(single)
        case _ as unreachable:
            assert_never(unreachable)


@receipted(REDACTION)
async def bridged(op: ComputationalGeometry, proxy: Proxy) -> AlgebraResult:
    # the async mirror routing the same `_dispatch` through the out-of-process solver: the blocking
    # `compas.rpc.Proxy` call crosses to the event loop through `anyio.to_thread.run_sync` so the sync
    # solver becomes the awaitable thunk `async_boundary` requires, sharing the one fault rail. The
    # aspect harvests the same `AlgebraResult.contribute()` over `emit_async` off the coroutine arm.
    return (await async_boundary(f"algebra.{op.tag}.rpc", lambda: anyio.to_thread.run_sync(lambda: _dispatch(op, proxy=proxy)))).default_value(
        _result(op.tag, (), Census(kind=op.tag, handles=0), converged=False)
    )
```

## [03]-[RESEARCH]

- [COMPAS_CATALOGUE_BAR]: the `.api/compas.md` catalogue cites `bestfit_plane` (ENTRYPOINTS [06]), `centroid_points` ([07]), `bestfit_frame_numpy` ([08]), `oriented_bounding_box_numpy` ([12]), `convex_hull_numpy` ([13]), `json_dumps`/`json_loads` (serialization [02]/[05]), the transform constructors `Frame.worldXY` ([01]), `Transformation.from_frame_to_frame` ([02]), `Translation.from_vector` ([03]), `Scale.from_factors` ([04]), `Projection.from_plane` ([05]), the datastructure accessors `Network.from_nodes_and_edges` ([01]), `Mesh.from_json`/`dual`/`subdivide` ([02]/[03]), `Mesh.vertices_attributes` ([04]), `Mesh.number_of_edges` ([05]), `VolMesh.dual` ([06]), `Assembly.graph` ([07]), `NurbsSurface.to_mesh` ([08]), and the `rpc.Proxy.function` dotted resolution ([02]) by name, with the `compas.geometry.__all__` line carrying 342 entries (56 classes, 286 functions) verified phantom-free by introspection on the cp312 companion. `Mesh.subdivide(scheme=...)` accepts a default subdivision scheme, so the bare `Mesh.from_json(p).subdivide()` table row binds the default; a scheme axis admits as a `DatastructureOp` row carrying the scheme tag. `Rotation`/`Shear` are dropped from the fence imports because no `NumericalOp` row constructs them; the transform table is the four pure-Python rows `RIGID`/`AFFINE`/`SIMILARITY`/`PROJECTIVE` only.
- [FORM_ENGINE_ARITY]: the `compas_dr` carriers live in `compas_dr.numdata` (`InputData`/`ResultData`), the solvers in `compas_dr.solvers`, the registry factory in `compas_dr.constraints`, and the calculator in `compas_dr.loads` — the package root carries no API (`.api/compas-dr.md` PUBLIC_TYPES [01], ENTRYPOINTS), so the fence imports each from its real submodule, the same dotted paths the RPC names already carry. The `dr_numpy(indata, kmax, dt, tol1, tol2, c, rk_steps)` and keyword-only `dr_constrained_numpy(*, indata, constraints, kmax, ...)` call shapes, `InputData.from_mesh(mesh, fixed, loads, qpre, ...)`, `ResultData.update_mesh(mesh)` write-back, `ResultData.residuals` field, and `SelfweightCalculator(mesh, density).compute_tributary_areas(xyz)` are catalogue-confirmed (`.api/compas-dr.md` ENTRYPOINTS solvers [02]/[03], construction [01], writeback [01], result fields [02], SelfweightCalculator [01]/[03]). The constraint construction is now `Constraint(decoded_geometry)`: the catalogue confirms `Constraint(geometry)` IS the polymorphic factory — `Constraint.__new__` calls `get_constraint_cls(geometry)` walking the geometry MRO against the `GEOMETRY_CONSTRAINT` registry (`Line`/`Plane`/`Circle`/`NurbsCurve`/`NurbsSurface` -> `*Constraint`) and returning the concrete subclass (`.api/compas-dr.md` PUBLIC_TYPES constraints [01], ENTRYPOINTS [01]/[02]), so the fence decodes the typed `NodeConstraint.geometry` COMPAS-JSON through `json_loads` and hands the real geometry straight to `Constraint(...)` — the prior `Constraint.get_constraint_cls(geom)(geom, node, damping=...)` form passing a node-index and damping into the class constructor was wrong (the constraint binds a node by setting `constraint.location`/`constraint.residual` and steps via `update(damping=0.1)`, not by constructor arguments), and the unconstrained `dr_numpy` path is the present fence with constrained-node binding/damping a gated growth on the `NodeConstraint` row. The `compas_tna` form/force path — `FormDiagram.from_mesh` ([11]), `ForceDiagram.from_formdiagram(form)` ([12], THE reciprocal factory; the inherited `FormDiagram.dual_diagram(cls)` is the lower-level primitive `from_formdiagram` rides and is NOT called directly so the uncited `cls` argument never crosses the fence), `relax_boundary_openings(form, fixed)` ([06]), `horizontal_numpy(form, force, alpha=100.0, kmax=100)` ([01]), `vertical_from_zmax(form, zmax, kmax, xtol, rtol, density, ...) -> tuple[FormDiagram, float]` ([05]) — are catalogue-confirmed (`.api/compas-tna.md` ENTRYPOINTS). `LoadUpdater(mesh, p0, thickness, density, live)` with `LoadUpdater.__call__(p, xyz) -> None` updating `p[:, 2]` in place is catalogue-confirmed (loads [01]/[02]); the fence constructs it against the form mesh and the fixed-load array and applies the callable to write tributary selfweight, never the prior dead construction. The `vertices_attributes("xyz")` Nx3 sequence both `SelfweightCalculator` and `LoadUpdater` consume is the catalogue-confirmed `Mesh` accessor (`.api/compas.md` ENTRYPOINTS [04]); the gated arity is whether the `LoadUpdater.__call__(p, xyz)` `p` array the fence passes round-trips back onto the form vertex loads (the catalogue documents the in-place `p[:,2]` write but the fence does not re-read the mutated `p` — the load reaches the form through the call, and the equilibrium solver reads the form's own vertex loads).
- [RECEIPT_AND_GRADUATION_CONTRACT]: `AlgebraResult.contribute` mints through the runtime two-argument `Receipt.of(owner, evidence)` factory over the `("emitted"/"admitted", subject, facts)` `Evidence` triple (`observability/receipts#RECEIPT` ENTRYPOINTS, the `(Phase, subject, facts)` triple minting the `fact` case), returning the `tuple[Receipt, ...]` the `ReceiptContributor.contribute` `Iterable[Receipt]` port streams — the four-positional `Receipt.of(phase, owner, subject, facts)` form and the single-`Receipt` return are the deleted forms the `nonmanifold`/`features` siblings already shed. The facts ride as native `dict[str, object]`: the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes `Census` `int`/`float` scalars without a `str()` coerce, so a pre-`str()` `dict[str, str]` map is the deleted form. `AlgebraResult.graduates(evidence_key)` routes the per-case `CaseSpec.ledger`/`CaseSpec.ceiling` through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission (`compute/graduation/handoff#GRADUATION`), the same residual-over-ceiling fold the sibling `scan/deviation`, `scan/registration`, `ifc/analysis`, and `ifc/structural` owners feed — the handoff `[02]-[CROSS_OWNER]` declares `network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` crossing from this owner keyed per `compas` dispatch case, so the subject the case names is the subject it crosses, never a minted-but-uncrossed field. The `GeometrySubject` literals `network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` are confirmed on the compute `GeometrySubject` union.
- [RPC_PROXY_LIFECYCLE]: the `compas.rpc.Proxy.function(dotted_name)` resolution (`.api/compas.md` ENTRYPOINTS rpc [02]) and the proxy server start/stop lifecycle (`restart_server`/`stop_server` [03]) confirm against the branch `compas` `rpc` subpackage catalogue before `bridged` routes the heavy scipy-backed band out of process; the `Proxy` context-manager shape (whether the server is started lazily on first `function` call or requires explicit lifecycle calls) is the gated lifecycle question, and whether the proxy server bring-up runs on the same worker thread as the `function` call or hoists to a `bridged`-owned `AsyncExitStack` is the second open item. The proxy `function` call is blocking RPC, so `bridged` crosses it to the event loop through `anyio.to_thread.run_sync` (`.api/anyio.md` thread-offload [01], confirmed) — the awaitable thunk `async_boundary` requires, never a sync lambda the awaitable rail rejects. The bridge band is exactly the `NumericalSpec.rpc`-rowed `bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy` primitives (scipy spatial/linalg cores) plus the `_dr`/`_tna` numpy solvers; the four pure-Python transform rows carry `rpc=None`, the `Network` build and the `from_json` datastructure algebra carry no scipy core, so a `bridged` numerical call on a transform row runs in-thread rather than marshalling a 4x4 matrix compose across the process wall.
- [COMPAS_CEM_GROWTH]: `compas_cem` constrained-equilibrium form-finding admits as a `FormEngine.CEM` row plus one `_FORM` arm once it ships `compas>=2.0` support — a RESEARCH-gated growth axis on the existing `FormFinding` case, not a present fence; its `TopologyDiagram`/`FormDiagram`/`ConstrainedFormDiagram` and `static_equilibrium`/`constrained_fdm` entrypoints confirm against a branch `compas_cem` catalogue before admission.
