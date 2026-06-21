# [PY_GEOMETRY_GRAPH_ALGEBRA]

AEC computational and numerical geometry. `ComputationalGeometry` is one tagged-union dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`, selected by a `FormEngine` sub-enum on the one form-finding case), datastructure algebra over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, and a parameterized `NumericalOp` table that folds best-fit/bbox/hull primitives AND the rigid/affine/similarity/projective transform rows into one keyed free-function/constructor catalogue — there is no separate `graph/transform` owner, because an affine map is a numerical op on a coordinate set, not a second concern. Every arm emits the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`) keyed to its case, never a bare `str`; the compute `HandoffAxis` geometry case is the single graduation rail. This owner is distinct from non-manifold topology (the `nonmanifold` sibling over `topologicpy`) and from raw mesh-file exchange at the data seam, which defers to data `MeshPayload`.

`compas` is the gated companion band — `compas>=2.15.1; python_version<'3.15'`, the cp312 floor forced by the scipy-family CPython 3.15 lag — alongside `compas_dr` and `compas_tna`; the owner and fences stay authored, dark on the intended cp315 core until the wheels ship.

## [01]-[INDEX]

- [01]-[ALGEBRA]: network adjacency, datastructure algebra, the `FormEngine`-keyed form-finding case, and the `NumericalOp` primitive-plus-transform table under one tagged union with one `GeometrySubject`-typed receipt.

## [02]-[ALGEBRA]

