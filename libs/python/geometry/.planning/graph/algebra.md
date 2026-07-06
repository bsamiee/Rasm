# [PY_GEOMETRY_GRAPH_ALGEBRA]

AEC computational and numerical geometry. `ComputationalGeometry` is one `@tagged_union` dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`, selected by a `FormEngine` sub-enum on the one form-finding case), datastructure algebra over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, and a parameterized `NumericalOp` table that folds best-fit/bbox/hull primitives AND the rigid/affine/similarity/projective transform rows into one keyed catalogue. There is no separate `graph/transform` owner: an affine map is a numerical op on a coordinate set, not a second concern. Every arm carries its geometry-minted `rasm.geometry.graduation` `GeometrySubject` member keyed to its `AlgebraKind`, never a bare `str`; `graduates()` returns the local `GeometryHandoff` carrier whose `wire()` projection is the compute crossing — this owner keeps `NUMERICAL_PRIMITIVE` (the compas-numerics evidence class the differentiated union retains for it) beside `NETWORK_GRAPH`/`FORM_FINDING`/`MESH_ALGEBRA`. This owner is distinct from non-manifold topology (the `nonmanifold` sibling over `topologicpy`) and from raw mesh-file exchange at the data seam, which defers to data `MeshPayload`.

`compas`, `compas_dr`, and `compas_tna` provide the graph-algebra and structural form-finding surfaces beside the `networkx` projection.

## [01]-[INDEX]

- [01]-[ALGEBRA]: the `ComputationalGeometry` `@tagged_union` of algebra kinds, the `CASE` subject/ledger/ceiling spec table, the `NUMERICAL` primitive-plus-transform table with its per-row RPC route, the `DATASTRUCTURE` and `_FORM` op tables, the woven `compas` rail with its proxy offload riding the runtime lane THREAD band (zero geometry-minted limiters), the typed `Census` and `FormResult` value objects, the `@receipted` aspect on the pure `_extract`, the polymorphic single-or-batch `run` and async `bridged` mirror over that one fenced rail, and the per-case `graduates` rail under one `ReceiptContributor`.

## [02]-[ALGEBRA]

- Owner: `ComputationalGeometry` — the `@tagged_union(frozen=True)` discriminating by `AlgebraKind`; `AlgebraResult` the typed receipt carrying the case kind as the closed `Literal` tag (never `str`), the COMPAS-JSON result handles, a typed `Census` value object, and the case-keyed `GeometrySubject`. The four per-case data axes — graduation subject, residual-ledger projector, ceiling, and whether the case graduates — are ONE `AlgebraKind`-keyed `CASE` `CaseSpec` table, never a `_subject` `match` racing a parallel ledger fold: the case already names its kind, so a new algebra kind is one `CaseSpec` row and one `@tagged_union` case, never a subject map drifting against the discriminant. `AlgebraResult` is the sole `ReceiptContributor`: `contribute` is the one emission, projecting `Census` through `msgspec.structs.asdict` into one phase-keyed `Receipt.of("geometry.graph.algebra", (phase, subject, facts))` row under the verified runtime two-argument factory — `phase="emitted"` for a converged/clean result and `phase="admitted"` for the entry caveat a non-converged form-finding pass keys off the residual census — so the admitted and emitted rows ride the one contributor path, never a discarded `Receipt.of` minted inside a decorator that never reaches the sink. The facts ride as native `dict[str, object]` carrying the `Census` `int`/`float` scalars; the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them without a `str()` coerce, so a pre-`str()` `dict[str, str]` map is the deleted form that owner rejects. Retry/telemetry rides the `boundary(f"algebra.{tag}")` fence subject and the `@receipted` aspect, not a second hand-rolled rail.
- Cases: `ComputationalGeometry` cases `Network(vertices, edges)` (compas `Network.from_nodes_and_edges` adjacency, `network-graph` subject) · `FormFinding(mesh, anchors, engine, params)` (the one structural case keyed by the `FormEngine` sub-enum carrying a typed `FormParams` value object — never two semantics crammed into one bare `float` — `DR` weaving the `SelfweightCalculator(density=rho).__call__` `area * rho` selfweight load, the `Constraint`-family node projection, `dr_numpy`/`dr_constrained_numpy` with `FormParams.rk_steps` and `tol1=FormParams.tol`, and `ResultData.update_mesh` write-back into one rail; `TNA` weaving `FormDiagram.from_mesh`, `relax_boundary_openings`, the `LoadUpdater` applied to write tributary selfweight (never constructed and discarded), the `ForceDiagram.from_formdiagram` reciprocal dual, `horizontal_numpy`, and `vertical_from_zmax` capturing the returned crown-scale — `form-finding` subject) · `Numerical(points, op)` (the closed `NumericalOp`-keyed `NUMERICAL` table folding `compas.geometry` best-fit/bbox/hull free functions AND the transform-matrix constructors, `numerical-primitive` subject) · `Datastructure(payload, op)` (the `DatastructureOp`-keyed `DATASTRUCTURE` table over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, `mesh-algebra` subject) — matched by total `match`/`case` closed with `assert_never`. The sub-op of every parameterized case is a closed `StrEnum`, never a raw string literal inside the payload.
- Entry: `run` is the one polymorphic module-level entrypoint over the `ComputationalGeometry` op union, discriminating a single op or a batch `Sequence[ComputationalGeometry]` — a single op fences the `@receipted` `_extract` once through `boundary(f"algebra.{op.tag}", ...)` so the exception-to-fault lift is the single runtime seam and the rail carries the `AlgebraResult` contributor, a batch builds a `Block` of the SAME fenced rail in one comprehension and folds them through `runtime.faults.traversed` (`Disposition.ACCUMULATE`) so one fault stays addressable in the aggregate while every successful `AlgebraResult` already emitted through the aspect on its arm. The `@receipted(REDACTION)` aspect sits on the pure `_extract` (not on `run`/`bridged`), harvesting `contribute` on exit so both the single arm and every per-batch-item arm emit identically and the `boundary` fence stays OUTSIDE the aspect exactly as the `nonmanifold` sibling wires it — a solve that raises is an `Error(BoundaryFault)` on the rail rather than a synthetic zero-handle receipt the aspect would falsely emit. `bridged` is the async mirror routing the same `@receipted` `_extract` through the `compas.rpc.Proxy` out-of-process CPython solver when the case is heavy: the blocking proxy RPC wait crosses through `lane.offload(..., modality=Modality.THREAD)` — the runtime-owned THREAD band bounds every concurrent bridged solve, zero geometry-minted `CapacityLimiter`s — and the lane's own `async_boundary` is the one fault rail both paths share, never a parallel async surface. The proxy reaches exactly the scipy-backed heavy band — the `_dr`/`_tna` numpy solvers and the `NumericalSpec.rpc`-routed `bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy` primitives whose scipy spatial/linalg cores must not block the gated companion in-process; the pure-Python transform-matrix rows carry `rpc=None`, the `Network` adjacency build and the datastructure `from_json` algebra carry no heavy solver, so the proxy route is a per-row capability (`spec.rpc is not None`) rather than a blanket re-entry that pointlessly marshals a matrix multiply across the process wall.
- Auto: the numerical case folds the `NUMERICAL` `NumericalSpec` table by `NumericalOp` — each row pairs the local callable to its optional RPC dotted-name, so best-fit/bbox/hull primitives reach the `_numpy`-accelerated variants routed out-of-process through `spec.rpc` when a proxy is supplied while the transform rows compose a `Transformation` from `Translation`/`Scale`/`Projection` constructors with `rpc=None`, and a new map is one table row, never a dispatch branch and never a parallel `_NUMERICAL_RPC` map drifting against the op table. The form-finding case threads the mesh, anchor set, and `FormParams` through the `FormEngine`-selected solver into a typed `FormResult`: `DR` weaving `numpy`-vectorized selfweight, optional geometric constraints, the RK-order solver, and `ResultData.update_mesh` write-back so the equilibrium mesh (not the raw `ResultData`) is the handle and the `numpy`-reduced max-abs residual the receipt fact, and `TNA` weaving the `LoadUpdater` applied to the form, the `ForceDiagram.from_formdiagram` reciprocal pair, and the `vertical_from_zmax` crown-scale. The datastructure case folds the `Mesh.dual`/`Mesh.subdivide`/`VolMesh`/`Assembly`/`NurbsSurface` algebra over `from_json` payloads; `json_dumps` serializes every result through the one COMPAS serializer for graduation, never a per-type encoder. The form-finding residual sets `AlgebraResult.converged` against `FormParams.tol`, so the contributor phase is data-driven, not a constant.
- Receipt: each evaluation produces an `AlgebraResult` that is the sole `ReceiptContributor`; `contribute` returns the one-element `tuple[Receipt, ...]` the port streams, minting one phase-keyed `Receipt.of("geometry.graph.algebra", (phase, subject, facts))` row — `phase="emitted"` for a converged/clean result and `phase="admitted"` for a form-finding pass whose `numpy`-reduced residual exceeds `FormParams.tol` (the caveat row, so the unconverged equilibrium is flagged rather than asserted), the facts being the `Census` value object (`structs.asdict`-projected) carrying the case kind, the input vertex/point/edge census, the result handle count, and the solver residual/crown-scale where the case produces one. `graduates` returns the local `GeometryHandoff` carrier — `GeometryHandoff.of(subject, key, ledger, ceiling)` over the case's `CaseSpec.ledger`-projected residual and its `CaseSpec.ceiling` — the form-finding `residual` against `_RESIDUAL_CEILING`, the network/numerical/datastructure cases their `empty_handle_fraction` against the zero ceiling so a vacuous result (no handle produced) does not graduate — so a result breaching its ceiling fails the carrier's residual-over-ceiling `admitted` verdict rather than crossing clean, and the crossing to compute is the carrier's `wire()` data, never an import. The residual census is the evidence the fold reads, never a re-measured value.
- Packages: `compas` (`datastructures.Network`/`Mesh`/`VolMesh`/`Assembly`/`NurbsSurface`, `geometry.bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy`/`bestfit_plane`/`centroid_points`/`Frame.worldXY`/`Transformation`/`Translation`/`Scale`/`Projection`, `json_dumps`/`json_loads`, `rpc.Proxy`), `compas_dr` (`numdata.InputData.from_mesh`/`numdata.ResultData.update_mesh`/`numdata.ResultData.residuals`/`solvers.dr_numpy`/`solvers.dr_constrained_numpy`/`constraints.Constraint`/`loads.SelfweightCalculator`), `compas_tna` (`diagrams.FormDiagram.from_mesh`/`diagrams.ForceDiagram.from_formdiagram`/`loads.LoadUpdater`/`equilibrium.relax_boundary_openings`/`equilibrium.horizontal_numpy`/`equilibrium.vertical_from_zmax`), `numpy` (`asarray`/`abs`/`max` over the load and residual reductions), `contextlib` (`asynccontextmanager` the `solver_proxy` async-resource owner driving the sync `Proxy` CM through the lane, `AsyncExitStack` a `bridged` consumer composes the scope through), `expression` (`tagged_union`/`case`/`tag`, `Block`), `msgspec` (`Struct`/`structs.asdict`), geometry (`GeometrySubject`/`GeometryHandoff` the graduation spine), runtime (`RuntimeRail`/`boundary`/`traversed`/`Disposition`, `Receipt`/`Redaction`/`receipted`, `ContentKey` from `rasm.runtime.identity`, `LanePolicy.offload`/`Modality.THREAD` the runtime-owned worker band the proxy bring-up, teardown, and RPC wait all ride, `RetryClass.RPC` the proxy bring-up transient row threaded as `offload(retry=)`).
- Growth: a new algebra kind is one `ComputationalGeometry` case, one `match` arm, and one `CASE` `CaseSpec` row carrying its subject/ledger/ceiling; a new numerical primitive or transform is one `NumericalOp` row plus one `NUMERICAL` `NumericalSpec` entry (its RPC route a row field, never a parallel map); a new datastructure verb is one `DatastructureOp` row plus one `DATASTRUCTURE` entry; a new form-finding engine is one `FormEngine` row plus one `_FORM` arm; a new geometric constraint is one `NodeConstraint` row whose decoded `compas.geometry` the `Constraint.get_constraint_cls` dispatch already resolves, never a new arm — `compas_cem` constrained-equilibrium admits as a `FormEngine.CEM` row once it ships `compas>=2.0` support, an admission-gated growth axis on the existing case, never a new case or page; zero new surface.

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
    # `op` carries the case's sub-op `StrEnum` value (`NumericalOp`/`DatastructureOp`/`FormEngine`) widened
    # to its `str` member; the network case has no sub-op and carries its edge count in the typed `edges`
    # slot, so no count is ever stuffed into the label field. Every count rides a real `int`.
    kind: AlgebraKind
    handles: int
    inputs: int = 0
    edges: int = 0
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


class AlgebraResult(Struct, frozen=True):  # conforms structurally to the runtime-checkable ReceiptContributor Protocol
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

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        # the per-case CaseSpec supplies subject, the Census-read residual ledger, and the ceiling; the
        # local carrier's residual-over-ceiling `admitted` verdict gates and `wire()` is the compute crossing.
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
    # `loads` is the Nx3 load buffer the updater mutates in place at column 2 against the live `xyz` (never the
    # coordinates), then rides back onto the form's per-vertex `px`/`py`/`pz` attributes the solvers read. The
    # write-back is a per-vertex `vertex_attributes(key, names, row)` loop over the SAME `keys` ordering the read
    # used, because `vertices_attributes(names, values=X)` hands the WHOLE `X` to every vertex (it zips `names`
    # against the outer rows of `X`, writing `px=X[0]`/`py=X[1]`/`pz=X[2]` onto every vertex and dropping the
    # rest) rather than one row per vertex — the row-write call is the broadcast defect, not a row distributor.
    loads = np.zeros_like(xyz)
    LoadUpdater(form, loads, density=params.rho)(loads, xyz)
    for key, row in zip(keys, loads.tolist()):
        form.vertex_attributes(key, ("px", "py", "pz"), row)
    horizontal = proxy.function("compas_tna.equilibrium.horizontal_numpy") if proxy else horizontal_numpy
    form, force = horizontal(form, force, alpha=params.alpha, kmax=params.kmax)
    form, scale = vertical_from_zmax(form, params.target, density=params.density)
    return FormResult((json_dumps(form), json_dumps(force)), scale)


# --- [TABLES] ---------------------------------------------------------------------------

# `bestfit_frame_numpy(pts)` returns the lower-level `(origin, xaxis, yaxis)` coordinate triple, NOT a `Frame`,
# so the `RIGID`/`SIMILARITY` rows wrap it as `compas.geometry.Frame(*triple)` (positional `Frame(point, xaxis,
# yaxis)`, auto-orthonormalized) before the constructor: `Transformation.from_frame_to_frame` requires a real
# `Frame` second arg, and the wrap keeps both fitted-frame rows on one explicit shape rather than relying on the
# looser `[point, vector, vector]` tuple coercion only `Scale.from_factors(frame=...)` documents.
_TRANSFORM: Final[Mapping[NumericalOp, Callable[[Coords], Transformation]]] = MappingProxyType({
    # `bestfit_frame_numpy` returns a `(point, xaxis, yaxis)` ndarray tuple, NOT a `Frame`, so the two
    # frame-consuming rows wrap it through `Frame(*...)` before the matrix constructors; the AFFINE/PROJECTIVE
    # rows take the plain `centroid_points` list and the `bestfit_plane` (point, normal) tuple directly.
    NumericalOp.RIGID: lambda pts: Transformation.from_frame_to_frame(
        compas.geometry.Frame.worldXY(), compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))
    ),
    NumericalOp.AFFINE: lambda pts: Translation.from_vector(compas.geometry.centroid_points(pts)),
    NumericalOp.SIMILARITY: lambda pts: Scale.from_factors([1.0, 1.0, 1.0], compas.geometry.Frame(*compas.geometry.bestfit_frame_numpy(pts))),
    NumericalOp.PROJECTIVE: lambda pts: Projection.from_plane(compas.geometry.bestfit_plane(pts)),
})

