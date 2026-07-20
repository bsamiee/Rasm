# [PY_GEOMETRY_GRAPH_NONMANIFOLD]

Non-manifold topological modeling over the stateless `topologicpy` static-method namespace: construction from B-rep/OCCT/JSON/OBJ/IFC bytes, hierarchy decomposition, the non-manifold boolean kernel, cell adjacency, attribute attachment, geometric analysis, and `Graph.ByTopology` dual-graph extraction with the connectivity/centrality/spanning/path analytics the C# `IfcSemanticModel` spatial projection does not perform. Each case folds through one of the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` data tables, never parallel arms; the `topology-graph` subject crosses HERE, and `network-graph` stays with the `features`/`algebra` siblings.

`topologicpy` is an opt-in Forge-lane companion excluded from the default server build — its `AGPL-3.0-or-later` network-copyleft terms require an explicit accepting worker lane — so every `topologicpy` and `ifcopenshell` binding stays function-local behind the cached `_topo`/`_graph`/`_cluster`/`_dictionary` facade accessors, never a module-top import loading the AGPL band into every companion start; the owner and fences stay authored, runtime admission binding to the companion-lane provisioning charter. Reducer-return vocabulary imports downward from the tier-0 `graph/analytic` substrate, no page-local twin; `run` and `bridged` return through the graduation `evidence_run` weave seeded `EvidenceScope.GRAPH_TOPOLOGY` — span, fence, and receipt harvest in one composition over the pure `_dispatch` — and `bridged` crosses as a `KernelTrait.HOSTILE` kernel onto the warm process pool, because the TopologicPy/OCCT core holds process-global native state and imports under no isolated subinterpreter, so a thread or subinterpreter arm is the untruthful trait; the sibling wiring convention holds unchanged.

## [01]-[INDEX]

- [01]-[TOPOLOGY]: `TopologyOp` union, its `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` tables over the cached AGPL facades, and the `run`/`bridged` pair under one `ReceiptContributor`.

## [02]-[TOPOLOGY]

- Owner: `run` is the one module-level entrypoint — no stateful capsule, no mutable receipt accumulator. `TopologyResult` is the sole `ReceiptContributor`, its phase data-driven — `emitted` for a clean extraction, `admitted` for a degenerate result (an empty decomposition, a null boolean, a zero-node dual graph) — so a caveat is flagged rather than asserted. Every parameterized case's sub-kind is a closed `StrEnum`, never a raw string in the payload.
- Entry: `run` discriminates a single op or a batch, each returning through its own weave rail; `bridged` never collapses an offload fault into a synthetic degenerate result — a failure stays an `Error(BoundaryFault)` on the returned rail.
- Auto: every static call returns an opaque `topologic_core` handle the next call consumes, so dispatch threads handles through the chain rather than mutating an object; the topologicpy centralities return vertex-ordered score lists — the Sequence arm of the substrate's one shape-discriminated `ranked` fold, shared with the networkx sibling's dict arm.
- Receipt: only the dual-graph case graduates — `GeometrySubject.TOPOLOGY_GRAPH`, gating `empty_node_fraction` against the zero ceiling so a degenerate graph breaches rather than crossing clean; the JSON-bytes payload is the evidence the crossing keys; the non-graph ops emit the receipt only.
- Packages: `topologicpy` and `ifcopenshell` bound ONLY through the cached facade accessors on the AGPL gate; the analytic vocabulary and the graduation spine import downward from their geometry owners.
- Growth: a new intake format is one `SourceKind` row and one `_CONSTRUCT` entry; a new boolean or analysis verb is one enum row and one table entry; a new graph analytic is one `GraphAnalytic` row and one `GRAPH_ANALYTIC` reducer; the bottom-up construction family (`Vertex.ByCoordinates`/`Cell.ByFaces`/`CellComplex.ByCells`), the `Aperture.ByTopologyContext` opening topology, and the `BVH` clash/raycast surface admit as further rows when a consumer demands them — table growth, never a new page.
- Boundary: `topologicpy` is admitted ONLY for the non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and never re-derived here; numerical/form-finding geometry is the `algebra` sibling's, mesh-feature projection the `features` sibling's, and raw mesh-file exchange stays at the data `MeshPayload` seam — `run` returns handle/JSON-bytes summaries and never writes a topology file.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from functools import cache, partial
from types import MappingProxyType
from typing import Final, Literal, assert_never

