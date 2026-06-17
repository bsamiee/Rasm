# [PY_GEOMETRY_COMPUTATIONAL_GEOMETRY_ALGEBRA]

AEC computational and numerical geometry. `ComputationalGeometry` is one dispatch surface over `compas`: graph/network adjacency, structural form-finding (dynamic relaxation over `compas_dr`, thrust-network analysis over `compas_tna`), best-fit/boolean/bbox numerical primitives, and mesh datastructure algebra. Form-found and network results graduate via the compute `HandoffAxis` geometry case. This is distinct from non-manifold topology (the `topology` sibling) and from raw mesh-file exchange at the data seam.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[ALGEBRA]`: network adjacency, structural form-finding, numerical primitives, and mesh datastructure algebra under one tagged union.

## [2]-[ALGEBRA]

- Owner: `ComputationalGeometry` — the tagged union discriminating by algebra-kind; `AlgebraResult` the typed receipt carrying the kind, the result handles, and the graduation subject.
- Cases: `ComputationalGeometry` cases `Network(vertices, edges)` (compas `Network` adjacency) · `FormFinding(mesh, anchors)` (dynamic relaxation over `compas_dr.solvers.dr_numpy`) · `Numerical(points, op)` (the `NumericalOp`-keyed `compas.geometry` free-function table) · `MeshAlgebra(mesh, op)` (compas `datastructures.Mesh` dual/subdivision) — matched by `match`/`case`, each dispatching to the compas surface that owns it; the numerical sub-op is the closed `NumericalOp` vocabulary, never a raw string literal inside the case payload.
- Entry: `evaluate` dispatches the case and returns a `RuntimeRail[AlgebraResult]`; the numerical case folds the `NUMERICAL` free-function table by `NumericalOp`, the form-finding case runs `dr_numpy` over a `Mesh.from_json` graph with fixed anchors, the network case builds a `Network` from nodes and edges, and `json_dumps` serializes each result for graduation.
- Auto: the compas surface composes pure-Python primitives with `_numpy`-accelerated best-fit and bbox variants reached through the `NUMERICAL` table; dynamic relaxation folds the vertex/edge graph and anchor set through `dr_numpy` returning equilibrium coordinates and residuals; `json_dumps` produces the graduation payload for every arm.
- Receipt: each evaluation contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject (`form-finding`, `network-graph`).
- Packages: `compas` (`geometry.bestfit_plane`/`geometry.bbox_numpy`/`datastructures.Mesh`/`datastructures.Network`/`json_dumps`), `compas_dr` (`solvers.dr_numpy`), `compas_tna` (form/force-diagram analysis), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new algebra kind is one `ComputationalGeometry` case plus one dispatch arm; a new numerical op is one `NumericalOp` row plus one `NUMERICAL` table entry, never a new dispatch branch; zero new surface.
- Boundary: non-manifold cell/aperture topology is the `topology` sibling over `topologicpy`, never folded here; robust mesh repair/boolean is the `mesh-utility` sibling over `trimesh`/`manifold3d`, not the compas datastructure algebra; raw mesh-file exchange stays at the data seam; `compas_cem` (constrained-equilibrium form-finding) does not yet support `compas>=2.0` and is a verify-before-admit candidate, never a settled fence — form-finding routes `compas_dr`/`compas_tna` until CEM lands 2.x support.

```python signature
from collections.abc import Callable
from typing import Final, Literal
import compas.geometry
from compas import json_dumps
from compas.datastructures import Mesh, Network
from compas_dr.solvers import dr_numpy
from enum import StrEnum
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.observability.receipts import ReceiptContributor
from rasm.runtime.faults import RuntimeRail, boundary

Points = tuple[tuple[float, float, float], ...]


class NumericalOp(StrEnum):
    BESTFIT_PLANE = "bestfit-plane"
    BBOX = "bbox"


NUMERICAL: Final[dict[NumericalOp, Callable[[list[list[float]]], object]]] = {
    NumericalOp.BESTFIT_PLANE: compas.geometry.bestfit_plane,
    NumericalOp.BBOX: compas.geometry.bbox_numpy,
}


@tagged_union(frozen=True)
class ComputationalGeometry:
    tag: Literal["network", "form_finding", "numerical", "mesh"] = tag()
    network: tuple[Points, tuple[tuple[int, int], ...]] = case()
    form_finding: tuple[str, tuple[int, ...]] = case()
    numerical: tuple[Points, NumericalOp] = case()
    mesh: tuple[str, Literal["subdivide", "dual"]] = case()

    @staticmethod
    def Network(vertices: Points, edges: tuple[tuple[int, int], ...]) -> "ComputationalGeometry":
        return ComputationalGeometry(network=(vertices, edges))

    @staticmethod
    def FormFinding(mesh: str, anchors: tuple[int, ...]) -> "ComputationalGeometry":
        return ComputationalGeometry(form_finding=(mesh, anchors))

    @staticmethod
    def Numerical(points: Points, op: NumericalOp) -> "ComputationalGeometry":
        return ComputationalGeometry(numerical=(points, op))

    @staticmethod
    def MeshAlgebra(mesh: str, op: Literal["subdivide", "dual"]) -> "ComputationalGeometry":
        return ComputationalGeometry(mesh=(mesh, op))


class AlgebraResult(Struct, frozen=True):
    kind: str
    handles: tuple[str, ...]
    graduation_subject: str


def evaluate(algebra: ComputationalGeometry) -> "RuntimeRail[AlgebraResult]":
    return boundary(f"algebra.{algebra.tag}", lambda: _dispatch(algebra))


def _dispatch(algebra: ComputationalGeometry) -> AlgebraResult:
    match algebra:
        case ComputationalGeometry(tag="network", network=(vertices, edges)):
            graph = Network.from_nodes_and_edges([list(v) for v in vertices], list(edges))
            return AlgebraResult("network", (json_dumps(graph),), "network-graph")
        case ComputationalGeometry(tag="numerical", numerical=(points, op)):
            return AlgebraResult("numerical", (json_dumps(NUMERICAL[op]([list(p) for p in points])),), "network-graph")
        case ComputationalGeometry(tag="form_finding", form_finding=(mesh, anchors)):
            result = dr_numpy(Mesh.from_json(mesh), list(anchors))
            return AlgebraResult("form_finding", (json_dumps(result),), "form-finding")
        case ComputationalGeometry(tag="mesh", mesh=(mesh, _)):
            return AlgebraResult("mesh", (json_dumps(Mesh.from_json(mesh).dual()),), "network-graph")
```

## [3]-[RESEARCH]

- [COMPAS_DATASTRUCTURE]: the `Network.from_nodes_and_edges` constructor arity, the `Mesh.from_json`/`Mesh.dual` round-trip, and the `compas.datastructures.Mesh` subdivision verb confirm against the branch `compas` catalogue on the companion interpreter; `bestfit_plane`, `bbox_numpy`, and `json_dumps` are catalogue-confirmed.
- [DR_NUMPY_ARITY]: the `compas_dr.solvers.dr_numpy` argument shape (mesh edge graph, fixed-anchor index set, force-density and load vectors) and the equilibrium/residual return confirm against the branch `compas_dr` catalogue; the `compas_tna` form/force-diagram entrypoints confirm before the thrust-network method is admitted as a `FormFinding` variant.