- Owner: `ComputationalGeometry` — the `@tagged_union(frozen=True)` discriminating by algebra-kind; `AlgebraResult` the typed receipt carrying the case kind as the closed `Literal` tag (never `str`), the COMPAS-JSON result handles, a typed `Census` value object, and the graduation subject as the canonical `GeometrySubject` literal. `_subject` is one total `match`/`assert_never` over the kind resolving each case to its `GeometrySubject` literal — the case already names its subject, so there is no `str`-keyed `_SUBJECT` map to drift against the discriminant (the parallel-subject defect the compute `graduation/handoff#HandoffAxis` owner forbids). `AlgebraResult` is the sole `ReceiptContributor`: `contribute` is the one emission, projecting `Census` through `msgspec.structs.asdict` into one `Receipt.of` row keyed by phase — `phase="emitted"` for a converged/clean result and `phase="admitted"` for the entry caveat a non-converged form-finding pass keys off the residual census — so the admitted and emitted rows ride the one contributor path rather than a discarded `Receipt.of` constructed inside a decorator that never reaches the sink. Retry/telemetry is the `boundary(f"algebra.{tag}")` fence subject (the single runtime seam), not a second hand-rolled aspect.
- Cases: `ComputationalGeometry` cases `Network(vertices, edges)` (compas `Network.from_nodes_and_edges` adjacency, `network-graph` subject) · `FormFinding(mesh, anchors, engine, params)` (the one structural case keyed by the `FormEngine` sub-enum carrying a typed `FormParams` value object — never two semantics crammed into one bare `float` — `DR` weaves `SelfweightCalculator.compute_tributary_areas` selfweight loads, the `Constraint`-family node projection, `dr_numpy`/`dr_constrained_numpy` with `FormParams.rk_steps`, and `ResultData.update_mesh` write-back into one rail; `TNA` weaves `FormDiagram.from_mesh`, the `LoadUpdater` applied to the form to write tributary selfweight (never constructed and discarded), `relax_boundary_openings`, `horizontal_numpy`, and `vertical_from_zmax` capturing the returned crown-scale — `form-finding` subject) · `Numerical(points, op)` (the closed `NumericalOp`-keyed `NUMERICAL` table folding `compas.geometry` best-fit/bbox/hull free functions AND the transform-matrix constructors, `numerical-primitive` subject) · `Datastructure(payload, op)` (the `DatastructureOp`-keyed `DATASTRUCTURE` table — the single dispatch, not a `match` wrapped by a redundant map — over the `Mesh`/`VolMesh`/`Assembly`/`NurbsSurface` family, `mesh-algebra` subject) — matched by total `match`/`case` closed with `assert_never`. The sub-op of every parameterized case is a closed `StrEnum`, never a raw string literal inside the payload.
- Entry: `evaluate` is the one entrypoint, wrapping `_dispatch` in `boundary(f"algebra.{algebra.tag}", ...)` so the exception-to-fault lift is the single runtime seam and the return is `RuntimeRail[AlgebraResult]`. `bridged` is the same dispatch routed through the `compas.rpc.Proxy` out-of-process CPython solver when the caller marks the case heavy, folding the blocking proxy call through `anyio.to_thread.run_sync` so the sync solver becomes the awaitable thunk `async_boundary` requires and the in-process and out-of-process paths share one fault rail rather than a parallel async surface. The proxy reaches exactly the scipy-backed heavy band: the `_dr`/`_tna` numpy solvers and the `_NUMERICAL_RPC`-routed `bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy` primitives whose scipy spatial/linalg cores must not block the gated companion in-process; the pure-Python transform-matrix rows, the `Network` adjacency build, and the datastructure `from_json` algebra carry no heavy solver and stay in-thread on either path, so the proxy route is a per-row capability (`op in _NUMERICAL_RPC`) rather than a blanket re-entry that pointlessly marshals a matrix multiply across the process wall.
- Auto: the numerical case folds the `NUMERICAL` table by `NumericalOp` — best-fit/bbox/hull primitives reach the `_numpy`-accelerated variants (`bestfit_frame_numpy`, `oriented_bounding_box_numpy`, `convex_hull_numpy`), routed out-of-process through the `op in _NUMERICAL_RPC` proxy guard inside the numerical arm when a proxy is supplied, and the transform rows compose a `Transformation` from `Translation`/`Scale`/`Projection` constructors keyed identically, so a new map is one table row, never a dispatch branch; the form-finding case threads the mesh, anchor set, and `FormParams` through the `FormEngine`-selected solver, `DR` weaving `numpy`-vectorized selfweight, optional geometric constraints, the RK-order solver, and `ResultData.update_mesh` write-back so the equilibrium mesh (not the raw `ResultData`) is the handle and the `numpy`-reduced max-abs residual the receipt fact, and `TNA` weaving the `LoadUpdater` applied to the form (not constructed and dropped), the reciprocal form/force diagram pair, and the `vertical_from_zmax` crown-scale; the datastructure case folds the `Mesh.dual`/`Mesh.subdivide`/`VolMesh`/`Assembly`/`NurbsSurface` algebra over `from_json` payloads; `json_dumps` serializes every result through the one COMPAS serializer for graduation, never a per-type encoder. The form-finding residual sets `AlgebraResult.converged` against `FormParams.tol`, so the contributor phase is data-driven, not a constant.
- Receipt: each evaluation produces an `AlgebraResult` that is the sole `ReceiptContributor`; `contribute` emits one phase-keyed `Receipt.of` row — `phase="emitted"` for a converged/clean result and `phase="admitted"` for a form-finding pass whose `numpy`-reduced residual exceeds `FormParams.tol` (the caveat row, so the unconverged equilibrium is flagged rather than asserted), the facts being the `Census` value object (`structs.asdict`-projected) carrying the case kind, the input vertex/point/edge census, the result handle count, and the solver residual/crown-scale where the case produces one. The admitted and emitted rows ride this one contributor, never a discarded `Receipt.of` minted inside a decorator that never reaches the sink. The result also seeds a geometry `GraduationReceipt` subject keyed by case through `_subject` (`network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`) the compute `graduates` rail folds against its residual ceiling — the residual census is the evidence that fold reads, never a re-measured value.
- Packages: `compas` (`datastructures.Network`/`Mesh`/`VolMesh`/`Assembly`/`NurbsSurface`, `geometry.bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy`/`bestfit_plane`/`centroid_points`/`Frame.worldXY`/`Transformation`/`Translation`/`Scale`/`Projection`, `json_dumps`/`json_loads`, `rpc.Proxy`, `is_rhino`/`is_grasshopper` host probes), `compas_dr` (`InputData.from_mesh`/`dr_numpy`/`dr_constrained_numpy`/`ResultData.update_mesh`/`ResultData.residuals`/`Constraint.get_constraint_cls`/`SelfweightCalculator`), `compas_tna` (`FormDiagram.from_mesh`/`FormDiagram.dual_diagram`/`LoadUpdater`/`relax_boundary_openings`/`horizontal_numpy`/`vertical_from_zmax`), `anyio` (`to_thread.run_sync`), `numpy` (`asarray`/`abs`/`ndarray.max` over the load and residual reductions), `expression` (`tagged_union`/`case`/`tag`), `msgspec` (`Struct`/`structs.asdict`), runtime (`RuntimeRail`/`boundary`/`async_boundary`, `Receipt`/`ReceiptContributor`), compute (`GeometrySubject`).
- Growth: a new algebra kind is one `ComputationalGeometry` case plus one `match` arm; a new numerical primitive or transform is one `NumericalOp` row plus one `NUMERICAL` table entry; a new datastructure verb is one `DatastructureOp` row plus one `DATASTRUCTURE` table entry; a new form-finding engine is one `FormEngine` row plus one `_FORM` arm; a new geometric constraint is one `NodeConstraint` row whose decoded `compas.geometry` the `Constraint.get_constraint_cls` dispatch already resolves, never a new arm — `compas_cem` constrained-equilibrium admits as a `FormEngine.CEM` row once it ships `compas>=2.0` support, a RESEARCH-gated growth axis on the existing case, never a new case or page; zero new surface.
- Boundary: non-manifold cell/aperture topology is the `nonmanifold` sibling over `topologicpy`, never folded here; robust mesh repair/boolean is the `mesh/repair` sibling over `trimesh`/`manifold3d`, not the compas datastructure algebra; raw mesh-file decode/encode and GLB preview stay at the data `MeshPayload` seam — `evaluate` returns COMPAS-JSON handles across the wire and never writes a mesh file; visualization-scene/USD/GLTF/OBJ export is the artifacts figures/scene owner; the rigid/affine/similarity/projective transform rows live HERE as `NumericalOp` table rows, never as a parallel `graph/transform` page; `compas.rpc.Proxy` is the out-of-process solver bridge for the gated companion, never an in-process re-entry, and the blocking proxy call crosses to the event loop only through `anyio.to_thread.run_sync` so the `async_boundary` thunk is a real awaitable rather than a sync lambda the awaitable rail would reject.

