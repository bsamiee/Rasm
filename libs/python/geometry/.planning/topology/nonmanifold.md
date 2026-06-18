# [PY_GEOMETRY_TOPOLOGY_NONMANIFOLD]

Non-manifold topological modeling. `TopologyAlgebra` is one dense dispatch surface over the stateless `topologicpy` static-method namespace: `CellComplex`/`Cell`/`Aperture` construction from B-rep bytes, decomposition into the topological hierarchy, and adjacency/dual-graph extraction the C# `IfcSemanticModel` spatial projection does not perform. Topology graphs graduate via the compute `HandoffAxis` geometry case; this owner is gated against re-deriving the in-process BIM space-graph.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[TOPOLOGY]`: non-manifold construction, decomposition, adjacency, dual-graph, and aperture extraction over the `topologicpy` handle chain.

## [2]-[TOPOLOGY]

- Owner: `TopologyAlgebra` — the static surface dispatching over the `topologicpy.Topology` handle-passing chain; `TopologyOp` the closed `StrEnum` selecting the operation; `TopologyResult` the typed receipt carrying the op, the result handles, and the graduation subject.
- Cases: `TopologyOp` rows `CONSTRUCT` (`Topology.ByBREPString` lift into a non-manifold complex) · `DECOMPOSE` (`Topology.Decompose` into the Cell/Face/Edge/Vertex hierarchy) · `ADJACENCY` (`Topology.AdjacentTopologies` cell-to-cell adjacency over `Topology.Cells`) · `DUAL_GRAPH` (`Graph.ByTopology` dual-graph extraction) · `APERTURE` (`Aperture.ByTopologyContext` binding each `Topology.Faces` opening into the host topology context) — matched by `match`/`case`, each consuming the prior static-call handle and projecting through `Topology.Analyze` or `Graph.JSONString`.
- Entry: `TopologyAlgebra.run` takes source bytes and a `TopologyOp`, and returns a `RuntimeRail[TopologyResult]`; `ByBREPString` lifts a B-rep into a `Topology` handle, `Cells`/`Decompose` enumerate the sub-topologies, `AdjacentTopologies` threads cell adjacency, and `Graph.ByTopology` projects the dual graph for graduation.
- Auto: the `topologicpy` surface is a stateless static-method namespace operating on `topologic_core` handles — each `Topology.ByX(...)` returns a handle the next static call consumes, so the dispatch threads handles through the chain rather than mutating an object; handle projection serializes through `Topology.Analyze` for the cell hierarchy and `Graph.JSONString` for the dual graph.
- Receipt: each run contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject (`topology-graph`).
- Packages: `topologicpy` (`Topology.ByBREPString`/`Topology.Cells`/`Topology.Faces`/`Topology.Decompose`/`Topology.AdjacentTopologies`/`Topology.Analyze`/`Graph.ByTopology`/`Graph.JSONString`/`Aperture.ByTopologyContext`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new topological operation is one `TopologyOp` row plus one dispatch arm threading the handle chain; IFC-bytes ingestion into non-manifold topology is a `Topology.ByIFCFile` growth axis gated on a bytes-capable IFC constructor in `nonmanifold#3-RESEARCH`; zero new surface.
- Boundary: `topologicpy` is admitted ONLY for non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and is never re-derived here; numerical and form-finding geometry is the `computational-geometry` sibling, not this owner; raw mesh-file exchange stays at the data seam; a co-equal `topologicpy`/`compas` fold into one file is the deleted form — non-manifold topology and compas numerical geometry are distinct professional domains in distinct owners.

```python signature
from enum import StrEnum
from typing import assert_never
from msgspec import Struct
from topologicpy.Aperture import Aperture
from topologicpy.Graph import Graph
from topologicpy.Topology import Topology

from rasm.runtime.receipts import ReceiptContributor
from rasm.runtime.faults import RuntimeRail, boundary


class TopologyOp(StrEnum):
    CONSTRUCT = "construct"
    DECOMPOSE = "decompose"
    ADJACENCY = "adjacency"
    DUAL_GRAPH = "dual-graph"
    APERTURE = "aperture"


class TopologyResult(Struct, frozen=True):
    op: TopologyOp
    handles: tuple[str, ...]
    graduation_subject: str = "topology-graph"


class TopologyAlgebra:
    @staticmethod
    def run(source_bytes: bytes, op: TopologyOp) -> "RuntimeRail[TopologyResult]":
        return boundary(f"topology.{op}", lambda: TopologyAlgebra._dispatch(source_bytes, op))

    @staticmethod
    def _dispatch(source_bytes: bytes, op: TopologyOp) -> TopologyResult:
        topo = Topology.ByBREPString(source_bytes.decode())
        match op:
            case TopologyOp.CONSTRUCT:
                return TopologyResult(op, (Topology.Analyze(topo),))
            case TopologyOp.DECOMPOSE:
                parts = Topology.Decompose(topo)
                return TopologyResult(op, tuple(Topology.Analyze(p) for p in parts))
            case TopologyOp.ADJACENCY:
                return TopologyResult(op, tuple(
                    Topology.Analyze(a) for c in Topology.Cells(topo) for a in Topology.AdjacentTopologies(c, topo)
                ))
            case TopologyOp.DUAL_GRAPH:
                return TopologyResult(op, (Graph.JSONString(Graph.ByTopology(topo)),))
            case TopologyOp.APERTURE:
                faces = Topology.Faces(topo)
                bound = tuple(Aperture.ByTopologyContext(f, topo) for f in faces)
                return TopologyResult(op, tuple(Topology.Analyze(a) for a in bound))
            case unreachable:
                assert_never(unreachable)
```

## [3]-[RESEARCH]

- [TOPOLOGIC_HANDLES]: the `Topology.Decompose` return collection, the `Topology.Faces` sub-topology accessor parallel to `Cells`, the `Topology.AdjacentTopologies(item, host)` arity, the `Topology.Analyze` handle-summary return shape, the `Graph.JSONString` graph-to-JSON projection verb spelling, and the `Aperture.ByTopologyContext(aperture, context)` face-into-host binding arity confirm against the branch `topologicpy` catalogue on the companion interpreter; `Topology.ByBREPString`, `Topology.Cells`, and `Graph.ByTopology` are catalogue-confirmed.
- [IFC_BYTES_CONSTRUCTOR]: the catalogue confirms `Topology.ByIFCFile`/`ByIFCPath` over a path, not an in-memory byte stream; an IFC-bytes ingestion arm into non-manifold topology resolves against a bytes-capable constructor or an `ifcopenshell.file`-to-`topologicpy` bridge on the branch `topologicpy` catalogue before the `CONSTRUCT` arm admits IFC alongside B-rep.