# each NumericalSpec pairs the local callable to its optional RPC dotted-name; the three scipy-backed _numpy
# primitives carry their rpc path, the four pure-Python transform rows carry rpc=None — the proxy route is a
# row field, never a parallel map. A new transform admits its constructor the same gated way. The three _numpy
# primitives return raw (frame triple / 8-corner list / `(indices, faces)` ndarray pair), NOT `Data` subclasses,
# yet `_dispatch` serializes each through one `json_dumps(value)` with no `.tolist()` coerce and no output
# projector: COMPAS `json_dumps` runs `DataEncoder`, whose `default` already maps `numpy.ndarray` -> `tolist()`
# and every numpy int/float scalar -> Python `int`/`float`, so the `convex_hull_numpy` integer arrays and any
# numpy scalar inside the OBB/frame returns plain-JSON natively — the heterogeneous `NumericalSpec.local`
# return (`object`) reaches the wire through the same single serializer the `Data`-subclass transform rows use.
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
    # the blocking bring-up offloaded once on scope entry: `Proxy(...)` eagerly `_try_reconnect`s to the
    # running localhost server (`url='http://127.0.0.1'`, `port=1753`) or spawns one through the blocking
    # `start_server()` (Popen + `time.sleep(0.1)` ping loop, `max_conn_attempts=100`), then `__enter__`
    # arms the ownership-aware teardown. `autoreload=False` keeps the worker from reloading mid-fan;
    # `capture_output=True` quiets the subprocess. A cold-start timeout raises `RPCServerError` here, the
    # transient `RetryClass.RPC` retries on the offload leg.
    proxy = Proxy(url="http://127.0.0.1", autoreload=False, capture_output=True)
    proxy.__enter__()
    return proxy