```python signature
from collections.abc import Callable
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
from compas_dr import Constraint, InputData, SelfweightCalculator, dr_constrained_numpy, dr_numpy
from compas_tna.diagrams import FormDiagram
from compas_tna.equilibrium import horizontal_numpy, relax_boundary_openings, vertical_from_zmax
from compas_tna.loads import LoadUpdater
from expression import case, tag, tagged_union
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

type AlgebraKind = Literal["network", "form_finding", "numerical", "datastructure"]
type Coords = list[list[float]]
type FormResult = tuple[tuple[str, ...], float]
Points = tuple[tuple[float, float, float], ...]
Edges = tuple[tuple[int, int], ...]


def _subject(kind: AlgebraKind) -> GeometrySubject:
    match kind:
        case "network":
            return "network-graph"
        case "form_finding":
            return "form-finding"
        case "numerical":
            return "numerical-primitive"
        case "datastructure":
            return "mesh-algebra"
        case unreachable:
            assert_never(unreachable)


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


class NodeConstraint(Struct, frozen=True):
    node: int  # constrained vertex index
    geometry: str  # COMPAS-JSON of the Plane/Line/Circle/Curve/Surface the node snaps to; get_constraint_cls dispatches on its decoded type
    damping: float = 0.1


class FormParams(Struct, frozen=True):
    target: float = 0.0
    density: float = 1.0
    rho: float = 1.0
    rk_steps: Literal[1, 2, 4] = 2
    kmax: int = 10000
    tol: float = 1e-3  # residual ceiling the convergence verdict folds against; mirrors the dr_numpy tol1 default
    constraints: tuple[NodeConstraint, ...] = ()  # each node snaps to its decoded constraint geometry each step; () = unconstrained


_TRANSFORM: Final[Mapping[NumericalOp, Callable[[Coords], Transformation]]] = MappingProxyType({
    NumericalOp.RIGID: lambda pts: Transformation.from_frame_to_frame(
        compas.geometry.Frame.worldXY(), compas.geometry.bestfit_frame_numpy(pts)
    ),
    NumericalOp.AFFINE: lambda pts: Translation.from_vector(compas.geometry.centroid_points(pts)),
    NumericalOp.SIMILARITY: lambda pts: Scale.from_factors([1.0, 1.0, 1.0], compas.geometry.bestfit_frame_numpy(pts)),
    NumericalOp.PROJECTIVE: lambda pts: Projection.from_plane(compas.geometry.bestfit_plane(pts)),
})

NUMERICAL: Final[Mapping[NumericalOp, Callable[[Coords], object]]] = MappingProxyType({
    NumericalOp.BESTFIT_FRAME: compas.geometry.bestfit_frame_numpy,
    NumericalOp.OBB: compas.geometry.oriented_bounding_box_numpy,
    NumericalOp.CONVEX_HULL: compas.geometry.convex_hull_numpy,
    **_TRANSFORM,
})

# the scipy-backed _numpy primitives are the only numerical rows the bridge offloads out of process;
# the transform rows are pure-Python matrix composition with no heavy solver, so they have no RPC route.
_NUMERICAL_RPC: Final[Mapping[NumericalOp, str]] = MappingProxyType({
    NumericalOp.BESTFIT_FRAME: "compas.geometry.bestfit_frame_numpy",
    NumericalOp.OBB: "compas.geometry.oriented_bounding_box_numpy",
    NumericalOp.CONVEX_HULL: "compas.geometry.convex_hull_numpy",
})


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


class Census(Struct, frozen=True):
    kind: AlgebraKind
    handles: int
    inputs: int = 0
    op: str = ""
    residual: float = 0.0


class AlgebraResult(Struct, ReceiptContributor, frozen=True):
    kind: AlgebraKind
    handles: tuple[str, ...]
    census: Census
    graduation_subject: GeometrySubject
    converged: bool = True

    def contribute(self) -> Receipt:
        phase: Literal["admitted", "emitted"] = "emitted" if self.converged else "admitted"
        facts = {k: str(v) for k, v in structs.asdict(self.census).items()}
        return Receipt.of(phase, "geometry.algebra", self.graduation_subject, facts)


def evaluate(algebra: ComputationalGeometry) -> "RuntimeRail[AlgebraResult]":
    return boundary(f"algebra.{algebra.tag}", lambda: _dispatch(algebra))


async def bridged(algebra: ComputationalGeometry, proxy: Proxy) -> "RuntimeRail[AlgebraResult]":
    return await async_boundary(
        f"algebra.{algebra.tag}.rpc",
        lambda: anyio.to_thread.run_sync(lambda: _dispatch(algebra, proxy=proxy)),
    )


def _dispatch(algebra: ComputationalGeometry, *, proxy: Proxy | None = None) -> AlgebraResult:
    match algebra:
        case ComputationalGeometry(tag="network", network=(vertices, edges)):
            graph = Network.from_nodes_and_edges([list(v) for v in vertices], list(edges))
            return _result("network", (json_dumps(graph),), Census(kind="network", handles=1, inputs=len(vertices), op=str(len(edges))))
        case ComputationalGeometry(tag="numerical", numerical=(points, op)):
            pts = [list(p) for p in points]
            # scipy-backed _numpy rows offload out of process when bridged; pure-Python transform rows stay in-thread.
            value = proxy.function(_NUMERICAL_RPC[op])(pts) if proxy and op in _NUMERICAL_RPC else NUMERICAL[op](pts)
            return _result("numerical", (json_dumps(value),), Census(kind="numerical", handles=1, inputs=len(points), op=op))
        case ComputationalGeometry(tag="form_finding", form_finding=(mesh, anchors, engine, params)):
            handles, residual = _FORM[engine](Mesh.from_json(mesh), list(anchors), params, proxy)
            return _result("form_finding", handles, Census(kind="form_finding", handles=len(handles), inputs=len(anchors), op=engine, residual=residual), converged=residual <= params.tol)
        case ComputationalGeometry(tag="datastructure", datastructure=(payload, op)):
            return _result("datastructure", (json_dumps(DATASTRUCTURE[op](payload)),), Census(kind="datastructure", handles=1, op=op))
        case unreachable:
            assert_never(unreachable)


def _result(kind: AlgebraKind, handles: tuple[str, ...], census: Census, *, converged: bool = True) -> AlgebraResult:
    return AlgebraResult(kind=kind, handles=handles, census=census, graduation_subject=_subject(kind), converged=converged)


def _dr(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    xyz = mesh.vertices_attributes("xyz")
    loads = (np.asarray(SelfweightCalculator(mesh, density=params.rho).compute_tributary_areas(xyz)) * (0.0, 0.0, -params.density)).tolist()
    indata = InputData.from_mesh(mesh, fixed=anchors, loads=loads, qpre=[1.0] * mesh.number_of_edges())
    constraints = tuple(Constraint.get_constraint_cls(geom := json_loads(c.geometry))(geom, c.node, damping=c.damping) for c in params.constraints)
    solve = proxy.function("compas_dr.solvers.dr_constrained_numpy" if constraints else "compas_dr.solvers.dr_numpy") if proxy else (dr_constrained_numpy if constraints else dr_numpy)
    result = solve(indata=indata, constraints=list(constraints), kmax=params.kmax, rk_steps=params.rk_steps) if constraints else solve(indata, kmax=params.kmax, rk_steps=params.rk_steps)
    result.update_mesh(mesh)
    return (json_dumps(mesh), json_dumps(result)), float(np.abs(np.asarray(result.residuals, dtype=float)).max(initial=0.0))


def _tna(mesh: Mesh, anchors: list[int], params: FormParams, proxy: Proxy | None) -> FormResult:
    form = relax_boundary_openings(FormDiagram.from_mesh(mesh), anchors)
    force = form.dual_diagram(FormDiagram)
    LoadUpdater(form, np.asarray(form.vertices_attributes("xyz"), dtype=float), density=params.rho)(form)
    horizontal = proxy.function("compas_tna.equilibrium.horizontal_numpy") if proxy else horizontal_numpy
    form, force = horizontal(form, force, kmax=params.kmax)
    form, scale = vertical_from_zmax(form, params.target, density=params.density)
    return (json_dumps(form), json_dumps(force)), scale


_FORM: Final[Mapping[FormEngine, Callable[[Mesh, list[int], FormParams, Proxy | None], FormResult]]] = MappingProxyType({
    FormEngine.DR: _dr,
    FormEngine.TNA: _tna,
})


DATASTRUCTURE: Final[Mapping[DatastructureOp, Callable[[str], object]]] = MappingProxyType({
    DatastructureOp.DUAL: lambda p: Mesh.from_json(p).dual(),
    DatastructureOp.SUBDIVIDE: lambda p: Mesh.from_json(p).subdivide(),
    DatastructureOp.VOLMESH_DUAL: lambda p: VolMesh.from_json(p).dual(),
    DatastructureOp.ASSEMBLY_GRAPH: lambda p: Assembly.from_json(p).graph,
    DatastructureOp.SURFACE_TESSELLATE: lambda p: NurbsSurface.from_json(p).to_mesh(),
})
```

