# [PY_GEOMETRY_GRAPH_NONMANIFOLD]

Non-manifold topological modeling over the stateless `topologicpy` static-method namespace: construction from B-rep/OCCT/JSON/OBJ/IFC bytes, hierarchy decomposition, the non-manifold boolean kernel, cell adjacency, attribute attachment, geometric analysis, and `Graph.ByTopology` dual-graph extraction with the connectivity/centrality/spanning/path analytics the C# `IfcSemanticModel` spatial projection does not perform. Each case folds through one of the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` data tables, never parallel arms; the `topology-graph` subject crosses HERE, and `network-graph` stays with the `features`/`algebra` siblings.

`topologicpy` is an opt-in Forge-lane companion excluded from the default server build — its `AGPL-3.0-or-later` network-copyleft terms require an explicit accepting worker lane — so every `topologicpy` binding stays function-local behind the cached `_topo`/`_graph`/`_cluster`/`_dictionary` seam accessors: a license audit reads the LEXICAL import graph, and a module-scope binding — `lazy` included, the soft keyword being module-scope by design — would mark every importer of this module AGPL-coupled, so the deferral dialect the compiled bands ride cannot satisfy this ban. `ifcopenshell` carries no such term and binds as one module-scope `lazy import`, reifying on the first loader call; the owner and fences stay authored, runtime admission binding to the companion-lane provisioning charter. Reducer-return vocabulary imports downward from the tier-0 `graph/analytic` substrate, no page-local twin; `run` and `bridged` return through the graduation `evidence_run` weave seeded `EvidenceScope.GRAPH_TOPOLOGY` — span, fence, and receipt harvest in one composition over the pure module-level `_dispatch`, both entries threading the caller's composition `ScopeKey` — and `bridged` ships that `_dispatch` `REFERENCE` as a `KernelTrait.HOSTILE` kernel onto the warm process pool with the op alone as crossing payload, because the TopologicPy/OCCT core holds process-global native state and imports under no isolated subinterpreter, so a thread or subinterpreter arm is the untruthful trait; the sibling wiring convention holds unchanged.

## [01]-[INDEX]

- [02]-[TOPOLOGY]: `TopologyOp` union, its `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` tables over the cached AGPL seam accessors, the held-board `graduates`/`frame` egress pair, and the `run`/`bridged` pair under one `ReceiptContributor`.

## [02]-[TOPOLOGY]

