# [PY_GEOMETRY_GRAPH_NONMANIFOLD]

Non-manifold topological modeling. `run` is one module-level entrypoint over the `TopologyOp` `@tagged_union(frozen=True)` discriminating the stateless `topologicpy` static-method namespace — non-manifold `Topology` construction (a `Cell`/`CellComplex` handle the `_CONSTRUCT` table lifts) from B-rep/OCCT/JSON/OBJ/IFC bytes, decomposition into the topological hierarchy, the non-manifold boolean kernel (`Union`/`Difference`/`Intersect`/`Slice`), cell-to-cell adjacency, `Dictionary`-attribute attachment, `BoundingBox`/`Centroid`/`Contains` analysis, and `Graph.ByTopology` dual-graph extraction folded through the `GRAPH_ANALYTIC` table for connectivity, centrality, spanning, and path analytics the C# `IfcSemanticModel` spatial projection does not perform. Each case carries its own payload and folds through one of the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` data tables rather than parallel arms; the dual-graph case graduates as the geometry-minted `GeometrySubject.TOPOLOGY_GRAPH` member, `graduates()` returning the local `rasm.geometry.graduation` `GeometryHandoff` carrier whose `wire()` projection is the compute crossing. The reducer-return vocabulary (`AnalyticValue`, the `ranked` board fold, the `peak_of` census projection) imports downward from the tier-0 `graph/analytic` substrate — no page-local twin. This owner is gated against re-deriving the in-process BIM space-graph.

