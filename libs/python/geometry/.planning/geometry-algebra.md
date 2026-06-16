# [PY_GEOMETRY_GEOMETRY_ALGEBRA]

Non-manifold topology AND AEC computational geometry folded into ONE algebra owner. `GeometryAlgebra` discriminates by algebra-kind row: non-manifold cell/aperture topology over `topologicpy` and AEC computational geometry (meshes, networks, assemblies, form-finding, numerical geometry) over `compas`. Form-found and topology results graduate via the compute `HandoffAxis` geometry case. This is distinct from raw mesh-file exchange (data) and is gated against the C# `IfcSemanticModel`, which already projects the BIM space-graph in-process.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                        |
| :-----: | :-------- | :------------------------------------------------------------ |
|   [1]   | ALGEBRA   | non-manifold topology + AEC computational geometry, one owner |

## [2]-[ALGEBRA]

- Owner: `GeometryAlgebra` — the tagged union discriminating by algebra-kind; `AlgebraResult` the typed receipt carrying the kind, the result handles, and the graduation subject.
- Cases: `GeometryAlgebra` cases `Topology(ifc_bytes)` (non-manifold `CellComplex`/`Cell`/`Aperture` over topologicpy) · `Network(vertices, edges)` (compas `Graph`/`Network` adjacency) · `FormFinding(mesh, anchors)` (compas form-finding) · `Numerical(points, op)` (compas best-fit/boolean/bbox) — matched by `match`/`case`, each dispatching to the package surface that owns it.
- Entry: `GeometryAlgebra.evaluate` dispatches the case and returns a `RuntimeRail[AlgebraResult]`; the topology case runs `topologicpy.Topology.ByIFCFile`/`ByBREPString` into a `CellComplex` and reads `Topology.Cells`/`Decompose`/`AdjacentTopologies`; the numerical case runs `compas.geometry` free functions (`bestfit_plane`, `boolean_difference_mesh_mesh`, `bbox_numpy`).
- Auto: the topologicpy surface is a stateless static-method namespace operating on `topologic_core` handles — `Topology.ByX(...)` returns a handle the next static call consumes; the compas surface composes pure-Python primitives with `_numpy`-accelerated best-fit/bbox variants; `compas.json_dumps` serializes a result graph for graduation.
- Receipt: each evaluation contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject (`topology-graph`, `form-finding`).
- Packages: `topologicpy` (`Topology.ByIFCFile`/`ByBREPString`/`Cells`/`Decompose`/`AdjacentTopologies`/`CellComplex.ByCells`/`Graph.ByTopology`), `compas` (`geometry.bestfit_plane`/`boolean_difference_mesh_mesh`/`bbox_numpy`/`datastructures.Mesh`/`Network`/`json_dumps`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new algebra kind is one `GeometryAlgebra` case plus one dispatch arm; a new numerical op is one branch in the numerical case; zero new surface, never two thin 1:1 wrappers.
- Boundary: `topologicpy` is admitted ONLY for non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process by GeometryGym and is never re-derived here; raw mesh-file exchange stays in data; a co-equal `topologicpy`/`compas` peer pair instead of one folded owner is the deleted form. This owner is `SPIKE` on the companion floor.

```python signature
from typing import Literal

import compas.geometry
from compas import json_dumps
from expression import case, tag, tagged_union
from msgspec import Struct
from topologicpy.Topology import Topology

from rasm.runtime.rails_resilience import RuntimeRail, boundary


@tagged_union(frozen=True)
class GeometryAlgebra:
    tag: Literal["topology", "network", "form_finding", "numerical"] = tag()
    topology: bytes = case()
    network: tuple[tuple[float, ...], tuple[tuple[int, int], ...]] = case()
    form_finding: tuple[str, tuple[int, ...]] = case()
    numerical: tuple[tuple[tuple[float, float, float], ...], str] = case()

    @staticmethod
    def Topology(ifc_bytes: bytes) -> "GeometryAlgebra":
        return GeometryAlgebra(topology=ifc_bytes)

    @staticmethod
    def Network(vertices: tuple[float, ...], edges: tuple[tuple[int, int], ...]) -> "GeometryAlgebra":
        return GeometryAlgebra(network=(vertices, edges))

    @staticmethod
    def Numerical(points: tuple[tuple[float, float, float], ...], op: str) -> "GeometryAlgebra":
        return GeometryAlgebra(numerical=(points, op))


class AlgebraResult(Struct, frozen=True):
    kind: str
    handles: tuple[str, ...]
    graduation_subject: str


def evaluate(algebra: GeometryAlgebra) -> "RuntimeRail[AlgebraResult]":
    return boundary(f"algebra.{algebra.tag}", lambda: _dispatch(algebra))


def _dispatch(algebra: GeometryAlgebra) -> AlgebraResult:
    match algebra:
        case GeometryAlgebra(tag="topology", topology=ifc_bytes):
            topo = Topology.ByBREPString(ifc_bytes.decode())
            cells = Topology.Cells(topo)
            return AlgebraResult("topology", tuple(str(id(c)) for c in cells), "topology-graph")
        case GeometryAlgebra(tag="numerical", numerical=(points, "bestfit-plane")):
            plane = compas.geometry.bestfit_plane(list(points))
            return AlgebraResult("numerical", (json_dumps(plane),), "form-finding")
        case _:
            return AlgebraResult(algebra.tag, (), "form-finding")
```

## [3]-[RESEARCH]

- [TOPOLOGIC_HANDLES]: the `topologicpy.Topology` static-method handle-passing pattern (`ByBREPString` to `Cells` to `Decompose`) and the `compas.geometry.bestfit_plane` return shape are verified against `.api/api-topologicpy.md` and `.api/api-compas.md`; the `CellComplex.ByCells` non-manifold construction confirms on the cp312 companion interpreter once the floor/lock-scope decision admits the sub-3.13 environment.