- Owner: `run` is the one module-level entrypoint — no stateful capsule, no mutable receipt accumulator. `TopologyResult` is the sole `ReceiptContributor`, its phase data-driven — `emitted` for a clean extraction, `admitted` for a degenerate result (an empty decomposition, a null boolean, a zero-node dual graph) — so a caveat is flagged rather than asserted. Every parameterized case's sub-kind is a closed `StrEnum`, never a raw string in the payload.
- Entry: `run` discriminates a single op or a batch, each returning through its own weave rail; `bridged` never collapses an offload fault into a synthetic degenerate result — a failure stays an `Error(BoundaryFault)` on the returned rail.
- Auto: every static call returns an opaque `topologic_core` handle the next call consumes, so dispatch threads handles through the chain rather than mutating an object; the topologicpy centralities return vertex-ordered score lists — the Sequence arm of the substrate's one shape-discriminated `ranked` fold, shared with the networkx sibling's dict arm — while the structural analytics return handle collections carrying no vertex index, so their reducers publish a `Scalar` count rather than seating handles in an index-declared partition.
- Receipt: only the dual-graph case graduates — `GeometrySubject.TOPOLOGY_GRAPH`, gating `empty_node_fraction` against the zero ceiling so a degenerate graph breaches rather than crossing clean; the non-graph ops emit the receipt only. `spec` is the JSON-bytes payload beside the op tag, and both egress ports fold it through the graduation spine's `evidence_key` mint, so `graduates()` and `frame()` key one evidence identically and neither takes a key from its caller. The result HOLDS its analytic board map rather than reducing it to census scalars at construction, so `frame` projects a centrality leaderboard or a spanning partition through the graduation `EvidenceFrame` port off the substrate's `tabled` columns — the same egress the features sibling carries. That split is what keeps the evidence honest: `TopologyCensus` carries STRUCTURAL counts alone, and the analytic facts derive off the measured map, so an analytic a one-node graph skipped or a narrowed `TopologyPolicy` excluded is ABSENT from the receipt rather than published as a zero its reducer never produced.
- Packages: `topologicpy` bound ONLY through the cached seam accessors, the AGPL license-isolation ban refusing every module-scope form including `lazy`; `ifcopenshell` binds as one module-scope `lazy import`, and every table row stays a call-time lambda so no cell dereferences a deferred name at import; the analytic vocabulary and the graduation spine import downward from their geometry owners.
- Law: an analytic reducer answers an OPTION and a row that MEASURED nothing leaves the board — the geodesic on a disconnected or single-vertex graph is the standing case, where the retired `Scalar(0.0)` published a zero-hop path a reader could not tell from a real one. A walk publishes the per-hop CENSUS it computed through the substrate's one `reached` fold, so the traversed count and the hop length both derive off one evidence and the policy's runtime `Depth` bounds the traversal with a typed `unreached` refusal past it, never a truncated census.
- Growth: a new intake format is one `SourceKind` row and one `_CONSTRUCT` entry; a new boolean or analysis verb is one enum row and one table entry; a new graph analytic is one `GraphAnalytic` row and one `Option`-returning `GRAPH_ANALYTIC` reducer, reaching `frame` through the held board map with no projection edit; a new composition is one `ScopeKey` threaded through the `composition` keyword both entries carry; the bottom-up construction family (`Vertex.ByCoordinates`/`Cell.ByFaces`/`CellComplex.ByCells`), the `Aperture.ByTopologyContext` opening topology, and the `BVH` clash/raycast surface admit as further rows when a consumer demands them — table growth, never a new page.
- Boundary: `topologicpy` is admitted ONLY for the non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and never re-derived here; numerical/form-finding geometry is the `algebra` sibling's, mesh-feature projection the `features` sibling's, and raw mesh-file exchange stays at the data `MeshPayload` seam — `run` returns handle/JSON-bytes summaries and never writes a topology file.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from functools import cache, partial
from types import MappingProxyType
from typing import Final, Literal, assert_never

import msgspec
from expression import Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

lazy import ifcopenshell

from rasm.geometry.graduation import EvidenceFrame, EvidenceScope, GeometryHandoff, GeometrySubject, evidence_key, evidence_run
from rasm.geometry.graph.analytic import AnalyticValue, ranked, reached
from rasm.runtime.faults import Depth, Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type OpTag = Literal["construct", "decompose", "adjacency", "boolean", "analysis", "attribute", "dual_graph"]
type Handle = object
type Reducer = Callable[[Handle, "TopologyPolicy"], Option[AnalyticValue]]


class SourceKind(StrEnum):
    BREP = "brep"
    OCCT = "occt"
    JSON = "json"
    OBJ = "obj"
    IFC = "ifc"


class SubTopologyKind(StrEnum):
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

_GRAPH_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_node_fraction": 0.0})

# --- [BOUNDARIES] -----------------------------------------------------------------------


@cache
def _topo() -> type:
    from topologicpy.Topology import Topology

    return Topology


@cache
def _graph() -> type:
    from topologicpy.Graph import Graph

    return Graph


@cache
def _cluster() -> type:
    from topologicpy.Cluster import Cluster

    return Cluster


@cache
def _dictionary() -> type:
    from topologicpy.Dictionary import Dictionary

    return Dictionary


# --- [MODELS] ---------------------------------------------------------------------------


class TopologyPolicy(Struct, frozen=True, gc=False):
    analytics: frozenset[GraphAnalytic] = frozenset(GraphAnalytic)
    centrality_top: int = 16
    geodesic: Depth = Depth(fixpoint=None)


class TopologyCensus(Struct, frozen=True, gc=False):
    op: OpTag
    handles: int
    cells: int = 0
    faces: int = 0
    edges: int = 0
    vertices: int = 0
    nodes: int = 0