import msgspec
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.graph.analytic import AnalyticValue, peak_of, ranked
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type OpTag = Literal["construct", "decompose", "adjacency", "boolean", "analysis", "attribute", "dual_graph"]
# every static call returns a bare `topologic_core` handle (`Topology`/`Graph`/`Dictionary` C++
# object), opaque with no Python stub, so handles thread the chain typed only as `object`.
type Handle = object
type Reducer = Callable[[Handle, "TopologyPolicy"], AnalyticValue]


class SourceKind(StrEnum):
    BREP = "brep"
    OCCT = "occt"
    JSON = "json"
    OBJ = "obj"
    IFC = "ifc"


class SubTopologyKind(StrEnum):
    # closed `Topology.SubTopologies(subTopologyType=)` vocabulary — the generic hierarchy
    # accessor; `Topology.Decompose` is the building-element role classifier, never bound here.
    VERTEX = "vertex"
    EDGE = "edge"
    WIRE = "wire"
    FACE = "face"
    SHELL = "shell"
    CELL = "cell"
    CELL_COMPLEX = "cellcomplex"
    CLUSTER = "cluster"
    APERTURE = "aperture"


class BooleanKind(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECT = "intersect"
    SLICE = "slice"


class AnalysisKind(StrEnum):
    BBOX = "bounding-box"
    CENTROID = "centroid"
    CONTAINS = "contains"


class GraphAnalytic(StrEnum):
    CONNECTIVITY = "connectivity"
    BETWEENNESS = "betweenness-centrality"
    CLOSENESS = "closeness-centrality"
    DEGREE = "degree-centrality"
    SPANNING = "spanning-tree"
    SHORTEST_PATH = "shortest-path"


# --- [CONSTANTS] ------------------------------------------------------------------------

# dual-graph residual the graduation carrier gates: a degenerate graph (zero nodes) breaches.
_GRAPH_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_node_fraction": 0.0})

# --- [BOUNDARIES] -------------------------------------------------------------------------


# AGPL gate: one cached boundary import per topologicpy facade, so the module loads clean on the
# default lane and the opt-in Forge lane pays the import once — never a module-top AGPL binding.
@cache
def _topo() -> type:
    from topologicpy.Topology import Topology  # noqa: PLC0415

    return Topology


@cache
def _graph() -> type:
    from topologicpy.Graph import Graph  # noqa: PLC0415

    return Graph


@cache
def _cluster() -> type:
    from topologicpy.Cluster import Cluster  # noqa: PLC0415

    return Cluster


@cache
def _dictionary() -> type:
    from topologicpy.Dictionary import Dictionary  # noqa: PLC0415

    return Dictionary


# --- [MODELS] ---------------------------------------------------------------------------


class TopologyPolicy(Struct, frozen=True, gc=False):
    analytics: frozenset[GraphAnalytic] = frozenset(GraphAnalytic)
    centrality_top: int = 16  # leaderboard cap the betweenness/closeness/degree rows truncate to


class TopologyCensus(Struct, frozen=True, gc=False):
    op: OpTag
    handles: int
    cells: int = 0
    faces: int = 0
    edges: int = 0
    vertices: int = 0
    nodes: int = 0  # dual-graph node count; 0 for non-graph ops
    components: int = 0
    betweenness_max: float = 0.0
    path_length: int = 0


class TopologyResult(Struct, frozen=True):
    op: OpTag
    handles: tuple[str, ...]  # `Topology.Analyze` summaries — the one projection every arm threads; the dual-graph arm a compact node/edge descriptor
    census: TopologyCensus
    evidence: bytes = b""  # `msgspec.json.encode` dual-graph payload; b"" for non-graph ops
    graduation_subject: GeometrySubject = GeometrySubject.TOPOLOGY_GRAPH
    degenerate: bool = False

    def contribute(self) -> tuple[Receipt, ...]:
        phase: Phase = "admitted" if self.degenerate else "emitted"
        facts: dict[str, object] = {**structs.asdict(self.census), "handle_count": len(self.handles)}
        return (Receipt.of("rasm.geometry.graph.nonmanifold", (phase, self.graduation_subject, facts)),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        empty = 0.0 if self.census.nodes else 1.0
        return GeometryHandoff.of(self.graduation_subject, evidence_key, {"empty_node_fraction": empty}, _GRAPH_CEILING)


# --- [OPERATIONS] -----------------------------------------------------------------------


@tagged_union(frozen=True)
class TopologyOp:
    tag: OpTag = tag()
    construct: tuple[bytes, SourceKind] = case()
    decompose: tuple[bytes, SubTopologyKind] = case()
    adjacency: bytes = case()
    boolean: tuple[bytes, bytes, BooleanKind] = case()
    analysis: tuple[bytes, AnalysisKind] = case()
    attribute: tuple[bytes, tuple[str, ...], tuple[str, ...]] = case()
    dual_graph: tuple[bytes, TopologyPolicy] = case()

    @staticmethod
    def Construct(source: bytes, kind: SourceKind = SourceKind.BREP) -> "TopologyOp":
        return TopologyOp(construct=(source, kind))

    @staticmethod
    def Decompose(source: bytes, kind: SubTopologyKind = SubTopologyKind.CELL) -> "TopologyOp":
        return TopologyOp(decompose=(source, kind))

    @staticmethod
    def Adjacency(source: bytes) -> "TopologyOp":
        return TopologyOp(adjacency=source)

    @staticmethod
    def Boolean(a: bytes, b: bytes, kind: BooleanKind) -> "TopologyOp":
        return TopologyOp(boolean=(a, b, kind))

    @staticmethod
    def Analysis(source: bytes, kind: AnalysisKind) -> "TopologyOp":
        return TopologyOp(analysis=(source, kind))

    @staticmethod
    def Attribute(source: bytes, keys: tuple[str, ...], values: tuple[str, ...]) -> "TopologyOp":
        return TopologyOp(attribute=(source, keys, values))

    @staticmethod
    def DualGraph(source: bytes, policy: TopologyPolicy = TopologyPolicy()) -> "TopologyOp":
        return TopologyOp(dual_graph=(source, policy))


def _lift(source: bytes, kind: SourceKind) -> Handle:
    return _CONSTRUCT[kind](source)


def _ifc_cluster(source: bytes) -> Handle:
    # SPF bytes -> in-memory `ifcopenshell.file` -> per-product topology list folded to one non-manifold
    # `Cluster` handle: `ByIFCFile` takes the file OBJECT (not a path) and returns a list, so the
    # `Cluster.ByTopologies` fold collapses it to one `Handle`, shape-uniform with the sibling rows.
    import ifcopenshell  # noqa: PLC0415  AGPL companion gate, the same in-memory loader mesh/daemon opens through

    return _cluster().ByTopologies(*_topo().ByIFCFile(ifcopenshell.file.from_string(source.decode("utf-8"))))


def _path(graph: Handle) -> AnalyticValue:
    # `Topology.Vertices` is the polymorphic accessor over `Topology` OR `Graph` OR the `Wire` `ShortestPath` returns; a null
    # geodesic (disconnected graph or <=1 vertex) folds to `Scalar(0.0)`, so the row stays total.
    verts = _topo().Vertices(graph)
    span = _graph().ShortestPath(graph, verts[0], verts[-1]) if len(verts) > 1 else None
    return AnalyticValue.Scalar(float(len(_topo().Vertices(span))) if span is not None else 0.0)


_CONSTRUCT: Final[Mapping[SourceKind, Callable[[bytes], Handle]]] = MappingProxyType({
    SourceKind.BREP: lambda b: _topo().ByBREPString(b.decode()),
    SourceKind.OCCT: lambda b: _topo().ByOCCTShape(b),  # OpenCASCADE handle intake: the row consumes the shape handle payload
    SourceKind.JSON: lambda b: _topo().ByJSONString(b.decode()),
    SourceKind.OBJ: lambda b: _topo().ByOBJPath(b.decode()),
    SourceKind.IFC: _ifc_cluster,
})

_BOOLEAN: Final[Mapping[BooleanKind, Callable[[Handle, Handle], Handle]]] = MappingProxyType({
    BooleanKind.UNION: lambda a, b: _topo().Union(a, b),
    BooleanKind.DIFFERENCE: lambda a, b: _topo().Difference(a, b),
    BooleanKind.INTERSECT: lambda a, b: _topo().Intersect(a, b),
    BooleanKind.SLICE: lambda a, b: _topo().Slice(a, b),
})

_ANALYSIS: Final[Mapping[AnalysisKind, Callable[[Handle], Handle]]] = MappingProxyType({
    AnalysisKind.BBOX: lambda t: _topo().BoundingBox(t),
    AnalysisKind.CENTROID: lambda t: _topo().Centroid(t),
    AnalysisKind.CONTAINS: lambda t: _topo().Contains(t, _topo().Centroid(t)),
})

# alias traps live here: connectivity is `len(ConnectedComponents(...))` — NEVER `Graph.Connectivity`, the documented
# `DegreeCentrality` alias whose per-vertex list is a type error here; spanning is the TRUE `MinimumSpanningTree` — never
# `Graph.Tree`, a BFS/DFS traversal tree; the wire evidence is the dict-returning `JSONData`, never `JSONString` pre-serialized.
GRAPH_ANALYTIC: Final[Mapping[GraphAnalytic, Reducer]] = MappingProxyType({
    GraphAnalytic.CONNECTIVITY: lambda g, _: AnalyticValue.Scalar(float(len(_graph().ConnectedComponents(g)))),
    GraphAnalytic.BETWEENNESS: lambda g, p: ranked(_graph().BetweennessCentrality(g), p.centrality_top),
    GraphAnalytic.CLOSENESS: lambda g, p: ranked(_graph().ClosenessCentrality(g), p.centrality_top),
    GraphAnalytic.DEGREE: lambda g, p: ranked(_graph().DegreeCentrality(g), p.centrality_top),
    GraphAnalytic.SPANNING: lambda g, _: AnalyticValue.Groups(
        tuple(tuple(_topo().Vertices(e)) for e in _graph().Edges(_graph().MinimumSpanningTree(g)))
    ),
    GraphAnalytic.SHORTEST_PATH: lambda g, _: _path(g),
})


def _dispatch(op: TopologyOp) -> TopologyResult:
    topo = _topo()
    match op:
        case TopologyOp(tag="construct", construct=(source, kind)):
            handle = _lift(source, kind)
            return _result("construct", (topo.Analyze(handle),), _census("construct", handle, handles=1))
        case TopologyOp(tag="decompose", decompose=(source, kind)):
            # `None` on a bad kind folds to the degenerate empty result.
            parts = tuple(topo.SubTopologies(_lift(source, SourceKind.BREP), subTopologyType=kind.value) or ())
            return _result("decompose", tuple(topo.Analyze(p) for p in parts), _decompose_census("decompose", parts))
        case TopologyOp(tag="adjacency", adjacency=source):
            handle = _lift(source, SourceKind.BREP)
            adj = tuple(a for c in topo.Cells(handle) for a in topo.AdjacentTopologies(c, handle))
            return _result("adjacency", tuple(topo.Analyze(a) for a in adj), _census("adjacency", handle, handles=len(adj)))
        case TopologyOp(tag="boolean", boolean=(a, b, kind)):
            combined = _BOOLEAN[kind](_lift(a, SourceKind.BREP), _lift(b, SourceKind.BREP))
            handles = (topo.Analyze(combined),) if combined is not None else ()
            return _result("boolean", handles, _census("boolean", combined, handles=len(handles)), degenerate=combined is None)
        case TopologyOp(tag="analysis", analysis=(source, kind)):
            value = _ANALYSIS[kind](_lift(source, SourceKind.BREP))
            return _result("analysis", (topo.Analyze(value),), _census("analysis", value, handles=1))
        case TopologyOp(tag="attribute", attribute=(source, keys, values)):
            handle = topo.AddDictionary(_lift(source, SourceKind.BREP), _dictionary().ByKeysValues(list(keys), list(values)))
            return _result("attribute", (topo.Analyze(handle),), _census("attribute", handle, handles=1))
        case TopologyOp(tag="dual_graph", dual_graph=(source, policy)):
            graph = _graph().ByTopology(_lift(source, SourceKind.BREP))
            census = _graph_census(graph, _graph_analytics(graph, policy))
            # wire summary is the compact census-keyed descriptor; the whole serialized document
            # rides `evidence` once, never a multi-KB JSON string duplicated across both fields.
            evidence = msgspec.json.encode(_graph().JSONData(graph))
            summary = f"graph nodes={census.nodes} components={census.components} path={census.path_length}"
            return _result("dual_graph", (summary,), census, evidence=evidence, degenerate=census.nodes == 0)
        case _ as unreachable:
            assert_never(unreachable)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _result(op: OpTag, handles: tuple[str, ...], census: TopologyCensus, *, evidence: bytes = b"", degenerate: bool = False) -> TopologyResult:
    return TopologyResult(op=op, handles=handles, census=census, evidence=evidence, degenerate=degenerate or not handles)


def _census(op: OpTag, handle: Handle, *, handles: int) -> TopologyCensus:
    # a null boolean handle yields the zero census; `_decompose_census` owns the tuple arm — no runtime shape probe.
    if handle is None:
        return TopologyCensus(op=op, handles=handles)
    topo = _topo()
    return TopologyCensus(
        op=op,
        handles=handles,
        cells=len(topo.Cells(handle)),
        faces=len(topo.Faces(handle)),
        edges=len(topo.Edges(handle)),
        vertices=len(topo.Vertices(handle)),
    )


def _decompose_census(op: OpTag, parts: tuple[Handle, ...]) -> TopologyCensus:
    return _census(op, parts[0], handles=len(parts)) if parts else TopologyCensus(op=op, handles=0)


def _graph_analytics(graph: Handle, policy: TopologyPolicy) -> Map[GraphAnalytic, AnalyticValue]:
    # a zero/one-node graph skips the path/centrality rows by data, not a branch.
    nodes = len(_topo().Vertices(graph))
    selected = (a for a in GraphAnalytic if a in policy.analytics and (nodes > 1 or a is GraphAnalytic.CONNECTIVITY))
    return Map.of_seq((a, GRAPH_ANALYTIC[a](graph, policy)) for a in selected) if nodes else Map.empty()


def _graph_census(graph: Handle, analytics: Map[GraphAnalytic, AnalyticValue]) -> TopologyCensus:
    # census reads the folded map through the substrate `peak_of` projection rather than re-measuring.
    return TopologyCensus(
        op="dual_graph",
        handles=1,
        nodes=len(_topo().Vertices(graph)),
        components=int(peak_of(analytics, GraphAnalytic.CONNECTIVITY)),
        betweenness_max=peak_of(analytics, GraphAnalytic.BETWEENNESS),
        path_length=int(peak_of(analytics, GraphAnalytic.SHORTEST_PATH)),
    )


def run(op: TopologyOp | Sequence[TopologyOp]) -> RuntimeRail[TopologyResult] | RuntimeRail[Block[TopologyResult]]:
    # each op returns through its own GRAPH_TOPOLOGY weave — span, fence, and receipt harvest in one composition — and
    # a batch folds the weave rails through traversed(ACCUMULATE); `i=item` binds the loop variable per closure.
    match op:
        case Sequence() as batch:
            return traversed(
                Block.of_seq([evidence_run(EvidenceScope.GRAPH_TOPOLOGY, f"run.{item.tag}", lambda i=item: _dispatch(i)) for item in batch]),
                by=Disposition.ACCUMULATE,
            )
        case TopologyOp() as single:
            return evidence_run(EvidenceScope.GRAPH_TOPOLOGY, f"run.{single.tag}", lambda: _dispatch(single))
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: TopologyOp, lane: LanePolicy) -> RuntimeRail[TopologyResult]:
    # HOSTILE: the OCCT-backed topologic core is GIL-hostile native state, so the closure ships VALUE onto the warm
    # process pool (op payloads are bytes/policy, picklable whole) and the weave's harvest emits loop-side.
    return await evidence_run(
        EvidenceScope.GRAPH_TOPOLOGY, f"bridged.{op.tag}", partial(lane.offload, Kernel.of(lambda: _dispatch(op), KernelTrait.HOSTILE))
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