@contextlib.asynccontextmanager
async def solver_proxy(lane: LanePolicy) -> AsyncIterator[Proxy]:
    # the one async-resource owner of the `compas.rpc.Proxy` lifecycle: the WHOLE blocking bring-up — the
    # eager reconnect-or-spawn construction plus `__enter__` — rides `lane.offload` on the runtime-owned
    # THREAD band with the cold-start transient retried under `RetryClass.RPC` (the `offload(retry=)`
    # pinned export), so the bring-up never blocks the event loop and no geometry-minted limiter exists.
    # On exit the ownership-aware `__exit__(None, None, None)` (which runs `stop_server()` only when this
    # proxy spawned the server) rides the SAME band, so teardown is bounded too and runs even on a solve
    # fault inside the scope. A `bridged` fan composes this through `async with AsyncExitStack() as
    # stack: proxy = await stack.enter_async_context(solver_proxy(lane))` so a fan of heavy solves shares
    # ONE reconnected worker under the one runtime band.
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
    # the pure dispatch the `@receipted` aspect wraps: it returns the `AlgebraResult` contributor the
    # aspect harvests through `Signals.emit` on exit, threading no inline emit call and no mutated
    # receipt accumulator. The `boundary` fence lives OUTSIDE in `run`/`bridged`, so the single
    # exception-to-fault seam wraps `_extract` on BOTH the single and the per-batch-item arm rather
    # than nesting inside the body — the same fence-outside-the-aspect wiring the `nonmanifold` sibling
    # carries, and the reason every successful batch result emits through the aspect on its own arm.
    return _dispatch(op, proxy=proxy)