class TopologyResult(Struct, frozen=True):
    op: OpTag
    handles: tuple[str, ...]
    census: TopologyCensus
    evidence: bytes = b""
    analytics: Map[GraphAnalytic, AnalyticValue] = Map.empty()
    graduation_subject: GeometrySubject = GeometrySubject.TOPOLOGY_GRAPH
    degenerate: bool = False

    def contribute(self) -> tuple[Receipt, ...]:
        phase: Phase = "admitted" if self.degenerate else "emitted"
        facts: dict[str, object] = {
            **structs.asdict(self.census),
            "handle_count": len(self.handles),
            **{analytic.value: value.peak() for analytic, value in self.analytics.items()},
        }
        return (Receipt.of("rasm.geometry.graph.nonmanifold", (phase, self.graduation_subject, facts)),)

    @property
    def spec(self) -> bytes:
        return b"|".join((self.op.encode(), self.evidence))

    def graduates(self) -> GeometryHandoff:
        empty = 0.0 if self.census.nodes else 1.0
        return GeometryHandoff.of(
            self.graduation_subject, evidence_key(self.graduation_subject, self.spec), {"empty_node_fraction": empty}, _GRAPH_CEILING
        )

    def frame(self, analytic: GraphAnalytic) -> "RuntimeRail[EvidenceFrame]":
        board = self.analytics.try_find(analytic).default_value(AnalyticValue.Leaderboard(())).tabled()
        return EvidenceFrame.of(self.graduation_subject, evidence_key(self.graduation_subject, self.spec), board)


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
    return _cluster().ByTopologies(*_topo().ByIFCFile(ifcopenshell.file.from_string(source.decode("utf-8"))))


def _path(graph: Handle, policy: TopologyPolicy) -> Option[AnalyticValue]:
    verts = _topo().Vertices(graph)
    span = _graph().ShortestPath(graph, verts[0], verts[-1]) if len(verts) > 1 else None
    return Option.of_obj(span).map(lambda held: _chain(len(_topo().Vertices(held)), policy.geodesic))


def _chain(hops: int, bound: Depth) -> AnalyticValue:
    return reached(lambda node: (peer for peer in (node - 1, node + 1) if 0 <= peer < hops), (0,), bound)


