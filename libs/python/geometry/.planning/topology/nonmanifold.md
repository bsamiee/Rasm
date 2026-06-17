# [PY_GEOMETRY_TOPOLOGY_NONMANIFOLD]

Non-manifold topological modeling. `TopologyAlgebra` is one dense dispatch surface over the stateless `topologicpy` static-method namespace: `CellComplex`/`Cell`/`Aperture` construction from B-rep or IFC bytes, decomposition into the topological hierarchy, and adjacency/dual-graph extraction the C# `IfcSemanticModel` spatial projection does not perform. Topology graphs graduate via the compute `HandoffAxis` geometry case; this owner is gated against re-deriving the in-process BIM space-graph.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[TOPOLOGY]`: non-manifold construction, decomposition, adjacency, dual-graph, and aperture extraction over the `topologicpy` handle chain.

## [2]-[TOPOLOGY]

- Owner: `TopologyAlgebra` — the static surface dispatching over the `topologicpy.Topology` handle-passing chain; `TopologyOp` the closed `StrEnum` selecting the operation; `TopologyResult` the typed receipt carrying the op, the result handles, and the graduation subject.
- Cases: `TopologyOp` rows `CONSTRUCT` (`Topology.ByBREPString` lift into a non-manifold complex) · `DECOMPOSE` (`Topology.Decompose` into the Cell/Face/Edge/Vertex hierarchy) · `ADJACENCY` (`Topology.AdjacentTopologies` cell-to-cell adjacency over `Topology.Cells`) · `DUAL_GRAPH` (`Graph.ByTopology` dual-graph extraction) · `APERTURE` (`Aperture.ByTopologyContext` opening/host-face binding) — matched by `match`/`case`, each consuming the prior static-call handle and projecting through `Topology.Analyze` or `Graph.JSONString`.
- Entry: `TopologyAlgebra.run` takes source bytes and a `TopologyOp`, and returns a `RuntimeRail[TopologyResult]`; `ByBREPString` lifts a B-rep into a `Topology` handle, `Cells`/`Decompose` enumerate the sub-topologies, `AdjacentTopologies` threads cell adjacency, and `Graph.ByTopology` projects the dual graph for graduation.
- Auto: the `topologicpy` surface is a stateless static-method namespace operating on `topologic_core` handles — each `Topology.ByX(...)` returns a handle the next static call consumes, so the dispatch threads handles through the chain rather than mutating an object; handle projection serializes through `Topology.Analyze` for the cell hierarchy and `Graph.JSONString` for the dual graph.
- Receipt: each run contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject (`topology-graph`).
- Packages: `topologicpy` (`Topology.ByBREPString`/`Topology.ByIFCFile`/`Topology.Cells`/`Topology.Decompose`/`Topology.AdjacentTopologies`/`Topology.Analyze`/`Graph.ByTopology`/`Graph.JSONString`/`Aperture.ByTopologyContext`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new topological operation is one `TopologyOp` row plus one dispatch arm threading the handle chain; zero new surface.
- Boundary: `topologicpy` is admitted ONLY for non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and is never re-derived here; numerical and form-finding geometry is the `computational-geometry` sibling, not this owner; raw mesh-file exchange stays at the data seam; a co-equal `topologicpy`/`compas` fold into one file (the over-collapse the prior model forced) is the deleted form — non-manifold topology and compas numerical geometry are distinct professional domains in distinct owners.

```python signature
from enum import StrEnum
from msgspec import Struct
from topologicpy.Aperture import Aperture
from topologicpy.Graph import Graph
from topologicpy.Topology import Topology

from rasm.runtime.observability.receipts import ReceiptContributor
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
                return TopologyResult(op, (Graph.JSONString(Graph.ByTopology(topo)),))
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
                bound = Aperture.ByTopologyContext(topo, topo)
                return TopologyResult(op, (Topology.Analyze(bound),))
```

## [3]-[RESEARCH]

- [TOPOLOGIC_HANDLES]: the `Topology.Decompose` return collection, the `Topology.AdjacentTopologies(item, host)` arity, the `Topology.Analyze` handle-summary return shape, and the `Aperture.ByTopologyContext(aperture, context)` host-face binding arity confirm against the branch `topologicpy` catalogue on the companion interpreter; `Topology.ByBREPString`, `Topology.Cells`, `Graph.ByTopology`, and `Graph.JSONString` are catalogue-confirmed.