def run(op: ComputationalGeometry | Sequence[ComputationalGeometry]) -> RuntimeRail[AlgebraResult] | RuntimeRail[Block[AlgebraResult]]:
    # the one polymorphic entrypoint over the input axis: a single op fences the `@receipted` `_extract`
    # once so the aspect emits and the rail carries the contributor; a batch folds one `Block` of the
    # SAME fenced rail through `traversed` (`ACCUMULATE`) so one fault stays addressable in the aggregate
    # while every successful `AlgebraResult` already emitted through the aspect on its arm. The default
    # `i=item` binds the loop variable per closure so the comprehension never captures the last `item`.
    match op:
        case Sequence() as batch:
            return traversed(Block.of_seq([boundary(f"algebra.{item.tag}", lambda i=item: _extract(i)) for item in batch]), by=Disposition.ACCUMULATE)
        case ComputationalGeometry() as single:
            return boundary(f"algebra.{single.tag}", lambda: _extract(single))
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: ComputationalGeometry, proxy: Proxy, lane: LanePolicy) -> RuntimeRail[AlgebraResult]:
    # the async mirror of `run`: it routes the SAME `@receipted` `_extract` through the out-of-process
    # solver, so the aspect emits on the worker thread off `_extract`'s exit exactly as the sync arm does
    # — `bridged` is not itself `@receipted`, the aspect lives on `_extract` for both paths. The `proxy`
    # is supplied by an enclosing `solver_proxy(lane)` scope, NOT constructed per call. The blocking
    # per-`function` RPC wait crosses through `lane.offload` on the runtime-owned THREAD band — the
    # lane's own async_boundary is the one fault rail, zero geometry-minted limiters.
    return await lane.offload(lambda: _extract(op, proxy=proxy), modality=Modality.THREAD)
```