`topologicpy` is an opt-in Forge-lane companion excluded from the default server build because its `AGPL-3.0-or-later` network-copyleft terms require an explicit accepting worker lane — so EVERY `topologicpy` and `ifcopenshell` binding is function-local behind the cached `_topo`/`_graph`/`_cluster`/`_dictionary` facade accessors, never a module-top import that loads the AGPL band into every companion start (the page's own gated-import discipline, the `ifc/analysis` parity). The owner and fences stay authored; runtime admission waits on the companion-lane provisioning charter.

## [01]-[INDEX]

- [01]-[TOPOLOGY]: the `TopologyOp` `@tagged_union`, the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS` op tables over the cached function-local facade accessors, the `GRAPH_ANALYTIC` analytics fold composing the tier-0 `AnalyticValue` substrate, the `TopologyPolicy` value object, the typed `TopologyCensus`, the `@receipted` aspect on the one pure `_extract`, the polymorphic single-or-batch `run` entrypoint over one pure `_dispatch`, and the async `bridged` mirror offloading the same body onto the runtime lane THREAD band.

## [02]-[TOPOLOGY]

- Owner: `run` is the one module-level entrypoint discriminating a single `TopologyOp` or a batch `Sequence[TopologyOp]`, mirroring the `graph/features.md` `Features.run` and `graph/algebra.md` `run` polymorphic single-or-batch siblings — there is no stateful capsule and no mutable receipt accumulator. `TopologyResult` is the sole receipt carrier (conforming structurally to the runtime-checkable `ReceiptContributor` Protocol), the typed receipt carrying the op tag, the result-handle summary strings, a typed `TopologyCensus` value object, and the graduation subject as the geometry-minted `GeometrySubject` member, never a bare `str`. `contribute` is the one emission, projecting `TopologyCensus` through `msgspec.structs.asdict` into one `Receipt.of("geometry.graph.nonmanifold", (phase, subject, facts))` `fact` row — `phase="emitted"` for a clean extraction and `phase="admitted"` for a degenerate result (an empty decomposition, a null boolean, or a zero-node dual graph) whose census flags the caveat rather than asserting a result. The `facts` map is `dict[str, object]` carrying native handle/cell/edge/analytic counts the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce. Retry and telemetry are the `boundary(f"topology.{op.tag}")` fence subject, not a second hand-rolled aspect; the dual-graph case alone graduates through `TopologyResult.graduates`.
- Cases: `TopologyOp` cases `construct(source, kind)` (the `SourceKind`-keyed `_CONSTRUCT` table — `BREP` via `Topology.ByBREPString`, `OCCT` via `Topology.ByOCCTShape` (the OpenCASCADE handle intake — the row consumes the shape handle payload the caller supplies), `JSON` via `Topology.ByJSONString`, `OBJ` via `Topology.ByOBJPath`, `IFC` via `Topology.ByIFCFile` over an in-memory `ifcopenshell.file.from_string` then `Cluster.ByTopologies` (the BIM-bytes row, never a path round-trip) — lifting bytes into a non-manifold `Topology` handle, never a `By*` family per source) · `decompose(source, kind)` (the `SubTopologyKind`-keyed `Topology.SubTopologies` hierarchy extraction — vertex/edge/wire/face/shell/cell/cellcomplex/cluster/aperture handles projected through `Topology.Analyze`; `Topology.Decompose` is the building-element role classifier returning a category `dict`, never the hierarchy accessor) · `adjacency(source)` (`Topology.AdjacentTopologies` cell-to-cell adjacency over `Topology.Cells`) · `boolean(a, b, kind)` (the `BooleanKind`-keyed `_BOOLEAN` table — `UNION`/`DIFFERENCE`/`INTERSECT`/`SLICE` over `Topology.Union`/`Difference`/`Intersect`/`Slice`, one polymorphic non-manifold boolean, never a per-op method) · `analysis(source, kind)` (the `AnalysisKind`-keyed `_ANALYSIS` table — `BBOX`/`CENTROID`/`CONTAINS`) · `attribute(source, keys, values)` (`Dictionary.ByKeysValues` built and attached via `Topology.AddDictionary`) · `dual_graph(source, policy)` (`Graph.ByTopology` extraction folded through the policy-selected `GRAPH_ANALYTIC` rows and serialized as JSON-bytes evidence via `msgspec.json.encode` over `Graph.JSONData`) — matched by total `match`/`case` closed with `assert_never`, each consuming the prior static-call handle. The sub-kind of every parameterized case is a closed `StrEnum`, never a raw string literal inside the payload.
- Entry: `run(op)` discriminates a single `TopologyOp` or a `Sequence[TopologyOp]` on the input axis, returning `RuntimeRail[TopologyResult]` for a single op and `RuntimeRail[Block[TopologyResult]]` for a batch. The `@receipted(REDACTION)` aspect wraps the ONE pure `_extract` both paths share, harvesting `contribute()` on exit, and the `boundary(f"topology.{op.tag}", ...)` exception-to-fault fence wraps `_extract` from OUTSIDE — the single runtime seam. A batch builds a `Block` of the same fenced rail in one comprehension (the `i=item` default binding the loop variable per closure) and folds them through `runtime.faults.traversed` with `Disposition.ACCUMULATE` so one fault stays addressable in the aggregate while every successful `TopologyResult` already emitted through the aspect. `bridged(op, lane)` is the bare async mirror returning `RuntimeRail[TopologyResult]`: it offloads the SAME `@receipted` `_extract` onto `lane.offload(..., modality=Modality.THREAD)` — the blocking `Graph.BetweennessCentrality`/`ClosenessCentrality`/`ShortestPath` all-pairs cores never block the event loop, the runtime-owned THREAD band bounds every concurrent bridged pass (zero geometry-minted `CapacityLimiter`s), the lane's own `async_boundary` is the single fence, and the aspect emits on `_extract`'s exit on the worker thread. `bridged` is NOT itself `@receipted` and never `.default_value`-collapses an offload fault into a synthetic degenerate result; a failed offload stays an `Error(BoundaryFault)` on the returned rail.
- Auto: the `topologicpy` surface is a stateless static-method namespace operating on `topologic_core` handles — each `Topology.ByX(...)` returns a handle the next static call consumes, so the dispatch threads handles through the chain rather than mutating an object; every facade binds through the cached `_topo`/`_graph`/`_cluster`/`_dictionary` function-local accessors (one `@cache`d boundary import per facade, the AGPL gate) so the module loads clean on the default lane. The construction case folds `_CONSTRUCT` by `SourceKind` so a new intake format is one `SourceKind` row plus one table entry; the boolean case folds `_BOOLEAN` by `BooleanKind`; the analysis case folds `_ANALYSIS` by `AnalysisKind` — never a dispatch branch. The dual-graph case threads the one `Graph` handle through every policy-selected `GRAPH_ANALYTIC` row, each reducer minting through the substrate `AnalyticValue` factories or the polymorphic `ranked` board fold (the topologicpy centralities return a vertex-ordered score `list`, the Sequence arm of the one shape-discriminated fold the networkx sibling's dict arm shares); the source-forced member law holds in the rows — the component count is `len(Graph.ConnectedComponents(graph))` (the island sub-graph list; `Graph.Connectivity` is a documented ALIAS of `DegreeCentrality` and a type error here), the spanning structure is `Graph.MinimumSpanningTree` (the true MST; `Graph.Tree` is a BFS/DFS traversal tree), and the wire evidence is the dict-returning `Graph.JSONData` feeding `msgspec.json.encode`. `_graph_census` reads `peak_of(analytics, key)` off the folded map so `betweenness_max` rides the max centrality and the board cardinality stays available to `scalar_of`, both off the one substrate union. `_census` is the matching topology fold reading `Cells`/`Faces`/`Vertices` once over a typed handle, and `_decompose_census` the parallel fold over the decomposed-parts tuple, so neither probes shape through a runtime `isinstance`. The cross-cutting emit is the `@receipted` aspect alone — each arm writes only its handle-chain body and returns a `TopologyResult`.
- Receipt: each `run` produces a `TopologyResult`; `contribute` emits one phase-keyed `Receipt.of` `fact` row, the facts being the `TopologyCensus` value object (`structs.asdict`-projected) carrying the op tag, the handle count, the cell/face/edge census, and the dual-graph analytic scalars where the case produces them. `dual_graph` alone graduates a geometry subject (`GeometrySubject.TOPOLOGY_GRAPH`) through `TopologyResult.graduates(evidence_key)`, returning the local `GeometryHandoff` carrier gating the measured `empty_node_fraction` against `_GRAPH_CEILING` — a degenerate zero-node graph breaches the carrier's residual-over-ceiling `admitted` verdict rather than crossing clean, and the `msgspec.json.encode` byte payload is the evidence the crossing keys; the non-graph ops emit the receipt only.
- Packages: `topologicpy` (`Topology.ByBREPString`/`ByOCCTShape`/`ByJSONString`/`ByOBJPath`/`ByIFCFile`/`Cells`/`Faces`/`Edges`/`Vertices`/`SubTopologies`/`AdjacentTopologies`/`Analyze`/`Union`/`Difference`/`Intersect`/`Slice`/`BoundingBox`/`Centroid`/`Contains`/`AddDictionary` — `Topology.ByIFCFile` ingests an in-memory `ifcopenshell.file` and returns a per-product topology `list` the `Cluster.ByTopologies` fold collapses to one non-manifold handle; `Topology.Vertices` is the polymorphic vertex accessor over a `Topology`, a `Graph`, OR a `ShortestPath` `Wire`, so it reads the graph node count and the geodesic hop count without a gated `Graph.Vertices`; `Graph.ByTopology`/`Edges`/`ConnectedComponents`/`MinimumSpanningTree`/`ShortestPath`/`BetweennessCentrality`/`ClosenessCentrality`/`DegreeCentrality`/`JSONData`, `Cluster.ByTopologies`, `Dictionary.ByKeysValues` — ALL bound through the cached function-local facade accessors, the AGPL gate), `ifcopenshell` (`file.from_string` — the in-memory SPF loader the `SourceKind.IFC` row opens the byte stream through, function-local, the same loader `mesh/daemon.md` uses), geometry (`AnalyticValue`/`ranked`/`peak_of` the tier-0 analytic substrate, `GeometrySubject`/`GeometryHandoff` the graduation spine), runtime (`RuntimeRail`/`boundary`/`traversed`/`Disposition`, `Receipt`/`Redaction`/`receipted`, `ContentKey` from `rasm.runtime.identity`, `LanePolicy.offload`/`Modality.THREAD` the runtime-owned worker band the bridged mirror rides), `expression` (`tagged_union`/`tag`/`case`, `Block`/`Map`), `numpy` (rides transitively through the substrate `peak` fold), `msgspec` (`Struct`/`structs.asdict`/`json.encode`).
- Growth: a new intake format is one `SourceKind` row plus one `_CONSTRUCT` entry; a new boolean or analysis verb is one enum row plus one table entry; a new graph analytic is one `GraphAnalytic` row plus one `GRAPH_ANALYTIC` reducer; the bottom-up construction family (`Vertex.ByCoordinates`/`Cell.ByFaces`/`CellComplex.ByCells`), the `Aperture.ByTopologyContext` opening topology, and the `BVH` clash/raycast surface admit as further `SourceKind`/`AnalysisKind` rows when a consumer demands them — table growth, never a new page; zero new surface.
- Boundary: `topologicpy` is admitted ONLY for non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and is never re-derived here; numerical and form-finding geometry is the `graph/algebra` sibling over `compas`; mesh-feature projection onto the network graph is the `graph/features` sibling; raw mesh-file exchange stays at the data `MeshPayload` seam — `run` returns handle/JSON-bytes summaries across the wire and never writes a topology file. The `topology-graph` subject crosses HERE; the `network-graph` subject stays in the `features`/`algebra` siblings. The deleted forms: a module-top `topologicpy`/`ifcopenshell` import loading the AGPL band into every companion start where the cached function-local facade accessors gate it; a page-local `AnalyticValue` twin or `_leaders` fold where the `graph/analytic` substrate owns the vocabulary; a geometry-minted `CapacityLimiter` or bare `anyio.to_thread.run_sync` offload where the runtime lane THREAD band owns the worker bound; a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff`; a stateful capsule mutating a `Block[Receipt]` accumulator; a flat `StrEnum`-keyed `match` where the `@tagged_union` case carries its own payload; a `By*` accessor/boolean family per topology kind; a `Graph.Connectivity` component count (the `DegreeCentrality` alias, a type error); a `Graph.Tree` spanning structure (a traversal tree, not the MST); a `Graph.JSONString` pre-serialized wire where the dict-returning `JSONData` feeds the codec; a `_census` probing shape through a runtime `isinstance`; a `ByOCCTShape(ByBREPString(...))` double-call; a 4-positional `Receipt.of`; a `str()`-coerced facts map; an inline `Receipt.of` threaded through the dispatch body; a second batch method; a temp-file round-trip where the byte-stream constructor owns the intake; and leaked tool markup anywhere in the page.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from functools import cache
from types import MappingProxyType
from typing import Final, Literal, assert_never

import msgspec
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.geometry.graduation import GeometryHandoff, GeometrySubject
from rasm.geometry.graph.analytic import AnalyticValue, peak_of, ranked
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------

type OpTag = Literal["construct", "decompose", "adjacency", "boolean", "analysis", "attribute", "dual_graph"]
type Phase = Literal["admitted", "emitted"]
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
    # the closed `Topology.SubTopologies(subTopologyType=)` vocabulary — the generic hierarchy
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

REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # topology facts carry no secret field
# the dual-graph residual the graduation carrier gates: a degenerate graph (zero nodes) breaches.
_GRAPH_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_node_fraction": 0.0})

# --- [BOUNDARIES] -------------------------------------------------------------------------


# the AGPL gate: one cached boundary import per topologicpy facade, so the module loads clean on the
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
    # the dual-graph analytic selection and reduction bounds; every threshold and toggle is a field,
    # never a positional float or a per-call-site fork. Mirrors the `graph/features.md` `FeaturePolicy`.
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


class TopologyResult(Struct, frozen=True):  # conforms structurally to the runtime-checkable ReceiptContributor Protocol
    op: OpTag
    handles: tuple[str, ...]  # `Topology.Analyze` summaries — the one projection every arm threads; the dual-graph arm a compact node/edge descriptor
    census: TopologyCensus
    evidence: bytes = b""  # `msgspec.json.encode` dual-graph payload; b"" for non-graph ops
    graduation_subject: GeometrySubject = GeometrySubject.TOPOLOGY_GRAPH
    degenerate: bool = False

    def contribute(self) -> tuple[Receipt, ...]:
        # runtime `Receipt.of(owner, (phase, subject, facts))` two-arg contract; native count/scalar
        # facts ride the `EventDict` `dict[str, object]` slots the `enc_hook=repr` renderer serializes.
        phase: Phase = "admitted" if self.degenerate else "emitted"
        facts: dict[str, object] = {**structs.asdict(self.census), "handle_count": len(self.handles)}
        return (Receipt.of("geometry.graph.nonmanifold", (phase, self.graduation_subject, facts)),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        # only the dual-graph case graduates: the local carrier's residual-over-ceiling `admitted`
        # verdict gates the empty-node fraction, and `wire()` is the compute crossing — never an import.
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
    # the hop count of the geodesic between the extreme vertices. `Topology.Vertices` is the
    # polymorphic accessor over `Topology` OR `Graph` OR the `Wire` `ShortestPath` returns, so one
    # accessor reads the node list and the geodesic hop count. A disconnected graph has no such path,
    # so a null geodesic folds to `Scalar(0.0)` rather than crashing — the row stays total.
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

# every reducer collapses its return to one substrate `AnalyticValue` case: connectivity to a `Scalar`
# component count over `len(ConnectedComponents(...))` — NEVER `Graph.Connectivity`, the documented
# `DegreeCentrality` alias whose per-vertex list is a type error here; the centralities to `ranked`
# boards (the Sequence arm of the one shape-discriminated fold — topologicpy returns vertex-ordered
# lists); the spanning structure to `Groups` over the TRUE `MinimumSpanningTree` — never `Graph.Tree`,
# a BFS/DFS traversal tree; the path to a `Scalar` hop count. A new analytic is one row.
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
            # `SubTopologies` yields real sub-topology HANDLES for the requested tier (`None` on a
            # bad kind folds to the degenerate empty result); `Decompose` returns a role-category
            # dict of building-element lists — iterating it walks string keys, the deleted form.
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
            # the wire summary is the compact census-keyed descriptor; the whole serialized document
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
    # `Cells`/`Faces`/`Edges`/`Vertices` enumerate the constituent handles of ONE topology; a null boolean
    # handle yields the zero census. No runtime shape probe — `_decompose_census` owns the tuple arm.
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
    # the decomposed-parts arm measures the lead handle and carries the part count, parallel to `_census`.
    return _census(op, parts[0], handles=len(parts)) if parts else TopologyCensus(op=op, handles=0)


def _graph_analytics(graph: Handle, policy: TopologyPolicy) -> Map[GraphAnalytic, AnalyticValue]:
    # the one fold over the policy-selected `GRAPH_ANALYTIC` rows; a zero/one-node graph skips the
    # path/centrality rows by data, not a branch. `Topology.Vertices` is the polymorphic node source.
    nodes = len(_topo().Vertices(graph))
    selected = (a for a in GraphAnalytic if a in policy.analytics and (nodes > 1 or a is GraphAnalytic.CONNECTIVITY))
    return Map.of_seq((a, GRAPH_ANALYTIC[a](graph, policy)) for a in selected) if nodes else Map.empty()


def _graph_census(graph: Handle, analytics: Map[GraphAnalytic, AnalyticValue]) -> TopologyCensus:
    # the dual-graph census reads the folded map through the substrate `peak_of` projection rather
    # than re-measuring: connectivity yields the component count, betweenness the max centrality,
    # path the hops — an absent row folds to 0.0 with no None arm.
    return TopologyCensus(
        op="dual_graph",
        handles=1,
        nodes=len(_topo().Vertices(graph)),
        components=int(peak_of(analytics, GraphAnalytic.CONNECTIVITY)),
        betweenness_max=peak_of(analytics, GraphAnalytic.BETWEENNESS),
        path_length=int(peak_of(analytics, GraphAnalytic.SHORTEST_PATH)),
    )


@receipted(REDACTION)
def _extract(op: TopologyOp) -> TopologyResult:
    # the ONE pure dispatch both paths share: it returns the `TopologyResult` contributor the aspect
    # harvests on exit, threading no emit call. The fence lives OUTSIDE — `run`'s `boundary` on the
    # sync arm, the lane's `async_boundary` on the bridged arm.
    return _dispatch(op)


def run(op: TopologyOp | Sequence[TopologyOp]) -> RuntimeRail[TopologyResult] | RuntimeRail[Block[TopologyResult]]:
    # one entrypoint over the input axis: a single op fences `_extract` once, a batch folds a `Block` of
    # the same fenced rail through `traversed` ACCUMULATE so one fault stays addressable in the aggregate
    # while every successful result already emitted through the aspect. The default `i=item` binds the
    # loop variable per closure so the comprehension does not capture the last `item` by reference.
    match op:
        case Sequence() as batch:
            return traversed(
                Block.of_seq([boundary(f"topology.{item.tag}", lambda i=item: _extract(i)) for item in batch]), by=Disposition.ACCUMULATE
            )
        case TopologyOp() as single:
            return boundary(f"topology.{single.tag}", lambda: _extract(single))
        case _ as unreachable:
            assert_never(unreachable)


async def bridged(op: TopologyOp, lane: LanePolicy) -> RuntimeRail[TopologyResult]:
    # the bare async mirror of `run`: the SAME `@receipted` `_extract` offloads onto the runtime lane
    # THREAD band — the all-pairs cores never block the loop, the runtime-owned band bounds every
    # concurrent pass (zero geometry-minted limiters), the lane's `async_boundary` is the single fence,
    # and the aspect emits on `_extract`'s exit on the worker thread. A failed offload stays an
    # `Error(BoundaryFault)` on the returned rail the caller reads.
    return await lane.offload(lambda: _extract(op), modality=Modality.THREAD)
```