## [03]-[RESEARCH]

- [COMPAS_CATALOGUE_BAR]: the `.api/compas.md` catalogue cites `bestfit_plane` (ENTRYPOINTS [06]), `bestfit_frame_numpy` ([07]), `oriented_bounding_box_numpy` ([11]), `convex_hull_numpy` ([12]), `json_dumps` (serialization [02]), `is_rhino`/`is_grasshopper` ([09]/[10]), and the `Mesh`/`Network`/`VolMesh`/`Assembly`/`NurbsSurface` datastructure rows by name, with the `compas.geometry.__all__` line carrying 342 entries (56 classes, 286 functions) verified phantom-free by introspection on the cp312 companion. The members the fence uses that are NOT yet on a cited catalogue table line — and so are companion-interpreter-gated, confirmed against the branch `compas` introspection before the fence is final, NOT asserted as catalogue-confirmed — are the construction/accessor classmethods `Network.from_nodes_and_edges`, `Mesh.dual`/`Mesh.subdivide`, `VolMesh.dual`, `Assembly.graph`, `NurbsSurface.to_mesh`, `number_of_edges`, `vertices_attributes`, the transform constructors `Frame.worldXY`/`Transformation.from_frame_to_frame`/`Translation.from_vector`/`Scale.from_factors`/`Projection.from_plane`, and the `centroid_points` free function (resolves under the verified 286-function `__all__` but its exact name/signature is gated). `Rotation`/`Shear` are dropped from the fence imports because no `NumericalOp` row constructs them; the transform table is the four pure-Python rows `RIGID`/`AFFINE`/`SIMILARITY`/`PROJECTIVE` only, and a new transform row admits its constructor the same gated way.
- [FORM_ENGINE_ARITY]: the `compas_dr.solvers.dr_numpy(indata, kmax, rk_steps)` and `dr_constrained_numpy(*, indata, constraints, kmax, rk_steps)` call shapes, `InputData.from_mesh(mesh, fixed, loads, qpre)`, `ResultData.update_mesh(mesh)` write-back, `Constraint.get_constraint_cls(geometry)` geometry-type dispatch, and `SelfweightCalculator(mesh, density).compute_tributary_areas(xyz)` confirm against the branch `compas_dr` catalogue; the `compas_tna` form/force path — `FormDiagram.from_mesh` ([11]), `relax_boundary_openings(form, fixed)` ([05]), `horizontal_numpy(form, force, kmax) -> tuple[FormDiagram, ForceDiagram]` ([01]), `vertical_from_zmax(form, zmax, density) -> tuple[FormDiagram, float]` ([04]) — are cited on `.api/compas-tna.md`. The TNA arity gated before the `FormEngine.TNA` arm is admitted: `FormDiagram.dual_diagram(FormDiagram)` (the .api documents `dual_diagram()` constructing the reciprocal `ForceDiagram` but does not cite the `cls` argument the fence passes), and the `LoadUpdater(mesh, p0, density)` constructor PLUS the `__call__(form)` application the fence invokes to write tributary selfweight onto the form (the .api cites the `LoadUpdater(mesh, p0, thickness, density, live)` constructor and `tributary_areas()`/`face_matrix()` methods but not the callable-instance update; if the introspection shows no `__call__`, the fence applies the load through `tributary_areas()` instead — the load MUST reach the form either way, never the prior dead construction). The `compas_dr` gated arity to confirm: the `ResultData.residuals` field (cited in `.api` IMPLEMENTATION_LAW as the residual field the fence reduces through `numpy.abs(...).max`), the `Constraint` constructor signature `get_constraint_cls(geom)(geom, node, damping=...)` (the .api cites `get_constraint_cls(geometry)` returning the subclass and `constraint.update(damping=0.1)`, but not the node-index binding the fence passes; the prior code constructed every constraint from a `compas.geometry.Point` for which the family has no subclass — `Circle`/`Line`/`Plane`/`Curve`/`Surface` are the only constraint geometries, so the fence now decodes the typed `NodeConstraint.geometry` COMPAS-JSON and dispatches on its real type, with the node-binding/damping argument shape the one gated piece), whether `vertices_attributes("xyz")` returns the Nx3 sequence `SelfweightCalculator`/`LoadUpdater` consume, and whether `Constraint`/`SelfweightCalculator`/`InputData`/`dr_numpy`/`dr_constrained_numpy` are `compas_dr` package-root re-exports (the fence imports them from the root) or only resolve under the `solvers`/`constraints`/`loads` submodules the dotted RPC names already name — the RPC `proxy.function("compas_dr.solvers.dr_numpy")` string is the cited submodule path either way.
- [RPC_PROXY_LIFECYCLE]: the `compas.rpc.Proxy.function(dotted_name)` resolution and the proxy server start/stop lifecycle confirm against the branch `compas` `rpc` subpackage catalogue before `bridged` routes the heavy scipy-backed band out of process; the `Proxy` context-manager shape (whether the server is started lazily on first `function` call or requires explicit `start_server`/`stop_server`) is the gated lifecycle question. The proxy `function` call is blocking RPC, so `bridged` crosses it to the event loop through `anyio.to_thread.run_sync` (`.api/anyio.md` ENTRYPOINTS [01], confirmed) — the awaitable thunk `async_boundary` requires, never a sync lambda the awaitable rail would reject; the open question is only whether the proxy server bring-up needs to run on the same worker thread as the `function` call or can be hoisted to a `bridged`-owned `AsyncExitStack`. The bridge band is the `_NUMERICAL_RPC`-rowed `bestfit_frame_numpy`/`oriented_bounding_box_numpy`/`convex_hull_numpy` primitives (scipy spatial/linalg cores) plus the `_dr`/`_tna` numpy solvers; the pure-Python transform rows, the `Network` build, and the `from_json` datastructure algebra carry no scipy core and are correctly absent from `_NUMERICAL_RPC`, so a `bridged` numerical call on a transform row runs in-thread rather than marshalling a 4x4 matrix compose across the process wall. To confirm: that the `proxy.function("compas.geometry.<name>")` dotted path resolves the `_numpy` primitives the same way the `compas_dr`/`compas_tna` submodule paths do.
- [COMPAS_CEM_GROWTH]: `compas_cem` constrained-equilibrium form-finding admits as a `FormEngine.CEM` row plus one `_FORM` arm once it ships `compas>=2.0` support — a RESEARCH-gated growth axis on the existing `FormFinding` case, not a present fence; its `TopologyDiagram`/`FormDiagram`/`ConstrainedFormDiagram` and `static_equilibrium`/`constrained_fdm` entrypoints confirm against a branch `compas_cem` catalogue before admission.