_CONSTRUCT: Final[Mapping[SourceKind, Callable[[bytes], Handle]]] = MappingProxyType({
    SourceKind.BREP: lambda b: _topo().ByBREPString(b.decode()),
    SourceKind.OCCT: lambda b: _topo().ByOCCTShape(b),
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

GRAPH_ANALYTIC: Final[Mapping[GraphAnalytic, Reducer]] = MappingProxyType({
    GraphAnalytic.CONNECTIVITY: lambda g, _: Some(AnalyticValue.Scalar(float(len(_graph().ConnectedComponents(g))))),
    GraphAnalytic.BETWEENNESS: lambda g, p: Some(ranked(_graph().BetweennessCentrality(g), p.centrality_top)),
    GraphAnalytic.CLOSENESS: lambda g, p: Some(ranked(_graph().ClosenessCentrality(g), p.centrality_top)),
    GraphAnalytic.DEGREE: lambda g, p: Some(ranked(_graph().DegreeCentrality(g), p.centrality_top)),
    GraphAnalytic.SPANNING: lambda g, _: Some(AnalyticValue.Scalar(float(len(_graph().Edges(_graph().MinimumSpanningTree(g)))))),
    GraphAnalytic.SHORTEST_PATH: _path,
})


def _dispatch(op: TopologyOp) -> TopologyResult:
    match op:
        case TopologyOp(tag="construct", construct=(source, kind)):
            handle = _lift(source, kind)
            return _result("construct", (_topo().Analyze(handle),), _census("construct", handle, handles=1))
        case TopologyOp(tag="decompose", decompose=(source, kind)):
            parts = tuple(_topo().SubTopologies(_lift(source, SourceKind.BREP), subTopologyType=kind.value) or ())
            return _result("decompose", tuple(_topo().Analyze(p) for p in parts), _decompose_census("decompose", parts))
        case TopologyOp(tag="adjacency", adjacency=source):
            handle = _lift(source, SourceKind.BREP)
            adj = tuple(a for c in _topo().Cells(handle) for a in _topo().AdjacentTopologies(c, handle))
            return _result("adjacency", tuple(_topo().Analyze(a) for a in adj), _census("adjacency", handle, handles=len(adj)))
        case TopologyOp(tag="boolean", boolean=(a, b, kind)):
            combined = _BOOLEAN[kind](_lift(a, SourceKind.BREP), _lift(b, SourceKind.BREP))
            handles = (_topo().Analyze(combined),) if combined is not None else ()
            return _result("boolean", handles, _census("boolean", combined, handles=len(handles)), degenerate=combined is None)
        case TopologyOp(tag="analysis", analysis=(source, kind)):
            value = _ANALYSIS[kind](_lift(source, SourceKind.BREP))
            return _result("analysis", (_topo().Analyze(value),), _census("analysis", value, handles=1))
        case TopologyOp(tag="attribute", attribute=(source, keys, values)):
            handle = _topo().AddDictionary(_lift(source, SourceKind.BREP), _dictionary().ByKeysValues(list(keys), list(values)))
            return _result("attribute", (_topo().Analyze(handle),), _census("attribute", handle, handles=1))
        case TopologyOp(tag="dual_graph", dual_graph=(source, policy)):
            graph = _graph().ByTopology(_lift(source, SourceKind.BREP))
            analytics = _graph_analytics(graph, policy)
            census = _graph_census(graph)
            evidence = msgspec.json.encode(_graph().JSONData(graph))
            summary = f"graph nodes={census.nodes} measured={','.join(sorted(a.value for a in analytics))}"
            return _result("dual_graph", (summary,), census, evidence=evidence, analytics=analytics, degenerate=census.nodes == 0)
        case _ as unreachable:
            assert_never(unreachable)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _result(
    op: OpTag,
    handles: tuple[str, ...],
    census: TopologyCensus,
    *,
    evidence: bytes = b"",
    analytics: Map[GraphAnalytic, AnalyticValue] = Map.empty(),
    degenerate: bool = False,
) -> TopologyResult:
    return TopologyResult(op=op, handles=handles, census=census, evidence=evidence, analytics=analytics, degenerate=degenerate or not handles)


def _census(op: OpTag, handle: Handle, *, handles: int) -> TopologyCensus:
    if handle is None:
        return TopologyCensus(op=op, handles=handles)
    return TopologyCensus(
        op=op,
        handles=handles,
        cells=len(_topo().Cells(handle)),
        faces=len(_topo().Faces(handle)),
        edges=len(_topo().Edges(handle)),
        vertices=len(_topo().Vertices(handle)),
    )


def _decompose_census(op: OpTag, parts: tuple[Handle, ...]) -> TopologyCensus:
    return _census(op, parts[0], handles=len(parts)) if parts else TopologyCensus(op=op, handles=0)


def _graph_analytics(graph: Handle, policy: TopologyPolicy) -> Map[GraphAnalytic, AnalyticValue]:
    nodes = len(_topo().Vertices(graph))
    selected = Block.of_seq(a for a in GraphAnalytic if a in policy.analytics and (nodes > 1 or a is GraphAnalytic.CONNECTIVITY))
    measured = selected.choose(lambda row: GRAPH_ANALYTIC[row](graph, policy).map(lambda value: (row, value)))
    return Map.of_seq(measured) if nodes else Map.empty()


def _graph_census(graph: Handle) -> TopologyCensus:
    return TopologyCensus(op="dual_graph", handles=1, nodes=len(_topo().Vertices(graph)))


def run(
    op: TopologyOp | Sequence[TopologyOp], *, composition: ScopeKey = DEFAULT_SCOPE
) -> RuntimeRail[TopologyResult] | RuntimeRail[Block[TopologyResult]]:
    match op:
        case Sequence() as batch:
            return traversed(
                Block.of_seq([
                    evidence_run(EvidenceScope.GRAPH_TOPOLOGY, f"run.{item.tag}", lambda i=item: _dispatch(i), composition=composition)
                    for item in batch
                ]),
                by=Disposition.ACCUMULATE,
            )
        case TopologyOp() as single:
            return evidence_run(EvidenceScope.GRAPH_TOPOLOGY, f"run.{single.tag}", lambda: _dispatch(single), composition=composition)
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: TopologyOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[TopologyResult]:
    return await evidence_run(
        EvidenceScope.GRAPH_TOPOLOGY,
        f"bridged.{op.tag}",
        partial(lane.offload, Kernel.of(_dispatch, KernelTrait.HOSTILE), op),
        composition=composition,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
