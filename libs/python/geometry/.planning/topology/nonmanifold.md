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
- Growth: a new topological operation is one `TopologyOp` row plus one dispatch arm threading the handle chain; IFC-bytes ingestion into non-manifold topology is one `TopologyOp.CONSTRUCT` growth admitting the `ifcopenshell.file`-to-`topologicpy` bridge alongside the B-rep path (never a new owner, never a temp-file detour), gated on the constructor-arity confirmation in `nonmanifold#3-IFC_BYTES_CONSTRUCTOR`; zero new surface.
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
- [IFC_BYTES_CONSTRUCTOR]: the folder `.api/topologicpy.md` confirms ONLY `Topology.ByIFCFile(file)` (entrypoint row [1]) and the IFC-gated rail's `Topology.ByIFCFile`/`Topology.ByIFCPath` path-or-handle pair — no `By*Bytes` or in-memory byte-stream constructor enumerates. The exact open question gating the `CONSTRUCT` IFC-bytes arm: does `Topology.ByIFCFile(file)` accept an already-opened `ifcopenshell.file` object (the catalogue's bare `(file)` spelling is ambiguous between a path string and a file handle) or strictly a path string? Resolution paths, in priority: (1) if `ByIFCFile` accepts an `ifcopenshell.file`, the arm opens the byte stream through `ifcopenshell.open` over `guess_format` (the same polymorphic in-memory open the `tessellation/daemon.md#DAEMON` IFC arm uses) and hands the model object straight in; (2) failing that, the `IFC` integration facade (`topologicpy.md` BIM-module row [1]) is inspected for a model-object entry; (3) failing both, the `Topology.ByOCCTShape(shape)` handle path (entrypoint row [6]) is the bridge — the `ifcopenshell.file` geometry lifts to an OCCT shape that `ByOCCTShape` ingests. The temp-file detour (writing the byte stream to disk so the path constructor reads it) is the FORBIDDEN form: the byte stream owns the model everywhere else in the companion, and a path round-trip re-introduces filesystem I/O the daemon's in-memory open exists to avoid. This residual stays a marked RESEARCH gate until a branch `topologicpy` companion-interpreter assay confirms one of the three constructor arities; the `CONSTRUCT` arm admits IFC alongside B-rep as ONE `TopologyOp.CONSTRUCT` growth (never a new owner) only after that confirmation. Secondary gate: `topologicpy` is itself absent from the manifest (consumed by this page and `computational-geometry/algebra.md`), so the admission is moot until the `python_version<'3.15'` companion-band row lands.
